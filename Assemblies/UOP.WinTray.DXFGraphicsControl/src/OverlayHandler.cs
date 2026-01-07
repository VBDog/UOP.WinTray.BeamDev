using UOP.DXFGraphicsControl.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WW.Cad.Base;
using WW.Cad.Drawing;
using WW.Cad.Drawing.Wpf;
using WW.Cad.Model;
using WW.Cad.Model.Entities;
using WW.Cad.Model.Tables;
using WW.Math;
using UOP.DXFGraphics;
using System.Diagnostics.Metrics;

namespace UOP.DXFGraphicsControl
{
    public class OverlayHandler :IDisposable
    {
        private readonly double baseDPI = 100d;
        private double scale = 1;
        private readonly List<ulong> doNotScale = new List<ulong>();
        public OverlayHandler(DXFViewer viewer, Matrix4D cTransform )
        {
            Init( viewer, null, cTransform );
        }
        public OverlayHandler( DXFViewer viewer, GraphicsConfig gConfig, Matrix4D cTransform )
        {
            Init( viewer, gConfig, cTransform );
        }

        private void Init( DXFViewer viewer, GraphicsConfig gConfig, Matrix4D cTransform )
        {
            this.viewer = viewer;
            modelwh = (gConfig == null) ? viewer.CADModels.staticOverlay : viewer.CADModels.overlay;
            model.Header.CurrentEntityLineWeight = 25;
            model.Header.ShowModelSpace = true;
            SupressRefresh = false;

            if (transformUtil == null || transformUtil.isDisposed)
                transformUtil = new TransformUtil( viewer, modelwh );

            if (gConfig == null)
            {
                isStatic = true;
                overlaycanvas = viewer.staticoverlaycanvas;
                graphicsConfig = (GraphicsConfig)GraphicsConfig.WhiteBackgroundCorrectForBackColor.Clone();

                BoundsCalculator boundsCalculator = new BoundsCalculator();
                boundsCalculator.GetBounds( model );
                bounds = viewer.CheckBounds(boundsCalculator.Bounds);
                Vector3D delta = bounds.Delta;
                Size estimatedCanvasSize = new Size( 500d, 500d );
                double estimatedScale = Math.Min( estimatedCanvasSize.Width / delta.X, estimatedCanvasSize.Height / delta.Y );
                double penWidthCorrectionFactor = estimatedScale;
                graphicsConfig.DotsPerInch = baseDPI;
                //graphicsConfig.DisplayLineWeight = false;
            }
            else
            {
                overlaycanvas = viewer.overlaycanvas;
                isStatic = false;
                graphicsConfig = gConfig;
                //graphicsConfig.DisplayLineWeight = false;
                centerTransform = cTransform;
            }

            overlayGraphicsCache = new WireframeGraphics2Cache(true, true)
            {
                Config = graphicsConfig
            };

            if (isStatic)
            {
                centerTransform = Transformation4D.Translation( Point3D.Zero - bounds.Center );
                overlayGraphicsCache.CreateDrawables( model, centerTransform );
                bounds.Transform( centerTransform );
            }

            overlayGraphicsCache.CreateDrawables( model, centerTransform );

            overlayWpfGraphics = new WpfWireframeGraphics3DUsingDrawingVisual
            {
                Config = graphicsConfig
            };
            overlayWpfGraphics.EnableDrawablesUpdate();

            overlayVisualContainer = new WpfWireframeGraphics3DUsingDrawingVisual.VisualContainer();
            overlaycanvas.Children.Add( overlayVisualContainer );

            UpdateWpfGraphics();
        }

        private DXFViewer viewer { get; set; }
        public ModelWithHandles modelwh { get; set; }
        private DxfModel model { get { return modelwh?.Model; } }
        private TransformUtil transformUtil { get; set; }
        private WpfWireframeGraphics3DUsingDrawingVisual overlayWpfGraphics;
        private WpfWireframeGraphics3DUsingDrawingVisual.VisualContainer overlayVisualContainer;
        private WireframeGraphics2Cache overlayGraphicsCache;
        private Canvas overlaycanvas { get; set; }
        public GraphicsConfig graphicsConfig { get; set; }
        public Matrix4D centerTransform { get; set; }

        private Bounds3D bounds;
        public double scaling = 1d;
        private Vector3D translation = new Vector3D();
        private bool disposedValue;

        private bool isStatic { get; set; }

        public bool SupressRefresh { get; set; }

        public void Clear()
        {
            transformUtil?.Dispose();
            overlaycanvas?.Children.Clear();
        }

        private void RefreshDisplay( DxfEntity ent )
        {
            if (SupressRefresh) return;

            overlayGraphicsCache.AddDrawables( model, new List<DxfEntity>() { ent }, Matrix4D.Identity );

            UpdateWpfGraphics();
        }

        private void RefreshDisplay( IEnumerable<DxfEntity> ents )
        {
            if (SupressRefresh) return;
            if (model != null && ents != null)
            {
                try { overlayGraphicsCache.AddDrawables(model, ents.ToList(), Matrix4D.Identity); } catch { }

            }


            try
            {
                UpdateWpfGraphics();
            }
            catch { }
        }

        public void RefreshDisplay()
        {
            if (SupressRefresh) return;

            //if (viewer.oImage != null)
            //{
                ShowUCS();
            //}
            try
            {
                overlayGraphicsCache?.Clear();
            }
            catch { }

            if(model != null)
            {
                
                try { DxfEntityCollection ents = model.Entities; if (ents != null && overlayGraphicsCache != null) overlayGraphicsCache.AddDrawables(model, ents, Matrix4D.Identity); } catch { }

            }

            try
            {
                UpdateWpfGraphics();
            }
            catch { }

            UpdateRenderTransform();
        }

        public void RefreshDisplay( ulong handle )
        {
            RefreshDisplay( modelwh.GetEntity( handle ) );
        }

        public void UpdateWpfGraphics()
        {
            overlayWpfGraphics.Clear();
            IWireframeGraphicsFactory2 graphicsFactory = overlayWpfGraphics.CreateGraphicsFactory( null );
            foreach (IWireframeDrawable2 drawable in overlayGraphicsCache.Drawables)
            {
                drawable.Draw( graphicsFactory );
            }
            UpdateWpfVisuals();
        }

