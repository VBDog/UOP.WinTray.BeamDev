using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Spreadsheet;
using iText.Layout.Properties;
using Microsoft.Office.Interop.Excel;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Windows;
using System.Windows.Documents;
using UOP.DXFGraphics;
using UOP.DXFGraphics.Utilities;
using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Documents;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Interfaces;
using UOP.WinTray.Projects.Materials;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.Projects.Tables;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;
using UOP.WinTray.UI.Interfaces;
using UOP.WinTray.UI.Model;
using Xceed.Wpf.Toolkit.Zoombox;
using static iText.Kernel.Pdf.Colorspace.PdfShading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace UOP.WinTray.UI.BusinessLogic
{
    public class AppDrawingMDDHelper : IappDrawingSource
    {
        #region Constants
        private const string TYP = " TYP";
        #endregion Constants

        #region Constructor
        public AppDrawingMDDHelper()
        {
            _Tool = new AppDrawingSourceHelper();
            _Tool.StatusChange += ToolStatusChange;

        }
        #endregion Constructor




        #region Events


        public delegate void CanceledDelegate();
        public event CanceledDelegate Canceled;

        public delegate void StatusChangeHandler(string StatusString);
        public event IappDrawingSource.StatusChangeHandler StatusChange;

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
                    Tool.CancelBorder();

                }
            }

        }

        public bool SuppressBorder { get; set; }

        private string _Status = "";
        public string Status
        {
            get => _Status;
            set
            {
                if (string.IsNullOrWhiteSpace(value)) value = "";
                if (string.Compare(value, _Status, ignoreCase: false) != 0)
                {
                    StatusWas = _Status;
                    _Status = value;
                    if (_Tool != null) _Tool.BaseStatus = _Status;
                    if (!SuppressWorkStatus) StatusChange?.Invoke(_Status);
                }


            }
        }

        private string _StatusWas = "";

        private string StatusWas
        {
            get => _StatusWas;
            set { if (string.IsNullOrWhiteSpace(value)) value = ""; _StatusWas = value; }
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

        #region Properties

        private AppDrawingSourceHelper _Tool;
        public AppDrawingSourceHelper Tool
        {
            get { _Tool ??= new AppDrawingSourceHelper(); _Tool.SuppressWorkStatus = SuppressWorkStatus; return _Tool; }
            set { _Tool = value; }
        }




        // local variable used as shorthand for Range.ShellID / 2
        private double ShellRad { get; set; }

        // local variable used as shorthand for Range.RingID / 2
        private double RingRad { get; set; }



        // the ring thickness
        private double RingTk { get; set; }



        private string LayerPre { get; set; }


        private double ElevationX { get; set; }


        private string DrawingType { get; set; }

        // the current drawing request
        public uopDocDrawing Drawing { get; set; }

        public mdDowncomer Downcomer { get; set; }


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
                    SetDrawingVars();
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
        public mdTrayAssembly Assy  => _Assy; 


        public bool Testing { get; set; }

        public colMDSpoutGroups SpoutGroups { get => MDRange?.SpoutGroups; }


        public mdSpoutGroup SpoutGroup { get; set; }


        private mdProject _MDProject;
        public mdProject MDProject
        {
            get => _MDProject;
            private set
            {
                if (_MDProject != null)
                    _MDProject.eventPartGeneration -= ProjectPartGenerationHandler;

                _MDProject = value;
                if (_MDProject != null)
                    _MDProject.eventPartGeneration += ProjectPartGenerationHandler;
            }
        }


        #endregion Properties

        #region Interface Implementation

        // flag which indicates if a drawing is being generated
        public bool DrawinginProgress { get; set; }

        public bool DrawPhantom { get; set; }

        private bool _SuppressWorkStatus;
        public bool SuppressWorkStatus { get => _SuppressWorkStatus; set { _SuppressWorkStatus = value; _Tool.SuppressWorkStatus = value; } }

        private dxfImage _Image = null;
        public dxfImage Image { get => _Image; set => _Image = value; }
        public dxfBlockSource BlockSource { get; set; }
        private bool DrawlayoutRectangles { get; set; }
        public TitleBlockHelper TitleHelper => Tool.TitleBlockHelper;

        public uopProject Project
        {
            get => _MDProject;
            set
            {
                if (value == null)
                {
                    if (_MDProject != null)
                    {
                        _MDProject.eventPartGeneration -= ProjectPartGenerationHandler;
                    }
                    _MDProject = null;
                    return;
                }

                if (value.ProjectFamily == uppProjectFamilies.uopFamMD)
                {
                    _MDProject = (mdProject)value;
                    _MDProject.eventPartGeneration += ProjectPartGenerationHandler;
                }
            }
        }

        mdTrayAssembly IappDrawingSource.MDAssy => Assy;

        bool IappDrawingSource.DrawLayoutRectangles => DrawlayoutRectangles;

        private void ProjectPartGenerationHandler(string aStatusString, bool? bBegin = null)
        {
            if (bBegin.HasValue)
                Status = bBegin.Value ? aStatusString : StatusWas;
            else
                Status = aStatusString;
        }

        public void TerminateObjects()
        {
            if (Image != null && Drawing != null)
            {
                Image.ImageName = Drawing.DrawingName;
            }

            MDRange = null;
         
            if (_MDProject != null)
            {
                _MDProject.eventPartGeneration -= ProjectPartGenerationHandler;
            }
            _MDProject = null;


            Drawing = null;
            //Assy = null;

            //_Tool.TitleBlockHelper = null;

            SpoutGroup = null;

            if (_Tool != null) _Tool.StatusChange -= ToolStatusChange;
            _Tool = null;

            _Image = null;

        }
        #endregion

        public static void BlockTransform( dxfEntity aEntity, dxeInsert aInsert )
        {
            //move and scale an entity copied from the block definition to align with the block insert (equivilent of using BlockTransform in AutoCAD)
            aEntity.MoveFromTo( dxfVector.Zero, aInsert.InsertionPt );
            aEntity.RotateAbout( aInsert.InsertionPt, aInsert.RotationAngle );
            aEntity.Rescale( aInsert.XScaleFactor, aInsert.InsertionPt );
            if (aInsert.XScaleFactor < 0 && aInsert.YScaleFactor > 0)
            {
                aEntity.Mirror( new dxeLine( aInsert.InsertionPt, new dxfVector( 0, 1, 0 ), 10 ) );
            }
            if (aInsert.XScaleFactor > 0 && aInsert.YScaleFactor < 0)
            {
                aEntity.Mirror( new dxeLine( aInsert.InsertionPt, new dxfVector( 1, 0, 0 ), 10 ) );
            }
        }

        public static void BlockTransform( dxfVector aPoint, dxeInsert aInsert )
        {
            //move and scale a point copied from the block definition to align with the block insert (equivilent of using BlockTransform in AutoCAD)
            aPoint.MoveFromTo( dxfVector.Zero, aInsert.InsertionPt );
            aPoint.RotateAbout( aInsert.InsertionPt, aInsert.RotationAngle );
            aPoint.Rescale( aInsert.XScaleFactor, aInsert.InsertionPt );
            if (aInsert.XScaleFactor < 0 && aInsert.YScaleFactor > 0)
            {
                aPoint.Mirror( new dxeLine( aInsert.InsertionPt, new dxfVector( 0, 1, 0 ), 10 ) );
            }
            if (aInsert.XScaleFactor > 0 && aInsert.YScaleFactor < 0)
            {
                aPoint.Mirror( new dxeLine( aInsert.InsertionPt, new dxfVector( 1, 0, 0 ), 10 ) );
            }
        }

        public static colDXFVectors BlockTransform( uopVectors aVectors, dxeInsert aInsert )
        {
            var _rVal = new colDXFVectors();
            foreach (var p in aVectors)
            {
                var v = p.ToDXFVector();
                BlockTransform( v, aInsert );
                _rVal.Add( v );
            }
            return _rVal;
        }

        /// <summary>
        /// generates the md support beam attachment drawing
        /// </summary>
        private void DDWG_BeamAttachments()
        {
            dxfRectangle fitRec = null;
            double paperscale = 0;
            dxfVector v0;
            dxfVector v1;
            dxfVector v2;
            Drawing.BorderSize = uppBorderSizes.DSize_Landscape;

            colDXFEntities view1;
            colDXFEntities view2;
            colDXFEntities view3;
            colDXFEntities view4;
            dxoDrawingTool draw = Image.Draw;
            dxoDimTool dims = Image.DimTool;
            dxoLeaderTool leaders = Image.LeaderTool;
            dxfRectangle rec1 = null;
            dxfRectangle rec2 = null;
            dxfRectangle rec3 = null;
            dxfRectangle rec4 = null;

            mdBeam beam = Assy?.Beam;
            if (beam == null)
                return;

            try
            {
                string lname = "GEOMETRY";
                Image.LinetypeLayers.Setting = dxxLinetypeLayerFlag.ForceToLayer;
                Image.Layers.Add( lname );


                Status = "Sizing Border";
                {
                    double idealdia = 10;

                    //================== Border File ==================================
                    paperscale = Tool.BestShellScale( uppBorderSizes.DSize_Landscape, ShellRad * 2, idealdia, Drawing.DrawingUnits == uppUnitFamilies.Metric, out string sclrstr, bSuppressAnsiScales: true );
                    Tool.Border( this, null, ref fitRec, arPaperScale: ref paperscale, bSuppressAnsiScales: false, aScaleString: sclrstr );
                    //================== Border File ==================================
                    xCancelCheck();
                    v1 = new dxfVector( fitRec.Left + (2.5 * paperscale + ShellRad), fitRec.Top - ShellRad - 2.0 * paperscale );
                    v2 = new dxfVector( fitRec.Right - (2.5 * paperscale + ShellRad), v1.Y );
                    v0 = v1.Clone();

                    xCancelCheck();
                }

                ////=========================================================== PLAN VIEW
                Status = "Drawing Plan View";
                {
                    Image.UCS.SetCoordinates( v1.X, v1.Y );
                    view1 = Draw_BeamAttachments_View_1( paperscale );
                    rec1 = view1.BoundingRectangle();

                    var l1 = new dxfVector( v1.X, rec1.Bottom - 0.1875 * paperscale );
                    l1.MoveFromTo( v1, dxfVector.Zero );

                    // ****************************** Draw view text **************************************

                    var ttxt = $"\\Fromand;\\LPLAN VIEW OF I-BEAM SUPPORTS\\PFOR TRAYS {Assy.SpanName()}";
                    view1.Add( draw.aText( l1, ttxt, 0.1875 * paperscale, dxxMTextAlignments.TopCenter, aTextStyle: "Standard" ) );

                    var ips = beam.BeamSupportInsertionPoints;
                    var ld1 = new dxfVector(ips[0].X, ips[0].Y);

                    var ld2 = new dxfVector( ld1.X - 60, ld1.Y + 30 );
                    var vecs = new colDXFVectors() { ld1, ld2 };
                    draw.aLeader.Text( vecs[ 0 ], vecs[ 1 ], "I-BEAM SUPPORT\\PSEE DETAIL C2" );
                    rec1 = view1.BoundingRectangle();

                    draw.aSymbol.ViewArrow( dxfVector.Zero.PolarVector( -45, ShellRad * 0.9 ), ShellRad * 2.1, 0.5, "A2", 135, 0.1875, 0.0, dxxArrowStyles.Undefined, true, false, "SECTION_ARROW_A2" );
                    draw.aSymbol.ViewArrow( dxfVector.Zero.PolarVector( 45, ShellRad * 0.9 ), ShellRad * 2.1, 0.5, "B2", 225, 0.1875, 0.0, dxxArrowStyles.Undefined, true, false, "SECTION_ARROW_B2" );

                    xCancelCheck();
                }

                var refPoints = new colDXFVectors();

                ////=========================================================== ELEVATION VIEW 1
                Status = "Drawing Elevation View 1";
                if (rec1 != null)
                {
                    view2 = new colDXFEntities();
                    //locate the view
                    v1 = new dxfVector( v1.X, rec1.Bottom - ShellRad / 2 );
                    Image.UCS.SetCoordinates( v1.X, v1.Y );

                    //define beam support block
                    string bname = "ELEVATION_VIEW1";
                    dxfBlock block = null;

                    block = uopBlocks.Attachements_Elevation_View( Image, Assy.Beam, Assy, bname, out refPoints );
                    Image.Blocks.Add( block );

                    var bref = draw.aInsert( bname, dxfVector.Zero );
                    view2.Add( bref );
                    //refPoints.TransferToPlane( bref.Plane, Image.UCS );

                    //dimensions
                    var bl = refPoints.GetTagged( "BOTTOMLEFTDIM" );
                    var br = refPoints.GetTagged( "BOTTOMRIGHTDIM" );
                    var tt = refPoints.GetTagged( "TOP_OF_TRAY" );
                    view2.Add( dims.Aligned( bl, br, -0.5, aSuffix: " COLUMN ID" ) );
                    view2.Add( dims.Aligned( new dxfVector( bl.X, 0 ), tt, 2.125 ) );

                    //Text
                    double ht = 0.125 * paperscale;
                    double tgap = 0.0625 * paperscale;
                    var l1 = new dxfVector( tt.X - tgap, tt.Y + tgap );
                    view2.Add( draw.aText( l1, "TOP OF\\PSUPPORT RING", ht, dxxMTextAlignments.BottomRight, aTextStyle: "Standard" ) );
                    l1 = new dxfVector( br.X + tgap, tt.Y + tgap );
                    view2.Add( draw.aText( l1, $"ODD TRAYS {uopUtils.SpanName( Assy.RingStart + 1, Assy.RingEnd, uppStackPatterns.Odd, false )}", ht, dxxMTextAlignments.BottomLeft, aTextStyle: "Standard" ) );

                    l1 = new dxfVector( tt.X - tgap, 0 );
                    view2.Add( draw.aText( l1, "TOP OF I-BEAM\\PSUPPORT", ht, dxxMTextAlignments.MiddleRight, aTextStyle: "Standard" ) );
                    l1 = new dxfVector( br.X + tgap, 0 + tgap );
                    view2.Add( draw.aText( l1, $"EVEN TRAYS {uopUtils.SpanName( Assy.RingStart, Assy.RingEnd, uppStackPatterns.Even, false )}", ht, dxxMTextAlignments.BottomLeft, aTextStyle: "Standard" ) );

                    rec2 = view2.BoundingRectangle( bIncludeSuppressed: true );

                    var l2 = new dxfVector( v1.X, rec2.Bottom - 0.1875 * paperscale );
                    l2.MoveFromTo( v1, dxfVector.Zero );
                    var ttxt = $"\\Fromand;\\LELEVATION VIEW A2 - A2";
                    view2.Add( draw.aText( l2, ttxt, 0.1875 * paperscale, dxxMTextAlignments.TopCenter, aTextStyle: "Standard" ) );
                    rec2 = view2.BoundingRectangle( bIncludeSuppressed: true );

                }

                ////=========================================================== ELEVATION VIEW 2
                Status = "Drawing Elevation View 2";
                if (rec2 != null)
                {
                    view3 = new colDXFEntities();
                    //locate the view
                    v1 = new dxfVector( v1.X, rec2.Bottom - ShellRad / 2 );
                    Image.UCS.SetCoordinates( v1.X, v1.Y );

                    //define beam support block
                    string bname = "ELEVATION_VIEW2";
                    dxfBlock block = null;

                    block = uopBlocks.Attachements_Elevation_View( Image, Assy.Beam, Assy, bname, out refPoints, false );
                    Image.Blocks.Add( block );

                    view3.Add( draw.aInsert( bname, dxfVector.Zero ) );

                    //dimensions
                    var bl = refPoints.GetTagged( "BOTTOMLEFTDIM" );
                    var br = refPoints.GetTagged( "BOTTOMRIGHTDIM" );
                    var tt = refPoints.GetTagged( "TOP_OF_TRAY" );
                    view3.Add( dims.Aligned( bl, br, -0.5, aSuffix: " COLUMN ID" ) );
                    view3.Add( dims.Aligned( new dxfVector( bl.X, 0 ), tt, 2.125 ) );

                    //Text
                    double ht = 0.125 * paperscale;
                    double tgap = 0.0625 * paperscale;
                    var l1 = new dxfVector( tt.X - tgap, tt.Y + tgap );
                    view3.Add( draw.aText( l1, "TOP OF\\PSUPPORT RING", ht, dxxMTextAlignments.BottomRight, aTextStyle: "Standard" ) );
                    l1 = new dxfVector( br.X + tgap, tt.Y + tgap );
                    view3.Add( draw.aText( l1, $"TRAY {Assy.RingStart}", ht, dxxMTextAlignments.BottomLeft, aTextStyle: "Standard" ) );

                    l1 = new dxfVector( tt.X - tgap, 0 );
                    view3.Add( draw.aText( l1, "TOP OF I-BEAM\\PSUPPORT", ht, dxxMTextAlignments.MiddleRight, aTextStyle: "Standard" ) );

                    rec3 = view3.BoundingRectangle( bIncludeSuppressed: true );

                    l1 = new dxfVector( v1.X, rec3.Bottom - 0.1875 * paperscale );
                    l1.MoveFromTo( v1, dxfVector.Zero );
                    var ttxt = $"\\Fromand;\\LELEVATION VIEW A2 - A2";
                    view3.Add( draw.aText( l1, ttxt, 0.1875 * paperscale, dxxMTextAlignments.TopCenter, aTextStyle: "Standard" ) );
                    rec3 = view3.BoundingRectangle( bIncludeSuppressed: true );

                }

                ////=========================================================== ELEVATION VIEW 3
                Status = "Drawing Elevation View 3";
                if (rec1 != null)
                {
                    view4 = new colDXFEntities();
                    //locate the view
                    v1 = new dxfVector( rec1.Right + ShellRad, rec1.Center.Y );
                    Image.UCS.SetCoordinates( v1.X, v1.Y );

                    //define beam support block
                    string bname = "ELEVATION_VIEW3";
                    dxfBlock block = null;

                    if (!Image.Blocks.TryGet( bname, ref block ))
                    {
                        block = uopBlocks.Attachements_Elevation_View( Image, Assy.Beam, Assy, bname, out refPoints, bShowSupportEndView: true );
                        Image.Blocks.Add( block );
                    }

                    view4.Add( draw.aInsert( bname, dxfVector.Zero, 90 ) );
                    rec4 = view4.BoundingRectangle( bIncludeSuppressed: true );

                    draw.aCenterlines( rec4, aSuppressVertical: true );

                    var l1 = new dxfVector( v1.X, rec4.Bottom - 0.1875 * paperscale );
                    l1.MoveFromTo( v1, dxfVector.Zero );
                    var ttxt = $"\\Fromand;\\LELEVATION VIEW B2 - B2";
                    view4.Add( draw.aText( l1, ttxt, 0.1875 * paperscale, dxxMTextAlignments.TopCenter, aTextStyle: "Standard" ) );
                    rec4 = view4.BoundingRectangle( bIncludeSuppressed: true );

                }

                ////=========================================================== BEAM SUPPORT DETAIL VIEW
                Status = "Drawing Beam Support Detail View";
                if (rec4 != null)
                {
                    colDXFEntities topView;
                    colDXFEntities rightView;
                    colDXFEntities frontView;
                    dxfRectangle toprec = null;
                    dxfRectangle rightrec = null;
                    dxfRectangle frontrec = null;

                    //Locate center of detail views
                    double aSpace = fitRec.Right - rec4.Right;
                    var dcenter = new dxfVector( rec4.Right + aSpace * 0.5, rec4.Center.Y );
                    Image.UCS.SetCoordinates( dcenter.X, dcenter.Y );
                    int scl = 4;

                    ////Top View
                    topView = new colDXFEntities();
                    string bname = "BEAM_SUPPORT_PLAN_VIEW";
                    dxfBlock block = null;

                    if (!Image.Blocks.TryGet( bname, ref block ))
                    {
                        block = mdBlocks.BeamSupport_View_Plan( Image, Assy.Beam, Assy, bname );
                        Image.Blocks.Add( block );
                    }

                    dxeInsert topIns = draw.aInsert( bname, dxfVector.Zero, 0, aScaleFactor: scl, aYScale: -scl );
                    topView.Add( topIns );
                    toprec = topView.BoundingRectangle();
                    topView.Add( draw.aCenterlines( toprec, 0.125 * paperscale, aSuppressVertical: true ).First() );

                    //Hole leader
                    var slotPt = block.Entities.GetByTag( "BOTTOM", "LEFT" ).First(); //NOTE BLOCK IS MIRRORED ABOUT X AXIS
                    var slot = (dxePolyline)block.Entities.GetByNearestDefPoint( slotPt.DefinitionPoint( dxxEntDefPointTypes.Center ), dxxEntDefPointTypes.Center,aGraphicType: dxxGraphicTypes.Polyline );
                    dxeArc arc = (dxeArc)slot.Segments.Nearest( slotPt.DefinitionPoint( dxxEntDefPointTypes.Center ), aEntPointType: dxxEntDefPointTypes.Center, aEntityType: dxxEntityTypes.Arc, bReturnClone: true );

                    string ldtxt = Tool.HoleLeader( Image, slot, 2, true );

                    //move and scale the arc to align with the block (equivilent of using BlockTransform in AutoCAD)
                    BlockTransform( arc, topIns );

                    var leaderTextPoint = new dxfVector( slotPt.X - .5 * paperscale, slotPt.Y + 1 * paperscale );
                    leaderTextPoint.MoveFromTo( dxfVector.Zero, dcenter );

                    double pang = Image.UCS.XDirection.AngleTo( arc.Center.DirectionTo( leaderTextPoint ) );
                    topView.Add( dims.RadialD( arc, pang, arc.Center.DistanceTo( leaderTextPoint ), aOverideText: ldtxt ) );

                    //Shell radius. Assumes first polyline is the plate
                    dxePolyline basePl = (dxePolyline)block.Entities[ 0 ].Clone();
                    dxeArc shellArc = basePl.ArcSegments().First();
                    BlockTransform( shellArc, topIns );
                    ldtxt = Tool.RadiusLeaderText( Image, Drawing.DisplayUnits, ShellRad, false );

                    //Radial dim tool doesn't function correctly for this case went with a manual approach.
                    dxeLine ldrline = new dxeLine( shellArc.Center, new dxfVector( Math.Min( shellArc.StartPt.X, shellArc.EndPt.X ) - 0.5 * paperscale, topIns.InsertionPt.Y + 0.25 * paperscale ) );
                    var ints = shellArc.Intersections( ldrline, true, true );
                    var lpt = ints.GetVector( dxxPointFilters.AtMinX ).Clone();
                    var tpt = ldrline.EndPt;
                    //Not sure why the moves are requried here
                    lpt.MoveFromTo( dcenter, dxfVector.Zero );
                    tpt.MoveFromTo( dcenter, dxfVector.Zero );
                    var shellLdr = leaders.Text( lpt, tpt, ldtxt );
                    shellLdr.MText.Alignment = dxxMTextAlignments.MiddleRight;
                    topView.Add( shellLdr );

                    //weld symbol
                    double ht = 0.125;
                    var wpt = ints.GetVector( dxxPointFilters.AtMinX );
                    wpt = new dxfVector( wpt.X, dcenter.Y - (wpt.Y - dcenter.Y) );

                    //re aquire intersection with shell arc
                    ldrline = new dxeLine( shellArc.Center, wpt );
                    ints = shellArc.Intersections( ldrline, true, true );
                    wpt = ints.GetVector( dxxPointFilters.AtMinX );

                    var wpt2 = new dxfVector( wpt.X - 0.5 * paperscale, wpt.Y - 0.5 * paperscale );
                    topView.Add( leaders.NoRef( wpt, wpt2, bSuppressUCS: true ) );
                    topView.Add( Image.SymbolTool.Weld( wpt2, dxxWeldTypes.Fillet, ht, aAngle: 180, bBothSides: true, bAllAround: true, Side1Dims: "10", NoteText: "TO WALL", bSuppressUCS: true ) );

                    //dimensions
                    Image.DimStyleOverrides.AutoReset = false;
                    Image.DimStyleOverrides.LinearScaleFactor = Drawing.DrawingUnits == uppUnitFamilies.Metric ? (25.4 / scl) : scl;
                    Image.DimStyleOverrides.DimTextMovement = dxxDimTextMovementTypes.TextWithLeader;
                    BlockTransform( basePl, topIns );
                    var bl = basePl.Vertices.GetVector( dxxPointFilters.GetBottomLeft );
                    var br = basePl.Vertices.GetVector( dxxPointFilters.GetBottomRight );
                    topView.Add( dims.Aligned( bl, br, -0.75, bSuppressUCS: true ) );

                    slotPt = block.Entities.GetByTag( "TOP", "LEFT" ).First().Clone(); //NOTE BLOCK IS MIRRORED ABOUT X AXIS
                    BlockTransform( slotPt, topIns );
                    var hpt = slotPt.DefinitionPoint( dxxEntDefPointTypes.Center );
                    topView.Add( dims.Horizontal( hpt, br, -0.25, bSuppressUCS: true ) );

                    var cpt = new dxfVector( br.X, dcenter.Y );
                    topView.Add( dims.Vertical( hpt, cpt, 0.25, aSuffix: " (TYP)", bSuppressUCS: true ) );
                    topView.Add( dims.Vertical( br, cpt, 1.35, bSuppressUCS: true ) );


                    //shift top vew left using its bounds
                    toprec = topView.BoundingRectangle();
                    topView.Move( toprec.Width * -0.45 );
                    shellLdr.MText.Alignment = dxxMTextAlignments.MiddleRight;

                    ////Right View
                    rightView = new colDXFEntities();
                    bname = "BEAM_SUPPORT_ELEVATION_END_VIEW";
                    block = null;

                    if (!Image.Blocks.TryGet( bname, ref block ))
                    {
                        block = mdBlocks.BeamSupport_View_Elevation_End( Image, Assy.Beam, Assy, bname, false, false );
                        Image.Blocks.Add( block );
                    }

                    dxeInsert rightIns = draw.aInsert( bname, dxfVector.Zero, 90, aScaleFactor: scl );
                    rightView.Add( rightIns );

                    //Weld Symbol
                    var topweld = block.Entities.GetByTag( "WELD", bContainsString: true )
                        .Where( solid => solid.Centers != null && solid.Centers.Any() )
                        .OrderByDescending( solid => solid.Centers.Max( center => center.X ) )
                        .FirstOrDefault();

                    wpt = topweld.Centers.First();
                    BlockTransform( wpt, rightIns );
                    wpt2 = new dxfVector( wpt.X + 0.5 * paperscale, wpt.Y + 0.5 * paperscale );
                    rightView.Add( leaders.NoRef( wpt, wpt2, bSuppressUCS: true ) );
                    rightView.Add( Image.SymbolTool.Weld( wpt2, dxxWeldTypes.Fillet, ht, bBothSides: true, Side1Dims: "10", Side2Dims: "10", NoteText: "TYP", bSuppressUCS: true ) );

                    //Dimensions
                    var topPl = block.Entities[ 0 ].Clone();
                    BlockTransform( topPl, rightIns );
                    var tl = topPl.Vertices.GetVector( dxxPointFilters.GetLeftTop );
                    bl = topPl.Vertices.GetVector( dxxPointFilters.GetLeftBottom );
                    rightView.Add( dims.Vertical( bl, tl, -0.5, bSuppressUCS: true ) );

                    br = topPl.Vertices.GetVector( dxxPointFilters.GetRightBottom );
                    rightView.Add( dims.Horizontal( bl, br, -0.25, aSuffix: " (TYP)", bSuppressUCS: true ) );

                    var legs = block.Entities.GetByEntityType( dxxEntityTypes.Polyline, true )
                        .Where( p => ((dxePolyline)p).Closed == false ).ToList();

                    var leftleg = legs.OrderBy( p => p.Vertices.GetVector( dxxPointFilters.GetLeftBottom )?.X ?? double.MaxValue ).FirstOrDefault();


                    BlockTransform( leftleg, rightIns );
                    var br2 = leftleg.Vertices.GetVector( dxxPointFilters.GetRightBottom );
                    rightView.Add( dims.Horizontal( bl, br2, -0.75, bSuppressUCS: true ) );

                    rightView.Add( dims.Vertical( br, br2, 0.5, bSuppressUCS: true, aSuffix: " (TYP)" ) );

                    //shift right view right using its bounds
                    rightrec = rightView.BoundingRectangle();
                    rightView.Move( rightrec.Width * 0.45 );

                    ////Front View
                    frontView = new colDXFEntities();
                    bname = "BEAM_SUPPORT_ELEVATION_VIEW";
                    block = null;

                    if (!Image.Blocks.TryGet( bname, ref block ))
                    {
                        block = mdBlocks.BeamSupport_View_Elevation( Image, Assy.Beam, Assy, bname, out _, false, false );
                        Image.Blocks.Add( block );
                    }

                    var frontIns = draw.aInsert( bname, dxfVector.Zero, 0, aScaleFactor: scl );
                    frontView.Add( frontIns );

                    mdBeamSupport support = beam.BeamSupport();
                    var topverts = BlockTransform(support.GetTopLegElevationVerticies(), frontIns );
                    var midverts = BlockTransform(support.GetMidLegElevationVerticies(), frontIns );
                    var botverts = BlockTransform(support.GetBottomLegElevationVerticies(), frontIns );

                    double minx = topverts.Concat( midverts ).Concat( botverts ).Min( v => v.X );
                    double maxx = topverts.Concat( midverts ).Concat( botverts ).Max( v => v.X );
                    double miny = topverts.Concat( midverts ).Concat( botverts ).Min( v => v.Y );
                    double maxy = topverts.Concat( midverts ).Concat( botverts ).Max( v => v.Y );

                    //dimensions
                    br = botverts.GetByTag( "AngleEnd" ).First();
                    frontView.Add( dims.Horizontal( new dxfVector( minx, maxy ), new dxfVector( maxx, maxy ), 0.25, bSuppressUCS: true ) );
                    frontView.Add( dims.Vertical( br, new dxfVector( maxx, maxy ), 0.25, bSuppressUCS: true ) );

                    var leftFoot = topverts.GetVectors( dxxPointFilters.AtMinY );
                    frontView.Add( dims.Horizontal( leftFoot[ 1 ], leftFoot[ 0 ], -0.25, bSuppressUCS: true, aSuffix: " (TYP)" ) );

                    frontView.Add( dims.Horizontal( botverts.GetVector( dxxPointFilters.GetBottomLeft ), br, -1.5, bSuppressUCS: true ) );
                    frontView.Add( dims.Horizontal( midverts.GetVector( dxxPointFilters.GetBottomLeft ), br, -1.75, bSuppressUCS: true ) );

                    if (beam.OccuranceFactor > 1)
                        frontView.Add( dims.Horizontal( topverts.GetVector( dxxPointFilters.GetBottomLeft ), br, -2.0, bSuppressUCS: true ) );

                    //shift front view down and left using top view bounds
                    frontrec = frontView.BoundingRectangle();
                    frontView.Move( toprec.Width * -0.45, -((toprec.Height / 2) + frontrec.Height ) );
                    frontrec = frontView.BoundingRectangle();

                    var l1 = new dxfVector( dcenter.X, frontrec.Bottom - 0.1875 * paperscale );
                    l1.MoveFromTo( dcenter, dxfVector.Zero );

                    string ttxt = "";

                    if (beam.OccuranceFactor == 1)
                        ttxt = $"\\Fromand;\\LDETAIL C2\\PI-BEAM SUPPORT\\P\\Fromans|c0;\\H0.6667x;\\lQTY: {Assy.Beam.SupportQuantity}\\PSCALE: {scl}X";
                    else
                        ttxt = $"\\Fromand;\\LDETAIL C2\\PI-BEAM SUPPORT\\P\\Fromans|c0;\\H0.6667x;\\lQTY: {Assy.Beam.SupportQuantity / 2}, {Assy.Beam.SupportQuantity / 2} OPPOSITE HAND\\PSCALE: {scl}X";

                    frontView.Add( draw.aText( l1, ttxt, 0.1875 * paperscale, dxxMTextAlignments.TopCenter, aTextStyle: "Standard" ) );
                }


                Image.UCS.Reset();

            }
            catch (Exception e)
            {
                HandleError( System.Reflection.MethodBase.GetCurrentMethod(), e );
            }
            finally
            {
                //Image.UCS.Reset();
                DrawinginProgress = false;
                Status = "";
                Drawing.ZoomExtents = false;
                Image.UCS.Reset();


                if (DrawlayoutRectangles)
                {

                    draw.aPolyline( rec1, new dxfDisplaySettings( "0", dxxColors.LightCyan ) );
                    draw.aPolyline( rec2, new dxfDisplaySettings( "0", dxxColors.LightCyan ) );
                    draw.aPolyline( rec3, new dxfDisplaySettings( "0", dxxColors.LightCyan ) );
                    draw.aPolyline( rec4, new dxfDisplaySettings( "0", dxxColors.LightCyan ) );
                }
            }
        }

        private void DDWG_SectionSketch()
        {
            // #1flag to request a return of the created DXF file
            // ^generates the Tray Sketch drawing shown on the Cross Flow input screen.

            if (Image == null || Assy == null)
            {
                return;
            }

            try
            {
                Image.DimStyle().TextHeight = 0.125;
                Image.DimStyle().SetGapsAndOffsets(0.03, 0.09);
                Image.DimStyle().SetColors(dxxColors.Blue, dxxColors.BlackWhite);
                Image.DimSettings.DimLayerColor = dxxColors.Blue;

                mdDowncomer aDComer = Assy.Downcomer();
                dxfVector p1 = dxfVector.Zero;
                dxfVector p2 = dxfVector.Zero;

                double thk = aDComer.Thickness;
                double dkthk = Assy.Deck.Thickness;
                double wd = aDComer.OutsideWidth;
                double ht = aDComer.OutsideHeight;
                double lg = 4 * wd;
                double ringSpc = Assy.RingSpacing;
                double how = aDComer.How;
                double apht = 0;
                double apthk = 0;
                colDXFVectors aPts = new(new dxfVector(lg, 0), dxfVector.Zero, new dxfVector(0, ht), new dxfVector(lg, ht));
                colDXFVectors bPts = new(new dxfVector(lg, thk), new dxfVector(thk, thk), new dxfVector(thk, ht));
                colDXFVectors cPts = new(new dxfVector(-wd, ht - how), new dxfVector(0, ht - how), new dxfVector(0, ht - how - dkthk), new dxfVector(-wd, ht - how - dkthk));
                colDXFVectors dPts = cPts.Clone();
                colDXFVectors fPts;
                colDXFVectors ePts;
                colDXFVectors apVerts = null;
                dxoDrawingTool draw = Image.Draw;
                dxoDimTool dims = Image.DimTool;
                dxeDimension dim = null;

                if (Assy.DesignOptions.HasAntiPenetrationPans)
                {
                    mdAPPan pan = new mdAPPan(Assy);
                    apht = pan.Height;
                    apthk = pan.Thickness;
                }


                Image.Header.Color = dxxColors.BlackWhite;

                dxfDisplaySettings dsp = new("0", dxxColors.BlackWhite, dxfLinetypes.Continuous);
                draw.aPolyline(aPts, false, aDisplaySettings: dsp);
                draw.aPolyline(bPts, false, aDisplaySettings: new dxfDisplaySettings("0", dxxColors.Green, dxfLinetypes.Hidden));

                //cPts.Add(-wd, ht - how);
                //cPts.Add(0, ht - how);
                //cPts.Add(0, ht - how - dkthk);
                //cPts.Add(-wd, ht - how - dkthk);
                draw.aPolyline(cPts, false, aDisplaySettings: dsp);

                //dPts = cPts.Clone();
                dPts.Move(aChangeY: -ringSpc);
                dPts.Item(2).Move(0.25 * wd);
                dPts.Item(3).Move(0.25 * wd);
                draw.aPolyline(dPts, false, aDisplaySettings: dsp);

                ePts = new colDXFVectors(dPts.Item(2).Moved(thk / 2, how));
                ePts.AddRelative(aY: -ht + thk / 2);
                ePts.AddRelative(wd - thk);
                ePts.AddRelative(aY: ht - thk / 2);
                draw.aPolylineTrace(ePts, thk, false, aLayerName: "0", aColor: dxxColors.BlackWhite, aLineType: dxfLinetypes.Continuous);

                fPts = dPts.Clone();
                fPts.Mirror(dPts.LineSegment(2));
                fPts.Move(wd);
                fPts.Item(1).X = lg;
                fPts.Item(4).X = lg;
                draw.aPolyline(fPts, false, aDisplaySettings: dsp);

                Image.Display.ZoomExtents(1.3, false);
                Image.DimStyle().FeatureScaleFactor = Image.Display.PaperScale;
                Image.DimStyle().TextHeight = 0.125;

                dxsDimOverrides dimorides = Image.DimStyleOverrides;
                dimorides.FeatureScaleFactor = Image.DimStyle().FeatureScaleFactor;
                dimorides.AutoReset = true;
                // ring space
                dim = dims.Vertical(cPts.Item(4), dPts.Item(4), -0.55, 0, aSuffix: "\\PRING SPACE");

                // dc below deck
                dimorides.SetSuppressionFlags(aSupExtLn1: true, aSupExtLn2: false);
                dim = dims.Vertical(cPts.Item(3), aPts.Item(2), -0.35, 0);

                // wier height
                dimorides.SetSuppressionFlags(aSupExtLn1: true, aSupExtLn2: false);
                dim = dims.Vertical(dPts.Item(2), ePts.Item(1).Moved(-thk / 2), -0.3, 0.1);

                // deck thickness
                dim = dims.Vertical(dPts.Item(4), dPts.Item(1), -0.35);

                // dc thickness
                dimorides.SetSuppressionFlags(aSupExtLn1: true, aSupExtLn2: true);
                dim = dims.Horizontal(ePts.Item(2).Moved(-thk / 2, 4 * thk), ePts.Item(2).Moved(thk / 2, 4 * thk), 0.15);

                // dc inside width
                dimorides.SetSuppressionFlags(aSupExtLn1: false, aSupExtLn2: false);
                dim = dims.Horizontal(ePts.Item(2).Moved(thk / 2, -thk / 2), ePts.Item(3).Moved(-thk / 2, -thk / 2), -0.3, 0, aSuffix: "\\PINSIDE");

                // deck below to dc bottom
                dim = dims.Vertical(aPts.Item(1), fPts.Item(1), 0.35);

                if (apht != 0)
                {
                    thk = Assy.GetSheetMetal(false).Thickness;  // the AP Pan material
                    apVerts = new colDXFVectors(new dxfVector(0.5 * lg - 0.5 * wd + thk / 2, 0));
                    apVerts.AddRelative(aY: -apht + apthk / 2);
                    apVerts.AddRelative(wd - apthk);
                    apVerts.AddRelative(aY: apht - apthk / 2);

                    draw.aPolylineTrace(apVerts, apthk, false);

                    p1 = apVerts.LastVector().Moved(apthk / 2);
                    p2 = p1.Moved(aYChange: -apht + apthk);

                    // inside ap height
                    dimorides.SetSuppressionFlags(aSupExtLn1: true, aSupExtLn2: false);
                    dim = dims.Vertical(p1, p2, 0.4, 0, "", aSuffix: "\\PINSIDE");

                    // ap thickness
                    p1 = p2.Moved(aYChange: -apthk);
                    dim = dims.Vertical(p1, p2, 0.6);

                    dimorides.SetSuppressionFlags(aSupExtLn1: true, aSupExtLn2: true);
                    p1.Move(-0.5 * wd);
                    p2.SetCoordinates(p1.X, fPts.Item(1).Y);

                    // clearance dim
                    dim = dims.Vertical(p1, p2, 0.4);
                }

                p1 = Image.Display.ExtentRectangle.BottomCenter.Moved(aYChange: -0.5 * Image.Display.PaperScale);
                draw.aText(p1, Assy.TrayName(true).ToUpper(), Image.Display.PaperScale * 0.25, dxxMTextAlignments.TopCenter, aLayer: "0", aColor: dxxColors.BlackWhite);
                Image.Display.ZoomExtents();

                DrawinginProgress = false;
            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
        }

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
                Draw_Shells(uppViews.Plan);
                Draw_Rings(uppViews.Plan);
                if (Assy.DesignFamily.IsBeamDesignFamily()) Draw_Beams(uppViews.Plan);
                if (Drawing.Options.AnswerB("Include End Plates?", false)) Draw_EndPlates(uppViews.Plan);

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

        private void DDWG_StartupLines()
        {
            // ^this function Included for test purposes.


            dxfVector v1;
            dxfVector p1;
            dxfVector p2 = dxfVector.Zero;

            mdSpoutGroup sGroup;
            mdStartupLines suLines;
            mdStartupLine aLine;
            dxePolyline maxBound;
            double paperscale;
            double dst;
            mdDowncomer aDC;
            dxePolyline aLineMarker;

            mdConstraint aCNS;


            try
            {

                dxoDrawingTool draw = Image.Draw;
                colMDSpoutGroups sGroups = Assy.SpoutGroups;
                List<mdDowncomer> DComers = Assy.Downcomers.GetByVirtual(aVirtualValue: false);
                dxfRectangle aRec = sGroups.BoundingRectangle().Expanded(1.05);
                dxfDisplaySettings dsp;
                Image.DimSettings.LeaderLayer = "";
                Image.DimSettings.DimLayer = "";

                Image.Display.SetDisplayWindow(aRec, true);
                dsp = dxfDisplaySettings.Null(aColor: dxxColors.Red);
                dxfDisplaySettings dsp2 = dxfDisplaySettings.Null(aColor: dxxColors.Blue);
                for (int i = 1; i <= DComers.Count; i++)
                {
                    aDC = DComers[i - 1];

                    draw.aEntity(new dxeLine(aDC.StartUpLimitLine), dsp);
                    draw.aEntity(new dxeLine(aDC.GetLimitLines(0.25 + 0.375).First().Line1), dsp2);
                    Image.Entities.Append(aDC.Boxes.First().Edges("0", dxxColors.BlackWhite, dxfLinetypes.Continuous, aBotYLimit: -Assy.DowncomerSpacing / 2)); // This needs to be modified to support multiple boxes
                }

                dsp = new dxfDisplaySettings("0", dxxColors.LightMagenta, dxfLinetypes.Continuous);
                for (int i = 1; i <= sGroups.Count; i++)
                {
                    sGroup = sGroups.Item(i);
                    if (sGroup.IsVirtual) continue;
                    aCNS = sGroup.Constraints(Assy);
                    maxBound = sGroup.StartupBound(true).Perimeter("0", dxxColors.LightGreen, dxfLinetypes.Continuous);
                    Image.Entities.Add(maxBound);
                    Image.Entities.Add(sGroup.SpoutBoundary.Perimeter("0", dxxColors.LightGrey, dxfLinetypes.Continuous));
                    draw.aCircle(sGroup.SpoutCenter, 0.375, dsp);
                }

                // ---------------------------------------
                Image.LinetypeLayers.Setting = dxxLinetypeLayerFlag.Suppressed;

                Image.Display.ZoomExtents(1.05, true);
                Drawing.ZoomExtents = false;


                suLines = mdStartUps.Lines_Basic(sGroups, Assy, uppStartupSpoutConfigurations.TwoByFour, bRegenBoundsAndLines: true);
                paperscale = Image.Display.PaperScale;
                Image.TextSettings.LayerName = "";
                Image.Display.SetFeatureScales(0.5 * paperscale);
                dst = 0.08 * paperscale;

                for (int i = 1; i <= suLines.Count; i++)
                {
                    aLine = suLines.Item(i);
                    aLineMarker = aLine.DisplayLine(dst);
                    Image.Entities.Add(aLineMarker);

                    Image.DimStyleOverrides.TextColor = aLine.Color;
                    Image.DimStyleOverrides.DimLineColor = aLine.Color;


                    uopVector u1 = aLine.ReferencePt;
                    if (u1 != null)
                    {
                        
                        draw.aCircle(aLine.ReferencePt, 0.375, dxfDisplaySettings.Null(aColor: aLine.Color));
                        p1 = new dxfVector(aLine.MidPt);
                        double dx = aLine.LeftSide ? -1 : 1;
                        double dy = aLine.TopSide ? 1 : -1;
                        p2.MoveTo(p1, 0.2 * paperscale * dx, -0.2 * paperscale * dy);
                        draw.aLeader.Text(p1, p2, $"{aLine.Tag};{u1.Tag}", null, aLayer: "LEADERS", aColor: aLine.Color, aLineType: aLineMarker.Linetype); // aMidXY was missing!!! we used null for now
                    }
                }

                DrawinginProgress = false;
            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = string.Empty; }
        }

        private void AADrawTest()
        {
            if (MDProject == null) return;
            MDRange ??= MDProject.SelectedRange;

            if (Assy == null) return;
            dxoDrawingTool draw = Image.Draw;
            double paperscale = 0;
           
            try
            {

                //string txt = string.Empty;
                //uopVector u1 = uopVector.Zero;
                //dxoEntityTool ents = Image.EntityTool;
                //string tname = Assy.TrayName();
                //DowncomerDataSet dcdata = Assy.DowncomerData;

                ////Image.UCS.Rotate(90);
                //Image.UCS.Move(2 * Assy.ShellID, Assy.ShellRadius);

                //Draw_FunctionalPlanView(out _, out _, out _, out _);
                
                dxeArc shell  = draw.aCircle(null, Assy.ShellRadius , dxfDisplaySettings.Null(aColor: dxxColors.LightGrey));
                dxeArc ring =  draw.aCircle(null, Assy.RingRadius, dxfDisplaySettings.Null(aColor: dxxColors.Blue,aLinetype: dxfLinetypes.Hidden));

                paperscale = Image.Display.ZoomExtents(1.1, true);

                draw.aCenterlines(shell, paperscale * 0.25);
                MDProject.InvalidateParts(false, Assy.RangeGUID, uppPartTypes.DeckSections);
                List<mdDeckSection> sections = MDProject.GetParts().AltDeckSections(Assy.RangeGUID);
                Draw_Installation_PlanView(aViewIndex: 3, paperscale, out _, out _);
                //Draw_DeckSections(uppViews.LayoutPlan, bSuppressSlots: true, bShowPns: true,  aViewIndex:2, bShowQuantities:true);
                //Draw_SpliceAngles(uppViews.Plan, out _, true, aViewIndex:2);
                //uopPanelSectionShapes panelshapes = dcdata.CreatePanelShapes(dcdata.PanelClearance, dcdata.DeckLap, out List<uopLine> panellines, out List<uopLinePair> weirs, bReturnVirtuals: false, bSetBPSites: true);

                //foreach (var panel in panelshapes)
                //    Image.Draw.aShape(panel, aInstances: null);

                if (Assy.DesignFamily.IsBeamDesignFamily())
                {
                    dxfBlock beam = mdBlocks.Beam_View_Plan(Image, Assy.Beam, Assy, true, true, true, aLayerName: "BEAMS");
                    draw.aInserts(beam, beam.Instances, false);
                }
            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = string.Empty; DrawinginProgress = false; }
        }

        private void BeamDrawTest()
        {
            try
            {
                if (Image == null) return;
                dxoDrawingTool draw = Image.Draw;

                Image.Display.SetDisplayWindow( 2.3 * MDRange.ShellID * 0.5, null, aDisplayHeight: 2.3 * MDRange.ShellID * 0.5, bSetFeatureScales: true );

                var beam = Assy.Beam;

                //define beam support block
                string bname = "BEAM_SUPPORT_PLAN_VIEW";
                dxfBlock block = null;
                uopVectors ips = beam.BeamSupportInsertionPoints;

                if (!Image.Blocks.TryGet( bname, ref block ))
                {
                    block = mdBlocks.BeamSupport_View_Plan( Image, beam, Assy, bname,bObscured: false,  bSuppressHoles: false );
                   block =  Image.Blocks.Add( block );
            
                }
                bname = block.Name;
                dxePolygon pg = beam.View_Plan( dxfVector.Zero, bShowObscured: true );
                pg.BlockName = "SUPPORT_BEAM";
                dxeInsert insert = draw.aInsert( pg, beam.Center, aDisplaySettings: dxfDisplaySettings.Null( aLinetype: dxfLinetypes.Hidden ), aLTLSetting: dxxLinetypeLayerFlag.ForceToColor );
                //insert beam supports

                draw.aInsert( bname, ips[0], beam.Rotation, aDisplaySettings: insert.DisplaySettings );
                draw.aInsert( bname, ips[1], beam.Rotation + 180, aYScale: -1, aDisplaySettings: insert.DisplaySettings );

                if (beam.OccuranceFactor > 1)
                {
                    insert = draw.aInsert( insert.BlockName, new dxfVector( -beam.X, -beam.Y ), aDisplaySettings: insert.DisplaySettings );
                    //insert beam supports
                    ips.Move(-2 * beam.X, -2 * beam.Y);
           
                    draw.aInsert( bname, ips[0], beam.Rotation, aYScale: -1, aDisplaySettings: insert.DisplaySettings );
                    draw.aInsert( bname, ips[1], beam.Rotation + 180, aDisplaySettings: insert.DisplaySettings );
                }

                draw.aArc( new dxfVector(), Assy.ShellID / 2.0 );


                //Draw_View_Elevation( uppViews.InstallationElevation, out _ );
                Image.UCS.Reset();

            }
            catch (Exception e) { HandleError( System.Reflection.MethodBase.GetCurrentMethod(), e ); }
            finally { Status = string.Empty; DrawinginProgress = false; }
        }

        private void DDWG_SectionShapes()
        {
            // ^this function Included for test purposes.
            if (MDProject == null) return;
            MDRange ??= MDProject.SelectedRange;

            if (Assy == null) return;
            dxoDrawingTool draw = Image.Draw;
            double paperscale = 0;

            try
            {
                
                bool regenshapes = Drawing.Options.AnswerB("Regenerate Section Shapes?", uopUtils.RunningInIDE);
                bool regenperims = Drawing.Options.AnswerB("Regenerate Perimeters?", uopUtils.RunningInIDE);
                bool drawrcarcs = Drawing.Options.AnswerB("Show Ring Clip Segments?", true);
                bool uniques = Drawing.Options.AnswerB("Only Unique Sections?", true);
                bool bothsides = Drawing.Options.AnswerB("Draw Both Sides?", false);
                bool perimeters = Drawing.Options.AnswerB("Show Perimeters?", true);
                bool baseshapes = Drawing.Options.AnswerB("Show Section Shapes?", true);
                bool mechbounds = Drawing.Options.AnswerB("Show Mechanical Bound?", true);
                bool freebubblingreas = Drawing.Options.AnswerB("Show Free Bubbling Areas?", false);
                bool slotbounds = Drawing.Options.AnswerB("Show Slot Grid Bounds?", false);
                if (baseshapes) Image.Layers.Add("BASE_SHAPES");
                if (drawrcarcs) Image.Layers.Add("RING CLIP SEGMENTS", dxxColors.DarkGrey);
                if (mechbounds) Image.Layers.Add("MECHANICAL_BOUNDS", dxxColors.LightGrey);

                dxeArc ring = draw.aCircle(null, Assy.ShellRadius, dxfDisplaySettings.Null("SHELL",aColor: dxxColors.DarkGrey));
                paperscale = Image.Display.ZoomExtents(bSetFeatureScale: true);
                paperscale *= 0.5;
                Image.Display.SetFeatureScales(paperscale);
                draw.aCenterlines(ring, 0.2 * paperscale);

                draw.aCircle(null, Assy.BoundingRadius, dxfDisplaySettings.Null(aColor: dxxColors.Red, aLinetype: dxfLinetypes.Hidden));
                ring = draw.aCircle(null, Assy.RingRadius, dxfDisplaySettings.Null("RING", aColor: dxxColors.DarkGreen));

                dxfBlock blok = null;
                uopSectionShape section = null;
               
                string txt = string.Empty;
                uopVector u1 = uopVector.Zero;

                dxoEntityTool ents = Image.EntityTool;
                uopSectionShapes sections = Assy.DeckSections.BaseShapes(false, Assy, bGetUniques: uniques, bRegenShapes: regenshapes);

                dxfDisplaySettings dsp_shp = dxfDisplaySettings.Null("BASE_SHAPES", dxxColors.Blue, dxfLinetypes.Continuous);
                double manid = Assy.ManholeID;

                Status = "Drawing Downcomers";
                foreach (mdDowncomer dc in Assy.Downcomers)
                {
                    var boxes = dc.Boxes;
                    foreach (var box in boxes) 
                        draw.aInserts(mdBlocks.DowncomerBox_View_Plan(Image, box, Assy,bSetInstances:bothsides,  bSuppressHoles: true, bShowObscured: true, aLayerName:"DOWNCOMERS", bIncludeStiffeners: false, bIncludeSupDefs: false, bIncludeBaffles: false), null, bOverrideExisting: false);
                }

                if (Assy.DesignFamily.IsBeamDesignFamily())
                {
                    Status = "Drawing Beams";
                    blok = mdBlocks.Beam_View_Plan(Image, Assy.Beam, Assy, bSetInstances: true, bObscured:true, aLayerName: "BEAMS");
                    draw.aInserts(blok, blok.Instances, bOverrideExisting: false);
                }

                if (freebubblingreas)
                    Image.Layers.Add("FREE BUBBLING AREAS", dxxColors.LightBlue);
                if (slotbounds)
                {
                    Image.Layers.Add("SLOT GRID BOUNDS", dxxColors.Magenta);
                    Image.Layers.Add("BLOCKED AREAS", dxxColors.Red);
                }
                    
                
                for (int s = 1; s <= sections.Count; s++)
                {
                    section = sections[s - 1];
                    uopCompoundShape sltbounds = null;
                    //if (section.Handle != "2,1") continue;

                    Status = uniques ? $"Drawing Unique {section.Descriptor}" : $"Drawing {section.Descriptor}";
                    if (regenperims) 
                        section.ResetMechanicaBounds();
        
                    dxfBlock block = new dxfBlock($"SECTION_{section.Handle.Replace(",", "_")}") { LayerName ="DECK_SECTIONS" };
                    if (baseshapes) block.Entities.AddShape(section, dsp_shp);

                    if (perimeters)
                    {
                        dxePolygon pgon = section.UpdatePerimeter(regenperims, Assy);
                        pgon.LayerName = "DECK_SECTIONS";
                        block.Entities.AddRange(pgon.SubEntities());
                    }

                    txt = $"{section.Handle}";
                    if (section.IsManway) txt += "\\PMANWAY";
                    if (section.IsHalfMoon) txt += "\\PMOON";

                    //txt += $"\\P{section.TopSpliceType}";
                    //txt += $"\\P{section.BottomSpliceType}";
                    u1 = new uopVector(section.Center);
                    block.Entities.Add(ents.Create_Text(u1, txt, aTextHeight: 0.25 * paperscale, aAlignment: dxxMTextAlignments.MiddleCenter));

                    if (drawrcarcs && (section.LapsRing || section.LapsDivider))
                    {
                        block.Entities.AddRange(uopRingClipSegment.ToDXFEntities(section.RingClipSegments(out _), dxfDisplaySettings.Null("RING CLIP SEGMENTS")));
                    
                    }
                    if (freebubblingreas)
                    {
                            block.Entities.AddShape(section.FreeBubblingArea, dxfDisplaySettings.Null("FREE BUBBLING AREAS"));
                    }

                    if (slotbounds )
                    {
                        sltbounds = section.SlotZoneBounds(out _);
                        block.Entities.AddShape(sltbounds, dxfDisplaySettings.Null("SLOT GRID BOUNDS"));
                        block.Entities.AddShapes(sltbounds.SubShapes, dxfDisplaySettings.Null("BLOCKED AREAS"));
                    

                    if (sltbounds != null && section.Handle == "5,2,1")
                        {
                           // sltbounds = section.SlotZoneBounds(out _);
                            //Image.Entities.AddShape(sltbounds, dxfDisplaySettings.Null("SLOT GRID BOUNDS"));
                            Tool.NumberVectors(draw, sltbounds.Vertices, paperscale);
                            //break;
                        }
                    }
                    if (mechbounds)
                    {
                            block.Entities.Add(section.SimplePerimeter, bAddClone:true, aDisplaySettings: dxfDisplaySettings.Null("MECHANICAL_BOUNDS"));
                    }

                    block.Entities.Move(-u1.X, -u1.Y);
                    uopInstances insts = new uopInstances(section.Instances);
                    if (!bothsides) insts.RemoveAll(x => x.Virtual);
                    block.Instances = insts.ToDXFInstances();
                    Image.LinetypeLayers.ApplyTo(block, dxxLinetypeLayerFlag.ForceToColor);
                    dxeInsert insert =  draw.aInserts(block, block.Instances,bOverrideExisting:false, aLTLSetting: dxxLinetypeLayerFlag.ForceToColor)[0];

                

                }


                dxfRectangle bounds = Image.Display.ExtentRectangle;
                bounds.Rescale(1.05);
                txt = MDRange.Name(true);
                txt += uniques ? " Unique Deck Section Shapes" : " Deck Section Shapes";
                if (bothsides) txt += "\\PTray Wide";

                Image.Draw.aText(bounds.BottomCenter, txt, aAlignment: dxxMTextAlignments.TopCenter);

            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = string.Empty; DrawinginProgress = false; }

        }

   
        private void DDWG_DowncomerFit()
        {
            // ^this function Included for test purposes.

            if (MDProject == null) return;


            mdTrayAssembly aAssy;
            colUOPTrayRanges aRngs;

            colDXFEntities bEnts;
            dxfRectangle aRec;
            dxfRectangle bRec = null;
            double d1;
            List<colDXFEntities> aGroups;
            dxfVector v1;
            dxePolygon aPG;

            dxeArc mAr;
            dxeText aTxt;
            dxeText bTxt;
            string txt;
            mdDowncomer aDC;
            double clrc;

            try
            {
                aGroups = new List<colDXFEntities>();
                aRngs = MDProject.TrayRanges;
                v1 = dxfVector.Zero;
                mAr = new dxeArc();

                aTxt = new dxeText()
                {
                    Alignment = dxxMTextAlignments.MiddleCenter,
                    ImageGUID = Image.GUID,
                    LayerName = "TEXT"
                };


                Image.Layers.Add("FIT_CIRCLES", dxxColors.Blue);
                Image.Layers.Add("MAN_CIRCLES", dxxColors.LightGrey);
                Image.Layers.Add("TEXT", dxxColors.BlackWhite);

                mAr.LCLSet("MAN_CIRCLES", dxxColors.ByLayer);

                for (int i = 1; i <= aRngs.Count; i++)
                {
                    colDXFEntities aEnts = new();
                    aAssy = aRngs.Item(i).GetMDTrayAssembly();
                    aDC = aAssy.Downcomers.ToList().Where(dc => dc.Boxes.Any(b => !b.IsVirtual)).Last();
                    aPG = aDC.View_ManholeFit(aAssy, v1);
                    dxeArc aAr = aPG.ExtentPoints.BoundingCircle();
                    mAr.Radius = aAssy.ManholeID / 2;

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
                }

                v1.SetCoordinates(0, 0);
                int j = 1;
                for (int i = 1; i <= aGroups.Count; i++)
                {
                    colDXFEntities aEnts = aGroups[i - 1];
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

                        if (i > 1)
                        {
                            j = -j;
                        }
                    }

                    bRec = aRec.Clone();
                    bEnts = aEnts;

                    Image.Display.ZoomExtents();
                }


            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = string.Empty; DrawinginProgress = false; }
        }

        private void DDWG_Baffles()
        {
            
            List<mdBaffle> assyParts = MDProject.GetParts().DeflectorPlates(Assy.RangeGUID);
            double paperscale;
            colDXFVectors aPts;

            //colDXFEntities aPGs = new();
            List<mdDowncomerBox> boxes = Assy.Downcomers.Boxes();

            mdStiffener aStf = Assy.Stiffener(Assy.Downcomers.FirstItem());
            colDXFVectors ips = new();
            dxfRectangle aRec;
            double YMin = double.MaxValue;
            double ymax = double.MinValue;
            dxfVector v1;


            dxfBlock stifnerblk;
            bool noNotches = this.Drawing.Options.AnswerB(1, false);
            string bname = String.Empty;
            double f1 = 1;
            try
            {

                dxoDrawingTool draw = Image.Draw;
                dxoDimTool dims = Image.DimTool;

                Image.Display.SetDisplayWindow(new dxfRectangle(1.05 * (ShellRad * 2), 1.05 * ShellRad));

                Image.SelectionSetInit(false);

                dxePolygon aPG = aStf.View_Profile(bShowObscured: false, bVisiblePartOnly: false, aCenter: dxfVector.Zero, aLayerName: "STIFFENERS", bCenterBaffleMount: false);
                Image.LinetypeLayers.ApplyTo(aPG, dxxLinetypeLayerFlag.ForceToColor);
                stifnerblk = Image.Blocks.Add(aPG.Block("Stiffener"));


                Image.GetOrAddReference("GEOMETRY", dxxReferenceTypes.LAYER, aColor: dxxColors.BlackWhite);
                Image.GetOrAddReference("STIFFENERS", dxxReferenceTypes.LAYER, aColor: dxxColors.BlackWhite);
                dxfDisplaySettings dsp = new("GEOMETRY");
                List<mdBaffle> assybafs = assyParts.OfType<mdBaffle>().ToList();
     
                foreach (var box in boxes)
                {
              
                    List<mdBaffle> dcbbaffles = assybafs.FindAll( x => x.IsAssociatedToParent(box.PartNumber ) && !x.ForDistributor && !x.IsBlank );
                    for (int j = 1; j <= dcbbaffles.Count; j++)
                    {
                        mdBaffle aBaf = dcbbaffles[ j - 1 ];

                        if (aBaf.IsVirtual) continue;
                        aPG = aBaf.View_Profile( aCenter: dxfVector.Zero, aLayerName: "GEOMETRY", bAddSlotPoints: true, aCenterElevationToMounts: false, bOmmitNotches: noNotches );
                        aPG.DisplaySettings = dsp;
                        uopVectors istpts = aBaf.Instances.MemberPoints(aBaf.Center, true);
                        bname = !noNotches ? $"Deflector_{aBaf.PartNumber}" : $"Deflector_Blank({aBaf.PartNumber})";
                        aRec = aPG.Vertices.BoundingRectangle();
                        aPG.TFVSet( bname, aBaf.SplicedOnTop.ToString().ToUpper(), box.Index );
                        aPG.BlockName = bname;
                        if (!Image.Blocks.BlockExists( bname ))
                        {
                            dxfBlock defblok = aPG.Block( bname, false, Image, dxxLinetypeLayerFlag.ForceToLayer );
                            defblok =Image.Blocks.Add( defblok );
                            bname = defblok.Name;
                        }

                        foreach (var item in istpts)
                        {
                            v1 = new dxfVector( item.Y, -item.X + aBaf.Z );
                            v1.TFVSet( !noNotches ? $"{aBaf.PartNumber}" : $"Blank({aBaf.PartNumber})", Image.FormatNumber( aRec.Width ), aRec.Width );
                            f1 = item.Y > box.Y ? -1 : 1;

                            draw.aInsert( bname, v1, 0, f1, dsp, 1, 1 );

                            ips.Add( v1 );
                        }
                       
                    }


                }




                Image.Display.ZoomExtents(bSetFeatureScale: true);
                paperscale = Image.Display.PaperScale;
                Image.DimStyle().FeatureScaleFactor = 0.5 * Image.DimStyle().FeatureScaleFactor;
                Drawing.ZoomExtents = false;
                aRec = Image.Display.ExtentRectangle;
                YMin = aRec.Bottom;
                ymax = aRec.Top;
                for (int i = 1; i <= ips.Count; i++)
                {
                    v1 = ips.Item(i);
                    v1.X += v1.Value / 2;

                    draw.aText(v1.Moved(0, -0.125 * paperscale), $"{v1.Tag}\\P{v1.Flag}", 0.125 * paperscale, dxxMTextAlignments.TopCenter, aLayer: "Standard");

                }

                aPts = Assy.StiffenerCenters().ToDXFVectors();
                //Debug.WriteLine(aPts.CoordinatesR(2, "\n"));

                aPts.RotateAbout(dxfVector.Zero, -90);
                aPts.Move(aChangeY: aStf.Z);

                draw.aInserts("Stiffener", aPts);

                draw.aLine(dxfVector.Zero, new dxfVector(0, YMin - 0.25 * paperscale), aLineType: dxfLinetypes.Center);

                DowncomerDataSet dcdata = Assy.DowncomerData;
                foreach (var box in dcdata)
                {
                        draw.aLine(box.X_Outside_Left, YMin - 0.25 * paperscale, box.X_Outside_Left, ymax + 0.25 * paperscale, "DC_EDGES", dxxColors.Green, dxfLinetypes.Continuous );
                        draw.aLine( box.X_Outside_Right, YMin - 0.25 * paperscale, box.X_Outside_Right, ymax + 0.25 * paperscale, "DC_EDGES", dxxColors.Green, dxfLinetypes.Continuous );

                        if (Math.Round( box.X, 1 ) != 0)
                        {
                            draw.aLine( -box.X_Outside_Left, YMin - 0.25 * paperscale, -box.X_Outside_Left, ymax + 0.25 * paperscale, "DC_EDGES", dxxColors.Green, dxfLinetypes.Continuous );
                            draw.aLine( -box.X_Outside_Right, YMin - 0.25 * paperscale, -box.X_Outside_Right, ymax + 0.25 * paperscale, "DC_EDGES", dxxColors.Green, dxfLinetypes.Continuous );
                        }
                 
                }

                aRec = Image.Display.ExtentRectangle;
                v1 = new dxfVector(0, aRec.Bottom - 0.25 * paperscale);

                draw.aText(v1, $"BAFFLE LAYOUT {MDRange.SpanLabel.ToUpper()}", 0.1875 * paperscale, dxxMTextAlignments.TopCenter, aLayer: "Standard");

            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally
            {
                Status = string.Empty;
                //if (aPGs != null) aPGs.Dispose();
            }
        }

        private void xCancelCheck()
        {
            // DoEvents
            if (CancelOps)
            {
                throw new Exception("Drawing Operations Canceled Prematurely");
                //MessageBox.Show("Drawing Operations Canceled Prematurely", $"Error Code {CancelErr}"); // I don't know if it has the same effect as above line from VB !!!
            }
        }

        private double ZoomExtents(bool bSuppressEndZoom = false, double aMargin = 1.05, bool bSetFeatureScales = true)
        {
            double result = 1;

            if (Image != null)
            {
                Image.Display.ZoomExtents(aMargin, bSetFeatureScales);
                if (bSuppressEndZoom && Drawing != null)
                {
                    Drawing.ZoomExtents = false;
                }
                result = Image.Display.PaperScale;
            }

            return result;
        }

        private dxfRectangle Draw_InstallationBubbles1(dxfRectangle aBoundary, dxeLine VerCenterLn, dxeLine HorCenterLn, uopHoleArray dsHoles,  colDXFVectors clips = null, colDXFEntities endangles = null)
        {

            uopProperties DTNames = Assy.AssemblyDetails();
            colMDDowncomers DComers = Assy.Downcomers;
            uopPart aPrt;
            uopPart commonpart;

            colDXFEntities dEnts = new();
            colDXFEntities bEnts;
            colDXFVectors aPts = null;
            dxfRectangle aRect;
            uopHoles aHls;
            dxfVector v1 = dxfVector.Zero;
            dxfVector v2 = dxfVector.Zero;
            dxfVector v3 = dxfVector.Zero;
            uopHole aHl;
                mdDeckSection aDS;

            double paperscale = Image.Display.PaperScale;

            double bubLn = 0.75 * paperscale * 0.5;
            double bubHt = 0.375 * paperscale * 0.5;
            double hexLn = 0.433 * paperscale * 0.5;

            double y2;
            double X1;
            string ptxt = "";
            string htxt = "";
            string ttxt = "";
            string lname = Image.SymbolSettings.LayerName;
            bool bOddDC = Assy.OddDowncomers;
            colDXFVectors tpPts = new();
            colDXFVectors lrPts = new();
            dxfRectangle lRec;
            double rad;
            dxeLeader Ldr;
            dxfVector org = null;
            double d1;
            double d2;
            double d3;
            double f1 = 1;
            double pwd = Assy.FunctionalPanelWidth;
            int places = 0;
            bool flg = false;
            dxxPointFilters fltr = dxxPointFilters.AtMaxX;
            uppSpliceStyles sstyle = Assy.DesignOptions.SpliceStyle;
            dxoLeaderTool leaders = Image.LeaderTool;
            dxoDrawingTool draw = Image.Draw;
            clips ??= new colDXFVectors();
            endangles ??= new colDXFEntities();
            List<uopDeckSplice> aSplcs;
            mdPartMatrix projParts = MDProject.GetParts();
            dxxOrthoDirections orthod = dxxOrthoDirections.Right;
            double y1 = 0;
            try
            {


                colUOPParts splcAngles = Assy.GenerateSpliceAngles(bTrayWide: true);
                if (dsHoles == null) dsHoles = Assy.DeckSections.GenHoles(Assy, bTrayWide: true);

                lname = Image.SymbolSettings.LayerName;
                if (VerCenterLn == null) VerCenterLn = draw.aLine(0, -1.01 * ShellRad, 0, 1.01 * ShellRad, aLineType: dxfLinetypes.Center);
                if (HorCenterLn == null) HorCenterLn = draw.aLine(-1.01 * ShellRad, 0, 1.01 * ShellRad, 0, aLineType: dxfLinetypes.Center);

                dEnts.Add(VerCenterLn);
                dEnts.Add(HorCenterLn);
                if (aBoundary == null) aBoundary = dEnts.BoundingRectangle();

                Status = "Drawing View 1 Annotations";

                //===================== END ANGLES BUBBLES =====================
                try
                {
                    Status = "Drawing End Angle Bbbbles";
                    if (endangles.Count > 0)
                    {
                        y1 = VerCenterLn.EndPoints().GetOrdinate(dxxOrdinateTypes.MaxY) + 0.35 * paperscale - Image.Y;


                        List<mdEndAngle> assyparts = projParts.EndAngles().FindAll(x => x.IsAssociatedToRange(MDRange.GUID) == true && x.Side == uppSides.Right);

                        dxfVector endAngleBubblePoint;
                        string endAnglePartNumber;
                        double endAngleHalfWidth = 0.5 * (DComers?.FirstItem()?.Boxes?.FirstOrDefault()?.EndAngles()?.FirstOrDefault()?.Width ?? 1.0);
                        foreach (var downcomer in DComers)
                        {
                            // Top points
                            (endAngleBubblePoint, endAnglePartNumber) = GetTopEndAngleBubblePointAndPartNumber(downcomer, DComers);
                            if (endAngleBubblePoint != null)
                            {
                                endAngleBubblePoint.X += downcomer.X >= 0 ? endAngleHalfWidth : -endAngleHalfWidth;

                                ptxt = endAnglePartNumber;
                                ttxt = (downcomer.Index == 1) ? "END ANGLE" : "";
                                htxt = (downcomer.Index == 1) ? DTNames.StringValue(ttxt) : "";

                                endAngleBubblePoint.Tag = ptxt; endAngleBubblePoint.Flag = htxt; endAngleBubblePoint.LayerName = ttxt;

                                tpPts.Add(endAngleBubblePoint, bAddClone: true);
                            }

                            // Bottom points
                            (endAngleBubblePoint, endAnglePartNumber) = GetBottomEndAngleBubblePointAndPartNumber(downcomer, DComers);
                            if (endAngleBubblePoint != null)
                            {
                                endAngleBubblePoint.X += downcomer.X >= 0 ? -endAngleHalfWidth : endAngleHalfWidth;

                                ptxt = endAnglePartNumber;
                                ttxt = (downcomer.Index == DComers.Count) ? "END ANGLE" : "";
                                htxt = (downcomer.Index == DComers.Count) ? DTNames.StringValue(ttxt) : "";

                                endAngleBubblePoint.Tag = ptxt; endAngleBubblePoint.Flag = htxt; endAngleBubblePoint.LayerName = ttxt;

                                tpPts.Add(endAngleBubblePoint, bAddClone: true);
                            }
                        }
                    }

                 
                }
                catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }



                //=============== JOGGLE SPLICE BUBBLES ==============================

                try
                {
                    Status = "Drawing Joggle Bubbles";
                    if (sstyle != uppSpliceStyles.Tabs)
                    {
                        if (DTNames.HasMember("DECK JOGGLE"))
                        {
                            aSplcs = Assy.DeckSplices.FindAll (x => x.SpliceType == uppSpliceTypes.SpliceWithJoggle);
                            if (aSplcs.Count > 0)
                            {
                                v1 = new colDXFVectors(aSplcs.Select(s => s.Center).ToList()).GetVector(dxxPointFilters.GetTopRight);
                                v1.X = -v1.X;
                                lrPts.Add(v1, bAddClone: true, aTag: "", aFlag: DTNames.StringValue("DECK JOGGLE"), aLayerName: "DECK JOGGLE");
                            }
                        }
                    }
                }
                catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }


                //=============== CROSSBRACE BUBBLES ==============================
                if (Assy.DesignOptions.CrossBraces)
                {
                    try
                    {
                        Status = "Drawing Cross Brace Bubbles";

                        v1.SetCoordinates(-Assy.CrossBrace.X - 0.5 * Assy.CrossBrace.Length + 0.5 * Assy.Downcomer().BoxWidth + 0.25 * pwd, -Assy.CrossBrace.Y - 0.25);

                        lrPts.Add(v1, bAddClone: true, aTag: Assy.CrossBrace.PartNumber, aFlag: DTNames.StringValue("CROSS BRACE"), aLayerName: "CROSS BRACE");
                    }
                    catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
                }

                //=============== RING CLIP BUBBLES ==============================

                try
                {
                    Status = "Drawing Ring Clip Bubbles";
                    v2 = dxfVector.Zero;
                    v1 = DComers.Item(1).Boxes[0].EndSupport(bTop: true).GenHoles("RING CLIP").Item(1).Last().CenterDXF;
                    v1.Y = -v1.Y;
                    v1.Z = -0.25 * paperscale;

                    aPrt = Assy.RingClip(true);
                    aPrt.UpdatePartProperties();
                    places = Assy.RingClipCenters(bBothSides: true, bExcludeDowncomers: false, bExcludeDeckSections: true).Count;

                    v1.Tag = aPrt.PartNumber; v1.Flag = DTNames.StringValue("DC RING CLIP"); v1.LayerName = $"RING CLIP\\P({places} PLACES)";

                    lrPts.Add(v1, bAddClone: true);

                    aHls = Assy.DeckSections.Item(1).GenHoles(Assy, "RING CLIP").Item(1);
                    if (aHls.Count > 0)
                    {
                        aHl = (aHls.Count > 2) ? aHls.Item(2) : aHls.Item(1);
                        v1 = aHl.CenterDXF;
                        aPrt = Assy.RingClip(false);
                        places = dsHoles.Item("RING CLIP").Count; //  Assy.RingClipCenters(bBothSides: true, bExcludeDowncomers: true, bExcludeDeckSections: false).Count;

                        ///uopFreeBubbleAreas fbas = uopFreeBubbleAreas.AssemblyAreas(Assy, true);
                        v1.Tag = aPrt.PartNumber; v1.Flag = DTNames.StringValue("RING CLIP"); v1.LayerName = $"RING CLIP";
                        lrPts.Add(v1, bAddClone: true, aTag:aPrt.PartNumber,aFlag: DTNames.StringValue("RING CLIP"), aLayerName: $"RING CLIP\\P({places} PLACES)");
                    }

                }
                catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }



                //=============== FINGER CLIP BUBBLES ==============================
                try
                {
                    Status = "Drawing Finger Clip Bubbles";
                    if (clips.Count > 0)
                    {

                        mdFingerClip clip = Assy.FingerClip;

                        v1 = clips.GetVector(dxxPointFilters.GetRightTop) - Image.UCS.Origin + new dxfVector(clip.Width, 0);
                        v1.Tag = clip.PartNumber;
                        v1.Flag = DTNames.StringValue("Finger Clip");
                        v1.LayerName = $"FINGER CLIP\\P({clips.Count} PLACES)";
                        lrPts.Add(v1, bAddClone: true, aTag: clip.PartNumber, aFlag: DTNames.StringValue("Finger Clip"), aLayerName: $"FINGER CLIP\\P({clips.Count} PLACES)");

                        aPts = mdUtils.FingerClipCenters(Assy, bSuppressedValue: true);
                        if (aPts.Count > 0)
                        {
                            ttxt = aPts.Count > 1 ? $"{aPts.Count} PLACES PER TRAY" : $"{aPts.Count} PLACE PER TRAY";
                            ttxt = $"DO NOT PLACE FINGER\\PCLIPS ON MANWAYS\\P{ttxt}";

                            lrPts.Add(aPts.GetVector(dxxPointFilters.GetRightTop), bAddClone: true, aTag: "", aFlag: "", aLayerName: ttxt);
                        }
                    }
                }
                catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }


                //=============== SPLICE ANGLE BUBBLES ==============================
                try
                {
                    Status = "Drawing Splice Angle Bubbles";
                    if (sstyle != uppSpliceStyles.Tabs)
                    {
                        aPrt = splcAngles.GetPartByType(uppPartTypes.SpliceAngle);

                        if (projParts.TryGetEqualPart(aPrt, out commonpart)) aPrt = commonpart;
                        if (aPrt != null)
                        {
                            places = splcAngles.FindAll(x => x.PartType == uppPartTypes.SpliceAngle && x.Length == aPrt.Length).Count;

                            v1 = splcAngles.CentersDXF().GetVector(dxxPointFilters.GetLeftTop).Moved(-0.25 * pwd, -0.375);
                            ptxt = aPrt.PartNumber;
                            htxt = DTNames.StringValue("Deck Splice");
                            ttxt = places > 1 ? $"SPLICE ANGLE\\P({places} PLACES)" : $"SPLICE ANGLE";

                            lrPts.Add(v1, bAddClone: false, aTag: ptxt, aFlag: htxt, aLayerName: ttxt);
                        }
                    }
                    else
                    {
                        uopVectors ctrs = Assy.DeckSplices.Centers(bGetClones: false, aSpliceType: uppSpliceTypes.SpliceWithTabs);
                        if (ctrs.Count > 0)
                        {
                            v1 = new dxfVector(ctrs.GetVector(dxxPointFilters.GetTopRight));
                            ptxt = ""; htxt = DTNames.StringValue("Deck Splice"); ttxt = "DECK SPLICE";
                            v1.X = -v1.X - 0.25 * pwd;
                            lrPts.Add(v1, bAddClone: false, aTag: ptxt, aFlag: htxt, aLayerName: ttxt);
                        }
                    }

                }
                catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }


                //=============== BUBBLE PROMOTER NOTE ==============================
                try
                {
                    Status = "Drawing Bubble Promoter Bubbles";
                    if (Assy.DesignOptions.HasBubblePromoters)
                    {
                        uopVectors bppts = Assy.BPCenters(bTrayWide: true, aSuppressedValue: false, bRegen: false);
                        if (bppts.Count > 0)
                        {
                            htxt = ""; ptxt = ""; ttxt = $"ALL EMBOSSED BUBBLE\\PPROMOTERS MUST PROTRUDE\\PABOVE DECK SURFACE";
                            lrPts.Add(bppts.GetVector(dxxPointFilters.GetTopLeft), bAddClone: true, aTag: ptxt, aFlag: htxt, aLayerName: ttxt);
                        }
                    }

                }
                catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }

                //=============== MANWAY DETAIL BUBBLES ==============================

                try
                {
                    Status = "Drawing Manway Bubble";
                    mdDeckSections aDSs = new(Assy.DeckSections.FindAll(x => x.InstalledOnAlternateRing1 && x.IsManway),bDontCloneMembers:true);
                    if (aDSs.Count > 0)
                    {
                        aPts = aDSs.Centers();

                        uopHoles holes = aDSs.GenHoles(Assy, "MANWAY", bTrayWide: true).Item(1);

                        v1 = aPts.GetVector(dxxPointFilters.GetRightTop, bReturnClone: true);
                        aDS = Assy.DeckSections.GetByHandle(v1.Tag);
                        htxt = sstyle == uppSpliceStyles.Tabs ? "manway splice" : "manway angle";
                        htxt = $"{DTNames.StringValue("manway")},{DTNames.StringValue("manway to downcomer")},{DTNames.StringValue(htxt)}";
                        lrPts.Add(new dxfVector(aDS.X, aDS.Y), bAddClone: false, aTag: "", aFlag: htxt, aLayerName: "MANWAY");


                        aPts = holes.CentersDXF();
                        if (aPts.Count > 0)
                        {
                            flg = Assy.DesignOptions.UseManwayClips;
                            fltr = sstyle == uppSpliceStyles.Tabs ? dxxPointFilters.GetTopRight : dxxPointFilters.GetBottomRight;
                            ttxt = flg ? "MANWAY WASHER\\PAND CLAMP" : "MANWAY CLAMP\\PAND STUD";
                            ttxt = $"{ttxt}\\P({holes.Count} PLACES)";
                            lrPts.Add(aPts.GetVector(fltr), bAddClone: false, aTag: flg ? "63,64" : "62,65", aFlag: "", aLayerName: ttxt);

                        }

                        if (sstyle != uppSpliceStyles.Tabs)
                        {


                            aPrt = splcAngles.GetPartByType(uppPartTypes.ManwayAngle);
                            if (projParts.TryGetEqualPart(aPrt, out commonpart)) aPrt = commonpart;

                            if (aPrt != null)
                            {
                                places = splcAngles.FindAll(x => x.PartType == uppPartTypes.ManwayAngle).Count;
                                lrPts.Add(new dxfVector(aDS.X - 0.25 * pwd, aDS.Y + 0.5 * aDS.Height), bAddClone: false, aTag: aPrt.PartNumber, aFlag: DTNames.StringValue("Manway Angle"), aLayerName: places > 1 ? $"MANWAY ANGLE\\P({places} PLACES)" : $"MANWAY ANGLE");
                            }
                        }
                        else
                        {


                            aPrt = splcAngles.GetPartByType(uppPartTypes.ManwaySplicePlate);
                            if (projParts.TryGetEqualPart(aPrt, out commonpart)) aPrt = commonpart;


                            if (aPrt != null)
                            {
                                places = splcAngles.FindAll(x => x.PartType == uppPartTypes.ManwaySplicePlate).Count;
                                lrPts.Add(new dxfVector(aDS.X - 0.25 * pwd, aPrt.Y, 0), bAddClone: false, aTag: aPrt.PartNumber, aFlag: DTNames.StringValue("Manway Splice"), aLayerName: places > 1 ? $"MANWAY SPLICE\\P({places} PLACES)" : $"MANWAY SPLICE");

                            }
                        }
                    }

                }
                catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }

                //=============== END ANGLE DETAIL ==============================
                if (Assy.DesignFamily.IsBeamDesignFamily())
                {
                    try
                    {
                        Status = "DRAWING END ANGLE DETAIL BUBBLES";

                        int totalNumberOfEndAngles = projParts.EndAngles().FindAll(x => x.IsAssociatedToRange(MDRange.GUID) == true).Count;
                        //var referenceDC = Assy.Downcomers.Item(Assy.Downcomers.Count - 2); // This is the downcomer whose end angles are used
                        //var allEndAngles = referenceDC.Boxes.First(b => !b.IsVirtual).EndAngles(); // We expect that end angles are for top right, top left, bottom right and bottom left, in order
                        
                        mdDowncomer referenceDC = null;
                        for (int i = Assy.Downcomers.Count; i >=1; i--)
                        {
                            referenceDC = Assy.Downcomers.Item(i);
                            if (referenceDC.Boxes.Count > 0)
                            {
                                break;
                            }
                        }

                        if (referenceDC != null)
                        {
                            var allEndAngles = FindBottomMostBox(referenceDC).EndAngles(); // We expect that end angles are for top right, top left, bottom right and bottom left, in order
                            var topRightEndAngle = allEndAngles.First(ea => ea.Side == uppSides.Right);
                            var topLeftEndAngle = allEndAngles.First(ea => ea.Side == uppSides.Left);

                            dxfVector topCornerAdjacentToEndSupport;
                            dxfVector leaderPoint = null;
                            double cornerX, direction;
                            foreach (var endAngle in new mdEndAngle[] { topLeftEndAngle, topRightEndAngle })
                            {
                                direction = endAngle.Side == uppSides.Left ? -1 : 1;
                                cornerX = endAngle.CenterDXF.X + direction * endAngle.Thickness;
                                topCornerAdjacentToEndSupport = new dxfVector(cornerX, endAngle.CenterDXF.Y + endAngle.Length / 2);

                                if (endAngle.Chamfered)
                                {
                                    double lineLength = Math.Sqrt(2 * Math.Pow(endAngle.Width - endAngle.Thickness, 2)) / 2;
                                    leaderPoint = topCornerAdjacentToEndSupport.Clone();
                                    leaderPoint.MovePolar(endAngle.Side == uppSides.Left ? 225 : -45, lineLength);
                                }
                                else
                                {
                                    leaderPoint = new dxfVector(topCornerAdjacentToEndSupport.X + direction * ((topLeftEndAngle.Width - topLeftEndAngle.Thickness) / 2), topCornerAdjacentToEndSupport.Y);
                                }
                            }
                        }
                        //leaderPoint.Tag = endAngle.PartNumber;
                        //leaderPoint.LayerName = $"END ANGLE\\P({totalNumberOfEndAngles / 2} PLACES)";
                        //lrPts.Add(leaderPoint, bAddClone: true, aTag: leaderPoint.Tag, aLayerName: leaderPoint.LayerName);
                        

                    }
                    catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }

                }

                //=============== BEAM DETAIL ==============================

                if (Assy.DesignFamily.IsBeamDesignFamily())
                {
                    try
                    {
                        double unitCoefficient = Image.DimStyle().LinearScaleFactor;
                        string unitSymbol = Drawing.DrawingUnits == uppUnitFamilies.Metric ? "mm" : "\"";
                        var formatedString = (double value) =>
                        {
                            string formatString = (Drawing.DisplayUnits == uppUnitFamilies.English) ? "0.000" : "0";
                            double displayValue = value * unitCoefficient;
                            return displayValue.ToString(formatString);
                        };

                        var beam = Assy.Beam;
                        var beamHole = beam.GenHoles("Beam", "BOTTOM RIGHT");
                        htxt = $"{DTNames.StringValue("BEAM")}";
                        ptxt = string.IsNullOrWhiteSpace(beam.PartNumber) ? "???" : beam.PartNumber;
                        ttxt = $"I-BEAM\\P{formatedString(beam.Height)}{unitSymbol} HIGH x\\P{formatedString(beam.Width)}{unitSymbol} WIDE";
                        dxfVector beamHoleLeaderPoint = beamHole.Item(1).Member.CenterDXF.Clone();
                        beamHoleLeaderPoint.Tag = ptxt;
                        beamHoleLeaderPoint.Flag = htxt;
                        beamHoleLeaderPoint.LayerName = ttxt;
                        lrPts.Add(beamHoleLeaderPoint, bAddClone: true, aTag: ptxt, aFlag: htxt, aLayerName: ttxt);

                    }
                    catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
                }

                //===================== VIEW ARROW =====================
                
               try 
                {
                    Status = "Drawing view Arrows";
                    v1 = !bOddDC ? dxfVector.Zero : new dxfVector(-Assy.Downcomer().Width / 2 - 0.25 * Assy.DowncomerSpacing, 0);
                    dEnts.Add(draw.aSymbol.ViewArrow(v1, aBoundary.Height + (4 * bubHt + 0.5 * paperscale), 0.5, "{\\Fromand;A1}", 0, 0.1875, 0.2, dxxArrowStyles.Undefined, null, false, "SECTION_ARROW"));

                }
                catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }

                //===================== VIEW LABEL =====================
                
                try
                {
                    Status = "Drawing view label";
                    aRect = dEnts.BoundingRectangle();
                    v1 = aRect.BottomCenter + new dxfVector(-Image.X, -Image.Y - 0.25 * paperscale);
                    ttxt = $"\\Fromand;\\LPLAN VIEW OF TRAYS {Assy.SpanName()}"; // In VB code it is Assy.Part.SpanName
                    ttxt = $"{ttxt}\\l\\P\\Fromans|c0;\\H0.66666x;FOR TRAY SPACING AND DOWNCOMER ORIENTATION";
                    ttxt = $"{ttxt}\\PSEE DRAWING D-???????";
                    dEnts.Add(draw.aText(v1, ttxt, 0.1875 * paperscale, dxxMTextAlignments.TopCenter, aTextStyle: "Standard"));
                }
                catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }

                //bubbles along the top and bottom
               
               try 
                {
                    Status = "drawing Bubble - TOP & BOTTOM";
                    y1 = aBoundary.Height / 2 + (bubHt + 0.125 * paperscale);
                    y2 = -aBoundary.Height / 2 - (bubHt + 0.125 * paperscale);

                    for (int i = 1; i <= tpPts.Count; i++)
                    {
                        v1 = tpPts.Item(i);
                        v2 = new dxfVector(v1.X, v1.Y > 0 ? y1 : y2);

                        ptxt = v1.Tag; htxt = v1.Flag; ttxt = v1.LayerName;

                        if (ptxt != "" || htxt != "" || ttxt != "")
                        {
                            orthod = (v1.X >= 0) ? dxxOrthoDirections.Right : dxxOrthoDirections.Left;
                            f1 = (v1.X >= 0) ? -1 : 1;
                            Tool.CreatePNBubbles(Image, v2, paperscale, orthod, ptxt, htxt, ttxt, aXOffset: f1 * bubLn, aCollector: dEnts);
                        }
                        f1 = (v1.Y > 0) ? -1 : 1;
                        v2.Move(aYChange: f1 * bubHt);
                        leaders.NoRef(v1, v2, bSuppressHook: true);
                        //xCancelCheck();
                    }
                }
                catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }


                // right side
               
                try{
                    Status = "drawing Bubble - RIGHT";
                    rad = ShellRad + 0.375 * paperscale;
                    tpPts = new colDXFVectors(lrPts.GetVectors(aFilter: dxxPointFilters.GreaterThanX, aOrdinate: 0, bOnIsIn: true, bRemove: true));
                    aPts = new colDXFVectors();
                    d1 = 0.375 * paperscale;
                    d2 = 0.125 * paperscale;
                    X1 = 0.5 * aBoundary.Width + 0.125 * paperscale;

                    for (int i = 1; i <= tpPts.Count; i++)
                    {
                        v1 = tpPts.Item(i);
                        d3 = (Math.Abs(v1.Y) > 0.25 * ShellRad) ? d2 : d1;

                        f1 = (v1.Y > 0) ? 1 : -1;
                        aPts.Add(X1, v1.Y + f1 * d3);

                    }
                    //xCancelCheck();

                    aPts.Clockwise(dxfVector.Zero, 90, false, aFollowers: tpPts);

                    X1 = aBoundary.Width / 2;
                    lRec = null;
                    v3 = dxfVector.Zero;
                    org = Image.UCS.Origin;

                    for (int i = 1; i <= tpPts.Count; i++)
                    {
                        v1 = tpPts.Item(i);
                        v2 = aPts.Item(i);

                        ptxt = v1.Tag; htxt = v1.Flag; ttxt = v1.LayerName;
                        if (ptxt != "" || htxt != "" || ttxt != "")
                        {
                            if (ttxt != "" && ptxt == "" && htxt == "")
                            {
                                Ldr = leaders.Text(v1, v2, ttxt, null, bCreateOnly: true);
                                v2 = Ldr.Vertices.LastVector();
                                if (lRec != null)
                                {
                                    if (v2.Y > (lRec.Bottom - (bubHt + 0.375 * paperscale)))
                                    {
                                        d1 = v2.Y - (lRec.Bottom - (bubHt + 0.375 * paperscale));
                                        v2.Move(aYChange: -d1);
                                    }
                                }
                                v2 -= org;

                                Image.Entities.Add(Ldr);

                                lRec = Ldr.MText.BoundingRectangle();
                                v2 = dxfVector.Zero;
                            }
                            else
                            {
                                Ldr = leaders.NoRef(v1, v2);
                                v2 = Ldr.Vertices.LastVector();
                                if (lRec != null)
                                {
                                    if (v2.Y > (lRec.Bottom - (bubHt + 0.375 * paperscale)))
                                    {
                                        d1 = v2.Y - (lRec.Bottom - (bubHt + 0.375 * paperscale));
                                        v2.Move(aYChange: -d1);
                                    }
                                }
                                v2 -= org;

                                Image.Entities.Add(Ldr);


                                bEnts = Tool.CreatePNBubbles(Image, v2, paperscale, dxxOrthoDirections.Right, ptxt, htxt, ttxt, false, aCollector: dEnts);
                                lRec = bEnts.BoundingRectangle();
                                v2 = dxfVector.Zero;
                            }
                        }
                        //xCancelCheck();
                    }
                }
                catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }


                // left side
              
               try 
                {
                    Status = "drawing Bubble - LEFT";
                    aPts = new colDXFVectors();
                    d1 = 0.375 * paperscale;
                    d2 = 0.125 * paperscale;

                    X1 = -0.5 * aBoundary.Width - 0.125 * paperscale;
                    for (int i = 1; i <= lrPts.Count; i++)
                    {
                        v1 = lrPts.Item(i);

                        d3 = (Math.Abs(v1.Y) > 0.25 * ShellRad) ? d2 : d1;
                        v2 = (Math.Round(v1.Y, 1) > 0) ? aPts.Add(X1, v1.Y + d3) : aPts.Add(X1, v1.Y - d3);

                        //xCancelCheck();
                    }

                    aPts.Clockwise(dxfVector.Zero, 90, true, aFollowers: lrPts);

                    lRec = null;
                    v3 = dxfVector.Zero;
                    for (int i = 1; i <= lrPts.Count; i++)
                    {
                        v1 = lrPts.Item(i);
                        v2 = aPts.Item(i);
                        ptxt = v1.Tag; htxt = v1.Flag; ttxt = v1.LayerName;


                        if (ptxt != "" || htxt != "" || ttxt != "")
                        {
                            if (ttxt != "" && ptxt == "" && htxt == "")
                            {
                                Ldr = leaders.Text(v1, v2, ttxt, bCreateOnly: true);
                                v2 = Ldr.Vertices.LastVector();
                                if (lRec != null)
                                {
                                    if (v2.Y > (lRec.Bottom - (bubHt + 0.375 * paperscale)))
                                    {
                                        d1 = v2.Y - (lRec.Bottom - (bubHt + 0.375 * paperscale));
                                        v2.Move(aYChange: -d1);
                                    }
                                }
                                v2 -= org;

                                Image.Entities.Add(Ldr);

                                lRec = Ldr.MText.BoundingRectangle();
                            }
                            else
                            {
                                Ldr = leaders.NoRef(v1, v2);
                                v2 = Ldr.Vertices.LastVector();
                                if (lRec != null)
                                {
                                    if (v2.Y > (lRec.Bottom - (bubHt + 0.375 * paperscale)))
                                    {
                                        d1 = v2.Y - (lRec.Bottom - (bubHt + 0.375 * paperscale));
                                        v2.Move(aYChange: -d1);
                                    }
                                }
                                v2 -= org;

                                Image.Entities.Add(Ldr);


                                bEnts = Tool.CreatePNBubbles(Image, v2, paperscale, dxxOrthoDirections.Left, ptxt, htxt, ttxt, !string.IsNullOrWhiteSpace(htxt) && !string.IsNullOrWhiteSpace(ptxt), aCollector: dEnts);
                                lRec = bEnts.BoundingRectangle();
                            }
                        }
                        //xCancelCheck();
                    }

                }
                catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }




                //xCancelCheck();
                return dEnts.BoundingRectangle();
            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = string.Empty; }
            return new dxfRectangle();
        }

        private dxfRectangle Draw_InstallationBubbles2(int aViewIndex, dxeLine VerCenterLn, dxeLine HorCenterLn, List<dxeInsert> aDeckInserts = null)
        {
            dxfVector v1 = dxfVector.Zero;
            double paperscale = Image.Display.PaperScale;
            double bubLn = 0.75 * paperscale * 0.5;
            double bubHt = 0.375 * paperscale / 2;
            dxeLine mirraxis;
            dxfRectangle _rVal = new();
            colDXFEntities dEnts = new();
            dxfVector offsetv = Image.UCS.Origin * -1;

            try
            {
                string lname = Image.SymbolSettings.LayerName;

                mdPartMatrix projParts = MDProject.GetParts();
                dxfRectangle rectangle = null;
                dxoDrawingTool draw = Image.Draw;
                dxeText aTxt;
                string txt;
                Status = $"Drawing View {aViewIndex} Annotations";
                aDeckInserts ??= new List<dxeInsert>();
                dxfPlane UCS = Image.UCS;
                dxfVector org = UCS.Origin.Clone();
                dxxOrthoDirections bubdir;
                dEnts.Add(VerCenterLn, bAddClone: true);
                dEnts.Add(HorCenterLn, bAddClone: true);
                bool SandT = Assy.SpliceStyle == uppSpliceStyles.Tabs;
                double y1 = VerCenterLn.EndPoints().GetOrdinate(dxxOrdinateTypes.MaxY) + 0.35 * paperscale - Image.Y;

                //=====================DECK BUBBLES =====================
                Status = "Drawing Deck Section Part Number Bubbles";
                if (aDeckInserts.Count > 0)
                {
                    List<mdDeckSection> DSecs = Assy.UniqueDeckSections(bAltRing: aViewIndex == 3).ToList();
                    dxfDisplaySettings dsp = new(Image.Layers.GetOrAdd("DECK_PN_BUBBLES", Image.SymbolSettings.LayerColor, bUpdateIfFound: true).Name);
                    Image.Layers.Add("DECK_PN_BUBBLES", Image.SymbolSettings.LayerColor);


                    foreach (dxeInsert insert in aDeckInserts)
                    {
                        bubdir = (aViewIndex < 3) ? dxxOrthoDirections.Right : SandT ? dxxOrthoDirections.Up : dxxOrthoDirections.Right;
                        string ptxt = insert.Tag;
                        mdDeckSection ds = DSecs.Find((x) => string.Compare(x.PartNumber, ptxt, true) == 0);

                        if (ds.IsHalfMoon)
                        {
                            bubdir = (aViewIndex < 3) ? dxxOrthoDirections.Up : dxxOrthoDirections.Right;
                        }
                        else
                        {
                            if (aViewIndex == 3 && SandT)
                                bubdir = ds.Bounds.AspectRatio > 1 ? dxxOrthoDirections.Up : dxxOrthoDirections.Right;
                        }

                        colDXFEntities bubbles = Tool.CreatePNBubbles(Image, insert.InsertionPt - org, paperscale, bubdir, ptxt, aXOffset: 0, aCollector: dEnts, aPillLayer: "DECK_PN_BUBBLES", bCenterOfPill: true);

                        xCancelCheck();
                    }

                }

                //===================== DOWNCOMER BUBBLES =====================
                Status = "Drawing Downcomer Part Number Bubbles";
                if(aViewIndex == 2){
                    dxfVector bubblePoint, bubblesTargetPoint;
                    string[] downcomerBoxPartNumbers;
                    colDXFEntities bubbles;
                    string pillString, trailerString;

                    foreach (var downcomer in Assy.Downcomers)
                    {
                        bubbles = null;
                        pillString = string.Empty;
                        trailerString = string.Empty;

                        (bubblesTargetPoint, downcomerBoxPartNumbers) = GetTopEndSupportBubblePointAndDCBPartNumbers(downcomer, Assy.Downcomers);
                        bubblePoint = new dxfVector(bubblesTargetPoint.X, y1);

                        for (int i = downcomerBoxPartNumbers.Length - 1; i >= 0; i--)
                        {
                            pillString = downcomerBoxPartNumbers[i];

                            if (bubbles == null)
                            {
                                if (downcomer.Index == 1)
                                {
                                    trailerString = $"DOWNCOMERS FOR\\P{(Assy.Downcomers.Count % 2 == 0 ? "EVEN" : "ODD")} {Assy.TrayRange.Name(false)}";
                                }

                                bubbles = Tool.CreatePNBubbles(Image, bubblePoint, paperscale, dxxOrthoDirections.Right, pillString, aTrailer: trailerString, aXOffset: 0, aCollector: dEnts, bCenterOfPill: true);

                                bubbles.Add(draw.aLeader.NoRef(bubblesTargetPoint, bubblePoint.Clone().Moved(aYChange: -bubHt)));
                            }
                            else
                            {
                                bubblePoint = bubblePoint.Moved(aYChange: 2 * bubHt);
                                Tool.CreatePNBubbles(Image, bubblePoint, paperscale, dxxOrthoDirections.Right, pillString, aXOffset: 0, aCollector: dEnts, bCenterOfPill: true);
                            }
                        }
                    }

                }

                //===================== DEFLECTOR PLATES =====================
                rectangle = dEnts.BoundingRectangle();

                if (aViewIndex != 3 && Assy.DesignFamily.IsEcmdDesignFamily())
                {
                    List<mdDowncomer> DComers = Assy.Downcomers.GetByVirtual(aVirtualValue: false);
                    Status = "Drawing Deflector Plate Part Number Bubbles";
                    {
                        List<List<colDXFEntities>> columns_r = new();

                        foreach (mdDowncomer dc in DComers)
                        {
                            columns_r.Add(new List<colDXFEntities>());
                        }

                        List<mdBaffle> aParts = MDProject.GetParts().DeflectorPlates().FindAll((x) => x.IsAssociatedToRange(Assy.RangeGUID));
                        if (aParts.Count > 0)
                        {

                            List<mdBaffle> distrib = aParts.FindAll(x => x.ForDistributor == true);

                            foreach (mdBaffle item in distrib) { aParts.Remove(item); }
                            List<mdBaffle> blanks = aParts.FindAll(x => x.IsBlank == true);
                            foreach (mdBaffle item in blanks) { aParts.Remove(item); }


                            List<List<mdBaffle>> baffles = new();
                            if (distrib.Count > 0 || blanks.Count > 0)
                            {
                                if (distrib.Count > 0) baffles.Add(distrib);
                                if (aParts.Count > 0) baffles.Add(aParts);
                                if (blanks.Count > 0) baffles.Add(blanks);
                            }
                            else
                            {
                                baffles.Add(aParts);
                            }

                            aTxt = null;
                            int iset = 0;
                            colDXFEntities subsetents_last = null;
                            foreach (List<mdBaffle> subset in baffles)
                            {
                                colDXFEntities subsetents = new();
                                if (subset.Count <= 0) continue;

                                iset++;
                                mdBaffle aBaf = subset[0];
                                string heading = $"DEFLECTOR PLATES";
                                if (baffles.Count > 1)
                                {

                                    string suff = aBaf.RingRanges.SpanNameList(aRangeToInclude: MDRange.RingRange);

                                    if (suff.Contains('-') || suff.Contains(','))
                                        heading += $"\\PFOR TRAYS {suff}";
                                    else
                                        heading += $"\\PFOR TRAY {suff}";
                                }

                                if (iset == 1)
                                    v1 = new dxfVector(0, -rectangle.Height / 2 - 0.125 * paperscale - 0.1 * paperscale);
                                else
                                    v1 = new dxfVector(0, (subsetents_last.BoundingRectangle().BottomCenter + offsetv).Y - 1.5 * bubHt);

                                aTxt = draw.aText(v1, heading, 0.125 * paperscale, dxxMTextAlignments.TopCenter);
                                subsetents.Add(aTxt);
                                dEnts.Add(aTxt);

                                dxfVector v2 = aTxt.BoundingRectangle().MiddleRight().Moved(-Image.X + 1.75 * bubLn, -Image.Y + bubHt);
                                colDXFVectors points = new();
                                colDXFEntities bubbles = new();

                                // In standard design, the last non-virtual downcomer is almost at the middle where we start the labels with a little offset from the heading text.
                                // This is not the case for the beam design, so we need to start from the middle.
                                for (int i = Assy.DesignFamily.IsStandardDesignFamily() ? DComers.Count : (int)Math.Ceiling(DComers.Count / 2.0); i >= 1; i--)
                                {
                                    mdDowncomer aDC = DComers[i - 1];
                                    double x1 = Math.Round(aDC.X, 1);

                                    List<mdBaffle> dcbaffles;

                                    if (Assy.DesignFamily.IsStandardDesignFamily())
                                    {
                                        dcbaffles = subset.FindAll(x => aDC.Boxes.Any(b => x.IsAssociatedToParent(b.PartNumber)));
                                    }
                                    else
                                    {
                                        var allNonVirtualBoxes = FindAllNonVirtualDowncomerBoxesForDowncomer(aDC, Assy.Downcomers);
                                        dcbaffles = subset.FindAll(x => allNonVirtualBoxes.Any(b => x.IsAssociatedToParent(b.PartNumber)));
                                    }

                                    if (dcbaffles.Count <= 0)
                                    {
                                        columns_r[i - 1].Add(new colDXFEntities());
                                        continue;
                                    }

                                    colDXFVectors dcbafpts = colDXFVectors.Zero;
                                    foreach (mdBaffle baffle in dcbaffles)
                                    {
                                        dcbafpts.Append(baffle.Instances.MemberPointsDXF(baffle.CenterDXF).GetAtCoordinate(aDC.X), aTag: baffle.PartNumber);
                                    }
                                    dcbafpts.Sort(dxxSortOrders.TopToBottom);
                                    colDXFEntities dcbubbles = Tool.CreatePNBubbleStack(dcbafpts, v2, Image, paperscale, aXOffset: -bubLn, aYOffset: -bubHt, aCollector: dEnts);
                                    columns_r[i - 1].Add(dcbubbles.GetByGraphicType(dxxGraphicTypes.Polyline));
                                    subsetents.Append(dcbubbles, aTag: $"COL_{iset}");
                                    bubbles.Append(dcbubbles, aTag: $"COL_{iset}");
                                    v1 = dcbubbles.BoundingRectangle().TopCenter + offsetv;
                                    if (iset == 1)
                                    {
                                        points = new colDXFVectors(aDC.LimitLines.Last().Line2.MidPt.ToDXFVector(), v1.Moved(aYChange: 0.2 * paperscale), new dxfVector(v1));
                                        draw.aLeader.NoRef(points, null);


                                    }
                                    if (aDC.OccuranceFactor == 2)
                                    {
                                        mirraxis = aTxt.Plane.YAxis();
                                        mirraxis.Translate(offsetv);
                                        dcbafpts.Mirror(mirraxis);
                                        dcbubbles = Tool.CreatePNBubbleStack(dcbafpts, v2.Mirrored(mirraxis), Image, paperscale, aXOffset: -bubLn, aYOffset: -bubHt, aCollector: dEnts);

                                        if (iset == 1)
                                        {
                                            points.Mirror(mirraxis);
                                            draw.aLeader.NoRef(points, null);
                                        }
                                    }

                                    v2.Move(2 * bubLn);
                                    xCancelCheck();
                                }
                                aTxt.Move(aYChange: -0.5 * bubbles.BoundingRectangle().Height + 0.5 * aTxt.BoundingRectangle().Height);
                                subsetents_last = subsetents;
                            }
                        }

                        dxfDisplaySettings dsp = dxfDisplaySettings.Null(lname);
                        for (int i = 1; i <= columns_r.Count; i++)
                        {
                            mdDowncomer aDC = DComers[i - 1];

                            List<colDXFEntities> col = columns_r[i - 1];
                            if (col.Count > 1)
                            {
                                for (int j = 1; j <= col.Count - 1; j++)
                                {
                                    colDXFEntities col1 = col[j - 1];
                                    if (col1.Count <= 0) break;
                                    colDXFEntities col2 = col[j];
                                    if (col2.Count <= 0) break;
                                    dxeLine ln1 = draw.aLine(col1.BoundingRectangle().BottomCenter, col2.BoundingRectangle().TopCenter, dsp, bSuppressUCS: true).Clone();
                                    if (aDC.OccuranceFactor > 1)
                                    {
                                        ln1.Mirror(VerCenterLn);
                                        Image.Entities.Add(ln1);
                                    }
                                }
                            }
                        }
                    }
                }

                //===================== VIEW LABEL =====================
                rectangle = dEnts.BoundingRectangle();
                v1 = new dxfVector(VerCenterLn.X, rectangle.Bottom - 0.5 * paperscale);
                txt = Assy.DesignFamily.IsEcmdDesignFamily() ? "\\LDOWNCOMER, DECK AND DEFLECTOR PLATE PARTS" : "\\LDOWNCOMER AND DECK PARTS";
                string spanname = Assy.SpanName(aPrefix: "TRAYS ");

                if (Assy.HasAlternateDeckParts)
                {
                    if (aViewIndex == 2)
                    {
                        spanname =( MDRange.RingRange.FirstAlternatingRange.RingCount == 1 ? "TRAY" :"TRAYS" ) + MDRange.RingRange.FirstAlternatingRange.SpanName;
                        
                    }
                    else if (aViewIndex == 3)
                    {
                        txt = "\\LDECK PLATE PARTS";
                        spanname = (MDRange.RingRange.SecondAlternatingRange.RingCount == 1 ? "TRAY" : "TRAYS" ) + MDRange.RingRange.SecondAlternatingRange.SpanName;
                    }
                }

                txt = $"{txt}\\l\\P\\Fromans|c0;\\H0.66666x;FOR {spanname.ToUpper()}";

                aTxt = draw.aText(v1, txt, 0.1875 * paperscale, dxxMTextAlignments.TopCenter, aTextStyle: "Standard", bSuppressUCS: true);
                dEnts.Add(aTxt);


                xCancelCheck();

            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);

            }
            finally
            {
                _rVal = dEnts.BoundingRectangle();
                Status = "";
            }
            return _rVal;
        }

        private dxfRectangle Draw_InstallationBubbles3(dxeLine VerCenterLn, bool bPhantomView = false)
        {


            dxeDimension aDim;
            List<mdDowncomer> DComers = Assy.Downcomers.GetByVirtual(aVirtualValue: false);
            mdDowncomer aDC = DComers.Count > 1 ? DComers[DComers.Count - 2] : DComers.Last();
            mdDowncomer bDC = DComers.Last();
            mdBaffle aBaf;
            dxeText aTxt;
            mdCrossBrace aCB;
            colDXFEntities dEnts = new();
            dxfRectangle aRect;
            double paperscale = Image.Display.PaperScale;

            double apht = Assy.DesignOptions.HasAntiPenetrationPans ? apht =  Assy.APPanHeight : 0;

            colDXFVectors aPts;


            try
            {

                dxoDrawingTool draw = Image.Draw;
                dxoDimTool dims = Image.DimTool;
                dxoLeaderTool leaders = Image.LeaderTool;
                dxeDimension weirdim = null;
                dxeDimension iddim = null;
                dEnts.Add(VerCenterLn, bAddClone: true);
                double halfSpace = MDRange.RingSpacing / 2;
                dxfVector org = Image.UCS.Origin;
                Image.DimStyle().TextFit = dxxDimTextFitTypes.MoveArrowsFirst;

                Status = "Drawing Elevation View Annotations";

                if (Drawing.DrawingUnits != uppUnitFamilies.English)
                {
                    Image.DimStyle().LinearPrecision = 0;
                }

                dxfVector tp = VerCenterLn.StartPt.Y > VerCenterLn.EndPt.Y ? VerCenterLn.StartPt.Clone() : VerCenterLn.EndPt.Clone();
                dxfVector bp = VerCenterLn.StartPt.Y < VerCenterLn.EndPt.Y ? VerCenterLn.StartPt.Clone() : VerCenterLn.EndPt.Clone();
                double y1 = (tp - org).Y + 0.25 * paperscale;

                dxfVector v1 = (tp - org).Moved(-RingRad);
                dxfVector v2 = (tp - org).Moved(RingRad);
                v1.Y = halfSpace;
                v2.Y = v1.Y;
                if (!bPhantomView)
                {
                    // ring id dim
                    aDim = dims.Horizontal(v1, v2, y1, aSuffix: " RING I.D.", bAbsolutePlacement: true);
                    dEnts.Add(aDim);
                    iddim = aDim;

                    y1 = aDim.BoundingRectangle().Top + 0.25 * paperscale - org.Y;

                    v1 = (tp - org).Moved(-MDRange.DeckRadius);
                    v2 = (tp - org).Moved(MDRange.DeckRadius);
                    v1.Y = halfSpace + Assy.Deck.Thickness;
                    v2.Y = v1.Y;

                    // tray od dim
                    aDim = dims.Horizontal(v1, v2, y1, aSuffix: " TRAY O.D.", bAbsolutePlacement: true);
                    dEnts.Add(aDim);
                    dxfVector v3 = bp - org;
                    y1 = v3.Y - 0.25 * paperscale;

                    double d1 = 0.25 * paperscale;
                    xCancelCheck();
                    v1.Y = v3.Y;
                    v2.Y = v3.Y;

                    if (DComers.Count > 1)
                    {
                        v1.X = aDC.X;
                        v2.X = bDC.X;

                        // dc spacing dim
                        aDim = dims.Horizontal(v1, v2, y1, aSuffix: "\\P(TYP)", bAbsolutePlacement: true);
                        y1 = aDim.BoundingRectangle().Bottom - org.Y - d1;
                        dEnts.Add(aDim);
                        if (!Assy.OddDowncomers)
                        {
                            v1.X = 0;
                            v2.X = bDC.X;
                            aDim = dims.Horizontal(v1, v2, y1, bAbsolutePlacement: true);
                            dEnts.Add(aDim);
                        }
                    }
                    else if (DComers.Count == 1)
                    {
                        v1.X = 0;
                        v2.X = bDC.X;
                        aDim = dims.Horizontal(v1, v2, y1, bAbsolutePlacement: true);
                        dEnts.Add(aDim);
                    }

                    aDC ??= bDC;

                    v1 = new dxfVector(-aDC.X, -halfSpace - aDC.HeightBelowDeck - apht);

                    v2 = v1.Moved(-0.5 * aDC.BoxWidth);
                    v1.Move(0.5 * aDC.BoxWidth);
                    // dc outside width dimension
                    aDim = dims.Horizontal(v1, v2, y1, bAbsolutePlacement: true);
                    dEnts.Add(aDim);
                    if (DComers.Count > 1)
                    {
                        v2 = v1.Clone();
                        y1 += d1;
                        v1.X = -bDC.X - 0.5 * bDC.BoxWidth;
                        // dc to dc width (between downcomers)
                        aDim = dims.Horizontal(v1, v2, y1, aSuffix: "\\P(REF)", bAbsolutePlacement: true);
                        dEnts.Add(aDim);
                    }


                    aDC = DComers[0];
                    v3 = new dxfVector(-ShellRad, -halfSpace + Assy.Deck.Thickness);
                    v1 = new dxfVector(-aDC.X - 0.5 * aDC.BoxWidth, -halfSpace + aDC.How + Assy.Deck.Thickness);
                    v2 = new dxfVector(v1.X, -halfSpace - aDC.HeightBelowDeck);
                    // overall dc height dim
                    aDim = dims.Vertical(v1, v2, -ShellRad - 0.8 * paperscale, aSuffix: " DOWNCOMER\\POVERALL HEIGHT", bAbsolutePlacement: true);

                    d1 = aDim.TextPrimary.BoundingRectangle().Right - (org.X - ShellRad - 0.125 * paperscale);
                    if (d1 > 0)
                    {
                        aDim.Extend(d1);
                        aDim.UpdateImage();
                    }

                    dEnts.Add(aDim);
                    xCancelCheck();
                    v1 = new dxfVector(MDRange.DeckRadius, -halfSpace + Assy.Deck.Thickness);
                    v2 = new dxfVector(aDC.X + 0.5 * aDC.BoxWidth, -halfSpace + aDC.How + Assy.Deck.Thickness);
                    // weir height dim
                    aDim = dims.Vertical(v1, v2, ShellRad + 0.95 * paperscale, aSuffix: " WEIR\\PHEIGHT", bAbsolutePlacement: true);


                    dEnts.Add(aDim);
                    weirdim = aDim;
                    if (Assy.DesignFamily.IsEcmdDesignFamily())
                    {
                        aDC = DComers.Where(dc => dc.Boxes.Any(dcb => !dcb.IsVirtual)).Last();
                         List<mdBaffle>aBafs = MDProject.GetParts().DeflectorPlates().FindAll((x) => x.IsAssociatedToRange(Assy.RangeGUID)  && x.DowncomerIndex == aDC.Index ); // MDProject.GetParts(uppPartTypes.Deflector, Assy.RangeGUID, aDC.Index);
                        if (aBafs.Count > 0)
                        {
                            aBaf = (mdBaffle)aBafs.LastOrDefault();
                            aPts = aBaf.GenHoles(Assy).DXFCenters("");

                            if (aPts.Count > 0)
                            {
                                v1 = aPts.GetVector(dxxPointFilters.AtMinY, 0, bReturnClone: true);
                                v1 = new dxfVector(v1.Y, halfSpace + v1.Z);

                                v2 = new dxfVector(v1.X - 0.375 * paperscale, halfSpace + aBaf.Z + aBaf.Height + (0.375 * paperscale * 0.5) + 0.125 * paperscale);
                                if (iddim != null)
                                {
                                    v2.Y = iddim.TextPrimary.BoundingRectangle().Bottom - 0.0625 * paperscale - Image.UCS.Y;
                                }
                                v3 = new dxfVector(-ShellRad - 0.125 * paperscale, v2.Y);


                                dEnts.Add(leaders.NoRef(v1, v3, v2));

                                if (aBafs.Count > 1)
                                {

                                    v1 = aPts.GetVector(dxxPointFilters.AtMaxY);
                                    v1 = new dxfVector(v1.Y, halfSpace + v1.Z);
                                    v2 = v2.Clone();
                                    v2.X = v1.X - 0.375 * paperscale;
                                    dEnts.Add(leaders.NoRef(v1, v3, v2));

                                    Tool.CreatePNBubbles(Image, v3, paperscale, dxxOrthoDirections.Left, "", "Q1,Q2", "DEFLECTOR\\PPLATE", aCollector: dEnts);
                                }
                                else
                                {
                                    Tool.CreatePNBubbles(Image, v3, paperscale, dxxOrthoDirections.Left, "", "Q1", "DEFLECTOR\\PPLATE", aCollector: dEnts);
                                }
                            }
                        }
                    }

                    if (Assy.DesignOptions.CrossBraces)
                    {
                        aDC = DComers[0];
                        aCB = Assy.CrossBrace;
                        if (aCB != null)
                        {
                            v1 = new dxfVector(aCB.X + 0.5 * aCB.Length, -halfSpace + aCB.Z - aCB.Height);
                            v2 = new dxfVector(v1.X + 0.25 * paperscale, v1.Y - 0.375 * paperscale);
                            v3 = new dxfVector(ShellRad + 0.125 * paperscale, v2.Y);

                            dEnts.Add(leaders.Text(v1, v3, "CROSSBRACE\\P(2 PLACES)", v2));
                        }
                    }

                    if (Assy.DesignOptions.HasAntiPenetrationPans)
                    {
                        aDC = DComers[1];
                        v1 = new dxfVector(aDC.X + 0.5 * aDC.BoxWidth, -halfSpace - aDC.HeightBelowDeck);
                        v2 = new dxfVector(v1.X + 0.125 * paperscale, v1.Y - 0.275 * paperscale);
                        if (weirdim != null)
                        {
                            double bot = weirdim.BoundingRectangle().Bottom;
                            if (v2.Y > bot - 0.25 * paperscale)
                                v2.Y = bot - 0.25 * paperscale;
                        }

                        v3 = new dxfVector(ShellRad + 0.25 * paperscale, v2.Y);
                        dEnts.Add(leaders.NoRef(v1, v3, v2));
                        Tool.CreatePNBubbles(Image, v3, paperscale, dxxOrthoDirections.Right, "", "S1,S2", "AP PAN", aCollector: dEnts);
                    }

                    aRect = dEnts.BoundingRectangle();
                    y1 = -1.25 * MDRange.RingSpacing;
                    v1 = new dxfVector(-ShellRad, y1);
                    v2 = new dxfVector(ShellRad, y1);
                    y1 = aRect.Bottom - org.Y;
                    aDim = dims.Horizontal(v1, v2, y1 - 0.25 * paperscale, aSuffix: " COLUMN I.D.", bAbsolutePlacement: true);
                    dEnts.Add(aDim);

                    xCancelCheck();
                    aRect = aDim.BoundingRectangle();
                    v1 = (aRect.BottomCenter - org).Moved(aYChange: -0.2 * paperscale);



                    aTxt = draw.aText(v1, "\\LSECTIONAL ELEVATION A1-A1", 0.1875 * paperscale, dxxMTextAlignments.TopCenter, aTextStyle: "Standard");
                    dEnts.Add(aTxt);

                    v1 = (aTxt.BoundingRectangle().BottomCenter - org).Moved(aYChange: -0.125 * paperscale);
                    aTxt = draw.aText(v1, "ROTATED 90%%D CCW", 0.125 * paperscale, dxxMTextAlignments.TopCenter, aTextStyle: "Standard");
                    dEnts.Add(aTxt);
                }
                else
                {
                    v1 = (bp - org).Moved(aYChange: -0.2 * paperscale);
                    draw.aText(v1, Assy.TrayName(true), 0.125 * paperscale, dxxMTextAlignments.TopCenter, aTextStyle: "Standard");

                }


                Status = "";

                xCancelCheck();

                return null; // This has been defined as a function in VB code but it does not return anything !!!
            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
                return null;
            }
        }

        private dxfRectangle Draw_View_Elevation(uppViews aView, out dxeLine rVerCenterLn, bool bPhantomView = false)
        {


            rVerCenterLn = null;
            dxfRectangle result = null;
            int si = Image.Entities.Count + 1;
            DrawPhantom = bPhantomView;
            try
            {
                Status = "Drawing Elevation View";
                if (aView != uppViews.InstallationElevation && aView != uppViews.LayoutElevation)
                {
                    aView = uppViews.LayoutElevation;
                }

                if (Assy.DesignFamily.IsBeamDesignFamily())
                {
                    ElevationX = (Assy.OddDowncomers) ? -Assy.DeckPanels.Item(Assy.DeckPanels.Count / 2).X : 0;
                }
                else
                {
                    ElevationX = (Assy.OddDowncomers) ? -Assy.DeckPanels.LastPanel().X : 0;
                }


                Draw_Shells(aView, out rVerCenterLn, out _);
                Draw_Rings(aView);

                if (aView == uppViews.LayoutElevation)
                {
                    Image.Display.ZoomExtents(1.25);
                }


                Draw_Downcomers(aView, out _,bDrawFingerClips:true, bDrawStiffeners:true,bDrawEndAngles: true,bDrawBaffles:true) ;
                //Draw_CrossBraces( aView, aDrawProfiles: false);
                Draw_SpliceAngles(aView, out string sSplcPnls);
                Draw_DeckSections(uppViews.LayoutElevation, bDrawBothSides: false, sSplcPnls: sSplcPnls);
                
                if (aView == uppViews.InstallationElevation && Assy.DesignFamily.IsBeamDesignFamily())
                {
                    var beam = Assy.Beam;
                    var beamDisplaySettings = dxfDisplaySettings.Null( aLayer: "IBeam", aColor: dxxColors.LightGrey, aLinetype: "PHANTOM" );
                    var beamElevationViewPolygon = beam.View_Elevation( aCenter: dxfVector.Zero, showHoles: false, angledView: true, beamDisplaySettings: beamDisplaySettings );
                    Image.Draw.aInsert( beamElevationViewPolygon, dxfVector.Zero, aBlockName: $"SUPPORT_BEAM{beam.PartNumber}_ELEVATION" );
                }


                result = Image.Entities.SubSet(si, Image.Entities.Count ).BoundingRectangle( dxfPlane.World );
                //VerCenterLn = Image.Draw.aLine( result.TopCenter, result.BottomCenter, aLineType: dxfLinetypes.Center, bSuppressUCS: true, bSuppressElevation: true );



                Draw_InstallationBubbles3(rVerCenterLn, bPhantomView);

                xCancelCheck();

                return result;
            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); return result; }
            finally { Status = string.Empty;  DrawPhantom = false; }
        }

        private void Draw_Installation_PlanView(int aViewIndex, double paperscale, out dxeLine VerCenterLn, out dxeLine HorCenterLn)
        {
            VerCenterLn = null;
            HorCenterLn = null;
            //^generates the md installation drawing plan view

            try
            {
                aViewIndex = mzUtils.LimitedValue(aViewIndex, 1, 3);

                dxfVector v1 = dxfVector.Zero;
                dxfVector v2 = dxfVector.Zero;

                dxfPlane aPln = Image.UCS;

                //=========================================================== Rings and shells
                dxfRectangle aRect = Draw_Shells(uppViews.InstallationPlan, out VerCenterLn, out HorCenterLn);
                Draw_Rings(uppViews.InstallationPlan);

                if (Assy.DesignFamily.IsBeamDesignFamily())
                {
                    Draw_Beams(uppViews.Plan, bSuppressCenterLine: true, bSuppressSupports: false);
                }

                if (paperscale <= 0) paperscale = Image.Display.ZoomExtents(bSetFeatureScale: true);

                xCancelCheck();

                Image.Layers.Add("DOWNCOMERS");
                Image.Layers.Add("DECK", dxxColors.z_41);

                if (aViewIndex == 1)
                {
                    Draw_DowncomersBelow(uppViews.InstallationPlan, bBothSides:true);
  
                }
                 if (aViewIndex ==3) Image.UCS.Rotate(90);
                List<dxeInsert> dcinserts = Draw_Downcomers(uppViews.InstallationPlan, out _, bDrawSpouts: false, bDrawBothSides: true,  aViewIndex: aViewIndex,bDrawBaffles:true);
                colDXFVectors fcpoints = new();
                colDXFEntities eainserts = new();

                if (aViewIndex == 1)
                {
                    dxfImage img = Image;
                    fcpoints = new colDXFVectors(); // Draw_FingerClips(true);
                    foreach (var dcinsert in dcinserts)
                    {
                        dxfBlock dcblok = null;
                         if (dcinsert.GetBlock(ref img,ref dcblok))
                        {
                            List<dxeInsert> fcinserts = dcblok.Entities.GetNestedInserts(img).FindAll((x)=> string.Compare(x.Tag,"FINGER CLIP",true) ==0);
                            foreach (var fcinsert in fcinserts)
                            {
                                colDXFVectors ipts = fcinsert.Instances.MemberPoints(fcinsert.InsertionPt, aReturnBasePt: true);
                                foreach (var pt in ipts)
                                {
                                    fcpoints.Add(pt + dcinsert.InsertionPt);
                                }
                            }

                        }
                        
                    }

                    eainserts = Draw_EndAngles(true);
                    if (Assy.HasAntiPenetrationPans)
                        Draw_APPans(uppViews.Plan, bObscured: true, bDrawBothSides: true);
                    Draw_SpliceAngles(uppViews.InstallationPlan, out _, true);
                    xCancelCheck();
                }

                List<dxeInsert> decksections = Draw_DeckSections(uppViews.InstallationPlan, aViewIndex: aViewIndex);
                if (aViewIndex == 3) Image.UCS.Rotate(-90);

                if (aViewIndex == 1)
                {
                    xCancelCheck();
                    Draw_ManwayFasteners(uppViews.InstallationPlan, true);
                    xCancelCheck();
                    Draw_Washers(true);
                }
                xCancelCheck();

                if (DrawlayoutRectangles && aRect != null)
                {
                    Image.Draw.aPolyline(aRect, aDisplaySettings: dxfDisplaySettings.Null(aColor: dxxColors.LightGrey), bSuppressUCS: true).SaveToFile = false;
                }

                if (aViewIndex == 1)
                {
                    uopHoleArray dsHoles = Assy.DeckSections.GenHoles(Assy, bTrayWide: true);
                    aRect = Draw_InstallationBubbles1(aRect, VerCenterLn, HorCenterLn, dsHoles, fcpoints, eainserts);
                    xCancelCheck();
                }
                else
                {
                    aRect = Draw_InstallationBubbles2(aViewIndex, VerCenterLn, HorCenterLn, decksections);
                    xCancelCheck();
                }



            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
            finally
            {
                Image.UCS.Reset();
                DrawinginProgress = false;
                Status = "";
                Drawing.ZoomExtents = false;
            }

        }


        public dxfImage GenerateImage(uopDocDrawing argDrawing, bool bSuppressErrors = false, System.Drawing.Size aImageSize = new System.Drawing.Size(), uopTrayRange preSelectedTrayRange = null, bool bUsingViewer = true, System.Drawing.Color? BackColor = null, bool bSuppressIDEEffects = false)
        {
            Tool.TitleBlockHelper = null;
            if (argDrawing == null) return null;

            DrawinginProgress = true;
            CancelOps = false;


            string ErrStr;
            System.Diagnostics.Stopwatch sw = new();

            try
            {

                DrawlayoutRectangles = uopUtils.RunningInIDE && !bSuppressIDEEffects;

                // direct the drawing request to the appropriate subroutine
                
                DrawinginProgress = false;
                if (aImageSize.IsEmpty) aImageSize = argDrawing.DeviceSize;
                if (!BackColor.HasValue) BackColor = appApplication.SketchColor;

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



                Drawing = argDrawing;
                SuppressBorder = Drawing.SuppressBorder;

                Project ??= Drawing.Project;
                // make the range and request available to all drawing functions
                MDRange = (mdTrayRange)preSelectedTrayRange ?? (mdTrayRange)Drawing.Range;
                if (MDRange == null)
                {
                    uopPart part = Drawing.Part;
                    MDRange = part?.GetMDRange(null);
                    if (MDRange == null)
                    {
                        Range = Drawing.Range;
                    }
                }

                Drawing.DrawTime = "";
                sw.Start();

                if (Drawing.DeviceSize.Width > 0 && Drawing.DeviceSize.Height > 0)
                    Image.Display.Size = Drawing.DeviceSize;


                // pass the requested draw settings to the draw object
                Tool.SetDrawProperties(Image, Drawing);

                DrawingType = Drawing.DrawingType.GetDescription();

                // set shortcut variable Values
                SetDrawingVars();

                Console.WriteLine($"{Image.Layer("0").Color}");
                DrawinginProgress = true;

                switch (Drawing.DrawingType)
                {
                    case uppDrawingTypes.StiffenerEdit:
                        DDWG_EditView_STIFFENERS();
                        break;
                    case uppDrawingTypes.AssemblyDetails:
                        DDWG_AssemblyDetails();
                        break;
                    case uppDrawingTypes.EndAngle:
                        DDWG_EndAngles_OLD();
                        break;
                    case uppDrawingTypes.EndAngles:
                        DDWG_EndAngles();
                        break;
                    case uppDrawingTypes.SpliceAngle:
                        DDWG_SpliceAnglesOLD();
                        break;
                    case uppDrawingTypes.SpliceAngles:
                        DDWG_SpliceAngles();
                        break;
                    case uppDrawingTypes.Functional:
                        Drawing.DrawingUnits = uppUnitFamilies.English;
                        DDWG_Functional();
                        break;
                    case uppDrawingTypes.FunctionalActiveAreas:
                        DDWG_FunctionalActiveAreas();
                        break;
                    case uppDrawingTypes.Installation:
                        DDWG_Installation();
                        break;
                    case uppDrawingTypes.DowncomerBox:
                        DDWG_DC_Box();
                        break;
                    case uppDrawingTypes.APPan:
                        DDWG_APPan();
                        break;
                    //case uppDrawingTypes.SpliceAngle:
                    //    DDWG_SpliceAngle(false);
                    //    break;
                    case uppDrawingTypes.ManwayAngle:
                        DDWG_SpliceAngle(true);
                        break;
                    case uppDrawingTypes.ManwaySplicePlate:
                        DDWG_ManwaySplice();
                        break;
                    case uppDrawingTypes.ManwaySplicePlates:
                        DDWG_ManwaySplice_Tabular();
                        break;
                    case uppDrawingTypes.DefinitionLines:
                        DDWG_DefinitionLines();
                        break;
                    case uppDrawingTypes.EqualSections:
                        DDWG_UniqueSections();
                        break;
                    case uppDrawingTypes.DeflectorPlate:
                        DDWG_DeflectorPlate(argDrawing.PartIndex);
                        break;
                    case uppDrawingTypes.SupplDeflector:
                        DDWG_SuplDeflector((mdSupplementalDeflector)argDrawing.Part);
                        break;
                    case uppDrawingTypes.SupplDeflectors:
                        DDWG_SupplementalDeflectors();
                        break;
                    case uppDrawingTypes.SheetIndex:
                        DDWG_SheetIndex();
                        break;
                    case uppDrawingTypes.Sheet2:
                        DDWG_MaterialList();
                        break;
                    case uppDrawingTypes.Sheet3:
                        DDWG_Notes();
                        break;
                    case uppDrawingTypes.Sheet4:
                        DDWG_StandardDetails();
                        break;
                    case uppDrawingTypes.Sheet5:
                        DDWG_SpoutPatterns();
                        break;
                    case uppDrawingTypes.Sheet6:
                  
                            DDWG_SlotAndTabDetails();
                        break;
                    case uppDrawingTypes.Sheet7:
                        //DDWG_HardwareQuantities();
                        break;
                    case uppDrawingTypes.DCAssembly:
                        DDWG_DCAssembly();
                        break;
                    case uppDrawingTypes.Baffles:
                        DDWG_Baffles();
                        break;
                    case uppDrawingTypes.Stiffener:
                        DDWG_Stiffener();
                        break;
                    case uppDrawingTypes.EndPlate:
                        DDWG_DC_EndPlate();
                        break;

                    case uppDrawingTypes.CrossBrace:
                        DDWG_CrossBrace(Assy.CrossBrace);
                        break;
                    case uppDrawingTypes.EndSupport:
                        DDWG_DC_EndSupport();
                        break;
                    case uppDrawingTypes.RingClip3in:
                    case uppDrawingTypes.RingClip4in:
                    case uppDrawingTypes.RingClipDC4in:
                        //DrawRingClip();
                        break;
                    case uppDrawingTypes.PanelsAndSlots:
                        DDWG_SlotLayout();
                        break;
                    case uppDrawingTypes.StartUpLines: // only visible when not running in IDE
                        DDWG_StartupLines();
                        break;
                    case uppDrawingTypes.BlockedAreas:
                        DDWG_FreeBubblingAreas();
                        //DDWG_BlockedAreas();
                        break;
                    case uppDrawingTypes.RingClipSegments:
                        DDWG_RingClipSegments();
                        break;
                    case uppDrawingTypes.TestDrawing:
                        Testing = true;
                        if (string.Compare( appApplication.User.NetworkName, "E342367", true ) == 0)
                            AADrawTest();
                        else
                            BeamDrawTest();
                        Testing = false;
                        break;
                    case uppDrawingTypes.InputSketch: // the sketch shown on the input form (not in list)
                        if (Drawing.Range != null) DDWG_InputSketch();

                        break;
                    case uppDrawingTypes.SectionSketch:
                        DDWG_SectionSketch();
                        break;
                    case uppDrawingTypes.LayoutElevation:
                        DDWG_ElevationView();
                        break;
                    case uppDrawingTypes.LayoutPlan:
                        DDWG_PlanView();
                        break;
                    case uppDrawingTypes.DowncomerDesign:
                        DDWG_DowncomerDesign();
                        //if (Drawing.PartIndex > 0 && Drawing.PartIndex <= Assy.Downcomers.Count)
                        //{
                        //    DDWG_DowncomerDesign(Assy.Downcomers.Item(Drawing.PartIndex));
                        //}
                        break;
                    case uppDrawingTypes.PanelsOnly:
                        DDWG_DeckSectionsOnly();
                        break;
                    case uppDrawingTypes.DowncomersOnly:
                        DDWG_DowncomersOnly();
                        break;
                    case uppDrawingTypes.DeckPanel:
                        DDWG_DeckSection();
                        break;
                    case uppDrawingTypes.ManClamp:
                        DDWG_ManwayClamp();
                        break;
                    case uppDrawingTypes.ManwayClip:
                        DDWG_ManwayClip();
                        break;
                    case uppDrawingTypes.SectionShapes:
                        DDWG_SectionShapes();
                        break;
                    case uppDrawingTypes.DowncomerManholeFit:
                        DDWG_DowncomerFit();
                        break;
                    case uppDrawingTypes.BeamDetails:
                        DDWG_Beam();
                        break;
                    case uppDrawingTypes.BeamAttachments:
                        DDWG_BeamAttachments();
                        break;
                    case uppDrawingTypes.SpoutAreas:
                        DDWG_SpoutAreas(); 
                        break;
                    case uppDrawingTypes.SGInputSketch:
                        Tool.DDWG_SpoutGroupInputSketch(this);

                        break;
                    default:
                        break;
                }

                SpoutGroup = Assy?.SpoutGroups.SelectedMember;

                // zoom extents
                if (!CancelOps)
                {
                    Status = "Drawing Complete";

                    //if (Drawing.ZoomExtents)
                    //{
                    //    Status = "Zooming Extents";
                    //    Image.Display.ZoomExtents();
                    //    Drawing.ExtentWidth = Image.Display.ViewWidth;
                    //    Drawing.ViewCenter = Image.Display.ViewCenter;
                    //    Status = "Drawing Complete";
                    //}
                }
                else
                {
                    Image.Screen.Entities.aScreenText(null, "Drawing Canceled Prematurely", 0.015, true, dxxRectangularAlignments.BottomLeft, dxxColors.LightRed, 0.8);
                }

                if (Image != null && Drawing != null)
                {
                    Image.ImageName = Drawing.DrawingName;
                }

            }
            catch (Exception e)
            {
                ErrStr = $"({e})";


                //Err.Raise 1000, "appDrawingsMDD.GenerateImage", ErrStr
                MessageBox.Show($"{e}", $"Error Code 1000"); // I don't know if it has the same effect as above line from VB !!!
            }
            finally
            {
                Testing = false;
                if (!CancelOps)
                {
                    Image.UCS.Reset();
                    if (!Image.UsingDxfViewer) Image.Display.Refresh(true);
                }

                //if (Image.ErrorCount > 0 && !bSuppressErrors)
                //{
                //    Image.DisplayErrors($"{Drawing.DrawingName} Drawing Errors");
                //}


                DrawinginProgress = false;
                sw.Stop();
                object secondsObject = sw.Elapsed.TotalSeconds;
                argDrawing.DrawTime = dxfUtils.FormatSeconds(secondsObject);




                DrawinginProgress = false;



            }

            return Image;
        }

        private void DDWG_SpoutAreas() => Tool.DDWG_SpoutAreas(this);

        private void DDWG_FunctionalActiveAreas()
        {
            Tool.DDWG_FunctionalActiveAreas(this);
        }

        private void DDWG_EditView_STIFFENERS()
        {
            if (Assy == null) return;
            try
            {
                Status = "Drawing Stiffener Edit View";
                Image.Layer("0").Color = dxxColors.LightGrey;
                Image.Layers.Add("DOWNCOMERS", dxxColors.BlackWhite);
                Image.Layers.Add("END ANGLES", dxxColors.BlackWhite);
                Image.Layers.Add("DOWNCOMERS BELOW", dxxColors.Green);
                Image.Layers.Add("SECTION SHAPES", dxxColors.LightGrey);
                Image.Layers.Add("LIMIT LINES", dxxColors.BlackWhite);
                Image.Layers.Add("STARTUPS", dxxColors.BlackWhite);
                Image.TextStyle().FontName = "RomanS.shx";
                Image.Header.LayerName = "0";
                dxoStyle tstyle = Image.TextStyle();
                tstyle.WidthFactor = 0.8;
                tstyle.FontName = "RomanS.shx";
                mdStartupSpouts StartUps = Assy.Downcomers.StartupSpouts(Assy, true);
                double lng = StartUps.Length;
                double suwd = 6 * Assy.Downcomer().Thickness;
                dxePolyline pill = Image.EntityTool.CreateShape_Pill(dxfVector.Zero, lng, suwd, 90, aLayer: "STARTUPS", aColor: dxxColors.BlackWhite, aLineType: dxfLinetypes.Continuous);
                bool bBafLines = Assy.DesignFamily.IsEcmdDesignFamily();
                Image.LinetypeLayers.Setting = dxxLinetypeLayerFlag.Suppressed;
                mdDeckSections aSecs = Assy.DeckSections;
                dxoDrawingTool draw = Image.Draw;
                dxfDisplaySettings dsp = new dxfDisplaySettings(aLayer: "SECTION SHAPES");
                for (int i = 1; i <= aSecs.Count; i++)
                {
                    uopShape aShp = aSecs.Item(i).BaseShape;
                    draw.aShape(aShp, aDisplaySettings: dsp);
                }
                Image.Display.ZoomExtents(bSetFeatureScale: true);
                double paperscale = Image.Display.PaperScale;

                dxfRectangle bounds = Image.Display.ExtentRectangle.Expanded(1.1);
                draw.aCenterlines(bounds, aSuppressVertical: true, aDisplaySettings: dxfDisplaySettings.Null(aLinetype: dxfLinetypes.Center, aColor: dxxColors.Yellow));
                double rad = Assy.DeckRadius + 0.125 * paperscale;
                colMDDowncomers dcs = Assy.Downcomers;

                // Draw deck section shapes
                var allPanelShapes = Assy.DowncomerData.PanelShapes(bIncludeClearance: false);

                 dsp = new dxfDisplaySettings("0", aColor: dxxColors.Blue, aLinetype: dxfLinetypes.Continuous);

                foreach (var panelShape in allPanelShapes)
                {
                    if (panelShape.IsVirtual) continue;

                    Image.Entities.Add(new dxePolyline(panelShape.Vertices, true, aDisplaySettings: dsp), aTag: $"PANEL{panelShape.PartIndex}");
                }

                double tht = 0.06 * paperscale;
                double y1;
                dxfVector ep;
                dxeLine l1;
                for (int i = 1; i <= dcs.Count; i++)
                {
                    mdDowncomer dc = dcs.Item(i);
                    if (dc.IsVirtual) continue;

                    draw.aText(new dxfVector(dc.X, Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(dc.X, 2))), $"{i}", aAlignment: dxxMTextAlignments.MiddleCenter);

                    foreach (var box in dc.Boxes)
                    {
                        if (box.IsVirtual) continue;

                        List<mdEndAngle> eangs = box.EndAngles();

                        colDXFVectors verts = box.Vertices(bRotated: false, bSupressFoldovers: true);
                        dsp.SetDisplayValues(aLayerName: "DOWNCOMERS", aColor: dxxColors.ByLayer, aLineType: dxfLinetypes.Continuous);
                        draw.aPolyline(verts, bClosed: true, aDisplaySettings: dsp);

                        dsp.SetDisplayValues(aLayerName: "END ANGLES", aColor: dxxColors.ByLayer, aLineType: dxfLinetypes.Continuous);
                        foreach (var item in eangs)
                        {
                            dxePolygon aPg = mdPolygons.EndAngle_View_Plan(item, bSuppressHoles: false, aLayerName: dsp.LayerName);
                            aPg.DisplaySettings = dsp;
                            Image.Entities.Append(aPg.SubEntities());
                        }

                        dsp.SetDisplayValues(aLayerName: "DOWNCOMERS BELOW", aColor: dxxColors.Green, aLineType: dxfLinetypes.Hidden);

                        verts = box.ExtractDowncomerBelowVertices(true, true);

                        colDXFVectors lefties = new colDXFVectors(verts.GetVectors(dxxPointFilters.LessThanX, bounds.Left));

                        lefties.SetCoordinates(bounds.Left);
                        Image.LinetypeLayers.Setting = dxxLinetypeLayerFlag.ForceToColor;
                        draw.aPolyline(verts, bClosed: true, aDisplaySettings: dsp);
                        if (dc.OccuranceFactor == 2)
                        {
                            verts.MirrorPlanar(aMirrorY: 0);
                            draw.aPolyline(verts, bClosed: false, aDisplaySettings: dsp);
                        }

                        //dsp.SetDisplayValues(aLayerName: "STARTUPS", aColor: dxxColors.ByLayer, aLineType: dxfLinetypes.Continuous);
                        uopVectors cntrs = StartUps.GetByDowncomerIndex(dc.Index,0).Centers();
                        int j = 0;
                        foreach (uopVector cntr in cntrs)
                        {
                            j++;
                            dxePolyline subound = pill.Clone();
                            subound.Move(cntr.X, cntr.Y);
                            subound.TFVSet(i.ToString(), j.ToString());
                            Image.Entities.Add(subound);
                        }

                        // Draw top stiffener required zones limit lines
                        var topMandatoryStiffenerRangeOrdinates = box.TopMandatoryStiffenerRangeOrdinates();

                        y1 = topMandatoryStiffenerRangeOrdinates.MaxY;
                        dsp.SetDisplayValues(aLayerName: "LIMIT LINES", aColor: dxxColors.Red, aLineType: dxfLinetypes.Continuous);
                        ep = new dxfVector(dc.X + 0.5 * dc.Width + 0.0875 * paperscale, y1);

                        l1 = draw.aLine(new dxfVector(dc.X - 0.5 * dc.Width, y1), ep, dsp);
                        l1.Tag = $"LIMIT LINE";
                        l1.Flag = "MAX";

                        draw.aText(ep.Moved(0.1 * paperscale), "MAX Y", aTextHeight: tht, aAlignment: dxxMTextAlignments.MiddleLeft, aColor: dsp.Color);

                        dsp.SetDisplayValues(aLayerName: "LIMIT LINES", aColor: dxxColors.Yellow);
                        y1 = topMandatoryStiffenerRangeOrdinates.MinY;
                        ep = new dxfVector(dc.X + 0.5 * dc.Width + 0.0875 * paperscale, y1);

                        l1 = draw.aLine(new dxfVector(dc.X - 0.5 * dc.Width, y1), ep, dsp);
                        l1.Tag = $"LIMIT LINE";
                        l1.Flag = "MIN";

                        draw.aText(ep.Moved(0.1 * paperscale), "MIN Y", aTextHeight: tht, aAlignment: dxxMTextAlignments.MiddleLeft, aColor: dsp.Color);



                        // Draw bottom stiffener required zone limit lines
                        var bottomMandatoryStiffenerRangeOrdinates = box.BottomMandatoryStiffenerRangeOrdinates();

                        dsp.SetDisplayValues(aLayerName: "LIMIT LINES", aColor: dxxColors.Yellow);
                        y1 = bottomMandatoryStiffenerRangeOrdinates.MaxY;
                        ep = new dxfVector(dc.X + 0.5 * dc.Width + 0.0875 * paperscale, y1);

                        l1 = draw.aLine(new dxfVector(dc.X - 0.5 * dc.Width, y1), ep, dsp);
                        l1.Tag = $"LIMIT LINE";
                        l1.Flag = "MIN";

                        draw.aText(ep.Moved(0.1 * paperscale), "-MIN Y", aTextHeight: tht, aAlignment: dxxMTextAlignments.MiddleLeft, aColor: dsp.Color);

                        y1 = bottomMandatoryStiffenerRangeOrdinates.MinY;
                        dsp.SetDisplayValues(aLayerName: "LIMIT LINES", aColor: dxxColors.Red, aLineType: dxfLinetypes.Continuous);
                        ep = new dxfVector(dc.X + 0.5 * dc.Width + 0.0875 * paperscale, y1);

                        l1 = draw.aLine(new dxfVector(dc.X - 0.5 * dc.Width, y1), ep, dsp);
                        l1.Tag = $"LIMIT LINE";
                        l1.Flag = "MAX";

                        draw.aText(ep.Moved(0.1 * paperscale), "-MAX Y", aTextHeight: tht, aAlignment: dxxMTextAlignments.MiddleLeft, aColor: dsp.Color);
                    }
                }
            }
            catch (Exception e)  {  HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);  }
            finally  {  Status = ""; DrawinginProgress = false;  }
        }

        private void DDWG_APPan()
        {

            //^generates the ap pan drawing for the passed pan
            if (Drawing.Part == null) return;
            if (Drawing.Part.PartType != uppPartTypes.APPan) return;
            colDXFEntities dwgents;
            dxePolygon Profile = null;
            dxePolygon Perim = null;
            dxePolygon Elev = null;

            try
            {
                mdAPPan aPan = (mdAPPan)Drawing.Part;

                if (aPan == null) return;

                dxfVector v1 = dxfVector.Zero;
                dxfVector v2 = dxfVector.Zero;
                dxfVector v3 = dxfVector.Zero;
                List<string> Notes = new();

                double paperscale = 0;
                double thk = aPan.Thickness;
                dxeLine cl1;
                bool zPan = aPan.OpenEnded;
                dxeInsert iPerim;
                dxeInsert iProfile;
                dxeInsert iElev = null;
                colDXFVectors vecs;
                dxoDrawingTool draw = Image.Draw;
                dxoDimTool dims = Image.DimTool;
                dxoLeaderTool leaders = Image.LeaderTool;
                dxoEntityTool etool = Image.EntityTool;
                dxeDimension dim;
                xCancelCheck();



                // ^creates the component drawing for the passed appan
                string lname = "GEOMETRY";
                Image.LayerColorAndLineType_Set(lname, dxxColors.ByLayer, "ByLayer", true);


                Profile = aPan.View_Profile(false, dxfVector.Zero, 0, false, lname);
                Profile.Orthoganolize(0.05);
                iProfile = etool.Create_Insert(Profile, dxfVector.Zero, aRotationAngle: 0, aBlockName: $"PAN_{aPan.PartNumber}_PROFILE_MFG", aLTLSetting: dxxLinetypeLayerFlag.ForceToColor);

                Perim = aPan.View_Plan(false, dxfVector.Zero, 0, lname);
                Perim.Orthoganolize(0.05);
                iPerim = etool.Create_Insert(Perim, dxfVector.Zero, aRotationAngle: -90, aBlockName: $"PAN_{aPan.PartNumber}_PLAN_MFG", aLTLSetting: dxxLinetypeLayerFlag.ForceToColor);
                Perim.Rotate(-90);

                Elev = null;
                dxfRectangle aRec;
                if (!zPan)
                {
                    Elev = aPan.View_Elevation(aTabEnd: true, bShowObscured: false, aCenter: dxfVector.Zero, aRotation: 0, aLayerName: lname);
                    iElev = etool.Create_Insert(Elev, dxfVector.Zero, aBlockName: $"PAN_{aPan.PartNumber}_ELEV_MFG", aLTLSetting: dxxLinetypeLayerFlag.ForceToColor);
                    aRec = new dxfRectangle(aWidth: iPerim.Width() + 2 * aPan.Width + 3, aHeight: 2 * aPan.Height + aPan.Width);
                }
                else
                {
                    aRec = new dxfRectangle(aWidth: iPerim.Width() + 2, aHeight: iPerim.Height() + 2.5 * iProfile.Height());
                }

                dwgents = new colDXFEntities(iPerim, iProfile, iElev);


                //=====================================================================================================
                // insert the border
                dxfRectangle fitRec = null;

                Tool.Border(this, aRec, ref fitRec, ref paperscale, 2, 2, bSuppressAnsiScales: false);
                aRec.MoveTo(fitRec.Center);


                xCancelCheck();
                iProfile.MoveFromTo(iProfile.DefinitionPoint(dxxEntDefPointTypes.BottomLeft), fitRec.BottomLeft.Moved(1.125 * paperscale, 1.0 * paperscale));
                dxfRectangle prRec = iProfile.BoundingRectangle();
                Profile.MoveFromTo(Profile.DefinitionPoint(dxxEntDefPointTypes.MiddleCenter), prRec.Center);


                iPerim.MoveFromTo(iPerim.DefinitionPoint(dxxEntDefPointTypes.TopLeft), fitRec.TopLeft.Moved(1.125 * paperscale, -1.125 * paperscale));
                dxfRectangle pRec = iPerim.BoundingRectangle();
                Perim.MoveFromTo(Perim.DefinitionPoint(dxxEntDefPointTypes.MiddleCenter), pRec.Center);
                dxfRectangle eRec = null;
                if (!zPan)
                {
                    eRec = iElev.BoundingRectangle();
                    v1 = eRec.TopRight;
                    v2 = new dxfVector(fitRec.Right - 0.125 * paperscale - aPan.Width, pRec.Top);

                    v3 = v2 - v1;
                    iElev.Translate(v3);
                    eRec.Translate(v3);
                    Elev.Translate(v3);
                }

                int startentidx = Image.Entities.Count + 1;
                Image.SaveEntity(null, dwgents);

                xCancelCheck();

                //===================== PLAN VIEW ========================

                Image.SelectionSetInit(false);
                {
                    colDXFEntities Holes = Perim.AdditionalSegments.GetByGraphicType(dxxGraphicTypes.Arc);

                    v1.MoveTo(Perim.InsertionPt, 0.5 * aPan.Length + aPan.FlangeLength + 0.1 * paperscale);
                    v2.MoveTo(Perim.InsertionPt, -0.5 * aPan.Length - aPan.TabLength - 0.1 * paperscale);
                    cl1 = draw.aLine(pRec.MiddleLeft().Moved(-0.125 * paperscale), pRec.MiddleRight().Moved(0.125 * paperscale), aLineType: dxfLinetypes.Center);
                    v1 = Perim.GetVertex(dxxPointFilters.GetRightTop, bReturnClone: true);
                    v2 = Perim.GetVertex(dxxPointFilters.GetRightBottom, bReturnClone: true);

                    if (Holes.Count > 0)
                    {
                        List<dxeLine> clines;
                        dxeArc aHole = (dxeArc)Holes.Item(1);
                        if (Holes.Count == 1)
                        {
                            dim = dims.Vertical(cl1.EndPt, v2, 0.45);
                            clines = draw.aCenterlines(Holes, 0.0625 * paperscale, aSuppressHorizontal: true);
                            cl1 = clines[0];
                        }
                        else
                        {
                            vecs = Holes.Centers();
                            aHole = (dxeArc)Holes.GetByCenter(vecs.GetVector(dxxPointFilters.AtMaxY)).LastOrDefault();
                            cl1 = draw.aLine(vecs.GetVector(dxxPointFilters.AtMinY).Moved(0, -0.25 * paperscale), vecs.GetVector(dxxPointFilters.AtMaxY).Moved(0, +0.25 * paperscale), aLineType: dxfLinetypes.Center);
                            clines = draw.aCenterlines(Holes, 0.0625 * paperscale, aSuppressVertical: true);

                            vecs = dxfEntities.DefiningVectors(clines);
                            v3 = vecs.GetVector(dxxPointFilters.GetRightBottom).Clone();
                            dim = dims.Vertical(v3, v2, 0.45, aTextOffset: -0.125, aSuffix: TYP);

                        }

                        dim = dims.Vertical(v2, v1, dim.BoundingRectangle().Right + 0.25 * paperscale, bAbsolutePlacement: true);  //overall width


                        Tool.CreateHoleLeader(Image, aHole, pRec.TopRight.Moved(0, 0.5 * paperscale), dims, Holes.Count);

                        vecs = cl1.EndPoints(true);

                        dim = dims.Horizontal(vecs.GetVector(dxxPointFilters.AtMinY), v2, -0.25);

                        vecs = Perim.Vertices.GetByTag("TAB_END");
                        if (vecs.Count > 0)
                        {
                            aRec = Image.SelectionSet().BoundingRectangle();

                            vecs.Sort(aOrder: dxxSortOrders.TopToBottom);
                            dims.Vertical(vecs.Item(2), vecs.Item(1), aRec.Left - 0.25 * paperscale, bAbsolutePlacement: true);
                            //dims.Vertical(vecs.Item(2), vecs.Item(3), aRec.Left - 0.5 * paperscale, bAbsolutePlacement: true);
                            dims.Vertical(vecs.Item(3), vecs.Item(4), aRec.Left - 0.25 * paperscale, bAbsolutePlacement: true);

                        }
                    }

                    aRec = Image.SelectionSet().BoundingRectangle();
                    v1 = aRec.MiddleLeft().Moved(-0.25 * paperscale);
                    if (!zPan) Image.SymbolTool.ViewArrow(v1, aPan.Width + 0.25 * paperscale, aLegLength: 0.375, aLabel: "A", aAngle: 0, aArrowStyle: dxxArrowStyles.AngledFull, aName: "VIEW_ARROW_A");

                    xCancelCheck();

                    //================ STAMP PN LEADER =====================================

                    v1 = pRec.TopCenter.Moved(0, -0.125 * paperscale);
                    v2 = new dxfVector(v1.X - 0.5 * paperscale, aRec.Top + 0.25 * paperscale);
                    draw.aLeader.Text(v1, v2, Tool.StampPNLeader(Image), null);
                    draw.aText(v1, "XXX", 0.5, dxxMTextAlignments.TopCenter, aTextStyle: "Standard");
                }
                if (iElev != null)
                {
                    v1 = new dxfVector(aRec.Right + 1.0 * paperscale, pRec.Top);
                    v2 = eRec.TopLeft;
                    v3 = v1 - v2;
                    iElev.Translate(v3);
                    eRec.Translate(v3);
                    Elev.Translate(v3);
                }
                if (DrawlayoutRectangles && appApplication.User.NetworkName == "E342367")
                {
                    //draw.aRectangle(fitRec, aColor: dxxColors.LightBlue);
                    draw.aRectangle(aRec, aColor: dxxColors.Red);
                    draw.aRectangle(prRec, aColor: dxxColors.Green);

                    int j = 0;
                    foreach (var item in Elev.Vertices)
                    {
                        j++;
                        draw.aText(item, item.Index, aTextHeight: 0.05 * paperscale, aAlignment: dxxMTextAlignments.MiddleCenter);
                        if (j == 16) break;
                    }
                }


                //===================== PROFILE ========================
                Image.SelectionSetInit(false);
                {
                    v1 = Profile.AdditionalSegments.GetByEntityType(dxxEntityTypes.Point).Item(1).HandlePt.Clone();
                    cl1 = draw.aLine(v1.Moved(0, 3 * thk), v1.Moved(0, -3 * thk), aLineType: dxfLinetypes.Center);
                    v1 = Profile.GetVertex(dxxPointFilters.GetRightTop, bReturnClone: true);
                    v2 = v1.Moved(-aPan.FlangeLength);
                    dims.Horizontal(v2, v1, 0.125 * paperscale);
                    //dims.Horizontal(cl1.StartPt, v1, 0.25 * paperscale);
                    dims.Vertical(v1, Profile.GetVertex(dxxPointFilters.GetBottomRight), aPan.FlangeLength + 0.0625 * paperscale);
                    if (!aPan.OpenEnded)
                    {
                        v1 = Profile.GetVertex(dxxPointFilters.GetLeftTop, bReturnClone: true);
                        v2 = v1.Moved(aPan.TabLength);
                        dims.Horizontal(v2, v1, 0.125 * paperscale);


                        dims.Vertical(Profile.GetVertex(dxxPointFilters.GetBottomLeft), v1, -0.25 * paperscale);

                    }
                    dims.Horizontal(Profile.GetVertex(dxxPointFilters.GetBottomRight), Profile.GetVertex(dxxPointFilters.GetBottomLeft), -0.125 * paperscale);

                    xCancelCheck();
                    //================ PERF LEADER =====================================
                    aRec = Image.SelectionSetInit(true).BoundingRectangle();
                    v1 = prRec.BottomCenter.Moved(0.25 * aPan.Length, thk);
                    v2 = new dxfVector(v1.X - 0.5 * paperscale, aRec.Top + 2.5 * 0.125 * paperscale);
                    draw.aLeader.Text(v1, v2, $"{aPan.PercentOpen}% OPEN AREA (PERFORATED)\\P%%C{Image.DimStyle().FormatNumber(aPan.PerforationDiameter, aPrecision: Drawing.DrawingUnits == uppUnitFamilies.Metric ? 0 : 3)} HOLES (BOTTOM SURFACE ONLY");
                }


                if (!zPan)
                {
                    ////===================== FLANGE END  ========================
                    Image.SelectionSetInit(false);
                    colDXFEntities clines = new();
                    vecs = Elev.AdditionalSegments.Centers(dxxEntityTypes.Point, true, aSearchTag: "TAB_CENTER");
                    if (vecs.Count > 0)
                    {
                        foreach (var item in vecs)
                        {
                            clines.Add(draw.aLine(item.Moved(aYChange: 0.125 * paperscale), item.Moved(aYChange: -0.125 * paperscale), aLineType: dxfLinetypes.Center));
                        }
                        cl1 = (dxeLine)clines.Item(2);
                        v1 = Elev.GetVertex(dxxPointFilters.GetLeftTop, bReturnClone: true);
                        dims.Horizontal(cl1.StartPt, v1, cl1.StartPt.Y + 0.125 * paperscale, bAbsolutePlacement: true, aSuffix: TYP);
                        dims.Horizontal(cl1.StartPt, clines.Item(1).DefinitionPoint(dxxEntDefPointTypes.StartPt), cl1.StartPt.Y + 0.35 * paperscale, bAbsolutePlacement: true, aSuffix: " REF");
                        v2 = Elev.GetVertex(dxxPointFilters.GetTopRight, bReturnClone: true);
                        v1 = Elev.GetVertex(dxxPointFilters.GetRightTop, bReturnClone: true);
                        dims.Vertical(v1, v2, v1.X + 0.375 * paperscale, bAbsolutePlacement: true);
                        v2 = Elev.GetVertex(dxxPointFilters.GetBottomRight, bReturnClone: true);

                        dims.Vertical(v1, v2, v1.X + 0.375 * paperscale, bAbsolutePlacement: true);

                        if (DrawlayoutRectangles && appApplication.User.NetworkName == "E342367")
                        {
                            dims.Vertical(Elev.GetVertex(dxxPointFilters.GetTopLeft), Elev.GetVertex(dxxPointFilters.GetBottomLeft), -0.125 * paperscale);
                        }
                        aRec = Image.SelectionSetInit(true).BoundingRectangle();
                        v1 = new dxfVector(eRec.X, aRec.BottomLeft.Y - 0.5 * paperscale);

                        Image.Draw.aText(v1, @"\LVIEW A-A", 0.1875 * paperscale, dxxMTextAlignments.TopCenter, aTextStyle: "Standard");
                    }
                }

                colDXFEntities ents = Image.Entities.SubSet(startentidx, Image.Entities.Count);
                aRec = ents.BoundingRectangle(dxfPlane.World);
                if (aRec.Left < fitRec.Left + 0.75 * paperscale)
                    ents.Move(fitRec.Left + 0.75 * paperscale - aRec.Left);

                //================== NOTES ==================================
                xCancelCheck();
                Tool.AddBorderNotes(aPan, Project, "ANTI-PENATRATION PAN", false, Image, fitRec, paperscale);

            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
            finally
            {
                Profile?.Dispose();
                Perim?.Dispose();
                Elev?.Dispose();

                Status = "";
                DrawinginProgress = false;
            }
        }

        private void DDWG_AssemblyDetails()
        {
            // All codes inside this method is commented out.
        }

        private void DDWG_FreeBubblingAreas()
        {

            try
            {
                //Assy.Invalidate(uppPartTypes.DeckSection);

                List<dxfBlock> blocks = Tool.DDWG_FreeBubblingAreas(this, DrawlayoutRectangles);

                Status = "Drawing Deck Section Simple Perimeters";
                dxoDrawingTool draw = Image.Draw;

                List<uopSectionShape> shapes = Assy.DeckSections.BaseShapes();

                mdDeckSections decksections = Assy.DeckSections;

                foreach(var shape in shapes)
                {
                    dxePolyline simple = new dxePolyline(shape.SimplePerimeter.Vertices,true) { LayerName = "DECK_SECTIONS"};
                    //simple.Move(-shape.X, -shape.Y);
                    uopInstances insts = shape.Instances;
                    simple.LCLSet("DECK_SECTIONS", dxxColors.LightGrey);
              //      simple.Instances = insts.ToDXFInstances();
                    dxfBlock blok = new dxfBlock($"DECK SECTION_{shape.Handle.Replace(",", "_")} SIMPLE _PERIMETER") { LayerName = "DECK_SECTIONS", Instances = insts.ToDXFInstances() };
                    simple.Move(-shape.X, -shape.Y);
                    blok.Entities.Add(simple);
                    draw.aInserts(blok, blok.Instances, false);

                    //draw.aInserts(simple, null, aBlockName: $"DECK SECTION_{shape.Handle.Replace(",", "_")}_SIMPLE _PERIMETER", aDisplaySettings: dxfDisplaySettings.Null("DECK_SECTIONS"));

                    //simple.Instances.Copy(shape.Instances.ToDXFInstances());


                    //Image.Entities.Add(simple);
                    
                    if (uopUtils.RunningInIDE )
                    {

                        //uopSectionShapes.SetMDMechanicalProperties(shape, Assy, false, null);

                        if (shape.GetSplices(out uopDeckSplice top, out uopDeckSplice bot))
                        {
                            if (top != null && top.SpliceType == uppSpliceTypes.SpliceWithAngle && shape.Handle == "3,2")
                            {

                                //uopShapes bas = top.BlockedAreas(shape);
                                //draw.aShapes(bas, dxfDisplaySettings.Null("TEST", dxxColors.DarkGreen));
                                //uopFlangeLine flngl = new uopFlangeLine(top, bOppositeGender: false);
                                //draw.aLine(flngl, dxfDisplaySettings.Null("FLANGE_LINES_TOP", dxxColors.LightCyan));
                                //draw.aLine(top.BaseLine(shape, out _), dxfDisplaySettings.Null("BASELINES_TOP", dxxColors.LightCyan));
                                //draw.aCircles(flngl.Points, 0.0625, dxfDisplaySettings.Null("BASELINES_TOP", dxxColors.LightCyan));
                            }
                            if (bot != null && bot.SpliceType == uppSpliceTypes.SpliceWithAngle && shape.Handle == "3,1")
                            {
                                //uopShapes bas = bot.BlockedAreas(shape);
                                //draw.aShapes(bas, dxfDisplaySettings.Null("TEST", dxxColors.DarkGreen));
                                //    uopFlangeLine flngl = new uopFlangeLine(bot, bOppositeGender: false);
                                //    draw.aLine(flngl, dxfDisplaySettings.Null("FLANGE_LINES_BOT", dxxColors.LightGreen));
                                //    draw.aLine(bot.BaseLine(shape, out _), dxfDisplaySettings.Null("BASELINES_BOT", dxxColors.LightGreen));
                            }
                        }



                    }
                }
            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = string.Empty; DrawinginProgress = false; }


        }


        private void DDWG_DowncomerDesign()
        {
            
     

            dxePolygon aPG;
            dxePolygon bPG;
            dxePolygon tPG;
            dxePolygon ePG;
            dxfVector v1 = dxfVector.Zero;
            dxfVector v2;
            dxfVector v3;
            dxfRectangle aRec = new();
            dxfRectangle tRec;
            dxfRectangle bRec;
            double paperscale = 1;
            double oset;
            double d3 = 0;
            double d4 = 0;
            dxfBlock blok = null;
            uopVectors ips = null;
            try
            {
                
                bool bNoEndview = Drawing.TrayQuery.AnswerB("Suppress End View?", false);
                bool bCenterOnBox = Drawing.TrayQuery.AnswerB("Center At Box End?", false);

                mdDowncomerBox aBox = (mdDowncomerBox) Drawing.Part;

            if (aBox == null) return;

                string pn = aBox.PartNumber;
                dxoDimTool dims = Image.DimTool;
                dxoDrawingTool draw = Image.Draw;

                mdDowncomer downcomer = aBox.Downcomer;
                Status = $"Creating DC {pn} Plan View Geometry";
                try
                {
                    //Tool.SetDrawProperties(Image, Drawing);

                    double d1 = aBox.Width;
                    aRec.Width = aBox.Downcomer.AssemblyLength();
                    double d2 = d1 / 2 + 1 + d1 + aBox.Height;
                    if (Assy.DesignFamily.IsEcmdDesignFamily())
                    {
                        d3 = Assy.DesignOptions.CDP;
                        d3 = (d3 > 0) ? d3 - aBox.How - aBox.DeckThickness : 0;
                    }
                    if (Assy.DesignOptions.HasAntiPenetrationPans) d4 = 2 * Assy.APPanHeight;


                    aRec.Height = 2 * d2 + d3 + d4;
                    if (bCenterOnBox)
                    {
                        v1.Move(0.5 * aBox.Downcomer.BoxLength(true));
                        aRec.Move(v1.X);
                    }

                    Image.Display.SetDisplayRectangle(aRec, 1.02, bSetFeatureScales: true, bNoRedraw: true);
                    Drawing.ZoomExtents = true;
                    paperscale = Image.Display.PaperScale;
                    xCancelCheck();

                    dxfBlock block1 = Image.Blocks.Add(mdBlocks.DowncomerBox_View_Plan(Image, aBox, Assy, false, bSuppressHoles: false, bIncludeSpouts: true, bShowObscured: false, bIncludeFingerClips:true));
                    draw.aInsert(block1.Name, v1, 90);

                    ////=============== FINGER CLIPS
                    //blok = mdBlocks.FingerClip_View_Plan(Image, Assy.FingerClip, aLayerName: "FINGER CLIPS");

                    //ips = aBox.FingerClipPoints(Assy, null);
                    //ips.Move(-aBox.X + v1.X, -aBox.Y + v1.Y);
                    //ips.Rotate(v1, 90);
                    //dxfDisplaySettings dsp = dxfDisplaySettings.Null(blok.LayerName);

                    //foreach (var ip in ips)
                    //{
                    //    draw.aInsert(blok.Name, ip, ip.Y > v1.Y ? -90 : 90, aDisplaySettings: dsp);
                    //}


                }
                catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
 

                //mdStiffener stf = new mdStiffener(aBox, 0);
                //dsp = dxfDisplaySettings.Null("STIFFENERS");
                //blok = mdBlocks.Stiffener_View_Plan(Image,Assy, aLayerName: dsp.LayerName ,aBox: aBox);
                //if(blok != null)
                //{
                //    List<double> xvals = aBox.GetStiffenerOrdinates(true);
                //    foreach (var item in xvals)
                //    {
                //        uopVector ip = new uopVector(v1.X + item, v1.Y);
                //        draw.aInsert(blok.Name, ip, 90, aDisplaySettings: dsp);
                //    }
                //}





                //aPG = aBox.View_Plan(Assy, aCenter: v1, aRotation: 90, bIncludeBoltOns: true, aCenterLineLength: 0.75, bOneLayer: false);
                //xCancelCheck();

                //Image.LinetypeLayers.ApplyTo(aPG, dxxLinetypeLayerFlag.ForceToColor);
                //Status = $"Drawing DC {aBox.Index} Plan View Geometry";
                //Image.Entities.Add(aPG);

                //oset = 1.5 * d1 + d3;
                //xCancelCheck();


                Status = $"Creating DC {aBox.Index} Long Side View Geometry";


                //bPG = aBox.View_Profile(Assy, bLongSide: true, bIncludeCrossBraces: false, bIncludeBoltOns: true, aCenter: v1, aRotation: 0, bOneLayer: false);
                //bRec = bPG.BoundingRectangle(dxfPlane.World);
                //d2 = -(0.5 * aBox.Width + 1 + (1 * paperscale) + 0.5 * bRec.Height);
                //bPG.Move(aYChange: d2);
                //bRec.Move(ChangeY: d2);

                //xCancelCheck();
                //Image.LinetypeLayers.ApplyTo(bPG, dxxLinetypeLayerFlag.ForceToColor);
                //Status = $"Drawing DC {aBox.Index} Long Side View Geometry";

                //Image.Entities.Add(bPG);
                //xCancelCheck();

                //draw.aPoints(bPG.AdditionalSegments.GetByTag("STARTUP").Centers(), "GEOMETRY", dxxColors.Grey);

                //Status = $"Creating DC {aBox.Index} Short Side View Geometry";

                //tPG = aBox.View_Profile(Assy, bLongSide: false, bIncludeCrossBraces: false, bIncludeBoltOns: true, aCenter: v1, aRotation: 0, bOneLayer: false);
                //tRec = tPG.BoundingRectangle(dxfPlane.World);
                //d2 = 0.5 * aBox.Width + 1 + (1 * paperscale) + 0.5 * tRec.Height;
                //tPG.Move(aYChange: d2);


                //xCancelCheck();
                //Image.LinetypeLayers.ApplyTo(tPG, dxxLinetypeLayerFlag.ForceToColor);
                //Status = $"Drawing DC {aBox.Index} Short Side View Geometry";
                //colDXFEntities pgents = tPG.SubEntities();
                //Image.Entities.Append(pgents);
                //tRec = pgents.BoundingRectangle(dxfPlane.World);
                //xCancelCheck();

                //draw.aPoints(tPG.AdditionalSegments.GetByTag("STARTUP").Centers(), "GEOMETRY", dxxColors.Grey);

                //Status = $"Creating DC {aBox.Index} Elevation View Geometry";
                //xCancelCheck();
                //v1 = aPG.BoundingRectangle(dxfPlane.World).MiddleRight();
                //v1.Move(1 * paperscale + aBox.How);
                //if (Assy.DesignFamily.IsEcmdDesignFamily() && Assy.DesignOptions.CDP > 0)
                //{
                //    v1.Move(Assy.DesignOptions.CDP - aBox.How);
                //}

                //if (!bNoEndview)
                //{
                //    xCancelCheck();
                //    ePG = aBox.View_Elevation(Assy, true, true, v1, 90, bOneLayer: false);
                //    Image.LinetypeLayers.ApplyTo(ePG, dxxLinetypeLayerFlag.ForceToColor);
                //    Status = $"Drawing DC {aBox.Index} Elevation View Geometry";
                //    Image.Entities.Add(ePG);
                //    xCancelCheck();
                //}

                //xCancelCheck();
                //Image.Display.ZoomExtents(bSetFeatureScale: true);
                //paperscale = Image.Display.PaperScale;

                //v1 = bRec.MiddleLeft();
                //v2 = bRec.MiddleRight();

                //v1.Y = bPG.Y;
                //v2.Y = v1.Y;

                //v3 = bRec.BottomCenter.Moved(aYChange: -0.35 * Image.Display.PaperScale);

                //dims.Horizontal(v1, v2, v3.Y, bAbsolutePlacement: true);

                //v1 = tRec.MiddleLeft();
                //v2 = tRec.MiddleRight();

                //v1.Y = tPG.Y;
                //v2.Y = v1.Y;

                //v3 = tRec.TopCenter.Moved(aYChange: 0.35 * Image.Display.PaperScale);

                //dims.Horizontal(v1, v2, v3.Y, bAbsolutePlacement: true);

                //if (DrawlayoutRectangles)
                //{
                //    draw.aPolyline(tRec);
                //}

                aRec = Image.Entities.BoundingRectangle(dxfPlane.World);
                v1 = aRec.BottomCenter.Moved(aYChange: -0.3 * paperscale);
                draw.aText(v1, $"{Assy.TrayName(true)}\\P Downcomer {aBox.PartNumber}", 0.25 * paperscale, dxxMTextAlignments.TopCenter, aTextStyle: "Standard");

                Image.Display.ZoomExtents();
                Drawing.ZoomExtents = false;
            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
        }


        private void DDWG_DowncomerDesign(mdDowncomer aDowncomer)
        {
            if (aDowncomer == null) return;


            //dxePolygon aPG;
            //dxePolygon bPG;
            //dxePolygon tPG;
            //dxePolygon ePG;
            //dxfVector v1 = dxfVector.Zero;
            //dxfVector v2;
            //dxfVector v3;
            //dxfRectangle aRec = new();
            //dxfRectangle tRec;
            //dxfRectangle bRec;
            //double xscal;
            //double oset;
            //double d3 = 0;
            //double d4 = 0;

            //try
            //{
            //    dxoDimTool dims = Image.DimTool;
            //    dxoDrawingTool draw = Image.Draw;

            //    bool bNoEndview = (bool)Drawing.Options.Answer(1, false);
            //    bool bCenterOnBox = (bool)Drawing.Options.Answer(2, false);

            //    Status = $"Creating DC {aDowncomer.Index} Plan View Geometry";

            //    //Tool.SetDrawProperties(Image, Drawing);

            //    double d1 = aDowncomer.BoxWidth;
            //    aRec.Width = aDowncomer.AssemblyLength();
            //    double d2 = d1 / 2 + 1 + d1 + aDowncomer.OutsideHeight;
            //    if (Assy.DesignFamily.IsEcmdDesignFamily())
            //    {
            //        d3 = Assy.DesignOptions.CDP;
            //        d3 = (d3 > 0) ? d3 - aDowncomer.How - aDowncomer.DeckThickness : 0;
            //    }
            //    if (Assy.DesignOptions.HasAntiPenetrationPans) d4 = 2 * Assy.APPan.Height;


            //    aRec.Height = 2 * d2 + d3 + d4;
            //    if (bCenterOnBox)
            //    {
            //        v1.Move(0.5 * aDowncomer.BoxLength(true));
            //        aRec.Move(v1.X);
            //    }

            //    Image.Display.SetDisplayRectangle(aRec, 1.02, bSetFeatureScales: true, bNoRedraw: true);
            //    Drawing.ZoomExtents = true;
            //    xscal = Image.Display.PaperScale;
            //    xCancelCheck();

            //    aPG = aDowncomer.View_Plan(Assy, aCenter: v1, aRotation: 90, bObscuredShelfs: false, bIncludeBoltOns: true, aCenterLineLength: 0.75, bOneLayer: false);
            //    xCancelCheck();

            //    Image.LinetypeLayers.ApplyTo(aPG, dxxLinetypeLayerFlag.ForceToColor);
            //    Status = $"Drawing DC {aDowncomer.Index} Plan View Geometry";
            //    Image.Entities.Add(aPG);

            //    oset = 1.5 * d1 + d3;
            //    xCancelCheck();


            //    Status = $"Creating DC {aDowncomer.Index} Long Side View Geometry";


            //    bPG = aDowncomer.View_Profile(Assy, bLongSide: true, bIncludeCrossBraces: false, bIncludeBoltOns: true, aCenter: v1, aRotation: 0, bOneLayer: false);
            //    bRec = bPG.BoundingRectangle(dxfPlane.World);
            //    d2 = -(0.5 * aDowncomer.BoxWidth + 1 + (1 * xscal) + 0.5 * bRec.Height);
            //    bPG.Move(aYChange: d2);
            //    bRec.Move(ChangeY: d2);

            //    xCancelCheck();
            //    Image.LinetypeLayers.ApplyTo(bPG, dxxLinetypeLayerFlag.ForceToColor);
            //    Status = $"Drawing DC {aDowncomer.Index} Long Side View Geometry";

            //    Image.Entities.Add(bPG);
            //    xCancelCheck();

            //    draw.aPoints(bPG.AdditionalSegments.GetByTag("STARTUP").Centers(), "GEOMETRY", dxxColors.Grey);

            //    Status = $"Creating DC {aDowncomer.Index} Short Side View Geometry";

            //    tPG = aDowncomer.View_Profile(Assy, bLongSide: false, bIncludeCrossBraces: false, bIncludeBoltOns: true, aCenter: v1, aRotation: 0, bOneLayer: false);
            //    tRec = tPG.BoundingRectangle(dxfPlane.World);
            //    d2 = 0.5 * aDowncomer.BoxWidth + 1 + (1 * xscal) + 0.5 * tRec.Height;
            //    tPG.Move(aYChange: d2);


            //    xCancelCheck();
            //    Image.LinetypeLayers.ApplyTo(tPG, dxxLinetypeLayerFlag.ForceToColor);
            //    Status = $"Drawing DC {aDowncomer.Index} Short Side View Geometry";
            //    colDXFEntities pgents = tPG.SubEntities();
            //    Image.Entities.Append(pgents);
            //    tRec = pgents.BoundingRectangle(dxfPlane.World);
            //    xCancelCheck();

            //    draw.aPoints(tPG.AdditionalSegments.GetByTag("STARTUP").Centers(), "GEOMETRY", dxxColors.Grey);

            //    Status = $"Creating DC {aDowncomer.Index} Elevation View Geometry";
            //    xCancelCheck();
            //    v1 = aPG.BoundingRectangle(dxfPlane.World).MiddleRight();
            //    v1.Move(1 * xscal + aDowncomer.How);
            //    if (Assy.DesignFamily.IsEcmdDesignFamily() && Assy.DesignOptions.CDP > 0)
            //    {
            //        v1.Move(Assy.DesignOptions.CDP - aDowncomer.How);
            //    }

            //    if (!bNoEndview)
            //    {
            //        xCancelCheck();
            //        ePG = aDowncomer.View_Elevation(Assy, true, true, v1, 90, bOneLayer: false);
            //        Image.LinetypeLayers.ApplyTo(ePG, dxxLinetypeLayerFlag.ForceToColor);
            //        Status = $"Drawing DC {aDowncomer.Index} Elevation View Geometry";
            //        Image.Entities.Add(ePG);
            //        xCancelCheck();
            //    }

            //    xCancelCheck();
            //    Image.Display.ZoomExtents(bSetFeatureScale: true);
            //    xscal = Image.Display.PaperScale;

            //    v1 = bRec.MiddleLeft();
            //    v2 = bRec.MiddleRight();

            //    v1.Y = bPG.Y;
            //    v2.Y = v1.Y;

            //    v3 = bRec.BottomCenter.Moved(aYChange: -0.35 * Image.Display.PaperScale);

            //    dims.Horizontal(v1, v2, v3.Y, bAbsolutePlacement: true);

            //    v1 = tRec.MiddleLeft();
            //    v2 = tRec.MiddleRight();

            //    v1.Y = tPG.Y;
            //    v2.Y = v1.Y;

            //    v3 = tRec.TopCenter.Moved(aYChange: 0.35 * Image.Display.PaperScale);

            //    dims.Horizontal(v1, v2, v3.Y, bAbsolutePlacement: true);

            //    if (DrawlayoutRectangles)
            //    {
            //        draw.aPolyline(tRec);
            //    }

            //    aRec = Image.Entities.BoundingRectangle(dxfPlane.World);
            //    v1 = aRec.BottomCenter.Moved(aYChange: -0.3 * xscal);
            //    draw.aText(v1, $"{Assy.TrayName(true)}\\P Downcomer {aDowncomer.PartNumber}", 0.25 * xscal, dxxMTextAlignments.TopCenter, aTextStyle: "Standard");

            //    Image.Display.ZoomExtents();
            //    Drawing.ZoomExtents = false;
            //}
            //catch (Exception e)
            //{
            //    HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            //}
        }

        private void DDWG_DowncomersOnly()
        {
          

            try
            {
                mzQuestions options = Drawing.Options;
                bool bothSide = options.AnswerB("Draw Both Sides?", !Assy.IsSymmetric);
            
               
                Image.Display.SetDisplayRectangle(Assy.Rectangle(), 1.1, true);
                Drawing.ZoomExtents = false;

                dxoDimTool dims = Image.DimTool;

                Draw_Shells(uppViews.LayoutPlan, out dxeLine vcl, out dxeLine hcl);

                Draw_Rings(uppViews.DesignView);
                //  bool inc_endplates = true ;
                bool inc_Spouts = !options.AnswerB("Suppress Spouts?", false);
                bool inc_Baffles = options.AnswerB("Draw Deflector Plates?", true) && Assy.ProjectType != uppProjectTypes.MDSpout && Assy.IsECMD;
                bool inc_APPans = options.AnswerB("Draw AP Pans?", true) && Assy.ProjectType != uppProjectTypes.MDSpout && Assy.HasAntiPenetrationPans;

                bool inc_Stiffeners = !options.AnswerB("Suppress Stiffeners?", true) && Assy.ProjectType != uppProjectTypes.MDSpout;
                bool inc_FingerClips = options.AnswerB("Draw Finger Clips?", true) && Assy.ProjectType != uppProjectTypes.MDSpout;
                bool inc_EndAngles = options.AnswerB("Draw End Angles?", true) && Assy.ProjectType != uppProjectTypes.MDSpout;
                bool inc_endsupports = !options.AnswerB("Suppress End Supports?", false);
                bool inc_shelves = !options.AnswerB("Suppress Deck Support Angles?", false);
                bool inc_Endplates = !options.AnswerB("Suppress End Plates?", false);
                bool inc_SupDefs = !options.AnswerB("Suppress Supplemental Deflectors?",false) && Assy.ProjectType != uppProjectTypes.MDSpout;
                int entid = Image.Entities.Count + 1;
                Draw_Downcomers(uppViews.DesignView, out colDXFEntities clines, bDrawSpouts: inc_Spouts, bDrawBothSides: bothSide, bDrawStartupCenterLines: true, bDrawFingerClips: inc_FingerClips, bDrawStiffeners: inc_Stiffeners,bDrawEndAngles: inc_EndAngles,bDrawBaffles: inc_Baffles, bSuppressEndSupports:!inc_endsupports, bSuppressShelves:!inc_shelves, bSuppressEndplates: !inc_Endplates, bSuppresSupDefs: !inc_SupDefs);

                //if (inc_Spouts) Draw_SpoutGroupBlocks(bDrawBothSides: bothSide);



                if (bothSide) clines.RemoveMembers(clines.FindAll(x => x.X < 0));
                Image.Display.ZoomExtents(bSetFeatureScale: true);
                double paperscale = Image.Display.PaperScale;
                double y = vcl.EndPoints().GetOrdinate(dxxOrdinateTypes.MinY) - 0.25 * paperscale;
                int dcnt = Assy.OddDowncomers ? 1 : 2;
                int cnt = 0;
                for (int i = clines.Count; i >= 1; i--)
                {
                    dxeLine cl = clines.Item(i) as dxeLine;
                    dims.Horizontal(cl.EndPoints().GetVector(dxxPointFilters.AtMinY), vcl.EndPoints().GetVector(dxxPointFilters.AtMinY), y, bAbsolutePlacement: true);
                    vcl = cl;
                    cnt++;
                    if (cnt == dcnt) break;
                    
                }

                if (Assy.HasAntiPenetrationPans)
                {
                    Draw_APPans(uppViews.LayoutPlan, bObscured: true, bDrawBothSides: bothSide);
                }

                Image.Display.ZoomExtents(bSetFeatureScale: true);
                paperscale = Image.Display.PaperScale;
                Image.Draw.aText(Image.Display.ExtentRectangle.BottomCenter.Moved(aYChange: -0.5 * paperscale), $"{MDRange.TrayName(true).ToUpper()} DOWNCOMERS", aAlignment: dxxMTextAlignments.TopCenter);

                //Draw_Washers();

                //Draw_CrossBraces(uppViews.LayoutPlan, true);
                if (inc_APPans) Draw_APPans(uppViews.Plan);

                DrawinginProgress = false;
            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
        }

        private void DDWG_ElevationView()
        {
            if (Assy == null) return;


            try
            {
                if (DrawlayoutRectangles)
                {
                    //Assy.Invalidate(uppPartTypes.APPan);
                }

                Image.Display.SetDisplayWindow(2.2 * ShellRad, Image.UCS.Origin, 2.5 * MDRange.RingSpacing, true);

                Draw_View_Elevation(uppViews.InstallationElevation, out dxeLine _, bPhantomView: Drawing.Options.AnswerB(1,false));

                Image.Display.ZoomExtents();

                DrawinginProgress = false;
            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
        }

        private void DDWG_EndAngles_OLD()
        {
            dxfVector v1;
            dxfVector v2;
            dxfVector v3;
            dxePolyline aPl;
            dxfRectangle aRect = new();
            dxfRectangle fitRec = new();
            dxeArc aA;
            dxoDimStyle DStyle;

            dxeLine l1;
            dxeLine l2;
            dxeLine l3;
            colUOPParts EAs = new();
            mdEndAngle aEA = null;
            double ang;
            int si = 0;
            int ei = 0;
            dxoDrawingTool draw = Image.Draw;
            dxoDimTool dim = Image.DimTool;
            dxoLeaderTool leader = Image.LeaderTool;
            string txt;
            try
            {
                bool projWide = Drawing.ProjectWide;
                double thk = 0.125;
                double lng = 5;
                double rad = mdGlobals.gsBigHole / 2;
                int t = 0;


                //================== Border File ==================================
                double paperscale = 1;
                Tool.Border(this, null, ref fitRec, arPaperScale: ref paperscale, bSuppressAnsiScales: true, aScaleString: "NTS");
                xCancelCheck();
                Drawing.ZoomExtents = false;
                //================== Border File ==================================
                if (MDProject == null) return;

                if (projWide)
                {
                    si = MDProject.TrayRanges.LastTray().RingStart;
                    ei = MDProject.TrayRanges.FirstTray().RingEnd;
                    mzUtils.SortTwoValues(true, ref si, ref ei);
                }

                DStyle = Image.DimStyle();
                Image.TableSettings.ColumnGap = 0.1;

                for (int i = 1; i <= 2; i++)
                {
                    Image.SelectionSetInit(false);
                    if (i == 1)
                    {
                        v1 = fitRec.TopLeft.Moved(2.85, -0.25); // it is not assigned any value before use in VB code so I just instatiated it
                        ang = 45;
                    }
                    else
                    {
                        v1 = fitRec.TopLeft.Moved(2.85, -4.5);
                        v1.Y = aRect.Bottom - 0.5; // it is not assigned any value before use in VB code so I just instatiated it
                        ang = 0;
                    }

                    aPl = (dxePolyline)Image.Primatives.Angle_Top(v1, 1, lng, thk, 90, ang);
                    aPl.LCLSet("GEOMETRY", dxxColors.ByLayer, dxfLinetypes.Continuous);
                    Image.SaveEntity(aPl);
                    v3 = aPl.GetVertex(dxxPointFilters.GetTopLeft, bReturnClone: true);
                    v1 = v3.Moved(1);
                    draw.aLine(v1.X - rad, v1.Y, v1.X - rad, v1.Y - thk, aLineType: dxfLinetypes.Hidden);
                    draw.aLine(v1.X + rad, v1.Y, v1.X + rad, v1.Y - thk, aLineType: dxfLinetypes.Hidden);
                    draw.aLine(v1.X, v1.Y + thk, v1.X, v1.Y - thk - thk, aLineType: dxfLinetypes.Center);

                    v2 = aPl.GetVertex(dxxPointFilters.GetTopRight, bReturnClone: true);
                    v1 = v2.Moved(-1);
                    draw.aLine(v1.X - rad, v1.Y, v1.X - rad, v1.Y - thk, aLineType: dxfLinetypes.Hidden);
                    draw.aLine(v1.X + rad, v1.Y, v1.X + rad, v1.Y - thk, aLineType: dxfLinetypes.Hidden);
                    draw.aLine(v1.X, v1.Y + thk, v1.X, v1.Y - thk - thk, aLineType: dxfLinetypes.Center);

                    dim.Vertical(aPl.GetVertex(dxxPointFilters.GetBottomRight), v2, v2.X + 0.375 * paperscale, bAbsolutePlacement: true);

                    if (i == 1)
                    {
                        v3.Move(aYChange: -thk);
                        v1 = v3.PolarVector(270, 0.1);
                        v2 = v3.PolarVector(270 + 45, 0.1);

                        Image.DimStyleOverrides.SuppressExtLine2 = true;
                        Image.DimStyle().TextFit = dxxDimTextFitTypes.MoveArrowsFirst;
                        dim.Angular3P(v3, v1, v2, 0.825, aImage: _Image);
                        Image.DimStyleOverrides.SuppressExtLine2 = false;

                    }

                    v1 = aPl.Center().Moved(aYChange: -2.5);
                    aPl = (dxePolyline)Image.Primatives.Angle_Top(v1, 1, lng, thk, -90);
                    aPl.LCLSet("GEOMETRY", dxxColors.ByLayer, dxfLinetypes.Continuous);
                    Image.Entities.Add(aPl);
                    dxfDisplaySettings dsp = aPl.DisplaySettings;
                    v1 = aPl.GetVertex(dxxPointFilters.GetBottomLeft).Moved(1, 0.625);
                    draw.aCircle(v1, rad, dsp);

                    draw.aLine(v1.X - (rad + 0.125), v1.Y, v1.X + (rad + 0.125), v1.Y, aLineType: dxfLinetypes.Center);
                    l1 = draw.aLine(v1.X, v1.Y - (rad + 0.125), v1.X, v1.Y + (rad + 0.125), aLineType: dxfLinetypes.Center);

                    v2 = aPl.GetVertex(dxxPointFilters.GetBottomRight, bReturnClone: true);
                    v1 = v2.Moved(-1, 0.625);
                    aA = draw.aCircle(v1, rad, dsp);

                    l2 = draw.aLine(v1.X - (rad + 0.125), v1.Y, v1.X + (rad + 0.125), v1.Y, aLineType: dxfLinetypes.Center);
                    l3 = draw.aLine(v1.X, v1.Y - (rad + 0.125), v1.X, v1.Y + (rad + 0.125), aLineType: dxfLinetypes.Center);

                    v3 = aPl.GetVertex(dxxPointFilters.GetTopRight, bReturnClone: true);
                    dim.Horizontal(l1.EndPt, l3.EndPt, v3.Y + 0.25, bAbsolutePlacement: true, aOverideText: "'B'");
                    dim.Horizontal(l3.EndPt, v3, v3.Y + 0.25, bAbsolutePlacement: true, aOverideText: "'C'");
                    dim.Horizontal(aPl.GetVertex(dxxPointFilters.GetTopLeft), v3, v3.Y + 0.625, bAbsolutePlacement: true, aOverideText: "'A'");

                    dim.Vertical(l2.EndPt, v2, v2.X + 0.375, bAbsolutePlacement: true);
                    dim.Vertical(v2, v3, v2.X + 0.8725, bAbsolutePlacement: true);

                    if (i == 1)
                    {
                        v1 = aPl.GetVertex(dxxPointFilters.GetBottomLeft, bReturnClone: true);
                        v1.Move(0.875);

                        draw.aLine(v1, 90, thk, aSegmentPtType: dxxSegmentPointTypes.StartPt, "GEOMETRY", dxxColors.Undefined, dxfLinetypes.Continuous);

                        v2.Move(-0.875);

                        
                        draw.aLine(v2, 90, thk, aSegmentPtType: dxxSegmentPointTypes.StartPt, "GEOMETRY", dxxColors.Undefined, dxfLinetypes.Continuous);
                    }

                    v1 = aA.AnglePoint(-45);
                    v2 = v1.Moved(0.5, -1);
                    Tool.CreateHoleLeader(Image, aA, v2, dim, 2, true, true);
                    //leader.Text(v1, v2, Tool.HoleLeader(Image, aA, 2), null); 

                    v1 = aPl.Center();
                    v1.Move(aYChange: thk / 2);

                    v1 = draw.aText(v1, "XXX", 0.5, dxxMTextAlignments.MiddleCenter, aTextStyle: "Standard").BoundingRectangle().BottomCenter;
                    v3 = v1.Moved(-0.5, -1);
                    v3.Y = v2.Y;
                    var ldr = leader.Text(v1, v3, @"\W0.7;" + Tool.StampPNLeader(Image), null);

                    aRect = Image.SelectionSetInit(true).BoundingRectangle();

                    if (i == 1)
                    {
                        v1 = aRect.BottomLeft.Moved(aYChange: -0.25);

                        // dividing line
                        aPl = new dxePolyline(new colDXFVectors(new dxfVector(fitRec.Left, v1.Y), new dxfVector(fitRec.Right, v1.Y)), false, aSegWidth: 0.02);
                        aPl.LCLSet("BORDER", aLineType: dxfLinetypes.Continuous);
                        Image.Entities.Add(aPl);
                    }

                    v1 = aRect.TopRight;
                    if (i == 1) v1.Y = fitRec.Top;
                    v1.Move(0.65, 0); //i == 1 ? -0.3 : 0);

                    //EAs = (!projWide) ? mdUtils.EndAngles_PROJ(MDProject, Assy.RangeGUID, bChamfered: i == 1)  : mdUtils.EndAngles_PROJ(MDProject,bChamfered: i == 1);



                    List<string> rows1 = new() { "PART NO.|# REQUIRED|'A'|'B'|'C'" };
                    List<string> rows2 = new() { "PART NO.|# REQUIRED|'A'|'B'|'C'" };

                    for (int j = 1; j <= EAs.Count; j++)
                    {
                        aEA = (mdEndAngle)EAs.Item(j);
                        string row = $"{aEA.PartNumber}|{aEA.SpareQuantity()}|{DStyle.FormatNumber(aEA.Length)}|{DStyle.FormatNumber(aEA.Length - 2 * aEA.HoleInset)}|{DStyle.FormatNumber(aEA.HoleInset)}";
                        if (j <= 15) { rows1.Add(row); } else { rows2.Add(row); }


                    }

                    dxeInsert table = null;

                    if (rows1.Count > 1)
                    {
                        t++;
                        table = draw.aTableBlk($"Table_{t}", v1, rows1, aGridStyle: dxxTableGridStyles.All, aFooter: rows2.Count <= 1 ? $"INCLUDES {aEA.SparePercentage}% SPARES" : ""); // in VB it was aEA.Part.SparePercentage

                    }
                    if (rows2.Count > 1)
                    {
                        t++;
                        v1.X = table.DefinitionPoint(dxxEntDefPointTypes.TopRight).X + 0.25 * paperscale;
                        table = draw.aTableBlk($"Table_{t}", v1, rows2, aGridStyle: dxxTableGridStyles.All, aFooter: $"INCLUDES {aEA.SparePercentage}% SPARES"); ; // in VB it was aEA.Part.SparePercentage

                    }
                }

                //================== NOTES ==================================

                Status = "Drawing Notes";
                txt = projWide ? $"FOR TRAYS {si}-{ei}" : $"PFOR TRAYS {Assy.SpanName()}";

                AppDrawingBorderData notes = new()
                {
                    PartNumber_Value = $"END ANGLE",
                    Location_Value = txt,
                };


                Tool.Add_B_BorderNotes(Image, notes, fitRec, paperscale);


                Status = "";
                DrawinginProgress = false;
            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
        }

        private void DDWG_EndAngles()
        {
            dxfVector v1;
            dxfVector v2;
            dxfVector v3;
            dxePolyline aPl;
            dxfRectangle aRect = new();
            dxfRectangle fitRec = new();
            dxeArc aA;
            dxoDimStyle DStyle;

            dxeLine l1;
            dxeLine l2;
            dxeLine l3;

            mdEndAngle aEA = null;
            double ang;
            int si = 0;
            int ei = 0;
            dxoDrawingTool draw = Image.Draw;
            dxoDimTool dim = Image.DimTool;
            dxoLeaderTool leader = Image.LeaderTool;

            try
            {

                double thk = 0.125;
                double lng = 5;
                double rad = mdGlobals.gsBigHole / 2;
                int t = 0;



                //================== Border File ==================================
                double paperscale = 1;
                Tool.Border(this, null, ref fitRec, arPaperScale: ref paperscale, bSuppressAnsiScales: true, aScaleString: "NTS");
                xCancelCheck();
                Drawing.ZoomExtents = false;
                //================== Border File ==================================
                if (MDProject == null) return;

                if (MDProject.ReverseSort)
                {
                    si = MDProject.TrayRanges.LastTray().RingStart;
                    ei = MDProject.TrayRanges.FirstTray().RingEnd;
                    mzUtils.SortTwoValues(true, ref si, ref ei);
                }
                else
                {
                    si = MDProject.TrayRanges.FirstTray().RingStart;
                    ei = MDProject.TrayRanges.LastTray().RingEnd;
                    mzUtils.SortTwoValues(true, ref si, ref ei);

                }

                DStyle = Image.DimStyle();
                Image.TableSettings.ColumnGap = 0.05;
                Image.TableSettings.RowGap = 0;
                for (int i = 1; i <= 2; i++)
                {
                    Image.SelectionSetInit(false);
                    if (i == 1)
                    {
                        v1 = fitRec.TopLeft.Moved(2.85, -0.25);
                        ang = 45;
                    }
                    else
                    {
                        v1 = fitRec.TopLeft.Moved(2.85, -4.5);
                        v1.Y = aRect.Bottom - 0.5;
                        ang = 0;
                    }

                    aPl = (dxePolyline)Image.Primatives.Angle_Top(v1, 1, lng, thk, 90, ang);
                    aPl.LCLSet("GEOMETRY", dxxColors.ByLayer, dxfLinetypes.Continuous);
                    Image.SaveEntity(aPl);
                    v3 = aPl.GetVertex(dxxPointFilters.GetTopLeft, bReturnClone: true);
                    v1 = v3.Moved(1);
                    draw.aLine(v1.X - rad, v1.Y, v1.X - rad, v1.Y - thk, aLineType: dxfLinetypes.Hidden);
                    draw.aLine(v1.X + rad, v1.Y, v1.X + rad, v1.Y - thk, aLineType: dxfLinetypes.Hidden);
                    draw.aLine(v1.X, v1.Y + thk, v1.X, v1.Y - thk - thk, aLineType: dxfLinetypes.Center);

                    v2 = aPl.GetVertex(dxxPointFilters.GetTopRight, bReturnClone: true);
                    v1 = v2.Moved(-1);
                    draw.aLine(v1.X - rad, v1.Y, v1.X - rad, v1.Y - thk, aLineType: dxfLinetypes.Hidden);
                    draw.aLine(v1.X + rad, v1.Y, v1.X + rad, v1.Y - thk, aLineType: dxfLinetypes.Hidden);
                    draw.aLine(v1.X, v1.Y + thk, v1.X, v1.Y - thk - thk, aLineType: dxfLinetypes.Center);

                    dim.Vertical(aPl.GetVertex(dxxPointFilters.GetBottomRight), v2, v2.X + 0.375 * paperscale, bAbsolutePlacement: true);

                    if (i == 1)
                    {
                        v3.Move(aYChange: -thk);
                        v1 = v3.PolarVector(270, 0.1);
                        v2 = v3.PolarVector(270 + 45, 0.1);

                        Image.DimStyleOverrides.SuppressExtLine2 = true;
                        Image.DimStyle().TextFit = dxxDimTextFitTypes.MoveArrowsFirst;
                        dim.Angular3P(v3, v1, v2, 0.825, aImage: _Image);
                        Image.DimStyleOverrides.SuppressExtLine2 = false;

                    }

                    v1 = aPl.Center().Moved(aYChange: -2.5);
                    aPl = (dxePolyline)Image.Primatives.Angle_Top(v1, 1, lng, thk, -90);
                    aPl.LCLSet("GEOMETRY", dxxColors.ByLayer, dxfLinetypes.Continuous);
                    Image.Entities.Add(aPl);
                    dxfDisplaySettings dsp = aPl.DisplaySettings;

                    v1 = aPl.GetVertex(dxxPointFilters.GetBottomLeft).Moved(1, 0.625);
                    draw.aCircle(v1, rad, dsp);

                    draw.aLine(v1.X - (rad + 0.125), v1.Y, v1.X + (rad + 0.125), v1.Y, aLineType: dxfLinetypes.Center);
                    l1 = draw.aLine(v1.X, v1.Y - (rad + 0.125), v1.X, v1.Y + (rad + 0.125), aLineType: dxfLinetypes.Center);

                    v2 = aPl.GetVertex(dxxPointFilters.GetBottomRight, bReturnClone: true);
                    v1 = v2.Moved(-1, 0.625);
                    aA = draw.aCircle(v1, rad, dsp);

                    l2 = draw.aLine(v1.X - (rad + 0.125), v1.Y, v1.X + (rad + 0.125), v1.Y, aLineType: dxfLinetypes.Center);
                    l3 = draw.aLine(v1.X, v1.Y - (rad + 0.125), v1.X, v1.Y + (rad + 0.125), aLineType: dxfLinetypes.Center);

                    v3 = aPl.GetVertex(dxxPointFilters.GetTopRight, bReturnClone: true);
                    dim.Horizontal(l1.EndPt, l3.EndPt, v3.Y + 0.25, bAbsolutePlacement: true, aOverideText: "'B'");
                    dim.Horizontal(l3.EndPt, v3, v3.Y + 0.25, bAbsolutePlacement: true, aOverideText: "'C'");
                    dim.Horizontal(aPl.GetVertex(dxxPointFilters.GetTopLeft), v3, v3.Y + 0.625, bAbsolutePlacement: true, aOverideText: "'A'");

                    dim.Vertical(l2.EndPt, v2, v2.X + 0.375, bAbsolutePlacement: true);
                    dim.Vertical(v2, v3, v2.X + 0.8725, bAbsolutePlacement: true);

                    if (i == 1)
                    {
                        v1 = aPl.GetVertex(dxxPointFilters.GetBottomLeft, bReturnClone: true);
                        v1.Move(0.875);

                        draw.aLine(v1, 90, thk, aSegmentPtType: dxxSegmentPointTypes.StartPt, "GEOMETRY", dxxColors.Undefined, dxfLinetypes.Continuous);

                        v2.Move(-0.875);
                        draw.aLine(v2, 90, thk, aSegmentPtType: dxxSegmentPointTypes.StartPt, "GEOMETRY", dxxColors.Undefined, dxfLinetypes.Continuous);
                    }

                    v1 = aA.AnglePoint(-45);
                    v2 = v1.Moved(0.5, -1);
                    Tool.CreateHoleLeader(Image, aA, v2, dim, 2, true, true);
                    //leader.Text(v1, v2, Tool.HoleLeader(Image, aA, 2), null); 

                    v1 = aPl.Center();
                    v1.Move(aYChange: thk / 2);

                    v1 = draw.aText(v1, "XXX", 0.5, dxxMTextAlignments.MiddleCenter, aTextStyle: "Standard").BoundingRectangle().BottomCenter;
                    v3 = v1.Moved(-0.5, -1);
                    v3.Y = v2.Y;
                    var ldr = leader.Text(v1, v3, @"\W0.7;" + Tool.StampPNLeader(Image), null);

                    aRect = Image.SelectionSetInit(true).BoundingRectangle();

                    if (i == 1)
                    {
                        v1 = aRect.BottomLeft.Moved(aYChange: -0.25);

                        // dividing line
                        aPl = new dxePolyline(new colDXFVectors(new dxfVector(fitRec.Left, v1.Y), new dxfVector(fitRec.Right, v1.Y)), false, aSegWidth: 0.02);
                        aPl.LCLSet("BORDER", aLineType: dxfLinetypes.Continuous);
                        Image.Entities.Add(aPl);
                    }

                    v1 = aRect.TopRight;
                    if (i == 1) v1.Y = fitRec.Top;
                    v1.Move(0.125, 0); //i == 1 ? -0.3 : 0);

                    List<mdEndAngle> EAs = MDProject.EndAngles(bChamfered: i == 1);

                    List<string> rows1 = new() { "PART NO.|QTY. *|'A'|'B'|'C'|FOR TRAYS" };
                    List<string> rows2 = new() { "PART NO.|QTY. *|'A'|'B'|'C'|FOR TRAYS" };

                    for (int j = 1; j <= EAs.Count; j++)
                    {
                        aEA = EAs[j - 1];
                        string row = $"{aEA.PartNumber}" +
                            $"|{uopUtils.CalcSpares(aEA.Quantity, MDProject.ClipSparePercentage, 2)}" +
                            $"|{DStyle.FormatNumber(aEA.Length)}" +
                            $"|{DStyle.FormatNumber(aEA.Length - 2 * aEA.HoleInset)}" +
                            $"|{DStyle.FormatNumber(aEA.HoleInset)}" +
                            $"|{aEA.RangeSpanNames(MDProject, ", ")}";
                        if (j <= 15) { rows1.Add(row); } else { rows2.Add(row); }


                    }

                    dxeInsert table = null;

                    if (rows1.Count > 1)
                    {
                        t++;
                        table = draw.aTableBlk($"Table_{t}", v1, rows1, aGridStyle: dxxTableGridStyles.All, aFooter: rows2.Count <= 1 ? $"*INCLUDES {MDProject.ClipSparePercentage}% SPARES" : ""); // in VB it was aEA.Part.SparePercentage

                    }
                    if (rows2.Count > 1)
                    {
                        t++;
                        v1.X = table.DefinitionPoint(dxxEntDefPointTypes.TopRight).X + 0.25 * paperscale;
                        table = draw.aTableBlk($"Table_{t}", v1, rows2, aGridStyle: dxxTableGridStyles.All, aFooter: $"INCLUDES {aEA.SparePercentage}% SPARES"); ; // in VB it was aEA.Part.SparePercentage

                    }
                }

                //================== NOTES ==================================

                Status = "Drawing Notes";
                AppDrawingBorderData notes = new()
                {
                    PartNumber_Value = $"END ANGLE",

                    Location_Value = $"FOR TRAYS {si}-{ei}",
                };


                Tool.Add_B_BorderNotes(Image, notes, fitRec, paperscale);


            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
            finally
            {
                Status = "";
                DrawinginProgress = false;
            }
        }


        private void DDWG_SpliceAngles()
        {
            //^generates the project wide splice angle drawing
            if (MDProject == null) return;
            dxfVector v1;
            dxfVector v2;
            dxfRectangle fitRec = null;
            dxoDimStyle DStyle = Image.DimStyle();
            uopPartList<mdSpliceAngle> SAngles = MDProject.GetParts().SpliceAngles();
            uopPartList<mdSpliceAngle> MAngles = MDProject.GetParts().ManwayAngles();
            dxeDimension dim;
            string txt;
            double y1;
            mdSpliceAngle angl;
            uopPartList<mdSpliceAngle> angs;
            dxfRectangle rec;

            try
            {

                dxoDrawingTool draw = Image.Draw;
                dxoDimTool dims = Image.DimTool;
                dxoLeaderTool leaders = Image.LeaderTool;


                mdSpliceAngle angle = SAngles.Count > 0 ? SAngles[0] : MAngles.Count > 0 ? MAngles[0] : null;


                dxeInsert ins;
                dxfRectangle rec_left = null;
                dxfRectangle rec_right = null;

                //================== Border File ==================================
                double paperscale = 0.85;
                Tool.Border(this, null, ref fitRec, arPaperScale: ref paperscale, bSuppressAnsiScales: true, aScaleString: "NTS");
                xCancelCheck();
                Drawing.ZoomExtents = false;
                Image.Display.SetFeatureScales(paperscale);

                Image.TableSettings.ColumnGap = 0.05;



                draw.aLine(8 * paperscale, 10 * paperscale, 8 * paperscale, 0.9375 * paperscale, aLineType: dxfLinetypes.Phantom);

                {
                    for (int i = 1; i <= 2; i++)
                    {
                        Status = i == 2 ? "Drawing Right Plan View" : "Drawing Left Plan View";
                        Image.SelectionSetInit(false);
                        angle = i == 1 ? SAngles.Count > 0 ? SAngles[0] : null : MAngles.Count > 0 ? MAngles[0] : null;
                        double width = angle != null ? angle.Width : 2.5;
                        double inset = angle != null ? angle.HoleInset : 0.6875;
                        double? holedia = angle != null ? angle.BoltHole.Diameter : mdGlobals.gsSmallHole;


                        dxePolygon profileview = uopPolygons.GenericSpliceAngle_Profile(i == 1 ? uppPartTypes.SpliceAngle : uppPartTypes.ManwayAngle, out dxePolygon planview);
                        //
                        v1 = i == 1 ? new dxfVector(0.65, 8.75) * paperscale : new dxfVector(8.65, 8.75) * paperscale;
                        ins = draw.aInsert(planview, v1, aDisplaySettings: dxfDisplaySettings.Null(aLayer: "GEOMETRY"));
                        planview.MoveTo(ins.InsertionPt);
                        txt = Image.FormatNumber(width);
                        dim = dims.Vertical(planview.Vertex(3), planview.Vertex(2), v1.X - 0.25 * paperscale, bAbsolutePlacement: true, aOverideText: txt);
                        colDXFVectors dimpts = planview.AdditionalSegments.GetByTag("VER_CENTERLINE").DefinitionPoints(dxxEntDefPointTypes.StartPt);
                        y1 = dimpts.Item(1).Y + 0.25 * paperscale;
                        dim = dims.Horizontal(dimpts.Item(1), planview.Vertex(2), y1, bAbsolutePlacement: true, aOverideText: "\"C\"");
                        dim = dims.Horizontal(dimpts.Item(4), planview.Vertex(7), y1, bAbsolutePlacement: true, aOverideText: "\"C\"");
                        dim = dims.Horizontal(dimpts.Item(1), dimpts.Item(2), y1, bAbsolutePlacement: true, aOverideText: "\"B\"");
                        dim = dims.Horizontal(dimpts.Item(2), dimpts.Item(3), y1, bAbsolutePlacement: true, aOverideText: "\"B\"");

                        rec = Image.SelectionSet(dxxSelectionTypes.CurrentSet).BoundingRectangle();
                        y1 = rec.Top + 0.25 * paperscale;
                        dim = dims.Horizontal(planview.Vertex(2), planview.Vertex(7), y1, bAbsolutePlacement: true, aOverideText: "\"A\"");

                        dimpts = planview.AdditionalSegments.GetByTag("HOR_CENTERLINE").DefinitionPoints(dxxEntDefPointTypes.EndPt);
                        if (i == 1)
                        {
                            dim = dims.Vertical(dimpts.GetVector(dxxPointFilters.AtMaxY), planview.Vertex(7), rec.Right + 0.25 * paperscale, bAbsolutePlacement: true, aOverideText: Image.FormatNumber(inset));
                        }
                        dim = dims.Vertical(planview.Vertex(6), dimpts.GetVector(dxxPointFilters.AtMinY), rec.Right + 0.125 * paperscale, bAbsolutePlacement: true, aOverideText: Image.FormatNumber(inset));

                        //hole diameter leader
                        dxeArc bolthole = planview.AdditionalSegments.Arcs()[3];
                        txt = Tool.HoleLeader(Image, bolthole, 0, aDiameter: holedia);
                        dim = dims.RadialD(bolthole, 360 - 75, 0.5, aOverideText: txt);
                        v1 = i == 1 ? new dxfVector(2.75, 8.375) * paperscale : new dxfVector(10.75, 8.375) * paperscale;

                        //stamp pn leader
                        dxeText mtxt = draw.aText(v1, "XXX", 0.2 * paperscale, aAlignment: dxxMTextAlignments.MiddleCenter);
                        v1 = mtxt.BoundingRectangle().BottomCenter;
                        v2 = dim.BoundingRectangle().BottomLeft.Moved(-0.075 * paperscale, -0.25 * paperscale);
                        leaders.Text(v1, v2, Tool.StampPNLeader(Image));
                        List<dxePolyline> hexes = planview.AdditionalSegments.Polylines().FindAll(x => string.Compare(x.Tag, "HEX_HEAD", true) == 0);

                        // bolt leader
                        hdwHexBolt bolt = MDProject.SmallBolt(bWeldedInPlace: true);
                        v1 = hexes[0].Vertex(5);
                        v2.X = v1.X + 0.25 * paperscale;
                        txt = bolt.IsMetric ? (bolt.Length * 25.4).ToString("0") : bolt.Length.ToString("0.0##") + "''";
                        txt = $"{bolt.SizeName} X {txt} HEX HEAD BOLTS";
                        txt = $"{txt}\\PTACK WELDED TO ANGLE";
                        txt = $"{txt}\\PWITH TWO OPPOSING TACKS (TYP)";
                        leaders.Text(v1, v2, txt);



                        rec = Image.SelectionSet(dxxSelectionTypes.CurrentSet).BoundingRectangle(dxfPlane.World);
                        if (i == 1) { rec_left = rec; } else { rec_right = rec; }
                        v1 = new dxfVector(rec.Right + 0.4 * paperscale, planview.Y);
                        ins = draw.aInsert(profileview, v1);
                        profileview.MoveTo(v1);
                        v1 = profileview.GetVertex(dxxPointFilters.GetBottomRight, aPrecis: 1);
                        dim = dims.Horizontal(profileview.GetVertex(dxxPointFilters.GetLeftBottom, aPrecis: 1), v1, v1.Y - 0.25 * paperscale, bAbsolutePlacement: true, aOverideText: "\"D\"");

                    }


                }

                // the tables

                for (int i = 1; i <= 2; i++)
                {
                    rec = (i == 1) ? rec_left : rec_right;
                    if (DrawlayoutRectangles)
                    {
                        draw.aPolyline(rec, aDisplaySettings: new dxfDisplaySettings(aColor: dxxColors.LightGrey));
                    }


                    List<string> atble = new() { "PT.\\PNO|\"A\"|\"B\"|\"C\"|\"D\"|BOLT\\PCOUNT|NO. PER\\PTRAY|NO.\\PREQ'D.*|FOR\\PTRAYS" };
                    angs = (i == 1) ? SAngles : MAngles;

                    for (int j = 1; j <= angs.Count; j++)
                    {
                        angl = (mdSpliceAngle)angs[j - 1];
                        List<string> values =
                        [
                            angl.PartNumber,
                            DStyle.FormatNumber(angl.Length),
                            DStyle.FormatNumber(angl.BoltSpacing),
                            DStyle.FormatNumber(1),
                            DStyle.FormatNumber(angl.Height),
                            angl.GenHoles().Item(1).Count.ToString(),
                            angl.OccuranceFactor.ToString(),
                            uopUtils.CalcSpares(angl.Quantity, MDProject.ClipSparePercentage, 2).ToString(),
                            angl.RangeSpanNames(MDProject, ", "),
                        ];

                        txt = mzUtils.ListToString(values, "|");
                        atble.Add(txt);
                    }
                    atble.Add("|||||");
                    v1 = rec.BottomLeft.Moved(aYChange: -0.5);
                    //v1.X = 4 + (i - 1) * 8;

                    v1.Y = draw.aTable($"TABLE {i}", v1, atble, aDelimiter: "|", aTableAlign: dxxRectangularAlignments.TopLeft, aGridStyle: dxxTableGridStyles.All, aFooter: "* INCLUDES 2% SPARES").BoundingRectangle().Bottom - 0.5;
                    if (fitRec.Bottom + 0.75 * paperscale < v1.Y) v1.Y = fitRec.Bottom + 0.75 * paperscale;
                    v1.X = 1 + (i - 1) * 8;

                    txt = (i == 1) ? "SPLICE" : "MANWAY";

                    draw.aText(v1, $"{txt} ANGLE\\PFOR MATERIAL SEE SHEET 2", 0.1875, dxxMTextAlignments.TopLeft, aTextStyle: "Standard");
                }

                //break;
                //return; // ??? there are some codes after it in VB

                ////================== NOTES ==================================
                //Status = "Drawing Notes";
                //txt = "END ANGLE";
                //txt = $"{txt}\\PFOR MATERIAL SEE SHEET 2";

                //Tool.AddBorderNotes(Image, txt, fitRec, 1);

                //Status = "";
                //DrawinginProgress = false;
                // }
            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = string.Empty; DrawinginProgress = false; }
        }


        private void DDWG_SpliceAnglesOLD()
        {
            //^generates the project wide splice angle drawing

            dxfVector v1;
            dxfVector v2;
            dxfRectangle fitRec = null;
            dxoDimStyle DStyle = Image.DimStyle();
            colUOPParts aParts = MDProject.TrayRanges.SpliceAngles(false, true, true);

            dxeDimension dim;


            List<uopPart> SAngles = aParts.GetByPartType(uppPartTypes.SpliceAngle);
            List<uopPart> MAngles = aParts.GetByPartType(uppPartTypes.ManwayAngle);
            string txt;
            double y1;

            mdSpliceAngle angl;
            List<uopPart> angs;
            dxfRectangle rec;

            try
            {

                dxoDrawingTool draw = Image.Draw;
                dxoDimTool dims = Image.DimTool;
                dxoLeaderTool leaders = Image.LeaderTool;


                mdSpliceAngle angle = SAngles.Count > 0 ? (mdSpliceAngle)SAngles[0] : MAngles.Count > 0 ? (mdSpliceAngle)MAngles[0] : null;

                dxeInsert ins;
                dxfRectangle rec_left = null;
                dxfRectangle rec_right = null;

                //================== Border File ==================================
                double paperscale = 1;
                Tool.Border(this, null, ref fitRec, arPaperScale: ref paperscale, bSuppressAnsiScales: true, aScaleString: "NTS");
                xCancelCheck();
                Drawing.ZoomExtents = false;


                Image.TableSettings.ColumnGap = 0.1;



                draw.aLine(8, 10, 8, 0.9375, aLineType: dxfLinetypes.Phantom);

                Status = "Drawing Left Plan View";
                {
                    for (int i = 1; i <= 2; i++)
                    {
                        if (i == 2) Status = "Drawing Right Plan View";
                        Image.SelectionSetInit(false);
                        angle = i == 1 ? SAngles.Count > 0 ? (mdSpliceAngle)SAngles[0] : null : MAngles.Count > 0 ? (mdSpliceAngle)MAngles[0] : null;
                        double width = angle != null ? angle.Width : 2.5;
                        double inset = angle != null ? angle.HoleInset : 0.6875;
                        double? holedia = angle != null ? angle.BoltHole.Diameter : mdGlobals.gsSmallHole;


                        dxePolygon profileview = uopPolygons.GenericSpliceAngle_Profile(i == 1 ? uppPartTypes.SpliceAngle : uppPartTypes.ManwayAngle, out dxePolygon planview);
                        //planview = uopPolygons.GenericSpliceAngle_Plan(i == 1 ? uppPartTypes.SpliceAngle : uppPartTypes.ManwayAngle);
                        v1 = i == 1 ? new dxfVector(0.65, 8.75) : new dxfVector(8.65, 8.75);
                        ins = draw.aInsert(planview, v1, aDisplaySettings: dxfDisplaySettings.Null(aLayer: "GEOMETRY"));
                        planview.MoveTo(ins.InsertionPt);
                        txt = Image.FormatNumber(width);
                        dim = dims.Vertical(planview.Vertex(3), planview.Vertex(2), v1.X - 0.25 * paperscale, bAbsolutePlacement: true, aOverideText: txt);
                        colDXFVectors dimpts = planview.AdditionalSegments.GetByTag("VER_CENTERLINE").DefinitionPoints(dxxEntDefPointTypes.StartPt);
                        y1 = dimpts.Item(1).Y + 0.25 * paperscale;
                        dim = dims.Horizontal(dimpts.Item(1), planview.Vertex(2), y1, bAbsolutePlacement: true, aOverideText: "\"C\"");
                        dim = dims.Horizontal(dimpts.Item(4), planview.Vertex(7), y1, bAbsolutePlacement: true, aOverideText: "\"C\"");
                        dim = dims.Horizontal(dimpts.Item(1), dimpts.Item(2), y1, bAbsolutePlacement: true, aOverideText: "\"B\"");
                        dim = dims.Horizontal(dimpts.Item(2), dimpts.Item(3), y1, bAbsolutePlacement: true, aOverideText: "\"B\"");

                        rec = Image.SelectionSet(dxxSelectionTypes.CurrentSet).BoundingRectangle();
                        y1 = rec.Top + 0.25 * paperscale;
                        dim = dims.Horizontal(planview.Vertex(2), planview.Vertex(7), y1, bAbsolutePlacement: true, aOverideText: "\"A\"");

                        dimpts = planview.AdditionalSegments.GetByTag("HOR_CENTERLINE").DefinitionPoints(dxxEntDefPointTypes.EndPt);
                        if (i == 1)
                        {
                            dim = dims.Vertical(dimpts.GetVector(dxxPointFilters.AtMaxY), planview.Vertex(7), rec.Right + 0.25 * paperscale, bAbsolutePlacement: true, aOverideText: Image.FormatNumber(inset));
                        }
                        dim = dims.Vertical(planview.Vertex(6), dimpts.GetVector(dxxPointFilters.AtMinY), rec.Right + 0.125 * paperscale, bAbsolutePlacement: true, aOverideText: Image.FormatNumber(inset));

                        //hole diameter leader
                        dxeArc bolthole = planview.AdditionalSegments.Arcs()[3];
                        txt = Tool.HoleLeader(Image, bolthole, 0, aDiameter: holedia);
                        dim = dims.RadialD(bolthole, 360 - 75, 0.5, aOverideText: txt);
                        v1 = i == 1 ? new dxfVector(2.575, 8.375) : new dxfVector(10.575, 8.375);

                        //stamp pn leader
                        dxeText mtxt = draw.aText(v1, "XXX", 0.2 * paperscale, aAlignment: dxxMTextAlignments.MiddleCenter);
                        v1 = mtxt.BoundingRectangle().BottomCenter;
                        v2 = dim.BoundingRectangle().BottomLeft.Moved(-0.075 * paperscale, -0.25 * paperscale);
                        leaders.Text(v1, v2, Tool.StampPNLeader(Image));
                        List<dxePolyline> hexes = planview.AdditionalSegments.Polylines().FindAll(x => string.Compare(x.Tag, "HEX_HEAD", true) == 0);

                        // bolt leader
                        hdwHexBolt bolt = MDProject.SmallBolt(bWeldedInPlace: true);
                        v1 = hexes[0].Vertex(5);
                        v2.X = v1.X + 0.25 * paperscale;
                        txt = bolt.IsMetric ? (bolt.Length * 25.4).ToString("0") : bolt.Length.ToString("0.0##") + "''";
                        txt = $"{bolt.SizeName} X {txt} HEX HEAD BOLTS";
                        txt = $"{txt}\\PTACK WELDED TO ANGLE";
                        txt = $"{txt}\\PWITH TWO OPPOSING TACKS (TYP)";
                        leaders.Text(v1, v2, txt);



                        rec = Image.SelectionSet(dxxSelectionTypes.CurrentSet).BoundingRectangle(dxfPlane.World);
                        if (i == 1) { rec_left = rec; } else { rec_right = rec; }
                        v1 = new dxfVector(rec.Right + 0.5 * paperscale, planview.Y);
                        ins = draw.aInsert(profileview, v1);
                        profileview.MoveTo(v1);
                        v1 = profileview.GetVertex(dxxPointFilters.GetBottomRight, aPrecis: 1);
                        dim = dims.Horizontal(profileview.GetVertex(dxxPointFilters.GetLeftBottom, aPrecis: 1), v1, v1.Y - 0.25 * paperscale, bAbsolutePlacement: true, aOverideText: "\"D\"");

                    }


                }

                // the tables

                for (int i = 1; i <= 2; i++)
                {
                    rec = (i == 1) ? rec_left : rec_right;
                    if (DrawlayoutRectangles)
                    {
                        draw.aPolyline(rec, aDisplaySettings: new dxfDisplaySettings(aColor: dxxColors.LightGrey));
                    }


                    List<string> atble = new() { "PT.\\PNO|\"A\"|\"B\"|\"C\"|\"D\"|BOLT\\PCOUNT|NO. PER\\PTRAY|NO.\\PREQ'D.*|FOR\\PTRAYS" };
                    angs = (i == 1) ? SAngles : MAngles;

                    for (int j = 1; j <= angs.Count; j++)
                    {
                        angl = (mdSpliceAngle)angs[j - 1];
                        List<string> values = new() { angl.PartNumber };
                        values.Add(DStyle.FormatNumber(angl.Length));
                        values.Add(DStyle.FormatNumber(angl.BoltSpacing));
                        values.Add(DStyle.FormatNumber(1));
                        values.Add(DStyle.FormatNumber(angl.Height));
                        values.Add(angl.GenHoles().Item(1).Count.ToString());
                        values.Add(angl.OccuranceFactor.ToString());
                        values.Add(uopUtils.CalcSpares(angl.Quantity * angl.TrayCount, MDProject.ClipSparePercentage, 2).ToString());
                        values.Add(angl.RangeSpanNames(MDProject));

                        txt = mzUtils.ListToString(values, "|");
                        atble.Add(txt);
                    }
                    atble.Add("|||||");
                    v1 = rec.BottomLeft.Moved(aYChange: -0.5);
                    //v1.X = 4 + (i - 1) * 8;

                    v1.Y = draw.aTable($"TABLE {i}", v1, atble, aDelimiter: "|", aTableAlign: dxxRectangularAlignments.TopLeft, aGridStyle: dxxTableGridStyles.All, aFooter: "* INCLUDES 2% SPARES").BoundingRectangle().Bottom - 0.5;
                    if (fitRec.Bottom + 0.75 * paperscale < v1.Y) v1.Y = fitRec.Bottom + 0.75 * paperscale;
                    v1.X = 1 + (i - 1) * 8;

                    txt = (i == 1) ? "SPLICE" : "MANWAY";

                    draw.aText(v1, $"{txt} ANGLE\\PFOR MATERIAL SEE SHEET 2", 0.1875, dxxMTextAlignments.TopLeft, aTextStyle: "Standard");
                }

                //break;
                //return; // ??? there are some codes after it in VB

                ////================== NOTES ==================================
                //Status = "Drawing Notes";
                //txt = "END ANGLE";
                //txt = $"{txt}\\PFOR MATERIAL SEE SHEET 2";

                //Tool.AddBorderNotes(Image, txt, fitRec, 1);

                //Status = "";
                //DrawinginProgress = false;
                // }
            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = string.Empty; DrawinginProgress = false; }
        }

        private void DDWG_SupplementalDeflectors()
        {
            //^generates the project wide suuplemental deflectors drawing


            dxfRectangle fitRec = null;
            dxoDimStyle DStyle = Image.DimStyle();
            uopPartList<mdSupplementalDeflector> aParts = MDProject.GetParts().SupplementalDeflectors();

            dxeDimension dim;

            dxfRectangle rec = new(10, 1);

            try
            {

                dxoDrawingTool draw = Image.Draw;
                dxoDimTool dims = Image.DimTool;

                //================== Border File ==================================
                double paperscale = 1;
                Tool.Border(this, null, ref fitRec, arPaperScale: ref paperscale, bSuppressAnsiScales: true, aScaleString: "NTS");
                xCancelCheck();
                Drawing.ZoomExtents = false;
                rec.MoveFromTo(rec.TopLeft, fitRec.TopLeft.Moved(2, -2));

                Image.TableSettings.ColumnGap = 0.1;
                if (DrawlayoutRectangles)
                {
                    draw.aPolyline(fitRec, aDisplaySettings: new dxfDisplaySettings(aColor: dxxColors.LightBlue));

                }

                draw.aPolyline(rec, aDisplaySettings: new dxfDisplaySettings(aLayer: "GEOMETRY"));
                dxfVector v1 = rec.TopLeft;
                dxfVector v2 = rec.TopRight;
                dxfVector v3 = rec.BottomRight;

                dim = dims.Horizontal(v1, v2, v1.Y + 0.5 * paperscale, bAbsolutePlacement: true, aOverideText: "\"A\"");
                dim = dims.Vertical(v3, v2, v2.X + 0.5 * paperscale, bAbsolutePlacement: true, aOverideText: "\"B\"");
                v1.SetCoordinates(dim.BoundingRectangle().Right + 1.125 * paperscale, v1.Y);

                dxfRectangle brec = new(0.25, rec.Height);
                brec.MoveFromTo(brec.TopLeft, v1);
                draw.aPolyline(brec, aDisplaySettings: new dxfDisplaySettings(aLayer: "GEOMETRY"));

                v2 = brec.BottomRight;
                v3 = brec.BottomLeft;
                dim = dims.Horizontal(v2, v3, v2.Y - 0.5 * paperscale, bAbsolutePlacement: true, aOverideText: "\"C\"");


                // the table

                List<string> atble = new() { "\"A\"|\"B\"|\"C\"|NO.\\PREQ'D.|FOR\\PDC's|FOR\\PTRAYS" };


                for (int j = 1; j <= aParts.Count; j++)
                {
                    mdSupplementalDeflector angl = aParts[j - 1];
                    List<string> values =
                    [
                        DStyle.FormatNumber(angl.Length),
                        DStyle.FormatNumber(angl.Height),
                        DStyle.FormatNumber(angl.Thickness),
                        angl.Quantity.ToString(),
                        angl.AssociatedParentList(),
                        angl.RangeSpanNames(MDProject),
                    ];


                    atble.Add(mzUtils.ListToString(values, "|"));
                }
                atble.Add("||||");
                v1 = fitRec.BottomLeft.Moved(1, 1);
                v1.X = rec.Left;
                draw.aTable($"TABLE_SUPPDEFS", v1, atble, aDelimiter: "|", aTableAlign: dxxRectangularAlignments.BottomLeft, aGridStyle: dxxTableGridStyles.All);
                if (fitRec.Bottom + 0.75 * paperscale < v1.Y) v1.Y = fitRec.Bottom + 0.75 * paperscale;
                v1.X = 11;



                draw.aText(v1, $"SUPPLEMENTAL DEFLECTORS\\PFOR MATERIAL SEE SHEET 2", 0.1875, dxxMTextAlignments.TopLeft, aTextStyle: "Standard");

            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = string.Empty; DrawinginProgress = false; }
        }

        private void DDWG_UniqueSections()
        {


            double paperscale;
            string txt;
            colDXFVectors aPts;
            dxePolygon aPG;
            string lname = "";
            string bname;
            dxfVector ip;
            double dx = 0;
            dxfDisplaySettings blue = dxfDisplaySettings.Null(aColor: dxxColors.Blue, aLinetype: dxfLinetypes.Hidden2);
            try
            {
                Image.Layers.Add("ECMD SLOTS", dxxColors.LightGrey);
                dxoDrawingTool draw = Image.Draw;

                int views = Assy.HasAlternateDeckParts ? 2 : 1;

                for (int view = 1; view <= views; view++)
                {
                    if (view == 2)
                    {
                        dx = 1.15 * Assy.ShellID;
                        Image.UCS.Translate(dx, 0);
                        Image.UCS.Rotate(90);

                    }
                    draw.aCircle(dxfVector.Zero, Assy.RingClipRadius, blue);
                }
                Image.UCS.Reset();

                xCancelCheck();

                paperscale = Image.Display.ZoomExtents(1.05, true);


                double tht = 0.125 * Image.Display.PaperScale;
                List<Tuple<string, dxxColors>> clrs = new();
                dxxColors clr = dxxColors.Red;  //1
                dxxColors bkgrnd = Image.Display.BackColor;
                for (int view = 1; view <= views; view++)
                {
                    if (view == 2)
                    {
                        dx = 1.15 * Assy.ShellID;
                        Image.UCS.Translate(dx, 0);
                        // Image.UCS.Rotate(90);

                    }
                    // if (DrawlayoutRectangles && view == 1 ) Assy.Invalidate(uppPartTypes.DeckSection);
                    List<mdDeckSection> Uniques = [.. Assy.UniqueDeckSections(view == 2)];

                    foreach (mdDeckSection section in Uniques)
                    {
                        dxoLayer layer = null;
                        Tuple<string, dxxColors> seccolor = clrs.Find((x) => x.Item1 == section.PartNumber);
                        if (seccolor == null)
                        {
                            while (clrs.FindIndex((x) => x.Item2 == clr) >= 0 && clr != bkgrnd && clr != dxxColors.LightGrey)
                            {
                                clr = (dxxColors)(int)(clr + 1);
                            }


                            clrs.Add(new Tuple<string, dxxColors>(section.PartNumber, clr));
                            layer = Image.Layers.Add(section.PartNumber, clr);
                        }
                        else
                        {
                            clr = seccolor.Item2;
                            layer = Image.Layers.GetOrAdd(section.PartNumber, clr);
                            layer = (dxoLayer)Image.GetOrAddReference(section.PartNumber, dxxReferenceTypes.LAYER, clr);
                        }

                        bname = $"SECTION_{section.PartNumber}_UNIQUEPLAN";

                        //aPG = section.View_Plan(Assy, bObscured: true, bIncludePromoters: true, bIncludeHoles: true, bIncludeSlotting: true, bRegeneratePerimeter: DrawlayoutRectangles, aCenter: dxfVector.Zero, aRotation: 0, aLayerName: lname, bSolidHoles: true);
                        //aPG.BlockName = bname;
                        Status = $"Creating Section {section.Handle} Instances";

                        dxfBlock block = mdBlocks.DeckSection_View_Plan(Image, section, Assy, bname, bSetInstances: true, bObscured: true, bIncludePromoters: true, bIncludeHoles: true, bIncludeSlotting: true, bRegeneratePerimeter: DrawlayoutRectangles, aLayerName: layer.Name, bSolidHoles: true, bShowPns: false, bShowQuantities: false);
                        double rot = 0;

                        //if (!Image.Blocks.TryGet(bname, ref block))
                        //{
                        //    Status = $"Creating Section {section.Handle} Instances";

                        //    block = aPG.Block(bname, aImage: Image, aLayerName: section.PartNumber);
                        //    //block.Entities.SetDisplayVariable(dxxDisplayProperties.Color, dxxColors.BlackWhite);
                        //    block = Image.Blocks.Add(block);
                        //}
                        //else
                        //{

                            if (view == 2)
                                rot = 180;
                        //}
                        dxfDisplaySettings dsp = new(section.PartNumber, dxxColors.ByLayer);
                        aPts = block.Instances.MemberPoints(section.Center,aReturnBasePt:true); // aRotationAdder: view == 1 ? 0 : rot);

                        Status = $"Drawing Section {section.Handle} Instances";

                        //if (section.IsHalfMoon)
                        //{
                        //    double rotationAdder = view == 1 ? 0 : rot;

                        //    // Only the left half-moon needs a 180 rotation
                        //    foreach (var insertionPoint in aPts)
                        //    {
                        //        draw.aInsert(bname, insertionPoint, aRotationAngle: rotationAdder + (insertionPoint.X > 0 ? 0 : 180), aDisplaySettings: dsp);
                        //    }
                        //}
                        //else
                        //{
                            draw.aInserts(bname, aPts, aDisplaySettings: dsp);
                        // }

                        for (int j = 1; j <= aPts.Count; j++)
                        {
                            ip = aPts.Item(j);
                            if (j == 1)
                            {
                                //draw.aInsert(aPG, ip, ip.Rotation, aBlockName: bname);
                                txt = $"{section.PartNumber}\\PQTY:{section.Quantity}";
                                txt = $"{txt}\\POCCURS:{section.OccuranceFactor}";
                                draw.aText(ip, txt, aTextHeight: tht, aAlignment: dxxMTextAlignments.MiddleCenter, aLayer: section.PartNumber, aColor: dxxColors.ByLayer, aHeightFactor: 0.5);

                            }
                            else
                            {
                                //draw.aInsert(bname, ip, ip.Rotation);
                                txt = $"{section.PartNumber}";
                                draw.aText(ip, txt, aTextHeight: tht, aAlignment: dxxMTextAlignments.MiddleCenter, aLayer: section.PartNumber, aColor: dxxColors.ByLayer, aHeightFactor: 0.375);
                                //draw.aCircle(ip, 0.015 * paperscale,aColor: (dxxColors)i);


                            }
                        }



                        xCancelCheck();
                        //if (view == 2) Image.UCS.ResetDirections();

                        uopRingRange trange = null;
                        if (views == 1)
                            trange = Assy.RingRange;
                        else
                            trange = view == 1 ? Assy.RingRange.FirstAlternatingRange : Assy.RingRange.SecondAlternatingRange;

                        txt = $"TRAYS {trange.SpanName.ToUpper()} EQUAL DECK SECTION ";

                        if (views == 2)
                            txt = $"{txt}\\P{trange.RingCount} OF {Assy.RingRange.RingCount} RINGS";
                        else
                            txt = $"{txt}\\P{trange.RingCount} RINGS";

                        draw.aText(new dxfVector(0, -ShellRad - 0.25 * paperscale), txt, aAlignment: dxxMTextAlignments.TopCenter);
                        //if (view == 2) Image.UCS.Rotate(90);


                    }
                }

                if (DrawlayoutRectangles)
                {



                    //colMDDeckSplices splices = Assy.DeckSplices;

                    //for (int i = 1; i <= Assy.PanelCount; i++)
                    //{
                    //    List<mdDeckSplice> psplices = splices.GetByPanelIndex(i);
                    //    for(int j = 1; j <= psplices.Count; j++)
                    //    {
                    //        mdDeckSplice splice = psplices[j - 1];
                    //        txt = $"{splice.Handle} - {j}";
                    //        txt = $"{txt}\\PMFTag : {splice.MFTag}";
                    //        draw.aText(new dxfVector(splice.X, splice.Y), txt, aAlignment: dxxMTextAlignments.MiddleCenter, aLayer: "SPLICE_INFO", aColor: dxxColors.BlackWhite, aHeightFactor: 0.5);
                    //    }
                    //}

                }

            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
            finally { Status = ""; DrawinginProgress = false; Image.UCS.Reset(); }
        }

        private void DDWG_Functional()
        {
            if (Assy == null) return;
            double paperscale = 0;

            dxfRectangle fitrec = null;
            dxfRectangle aRec = new(2.5 * ShellRad, 2.5 * ShellRad);

            try
            {

                if (DrawlayoutRectangles)
                {
                    Image.Header.UCSMode = dxxUCSIconModes.Origin;
                }

                //================== Border File ==================================
                Tool.Border(this, aRec, ref fitrec, arPaperScale: ref paperscale, bSuppressAnsiScales: true, aScaleString: "NONE", aSheetNo: Range.Index);
                //================== Border File ==================================

                dxfVector v1 = fitrec.TopLeft.Moved(1.3 * ShellRad, -1.5 * ShellRad);

                xCancelCheck();

                Image.UCS.SetCoordinates(v1.X, v1.Y);

                Draw_FunctionalPlanView(out dxeLine VerCenterLn, out dxeLine HorCenterLn,out colDXFEntities DCClines, out List<dxeInsert> sections);
               
                xCancelCheck();
                Draw_FunctionalNotation(paperscale, HorCenterLn, VerCenterLn, null, DCClines, sections);
                xCancelCheck();
                Image.UCS.Reset();
                xCancelCheck();
                Draw_FunctionalBlocks(paperscale);



            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
            finally
            {
                DrawinginProgress = false;
            }
        }

        private void DDWG_InputSketch()
        {
            if (Assy == null) return;

            dxePolyline aPl;
             double aY;
            
            colMDDowncomers aDCs = Assy.Downcomers;
            dxfDisplaySettings dsp;
            dxoDrawingTool draw;
            dxxColors hclr;
            dxfRectangle ViewRectang;
            double minY;
            int beamcount = 0;
            if (Assy.DesignFamily.IsBeamDesignFamily()) beamcount = Assy.Beam.OccuranceFactor; 
            dxfRectangle rect;
            double paperscale;
            try
            {
                Status = "Initializing";
                {
                    draw = Image.Draw;
                    hclr = appApplication.DrawSettings.HiddenLineColor;

                    ViewRectang = Assy.SpoutGroups.BoundingRectangle();
                    Image.Display.ZoomOnRectangle(ViewRectang, 0.5, 0.5, bSetFeatureScales: false, bSuppressAutoRedraw: true);

                    Image.Header.LineTypeScale = 0.25 / Image.Display.ZoomFactor;
                    minY = ViewRectang.Bottom;

                    Drawing.ZoomExtents = false;

                    Image.Header.UCSMode = dxxUCSIconModes.None;
                }

                uopRectangle urect = null;

                if (Assy.IsSymmetric)
                {
                    urect = Assy.FeatureViewRectangle(bIncludePanels: true, bIncludDowncomers: false, bIncludFullDowncomers: false, aWidthBuffer: Assy.Downcomer().Boxes.First().Width, aHeightBuffer: Assy.Downcomer().Boxes.First().Width);
                    urect.Bottom = !Assy.OddDowncomers ? Assy.Downcomers.GetByVirtual(true)[0].X : -Assy.Downcomer().BoxWidth / 2 - 0.5 * Assy.FunctionalPanelWidth;
                    urect.Left = urect.Bottom;
                }
                else
                {
                    double d1 = MDRange.ShellID * 0.5 * 1.05;
                    urect = new uopRectangle(-d1, d1, d1, -d1);
                }

                rect =  urect.ToDXFRectangle();

                if (uopUtils.RunningInIDE)
                    draw.aRectangle(rect, aColor: dxxColors.Red);

                Image.Display.SetLimits(rect);

                Status = "Drawing Ring and Shell Arcs";
                {
                    
                    dxeArc arc1 = new(dxfVector.Zero, MDRange.RingID * 0.5, aDisplaySettings: new dxfDisplaySettings("0", dxxColors.Green, dxfLinetypes.Hidden));
                   
                    Image.Entities.Add(arc1);
                    arc1 = new dxeArc(dxfVector.Zero, MDRange.ShellID * 0.5, aDisplaySettings: new dxfDisplaySettings("0", dxxColors.BlackWhite, dxfLinetypes.Continuous));
                   
                    Image.Entities.Add(arc1);
                }

                    Status = "Drawing Centerlines";
                {
                    draw.aLine(new dxfVector(0, rect.Top), new dxfVector(0, rect.Bottom), new dxfDisplaySettings(aLayer: "0", aColor: dxxColors.Red, aLinetype: dxfLinetypes.Center));
                    draw.aLine(new dxfVector(rect.Left, 0), new dxfVector(rect.Right, 0), new dxfDisplaySettings(aLayer: "0", aColor: dxxColors.Red, aLinetype: dxfLinetypes.Center));

                    paperscale = Image.Display.ZoomExtents(bSetFeatureScale: true);
                }


                Status = "Drawing Downcomers";
                {
                    Draw_Downcomers(uppViews.LayoutPlan, out colDXFEntities DCClines, bDrawSpouts: false, bDrawBothSides: false, bSuppressHoles: true, aLayer: "DOWNCOMERS", bDrawFingerClips: false, bDrawStiffeners: false, bDrawEndAngles: true, bDrawBaffles: false);
                    uopVectors fcPts = Assy.FingerClipPoints(bTrayWide: false, bReturnCenters: true);
                    mdFingerClip aFC = Assy.FingerClip;
                    List<dxfEntity> rectangles = draw.aRectangles(fcPts, aFC.Width, aFC.Length, aDisplaySettings: dxfDisplaySettings.Null("FINGER_CLIPS", dxxColors.BlackWhite, dxfLinetypes.Continuous)).OfType<dxfEntity>().ToList();
                    // draw downcomers below

                    dxfBlock block = mdBlocks.Stiffener_View_Plan(Image, Assy, aBox: Assy.Downcomers.Boxes(1).FirstOrDefault(), bSetInstances: true, bSuppressHoles:true);
                    draw.aInserts(block, block.Instances, bOverrideExisting: false);

                    Draw_DowncomersBelow(uppViews.Plan,bBothSides: Assy.DesignFamily.IsStandardDesignFamily());  
                   
                }

                Status = "Drawing Spout Groups";
                {
                    //draw the spout groups
                    colMDSpoutGroups SGs = Assy.SpoutGroups;
                    dsp = new("0", dxxColors.BlackWhite, dxfLinetypes.Continuous);
                    foreach (mdSpoutGroup sg in SGs)
                    {
                        if (sg.IsVirtual) continue;
                        if (sg.SpoutCount(Assy) <= 0) continue;
                        Status = $"Creating Spout Group {sg.Handle} Block";
                        string bname = $"SG_{sg.DowncomerIndex}_{sg.PanelIndex}";
                        if (beamcount > 1) bname += $"_{sg.BoxIndex}";
                        dxfBlock block = Image.Blocks.Add(sg.Block(aBlockname: bname, dsp: dsp, aImage: Image, bSuppressInstances:false));
                        if (block.Entities.Count > 0)
                        {
                            Status = $"Drawing Spout Group {sg.Handle} Block";
                            draw.aInsert(block.Name, new dxfVector(sg.X, sg.Y), aDisplaySettings: dsp);
                        }
                    }
                }

                Status = "Drawing Deck Sections";
                {
                    double manid = Assy.ManholeID;
                    //deck panel perimeters
                    //if (Assy.DesignFamily.IsBeamDesignFamily())
                    //{
                    //    colDXFVectors panelVertices;
                    //    uopShapes allPanelShapes = Assy.DowncomerData.CreatePanelShapes(bIncludeClearance: false);

                    //    foreach (var panelShape in allPanelShapes)
                    //    {
                    //        if (panelShape.IsVirtual) continue;

                    //        dsp.Color = dxxColors.Blue;
                    //        panelVertices = new colDXFVectors(panelShape.Vertices);
                    //        var pl = new dxePolyline(panelVertices, true, aDisplaySettings: dsp);
                    //        Image.Entities.Add(pl, aTag: $"PANEL{panelShape.PartIndex}");
                    //    }
                    //}
                    //else
                    //{


                    List<uopSectionShape> sections = Assy.DeckSections.BaseShapes(false, Assy);
                        dsp = new dxfDisplaySettings("0", dxxColors.Grey, dxfLinetypes.Continuous);

                        foreach(var section in sections)
                        {
                   
                            aY = section.Top;

                            dsp.Color = (!section.FitsThruManhole(manid)) ? dxxColors.Red : dxxColors.Grey;
                            aPl = draw.aShape(section, dsp);
                            if (section.IsManway)
                            {
                                draw.aHatch_ByFillStyle(dxxFillStyles.DiagonalCross, aPl, aDisplaySettings: new dxfDisplaySettings("0", dxxColors.LightBlue, dxfLinetypes.Continuous), aLineSpacing: 0.125);
                            }

                        //}
                        //mdDeckSections aDSs = Assy.DeckSections;
                        //foreach (mdDeckSection ds in aDSs)
                        //{

                        //    uopShape aBound = ds.Boundary;
                        //    aY = aBound.Limits().Top;


                        //    clr = (!ds.FitsThroughManhole(Assy, ref manid)) ? dxxColors.Red : dxxColors.Grey;

                        //    if (!ds.IsManway)
                        //    {
                        //        aPl = (dxePolyline)Image.Entities.Add(aBound.Perimeter("0", clr, dxfLinetypes.Continuous));
                        //    }
                        //    else
                        //    {
                        //        if (clr == dxxColors.Grey) { clr = dxxColors.LightBlue; }
                        //        aPl = (dxePolyline)Image.Entities.Add(aBound.Perimeter("0", clr, dxfLinetypes.Continuous));
                        //        draw.aHatch_ByFillStyle(dxxFillStyles.DiagonalCross, aPl, aDisplaySettings: new dxfDisplaySettings("0", dxxColors.LightBlue, dxfLinetypes.Continuous), aLineSpacing: 0.125);
                        //    }

                        //}
                    }
                }

            

                Status = "Drawing Bubble Promoters";
                {
                    // draw Startups
                    if (Assy.DesignOptions.HasBubblePromoters)
                    {

                        uopVectors aBPs = Assy.BPCenters(bRegen: DrawlayoutRectangles);
                        dsp = new dxfDisplaySettings("BUBBLE_PROMOTERS", dxxColors.BlackWhite, dxfLinetypes.Continuous);
                        //get the unsuppressed sites and draw them black/white
                        uopVectors unsuppressed = aBPs.GetBySuppressed(false, out uopVectors suppressed);


                        if (unsuppressed.Count > 0) draw.aCircles(unsuppressed, mdGlobals.BPRadius, dsp);
                        //draw the suppressed sites in red
                        if (suppressed.Count > 0)
                        {
                            dsp.Color = dxxColors.Red;
                            draw.aCircles(suppressed, mdGlobals.BPRadius, dsp);
                        }


                    }

                }

                Status = "Drawing Startup Pointers";
                {
                    //Assy.StartupSpouts.GetCenters(Assy, out p1, out colDXFVectors p2, out colDXFVectors p3, out colDXFVectors p4, bSeperateObscured: Assy.ProjectType == uppProjectTypes.MDDraw);
                    double wd = Assy.FunctionalPanelWidth / 6.5;
                    //wd = paperscale * 0.1875;
                    mdStartupSpouts startups = Assy.StartupSpouts;
                    startups.SetObscured(Assy, null);
                    uopVectors centers = startups.Centers(aXOffset: 0);
                    dsp = new dxfDisplaySettings("STARTUP_POINTERS", dxxColors.BlackWhite, dxfLinetypes.Continuous);

                    draw.aPointers(centers.FindAll(x => !x.Mark && !x.Suppressed), wd, aRotation: null, aDisplaySettings: dsp);
                    dsp.Color = dxxColors.Red;
                    draw.aPointers(centers.FindAll(x => x.Mark && !x.Suppressed), wd, aRotation: null, aDisplaySettings: dsp);
                }

                if (Assy.DesignFamily.IsBeamDesignFamily())
                        Draw_Beams(uppViews.Plan);
        
            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
            finally { Status = ""; DrawinginProgress = false; }
        }


        private void DDWG_Installation()
        {
            //^generates the md installation drawing

            dxfRectangle fitRec = null;

            double paperscale = 0;
            dxfVector v0;
            dxfVector v1;
            dxfVector v2;
            Drawing.BorderSize = uppBorderSizes.DSize_Landscape;

            int si = 0;

            colDXFEntities view1;
            colDXFEntities view2;
            colDXFEntities view3;
            colDXFEntities view4;
            dxoDrawingTool draw = Image.Draw;
            dxfRectangle rec1 = null;
            dxfRectangle rec2 = null;
            dxfRectangle rec3 = null;
            dxfRectangle rec4 = null;
            //dxeLine vcl_1;
            //dxeLine hcl_1 ;

            //dxeLine vcl_2;
            //dxeLine hcl_2;

            //dxeLine vcl_3 = null;
            //dxeLine hcl_3 = null;

            try
            {
                // functional note
                uopHoleArray dsHoles = Assy.DeckSections.GenHoles(Assy, bTrayWide: true);

                Status = "Sizing Border";
                {

                    // there will be two deck assembly plan views if Assy.HasAlternateDeckParts

                    double idealdia = !Assy.HasAlternateDeckParts ? 10 : 8;

                    //================== Border File ==================================
                    paperscale = Tool.BestShellScale(uppBorderSizes.DSize_Landscape, ShellRad * 2, idealdia, Drawing.DrawingUnits == uppUnitFamilies.Metric, out string sclrstr, bSuppressAnsiScales: true);
                    Tool.Border(this, null, ref fitRec, arPaperScale: ref paperscale, bSuppressAnsiScales: false, aScaleString: sclrstr);
                    //================== Border File ==================================
                    xCancelCheck();
                    v1 = new dxfVector(fitRec.Left + (3.5 * paperscale + ShellRad), fitRec.Top - ShellRad - 1.5 * paperscale);
                    v2 = new dxfVector(fitRec.Right - (3.5 * paperscale + ShellRad), v1.Y);
                    v0 = v1.Clone();


                    xCancelCheck();
                }


                Status = "Drawing Plan View 1";
                {
                    si = Image.Entities.Count + 1;
                    Image.UCS.SetCoordinates(v1.X, v1.Y);

                    Draw_Installation_PlanView(1, paperscale, out _, out _);
                    view1 = Image.Entities.SubSet(si, Image.Entities.Count);
                    rec1 = view1.BoundingRectangle();
                    xCancelCheck();
                }



                Status = "Drawing Plan View 2";
                {
                    si = Image.Entities.Count + 1;
                    Image.UCS.SetCoordinates(v2.X, v2.Y);
                    Draw_Installation_PlanView(2, paperscale, out _, out _);
                    view2 = Image.Entities.SubSet(si, Image.Entities.Count);
                    rec2 = view2.BoundingRectangle();
                    v1 = new dxfVector(rec2.Right, v1.Y);

                    v2 = new dxfVector(fitRec.Right - 0.75 * paperscale, v1.Y);
                    v1 = v2 - v1;
                    view2.Translate(v1);
                    rec2.Translate(v1);


                    xCancelCheck();
                }


                Status = "Drawing Plan View 3";
                {
                    if (Assy.HasAlternateDeckParts)
                    {
                        si = Image.Entities.Count + 1;
                        v2 = new dxfVector(rec1.Right + (rec2.Left - rec1.Right) / 2, v2.Y);

                        Image.UCS.SetCoordinates(v2.X, v2.Y);
                     
                        Draw_Installation_PlanView(3, paperscale, out _, out _);
                        if (DrawlayoutRectangles)
                        {
                            view3 = Image.Entities.SubSet(si, Image.Entities.Count);
                            rec3 = view3.BoundingRectangle();

                        }
                        xCancelCheck();
                    }
                }

                ////=========================================================== ELEVATION VIEW
                Status = "Drawing Elevation View";
                if (rec1 != null)
                {
                    si = Image.Entities.Count + 1;

                    v2 = rec1.BottomLeft;
                    v1 = fitRec.BottomLeft;
                    Image.UCS.X = v0.X;
                    Image.UCS.Y = v1.Y + (v2.Y - v1.Y) / 2 - 1.25 * paperscale;


                    Draw_View_Elevation(uppViews.InstallationElevation, out _);

                    view4 = Image.Entities.SubSet(si, Image.Entities.Count);
                    rec4 = view4.BoundingRectangle();
                    v1 = new dxfVector(v0.X, rec1.Bottom - 0.5 * paperscale);
                    v2 = new dxfVector(v0.X, rec4.Top);

                    if (v1.Y < v2.Y)
                    {
                        v1 -= v2;
                        view4.Translate(v1);
                        rec4.Translate(v1);
                    }


                }

                Image.UCS.Reset();

                //===================== BIG ASSEMBLY DETAIL =====================
                Status = "Insertng IsoMetric Detail";
                {
                    xCancelCheck();
                    Image.UCS.SetCoordinates(v0.X, v0.Y);
                    v1 = Assy.Downcomers.Item(1).Boxes.Last().EndSupport(bTop: false).CenterDXF;
                    string bname = Assy.DesignFamily.IsEcmdDesignFamily() ? "ECMD_ISO" : "MD_ISO";
                    Status = $"Inserting Block '{bname}'";
                    Tool.LoadBlock(this, aSource: "Miscellaneuos", aBlockSource: BlockSource, aBlockName: bname, aInsertionPt: v1, aInsertionScale: paperscale, aDisplaySettings: dxfDisplaySettings.Null(aLayer: "GEOMETRY"));

                }



            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
            finally
            {
                //Image.UCS.Reset();
                DrawinginProgress = false;
                Status = "";
                Drawing.ZoomExtents = false;
                Image.UCS.Reset();


                if (DrawlayoutRectangles)
                {

                    draw.aPolyline(rec1, new dxfDisplaySettings("0", dxxColors.LightCyan));
                    draw.aPolyline(rec2, new dxfDisplaySettings("0", dxxColors.LightCyan));
                    draw.aPolyline(rec3, new dxfDisplaySettings("0", dxxColors.LightCyan));
                    draw.aPolyline(rec4, new dxfDisplaySettings("0", dxxColors.LightCyan));
                }
            }

        }


        private void DDWG_ManwayClamp()
        {
            //^generates the Manway Clamp drawing

            //^creates the component drawing for the manway clamp

            dxfVector p1;
            dxfVector p2;
            dxfVector p3;
            dxfVector p4;
            dxfVector p5;

            hdwStud Stud;
            dxeHole Hole;
            Collection Notes;
            dxeLine l1;
            dxeLine l2;
            dxeLine lt;
            dxfVector tpt;
            dxePolygon PGon;
            colDXFEntities Segs;
            dxeLine dLine;
            dxeLine uline = null;
            dxeLine hLine1;

            double thk;
            double xscal;
            double w;
            double l;
            string txt;
            dxeArc ar1;
            double md;
            double ang1;
            colDXFVectors Pts;
            double T;
            double X1;
            double x2;
            double y1;
            int precwas;

            try
            {
                // initialize
                p1 = dxfVector.Zero;
                p2 = dxfVector.Zero;
                p3 = dxfVector.Zero;
                p4 = dxfVector.Zero;
                p5 = dxfVector.Zero;
                Notes = new Collection();
                lt = new dxeLine();

                MDProject.ManwayClipsClamps(out _, out uopManwayClamp Clamp);

                if (Clamp == null)
                {
                    return;
                }

                xscal = Image.Display.PaperScale;
                Image.LayerColorAndLineType_Set("MANWAY_CLAMPS", dxxColors.ByLayer, "ByLayer", true);
                precwas = Image.DimStyle().LinearPrecision;

                thk = Clamp.Material.Thickness;
                l = Clamp.Length;
                w = Clamp.Width;

                //================== TOP VIEW ==================================

                // this draw the part profile
                PGon = Clamp.Perimeter();
                Hole = Clamp.Hole.ToDXFHole;
                PGon.MoveFromTo(PGon.InsertionPt, p1, aYChange: -Hole.Inset);
                Image.Draw.aPolygon(PGon);

                Tool.DrawHoleOld(Image, Hole, out uline, out _, true, true, false, 1.5);

                Segs = PGon.Segments;

                // centerlines
                p2.X = -0.55 * l;
                p3.X = 0.55 * l;
                hLine1 = Image.Draw.aLine(p2, p3, aLineType: dxfLinetypes.Center);
                p2.SetCoordinates(0, -0.55 * w);
                p3.SetCoordinates(0, 0.55 * w);
                dLine = Image.Draw.aLine(p2, p3, aLineType: dxfLinetypes.Center);

                // dimension arcs
                ar1 = Segs.ArcItem(1);
                Image.Draw.aDim.RadialR(ar1, 45, ar1.Radius + 0.4 * xscal, aSuffix: TYP);

                p2.MoveTo(ar1.Center, 0, Image.DimStyle().CenterMarkSize * xscal); // the aChangeX is optional in VB unlike here, so I had to use 0
                Image.Draw.aDim.Horizontal(dLine.EndPt, p2, 0.5, aSuffix: TYP);

                ar1 = Segs.ArcItem(2);
                Image.Draw.aDim.RadialR(ar1, 115, ar1.Radius + 0.75 * xscal, aSuffix: TYP);

                // dimension hole
                p2.MoveTo(Hole.Perimeter().GetVertex(dxxPointFilters.GetBottomRight));
                p3.MoveTo(hLine1.StartPt);
                Image.DimStyle().LinearPrecision = 3;
                txt = $"{Image.FormatNumber(p3.Y - p2.Y)}%%p{Image.FormatNumber(0.002)}";
                Image.DimStyle().LinearPrecision = precwas;
                txt = dxfUtils.CADTextToScreenText(txt); // It was goDXFUtils
                Image.Draw.aDim.Vertical(p2, p3, 0.75, aOverideText: txt);

                p3.MoveTo(uline.StartPt); // It was "p3.MoveTo(uline)" in VB. The type does not match So, we used EndPt property.
                p2.SetCoordinates(p3.X, 0);
                Image.DimStyleOverrides.SuppressExtLine2 = true;
                Image.Draw.aDim.Vertical(p3, p2, -0.1);

                p3.MoveTo(Hole.Center, Hole.Radius);
                p2.MoveTo(Hole.Center, -Hole.Radius);

                Image.DimStyle().LinearPrecision = 3;
                txt = $"{Image.FormatNumber(Hole.Diameter)}%%p{Image.FormatNumber(0.002)}";
                Image.DimStyle().LinearPrecision = precwas;
                txt = dxfUtils.CADTextToScreenText(txt); // It was goDXFUtils
                Image.Draw.aDim.Horizontal(p2, p3, dLine.StartPt.Y - 0.15 * xscal, bAbsolutePlacement: true, aOverideText: txt);

                // length dim
                p3.SetCoordinates(-0.5 * l, 0);
                p2.SetCoordinates(0.5 * l, 0);
                Image.Draw.aDim.Horizontal(p2, p3, dLine.StartPt.Y - 0.35 * xscal, bAbsolutePlacement: true);

                // width dim
                p3.SetCoordinates(0, -0.5 * w);
                p2.SetCoordinates(0, 0.5 * w);
                Image.Draw.aDim.Vertical(p2, p3, -0.5 * l - 0.75 * xscal, bAbsolutePlacement: true);

                // grip height
                p2.SetCoordinates(-0.5 * l + Clamp.GripInset + 0.1, 0.5 * Clamp.GripLength);
                p3.MoveTo(p2, 0, -Clamp.GripLength); // the aChangeX is optional in VB unlike here, so I had to use 0
                Image.Draw.aDim.Vertical(p2, p3, -0.5 * l - 0.5 * xscal, bAbsolutePlacement: true);

                //================== SIDE VIEW ==================================
                PGon = Clamp.Elevation();
                p1.SetCoordinates(0, Image.Display.Extent_LL.Y - 0.75 * xscal);
                PGon.MoveFromTo(PGon.InsertionPt, p1);
                Image.Draw.aPolygon(PGon);

                Tool.DrawHoleOld(Image, Hole, out _, out uline, CenterlineScaleFactor: 4);

                Segs = PGon.Segments;


                // nib width
                p1.MoveTo(Segs.ArcItem(4).DefinitionPoint(dxxEntDefPointTypes.EndPt));
                p2.MoveTo(Segs.ArcItem(3).DefinitionPoint(dxxEntDefPointTypes.StartPt));
                Image.Draw.aDim.Horizontal(p1, p2, 0.7);

                // nib inset
                p1.MoveTo(Segs.ArcItem(4).EndPt);
                p2.MoveTo(PGon.Vertices.GetVector(dxxPointFilters.GetLeftTop));
                Image.DimStyleOverrides.SuppressExtLine1 = true;
                Image.Draw.aDim.Horizontal(p1, p2, 0.4);

                // nib ht
                p1.MoveTo(Segs.ArcItem(4).StartPt);
                Image.Draw.aDim.Vertical(p1, p2, -0.35);

                // thickness
                p1.MoveTo(PGon.Vertices.GetVector(dxxPointFilters.GetRightBottom));
                p2.MoveTo(PGon.Vertices.GetVector(dxxPointFilters.GetRightTop));

                Image.Draw.aDim.Vertical(p1, p2, 0.65, aOverideText: txt);

                //================== CLAMP NOTES ==================================
                p1.SetCoordinates(-0.5 * l, uline.StartPt.Y - 0.45 * xscal);
                tpt = p1.Clone();

                //========== STUD BEGINS ======================

                Stud = Clamp.Stud;

                p1.MoveTo(Image.Display.Extent_UR, 2 * xscal, -0.75 * xscal);

                Hole = new dxeHole(p1, Stud.Diameter, aRotation: 90, aMinorRadius: Stud.MinorDiameter / 2);

                Tool.DrawHoleOld(Image, Hole, out _, out _, CenterlineScaleFactor: 1.3, Polygon: PGon);

                p2.MoveTo(p1, -Hole.Radius);
                p3.MoveTo(PGon.GetVertex(dxxPointFilters.GetRightBottom));
                Image.DimStyle().LinearPrecision = 3;
                txt = $"{Image.FormatNumber(p3.X - p2.X)}%%p{Image.FormatNumber(0.002)}";
                Image.DimStyle().LinearPrecision = precwas;
                txt = dxfUtils.CADTextToScreenText(txt); // It was goDXFUtils
                Image.Draw.aDim.Horizontal(p2, p3, -0.3, 0, aOverideText: txt);

                p2.MoveTo(PGon.GetVertex(dxxPointFilters.GetRightTop), 0, -0.1); // the aChangeX is optional in VB unlike here, so I had to use 0
                w = dxfUtils.DistanceBetweenPoints(p1, p2); // It was goDXFUtils

                ang1 = dxfUtils.AngleBetweenPoints(p1, p2); // It was goDXFUtils and the ref type is not right
                Image.Draw.aArc(p1, w, ang1, 360 - ang1);

                p2 = dxfUtils.PolarVector(p1, 75, w / 2); // It was goDXFUtils
                p3.MoveTo(p2, 0.2 * xscal, 0.2 * xscal);

                if (Stud.Size == uppHardwareSizes.ThreeEights)
                {
                    txt = "3/8 UNC STUD (SHAVED)";
                }
                else
                {
                    txt = "M10 STUD (SHAVED)";
                }
                Image.Draw.aLeader.Text(p2, p3, txt, null); // The last argument is not optional here, unlike in VB code

                l = Stud.Length;
                w = Stud.Diameter; // it was "Stud.dia" in VB
                md = Stud.MinorDiameter;
                T = 0.1 * xscal;

                //============ LONG VIEW ===================

                p1.Y = Image.Entities.LastText().BoundingRectangle().Bottom - 0.3 * xscal;
                X1 = p1.X - 0.5 * w + 0.06 * xscal;
                x2 = X1 - T;
                y1 = p1.Y;

                T = Stud.Length / 51;

                Pts = new colDXFVectors();

                p2 = new dxfVector(X1, y1);
                Pts.Add(p2.Clone());

                for (int i = 1; i <= 25; i++)
                {
                    p3 = new dxfVector(x2, y1 - T);
                    p4 = new dxfVector(X1, y1 - 2 * T);

                    Pts.Add(p3.Clone());
                    Pts.Add(p4.Clone());

                    y1 -= 2 * T;
                    //DoEvents
                }
                p3 = new dxfVector(x2, y1 - T);
                Pts.Add(p3.Clone());

                Image.Draw.aPolyline(Pts, false);

                p4.MoveTo(p1, -w / 2 + md);
                p5 = new dxfVector(p4.X, p3.Y);
                l1 = Image.Draw.aLine(p2, p4);
                l2 = Image.Draw.aLine(p4, p5);

                // length dimension
                txt = Image.FormatNumber(Stud.Length);
                Image.Draw.aDim.Vertical(p4, p5, 0.6);

                Image.Draw.aLine(p5, p3);

                // thread lines
                for (int i = 2; i <= Pts.Count; i++)
                {
                    p2 = Pts.Item(i);
                    lt.StartPt = p2;
                    lt.EndPt = dxfUtils.PolarVector(p2, 8, 4 * w); // It was goDXFUtils

                    if (i > 2)
                    {
                        p3 = lt.IntersectPoint(l2);
                    }
                    else
                    {
                        p3 = lt.IntersectPoint(l1);
                    }
                    Image.Draw.aLine(p2, p3);
                    //DoEvents
                }

                // centerline
                p2.MoveTo(p1, 0, 0.1 * xscal); // the aChangeX is optional in VB unlike here, so I had to use 0
                p3 = new dxfVector(p2.X, Pts.Item(Pts.Count - 1).Y - 0.25 * xscal);
                dLine = Image.Draw.aLine(p2, p3, aLineType: dxfLinetypes.Center);

                //================= STUD NOTES =======================

                Notes = new Collection();
                p1 = new dxfVector(dLine.EndPt.X, tpt.Y);

                DrawinginProgress = false;
            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
        }

        private void DDWG_ManwayClip()
        {
            if (Assy == null)
            {
                return;
            }

            // ^creates the component drawing for the manway clip

            dxfVector v1;
            dxfVector v2;
            dxfVector v3;


            dxePolygon Perim;
            dxeLine l1;
            dxeLine l2;

            double thk;
            uopManwayClipWasher Washer;

            double iScale;
            dxfRectangle aRec;
            dxeArc aA;


            dxfRectangle fitRec = null;

            try
            {
                // initialize
                MDProject.ManwayClipsClamps(out uopManwayClip Clip, out _);
                if (Clip == null)
                {
                    return;
                }

                Washer = Clip.Washer;
                thk = Clip.Material.Thickness;

                iScale = 0.5;
                //=====================================================================================================
                // insert the border
                Tool.Border(this, null, ref fitRec, arPaperScale: ref iScale);
                //=====================================================================================================
                xCancelCheck();

                Image.DimStyle().TextFit = dxxDimTextFitTypes.MoveArrowsFirst;

                //================== TOP VIEW ==================================

                v1 = fitRec.TopLeft;
                v1.Move(2.25, -1);
                Perim = Clip.View_Plan(false, true, false, false, v1, 0, "GEOMETRY");
                Image.LinetypeLayers.ApplyTo(Perim, dxxLinetypeLayerFlag.ForceToLayer);
                Image.Entities.Add(Perim);

                aA = (dxeArc)Perim.AdditionalSegments.GetTagged("CLIP HOLE");

                aRec = Perim.BoundingRectangle();
                l1 = Image.Draw.aLine(aRec.Center, 0, 1.3 * aRec.Width, aLineType: dxfLinetypes.Center);

                
                l2 = Image.Draw.aLine(aA.Center, 90, 1.5 * aA.Diameter, aLineType: dxfLinetypes.Center);
                v1 = Perim.GetVertex(dxxPointFilters.GetTopLeft, bReturnClone: true);
                v2 = Perim.GetVertex(dxxPointFilters.GetBottomLeft, bReturnClone: true);
                v3 = Perim.GetVertex(dxxPointFilters.GetBottomRight, bReturnClone: true);

                Image.Screen.NumberVectors(Perim);
                Image.Draw.aDim.Vertical(v1, v2, l1.StartPt.X - 0.35 * iScale, bAbsolutePlacement: true, aImage: _Image);
                Image.Draw.aDim.Horizontal(v2, l2.StartPt, v2.Y - 0.35 * iScale, bAbsolutePlacement: true);

                Image.Draw.aDim.Horizontal(v2, v3, v2.Y - 0.65 * iScale, bAbsolutePlacement: true);
                Image.Draw.aDim.Vertical(v3, l1.EndPt, l1.EndPt.X + 0.35 * iScale, bAbsolutePlacement: true);

                v2 = aA.AnglePoint(45);
                v3.SetCoordinates(v2.X + 0.25 * iScale, v1.Y + 0.25 * iScale);
                Image.Draw.aLeader.Text(v2, v3, Tool.HoleLeader(Image, aA, 0));
                //return; // There is a "GoTo Done:" in VB code

                //Image.Draw.aDim.RadialD(aA, 45, 1 ,aSuffix: " HOLE");

                ////================== SIDE VIEW ==================================

                //v1.Y = aDim.ExtensionLine1.EndPt.Y - 3;
                //Perim = Clip.Elevation();
                //Perim.MoveTo(v1);
                //Image.Draw.aPolygon(Perim, aLayer: "GEOMETRY", aLineType: dxfLinetypes.Continuous);

                //Segs = Perim.Segments;
                //l1 = (dxeLine)Segs.Item(3);
                //l2 = (dxeLine)Segs.Item(5);
                //l3 = (dxeLine)Segs.Item(6);
                //aA = (dxeArc)Segs.Item(4);

                //v2 = Perim.Vertex(1, true);
                //aPl = (dxePolyline)Perim.FilletLines(0.035 * iScale).Item(3);
                //v3 = aPl.Vertex(2, true);

                //Image.Draw.aDim.Horizontal(v2, v3, v2.Y + 0.35, bAbsolutePlacement: true);
                //Image.Draw.aDim.Horizontal(v2, Perim.VertexLast, v2.Y + 0.15, bAbsolutePlacement: true);
                //Image.Draw.aDim.Vertical(v2, Perim.AdditionalSegments.Item(1).DefinitionPoint(dxxEntDefPointTypes.StartPt), -0.45);
                //Image.Draw.aDim.Vertical(l1.StartPt, v2, -0.9);

                //aPl.Closed = false;
                //Image.Draw.aPolyline(aPl, aPl.Closed, aLayer: Image.DimSettings.DimLayer);

                //aDim = Image.Draw.aDim.Aligned(v3, l3.EndPt, 0.5);

                //v2 = aA.MidPt.PolarVector(250, 0.5);

                //Image.Draw.aDim.Angular(l1, l2,aPlacementPointXY: v2);

                ////===================== End view

                //v1.X = aDim.ExtensionLine2.EndPt.X + 2.25;
                //Perim = Clip.Profile();
                //Perim.MoveTo(v1);
                //Image.Draw.aPolygon(Perim, aLayer: "GEOMETRY", aLineType: dxfLinetypes.Continuous);

                //aRec = Perim.BoundingRectangle();
                //allLines = null;
                //Image.Draw.aLinePolarMPT(aRec.Center, 90, 1.3 * aRec.Height, ref allLines, aLineType: dxfLinetypes.Center); // allLines is optional in VB
                //v2 = Perim.Vertex(1, true);
                //Image.Draw.aDim.Horizontal(v2, Perim.Vertex(3), v2.Y + 0.35 * iScale, bAbsolutePlacement: true);
                //aDim = Image.Draw.aDim.Horizontal(v2, Perim.VertexLast, v2.Y + 0.65 * iScale, bAbsolutePlacement: true);
                // return;

                // //================== CLIP NOTES ==================================
                // tpt = fitRec.BottomLeft;
                // tpt.Move(3, 1.5);
                // Image.Draw.aText(tpt, $"MANWAY CLAMP\\PPART NUMBER {Clip.PartNumber}\\PNUMBER REQUIRED:\\PFOR TRAYS:\\PFOR MATERIAL SEE SHEET 2", 0.1875, aTextStyle: "Standard");

                // v2 = fitRec.TopCenter;
                // v3 = fitRec.BottomCenter;
                // v3.Y = 0.9375;
                // Image.Draw.aLine(v2, v3, aLineType: dxfLinetypes.Phantom);

                // //================================================================
                // //   WASHER STARTS
                // //================================================================

                // v1 = v2.Moved(4, -2);
                // Perim = Washer.View_Plan(false); // In VB the argument is optional
                // Perim.MoveTo(v1);
                // Image.Draw.aPolygon(Perim, aLayer: "GEOMETRY", aLineType: dxfLinetypes.Continuous);

                // aRec = Perim.BoundingRectangle();
                // allLines = null;
                // l1 = Image.Draw.aLinePolarMPT(aRec.Center, 0, 1.2 * aRec.Width, ref allLines, aLineType: dxfLinetypes.Center); // allLines is optional in VB

                // v2 = Perim.Vertex(2, true);
                // v3 = Perim.Vertex(3, true);
                // Image.Draw.aDim.Horizontal(v2, l3.StartPt, -0.5);
                // aDim = Image.Draw.aDim.Horizontal(l3.StartPt, l2.StartPt, -0.5);
                // bDim = Image.Draw.aDim.Horizontal(v2, v3, aDim.TextPt.Y - 0.375 * iScale, bAbsolutePlacement: true);

                // aDim = Image.Draw.aDim.Vertical(v3, l1.EndPt, 0.35);
                // Image.Draw.aDim.Vertical(v3, Perim.Vertex(4), aDim.TextPt.X + 0.45 * iScale, bAbsolutePlacement: true);

                // v1.Y = bDim.TextPt.Y - 1.5;
                // Perim = Washer.Profile(); // in VB it was "Washer.Elevation" but it does not exist. The Profile was the closest thing.
                // Perim.MoveTo(v1);
                // Image.Draw.aPolygon(Perim, aLayer: "GEOMETRY", aLineType: dxfLinetypes.Continuous, bSuppressAddSegs: true); // It was "aPolygon(Perim, aLayer: "GEOMETRY", aLineType: dxfLinetypes.Continuous, , , True)" in VB. The last argument type is not right, so we used it for bSuppressAddSegs.

                // allLines = null;
                //// Image.Draw.aLinePolarMPT(Perim.Holes.Centers(), 90, 3 * Washer.Material.Thickness, ref allLines, aLineType: dxfLinetypes.Center); // allLines is optional in VB
                // Image.Draw.aDim.Vertical(Perim.Vertex(2), Perim.Vertex(3), 0.25);

                // tpt.X = v1.X;
                // Image.Draw.aText(tpt, $"MANWAY CLAMP WASHER\\PPART NUMBER {Washer.PartNumber}\\PNUMBER REQUIRED:\\PFOR TRAYS:\\PFOR MATERIAL SEE SHEET 2", 0.1875, aTextStyle: "Standard");
            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
        }



        private void DDWG_DeckSectionsOnly()
        {
            // ^generates the "Deck Sections Only" drawing
            // ~this drawing is not part of the package but may be useful
            // ~during the design phase
            if (Assy == null || Image == null || Drawing == null) return;

            dxoDrawingTool draw = Image.Draw;
            //if (DrawlayoutRectangles) Assy.Invalidate(uppPartTypes.DeckSections);

            string lname = "GEOMETRY";
            mzQuestions options = Drawing.Options;
            dxfDisplaySettings dsp = new(aLayer: lname);

        
            bool bothSide = options.AnswerB("Draw Both Sides?", true);
            bool bSuppressSlots = options.AnswerB("Suppress ECMD Slots?", false) || Assy.DesignFamily.IsMdDesignFamily();
            bool bBPInfo = options.AnswerB("Show Suppressed Bubble Promoters?", false);
            bool showangles = options.AnswerB("Show Splice Angles?", false);
            bool showpns = options.AnswerB("Show Part Numbers?", false);
            bool showquantities = options.AnswerB("Show Quantities?", false);
            bool regensections = options.AnswerB("Regenerate Deck Sections?", false);
 
            if (regensections) Assy.Invalidate(uppPartTypes.DeckSection);

            int viewcount = Assy.HasAlternateDeckParts ? 2 : 1;
            string trayname = Assy.TrayName(false);
         
            dxeLine vcl2 = null;
            dxeLine hcl2 = null;
            double dx = 0;
            dxfVector v1;
            try
            {
                if (!bSuppressSlots)
                    Image.Layers.Add("ECMD SLOTS", dxxColors.LightGrey);

                Image.Display.SetDisplayWindow(2.05 * ShellRad * viewcount, aDisplayHeight: 2.05 * ShellRad * viewcount, bSetFeatureScales: true);
                double paperscale = Image.Display.PaperScale;

                //draw the shell, ring and splice angles (optionally)
                Draw_Shells(uppViews.LayoutPlan, out dxeLine vcl1, out dxeLine hcl1);
                Draw_Rings(uppViews.LayoutPlan);
                if (showangles) Draw_SpliceAngles(uppViews.LayoutPlan, out _, bothSide);


                if (viewcount == 2)
                {
                    //draw the alternate ring shell, ring and splice angles (optionally)
                    dx = 2 * ShellRad + paperscale * 0.5;
                    Image.UCS.Move(dx);
                        Draw_Shells(uppViews.LayoutPlan, out vcl2, out hcl2);
                    Draw_Rings(uppViews.LayoutPlan);
                    
                    if (showangles)
                    {
                        Image.UCS.Rotate(90);
                        Draw_SpliceAngles(uppViews.LayoutPlan, out _, bothSide, aViewIndex: 2);
                        Image.UCS.Rotate(-90);
                    }
                    
                    Image.UCS.Move(-dx);
                }

                paperscale = Image.Display.ZoomExtents(bSetFeatureScale: true);
                dxfBlock downcomers = null;
                dxeInsert dcs = null;
                //draw 1 or 2 views of the deck section
                for (int view = 1; view <= viewcount; view++)
                {
                    //get the unique sections from the project 
                    //List<mdDeckSection> uniquesections = view == 2 ?  MDProject.GetParts().AltDeckSections(Assy.RangeGUID) : MDProject.GetParts().DeckSections(Assy.RangeGUID);
                    //double? rot = null;
                    if (view == 2)
                    {
                        Image.UCS.Move(dx);
                        Image.UCS.Rotate(90);
                        //rot = 90;
                    }
                    List<mdDeckSection> sections =view ==2 ? MDProject.GetParts().AltDeckSections(Assy.RangeGUID) : MDProject.GetParts().DeckSections(Assy.RangeGUID) ;
                    Draw_DeckSections(uppViews.LayoutPlan,bDrawBothSides: bothSide, bRegenPerims: false, bSuppressSlots: bSuppressSlots, bShowPns: showpns, aViewIndex: view, aSections:sections, bShowQuantities: showquantities);

                    //foreach(var splice in Assy.DeckSplices)
                    //{
                    //    if (!splice.Vertical)
                    //    {
                    //        draw.aLine(new dxfVector(splice.X - splice.Length / 2, splice.Y), new dxfVector(splice.X + splice.Length / 2, splice.Y));
                    //    }
                    //}
                    xCancelCheck();

                  
                    if (DrawlayoutRectangles)
                    {
                        
                        dsp = new dxfDisplaySettings(aLayer: "DOWNCOMERS");
                        Image.Layers.Add(dsp.LayerName, dxxColors.Blue, dxfLinetypes.Hidden);
                      
                        if (downcomers == null)
                        {
                            downcomers = mdBlocks.DowncomerBoxs_View_Plan(Image, Assy, aBlockName:dsp.LayerName,  bSetInstances: bothSide, bOutLineOnly: true, aLayerName: dsp.LayerName, aRotation: 0);
                            dcs = draw.aInsert(downcomers.Name, null, 0, bSuppressUCS: true);

                        }
                        else
                        {
                            dcs.Instances.Add(dx, 0, aRotation: 90);
                        }

                        
                    }
                Image.UCS.Reset();
              

                }

                if (bBPInfo)
                {

                    dsp = dxfDisplaySettings.Null(aLayer: "0", aColor: dxxColors.Red, aLinetype: dxfLinetypes.Continuous);
                    uopRectangles rectangles = Assy.DeckSections.SpliceLimits(Assy, 0.9 * mdGlobals.BPRadius);
                    foreach (uopRectangle rectangle in rectangles)
                    {
                        draw.aPolyline(rectangle.ToDXFRectangle(), dsp);
                    }

                    uopVectors ctrs = Assy.BPCenters(bRegen: DrawlayoutRectangles);

                    draw.aCircles(ctrs.GetBySuppressed(true), mdGlobals.BPRadius, dxfDisplaySettings.Null(aColor: dxxColors.Red));
                }

                v1 = viewcount == 1 ? vcl1.EndPoints().GetVector(dxxPointFilters.AtMinX).Moved(aYChange: -0.2 * paperscale) : vcl1.EndPoints().GetVector(dxxPointFilters.AtMinX).MidPoint(vcl2.EndPoints().GetVector(dxxPointFilters.AtMinX)).Moved(aYChange: -0.5 * paperscale);
                draw.aText(v1, $"{trayname.ToUpper()} DECK SECTIONS", aAlignment: dxxMTextAlignments.TopCenter);

                if (viewcount == 2)
                {
                    v1 = vcl1.EndPoints().GetVector(dxxPointFilters.AtMinX).Moved(aYChange: -0.2 * paperscale);

                    draw.aText(v1, $"TRAYS {Assy.RingRange.FirstAlternatingRange.SpanName.ToUpper()}", aAlignment: dxxMTextAlignments.TopCenter);
                    v1 = vcl2.EndPoints().GetVector(dxxPointFilters.AtMinX).Moved(aYChange: -0.2 * paperscale);

                    draw.aText(v1, $"TRAYS {Assy.RingRange.SecondAlternatingRange.SpanName.ToUpper()}", aAlignment: dxxMTextAlignments.TopCenter);


                }

            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
            finally { Status = ""; DrawinginProgress = false; Image.UCS.Reset(); }
        }

        private void DDWG_PlanView()
        {

            // ^generates the Tray Sketch drawing shown on the MD Image input screen.
            if (Assy == null) return;

            try
            {
                // initialize

                bool bSuppressSlots = Drawing.Options.AnswerB("Suppress ECMD Slots?", true)  || !MDRange.DesignFamily.IsEcmdDesignFamily();
                bool bDrawSpouts = !Drawing.Options.AnswerB("Suppress Spouts?", true);
                bool bBothSides = Drawing.Options.AnswerB("Draw Full Tray?", true) ;
                bool bCircleSUs = Drawing.Options.AnswerB("Circles On Startup Spouts?", true);
                bool dcsBelow = Drawing.Options.AnswerB("Draw Downcomers Below?", true);
                bool showpns = Drawing.Options.AnswerB("Show Part Numbers?", false);
                Image.Display.SetDisplayRectangle(Assy.Rectangle(), 1.1, true);

                Drawing.ZoomExtents = false;
                Image.Layers.Add("DOWNCOMERS");
                Image.Layers.Add("DECK", dxxColors.z_41);

                Draw_Shells(uppViews.LayoutPlan, out dxeLine VerCenterLn, out dxeLine HorCenterLn);
                xCancelCheck();

                Draw_Rings(uppViews.LayoutPlan);
                xCancelCheck();
                if (dcsBelow)
                {
                    Draw_DowncomersBelow(uppViews.LayoutPlan, bBothSides);
                    xCancelCheck();
                }

                //Draw_CrossBraces(uppViews.LayoutPlan, bBothSides);
                //xCancelCheck();
                Draw_SpliceAngles(uppViews.LayoutPlan, out _, bBothSides);
                xCancelCheck();
                Draw_DeckSections(uppViews.LayoutPlan, bBothSides, bSuppressSlots: bSuppressSlots, bShowPns: showpns);
                if (Assy.HasAntiPenetrationPans)
                    Draw_APPans(uppViews.Plan, bObscured: true, bDrawBothSides: bBothSides);
                xCancelCheck();

                Draw_Downcomers(uppViews.LayoutPlan, out _, bDrawSpouts, bBothSides, bDrawStartupCenterLines: true,bDrawFingerClips:true,bDrawStiffeners: true, bDrawEndAngles:true,bDrawBaffles:true);
                xCancelCheck();
                Draw_ManwayFasteners(uppViews.LayoutPlan, bBothSides);
                xCancelCheck();

                if (Assy.DesignFamily.IsBeamDesignFamily())
                    Draw_Beams(uppViews.Plan, bSuppressSupports: false);

                Draw_Washers(bBothSides);
                xCancelCheck();

                if (bCircleSUs)
                {
                    colDXFVectors aPts;

                    Status = "Drawing Startup Circles";
                    aPts = Assy.Downcomers.GenHoles(Assy, "STARTUP", bTrayWide: bBothSides).DXFCenters("STARTUP");

                    Image.Draw.aCircles(aPts, Assy.StartupLength / 2, new dxfDisplaySettings("STARTUP_CIRCLES", dxxColors.Orange, dxfLinetypes.Continuous));
                }

                Image.Display.ZoomExtents(bSetFeatureScale: true);
                dxfRectangle bounds = Image.Display.ExtentRectangle;
                bounds.Rescale(1.05);
                Image.Draw.aText(bounds.BottomCenter, MDRange.Name(true), aAlignment: dxxMTextAlignments.TopCenter);
                //if (DrawlayoutRectangles)
                //{
                //    List<mdDowncomer> dcs = Assy.Downcomers.GetByVirtual(aVirtualValue: bBothSides ? null: false);
                //    dxfDisplaySettings dsp = new dxfDisplaySettings(aLayer: "0", aColor: dxxColors.Blue, aLinetype: dxfLinetypes.Continuous);
                //    foreach (mdDowncomer item in dcs)
                //    {
                //        Image.Draw.aPolyline(item.Box.Vertices(false, item, true), true, aDisplaySettings: dsp);
                //        xCancelCheck();
                //    }

                //}
                DrawinginProgress = false;
            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
        }


        private void DDWG_RingClipSegments()
        {
            // ^this function Included for test purposes.
            // ~only available in the list of drawings on the drawing form when the app
            // ~is running in the VB design environment.



            try
            {
                DrawinginProgress = false; // shouldn't it be "true"?
                bool bothsides = Drawing.Options.AnswerB("Draw Both Sides?",false);
                bool showwashers = Drawing.Options.AnswerB("Show Washers?",true);
                bool regenshapes = Drawing.Options.AnswerB("Regenerate Section Shapes?", false);
                dxfDisplaySettings dsp1 = dxfDisplaySettings.Null("DECK_SHAPES", dxxColors.LightGrey);
                dxfDisplaySettings dsp2 = dxfDisplaySettings.Null("RING_CLIP_SEGMENTS", dxxColors.Blue);
                dxfDisplaySettings dsp3 = dxfDisplaySettings.Null("RING_CLIP_HOLES", dxxColors.Orange);
                dxfDisplaySettings dsp4 = dxfDisplaySettings.Null("RING_CLIP_BOUNDS", dxxColors.LightBlue);
                dxfDisplaySettings dsp5 = dxfDisplaySettings.Null("RING_CLIP_WASHERS", dxxColors.Orange);
                Image.Display.SetDisplayRectangle(Assy.Rectangle(), 1.1, true);
                Drawing.ZoomExtents = false;
                if (regenshapes) Assy.Invalidate(uppPartTypes.DeckSections);

                double paperscale = Image.Display.PaperScale;
                dxoDrawingTool draw = Image.Draw;

                Draw_Shells(uppViews.LayoutPlan);
                Draw_Rings(uppViews.LayoutPlan);

                if (Assy.DesignFamily.IsBeamDesignFamily()) Draw_Beams(uppViews.Plan);

                List<mdDowncomerBox> boxes = Assy.Downcomers.Boxes();
                List<uopRingClipSegment> rcsegments = null;
                Status = "Drawing Downcomers";
                uopHole rchole = mdGlobals.RingClipHole(Assy,false);
                uopHole dchole = mdGlobals.RingClipHole(Assy, true);
                foreach(var dc in boxes)
                {
                    dxfBlock block = mdBlocks.DowncomerBox_View_Plan(Image, dc, Assy, bSetInstances: bothsides, bSuppressHoles: true, bIncludeSpouts: false, bShowObscured: true, aLayerName: "DOWNCOMERS", bIncludeEndPlates: true, bIncludeEndSupports: true, bIncludeShelfAngles: true, bIncludeStiffeners: false, bIncludeBaffles: false, bIncludeSupDefs: false, bIncludeFingerClips: false, bIncludeEndAngles: true);
                    rcsegments =dc.RingClipSegments();
                    foreach (var seg in rcsegments)
                    {
                        if (seg.IsArc)
                        {
                            uopArc arc = seg.Arc.Moved(-dc.X, -dc.Y);
                            block.Entities.Add(new dxeArc(arc, aDisplaySettings: dxfDisplaySettings.Null(dsp2.LayerName, dxxColors.Purple)));
                
                        }
                            
                        else
                        {
                            uopLine line = seg.Line.Moved(-dc.X, -dc.Y);
                            dxeLine dline = (dxeLine)block.Entities.Add(new dxeLine(line, aDisplaySettings: dxfDisplaySettings.Null(dsp2.LayerName, dxxColors.Purple)));
                            if (showwashers) 
                            {
                                foreach (var p in line.Points)
                                {
                                    block.Entities.AddArc(p, dchole.Radius, aDisplaySettings: dline.DisplaySettings);
                                }
                            }
                            
                        }
                            

                        
                    }


                    draw.aInserts(block, block.Instances, bOverrideExisting: false);

                }
                //Draw_Downcomers(uppViews.LayoutPlan, out _,bDrawSpouts: false, bDrawBothSides: false, bSuppressHoles : true, aLayer: "0", bDrawFingerClips:false, bDrawStiffeners:false, bDrawEndAngles: true, bSuppressEndSupports: false);
                //Draw_SpliceAngles(uppViews.LayoutPlan, out _, bothsides);
                rchole = mdGlobals.RingClipHole( Assy,false);
    
                dxoEntityTool ents = Image.EntityTool;
                Image.Layers.Add("LABELS", dxxColors.Cyan);

                List<uopSectionShape> sections = Assy.DeckSections.BaseShapes(false, Assy, bGetUniques:true).FindAll(x => x.LapsDivider || x.LapsRing);
                foreach (var item in sections)
                {
                    dxoInstances insts = item.Instances.ToDXFInstances();
                    if (!bothsides) insts.Clear();
                    dxfBlock blok = new dxfBlock($"SECTION_{item.Handle.Replace(",", "_")}") { LayerName = dsp1.LayerName, Instances = insts};
                    dxePolyline simpleperim = new dxePolyline(item.SimplePerimeter) { DisplaySettings = dsp1 };
                    blok.Entities.Add(simpleperim);
//                    blok.Entities.AddShape(item, dsp1);
                    blok.Entities.Add(ents.Create_Text(item.Center, item.Handle,  aTextHeight:0.125 * paperscale,aAlignment: dxxMTextAlignments.MiddleCenter, aLayer: "LABELS"));
                    uopVectors rcpoints = item.RingClipCenters(out rcsegments, out uopShape segbounds);
                    int arccnt = 0;

                    if (item.Handle == "2,1")
                    {
                        draw.aShape(segbounds);
                        }

                    if (rcsegments != null && rcsegments.Count > 0)
                    {
                        blok.Entities.AddShape(segbounds, dsp4);

                        foreach (var seg in rcsegments)
                        {

                            if (seg.IsArc)
                            {
                                blok.Entities.Add(new dxeArc(seg.Arc, aDisplaySettings: dsp2));
                                arccnt++;
                                //uopVectors extpts = seg.Arc.PhantomPoints();
                                //draw.aCircles(extpts, 0.0125 * paperscale, dxfDisplaySettings.Null("PHANTOMS", dxxColors.Blue));

                            }

                            else
                                blok.Entities.Add(new dxeLine(seg.Line, aDisplaySettings: dsp2));

                        }
                        foreach (var pt in rcpoints)
                        {
                            blok.Entities.Add(new dxeArc(pt, rchole.Radius, aDisplaySettings: dsp3));
                            if (showwashers) blok.Entities.Add(new dxeArc(pt, mdGlobals.HoldDownWasherRadius, aDisplaySettings: dsp5));

                        }

                        
                    }
                    

                  if(arccnt == 1)
                    {
                        uopSectionShape relative = item.Y >= 0 ? item.SectionBelow : item.SectionAbove;
                        if (relative != null && (!item.IsHalfMoon && !relative.LapsRing) )
                            Image.Entities.Add(new dxePolyline(relative.SimplePerimeter) { DisplaySettings = dsp1 });

                    }
                    blok.Entities.Translate(-item.X, -item.Y);
                    draw.aInserts(blok, blok.Instances, false);

                }
          
                    
                

                dxfVector v1 = Image.Entities.BoundingRectangle().BottomCenter.Moved(aYChange: -0.75 * paperscale);
                draw.aText(v1, $"{Assy.TrayName(false)}\\PRing Clip Segments", aTextHeight: 0.25 * paperscale, aAlignment: dxxMTextAlignments.TopCenter);
            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = string.Empty; DrawinginProgress = false; }
        }

        private void DDWG_Notes()
        {
            // ^generates the Sheet 2 drawing.


            dxfVector p1;
            dxeText aTxt;
            dxfRectangle uRect = null;
            string txt = "";
            string ref1 = "";
            string ref2 = "";
            double iScale;
            dxfVector p2;
            double gp;

            try
            {
                gp = 0.2;
                dxoDrawingTool draw = Image.Draw;

                // initialize

                //================== Border ==================================
                iScale = 1;
                Tool.Border(this, null, ref uRect, arPaperScale: ref iScale, bSuppressAnsiScales: true, aScaleString: "NTS", aSheetNo: 3);
                xCancelCheck();
                Drawing.ZoomExtents = false;
                //================== Border ==================================
                Status = "Drawing Manufacturing Notes (Sheet 3)";

                p1 = uRect.TopLeft;
                p1.Move(0.35, -0.5);
                ref1 = uopGlobals.goGlobalVars().ValueS("ToleranceRefDocument", "XXXXX"); // It was "ref1 = goUtils.GlobalVars.ValueGet("ToleranceRefDocument", "XXXXX")" in VB

                if (MDProject.Customer.ForExport)
                {
                    ref2 = uopGlobals.goGlobalVars().ValueS("ExportPackingRefDocument", "XXXXX"); // It was "ref2 = goUtils.GlobalVars.ValueGet("ExportPackingRefDocument", "XXXXX")" in VB
                }
                else
                {
                    ref2 = uopGlobals.goGlobalVars().ValueS("DomesticPackingRefDocument", "XXXXX"); // It was "ref2 = goUtils.GlobalVars.ValueGet("DomesticPackingRefDocument", "XXXXX")" in VB
                }

                //====BUILD THE NOTES =======================

                //-------------- NOTE 1

                aTxt = draw.aText(p1, "\\LMANUFACTURING NOTES:", 0.1875, dxxMTextAlignments.TopLeft, aTextStyle: "Standard");
                p1 = aTxt.BoundingRectangle().BottomLeft;
                p1.Move(aYChange: -0.25);

                aTxt = draw.aText(p1, "1.", aAlignment: dxxMTextAlignments.TopLeft);
                p2 = p1.Moved(2 * aTxt.Width);

                txt = uopGlobals.goGlobalVars().ValueS("ManufacturingNote1", "XXXXX").ToUpper().Trim(); // It was "goUtils.GlobalVars.ValueGet("ManufacturingNote1", "XXXXX")" in VB
                txt = $"MD/ECMD {txt}";
                txt = txt.Replace("%%TOLERANCEREFDOCUMENT%%", ref1);

                aTxt = draw.aText(p2, txt, aAlignment: dxxMTextAlignments.TopLeft);

                //-------------- NOTE 2
                p1.Y = aTxt.BoundingRectangle().Bottom - gp;
                p2.Y = p1.Y;
                aTxt = draw.aText(p1, "2.", aAlignment: dxxMTextAlignments.TopLeft);
                txt = uopGlobals.goGlobalVars().ValueS("ManufacturingNote2", "XXXXX").ToUpper().Trim(); // It was "goUtils.GlobalVars.ValueGet("ManufacturingNote2", "XXXXX")" in VB
                aTxt = draw.aText(p2, txt, aAlignment: dxxMTextAlignments.TopLeft);

                //-------------- NOTE 3
                p1.Y = aTxt.BoundingRectangle().Bottom - gp;
                p2.Y = p1.Y;
                aTxt = draw.aText(p1, "3.", aAlignment: dxxMTextAlignments.TopLeft);
                txt = uopGlobals.goGlobalVars().ValueS("ManufacturingNote3", "XXXXX").ToUpper().Trim(); // It was "goUtils.GlobalVars.ValueGet("ManufacturingNote3", "XXXXX")" in VB
                txt = txt.Replace("%%BENDFACTOR%%", string.Format("{0:0.00}", uopGlobals.goGlobalVars().ValueS("BendFactor", "XXXXX"))); // It was "goUtils.GlobalVars.ValueGet("BendFactor", 1.7)" in VB
                aTxt = draw.aText(p2, txt, aAlignment: dxxMTextAlignments.TopLeft);

                //-------------- NOTE 4
                p1.Y = aTxt.BoundingRectangle().Bottom - gp;
                p2.Y = p1.Y;
                aTxt = draw.aText(p1, "4.", aAlignment: dxxMTextAlignments.TopLeft);

                txt = uopGlobals.goGlobalVars().ValueS("ManufacturingNote4", "XXXXX").ToUpper().Trim(); // It was "goUtils.GlobalVars.ValueGet("ManufacturingNote4", "XXXXX")" in VB
                if (Drawing.DrawingUnits == uppUnitFamilies.Metric)
                {
                    txt = txt.Replace("%%UNITS%%", "MILLIMETERS");
                }
                else
                {
                    txt = txt.Replace("%%UNITS%%", "INCHES");
                }

                aTxt = draw.aText(p2, txt, aAlignment: dxxMTextAlignments.TopLeft);

                //-------------- NOTE 5
                p1.Y = aTxt.BoundingRectangle().Bottom - gp;
                p2.Y = p1.Y;
                aTxt = draw.aText(p1, "5.", aAlignment: dxxMTextAlignments.TopLeft);
                txt = uopGlobals.goGlobalVars().ValueS("ManufacturingNote6", "XXXXX").ToUpper().Trim(); // It was "goUtils.GlobalVars.ValueGet("ManufacturingNote6", "XXXXX")" in VB. Also, shouldn't be "ManufacturingNote5"?
                aTxt = draw.aText(p2, txt, aAlignment: dxxMTextAlignments.TopLeft);

                //-------------- NOTE 6
                p1.Y = aTxt.BoundingRectangle().Bottom - gp;
                p2.Y = p1.Y;
                aTxt = draw.aText(p1, "6.", aAlignment: dxxMTextAlignments.TopLeft);
                txt = uopGlobals.goGlobalVars().ValueS("ManufacturingNote7", "XXXXX").ToUpper().Trim(); // It was "goUtils.GlobalVars.ValueGet("ManufacturingNote7", "XXXXX")" in VB. Also, shouldn't be "ManufacturingNote6"?

                if (MDProject.Customer.ForExport)
                {
                    txt = txt.Replace("%%SHIPTYPE%%", "EXPORT");
                }
                else
                {
                    txt = txt.Replace("%%SHIPTYPE%%", "DOMESTIC");
                }

                txt = txt.Replace("%%CRATEDOC%%", ref2);
                aTxt = draw.aText(p2, txt, aAlignment: dxxMTextAlignments.TopLeft);

                //-------------- NOTE 7
                txt = uopGlobals.goGlobalVars().ValueS("ManufacturingNote8", "XXXXX").ToUpper().Trim(); // It was "goUtils.GlobalVars.ValueGet("ManufacturingNote8", "XXXXX")" in VB. Also, shouldn't be "ManufacturingNote7"?
                if (!string.IsNullOrWhiteSpace(txt))
                {
                    p1.Y = aTxt.BoundingRectangle().Bottom - gp;
                    p2.Y = p1.Y;
                    aTxt = draw.aText(p1, "7.", aAlignment: dxxMTextAlignments.TopLeft);

                    aTxt = draw.aText(p2, txt, aAlignment: dxxMTextAlignments.TopLeft);
                }
            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
        }

        private void DDWG_MaterialList()
        {
            // ^generates the Sheet 2 drawing.

            throw new NotImplementedException("DDWG_MaterialList is not implemented yet.");

            //    dxfVector p1;
            //    dxeText aTxt;
            //    dxfRectangle uRect = null;
            //    uopTable uTab;
            //    dxeTable dTable;
            //    string txt = "";
            //    double iScale;

            //    try
            //    {
            //        // initialize


            //        //================== Border ==================================
            //        iScale = 1;
            //        Tool.Border(this, null, ref uRect, arPaperScale: ref iScale, bSuppressAnsiScales: true, aScaleString: "NTS", aSheetNo: 2);
            //        xCancelCheck();
            //        Drawing.ZoomExtents = false;

            //        //================== Border ==================================
            //        Status = "Material Specs. (Sheet 2)";

            //        p1 = uRect.TopLeft;
            //        p1.Move(0.35);

            //        txt = uopGlobals.goGlobalVars().ValueS("ManufacturingNote5", "XXXXX").ToUpper().Trim(); // It was "goUtils.GlobalVars.ValueGet("ManufacturingNote5", "XXXXX")" in VB
            //        aTxt = Image.Draw.aText(p1, txt, aAlignment: dxxMTextAlignments.TopLeft);

            //        //=====================  MATERIAL TABLE =================================

            //        p1.Y = aTxt.BoundingRectangle().Bottom - 0.1;

            //        uTab = mdUtils.MaterialTable(MDProject, Drawing.DrawingUnits);
            //        List<string> als = new();
            //        als.Add($"{dxxMTextAlignments.MiddleCenter}|{dxxMTextAlignments.MiddleCenter}|{dxxMTextAlignments.MiddleCenter}|{dxxMTextAlignments.MiddleCenter}"); // it was dxfAlign_MiddleCenter in VB. I am not sure about the type
            //        als.Add($"{dxxMTextAlignments.MiddleLeft}|{dxxMTextAlignments.MiddleCenter}|{dxxMTextAlignments.MiddleCenter}|{dxxMTextAlignments.MiddleLeft}"); // it was dxfAlign_MiddleCenter in VB. I am not sure about the type

            //        Image.TableSettings.HeaderTextScale = 1; //(0.1875 / 0.125)
            //        Image.TableSettings.HeaderTextStyle = "Standard";
            //        Image.TableSettings.TextStyleName = "Standard";
            //        Image.TableSettings.TextSize = 0.125;
            //        Image.TableSettings.TextGap = 0.05;
            //        Image.TableSettings.GridStyle = dxxTableGridStyles.All;
            //        Image.TableSettings.ColumnGap = 0.25;

            //        Image.TableSettings.HeaderRow = 1;

            //        List<string> rows = uTab.ToStrings(false, "All", aDelimitor: "|");


            //        dTable = Image.Draw.aTable("SpecTable", p1, rows, als, aDelimiter: "|");
            //    }
            //    catch (Exception e)
            //    {
            //        HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            //    }
            //}

            //private void DDWG_HardwareQuantities()
            //{
            //    // #1flag to request a return of the created DXF file
            //    // ^generates the Sheet 6 or 7 drawing.

            //    List<string> tbl1;
            //    List<string> als1;
            //    long tcnt;
            //    dxfVector p1;
            //    string txt;
            //    double iScale;
            //    dxfRectangle uRect = null;
            //    uopTable uTbl;

            //    dxeTable aTbl;

            //    try
            //    {
            //        // initialize
            //        tbl1 = new List<string>();
            //        als1 = new List<string>();

            //        p1 = dxfVector.Zero;
            //        Image.TableSettings.TitleTextStyle = "Standard";
            //        Image.TableSettings.TitleTextScale = 0.1875 / 0.125;
            //        Image.TableSettings.HeaderTextScale = 1; // (0.1875 / 0.125)
            //        Image.TableSettings.TextStyleName = "Standard";
            //        Image.TableSettings.TextSize = 0.125;
            //        Image.TableSettings.TextGap = 0.05;
            //        Image.TableSettings.GridStyle = dxxTableGridStyles.All;
            //        Image.TableSettings.ColumnGap = 0.25;
            //        Image.TableSettings.HeaderRow = 1;
            //        Image.TableSettings.TitleAlignment = dxxHorizontalJustifications.Center;
            //        Image.TableSettings.FooterAlignment = dxxHorizontalJustifications.Center;

            //        if (Assy == null)
            //        {
            //            var tempAssembly = MDProject.TrayRanges.SelectedRange.GetMDTrayAssembly(); // In VB it was SelectedRange.TrayAssembly
            //            tcnt = tempAssembly?.TrayCount ?? 0;
            //        }
            //        else
            //        {
            //            tcnt = Assy.TrayCount; // in VB it was Assy.Part.TrayCount
            //        }

            //        //================== Border ==================================
            //        iScale = 1;
            //        txt = Drawing.SheetNumber.ToString();

            //        Tool.Border(this, null, ref uRect, arPaperScale: ref iScale, bSuppressAnsiScales: true, aScaleString: "NTS", aSheetNo: txt);

            //        xCancelCheck();

            //        //================== Border ==================================
            //        Status = $"Drawing {Drawing.SelectText}";

            //        p1 = uRect.TopLeft;
            //        p1.Move(0.25, -0.25);

            //        //==============================================
            //        uTbl = MDProject.GetTable("HARDWARE");

            //        als1.Add($"{dxxMTextAlignments.MiddleCenter}|{dxxMTextAlignments.MiddleCenter}|{dxxMTextAlignments.MiddleCenter}"); // it was dxfAlign_MiddleCenter in VB. I am not sure about the type
            //        tbl1.AddRange(uTbl.ToStrings(false, "All", aDelimitor: "|"));

            //        tbl1.Add("||");
            //        tbl1.Add("||");
            //        tbl1.Add("||");

            //        aTbl = (dxeTable)Image.Draw.aTable("HARDWARE", p1, tbl1, als1, aTableAlign: dxxRectangularAlignments.TopLeft, aTitle: "\\LHARDWARE LIST", aFooter: "INCLUDES 5% SPARES (2 MINIMUM)");

            //        DrawinginProgress = false;
            //    }
            //    catch (Exception e)
            //    {
            //        HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            //    }
        }

        private void DDWG_StandardDetails()
        {
            // ^generates the Sheet 4 drawing.

            dxfAttributes attVals;
            string bname;
            dxfRectangle aRect = null;
            dxfVector p1;
            colUOPTrayRanges aRngs;
            mdTrayAssembly aAssy;
            dxxDrawingUnits dunits;
            int cnt_halfC = 0;
            int cnt_fullC = 0;
            int cnt_BP = 0;
            int cnt_Tabs = 0;
            int cnt_Slots = 0;
            string punch_Block;
            dxfBlock blk = null;
            dxeInsert insert = null;
            try
            {
                aRngs = MDProject.TrayRanges;
                attVals = new dxfAttributes();
                p1 = dxfVector.Zero;
                punch_Block = "PUNCH_EITHER";
                dxoDrawingTool draw = Image.Draw;

                for (int i = 1; i <= aRngs.Count; i++)
                {
                    aAssy = aRngs.Item(i).GetMDTrayAssembly();
                    if (aAssy.DesignFamily.IsEcmdDesignFamily())
                    {
                        cnt_Slots++;
                        if (aAssy.Deck.SlotType == uppFlowSlotTypes.HalfC)
                            cnt_halfC++;
                        else
                            cnt_fullC++;
                    }
                    if (aAssy.DesignOptions.HasBubblePromoters)
                        cnt_BP++;
                    if (aAssy.DesignOptions.SpliceStyle == uppSpliceStyles.Tabs)
                        cnt_Tabs++;

                    if (i == 1)
                    {
                        if (aAssy.Deck.PunchDirection == uppPunchDirections.FromAbove)
                            punch_Block = "PUNCH_DOWN";
                        else if (aAssy.Deck.PunchDirection == uppPunchDirections.FromBelow)
                            punch_Block = "PUNCH_UP";
                        else
                            punch_Block = "PUNCH_EITHER";
                    }
                }

                p1.SetCoordinates(-0.5313, -0.4687);

                //================== Border ==================================
                double paperscale = 1;
                Tool.Border(this, null, ref aRect, arPaperScale: ref paperscale, bSuppressAnsiScales: true, aScaleString: "NTS", aSheetNo: 4);
                xCancelCheck();

                //================== Border ==================================
                Status = "Drawing Standard Deck Details and Views (Sheet 4)";

                dunits = Drawing.DrawingUnitsDXF;

                Image.Layers.Add(Image.DimSettings.DimLayer, Image.DimSettings.DimLayerColor);

                //=====================================================
                bname = "PERF_PATTERN";
                blk = Tool.LoadBlock(this, "mfgDetails", BlockSource, bname);
                if (blk != null)
                {
                    attVals = new dxfAttributes();
                    if (dunits == dxxDrawingUnits.English)
                    {
                        attVals.AddTagValue("WTOX1", ".8");
                        attVals.AddTagValue("WTOX2", "1.25");
                        attVals.AddTagValue("WTOY1", ".8");
                        attVals.AddTagValue("WTOY2", "1.25");
                        attVals.AddTagValue("XTOY1", ".8");
                        attVals.AddTagValue("XTOY2", "1.25");
                    }
                    else
                    {
                        attVals.AddTagValue("WTOX1", "20.3");
                        attVals.AddTagValue("WTOX2", "31.8");
                        attVals.AddTagValue("WTOY1", "20.3");
                        attVals.AddTagValue("WTOY2", "31.8");
                        attVals.AddTagValue("XTOY1", "20.3");
                        attVals.AddTagValue("XTOY2", "31.8");
                    }
                    //SetAttributePlaceHolders( attVals, bname );
                    Status = $"Drawing Block '{blk.Name}'";
                    insert = draw.aInsert(blk.Name, p1, aDisplaySettings: dxfDisplaySettings.Null("0"), aAttributeVals: attVals);
                }

                //=====================================================
                bname = "HALF_C";
                blk = Tool.LoadBlock(this, "mfgDetails", BlockSource, bname);
                if (blk != null)
                {
                    attVals = new dxfAttributes();

                    if (dunits == dxxDrawingUnits.English)
                    {
                        attVals.AddTagValue("WD1", ".375");
                        attVals.AddTagValue("TOL1", ".040");
                        attVals.AddTagValue("TOL2", ".046");
                        attVals.AddTagValue("UNI1", "inch*");
                    }
                    else
                    {
                        attVals.AddTagValue("WD1", "9.5");
                        attVals.AddTagValue("TOL1", "1.02");
                        attVals.AddTagValue("TOL2", "1.17");
                        attVals.AddTagValue("UNI1", "mm*");
                    }
                    //SetAttributePlaceHolders( attVals, bname );
                    aRect = draw.aInsert(bname, p1, 0, aAttributeVals: attVals).BoundingRectangle();
                    if (cnt_halfC <= 0)
                    {
                        double lx = 5.74;
                        double ty = 10.00;
                        double rx = 10.27;
                        double by = 7.37;
                        draw.aLine(new dxfVector(lx, by), new dxfVector(rx, ty), "0");
                        draw.aLine(new dxfVector(lx, ty), new dxfVector(rx, by), "0");
                    }
                }

                //=====================================================
                bname = "FULL_C";
                blk = Tool.LoadBlock(this, "mfgDetails", BlockSource, bname);
                if (blk != null)
                {
                    attVals = new dxfAttributes();

                    if (dunits == dxxDrawingUnits.English)
                    {
                        attVals.AddTagValue("WD1", ".375");
                        attVals.AddTagValue("TOL1", ".083");
                        attVals.AddTagValue("TOL2", ".089");
                        attVals.AddTagValue("UNI1", "inch*");
                    }
                    else
                    {
                        attVals.AddTagValue("WD1", "9.5");
                        attVals.AddTagValue("TOL1", "2.11");
                        attVals.AddTagValue("TOL2", "2.26");
                        attVals.AddTagValue("UNI1", "mm*");
                    }
                    //SetAttributePlaceHolders( attVals, bname );
                    aRect = draw.aInsert(bname, p1, aAttributeVals: attVals).BoundingRectangle();
                    if (cnt_fullC <= 0)
                    {
                        double lx = 10.27;
                        double ty = 10.00;
                        double rx = 14.49;
                        double by = 7.37;
                        draw.aLine(new dxfVector(lx, by), new dxfVector(rx, ty), "0");
                        draw.aLine(new dxfVector(lx, ty), new dxfVector(rx, by), "0");
                    }
                }

                //=====================================================
                bname = "DECK_TOLERANCES";
                blk = Tool.LoadBlock(this, "mfgDetails", BlockSource, bname);
                if (blk != null)
                {
                    attVals = new dxfAttributes();

                    if (dunits == dxxDrawingUnits.English)
                    {
                        attVals.AddTagValue("TOL1", ".06");
                        attVals.AddTagValue("TOL2", ".03");
                        attVals.AddTagValue("TOL3", ".25''");
                        attVals.AddTagValue("TOL4", ".37''");
                        attVals.AddTagValue("TOL5", ".06''");
                    }
                    else
                    {
                        attVals.AddTagValue("TOL1", "1.5");
                        attVals.AddTagValue("TOL2", ".8");
                        attVals.AddTagValue("TOL3", "6.4");
                        attVals.AddTagValue("TOL4", "9.4");
                        attVals.AddTagValue("TOL5", "1.5 mm");
                    }
                    //SetAttributePlaceHolders( attVals, bname );
                    Status = $"Drawing Block '{bname}'";
                    draw.aInsert(bname, p1, aDisplaySettings: dxfDisplaySettings.Null(aLayer: "0"), aAttributeVals: attVals);
                }

                //=====================================================
                bname = "BUBBLE_PROMOTER";
                blk = Tool.LoadBlock(this, "mfgDetails", BlockSource, bname);
                if (blk != null)
                {
                    attVals = new dxfAttributes();

                    if (dunits == dxxDrawingUnits.English)
                    {
                        attVals.AddTagValue("DIM1", mdGlobals.gsMDBubblePromoterDiameter.ToString("#0.00"));
                        attVals.AddTagValue("DIM2", ".5");
                        attVals.AddTagValue("DIM3", "1.875");
                    }
                    else
                    {
                        attVals.AddTagValue("DIM1", "64.8");
                        attVals.AddTagValue("DIM2", "12.7");
                        attVals.AddTagValue("DIM3", "47.6");
                    }
                    //SetAttributePlaceHolders( attVals, bname );
                    aRect = draw.aInsert(bname, p1, aDisplaySettings: dxfDisplaySettings.Null(aLayer: "0"), aAttributeVals: attVals).BoundingRectangle();
                    if (cnt_BP <= 0)
                    {
                        double lx = 5.74;
                        double ty = 7.37;
                        double rx = 10.27;
                        double by = 5.10;
                        draw.aLine(new dxfVector(lx, by), new dxfVector(rx, ty), "0");
                        draw.aLine(new dxfVector(lx, ty), new dxfVector(rx, by), "0");
                    }
                }

                //=====================================================
                bname = "DECK_NOTCH";
                blk = Tool.LoadBlock(this, "mfgDetails", BlockSource, bname);
                if (blk != null)
                {
                    attVals = new dxfAttributes();

                    if (dunits == dxxDrawingUnits.English)
                    {
                        attVals.AddTagValue("DIM1", "1.5");
                        attVals.AddTagValue("DIM2", "3.0");
                        attVals.AddTagValue("DIM3", ".75");
                        attVals.AddTagValue("RAD1", ".375");
                    }
                    else
                    {
                        attVals.AddTagValue("DIM1", "38.1");
                        attVals.AddTagValue("DIM2", "76.2");
                        attVals.AddTagValue("DIM3", "19.0");
                        attVals.AddTagValue("RAD1", "9.5");
                    }
                    //SetAttributePlaceHolders( attVals, bname );
                    aRect = draw.aInsert(bname, p1, aDisplaySettings: dxfDisplaySettings.Null(aLayer: "0"), aAttributeVals: attVals).BoundingRectangle();
                    if (Project.InstallationType == uppInstallationTypes.GrassRoots)
                    {
                        double lx = 10.27;
                        double ty = 7.37;
                        double rx = 14.49;
                        double by = 5.10;
                        draw.aLine(new dxfVector(lx, by), new dxfVector(rx, ty), "0");
                        draw.aLine(new dxfVector(lx, ty), new dxfVector(rx, by), "0");
                    }
                }

                //=====================================================
                bname = punch_Block;
                blk = Tool.LoadBlock(this, "mfgDetails", BlockSource, bname);
                if (blk != null)
                {
                    Status = $"Drawing Block '{bname}'";
                    draw.aInsert(bname, p1, aDisplaySettings: dxfDisplaySettings.Null("0"));
                }

                //=====================================================
                bname = "SLOTTING";
                blk = Tool.LoadBlock(this, "mfgDetails", BlockSource, bname);
                if (blk != null)
                {
                    aRect = draw.aInsert(bname, p1, aDisplaySettings: dxfDisplaySettings.Null("0")).BoundingRectangle();
                    if (cnt_Slots <= 0) // in VB this is just cnt_Tabs, I don't know what is the correct condition
                    {
                        double lx = 10.27;
                        double ty = 5.10;
                        double rx = 14.49;
                        double by = 3.20;
                        draw.aLine(new dxfVector(lx, by), new dxfVector(rx, ty), "0");
                        draw.aLine(new dxfVector(lx, ty), new dxfVector(rx, by), "0");
                    }
                }

                p1.SetCoordinates(11.947, 3);
                draw.aText(p1, $"TRAY DECK\nMANUFACTURING DETAILS", 0.1875, dxxMTextAlignments.TopLeft, aTextStyle: "Standard");

                DrawinginProgress = false;
            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
        }

        private void DDWG_SpoutPatterns()
        {
            // ^generates the Sheet 5 drawing.

            dxfAttributes attVals;
            string bname;
            dxfRectangle aRect = null;
            double iScale;
            dxfVector v1;

            try
            {
                v1 = dxfVector.Zero;
                bname = "SPOUT_PATTERN";
                attVals = new dxfAttributes();
                dxfBlock blk = null;
                //================== Border ==================================
                iScale = 1;
                Tool.Border(this, null, ref aRect, arPaperScale: ref iScale, bSuppressAnsiScales: true, aScaleString: "NTS", aSheetNo: 5);
                xCancelCheck();
                //================== Border ==================================
                Status = "Drawing Standard Spout Patterns (Sheet 5)";

                v1.SetCoordinates(-0.5313, -0.4687);

                blk = Tool.LoadBlock(this, "SpoutPatterns", BlockSource, bname);
                if (blk != null)
                {
                    if (Drawing.DrawingUnits == uppUnitFamilies.English)
                    {
                        attVals.AddTagValue("PITCH1", ".9375''");
                        attVals.AddTagValue("PITCH2", ".8125''");
                        attVals.AddTagValue("PITCH3", "1.125''");
                        attVals.AddTagValue("PITCH4", "1.75''");
                        attVals.AddTagValue("DIM1", ".2344''");
                        attVals.AddTagValue("CLEARANCE", ".25''");
                        attVals.AddTagValue("GAP", ".25''");
                        attVals.AddTagValue("SPTDIA", "%%C.75''");
                    }
                    else
                    {
                        attVals.AddTagValue("PITCH1", "23.8");
                        attVals.AddTagValue("PITCH2", "20.6");
                        attVals.AddTagValue("PITCH3", "28.6");
                        attVals.AddTagValue("PITCH4", "44.5");
                        attVals.AddTagValue("DIM1", "6");
                        attVals.AddTagValue("CLEARANCE", "6.4");
                        attVals.AddTagValue("GAP", "6.4");
                        attVals.AddTagValue("SPTDIA", "%%C19");
                    }
                    Status = $"Drawing Block '{bname}'";
                    Image.Draw.aInsert(bname, v1, 0, aDisplaySettings: dxfDisplaySettings.Null("0"), aAttributeVals: attVals);

                    Status = "";
                }

                DrawinginProgress = false;
            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
        }

        private void DDWG_SlotAndTabDetails()
        {
            // ^generates the Sheet 6 drawing.

            dxfAttributes attVals;
            string bname;
            dxfRectangle aRect = null;
            double iScale;
            dxfVector v1;

            try
            {
                v1 = dxfVector.Zero;

                attVals = new dxfAttributes();
                //================== Border ==================================
                iScale = 1;
                Tool.Border(this, null, ref aRect, arPaperScale: ref iScale, bSuppressAnsiScales: true, aScaleString: "NTS", aSheetNo: 6);
                xCancelCheck();
                //================== Border ==================================
                Status = "Drawing Slot And Tab Deck Manufacturing Details (Sheet 6)";

                if (Drawing.DrawingUnits == uppUnitFamilies.Metric)
                {
                    bname = "DETAILS_METRIC";
                    v1.SetCoordinates(0, 3.1619);
                }
                else
                {
                    bname = "DETAILS_ENGLISH";
                    v1.SetCoordinates(0, 2.706);
                }

                dxfBlock blk = Tool.LoadBlock(this, "SlotAndTabDetails", BlockSource, bname);
                if (blk != null)
                {
                    attVals.AddTagValue("JOGGLE", "SEE\\PDRAWING");

                    Status = $"Drawing Block '{bname}'";
                    Image.Draw.aInsert(bname, v1, 0, aDisplaySettings: dxfDisplaySettings.Null("0"), aAttributeVals: attVals);
                }

                DrawinginProgress = false;
            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
        }

        private void DDWG_DCAssembly()
        {
            // ^generates the Sheet 7 or 8 drawing.

            string bname1;
            string bname2;
            dxfRectangle aRect = null;
            dxfRectangle bRec;
            dxfVector org;
            dxfVector v1;
            dxfVector v2;
            dxfVector v3;
            string txt;
            List<string> aTbl = new();
            mdDowncomer aDC;
            List<mdDowncomer> aDCs = MDProject.TrayRanges.Downcomers();
            string gstlst = "";
            double f1 = 0;
            string fmat = "";
            dxeLine l1;
            bool bECMD = false;
            bool bBoltOns = false;


            try
            {

                txt = "PART NO.\\PDOWNCOMER";
                if (Drawing.DrawingUnits == uppUnitFamilies.English)
                {
                    f1 = 1;
                    fmat = "{0:0.000}";
                    mzUtils.ListAdd(ref txt, "LENGTH\\POVERALL (IN.)", aDelimitor: "|");
                }

                mzUtils.ListAdd(ref txt, "REQ.\\PQTY.", aDelimitor: "|");
                aTbl.Add(txt);

                for (int i = 1; i <= aDCs.Count; i++)
                {
                    aDC = aDCs[i - 1];

                    txt = aDC.PartNumber;
                    if (aDC.GussetedEndplates)
                    {
                        mzUtils.ListAdd(ref gstlst, txt); // it was "goUtils.AddToList gstlst, txt" in VB but I could not find it here
                    }

                    txt = $"{txt}|{string.Format(fmat, aDC.AssemblyLength() * f1)}"; // the fmat and f1 don't get initialize properly in all scenarios. I just initialized them to their default values
                    txt = $"{txt}\\\\{string.Format(fmat, aDC.AssemblyLength(null, true) * f1)}";
                    txt = $"{txt}|{aDC.OccuranceFactor * aDC.TrayCount}"; // in VB it was aDC.Part.TrayCount
                    aTbl.Add(txt);

                    int designFamilyPropertyValue = (int)aDC.PropValGet("DesignFamily", out _, aPartType: uppPartTypes.TrayRange);
                    if (Enum.IsDefined(typeof(uppMDDesigns), designFamilyPropertyValue))
                    {
                        uppMDDesigns designFamily = (uppMDDesigns)designFamilyPropertyValue;
                        if (designFamily.IsEcmdDesignFamily())
                        {
                            bECMD = true;
                        }
                    }

                    if (aDC.BoltOnEndplates)
                    {
                        bBoltOns = true;
                    }
                }

                //================== Border ==================================
                double paperscale = 1;
                Tool.Border(this, null, ref aRect, arPaperScale: ref paperscale, bSuppressAnsiScales: true, aScaleString: "NTS");
                xCancelCheck();
                //================== Border ==================================
                Status = $"Drawing {Drawing.SelectText}";
                v1 = aRect.TopLeft;
                v1.Move(0.25, -0.125);

                txt = uopGlobals.goGlobalVars().ValueS("WeldingRefDoc", "XXXXX"); // It was "goUtils.GlobalVars.ValueGet("WeldingRefDoc", "XXXXX")" in VB

                txt = $"\\LNOTE:\\l ASSEMBLY TO BE WELDED PER UOP DWG. {txt}.";
                Image.Draw.aText(v1, txt, 0.1875, dxxMTextAlignments.TopLeft, aTextStyle: "Standard");
                txt = "";

                if (bECMD)
                {
                    v1.Move(-0.125, -0.375);
                    txt = "\\LBOLT STIFFENER IN PLACE - DO NOT WELD??\\l";
                    if (_MDProject.Bolting == uppUnitFamilies.English)
                    {
                        txt = $"{txt}\\PWITH 3/8 BOLT, NUT AND LOCKWASHER";
                    }
                    else
                    {
                        txt = $"{txt}\\PWITH 10mm BOLT, NUT AND LOCKWASHER";
                    }
                    txt = $"{txt}\\P1000 SERIES ONLY ??";
                }

                if (bBoltOns)
                {
                    if (!string.IsNullOrWhiteSpace(txt))
                    {
                        txt = $"{txt}\\P\\P";
                    }
                    txt = $"{txt}?? BOLT ON END PLATE ??";
                }

                if (!string.IsNullOrWhiteSpace(txt))
                {
                    Image.Draw.aText(v1, txt, 0.125, dxxMTextAlignments.TopLeft, null, "RomanC", "0", dxxColors.Red);
                }

                v1 = aRect.TopCenter;
                v1.Move(-0.5, -2.25);
                org = v1.Clone();

                if (bECMD)
                {
                    bname1 = "PLANVIEW_ECMD2";
                    bname2 = "ELEVVIEW_ECMD";
                }
                else
                {
                    bname1 = "PLANVIEW_MD2";
                    bname2 = "ELEVVIEW_MD";
                }
                Image.TableSettings.ColumnGap = 0.02;
                Image.TableSettings.TextGap = 0.02;
                Image.SelectionSetInit(false);
                dxfBlock blk = Tool.LoadBlock(this, "DCASSEMBLY", BlockSource, bname1);
                if (blk != null)
                {
                    Status = $"Drawing Block '{bname1}'";
                    Image.Draw.aInsert(bname1, org, 0, aDisplaySettings: dxfDisplaySettings.Null("GEOMETRY"));

                    Status = "";
                }

                v2 = v1.Moved(3.3657, -0.1782);
                v3 = v2.Moved(0.75, 0.5);
                txt = "\\W0.8;END SUPPORT\\PNote opposite hand when end is not square.";
                Image.Draw.aLeader.Text(v2, v3, txt, null); // in VB the aMidXY is optional but here is not, so I used null for it

                v2 = v1.Moved(1.8125, -0.5438);
                v3 = v2.Moved(0.3, -0.5);
                txt = "\\W0.8;SUPPORT ANGLE\\P";
                txt = $"{txt}Longer angle on long side of assembly (where applicable).\\P";
                if (Drawing.DrawingUnits == uppUnitFamilies.Metric)
                {
                    txt = $"{txt}3mm x 25mm x 25mm structural angle (ASTM A-36) may be\\P";
                }
                else
                {
                    txt = $"{txt}1/8 x 1.0 x 1.0 structural angle (ASTM A-36) may be\\P";
                }
                txt = $"{txt}substituted for carbon steel sheet (ASTM A-1011 Gr. B - non killed).";
                Image.Draw.aLeader.Text(v2, v3, txt, null); // in VB the aMidXY is optional but here is not, so I used null for it

                txt = "\\W0.8;STIFFENERS at each hole pair in downcomer body, except end slot pairs.\\P";
                txt = $"{txt}Line up holes in body and stiffener. Top of stiffener to be flush with top of body.";
                if (bECMD)
                {
                    v2 = v1.Moved(-1.4529, -0.25);
                    v3 = v2.Moved(0.4917, -1.6636);
                }
                else
                {
                    v2 = v1.Moved(-1.463, -0.0909);
                    v3 = v2.Moved(0.6248, -1.806);
                    txt = $"{txt}\\PStiffeners should face as shown. ECMD central flange to be on long side (where applicable).";
                }

                Image.Draw.aLeader.Text(v2, v3, txt, null); // in VB the aMidXY is optional but here is not, so I used null for it
                bRec = Image.SelectionSetInit(true).BoundingRectangle();

                org.Y = bRec.Bottom - 0.5;
                if (bECMD)
                {
                    org.Move(aYChange: -0.5);
                }
                blk = Tool.LoadBlock(this, "DCASSEMBLY", BlockSource, bname2);
                if (blk != null)
                {
                    Status = $"Drawing Block '{bname2}'";
                    Image.Draw.aInsert(bname2, org, 0, aDisplaySettings: dxfDisplaySettings.Null("GEOMETRY"));

                    Status = "";

                    v1 = org.Moved(-2.25);
                    v2 = org.Moved(-2.1875);
                    txt = Drawing.DrawingUnits == uppUnitFamilies.English ? "1/8 INCH GAP" : "3mm GAP";


                    Image.Draw.aDim.Horizontal(v2, v1, 0.365, 0, aOverideText: txt);

                    v1 = org.Moved(-1.6875, -1.25);
                    v2 = v1.Moved(0.5, -0.25);
                    txt = "DOWNCOMER BODY";
                    Image.Draw.aLeader.Text(v1, v2, txt, null); // in VB the aMidXY is optional but here is not, so I used null for it

                    if (!string.IsNullOrWhiteSpace(gstlst))
                    {
                        v1 = org.Moved(-3.25, -0.25);
                        v2 = org.Moved(-2.3125, -1.1875);
                        Image.Draw.aLine(v1, v2, aLayer: "GEOMETRY");

                        v1 = org.Moved(3.25, -0.25);
                        v2 = org.Moved(2.3125, -1.1875);
                        l1 = Image.Draw.aLine(v1, v2, "GEOMETRY");

                        v1 = l1.Point(10, bPercentPassed: true); // In VB it was ".LineVector" which does not exist here. The closest thing I could find was ".Point"
                        v2 = v1.Moved(0.355, -0.25);
                        txt = "END PLATE";
                        txt = $"{txt}\\PGUSSETS FOR FOR P/N(S): {gstlst}";
                        Image.Draw.aLeader.Text(v1, v2, txt, null); // in VB the aMidXY is optional but here is not, so I used null for it

                        v1 = org.Moved(2.5, -0.25);
                        v2 = v1.Moved(0.5, 0.5);
                        if (Drawing.DrawingUnits == uppUnitFamilies.English)
                        {
                            txt = "1-3";
                        }
                        else
                        {
                            txt = "25-75";
                        }

                        Image.Draw.aSymbol.Weld(new colDXFVectors(v1, v2), dxxWeldTypes.Fillet, 0.125, false, false, txt, bSuppressTail: true, aArrowSize: 0.09); // In VB the "0.09" was passed to bSuppresUCS argument. We use it as arrow size
                    }
                    else
                    {
                        v1 = org.Moved(2.3125, -0.45);
                        v2 = v1.Moved(0.355, -0.25);
                        txt = "END PLATE";
                        Image.Draw.aLeader.Text(v1, v2, txt, null);
                    }
                }

                // the table
                Status = "Drawing Table";
                Image.TableSettings.ColumnGap = 0.125;
                v1 = aRect.BottomLeft;
                v1.Move(0.15);
                v1.Y = 1.15;
                Image.Draw.aTable("Table1", v1, aTbl, aTableAlign: dxxRectangularAlignments.BottomLeft);

                AppDrawingBorderData notes = new()
                {
                    PartNumber_Value = $"DOWNCOMER (OVERVIEW)",
                    ForParts_Value = "FOR GENERAL NOTES SEE SHEET 2",
                    ForMaterial_Tag = "",
                    NumberRequired_Value = "",
                    Location_Value = "",
                };


                Tool.Add_B_BorderNotes(Image, notes, aRect, paperscale);
            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
            finally
            {
                Status = "";
                DrawinginProgress = false;
            }
        }

        private void DDWG_SheetIndex()
        {
            // #1flag to request a return of the created DXF file
            // ^generates the Sheet Index drawing.

            List<string> alns;
            List<string> tbDat;
            dxfVector p1;
            uopCustomer cust;
            double iScale;
            dxeTable aTbl;
            dxeTable bTbl;
            dxfRectangle uRect = null;
            dxoSettingsTable tsets = Image.TableSettings;
            dxoDrawingTool draw = Image.Draw;
            try
            {
                // initialize

                tbDat = new List<string>();
                alns = new List<string>();
                p1 = dxfVector.Zero;

                //================== Border ==================================
                iScale = 1;
                Tool.Border(this, null, ref uRect, arPaperScale: ref iScale, bSuppressAnsiScales: true, aScaleString: "NTS", aSheetNo: 1);
                xCancelCheck();
                Image.Status = "Drawing Sheet Index (Sheet 1)";
                Drawing.ZoomExtents = false;
                //================== Border ==================================
                p1 = uRect.TopLeft;
                p1.Move(2, -1.5);
                draw.aText(p1, @"\LSHEET INDEX", 0.1875, dxxMTextAlignments.TopLeft, aTextStyle: "Standard");

                //=======================================
                List<string> tempList = mzUtils.StringsFromDelimitedList(Drawing.Tag); // in vb it is assigned directly to the tbDat which has type mismatch here
                foreach (var s in tempList)
                {
                    tbDat.Add(s);
                }
                alns.Add($"{dxxMTextAlignments.MiddleCenter}|{dxxMTextAlignments.MiddleLeft}");
                tsets = Image.TableSettings;
                tsets.TextColor = Image.TextSettings.LayerColor;
                tsets.TextSize = 0.125;
                tsets.HeaderTextScale = 0.1875 / 0.125;
                tsets.HeaderTextStyle = "Standard";
                tsets.ColumnGap = 0;
                tsets.TextGap = 0.065;
                tsets.HeaderRow = 1;

                p1.Move(1, -0.5);
                aTbl = draw.aTable("Table1", p1, tbDat, alns, aGridStyle: dxxTableGridStyles.None, bScaleToScreen: false, aHeaderRow: 1.0, bSuppressBorders: true);

                //=======================================

                cust = _MDProject.Customer;
                tbDat = new List<string>();
                alns = new List<string>();

                tbDat.Add($"CUSTOMER:||{cust.Name.ToUpper()}");
                tbDat.Add($"LOCATION:||{cust.Location.ToUpper()}");
                tbDat.Add($"ITEM NO.:||{cust.Item.ToUpper()}");
                tbDat.Add($"PO NO.:||{cust.PO.ToUpper()}");

                alns.Add($"{dxxMTextAlignments.MiddleRight}|{dxxMTextAlignments.MiddleLeft}|{dxxMTextAlignments.MiddleLeft}");
                p1.X -= 1;
                tsets.TextStyleName = "Standard";
                tsets.TextSize = 0.1875;
                tsets.HeaderRow = 0;

                p1.Move(-1.5, -aTbl.Height - 1);
                bTbl = (dxeTable)draw.aTable("Table2", p1, tbDat, alns, aGridStyle: dxxTableGridStyles.None, bScaleToScreen: false, aHeaderRow: 0.0, aHeaderCol: 0.0, bSuppressBorders: true);

                //==============================================
                tbDat = new List<string>();
                alns = new List<string>
                {
                    dxxMTextAlignments.MiddleCenter.Description(),
                    dxxMTextAlignments.MiddleCenter.Description()
                };

                tbDat.Add("CONFIDENTIAL");
                string txt = @"THE INFORMATION CONTAINED HEREIN IS PROPRIETARY\PINFORMATION OF UOP AND SHALL NOT BE COPIED AND/OR\PREPRODUCED IN ANY MANNER, NOR USED FOR ANY\PPURPOSE WHATSOEVER EXCEPT AS SPECIFICALLY\PPERMITTED PURSUANT TO WRITTEN AGREEMENT WITH UOP.";
                tbDat.Add(txt);

                //tbDat.Add("THE INFORMATION CONTAINED HEREIN IS PROPRIETARY ");
                //tbDat.Add("INFORMATION OF UOP AND SHALL NOT BE COPIED AND/");
                //tbDat.Add("OR REPRODUCED IN ANY MANNER, NOR USED FOR ANY");
                //tbDat.Add("PURPOSE WHATSOEVER EXCEPT AS SPECIFICALLY");
                //tbDat.Add("PERMITTED PURSUANT TO WRITTEN AGREEMENT WITH UOP.");
                //================================================================

                tsets.TextStyleName = "Standard";
                tsets.ColumnGap = 0.75;
                tsets.TextSize = 0.125;
                tsets.HeaderTextScale = 0.1875 / 0.125;
                tsets.HeaderTextStyle = "Standard";
                tsets.HeaderRow = 1;

                p1.X = bTbl.BoundingRectangle().Right + 1.25;
                draw.aTable("Table3", p1, tbDat, alns, aGridStyle: dxxTableGridStyles.None);

                DrawinginProgress = false;
            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
        }

        private void SetDrawingVars()
        {
            // ^executed prior to all top level drawing functions to set the module
            // ^level vars used by the lower level drawing routines.


            LayerPre = "";


            //Image.Styles.AddTextStyle( "RomanD", "romand.shx", 0.1875f, 0.8f );
            //Image.Styles.AddTextStyle( "RomanS", "RomanS.shx", 0f, 0.8f, "", false, false, false, false, true );

            if (Assy == null)
            {
                return;
            }

            try
            {
                RingRad = MDRange.RingID * 0.5;
                ShellRad = MDRange.ShellID * 0.5;
                RingTk = MDRange.RingThk;
                if (RingTk <= 0)
                {
                    RingTk = 0.05;
                }




            }
            catch (Exception e)
            {
                throw new Exception("SetDrawingVars", e); // I am not sure if this does the same thing as VB
            }
        }

        private void Draw_FunctionalBlocks(double iScale = 1, string aBlockList = "")
        {
         

            double multi;

            double mechArea;
            double funcArea;
            dxfRectangle aRect;
            string bname;
            double topY;
            double X_1;
            dxfBlock blk;
            dxeInsert ins;
            bool bDoit;
            dxoDrawingTool draw = Image.Draw;
            bool attribs2text = false;
            
            try
            {
                aBlockList = aBlockList.Trim();

                topY = 22.75 * iScale;
                X_1 = 28.75 * iScale;

                Image.Layers.Add("HEAVY", dxxColors.Grey, dxfLinetypes.Continuous);

                Status = "Computing Tray Blocked Areas";

                multi = Drawing.DrawingUnits == uppUnitFamilies.Metric ? 2.54 : 1;

                Status = "Inserting Function Inserting Blocks";
                dxfVector v1 = dxfVector.Zero;

                if (iScale <= 0) iScale = 1;


                mechArea = Assy.MechanicalActiveArea;
                funcArea = Assy.FunctionalActiveArea;
                Status = "";

                //============================= BUBLLE PROMOTER DETAIL
                bname = "BP_DETAIL";
                bDoit = mzUtils.ListContains(bname, aBlockList, bReturnTrueForNullList: true);
                if (bDoit)
                {
                    blk = Tool.CreateBlock(bname, Image, _MDProject, Assy, Drawing, mechArea, funcArea, bAttribsToText: attribs2text);
                    if (blk != null)
                    {
                        Status = $"Inserting Block '{bname}'";
                        v1.SetCoordinates(3.5 * iScale, 3.75 * iScale);

                        //colDXFEntities bents = blk.Entities.Clone();
                        //bents.Rescale(iScale, blk.InsertionPt);
                        //bents.Translate(v1);
                        //aRect = bents.BoundingRectangle();
                        //Image.Entities.Append(bents);
                        blk = Image.Blocks.Add(blk, true);


                        ins = draw.aInsert(blk.Name, v1, aScaleFactor: iScale); aRect = ins.BoundingRectangle();
                        if (!Assy.DesignOptions.HasBubblePromoters)
                        {
                            draw.aLine(aRect.BottomLeft, aRect.TopRight, "HEAVY", dxxColors.ByLayer, "ByLayer");
                            draw.aLine(aRect.TopLeft, aRect.BottomRight, "HEAVY", dxxColors.ByLayer, "ByLayer");
                        }
                    }

                    Status = "";
                }
                xCancelCheck();

                //============================= DC CROSS SECTION
                bname = "DC_XSECTION";
                bDoit = mzUtils.ListContains(bname, aBlockList, bReturnTrueForNullList: true);
                if (bDoit)
                {
                    blk = Tool.CreateBlock(bname, Image, _MDProject, Assy, Drawing, mechArea, funcArea, bAttribsToText: attribs2text);
                    if (blk != null)
                    {
                        Status = $"Inserting Block '{bname}'";
                        blk = Image.Blocks.Add(blk, true);

                        v1.SetCoordinates(3.307 * iScale, topY);
                        ins = draw.aInsert(blk.Name, v1, aScaleFactor: iScale); aRect = ins.BoundingRectangle();

                    }

                    Status = "";
                }
                xCancelCheck();

                //============================= MATERIAL TABLE
                bname = "TRAY_MATERIAL";
                bDoit = mzUtils.ListContains(bname, aBlockList, bReturnTrueForNullList: true);
                if (bDoit)
                {
                    blk = Tool.CreateBlock(bname, Image, _MDProject, Assy, Drawing, mechArea, funcArea, bAttribsToText: attribs2text);
                    if (blk != null)
                    {
                        Status = $"Inserting Block '{bname}'";
                        blk = Image.Blocks.Add(blk, true);

                        v1.SetCoordinates(10.5671 * iScale, topY);
                        ins = draw.aInsert(blk.Name, v1, aScaleFactor: iScale); aRect = ins.BoundingRectangle();
                    }

                    Status = "";
                }
                xCancelCheck();

                //============================= DC ELEVATION
                bname = "DC_ELEVATION";
                bDoit = mzUtils.ListContains(bname, aBlockList, bReturnTrueForNullList: true);
                if (bDoit)
                {
                    blk = Tool.CreateBlock(bname, Image, _MDProject, Assy, Drawing, mechArea, funcArea, bAttribsToText: attribs2text);
                    if (blk != null)
                    {
                        Status = $"Inserting Block '{bname}'";
                        blk = Image.Blocks.Add(blk, true);

                        v1.SetCoordinates(17.214 * iScale, topY);
                        ins = draw.aInsert(blk.Name, v1, aScaleFactor: iScale); aRect = ins.BoundingRectangle();
                    }

                    Status = "";
                }
                xCancelCheck();

                //============================= BEAM DETAIL
                if (Assy.DesignFamily.IsBeamDesignFamily())
                {
                    Image.Layers.Add("IBeam", dxxColors.z_110);
                    Image.Layers.Add("AM_Dimension", dxxColors.Yellow);

                    bname = "BEAM_DETAIL";
                    bDoit = mzUtils.ListContains(bname, aBlockList, bReturnTrueForNullList: true);
                    if (bDoit)
                    {
                        dxfBlock aBlk = Tool.CreateBlock(bname, Image, _MDProject, Assy, Drawing, mechArea, funcArea, bAttribsToText: attribs2text);
                        if (aBlk != null)
                        {
                            Status = $"Inserting Block '{bname}'";
                            aBlk = Image.Blocks.Add(aBlk, true);

                            v1.SetCoordinates(275, 70);
                            var insert = draw.aInsert(aBlk.Name, v1); aRect = insert.BoundingRectangle();

                        }

                        Status = "";
                    }
                    xCancelCheck();
                }

                //============================= FUNCTIONAL TABLE
                bname = "MDFUNCTIONAL_TABLE";
                bDoit = mzUtils.ListContains(bname, aBlockList, bReturnTrueForNullList: true);
                if (bDoit)
                {
                    blk = Tool.CreateBlock(bname, Image, _MDProject, Assy, Drawing, mechArea, funcArea, bAttribsToText: attribs2text);
                    if (blk != null)
                    {
                        Status = $"Inserting Block '{bname}'";
                        blk = Image.Blocks.Add(blk, true);

                        v1.SetCoordinates(X_1, topY);
                        ins = draw.aInsert(blk.Name, v1, aScaleFactor: iScale); aRect = ins.BoundingRectangle();
                    }

                    Status = "";
                }
                xCancelCheck();

                //============================= STARTUP TABLE
                bname = "STARTUP_TABLE";
                bDoit = mzUtils.ListContains(bname, aBlockList, bReturnTrueForNullList: true);
                if (bDoit)
                {
                    blk = Tool.CreateBlock(bname, Image, _MDProject, Assy, Drawing, mechArea, funcArea, bAttribsToText: attribs2text);
                    if (blk != null)
                    {
                        Status = $"Inserting Block '{bname}'";
                        blk = Image.Blocks.Add(blk, true);

                        v1.SetCoordinates(X_1, topY - (1.9836 + 0.25) * iScale);
                        ins = draw.aInsert(blk.Name, v1, aScaleFactor: iScale); aRect = ins.BoundingRectangle();
                    }

                    Status = "";
                }
                xCancelCheck();

                //============================= DOWNCOMER TABLE
                bname = "DC_TABLE";
                bDoit = mzUtils.ListContains(bname, aBlockList, bReturnTrueForNullList: true);
                if (bDoit)
                {
                    blk = Tool.CreateBlock(bname, Image, _MDProject, Assy, Drawing, mechArea, funcArea, bAttribsToText: attribs2text);
                    if (blk != null)
                    {
                        Status = $"Inserting Block '{bname}'";
                        blk = Image.Blocks.Add(blk, true);

                        v1.SetCoordinates(X_1 - (5.8248 * iScale) + (2.975 / 2 * iScale), topY - 2 * (1.9836 + 0.25) * iScale);
                        ins = draw.aInsert(blk.Name, v1, aScaleFactor: iScale); aRect = ins.BoundingRectangle();
                        topY = aRect.Bottom - 0.25 * iScale;
                    }

                    Status = "";
                }
                xCancelCheck();

                //============================= DESIGN DATA TABLE
                bname = "DESIGN_DATA";
                bDoit = mzUtils.ListContains(bname, aBlockList, bReturnTrueForNullList: true);
                if (bDoit)
                {
                    blk = Tool.CreateBlock(bname, Image, _MDProject, Assy, Drawing, mechArea, funcArea, bAttribsToText: attribs2text);
                    if (blk != null)
                    {
                        Status = $"Inserting Block '{bname}'";
                        blk = Image.Blocks.Add(blk, true);

                        v1.SetCoordinates(22.2329 * iScale, topY);
                        ins = draw.aInsert(blk.Name, v1, aScaleFactor: iScale); aRect = ins.BoundingRectangle();
                    }
                    v1.SetCoordinates(22.2329 * iScale, topY - (2.2958 + 0.25) * iScale);
                    Status = "";
                }
                xCancelCheck();

                //============================= SPOUT TABLE
                bname = "SPOUT_TABLE";
                bDoit = mzUtils.ListContains(bname, aBlockList, bReturnTrueForNullList: true);
                if (bDoit)
                {
                    blk = Tool.CreateBlock(bname, Image, _MDProject, Assy, Drawing, mechArea, funcArea, bAttribsToText: attribs2text);
                    if (blk != null)
                    {
                        Status = $"Inserting Block '{bname}'";
                        blk = Image.Blocks.Add(blk, true);

                        v1.SetCoordinates(22.2329 * iScale, topY - (2.2958 + 0.25) * iScale);
                        ins = draw.aInsert(blk.Name, v1, aScaleFactor: iScale); aRect = ins.BoundingRectangle();
                    }

                    Status = "";
                }
                xCancelCheck();

                //============================= ECMD DESIGN DATA TABLE
                bname = "ECMD_DESIGN_DATA";
                bDoit = mzUtils.ListContains(bname, aBlockList, bReturnTrueForNullList: true);
                if (bDoit)
                {
                    blk = Tool.CreateBlock(bname, Image, _MDProject, Assy, Drawing, mechArea, funcArea, bAttribsToText: attribs2text, bCrossOut: !Assy.DesignFamily.IsEcmdDesignFamily());
                    if (blk != null)
                    {
                        Status = $"Inserting Block '{bname}'";
                        blk = Image.Blocks.Add(blk, true);

                        v1.SetCoordinates(29.5448 * iScale, topY);
                        ins = draw.aInsert(blk.Name, v1, aScaleFactor: iScale);

                        topY -= (2.5435 + 0.25) * iScale;
                    }

                    Status = "";
                }
                xCancelCheck();

                //============================= AP PAN TABLE
                if (Assy.HasAntiPenetrationPans || !Assy.HasFlowEnhancement)
                {
                    bname = "AP_DESIGN";
                    bDoit = mzUtils.ListContains(bname, aBlockList, bReturnTrueForNullList: true);
                    if (bDoit)
                    {
                        blk = Tool.CreateBlock(bname, Image, _MDProject, Assy, Drawing, mechArea, funcArea, bAttribsToText: attribs2text, bCrossOut: !Assy.HasAntiPenetrationPans);
                        if (blk != null)
                        {
                            Status = $"Inserting Block '{bname}'";
                            blk = Image.Blocks.Add(blk, true);
                            draw.aInsert(blk.Name, new dxfVector(29.5448 * iScale, topY), aScaleFactor: iScale);
                            topY -= (2.167 + 0.25) * iScale;
                        }
                    }
                }
                else
                {
                    bname = "FED";
                    bDoit = mzUtils.ListContains(bname, aBlockList, bReturnTrueForNullList: true);
                    if (bDoit)
                    {
                        blk = Tool.CreateBlock(bname, Image, _MDProject, Assy, Drawing, mechArea, funcArea, bAttribsToText: attribs2text);
                        if (blk != null)
                        {
                            Status = $"Inserting Block '{bname}'";
                            blk = Image.Blocks.Add(blk, true);
                            draw.aInsert(blk.Name, new dxfVector(29.5448 * iScale, topY), aScaleFactor: iScale);
                            topY -= (2.167 + 0.25) * iScale;
                        }
                    }
                }
                Status = "";
                xCancelCheck();

                //============================= BLOCKED_AREA
                bname = "BLOCKED_AREA";
                bDoit = mzUtils.ListContains(bname, aBlockList, bReturnTrueForNullList: true);
                if (bDoit)
                {
                    blk = Tool.CreateBlock(bname, Image, _MDProject, Assy, Drawing, mechArea, funcArea, bAttribsToText: attribs2text);
                    if (blk != null)
                    {
                        Status = $"Inserting Block '{bname}'";
                        blk = Image.Blocks.Add(blk, true);

                        ins = draw.aInsert(blk.Name, new dxfVector(29.5448 * iScale, topY), aScaleFactor: iScale);


                        topY -= (1.5435 + 0.25) * iScale;
                    }

                    Status = "";
                }
                xCancelCheck();

                //============================= DOWNCOMER NOTES
                bname = "DCNOTES";
                bDoit = mzUtils.ListContains(bname, aBlockList, bReturnTrueForNullList: true);
                if (bDoit)
                {
                    blk = Tool.CreateBlock(bname, Image, _MDProject, Assy, Drawing, mechArea, funcArea, bAttribsToText: attribs2text);
                    if (blk != null)
                    {
                        Status = $"Inserting Block '{bname}'";
                        blk = Image.Blocks.Add(blk, true);

                        v1.SetCoordinates(29.5448 * iScale, topY);
                        draw.aInsert(blk.Name, v1, aScaleFactor: iScale);


                    }

                    Status = "";
                }
                xCancelCheck();
            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
        }

        private void DDWG_DC_EndSupport()
        {
            if (Drawing.Part == null) return;
            mdEndSupport aSupport = (mdEndSupport)Drawing.Part;

            if (aSupport == null) return;

            colDXFVectors DimPts = new();
            double paperscale = 0;
            double d1;
            dxfRectangle aRec;
            dxfRectangle fitRec = null;
            colDXFEntities aHoles;
            bool bTria = aSupport.HasTriangularEndPlate && !aSupport.IsSquare;
            dxeDimension aDim;
            string txt;
            dxxPointFilters fltr;
            dxfEntity aEnt;
            dxfVector v2;
            dxfVector v3;
            dxfVector v4;
            string bname;
            dxeInsert ins;
            dxeLine cLn;
            colDXFVectors chamferpts;
            dxfVector cpt1;
            dxfVector cpt2;
            dxeLine lapline;
            dxeLine limline;
            dxfRectangle bRec;
            colDXFEntities ents;
            dxePolygon vRight = null;
            dxePolygon vLeft = null;
            dxePolygon vPlan = null;
            dxePolygon vElev = null;
            bool topsupport = !aSupport.BottomSide;
            colDXFEntities dimsL = new();
            colDXFEntities dimsR = new();
            double f1 = topsupport ? 1 : -1;
            dxfDisplaySettings dsp = dxfDisplaySettings.Null(aLayer: "GEOMETRY");
            try
            {
                dxoDrawingTool draw = Image.Draw;
                dxoDimTool dims = Image.DimTool;
                dxoLeaderTool leaders = Image.LeaderTool;
                Status = "Creating View Geometry";
                vPlan = aSupport.View_Plan(aCenterLineLength: 8 * aSupport.Thickness, aCenter: dxfVector.Zero, aRotation: aSupport.BottomSide ? 180 : 0);
                aHoles = vPlan.AdditionalSegments.GetByGraphicType(dxxGraphicTypes.Arc);
                bool bTwoHoles = aHoles.Count > 1;
                vRight = mdPolygons.EndSupport_View_Layout(aSupport, vPlan, true, aCenter: dxfVector.Zero);
                vLeft = mdPolygons.EndSupport_View_Layout(aSupport, vPlan, false, aCenter: dxfVector.Zero);
                vElev = aSupport.View_Elevation(aCenter: dxfVector.Zero, aHoleClines: !bTwoHoles ? "END ANGLE" : "END ANGLE,RING CLIP");

                aRec = new dxfRectangle(aSupport.Width + (aSupport.WeirHeight * 2.5), vPlan.Height() +( aSupport.WeirHeight * 2.5));

                Status = $"Drawing DC {aSupport.PartNumber} End Support";
                //=====================================================================================================
                // insert the border
                Tool.Border(this, aRec, ref fitRec, ref paperscale,aWidthBuffer: 0, aHeightBuffer: 0);
                xCancelCheck();
                //=====================================================================================================

                Image.SelectionSetInit(false);

                //================ TOP VIEW ===============================================
                dxfVector vcen = fitRec.TopCenter.Moved(aYChange: -0.5 * aSupport.Height - 1.25 * paperscale);

                
                Image.SelectionSetInit( false );
       
                Status = "Drawing Top View Geometry";
                {
                    bname = $"END_SUPPORT_{aSupport.PartNumber}_PLAN_MFG";
                    ins = draw.aInsert(vPlan, vcen, 0, bname, aDisplaySettings: dsp);
                    ins.Plane = new dxfPlane(vPlan.Plane.Origin, vPlan.Plane.XDirection, vPlan.Plane.YDirection);
                    vPlan.MoveTo(vcen);

                    aRec = ins.BoundingRectangle(dxfPlane.World);
                    cLn = draw.aCenterlines(aRec, 0.25 * paperscale, aSuppressHorizontal: true)[0];

                    List<dxePoint> pts = vPlan.AdditionalSegments.Points();
                    List<dxeLine> eanglecls = vPlan.AdditionalSegments.Lines().FindAll(x => string.Compare(x.Tag, "END ANGLE", true) == 0);

                    List<dxeLine> lines = vPlan.Segments.Lines();
                    lapline = lines.Find(x => string.Compare(x.Tag, "LAP_LINE", true) == 0);
                    limline = lines.Find(x => string.Compare(x.Tag, "LIM_LINE", true) == 0);
                    dxeLine shortline = lines.Find(x => string.Compare(x.Tag, "SHORT", true) == 0);
                    v2 = limline.IntersectPoint(shortline);
                    DimPts = new colDXFVectors(limline.StartPt.Clone(), v2, shortline.EndPt.Clone());

                    draw.aPolyline(DimPts, false, aDisplaySettings: dxfDisplaySettings.Null(aLinetype: dxfLinetypes.Phantom));

                    dxeLine insideleft = lines.Find(x => string.Compare(x.Tag, "INSIDE_LEFT", true) == 0);
                    v3 = limline.IntersectPoint(insideleft);
                    DimPts = new colDXFVectors(limline.EndPt.Clone(), v3, insideleft.StartPt.Clone());

                    draw.aPolyline(DimPts, false, aDisplaySettings: dxfDisplaySettings.Null(aLinetype: dxfLinetypes.Phantom));

                    chamferpts = new colDXFVectors(v2.Clone(), v3.Clone());
                    cpt2 = chamferpts.GetVector(dxxPointFilters.AtMaxX);
                    cpt1 = chamferpts.GetVector(dxxPointFilters.AtMinX);
                }

                //================ TOP VIEW DIMENSIONS ====================================

                aHoles = vPlan.AdditionalSegments.GetByGraphicType(dxxGraphicTypes.Arc);
                
                Status = "Drawing Top View Dimensions";
                {
                    var v1 = topsupport ? cpt2.Clone() : cpt1.Clone();
                    if (!aSupport.IsSquare)
                    {
                        fltr = topsupport ? dxxPointFilters.GetRightTop : dxxPointFilters.GetLeftTop;
                        v2 = vPlan.GetVertex(fltr, bReturnClone: true);
                        DimPts = new colDXFVectors(v1, v2);
                        //fltr = topsupport ? dxxPointFilters.GreaterThanX : dxxPointFilters.LessThanX;
                        DimPts.Append(aHoles.Centers()); // .GetVectors(fltr, cLn.X, true));
                        dimsL = dims.Stack_Vertical(DimPts, f1 * 0.35 * paperscale, 0.125 * paperscale, aBotToTopLeftToRight: true);

                    }

                    v2 = v1.Moved(0.35 * paperscale * f1, -0.35 * paperscale);
                    txt = Image.FormatNumber(aSupport.RightChamfer, aPrecision: Drawing.DrawingUnits == uppUnitFamilies.Metric ? 0 : -1);
                    txt = aSupport.LeftChamfer == aSupport.RightChamfer ? $"{txt} CHAMFER\\P2 PLACES" : $"{txt} CHAMFER";
                    leaders.Text(v1, v2, txt);

                    if (!bTria)
                    {
                        v2 = vPlan.Vertex(1, true);
                        fltr = topsupport ? dxxPointFilters.AtMinX : dxxPointFilters.AtMaxX;
                        v1 = chamferpts.GetVector(fltr, bReturnClone: true);

                        DimPts = new colDXFVectors(v1, v2);
                        fltr = topsupport ? dxxPointFilters.LessThanX : dxxPointFilters.GreaterThanX;
                        DimPts.Append(aHoles.Centers().GetVectors(aFilter: fltr, aOrdinate: cLn.X, bOnIsIn: false));
                        dimsR = dims.Stack_Vertical(DimPts, -f1 * 0.65 * paperscale, 0.125 * paperscale, aBotToTopLeftToRight: true);
                        fltr = topsupport ? dxxPointFilters.GetLeftBottom : dxxPointFilters.GetRightBottom;
                        v2 = vPlan.GetVertex(fltr, bReturnClone: true);
                        aDim = dims.Horizontal(v2, v1, v1.Y - 0.25 * paperscale, aSuffix: " (TYP)", bAbsolutePlacement: true);

                    }
                    else
                    {
                        // tall side dims and leaders
                        fltr = topsupport ? dxxPointFilters.GetLeftBottom : dxxPointFilters.GetRightBottom;
                        v2 = vPlan.GetVertex(fltr, bReturnClone: true);
                        v3 = topsupport ? cpt1 : cpt2;
                        aDim = dims.Horizontal(v3, v2, v3.Y - 0.45 * paperscale, aSuffix: "\\P(TYP)", bAbsolutePlacement: true);
                        DimPts = new colDXFVectors(cpt2.Clone(), cpt1.Clone(), vPlan.Vertex(1, true));
                        dimsR = dims.Stack_Vertical(DimPts, f1 * (-aDim.BoundingRectangle().Width - 0.125 * paperscale), aBotToTopLeftToRight: true);
                        if (mzUtils.CompareVal(aSupport.LeftChamfer, aSupport.RightChamfer, 2) != mzEqualities.Equals)
                        {
                            v1 = topsupport ? cpt1.Clone() : cpt2.Clone();
                            v2 = v1.Moved(-0.25 * paperscale * f1, 0.35 * paperscale);
                            txt = $"{Image.FormatNumber(aSupport.LeftChamfer, aPrecision: Drawing.DrawingUnits == uppUnitFamilies.Metric ? 0 : -1)} CHAMFER";
                            leaders.Text(v1, v2, txt);
                        }


                    }

                    if (bTwoHoles)
                    {
                        fltr = topsupport ? dxxPointFilters.AtMinX : dxxPointFilters.AtMaxX;
                        v1 = aHoles.Centers().GetVector(dxxPointFilters.AtMaxY, bReturnClone: true);
                        v2 = cLn.EndPoints().GetVector(dxxPointFilters.AtMaxY, bReturnClone: true);
                        aDim = dims.Horizontal(v1, v2, v2.Y + 0.25 * paperscale, aSuffix: " (TYP)", bAbsolutePlacement: true);
                    }

                    aRec = Image.SelectionSet(dxxSelectionTypes.CurrentSet).BoundingRectangle(dxfPlane.World);
                    // hole leader
                    if (!bTria)
                    {
                        aEnt = aHoles.GetByPoint(dxxPointFilters.AtMinX);
                        if (aEnt != null)
                        {
                            v2 = new dxfVector(aEnt.X + 0.5 * paperscale, aRec.Bottom - 0.25 * paperscale);
                            Tool.CreateHoleLeader(Image, aEnt, v2, dims, aHoles.Count, true);
                        }
                    }
                    else
                    {
                        aEnt = aHoles.GetByPoint(dxxPointFilters.AtMaxY);
                        if (aEnt != null)
                        {
                            dxeArc circ = aEnt as dxeArc;
                            v1 = new dxfVector(aEnt.X, aEnt.Y);
                            v2 = v1.ProjectedToLine(lapline);
                            v2 += v1.DirectionTo(v2) * 0.2 * paperscale;
                            v3 = dimsL.BoundingRectangle(dxfPlane.World).TopCenter;
                            if (v2.Y < v3.Y + 0.25 * paperscale) v2.Y = v3.Y + 0.25 * paperscale;
                            Tool.CreateHoleLeader(Image, aEnt, v2, dims, aHoles.Count);
                        }
                    }

                    aRec = Image.SelectionSet(dxxSelectionTypes.CurrentSet).BoundingRectangle(dxfPlane.World);

                    //shift down to align with fitrec
                    Image.SelectionSet( dxxSelectionTypes.CurrentSet ).MoveFromTo( aRec.TopCenter, fitRec.TopCenter );
                    var tc = aRec.TopCenter.Clone();
                    aRec.MoveFromTo( tc, fitRec.TopCenter );
                    vcen.MoveFromTo( tc, fitRec.TopCenter );
                    vPlan.MoveFromTo( tc, fitRec.TopCenter );

                    if (DrawlayoutRectangles)
                    {
                        draw.aPolyline(aRec, new dxfDisplaySettings(aColor: dxxColors.LightGrey));
                    }

                }


                //================ RIGHT VIEW ===============================================
                Status = "Drawing Right View";
                {
                    
                    Image.SelectionSetInit(false);
                    bRec = vRight.BoundingRectangle(dxfPlane.World);

                    v2 = vPlan.InsertionPt.Clone();
                    var v1 = new dxfVector(aRec.Right + 1.0 * paperscale + bRec.Width, vcen.Y);
                    bname = $"END_SUPPORT_{aSupport.PartNumber}_VIEW_RIGHT_MFG";

                    ins = draw.aInsert(vRight, v1, 0, bname, aDisplaySettings: dsp);

                    vRight.MoveTo(ins.InsertionPt);
                    d1 = ins.X - vRight.Vertex(1).X;
                    vRight.Move(d1);
                    bname = ins.BlockName;
                    aHoles = vRight.AdditionalSegments.GetByGraphicType(dxxGraphicTypes.Arc);
                }

                //================ RIGHT VIEW DIMENSIONS ====================================
                {
                    Status = "Drawing Right View Dimensions";
                    aEnt = aHoles.Item(1);
                    var v1 = vRight.GetVertex(dxxPointFilters.GetLeftTop, bReturnClone: true);
                    v2 = new dxfVector(aEnt.X, aEnt.Y);
                    v3 = vRight.GetVertex(dxxPointFilters.GetLeftBottom, bReturnClone: true);

                    aDim = dims.Vertical(v2, v1, -0.25);
                    bRec = aDim.BoundingRectangle();
                    aDim = dims.Vertical(v1, v3, bRec.Left - 0.25 * paperscale, bAbsolutePlacement: true);
                    dims.Horizontal(v2, v1, 0.25);

                    v3 = vRight.GetVertex(dxxPointFilters.GetRightTop, bReturnClone: true);
                    v4 = vRight.GetVertex(dxxPointFilters.GetRightBottom, bReturnClone: true);
                    dims.Vertical(v4, v3, 0.35);

                    bRec = Image.SelectionSet(dxxSelectionTypes.CurrentSet).BoundingRectangle();

                    if (DrawlayoutRectangles)
                    {
                        draw.aPolyline(bRec, new dxfDisplaySettings(aColor: dxxColors.LightGrey));
                    }

                   

                    colDXFVectors vrts = vRight.Vertices;
                    v1 = vRight.Vertex("SLOT_POINT", true);
                    if (v1 != null)
                    {
                        v2 = v1.Moved(-0.25 * paperscale, -0.35 * paperscale);
                        leaders.Text(v1, v2, "SLOTS MUST ACCOMMODATE\\PFIT-UP WITH END PLATE");
                    }


                    ents = Image.SelectionSet(dxxSelectionTypes.CurrentSet);
                    bRec = ents.BoundingRectangle();
                    d1 = aRec.Right + 0.35 * paperscale - bRec.Left;

                    if (d1 > 0)
                    {
                        ents.Move(d1);
                        vRight.Move(d1);
                        bRec.Move(d1);
                    }
                    v1 = bRec.TopRight.Moved(-0.65 * paperscale, 0.2 * paperscale);
                    Tool.CreateHoleLeader(Image, aEnt, v1, dims, 1);
                }

                //================ LEFT VIEW ===============================================
                Status = "Drawing Left View Geometry";
                Image.SelectionSetInit(false);
                {
                   
                    bRec = vLeft.BoundingRectangle(dxfPlane.World);

                    v2 = vPlan.InsertionPt.Clone();
                    var v1 = new dxfVector(aRec.Left - 1.0 * paperscale - bRec.Width, vcen.Y);
                    bname = $"END_SUPPORT_{aSupport.PartNumber}_VIEW_LEFT_MFG";

                    ins = draw.aInsert(vLeft, v1, 0, bname, aDisplaySettings: dsp);

                    vLeft.MoveTo(ins.InsertionPt);
                    aHoles = vLeft.AdditionalSegments.GetByGraphicType(dxxGraphicTypes.Arc);

                }

                //================ LEFT VIEW DIMENSIONS ====================================
                Status = "Drawing Left View Dimensions";
                {
                    aEnt = aHoles.Item(1);
                    var v1 = vLeft.GetVertex(dxxPointFilters.GetRightTop, bReturnClone: true);
                    v2 = new dxfVector(aEnt.X, aEnt.Y);
                    v3 = vLeft.GetVertex(dxxPointFilters.GetRightBottom, bReturnClone: true);

                    aDim = dims.Vertical(v2, v1, 0.25);
                    bRec = aDim.BoundingRectangle();
                    aDim = dims.Vertical(v1, v3, bRec.Right + 0.25 * paperscale, bAbsolutePlacement: true);
                    dims.Horizontal(v2, v1, 0.25);

                    v3 = vLeft.GetVertex(dxxPointFilters.GetLeftTop, bReturnClone: true);
                    v4 = vLeft.GetVertex(dxxPointFilters.GetLeftBottom, bReturnClone: true);
                    dims.Vertical(v4, v3, -0.35);

                    bRec = Image.SelectionSet(dxxSelectionTypes.CurrentSet).BoundingRectangle();

                    if (DrawlayoutRectangles)
                    {
                        draw.aPolyline(bRec, new dxfDisplaySettings(aColor: dxxColors.LightGrey));
                    }
                    

                    v1 = vLeft.Vertex("SLOT_POINT", true);
                    v2 = new dxfVector(v3.X, bRec.Bottom);
                    dims.Horizontal(v2, v1, v2.Y - 0.25 * paperscale, bAbsolutePlacement: true, aSuffix: "\\P(TYP)");
                    ents = Image.SelectionSet(dxxSelectionTypes.CurrentSet);
                    bRec = ents.BoundingRectangle();
                    d1 = aRec.Left - 0.25 * paperscale - bRec.Right;

                    if (d1 < 0)
                    {
                        ents.Move(d1);
                        vLeft.Move(d1);
                        bRec.Move(d1);
                    }
                    v1 = bRec.TopLeft.Moved(0.65 * paperscale, 0.2 * paperscale);
                    Tool.CreateHoleLeader(Image, aEnt, v1, dims, 1);
                }

                //================ END VIEW ===============================================
                Status = "Drawing End View Geometry";
                {
                    Image.SelectionSetInit(false);
                    
                    bRec = vElev.Vertices.BoundingRectangle();
                    var v1 = new dxfVector(vPlan.X, aRec.Bottom - 0.25 * paperscale - bRec.Height);
                    bname = $"END_SUPPORT_{aSupport.PartNumber}_VIEW_ELEV_MFG";
                    ins = draw.aInsert(vElev, v1, 0, bname, aDisplaySettings: dsp);
                    vElev.MoveTo(ins.InsertionPt);
                }


                //================ END VIEW DIMENSIONS ====================================

                Status = "Drawing End View Dimensions";
                {
                    bRec = vElev.Vertices.BoundingRectangle(dxfPlane.World);
                    cLn = draw.aCenterlines(bRec, 0.15 * paperscale, aSuppressHorizontal: true)[0];

                    DimPts = new colDXFVectors(bRec.TopLeft, cLn.EndPoints().GetVector(dxxPointFilters.AtMaxY), bRec.TopRight);
                    dims.Stack_Horizontal(DimPts, 0.25 * paperscale, 0.05 * paperscale);
                    v2 = vElev.GetVertex(dxxPointFilters.GetLeftTop, bReturnClone: true);

                    var v1 = vElev.GetVertex(dxxPointFilters.GetBottomLeft, bReturnClone: true);
                    dims.Vertical(v2, v1, -0.45);

                    ents = Image.SelectionSet(dxxSelectionTypes.CurrentSet);
                    bRec = ents.BoundingRectangle(dxfPlane.World);
                    d1 = aRec.Bottom - 0.15 * paperscale - bRec.Top;
                    ents.Move(0, d1);
                    bRec.Move(0, d1);
                    if (DrawlayoutRectangles)
                    {
                        aEnt = draw.aPolyline(bRec, new dxfDisplaySettings(aColor: dxxColors.LightGrey));
                    }

                }

                // ======= CENTER THE DRAWING ENTITIES
                {
                    ents = Image.SelectionSet(dxxSelectionTypes.SelectAll);
                    bRec = ents.BoundingRectangle();

                    ents.MoveFromTo( bRec.TopCenter, fitRec.TopCenter );
                    bRec = ents.BoundingRectangle();

                    if (DrawlayoutRectangles)
                    {
                        aEnt = draw.aPolyline(fitRec, new dxfDisplaySettings(aColor: dxxColors.LightBlue));
                    }




                    //v1 = fitRec.TopLeft.Moved(0.25 * paperscale, -0.15 * paperscale);
                    //v2 = bRec.TopLeft;

                    //v4 = v2.DirectionTo(v1, ref d1) * d1;
                    //ents.Translate(v4);
                    //bRec.Translate(v4);
                    if (DrawlayoutRectangles)
                    {
                        aEnt = draw.aPolyline(bRec, new dxfDisplaySettings(aColor: dxxColors.LightGrey));
                    }
                }

                //================ NOTES ===============================================


                Tool.AddBorderNotes(aSupport, Project, "END SUPPORT", true, Image, fitRec, paperscale);


            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
            finally
            {
                vPlan?.Dispose();
                vRight?.Dispose();
                vLeft?.Dispose();
                vElev?.Dispose();
                Status = "";
                DrawinginProgress = false;
            }
        }

        private void DDWG_DC_EndPlate()
        {
            mdEndPlate aEPlate = (mdEndPlate)Drawing.Part;

            if (aEPlate == null) return;

            dxoDimTool dims = Image.DimTool;
            dxoDrawingTool draw = Image.Draw;

            dxfVector v1;
            dxfVector v2 = null;
            dxfVector v3 = null;

            dxfRectangle bRec;
            dxfRectangle fitRec = null;
            dxeDimension aDim;
            dxeLeader aLdr;
            dxePolygon vRight = null;
            dxePolygon vLeft = null;
            dxePolygon vElev = null;
            dxePolygon vPlan = aEPlate.View_Plan(false, dxfVector.Zero, aRotation: aEPlate.BottomSide ? 180 : 0);
            colDXFVectors aVerts;
            dxePolyline aPl = null;
            bool bBltOn = aEPlate.BoltOn;
            dxfDisplaySettings dsp = dxfDisplaySettings.Null(aLayer: "GEOMETRY");
            colDXFEntities segs;
            string bname = $"ENDPLATE_{aEPlate.PartNumber}";
            double paperscale = 0;
            double oset = 1.6;
            double d1 = aEPlate.GussetLength + aEPlate.Length;
            bool bTria = aEPlate.IsTriangular;
            string txt;
            double f1 = aEPlate.BottomSide && bTria ? -1 : 1;
            bool bGust = aEPlate.GussetLength > 0;
            bool bCentral = Math.Round(aEPlate.X, 1) == 0;
            double thk = aEPlate.Thickness;
            dxeArc arc1;
            string designernote = null;
            dxxPointFilters fltr1;
            dxxPointFilters fltr2;

            colDXFEntities ents;
            dxeInsert ins;
            int iStart;
            dxfRectangle elevBounds = null;
            dxfRectangle rightBounds = null;
            try
            {


                Status = "Creating Plan View Geometry";
                vPlan = aEPlate.View_Plan(false, dxfVector.Zero, aRotation: aEPlate.BottomSide ? 180 : 0);
                vPlan.AdditionalSegments.SetDisplayVariable(dxxDisplayProperties.Linetype, "ByLayer", aTagList: "GUSSET");

                if (bGust) d1 *= 2;

                dxfRectangle aRec = new(aEPlate.Width + d1, aEPlate.Height + d1 + vPlan.Height());

                Status = $"Drawing DC {aEPlate.ParentList} End Plate";
                {
                    //================insert the border=====================================================================================

                    Tool.Border(this, aRec, ref fitRec, ref paperscale, 0, 1.5);
                    xCancelCheck();

                    oset = aEPlate.BoltOn ? 1.6 * paperscale : 1.0 * paperscale;


                    if (DrawlayoutRectangles)
                    {
                        draw.aPolyline(fitRec, new dxfDisplaySettings(aColor: dxxColors.LightBlue));
                    }

                }

                iStart = Image.Entities.Count + 1;

                //================ PLAN VIEW ===============================================
                Status = "Drawing Plan View Geometry";
                {
                    Image.SelectionSetInit(false);
                    vPlan = aEPlate.View_Plan(true, dxfVector.Zero, aRotation: aEPlate.BottomSide ? 180 : 0, aCenterLineLength: 0.15 * paperscale, bIncludeBendPoints: true);
                    d1 = -0.75 * paperscale - (0.5 * aEPlate.Length + aEPlate.GussetLength);
                    if (bGust)
                    {
                        vPlan.AdditionalSegments.SetDisplayVariable(dxxDisplayProperties.Linetype, dxfLinetypes.Continuous, aTagList: "GUSSET");
                        v1 = fitRec.TopCenter.Moved(-0.5 * aEPlate.Width, d1);
                    }
                    else
                    {
                        v1 = f1 == 1 ? fitRec.TopLeft.Moved(1.75 * paperscale + 0.5 * aEPlate.Width, d1) : fitRec.TopRight.Moved(-4.5412 * paperscale - 1.85 * paperscale - 0.5 * aEPlate.Width, d1);

                    }
                    if (DrawlayoutRectangles)
                    {
                        draw.aCircle(v1, 0.1 * paperscale, dxfDisplaySettings.Null(aColor: dxxColors.Red));
                    }


                    ins = draw.aInsert(vPlan, v1, aBlockName: $"{bname}_PLAN", aDisplaySettings: dsp);
                    vPlan.MoveTo(ins.InsertionPt);

                    vPlan.UpdatePath(true, Image);
                    segs = vPlan.Segments;

                }

                //================ PLAN VIEW DIMENSIONS ====================================
                Status = "Drawing Plan View Dimensions";
                {
                    List<dxfEntity> arcsegs = segs.FindAll(x => x.GraphicType == dxxGraphicTypes.Arc);
                    List<dxfEntity> linesesegs = segs.FindAll(x => x.GraphicType == dxxGraphicTypes.Line);
                    List<dxeLine> lines = segs.Lines();
                    List<dxeArc> arcs = segs.Arcs();
                    List<dxePoint> pts = vPlan.AdditionalSegments.Points();

                    dxeDimension dim1;
                    dxeLine seg1 = lines[0];
                    dxeLine seg2 = f1 == 1 ? lines[1] : lines[7];
                    dxeLine seg3 = f1 == 1 ? lines[3] : lines[5];
                    dxeLine seg4 = f1 == 1 ? lines[4] : lines[4];
                    dxeLine seg5 = f1 == 1 ? lines[5] : lines[3];
                    dxeLine seg6 = f1 == 1 ? lines[7] : lines[1];

                    dxePolyline fltline1 = null;


                    //if (DrawlayoutRectangles)
                    //{
                    //    draw.aText(seg1.MidPt, 1, 0.05 * paperscale, dxxMTextAlignments.MiddleCenter);
                    //    draw.aText(seg2.MidPt, 2, 0.05 * paperscale, dxxMTextAlignments.MiddleCenter);
                    //    draw.aText(seg3.MidPt, 3, 0.05 * paperscale, dxxMTextAlignments.MiddleCenter);
                    //    draw.aText(seg4.MidPt, 4, 0.05 * paperscale, dxxMTextAlignments.MiddleCenter);
                    //    draw.aText(seg5.MidPt, 5, 0.05 * paperscale, dxxMTextAlignments.MiddleCenter);
                    //    draw.aText(seg6.MidPt, 6, 0.05 * paperscale, dxxMTextAlignments.MiddleCenter);

                    //}

                    v1 = seg2.EndPoints(true).GetVector(dxxPointFilters.AtMaxY);
                    if (bTria)
                    {

                        Tool.FilletLines(Image, seg1, seg2, out fltline1, aGap: 0.01 * paperscale);
                        v3 = (f1 == 1) ? pts.Find(x => string.Compare(x.Tag, "BEND_POINT_2", true) == 0).Center : pts.Find(x => string.Compare(x.Tag, "BEND_POINT_1", true) == 0).Center;
                        v2 = pts.Find(x => string.Compare(x.Tag, "BEND_POINT_3", true) == 0).Center;

                        //draw.aCircle(v3, 0.0625 * paperscale);
                        dim1 = dims.Vertical(v2, v1, v1.X + 0.25 * paperscale * f1, bAbsolutePlacement: true);
                        d1 = f1 == 1 ? dim1.BoundingRectangle().Right + 0.15 * paperscale * f1 : dim1.BoundingRectangle().Left + 0.5 * paperscale * f1;
                        dxfVector.SwapVectors(ref v1, ref v3, f1 == 1);
                        dim1 = dims.Vertical(v1, v3, d1, bAbsolutePlacement: true, aSuffix: " (REF)");

                        dim1 = (f1 == 1) ? dims.Angular(seg4, seg5, 0.75 * seg5.Length) : dims.Angular(seg5, seg4, 0.75 * seg5.Length);


                    }
                    else
                    {
                        txt = bGust ? " (TYP)" : "";
                        v1 = vPlan.GetVertex(aEPlate.BottomSide ? dxxPointFilters.GetTopLeft : dxxPointFilters.GetTopRight, bReturnClone: true);
                        v2 = seg1.EndPoints(true).GetVector(aEPlate.BottomSide ? dxxPointFilters.AtMinX : dxxPointFilters.AtMaxX);
                        dim1 = dims.Vertical(v2, v1, v1.X + (aEPlate.BottomSide ? -1 : 1) * 0.45 * paperscale, bAbsolutePlacement: true, aSuffix: txt);
                    }

                    if (bTria)
                    {
                        v1 = seg6.EndPoints().GetVector(dxxPointFilters.AtMaxY);

                        v2 = Tool.FilletLines(Image, seg6, seg1, out fltline1, aGap: 0.01 * paperscale);

                        ////draw.aCircle(v3, 0.0625 * paperscale);
                        dim1 = dims.Vertical(v2, v1, v1.X - 0.25 * paperscale * f1, bAbsolutePlacement: true);

                        v2 = pts.Find(x => string.Compare(x.Tag, "BEND_POINT_3", true) == 0).Center;

                        d1 = f1 == 1 ? dim1.BoundingRectangle().Left - 0.15 * paperscale : dim1.BoundingRectangle().Right + 0.1 * paperscale;
                        dim1 = dims.Vertical(v2, v1, d1, bAbsolutePlacement: true);

                        v2 = f1 == 1 ? pts.Find(x => string.Compare(x.Tag, "BEND_POINT_2", true) == 0).Center : pts.Find(x => string.Compare(x.Tag, "BEND_POINT_1", true) == 0).Center;

                        d1 = f1 == 1 ? dim1.BoundingRectangle().Left - 0.35 * paperscale : dim1.BoundingRectangle().Right + 0.35 * paperscale;
                        dim1 = dims.Vertical(v2, v1, d1, bAbsolutePlacement: true, aSuffix: "\\P(REF)");


                    }
                    else if (bGust)
                    {
                        dims.Vertical(vPlan.GetVertex(aEPlate.BottomSide ? dxxPointFilters.GetBottomRight : dxxPointFilters.GetBottomLeft), aEPlate.BottomSide ? vPlan.BoundingRectangle().TopRight : vPlan.BoundingRectangle().TopLeft, aEPlate.BottomSide ? 0.45 : -0.45);
                    }

                    ents = Image.SelectionSetInit(true, bMaintainStartPoint: true);
                    aRec = ents.BoundingRectangle();

                    if (DrawlayoutRectangles)
                        draw.aPolyline(aRec, new dxfDisplaySettings(aColor: dxxColors.LightGrey));

                }


                //================ ELEVATION VIEW ===============================================

                Status = "Drawing Elevation View Geometry";
                {
                    Image.SelectionSetInit(false);

                    v1 = aRec.BottomCenter;
                    v1.Y -= 2 * paperscale;
                    v1.X = vPlan.X;
                    vElev = aEPlate.View_Elevation(dxfVector.Zero, 0, "GEOMETRY", 0.15 * paperscale);
                    vElev.Vertices.SetDisplayVariable(dxxDisplayProperties.Linetype, dxfLinetypes.Hidden, aTagList: "THK");
                    vElev.AdditionalSegments.SetDisplayVariable(dxxDisplayProperties.Linetype, dxfLinetypes.Hidden, aTagList: "TAB");
                    if (f1 != 1)
                    {
                        vElev.Vertex(1).Linetype = dxfLinetypes.Hidden;
                        vElev.Vertex(11).Linetype = dxfLinetypes.Hidden;
                    }

                    ins = draw.aInsert(vElev, v1, aBlockName: $"{bname}_ELEV", aDisplaySettings: dsp);
                    vElev.MoveTo(ins.InsertionPt);
                }

                //================ ELEVATION VIEW DIMENSIONS ====================================
                Status = "Drawing Elevation View Dimensions";
                {
                    //overall height
                    v1 = vElev.GetVertex(dxxPointFilters.GetTopRight);
                    v2 = vElev.GetVertex(dxxPointFilters.GetBottomRight);
                    dims.Vertical(v2, v1, 0.45);

                    //notch width
                    v1 = vElev.GetVertex(dxxPointFilters.GetRightBottom);
                    dims.Horizontal(v2, v1, -0.4, aSuffix: "\\P(TYP)");

                    //nocth height
                    v2 = vElev.GetVertex(dxxPointFilters.GetBottomLeft);
                    v1 = vElev.GetVertex(dxxPointFilters.GetLeftBottom);
                    aDim = dims.Vertical(v1, v2, -0.25, aSuffix: " (TYP)");

                    if (DrawlayoutRectangles)
                        draw.aPolyline(aDim.TextPrimary.BoundingRectangle(), new dxfDisplaySettings(aColor: dxxColors.Blue));


                    //overall width
                    v1 = vElev.GetVertex(dxxPointFilters.GetTopRight);
                    v2 = vElev.GetVertex(dxxPointFilters.GetTopLeft);
                    dims.Horizontal(v1, v2, 0.45);

                    ents = Image.SelectionSetInit(true);
                    elevBounds = ents.BoundingRectangle();
                    if (DrawlayoutRectangles)
                    {
                        draw.aPolyline(elevBounds, new dxfDisplaySettings(aColor: dxxColors.LightGrey));
                    }
                    v1 = dxfVector.Zero;
                    v2 = dxfVector.Zero;


                }

                //================ RIGHT VIEW ===============================================
                Status = "Drawing Right View Geometry";
                {
                    Image.SelectionSetInit(false);

                    var mustDrawLeftView = LeftSideViewMustBeDrawnForEndPlate(vPlan) || !aEPlate.IsTriangular;

                    v1 = bTria && mustDrawLeftView ? elevBounds.MiddleLeft().Moved(-oset - 0.5 * aEPlate.Length) : elevBounds.MiddleRight().Moved(oset);

                    if (bBltOn) v1.Move(1.25 * paperscale);
                    v1.Y = vElev.Y;

                    double rot = -90;
                    if (!bCentral)
                    {
                        vRight = (bGust && aEPlate.BottomSide) || mustDrawLeftView ? aEPlate.View_LayoutLeft(out dxePolygon _, dxfVector.Zero, rot) : aEPlate.View_LayoutRight(out dxePolygon _, dxfVector.Zero, rot);
                    }
                    else
                    {
                        vRight = aEPlate.View_Profile(false, aEPlate.Y < aEPlate.BoxY, false, dxfVector.Zero, 0, "GEOMETRY", !bBltOn);
                    }


                    draw.aInsert(vRight, v1, aBlockName: $"{bname}_PROFILE_RIGHT", aDisplaySettings: dsp);
                    vRight.MoveTo(v1);


                    if (DrawlayoutRectangles)
                        draw.aSymbol.Axis(vRight.Plane);
                }

                //================ RIGHT VIEW DIMENSIONS ====================================
                Status = "Drawing Right View Dimensions";
                {
                    aVerts = vRight.Vertices.Clone();
                    if (bTria)
                    {
                        aPl = vRight.AdditionalSegments.Polylines()[0];
                        aVerts.Append(aPl.Vertices, bAppendClones: true);
                        aVerts.ReIndex();

                        if (DrawlayoutRectangles)
                            Tool.NumberVectors(draw, aVerts, paperscale);


                        //the tab height
                        fltr1 = !aEPlate.BottomSide ? dxxPointFilters.GetRightBottom : dxxPointFilters.GetLeftBottom;
                        fltr2 = !aEPlate.BottomSide ? dxxPointFilters.GetTopRight : dxxPointFilters.GetTopLeft;
                        v1 = aVerts.GetVector(fltr1);
                        v2 = aVerts.GetVector(fltr2);

                        aDim = dims.Vertical(v1, v2, 0.65 * f1, aSuffix: " (TYP)");

                        //the tab depth
                        Image.DimStyleOverrides.SuppressExtLine1 = true;
                        aDim = dims.Horizontal(aVerts.NextVector(v1), v1, -0.45, aSuffix: "\\P(TYP)");

                    }
                    else
                    {
                        if (bGust)
                        {

                            if (!aEPlate.BottomSide)
                            {
                                if (DrawlayoutRectangles)
                                    Tool.NumberVectors(draw, aVerts, paperscale);


                                //the tab height

                                v1 = aVerts.GetVector(dxxPointFilters.GetRightBottom);
                                v2 = aVerts.GetVector(dxxPointFilters.GetTopRight);
                                Image.DimStyleOverrides.SuppressExtLine1 = true;
                                aDim = dims.Vertical(v1, v2, 0.45, aSuffix: " (TYP)");


                                //the tab depth
                                Image.DimStyleOverrides.SuppressExtLine1 = true;
                                aDim = dims.Horizontal(aVerts.NextVector(v1, -1), v1, -0.45, aSuffix: "\\P(TYP)");
                                Image.DimStyleOverrides.SuppressExtLine1 = false;
                            }
                            else
                            {
                                aPl = (dxePolyline)vRight.AdditionalSegments.GetTagged("GUSSET");
                                if (aPl != null)
                                {
                                    dxeLine l1 = aPl.Vertices.LineSegment(2, true);
                                    dxeLine l2 = aPl.Vertices.LineSegment(2);

                                    dims.Angular(l1, l2, 0.35 * l2.Length);
                                    v2 = vRight.Vertex(5, true);
                                    v1 = aPl.Vertex(3, true);

                                    aDim = dims.Vertical(v1, v2, 0.55, aSuffix: " (REF)");
                                }

                            }


                        }
                        else
                        {
                            if (DrawlayoutRectangles)
                                Tool.NumberVectors(draw, aVerts, paperscale);

                            //the tab height
                            if (!aEPlate.BoltOn)
                            {
                                v1 = aVerts.GetVector(dxxPointFilters.GetRightBottom);
                                v2 = aVerts.GetVector(dxxPointFilters.GetTopRight);
                                aDim = dims.Vertical(v1, v2, 0.65, aSuffix: " (TYP)");

                                //the tab depth
                                Image.DimStyleOverrides.SuppressExtLine1 = true;
                                aDim = dims.Horizontal(aVerts.NextVector(v1, -1), v1, -0.45, aSuffix: "\\P(TYP)");
                            }
                        }
                    }

                    if (bBltOn)
                    {
                        segs = vRight.AdditionalSegments.GetByGraphicType(dxxGraphicTypes.Arc);

                        aVerts = segs.Centers(bReturnClones: true);
                        aVerts.RemoveCoincidentVectors();
                        if (aVerts.Count > 0)
                        {
                            v1 = vRight.GetVertex(dxxPointFilters.GetTopRight, bReturnClone: true);


                            aVerts.Add(v1);
                            Image.DimTool.Stack_Vertical(aVerts, 0.35 * paperscale, aBotToTopLeftToRight: false);
                            ents = Image.SelectionSet(dxxSelectionTypes.CurrentSet);
                            bRec = ents.BoundingRectangle();
                            v3 = new dxfVector(elevBounds.Right + 0.5 * paperscale - bRec.Left);
                            ents.Translate(v3);
                            bRec.Translate(v3);
                            ents.UpdateImage();
                            vRight.Translate(v3);
                            arc1 = (dxeArc)segs.GetByPoint(dxxPointFilters.AtMinY, aEntPointType: dxxEntDefPointTypes.Center, aEntityType: dxxEntityTypes.Circle);
                            v2 = vRight.GetVertex(dxxPointFilters.GetLeftBottom, bReturnClone: true);
                            dims.Horizontal(v2, arc1.Center, -0.45);

                            arc1 = (dxeArc)segs.GetByPoint(dxxPointFilters.AtMaxY, aEntPointType: dxxEntDefPointTypes.Center, aEntityType: dxxEntityTypes.Circle);
                            txt = Tool.HoleLeader(Image, segs.Item(1), segs.Count * 2, false);
                            txt = (Assy.Bolting == uppUnitFamilies.English) ? txt = $"{txt}\\PSUPPLY 3/8 x 1 UNC HEX HD BOLT," : txt = $"{txt}\\PSUPPLY M10 x 25mm HEX HD BOLT,";
                            txt = $"{txt}\\PNUT AND LOCKWASHER FOR\\PEACH HOLE. SHIP FULLY TIGHT TO\\PDOWNCOMER AND END SUPPORT.";


                            v1 = arc1.AnglePoint(45, aPlane: dxfPlane.World);
                            v2 = new dxfVector(v1.X + 0.35 * paperscale, bRec.Top + 0.15 * paperscale);
                            v3 = new dxfVector(bRec.Right + 0.1 * paperscale, v2.Y);

                            if (DrawlayoutRectangles)
                                draw.aPolyline(bRec, new dxfDisplaySettings(aColor: dxxColors.Green));


                            aLdr = draw.aLeader.Text(new List<dxfVector> { v1, v2, v3 }, v3, txt, null);

                            designernote = $"CONFIRM BOLT HOLE LOCATIONS AND FIT UP OF ENDPLATE TO DC BODY!";


                        }
                    }

                    var rents = Image.SelectionSetInit( true );
                    rightBounds = rents.BoundingRectangle();

                    if (!bTria)
                    {
                        //================ LEft VIEW ===============================================
                        Status = "Drawing Left View Geometry";
                        {
                            if (bGust)
                            {
                                vLeft = aEPlate.BottomSide ? aEPlate.View_LayoutRight(out dxePolygon _, dxfVector.Zero, -90) : aEPlate.View_LayoutLeft(out dxePolygon _, dxfVector.Zero, -90);
                            }
                            else
                            {
                                vLeft = vRight.Clone();
                                vLeft.Mirror(vLeft.Plane.YAxis());
                                vLeft.MoveTo(dxfVector.Zero);
                            }
                            v1 = new dxfVector(elevBounds.Left - oset, vElev.InsertionPt.Y);

                            draw.aInsert(vLeft, v1, aBlockName: $"{bname}_PROFILE_LEFT", aDisplaySettings: dsp);
                            vLeft.MoveTo(v1);
                            //Image.Entities.Add(vLeft);

                            if (DrawlayoutRectangles)
                            {
                                draw.aSymbol.Axis(vLeft.Plane);
                                Tool.NumberVectors(draw, vLeft.Vertices, paperscale);
                            }
                        }

                        //================ LEft VIEW DIMENSIONS ===============================================
                        Status = "Drawing Left View Dimensions";
                        {
                            if (aEPlate.BoltOn)
                            {
                                //the tab height
                                v1 = vLeft.GetVertex(dxxPointFilters.GetLeftBottom);
                                v2 = vLeft.GetVertex(dxxPointFilters.GetTopLeft);
                                aDim = dims.Vertical(v1, v2, -0.65, aSuffix: " (TYP)");


                                //the tab depth
                                Image.DimStyleOverrides.SuppressExtLine1 = true;
                                aDim = dims.Horizontal(vLeft.Vertices.NextVector(v1, -1), v1, -0.45, aSuffix: "\\P(TYP)");
                            }
                        }

                        if (bGust)
                        {

                            if (aEPlate.BottomSide)
                            {
                                //the tab height
                                v1 = vLeft.GetVertex(dxxPointFilters.GetLeftBottom);
                                v2 = vLeft.GetVertex(dxxPointFilters.GetTopLeft);
                                Image.DimStyleOverrides.SuppressExtLine1 = true;
                                aDim = dims.Vertical(v1, v2, -0.55, aSuffix: " (TYP)");


                                //the tab depth
                                Image.DimStyleOverrides.SuppressExtLine1 = true;
                                aDim = dims.Horizontal(vLeft.Vertices.NextVector(v1, -1), v1, -0.45, aSuffix: "\\P(TYP)");


                            }
                            else
                            {
                                aPl = (dxePolyline)vLeft.AdditionalSegments.GetTagged("GUSSET");
                                if (aPl != null)
                                {
                                    dxeLine l1 = aPl.Vertices.LineSegment(2, true);
                                    dxeLine l2 = aPl.Vertices.LineSegment(2);

                                    dims.Angular(l1, l2, -0.35 * l2.Length);
                                    v2 = vLeft.Vertex(5, true);
                                    v1 = aPl.Vertex(3, true);

                                    aDim = dims.Vertical(v1, v2, -0.55, aSuffix: " (REF)");
                                }
                            }
                        }
                    }
                }

                //================ CENTER THE OUTPUT ===============================================
                Status = "Centering Output";
                {
                    ents = Image.Entities.SubSet(iStart, Image.Entities.Count);
                    bRec = ents.BoundingRectangle();
                    v1 = new dxfVector((fitRec.Left + 1.5 * paperscale) - bRec.Left, (fitRec.Top - 0.75 * paperscale) - bRec.Top);
                    //if (bTria) v1.X -= 0.75 * vRight.BoundingRectangle().Width;
                    ents.Translate(v1);
                    bRec.Translate(v1);
                    rightBounds.Translate( v1 );
                }
                //================ NOTES ===============================================
                {
                    if (bGust || !string.IsNullOrWhiteSpace(designernote))
                    {
                        if (bGust)
                        {
                            txt = "CONFIRM GUSSET DIMENSIONS AND CHECK FOR INTERFERENCES.";
                            designernote = string.IsNullOrWhiteSpace(designernote) ? txt : $"{designernote}\\P{txt}";
                        }
                        draw.aText(fitRec.TopLeft, $"DESIGNER NOTE: {designernote}", 0.1875 * paperscale, dxxMTextAlignments.TopLeft, aTextStyle: "Standard", aColor: dxxColors.Red);
                    }

                    ents = Image.Entities.SubSet(iStart, Image.Entities.Count);
                    bRec = ents.BoundingRectangle();

                    var noteIns = Tool.AddBorderNotes(aEPlate, Project, "END PLATE", true, Image, fitRec, paperscale);
                    var noteRec = noteIns.BoundingRectangle();

                    if (noteRec.Left < bRec.Right)
                    {
                        Status = "Adjusting for overlapping border notes";
                        v1 = new dxfVector((fitRec.Left + 0.5 * paperscale) - bRec.Left, (fitRec.Top - 0.5 * paperscale) - bRec.Top);
                        ents.Translate(v1);
                        bRec.Translate(v1);
                        rightBounds.Translate(v1);
                        v1 = new dxfVector(0.5 * paperscale, (rightBounds.Bottom - 0.5 * paperscale) - noteRec.Top);
                        noteIns.Translate(v1);
                    }

                    //Tool.Add_B_BorderNotes(Image, notes, fitRec, paperscale);
                }


            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
            finally
            {
                Status = "";
                vPlan?.Dispose();
                vElev?.Dispose();
                vRight?.Dispose();
                vLeft?.Dispose();
            }
        }

        private bool LeftSideViewMustBeDrawnForEndPlate(dxePolygon planView)
        {
            var allVertices = planView.GetVertices();
            var leftTopVertex = allVertices.GetVector(dxxPointFilters.GetLeftTop);
            var rightTopVertex = allVertices.GetVector(dxxPointFilters.GetRightTop);

            var shouldDrawLeftView = leftTopVertex.Y < rightTopVertex.Y;

            return shouldDrawLeftView;
        }

        private void DDWG_DeflectorPlate(int aIndex)
        {

            dxfVector v2;
            dxfVector v3;
            dxfRectangle fitRec = null;
            colDXFEntities dwgEnts = new();
            colDXFEntities aRads;
            dxeDimension aDim;
            dxeLeader aLdr;
            colDXFVectors aHoles;
            dxeArc aArc;
            double paperscale = 0;

            bool ntchs;


            try
            {
                mdBaffle aBaffle = (mdBaffle)Drawing.Part;

                if (aBaffle == null) return;
                MDRange ??= aBaffle.GetMDRange(null);
                dxfRectangle aRec = new(aBaffle.Length, aBaffle.Height);
                dxoDrawingTool draw = Image.Draw;
                dxoDimTool dims = Image.DimTool;
                Status = $"Drawing Deflector {aBaffle.PartNumber}";

                //=====================================================================================================
                // insert the border
                Tool.Border(this, aRec, ref fitRec, ref paperscale, 4, 4);
                xCancelCheck();

                //=====================================================================================================
                if (DrawlayoutRectangles)
                {
                    Image.Screen.Entities.aRectangle(fitRec, aColor: dxxColors.LightGrey);
                }

                dxePolygon vRight = aBaffle.View_Profile(bLongSide: true, aCenter: dxfVector.Zero, aLayerName: "GEOMETRY", bAddSlotPoints: true);

                vRight.Orthoganolize();
                double tht = 0.125 * paperscale;
                double tgap = 0.05 * paperscale;
                double oset = 0.5 * paperscale;
                double d1;
                dxfVector v1;
                dxfVector v0;
                dxfRectangle geobounds;
                colDXFVectors DimPts = new();
                ntchs = vRight.Vertices.GetByTag("NOTCH").Count > 0;

                //================ RIGHT VIEW ===============================================

                Status = "Drawing Profile View Geometry";
                {
                    aRec = vRight.Vertices.BoundingRectangle();
                    v1 = fitRec.MiddleCenter() - aRec.MiddleCenter();
                    dwgEnts.Add(draw.aInsert(vRight, v1, aBlockName: $"DEFLECTOR_PLATE_{aBaffle.PartNumber}_PROFILE"));
                    vRight.Translate(v1);

                    //Image.LinetypeLayers.ApplyTo(vRight, dxxLinetypeLayerFlag.ForceToLayer);
                    //dwgEnts.Add(Image.Entities.Add(vRight));

                    v0 = vRight.GetVertex(dxxPointFilters.GetBottomLeft, bReturnClone: true);
                    geobounds = vRight.BoundingRectangle();
                }


                //================ RIGHT VIEW DIMENSIONS ====================================


                Status = "Drawing Horizontal Dimensions";
                {
                    aHoles = vRight.AdditionalSegments.Centers(dxxEntityTypes.Point);
                    DimPts.Append(aHoles, true);
                    DimPts.RemoveCoincidentVectors();
                    v1 = vRight.GetVertex(dxxPointFilters.GetBottomRight);
                    DimPts.Add(v1.X, v1.Y);
                    DimPts.Sort(dxxSortOrders.LeftToRight);

                    v1 = v0.Moved(aYChange: -oset);
                    aDim = dims.OrdinateH(v0, v0, v1.Y, v1.X, aAbsolutePlacement: true);
                    dwgEnts.Add(aDim);
                    v1.Move(tht + tgap);

                    for (int i = 1; i <= DimPts.Count; i++)
                    {
                        v2 = DimPts.Item(i);
                        v1.X = Math.Max(v1.X, v2.X);
                        aDim = dims.OrdinateH(v0, v2, v1.Y, v1.X, aAbsolutePlacement: true);
                        dwgEnts.Add(aDim);
                        v1.Move(tht + tgap);
                    }

                    d1 = vRight.GetOrdinate(dxxOrdinateTypes.MaxY, false);
                    DimPts = vRight.Vertices.GetAtCoordinate(aY: d1, aPrecis: 1, bReturnClones: true);
                    DimPts.GetAtCoordinate(v0.X, aPrecis: 1, bRemove: true);
                    DimPts.GetAtCoordinate(vRight.GetOrdinate(dxxOrdinateTypes.MaxX), aPrecis: 1, bRemove: true);

                    if (DimPts.Count > 0)
                    {
                        DimPts.Sort(dxxSortOrders.LeftToRight);
                        v2 = DimPts.Remove(1);
                        v1 = v2.Clone();
                        v1.Move(aYChange: oset);
                        aDim = dims.OrdinateH(v0, v2, v1.Y, v1.X, aAbsolutePlacement: true);
                        dwgEnts.Add(aDim);
                        v1.Move(tht + tgap);

                        for (int i = 1; i <= DimPts.Count; i++)
                        {
                            v2 = DimPts.Item(i);
                            if (v2.X > v1.X) v1.X = v2.X;

                            aDim = dims.OrdinateH(v0, v2, v1.Y, v1.X, aAbsolutePlacement: true);
                            dwgEnts.Add(aDim);
                            v1.Move(tht + tgap);
                        }
                    }
                }


                //================ RIGHT VIEW DIMENSIONS ====================================

                Status = "Drawing Vertical Dimensions";
                {
                    DimPts = new colDXFVectors(vRight.GetVertex(dxxPointFilters.GetTopLeft, bReturnClone: true));
                    if (ntchs)
                    {
                        colDXFVectors ntchvrts = vRight.Vertices.GetByTag("NOTCH");
                        v1 = ntchvrts.GetVector(dxxPointFilters.AtMinX);
                        if (v1 != null)
                        {
                            ntchvrts = vRight.Vertices.GetAtCoordinate(v0.X, v1.Y);
                            v2 = ntchvrts.Count > 0 ? ntchvrts.Item(1) : null;

                            DimPts.Add(v2 ?? v1, bAddClone: true);

                        }
                    }



                    DimPts.Add(aHoles.GetVector(dxxPointFilters.AtMinX), bAddClone: true);
                    DimPts.Add(v0, bAddClone: true);

                    List<dxeDimension> vDims = dims.Stack_Ordinate(dxxOrdinateDimTypes.OrdVertical, v0, DimPts, -0.35 * paperscale);

                    aRec = dwgEnts.BoundingRectangle();
                    aRec.Stretch(0.2 * paperscale);

                    if (DrawlayoutRectangles)
                    {
                        Image.Screen.Entities.aRectangle(aRec, true, dxxColors.LightGrey);
                    }
                }


                //================ STAMP PN LEADER =====================================
                Status = "Drawing Vertical Dimensions";
                {

                    v1 = geobounds.TopCenter;
                    v1 = vRight.Vertices.NearestVector(v1, bReturnClone: true);
                    v2 = vRight.Vertex(v1.Index - 1, true);
                    v3 = v1.Index + 1 <= vRight.VertexCount ? vRight.Vertex(v1.Index + 1, true) : geobounds.TopRight;
                    v1 = (v1.Y == v2.Y) ? v1.MidPoint(v2) : v1.MidPoint(v3);
                    v1.Y -= 0.1 * paperscale;

                    v2 = new dxfVector(v1.X + 0.375 * paperscale, aRec.Top + 2.75 * tht + tgap);
                    aLdr = draw.aLeader.Text(v1, v2, Tool.StampPNLeader(Image), null);
                    dwgEnts.Add(aLdr);
                    dwgEnts.Add(draw.aText(v1, "XXX", 0.5, dxxMTextAlignments.TopCenter, aTextStyle: "Standard"));
                }


                //================LEADERS ===================
                Status = "Drawing Leaders";
                {
                    colDXFEntities slots = vRight.AdditionalSegments.GetByEntityType(dxxEntityTypes.Polyline);
                    dxePolyline slot;
                    if (slots.Count > 0)
                    {

                        v1 = slots.Centers().GetVector(dxxPointFilters.AtMaxX);
                        slot = (dxePolyline)slots.GetByCenter(v1)[0];


                        v2 = new dxfVector(aRec.Right, geobounds.Top + (1.5 * tht + tgap));
                        dwgEnts.Add(Tool.CreateHoleLeader(Image, slot, v2, dims, slots.Count));
                    }
                }


                //================ RADIUS LEADER ===================

                if (ntchs)
                {
                    aRads = vRight.Segments.GetArcs();
                    if (aRads.Count > 0)
                    {
                        aArc = (dxeArc)aRads.GetByPoint(dxxPointFilters.AtMinX);
                        v1 = aArc.MidPt;
                        v2 = v1.Moved(-0.5 * paperscale);
                        v2.Y = aRec.Top + (1.5 * tht + tgap);
                        string txt = Tool.RadiusLeaderText(Image, Drawing.DrawingUnits, aArc.Radius, true);

                        dwgEnts.Add(draw.aLeader.Text(v1, v2, txt, null));
                    }
                }

                //================ AXIS =====================================

                v1 = aRec.BottomLeft;
                v1.Move(-tht, -3 * tht);
                dwgEnts.Add(draw.aSymbol.Axis(v1, 0.375 * paperscale, aTextHeight: 0.125, aTextStyle: "Standard"));


                //================ DISTRIBUTOR NOTE =====================================

                if (aBaffle.ForDistributor)
                {
                    v1 = fitRec.TopLeft;
                    v1.Move(tht, -3 * tht);
                    mdDistributor dstrib = (mdDistributor)MDProject.Distributors.Item(aBaffle.DistributorIndex, bSuppressIndexError: true);

                    string txt = dstrib != null ? $" DESIGNER NOTE: NOTCHES ARE REQUIRED FOR \\PFIT-UP WITH '{dstrib.DescriptiveName.ToUpper()}'" : $" DESIGNER NOTE: NOTCHES ARE REQUIRED FOR \\PFIT-UP WITH DISTRIBUTOR {aBaffle.DistributorIndex}";
                    draw.aText(v1, txt, 0.1875 * paperscale, dxxMTextAlignments.TopLeft, aColor: dxxColors.Red);

                }





                //================ NOTES ===============================================
                Tool.AddBorderNotes(aBaffle, Project, "DEFLECTOR PLATE", false, Image, fitRec, paperscale);

                Status = "";
            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
        }

        private void DDWG_SuplDeflector(mdSupplementalDeflector aDeflc)
        {
            if (aDeflc == null) return;
            MDRange ??= aDeflc.GetMDRange(null);
            dxfVector v1;


            dxfRectangle fitRec = null;
            colDXFEntities dwgEnts = new();
            dxePolygon vRight;


            double paperscale = 0;

            try
            {

                if (Assy == null) throw new Exception("Unable To find parent tray assembly");
                mdDowncomer aDowncomer = Assy.Downcomer(aDeflc.DowncomerIndex);
                if (aDowncomer == null) throw new Exception("Unable To find parent downcomer");
                aDeflc = aDowncomer.SupplementalDeflector();
                dxfRectangle aRec = new(aDeflc.Length, aDeflc.Height);

                Status = $"Drawing Deflector {aDeflc.PartNumber}";

                //=====================================================================================================
                // insert the border
                Tool.Border(this, aRec, ref fitRec, ref paperscale, 2, 2);
                xCancelCheck();

                //=====================================================================================================
                if (DrawlayoutRectangles)
                {
                    Image.Screen.Entities.aRectangle(fitRec, aColor: dxxColors.LightGrey);
                }

                vRight = aDeflc.View_Profile(fitRec.Center);

                vRight.Orthoganolize(0.01);

                //================ RIGHT VIEW ===============================================
                Status = "Drawing Profile View Geometry";

                aRec = vRight.BoundingRectangle();
                v1 = fitRec.TopCenter;
                v1 -= aRec.TopCenter;
                v1.Move(aYChange: -1.75 * paperscale);
                vRight.Translate(v1);

                dwgEnts.Add(Image.Entities.Add(vRight));

                //================ RIGHT VIEW DIMENSIONS ====================================

                Status = "Drawing  Dimensions";

                v1 = vRight.GetVertex(dxxPointFilters.GetBottomRight, bReturnClone: true);
                dwgEnts.Add(Image.Draw.aDim.Horizontal(vRight.GetVertex(dxxPointFilters.GetBottomLeft), v1, -0.45, aImage: _Image));
                dwgEnts.Add(Image.Draw.aDim.Vertical(v1, vRight.GetVertex(dxxPointFilters.GetTopRight), 0.45, aImage: _Image));

                //================== NOTES ==================================



                AppDrawingBorderData notes = new()
                {
                    PartNumber_Value = $"SUPPLEMENTAL DEFLECTOR (SUB-PART)",
                    ForParts_Value = $"FOR DOWNCOMER {aDowncomer.PartNumber}",
                    ForMaterial_Tag = "FOR MATERIAL SEE SHEET 2",
                    NumberRequired_Value = $"NUMBER REQUIRED: {string.Format("{0:#,0}", aDowncomer.OccuranceFactor * Assy.TrayCount)}",
                    Location_Value = $"FOR TRAYS {Assy.SpanName()}",
                };
                Tool.AddBorderNotes(aDeflc, Project, "SUPPLEMENTAL DEFLECTOR", true, Image, fitRec, paperscale);

                //Tool.Add_B_BorderNotes(Image, notes, fitRec, paperscale);
            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
            finally
            {
                Status = "";
                DrawinginProgress = false;
            }
        }

        private void DDWG_DC_Box()
        {

            mdDowncomerBox aBox = (mdDowncomerBox)Drawing.Part;

            if (aBox == null) return;

            mdDowncomer aDowncomer = aBox.GetMDDowncomer();
            if (aDowncomer == null) return;

            MDRange = aDowncomer.GetMDRange();

            colMDSpoutGroups aSGs = aBox.SpoutGroups(null);
            mdSpoutGroup aSG;
            dxoDimStyle DStyle = Image.DimStyle();
            colDXFEntities eShelfs;
            colDXFEntities aEnts;
            colDXFEntities eSpouts;
            colDXFEntities eHoles;

            colDXFEntities dwgEnts = new();
            colDXFEntities endViewEnts = new();
            colDXFEntities layoutEnts = new();
            colDXFEntities detEnts = new();
            colDXFVectors aPts;
            colDXFVectors bPts;
            colDXFVectors cPts;
            dxeInsert noteblock = null;
            colDXFVectors DimPts;

            dxfVector v0;
            dxfVector v1;
            dxfVector v2;
            dxfVector v3;
            dxfVector v4;
            dxfVector ctr = dxfVector.Zero;
            dxfPlane worldPln = dxfPlane.World;
            dxfRectangle dRec;
            dxfRectangle endviewRec = null;
            dxfRectangle layoutviewRec = null;
            dxfRectangle sideviewRec = null;
            dxePolygon Perim;
            dxfRectangle fitRec = null;
            dxePolyline aPl = null;
            dxePolygon aPG = null;
            dxeArc aArc;

            dxeLine cLn;
            uopHole aHl;
            uopHoles aHls;
            dxfEntity aEnt;
            dxeDimension aDim;
            dxeLeader aLdr;
            dxeDimension endangleSlotLeader = null;
            dxeDimension startupLeader = null;
            dxeDimension fingerclipHoleLeader = null;
            dxeDimension appHoleLeader = null;
            dxeDimension appSlotLeader = null;

            double perimRotation = 90;
            double paperscale = 0;
            double d1;
            double d2;


            string bname;
            string txt;
            double tgap;
            double oset;
            double thk = aBox.Thickness;
            dxfVector star1 = null;
            dxfVector star2 = null;
            double maxY;
            double minX;
            hdwHexNut APNut = (Assy.HasAntiPenetrationPans || Assy.DesignOptions.CrossBraces) & aBox.WeldedBottomNuts ? aBox.SmallBolt(bWeldedInPlace: true).GetNut() : null;
            dxeInsert ins;
            double sFldv = aBox.FoldOverHeight;
            bool bCentral = Math.Round(aBox.X, 1) == 0;
            colDXFVectors vPts = new();
            List<double> ords;
            colDXFVectors apPts = null;
            dxfRectangle aRec = null;
            try
            {
                Image.DimStyle().ExtLineOffset = 0.04;
                dxoDrawingTool draw = Image.Draw;
                dxoDimTool dims = Image.DimTool;
                dxoLeaderTool leaders = Image.LeaderTool;

                List<string> spoutTableText = new() { "LINE|SPOUTS|PATTERN|X PITCH" };

                //============== BORDER ===============================================================
                Status = $"Drawing DC {aBox.PartNumber}";
                {
                    var minWidthToHeightRatio = 5;
                    var initialRecWidth = aBox.LongLength + aBox.Width + 2;
                    var initialRecHeight = 2 * aBox.Height + aBox.Width;

                    if (initialRecWidth >= minWidthToHeightRatio * initialRecHeight)
                    {
                        aRec = new dxfRectangle(initialRecWidth, initialRecHeight);
                    }
                    else
                    {
                        aRec = new dxfRectangle(minWidthToHeightRatio * initialRecHeight, initialRecHeight);
                    }

                    // insert the border
                    // long and skinny

                    Tool.Border(this, aRec, ref fitRec, ref paperscale, 4.5, 4);
                    xCancelCheck();
                    tgap = Image.DimStyle().TextGap * paperscale;
                    oset = 0.25 * paperscale;
                    maxY = fitRec.Top;

                    if (DrawlayoutRectangles)
                        draw.aPolyline(fitRec, new dxfDisplaySettings(aColor: dxxColors.LightBlue));

                }

                //================ END VIEW ===============================================
                Status = "Drawing End View";
                {
                    xCancelCheck();

                    Image.SelectionSetInit(false);
                    v1 = fitRec.TopRight;
                    v1.Move(-0.5 * aBox.Width - 1 - 0.75 * paperscale, -aBox.How - 0.75 * paperscale);

                    Perim = aBox.View_Elevation(Assy, true, true, true, dxfVector.Zero, 0, "GEOMETRY");
                    Perim.ImageGUID = Image.GUID;


                    if (sFldv > 0)
                    {
                        aPl = new dxePolyline(Perim.Vertices.GetByTag("FOLDOVER", "RIGHT", bReturnClones: true), false);

                        v1 = aPl.Vertices.Item(1, bReturnClone: true).Moved(0, -0.375);
                        aPl.Vertices.Add(v1, aBeforeIndex: 1);
                        v1 = aPl.Vertices.Add(Perim.Vertex(1), bAddClone: true).Moved(0, -0.375);
                        aPl.Vertices.Add(v1);

                        aArc = aPl.Vertices.BoundingCircle();
                        aPl.Rescale(4, aArc.Center);
                        aArc.Rescale(1.35);
                        aArc.Linetype = dxfLinetypes.Phantom;
                        aArc.Color = dxxColors.LightGrey;
                        Perim.AddAdditionalSegment(aArc.Clone());

                        v1 = aArc.AnglePoint(80);
                        v2 = aArc.AnglePoint(80, false, 0.65 * paperscale);
                        Perim.AdditionalSegments.AddLine(v1, v2, dxfDisplaySettings.Null(aColor: dxxColors.LightGrey, aLinetype: dxfLinetypes.Phantom));

                        aArc.Rescale(4);
                        v1 = aArc.AnglePoint(260);
                        aArc.MoveFromTo(v1, v2);
                        Perim.AddAdditionalSegment(aArc, bAddClone: true);
                        aPl.MoveFromTo(v1, v2);
                        aPl.MoveFromTo(aPl.Vertices.BoundingCircle().Center, aArc.Center);
                        cLn = new dxeLine(aPl.Vertex(2), aPl.Vertex(1));
                        aPl.Vertex(1).MoveTo(cLn.IntersectPt(aArc, true, true));
                        v1 = aPl.Vertices.LastVector();
                        cLn.StartPt = aPl.Vertex(aPl.VertexCount - 1);
                        cLn.EndPt = v1.Clone();
                        v1.MoveTo(cLn.IntersectPt(aArc, true, true));

                        Perim.AddAdditionalSegment(aPl.Clone(), aTag: "FOLD_DETAIL");

                    }

                    bname = $"DC_{aBox.PartNumber}_END_VIEW_DETAIL";
                    ins = draw.aInsert(Perim, v1, aBlockName: bname, aDisplaySettings: dxfDisplaySettings.Null(aLayer: "GEOMETRY"));
                    bname = ins.BlockName;
                    Perim.MoveTo(ins.InsertionPt);


                    v1 = Perim.GetVertex(dxxPointFilters.GetBottomRight, bReturnClone: true);
                    v2 = Perim.GetVertex(dxxPointFilters.GetBottomLeft, bReturnClone: true);
                    aDim = dims.Horizontal(v1, v2, -0.35, aImage: Image);
                    maxY = aDim.BoundingRectangle().Bottom - 0.25 * paperscale;
                    List<dxePolyline> plines = Perim.AdditionalSegments.Polylines();
                    dxePolyline rightshelf = plines.Find(x => string.Compare(x.Tag, "SHELF", true) == 0 && string.Compare(x.Flag, "RIGHT", true) == 0);

                    aEnt = Perim.AdditionalSegments.GetTagged("SHELF", "RIGHT", bReturnClone: true);
                    if (rightshelf != null)
                    {
                        v2 = rightshelf.GetVertex(dxxPointFilters.GetTopRight, bReturnClone: true);
                        v1.Move(aYChange: aBox.Height);
                        dims.Vertical(v2, v1, 0.2, aImage: Image);
                        aPl = plines.Find(x => string.Compare(x.Tag, "FOLD_DETAIL", true) == 0);

                        if (aPl != null)
                        {
                            txt = DStyle.FormatNumber(Math.Abs(aPl.Vertex(5).Y - aPl.Vertex(3).Y) / 4);
                            dims.Vertical(aPl.Vertex(5), aPl.Vertex(3), 0.2, aOverideText: txt);

                            txt = DStyle.FormatNumber(Math.Abs(aPl.Vertex(2).X - aPl.Vertex(4).X) / 4) + " (MAX)";
                            dims.Horizontal(aPl.Vertex(4), aPl.Vertex(2), 0.45, aOverideText: txt);
                        }

                    }

                    xCancelCheck();

                    aDim = dims.Vertical(v1, v1.Moved(aYChange: -aBox.Height), 0.375, aImage: Image);
                    if (aDim.TextPrimary.BoundingRectangle().Left < (v1.X + 0.25 * paperscale))
                        aDim.Extend(v1.X + 0.25 * paperscale - aDim.TextPrimary.BoundingRectangle().Left);

                    dwgEnts = Image.SelectionSet(dxxSelectionTypes.CurrentSet);

                    dRec = dwgEnts.BoundingRectangle();
                    aPts = dRec.Corners();

                    v3 = v1.Moved(-aBox.Width);
                    v1 = Perim.BoundingRectangle().BottomCenter;
                    v1.Y = maxY;
                    txt = @"\fArial Narrow|b1|;\H1.15x;END VIEW OF BODY\PAFTER FORMING\P\fArial Narrow|b0|;\H1x;SCALE 1x";
                    //txt = $"{txt}\\P\\FRomanS.shx;\\H{(0.125 * paperscale)};SCALE 1x";
                    draw.aText(v1, txt, 0.125 * paperscale, dxxMTextAlignments.TopCenter, aTextStyle: "Standard");

                    v2 = (sFldv <= 0) ? v3.Moved(aYChange: -0.5 * aBox.How) : v3.Moved(-(0.375 - thk), -0.5 * sFldv);

                    txt = Tool.StampPNLeader(Image, false).Replace("HIGH", @"\PHIGH");
                    txt = $"{txt} ON LONG SIDE, NEAR\\PLONGITUDINAL CENTER, NEAR TOP OF BODY";
                    aPts.Add(v2, bAddClone: true, aTag: txt);


                    xCancelCheck();

                    aEnt = Perim.AdditionalSegments.GetTagged("SHELF", "LEFT", bReturnClone: true);
                    if (aEnt != null)
                    {
                        v2 = aEnt.Vertices.GetVector(dxxPointFilters.GetTopLeft, bReturnClone: true);
                        v2.Move(0.25, -thk);
                        txt = (Drawing.DrawingUnits == uppUnitFamilies.English) ? @"SUPPORT ANGLES\P1 IN. x 1 IN. x BODY THK.(MIN)" : @"SUPPORT ANGLES\P25.4 mm x 25.4 mm x BODY THK.(MIN)";
                        aPts.Add(v2, bAddClone: true, aTag: txt);
                    }

                    draw.aPolyline(aPts.BoundingRectangle(), aDisplaySettings: new dxfDisplaySettings(aColor: dxxColors.Green));


                    leaders.Stacks(aPts, aLeaderLength: 0.25 * paperscale, aXOffset: 0.125 * paperscale, aCenterPt: null, aSpaceAdder: 0.125 * paperscale, aLeftLeaderAngle: 65);


                    endViewEnts = Image.SelectionSetInit(true);

                    aLdr = endViewEnts.Leaders().LastOrDefault();

                    endviewRec = endViewEnts.BoundingRectangle();
                    //v1 = fitRec.TopRight - endviewRec.TopRight;

                    //endViewEnts.Translate(v1);
                    //endviewRec.Translate(v1);
                    ////dwgEnts.UpdateImage(true);
                    //if (DrawlayoutRectangles)
                    //    draw.aPolyline(endviewRec, aDisplaySettings: new dxfDisplaySettings(aColor: dxxColors.LightGrey));

                    xCancelCheck();
                }

                //================ GEOMETRY ===============================================

                Status = "Creating Layout Geometry";
                {
                    Image.SelectionSetInit(false);
                    v1 = dxfVector.Zero;
                    // calculate then insertion pt coordinates

                    if (aLdr != null) maxY = aLdr.MText.BoundingRectangle().Bottom - 0.0625 * paperscale;

                    d2 = Math.Truncate(aRec.Width) + 0.44444;
                    d1 = Image.DimStyle().CreateDimensionText(d2, aImage: Image, aSuffix: ".").BoundingRectangle().Width;
                    d1 = d1 + oset + tgap + 0.5 * aRec.Height + 0.65 * paperscale;
                    maxY -= d1;

                    minX = fitRec.Left + 0.0625 * paperscale;
                    d2 = aRec.Width;

                    d1 = 0.65 * paperscale + oset + 2 * tgap + 0.5 * aRec.Width + Image.DimStyle().CreateDimensionText(0, _Image).BoundingRectangle().Width;
                    minX += d1;
                    xCancelCheck();


                    v1.SetCoordinates(minX, maxY);
                    //If(bCentral} perimRotation = 270;
                    Perim = aBox.View_Layout(Assy, bSuppressSpouts: false, aCenter: dxfVector.Zero, aRotation: perimRotation, aLayerName: "GEOMETRY");

                }

                Status = "Drawing Geometry";
                {
                    bname = $"DC_{aBox.PartNumber}_LAYOUT_MFG";
                    ins = draw.aInsert(Perim, v1, aBlockName: bname, aDisplaySettings: dxfDisplaySettings.Null(aLayer: "GEOMETRY"));
                    bname = ins.BlockName;
                    //Perim.MoveTo(ins.InsertionPt);
                    BlockTransform( Perim, ins );

                    aRec = ins.BoundingRectangle(worldPln);
                    v0 = Perim.GetVertex(dxxPointFilters.GetBottomLeft, bReturnClone: true);


                    dwgEnts = aRec.BorderLines();
                    d1 = aRec.Width;
                    v1 = aRec.Center;
                    cLn = draw.aLine(v1.X - d1 / 2, v1.Y, v1.X + d1 / 2, v1.Y, aLineType: dxfLinetypes.Center);
                    dwgEnts.Add(cLn);

                    eSpouts = Perim.AdditionalSegments.GetByTag("SPOUT");
                    eShelfs = Perim.AdditionalSegments.GetByTag("SHELF");

                    xCancelCheck();

                }

                //================  DIMENSIONS ===============================================

                Status = "Creating Dimension Points";
                {
                    ctr = aRec.Center;

                    //get the hole centers
                    eHoles = Perim.AdditionalSegments.GetByTag("FINGER CLIP,STARTUP,END ANGLE,ENDPLATE,CROSSBRACE", aEntityType: dxxEntityTypes.Point, aDelimitor: ",");
                    DimPts = eHoles.Centers(bReturnClones: true);

                    DimPts.SetTagsAndFlags("H", "NOV");
                    eHoles = Perim.AdditionalSegments.GetByTag("APPAN_HOLE,APPAN_SLOT", aEntityType: dxxEntityTypes.Point, aDelimitor: ",");
                    apPts = eHoles.Centers(bReturnClones: true);
                    apPts.SetTagsAndFlags(null, "HV");
                    //DimPts.Append(aPts, true, aFlag: "HV");

                    aPts = Perim.Vertices.Clone();
                    DimPts.Append(aPts, true);
                    DimPts.SetTagsAndFlags(aFlag: "H", aSearchTag: "CROSSBRACE,APPAN_HOLE,APPAN_SLOT");
                    DimPts.Append(Perim.BoundingRectangle().Stretched(oset, true, true, bMaintainOrigin: true).Corners(), false, aTag: "LIMITS", aFlag: "NODIM");

                    xCancelCheck();

                    //------------------ SPOUTS ----------------------------------------
                    // add the spout group reference holes to the dimension points

                    aPts = eSpouts.DefinitionPoints(dxxEntDefPointTypes.Center, bReturnClones: true);
                    txt = "";
                    colDXFVectors aLnPts = new();
                    colDXFVectors bLnPts = new();
                    cPts = new colDXFVectors();
                    colDXFVectors dPts;
                    List<dxfEntity> ents;

                    for (int i = 1; i <= aSGs.Count; i++)
                    {
                        aSG = aSGs.Item(i);

                        txt = $"LINE {i}";
                        v4 = dxfVector.Zero;
                        dPts = aPts.GetByFlag(aSG.Handle);
                        bPts = new colDXFVectors(dPts.GetVectors(aFilter: dxxPointFilters.LessThanX, aOrdinate: ctr.X, bOnIsIn: true));

                        v1 = bPts.GetVector(dxxPointFilters.GetLeftTop);
                        ents = eSpouts.GetByDefPoint(v1, dxxEntDefPointTypes.Center, bGetJustOne: true);
                        if (ents.Count > 0)
                        {

                            aEnt = ents[0];

                            if (v1.Y >= ctr.Y)
                            {
                                v2 = aEnt.Segmented().Centers().GetVector(dxxPointFilters.AtMaxY);
                                v3 = v2.Projected(worldPln.Direction(90 - 15), aSG.SpoutDiameter() / 2);
                                aLnPts.Add(v3, aTag: txt, aFlag: "");
                                v4 = aEnt.BoundingRectangle().TopCenter;
                            }
                            else
                            {
                                v2 = aEnt.Segmented().Centers().GetVector(dxxPointFilters.AtMinY);
                                v3 = v2.Projected(worldPln.Direction(270 + 15), aSG.SpoutDiameter() / 2);
                                v4 = v3.Moved(0.2 * paperscale, -0.2 * paperscale);
                                leaders.Text(v3, v4, txt);
                                v4 = aEnt.BoundingRectangle().BottomCenter;
                            }

                            dxfVector cp = new(aEnt.X, aEnt.Y);
                            DimPts.Add(cp, bAddClone: true, aTag: "H", aFlag: "NOV");
                            aSG.GetMirrorValues(out double? mirrX, out double? mirrY);
                            if (mirrY.HasValue)
                            {
                                cp = cp.Moved(2 * (ctr.X - cp.X));
                                DimPts.Add(cp, bAddClone: true, aTag: "H", aFlag: "NOV");
                            }
                            if (aSG.PatternType == uppSpoutPatterns.Astar && star1 == null)
                                star1 = cp.Clone();

                            if (aSG.PatternType == uppSpoutPatterns.SStar)
                                star2 = cp.Clone();
                            cPts.Add(v4, bAddClone: true, aTag: "H", aFlag: "NOV");
                            v4.Y = v1.Y;


                            //add the mirrored leader
                            if (mirrY.HasValue)
                            {
                                v2 = v2.Moved(2 * (ctr.X - v2.X));
                                v3 = v3.Moved(2 * (ctr.X - v3.X));
                                v4 = v4.Moved(2 * (ctr.X - v4.X));
                                cPts.Add(v4, bAddClone: true, aTag: "H");


                                v4.Y = v1.Y;
                                if (v1.Y >= ctr.Y)
                                {
                                    bLnPts.Add(v3, aTag: txt, aFlag: "");
                                }
                                else
                                {
                                    v4 = v3.Moved(-0.2 * paperscale, -0.2 * paperscale);
                                    leaders.Text(v3, v4, txt);
                                }
                            }
                        }

                        aHls = aSG.Spouts;
                        aHl = aHls.Item(1);
                        DStyle.SetZeroSuppressionDecimal(false, false);

                        xCancelCheck();
                        txt = $"{i}|";
                        if (aSG.PatternType == uppSpoutPatterns.SStar)
                        {
                            txt = $"{txt}{Tool.HolesString(aHls, Drawing.DrawingUnits, DStyle, "\\P")}|";
                        }
                        else
                        {
                            txt = $"{txt}{aSG.SpoutCount()} - {Tool.HoleLeader(Image, aHl, 0, false)}";
                            if (aSG.SpoutCount() > 1) txt = $"{txt}S";

                            txt = $"{txt}|";
                        }
                        txt = $"{txt}{aSG.PatternName}|";

                        txt = (aSG.RowCount > 1) ? $"{txt}{DStyle.FormatNumber(aSG.VerticalPitch)}" : txt = $"{txt}N\\\\A";

                        spoutTableText.Add(txt);
                        xCancelCheck();
                    }
                    if (aLnPts.Count > 0)
                    {
                        v1 = aLnPts.GetVector(dxxPointFilters.AtMaxY);

                        leaders.Stack_Horizontal(aLnPts, v1.Y + 0.1 * paperscale, bLeftToRight: true, bOffsetIsYOrdinate: true);
                        xCancelCheck();
                        leaders.Stack_Horizontal(bLnPts, v1.Y + 0.1 * paperscale, bOffsetIsYOrdinate: true);
                    }
                    xCancelCheck();
                }

                // draw the dimensions
                Status = "Drawing Dimensions";
                {
                    // horizontal bottom
                    bPts = new colDXFVectors(DimPts.GetVectors(aFilter: dxxPointFilters.LessThanY, aOrdinate: ctr.Y));
                    bPts.Add(v0.Clone(), aTag: "V");
                    bPts.Add(Perim.GetVertex(dxxPointFilters.GetBottomRight), aTag: "V");
                    bPts.Append(cPts.GetByTag("H").GetVectors(aFilter: dxxPointFilters.LessThanY, aOrdinate: ctr.Y), true);

                    cPts = apPts.GetByTag("APPAN_HOLE");
                    bPts.Append(cPts.GetVectors(aFilter: dxxPointFilters.LessThanY, aOrdinate: ctr.Y, bOnIsIn: false));

                    //to return the h dim ordinates that were dimensioned
                    ords = new List<double>();
                    List<dxeDimension> hDims_Bot = dims.Stack_Ordinate(dxxOrdinateDimTypes.OrdHorizontal, v0, bPts, -oset, aTagsToSkip: "APPAN_HOLE,APPAN_SLOT", rDimOrdinates: ords);
                    xCancelCheck();

                    // horizontal top
                    //passing the ords that were dimensioned on th ebottom so they are not dimensioned again
                    bPts = new colDXFVectors(DimPts.GetVectors(aFilter: dxxPointFilters.GreaterThanY, aOrdinate: ctr.Y, bOnIsIn: true));
                    bPts.Append(cPts.GetByTag("H").GetVectors(aFilter: dxxPointFilters.GreaterThanY, aOrdinate: ctr.Y), true);
                    bPts.Append(apPts.GetVectors(aFilter: dxxPointFilters.GreaterThanY, aOrdinate: ctr.Y, bOnIsIn: true));
                    List<dxeDimension> hDims_Top = dims.Stack_Ordinate(dxxOrdinateDimTypes.OrdHorizontal, v0, bPts, oset, aOrdinatesToSkip: ords);


                    xCancelCheck();

                    // vertical
                    v0.Y = cLn.Y;
                    bPts = new colDXFVectors(Perim.Vertices.GetByTag("INFLECTION").GetVectors(aFilter: dxxPointFilters.GreaterThanX, aOrdinate: ctr.X));
                    cPts = new colDXFVectors(Perim.AdditionalSegments.GetByTag("APPAN_SLOT").Centers().ToList());

                    if (bPts.Count > 0 || cPts.Count > 0 || star1 != null || star2 != null)
                    {
                        dims.OrdinateV(v0, v0, -oset / paperscale);
                        if (cPts.Count > 0) bPts.Append(cPts.GetVectors(aFilter: dxxPointFilters.AtMaxX), true, aTag: "V");
                        if (star1 != null) bPts.Add(star1);
                        if (star2 != null) bPts.Add(star2);
                        bPts.Add(cLn.EndPt.Clone(), aTag: "NODIM");
                        ords = new List<double>() { 0 };

                        List<dxeDimension> vDims_Right = dims.Stack_Ordinate(dxxOrdinateDimTypes.OrdVertical, v0, bPts, oset, 0, false, null, false, "LIMITS", false, aBox.BoltOnEndplates, aOrdinatesToSkip: ords);
                    }

                    xCancelCheck();
                    dwgEnts = Image.SelectionSetInit(true, true);
                    layoutviewRec = dwgEnts.BoundingRectangle();

                    xCancelCheck();

                    //================ OVERALL PLATE WIDTH ==================================================
                    v1 = Perim.GetVertex(dxxPointFilters.GetTopLeft, bReturnClone: true);
                    v2 = Perim.GetVertex(dxxPointFilters.GetBottomLeft, bReturnClone: true);


                    aDim = dims.Vertical(v1, v2, layoutviewRec.Left - 0.5 * paperscale, 0, "", @"\P(REF)", true);
                    xCancelCheck();
                }

                //================ LEADERS ===============================================
                Status = "Drawing Leaders";
                {
                    dxfRectangle lastRec = null;
                    Status = "Drawing Support Angle Leaders";
                    {
                        bool shortIsTop = aBox.RightLength < aBox.LeftLength;
                        string label1 = shortIsTop ? "SHORT" : "LONG";
                        string label2 = shortIsTop ? "LONG" : "SHORT";
                        aEnts = eShelfs.GetByTag( "SHELF", label1, aEntityType: dxxEntityTypes.Polyline );
                        if (aEnts.Count > 0)
                        {
                            v1 = aEnts.BoundingRectangle( worldPln ).TopLeft.Moved( 2 );
                            v2 = aDim.BoundingRectangle().TopCenter.Moved( 0.125 * paperscale, 2 * oset );
                            txt = (!bCentral) ? $"{label1} ANGLE\\P(CENTRAL) {DStyle.FormatNumber( aBox.ShelfAngle( bLeft: false ).Length )}" : $"BOTH ANGLES\\P(CENTRAL) {DStyle.FormatNumber( aBox.ShelfAngle( bLeft: false ).Length )}";
                            leaders.Text( v1, v2, txt, null );
                        }

                        if (!bCentral)
                        {
                            aEnts = eShelfs.GetByTag( "SHELF", label2, aEntityType: dxxEntityTypes.Polyline );
                            if (aEnts.Count > 0)
                            {
                                aPl = (dxePolyline)aEnts.GetByPoint( dxxPointFilters.AtMaxX, aEntPointType: dxxEntDefPointTypes.StartPt, aEntityType: dxxEntityTypes.Polyline );
                                v1 = aPl.GetVertex( dxxPointFilters.GetTopRight, bReturnClone: true );
                                txt = $"{label2} ANGLE\\P(CENTRAL) {DStyle.FormatNumber( aBox.ShelfAngle( bLeft: true ).Length )}";
                                v2 = new dxfVector( layoutviewRec.Right, v1.Y - 0.125 * paperscale );
                                leaders.Text( v1, v2, txt );
                            }
                        }

                        xCancelCheck();
                    }


                    if (aBox.BoltOnEndplates)
                    {
                        Status = "Drawing Bolt On Endplate Hole Leaders";

                        aEnts = Perim.AdditionalSegments.GetByTag("ENDPLATE", aEntityType: dxxEntityTypes.Circle);
                        if (aEnts.Count > 0)
                        {
                            if (perimRotation != 90)
                            {
                                aArc = (dxeArc)aEnts.Nearest(cLn.EndPt, aEntPointType: dxxEntDefPointTypes.Center, aEntityType: dxxEntityTypes.Circle);
                                v2 = new dxfVector(layoutviewRec.Right, aArc.Y + 0.25 * paperscale);
                            }
                            else
                            {
                                aArc = (dxeArc)aEnts.Nearest(Perim.GetVertex(dxxPointFilters.GetBottomLeft), aEntPointType: dxxEntDefPointTypes.Center, aEntityType: dxxEntityTypes.Circle);
                                v2 = new dxfVector(layoutviewRec.Left - 0.125 * paperscale, layoutviewRec.Bottom + 0.125 * paperscale);
                            }

                            Tool.CreateHoleLeader(Image, aArc, v2, dims, aEnts.Count);
                            xCancelCheck();
                        }
                    }

                    Status = "Drawing End Angle Slot Leaders";
                    {
                        endangleSlotLeader = null;
                        aEnts = Perim.AdditionalSegments.GetByTag("END ANGLE", aEntityType: dxxEntityTypes.Polyline);
                        if (aEnts.Count > 0)
                        {
                            aPts = aEnts.Centers();
                            v1 = aPts.GetVector(dxxPointFilters.GetTopRight);
                            aPl = (dxePolyline)aEnts.Nearest(v1);
                            //v2 = new dxfVector(v1.X - 0.25 * paperscale, v1.Y > ctr.Y ? layoutviewRec.Top + 0.5 * paperscale : layoutviewRec.Bottom - 0.5 * paperscale);
                            v2 = v1.Y > ctr.Y ? Perim.GetVertex(dxxPointFilters.GetTopRight) : Perim.GetVertex(dxxPointFilters.GetBottomRight);
                            v2 = new dxfVector(v2.X + 0.25 * paperscale, v2.Y);
                            endangleSlotLeader = Tool.CreateHoleLeader(Image, aPl, v2, dims, aEnts.Count);
                            if (endangleSlotLeader != null) lastRec = endangleSlotLeader.TextPrimary.BoundingRectangle();
                            xCancelCheck();
                        }
                    }

                    Status = "Drawing Startup Spout Slot Leader";
                    {
                        startupLeader = null;
                        aEnts = Perim.AdditionalSegments.GetByTag("STARTUP", aEntityType: dxxEntityTypes.Polyline);
                        if (aEnts.Count > 0)
                        {
                            aPts = aEnts.Centers();
                            if (aPts.Count > 0)
                            {
                                v1 = aPts.GetVector(dxxPointFilters.GetTopRight);
                                if (aPts.Count > 1) v1 = aPts.NearestVector(v1);

                                aPl = (dxePolyline)aEnts.Nearest(v1);

                                v2 = new dxfVector(aPl.BoundingRectangle().Left - 0.25 * paperscale, v1.Y > ctr.Y ? layoutviewRec.Top + 0.5 * paperscale : layoutviewRec.Bottom - 0.5 * paperscale);
                                if (lastRec != null)
                                {
                                    if (v2.X > lastRec.Left - 0.5 * paperscale) v2.X = lastRec.Left - 0.5 * paperscale;
                                }
                                startupLeader = Tool.CreateHoleLeader(Image, aPl, v2, dims, aEnts.Count);
                                if (startupLeader != null) lastRec = startupLeader.TextPrimary.BoundingRectangle();
                                xCancelCheck();
                            }
                        }
                    }

                    if (Assy.HasAntiPenetrationPans)
                    {
                        appHoleLeader = null;
                        appSlotLeader = null;
                        Status = "Drawing AP Pan Hole Leaders";
                        {
                            aEnts = Perim.AdditionalSegments.GetByTag("APPAN_HOLE", aEntityType: dxxEntityTypes.Circle);
                            if (aEnts.Count > 0)
                            {
                                aPts = aEnts.Centers();

                                if (aPts.Count > 0)
                                {
                                    v1 = aPts.NearestVector(ctr);
                                    aArc = (dxeArc)aEnts.Nearest(v1);
                                    v2 = new dxfVector(v1.X - 0.25 * paperscale, layoutviewRec.Top + (APNut == null ? 0.5 * paperscale : 0.75 * paperscale));
                                    if (lastRec != null)
                                    {
                                        if (v2.X > lastRec.Left - 0.5 * paperscale) v2.X = lastRec.Left - 0.5 * paperscale;
                                    }

                                    appHoleLeader = Tool.CreateHoleLeader(Image, aArc, v2, dims, aEnts.Count, aWeldedHDW: APNut);
                                    if (appHoleLeader != null) lastRec = appHoleLeader.TextPrimary.BoundingRectangle();
                                    xCancelCheck();
                                }
                            }

                            aEnts = Perim.AdditionalSegments.GetByTag("APPAN_SLOT", aEntityType: dxxEntityTypes.Polyline);
                            if (aEnts.Count > 0)
                            {
                                aPts = new colDXFVectors(aEnts.Centers().GetVectors(aFilter: dxxPointFilters.LessThanY, aOrdinate: ctr.Y));

                                if (aPts.Count > 0)
                                {
                                    v1 = aPts.NearestVector(ctr);
                                    aPl = (dxePolyline)aEnts.Nearest(v1);
                                    v2 = new dxfVector(v1.X - 0.25 * paperscale, layoutviewRec.Bottom - 0.5 * paperscale);
                                    appSlotLeader = Tool.CreateHoleLeader(Image, aPl, v2, dims, aEnts.Count);
                                    xCancelCheck();
                                }
                            }

                        }

                    }


                    Status = "Drawing Finger Clip Hole Leader";
                    {
                        fingerclipHoleLeader = null;
                        aEnts = Perim.AdditionalSegments.GetByTag("FINGER CLIP", aEntityType: dxxEntityTypes.Circle);
                        if (aEnts.Count > 0)
                        {
                            aPts = aEnts.Centers();

                            if (aPts.Count > 0)
                            {
                                v1 = aPts.GetVector(dxxPointFilters.GetTopLeft);
                                aArc = (dxeArc)aEnts.Nearest(v1);
                                v2 = new dxfVector(v1.X - 0.25 * paperscale, v1.Y > ctr.Y ? layoutviewRec.Top + 0.5 * paperscale : layoutviewRec.Bottom - 0.5 * paperscale);
                                if (lastRec != null)
                                {
                                    if (v2.X > lastRec.Left - 0.5 * paperscale) v2.X = lastRec.Left - 0.5 * paperscale;
                                }

                                fingerclipHoleLeader = Tool.CreateHoleLeader(Image, aArc, v2, dims, aEnts.Count);
                                if (fingerclipHoleLeader != null) lastRec = fingerclipHoleLeader.TextPrimary.BoundingRectangle();
                                xCancelCheck();
                            }
                        }
                    }
                    layoutEnts = Image.SelectionSetInit( true );
                    layoutviewRec = layoutEnts.BoundingRectangle();
                    v1 = fitRec.TopCenter - layoutviewRec.TopCenter;

                    layoutEnts.Translate( v1 );
                    layoutviewRec.Translate( v1 );
                    if (DrawlayoutRectangles)
                        draw.aPolyline( layoutviewRec, aDisplaySettings: new dxfDisplaySettings( aColor: dxxColors.LightGrey ) );


                    endviewRec = endViewEnts.BoundingRectangle();
                    v1 = layoutviewRec.BottomCenter - endviewRec.TopRight;
                    v1.Move( aYChange: -0.25 * paperscale );

                    endViewEnts.Translate( v1 );
                    endviewRec.Translate( v1 );
                    if (DrawlayoutRectangles)
                        draw.aPolyline( endviewRec, aDisplaySettings: new dxfDisplaySettings( aColor: dxxColors.LightGrey ) );

                }


                //================ SPOUT TABLE ===============================================

                Status = "Drawing Spout Table";
                {
                    Image.TableSettings.TextGap = 0.065;
                    Image.TableSettings.ColumnGap = 0.125;

                    v1 = new dxfVector(0.125 * paperscale, 1.0625 * paperscale);
                    aEnt = draw.aTable("SPOUTS", v1, spoutTableText, aTableAlign: dxxRectangularAlignments.BottomLeft, aGridStyle: dxxTableGridStyles.All, aColumnGap: 0.25);
                    v2 = aEnt.BoundingRectangle().TopLeft;
                }

                ////================ AXIS SYMBOL ===============================================
                Status = "Drawing Axis Symbol";
                {
                    //v1 = fitRec.BottomLeft.Moved(0.25 * paperscale, 0.25 * paperscale);
                    v1 = new dxfVector(fitRec.Left + 0.25 * paperscale, layoutviewRec.Bottom - 0.375 * paperscale);
                    if (v1.Y < v2.Y + 0.25 * paperscale) v1.Y = v2.Y + 0.25 * paperscale;
                    dwgEnts.Add(draw.aSymbol.Axis(v1, 0.375 * paperscale, aTextHeight: 0.125, aTextStyle: "Standard"));

                    xCancelCheck();
                }

                //================ NOTES ===============================================
                Status = "Drawing Notes";
                {
                    xCancelCheck();
                    noteblock = Tool.AddBorderNotes(aBox, Project, "DOWNCOMER BODY", true, Image, fitRec, paperscale);
                }

                //================ SIDE VIEW DETAIL ===============================================
                Status = "Drawing Side View Detail";
                {
                    Image.SelectionSetInit(false);
                    bname = $"DC_{aBox.PartNumber}_END_DETAIL";


                    aPG = mdPolygons.DCBox_View_EndDetail(aBox, Assy, paperscale, dxfVector.Zero, aRotation: 0, "GEOMETRY", bInvert: true);
                    v1 = layoutviewRec.BottomRight.Moved(-aPG.BoundingRectangle().Width - 1.25 * paperscale, -0.75 * paperscale);
                    ins = draw.aInsert(aPG, v1, -90, bname, aScaleFactor: 1, aDisplaySettings: dxfDisplaySettings.Null(aLayer: "GEOMETRY"));
                    //aPG.Translate(v1);
                    BlockTransform( aPG, ins );
                    DimPts = new colDXFVectors(v1) + aPG.AdditionalSegments.Centers(dxxEntityTypes.Point);
                    dims.Stack_Ordinate(dxxOrdinateDimTypes.OrdVertical, v1, DimPts, 0.65 * paperscale, 0.25 * paperscale, aInvertStack: true);

                    var brec = aPG.BoundingRectangle(); //Bottom Center has a bug where it returns bottom left in lieu of center

                    v1 = new dxfVector(brec.Center.X, brec.Bottom);
                    v1.Move(0, -0.25 * paperscale);

                    txt = @"\fArial Narrow|b1|;\H1.15x;SIDE VIEW OF BODY\PAFTER FORMING\P\fArial Narrow|b0|;\H1x;SCALE 1x";

                    draw.aText(v1, txt, aAlignment: dxxMTextAlignments.TopCenter, aTextStyle: "Standard");

                    detEnts = Image.SelectionSetInit(true);
                    sideviewRec = detEnts.BoundingRectangle();

                    v1 = endviewRec.TopRight - sideviewRec.TopLeft;
                    v1.Move( aXChange: 0.25 * paperscale );

                    detEnts.Translate( v1 );
                    sideviewRec.Translate( v1 );
                    if (DrawlayoutRectangles)
                        draw.aPolyline( sideviewRec, aDisplaySettings: new dxfDisplaySettings( aColor: dxxColors.LightGrey ) );


                    //if (layoutviewRec.Right <= endviewRec.Left + 1 * paperscale)
                    //{
                    //    v1 = new dxfVector(endviewRec.Right - 0.25 * paperscale, endviewRec.Bottom - 0.25 * paperscale) - sideviewRec.TopRight;
                    //    detEnts.Translate(v1);
                    //    sideviewRec.Translate(v1);
                    //}
                    //if (DrawlayoutRectangles)
                    //{
                    //    aPG.Translate(v1);

                    //    draw.aPolyline(sideviewRec, aDisplaySettings: new dxfDisplaySettings(aColor: dxxColors.LightGrey));
                    //}

                }
            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
            finally
            {
                Status = "";
                DrawinginProgress = false;
            }
        }

        private void DDWG_CrossBrace(mdCrossBrace aCB)
        {
            // ^creates the component drawing for the tray assembly cross Brace
            throw new NotImplementedException();
            //if (aCB == null)
            //{
            //    return;
            //}

            //dxfVector v0;
            //dxfVector v1;
            //dxfVector v2;
            //dxePolygon Perim;
            //dxePolygon Profile;
            //colDXFEntities Holes;
            //dxeArc aHole = null;
            //dxfRectangle aRect;
            //dxfRectangle fitRec = null;
            //colDXFVectors aPts;
            //dxfRectangle dRec;
            //double paperscale = 0;
            //double dScale;
            //uopHole aHl;

            //dxeDimension aDim = null;

            //try
            //{
            //    v0 = dxfVector.Zero;
            //    v1 = dxfVector.Zero;
            //    v2 = dxfVector.Zero;

            //    Perim = aCB.View_Plan(Assy, new dxfVector());
            //    aRect = Perim.Vertices.BoundingRectangle();

            //    aHl = aCB.BoltHole;

            //    //=====================================================================================================
            //    // insert the border
            //    Tool.Border(this, aRect, ref fitRec, ref paperscale, 2, 0);
            //    xCancelCheck();
            //    Drawing.ZoomExtents = false;
            //    //=====================================================================================================

            //    Perim.Vertex(1).Linetype = dxfLinetypes.Hidden;
            //    Perim.Move(fitRec.X, fitRec.Y + 2.5 * paperscale);

            //    Image.SelectionSetInit(false);

            //    Image.LinetypeLayers.ApplyTo(Perim, dxxLinetypeLayerFlag.ForceToLayer);
            //    Image.Entities.Add(Perim);

            //    aRect = Perim.BoundingRectangle();

            //    Holes = Perim.AdditionalSegments.GetByGraphicType(dxxGraphicTypes.Arc);

            //    aPts = Holes.DefinitionPoints(dxxEntDefPointTypes.Center);
            //    if (aPts.Count >= 1)
            //    {
            //        aHole = (dxeArc)Holes.GetByPoint(dxxPointFilters.AtMinX);
            //        v0 = Perim.GetVertex(dxxPointFilters.GetTopLeft, bReturnClone: true);
            //        v1 = aHole.Center;
            //        aDim = Image.Draw.aDim.Horizontal(v1, v0, 0.35);
            //    }

            //    if (aPts.Count >= 2)
            //    {

            //        aPts.Sort(dxxSortOrders.NearestToFarthest, v2);
            //        v2 = aPts.Item(2);
            //        Image.Draw.aDim.Horizontal(v1, v2, aDim.DimPt1.Y, aSuffix: " (TYP)", bAbsolutePlacement: true);

            //        v1 = aHole.AnglePoint(-45);

            //        v2.SetCoordinates(v1.X + 0.3 * paperscale, v0.Y - 0.65 * paperscale);
            //        Image.Draw.aLeader.Text(v1, v2, Tool.HoleLeader(Image, aHole, Holes.Count), null); // the aMidXY is not optional here, unlike VB so I used null
            //    }
            //    Image.Draw.aDim.Horizontal(v0, Perim.GetVertex(dxxPointFilters.GetTopRight), aDim.BoundingRectangle().Top + 0.25 * paperscale, bAbsolutePlacement: true);

            //    v1 = v0.Moved(0.5 * aCB.Length + 0.45 * paperscale, -0.8);
            //    v2 = v1.Moved(0.3 * paperscale, -0.1 - 0.55 * paperscale);
            //    Image.Draw.aLeader.Text(v1, v2, Tool.StampPNLeader(Image, true), null);

            //    Image.Draw.aText(v1, "XXX", 0.5, dxxMTextAlignments.BottomCenter, aTextStyle: "Standard");

            //    aRect = Image.SelectionSetInit(true, true).BoundingRectangle();

            //    v1.SetCoordinates(aRect.Left - 0.35 * paperscale, aRect.Y);
            //    Image.Draw.aSymbol.ViewArrow(v1, aRect.Height + 0.1 * paperscale, 0.25, "A", aTextHeight: 0.125, aTextStyle: "Standard");

            //    aRect = Image.SelectionSetInit(true, true).BoundingRectangle();

            //    dRec = fitRec.Clone();
            //    dRec.Width = fitRec.Width - paperscale * 4.5;
            //    dRec.X = fitRec.Left + dRec.Width / 2;
            //    dRec.Height = aRect.Bottom - 1 * paperscale;
            //    dRec.Y = aRect.Bottom - 0.5 * dRec.Height;

            //    if (DrawlayoutRectangles)
            //    {
            //        Image.Screen.Entities.aRectangle(fitRec, false, dxxColors.LightGrey);
            //        Image.Screen.Entities.aRectangle(aRect, false, dxxColors.LightGrey);
            //        Image.Screen.Entities.aRectangle(dRec, false, dxxColors.LightGrey);
            //    }

            //    //================= END VIEW =============================
            //    Image.SelectionSetInit(false, false);

            //    Profile = aCB.View_Profile(Assy, false, true, dRec.Center);
            //    dScale = paperscale <= 12 ? 8 : 16;

            //    Profile.Rescale(dScale, dRec.Center);
            //    Profile.Rotate(90);
            //    Image.LinetypeLayers.ApplyTo(Profile, dxxLinetypeLayerFlag.ForceToLayer);
            //    Image.Entities.Add(Profile);

            //    Image.DimStyleOverrides.LinearScaleFactor = Image.DimStyleOverrides.LinearScaleFactor / dScale;
            //    v0 = Profile.GetVertex(dxxPointFilters.GetTopLeft, bReturnClone: true);
            //    Image.Draw.aDim.Horizontal(Profile.GetVertex(dxxPointFilters.GetTopRight), v0, 0.35);

            //    v1 = v0.Moved(0.5 * aCB.Thickness * dScale, -aHl.Inset * dScale);
            //    v2 = Image.Draw.aLine(v1.X - 1.5 * aCB.Thickness * dScale, v1.Y, v1.X + 1.5 * aCB.Thickness * dScale, v1.Y, aLineType: dxfLinetypes.Center).StartPt;

            //    aDim = Image.Draw.aDim.Vertical(v0, v2, -0.85);

            //    v1 = Profile.GetVertex(dxxPointFilters.GetLeftBottom, bReturnClone: true);
            //    aDim = Image.Draw.aDim.Vertical(v0, v1, aDim.DimPt1.X - 0.85 * paperscale, bAbsolutePlacement: true);

            //    aRect = Image.SelectionSetInit(true, true).BoundingRectangle();
            //    if (DrawlayoutRectangles)
            //    {
            //        Image.Screen.Entities.aRectangle(aRect, false, dxxColors.LightGrey);
            //    }
            //    v0.Y = aRect.Bottom - 0.25 * paperscale;
            //    Image.Draw.aText(v0, $"\\LVIEW A-A\\l\\P\\H{paperscale * 0.125};{dScale}x SCALE", 0.1875 * paperscale, dxxMTextAlignments.TopCenter, aTextStyle: "Standard");

            //    //============ NOTE TEXT =======================================




            //    Tool.AddBorderNotes(aCB, Project, "CROSS BRACE", false, Image, fitRec, paperscale);


            //}
            //catch (Exception e)
            //{
            //    HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e );
            //}
            //finally
            //{
            //    Status = "";
            //    DrawinginProgress = false;

            //}
        }

        private void DDWG_DeckSection()
        {
            // #1the deck panel to draw
            // ^creates the component drawing of the passed deck panel section.

            double paperscale = 0;
            dxfRectangle fitRec = null;
            dxfVector v1;
            string txt1;
            string txt2;
            mdSlotZone aZone;
            List<string> aTable;
            dxfRectangle aEntRect = null;
            colDXFEntities aBubPromoters;
            dxeTable aTB;
            mdDeckSection aSection;
            dxeText cornerNote = null;
            try
            {
                if (Drawing.Part == null)
                    return;
                aSection = (mdDeckSection)Drawing.Part;
                if (aSection == null) return;


                dxoDrawingTool draw = Image.Draw;
                dxoDimStyle dstyle = Image.DimStyle();
                int precis = Drawing.DrawingUnits == uppUnitFamilies.English ? 4 : 1;
                colDXFEntities geometry = null;
                double rotation = 0;
                Status = $"Drawing {aSection.PartPath(true)}"; // it was aSection.Part.PartPath
                Image.SelectionSetInit(false);
                if (aSection.IsManway)
                {
                    geometry = Draw_DeckSection_MANWAY(aSection, out fitRec, out paperscale, out aEntRect, out aZone, out aBubPromoters, out cornerNote, out rotation);
                }
                else if (aSection.PanelIndex == 1)
                {
                    geometry = Draw_DeckSection_MOON(aSection, out fitRec, out paperscale, out aEntRect, out aZone, out aBubPromoters, out cornerNote, out rotation);
                }
                else
                {
                    geometry = Draw_DeckSection_FIELD(aSection, out fitRec, out paperscale, out aEntRect, out aZone, out aBubPromoters, out cornerNote, out rotation);
                }
                Image.SetFeatureScales(paperscale);
                draw = Image.Draw;
                Drawing.ZoomExtents = false;

                dxfRectangle geometryBounds = geometry.BoundingRectangle();
                geometryBounds.ScaleDimensions(1.05, 1.05);
                if (DrawlayoutRectangles)
                {
                    draw.aRectangle(fitRec, aColor: dxxColors.LightBlue);
                    draw.aRectangle(aEntRect, aColor: dxxColors.LightMagenta);
                    draw.aRectangle(geometryBounds, aColor: dxxColors.Magenta);
                }


                dxeInsert flowarrow = null;
                //======================= Draw The flow arrow ===========================================
                if (aZone != null)
                {
                    Status = "Adding Flow Symbol";
                    v1 = geometryBounds.MiddleRight().Moved(0.95 * paperscale);
                    double rot = aSection.Inverted ? 180d : 0d;
                    colDXFEntities aEnts = Tool.DeckSectionFlowSymbol(Image, paperscale, "Standard", aRotation: rotation + rot, aSlotZone: aZone, aAssy: Assy, aDeckSection: aSection);
                    dxfBlock flowsym = new(aEnts, "FLOW_ARROWS") { InsertionPt = dxfVector.Zero };
                    Image.Blocks.Add(flowsym);
                    flowarrow = draw.aInsert(flowsym.Name, v1, aDisplaySettings: dxfDisplaySettings.Null(aLayer: "DIMS"));
                    dxfRectangle irec = flowarrow.BoundingRectangle();
                    if (irec.Right > fitRec.Right - 0.125 * paperscale)
                    {
                        flowarrow.MoveFromTo(irec.TopCenter, geometryBounds.BottomCenter.Moved(0, -0.125 * paperscale));
                    }
                    //draw.aCircle(geometryBounds.MiddleRight(), 0.125 * paperscale);
                }


                //======================= AXIS SYMBOL ===========================================
                Status = "Adding Axis Symbol";
                {
                    if (aEntRect == null)
                    {
                        geometry.GetByTag("CORNER NOTE", bRemove: true);
                        aEntRect = geometry.BoundingRectangle();

                    }

                    v1 = new dxfVector(aEntRect.Left - 0.25 * paperscale, aEntRect.Bottom - 0.125 * paperscale);
                    draw.aSymbol.Axis(v1, aXLabel: "X", aAxisLength: paperscale * 0.5, aYLabel: "Y", aTextHeight: 0.125, bScaleToScreen: false, aName: "X-Y Axis");
                    //dwgEnts.Add( draw.aSymbol.Axis( v1, 0.375 * paperscale, aTextHeight: 0.125, aTextStyle: "Standard" ) );

                }

                //============  DECK PERFORATION TABLE =======================================
                Status = "Add Deck Perforation Table";
                {
                    dxoSettingsTable tsets = Image.TableSettings;
                    tsets.TitleTextStyle = "Standard";
                    tsets.TitleAlignment = dxxHorizontalJustifications.Left;
                    tsets.FooterAlignment = dxxHorizontalJustifications.Left;
                    tsets.TextGap = 0.125;
                    tsets.ColumnGap = 0.15;
                    tsets.TitleTextColor = dxxColors.Cyan;
                    tsets.CellAlignment = dxxRectangularAlignments.MiddleCenter;

                    Status = "Adding Perforation Table";
                    aTable = new List<string>();
                    txt1 = "% OPEN|DIAMETER";
                    txt2 = "SEE SHEET 4 - PERFORATION PATTERN";
                    if (aBubPromoters != null && aBubPromoters.Count > 0)
                    {
                        txt1 = $"{txt1}|BUBBLE PROMOTERS";
                        txt2 = $"{txt2}, BUBBLE PROMOTER DETAIL";
                    }
                    if (aZone != null)
                    {
                        txt2 = $"{txt2}, SLOT DETAIL";
                    }

                    aTable.Add(txt1);
                    txt1 = $"{string.Format("{0:0.00}", Assy.PerfFractionPercentOpen(out _, out _))}|";
                    txt1 = $"{txt1}{dstyle.FormatNumber(Assy.Deck.DP, aPrecision: precis)}";

                    if (aBubPromoters != null && aBubPromoters.Count > 0)
                    {
                        txt1 = $"{txt1}|{aBubPromoters.Count}";
                    }
                    aTable.Add(txt1);

                    v1 = fitRec.BottomLeft;
                    aTB = (dxeTable)draw.aTable("DECK_PERF_TABLE", v1, aTable, aTableAlign: dxxRectangularAlignments.TopLeft, aTitle: "DECK PERFORATION", aFooter: txt2);
                }


                //============  SLOTTING TABLE =======================================
                Status = "Add ECMD Slottinh Table";
                {
                    if (aZone != null)
                    {
                        Status = "Adding Slotting Table";
                        aTable = new List<string> { "TYPE|QUANTITY|X PITCH|Y PITCH" };

                        txt1 = Assy.Deck.SlotType == uppFlowSlotTypes.FullC ? "FULL 'C'|" : "HALF 'C'|";
                        txt1 = $"{txt1}{aZone.GridPointCount}|";

                        txt1 = $"{txt1}{dstyle.FormatNumber(aZone.VPitch, aPrecision: precis)}|{Image.FormatNumber(aZone.HPitch, aPrecision: precis)}";

                        aTable.Add(txt1);
                        v1.X = aTB.BoundingRectangle().Right + 0.5 * paperscale;
                        draw.aTable("SLOTTING_TABLE", v1, aTable, aTableAlign: dxxRectangularAlignments.TopLeft, aTitle: "ECMD SLOTTING");
                    }
                }

                //============ NOTE TEXT =======================================
                aSection.UpdatePartProperties();


                Tool.AddBorderNotes(aSection, Project, "TRAY DECK SEGMENT" + (aSection.IsManway ? " (MANWAY)" : ""), false, Image, fitRec, paperscale);


                // Tool.Add_B_BorderNotes(Image, notes, fitRec, paperscale);
            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
            finally
            {
                Status = "";
                DrawinginProgress = false;
            }
        }

        private colDXFEntities Draw_DeckSection_MANWAY(mdDeckSection aSection, out dxfRectangle rFitRec, out double rPaperScale, out dxfRectangle rEntRectangle, out mdSlotZone rZone, out colDXFEntities rBubPromoters, out dxeText rCornerNote, out double rInsertRotation)
        {
            // #1the deck panel to draw
            // ^creates the component drawing of the passed deck panel section.

            colDXFEntities _rVal = new();
            rFitRec = null;
            rPaperScale = 0;
            rEntRectangle = null;
            rZone = null;
            rBubPromoters = new colDXFEntities();
            rCornerNote = null;
            rInsertRotation = 0;
            dxePolygon Perim;
            dxfVector v0;
            dxfVector v1;
            dxfVector ctr;
            colDXFVectors DimPts;
            colDXFVectors aPts;
            colDXFVectors bPts;
            colDXFVectors cPts;
            colDXFVectors ldrPts;
            dxfVector ldrCtr;

            colDXFVectors ntchPts = colDXFVectors.Zero;
            colDXFEntities aEnts;
            dxfEntity aEnt;
            dxeArc aArc;

            dxeLine hCline;


            colDXFEntities aHoles;
            colDXFEntities aFlts;
            double d1;
            string txt1;
            double oset;
            double cy;


            try
            {
                bool bTabs = Assy.DesignOptions.SpliceStyle == uppSpliceStyles.Tabs;

                // initialize
                Image.Layers.Add("GEOMETRY");

                dxoDrawingTool draw = Image.Draw;
                dxoDimTool dims = Image.DimTool;

                if (Assy.DesignFamily.IsEcmdDesignFamily())
                {
                    rZone = aSection.SlotZone(Assy);
                    if (rZone != null)
                    {
                        if (rZone.GridPointCount <= 0) rZone = null;
                        if (rZone != null)
                            Image.Layers.Add("SLOTS", dxxColors.LightGrey);
                    }
                }

                // rotate the section perimeter for horizontal viewing
                dxeLine aAxis = aSection.Plane.ZAxis();
                aAxis.Value = -90;

                dxfRectangle baseRec = new(aSection.Height, aSection.Width);

                //=====================================================================================================
                // insert the border
                Status = "Inserting Border";
                {
                    Tool.Border(this, baseRec, ref rFitRec, ref rPaperScale, 3, 3);
                    xCancelCheck();
                    oset = rPaperScale * 0.35;

                }


                //======================= DRAW CORNER NOTES ===========================================

                Status = "Adding Corner Notes";
                {
                    txt1 = "PART IS SYMMETRICAL ABOUT CENTERLINE";
                    rCornerNote = draw.aText(rFitRec.TopLeft, txt1, 0.125 * rPaperScale, dxxMTextAlignments.TopLeft, aTextStyle: "Standard");
                    rCornerNote.Tag = "CORNER_NOTE";
                    Image.SelectionSetInit(false);
                }

                //======================= CREATE GEOMETRY ===========================================
                Status = "Creating Section Geometry";
                {
                    rInsertRotation = aAxis.Value;
                    Perim = aSection.View_Plan(Assy, bObscured: false, bIncludePromoters: true, bIncludeHoles: true, bIncludeSlotting: true, bRegeneratePerimeter: DrawlayoutRectangles, aCenter: dxfVector.Zero, aRotation: rInsertRotation, aLayerName: "GEOMETRY");
                }

                //======================= DRAW GEOMETRY ===========================================
                Status = "Drawing Section Geometry";
                {

                    v1 = new dxfVector(rFitRec.X, rCornerNote.BoundingRectangle().Bottom - 0.5 * rPaperScale - 0.5 * aSection.Width);
                    //d1 = mzUtils.VarToDouble(aAxis.Value);
                    draw.aInsert(Perim, v1, aBlockName: $"SECTION_{aSection.PartNumber}_PLAN_MFG", aRotationAngle: 0, aLTLSetting: dxxLinetypeLayerFlag.ForceToLayer, aDisplaySettings: Perim.DisplaySettings);

                    //Perim.Rotate(d1);
                    Perim.MoveTo(v1);

                    baseRec = Perim.Vertices.BoundingRectangle();
                    v0 = Perim.GetVertex(dxxPointFilters.GetBottomLeft, bReturnClone: true);

                }

                //======================= CREATE DIMENSION POINTS ===========================================
                // build the dimension points
                Status = "Creating Dimension Points";
                {
                    DimPts = Perim.FilletPoints(0.5);
                    v0 = DimPts.GetVector(dxxPointFilters.GetBottomLeft);

                    cy = baseRec.Y;
                    hCline = draw.aLine(baseRec.Left - 0.125 * rPaperScale, cy, baseRec.Right + 0.125 * rPaperScale, cy, aLineType: dxfLinetypes.Center);
                    baseRec = DimPts.BoundingRectangle(dxfPlane.World);
                    DimPts.Add(hCline.EndPt, bAddClone: true, aFlag: "NOH");

                    // draw the rectangles
                    if (DrawlayoutRectangles)
                    {
                        Image.Screen.Entities.aRectangle(baseRec, false, dxxColors.LightGrey);
                    }

                    // set the center to use for dimensioning
                    ctr = baseRec.Center;
                    ldrCtr = baseRec.Center;

                    // add the hole centers to the dim pts.
                    aHoles = Perim.AdditionalSegments.GetByValue((double)uppHoleTypes.Hole, bReturnClones: true);
                    bPts = aHoles.Centers(bReturnClones: true);
                    if (bPts.Count > 0)
                    {
                        d1 = bPts.GetOrdinate(dxxOrdinateTypes.MaxY);
                        if (d1 > ctr.Y)
                        {
                            aPts = bPts.GetAtCoordinate(aY: d1);
                            if (aPts.Count > 0)
                            {
                                aPts.Sort(dxxSortOrders.LeftToRight);
                                aPts.SetTagsAndFlags(aFlag: "NOV");
                                aPts.LastVector().Flag = "";
                                DimPts.Append(aPts, true);
                            }
                        }
                        else
                        {
                            aPts = new colDXFVectors();
                        }

                        d1 = bPts.GetOrdinate(dxxOrdinateTypes.MinY);
                        if (d1 < ctr.Y)
                        {
                            cPts = bPts.GetAtCoordinate(aY: d1);
                            if (cPts.Count > 0)
                            {
                                cPts.Sort(dxxSortOrders.LeftToRight);
                                if (aPts.Count <= 0)
                                {
                                    cPts.SetTagsAndFlags(aFlag: "NOV");
                                    cPts.LastVector().Flag = "";
                                    DimPts.Append(cPts, true);
                                }
                                else
                                {
                                    v1 = cPts.LastVector(true);
                                    v1.Flag = "NOH";
                                    DimPts.Add(v1);
                                }
                            }
                        }
                    }

                    if (!bTabs)
                    {
                        if (bPts.Count > 0)
                        {
                            d1 = bPts.GetOrdinate(dxxOrdinateTypes.MaxX);
                            if (d1 > ctr.X)
                            {
                                aPts = bPts.GetAtCoordinate(d1);
                                if (aPts.Count > 0)
                                {
                                    aPts.Sort(dxxSortOrders.BottomToTop);
                                    aPts.SetTagsAndFlags(aFlag: "NOH");
                                    aPts.LastVector().Flag = "";
                                    DimPts.Append(aPts, true);
                                }
                            }
                            else
                            {
                                aPts = new colDXFVectors();
                            }

                            d1 = bPts.GetOrdinate(dxxOrdinateTypes.MinX);
                            if (d1 < ctr.X)
                            {
                                cPts = bPts.GetAtCoordinate(d1);
                                if (cPts.Count > 0)
                                {
                                    cPts.Sort(dxxSortOrders.BottomToTop);
                                    if (aPts.Count <= 0)
                                    {
                                        cPts.SetTagsAndFlags(aFlag: "NOH");
                                        cPts.LastVector().Flag = "";
                                        DimPts.Append(cPts, true);
                                    }
                                    else
                                    {
                                        v1 = cPts.LastVector(true);
                                        v1.Flag = "NOV";
                                        DimPts.Add(v1);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        v1 = Perim.GetVertex(dxxPointFilters.GetRightBottom, bReturnClone: true);

                        DimPts.Append(Perim.AdditionalSegments.GetByTag("TABPOINT").Centers(), true, aTag: "TABPOINT", aFlag: "NOH");

                        ntchPts = Perim.Segments.GetArcs(0.236, 3).Centers(bReturnClones: true);
                        if (ntchPts.Count > 0)
                        {
                            ntchPts.RemoveCoincidentVectors();
                            ntchPts.SetPropertyValues(dxxVectorProperties.Flag, "NOH");
                            new colDXFVectors(ntchPts.GetVectors(aFilter: dxxPointFilters.AtY, aOrdinate: hCline.Y, bOnIsIn: true, aPrecis: 2)).SetPropertyValues(dxxVectorProperties.Flag, "NODIM");
                            v1 = ntchPts.Item(1, true);
                            if (v1.X < ctr.X)
                            {
                                ntchPts.Move(-0.75);
                            }
                            else
                            {
                                ntchPts.Move(0.75);
                            }
                            DimPts.Append(ntchPts, true, aTag: "NOTCHPOINT");
                        }
                    }

                    // add the bubble promoter center to the dimension points
                    if (Assy.DesignOptions.HasBubblePromoters)
                    {
                        rBubPromoters = Perim.AdditionalSegments.GetByTag("BUBBLE PROMOTER");

                        if (rBubPromoters.Count > 0)
                        {
                            aPts = rBubPromoters.Centers(bReturnClones: true);
                            aPts.Sort(dxxSortOrders.LeftToRight);
                            aPts.SetTagsAndFlags("BUBBLE PROMOTER", "NOH");
                            v1 = aPts.LastVector();
                            v1.Flag = "";
                            DimPts.Append(aPts, true);
                        }
                    }

                }

                //======================= DRAW DIMENSIONS ===========================================
                Status = "Drawing Dimensions";
                {
                    if (rZone != null)
                    {
                        aEnts = Perim.AdditionalSegments.GetByTag("SLOT", aEntityType: dxxEntityTypes.Point);
                        if (aEnts.Count > 0)
                        {
                            aPts = aEnts.Centers();
                            v1 = aPts.GetVector(dxxPointFilters.GetBottomLeft);
                            DimPts.Add(v1.X, v1.Y);
                        }
                    }
                    // add the dimensions to the drawing
                    dims.DimensionVertices(false, v0, DimPts, ctr, 0.35 * rPaperScale);

                    baseRec = Image.SelectionSet().BoundingRectangle(dxfPlane.World);
                    ldrPts = baseRec.Corners();

                    // draw the rectangles
                    if (DrawlayoutRectangles)
                        draw.aRectangle(baseRec, aColor: dxxColors.LightGrey);

                }


                ////======================= AXIS SYMBOL ===========================================
                //Status = "Drawing Axis Symbol";
                //draw.aSymbol.Axis(baseRec.BottomLeft.Moved(-0.25 * rPaperScale, -0.25 * rPaperScale), 0.65 * rPaperScale, 0, "X", "Y", 0.125);

                //======================= STAMP PN LEADER ===========================================
                Status = "Drawing PN Leader";
                {
                    aPts = colDXFVectors.Zero;
                    baseRec = Perim.Vertices.BoundingRectangle(dxfPlane.World);
                    v1 = baseRec.MiddleLeft();
                    d1 = bTabs && ntchPts.Count % 2 != 0 ? 1 : 0;

                    v1.Move(0.0625 * rPaperScale, d1);
                    draw.aText(v1, "XXXX", 0.5, dxxMTextAlignments.BottomCenter, -90, "Standard");

                    ldrPts.Add(v1, bAddClone: true, aTag: Tool.StampPNLeader(Image));
                }

                //======================= BUBBLE PROMOTER LEADER ======================================
                Status = "Drawing BP Leader";
                if (rBubPromoters.Count > 0)
                {
                    aPts = DimPts.GetByTag("BUBBLE PROMOTER");

                    v1 = aPts.GetVector(dxxPointFilters.AtMaxX).Clone();
                    ldrCtr = v1.Moved(-0.25);
                    ldrPts.Add(v1, bAddClone: true, aTag: "BUBBLE PROMOTER", aFlag: "HV");
                }

                //======================= SHEET 6 LEADERS ===========================================
                Status = "Drawing Sheet 6 Leaders";
                {
                    if (bTabs)
                    {
                        bPts = DimPts.GetByTag("TABPOINT");
                        if (bPts.Count > 0)
                        {
                            aPts = new colDXFVectors();
                            txt1 = "MALE JOGGLE DETAIL\\PSEE SHEET 6";
                            if (bPts.Count > 1)
                            {
                                txt1 = $"{txt1}\\P({bPts.Count} PLACES)";
                            }
                            ldrPts.Add(bPts.GetVector(dxxPointFilters.AtMinX).Moved(aYChange: 0.75), aTag: txt1);
                        }

                        if (ntchPts.Count > 0)
                        {
                            txt1 = "OPEN SLOT DETAIL\\PSEE SHEET 6";
                            v1 = ntchPts.Item(1);
                            v1 = new colDXFVectors(ntchPts.GetVectors(aFilter: dxxPointFilters.GreaterThanY, aOrdinate: cy)).NearestVector(v1.X < baseRec.X ? baseRec.MiddleLeft() : baseRec.MiddleRight(), bReturnClone: true);
                            if (ntchPts.Count > 1)
                                txt1 = $"{txt1}\\P({ntchPts.Count} PLACES)";

                            v1.Move(aYChange: 0.25);
                            ldrPts.Add(v1, bAddClone: true, aTag: txt1);
                        }
                    }
                }
                //======================= FILLET RADIUS LEADERS ===========================================

                Status = "Drawing Corner Radius Leader";
                {
                    aFlts = Perim.Segments.GetArcs(0.5, 3);
                    if (aFlts.Count > 0)
                    {
                        aArc = (dxeArc)aFlts.GetByPoint(dxxPointFilters.GetTopLeft);
                        v1 = aArc.MidPt;
                        txt1 = $"{Image.FormatNumber(aArc.Radius)} RAD.\\P(4 PLACES)";
                        ldrPts.Add(v1, bAddClone: true, aTag: txt1);
                    }
                }

                //================== LEADERS ON HOLES =============================================
                Status = "Drawing Hole Leaders";
                {
                    if (aHoles.Count > 0)
                    {
                        dxfRectangle aRec;

                        aEnt = aHoles.GetByPoint(dxxPointFilters.GetLeftBottom);
                        txt1 = Tool.HoleLeader(Image, aEnt, aHoles.Count);
                        v1 = aEnt.DefinitionPoint(dxxEntDefPointTypes.Center);
                        aRec = aEnt.BoundingRectangle(dxfPlane.World);
                        v1 = v1.X < baseRec.X ? aRec.MiddleLeft() : aRec.MiddleRight();
                        if (Math.Round(aRec.Height, 2) > Math.Round(aRec.Width, 2))
                        {
                            v1.Move(aYChange: 0.35 * aRec.Height);
                        }
                        ldrPts.Add(v1, bAddClone: true, aTag: txt1);
                    }
                }

                //================== LEADERS =============================================

                Status = "Drawing Leaders";
                {
                    draw.aLeader.Stacks(ldrPts, ldrCtr, 0.25 * rPaperScale, 0, 0, 65, 65);
                    rEntRectangle = Image.SelectionSet().BoundingRectangle(dxfPlane.World);
                }

                _rVal = Image.SelectionSet(dxxSelectionTypes.SelectAll);
                _rVal.Remove(rCornerNote);

                ////======================= Draw The flow arrow ===========================================

                //Status = "Drawing Flow Symbol";
                //{
                //    if (rZone != null)
                //    {
                //        v1 = rEntRectangle.MiddleRight().Moved(0.25 * rPaperScale);
                //        Status = "Adding Flow Symbol";
                //        aEnts = Tool.DeckSectionFlowSymbol(Image, rPaperScale, "Standard", mzUtils.VarToDouble(aAxis.Value), rZone, Assy, aSection);
                //        aEnts.MoveFromTo(aEnts.BoundingRectangle().MiddleLeft(), v1);
                //        Image.Entities.Append(aEnts);
                //    }

                //    xCancelCheck();
                //}

            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = string.Empty; }

            return _rVal;
        }

        private colDXFEntities Draw_DeckSection_FIELD(mdDeckSection aSection, out dxfRectangle rFitRec, out double rPaperScale, out dxfRectangle rEntRectangle, out mdSlotZone rZone, out colDXFEntities rBubPromoters, out dxeText rCornerNote, out double rInsertRotation)
        {
            // I am not sure if these should be out parameters
            colDXFEntities _rVal = new();
            rFitRec = null;
            rPaperScale = 0;
            rEntRectangle = null;
            rZone = null;
            rBubPromoters = new colDXFEntities();
            rCornerNote = null;
            rInsertRotation = 0;
            // #1the deck panel to draw
            // ^creates the component drawing of the passed deck panel section.

            dxePolygon Perim;
            dxfVector v0;
            dxfVector v1;
            dxfVector v2;
            dxfVector ctr;
            colDXFVectors DimPts;
            colDXFVectors aPts;
            colDXFVectors bPts;
            colDXFVectors ldrPts;
            dxfVector ldrCtr;

            colDXFEntities aEnts;
            dxeArc aArc;
            dxeArc bArc;
            dxfEntity aEnt;
            dxeLine aAxis;

            dxePolyline aPln;
            dxfRectangle baseRec;
            colDXFEntities addsegs;
            colDXFEntities lftHoles;
            colDXFEntities rgtHoles;
            colDXFEntities RCHoles;
            colDXFEntities spliceHoles;
            colDXFEntities slotPts = new();
            dxeLine vCline = null;
            dxeLine hCline = null;
            uopHole aHl;
            bool bTabs = Assy.DesignOptions.SpliceStyle == uppSpliceStyles.Tabs;
            double d1;
            string txt1;
            uopDeckSplice bSplice = aSection.BottomSplice;
            uopDeckSplice tSplice = aSection.TopSplice;
            uopHoles tSlots = aSection.TabSlots;
            uppSpliceIndicators tSpliceType = (tSplice != null) ? tSplice.SpliceIndicator : uppSpliceIndicators.ToRing;
            uppSpliceIndicators bSpliceType = (bSplice != null) ? bSplice.SpliceIndicator : uppSpliceIndicators.ToRing;
            bool bSymet = !aSection.LapsRing || Math.Round(aSection.X, 1) == 0;

            rInsertRotation = (!aSection.SingleSection) ? Math.Round(aSection.Y, 1) >= 0 ? -90 : 90 : 90;
            dxeInsert insert = null;
            dxxPointFilters fltr = dxxPointFilters.GetTopLeft;
            try
            {



                // initialize
                Image.Layers.Add("GEOMETRY");
                dxoDrawingTool draw = Image.Draw;
                dxoDimStyle dstyle = Image.DimStyle();
                dxoDimTool dims = Image.DimTool;
                dxoLeaderTool leaders = Image.LeaderTool;

                // prepare for slots ==========================
                if (Assy.DesignFamily.IsEcmdDesignFamily())
                {
                    rZone = aSection.SlotZone(Assy);
                    if (rZone != null)
                    {
                        if (rZone.GridPointCount <= 0) rZone = null;
                    }
                    if (rZone != null) Image.Layers.Add("SLOTS", dxxColors.LightGrey);
                }

                // rotate the section perimeter for horizontal viewing
                aAxis = aSection.Plane.ZAxis();

                baseRec = new dxfRectangle();
                baseRec.Rotate(rInsertRotation);

                baseRec = aSection.BoundingRectangle(baseRec.Plane);

                //=====================================================================================================
                // insert the border
                Tool.Border(this, baseRec, ref rFitRec, ref rPaperScale, 5, 4);

                xCancelCheck();

                //======================= SET THE INSERTION PT ===========================================
                v1 = rFitRec.TopCenter;
                txt1 = aSection.LapsRing ? "XXXX\\PXXXX" : "";
                if (bSymet) txt1 = $"{txt1}\\PXXX";
                if (txt1 != "") v1.Y -= Image.ScreenTextRectangle(txt1, 0.125 * rPaperScale, "Standard").Height - 0.0625 * rPaperScale;



                //======================= CREATE GEOMETRY ===========================================
                Status = "Creating Section Geometry";
                {
                    d1 = dstyle.CreateDimensionText(Math.Truncate(baseRec.Width) + 0.4444, aImage: Image).BoundingRectangle().Width;
                    d1 += (rPaperScale * 0.35) + (dstyle.TextGap * rPaperScale);
                    v1.Y += (-0.5 * baseRec.Height) - d1;
                    Perim = aSection.View_Plan(Assy, bObscured: false, bIncludePromoters: true, bIncludeHoles: true, bIncludeSlotting: true, bRegeneratePerimeter: DrawlayoutRectangles, aCenter: dxfVector.Zero, aRotation: rInsertRotation, aLayerName: "GEOMETRY");

                }

                //======================= DRAW GEOMETRY ===========================================
                Status = "Drawing Section Geometry";
                {
                    // The center point of the deck section does not work correctly at the moment.
                    // Using the insertion mechanism relies on the correct value for center.
                    // Therefore, here we find the place where we need to put the perimeter so that its shape is placed where v1 points to.
                    // Also, as this causes issue with rotation, we do the rotation when calling the View_Plan instead of rotating the insert.
                    var diff = Perim.BoundingRectangle().Center - Perim.Plane.Origin;
                    var adjustedInsertionPoint = v1 - diff;
                    
                    insert = draw.aInsert(Perim, adjustedInsertionPoint, 0, aBlockName: $"SECTION_{aSection.PartNumber}_PLAN_MFG", aLTLSetting: dxxLinetypeLayerFlag.ForceToLayer, aDisplaySettings: Perim.DisplaySettings);
                    
                    Perim.MoveFromTo(dxfVector.Zero, adjustedInsertionPoint);
                    
                    //Perim.Rotate(rInsertRotation);
                    //Perim.MoveTo(insert.InsertionPt);
                    baseRec = Perim.Vertices.BoundingRectangle(dxfPlane.World);
                    v0 = Perim.GetVertex(dxxPointFilters.GetBottomLeft, bReturnClone: true);
                    addsegs = Perim.AdditionalSegments.Clone();
                }


                //======================= DRAW CORNER NOTES ===========================================
                if (aSection.LapsRing || bSymet || aSection.SingleSection)
                {
                    Status = "Adding Notes";
                    txt1 = "";
                    if (aSection.LapsRing)
                    {
                        var arcs = Perim.ArcSegments(aSection.Radius, 2);
                        if (arcs.Count > 0)
                        {
                            aArc = arcs[ 0 ];
                            Status = "Adding Notes";
                            v2 = aArc.Center - v0;
                            txt1 = $"PERIPHERAL ARC RADIUS: {dstyle.FormatNumber(aArc.Radius)}";
                            txt1 = $"{txt1}\\PPERIPHERAL ARC CENTER (X,Y): {dstyle.FormatNumber(v2.X)}";
                            txt1 = $"{txt1}, {dstyle.FormatNumber(v2.Y)}";
                        }
                    }

                    if (bSymet || aSection.SingleSection)
                    {
                        txt1 = $"{txt1}\\PPART IS SYMMETRICAL ABOUT CENTERLINE";
                        if (bSymet && aSection.SingleSection)
                        {
                            txt1 = $"{txt1}S";
                        }
                    }

                    rCornerNote = draw.aText(rFitRec.TopLeft, txt1, 0.125 * rPaperScale, dxxMTextAlignments.TopLeft, aTextStyle: "Standard");
                    rCornerNote.Tag = "CORNER_NOTE";
                }



                Image.SelectionSetInit(false);
                //======================= CREATE DIMENSION POINTS ===========================================
                // build the dimension points
                Status = "Creating Dimension Points";
                {
                    DimPts = new colDXFVectors(Perim.Vertices);
                    DimPts.GetByFlag("NODIM", bRemove: true);

                    baseRec = Perim.BoundingRectangle(dxfPlane.World);
                    double cy = baseRec.Y;

                    if (bSymet)
                    {
                        if (aSection.LapsRing) DimPts.GetVector(dxxPointFilters.AtMaxX).Flag = "NOV";
                        // center line
                        hCline = draw.aLine(baseRec.Left - 0.125 * rPaperScale, cy, baseRec.Right + 0.125 * rPaperScale, cy, aLineType: dxfLinetypes.Center);
                    }

                    if (aSection.SingleSection)
                        vCline = draw.aLine(baseRec.X, baseRec.Top + 0.25 * rPaperScale, baseRec.X, baseRec.Bottom - 0.25 * rPaperScale, aLineType: dxfLinetypes.Center);

                    baseRec = DimPts.BoundingRectangle();


                    // set the center to use for dimensioning
                    ctr = baseRec.Center;
                    ldrCtr = baseRec.Center;

                    // add the hole centers to the dim pts.
                    rBubPromoters = addsegs.GetByGraphicType(dxxGraphicTypes.Arc, bRemove: true, aSearchTag: "BUBBLE PROMOTER");
                    if (rZone != null) slotPts = addsegs.GetByTag("SLOT", aEntityType: dxxEntityTypes.Point, bRemove: true);
                    lftHoles = addsegs.GetByGraphicType(dxxGraphicTypes.Arc);

                    RCHoles = lftHoles.GetByTag("RING CLIP", bRemove: true);
                    spliceHoles = lftHoles.GetByTag("SPLICE HOLE", bRemove: true);

                    rgtHoles = lftHoles.GetEntities(dxxPointFilters.GreaterThanX, ctr.X, bRemove: true);
                    bPts = lftHoles.Centers(bReturnClones: true);

                    if (RCHoles.Count > 0)
                    {
                        aPts = RCHoles.Centers(bReturnClones: true);
                        dxfRectangle prec = aPts.BoundingRectangle();
                        if (prec.Left < ctr.X && prec.Right > ctr.X)
                        {
                            bPts = new colDXFVectors(aPts.GetVectors(aFilter: dxxPointFilters.GreaterThanX, aOrdinate: ctr.X));
                            DimPts.Append(bPts, true, aTag: "HV");
                            DimPts.Append(aPts.GetVectors(aFilter: dxxPointFilters.LessThanX, aOrdinate: ctr.X), true, aTag: "HONLY");
                        }
                        else
                        {
                            bPts = aPts;
                            DimPts.Append(aPts, true, aFlag: "HV");

                        }

                        v1 = bPts.GetVector(dxxPointFilters.AtMinX);
                        if (!bSymet)
                            ctr.X = v1.X - 0.1;

                        ldrCtr.X = ctr.X;
                    }
                    if (spliceHoles.Count > 0)
                    {
                        aPts = spliceHoles.Centers(bReturnClones: true);
                        dxfRectangle prec = aPts.BoundingRectangle();
                        if (prec.Left < ctr.X && prec.Right > ctr.X)
                        {

                            DimPts.Append(aPts.GetVectors(aFilter: dxxPointFilters.GreaterThanX, aOrdinate: ctr.X), true, aTag: "V");
                            DimPts.Append(aPts.GetVectors(aFilter: dxxPointFilters.LessThanX, aOrdinate: ctr.X), true, aTag: "HONLY");
                        }
                        else
                        {
                            DimPts.Append(aPts, true, aFlag: "V");
                        }
                    }


                    if (lftHoles.Count > 0)
                    {
                        aPts = lftHoles.Centers(bReturnClones: true);
                        aPts.Sort(dxxSortOrders.TopToBottom);
                        aPts.SetTagsAndFlags(aFlag: "NOH");
                        aPts.Item(1).Flag = "";
                        DimPts.Append(aPts, true);
                    }

                    if (rgtHoles.Count > 0)
                    {
                        aPts = rgtHoles.Centers(bReturnClones: true);
                        aPts.Sort(dxxSortOrders.TopToBottom);
                        aPts.SetTagsAndFlags(aFlag: "NOH");
                        aPts.Item(1).Flag = "";
                        DimPts.Append(aPts, bAppendClones: true);
                    }

                    if (bTabs)
                    {
                        aPts = addsegs.Centers(dxxEntityTypes.Point, bReturnClones: true, aSearchTag: "TABPOINT");
                        bPts = addsegs.Centers(dxxEntityTypes.Point, bReturnClones: true, aSearchTag: "TABSLOT");
                        if (aPts.Count > 0)
                            DimPts.Append(aPts, bAppendClones: true, aTag: "TABPOINT", aFlag: "NOH");
                        else
                            DimPts.Append(bPts.GetVectors(aFilter: dxxPointFilters.AtMaxX), bAppendClones: true, aFlag: "NOH");
                    }
                    // add then bubble promoter center to the dimension points
                    if (rBubPromoters.Count > 0)
                    {

                        aPts = rBubPromoters.Centers(bReturnClones: true);
                        aPts.Sort(!aSection.SingleSection ? dxxSortOrders.LeftToRight : dxxSortOrders.RightToLeft);
                        ctr.Y = aPts.Item(1).Y - 0.25;
                        DimPts.Append(aPts, bAppendClones: true, aTag: "BUBBLE PROMOTER", aFlag: "NOV");
                        DimPts.Item(DimPts.Count - aPts.Count + 1).Flag = "";
                        if (aSection.SingleSection)
                            ctr.X = DimPts.Item(DimPts.Count - aPts.Count + 1).X - 0.1;

                    }

                }

                //======================= DRAW DIMENSIONS ===========================================
                Status = "Drawing Dimensions";
                {

                    if (slotPts.Count > 0) DimPts.Add(slotPts.Centers().GetVector(dxxPointFilters.GetBottomLeft), aFlag: "HV");
                    if (hCline != null) DimPts.Add(hCline.EndPt, bAddClone: true, aFlag: "NOH");
                    if (vCline != null) DimPts.Add(vCline.StartPt, bAddClone: true, aFlag: "NOV");

                    List<dxeDimension> orddims = dims.DimensionVertices(aDimensionOrigin: true, aBaseVertexXY: v0, aVerticesXY: DimPts, aCenterXY: ctr, aDimOffset: rPaperScale * 0.35, aInvertStacks: false, bTagsAsSuffixes: false, aDimStyle: "DIMENSION");
                    colDXFEntities diments = new(orddims.OfType<dxfEntity>().ToList());
                    //aPts = diments.ExtentPoints();
                    baseRec = diments.BoundingRectangle();

                    // draw the rectangles
                    if (DrawlayoutRectangles)
                    {
                        draw.aRectangle(baseRec, aColor: dxxColors.LightGrey);
                    }
                }


                ldrPts = baseRec.Corners();
                Status = "Adding Leaders";
                {

                    //======================= STAMP PN LEADER ===========================================

                    {

                        dxeText xxx = Image.EntityTool.Create_Text(dxfVector.Zero, "XXXX", 0.5, aAlignment: dxxMTextAlignments.MiddleCenter, aStyleName: "Standard", aTextAngle: !aSection.SingleSection ? 90 : 0);
                        Image.Entities.Add(xxx);

                        if (!aSection.SingleSection)
                        {

                            v1 = Perim.Vertices.BoundingRectangle(dxfPlane.World).MiddleLeft().Moved(aYChange: xxx.BoundingRectangle(dxfPlane.World).Height);
                            v1.X = Perim.Vertices.GetVector(dxxPointFilters.GetBottomLeft).X + 0.0625 * rPaperScale;
                            xxx.MoveTo(v1);

                            ldrPts.Add(v1, bAddClone: true, aTag: Tool.StampPNLeader(Image));
                        }
                        else
                        {
                            v1 = Perim.Vertices.BoundingRectangle(dxfPlane.World).BottomCenter.Moved(aXChange: xxx.BoundingRectangle(dxfPlane.World).Width, aYChange: 0.0625 * rPaperScale);
                            xxx.MoveTo(v1);
                            v2 = new dxfVector(v1.X + 0.5 * rPaperScale, baseRec.Bottom - 0.25 * rPaperScale);

                            leaders.Text(v1, v2, Tool.StampPNLeader(Image));
                        }


                    }

                    //======================= BUBBLE PROMOTER LEADER ======================================
                    if (rBubPromoters.Count > 0)
                    {
                        aPts = DimPts.GetByTag("BUBBLE PROMOTER");

                        v2 = aPts.GetVector(dxxPointFilters.AtMinX, bReturnClone: true);
                        v1 = aPts.GetVector(dxxPointFilters.AtMaxX, bReturnClone: true);
                        if (!aSection.SingleSection)
                        {
                            ldrCtr.X = v2.X - 0.1;
                            dxfVector v3 = v1.X > ldrCtr.X ? new dxfVector(0.9016, -0.9016) : new dxfVector(-0.9016, 0.9016);
                            ldrPts.Add(v1 + v3, aTag: "BUBBLE\\PPROMOTER");
                        }
                        else
                        {
                            ldrCtr.X = v2.X + 0.1;
                            ldrPts.Add(v2.Moved(-0.9016, 0.9016), aTag: "BUBBLE\\PPROMOTER");
                        }
                    }

                    //======================= SHEET 6 LEADERS ===========================================
                    if (bTabs)
                    {
                        bPts = DimPts.GetByTag("TABPOINT");

                        if (bPts.Count > 0)
                        {
                            txt1 = "MALE JOGGLE DETAIL\\PSEE SHEET 6";
                            if (bPts.Count > 1) txt1 = $"{txt1}\\P({bPts.Count} PLACES)";

                            ldrPts.Add(bPts.GetVector(dxxPointFilters.AtMinX).Moved(aYChange: 0.75), aTag: txt1);
                        }
                        bPts = addsegs.Centers(dxxEntityTypes.Point, true, "TABSLOT");

                        if (bPts.Count > 0)
                        {
                            txt1 = "FEMALE JOGGLE DETAIL\\PSEE SHEET 6";
                            v1 = bPts.GetVector(dxxPointFilters.GetRightBottom);
                            dxfVector v3 = v1.X > ctr.X ? new dxfVector(1.28, 2) : new dxfVector(-1.28, 0.5);
                            if (v1.X > ctr.X)
                            {
                                if (bPts.GetOrdinate(dxxOrdinateTypes.MinX) < ctr.X) txt1 = $"{txt1}\\P(2 PLACES)";
                            }

                            ldrPts.Add(v1 + v3, aTag: txt1);
                            v1 = bPts.GetVector(dxxPointFilters.GetTopLeft);
                            if(tSlots.Count > 0)
                            {
                                aHl = tSlots.Item(1);
                                if (aHl != null)
                                {
                                    d1 = v1.Y >= ctr.Y ? aHl.Length / 3 : -aHl.Length / 3;
                                    if (v1.X < ctr.X) d1 -= aHl.Radius;
                                    v1.Y += d1;
                                    if (v1.X >= ctr.X) v1.X += aHl.Radius;

                                    txt1 = Tool.HoleLeader(Image, aHl, tSlots.Count, true);
                                    ldrPts.Add(v1, bAddClone: true, aTag: txt1);
                                }
                            }
                           

                        }
                    }

                    //================== LEADERS ON HOLES =============================================
                    if (lftHoles.Count > 0)
                    {
                        txt1 = "";
                        aArc = (dxeArc)lftHoles.GetByPoint(dxxPointFilters.GetBottomLeft, aEntityType: dxxEntityTypes.Circle);
                        if (aArc != null)
                        {
                            txt1 = Tool.HoleLeader(Image, aArc, lftHoles.Count);
                            v1 = aArc.AnglePoint(180);

                            if (rgtHoles.Count > 0)
                            {
                                bArc = (dxeArc)rgtHoles.GetByPoint(dxxPointFilters.GetBottomRight, aEntityType: dxxEntityTypes.Circle);
                                if (bArc != null)
                                {
                                    if (Math.Round(aArc.Radius, 3) == Math.Round(bArc.Radius, 3))
                                    {
                                        aArc = bArc;
                                        txt1 = Tool.HoleLeader(Image, aArc, lftHoles.Count + rgtHoles.Count);
                                        rgtHoles.Clear();
                                        v1 = aArc.AnglePoint(0);
                                    }
                                }
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(txt1))
                            ldrPts.Add(v1, bAddClone: true, aTag: txt1);

                    }
                    if (rgtHoles.Count > 0)
                    {
                        aEnt = rgtHoles.GetByPoint(dxxPointFilters.GetBottomRight);
                        txt1 = Tool.HoleLeader(Image, aEnt, rgtHoles.Count);
                        if (aEnt.GraphicType == dxxGraphicTypes.Arc)
                        {
                            aArc = (dxeArc)aEnt;
                            v1 = aArc.AnglePoint(0);
                        }
                        else
                        {
                            aPln = (dxePolyline)aEnt;
                            v1 = aPln.BoundingRectangle(dxfPlane.World).BottomCenter;
                        }

                        ldrPts.Add(v1, bAddClone: true, aTag: txt1);
                    }

                    if (RCHoles.Count > 0)
                    {
                        fltr = !aSection.SingleSection ? dxxPointFilters.GetBottomRight : dxxPointFilters.GetBottomRight;
                        aArc = (dxeArc)RCHoles.GetByPoint(fltr);
                        v1 = new dxfVector(aArc.X > insert.X ? baseRec.Right + 0.125 * rPaperScale : baseRec.Left - 0.125 * rPaperScale, aArc.Y - .25 * rPaperScale);
                        Tool.CreateHoleLeader(Image, aArc, v1, dims, RCHoles.Count);


                    }

                    if (spliceHoles.Count > 0)
                    {
                        fltr = dxxPointFilters.GetTopLeft;
                        aArc = (dxeArc)spliceHoles.GetByPoint(fltr);
                        v1 = new dxfVector(aArc.X > insert.X ? baseRec.Right + 0.125 * rPaperScale : baseRec.Left - 0.125 * rPaperScale, aArc.Y + .25 * rPaperScale);
                        Tool.CreateHoleLeader(Image, aArc, v1, dims, spliceHoles.Count);


                    }

                    //================== LEADERS =============================================

                    leaders.Stacks(ldrPts, ldrCtr, aLeaderLength: 0.25 * rPaperScale, aXOffset: 0, aLeftLeaderAngle: 65, aRightLeaderAngle: 180 - 65);


                }

                aEnts = Image.SelectionSet();
                aEnts.Add(insert);
                rEntRectangle = aEnts.BoundingRectangle(dxfPlane.World);

                if (rCornerNote != null)
                {
                    dxfRectangle trec = rCornerNote.BoundingRectangle();


                    d1 = rEntRectangle.Top - (trec.Bottom - 0.125 * rPaperScale);
                    if (d1 > 0)
                    {
                        v1 = Image.UCS.YDirection * -d1;
                        aEnts.Translate(v1);
                        rEntRectangle.Translate(v1);

                        List<dxeLeader> ldrs = aEnts.Leaders();

                    }

                }


                //======================= Draw The profile view ===========================================

                if (tSpliceType == uppSpliceIndicators.JoggleFemale || tSpliceType == uppSpliceIndicators.JoggleMale || tSpliceType == uppSpliceIndicators.TabFemale || bSpliceType == uppSpliceIndicators.JoggleFemale || bSpliceType == uppSpliceIndicators.JoggleMale || bSpliceType == uppSpliceIndicators.TabFemale) // does not look right!
                {
                    v1 = dxfVector.Zero;
                    v1.SetCoordinates(Perim.BoundingRectangle().X, rEntRectangle.Bottom - 0.375 * rPaperScale);
                    Draw_SectionProfile(v1, rPaperScale, aSection, Perim, rInsertRotation, tSpliceType, bSpliceType);
                }

                _rVal = Image.SelectionSetInit(true);
                _rVal.Remove(rCornerNote);


                return _rVal;
            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
            return _rVal;
        }

        private void Draw_SectionProfile(dxfVector aAligmentPt, double aPaperScale, mdDeckSection aSection, dxePolygon aPerimter, double aPerimRotation, uppSpliceIndicators tSpliceType, uppSpliceIndicators bSpliceType)
        {
            if (aSection == null) return;

            dxfVector v1 = aAligmentPt != null ? aAligmentPt.Clone() : dxfVector.Zero;
            dxfVector v2; ;
            dxePolygon Perim;
            hdwHexBolt aBlt;
            int cnt;
            string txt;
            bool longside = aPerimRotation > 0;
            string lname = "GEOMETRY";
            string bname = $"SECTION_{aSection.Handle}_PROFILE_MFG";
            colDXFEntities aEnts;
            dxfRectangle aRec;
            dxeInsert ins;

            try
            {
                dxoDrawingTool draw = Image.Draw;
                dxoDimTool dims = Image.DimTool;
                if (aPaperScale <= 0) aPaperScale = 1;

                if (aSection.PanelIndex > 1)
                {
                    Status = "Drawing Section Profile View";
                    Perim = aSection.View_Profile(Assy, bLongSide: longside, bIncludePromoters: true, bSuppressHoles: false, bIncludeBolts: true, aCenter: dxfVector.Zero, aLayerName: lname);
                    if (aPerimRotation > 0)
                    {
                        uppSpliceIndicators t = tSpliceType;
                        tSpliceType = bSpliceType;
                        bSpliceType = t;
                    }
                }
                else
                {
                    Status = "Drawing Section Elevation View";
                    bool jaIncluded = false;
                    Perim = aSection.View_Elevation(Assy, ref jaIncluded, bIncludePromoters: true, bSuppressHoles: false, bIncludeBolts: true, aCenter: dxfVector.Zero, aRotation: 90);
                }

                ins = draw.aInsert(Perim, v1, aBlockName: bname, aDisplaySettings: dxfDisplaySettings.Null(aLayer: lname));
                Perim.MoveTo(ins.InsertionPt);

                switch (bSpliceType)
                {
                    case uppSpliceIndicators.JoggleMale:
                        txt = tSpliceType == uppSpliceIndicators.JoggleMale ? " (TYP)" : "";
                        v1 = Perim.GetVertex(dxxPointFilters.GetLeftBottom, bReturnClone: true);
                        if (aSection.PanelIndex > 1)
                        {
                            v2 = v1.Moved(aYChange: Assy.DesignOptions.JoggleAngle);
                            dims.Vertical(v2, v1, -0.35, aSuffix: txt);
                        }
                        else
                        {
                            v2 = v1.Moved(Assy.DesignOptions.JoggleAngle);
                            dims.Horizontal(v1, v2, -0.35, aSuffix: txt);
                        }


                        aEnts = aPerimter.AdditionalSegments.GetByTag("BOLT");

                        cnt = aEnts.Count;
                        if (cnt > 0)
                        {
                            aRec = aEnts.BoundingRectangle(dxfPlane.World);

                            aBlt = aSection.JoggleBolt;
                            aBlt.ObscuredLength = aSection.Thickness;
                            aEnts = Perim.AdditionalSegments.GetByTag("WELDED BOLT");
                            if (aEnts.Count > 0)
                            {
                                aRec = aEnts.BoundingRectangle(dxfPlane.World);
                                txt = aBlt.GetFriendlyName(true);
                                txt = $"{txt} ({cnt} PLACES)";
                                txt = $"{txt}\\PTACK WELD TWO PLACES PER BOLT";
                                if (aSection.PanelIndex > 1)
                                {
                                    v1 = aRec.BottomRight.Moved(-0.25 * aBlt.G);
                                    v2 = v1.Moved(0.25 * aPaperScale, -0.25 * aPaperScale);
                                }
                                else
                                {
                                    v1 = aRec.TopRight.Moved(-0.25 * aBlt.G);
                                    v2 = v1.Moved(0.25 * aPaperScale, 0.25 * aPaperScale);
                                }

                                draw.aLeader.Text(v1, v2, txt);

                            }
                        }
                        break;
                    default:
                        break;
                }

                if (aSection.PanelIndex > 1)
                {
                    txt = "";
                    if ((tSpliceType == uppSpliceIndicators.JoggleMale && bSpliceType != uppSpliceIndicators.JoggleMale) || tSpliceType == uppSpliceIndicators.TabFemale)
                    {
                        if (bSpliceType == uppSpliceIndicators.TabFemale)
                        {
                            txt = " (TYP)";
                        }
                        v1 = Perim.GetVertex(dxxPointFilters.GetRightBottom, bReturnClone: true);

                        v2 = v1.Moved(aYChange: Assy.DesignOptions.JoggleAngle);
                        dims.Vertical(v1, v2, 0.35, aSuffix: txt);
                    }
                    else if (bSpliceType == uppSpliceIndicators.TabFemale)
                    {
                        if (tSpliceType == uppSpliceIndicators.TabFemale)
                        {
                            txt = " (TYP)";
                        }
                        v1 = Perim.GetVertex(dxxPointFilters.GetLeftBottom, bReturnClone: true);
                        v2 = v1.Moved(aYChange: Assy.DesignOptions.JoggleAngle);
                        dims.Vertical(v1, v2, -0.35, aSuffix: txt);
                    }
                }
                else
                {
                    txt = "";
                    if (bSpliceType == uppSpliceIndicators.TabFemale)
                    {
                        v2 = Perim.GetVertex(dxxPointFilters.GetBottomRight, bReturnClone: true);
                        v1 = v2.Moved(-Assy.DesignOptions.JoggleAngle);
                        dims.Horizontal(v1, v2, -0.35);
                    }
                }
                xCancelCheck();
            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = string.Empty; }


        }

        private colDXFEntities Draw_DeckSection_MOON(mdDeckSection aSection, out dxfRectangle rFitRec, out double rPaperScale, out dxfRectangle rEntRectangle, out mdSlotZone rZone, out colDXFEntities rBubPromoters, out dxeText rCornerNote, out double rInsertRotation)
        {
            // I am not sure if these should be out parameters
            colDXFEntities _rVal = new();
            rFitRec = null;
            rPaperScale = 0;
            rEntRectangle = null;
            rZone = null;
            rBubPromoters = new colDXFEntities();
            rCornerNote = null;
            rInsertRotation = 0;
            if (aSection == null) return _rVal;

            // #1the deck panel to draw
            // ^creates the component drawing of the passed deck panel section.

            dxePolygon Perim;
            dxfVector v0 = dxfVector.Zero;
            dxfVector v1;
            dxfVector v2;
            dxfVector ctr;
            colDXFVectors DimPts;
            colDXFVectors aPts;

            colDXFVectors ldrPts;
            dxfVector ldrCtr;

            uopHoles tSlots;
            dxfRectangle baseRec;
            colDXFEntities aHoles;
            colDXFEntities aEnts;
            colDXFEntities RCHoles;

            dxeArc aArc;
            dxeText aTxt;
            double oset;

            bool bTabs = Assy.DesignOptions.SpliceStyle == uppSpliceStyles.Tabs;
            bool bSpliced;
            bool bProfile;
            string txt1 = null;
            uopDeckSplice bSplice = aSection.BottomSplice;
            uopDeckSplice tSplice = aSection.TopSplice;
            uppSpliceIndicators tSpliceType = (tSplice != null) ? tSplice.SpliceIndicator : uppSpliceIndicators.ToRing;
            uppSpliceIndicators bSpliceType = (bSplice != null) ? bSpliceType = bSplice.SpliceIndicator : uppSpliceIndicators.ToDowncomer;

            try
            {

                dxoDrawingTool draw = Image.Draw;
                dxoDimTool dims = Image.DimTool;

                // initialize
                Image.Layers.Add("GEOMETRY");

                Status = "Calculating Paper Scale";
                {
                    bProfile = tSpliceType == uppSpliceIndicators.JoggleMale || tSpliceType == uppSpliceIndicators.JoggleFemale || tSpliceType == uppSpliceIndicators.TabFemale || bSpliceType == uppSpliceIndicators.JoggleMale || bSpliceType == uppSpliceIndicators.JoggleFemale || bSpliceType == uppSpliceIndicators.TabFemale; // does not look right!

                    if (Assy.DesignFamily.IsEcmdDesignFamily())
                    {
                        mdSlotZones zones = Assy.SlotZones;
                        rZone = zones.GetBySectionHandle(aSection.Handle);

                        if (rZone != null)
                        {
                            if (rZone.GridPointCount <= 0) rZone = null;

                        }
                    }

                    tSlots = aSection.TabSlots;

                    bSpliced = tSplice != null || bSplice != null;

                    baseRec = new dxfRectangle();
                    baseRec.Rotate(90);
                    baseRec = aSection.BoundingRectangle(baseRec.Plane);

                    if (bProfile)
                    {
                        baseRec.Stretch(Assy.DesignOptions.SpliceAngle + 1, aStretchWidth: true, aStretchHeight: false, bMaintainBaseline: true, bMaintainOrigin: true);
                    }
                }

                Status = "Inserting Border";
                {
                    //=====================================================================================================
                    // insert the border
                    Tool.Border(this, baseRec, ref rFitRec, ref rPaperScale, !bProfile ? 3 : 6, 3);
                    xCancelCheck();
                }

                oset = 0.35 * rPaperScale;
                Status = "Creating Section Geometry";
                {
                    if (rZone != null) Image.Layers.Add("SLOTS", dxxColors.LightGrey);
                    rInsertRotation = 90;
                    Perim = aSection.View_Plan(Assy, bObscured: false, bIncludePromoters: true, bIncludeHoles: true, bIncludeSlotting: true, bRegeneratePerimeter: DrawlayoutRectangles, aCenter: dxfVector.Zero, aRotation: rInsertRotation, aLayerName: "GEOMETRY");
                }

                //======================= PERIPHERAL ARC NOTE ===========================================
                Status = "Adding Corner Notes";
                {
                    txt1 = "PART SYMMETRICAL ABOUT CENTER LINE";
                    aArc = Perim.ArcSegments(aSection.Radius, 2)[0];
                    if (aArc != null)
                    {
                        v1 = aArc.Center - v0;
                        txt1 = $"{txt1}\\PPERIPHERAL ARC RADIUS: {Image.FormatNumber(aArc.Radius)}";
                        txt1 = $"{txt1}\\PPERIPHERAL ARC CENTER (X,Y): {Image.FormatNumber(v1.X)}";
                        txt1 = $"{txt1}, {Image.FormatNumber(v1.Y)}";
                    }
                    v2 = rFitRec.TopLeft;
                    rCornerNote = draw.aText(rFitRec.TopLeft, txt1, 0.125 * rPaperScale, dxxMTextAlignments.TopLeft, aTextStyle: "Standard");
                    rCornerNote.Tag = "CORNER_NOTE";

                }

                // draw the plan view
                Status = "Drawing Section Geometry";
                {
                    Image.SelectionSetInit(false);

                    v1 = new dxfVector(rFitRec.X, rCornerNote.BoundingRectangle().Bottom - aSection.Width / 2 - 1.5 * rPaperScale);
                    draw.aInsert(Perim, v1, aBlockName: $"SECTION_{aSection.PartNumber}_PLAN_MFG", aRotationAngle: 0, aLTLSetting: dxxLinetypeLayerFlag.ForceToLayer, aDisplaySettings: Perim.DisplaySettings);
                    Perim.MoveTo(v1);
                }

                // build the dimension points
                Status = "Creating Dimension Points";
                {
                    DimPts = Perim.Vertices.Clone();
                    v0 = DimPts.GetVector(dxxPointFilters.GetLeftBottom, bReturnClone: true);
                    if (aSection.SectionIndex == 1) DimPts.Remove(DimPts.GetVector(dxxPointFilters.GetBottomLeft));

                    baseRec = DimPts.BoundingRectangle();

                    // set the center to use for dimensioning
                    ctr = baseRec.Center;
                    ldrCtr = baseRec.Center;

                    if (aSection.SectionIndex == 1)
                    {
                        v1 = DimPts.GetVector(dxxPointFilters.AtMaxY);
                        v1.Flag = "NOH";
                        DimPts.GetVector(dxxPointFilters.GetLeftTop).Flag = "NOH";
                        DimPts.GetVector(dxxPointFilters.GetRightTop).Flag = "NOV";
                    }
                    ctr.Y = v0.Y + (bSpliceType == uppSpliceIndicators.ToDowncomer ? 0.5125 : 1.5125);


                    v1 = new dxfVector(ctr.X, baseRec.Bottom - 0.125 * rPaperScale);
                    v2 = new dxfVector(ctr.X, baseRec.Top + 0.125 * rPaperScale);
                    DimPts.Add(v1, bAddClone: true, aFlag: "NODIM");
                    DimPts.Add(v2, bAddClone: true, aFlag: "NODIM");

                    draw.aLine(v1, v2, aLineType: dxfLinetypes.Center);

                    aHoles = Perim.AdditionalSegments.GetByValue((double)uppHoleTypes.Hole, bReturnClones: true);
                    rBubPromoters = Perim.AdditionalSegments.GetByTag("BUBBLE PROMOTER");

                    ldrPts = baseRec.Corners();
                    ldrPts.SetTagsAndFlags(aTag: "", aFlag: "");
                    //ldrPts = new colDXFVectors();
                    RCHoles = aHoles.GetByTag("RING CLIP", bRemove: true);

                    // add the hole centers to the dim pts.

                    if (RCHoles.Count > 0)
                    {
                        aPts = RCHoles.Centers(bReturnClones: true);
                        DimPts.Append(aPts.GetVectors(aFilter: dxxPointFilters.GreaterThanX, aOrdinate: ctr.X, bOnIsIn: true), bAppendClones: true);
                        DimPts.Append(aPts.GetVectors(aFilter: dxxPointFilters.LessThanX, aOrdinate: ctr.X, bOnIsIn: false), bAppendClones: true, aFlag: "NOV");
                    }

                    if (aHoles.Count > 0)
                    {
                        aPts = aHoles.Centers(bReturnClones: true);
                        aPts.Sort(dxxSortOrders.LeftToRight);
                        DimPts.Append(aPts, aFlag: "NOV");
                        DimPts.LastVector().Flag = "";
                    }

                    if (tSlots.Count > 0)
                    {
                        aPts = Perim.AdditionalSegments.Centers(dxxEntityTypes.Point, true, "TABSLOT");
                        DimPts.Append(aPts, true, aTag: "TABSLOT", aFlag: "NOV");
                    }
                    aPts = Perim.AdditionalSegments.Centers(dxxEntityTypes.Point, true, "TABPOINT");
                    DimPts.Append(aPts, true, aTag: "TABPOINT", aFlag: "NOV");

                    // add then bubble promoter center to the dimension points
                    if (Assy.DesignOptions.HasBubblePromoters)
                    {
                        if (rBubPromoters.Count > 0)
                        {
                            aPts = rBubPromoters.Centers(bReturnClones: true);
                            aPts.Sort(dxxSortOrders.LeftToRight);
                            for (int p = 1; p <= aPts.Count; p++)
                            {
                                v1 = aPts.Item(p, true);
                                draw.aDim.OrdinateH(v0, v1, v0.Y - 0.25 * rPaperScale, v1.X, aAbsolutePlacement: true);
                            }
                            DimPts.Add(v1, bAddClone: true, aFlag: "NOH");
                        }
                    }
                    if (DrawlayoutRectangles)
                        draw.aRectangle(DimPts.BoundingRectangle(), aColor: dxxColors.LightGreen);

                }

                //======================= DIMENSIONS ===========================================
                Status = "Drawing Dimensions";
                {
                    if (rZone != null)
                    {
                        aEnts = Perim.AdditionalSegments.GetByTag("SLOT", aEntityType: dxxEntityTypes.Point);
                        if (aEnts.Count > 0)
                        {
                            aPts = aEnts.Centers();
                            v1 = aPts.GetVector(dxxPointFilters.GetBottomLeft);
                            DimPts.Add(v1.X, v1.Y);
                            ctr.Y = v1.Y + 0.01;
                        }
                    }


                    aPts = DimPts.Clone();
                    aPts.SetTagsAndFlags("");
                    if (aSection.SectionIndex == 1)
                    {
                        aPts.GetVector(dxxPointFilters.AtMaxY).Tag = " (REF)";
                    }
                    dxfVectors.SetTagsAndFlags(aPts.GetVectors(aFilter: dxxPointFilters.AtMinX), aFlag: "NOV");
                    dxfVectors.SetTagsAndFlags(aPts.GetVectors(aFilter: dxxPointFilters.AtMaxX), aFlag: "NOV");

                    dims.DimensionVertices(true, v0, aPts, ctr, oset, false, bTagsAsSuffixes: true);
                    baseRec = Image.SelectionSet().BoundingRectangle(dxfPlane.World);
                    ldrPts.Add(baseRec.MiddleRight());

                    if (DrawlayoutRectangles)
                        draw.aRectangle(baseRec, aColor: dxxColors.LightGrey);

                }

                Status = "Adding Leaders";
                {
                    //======================= SHEET 6 LEADERS ===========================================
                    if (bTabs && aSection.SectionIndex > 1)
                    {
                        aPts = DimPts.GetByTag("TABPOINT");
                        if (aPts.Count > 0)
                        {
                            txt1 = "MALE JOGGLE DETAIL\\PSEE SHEET 6";
                            if (aPts.Count > 1)
                            {
                                txt1 = $"{txt1}\\P({aPts.Count} PLACES)";
                            }

                            v1 = aPts.GetVector(dxxPointFilters.AtMaxX).Clone();
                            v2 = v1.Clone();
                            v2.SetCoordinates(v1.X + 0.5 * rPaperScale, baseRec.Top + 0.5 * rPaperScale);
                            draw.aLeader.Text(v1, v2, txt1);
                        }
                    }

                    //======================= STAMP PN LEADER ===========================================
                    if (aSection.SectionIndex == 1)
                    {
                        if (bSpliceType == uppSpliceIndicators.ToDowncomer)
                        {
                            v1 = Perim.BoundingRectangle(dxfPlane.World).BottomCenter.Moved(aYChange: 0.25);
                            aTxt = Image.EntityTool.Create_Text(v1, "XXXX", 0.5, dxxMTextAlignments.BottomCenter, aStyleName: "Standard");
                            v2 = baseRec.BottomCenter.Moved(aYChange: -0.25 * rPaperScale);

                            Image.Entities.Add(aTxt);
                            v2.X = v1.X + 0.5 * rPaperScale;
                            txt1 = Tool.StampPNLeader(Image);
                            draw.aLeader.Text(v1, v2, txt1);
                        }
                        else
                        {
                            aTxt = Image.EntityTool.Create_Text(v1, "XXXX", 0.5, dxxMTextAlignments.TopCenter, aStyleName: "Standard");
                            v1 = Perim.BoundingRectangle(dxfPlane.World).TopCenter.Moved(-0.5 * aTxt.Width, -0.25);

                            aTxt.MoveTo(v1);
                            Image.Entities.Add(aTxt);
                            v1 = aTxt.BoundingRectangle().MiddleLeft();
                            v1.Tag = Tool.StampPNLeader(Image, true);
                            ldrPts.Add(v1, bAddClone: true);
                        }
                    }
                    else
                    {
                        if (bSpliceType == uppSpliceIndicators.ToDowncomer)
                        {
                            v1 = Perim.BoundingRectangle().BottomCenter.Moved(aYChange: 0.25);
                            aTxt = Image.EntityTool.Create_Text(v1, "XXXX", 0.5, dxxMTextAlignments.BottomCenter, aStyleName: "Standard");
                            v2 = baseRec.BottomCenter.Moved(0.5 * rPaperScale, -0.25 * rPaperScale);
                        }
                        else
                        {
                            v1 = Perim.BoundingRectangle(dxfPlane.World).TopCenter.Moved(aYChange: -0.25);
                            aTxt = Image.EntityTool.Create_Text(v1, "XXXX", 0.5, dxxMTextAlignments.TopCenter, aStyleName: "Standard");
                            v2 = baseRec.TopCenter.Moved(0.5 * rPaperScale, 0.25 * rPaperScale);
                        }

                        Image.Entities.Add(aTxt);
                        txt1 = Tool.StampPNLeader(Image);

                        draw.aLeader.Text(v1, v2, txt1);
                    }
                }
                //================== LEADERS =============================================
                Status = "Adding Leaders";
                {
                    if (RCHoles.Count > 0)
                    {
                        dxfEntity hole = RCHoles.GetByPoint(dxxPointFilters.GetRightBottom);

                        v1 = new dxfVector(baseRec.Right + 0.25 * rPaperScale, baseRec.Bottom - 0.125 * rPaperScale);
                        Tool.CreateHoleLeader(Image, hole, v1, dims, RCHoles.Count);

                    }
                    if (aHoles.Count > 0)
                    {

                        dxxPointFilters fltr = aSection.SectionIndex == 1 ? dxxPointFilters.AtMaxX : dxxPointFilters.AtMinX;
                        dxfEntity hole = aHoles.GetByPoint(fltr);
                        ldrPts.Add(new dxfVector(hole.X, hole.Y), bAddClone: true, aTag: Tool.HoleLeader(Image, hole, aHoles.Count));
                    }

                    if (rBubPromoters.Count > 0)
                    {
                        v1 = rBubPromoters.GetByPoint(dxxPointFilters.AtMinX).DefinitionPoint(dxxEntDefPointTypes.Center).Clone();
                        if (v1.X >= ldrCtr.X)
                            v1.Move(mdGlobals.BPRadius);
                        else
                            v1.Move(-mdGlobals.BPRadius);
                        ldrPts.Add(v1, bAddClone: true, aTag: "BUBBLE PROMOTER");
                    }

                    draw.aLeader.Stacks(ldrPts, ldrCtr, 0.5 * rPaperScale, 0, 0, aRightLeaderAngle: (aSection.SectionIndex == 1) ? 135 : 45);

                    //======================= TAB SLOT LEADER ===========================================
                    if (tSlots.Count > 0)
                    {
                        aPts = DimPts.GetByTag("TABSLOT");
                        v1 = aPts.GetVector(dxxPointFilters.AtMaxX);
                        txt1 = Tool.HoleLeader(Image, tSlots.Item(1), tSlots.Count, false);
                        txt1 = $"{txt1}\\PFEMALE JOGGLE DETAIL\\PSEE SHEET 6";
                        v2 = v1.Clone();
                        v2.SetCoordinates(v1.X + 0.5 * rPaperScale, baseRec.Bottom - 0.5 * rPaperScale);
                        draw.aLeader.Text(v1, v2, txt1);
                    }

                }
                rEntRectangle = Image.SelectionSet().BoundingRectangle(dxfPlane.World);

                //======================= Draw The profile view ===========================================
                if (bProfile)
                {
                    v1 = dxfVector.Zero;
                    v1.SetCoordinates(rEntRectangle.Right + 0.55 * rPaperScale, baseRec.Y);
                    Draw_SectionProfile(v1, rPaperScale, aSection, Perim, 90, tSpliceType, bSpliceType);
                }

                _rVal = Image.SelectionSet(dxxSelectionTypes.SelectAll);
                _rVal.Remove(rCornerNote);

                return _rVal;

            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }

            return _rVal;
        }

        private void DDWG_SlotLayout()
        {
       
            mdSlotZones aZones = Assy.GenerateSlotZones();
            dxoDrawingTool draw = Image.Draw;

            try
            {

               dxeArc shell = draw.aCircle(null, Assy.ShellRadius, dxfDisplaySettings.Null(aColor: dxxColors.LightGrey));
               
                dxeArc ring = draw.aCircle(null, Assy.RingRadius, dxfDisplaySettings.Null(aColor: dxxColors.Blue, aLinetype: dxfLinetypes.Hidden));
                bool includesectionperimeters = Drawing.Options.AnswerB("Include Deck Section Perimeters?", true);
                bool includebounds = Drawing.Options.AnswerB("Include Zone Boundary?", true);
                bool includeblockedareas = Drawing.Options.AnswerB("Include Zone Blocked Areas?", true);
                bool includesuppressed = Drawing.Options.AnswerB("Show Suppressed Slots?", true);
                bool includezoneorigin = Drawing.Options.AnswerB("Show Zone Origin?", true);
                bool includemirrorlines = Drawing.Options.AnswerB("Show Zone Mirror Line?", true);
                bool includenames = Drawing.Options.AnswerB("Show Zone Names?", true);
                bool includegridlines = Drawing.Options.AnswerB("Show Grid Lines?", false);
                bool includeweirs = Drawing.Options.AnswerB("Show Weir Lines?", false);
                bool includesplangles = Drawing.Options.AnswerB("Show Splice Angles?", false);
                bool regenslots = Drawing.Options.AnswerB("Regenerate Slots?", false);
                bool showcenterpoints = Drawing.Options.AnswerB("Show Slot Center Points?", true);

                double paperscale = Image.Display.ZoomExtents(bSetFeatureScale: true);
                paperscale *= 0.5;
                Image.Display.SetFeatureScales(paperscale);
                // draw.aCenterlines(ring, 0.2 * paperscale);

                if(regenslots && DrawlayoutRectangles)
                {
                    aZones = Assy.GenerateSlotZones(true);
                    regenslots = false;
                }

                Image.Header.UCSMode = dxxUCSIconModes.None;
                var Panels = Assy.DeckPanels.ActivePanels(Assy);
                dxfRectangle ZoomBox = Assy.DeckPanels.Bounds(bExcludeVirtuals: true).ToDXFRectangle(); // new dxfRectangle(new dxfVector(d1 + wd / 2, 0), wd, ht);
                ZoomBox.Expand(1.05, true, true);

                Image.Display.SetDisplayWindow(ZoomBox);


                draw.aCenterlines(shell, 0.25 * paperscale);

                 paperscale = Image.Display.ZoomExtents(1.05, true);
                Image.Header.UCSMode = dxxUCSIconModes.LowerLeft;

                double tht = paperscale * 0.125;
                dxfVector v1 = new(0, ZoomBox.Top - 0.1 * tht);
                foreach (var item in Panels)
                {
                    v1.X = item.X;
                    draw.aText(v1, item.Index, tht, dxxMTextAlignments.TopCenter);
                }
             
                Image.Layers.Add("SLOT_ZONES");
                if(includesectionperimeters) Image.Layers.Add("SECTION PERIMETERS", dxxColors.DarkGrey);

                foreach (var zone in aZones)
                {
                    zone.UpdateSlotPoints(Assy, bMaintainSuppressed: true, regenslots);
                    dxfBlock zoneblock = mdBlocks.Slot_Zone(Image, zone, "SLOT_ZONES", bIncludeBounds: includebounds,bIncludeSectionPerimeter : includesectionperimeters, bIncludeBlockedAreas: includeblockedareas, bIncludeMirrorLine: includemirrorlines, bIncludeWeirLines: includeweirs, bIncludeOrginCircle: includezoneorigin, bIncludeHandle: includenames, bIncludeSupressed: includesuppressed, bIncludeGridLines: includegridlines, bDontAddToImage: false, bAddCenterPoints: showcenterpoints);
                    draw.aInsert(zoneblock.Name, zone.Center);

                    //if(zone.Handle == "2,1")
                    //{
                    //    Console.WriteLine("HERE");
                    //    Tool.NumberVectors(draw, zone.ColumnLines().EndPoints(bGetEnd:false), paperscale);
                    //    Tool.NumberVectors(draw, zone.RowLines().EndPoints(bGetEnd: false), paperscale);
                    //}
                }



                List<mdDowncomerBox> boxes = Assy.Downcomers.Boxes();
          
                    foreach (var box in boxes)
                    {
                        
                        draw.aPolyline(box.EndPlate(bTop: true).View_Plan(true).Vertices, bClosed: true, aDisplaySettings: dxfDisplaySettings.Null(aLayer: "END PLATES", aColor: dxxColors.BlackWhite, aLinetype: dxfLinetypes.Continuous));
                        draw.aPolyline(box.EndPlate(bTop: false).View_Plan(true).Vertices, bClosed: true, aDisplaySettings: dxfDisplaySettings.Null(aLayer: "END PLATES", aColor: dxxColors.BlackWhite, aLinetype: dxfLinetypes.Continuous));
                    }
          
                if (Assy.DesignFamily.IsBeamDesignFamily())
                {
                    dxfBlock beamblock = mdBlocks.Beam_View_Plan(Image, Assy.Beam, Assy, bSetInstances: true, bObscured: true, bSuppressHoles: true, aCenterLineLength: 0, aLayerName: "BEAM", bIncludeSupports: false);

                    draw.aInserts(beamblock, null, bOverrideExisting: false);
                }

                if (includesplangles)
                {
                    Status = "Drawing Support Parts";
                    {
                        //Draw_Rings(uppViews.LayoutPlan);
                        Draw_SpliceAngles(uppViews.LayoutPlan, out _);
                        Draw_ManwayFasteners(uppViews.LayoutPlan, false);
                        // Draw_Downcomers(uppViews.LayoutPlan, out _, bDrawSpouts: false, bDrawBothSides: false, bSuppressHoles: true, aLayer: "0", bDrawFingerClips: false, bDrawStiffeners: false, bDrawEndAngles: false, bSuppressEndSupports: true);

                    }

                }

                //Status = "Drawing Deck Sections";
                //{
                //    //Image.LinetypeLayers.Clear();
                //    aSecs = Assy.UniqueDeckSections(false).FindAll(x => !x.IsVirtual && x.InstalledOnAlternateRing1).OfType<mdDeckSection>().ToList();
                //    dsp = dxfDisplaySettings.Null("SECTIONS");
                //    foreach (mdDeckSection section in aSecs)
                //    {

                //        colDXFVectors aPts = section.Instances.MemberPointsDXF();
                //        if (section.X > 0 && Assy.IsStandardDesign) aPts.GetVectors(aFilter: dxxPointFilters.LessThanX, aOrdinate: 0, bOnIsIn: false, bRemove: true);
                //        aPG = section.View_Plan(Assy, bObscured: true, bIncludePromoters: true, bIncludeHoles: true, bIncludeSlotting: false, bRegeneratePerimeter: DrawlayoutRectangles, aCenter: dxfVector.Zero, aRotation: 0, aLayerName: dsp.LayerName);
                //        Image.LinetypeLayers.ApplyTo(aPG);
                //        draw.aInserts(aPG, aPts, aBlockName: $"SECTION_{section.PartNumber}_Perim", aDisplaySettings: dsp, aLTLSetting: dxxLinetypeLayerFlag.ForceToLayer);
                //        xCancelCheck();
                //    }


                //    xCancelCheck();
                //}

                //Draw_ECMDSlots(false, false);

            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = string.Empty; DrawinginProgress = false; }
        }

        private void DDWG_ManwaySplice()
        {
            // ^creates the component drawing for the tray assembly manway splice plate
            mdSpliceAngle aAng = MDProject.GetParts().ManwaySplicePlates()[Drawing.PartIndex - 1];

            if (aAng == null) return;

            dxfVector v1 = dxfVector.Zero;
            dxfVector v2;
            dxfVector v3;
            dxfVector v4;

            double paperscale = 0;
            double thk = aAng.Thickness;


            List<dxeLine> bEnts;
            colDXFVectors ePts;
            colDXFVectors dPts;
            hdwHexBolt aBolt;
            colDXFEntities aEnts;
            dxeDimension aDim = null;
            dxfRectangle aRec;
            dxfRectangle bRec;
            dxfRectangle fitRec = null;
            string txt = "";


            try
            {

                dxoDrawingTool draw = Image.Draw;
                dxoDimTool dims = Image.DimTool;
                dxeInsert ins;
                DrawinginProgress = true;

                Image.LayerColorAndLineType_Set("GEOMETRY", dxxColors.ByLayer, "ByLayer", true);
                dxePolygon Perim = aAng.View_Plan(false, bSuppressOrientations: true, aCenter: dxfVector.Zero);

                aBolt = MDProject.SmallBolt("");
                aRec = Perim.Vertices.BoundingRectangle(dxfPlane.World);
                aRec.Stretch(aAng.Height, true, false, false, false);
                v1.X = 0.35 * aAng.Length;
                paperscale = 0;
                //=====================================================================================================
                // insert the border
                Tool.Border(this, aRec, ref fitRec, ref paperscale, 6, 4, false);
                xCancelCheck();

                //=====================================================================================================

                Status = "Drawing Plan View";
                {

                    if (DrawlayoutRectangles)
                    {
                        draw.aPolyline(fitRec, new dxfDisplaySettings(aColor: dxxColors.LightBlue));
                    }

                    Image.DimStyleOverrides.TextFit = dxxDimTextFitTypes.MoveArrowsFirst;

                    //===================== TOP VIEW ========================

                    Image.SelectionSetInit(false);
                    v1 = new dxfVector(fitRec.Left + 0.5 * aRec.Width + 0.85 * paperscale, fitRec.Y + 0.5 * paperscale);
                    ins = draw.aInsert(Perim, v1, aBlockName: $"{aAng.PartNumber} _VIEW_PLAN_MFG", aDisplaySettings: new dxfDisplaySettings(aLayer: "GEOMETRY"));
                    Perim.MoveTo(ins.InsertionPt);


                    aEnts = Perim.AdditionalSegments.GetByGraphicType(dxxGraphicTypes.Arc);

                    aRec = Perim.BoundingRectangle();

                    ePts = aEnts.Centers(bReturnClones: true);
                    dPts = new colDXFVectors();
                    colDXFEntities cLines = new();
                    colDXFEntities hLines = new();
                    v2 = null;
                    dxeLine vcl;
                    dxeLine hcl;
                    List<dxePolyline> bolts = Perim.AdditionalSegments.Polylines();
                    ePts = new colDXFVectors(Perim.GetVertex(dxxPointFilters.GetBottomLeft, bReturnClone: true, aPrecis: 1));

                    foreach (dxePolyline item in bolts)
                    {
                        bEnts = draw.aCenterlines(item, 0.125 * paperscale);
                        vcl = bEnts.Find((x) => x.Flag == "VERTICAL");
                        hcl = bEnts.Find((x) => x.Flag == "HORIZONTAL");

                        if (item.Vertex(1).Y < Perim.Y)
                        {
                            cLines.Add(vcl, bAddClone: true);
                            ePts.Add(vcl.EndPoints().GetVector(dxxPointFilters.AtMinY, bReturnClone: true));
                        }
                        hLines.Add(hcl, bAddClone: true);
                    }

                    ePts.Add(Perim.GetVertex(dxxPointFilters.GetBottomRight, bReturnClone: true, aPrecis: 1));

                    ePts.Sort(dxxSortOrders.LeftToRight);
                    v1 = ePts.Item(1);
                    v4 = v1.Moved(0, -0.375 * paperscale);
                    for (int i = 2; i <= ePts.Count - 2; i++)
                    {
                        v2 = ePts.Item(i);
                        v3 = ePts.Item(i + 1);
                        if (i == 2)
                        {
                            aDim = dims.Horizontal(v2, v1, v4.Y, bAbsolutePlacement: true);
                        }
                        aDim = dims.Horizontal(v2, v3, v4.Y, bAbsolutePlacement: true);
                    }

                    bRec = Image.SelectionSet(dxxSelectionTypes.CurrentSet).BoundingRectangle();
                    v2 = ePts.LastVector(bReturnClone: true);
                    aDim = dims.Horizontal(v2, v1, bRec.Bottom - 0.35 * paperscale, bAbsolutePlacement: true);

                    ePts = Perim.AdditionalSegments.GetByGraphicType(dxxGraphicTypes.Arc).Centers();
                    if (ePts.Count > 0)
                    {
                        v1 = ePts.GetVector(dxxPointFilters.GetTopRight, aPrecis: 1);
                        dxeArc hole = Perim.AdditionalSegments.Arcs().Find(x => x.X == v1.X && x.Y == v1.Y);
                        v2 = v1.Moved(-0.35 * paperscale);
                        v2.Y = bRec.Top + 0.65 * paperscale;
                        aDim = Tool.CreateHoleLeader(Image, hole, v2, dims, ePts.Count, aWeldedHDW: aBolt);


                    }

                    bRec = Perim.Vertices.BoundingRectangle(dxfPlane.World);
                    v1 = bRec.TopCenter.Moved(aYChange: -0.125 * paperscale);

                    if (uopUtils.IsOdd(ePts.Count / 2))
                    {
                        v1.X -= aAng.BoltSpacing / 2;
                    }
                    v2 = new dxfVector(v1.X - 0.5 * paperscale, bRec.Top + 0.35 * paperscale);

                    draw.aText(v1, "XXX", 0.5, aAlignment: dxxMTextAlignments.TopCenter);
                    draw.aLeader.Text(v1, v2, Tool.StampPNLeader(Image));


                    ePts = hLines.EndPoints(bReturnClones: true);
                    if (ePts.Count > 0)
                    {
                        v1 = bRec.TopRight;
                        aDim = dims.Vertical(ePts.GetVector(dxxPointFilters.GetRightTop), v1, v1.X + 0.35 * paperscale, bAbsolutePlacement: true);
                        dims.Vertical(ePts.GetVector(dxxPointFilters.GetRightBottom), bRec.BottomRight, v1.X + 0.35 * paperscale, bAbsolutePlacement: true);
                    }

                }
                //===================== SECTION VIEW ========================

                Status = "Drawing Section View";
                {


                    Image.SelectionSetInit(false);
                    dxePolygon Profile = aAng.View_Profile(false, aRotation: 90, aCenter: dxfVector.Zero, aLayerName: "GEOMETRY", bAddHolePoints: false, bAddBolt: true);

                    bRec = Profile.BoundingRectangle();
                    v1 = new dxfVector(fitRec.Right - 0.5 * bRec.Width - 2 * paperscale, ins.Y);
                    ins = draw.aInsert(Profile, v1, aBlockName: $"{aAng.PartNumber} _VIEW_SECTION_MFG", aDisplaySettings: new dxfDisplaySettings(aLayer: "GEOMETRY"));

                    Profile.MoveTo(ins.InsertionPt);

                    Image.DimStyleOverrides.TextFit = dxxDimTextFitTypes.BestFit;

                    v1 = Profile.FilletPoints(2 * thk, false).GetVector(dxxPointFilters.AtMinY);

                    draw.aCircles(dPts, 0.02 * paperscale, aDisplaySettings: new dxfDisplaySettings(aColor: dxxColors.Cyan));

                    dPts = Profile.FilletPoints(thk, false);
                    dPts.Remove(dPts.GetVector(dxxPointFilters.AtMinY));
                    //draw.aCircles(dPts, 0.02 * paperscale, aDisplaySettings: new dxfDisplaySettings(aColor: dxxColors.Red));
                    v1 = new colDXFVectors(Profile.Vertices.GetVectors(aFilter: dxxPointFilters.AtX, aOrdinate: v1.X)).GetVector(dxxPointFilters.AtMinY);
                    v2 = Profile.GetVertex(dxxPointFilters.GetBottomRight, aPrecis: 1);


                    dims.Horizontal(v1, v2, v2.Y - 0.5 * paperscale, bAbsolutePlacement: true);

                    colDXFEntities clines = Profile.AdditionalSegments.GetByFlag("CENTERLINE");
                    bRec = clines.BoundingRectangle(dxfPlane.World);
                    v2 = dPts.GetVector(dxxPointFilters.AtMinY);
                    v1 = Profile.GetVertex(dxxPointFilters.GetBottomLeft, aPrecis: 1);
                    v3 = new colDXFVectors(Profile.Vertices.GetVectors(aFilter: dxxPointFilters.AtX, aOrdinate: v2.X)).GetVector(dxxPointFilters.AtMaxY);
                    dims.Vertical(v3, v1, bRec.Left - 0.25 * paperscale, bAbsolutePlacement: true);

                    v2 = dPts.GetVector(dxxPointFilters.AtMaxY);
                    v1 = Profile.GetVertex(dxxPointFilters.GetTopRight, aPrecis: 1);
                    v4 = new colDXFVectors(Profile.Vertices.GetVectors(aFilter: dxxPointFilters.AtX, aOrdinate: v2.X)).GetVector(dxxPointFilters.AtMinY);
                    dims.Vertical(v4, v1, bRec.Right + 0.25 * paperscale, bAbsolutePlacement: true);

                    bRec = Image.SelectionSet(dxxSelectionTypes.CurrentSet).BoundingRectangle(dxfPlane.World);
                    dims.Horizontal(v1, v3, bRec.Top + 0.25 * paperscale, bAbsolutePlacement: true);

                    v1 = Profile.GetVertex(dxxPointFilters.GetTopLeft);
                    v2 = Profile.GetVertex(dxxPointFilters.GetBottomLeft);
                    dims.Vertical(v1, v2, bRec.Left - 0.25 * paperscale, bAbsolutePlacement: true);


                    bRec = Image.SelectionSet(dxxSelectionTypes.CurrentSet).BoundingRectangle(dxfPlane.World);
                    colDXFEntities welds = Profile.AdditionalSegments.GetByGraphicType(dxxGraphicTypes.Solid);
                    dPts = welds.Centers();
                    v1 = dPts.GetVector(dxxPointFilters.GetTopLeft);
                    v2 = new dxfVector(v1.X - 0.5 * paperscale, bRec.Top + 0.5 * paperscale);
                    txt = "TACK WELD 2 PLACES\\P@ 180%%D APART (TYP)";
                    draw.aSymbol.Weld(new colDXFVectors(v1, v2), dxxWeldTypes.Fillet, NoteText: txt);

                }


                //================== NOTES ==================================


                Tool.AddBorderNotes(aAng, Project, "MANWAY DECK SPLICE", false, Image, fitRec, paperscale);

                // Tool.Add_B_BorderNotes(Image, notes, fitRec, paperscale);
            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
            finally
            {
                Status = "";
                DrawinginProgress = false;
            }
        }
        private void DDWG_ManwaySplice_Tabular()
        {
            //^generates the project wide splice angle drawing

            dxfVector v1;
            dxfVector v2;
            dxfRectangle fitRec = null;
            dxoDimStyle DStyle = Image.DimStyle();
            dxeDimension dim;
            string txt;
            double y1;
            mdSpliceAngle angl;
            dxfRectangle rec;
            double sclr = 2;

           // MDProject.InvalidateParts(aPartType: uppPartTypes.ManwaySplicePlate);

            uopPartList<mdSpliceAngle> SAngles = MDProject.GetParts().ManwaySplicePlates();
            try
            {

                dxoDrawingTool draw = Image.Draw;
                dxoDimTool dims = Image.DimTool;
                dxoLeaderTool leaders = Image.LeaderTool;
                mdSpliceAngle angle = SAngles.Count > 0 ? (mdSpliceAngle)SAngles[0] : null;
                dxePolygon planview = null;
                dxePolygon profileview = null;
                dxeInsert ins;

                //================== Border File ==================================
                double paperscale = 1;
                Tool.Border(this, null, ref fitRec, arPaperScale: ref paperscale, bSuppressAnsiScales: true, aScaleString: "NTS");
                xCancelCheck();
                Drawing.ZoomExtents = false;


                Image.TableSettings.ColumnGap = 0.1;
                Status = "Drawing  Plan View";
                {

                    Image.SelectionSetInit(false);
                    angle = SAngles.Count > 0 ? (mdSpliceAngle)SAngles[0] : null;
                    double width = angle != null ? angle.Width : 2.5;
                    double inset = angle != null ? angle.HoleInset : 0.6875;
                    double? holedia = angle != null ? angle.BoltHole.Diameter : mdGlobals.gsSmallHole;
                    profileview = uopPolygons.GenericSpliceAngle_Profile(uppPartTypes.ManwaySplicePlate, out planview);
                    planview.Rescale(sclr);
                    profileview.Rescale(sclr);

                    //planview = uopPolygons.GenericSpliceAngle_Plan(i == 1 ? uppPartTypes.SpliceAngle : uppPartTypes.ManwayAngle);
                    v1 = new dxfVector(1.65, 8.75);
            
                    ins = draw.aInsert(planview, v1, aDisplaySettings: dxfDisplaySettings.Null(aLayer: "GEOMETRY"), aLTLSetting: dxxLinetypeLayerFlag.ForceToColor);
                    planview.MoveTo(ins.InsertionPt);
                    rec = Image.SelectionSet(dxxSelectionTypes.CurrentSet).BoundingRectangle(dxfPlane.World);
                  
                    if (DrawlayoutRectangles)
                    {
                        draw.aPolyline(rec, aDisplaySettings: new dxfDisplaySettings(aColor: dxxColors.LightGrey));
                    }
                    txt = Image.FormatNumber(width);
                    dim = dims.Vertical(planview.Vertex(3), planview.Vertex(2), v1.X - 0.25 * paperscale, bAbsolutePlacement: true, aOverideText: txt);
                    colDXFVectors dimpts = planview.AdditionalSegments.GetByTag("VER_CENTERLINE").DefinitionPoints(dxxEntDefPointTypes.StartPt);
                    y1 = dimpts.Item(1).Y + 0.25 * paperscale;
                    dim = dims.Horizontal(dimpts.Item(1), planview.Vertex(2), y1, bAbsolutePlacement: true, aOverideText: "\"C\"");
                    dim = dims.Horizontal(dimpts.Item(4), planview.Vertex(7), y1, bAbsolutePlacement: true, aOverideText: "\"C\"");
                    dim = dims.Horizontal(dimpts.Item(1), dimpts.Item(2), y1, bAbsolutePlacement: true, aOverideText: "\"B\"");
                    dim = dims.Horizontal(dimpts.Item(2), dimpts.Item(3), y1, bAbsolutePlacement: true, aOverideText: "\"B\"");

                    rec = Image.SelectionSet(dxxSelectionTypes.CurrentSet).BoundingRectangle();
                    y1 = rec.Top + 0.25 * paperscale;
                    dim = dims.Horizontal(planview.Vertex(2), planview.Vertex(7), y1, bAbsolutePlacement: true, aOverideText: "\"A\"");

                    dimpts = planview.AdditionalSegments.GetByTag("HOR_CENTERLINE").DefinitionPoints(dxxEntDefPointTypes.EndPt);
                    dim = dims.Vertical(dimpts.GetVector(dxxPointFilters.AtMaxY), planview.Vertex(7), rec.Right + 0.25 * paperscale, bAbsolutePlacement: true, aOverideText: Image.FormatNumber(inset));
                    dim = dims.Vertical(planview.Vertex(6), dimpts.GetVector(dxxPointFilters.AtMinY), rec.Right + 0.125 * paperscale, bAbsolutePlacement: true, aOverideText: Image.FormatNumber(inset));

                    //hole diameter leader
                    dxeArc bolthole = planview.AdditionalSegments.Arcs()[3];
                    txt = Tool.HoleLeader(Image, bolthole, 0, aDiameter: holedia);
                    dim = dims.RadialD(bolthole, 360 - 75, 0.75, aOverideText: txt);
                    v1 = new dxfVector(5.5, 7.525);

                    //stamp pn leader
                    dxeText mtxt = draw.aText(v1, "XXX", 0.2 * paperscale, aAlignment: dxxMTextAlignments.MiddleCenter);
                    v1 = mtxt.BoundingRectangle().BottomCenter;
                    v2 = dim.BoundingRectangle().BottomLeft.Moved(-0.075 * paperscale, -0.25 * paperscale);
                    leaders.Text(v1, v2, Tool.StampPNLeader(Image));
                    List<dxePolyline> hexes = planview.AdditionalSegments.Polylines().FindAll(x => string.Compare(x.Tag, "HEX_HEAD", true) == 0);

                    // bolt leader
                    hdwHexBolt bolt = MDProject.SmallBolt(bWeldedInPlace: true);
                    if(hexes.Count > 0){
                        v1 = hexes[0].Vertex(5);
                        v2.X = v1.X + 0.25 * paperscale;
                        txt = bolt.IsMetric ? (bolt.Length * 25.4).ToString("0") : bolt.Length.ToString("0.0##") + "''";
                        txt = $"{bolt.SizeName} X {txt} HEX HEAD BOLTS";
                        txt = $"{txt}\\PTACK WELDED TO ANGLE";
                        txt = $"{txt}\\PWITH TWO OPPOSING TACKS (TYP)";
                        leaders.Text(v1, v2, txt);
                        rec = Image.SelectionSet(dxxSelectionTypes.CurrentSet).BoundingRectangle(dxfPlane.World);

                        if (DrawlayoutRectangles)
                        {
                            draw.aPolyline(rec, aDisplaySettings: new dxfDisplaySettings(aColor: dxxColors.LightGrey));
                        }

                    }
                }
                Status = "Drawing Cross Section View";
                {

                    v1 = new dxfVector(rec.Right + 1.5 * paperscale, planview.Y);
                    ins = draw.aInsert(profileview, v1);
                    profileview.MoveTo(v1);
                    v1 = profileview.GetVertex(dxxPointFilters.GetBottomRight, aPrecis: 1);
                    v2 = profileview.Vertex(profileview.Vertices.IndexOf(v1) + 2);
                    dxfVector v3 = profileview.Vertex(profileview.Vertices.IndexOf(v1) - 1);
                    double thk = v3.Y - v1.Y;
                    dim = dims.Horizontal(v2, v1, v1.Y - 0.25 * paperscale, bAbsolutePlacement: true, aOverideText: "\"D\"");
                }

                List<string> atble = new() { "PT.\\PNO|\"A\"|\"B\"|\"C\"|\"D\"|\"E\"|BOLT\\PCOUNT|NO. PER\\PTRAY|NO.\\PREQ'D.*|FOR\\PTRAYS" };

                for (int j = 1; j <= SAngles.Count; j++)
                {
                    angl = (mdSpliceAngle)SAngles[j - 1];
                    List<string> values = new() { 
                        angl.PartNumber,
                        DStyle.FormatNumber(angl.Length),
                        DStyle.FormatNumber(angl.BoltSpacing),
                        DStyle.FormatNumber(1),
                        DStyle.FormatNumber(angl.Height),
                        DStyle.FormatNumber(angl.JoggleDepth),
                        (angl.BoltCount * 2).ToString(),
                        angl.OccuranceFactor.ToString(),
                        uopUtils.CalcSpares(angl.Quantity, MDProject.ClipSparePercentage, 2).ToString(),
                        angl.RangeSpanNames(MDProject, ", ")
                    };
                    

                    txt = mzUtils.ListToString(values, "|");
                    atble.Add(txt);
                }
                atble.Add("|||||");
                v1 = rec.BottomLeft.Moved(aYChange: -0.5);
                //v1.X = 4 + (i - 1) * 8;

                v1.Y = draw.aTable($"TABLE_MANSPLICES", v1, atble, aDelimiter: "|", aTableAlign: dxxRectangularAlignments.TopLeft, aGridStyle: dxxTableGridStyles.All, aFooter: "* INCLUDES 2% SPARES").BoundingRectangle().Bottom - 0.5;
                if (fitRec.Bottom + 0.75 * paperscale < v1.Y) v1.Y = fitRec.Bottom + 0.75 * paperscale;
                v1.X = 1 + 8;



                //         draw.aText(v1, $"MANWAY SPLICE ANGLE\\PFOR MATERIAL SEE SHEET 2", 0.1875, dxxMTextAlignments.TopLeft, aTextStyle: "Standard");


                //break;
                //return; // ??? there are some codes after it in VB

                ////================== NOTES ==================================
                //Status = "Drawing Notes";
                //txt = "END ANGLE";
                //txt = $"{txt}\\PFOR MATERIAL SEE SHEET 2";
                txt = $"MANWAY SPLICE ANGLE\\PFOR MATERIAL SEE SHEET 2";
                Tool.AddBorderNotes(Image, txt, fitRec, 1);

                //Status = "";
                //DrawinginProgress = false;
                // }
            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = string.Empty; DrawinginProgress = false; }
        }


        private void DDWG_SpoutGroupSketch()
        {

            try
            {

                Tool.DDWG_SpoutGroupInputSketch(this);


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

        private void DDWG_SpliceAngle(bool DrawManAng)
        {
            // ^creates the component drawing for the tray assembly manway splice plate

            dxfVector v1;
            dxfVector v2;
            dxfVector v3;
            dxfVector v4;
            dxfVector v5;
            dxePolygon Perim;
            dxePolygon Profile;
            mdSpliceAngle aAng;
            double paperscale = 0;
            double thk;
            List<dxeLine> centerlines;
            colDXFEntities cLines;
            colDXFEntities hLines;
            colDXFEntities bEnts;
            colDXFVectors ePts;
            colDXFVectors dPts;
            hdwHexBolt aBolt;
            colDXFEntities aEnts;

            int i;
            dxePolyline aHex;
            dxeDimension aDim;
            dxfRectangle aRec;
            dxfRectangle bRec;
            dxfRectangle fitRec = null;
            dxeText aTxt;
            string txt = "";
            dxePolygon aPGon = null;
            colUOPParts aParts;

            try
            {
                aParts = Assy.SpliceAngles(true);
                if (DrawManAng)
                {
                    aAng = (mdSpliceAngle)aParts.GetPartByType(uppPartTypes.ManwayAngle);
                }
                else
                {
                    aAng = (mdSpliceAngle)aParts.GetPartByType(uppPartTypes.SpliceAngle);
                }

                if (aAng == null)
                {
                    return;
                }
                DrawinginProgress = true;
                v1 = dxfVector.Zero;
                thk = aAng.Thickness;

                Image.LayerColorAndLineType_Set("GEOMETRY", dxxColors.ByLayer, "ByLayer", true);

                Perim = aAng.View_Plan(false, bSuppressOrientations: true);
                Profile = aAng.View_Profile(false, aRotation: 90, bAddHolePoints: true);
                aBolt = aAng.Bolt;
                aRec = Perim.BoundingRectangle();
                aRec.Stretch(aAng.Height, true, false, false, false);
                v1.X = 0.35 * aAng.Length;

                //=====================================================================================================
                // insert the border
                Tool.Border(this, aRec, ref fitRec, ref paperscale, 6, 4, true, "NTS");
                xCancelCheck();

                //=====================================================================================================
                if (DrawlayoutRectangles)
                {
                    Image.Screen.Entities.aRectangle(fitRec, aColor: dxxColors.LightGrey);
                }
                Image.DimStyleOverrides.TextFit = dxxDimTextFitTypes.MoveArrowsFirst;

                //===================== TOP VIEW ========================

                Perim.Move(fitRec.Right - aRec.Right - 1 * paperscale, fitRec.Y - aRec.Y + 0.5 * paperscale);

                // Profile.Rotate 90
                bRec = Profile.BoundingRectangle();

                aEnts = Perim.AdditionalSegments.GetByGraphicType(dxxGraphicTypes.Arc);
                aHex = (dxePolyline)Image.Primatives.Polygon(v1, false, 6, aBolt.G / 2);

                Image.SelectionSetInit(false);

                Image.Draw.aPolygon(Perim, aLayer: "GEOMETRY");

                aRec = Perim.BoundingRectangle();

                ePts = aEnts.Centers(bReturnClones: true);
                dPts = new colDXFVectors();
                cLines = new colDXFEntities();
                hLines = new colDXFEntities();
                v2 = null;
                aDim = null;
                aHex.LCLSet("HIDDEN", dxxColors.ByLayer, "ByLayer");

                for (i = 1; i <= ePts.Count; i++)
                {
                    v1 = ePts.Item(i);
                    aHex.MoveFromTo(aHex.Center(), v1);
                    Image.Entities.Add(aHex, bAddClone: true);
                    centerlines = Image.Draw.aCenterlines(aHex, 0.125 * paperscale);
                    if (v1.Y > Perim.Y)
                    {
                        cLines.Append(centerlines.FindAll((x) => x.Flag == "VERTICAL"), true);
                        if (v2 == null)
                        {
                            v2 = v1.Clone();
                        }
                        if (DrawManAng)
                        {
                            dPts.Add(v1, bAddClone: true);
                        }
                    }
                    else
                    {
                        dPts.Add(v1, bAddClone: true);
                    }
                    hLines.Append(centerlines.FindAll((x) => x.Flag == "HORIZONTAL"), true);
                }

                if (v2 != null)
                {
                    ePts = new colDXFVectors(cLines.EndPoints().GetVectors(aFilter: dxxPointFilters.GreaterThanY, aOrdinate: v2.Y, bReturnClones: true));
                    v3 = Perim.BoundingRectangle().TopLeft;
                    ePts.Add(v3, bAddClone: true);
                    ePts.Sort(dxxSortOrders.LeftToRight);

                    for (i = 1; i <= ePts.Count - 1; i++)
                    {
                        if (i == 1)
                        {
                            v2 = ePts.Item(i);
                            v1 = ePts.Item(i + 1);
                            Image.DimStyleOverrides.SuppressExtLine1 = true;
                        }
                        else
                        {
                            v1 = ePts.Item(i);
                            v2 = ePts.Item(i + 1);
                            if (i == 2)
                            {
                                Image.DimStyleOverrides.SuppressExtLine1 = false;
                            }
                        }

                        aDim = Image.Draw.aDim.Horizontal(v1, v2, v3.Y + 0.5 * paperscale, bAbsolutePlacement: true);
                    }

                    if (aDim != null)
                    {
                        aRec = aDim.BoundingRectangle();
                        v2 = Perim.BoundingRectangle().TopRight;
                        Image.Draw.aDim.Horizontal(v3, v2, aDim.DefPt10.Y + 0.35 * paperscale, bAbsolutePlacement: true);
                    }
                }

                if (dPts.Count > 0)
                {
                    dPts.Sort(dxxSortOrders.LeftToRight);
                    v1 = dPts.GetVector(dxxPointFilters.AtMaxX);
                    v1.MovePolar(240, 0.5 * aBolt.G, dxfPlane.World);
                    v2 = dxfVector.Zero;
                    v2.SetCoordinates(v1.X - 0.375 * paperscale, Perim.BoundingRectangle().Bottom - 0.75 * paperscale);
                    txt = Tool.HoleLeader(Image, aEnts.Item(1), aEnts.Count, aWeldedHDW: aBolt, bSuppressTackText: true);

                    Image.Draw.aLeader.Text(v1, v2, txt);

                    v1 = dPts.Item(1, true);
                    v1.Move(0.5 * aAng.BoltSpacing);
                    v1.Y = Perim.Y;

                    aTxt = Image.Draw.aText(v1, "XXX", 0.5, dxxMTextAlignments.MiddleCenter, aTextStyle: "Standard");

                    v1 = aTxt.BoundingRectangle().BottomCenter;
                    v2.X = v1.X + 0.375 * paperscale;
                    Image.Draw.aLeader.Text(v1, v2, Tool.StampPNLeader(Image));
                }

                dPts = hLines.EndPoints(bReturnClones: true);
                if (dPts.Count > 0)
                {
                    aRec = Perim.BoundingRectangle();
                    aDim = Image.Draw.aDim.Vertical(dPts.GetVector(dxxPointFilters.GetRightTop), aRec.TopRight, 0.35);
                    if (!DrawManAng)
                    {
                        Image.Draw.aDim.Vertical(dPts.GetVector(dxxPointFilters.GetLeftBottom), aRec.BottomLeft, -0.35);
                    }
                }

                //===================== SECTION VIEW ========================

                bRec = Image.SelectionSetInit(true).BoundingRectangle();
                v1 = Perim.BoundingRectangle().MiddleLeft();
                v1.X = fitRec.Left;
                aEnts = new colDXFEntities();

                aRec = Profile.Vertices.BoundingRectangle();

                Profile.MoveFromTo(aRec.MiddleLeft(), v1, 1.25 * paperscale);
                Profile = Image.Draw.aPolygon(Profile, aLayer: "GEOMETRY");
                aEnts.Add(Profile);

                Image.DimStyleOverrides.TextFit = dxxDimTextFitTypes.BestFit;
                dPts = Profile.FilletPoints();

                aEnts.Add(Image.Draw.aDim.Horizontal(Profile.Vertex(1), Profile.Vertex(7), 0.375, aImage: _Image));
                aEnts.Add(Image.Draw.aDim.Vertical(Profile.Vertex(8), Profile.Vertex(2), -0.85, aImage: _Image));

                aRec = aEnts.BoundingRectangle();
                bEnts = Profile.AdditionalSegments.GetByGraphicType(dxxGraphicTypes.Point);
                aBolt.ObscuredLength = thk;
                v5 = null;
                if (bEnts.Count > 0)
                {
                    ePts = bEnts.Centers();
                    if (!DrawManAng)
                    {
                        v1 = ePts.GetVector(dxxPointFilters.AtMinY).Moved(thk / 2);
                        aPGon = aBolt.View_Profile(1, v1, aDirection: dxxOrthoDirections.Left, bIncludeCenterLine: true);
                        Image.LinetypeLayers.ApplyTo(aPGon, dxxLinetypeLayerFlag.ForceToLayer);
                        Image.Entities.Add(aPGon);

                        v2 = v1.Moved(0, 0.5 * aBolt.G);
                        v3 = v2.Moved(0, 0.08);
                        v4 = v2.Moved(0.08);
                        Image.Draw.aSolid(new colDXFVectors(v2, v3, v4), "GEOMETRY");

                        v2 = v1.Moved(0, -0.5 * aBolt.G);
                        v3 = v2.Moved(0, -0.08);
                        v4 = v2.Moved(0.08);
                        Image.Draw.aSolid(new colDXFVectors(v2, v3, v4), "GEOMETRY");

                        v5 = v3.MidPoint(v4);
                    }

                    v1 = ePts.GetVector(dxxPointFilters.AtMaxY).Moved(thk / 2);
                    aPGon = aBolt.View_Profile(1, v1, aDirection: dxxOrthoDirections.Left, bIncludeCenterLine: true);
                    Image.LinetypeLayers.ApplyTo(aPGon, dxxLinetypeLayerFlag.ForceToLayer);
                    Image.Entities.Add(aPGon);

                    v2 = v1.Moved(0, -0.5 * aBolt.G);
                    v3 = v2.Moved(0, -0.08);
                    v4 = v2.Moved(0.08);
                    Image.Draw.aSolid(new colDXFVectors(v2, v3, v4), "GEOMETRY");

                    v2 = v1.Moved(0, 0.5 * aBolt.G);
                    v3 = v2.Moved(0, 0.08);
                    v4 = v2.Moved(0.08);
                    Image.Draw.aSolid(new colDXFVectors(v2, v3, v4), "GEOMETRY");
                }

                if (v5 != null)
                {
                    v2 = v5.Moved(0.25 * paperscale);
                    v2.Y = aRec.Bottom - 0.675 * paperscale;

                    Image.Draw.aSymbol.Weld(new colDXFVectors(v5, v2), dxxWeldTypes.Fillet, bBothSides: false, bAllAround: false, NoteText: "TACK WELD 2 PLACES\\P180%%D APART (TYP)");
                }




                Tool.AddBorderNotes(aAng, Project, "", false, Image, fitRec, paperscale);

                ;
            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
                DrawinginProgress = false;
            }
            finally
            {
                Status = "";
                DrawinginProgress = false;
            }
        }


        private void DDWG_Stiffener()
        {
            // ^creates the component drawing for the tray assembly downcomer stiffener
            dxfVector v2 = dxfVector.Zero;
            dxfVector v3 = dxfVector.Zero;
            dxePolygon Profile;
            dxeLine cLine;
            dxfRectangle fitRec = null;
            dxeDimension aDim;
            dxeArc aArc;

            try
            {
                if (Drawing.Part == null)
                    return;

                dxoDrawingTool draw = Image.Draw;
                dxoDimTool dims = Image.DimTool;
                dxoLeaderTool leaders = Image.LeaderTool;

                mdStiffener aStiff = (mdStiffener)Drawing.Part; // aStfs.Item(Drawing.PartIndex);
                aStiff.UpdatePartProperties();
                bool bNotch = aStiff.SupplementalDeflectorHeight > 0;

                string lname = "GEOMETRY";
                Image.LinetypeLayers.Setting = dxxLinetypeLayerFlag.ForceToLayer;

                Image.Layers.Add(lname);

                dxfRectangle aRec = new(aStiff.Width + aStiff.FlangeWidth, aStiff.OverallHeight);

                //=================== BORDER =====================================
                double paperscale = 0;
                Tool.Border(this, aRec, ref fitRec, ref paperscale, 5, 3);
                xCancelCheck();
                //=================== BORDER =====================================

                //================== PLAN VIEW

                colDXFEntities dwgEnts = new();
                Image.SelectionSetInit(false);
                Status = "Creating Plan View Geometry";
                dxfVector v1 = fitRec.TopLeft.Moved(1 * paperscale + 0.5 * aStiff.Width, -1 * paperscale);
                dxePolygon Perim = aStiff.View_Plan(null, Assy, false, true, dxfVector.Zero, aLayerName: lname, bHoleCenterLines: true);

                Status = "Drawing Plan View Geometry";

                dxeInsert insert = draw.aInsert(Perim, v1, aBlockName: "Stiffener_View_ELEV");
                Perim.MoveTo(v1);

                //================== plan view dims

                Status = "Drawing Plan View Dimensions";
                if (aStiff.SupportsBaffle)
                {
                    v1 = Perim.Vertex(11, true);
                    v2 = Perim.Vertex(16, true);
                    dims.Horizontal(v1, Perim.Vertex(4), -0.35);
                    dims.Horizontal(v1, v2, -0.75);
                    dims.Vertical(v2, Perim.Vertex(14), 0.65);
                }
                else
                {
                    v1 = Perim.Vertex(1, true);
                    v2 = Perim.Vertex(6, true);
                    dims.Horizontal(v1, v2, -0.35);
                    dims.Vertical(v2, Perim.Vertex(4), 0.75);
                }

                dwgEnts = Image.SelectionSetInit(true);
                aRec = dwgEnts.BoundingRectangle();

                if (DrawlayoutRectangles)
                {
                    draw.aRectangle(aRec, aColor: dxxColors.LightGreen);
                    //draw.aRectangle(insert.BoundingRectangle(), aColor: dxxColors.LightBlue);
                }


                //================== ELEVATION VIEW



                Status = "Creating Elevation View Geometry";

                dxePolygon Elev = aStiff.View_Elevation(false, false, dxfVector.Zero, aLayerName: lname, bHoleCenterLines: true);

                v1 = aRec.BottomCenter.Moved(aYChange: -0.825 * paperscale - ((aStiff.BaffleMountHeight + 0.25 + aStiff.TopZ) * 0.75));
                v1.X = Perim.X;

                Status = "Drawing Elevation View Geometry";

                draw.aInsert(Elev, v1, aBlockName: "Stiffener_View_PLAN");
                Elev.MoveTo(v1);

                //================== elevation dims

                colDXFVectors aPts = new();
                if (aStiff.SupportsBaffle)
                {
                    v1 = Elev.Vertex(5, true);
                    v2 = Elev.LastVertex(true);
                    v3 = Elev.GetVertex(dxxPointFilters.GetRightTop, bReturnClone: true);

                    dims.Horizontal(Elev.Vertex(3), Elev.Vertex(2), 0.75);
                    dims.Horizontal(v1, Elev.Vertex(4), 0.35);

                    aPts.Add(v2, bAddClone: true);
                    aPts.Add(v3, bAddClone: true);
                    aPts.Add(v1, bAddClone: true);

                    if (bNotch)
                    {
                        aDim = dims.Horizontal(Elev.Vertex(12), v2, v2.Y - 0.35 * paperscale, bAbsolutePlacement: true);
                        aDim = dims.Horizontal(Elev.Vertex(12), Elev.Vertex(11), v2.Y - 0.35 * paperscale, bAbsolutePlacement: true);
                        aPts.Add(Elev.Vertex(12), bAddClone: true);
                    }
                    dims.Stack_Vertical(aPts, 0.6 * paperscale, aAdditionalDimSpace: 0.2 * paperscale);

                    v1 = Elev.Vertex(6, true);
                    v2 = Elev.Vertex(9, true);
                    v2.X = v1.X;
                    v3 = Elev.Vertex(8, true);

                    Image.DimStyleOverrides.SuppressExtLine1 = true;
                    dims.Vertical(v2, v1, 0.45);
                    Image.DimStyleOverrides.SuppressExtLine1 = false;
                    dims.Horizontal(v2.Moved(0, 0.25), v3, -0.25);

                    v2 = v3.PolarVector(135, 0.35 * paperscale);
                    leaders.Text(v3, v2, "R");
                }
                else
                {
                    v1 = Elev.Vertex(2, true);
                    aPts.Add(v1, bAddClone: true);
                    aPts.Add(Elev.Vertex(3), bAddClone: true);
                    if (bNotch)
                    {
                        aPts.Add(Elev.Vertex(6), bAddClone: true);
                        dims.Horizontal(Elev.Vertex(6), v1, v1.Y - 0.35 * paperscale, bAbsolutePlacement: true);
                        dims.Horizontal(Elev.Vertex(6), Elev.Vertex(7), v1.Y - 0.35 * paperscale, bAbsolutePlacement: true);
                    }

                    dims.Stack_Vertical(aPts, -0.35 * paperscale);

                }


                //================== PROFILE VIEW

                dwgEnts = Image.SelectionSetInit(true);
                aRec = dwgEnts.BoundingRectangle();

                if (DrawlayoutRectangles)
                {
                    Image.Screen.Entities.aRectangle(aRec, aColor: dxxColors.LightGrey);
                }


                Status = "Creating Profile View Geometry";
                v1 = aRec.MiddleRight().Moved(2.75 * paperscale + 0.5 * aStiff.FlangeWidth);
                v1.Y = Elev.Y;
                Profile = aStiff.View_Profile(false, aCenter: dxfVector.Zero, bMirrored: true);

                Status = "Drawing Profile View Geometry";

                draw.aInsert(Profile, v1, aBlockName: "Stiffener_View_PROFILE");
                Profile.MoveTo(v1);

                //====Profile dims

                //vertical center line

                aRec = Profile.BoundingRectangle();
                v1 = Profile.InsertionPt.Clone();
                cLine = draw.aLine(Profile.X, aRec.Bottom - 0.15 * paperscale, Profile.X, aRec.Top + 0.15 * paperscale, aLineType: dxfLinetypes.Center);

                aArc = (dxeArc)Profile.AdditionalSegments.GetTagged("MOUNT", aEntityType: dxxEntityTypes.Circle, bReturnClone: true);
                dxePoint spt = (dxePoint)Profile.AdditionalSegments.GetTagged("BAFFLE MOUNT", aEntityType: dxxEntityTypes.Point, bReturnClone: true);

                v3 = Profile.GetVertex(dxxPointFilters.GetLeftTop, bReturnClone: true);
                v1 = aStiff.SupportsBaffle ? Profile.Vertex(6, true) : v3;
                dims.Horizontal(v3, cLine.EndPt, 0.45);

                if (aArc != null)
                {
                    v2 = aArc.Center.Clone();
                    dims.Vertical(v1, v2, -0.9);
                    v2.SetCoordinates(aRec.Right + 0.35 * paperscale, aArc.Y + 0.45 * paperscale);
                    Tool.CreateHoleLeader(Image, aArc, v2, dims, 2);
                }

                if (spt != null)
                {
                    v2 = spt.Center;
                    dims.Vertical(v1, v2, -0.9);

                    Image.DimStyleOverrides.ForceTextBetweenExtLine = true;

                    dims.Vertical(v3, v2, -0.9, aSuffix: " (REF)");

                    v3 = v2 + new dxfDirection(0, 1, 0) * 11 / 25.4 / 2;
                    v2.SetCoordinates(aRec.Right + 0.35 * paperscale, v3.Y + 0.45 * paperscale);
                    dxePolyline slot = (dxePolyline)Profile.AdditionalSegments.GetTagged("BAFFLE MOUNT", aEntityType: dxxEntityTypes.Polyline, bReturnClone: true);
                    Tool.CreateHoleLeader(Image, slot, v2, dims, 1);


                }

                // Add notes
                string heading = $"{(aStiff.DesignFamily.IsEcmdDesignFamily() ? "ECMD STIFFENER" : "MD STIFFENER")} (SUB-PART)";

                string textForParts = uppPartTypes.Downcomer.Description().ToUpper();
                if (aStiff.ParentList.Count() > 1) textForParts += "S";
                List<string> parents = mzUtils.StringsFromDelimitedList(aStiff.ParentList).Distinct().ToList();
                int countDown = 2; // The first line should have 2 downcomers. Other lines should have 6 downcomers
                for (int i = 0; i < parents.Count; i++)
                {
                    if (countDown == 0)
                    {
                        parents[i] = $"\\P{parents[i]}";
                        countDown = 6;
                    }
                    countDown--;
                }
                textForParts = $"FOR {textForParts} {string.Join(", ", parents)}";

                string textForTrays = !aStiff.HasRingRanges ? aStiff.RangeSpanNames(Project, ", ").ToUpper() : aStiff.RingRanges.SpanNameList(", ");
                if (textForTrays.Contains('-') || textForTrays.Contains(','))
                    textForTrays = $"FOR TRAYS {textForTrays}";
                else
                    textForTrays = $"FOR TRAY {textForTrays}";

                string notes = $"{heading}\\P{textForParts}\\PFOR MATERIAL SEE SHEET 2\\PNUMBER REQUIRED:  {string.Format("{0:#,0}", aStiff.Quantity)}\\P{textForTrays}";

                var noteInsertionPoint = fitRec.BottomRight.Moved(-(0.14 * fitRec.Width * paperscale), 0.2 * paperscale);

                Image.Draw.aText(noteInsertionPoint, notes, aTextHeight: 0.1875 * paperscale);
            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
            finally
            {
                Status = "";
                DrawinginProgress = false;
            }
        }


        private void DDWG_Beam()
        {
            dxfVector v2 = dxfVector.Zero;
            dxfVector v3 = dxfVector.Zero;

            dxfRectangle fitRec = null;

            try
            {

                // var mdTrayAssembly = (mdTrayAssembly)((mdTrayRange)this.Drawing.Part).Assembly;
                var beam = (mdBeam)Drawing.Part;

                dxoDrawingTool draw = Image.Draw;
                dxoDimTool dims = Image.DimTool;
                dxoLeaderTool leaders = Image.LeaderTool;

                string lname = "GEOMETRY";
                Image.LinetypeLayers.Setting = dxxLinetypeLayerFlag.ForceToLayer;

                Image.Layers.Add(lname);
                // Image.Layers.Add("IBeam", dxxColors.z_110);
                //Image.Layers.Add("Dimension", dxxColors.Yellow, aLineWeight: dxxLineWeights.LW_030);
                //Image.Layers.Add("CL", dxxColors.Red, aLineWeight: dxxLineWeights.LW_015);

                double drawingAreaWidth = 1.2 * beam.Length;
                double drawingAreaHeight = 8 * beam.Height;
                dxfRectangle aRec = new(drawingAreaWidth, drawingAreaHeight);

                //=================== BORDER =====================================
                double paperscale = 0;
                Tool.Border(this, aRec, ref fitRec, ref paperscale, 0, 0);
                xCancelCheck();
                //=================== BORDER =====================================

                Image.SelectionSetInit(false);


                colDXFEntities ents1 = Draw_BeamDetail_PlanView(fitRec, beam, paperscale, draw, dims, out dxfVector v0);
                dxfRectangle bounds1 = ents1.BoundingRectangle();
                if (DrawlayoutRectangles)
                    draw.aRectangle(bounds1, aColor: dxxColors.LightGrey);


                colDXFEntities ents2 = Draw_BeamDetail_ElevationView(new dxfVector(v0.X, bounds1.Bottom - 0.5 * paperscale), fitRec, beam, paperscale, draw, dims, leaders);
                dxfRectangle bounds2 = ents2.BoundingRectangle();
                if (bounds2.Top > bounds1.Bottom)
                {
                    double d1 = bounds2.Top - bounds1.Bottom;
                    ents2.Move(0, -d1);
                    bounds2.Move(0, -d1);
                }
                if (DrawlayoutRectangles)
                    draw.aRectangle(bounds2, aColor: dxxColors.LightGrey);



                dxfVector iPt = new dxfVector(fitRec.Left + (1.5 * paperscale) + (0.5 * beam.Width), fitRec.Bottom - 0.75 * paperscale);

                colDXFEntities ents3 = Draw_BeamDetail_ProfileView(iPt, fitRec, beam, paperscale, draw, dims, leaders);
                dxfRectangle bounds3 = ents3.BoundingRectangle();
                dxfVector trans = (bounds2.BottomLeft - bounds3.TopLeft).Moved(0, -0.375 * paperscale);
                ents3.Translate(trans);
                bounds3.Translate(trans);
                if (DrawlayoutRectangles)
                    draw.aRectangle(bounds3, aColor: dxxColors.LightGrey);


                iPt = new dxfVector(bounds3.Right + (0.5 * paperscale), bounds3.Top);

                colDXFEntities ents4 = Draw_Beam_Text(iPt, fitRec, beam, paperscale, draw);
                dxfRectangle bounds4 = ents4.BoundingRectangle();

            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
            finally
            {
                Status = "";
                DrawinginProgress = false;
            }
        }

        private void Draw_CrossBraces()
        {
            //uppViews aView, bool aDrawBothSides = true, bool aDrawProfiles = true
            throw new NotImplementedException();
            //if (!Assy.DesignOptions.CrossBraces) return;

            //dxePolygon PGon;

            //mdCrossBrace aCB;
            // string lname;

            //try
            //{


            //    Status = "Drawing Cross Braces";

            //    lname = !DrawPhantom? $"{LayerPre}CROSS_BRACES" : dxfLinetypes.Phantom;
            //    Image.Layers.Add(lname);
            //    Tool.CurrentLayer = lname;
            //    switch (aView)
            //    {
            //        case uppViews.LayoutPlan:
            //        case uppViews.InstallationPlan:
            //            {
            //                aCB = Assy.CrossBrace;
            //                if (aCB == null) return;

            //                dxfVector v1  = aCB.CenterDXF;
            //                colDXFVectors ips = new();
            //                ips.AddInsertionPt(v1.X, v1.Y, aRotation: 0);
            //                ips.AddInsertionPt(-v1.X, -v1.Y, aRotation: 180);
            //                PGon = aCB.View_Plan(Assy);
            //                PGon.Move(-v1.X, -v1.Y);
            //                PGon.LCLSet(lname, dxxColors.ByLayer, dxfLinetypes.Hidden);
            //                PGon.BlockName = "CROSSBRACE_PLAN";
            //                Image.Draw.aInserts(PGon, ips);
            //                PGon.Dispose();

            //                break;
            //            }

            //        case uppViews.LayoutElevation:
            //        case uppViews.InstallationElevation:
            //            {

            //                double halfSpace = MDRange.RingSpacing / 2;
            //                for (int i = 1; i <= 2; i++)
            //                {
            //                    aCB = Assy.CrossBrace;
            //                    if (aCB != null)
            //                    {
            //                        if (i == 1)
            //                        {
            //                            PGon = aCB.View_Elevation(Assy, dxfVector.Zero);
            //                            colDXFVectors ips = new();
            //                            ips.AddInsertionPt(aCB.X, aCB.Z - halfSpace, aRotation: 0);
            //                            Image.Draw.aInsert(PGon, new dxfVector(aCB.X, aCB.Z - halfSpace));
            //                            PGon.Dispose();

            //                        }
            //                        else
            //                        {
            //                            if (!aDrawProfiles) return;

            //                            PGon = aCB.View_Profile(Assy, true, false, Image.CreateVector(aCB.Y, aCB.Z + halfSpace));
            //                            PGon.BlockName = "CROSSBRACE_ELEV";
            //                            PGon.Instances.Add(2 * aCB.Y, 0, bLeftHanded: true);
            //                            colDXFVectors ips = new();
            //                            ips.AddInsertionPt(aCB.X, aCB.Z - halfSpace, aRotation: 0);
            //                            Image.Draw.aInserts(PGon, ips);
            //                            PGon.Dispose();

            //                        }

            //                        // draw the perimeter
            //                        PGon.LCLSet(lname);
            //                        Image.LinetypeLayers.ApplyTo(PGon, dxxLinetypeLayerFlag.ForceToLayer);
            //                        Image.Entities.Add(PGon);
            //                    }

            //                    xCancelCheck();
            //                }
            //                break;
            //            }

            //        default:
            //            break;
            //    }


            //}
            //catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e ); }
            //finally { Status = StatusWas; xCancelCheck(); }

        }

        private List<dxeInsert> Draw_DeckSections(uppViews aView, bool bDrawBothSides = true, bool bSuppressBubblePromoters = false, bool bRegenPerims = false, string sSplcPnls = "", bool bSuppressSlots = false, bool bShowPns = false, int aViewIndex = 1, List<mdDeckSection > aSections = null, bool bShowQuantities = false)
        {
            // #1the view to draw
            // ^used by higher level drawing functions to draw the deck panels in the tray assembly
            // " , "the view argument controls how the panel is drawn.

            List<dxeInsert> _rVal = new();
            dxfVector v1;
            dxfRectangle aRec;
            string lname = !DrawPhantom ? $"{LayerPre}DECK" : dxfLinetypes.Phantom;
            string bname;
         
            double? rot = null;

            if (!Assy.IsSymmetric) bDrawBothSides = true;
            //bool regen = DrawlayoutRectangles;
            try
            {
                if (!Assy.HasAlternateDeckParts && aViewIndex > 2) aViewIndex = 2;
                Status = "Drawing Deck Sections";
                dxfDisplaySettings dsp = dxfDisplaySettings.Null(aLayer: lname);
                dxoDrawingTool draw = Image.Draw;
                dxeInsert insert = null;
                List<mdDeckSection> sections = aSections == null ? aViewIndex == 3 ? MDProject.GetParts().AltDeckSections(Assy.RangeGUID) : MDProject.GetParts().DeckSections(Assy.RangeGUID) : aSections;
                switch (aView)
                {
                    case uppViews.InstallationPlan:
                        {
                            //dxeLine zaxis = new dxeLine(dxfVector.Zero, new dxfVector(0, 0, 100));
                            //dxeLine yaxis = new dxeLine(dxfVector.Zero, new dxfVector(0, 100));
                            double paperscale = Image.Display.PaperScale;

                            if (!Assy.HasAlternateDeckParts && aViewIndex > 2) aViewIndex = 2;
                            rot = 0;
                            if (aViewIndex > 2)
                            {
                                
                                rot = 90;
                            }

                            bool bBPs = Assy.DesignOptions.HasBubblePromoters;
                            bool nobps = false;

                
                            bool bSlots = false;
                            if (bSlots)
                            {
                                //bSlots = aView != uppViews.Plan;
                                if (bSlots) Image.Layers.Add("SLOTS", dxxColors.LightGrey);
                            }


                            foreach (mdDeckSection section in sections)
                            {
                                bname = (!section.IsManway) ? $"DECK_SECTION_{section.PartNumber}_PLAN" : $"MANWAY_{section.PartNumber}_PLAN";

                                if (bBPs && aViewIndex > 1 && aView == uppViews.InstallationPlan)
                                {
                                    nobps = true;
                                    bname = $"{bname}_NO_PROMOTERS";
                                }

                                if (aViewIndex > 1) bname += $"_{aViewIndex}";


                                dxfBlock block = null;


                                if (!Image.Blocks.TryGet(bname, ref block))
                                {
                                    block = mdBlocks.DeckSection_View_Plan(Image, section, Assy, bname, bSetInstances: bDrawBothSides, bObscured: true, bIncludeHoles: aView != uppViews.Plan, bIncludePromoters: !nobps, bIncludeSlotting: bSlots, bRegeneratePerimeter: bRegenPerims, aLayerName: lname, bSolidHoles: aView == uppViews.InstallationPlan && aViewIndex > 1, bShowPns: bShowPns, bShowQuantities : bShowQuantities);
                                    
                                }
                                Status = $"Inserting Deck Section Block {block.Name}";
                                if (aViewIndex == 3)
                                {
                                    
                                    uopVectors ips = section.Instances.MemberPoints(section.Center, true);
                                    foreach (var ip in ips) _rVal.Add(draw.aInsert(block.Name, ip, ip.Rotation + 90, aDisplaySettings: dsp, aTag: section.PartNumber));
                                }
                                else
                                {
                                    _rVal.AddRange(draw.aInserts(block, null, bOverrideExisting: false, aDisplaySettings: dsp, aTag: section.PartNumber));
                                }
                                
                            }


                            break;
                        }

                    case uppViews.LayoutPlan:
                    case uppViews.Plan:
                        {

                            double paperscale = Image.Display.PaperScale;
                            if (!Assy.HasAlternateDeckParts && aViewIndex > 1) aViewIndex = 1;
                            if (aViewIndex > 1)
                            {
                    
                                rot = 90;
                            }
                            bool bSlots = Assy.DesignFamily.IsEcmdDesignFamily() && !bSuppressSlots;
                            if (bSlots)
                            {
                                bSlots = aView != uppViews.Plan;
                                if (bSlots) Image.Layers.Add("ECMD SLOTS", dxxColors.LightGrey);
                            }


                            bool bBPs = Assy.DesignOptions.HasBubblePromoters;
                           // if (regen) Assy.Invalidate(uppPartTypes.DeckSection);

                          
                            double minx = 0;
                            if (!bDrawBothSides && Assy.OddDowncomers) minx = -0.5 * (Assy.Downcomer().Width + 2 * Assy.Downcomer().Thickness + 0.25) - 0.5 * Assy.FunctionalPanelWidth;


                            foreach (mdDeckSection section in sections)
                            {

                                // if (section.IsVirtual || !section.InstalledOnAlternateRing1) continue;
                                bname = (!section.IsManway) ? $"DECK_SECTION_{section.PartNumber}_PLAN" : $"MANWAY_{section.PartNumber}_PLAN";
                                if (aViewIndex > 1) bname += $"_{aViewIndex}";
                                Status = $"Creating Deck Section Block {bname}";

                                dxfBlock block = null;
                                //colDXFVectors ips = null;

                                if (!Image.Blocks.TryGet(bname, ref block))
                                {
                                    block = mdBlocks.DeckSection_View_Plan(Image, section, Assy, bname, bSetInstances: bDrawBothSides, bObscured: true, bIncludeHoles: aView != uppViews.Plan, bIncludePromoters: !bSuppressBubblePromoters, bIncludeSlotting: bSlots, bRegeneratePerimeter: bRegenPerims, aLayerName: lname, bSolidHoles: false, bShowPns: bShowPns, bShowQuantities: bShowQuantities);
                                    
                                }

                                if((rot.HasValue &&rot.Value != 0) && block.Instances.Count > 0)
                                {
                                     block.Instances.UpdateMembers(aRotationAdder : rot);
                                    
                                }
                                Status = $"Inserting Deck Section Block {block.Name}";
                              
                                List<dxeInsert> inserts = draw.aInserts(block, block.Instances, bOverrideExisting:false, aRotationAngle: rot.HasValue ? rot.Value : 0, aDisplaySettings:dsp, aLTLSetting: dxxLinetypeLayerFlag.ForceToColor, aTag: section.PartNumber);
                    

                            }
                            break;
                        }

                    case uppViews.LayoutElevation:
                    case uppViews.InstallationElevation:
                        {
                            colDXFVectors bPts = null;
                            if (Assy.DesignOptions.HasBubblePromoters)
                            {
                                bPts = Assy.DeckSections.BPCenters(Assy.IsStandardDesign);
                                List<double> XOrds = bPts.GetOrdinates(dxxOrdinateDescriptors.X, bUniqueValues: true, aPrecision: 3);

                                colDXFVectors points = new();
                                for (int i = 0; i < XOrds.Count; i++)
                                {
                                    points.Add(bPts.GetAtCoordinate(XOrds[i], aPrecis: 3).FirstOrDefault());
                                }
                                bPts = points;
                            }

                            mdDeckSections DSs = Assy.DeckSections;
                            uppSpliceStyles sstyle = Assy.DesignOptions.SpliceStyle;
                            bool bFlngs = sstyle != uppSpliceStyles.Angle;
                            dsp = dxfDisplaySettings.Null(lname);
                            double halfSpace = MDRange.RingSpacing / 2;
                            for (int i = 1; i <= 2; i++)
                            {
                                if (i == 1)
                                {
                                    mdDeckSections bDSs = DSs.GetByPanelIndex(Assy.DesignFamily.IsBeamDesignFamily() ? Assy.DeckPanels.Count / 2 : Assy.DeckPanels.Count);

                                    for (int j = 1; j <= bDSs.Count; j++)
                                    {
                                        mdDeckSection section = bDSs.Item(j);
                                        v1 = new dxfVector(-section.BoundingRectangle().Y, section.Z + halfSpace);
                                        dxePolygon PGon = section.View_Profile(Assy, true, true, false, false, dxfVector.Zero, aLayerName: lname);
                                        bname = (!section.IsManway) ? $"DECK_SECTION_{section.PartNumber}_PROFILE" : $"MANWAY_{section.PartNumber}_PROFILE";

                                        insert = draw.aInsert(PGon, v1, 0, bname, aDisplaySettings: dsp, aLTLSetting: dxxLinetypeLayerFlag.ForceToLayer);
                                        _rVal.Add(insert);

                                        if (Assy.DesignFamily.IsBeamDesignFamily())
                                        {
                                            bname += "_MIRROR";
                                            PGon.Mirror(new dxeLine(new dxfVector(0, -100), new dxfVector(0, 100)));
                                            v1.X = -v1.X;
                                            insert = draw.aInsert(PGon, v1, 0, bname, aDisplaySettings: dsp, aLTLSetting: dxxLinetypeLayerFlag.ForceToLayer);
                                            _rVal.Add(insert);
                                        }

                                        PGon.Dispose();

                                        xCancelCheck();
                                    }
                                }
                                else
                                {
                                    bool bFlngDrawn = false;
                                    mdDeckSections bDSs;

                                    int cnt = Assy.DeckPanels.GetByVirtual(aVirtualValue: false).Count;
                                    for (int pid = 1; pid <= cnt; pid++)
                                    {
                                        bDSs = DSs.GetByPanelIndex(pid);
                                        bDSs.SortByOrdinate(dxxOrdinateDescriptors.Y);
                                        mdDeckSections cDSs = new mdDeckSections();
                                        foreach (var deckSection in bDSs)
                                        {
                                            if ((deckSection.Top + 1 >= ElevationX && deckSection.Bottom - 1 <= ElevationX) || ElevationX >= deckSection.Top + 1)
                                            {
                                                cDSs.Add(deckSection);
                                                break;
                                            }
                                        }


                                        for (int j = 1; j <= cDSs.Count; j++)
                                        {
                                            mdDeckSection section = cDSs.Item(j);
                                            if (section.IsVirtual) continue;

                                            aRec = section.BoundingRectangle();
                                            v1 = new dxfVector(section.X, -halfSpace);

                                            dxePolygon PGon = section.View_Elevation(Assy, ref bFlngDrawn, false, true, false, dxfVector.Zero, aLayerName: lname);
                                            bname = (!section.IsManway) ? $"DECK_SECTION_{section.PartNumber}_ELEV" : $"MANWAY_{section.PartNumber}_ELEV";
                                            insert = draw.aInsert(PGon, v1, 0, bname, aDisplaySettings: dsp, aLTLSetting: dxxLinetypeLayerFlag.ForceToLayer);
                                            _rVal.Add(insert);

                                            if (Assy.DesignFamily.IsBeamDesignFamily())
                                            {
                                                v1.X = -v1.X;
                                                insert = draw.aInsert(PGon, v1, 0, bname, aDisplaySettings: dsp, aLTLSetting: dxxLinetypeLayerFlag.ForceToLayer);
                                                _rVal.Add(insert); 
                                            }


                                            if (Math.Round(section.X, 1) > 0 && Assy.IsSymmetric)
                                            {
                                                v1 = new dxfVector(-section.X, -halfSpace);
                                                insert = draw.aInsert(insert.BlockName, v1, 0, aDisplaySettings: insert.DisplaySettings);
                                                _rVal.Add(insert);
                                            }
                                            if (pid > 1)
                                            {
                                                break;
                                            }
                                            xCancelCheck();
                                        }
                                        xCancelCheck();
                                        if (pid > 1 && !bFlngDrawn && bFlngs)
                                        {
                                            if (!mzUtils.ListContains(pid, sSplcPnls))
                                            {
                                                for (int j = 1; j <= cDSs.Count; j++)
                                                {
                                                    mdDeckSection section = cDSs.Item(j);
                                                    if (section.HasJoggleAngle)
                                                    {
                                                        dxePolygon PGon = section.View_Elevation(Assy, ref bFlngDrawn, false, true, false, Image.CreateVector(section.X, section.Z - halfSpace), aLayerName: lname);
                                                        bname = (!section.IsManway) ? $"DECK_SECTION_{section.PartNumber}_ELEV" : $"MANWAY_{section.PartNumber}_ELEV";

                                                        dxePolyline aPl = (dxePolyline)PGon.AdditionalSegments.GetTagged("JOGGLE ANGLE");
                                                        if (aPl != null)
                                                        {
                                                            if (Math.Round(section.X, 1) > 0)
                                                            {
                                                                aPl.Instances.Add(-2 * section.X, 0, bLeftHanded: true);
                                                            }

                                                            Image.Entities.Add(aPl);
                                                        }
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        xCancelCheck();
                                    }
                                }
                            }
                            xCancelCheck();
                            if (bPts != null)
                            {
                                dxeArc aAr = new(dxfVector.Zero, 1.8756, 47.174, 132.823) { LayerName = lname };

                                aAr.Y = Image.Y - halfSpace - (1.3756 - Assy.Deck.Thickness);
                                for (int j = 1; j <= bPts.Count; j++)
                                {
                                    v1 = bPts.Item(j);
                                    aAr.X = Image.X + v1.X;
                                    Image.Entities.Add(aAr, bAddClone: true);
                                }
                            }

                            break;
                        }

                    case uppViews.FunctionalDesign:
                        {
                            bool slots = Assy.TotalSlotCount > 0;
                            Image.Layers.Add(lname, (dxxColors)41);
                            List<mdDeckSection> DSs = MDProject.GetParts().DeckSections(Assy.RangeGUID).FindAll((x) => x.InstalledOnAlternateRing1);
                            dsp = dxfDisplaySettings.Null(lname);
                            foreach(mdDeckSection section in DSs)
                            {
                                bname = (!section.IsManway) ? $"DECK_SECTION_{section.PartNumber}" : $"MANWAY_{section.PartNumber}";
                                Status = $"Creating Deck Section Block {bname}";
                                dxfBlock block = Image.Blocks.Add(mdBlocks.DeckSection_View_Plan(Image, section, Assy, bname,bSetInstances: bDrawBothSides, bObscured: true, bIncludeHoles: aView != uppViews.Plan, bIncludePromoters: !bSuppressBubblePromoters, bIncludeSlotting: false, bRegeneratePerimeter: bRegenPerims, aLayerName: lname, bSolidHoles: false));
                                bname = block.Name;
                                Status = $"Inserting Deck Section Block {bname}";
                                List<dxeInsert> inserts =  draw.aInserts(block, block.Instances, bOverrideExisting:false, aDisplaySettings: dsp , aTag: section.PartNumber);
                                _rVal.AddRange(inserts);

                            }
                            xCancelCheck();


                            break;
                        }

                    default:
                        break;
                }



            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
            finally
            {

              
                StatusWas = "";

            }
            return _rVal;
        }

        private void Draw_ECMDSlots(bool bForFunctional = false, bool bSuppressPoints = false)
        {
            if (Assy == null) return;
            if (!Assy.DesignFamily.IsEcmdDesignFamily()) return;

            string statwuz = Status;
            try
            {
                Status = "Drawing ECMD Slots";
                mdSlotZones aZns = Assy.SlotZones;
                uopVectors points = aZns.GridPoints(Assy, bBothSides: !bForFunctional, bRegen: false);
                dxoDrawingTool draw = Image.Draw;
                if (points == null) return;
                if (points.Count <= 0) return;

                dxfDisplaySettings dsp = dxfDisplaySettings.Null("SLOTS", dxxColors.ByLayer, dxfLinetypes.Continuous);
                Image.Layers.Add(dsp.LayerName, dxxColors.LightGrey);

                bool rotated = bForFunctional && !Assy.DesignFamily.IsDividedWallDesignFamily();
                dxfBlock aBlk = null;
                string bname = "ECMDSLOT";
                //rotated = false;
                if(!Image.Blocks.TryGet(bname, ref aBlk))
                {
                    colDXFEntities slotents = new(Assy.FlowSlot.FootPrint(bSuppressAngle: true, bZeroCenter: true, bArrowStyle: false, aDisplaySettings: dsp, bInverted: false), new dxePoint(dxfVector.Zero, dsp) { LayerName = "DefPoints" });
                    aBlk = Image.Blocks.Add(new("ECMDSLOT", aDomain: dxxDrawingDomains.Model, aEntities: slotents) { LayerName = dsp.LayerName });
                }
                if (bSuppressPoints)
                    aBlk.Entities.RemoveByGraphicType(dxxGraphicTypes.Point);
                   
   
                uopVector u1 = points[0];
                dxfVector v1 = new dxfVector(u1.X, u1.Y) { Rotation = u1.Rotation };

                dxeInsert slotinsert = new dxeInsert(aBlk, v1) { RotationAngle = v1.Rotation };
                //dxoInstances insts = slotinsert.Instances;
                slotinsert.Instances.DefineWithVectors(points, v1, false);
                dxfBlock block = new dxfBlock( $"{Assy.TrayName().Replace("-","_").ToUpper()}_SLOTS");
                block.Entities.Add(slotinsert);
                block = Image.Blocks.Add(block);

                Image.Draw.aInsert(block.Name ,uopVector.Zero,!rotated ?0 :180, aDisplaySettings:dsp );

            }

            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
            finally
            {
                Status = statwuz;
            }

        }
        private List<dxeInsert> Draw_Downcomers(uppViews aView, out colDXFEntities rDCCenterlines, bool bDrawSpouts = true, bool bDrawBothSides = true, bool bSuppressHoles = false, string aLayer = "", int aViewIndex = 1, bool bDrawStartupCenterLines = false, bool bDrawFingerClips = false, bool bDrawStiffeners = true, bool bDrawEndAngles = false, bool bDrawBaffles = false, bool bSuppressEndSupports = false,  bool bSuppressShelves = false, bool bSuppressEndplates = false, bool bSuppresSupDefs = false)
        {
            // the argument order has been changed as rDCCenterlines need to be out parameter
            rDCCenterlines = null;
            List<dxeInsert> _rVal = new List<dxeInsert>();
            // #1the view to draw
            // ^used by higher level drawing functions to draw the downcomer boxes in the tray assembly
            // " , "the view argument controls how the downcomer box is drawn.
            bool stddesign = Assy.DesignFamily.IsStandardDesignFamily();

            List<mdDowncomer> DComers = Assy.Downcomers.GetByVirtual(aVirtualValue: !stddesign ? null : false);
            dxfRectangle aRec;
            dxeLine cLine;
            dxePolygon PGon = null;
            dxeInsert aIns;
            dxoDrawingTool draw = Image.Draw;
            bool inc_endplates = !bSuppressEndplates;
            bool inc_endsupports = !bSuppressEndSupports;
            bool inc_shelves = !bSuppressShelves;
            bool inc_Stiffeners = bDrawStiffeners && Assy.ProjectType != uppProjectTypes.MDSpout;
            bool inc_FingerClips = bDrawFingerClips && Assy.ProjectType != uppProjectTypes.MDSpout;
            bool inc_EndAngles = bDrawEndAngles && Assy.ProjectType != uppProjectTypes.MDSpout;
            bool inc_Baffles= bDrawBaffles && Assy.ProjectType != uppProjectTypes.MDSpout;
            bool inc_SupDefs = !bSuppresSupDefs && Assy.ProjectType != uppProjectTypes.MDSpout;

            try
            {
                xCancelCheck();


                double paperscale = Image.Display.PaperScale;
                double clen = bDrawStartupCenterLines ? 0.75 : 0;

                Status = "Drawing Downcomers";
                string lname = string.IsNullOrWhiteSpace(aLayer) ? $"{LayerPre}DOWNCOMERS" : aLayer;

              
                if (DrawPhantom) lname = dxfLinetypes.Phantom;
                dxfDisplaySettings dsp = dxfDisplaySettings.Null(lname);
                switch (aView)
                {
                    case uppViews.InstallationPlan:
                        {
                            //if (aViewIndex == 3)
                            //    Image.UCS.Rotate(90);
                            inc_FingerClips = aViewIndex == 1;
                            bDrawBothSides = true;
                            rDCCenterlines = new colDXFEntities();
                            double rot = aViewIndex == 3 ? 90 : 0;

                            string suffix = aViewIndex > 1 ? "_BOX" : null;
                            foreach (mdDowncomer dc in DComers)
                            {
                                Status = $"Generating DC {dc.Index} Plan View";
                                xCancelCheck();
                                dxfBlock dcblock = mdBlocks.Downcomer_View_Plan(aImage: Image, dc, Assy, bSetInstances: true, bShowObscured: true, bSuppressHoles: false, bIncludeSpouts: false, aCenterLineLength: clen, aLayerName: lname, bIncludeEndPlates: inc_endplates, bIncludeEndSupports: inc_endsupports, bIncludeShelfAngles: inc_shelves, bIncludeStiffeners: inc_Stiffeners, bIncludeBaffles: inc_Baffles, bIncludeSupDefs: inc_SupDefs, bIncludeFingerClips: inc_FingerClips, bIncludeEndAngles: inc_EndAngles, aBlockNameSuffix: suffix);
                                if (aViewIndex == 3)
                                {

                                    uopVectors ips = dc.Instances.MemberPoints(dc.Center, true);
                                    foreach (var ip in ips)
                                        _rVal.Add(draw.aInsert(dcblock.Name, ip, ip.Rotation + 90, aDisplaySettings: dsp, aTag: dc.PartNumber));
                                }
                                else
                                {
                                    _rVal.AddRange(draw.aInserts(dcblock, null, bOverrideExisting: false, aDisplaySettings: dsp, aTag: dc.PartNumber));
                                }


                            }

                            break;

                        }

                    case uppViews.Plan:
                    case uppViews.LayoutPlan:
                    case uppViews.DesignView:
                        {
                            rDCCenterlines = new colDXFEntities();


                            if (aView == uppViews.FunctionalDesign) bDrawSpouts = true;
                            bool obscured = aView != uppViews.DesignView;

                            foreach (mdDowncomer dc in DComers)
                            {
                                List<mdDowncomerBox> boxes = dc.Boxes.FindAll((x) => !x.IsVirtual);
                                if (boxes.Count <= 0) continue;
                                foreach (var box in boxes)
                                {
                                    Status = $"Generating DC {dc.Index} Plan View";
                                    dxfBlock dcblock = mdBlocks.DowncomerBox_View_Plan(aImage: Image, box, Assy, bSetInstances: bDrawBothSides, bShowObscured: obscured, bSuppressHoles: bSuppressHoles, bIncludeSpouts: bDrawSpouts, aCenterLineLength: clen, aLayerName: lname, bIncludeEndPlates: inc_endplates, bIncludeEndSupports: inc_endsupports, bIncludeShelfAngles: inc_shelves, bIncludeStiffeners: inc_Stiffeners, bIncludeBaffles: inc_Baffles, bIncludeSupDefs: inc_SupDefs, bIncludeFingerClips: inc_FingerClips, bIncludeEndAngles: inc_EndAngles);
                                    Status = $"Drawing DC {dc.Index} Plan View";
                                    _rVal.AddRange(draw.aInserts(dcblock, dcblock.Instances, bOverrideExisting: false));
                                }


                                //    Status = $"Generating DC {dc.Index} Plan View";
                                //dxfBlock dcblock = mdBlocks.Downcomer_View_Plan(aImage: Image, dc, Assy, bSetInstances: bDrawBothSides, bShowObscured: obscured, bSuppressHoles: bSuppressHoles, bIncludeSpouts: bDrawSpouts, aCenterLineLength: clen, aLayerName: lname, bIncludeEndPlates: inc_endplates, bIncludeEndSupports: inc_endsupports, bIncludeShelfAngles: inc_shelves, bIncludeStiffeners: inc_Stiffeners, bIncludeBaffles: inc_Baffles, bIncludeSupDefs: inc_SupDefs, bIncludeFingerClips: inc_FingerClips, bIncludeEndAngles: inc_EndAngles);
                                //Status = $"Drawing DC {dc.Index} Plan View";



                                //_rVal.AddRange(draw.aInserts(dcblock, dcblock.Instances));

                                Status = "Drawing Downcomer Centerlines";

                                if (Math.Round(dc.X, 1) != 0)
                                {
                                    rDCCenterlines.Add(draw.aLine(dc.X, Math.Sqrt(Math.Pow(1.05 * ShellRad, 2) - Math.Pow(dc.X, 2)), dc.X, -Math.Sqrt(Math.Pow(1.05 * ShellRad, 2) - Math.Pow(dc.X, 2)), aLineType: dxfLinetypes.Center));
                                    if (bDrawBothSides)
                                    {
                                        rDCCenterlines.Add(draw.aLine(-dc.X, Math.Sqrt(Math.Pow(1.05 * ShellRad, 2) - Math.Pow(dc.X, 2)), -dc.X, -Math.Sqrt(Math.Pow(1.05 * ShellRad, 2) - Math.Pow(dc.X, 2)), aLineType: dxfLinetypes.Center));
                                    }
                                }
                                xCancelCheck();
                            }
                            break;
                        }
                    case uppViews.FunctionalDesign:
                        {
                            rDCCenterlines = new colDXFEntities();

                            List<mdDowncomerBox> realboxes = Assy.Downcomers.Boxes();
                            bDrawSpouts = true;
                            bool obscured = true;


                            foreach (var box in realboxes)
                            {
                                Status = $"Generating DC Box {box.DowncomerIndex} Plan View";
                                dxfBlock block_withspouts = mdBlocks.DowncomerBox_View_Plan(aImage: Image, box, Assy, bSetInstances: false, bShowObscured: obscured, bSuppressHoles: bSuppressHoles, bIncludeSpouts: bDrawSpouts, aCenterLineLength: clen, aLayerName: lname, bIncludeEndPlates: inc_endplates, bIncludeEndSupports: inc_endsupports, bIncludeShelfAngles: inc_shelves, bIncludeStiffeners: inc_Stiffeners, bIncludeBaffles: inc_Baffles, bIncludeSupDefs: inc_SupDefs, bIncludeFingerClips: inc_FingerClips, bIncludeEndAngles: inc_EndAngles);
                                Status = $"Drawing DC {box.DowncomerIndex} Plan View";
                                string bname = block_withspouts.Name;
                                block_withspouts.Name += "_WITH_SPOUTS";
                                block_withspouts = Image.Blocks.Add(block_withspouts);
                                dxfVector v1 = new dxfVector(box.X, box.Y) { Rotation = 0 };
                                dxfVector v2 = new dxfVector(-box.X, -box.Y) { Rotation = 180 };
                                block_withspouts.Instances.Clear();

                                dxfBlock block_withoutspouts = null;
                                if (box.OccuranceFactor > 1)
                                {
                                    block_withoutspouts = new dxfBlock(block_withspouts) { Name = bname + "_NO_SPOUTS" };

                                    block_withoutspouts.Entities.RemoveByTag("SPOUT GROUP", null);

                                    block_withspouts.Instances.Clear();

                                    block_withoutspouts = Image.Blocks.Add(block_withoutspouts);
                                    //dxfVector insertionPoint = new dxfVector(dc.X + leftSideInstance.XOffset, dc.Y + leftSideInstance.YOffset);
                                    //block_withspouts.Instances.BasePlane = new dxfPlane(insertionPoint);
                                }
                                //if (Assy.DesignFamily.IsStandardDesignFamily())
                                //{
                                //    dxfVector v3 = new dxfVector(v1);
                                //    v1 = new dxfVector(v2);
                                //    v2 = new dxfVector(v3);
                                //}
                                _rVal.Add(draw.aInsert(block_withspouts.Name, v1, aRotationAngle: v1.Rotation));
                                if (block_withoutspouts != null) _rVal.Add(draw.aInsert(block_withoutspouts.Name, v2, aRotationAngle: v2.Rotation));
                                xCancelCheck();




                            }

                            Status = "Drawing Downcomer Centerlines";
                            List<double> xVals = Assy.DowncomerData.XValues_Downcomers;
                            foreach (var xval in xVals)
                            {
                                rDCCenterlines.Add(draw.aLine(xval, Math.Sqrt(Math.Pow(1.05 * ShellRad, 2) - Math.Pow(xval, 2)), xval, -Math.Sqrt(Math.Pow(1.05 * ShellRad, 2) - Math.Pow(xval, 2)), aLineType: dxfLinetypes.Center));
                            }
                        }
                        break;


                    case uppViews.LayoutElevation:
                    case uppViews.InstallationElevation:
                        {
                            rDCCenterlines = new colDXFEntities();
                            cLine = new dxeLine();

                            cLine.LCLSet(Image.LinetypeLayers.LineLayer(aLinetypeName: dxfLinetypes.Center), aLineType: dxfLinetypes.Center);
                            Image.LinetypeLayers.ApplyTo(cLine, dxxLinetypeLayerFlag.ForceToColor, aImage: _Image);
                            double halfSpace = MDRange.RingSpacing / 2;
                            string aBoltOnList = inc_EndAngles ? "END ANGLES" : string.Empty;
                            if(bDrawStiffeners)aBoltOnList += aBoltOnList = !string.IsNullOrWhiteSpace(aBoltOnList) ? $"{aBoltOnList},STIFFENERS" : $"STIFFENERS";
                            if (inc_FingerClips) aBoltOnList = !string.IsNullOrWhiteSpace(aBoltOnList) ? $"{aBoltOnList},FINGER CLIPS" : $"FINGER CLIPS";
                            if (inc_Baffles) aBoltOnList = !string.IsNullOrWhiteSpace(aBoltOnList) ? $"{aBoltOnList},BAFFLES" : $"BAFFLES";
                            if (inc_Baffles && !bDrawStiffeners) aBoltOnList = !string.IsNullOrWhiteSpace(aBoltOnList) ? $"{aBoltOnList},STIFFENERS" : $"STIFFENERS";
                            if (inc_EndAngles) aBoltOnList = !string.IsNullOrWhiteSpace(aBoltOnList) ? $"{aBoltOnList},END ANGLES" : $"END ANGLES";
                   
                            for (int i = 1; i <= 2; i++)
                            {
                                if (i == 1)
                                {


                                    for (int j = 1; j <= DComers.Count; j++)
                                    {
                                        mdDowncomer aDC = DComers[j - 1];

                                        mdDowncomerBox box = aDC.Boxes.FindAll((x) => !x.IsVirtual).FirstOrDefault();

                                        if (box == null) continue;
                                        Status = $"Generating DC {i} Elevation View";
                                        PGon = aDC.View_Elevation(Assy, true, !string.IsNullOrWhiteSpace(aBoltOnList), new dxfVector(), 0, aBoltOnList, lname);
                                        PGon.BlockName = $"DC_{aDC.PartNumber}_ELEVATION";
                                        if (DrawPhantom)
                                        {
                                            PGon.AdditionalSegments.SetDisplayVariable(dxxDisplayProperties.Linetype, dxfLinetypes.Phantom);
                                            PGon.AdditionalSegments.SetDisplayVariable(dxxDisplayProperties.LayerName, dxfLinetypes.Phantom);

                                        }

                                        Status = $"Drawing DC {i} Elevation View";
                                        aIns = draw.aInsert(PGon, new dxfVector(aDC.X, -halfSpace), aRotationAngle: 0, aLTLSetting: dxxLinetypeLayerFlag.ForceToColor, aDisplaySettings: PGon.DisplaySettings);

                                        aRec = aIns.BoundingRectangle();
                                        cLine.SetCoordinates2D(aIns.X, aRec.Top + 0.125 * paperscale, aIns.X, aRec.Bottom - 0.125 * paperscale);
                                        rDCCenterlines.Add(Image.Entities.Add(cLine, bAddClone: true, aTag: $"DC_{j}", aFlag: "RIGHT"));

                                        if (aDC.OccuranceFactor > 1)
                                        {
                                            aIns = draw.aInsert(PGon, new dxfVector(-aDC.X, -halfSpace), aRotationAngle: 0, aLTLSetting: dxxLinetypeLayerFlag.ForceToColor, aDisplaySettings: PGon.DisplaySettings);
                                            cLine.Mirror(Image.UCS.YAxis());
                                            rDCCenterlines.Add(Image.Entities.Add(cLine, bAddClone: true, aTag: $"DC_{j}", aFlag: "LEFT"));
                                        }
                                        PGon?.Dispose();
                                        xCancelCheck();
                                    }
                                }
                                else
                                {
                                    mdDowncomer aDC = Assy.Downcomers.LongestMember;
                                    
                                    Status = $"Generating DC {aDC.Index} Profile View";
                                    PGon = aDC.View_Profile(Assy, true, true, !string.IsNullOrWhiteSpace(aBoltOnList), new dxfVector(), 0, aBoltOnList, lname);
                                    if (DrawPhantom)
                                    {
                                        PGon.AdditionalSegments.SetDisplayVariable(dxxDisplayProperties.Linetype, dxfLinetypes.Phantom);
                                        PGon.AdditionalSegments.SetDisplayVariable(dxxDisplayProperties.LayerName, dxfLinetypes.Phantom);

                                    }
                                    PGon.BlockName = $"DC_{aDC.PartNumber}_PROFILE";
                                    Status = $"Drawing DC {aDC.Index} Profile View";

                                    aIns = draw.aInsert(PGon, new dxfVector(0, halfSpace), aRotationAngle: 0, aLTLSetting: dxxLinetypeLayerFlag.ForceToColor, aDisplaySettings: PGon.DisplaySettings);

                                    if (Assy.DesignFamily.IsBeamDesignFamily())
                                    {
                                        PGon.BlockName = $"DC_{aDC.PartNumber}_PROFILE_MIRROR";
                                        PGon.Mirror(new dxeLine(new dxfVector(0, -100), new dxfVector(0, 100)));
                                        aIns = draw.aInsert(PGon, new dxfVector(0, halfSpace), aRotationAngle: 0, aLTLSetting: dxxLinetypeLayerFlag.ForceToColor, aDisplaySettings: PGon.DisplaySettings);
                                    }

                                    PGon?.Dispose();
                                }
                            }
                            break;
                        }

                    default:
                        break;
                }


            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
            finally
            {
              
                Status = StatusWas;
                xCancelCheck();
            }
            return _rVal;
        }

        private void Draw_APPans(uppViews aView, bool bObscured = true, bool bDrawBothSides = true, string aLayer = "")
        {

            // #1the view to draw
            // ^used by higher level drawing functions to draw the ap pans in the tray assembly
            // " , "the view argument controls how the downcomer box is drawn.
            if (Assy == null) return;
            if (!Assy.HasAntiPenetrationPans) return;
            colUOPParts pans = Assy.GenerateAPPans(DrawlayoutRectangles);
            if (pans.Count <= 0) return;


            try
            {
                string lname = string.IsNullOrWhiteSpace(aLayer) ? $"{LayerPre}APPANS" : aLayer;

                double paperscale = Image.Display.PaperScale;
                dxoDrawingTool draw = Image.Draw;

                xCancelCheck();


                Status = "Drawing AP Pans";

                dxfDisplaySettings dsp = new(lname);
                switch (aView)
                {
                    case uppViews.Plan:
                    case uppViews.LayoutPlan:
                    case uppViews.InstallationPlan:
                    case uppViews.FunctionalDesign:
                    case uppViews.DesignView:
                        dxeInsert ins = null;
                        foreach (uopPart item in pans)
                        {
                            mdAPPan pan = (mdAPPan)item;
                            dxePolygon pgon = pan.View_Plan(bObscured, dxfVector.Zero, aLayerName: lname);

                            colDXFVectors centers = pan.Instances.MemberPointsDXF();
                            if (!bDrawBothSides)
                            {
                                List<dxfVector> otherside = centers.RemoveAll(x => x.X < Assy.Downcomers.GetByVirtual(aVirtualValue: false).Last().X);

                            }


                            for (int i = 1; i <= centers.Count; i++)
                            {
                                dxfVector ip = centers.Item(i);
                                if (i == 1)
                                {
                                    ins = draw.aInsert(pgon, new dxfVector(ip.X, ip.Y), aRotationAngle: ip.Rotation, aBlockName: $"APPAN_{pan.PartNumber}_PLAN_OBSCURED", aDisplaySettings: dsp, aLTLSetting: dxxLinetypeLayerFlag.ForceToColor);
                                }
                                else
                                {
                                    draw.aInsert(ins.BlockName, new dxfVector(ip.X, ip.Y), aRotationAngle: ip.Rotation, aDisplaySettings: dsp);
                                }
                            }

                        }

                        break;
                    case uppViews.LayoutElevation:
                    case uppViews.InstallationElevation:

                        break;
                    default:
                        break;
                }


            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
            finally
            {
                Status = StatusWas;
                xCancelCheck();
            }
        }

        /// <summary>
        /// used by higher level drawing functions to draw the downcomer boxes in the tray assembly on the ring below
        /// </summary>
        /// <param name="aView">the view to draw</param>
        /// <param name="bBothSides">flag to draw the virtual downcomer boxes</param>
        private void Draw_DowncomersBelow(uppViews aView, bool bBothSides)
        {
            try
            {
                Status = "Drawing Downcomers Below";
                dxfBlock block = mdBlocks.DowncomersBelow_View_Plan(Image, Assy, $"{LayerPre}DCS_BELOW", bBothSides: bBothSides);
                Image.Draw.aInsert(block, null);
            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
            finally { Status = StatusWas; xCancelCheck(); }
        }

        private void Draw_FunctionalNotation(double aPaperScale, dxeLine aHorCenterLn, dxeLine aVerCenterLn, uopHoles ioSUSpouts,  colDXFEntities ioDCClines, List<dxeInsert> aDeckSectionInserts)
        {
            // #1the horizontal centerline of the shell
            // #2the vertical centerline of the shell
            // #3the downcomer box startup spout holes
            // #4the downcomer centerlines
            // ^generates the functional design drawing notations
            ioDCClines ??= new colDXFEntities();
            dxfVector v1;
            dxfVector v2;
            dxfVector v3;
            colDXFVectors Pts;

            int i;
            int j;
            dxfVector org;
            dxoDimTool dims;
            dxoDrawingTool draw;
            dxoLeaderTool leader;
            try
            {
                ioSUSpouts ??= Assy.StartupSpouts.Slots;
                draw = Image.Draw;
                dims = Image.DimTool;
                leader = Image.LeaderTool;
                org = Image.UCS.Origin;
                v1 = dxfVector.Zero;
                v2 = dxfVector.Zero;
                ioDCClines ??= new colDXFEntities();


                if (ioSUSpouts.Count > 0)
                {
                    Pts = ioSUSpouts.CentersDXF();

                    if (Assy.OddDowncomers)
                        Pts.GetVectors(aFilter: dxxPointFilters.LessThanX, aOrdinate: -(Assy.Downcomer().Width / 2) - 1, bOnIsIn: false, bRemove: true);
                    else
                        Pts.GetVectors(aFilter: dxxPointFilters.LessThanX, aOrdinate: 0, bOnIsIn: false, bRemove: true);

                    Image.Layers.Add("SUS", dxxColors.Blue);

                    Pts.SetOrdinates(0, dxxOrdinateDescriptors.Z);
                    draw.aCircles(Pts, ioSUSpouts.Item(1).Length / 2, new dxfDisplaySettings("SUS", dxxColors.ByLayer, dxfLinetypes.Continuous));


                    v1 = Pts.GetVector(dxxPointFilters.GetRightTop).Clone();

                    v1.MovePolar(45, ioSUSpouts.Item(1).Length / 2);

                    v2.SetCoordinates(8 * aPaperScale, 5.8 * aPaperScale);

                    leader.Text(v1, v2, $"NOTE: CIRCLES\nINDICATE START-UP\nSPOUT LOCATIONS");
                }
                xCancelCheck();
                Pts = ioDCClines.EndPoints(bIncludeStartPts: true, bReturnClones: true);
                if (Assy.OddDowncomers)
                {
                    if (aVerCenterLn != null)
                        Pts.Append(aVerCenterLn.EndPoints(), true);
                }

                if (Pts.Count > 0)
                {
                    Pts.Sort(dxxSortOrders.LeftToRight);

                    // The Pts contains more points than the number of downcomers. To match them by the downcomers, we only pick the unique Xs.
                    // Also, the ordinates do not follow the same pattern. Sometimes, for the same X, the first one is the higher ordinate and sometimes the lower ordinate. To cope with that, we use a dictionary to pick the higher one for each unique X.
                    Dictionary<double, dxfVector> ptsDic = new Dictionary<double, dxfVector>();
                    foreach (var point in Pts)
                    {
                        if (ptsDic.ContainsKey(point.X))
                        {
                            if (point.Y > ptsDic[point.X].Y)
                            {
                                ptsDic[point.X] = point;
                            }
                        }
                        else
                        {
                            ptsDic.Add(point.X, point);
                        }
                    }

                    j = 0;
                    mdDowncomer currentDC;
                    int downcomerNumber;
                    foreach (var kv in ptsDic)
                    {
                        v1 = (kv.Value - org).Moved(aYChange: 0.2 * aPaperScale);
                        currentDC = Assy.Downcomers.Item(Assy.Downcomers.Count - j);
                        downcomerNumber = currentDC.Index;
                        if (currentDC.Boxes.Any(b => b.IsVirtual))
                        {
                            downcomerNumber = Assy.Downcomers.Count - currentDC.Index + 1;
                        }
                        draw.aText(v1, $"DC# {downcomerNumber}", 0.125 * aPaperScale, dxxMTextAlignments.BottomCenter);
                        j++;
                    }
                }
                Pts = new colDXFVectors(Assy.DeckPanels.Centers(false));
                Pts.RotateAbout(dxfDirection.WorldZ, 90);
                Pts.Sort(dxxSortOrders.TopToBottom);
                //v1 = aHorCenterLn.StartPt.Subtracted(org);
                v1 = new dxfVector( aHorCenterLn.StartPt);
                for (i = 1; i <= Pts.Count; i++)
                {
                    v2 = Pts.Item(i);
                    v2.Y = org.Y + v2.Y;
                    v2.X = v1.X;
                    dims.OrdinateV(v1, v2, -0.4, aOverideText: $"GROUP# {i}");

                }

                Pts = ioDCClines.EndPoints(bIncludeStartPts: true, bReturnClones: true);
                Pts = new colDXFVectors(Pts.GetVectors(aFilter: dxxPointFilters.LessThanY, aOrdinate: Image.UCS.Y));
                Pts = new colDXFVectors(Pts.GetVectors(aFilter: dxxPointFilters.GreaterThanX, aOrdinate: Image.UCS.X));
                if (Pts.Count > 0)
                {
                    Pts.Sort(dxxSortOrders.RightToLeft);
                    Pts.Add(aVerCenterLn.EndPoints().GetVector(dxxPointFilters.AtMinY), bAddClone: true);

                    v1 = Pts.GetVector(dxxPointFilters.AtMinY).Clone();
                    v1.Y -= 0.4 * aPaperScale;
                    for (i = 1; i <= Pts.Count - 1; i++)
                    {
                        v2 = Pts.Item(i);
                        v3 = Pts.Item(i + 1);
                        if (i > 1)
                            Image.DimStyleOverrides.SuppressExtLine1 = true;

                        dims.Horizontal(v2, v3, v1.Y, bAbsolutePlacement: true, bSuppressUCS: true);
                    }

                    Image.DimStyleOverrides.ClearLineSuppressions();
                    v2 = Pts.LastVector();
                    v3 = Image.UCS.Vector(ShellRad);
                    dims.Horizontal(v2, v3, v1.Y - 0.5 * aPaperScale, aSuffix: " INSIDE RADIUS", bAbsolutePlacement: true, bSuppressUCS: true);
                }
                xCancelCheck();
                if (Assy.DesignOptions.HasBubblePromoters && aDeckSectionInserts != null)
                {

                    uopVectors bps = Assy.BPCenters(true, aSuppressedValue: false); //dxfEntities.GetInstanceMemberPoints(aDeckSectionInserts).FindAll((x) => string.Compare(x.Tag, "BUBBLE PROMOTER", true) == 0);

                    if (bps.Count > 0)
                    {
                        v1 =   new dxfVector(dxfVectors.FindVector(bps, dxxPointFilters.GetLeftBottom));
                        v2 = dxfVector.Zero;

                        v2.Project(v2.DirectionTo(v1), ShellRad + 0.5 * aPaperScale);
                        leader.Text(v1, v2, $"{bps.Count} BUBBLE\\PPROMOTERS");
                    }
                }
                xCancelCheck();

                // Beam leader note
                if (Assy.DesignFamily.IsBeamDesignFamily())
                    DrawBeamLeaderForFunctionalDrawing(aPaperScale, Assy.Beam, leader);

            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = string.Empty; }
        }

        private void DrawBeamLeaderForFunctionalDrawing( double paperscale, mdBeam beam, dxoLeaderTool leader)
        {
            // Gather the information and convert them to metric
            //double beamHeightM, beamLengthM, beamWidthM;

            //beamHeightM = beam.Height * 25.4;
            //beamLengthM = beam.Length * 25.4;
            //beamWidthM = beam.Width * 25.4;
            dxoDimStyle dstyle  = Image.DimStyle();
            List<object> dims = new List<object>() { beam.Height, beam.Width, beam.Length };
            List<string> english = dstyle.FormatNumbers(  dims, bApplyLinearMultiplier: false, aPrecision: 3, aSuffix: "\"");
            List<string> metric = dstyle.FormatNumbers(dims, bApplyLinearMultiplier: false, aPrecision:0, aSuffix: " mm", aMultiplier:25.4);

            string beamLeaderNote = $"I-BEAM {english[0]} [{metric[0]}] HIGH\\Px {english[1]} [{metric[1]}] WIDE\\Px {english[2]} [{metric[2]}] LONG";
            if (beam.OccuranceFactor > 1) // For two beams scenario
                beamLeaderNote += " (2 PLACES)";

            dxfVector v0 = Image.UCS.Origin;
            dxfVector v1 = beam.Plane.Vector(beam.Length / 2 - 0.25 * paperscale) + v0;
            dxfDirection dir = v0.DirectionTo(v1);
            dxfVector v2 = v0 + dir * (Assy.ShellID / 2 + 1.5 * paperscale);
            leader.Text(v1-v0, v2-v0, beamLeaderNote);

            // Format the leader not text
           // string beamLeaderNote = $"I-BEAM {beam.Height:F3}\" [{beamHeightM:F1}mm] HIGH\r\nx {beam.Width:F3}\" [{beamWidthM:F1}mm] WIDE\r\nx {beam.Length:F3}\" [{beamLengthM:F1}mm] LONG";

            //// Find the leader points
            //double leaderLineAngle = 45;
            //if (beam.OccuranceFactor > 1) // For two beams scenario
            //{
            //    leaderLineAngle = 75;
            //    beamLeaderNote += "\r\nTYP.";
            //}

            //dxeLine tempLine = new dxeLine(beam.Vertices().GetVector(dxxPointFilters.GetTopLeft), beam.Vertices().GetVector(dxxPointFilters.GetRightTop));
            //dxfVector leaderArrowPoint = tempLine.MidPt;

            //dxfVector leaderTextPoint = leaderArrowPoint.Clone();
            //leaderTextPoint.MovePolar(leaderLineAngle, 40);

            //// Set the leader
            //leader.Text(leaderArrowPoint, leaderTextPoint, beamLeaderNote);
        }

        private colDXFVectors Draw_FingerClips(bool bBothSides,bool bSuppressInstances = false)
        {
     
            if (Assy == null) return colDXFVectors.Zero;

            List<mdFingerClip> parts = mdPartGenerator.FingerClips_ASSY(Assy, null, bApplyInstances: true,bTrayWide:  bBothSides);
            if (parts.Count <= 0) return colDXFVectors.Zero;

            mdFingerClip part = parts[0];
          
            string lname = $"{LayerPre}FINGER_CLIPS";

           // if (!Assy.IsSymmetric) bBothSides = true;
            dxfDisplaySettings dsp = dxfDisplaySettings.Null(aLayer: lname);
            //dxePolygon pg = null;
            Status = "Drawing Finger Clips";
            colDXFVectors _rVal = colDXFVectors.Zero;
            try
            {
                dxoDrawingTool draw = Image.Draw;
                dxfBlock fcblock = mdBlocks.FingerClip_View_Plan(Image, part, aLayerName: lname);

                dxfVector org = Image.UCS.Origin;
                dxfVector u0 = new dxfVector(part.Center) { Rotation = part.Rotation } ;
                _rVal.Add(u0);


                //pg = part.View_Plan(aCenter: dxfVector.Zero, aLayerName: lname);
                string bname = fcblock.Name;
                //pg.BlockName = bname;

                dxeInsert ins = null;
                uopInstances insts = part.Instances;

                double? minx = null;
                if (!bBothSides && Assy.IsSymmetric) minx = -Assy.Downcomer().Width - 1;

                if (bSuppressInstances)
                {

                    ins = draw.aInsert(aBlockName: bname, aInsertPT: u0, aRotationAngle: part.Rotation,  aDisplaySettings: dsp);
                    bname = ins.BlockName;

                }

                foreach (uopInstance item in insts)
                {
                    dxfVector ip = new(u0.X + item.DX, u0.Y + item.DY) { Rotation = item.Rotation};
                    if (!minx.HasValue || (ip.X >= minx.Value))
                    {
                        
                        if (bSuppressInstances)  draw.aInsert(bname, ip, item.Rotation, aDisplaySettings: dsp);
                        _rVal.Add(ip);
                    }

                }
               if (!bSuppressInstances) draw.aInserts(bname, _rVal,  aDisplaySettings: dsp, bUseInsertionPtRotations: true);
            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = string.Empty; }

            return _rVal;

        }


        private colDXFEntities Draw_EndAngles(bool bBothSides)
        {
            colDXFEntities rInserts = new();
            if (Assy == null) return rInserts;

            List<mdEndAngle> parts = mdPartGenerator.EndAngles_ASSY(Assy, bApplyInstances: true);
            if (parts.Count <= 0) return rInserts;


            string lname = $"{LayerPre}END_ANGLES";
            dxfDisplaySettings dsp = dxfDisplaySettings.Null(aLayer: lname);



            Status = "Drawing End Angles";

            try
            {
                dxoDrawingTool draw = Image.Draw;

                foreach (mdEndAngle part in parts)
                {
                    string pn = part.PartNumber;

                    string bname = $"END_ANGLE_{pn.ToUpper()}_PLAN";
                    dxePolygon pg = part.View_Plan(dxfVector.Zero);
                    pg.BlockName = bname;
                    dxeInsert ins = null;
                    uopInstances insts = part.Instances;
                    uopVector u0 = part.Center;
                    
                    double? minx = null;
                    if (!bBothSides && Assy.IsSymmetric)
                        minx = -Assy.Downcomer().Width;

                    ins = draw.aInsert(pg, new dxfVector(u0.X, u0.Y), part.Rotation, bname, aDisplaySettings: dsp, aLTLSetting: dxxLinetypeLayerFlag.ForceToColor);
                    rInserts.Add(ins, aTag: pn);
                    bname = ins.BlockName;
                    foreach (uopInstance item in insts)
                    {
                        dxfVector ip = new(u0.X + item.DX, u0.Y + item.DY);
                        if (!minx.HasValue || (ip.X >= minx.Value))
                        {
                            ins = draw.aInsert(bname, ip, item.Rotation, aDisplaySettings: dsp);

                            rInserts.Add(ins, aTag: pn);
                            if (bBothSides && Assy.DesignFamily.IsBeamDesignFamily())
                            {
                                ip *= -1;
                                ins = draw.aInsert(bname, ip, item.Rotation + 180, aDisplaySettings: dsp);
                                rInserts.Add(ins, aTag: pn);
                            }
                        }

                    }
                }


            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = string.Empty; }

            return rInserts;

        }

        /// <summary>
        ///#1the view to draw
        ///^used by higher level drawing functions to draw the downcomer boxes end plates in the tray assembly
        ///~the view argument controls how the downcomer box is drawn.
        /// </summary>
        /// <param name="aView"></param>

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

        private void Draw_Washers(bool bBothSides)
        {

            Status = "Drawing Washers";
            string lname = $"{LayerPre}WASHERS";
            try
            {
                dxoDrawingTool draw = Image.Draw;

                Assy.GetRingClipCenters(bBothSides, false, false, out colDXFVectors dcPts, out colDXFVectors dkPts);
                if (dcPts.Count > 0)
                {
                    double rad = Assy.SmallBolt().GetWasher().OD / 2;
                    colDXFVectors iPts = dcPts.GetByTag("HIDDEN", bRemove: true);
                    if (iPts.Count > 0)
                    {
                        draw.aCircles(iPts, rad, new dxfDisplaySettings(lname, dxxColors.ByLayer, dxfLinetypes.Hidden));
                    }
                    draw.aCircles(dcPts, rad, new dxfDisplaySettings(lname, dxxColors.ByLayer, dxfLinetypes.Continuous));
                }

                if (dkPts.Count > 0)
                {
                    double rad = mdGlobals.HoldDownWasherRadius;
                    colDXFVectors iPts = dkPts.GetByTag("HIDDEN", bRemove: true);
                    if (iPts.Count > 0)
                    {
                        draw.aCircles(iPts, rad, new dxfDisplaySettings(lname, dxxColors.ByLayer, dxfLinetypes.Hidden));
                    }
                    draw.aCircles(dkPts, rad, new dxfDisplaySettings(lname, dxxColors.ByLayer, dxfLinetypes.Continuous));
                }


                xCancelCheck();
            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = string.Empty; }
        }

        private void Draw_ManwayFasteners(uppViews aView, bool aDrawBothSides = false)
        {
            if (Assy == null) return;


            // #1the view to draw
            // ^used by higher level drawing functions to draw manway fasteners in the tray assembly
            // " , "the view argument controls how the clamps are drawn.

            uopManwayClip aClip;
            uopManwayClamp aClamp;
            colUOPParts aParts;
            dxfBlock aBlk = null; // Perhaps it should get instantiated instead of being null!
            dxeInsert aIns;
            string lname = $"{LayerPre}MANWAY_FASTENERS";
            dxfVector v1;
            colDXFVectors ips;

            double ang = 0;
            dxePolygon PGon;
            dxfDisplaySettings dsp = new(lname, dxxColors.Undefined, "");
            try
            {
                Status = "Drawing Manway Fasteners";

                switch (aView)
                {
                    case uppViews.LayoutPlan:
                    case uppViews.InstallationPlan:
                    case uppViews.SlotZoneLayout:
                        aParts = Assy.ManwayFasteners(true, aDrawBothSides);
                        if (aParts.Count > 0)
                        {
                            v1 = dxfVector.Zero;


                            if (Assy.DesignOptions.UseManwayClips)
                            {
                                ips = new colDXFVectors();
                                aClip = new uopManwayClip();
                                PGon = aClip.View_Plan(bIncludeWasher: true, bObscured: true);
                                PGon.BlockName = "MANWAY_CLIP_PLAN";
                                PGon.LayerName = lname;
                                for (int i = 1; i <= aParts.Count; i++)
                                {
                                    aClip = (uopManwayClip)aParts.Item(i);
                                    v1 = new dxfVector(aClip.X, aClip.Y);
                                    if (aClip.Angle == 180)
                                    {
                                        ang = 180;
                                        v1.Move(-1.03125);
                                    }
                                    else if (aClip.Angle == 0)
                                    {
                                        ang = 0;
                                        v1.Move(1.03125);
                                    }
                                    else if (aClip.Angle == 90)
                                    {
                                        ang = 90;
                                        v1.Move(aYChange: 1.03125);
                                    }
                                    else if (aClip.Angle == 270)
                                    {
                                        ang = 270;
                                        v1.Move(aYChange: -1.03125);
                                    }

                                    aIns = Image.Draw.aInsert(PGon, v1, ang, aDisplaySettings: dsp, aLTLSetting: dxxLinetypeLayerFlag.ForceToColor);


                                }

                            }
                            else
                            {
                                aClamp = (uopManwayClamp)aParts.Item(1);

                                aIns = aClamp.Insert(uppPartViews.Top, Image, dxxLinetypeLayerFlag.ForceToColor, lname, dxxColors.BlackWhite, dxfLinetypes.Continuous, aBlock: aBlk);
                                Image.Blocks.Add(aBlk);

                                for (int i = 1; i <= aParts.Count; i++)
                                {
                                    aClamp = (uopManwayClamp)aParts.Item(i);
                                    v1.SetCoordinates(aClamp.X, aClamp.Y);
                                    Image.Draw.aInsert(aBlk.Name, v1, aClamp.Angle);
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }

                //DoEvents // I don't know what it does
            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
        }

        private void Draw_Rings(uppViews aView)
        {
            // #1the view to draw
            // ^used by higher level drawing functions to draw the support ring
            // " , "the view argument controls how the ring is drawn.



            try
            {

                dxoDrawingTool draw = Image.Draw;
                Status = "Drawing Ring";
                string lname = $"{LayerPre}RING";
                switch (aView)
                {
                    case uppViews.LayoutElevation:
                    case uppViews.InstallationElevation:
                        {
                            Image.Layers.Add(lname);

                            uopRingClip aRC = Assy.RingClip();
                            double rad = Assy.RingClipRadius;
                            double smallHeight = aRC.Size == uppRingClipSizes.ThreeInchRC ? 0.625 : 0.75;
                            dxfDisplaySettings dsp = dxfDisplaySettings.Null(lname, aColor: dxxColors.BlackWhite, aLinetype: dxfLinetypes.Continuous);
                            dxePolygon aPG = aRC.View_Profile(false, false, false, dxfVector.Zero, aLayerName: lname);
                            double halfSpace = MDRange.RingSpacing / 2;
                            // draw the rings

                            draw.aRectangle( new dxfVector(-ShellRad, halfSpace), ShellRad - RingRad, -RingTk, dxxRectangleMethods.ByCorner, aDisplaySettings:dsp);
                            draw.aRectangle(new dxfVector(ShellRad, halfSpace), -(ShellRad - RingRad), -RingTk, dxxRectangleMethods.ByCorner,  aDisplaySettings: dsp);

                            draw.aRectangle(new dxfVector(-ShellRad, -halfSpace), ShellRad - RingRad, -RingTk, dxxRectangleMethods.ByCorner, aDisplaySettings: dsp);
                            draw.aRectangle(new dxfVector(ShellRad, -halfSpace), -(ShellRad - RingRad), -RingTk, dxxRectangleMethods.ByCorner, aDisplaySettings: dsp);

                            // draw the ring clips
                            if (DrawPhantom)
                            {
                                aPG.Linetype = dxfLinetypes.Phantom;
                                aPG.AdditionalSegments.SetDisplayVariable(dxxDisplayProperties.Linetype, dxfLinetypes.Phantom);
                                aPG.AdditionalSegments.SetDisplayVariable(dxxDisplayProperties.LayerName, dxfLinetypes.Phantom);
                                Image.LinetypeLayers.ApplyTo(aPG, dxxLinetypeLayerFlag.ForceToLayer);
                                dsp.Linetype = dxfLinetypes.Phantom;
                                dsp.LayerName = dxfLinetypes.Phantom;
                            }
                            draw.aInsert(aPG, new dxfVector(rad, halfSpace - (RingTk + smallHeight)), 0, "RING_CLIP", aDisplaySettings: dsp);
                            draw.aInsert("RING_CLIP", new dxfVector(rad, -halfSpace - (RingTk + smallHeight)), 0, aDisplaySettings: dsp);
                            draw.aInsert("RING_CLIP", new dxfVector(-rad, -halfSpace - (RingTk + smallHeight)), 0, aDisplaySettings: dsp, aScaleFactor: -1, aYScale: 1, aZScale: 1);
                            draw.aInsert("RING_CLIP", new dxfVector(-rad, halfSpace - (RingTk + smallHeight)), 0, aDisplaySettings: dsp, aScaleFactor: -1, aYScale: 1, aZScale: 1);
                            break;
                        }


                    case uppViews.LayoutPlan:
                    case uppViews.DesignView:
                    case uppViews.InstallationPlan:
                        {
                            lname = "HEAVY";
                            Image.Layers.Add(lname, dxxColors.Grey, dxfLinetypes.Continuous);
                            dxeArc ring = draw.aCircle(dxfVector.Zero, RingRad, new dxfDisplaySettings(lname, Image.LinetypeLayers.LineColor(dxfLinetypes.Hidden), dxfLinetypes.Hidden));


                            break;
                        }

                    default:
                        break;
                }

                // doevents
                Status = "";

                xCancelCheck();
            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = string.Empty; xCancelCheck(); }
        }

        private void Draw_ShelfAngles(uppViews aView, bool bDrawBothSides = true)
        {
            // #1the view to draw
            // ^used by higher level drawing functions to draw downcomer shelf angles in the tray assembly
            // " , "the view argument controls how the beam is drawn.

            mdDowncomer DComer;
            dxePolygon PGon;
            List<colUOPParts> listOfShelfAnglePairs;
            mdShelfAngle SAngle;
            colMDDowncomers DComers;
            dxfVector v1;

            string lname;

            try
            {
                Status = "Drawing Shelf Angles";

                lname = $"{LayerPre}SHELF_ANGLES";
                dxoDrawingTool draw = Image.Draw;
                dxfDisplaySettings dsp = Image.GetDisplaySettings(dxxEntityTypes.Polyline, lname);

                switch (aView)
                {
                    case uppViews.LayoutPlan:
                    case uppViews.InstallationPlan:
                        Image.Layers.Add(lname, Image.LinetypeLayers.LineColor(dxfLinetypes.Hidden), dxfLinetypes.Hidden);
                        DComers = Assy.Downcomers;
                        for (int i = 1; i <= DComers.Count; i++)
                        {
                            DComer = DComers.Item(i);
                            listOfShelfAnglePairs = DComer.ShelfAngles;
                            foreach (var SAngles in listOfShelfAnglePairs)
                            {
                                for (int j = 1; j <= SAngles.Count; j++)
                                {
                                    SAngle = (mdShelfAngle)SAngles.Item(j);

                                    PGon = SAngle.View_Plan(dxfVector.Zero, aLayerName: lname);
                                    PGon.BlockName = $"ShelfAngle_{DComer.PartNumber}_{j}";
                                    v1 = SAngle.CenterDXF;
                                    draw.aInsert(PGon, v1, aDisplaySettings: dsp, aLTLSetting: dxxLinetypeLayerFlag.ForceToColor);

                                    if (bDrawBothSides)
                                    {
                                        v1.X *= -1;

                                        draw.aInsert(PGon, v1, aRotationAngle: 180, aDisplaySettings: dsp, aLTLSetting: dxxLinetypeLayerFlag.ForceToColor);
                                    }
                                    PGon.Dispose();
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }

                // doevents
                Status = "";

                xCancelCheck();
            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
        }


        private (List<mdSpoutGroup>, List<dxfBlock>) Draw_SpoutGroupBlocks(bool bDrawBothSides = true, string aSGHandle = null)
        {
            // ^used by higher level drawing functions to draw spout groups in the tray assembly as blocks
            // " , "the view argument controls how the beam is drawn.

            dxfBlock block;
            List<mdSpoutGroup> sgs = Assy.SpoutGroups.GetByVirtual(aVirtualValue: false);

            string lname = $"{LayerPre}SPOUTS";
            List<mdSpoutGroup> rgroups = new();
            List<dxfBlock> rBlocks = new();

            try
            {
                Status = "Drawing Spout Groups";
                dxoDrawingTool draw = Image.Draw;
                dxfDisplaySettings dsp = new(lname, dxxColors.ByLayer);
                if (!String.IsNullOrWhiteSpace(aSGHandle))
                {
                    mdSpoutGroup asg = sgs.Find(x => string.Compare(x.Handle, aSGHandle, true) == 0);
                    if (asg == null) return (rgroups, rBlocks);

                    block = asg.Block(dsp: dsp, aImage: Image);
                    if (block.Entities.Count > 0)
                    {
                        rgroups.Add(asg);
                        rBlocks.Add(block);
                        block = Image.Blocks.Add(block);

                        draw.aInsert(block.Name, new dxfVector(asg.X, asg.Y), 0, aDisplaySettings: dsp);

                    }
                }
                else
                {
                    foreach (mdSpoutGroup sg in sgs)
                    {

                        if (sg.SpoutCount(Assy) <= 0) continue;

                        block = sg.Block(dsp: dsp, aImage: Image);
                        if (block.Entities.Count > 0)
                        {
                            block = Image.Blocks.Add(block);
                            rgroups.Add(sg);
                            rBlocks.Add(block);
                            uopVector ip = new(sg.X, sg.Y);
                            draw.aInsert(block.Name, ip, 0, aDisplaySettings: dsp);
                            sg.GetMirrorValues(out double? mirry, out double? mirrx);
                            if (mirry.HasValue)
                            {
                                ip.Mirror(null, mirry.Value);
                                draw.aInsert(block.Name, ip, 0, aDisplaySettings: dsp, aYScale: -1);

                            }
                            if (bDrawBothSides && mirrx.HasValue)
                            {
                                ip = new uopVector(sg.X, sg.Y);
                                ip.Mirror(mirrx.Value, null);
                                draw.aInsert(block.Name, ip, 0, aDisplaySettings: dsp, aScaleFactor: -1, aYScale: 1, aZScale: 1);
                                if (mirry.HasValue)
                                {
                                    ip.Mirror(null, mirry.Value);
                                    draw.aInsert(block.Name, ip, 0, aDisplaySettings: dsp, aScaleFactor: -1, aYScale: -1, aZScale: 1);

                                }
                            }

                        }
                    }


                }

                return (rgroups, rBlocks);


            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
            finally { xCancelCheck(); Status = ""; }
            return (rgroups, rBlocks);
        }

        private dxfRectangle Draw_Shells(uppViews aView = uppViews.LayoutPlan)
        {
            return Draw_Shells(aView, out dxeLine _, out dxeLine _);
        }


        private colDXFEntities Draw_BeamAttachments_View_1(double paperscale)
        {
            int marker = Image.Entities.Count + 1;

            mdBeam beam = Assy.Beam;

            try
            {
                Status = "Drawing Plan View";
                dxoDrawingTool draw = Image.Draw;
                dxfVector origin = Image.UCS.Origin;
                Draw_Shells(uppViews.AttachPlan, out dxeLine vCline, out dxeLine hCline);

                dxfVector v1 = vCline.EndPoints().GetVector(dxxPointFilters.AtMaxY) - origin + new dxfVector(0, 0.375 * paperscale);
                draw.aText(v1, "0%%D", aTextHeight: 0.1875 * paperscale, aAlignment: dxxMTextAlignments.MiddleCenter, aTextType: dxxTextTypes.DText);

                v1 = vCline.EndPoints().GetVector(dxxPointFilters.AtMinY) - origin + new dxfVector(0, -0.375 * paperscale);
                draw.aText(v1, "180%%D", aTextHeight: 0.1875 * paperscale, aAlignment: dxxMTextAlignments.MiddleCenter, aTextType: dxxTextTypes.DText);

                v1 = hCline.EndPoints().GetVector(dxxPointFilters.AtMaxX) - origin + new dxfVector(0.375 * paperscale, 0);
                draw.aText(v1, "90%%D", aTextHeight: 0.1875 * paperscale, aAlignment: dxxMTextAlignments.MiddleCenter, aTextType: dxxTextTypes.DText);

                v1 = hCline.EndPoints().GetVector(dxxPointFilters.AtMinX) - origin + new dxfVector(-0.375 * paperscale, 0);
                draw.aText(v1, "270%%D", aTextHeight: 0.1875 * paperscale, aAlignment: dxxMTextAlignments.MiddleCenter, aTextType: dxxTextTypes.DText);

                dxeLine line45 = new dxeLine(hCline);
                line45.RotateAbout(line45.MidPt, beam.Rotation);
                dxfDirection d1 = line45.Direction();
                Image.Entities.Add(line45);

                v1 = (line45.EndPoints().GetVector(dxxPointFilters.AtMaxX) - origin) + d1 * (0.375 * paperscale);
                draw.aText(v1, $"{line45.AngleOfInclination():0}%%D", aTextHeight: 0.1875 * paperscale, aAlignment: dxxMTextAlignments.MiddleCenter, aTextType: dxxTextTypes.DText);

                v1 = (line45.EndPoints().GetVector(dxxPointFilters.AtMinX) - origin) - d1 * (0.375 * paperscale);
                draw.aText(v1, $"{dxfUtils.NormalizeAngle(line45.AngleOfInclination() + 180):0}%%D", aTextHeight: 0.1875 * paperscale, aAlignment: dxxMTextAlignments.MiddleCenter, aTextType: dxxTextTypes.DText);

                line45 = line45.Clone();
                line45.RotateAbout( line45.MidPt, 90 );
                d1 = line45.Direction();
                Image.Entities.Add( line45 );

                v1 = (line45.EndPoints().GetVector( dxxPointFilters.AtMaxX ) - origin) - d1 * (0.375 * paperscale);
                draw.aText( v1, $"{line45.AngleOfInclination():0}%%D", aTextHeight: 0.1875 * paperscale, aAlignment: dxxMTextAlignments.MiddleCenter, aTextType: dxxTextTypes.DText );

                v1 = (line45.EndPoints().GetVector( dxxPointFilters.AtMinX ) - origin) + d1 * (0.375 * paperscale);
                draw.aText( v1, $"{dxfUtils.NormalizeAngle( line45.AngleOfInclination() + 180 ):0}%%D", aTextHeight: 0.1875 * paperscale, aAlignment: dxxMTextAlignments.MiddleCenter, aTextType: dxxTextTypes.DText );


                //Draw actual supports
                //define beam support block
                string bname = "BEAM_SUPPORT_PLAN_VIEW";
                dxfBlock block = null;
                uopVectors ips  = beam.BeamSupportInsertionPoints;

                if (!Image.Blocks.TryGet( bname, ref block ))
                {
                    block = mdBlocks.BeamSupport_View_Plan( Image, beam, Assy, bname, bObscured: false, bSuppressHoles: false );
                    Image.Blocks.Add( block );
                }

                var lip = new dxfVector(ips[0].X, ips[0].Y);
                var rip = new dxfVector(ips[1].X, ips[1].Y);

                draw.aInsert( bname, lip, beam.Rotation );
                draw.aInsert( bname, rip, beam.Rotation + 180, aYScale: -1 );

                if (beam.OccuranceFactor > 1)
                {
                    lip.MoveFromTo( beam.Center, new dxfVector( -beam.X, -beam.Y ) );
                    rip.MoveFromTo( beam.Center, new dxfVector( -beam.X, -beam.Y ) );
                    draw.aInsert( bname, lip, beam.Rotation, aYScale: -1 );
                    draw.aInsert( bname, rip, beam.Rotation + 180 );
                }
                dxoDimTool dims = Image.DimTool;
                draw.aDim.Aligned( lip, new dxfVector( -beam.X, -beam.Y ), -1, aSuffix: " (TYP)\\PI-BEAM SUPPORT" );


                if (!Assy.DesignFamily.IsBeamDesignFamily()) return Image.Entities.SubSet(marker, Image.Entities.Count);



            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = string.Empty; }
            return Image.Entities.SubSet(marker, Image.Entities.Count);

        }

        /// <summary>
        /// used by higher level drawing functions to draw the column shell
        /// </summary>
        /// <remarks>the view argument controls how the shell is drawn.</remarks>
        /// <param name="aView">the view to draw</param>
        /// <param name="rVerCenterLn">returns the vertical centerline used for dimensions in other functions</param>
        /// <param name="rHorCenterLn">the view to draw</param>
        /// <returns></returns>
        private dxfRectangle Draw_Shells(uppViews aView, out dxeLine rVerCenterLn, out dxeLine rHorCenterLn)
        {
            rVerCenterLn = null;
            rHorCenterLn = null;
            dxfRectangle result = null;
            colDXFEntities dents = new();
            double lg;

            try
            {
                Image.SelectionSetInit(false);

                Status = "Drawing Shell";
                dxoDrawingTool draw = Image.Draw;
                // put lines or a single circle into the dxf but draw rectangles or double circles to the screen

                Image.Layers.Add("HEAVY", dxxColors.Grey);

                Image.Layers.Add("CENTER", dxxColors.Red, aLinetype: dxfLinetypes.Center);
                switch (aView)
                {

                    case uppViews.LayoutElevation:
                    case uppViews.InstallationElevation:
                        {
                            lg = 1.25 * MDRange.RingSpacing;
                            dents.Add(draw.aLine(-ShellRad, -lg, -ShellRad, lg, "HEAVY", dxxColors.ByLayer, dxfLinetypes.Continuous));
                            dents.Add(draw.aLine(ShellRad, -lg, ShellRad, lg, "HEAVY", dxxColors.ByLayer, dxfLinetypes.Continuous));

                            // vertical centerline
                            rVerCenterLn = (dxeLine)draw.aCenterlines(dents.BoundingRectangle(), aSuppressHorizontal: true)[0];
                            break;

                        }
                    case uppViews.Plan:
                    case uppViews.AttachPlan:
                    case uppViews.LayoutPlan:
                    case uppViews.InstallationPlan:
                        {

                            // draw and add the inner diameter circle to the screen and to the dxf
                            dxeArc shell = draw.aCircle(dxfVector.Zero, ShellRad, dxfDisplaySettings.Null("HEAVY"));

                            lg = 1.05 * (2 * (ShellRad + 0.75));

                            rVerCenterLn = draw.aLine(new dxfVector(0, -lg / 2), new dxfVector(0, lg / 2), dxfDisplaySettings.Null(aLayer: "Center"));
                            rHorCenterLn = draw.aLine(new dxfVector(-lg / 2, 0), new dxfVector(lg / 2, 0), rVerCenterLn.DisplaySettings);



                            break;
                        }


                    default:
                        break;
                }

                // doevents


            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally
            {
                Status = StatusWas;
                xCancelCheck();

                result = Image.SelectionSetInit(true).BoundingRectangle();

            }



            return result;
        }

        /// <summary>
        /// used by higher level drawing functions to draw the splice angles in the tray assembly
        /// </summary>
        /// <param name="aView">the view to draw</param>
        /// <param name="rSplcPnls">returns a comma delimited list of the draw psplice angle part numbers</param>
        /// <param name="bDrawBothSides">flag to draw the 'virtual' side on standrard trays</param>

        private void Draw_SpliceAngles(uppViews aView, out string rSplcPnls, bool bDrawBothSides = false, int aViewIndex = 1)
        {
            rSplcPnls = "";

            mdSpliceAngle aAngle;
            dxePolygon PGon = null;
           
            int pid;
            dxePolygon block;
            try
            {
                dxoDrawingTool draw = Image.Draw;
                colUOPParts SAngles = null;
                List<uopPart> subset;

                Status = "Drawing Splice Angles";
                string lname = !DrawPhantom ? $"{LayerPre}SPLICE_ANGLES" : dxfLinetypes.Phantom;

                switch (aView)
                {
                    case uppViews.LayoutElevation:
                    case uppViews.InstallationElevation:
                        {
                            SAngles =new colUOPParts( Assy.SpliceAngles().GetByPoints(dxxPointFilters.GreaterThanY, ElevationX, true));
                            double halfSpace = MDRange.RingSpacing / 2;

                            if (SAngles.Count > 0)
                            {
                                colDXFVectors ips = new();
                                block = null;
                                for (pid = 2; pid <= Assy.DeckPanels.Count; pid++)
                                {
                                    if (Assy.DeckPanels.Item(pid).IsHalfMoon)
                                    {
                                        continue;
                                    }

                                    aAngle = (mdSpliceAngle)SAngles.GetByPoint(dxxPointFilters.AtMinY, aPanelID: pid);
                                    if (aAngle != null)
                                    {
                                        ips.Add(new dxfVector(aAngle.X, aAngle.Z - halfSpace));
                                        if (block == null)
                                        {
                                            PGon = aAngle.View_Elevation(dxfVector.Zero, aLayerName: lname);
                                            PGon.BlockName = "SPLICE_ANGLE_ELEV";
                                            block = PGon;
                                        }

                                        if (Math.Round(aAngle.X, 1) > 0)
                                        {
                                            ips.Add(new dxfVector(-aAngle.X, aAngle.Z - halfSpace, aRotation: aViewIndex == 2 ? -90 : 0));
                                        }

                                        mzUtils.ListAdd(ref rSplcPnls, pid);
                                    }
                                    xCancelCheck();
                                }
                                if (ips.Count > 0)
                                {
                                    draw.aInserts(block, ips, aDisplaySettings: dxfDisplaySettings.Null(aLayer: lname), aLTLSetting: dxxLinetypeLayerFlag.ForceToColor);
                                }
                                block?.Dispose();

                                ips = new colDXFVectors();
                                block = null;
                                subset = Assy.SpliceAngles().FindAll(x => x.PanelIndex ==Assy.DeckPanels.Count);
                                for (int i = 1; i <= subset.Count; i++)
                                {
                                    aAngle = (mdSpliceAngle)subset[i - 1];
                                    ips.Add(new dxfVector(aAngle.Y, aAngle.Z + halfSpace));
                                    if (block == null)
                                    {

                                        PGon = aAngle.View_Profile(false, true, dxfVector.Zero, aLayerName: lname);
                                        PGon.BlockName = "SPLICE_ANGLE_ELEV_MOON";
                                        block = PGon;
                                    }

                                    xCancelCheck();
                                }
                                if (ips.Count > 0)
                                {
                                    draw.aInserts(block, ips, aDisplaySettings: new dxfDisplaySettings(aLayer: lname), aLTLSetting: dxxLinetypeLayerFlag.ForceToColor);
                                }
                                block?.Dispose();
                            }
                            break;
                        }

                    case uppViews.Plan:
                    case uppViews.LayoutPlan:
                    case uppViews.InstallationPlan:

                        
                        {
                            double rot = 0;
                            if (aViewIndex >1) 
                                rot = 90;

                            double? minX = null;
                            if (!bDrawBothSides && Assy.DesignFamily.IsStandardDesignFamily())
                            {
                                mdDeckPanel lastpanel = (mdDeckPanel)Assy.DeckPanels.FindAll(x => !x.IsVirtual).Last();
                                if (lastpanel != null) minX = lastpanel.Left();
                            }

                            List<mdDeckSection> sections = aViewIndex >1 ? MDProject.GetParts().AltDeckSections(Assy.RangeGUID) : MDProject.GetParts().DeckSections(Assy.RangeGUID);
                            List<mdSpliceAngle> angles = mdDeckSections.GenerateSectionSpliceAngles(sections, Assy,bSetInstances: true);
                            dxfBlock blok = null;
                            foreach (var angle in angles)
                            {
                                blok = Image.Blocks.Add(mdBlocks.SpliceAngle_View_Plan(Image, angle, Assy, aLayerName: lname, bShowHidden: true, bSuppressHoles: false, bSetInstances: true));
                                if (rot != 0 && blok.Instances.Count > 0)
                                    blok.Instances.UpdateMembers(aRotationAdder: rot);
                                
                                if(minX.HasValue)
                                {
                                    dxfVector basept = blok.Instances.BasePlane.Origin;
                                    colDXFVectors ips = blok.Instances.MemberPoints();
                                    ips.RemoveAll(x => x.X <= minX);
                                    blok.Instances.DefineWithVectors(ips, basept,false);
                                }

                                draw.aInserts(blok, blok.Instances, false,rot);
                            }




                            //Assy.DeckSections.GenerateSpliceAngles(Assy, true);

                            //foreach (uppPartTypes item in types)
                            //{
                            //    subset =   .FindAll(x => x.PartType == item);
                            //    uopVector insetpt = uopVector.Zero;

                            //    for (int i = 1; i <= subset.Count; i++)
                            //    {
                            //        aAngle = (mdSpliceAngle)subset[i - 1];
                            //        v1 = aAngle.CenterDXF;
                            //        v1.Z = 0;
                            //        if (i == 1)
                            //        {

                            //            insetpt = aAngle.Center;

                            //        }
                            //        if (Math.Round(aAngle.Y, 1) >= 0)
                            //        {
                            //            if (aAngle.Orientation == dxxRadialDirections.TowardsCenter)
                            //            {
                            //                blok.Instances.Add(v1.X, v1.Y, aRotation: 0);
                            //                //if (bDrawBothSides && Math.Round(v1.X, 1) != 0) ips.AddInsertionPt(-v1.X, -v1.Y, 0, 180);

                            //            }
                            //            else
                            //            {
                            //                blok.Instances.Add(v1.X, v1.Y, aRotation: 180);
                            //                //if (bDrawBothSides && Math.Round(v1.X, 1) != 0) ips.AddInsertionPt(-v1.X, -v1.Y, 0, 0);

                            //            }
                            //        }
                            //        else
                            //        {
                            //            if (aAngle.Orientation == dxxRadialDirections.AwayFromCenter)
                            //            {
                            //                blok.Instances.Add(v1.X, v1.Y, aRotation: 0);
                            //                //if (bDrawBothSides && Math.Round(v1.X, 1) != 0) ips.AddInsertionPt(-v1.X, -v1.Y, 0, 180);

                            //            }
                            //            else
                            //            {
                            //                blok.Instances.Add(v1.X, v1.Y, aRotation: 180);
                            //                //if (bDrawBothSides && Math.Round(v1.X, 1) != 0) ips.AddInsertionPt(-v1.X, -v1.Y, 0, 0);

                            //            }
                            //        }
                            //    //    }
                            //    //    draw.aInserts(blok, blok.Instances,false  );

                            //    }
                            break;
                        }



                      
                    default:
                        break;
                }



                xCancelCheck();
                // doevents
            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = StatusWas; }

        }

        private void Draw_FunctionalPlanView(out dxeLine rVerCenterLn, out dxeLine rHorCenterLn, out colDXFEntities rDCClines, out List<dxeInsert> rSectionInserts)
        {
            rDCClines = new colDXFEntities();
            rVerCenterLn = null;
            rHorCenterLn = null;
            rSectionInserts = new List<dxeInsert>();
            if (Assy == null) return;

            try
            {
                Image.Layers.Add("DOWNCOMERS", dxxColors.z_132);
                Image.Layers.Add("END ANGLES", dxxColors.z_132);
                Image.Layers.Add("FINGER CLIPS", dxxColors.z_132);
                Image.Layers.Add("STIFFENERS", dxxColors.z_132);
                if (Assy.DesignFamily.IsEcmdDesignFamily()) Image.Layers.Add("DEFLECTOR PLATES", dxxColors.z_132);
                Image.Layers.Add("SPOUTS", dxxColors.z_132);
                Image.Layers.Add("DECK", dxxColors.z_41);
                xCancelCheck();

                Draw_Shells(uppViews.LayoutPlan, out rVerCenterLn, out rHorCenterLn);
                Draw_Rings(uppViews.LayoutPlan);

                xCancelCheck();
                if (Assy.DesignFamily.IsBeamDesignFamily())
                {
                    Draw_Beams(uppViews.Plan, bSuppressCenterLine: true);
                }
                xCancelCheck();
                Draw_DowncomersBelow(uppViews.FunctionalDesign, true);
                xCancelCheck();
                Draw_Downcomers(uppViews.FunctionalDesign, out rDCClines, bDrawSpouts: true, bDrawBothSides: true, bSuppressHoles: false, aLayer: "DOWNCOMERS", bDrawFingerClips: true, bDrawStiffeners: true, bDrawEndAngles: true, bDrawBaffles: true);
                xCancelCheck();
                Draw_SpliceAngles(uppViews.LayoutPlan, out _, true);
                xCancelCheck();
                rSectionInserts = Draw_DeckSections(uppViews.FunctionalDesign, bSuppressSlots: true);
                if (Assy.DesignFamily.IsEcmdDesignFamily())
                {
                    Draw_ECMDSlots(true, true);
                }
                xCancelCheck();
                Draw_ManwayFasteners(uppViews.LayoutPlan, true);
                xCancelCheck();
                Draw_Washers(true);
                xCancelCheck();
                Draw_APPans(uppViews.FunctionalDesign);

            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
            finally
            {
                DrawinginProgress = false;
            }
        }

       

        private void oDrawing_CancelDrawing(out bool rRecieved)
        {
            CancelOps = true;
            Status = "Drawing Canceled";
            rRecieved = true;
            //DoEvents // I don't know it does
        }



        public void HandleError(System.Reflection.MethodBase aMethod, Exception e)
        {
            if (Image == null || e == null || aMethod == null) return;
            Image.HandleError(string.IsNullOrWhiteSpace(Status) ? aMethod.Name : $"{aMethod.Name}~{Status}", this.GetType().Name, e.Message + (uopUtils.RunningInIDE ? e.StackTrace : ""));
        }

        public void Draw_Beams(uppViews aView, bool bSuppressHoles = false, string aLayerName = "BEAM", bool bSuppressCenterLine = false, bool bSuppressSupports = true, bool bObscured = true)
        {
            if (MDRange == null || Image == null) return;
            if (!Assy.DesignFamily.IsBeamDesignFamily()) return;
            dxoDrawingTool draw = Image.Draw;
            if (!string.IsNullOrWhiteSpace(aLayerName))
                aLayerName = Image.GetOrAddReference(aName: aLayerName, aRefType: dxxReferenceTypes.LAYER).Name;
            else
                aLayerName = "BEAM";

            
            string statuswuz = Status;
            double paperscale = Image.Display.PaperScale;
            try
            {
                if (aView == uppViews.Plan || aView == uppViews.ZoneView || aView == uppViews.LayoutPlan)
                {
                    mdBeam beam = Assy.Beam;
                    int beamcount = beam.OccuranceFactor;
                            Status = "Drawing Support Beams";

                    double centerLineLength = bSuppressCenterLine ? 0 : beam.Length + 0.5 * paperscale;
                    dxfDisplaySettings dsp = dxfDisplaySettings.Null(aLayer:aLayerName,  aLinetype: bObscured ? dxfLinetypes.Hidden:"");
                    
                    dxfBlock  beamblock = mdBlocks.Beam_View_Plan(Image,beam,Assy,bSetInstances:true , bObscured:true,bSuppressHoles: bSuppressHoles,aCenterLineLength: centerLineLength,aLayerName: aLayerName, bIncludeSupports: !bSuppressSupports);

                    draw.aInserts(beamblock, null, bOverrideExisting: false);

                    //dxeInsert insert = new dxeInsert(beamblock, beam.Center ) {DisplaySettings = dsp } ;
                    //if(beamcount ==2) insert.Instances.Add(-2*beam.X,-2*beam.Y);
                    //Image.Entities.Add(insert, aTag: beam.PartNumber);

                    //colDXFVectors ips = beamblock.Instances.MemberPoints(beam.Center);

                    //if (DrawlayoutRectangles)
                    //{
                    //    draw.aSymbol.Axis(beam.Plane, aAxisLength: 0.25 * paperscale);
                    //    //draw.aCircle(dxfVector.Zero, beam.BoundingRadius, dxfDisplaySettings.Null(aLinetype: dxfLinetypes.Phantom, aColor: dxxColors.LightGrey));
                    //}
                    //if (beamcount >1)
                    //{
                    //    insert = draw.aInsert(insert.BlockName, new dxfVector(-beam.X, -beam.Y), aDisplaySettings: insert.DisplaySettings);
                    //}

                    //if (!bSuppressSupports)
                    //{
                    //    //define beam support block
                    //    string bname = $"BEAM{beam.PartNumber}_SUPPORT_PLAN_VIEW";
                    //    dxfBlock block = null;
                     
                    //    if (!Image.Blocks.TryGet( bname, ref block ))
                    //        block = Image.Blocks.Add(mdBlocks.BeamSupport_View_Plan( Image, beam, Assy, bname, out ips, true ));
                    //    else
                    //        ips = beam.BeamSupportInsertionPoints;

                    //    //insert beam supports
                    //    var lip = beam.Plane.Vector( ips[ 0 ].X, ips[ 0 ].Y );
                    //    var rip = beam.Plane.Vector( ips[ 1 ].X, ips[ 1 ].Y );

                    //    draw.aInsert( bname, lip, beam.Rotation, aDisplaySettings: insert.DisplaySettings );
                    //    draw.aInsert( bname, rip, beam.Rotation + 180, aYScale: -1, aDisplaySettings: insert.DisplaySettings );

                    //    if (beamcount > 1)
                    //    {
                    //        insert = draw.aInsert( insert.BlockName, new dxfVector( -beam.X, -beam.Y ), aDisplaySettings: insert.DisplaySettings );
                    //        //insert beam supports
                    //        lip.MoveFromTo( beam.Center, new dxfVector( -beam.X, -beam.Y ) );
                    //        rip.MoveFromTo( beam.Center, new dxfVector( -beam.X, -beam.Y ) );
                    //        draw.aInsert( bname, lip, beam.Rotation, aYScale: -1, aDisplaySettings: insert.DisplaySettings );
                    //        draw.aInsert( bname, rip, beam.Rotation + 180, aDisplaySettings: insert.DisplaySettings );
                    //    }
                    //}
                }
            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = statuswuz; }

        }

        private colDXFEntities Draw_BeamDetail_PlanView(dxfRectangle drawingArea, mdBeam beam, double paperScale, dxoDrawingTool draw, dxoDimTool dims, out dxfVector centerOfBeam)
        {
            string iBeamLayerName = "GEOMETRY";
            int entmarker = Image.Entities.Count + 1;
            centerOfBeam = new dxfVector(drawingArea.Center.X, drawingArea.TopLeft.Y - 0.75 * paperScale - beam.Width / 2);
            try
            {
                Status = "Drawing beam plan view";
                //tring dimensionLayerName = "Dimension";

                //double unitCoefficient =Image.DimStyle().LinearScaleFactor;
                //var formatedString = (double value) =>
                //{
                //    string formatString = (Drawing.DisplayUnits == uppUnitFamilies.English) ? "0.000" : "0";
                //    double displayValue = value * unitCoefficient;
                //    return displayValue.ToString(formatString);
                //};

                //double halfBeamLength = beam.Length / 2;
                //double halfBeamWidth = beam.Width / 2;
                //double halfCenterLineLength = (beam.Length + 0.5 * paperScale) / 2;


                //dxfVector centerLineLeftPoint = new dxfVector(centerOfBeam.X - halfCenterLineLength, centerOfBeam.Y);

                dxePolygon beamPlanViewPolygon = beam.View_Plan(dxfVector.Zero, aLayerName: iBeamLayerName, aCenterLineLength: beam.Length + 0.25 * paperScale, suppressRotation: true);
                draw.aInsert(beamPlanViewPolygon, centerOfBeam, aBlockName: $"SUPPORT_BEAM_{beam.PartNumber}_PLAN");
                beamPlanViewPolygon.MoveTo(centerOfBeam);

                // ****************************** Acquire holes information ****************************************
                //var beamHoles = beam.GenHoles("BEAM").Item(1);

                //double holeRadius = beamHoles.Member.Diameter / 2;
                //double holeLength = beamHoles.Member.Length; // This is the whole length of the hole
                //double centersOffset = holeLength - beamHoles.Member.Diameter; // This is the distance between the centers of the two half circles of the hole
                //double halfCentersOffset = centersOffset / 2;

                //dxfVector holeCenter;
                //dxfVector holeCenterInWorld;
                //dxfVector topLeftHoleCenterInDrawing = null;
                //dxfVector bottomLeftHoleCenterInDrawing = null;
                //dxfVector bottomRightHoleCenterInDrawing = null;

                //foreach (var currentCenter in beamHoles.Centers)
                //{
                //    holeCenterInWorld = new dxfVector(currentCenter);
                //    holeCenter = holeCenterInWorld.WithRespectToPlane(beam.Plane); // This gives us the coordinates relative to the beam center as if the the beam was not rotated
                //    holeCenter.SetCoordinates(centerOfBeam.X + holeCenter.X, centerOfBeam.Y + holeCenter.Y);
                //    if (currentCenter.Flag.Contains("TOP"))
                //    {
                //        if (currentCenter.Flag.Contains("LEFT"))
                //        {
                //            topLeftHoleCenterInDrawing = holeCenter;
                //        }
                //    }
                //    else
                //    {
                //        // We expect it to contain "BOTTOM"
                //        if (currentCenter.Flag.Contains("LEFT"))
                //        {
                //            bottomLeftHoleCenterInDrawing = holeCenter;
                //        }
                //        else
                //        {
                //            // We expect it to contain "RIGHT"
                //            bottomRightHoleCenterInDrawing = holeCenter;
                //        }
                //    }
                //}

                // ****************************** Draw dimensions **************************************************

                dxeLine cl = beamPlanViewPolygon.AdditionalSegments.Lines().Find((x) => x.Tag == "CENTERLINE");
                dxfVector ordinateBasePoint = beamPlanViewPolygon.Vertices.GetVector(dxxPointFilters.GetBottomLeft); //  new dxfVector(centerOfBeam.X - halfBeamLength, centerOfBeam.Y - halfBeamWidth); // Bottom left corner of the beam

                //dxfVector ordinateDimensionPoint;
                colDXFVectors slotcenters = beamPlanViewPolygon.AdditionalSegments.DefinitionPoints(dxxEntDefPointTypes.HandlePt, aEntityType: dxxEntityTypes.Point);
                // Vertical ordinate dimensions
                List<dxfVector> ordinatePoints = new List<dxfVector>()
            {
            beamPlanViewPolygon.Vertices.GetVector(dxxPointFilters.GetTopLeft),
            slotcenters.GetVector(dxxPointFilters.GetTopLeft),
                cl.EndPoints().GetVector(dxxPointFilters.AtMinX),
               slotcenters.GetVector(dxxPointFilters.GetBottomLeft),
               ordinateBasePoint,
            };
                //ordinateDimensionPoint = beamPlanViewPolygon.Vertices.GetVector(dxxPointFilters.GetTopLeft); // new dxfVector(centerOfBeam.X - halfBeamLength, centerOfBeam.Y + halfBeamWidth); // Top left corner of the beam

                //ordinateDimensionPoint = slotcenters.GetVector(dxxPointFilters.GetTopLeft);//  topLeftHoleCenterInDrawing; // Top left hole center
                //ordinateDimensionPoint = slotcenters.GetVector(dxxPointFilters.GetBottomLeft);//  topLeftHoleCenterInDrawing; // Top left hole center
                //ordinateDimensionPoint = centerLineLeftPoint; // Left point of the beam red center line
                //ordinatePoints.Add(ordinateDimensionPoint);

                //ordinateDimensionPoint = bottomLeftHoleCenterInDrawing; // Bottom left hole center
                //ordinatePoints.Add(ordinateDimensionPoint);

                //ordinateDimensionPoint = ordinateBasePoint; // Bottom left corner of the beam which is the ordinate base point as well
                //ordinatePoints.Add(ordinateDimensionPoint);
                //ordinatePoints.Add(ordinateBasePoint);
                dims.Stack_Ordinate(dxxOrdinateDimTypes.OrdVertical, ordinateBasePoint, ordinatePoints, aDimOffset: -0.5 * paperScale, aInvertStack: true);
                ordinatePoints = new List<dxfVector>()
            {
                ordinateBasePoint,
                slotcenters.GetVector(dxxPointFilters.GetBottomLeft),
                slotcenters.GetVector(dxxPointFilters.GetBottomRight),
                beamPlanViewPolygon.Vertices.GetVector(dxxPointFilters.GetBottomRight)
            };
                //// Horizontal ordinate dimensions
                //ordinatePoints.Clear();
                ////ordinateDimensionPoint = ordinateBasePoint; // Bottom left corner of the beam which is the ordinate base point as well
                //ordinatePoints.Add(ordinateBasePoint);

                ////ordinateDimensionPoint = bottomLeftHoleCenterInDrawing; // Bottom left hole center
                //ordinatePoints.Add(slotcenters.GetVector(dxxPointFilters.GetBottomLeft));

                ////ordinateDimensionPoint = bottomRightHoleCenterInDrawing; // Bottom left hole center
                //ordinatePoints.Add(slotcenters.GetVector(dxxPointFilters.GetBottomRight));

                ////ordinateDimensionPoint = new dxfVector(centerOfBeam.X + halfBeamLength, centerOfBeam.Y - halfBeamWidth); // Bottom right corner of the beam
                //ordinatePoints.Add(beamPlanViewPolygon.Vertices.GetVector(dxxPointFilters.GetBottomRight));

                dims.Stack_Ordinate(dxxOrdinateDimTypes.OrdHorizontal, ordinateBasePoint, ordinatePoints, aDimOffset: -0.375 * paperScale);

                // Draw hole leader
                colDXFEntities slots = beamPlanViewPolygon.AdditionalSegments.GetByTag("BEAM", aEntityType: dxxEntityTypes.Polyline);

                dxfVector v1 = beamPlanViewPolygon.Vertices.GetVector(dxxPointFilters.GetTopRight);
                dxePolyline slot = (dxePolyline)slots.GetByNearestDefPoint(v1, dxxEntDefPointTypes.Center);
                //dxfVector leaderArrowPoint = slotcenters.GetVector(dxxPointFilters.GetTopRight); //  topLeftHoleCenterInDrawing + new dxfVector(halfCentersOffset, holeRadius); // Top right corner of the top left hole where arc starts
                dxfVector leaderTextPoint = beamPlanViewPolygon.Vertices.GetVector(dxxPointFilters.GetTopRight).Moved(0.25 * paperScale, 0.25 * paperScale);
                //            leaderTextPoint.MovePolar(65, beam.Width + 2);
                //leaderTextPoint.Project
                Tool.CreateHoleLeader(Image, slot, leaderTextPoint, dims, 4);

            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
            finally
            {
                Status = "";

            }

            return Image.Entities.SubSet(entmarker, Image.Entities.Count);
            //leaders.Text(new List<dxfVector>() { leaderArrowPoint }, leaderTextPoint, $"{formatedString(beamHoles.Member.Diameter)} X {formatedString(holeLength)} SLOT\r\n(4 PLACES)", aLayer: dimensionLayerName, aTextJustification: dxxVerticalJustifications.Center);
        }

        private colDXFEntities Draw_BeamDetail_ElevationView(dxfVector centerOfBeam, dxfRectangle drawingArea, mdBeam beam, double paperScale, dxoDrawingTool draw, dxoDimTool dims, dxoLeaderTool leaders)
        {
            string iBeamLayerName = "GEOMETRY";
            //string dimensionLayerName = "Dimension";

            int entmaker = Image.Entities.Count + 1;
            try
            {
                Status = "Drawing beam elevation view";
                dxoDimStyle dstyle = Image.DimStyle();
                dxfVector v1 = null;
                dxfVector v2 = null;
                string txt = string.Empty;
                //int intPaperScale = (int)paperScale;
                //double unitCoefficient =Image.DimStyle().LinearScaleFactor;
                //var formatedString = (double value, bool withUnit = false) =>
                //{
                //    string unit;
                //    string formatString;
                //    if (Drawing.DisplayUnits == uppUnitFamilies.English)
                //    {
                //        formatString = "0.000";
                //        unit = "\"";
                //    }
                //    else
                //    {
                //        formatString = "0";
                //        unit = "mm";

                //    }

                //    double displayValue = value * unitCoefficient;
                //    string result = withUnit ? $"{displayValue.ToString(formatString)}{unit}" : displayValue.ToString(formatString);
                //    return result;
                //};

                //double offsetForPlanView = drawingArea.TopLeft.Y - 20 - beam.Width - 50;
                //double halfBeamHeight = beam.Height / 2;
                //double halfBeamLength = beam.Length / 2;

                if (centerOfBeam == null)
                { centerOfBeam = new dxfVector(drawingArea.Center.X, (drawingArea.TopLeft.Y - 20 - beam.Width - 50) - beam.Height / 2); }
                else
                {
                    centerOfBeam = new dxfVector(centerOfBeam);
                    centerOfBeam.Y -= beam.Height / 2;
                }

                //double beamTopYOut = centerOfBeam.Y + beam.Height / 2;
                //double beamTopYIn = beamTopYOut - beam.FlangeThickness;
                //double beamBottomYOut = centerOfBeam.Y - beam.Height / 2;
                //double beamBottomYIn = beamBottomYOut + beam.FlangeThickness;
                //double beamLeftX = centerOfBeam.X - halfBeamLength;
                //double beamRightX = centerOfBeam.X + halfBeamLength;
                //double beamLeftWebInsetX = beamLeftX + beam.WebInset;
                //double beamRightWebInsetX = beamRightX - beam.WebInset;
                //double halfWebOpeningSize = beam.WebOpeningSize / 2;

                var beamElevationViewPolygon = beam.View_Elevation(aCenter: dxfVector.Zero, aLayerName: iBeamLayerName);

                //get the web opening centers and polylines
                var openingEnts = beamElevationViewPolygon.AdditionalSegments.FindAll((x) => string.Compare(x.Tag, "WEB OPENING", true) == 0); //points and polylines
                var openingPoints = openingEnts.FindAll((x) => x.EntityType == dxxEntityTypes.Point); //points and polylines
                beamElevationViewPolygon.AdditionalSegments.RemoveMembers(openingPoints);

                draw.aInsert(beamElevationViewPolygon, centerOfBeam, aBlockName: $"SUPPORT_BEAM_{beam.PartNumber}_ELEVATION");
                beamElevationViewPolygon.Translate(centerOfBeam);
                openingEnts.RemoveAll((x) => x.EntityType == dxxEntityTypes.Point);
                //dxfEntities.TranslateEntities(openingEnts, centerOfBeam);
                dxfEntities.TranslateEntities(openingPoints, centerOfBeam);
                colDXFVectors verts = beamElevationViewPolygon.Vertices;


                // ****************************** Draw dimensions ************************************
                //double cornerRadius = 30 / 25.4; // In inches
                //dxfVector ordinateBasePoint = verts.GetVector(dxxPointFilters.GetBottomLeft); // new dxfVector(beamLeftX, beamBottomYOut); // Bottom left corner of the beam
                //dxfVector ordinateDimensionPoint = null; // Top left corner of the beam
                dxePolyline opening = null;

                // Vertical ordinate dimensions
                dxfVector v0 = verts.GetVector(dxxPointFilters.GetBottomLeft);
                List<dxfVector> ordinatePoints = new List<dxfVector>()
            {
               v0,
                verts.GetVector(dxxPointFilters.GetTopLeft),
            };

                if (openingEnts.Any())
                {
                    opening = (dxePolyline)dxfEntities.GetByNearestDefPoint(openingEnts, v0, dxxEntDefPointTypes.Center);
                    ordinatePoints.Add(opening.GetVertex(dxxPointFilters.GetTopLeft));
                    ordinatePoints.Add(opening.GetVertex(dxxPointFilters.GetBottomLeft));
                }

                //IEnumerable<dxfVector> webOpeningCenters =  mdPolygons.FindWebOpeningCenters(beam.WebOpeningSize, beam.WebOpeningCount, beam.WebOpeningSize); // These coordinates are based on beam elevation plane in which the center of beam is at (0, 0)
                //webOpeningCenters = webOpeningCenters.Select(c => c + centerOfBeam).ToList();
                //if (webOpeningCenters.Any())
                //{
                //    dxfVector leftMostWebOpening = webOpeningCenters.First();

                //    double webOpeningOrdinatePointsX = leftMostWebOpening.X - halfWebOpeningSize + cornerRadius;

                //    ordinateDimensionPoint = new dxfVector(webOpeningOrdinatePointsX, centerOfBeam.Y + halfWebOpeningSize); // Top of web opening
                //    ordinatePoints.Add(ordinateDimensionPoint);

                //    ordinateDimensionPoint = new dxfVector(webOpeningOrdinatePointsX, centerOfBeam.Y - halfWebOpeningSize); // Bottom of web opening
                //    ordinatePoints.Add(ordinateDimensionPoint);
                //}

                //ordinateDimensionPoint = ordinateBasePoint; // Bottom left corner of the beam which is the ordinate base point as well
                //ordinatePoints.Add(ordinateDimensionPoint);

                dims.Stack_Ordinate(dxxOrdinateDimTypes.OrdVertical, ordinatePoints.First(), ordinatePoints, -0.5 * paperScale, aInvertStack: true);


                // Horizontal ordinate dimensions
                ordinatePoints = new List<dxfVector>()
            {
               v0,
               verts.GetTagged("WEB","BOTTOM LEFT"),
               verts.GetTagged("WEB","BOTTOM RIGHT"),
                verts.GetVector(dxxPointFilters.GetBottomRight),
            };

                //ordinatePoints.Clear();
                //ordinateDimensionPoint = ordinateBasePoint; // Bottom left corner of the beam which is the ordinate base point as well
                //ordinatePoints.Add(ordinateDimensionPoint);

                //ordinateDimensionPoint = new dxfVector(beamLeftX + beam.WebInset, beamBottomYIn); // Bottom of the left web inset line
                //ordinatePoints.Add(ordinateDimensionPoint);
                if (openingEnts.Any())
                {
                    openingEnts.OrderBy(x => x.X);
                    foreach (var item in openingEnts)
                    {
                        opening = (dxePolyline)item;
                        ordinatePoints.Add(opening.GetVertex(dxxPointFilters.GetLeftBottom));
                        ordinatePoints.Add(opening.GetVertex(dxxPointFilters.GetRightBottom));

                    }
                    opening = (dxePolyline)dxfEntities.GetByNearestDefPoint(openingEnts, beamElevationViewPolygon.InsertionPt, dxxEntDefPointTypes.Center);
                    //opening = (dxePolyline)openingEnts[openingEnts.Count - 3];
                    v1 = verts.Item(2).Moved(0, 0.375 * paperScale);
                    v1.X = opening.BoundingRectangle().Right + 0.375 * paperScale;
                    txt = Drawing.DrawingUnits == uppUnitFamilies.English ? dstyle.FormatNumber(beam.WebOpeningSize, aPrecision: -1) : dstyle.FormatNumber(beam.WebOpeningSize, aPrecision: 0);
                    txt = $"{txt} X {txt} HOLE\r\n({beam.WebOpeningCount} PLACES)";
                    Tool.CreateHoleLeader(Image, opening, v1, dims, openingEnts.Count, aOverideText: txt);



                    dxeArc radarc = (dxeArc)dxfEntities.GetByNearestDefPoint(opening.Segments.Arcs(), opening.BoundingRectangle().TopLeft);
                    dims.RadialR(radarc, 120, 0.525 * paperScale, aSuffix: "\\P(TYP)", aOverridePrecision: Drawing.DrawingUnits == uppUnitFamilies.Metric ? 0 : 3);

                    //string txt = Tool.HoleLeader(Image,opening,)
                    //Drawing.DrawingUnits == uppUnitFamilies.English ? dstyle.FormatNumber(beam.WebOpeningSize, aPrecision: -1) : dstyle.FormatNumber(beam.WebOpeningSize, aPrecision: 0);

                    //opening = (dxePolyline)dxfEntities.GetByNearestDefPoint(openingEnts, beamElevationViewPolygon.InsertionPt, dxxEntDefPointTypes.Center);

                    //    string txt = Drawing.DrawingUnits == uppUnitFamilies.English ? dstyle.FormatNumber(beam.WebOpeningSize, aPrecision: -1) : dstyle.FormatNumber(beam.WebOpeningSize, aPrecision: 0);

                    //    leaders.Text(webOpeningSpecLeaderPoint, webOpeningSpecLeaderTextPoint, $"{txt} X {txt} HOLE\r\n({beam.WebOpeningCount} PLACES)", aMidPt: webOpeningSpecLeaderMidPoint, aLayer: Image.DimSettings.DimLayer, aColor: Image.DimSettings.DimLayerColor);

                }

                //if (openingPoints.Count > 0)
                //{
                //    double webOpeningHorizontalOrdinatesY = centerOfBeam.Y - halfWebOpeningSize + cornerRadius;

                //    foreach (var webOpeningCenter in openingPoints)
                //    {
                //        ordinateDimensionPoint = new dxfVector(webOpeningCenter.X - halfWebOpeningSize, webOpeningCenter.Y - halfWebOpeningSize + cornerRadius); // For left edge of web opening
                //        ordinatePoints.Add(ordinateDimensionPoint);

                //        ordinateDimensionPoint = new dxfVector(webOpeningCenter.X + halfWebOpeningSize, webOpeningCenter.Y - halfWebOpeningSize + cornerRadius); // For right edge of web opening
                //        ordinatePoints.Add(ordinateDimensionPoint);
                //    }
                //}

                //ordinateDimensionPoint = new dxfVector(beamRightX - beam.WebInset, beamBottomYIn); // Bottom of the right web inset line
                //ordinatePoints.Add(ordinateDimensionPoint);

                //ordinateDimensionPoint = new dxfVector(beamRightX, beamBottomYOut); // Bottom right corner of the beam
                //ordinatePoints.Add(ordinateDimensionPoint);

                dims.Stack_Ordinate(dxxOrdinateDimTypes.OrdHorizontal, v0, ordinatePoints, -0.375 * paperScale);
                //return Image.Entities.SubSet(entmaker, Image.Entities.Count);


                // Draw part number text and leader
                double d1 = beam.Height - 2 * beam.FlangeThickness;
                v1 = verts.GetTagged("WEB", "BOTTOM LEFT").Moved(0, 0.5 * d1);
                dxeText etxt = Image.EntityTool.Create_Text(v1, "XXX", aTextHeight: 0.25 * d1, aAlignment: dxxMTextAlignments.MiddleCenter);

                etxt.Move(0.125 * paperScale + 0.5 * etxt.Width);
                Image.Entities.Add(etxt);

                //   var partNumberText = draw.aText(v1, "{\\Fromand;XXX}",  aTextHeight: 0.5, aAlignment: dxxMTextAlignments.MiddleCenter);

                v1 = etxt.BoundingRectangle().TopCenter;
                v2 = verts.GetVector(dxxPointFilters.GetTopLeft).Moved(0, 0.5 * paperScale);
                v2.X = v1.X + 0.5 * paperScale;
                leaders.Text(v1, v2, Tool.StampPNLeader(Image));

                //dxfVector partNumberLeaderPoint = new dxfVector(partNumberTextLocation.X, partNumberTextLocation.Y + partNumberText.Height / 2);
                //dxfVector partNumberLeaderTextPoint = new dxfVector(partNumberTextLocation.X + 20, partNumberTextLocation.Y + 20);
                //dxfVector partNumberLeaderMidPoint = new dxfVector(partNumberLeaderTextPoint.X - 5, partNumberLeaderTextPoint.Y);
                //  leaders.Text(partNumberLeaderPoint, partNumberLeaderTextPoint,  Tool.StampPNLeader(Image), partNumberLeaderMidPoint, aLayer: Image.DimSettings.DimLayer, aColor: dxxColors.Yellow); // The original number in this text was 13mm

                // Draw web opening corner radius dimension
                //int numberOfWebOpeningsInHalfOfBeam = beam.WebOpeningCount / 2;
                //int numberToSkip = numberOfWebOpeningsInHalfOfBeam - 2;
                ////if (openingPoints.Any() && numberToSkip >= 0)
                //{
                //    dxfVector centerOfWebOpeningForCornerRadiusDim = openingPoints.Skip(numberToSkip).First();

                //    dxeArc cornerRadiusArc = new dxeArc(new dxfVector(centerOfWebOpeningForCornerRadiusDim.X + halfWebOpeningSize - cornerRadius, centerOfWebOpeningForCornerRadiusDim.Y + halfWebOpeningSize - cornerRadius), cornerRadius);

                //    string txt = dstyle.FormatNumber(cornerRadius, aPrecision: Drawing.DrawingUnits == uppUnitFamilies.English ? -1 : 0);
                //    dims.RadialR(cornerRadiusArc,aPlacementAngle: 65, aPlacementDistance: 0.475 * paperScale, aOverideText: $"R{txt} (TYP)");
                //}

                // Draw web opening spec leader
                //bool odd = beam.WebOpeningCount % 2 == 1;
                //numberToSkip = numberOfWebOpeningsInHalfOfBeam + 1 + (odd ? 1 : 0);
                //if (openingPoints.Any() && numberToSkip >= 0)
                //{
                //    dxfVector centerOfWebOpeningForSpecLeader = openingPoints.Skip(numberToSkip).First();

                //    dxfVector webOpeningSpecLeaderPoint = new dxfVector(centerOfWebOpeningForSpecLeader.X + halfWebOpeningSize - cornerRadius, centerOfWebOpeningForSpecLeader.Y + halfWebOpeningSize);
                //    dxfVector webOpeningSpecLeaderTextPoint = new dxfVector(webOpeningSpecLeaderPoint.X + 20, webOpeningSpecLeaderPoint.Y + 15);
                //    dxfVector webOpeningSpecLeaderMidPoint = new dxfVector(webOpeningSpecLeaderTextPoint.X - 5, webOpeningSpecLeaderTextPoint.Y);
                //    string txt = Drawing.DrawingUnits == uppUnitFamilies.English ? dstyle.FormatNumber(beam.WebOpeningSize, aPrecision: -1) : dstyle.FormatNumber(beam.WebOpeningSize, aPrecision: 0);

                //    leaders.Text(webOpeningSpecLeaderPoint, webOpeningSpecLeaderTextPoint, $"{txt} X {txt} HOLE\r\n({beam.WebOpeningCount} PLACES)", aMidPt: webOpeningSpecLeaderMidPoint, aLayer: Image.DimSettings.DimLayer, aColor: Image.DimSettings.DimLayerColor);
                //}

                // ****************************** Draw section markers *******************************

                dxfRectangle rec = Image.Entities.SubSet(entmaker, Image.Entities.Count).BoundingRectangle();
                v1 = new dxfVector(rec.Right + 0.575 * paperScale, centerOfBeam.Y);
                d1 = beam.Height + 0.375 * paperScale;
                //dxfVector sectionMarkerCenter = new dxfVector(beamLeftX - 35, centerOfBeam.Y);
                draw.aSymbol.ViewArrow(v1, d1, 0.475, "A", 180, 0.1875, 0.0, dxxArrowStyles.Undefined, true, false, "SECTION_ARROW");

                // ****************************** Draw axis symbol ***********************************
                // rec = Image.Entities.SubSet(entmaker, Image.Entities.Count).BoundingRectangle();
                v1 = new dxfVector(rec.Left + 0.25 * paperScale, rec.Bottom - 0.25 * paperScale);
                //dxfVector axisLocation = new dxfVector(beamLeftX - 35, beamBottomYOut - 45);
                draw.aSymbol.Axis(v1, 0.5 * paperScale);
            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
            finally
            {
                Status = "";
            }


            return Image.Entities.SubSet(entmaker, Image.Entities.Count);

        }

        private colDXFEntities Draw_BeamDetail_ProfileView(dxfVector insertionPt, dxfRectangle drawingArea, mdBeam beam, double paperScale, dxoDrawingTool draw, dxoDimTool dims, dxoLeaderTool leaders)
        {
            // This method draws with 4x scale
            double scale = 4;
            int entmarker = Image.Entities.Count + 1;
            string iBeamLayerName = "GEOMETRY";

            try
            {

                Image.DimStyleOverrides.AutoReset = false;
                Image.DimStyleOverrides.LinearScaleFactor = Drawing.DrawingUnits == uppUnitFamilies.Metric ? (25.4 / scale) : scale;
                Status = "Drawing beam profile view";
                //double unitCoefficient = Image.DimStyle().LinearScaleFactor;
                //var formatedString = (double value) =>
                //{
                //    string formatString = (Drawing.DisplayUnits == uppUnitFamilies.English) ? "0.000" : "0";
                //    double displayValue = value * unitCoefficient;
                //    return displayValue.ToString(formatString);
                //};
                //var scaled = (double v) => scale * v;

                //double offsetForPlanView = drawingArea.TopLeft.Y - 20 - beam.Width - 50 - beam.Height - 60;

                //double scaledBeamHeight = scaled(beam.Height);
                //double scaledBeamWidth = scaled(beam.Width);
                //double halfBeamHeight = scaledBeamHeight / 2;
                //double halfBeamWidth = scaledBeamWidth / 2;
                //double scaledFlangeThickness = scaled(beam.FlangeThickness);
                //double scaledWebThickness = scaled(beam.WebThickness);

                if (insertionPt == null)
                {
                    insertionPt = new dxfVector(drawingArea.Center.X - beam.Length / 2 + beam.Width * scale * 0.5, (drawingArea.TopLeft.Y - 20 - beam.Width - 50 - beam.Height - 60) - (beam.Height * scale * 0.5));
                }
                else
                {
                    insertionPt = new dxfVector(insertionPt);

                }
                //double beamTopYOut = insertionPt.Y + halfBeamHeight;
                //double beamTopYIn = beamTopYOut - scaledFlangeThickness;
                //double beamBottomYOut = insertionPt.Y - halfBeamHeight;
                //double beamBottomYIn = beamBottomYOut + scaledFlangeThickness;
                //double beamLeftX = insertionPt.X - halfBeamWidth;
                //double beamRightX = insertionPt.X + halfBeamWidth;
                //double halfWebThinkness = scaledWebThickness / 2;
                //double leftWebX = insertionPt.X - halfWebThinkness;
                //double rightWebX = insertionPt.X + halfWebThinkness;

                var beamProfileViewPolygon = beam.View_Profile(dxfVector.Zero, iBeamLayerName, aScaleFactor: scale, aCenterlineScalefactor: 1.2);
                draw.aInsert(beamProfileViewPolygon, insertionPt, aScaleFactor: 1, aBlockName: $"SUPPORT_BEAM{beam.PartNumber}_PROFILE");

                beamProfileViewPolygon.MoveTo(insertionPt);
                colDXFVectors verts = beamProfileViewPolygon.Vertices;

                // ****************************** Draw dimensions *************************************



                dxfVector v1 = verts.GetVector(dxxPointFilters.GetTopLeft);
                dxfVector v2 = verts.Last();
                dxfVector v3 = verts.Item(verts.IndexOf(v2) - 3);
                dxfVector v4 = verts.GetVector(dxxPointFilters.GetBottomLeft);
                dxfVector v5 = verts.Item(verts.IndexOf(v4) + 2);
                dxfVector v6 = verts.Item(verts.IndexOf(v5) - 5);
                dxfVector v7 = verts.GetVector(dxxPointFilters.GetBottomRight);
                dxfVector v8 = verts.Item(verts.IndexOf(v6) - 1);

                dxeDimension d1 = dims.Vertical(v1, v2, -0.375); // Top flange thickness dimension
                dxeDimension d2 = dims.Vertical(v3, v4, -0.375); // Bottom flange thickness dimension
                dxeDimension d3 = dims.Vertical(v1, v4, -1); // Draw beam height dimension
                dxeDimension d4 = dims.Horizontal(v5, v6, v4.Y - 0.25 * paperScale, bAbsolutePlacement: true); // Draw web thickness
                dxeDimension d5 = dims.Horizontal(v4, v7, d4.BoundingRectangle().Bottom - 0.125 * paperScale, bAbsolutePlacement: true); // Draw beam width dimension

                // ****************************** Draw weld symbol ************************************
                dxfVector v9 = new dxfVector(v8.X + 0.25 * beam.Width * scale, insertionPt.Y);
                dxeSymbol weldsy = Image.SymbolTool.Weld(v9, dxxWeldTypes.Fillet, bBothSides: true, bSuppressTail: true);
                leaders.NoRef(v6, v9);
                leaders.NoRef(v8, v9);

                colDXFEntities ents = Image.Entities.SubSet(entmarker, null);

                v1 = new dxfVector(insertionPt.X, ents.BoundingRectangle().Bottom - 0.125 * paperScale);
                // ****************************** Draw view text **************************************

                Image.Draw.aText(v1, $"\\LVIEW A-A\\l\\P\\H{paperScale * 0.125};SCALE: {scale}x", 0.1875 * paperScale, dxxMTextAlignments.TopCenter, aTextStyle: "Standard");
                // string dimensionText = $"{formatedString(beam.FlangeThickness)}";
                //// Draw flange thickness dimensions
                //dxfVector dimensionTopPoint =  new dxfVector(beamLeftX, beamTopYOut);
                //dxfVector dimensionBottomPoint = new dxfVector(beamLeftX, beamTopYIn);

                //dimensionTopPoint = new dxfVector(beamLeftX, beamBottomYIn);
                //dimensionBottomPoint = new dxfVector(beamLeftX, beamBottomYOut);
                //dims.Vertical(dimensionTopPoint, dimensionBottomPoint, -0.5, aOverideText: dimensionText); // Bottom flange thickness dimension

                //// Draw beam height dimension
                //dimensionText = $"{formatedString(beam.Height)}";
                //dimensionTopPoint = new dxfVector(beamLeftX, beamTopYOut);
                //dimensionBottomPoint = new dxfVector(beamLeftX, beamBottomYOut);
                //dims.Vertical(dimensionTopPoint, dimensionBottomPoint, -1.3, aOverideText: dimensionText);

                //// Draw web thickness dimension
                //dimensionText = $"{formatedString(beam.WebThickness)}";
                //dxfVector dimensionLeftPoint = new dxfVector(leftWebX, beamBottomYIn);
                //dxfVector dimensionRightPoint = new dxfVector(rightWebX, beamBottomYIn);
                //dims.Horizontal(dimensionLeftPoint, dimensionRightPoint, -0.45, aOverideText: dimensionText);

                //// Draw beam width dimension
                //dimensionText = $"{formatedString(beam.Width)}";
                //dimensionLeftPoint = new dxfVector(beamLeftX, beamBottomYOut);
                //dimensionRightPoint = new dxfVector(beamRightX, beamBottomYOut);
                //dims.Horizontal(dimensionLeftPoint, dimensionRightPoint, -0.5, aOverideText: dimensionText);

                //// ****************************** Draw view text **************************************
                //string viewText = @"{\Fromand;\LVIEW A-A\L}";
                //dxfVector viewTextLocation = new dxfVector(insertionPt.X, insertionPt.Y - halfBeamHeight - 25);
                //var viewTextObj = draw.aText(viewTextLocation, viewText, aAlignment: dxxMTextAlignments.MiddleCenter);

                //viewText = $"SCALE {scale:0}x";
                //viewTextLocation = new dxfVector(viewTextLocation.X, viewTextLocation.Y - viewTextObj.TextHeight - 3);
                //draw.aText(viewTextLocation, viewText, aAlignment: dxxMTextAlignments.MiddleCenter);

                //// ****************************** Draw weld symbol ************************************
                //dxfDisplaySettings weldSymbolDisplaySetting = dxfDisplaySettings.Null(aLayer: Image.DimSettings.DimLayer, aColor: Image.DimSettings.DimLayerColor, aLinetype: dxfLinetypes.Continuous, aLTScale: paperScale);
                //dxfVector intersectionPoint = new dxfVector(beamRightX, insertionPt.Y);

                //dxfVector webFlangeCornerTop = new dxfVector(rightWebX, beamTopYIn); // Top intersection of flange thickness and right web line
                //dxeLeader leaderObj = leaders.NoRef(webFlangeCornerTop, intersectionPoint);
                //leaderObj.LayerName = Image.DimSettings.DimLayer;

                //dxfVector webFlangeCornerBottom = new dxfVector(rightWebX, beamBottomYIn); // bottom intersection of flange thickness and right web line
                //leaderObj = leaders.NoRef(webFlangeCornerBottom, intersectionPoint);
                //leaderObj.LayerName = Image.DimSettings.DimLayer;

                //dxfVector endOfWeldSymbolHorizontalLine = new dxfVector(intersectionPoint.X + 21.66, intersectionPoint.Y);
                //draw.aLine(intersectionPoint, endOfWeldSymbolHorizontalLine, weldSymbolDisplaySetting); // Weld symbol horizontal line

                //dxfVector triangleBaseSideLeftPoint = new dxfVector(intersectionPoint.X + 7.08, intersectionPoint.Y);
                //dxfVector triangleBaseSideRightPoint = new dxfVector(triangleBaseSideLeftPoint.X + 7.5, intersectionPoint.Y);
                //dxfVector topTriangleThirdPoint = new dxfVector(triangleBaseSideLeftPoint.X, triangleBaseSideLeftPoint.Y + 7.5); // For the perpendicular side of top triangle
                //dxfVector bottomTriangleThirdPoint = new dxfVector(triangleBaseSideLeftPoint.X, triangleBaseSideLeftPoint.Y - 7.5); // For the perpendicular side of bottom triangle

                //draw.aPolyline(new colDXFVectors() { triangleBaseSideLeftPoint, triangleBaseSideRightPoint, topTriangleThirdPoint }, true, weldSymbolDisplaySetting); // Top triangle
                //draw.aPolyline(new colDXFVectors() { triangleBaseSideLeftPoint, triangleBaseSideRightPoint, bottomTriangleThirdPoint }, true, weldSymbolDisplaySetting); // Bottom triangle

            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
            finally
            {
                Status = "";
                Image.DimStyleOverrides.Reset();
            }

            //string dimensionLayerName = "Dimension";
            return Image.Entities.SubSet(entmarker, Image.Entities.Count);


        }

        private colDXFEntities Draw_Beam_Text(dxfVector insertionPt, dxfRectangle drawingArea, mdBeam beam, double paperScale, dxoDrawingTool draw)
        {
            int entmarker = Image.Entities.Count + 1;
            colDXFEntities _rVal = new colDXFEntities();
            try
            {
                dxoDimStyle dStyle = Image.DimStyle();

                uppUnitFamilies units = Drawing.DrawingUnits;
                double unitCoefficient = Image.DimStyle().LinearScaleFactor;
                var formatedString = (double value, bool withUnit = false) =>
                {
                    string unit;
                    string formatString;
                    if (Drawing.DrawingUnits == uppUnitFamilies.English)
                    {
                        formatString = "0.000";
                        unit = "\"";
                    }
                    else
                    {
                        formatString = "0";
                        unit = "mm";

                    }

                    double displayValue = value * unitCoefficient;
                    string result = withUnit ? $"{displayValue.ToString(formatString)}{unit}" : displayValue.ToString(formatString);
                    return result;
                };


                // The original numbers in this text were: 4, 2, and 5 mm.
                double precamber = units == uppUnitFamilies.Metric ? 4 / 25.4 : 3 / 16;
                double precambertol = units == uppUnitFamilies.Metric ? 2 / 25.4 : 1 / 16;
                double outofsquare = units == uppUnitFamilies.Metric ? 5 / 25.4 : 4 / 16;
                string unitsuf = units == uppUnitFamilies.Metric ? "mm" : "\"";
                int unitprecis = units == uppUnitFamilies.Metric ? 0 : 3;
                string noteText = $@"NOTE:
THE AMOUNT OF PRE-CAMBER (UPWARD) AT THE
MID-POINT OF THE BEAM SHALL BE {dStyle.FormatNumber(precamber, aPrecision: unitprecis)} %%p{dStyle.FormatNumber(precambertol, aPrecision: unitprecis, aSuffix: unitsuf)}.
THE CAMBERING SHALL BE WITH SMOOTH
CURVATURE FROM BOTH ENDS TOWARDS THE
MID-POINT.
THE MAXIMUM OUT OF SQUARE SHALL NOT
EXCEED {dStyle.FormatNumber(outofsquare, aPrecision: unitprecis)}.";
                // dxfVector noteTextLocation = new dxfVector(drawingArea.Center.X - 75, drawingArea.BottomEdge.StartPt.Y + 35);
                draw.aText(insertionPt, noteText,  aTextHeight:0.125 * paperScale, aAlignment: dxxMTextAlignments.TopLeft);

                string beamInformationText = $@"I-BEAM ({formatedString(beam.Height)}H x {formatedString(beam.Width)}W)";
                //PART NUMBER: {beam.PartNumber}
                //NUMBER REQUIRED: {beam.OccuranceFactor}
                //FOR MATERIAL SEE SHEET 2
                //FOR TRAYS { beam.RangeSpanNames(Project)}";
                // The beamInformationTextX calculation is based on our current settings (Imperial paperscale = 1:32, Metric paperscale = 1:30)
                // If these scales change, it needs to be adjusted so that the text gets aligned with the left edge of the information table at bottom right portion of the border
                //double beamInformationTextX = drawingArea.Center.X + (paperScale) * 3.77875 - 9.6;
                //dxfVector beamInformationTextLocation = new dxfVector(beamInformationTextX, drawingArea.BottomRight.Y + 35);
                // draw.aText(insertionPt, beamInformationText, aAlignment: dxxMTextAlignments.TopLeft);
                _rVal = Image.Entities.SubSet(entmarker, Image.Entities.Count);
                Tool.AddBorderNotes(beam, Project, beamInformationText, false, Image, drawingArea, paperScale);

            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
            finally
            {
                Status = "";
                Image.DimStyleOverrides.Reset();
            }
            return _rVal;

        }



        private (dxfVector, string) GetTopEndAngleBubblePointAndPartNumber(mdDowncomer downcomer, colMDDowncomers allDowncomers)
        {
            bool topMostBoxIsVirtual = IsTopMostBoxVirtual(downcomer);

            if (topMostBoxIsVirtual)
            {
                mdDowncomer correspondingDowncomer = FindCorrespondingDowncomer(downcomer, allDowncomers);

                List<mdEndAngle> downcomerEndAngles = mdPartGenerator.EndAngles_ASSY(Assy, bApplyInstances: false, correspondingDowncomer.Index);

                if (downcomer.X >= 0)
                {
                    // Find the bottom left end angle of corresponding box
                    var bottomLeftEndAngles = downcomerEndAngles.Where(ea => ea.End == uppSides.Bottom && ea.Side == uppSides.Left);
                    var bottomLeftEndAngle = bottomLeftEndAngles.OrderBy(ea => ea.Y).FirstOrDefault();
                    if (bottomLeftEndAngle == null)
                    {
                        return (null, null);
                    }

                    return (new dxfVector(-bottomLeftEndAngle.X, -bottomLeftEndAngle.Y), bottomLeftEndAngle.PartNumber);
                }
                else
                {
                    // Find the bottom right end angle of corresponding box
                    var bottomRightEndAngles = downcomerEndAngles.Where(ea => ea.End == uppSides.Bottom && ea.Side == uppSides.Right);
                    var bottomRightEndAngle = bottomRightEndAngles.OrderBy(ea => ea.Y).FirstOrDefault();
                    if (bottomRightEndAngle == null)
                    {
                        return (null, null);
                    }

                    return (new dxfVector(-bottomRightEndAngle.X, -bottomRightEndAngle.Y), bottomRightEndAngle.PartNumber);
                }
            }
            else
            {
                List<mdEndAngle> downcomerEndAngles = mdPartGenerator.EndAngles_ASSY(Assy, bApplyInstances: false, downcomer.Index);

                if (downcomer.X >= 0)
                {
                    // Find the top right end angle
                    var topRightEndAngles = downcomerEndAngles.Where(ea => ea.End == uppSides.Top && ea.Side == uppSides.Right);
                    var topRightEndAngle = topRightEndAngles.OrderByDescending(ea => ea.Y).FirstOrDefault();
                    if (topRightEndAngle == null)
                    {
                        return (null, null);
                    }

                    return (new dxfVector(topRightEndAngle.X, topRightEndAngle.Y), topRightEndAngle.PartNumber);
                }
                else
                {
                    // Find the top left end angle
                    var topLeftEndAngles = downcomerEndAngles.Where(ea => ea.End == uppSides.Top && ea.Side == uppSides.Left);
                    var topLeftEndAngle = topLeftEndAngles.OrderByDescending(ea => ea.Y).FirstOrDefault();
                    if (topLeftEndAngle == null)
                    {
                        return (null, null);
                    }

                    return (new dxfVector(topLeftEndAngle.X, topLeftEndAngle.Y), topLeftEndAngle.PartNumber);
                }
            }
        }

        private (dxfVector, string[]) GetTopEndSupportBubblePointAndDCBPartNumbers(mdDowncomer downcomer, colMDDowncomers allDowncomers)
        {
            dxfVector bubblePoint;
            mdEndSupport endSupport;
            uopHoles ringCliphole;
            mdDowncomerBox box;
            bool isTopMostBoxVirtual = IsTopMostBoxVirtual(downcomer);
            
            if (isTopMostBoxVirtual)
            {
                mdDowncomer correspondingDowncomer = FindCorrespondingDowncomer(downcomer, allDowncomers);
                box = FindBottomMostBox(correspondingDowncomer);

                endSupport = box.EndSupport(bTop: false);
                ringCliphole = endSupport.GenHoles("RING CLIP").Item(1);

                bubblePoint = new dxfVector(-ringCliphole.Centers.First.X, -ringCliphole.Centers.First.Y);
            }
            else
            {
                box = FindTopMostBox(downcomer);

                endSupport = box.EndSupport(bTop: true);
                ringCliphole = endSupport.GenHoles("RING CLIP").Item(1);

                bubblePoint = new dxfVector(ringCliphole.Centers.First.X, ringCliphole.Centers.First.Y);
            }

            string[] downcomerBoxPartNumbers = GetDowncomerBoxPartNumbersForDowncomer(downcomer, allDowncomers).ToArray();

            return (bubblePoint, downcomerBoxPartNumbers);
        }

        private List<string> GetDowncomerBoxPartNumbersForDowncomer(mdDowncomer downcomer, colMDDowncomers allDowncomers)
        {
            var boxes = FindAllNonVirtualDowncomerBoxesForDowncomer(downcomer, allDowncomers);
            var partNumbers = boxes.Select(b => b.PartNumber).ToList();
            return partNumbers;
        }

        private List<mdDowncomerBox> FindAllNonVirtualDowncomerBoxesForDowncomer(mdDowncomer downcomer, colMDDowncomers allDowncomers)
        {
            // Each downcomer can comprise of virtual and non-virtual boxes. With the current architecture, we do not keep an instance for the virtual boxes in beam design
            // So, if the box is virtual its non-virtual version belongs to another downcomer.
            // This method finds all the non-virtual downcomer boxes whether they belong to the downcomer or belong to another downcomer.

            var assembly = downcomer.GetMDTrayAssembly();
            bool isBeamDesign = assembly.DesignFamily.IsBeamDesignFamily();

            var correspondingDowncomer = FindCorrespondingDowncomer(downcomer, allDowncomers);

            List<mdDowncomerBox> boxes;

            if (isBeamDesign)
            {
                int maxRow = assembly.Beam.Offset > 0 ? 3 : 2;
                var downcomerInfo = downcomer.CurrentInfo();
                var correspondingDowncomerInfo = correspondingDowncomer.CurrentInfo();
                var boxLines = downcomerInfo.BoxLines;

                boxes = new List<mdDowncomerBox>();
                mdDowncomerBox box;

                foreach (var boxLinePair in boxLines)
                {
                    if (boxLinePair.IsVirtual)
                    {
                        box = correspondingDowncomer.Boxes.First(b => b.Row == (maxRow - boxLinePair.Row + 1));
                    }
                    else
                    {
                        box = downcomer.Boxes.First(b => b.Row == boxLinePair.Row);
                    }

                    boxes.Add(box);
                }
            }
            else
            {
                if (downcomer.IsVirtual)
                {
                    boxes = correspondingDowncomer.Boxes.Reverse<mdDowncomerBox>().ToList();
                }
                else
                {
                    boxes = downcomer.Boxes.ToList();
                }
            }

            return boxes;
        }

        private (dxfVector, string) GetBottomEndAngleBubblePointAndPartNumber(mdDowncomer downcomer, colMDDowncomers allDowncomers)
        {
            bool isBottomMostBoxVirtual = IsBottomMostBoxVirtual(downcomer);
            
            if (isBottomMostBoxVirtual)
            {
                mdDowncomer correspondingDowncomer = FindCorrespondingDowncomer(downcomer, allDowncomers);

                List<mdEndAngle> downcomerEndAngles = mdPartGenerator.EndAngles_ASSY(Assy, bApplyInstances: false, correspondingDowncomer.Index);

                if (downcomer.X >= 0)
                {
                    // Find the top right end angle of corresponding box
                    var topRightEndAngles = downcomerEndAngles.Where(ea => ea.End == uppSides.Top && ea.Side == uppSides.Right);
                    var topRightEndAngle = topRightEndAngles.OrderByDescending(ea => ea.Y).FirstOrDefault();
                    if (topRightEndAngle == null)
                    {
                        return (null, null);
                    }

                    return (new dxfVector(-topRightEndAngle.X, -topRightEndAngle.Y), topRightEndAngle.PartNumber);
                }
                else
                {
                    // Find the top left end angle of corresponding box
                    var topLeftEndAngles = downcomerEndAngles.Where(ea => ea.End == uppSides.Top && ea.Side == uppSides.Left);
                    var topLeftEndAngle = topLeftEndAngles.OrderByDescending(ea => ea.Y).FirstOrDefault();
                    if (topLeftEndAngle == null)
                    {
                        return (null, null);
                    }

                    return (new dxfVector(-topLeftEndAngle.X, -topLeftEndAngle.Y), topLeftEndAngle.PartNumber);
                }
            }
            else
            {
                List<mdEndAngle> downcomerEndAngles = mdPartGenerator.EndAngles_ASSY(Assy, bApplyInstances: false, downcomer.Index);

                if (downcomer.X >= 0)
                {
                    // Find the bottom left end angle
                    var bottomLeftEndAngles = downcomerEndAngles.Where(ea => ea.End == uppSides.Bottom && ea.Side == uppSides.Left);
                    var bottomLeftEndAngle = bottomLeftEndAngles.OrderBy(ea => ea.Y).FirstOrDefault();
                    if (bottomLeftEndAngle == null)
                    {
                        return (null, null);
                    }

                    return (new dxfVector(bottomLeftEndAngle.X, bottomLeftEndAngle.Y), bottomLeftEndAngle.PartNumber);
                }
                else
                {
                    // Find the bottom right end angle
                    var bottomRightEndAngles = downcomerEndAngles.Where(ea => ea.End == uppSides.Bottom && ea.Side == uppSides.Right);
                    var bottomRightEndAngle = bottomRightEndAngles.OrderBy(ea => ea.Y).FirstOrDefault();
                    if (bottomRightEndAngle == null)
                    {
                        return (null, null);
                    }

                    return (new dxfVector(bottomRightEndAngle.X, bottomRightEndAngle.Y), bottomRightEndAngle.PartNumber);
                }
            }
        }

        /// <summary>
        /// This method returns the corresponding downcomer whose non-virtual box is identical to the downcomer box's virtual box
        /// </summary>
        private mdDowncomer FindCorrespondingDowncomer(mdDowncomer downcomer, colMDDowncomers allDowncomers)
        {
            int correspondingDowncomerIndex = allDowncomers.Count - downcomer.Index + 1;
            mdDowncomer correspondingDowncomer = allDowncomers.Item(correspondingDowncomerIndex);
            return correspondingDowncomer;
        }

        private mdDowncomerBox FindTopMostBox(mdDowncomer downcomer)
        {
            mdDowncomerBox topMostBox = downcomer.Boxes.FirstOrDefault();
            if (topMostBox == null)
            {
                return null; // The downcomer has no boxes
            }

            foreach (var box in downcomer.Boxes)
            {
                if (box.Y > topMostBox.Y)
                {
                    topMostBox = box;
                }
            }

            return topMostBox;
        }

        private bool IsTopMostBoxVirtual(mdDowncomer downcomer)
        {
            if (downcomer.IsVirtual || downcomer.Boxes.Count == 0)
            {
                return true;
            }

            var dcInfo = downcomer.CurrentInfo();
            var topMostBoxLines = dcInfo.BoxLines.First();
            return topMostBoxLines.IsVirtual;
        }

        private bool IsBottomMostBoxVirtual(mdDowncomer downcomer)
        {
            if (downcomer.IsVirtual || downcomer.Boxes.Count == 0)
            {
                return true;
            }

            var dcInfo = downcomer.CurrentInfo();
            var bottomMostBoxLines = dcInfo.BoxLines.Last();
            return bottomMostBoxLines.IsVirtual;
        }

        private mdDowncomerBox FindBottomMostBox(mdDowncomer downcomer)
        {
            mdDowncomerBox bottomMostBox = downcomer.Boxes.FirstOrDefault();
            if (bottomMostBox == null)
            {
                return null; // The downcomer has no boxes
            }

            foreach (var box in downcomer.Boxes)
            {
                if (box.Y < bottomMostBox.Y)
                {
                    bottomMostBox = box;
                }
            }

            return bottomMostBox;
        }
    }
}