        private void UpdateWpfVisuals()
        {
            try
            {
                overlayVisualContainer.Visuals.Clear();
                
            }
            catch (Exception) { }
            try
            {
      
                overlayWpfGraphics.Draw(overlayVisualContainer.Visuals);
            }
            catch (Exception) { }
        }

        public void AddEntities( List<UOP.DXFGraphics.dxfEntity> Entities )
        {
            foreach (var ent in Entities)
            {
                RefreshDisplay( transformUtil.TransformToDXF( ent ) );
            }
        }

        public void AddEntities( List<DxfEntity> Entities )
        {
            if (model == null || Entities == null) return;
            if (model.Entities == null) return;
            foreach (var ent in Entities)
            {
                model.Entities.Add( ent );
            }
            UpdateRenderTransform();
        }

        public void UpdateRenderTransform()
        {
            if (!isStatic || viewer == null) return;

            BoundsCalculator boundsCalculator = new BoundsCalculator();
            boundsCalculator.GetBounds( model );
            bounds = viewer.CheckBounds( boundsCalculator.Bounds );

            double canvasWidth = overlaycanvas.ActualWidth;
            double canvasHeight = overlaycanvas.ActualHeight;

            // Update the WPF graphics from the cache to fix the displayed line width (only needed for model space).
            if (model != null && (model.ActiveLayout == null || model.Header.ShowModelSpace))
            {
                Vector3D delta = bounds.Delta;
                double scale = Math.Min( canvasWidth / delta.X, canvasHeight / delta.Y );
                graphicsConfig.DotsPerInch = baseDPI;

                // In model space line weight is independent of zoom level.
                graphicsConfig.DotsPerInch /= scaling;

                UpdateWpfGraphics();
            }

            MatrixTransform baseTransform = DxfUtil.GetScaleWMMatrixTransform(
                new Point2D(1d, 1d),
                new Point2D(canvasWidth, canvasHeight),
                new Point2D(0.5d * (canvasWidth + 1d), 0.5d * (canvasHeight + 1d)),
                new Point2D(1d, canvasHeight),
                new Point2D(canvasWidth, 1d),
                new Point2D(0.5d * (canvasWidth + 1d), 0.5d * (canvasHeight + 1d))
                );

            TransformGroup transformGroup = new TransformGroup();
            transformGroup.Children.Add(baseTransform);
            transformGroup.Children.Add(new TranslateTransform()
            {
                X = -canvasWidth / 2d,
                Y = -canvasHeight / 2d
            });
            transformGroup.Children.Add(new ScaleTransform()
            {
                ScaleX = scaling,
                ScaleY = scaling
            });
            transformGroup.Children.Add(new TranslateTransform()
            {
                X = canvasWidth / 2d,
                Y = canvasHeight / 2d
            });
            transformGroup.Children.Add(new TranslateTransform()
            {
                X = translation.X * canvasWidth / 2d,
                Y = -translation.Y * canvasHeight / 2d
            });

            overlaycanvas.RenderTransform = transformGroup;
        }

        public void AdjustScale()
        {
            AdjustScale( 1 / scale );
        }

        public void AdjustDPI()
        {
            if (isStatic)
            {
                graphicsConfig.DotsPerInch = baseDPI;
                RefreshDisplay();
            }
            if(viewer != null)
            {
                double canvasWidth = overlaycanvas.ActualWidth;
                double canvasHeight = overlaycanvas.ActualHeight;
                Point3D origin = new Point3D(0.0, 0.0, 0.0);
                Point3D top = new Point3D(0.0, -canvasHeight, 0.0);
                Point3D right = new Point3D(canvasWidth, 0.0, 0.0);

                var bpt = viewer.projectionTransform.GetInverse().Transform(origin);
                var tpt = viewer.projectionTransform.GetInverse().Transform(top);
                var rpt = viewer.projectionTransform.GetInverse().Transform(right);
                double deltaY = Math.Abs(tpt.Y - bpt.Y);
                double deltaX = Math.Abs(rpt.X - bpt.X);

                double estimatedScale = Math.Min(canvasWidth / deltaX, canvasHeight / deltaY);
                double penWidthCorrectionFactor = estimatedScale;
                graphicsConfig.DotsPerInch = baseDPI / penWidthCorrectionFactor;
            }
            

            RefreshDisplay();
        }

        public void AdjustScale(double scale)
        {
            if (isStatic || model == null) return;
            this.scale *= scale;
            var tconfig = new TransformConfig();
            foreach (var ent in model.Entities)
            {
                if (doNotScale.Contains( ent.Handle )) continue;
                if (ent is DxfInsert)
                {
                    var trans = Transformation4D.Scaling( scale, ((DxfInsert)ent).InsertionPoint );
                    ent.TransformMe( tconfig, trans );
                }
                else
                {
                    BoundsCalculator boundsCalculator = new BoundsCalculator();
                    boundsCalculator.GetBounds( model, ent );
                    var ebounds = viewer.CheckBounds( boundsCalculator.Bounds );
                    var trans = Transformation4D.Scaling( scale, ebounds.Center );
                    ent.TransformMe( tconfig, trans );
                }
            }

            if (!isStatic)
                graphicsConfig.DotsPerInch *= scale;

            RefreshDisplay();
        }

        private AttachmentPoint GetAttachmentPoint(dxxRectangularAlignments alignments)
        {
            switch (alignments)
            {
                case dxxRectangularAlignments.General:
                case dxxRectangularAlignments.TopLeft:
                    return AttachmentPoint.TopLeft;
                case dxxRectangularAlignments.MiddleLeft:
                    return AttachmentPoint.MiddleLeft;
                case dxxRectangularAlignments.BottomLeft:
                    return AttachmentPoint.BottomLeft;
                case dxxRectangularAlignments.TopCenter:
                    return AttachmentPoint.TopCenter;
                case dxxRectangularAlignments.MiddleCenter:
                    return AttachmentPoint.MiddleCenter;
                case dxxRectangularAlignments.BottomCenter:
                    return AttachmentPoint.BottomCenter;
                case dxxRectangularAlignments.TopRight:
                    return AttachmentPoint.TopRight;
                case dxxRectangularAlignments.MiddleRight:
                    return AttachmentPoint.MiddleRight;
                case dxxRectangularAlignments.BottomRight:
                    return AttachmentPoint.BottomRight;
                default:
                    break;
            }
            return AttachmentPoint.BottomLeft;
        }

        public ulong DrawText(string textString, dxfVector pt, double ht, ulong existingHandle)
        {
            DxfEntity existing = null;
            DxfMText txt = null;
            if (existingHandle != 0)
            {
                existing = transformUtil.ReadDxfEntityUsingHandle( existingHandle );
            }

            if (existing == null)
            {
                txt = new DxfMText( textString, pt.ConvertToPoint3D(), ht);
                AddEntities( new List<DxfEntity>() { txt } );
            }
            else
            {
                txt = existing as DxfMText;
                txt.Text = textString;
                txt.Height = ht;
                txt.InsertionPoint = pt.ConvertToPoint3D();
            }
            txt.LineWeight = 25;
            txt.AttachmentPoint = AttachmentPoint.TopLeft;

            RefreshDisplay();

            return txt.Handle;
        }

        public void DrawText(dxoScreenEntity sEnt)
        {
            var domain = sEnt.Domain;
            if (domain == dxxDrawingDomains.Screen && sEnt.Vectors != null && sEnt.Vectors.Count == 1)
            {
                domain = dxxDrawingDomains.Model;
            }
            DxfEntity existing = transformUtil.ReadDxfEntityUsingHandle( sEnt.DxfHandle );

            double textHeight = sEnt.ScreenFraction * ProjectedScreenHeight;
            var ip = new Point3D( sEnt.Plane.Origin.X, sEnt.Plane.Origin.Y, 0.0 );
            if (sEnt.Vectors != null && sEnt.Vectors.Count > 0)
            {
                ip = sEnt.Vectors.Item( 1 ).ConvertToPoint3D();
            }

            if (domain == dxxDrawingDomains.Screen)
            {
                textHeight = sEnt.ScreenFraction * overlaycanvas.ActualHeight;
                double yCoordinate = CalculateYCoordinateUsingScreenFraction( overlaycanvas.ActualHeight, sEnt.Alignment );
                double xCoordinate = CalculateXCoordinateUsingScreenFraction( overlaycanvas.ActualWidth, sEnt.Alignment );
                ip = new Point3D( xCoordinate, yCoordinate, 0 );
            }
            var txt = new DxfMText( sEnt.TextString, ip, textHeight );

            if (existing != null)
            {
                txt = existing as DxfMText;
                txt.Height = textHeight;
                txt.InsertionPoint = ip;
                txt.Text = sEnt.TextString;
            }
            txt.LineWeight = 25;

            txt.AttachmentPoint = GetAttachmentPoint( sEnt.Alignment );

            txt.Color = EntityColor.CreateFromColorIndex( (short)sEnt.Color );

            if (existing == null)
            {
                AddEntities( new List<DxfEntity>() { txt } );
                sEnt.DxfHandle = txt.Handle;
            }

            RefreshDisplay( txt );
        }

        public void EraseScreeenEnt (dxoScreenEntity sEnt)
        {
            DxfEntity existing = transformUtil.ReadDxfEntityUsingHandle( sEnt.DxfHandle );
            if (existing != null)
            {
                model.Entities.Remove( existing );
                sEnt.DxfHandle = 0;
                RefreshDisplay();
            }
        }

        public void ScreenEntitiesRefresh(dxoScreenEntities entities)
        {
            model.Entities.Clear();
            for (int i = 1; i <= entities.Count; i++)
            {
                var sEnt = entities.Item( i );
                if (sEnt == null) continue;
                if (isStatic && sEnt.Domain == dxxDrawingDomains.Model || !isStatic && sEnt.Domain == dxxDrawingDomains.Screen)
                    continue;

                switch (sEnt.Type)
                {
                    case dxxScreenEntityTypes.Text:
                        {
                            DrawText( sEnt );
                            break;
                        }
                    case dxxScreenEntityTypes.Axis:
                        {
                            //throw new NotImplementedException( "Screen Axis Not Implemented" );
                            break;
                        }
                    case dxxScreenEntityTypes.Rectangle:
                        {
                            DrawRectangle(sEnt);
                            break;
                        }
                    case dxxScreenEntityTypes.Point:
                        {
                            throw new NotImplementedException( "Screen Point Not Implemented" );
                        }
                    case dxxScreenEntityTypes.Circle:
                        {
                            DrawCircle(sEnt);
                            break;
                        }
                    case dxxScreenEntityTypes.Triangle:
                        {
                            throw new NotImplementedException( "Screen Triangle Not Implemented" );
                        }
                    case dxxScreenEntityTypes.Pointer:
                        {
                            DrawPointer(sEnt);
                            break;
                        }
                    case dxxScreenEntityTypes.Pill:
                        {
                            throw new NotImplementedException( "Screen Pill Not Implemented" );
                        }
                    case dxxScreenEntityTypes.Line:
                        {
                            throw new NotImplementedException( "Screen AxLineis Not Implemented" );
                        }
                }

            }
            RefreshDisplay();
        }

        private double CalculateYCoordinateUsingScreenFraction(double overlayCanvasHeight, dxxRectangularAlignments alignment)
        {
            double padding = overlayCanvasHeight * 0.01;
            double y = 0;
            switch (alignment)
            {
                case dxxRectangularAlignments.General:
                case dxxRectangularAlignments.TopLeft:
                case dxxRectangularAlignments.TopCenter:
                case dxxRectangularAlignments.TopRight:
                    y = overlayCanvasHeight - padding;
                    break;
                case dxxRectangularAlignments.MiddleLeft:
                case dxxRectangularAlignments.MiddleCenter:
                case dxxRectangularAlignments.MiddleRight:
                    y = overlayCanvasHeight / 2;
                    break;
                case dxxRectangularAlignments.BottomLeft:
                case dxxRectangularAlignments.BottomCenter:
                case dxxRectangularAlignments.BottomRight:
                    y = padding;
                    break;
                default:
                    break;
            }
            return y;
        }

        private double CalculateXCoordinateUsingScreenFraction(double overlayCanvasWidth, dxxRectangularAlignments alignment)
        {
            double padding = overlayCanvasWidth * 0.01;
            double x = 0;
            switch (alignment)
            {
                case dxxRectangularAlignments.General:
                case dxxRectangularAlignments.TopLeft:
                case dxxRectangularAlignments.MiddleLeft:
                case dxxRectangularAlignments.BottomLeft:
                    x = padding;
                    break;
                case dxxRectangularAlignments.TopCenter:
                case dxxRectangularAlignments.MiddleCenter:
                case dxxRectangularAlignments.BottomCenter:
                    x = overlayCanvasWidth / 2;
                    break;
                case dxxRectangularAlignments.TopRight:
                case dxxRectangularAlignments.MiddleRight:
                case dxxRectangularAlignments.BottomRight:
                    x = overlayCanvasWidth - padding;
                    break;
                default:
                    break;
            }
            return x;
        }

        public void ClearUCS()
        {
            if (model == null) return;
            if (model.Entities == null) return;
            DxfEntity toRemove = null;
            foreach (var ent in model.Entities)
            {
                if (ent is DxfInsert)
                {
                    var ins = ent as DxfInsert;
                    if (ins.Block.Name == "ucs")
                    {
                        toRemove = ent;
                        break;
                    }
                }
            }
            if (toRemove != null)
                model.Entities.Remove( toRemove );
        }

        public double ProjectedScreenHeight
        {
            get 
            {
                var transform = viewer.projectionTransform;
                Point3D origin = new Point3D( 0.0, 0.0, 0.0 );
                Point3D top = new Point3D( 0.0, -viewer.canvas.ActualHeight, 0.0 );
                Point3D rt = new Point3D( viewer.canvas.ActualWidth, 0.0, 0.0 );

                var bpt = transform.GetInverse().Transform( origin );
                var tpt = transform.GetInverse().Transform( top );
                var rpt = transform.GetInverse().Transform( rt );
                return Math.Min( Math.Abs( tpt.Y - bpt.Y ), Math.Abs( rpt.X - bpt.X ) );
            }
        }

        public void ShowUCS()
        {
            if (viewer?.CADModels == null)
                return;

            ClearUCS();

            int ucsMode = viewer.CADModels.UCSMode;
            var clr = viewer.CADModels.UCSColor;
            double screenFraction = viewer.CADModels.UCSSize;

            if (ucsMode == 0) return;
            if (ucsMode == 1 && !isStatic) return;
            if (ucsMode == 2 && isStatic) return;

            Vector3D scale = new Vector3D( 1.0, 1.0, 1.0 );

            double displayHeight = Math.Min(overlaycanvas.ActualHeight, overlaycanvas.ActualWidth);
            double unit = displayHeight * (screenFraction / 100 / 4);
            Point3D baseIPt = new Point3D( unit * 2, unit * 2, 0.0 );
            short lw = -1;

            if (!isStatic)
            {
                DxfUcs ucs = viewer.CADModels.UCS;

                baseIPt = ucs !=null ? new Point3D(ucs.Origin.X, ucs.Origin.Y, 0.0 ) : new Point3D(0,0,0);
                double projectedHeight = ProjectedScreenHeight;
                double scl = projectedHeight / displayHeight;
                scale = new Vector3D( scl, scl, scl );
            }
            if (model == null) return;
            if (!model.Blocks.Contains("ucs"))
            {
                DxfBlock blk = new DxfBlock( "ucs" );
                var ents = new List<DxfEntity>();
                double hunit = unit / 2.0;
                Point3D basePt = new Point3D( 0.0, 0.0, 0.0 );
                var p1 = new Point3D( basePt.X - hunit, basePt.Y + hunit, 0.0 );
                var p2 = new Point3D( p1.X + unit, p1.Y, 0.0 );
                var p3 = new Point3D( p2.X, p2.Y - unit, 0.0 );
                var p4 = new Point3D( p1.X, p3.Y, 0.0 );
                var p5 = new Point3D( basePt.X, basePt.Y + (unit * 4), 0.0 );
                var p6 = new Point3D( p5.X, p5.Y + unit, 0.0 );
                var p7 = new Point3D( basePt.X + (unit * 4), basePt.Y, 0.0 );
                var p8 = new Point3D( p7.X + unit, p7.Y, 0.0 );

                var pl = new DxfLwPolyline
                {
                    Color = EntityColor.CreateFromColorIndex((short)clr),
                    ConstantWidth = 0
                };
                pl.Vertices.Clear();
                pl.Vertices.Add( p1.As2D() );
                pl.Vertices.Add( p2.As2D() );
                pl.Vertices.Add( p3.As2D() );
                pl.Vertices.Add( p4.As2D() );
                pl.Closed = true;
                pl.LineWeight = lw;
                ents.Add( pl );
                var pl2 = new DxfLwPolyline
                {
                    Color = EntityColor.CreateFromColorIndex((short)clr)
                };
                pl.ConstantWidth = 0;
                pl2.Vertices.Clear();
                pl2.Vertices.Add( p5.As2D() );
                pl2.Vertices.Add( basePt.As2D() );
                pl2.Vertices.Add( p7.As2D() );
                pl2.LineWeight = lw;
                ents.Add( pl2 );
                var xmtx = new DxfMText("\\fArial;Y", p6, unit)
                {
                    AttachmentPoint = AttachmentPoint.MiddleCenter,
                    Color = EntityColor.CreateFromColorIndex((short)clr)
                };
                ents.Add( xmtx );
                var ymtx = new DxfMText("\\fArial;X", p8, unit)
                {
                    AttachmentPoint = AttachmentPoint.MiddleCenter,
                    Color = EntityColor.CreateFromColorIndex((short)clr)
                };
                ents.Add( ymtx );
                blk.Entities.AddRange( ents );
                model.Blocks.Add( blk );
            }
            var uinsert = new DxfInsert(model.Blocks["ucs"], baseIPt)
            {
                ScaleFactor = scale,
                LineWeight = lw
            };
            AddEntities( new List<DxfEntity>() { uinsert } );
        }

        public void DrawPointer(dxoScreenEntity sEnt)
        {
            if (sEnt.Filled)
            {
                DrawSolidPointer(sEnt);
            }
            else
            {
                DrawEmptyPointer(sEnt);
            }
        }

        public void DrawSolidPointer(dxoScreenEntity sEnt)
        {
            DxfEntity existing = transformUtil.ReadDxfEntityUsingHandle(sEnt.DxfHandle);

            if (existing == null)
            {
                DxfSolid dxfSolid;
                List<DxfEntity> solids = new List<DxfEntity>();
                for (int i = 1; i <= sEnt.Vectors.Count; i++)
                {
                    dxfSolid = new DxfSolid();
                    dxfVector basePoint = sEnt.Vectors.Item(i);
                    var height = sEnt.Plane.Height;
                    var width = sEnt.Plane.Width;
                    if (sEnt.Plane.XDirection.X == 0)
                    {
                        height = sEnt.Plane.Width;
                        width = sEnt.Plane.Height;
                    }
                    dxfVector xPoint = new dxfVector(basePoint.X, basePoint.Y);
                    dxfVector yPoint1 = new dxfVector(basePoint.X + (sEnt.Plane.XDirection.Y * width), basePoint.Y + (height / 2));
                    dxfVector yPoint2 = new dxfVector(basePoint.X + (sEnt.Plane.XDirection.Y * width), basePoint.Y - (height / 2));

                    dxfSolid.Points.Add(xPoint.ConvertToPoint3D());
                    dxfSolid.Points.Add(yPoint1.ConvertToPoint3D());
                    dxfSolid.Points.Add(yPoint2.ConvertToPoint3D());

                    solids.Add(dxfSolid);
                }

                if (solids.Count >= 1)
                {
                    AddEntities(solids);
                    sEnt.DxfHandle = solids.First().Handle;
                    RefreshDisplay(solids);
                }
            }
            // The else is omitted because we should be able to receive a list of entities using the sEnt.DxfHandle
        }

        public void DrawEmptyPointer(dxoScreenEntity sEnt)
        {
            DxfEntity existing = transformUtil.ReadDxfEntityUsingHandle(sEnt.DxfHandle);

            if (existing == null)
            {
                DxfPolyline2D dxfPolyline;
                List<DxfEntity> pointers = new List<DxfEntity>();
                for (int i = 1; i <= sEnt.Vectors.Count; i++)
                {
                    dxfPolyline = new DxfPolyline2D();
                    dxfVector basePoint = sEnt.Vectors.Item(i);
                    var height = sEnt.Plane.Height;
                    var width = sEnt.Plane.Width;
                    if (sEnt.Plane.XDirection.X == 0)
                    {
                        height = sEnt.Plane.Width;
                        width = sEnt.Plane.Height;
                    }
                    DxfVertex2D xPoint = new DxfVertex2D(basePoint.X, basePoint.Y);
                    DxfVertex2D yPoint1 = new DxfVertex2D(basePoint.X + (sEnt.Plane.XDirection.Y * width), basePoint.Y + (height / 2));
                    DxfVertex2D yPoint2 = new DxfVertex2D(basePoint.X + (sEnt.Plane.XDirection.Y * width), basePoint.Y - (height / 2));

                    dxfPolyline.Vertices.Add(xPoint);
                    dxfPolyline.Vertices.Add(yPoint1);
                    dxfPolyline.Vertices.Add(yPoint2);

                    dxfPolyline.Closed = true;

                    pointers.Add(dxfPolyline);
                }

                if (pointers.Count >= 1)
                {
                    AddEntities(pointers);
                    sEnt.DxfHandle = pointers.First().Handle;
                    RefreshDisplay(pointers);
                }
            }
            // The else is omitted because we should be able to receive a list of entities using the sEnt.DxfHandle
        }

        public void DrawLine( dxoScreenEntity sEnt )
        {
            DxfEntity existing = transformUtil.ReadDxfEntityUsingHandle( sEnt.DxfHandle );
            DxfLine line;

            if (existing == null)
            {
                line = new DxfLine(new Point3D( sEnt.Vectors.Item( 1 ).X, sEnt.Vectors.Item( 1 ).Y , 0),
                                    new Point3D( sEnt.Vectors.Item( 2 ).X, sEnt.Vectors.Item( 2 ).Y, 0 ));
                AddEntities( new List<DxfEntity>() { line } );
            }
            else
            {
                line = (DxfLine)existing;
                line.Start = new Point3D( sEnt.Vectors.Item( 1 ).X, sEnt.Vectors.Item( 1 ).Y, 0 );
                line.End = new Point3D( sEnt.Vectors.Item( 2 ).X, sEnt.Vectors.Item( 2 ).Y, 0 );
            }

            line.Color = EntityColor.CreateFromColorIndex( (short)sEnt.Color );

            RefreshDisplay();
        }

        public void DrawCircle(dxoScreenEntity sEnt)
        {
            DxfEntity existing = transformUtil.ReadDxfEntityUsingHandle(sEnt.DxfHandle);

            if (existing == null)
            {
                DxfCircle dxfCircle;
                List<DxfEntity> circles = new List<DxfEntity>();

                for (int i = 1; i <= sEnt.Vectors.Count; i++)
                {
                    dxfCircle = new DxfCircle();
                    dxfVector center = sEnt.Vectors.Item(i);
                    var radius = sEnt.Plane.Height;

                    dxfCircle.Center = center.ConvertToPoint3D();
                    dxfCircle.Radius = radius;

                    circles.Add(dxfCircle);
                }

                if (circles.Count > 1)
                {
                    AddEntities(circles);
                    sEnt.DxfHandle = circles.First().Handle;
                    RefreshDisplay(circles);
                }
            }
            // The else is omitted because we should be able to receive a list of entities using the sEnt.DxfHandle
        }

        public ulong DrawCircle(dxfVector centerPt, double radius, dxxColors clr, ulong existingHandle)
        {
            DxfEntity existing = null;
            DxfCircle dxfCircle = null;
            if (existingHandle != 0)
            {
                existing = transformUtil.ReadDxfEntityUsingHandle( existingHandle );
            }

            if (existing == null)
            {
                dxfCircle = new DxfCircle();
                AddEntities( new List<DxfEntity>() { dxfCircle } );
            }
            else
            {
                dxfCircle = existing as DxfCircle;
            }
            dxfCircle.Center = centerPt.ConvertToPoint3D();
            dxfCircle.Radius = radius;
            dxfCircle.Color = EntityColor.CreateFromColorIndex( (short)clr );

            RefreshDisplay();

            return dxfCircle.Handle;
        }

        public ulong DrawFilledCircle(iVector centerPt, double radius, dxxColors clr, ulong existingHandle)
        {
           
            DxfLwPolyline p = null;

            if (existingHandle != 0)
            {
                RemoveByHandle( existingHandle );
            }

            p = new DxfLwPolyline();

            double x = centerPt != null ? centerPt.X : 0;
            double y = centerPt != null ? centerPt.Y : 0;
            p.Vertices.Add( new DxfLwPolyline.Vertex( new Point2D( x + 0.5 * radius, y ), 1 ) );
            p.Vertices.Add( new DxfLwPolyline.Vertex( new Point2D( x - 0.5 * radius, y ), 1 ) );
            p.Closed = true;
            p.ConstantWidth = radius;
            p.Color = EntityColor.CreateFromColorIndex( (short)clr );
            AddEntities( new List<DxfEntity>() { p } );
            RefreshDisplay();
            return p.Handle;
        }

        public ulong DrawHorzLine(dxfVector centerPt, double width, dxxColors clr, short lineWeight, string lineType, ulong existingHandle )
        {
            DxfEntity existing = null;
            DxfLine dxfLine = null;
            if (existingHandle != 0)
            {
                existing = transformUtil.ReadDxfEntityUsingHandle( existingHandle );
            }

            if (existing == null)
            {
                dxfLine = new DxfLine();
                AddEntities( new List<DxfEntity>() { dxfLine } );
            }
            else
            {
                dxfLine = existing as DxfLine;
            }
            Point3D cen = centerPt.ConvertToPoint3D();
            dxfLine.Start = new Point3D( cen.X - width, cen.Y, 0 );
            dxfLine.End = new Point3D( cen.X + width, cen.Y, 0 );
            dxfLine.Color = EntityColor.CreateFromColorIndex( (short)clr );
            dxfLine.LineWeight = lineWeight;
            dxfLine.LineType = viewer.GetLineType( lineType );
            dxfLine.LineTypeScale = 10;

            RefreshDisplay();

            return dxfLine.Handle;
        }

        public void RemoveByHandle(ulong handle, bool refresh = true )
        {
            var existing = transformUtil.ReadDxfEntityUsingHandle( handle );
            if (existing == null) return;
            model.Entities.Remove( existing );
            if (refresh)
                RefreshDisplay();
            if (doNotScale.Contains( handle )) doNotScale.Remove( handle );
        }

        public void RemoveByHandle(List<ulong> handles)
        {
            foreach (var hnd in handles)
            {
                RemoveByHandle( hnd, false );
            }
            RefreshDisplay();
        }

        public void DrawRectangle(dxoScreenEntity sEnt)
        {
            if (sEnt.Vectors.Count == 0)
            {
                return;
            }

            try
            {

                if (sEnt.Vectors.Count == 1)
                {
                    DxfEntity existing = model.Entities.Where(e => e is DxfPolyline2D && e.Color == EntityColor.CreateFromColorIndex((short)sEnt.Color)).FirstOrDefault();
                    DxfPolyline2D polyline;

                    if (existing == null)
                    {
                        polyline = new DxfPolyline2D();
                        AddEntities(new List<DxfEntity>() { polyline });
                        sEnt.DxfHandle = polyline.Handle;
                    }
                    else
                    {
                        polyline = (DxfPolyline2D)existing;
                    }

                    polyline.Vertices.Clear();
                    if (!doNotScale.Contains(polyline.Handle)) doNotScale.Add(polyline.Handle);


                    DxfVertex2D center = new DxfVertex2D(sEnt.Vectors.Item(1).X, sEnt.Vectors.Item(1).Y);
                    DxfVertex2D topLeft = new DxfVertex2D(center.X - (sEnt.Plane.Width / 2), center.Y + (sEnt.Plane.Height / 2));
                    DxfVertex2D topRight = new DxfVertex2D(topLeft.X + sEnt.Plane.Width, topLeft.Y);
                    DxfVertex2D bottomRight = new DxfVertex2D(topLeft.X + sEnt.Plane.Width, topLeft.Y - sEnt.Plane.Height);
                    DxfVertex2D bottomleft = new DxfVertex2D(topLeft.X, topLeft.Y - sEnt.Plane.Height);
                    polyline.Vertices.Add(topLeft);
                    polyline.Vertices.Add(topRight);
                    polyline.Vertices.Add(bottomRight);
                    polyline.Vertices.Add(bottomleft);
                    polyline.Closed = true;

                    polyline.Color = EntityColor.CreateFromColorIndex((short)sEnt.Color);
                }
                else
                {
                    var rectanglesToAdd = new List<DxfEntity>();

                    DxfPolyline2D polyline = new DxfPolyline2D();
                    DxfVertex2D topLeft;
                    DxfVertex2D topRight;
                    DxfVertex2D bottomRight;
                    DxfVertex2D bottomleft;
                    DxfVertex2D center;

                    for (int i = 1; i <= sEnt.Vectors.Count; i++)
                    {
                        polyline = new DxfPolyline2D();

                        center = new DxfVertex2D(sEnt.Vectors.Item(i).X, sEnt.Vectors.Item(i).Y);
                        topLeft = new DxfVertex2D(center.X - (sEnt.Plane.Width / 2), center.Y + (sEnt.Plane.Height / 2));
                        topRight = new DxfVertex2D(topLeft.X + sEnt.Plane.Width, topLeft.Y);
                        bottomRight = new DxfVertex2D(topLeft.X + sEnt.Plane.Width, topLeft.Y - sEnt.Plane.Height);
                        bottomleft = new DxfVertex2D(topLeft.X, topLeft.Y - sEnt.Plane.Height);
                        polyline.Vertices.Add(topLeft);
                        polyline.Vertices.Add(topRight);
                        polyline.Vertices.Add(bottomRight);
                        polyline.Vertices.Add(bottomleft);
                        polyline.Closed = true;

                        polyline.Color = EntityColor.CreateFromColorIndex((short)sEnt.Vectors.Item(i).Color);

                        if (i == 1)
                        {
                            sEnt.DxfHandle = polyline.Handle;
                        }

                        Point3D cpt = new Point3D(center.X, center.Y, 0);
                        var transConfig = new TransformConfig();
                        var trans =
                            Transformation4D.Translation((Vector3D)cpt) *
                            Transformation4D.RotateZ(transformUtil.DegreeToRadian(sEnt.Vectors.Item(i).Rotation)) *
                            Transformation4D.Translation(-(Vector3D)cpt);
                        polyline.TransformMe(transConfig, trans);

                        rectanglesToAdd.Add(polyline);
                    }

                    AddEntities(rectanglesToAdd);
                    if (sEnt.Vectors.Count > 0)
                        sEnt.DxfHandle = rectanglesToAdd[0].Handle;

                    foreach (var rec in rectanglesToAdd)
                    {
                        if (!doNotScale.Contains(rec.Handle)) doNotScale.Add(rec.Handle);
                    }
                }

                RefreshDisplay();
            }
            catch { }

        }

        public void ClearRectangles()
        {
            model?.Entities?.Clear();
            doNotScale.Clear();
            RefreshDisplay();
        }

        public ulong DrawRectangle( iRectangle aRec, dxxColors aclr, short lineWeight, double aRotationDG, bool suppressRefresh = false, bool ignoreExisting = false)
        {
            EntityColor color = EntityColor.CreateFromColorIndex( (short)aclr );
            return DrawRectangle( aRec, color, lineWeight, 0, ignoreExisting, aRotationDG, suppressRefresh);
        }
        public ulong DrawRectangle( iRectangle aRec, System.Drawing.Color clr, short lineWeight , bool suppressRefresh = false, bool ignoreExisting = false)
        {
            EntityColor color = EntityColor.CreateFromRgb( clr.R, clr.G, clr.B );
            return DrawRectangle( aRec, color, lineWeight, 0, ignoreExisting, 0 , suppressRefresh);
        }

        public ulong DrawRectangle( iRectangle aRec, dxxColors aclr, short lineWeight, ulong toReplace, double aRotationDG, bool suppressRefresh = false)
        {
            EntityColor color = EntityColor.CreateFromColorIndex( (short)aclr );
            return DrawRectangle( aRec, color, lineWeight, toReplace, false, aRotationDG, suppressRefresh);
        }
        public ulong DrawRectangle( iRectangle aRec, System.Drawing.Color clr, short lineWeight, ulong toReplace, bool suppressRefresh = false)
        {
            EntityColor color = EntityColor.CreateFromRgb( clr.R, clr.G, clr.B );
            return DrawRectangle( aRec, color, lineWeight, toReplace, true, 0 , suppressRefresh);
        }

        public ulong DrawRectangle( iRectangle aRec, dxxColors aclr, short lineWeight, ulong toReplace, bool ignoreExisting, double aRotationDG , bool suppressRefresh = false)
        {
            EntityColor color = EntityColor.CreateFromColorIndex( (short)aclr );
            return DrawRectangle( aRec, color, lineWeight, toReplace, ignoreExisting, aRotationDG, suppressRefresh);
        }
        public ulong DrawRectangle( iRectangle aRec, System.Drawing.Color clr, short lineWeight, ulong toReplace, bool ignoreExisting, double aRotationDG, bool suppressRefresh = false)
        {
            EntityColor color = EntityColor.CreateFromRgb( clr.R, clr.G, clr.B );
            return DrawRectangle( aRec, color, lineWeight, toReplace, ignoreExisting, aRotationDG, suppressRefresh);
        }
        internal ulong DrawRectangle( iRectangle aRec, EntityColor aclr, short lineWeight, ulong toReplace, bool ignoreExisting, double aRotationDG, bool suppressRefresh = false)
        {
            DxfEntity existing;
            DxfPolyline2D polyline;

            ////delete the old one
            //if (toReplace != 0)
            //{
            //    existing = modelwh.GetEntity(toReplace, "AcDb2dPolyline");
            //    if (existing != null) modelwh.Model.Entities.Remove(existing);
            //    existing = null;
            //    //if (existing == null)
            //    //{
            //    //    polyline = new DxfPolyline2D();
            //    //    AddEntities(new List<DxfEntity>() { polyline });
            //    //}
            //    //else
            //    //{
            //    //    polyline = (DxfPolyline2D)existing;
            //    //}
            //}

        
            ulong ret = 0;
            if (model == null) return ret;

            try
            {
                existing = model.Entities != null ? model.Entities.Where(e => e is DxfPolyline2D && e.Color == EntityColor.CreateFromRgb(aclr.R, aclr.G, aclr.B)).FirstOrDefault() : null;


                if (toReplace == 0 && (existing == null || ignoreExisting))
                {
                    polyline = new DxfPolyline2D();
                    if (aRec != null) AddEntities(new List<DxfEntity>() { polyline });
                }
                else if (toReplace != 0)
                {
                    existing = modelwh.GetEntity(toReplace, "AcDb2dPolyline");


                    if (existing == null)
                    {
                        polyline = new DxfPolyline2D();
                        if (aRec != null) AddEntities(new List<DxfEntity>() { polyline });
                    }
                    else
                    {
                        polyline = (DxfPolyline2D)existing;
                        if (aRec != null) { polyline.Vertices.Clear(); } else { modelwh.Model.Entities.Remove(existing); }
                    }
                }
                else
                {
                    polyline = (DxfPolyline2D)existing;
                    polyline.Vertices.Clear();
                }

                ret = polyline.Handle;

                if (aRec != null)
                {
                    DxfVertex2D topLeft = new DxfVertex2D(aRec.Left, aRec.Top);
                    DxfVertex2D topRight = new DxfVertex2D(aRec.Right, aRec.Top);
                    DxfVertex2D bottomRight = new DxfVertex2D(aRec.Right, aRec.Bottom);
                    DxfVertex2D bottomleft = new DxfVertex2D(aRec.Left, aRec.Bottom);
                    polyline.Vertices.Add(topLeft);
                    polyline.Vertices.Add(topRight);
                    polyline.Vertices.Add(bottomRight);
                    polyline.Vertices.Add(bottomleft);

                }
                else { return 0; }

                polyline.Closed = true;
                polyline.LineWeight = lineWeight;

                polyline.Color = aclr;

                Point3D cpt = (aRec != null) ? new Point3D(aRec.Center.X, aRec.Center.Y, 0) : new Point3D(0, 0, 0);
                var transConfig = new TransformConfig();
                var trans =
                    Transformation4D.Translation((Vector3D)cpt) *
                    Transformation4D.RotateZ(transformUtil.DegreeToRadian(aRotationDG)) *
                    Transformation4D.Translation(-(Vector3D)cpt);
                polyline.TransformMe(transConfig, trans);

                if (!suppressRefresh) RefreshDisplay();
                if (!doNotScale.Contains(ret)) doNotScale.Add(ret);
            }
            catch 
            { 
                ret = 0; 
            }

            
            return ret;
        }

        public ulong DrawRectangleUsingHandle(iRectangle aRec, dxxColors aclr, short lineWeight, double aRotationDG, ulong handle = 0, bool suppressRefresh = false)
        {
            EntityColor clr = EntityColor.CreateFromColorIndex((short)aclr);

            ulong ret = 0;

            DxfEntity existing = null;
            if (handle > 0)
            {
                existing = model.Entities.Where(e => e is DxfPolyline2D && e.Handle == handle).FirstOrDefault();
            }
             
            DxfPolyline2D polyline;
            if (existing == null)
            {
                polyline = new DxfPolyline2D();
                AddEntities(new List<DxfEntity>() { polyline });
            }
            else
            {
                polyline = (DxfPolyline2D)existing;
            }
            
            ret = polyline.Handle;
            polyline.Vertices.Clear();

            DxfVertex2D topLeft = new DxfVertex2D(aRec.Left, aRec.Top);
            DxfVertex2D topRight = new DxfVertex2D(aRec.Right, aRec.Top);
            DxfVertex2D bottomRight = new DxfVertex2D(aRec.Right, aRec.Bottom);
            DxfVertex2D bottomleft = new DxfVertex2D(aRec.Left, aRec.Bottom);
            polyline.Vertices.Add(topLeft);
            polyline.Vertices.Add(topRight);
            polyline.Vertices.Add(bottomRight);
            polyline.Vertices.Add(bottomleft);
            polyline.Closed = true;
            polyline.LineWeight = lineWeight;

            polyline.Color = clr;

            if (aRotationDG != 0)
            {
                Point3D cpt = new Point3D(aRec.Center.X, aRec.Center.Y, 0);
                var transConfig = new TransformConfig();
                var trans =
                    Transformation4D.Translation((Vector3D)cpt) *
                    Transformation4D.RotateZ(transformUtil.DegreeToRadian(aRotationDG)) *
                    Transformation4D.Translation(-(Vector3D)cpt);
                polyline.TransformMe(transConfig, trans);
            }

            if (!suppressRefresh) RefreshDisplay();
            if (!doNotScale.Contains(ret)) doNotScale.Add(ret);
            return ret;
        }

        public void RemoveEntitiesUsingHandle(IEnumerable<ulong> handlesToRemove, bool bRefreshDisplay = true)
        {
            var entitiesToRemove = new List<DxfEntity>();
            List<DxfEntity> entitiesMatchingCurrentHandle;
            foreach (var currentHandle in handlesToRemove)
            {
                entitiesMatchingCurrentHandle = model.Entities.Where(e => e.Handle == currentHandle).ToList();
                entitiesToRemove.AddRange(entitiesMatchingCurrentHandle);
            }

            foreach (var e in entitiesToRemove)
            {
                model.Entities.Remove(e);
            }

            if(bRefreshDisplay) RefreshDisplay();
        }

        public void RemoveEntitiesUsingHandle(IEnumerable<int> handlesToRemove, bool bRefreshDisplay = true)
        {
            var entitiesToRemove = new List<DxfEntity>();
            List<DxfEntity> entitiesMatchingCurrentHandle;
            foreach (var currentHandle in handlesToRemove)
            {
                entitiesMatchingCurrentHandle = model.Entities.Where(e => e.Handle == (ulong)currentHandle).ToList();
                entitiesToRemove.AddRange(entitiesMatchingCurrentHandle);
            }

            foreach (var e in entitiesToRemove)
            {
                model.Entities.Remove(e);
            }

            if(bRefreshDisplay) RefreshDisplay();
        }

        public void RemoveEntitiesUsingHandle(ulong handleToRemove, bool bRefreshDisplay = true)
        {
            var entitiesToRemove = new List<DxfEntity>();
            List<DxfEntity> entitiesMatchingCurrentHandle;
            entitiesMatchingCurrentHandle = model.Entities.Where(e => e.Handle == handleToRemove).ToList();
            entitiesToRemove.AddRange(entitiesMatchingCurrentHandle);
            
            foreach (var e in entitiesToRemove)
            {
                model.Entities.Remove(e);
            }

            if(bRefreshDisplay) RefreshDisplay();
        }

        public void RemoveByType(dxxScreenEntityTypes screenEntityType)
        {
            List<DxfEntity> toBeRemoved = null;
            switch (screenEntityType)
            {
                case dxxScreenEntityTypes.All:
                    break;
                case dxxScreenEntityTypes.Undefined:
                    break;
                case dxxScreenEntityTypes.Text:
                    toBeRemoved = model.Entities.Where(e => e.GetType() == typeof(DxfMText)).ToList();
                    break;
                case dxxScreenEntityTypes.Axis:
                    break;
                case dxxScreenEntityTypes.Rectangle:
                    toBeRemoved = model.Entities.Where(e => e.GetType() == typeof(DxfPolyline2D)).ToList();
                    break;
                case dxxScreenEntityTypes.Point:
                    break;
                case dxxScreenEntityTypes.Circle:
                    toBeRemoved = model.Entities.Where(e => e.GetType() == typeof(DxfCircle)).ToList();
                    break;
                case dxxScreenEntityTypes.Triangle:
                    break;
                case dxxScreenEntityTypes.Pointer:
                    toBeRemoved = model.Entities.Where(e => e.GetType() == typeof(DxfSolid) || e.GetType() == typeof(DxfPolyline2D)).ToList();
                    break;
                case dxxScreenEntityTypes.Pill:
                    break;
                case dxxScreenEntityTypes.Line:
                    break;
                default:
                    break;
            }

            if ((toBeRemoved?.Count ?? 0) > 0)
            {
                foreach (var item in toBeRemoved)
                {
                    model.Entities.Remove(item);
                }
            }
        }

        protected virtual void Dispose( bool disposing )
        {
            if (!disposedValue)
            {
                if (disposing)
                    if (disposing)
                    {
                        transformUtil.Dispose();
                    }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                viewer = null;
                overlaycanvas.Children.Clear();
                //overlaycanvas = null;
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~OverlayHandler()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose( disposing: true );
            GC.SuppressFinalize( this );
        }

        public void ClearEntities()
        {
            model.Entities.Clear();
        }

    }
}
