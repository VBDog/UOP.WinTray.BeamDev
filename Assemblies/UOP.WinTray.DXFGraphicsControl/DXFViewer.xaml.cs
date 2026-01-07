
//using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UOP.DXFGraphics;
using UOP.DXFGraphics.Utilities;
using WW.Cad.Base;
using WW.Cad.Drawing;
using WW.Cad.Drawing.Wpf;
using WW.Cad.IO;
using WW.Cad.Model;
using WW.Cad.Model.Entities;
using WW.Cad.Model.Objects;
using WW.Cad.Model.Tables;
using WW.Drawing;
using WW.Math;
using static System.Net.WebRequestMethods;

namespace UOP.DXFGraphicsControl
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class DXFViewer : UserControl, IDisposable
	{
        public CADLibModels CADModels { get; set; }
        public ModelWithHandles modelwh { get { return CADModels?.modelwh; } }
        private DxfModel model { get { return modelwh?.Model; } }
        private DxfLayout paperSpaceLayout { get; set; }
        private WpfWireframeGraphics3DUsingDrawingVisual wpfGraphics { get; set; }
        private WpfWireframeGraphics3DUsingDrawingVisual.VisualContainer visualContainer { get; set; }
        private WireframeGraphics2Cache graphicsCache { get; set; }

        public DxfEntityCollection Entities => model != null  ? model.Entities : new DxfEntityCollection();

        public int EntitiyCount => Entities.Count;

        public string ImageGUID { get; set; } = string.Empty;

        private GraphicsConfig _GraphicsConfig = null;
        private GraphicsConfig MyGraphicsConfig 
        {
            get 
            {
                if(_GraphicsConfig == null)
                {
                    Init();

                    
                }
                return _GraphicsConfig;
            }
            set => _GraphicsConfig = value;
        }

        private Bounds3D _Bounds = null;
        private Bounds3D MyBounds
        {
            get
            {
                if (_Bounds == null)
                {
                    if(_GraphicsConfig == null) Init(true);
                    UpdateRenderTransform(recalculateBounds: true);
                }

                return _Bounds;
            }
            set => _Bounds = value;
        }

        // The translation:
        // x = -1 is the left side, x = 1 is the right side.
        // y = -1 is the bottom side, y = 1 is the top side.
        private Vector3D translation;
		private Vector3D translationAtMouseClick;
		private double scaling = 1d;
		public Point2D mouseDownLocation;
		public bool mouseDown;
		//public UOP.DXFGraphics.dxfImage oImage;
		public Matrix4D centerTransform = Matrix4D.Identity;
        public Matrix4D projectionTransform;
        private RenderedEntityInfo highlightedEntity;
        private ArgbColor highlightColor = ArgbColors.Magenta;
        private ArgbColor secondaryHighlightColor = ArgbColors.Cyan;
        public bool SupressRefresh = false;
        public OverlayHandler overlayHandler { get; set; }
        public OverlayHandler staticOverlayHandler { get; set; }
        private readonly bool isReset = false;
		private double viewRotation { get; set; }

        public dxfImage DXFImageSource
        {
            get { return (dxfImage)GetValue( DXFImageSourceProperty ); }
            set 
            {
                var _value = (dxfImage)GetValue( DXFImageSourceProperty );
                if (value == null && _value != null)
                {
                    _value.Dispose();
                }
                SetValue( DXFImageSourceProperty, value );
                if (value != null)
                {
                    SetImage(value);
                    if (value.Display.HasLimits)
                    { ZoomWindow(value.Display.LimitsRectangle); }
                    else
                    { ZoomExtents(); }
                    RefreshDisplay();
                }
             
                //else
                //    Clear();
            }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DXFImageSourceProperty =
            DependencyProperty.Register( "DXFImageSource", typeof( dxfImage ), typeof( DXFViewer ), new UIPropertyMetadata(ImageChangedCallback));

        private static void ImageChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DXFViewer viewer = (DXFViewer)d;
            viewer.DXFImageSource = e.NewValue as dxfImage;
        }


        public System.Drawing.Size Size 
        {
            get
            {
                return new System.Drawing.Size(Convert.ToInt32(canvas.ActualWidth), Convert.ToInt32(canvas.ActualHeight));
//                return new System.Drawing.Size(Convert.ToInt32(ActualWidth), Convert.ToInt32(ActualHeight));
            }
        }

        public event EventHandler<ulong> EntitySelected;

        public DxfDimensionStyleOverrides ActiveDimOverrides { get; set; }
		public List<DxfMessage> messages = new List<DxfMessage>();
        public Point2D mouseLocation { get; set; }
        public ulong SelectedEntityHandle { get; set; }

        private readonly double baseDPI = 100d;
        public int UCSMode { get; set; }

        private readonly short arcResolution = 100;

        public bool PanIsDisabled { get; set; }

        public bool ZoomIsDisabled { get; set; }
        public bool IgnoreSelection { get; set; }

        public Canvas dragSelectionCanvas
        {
            get
            {
                return DragSelectionCanvas;
            }
        }

        public Border dragSelectionBorder
        {
            get
            {
                return DragSelectionBorder;
            }
        }

        public delegate void MouseDownHander( object sender, MouseButtonEventArgs e );
        public event MouseDownHander OnViewerMouseDown;

        public delegate void MouseUpHander(object sender, MouseButtonEventArgs e);
        public event MouseUpHander OnViewerMouseUp;

        public bool EnableRightMouseZoom { get; set; }

        public bool InZoomWindow { get; internal set; }

        public ZoomData CurrentZoom { get 
            { 
                UpdateRenderTransform(true );
                return new ZoomData(scaling, MyBounds.Clone(), translation);  
            } 
        }
        public ZoomData LastZoom { get { return CADModels?.LastZoom; } }
        public ZoomData PrevZoom { get { return CADModels?.PrevZoom; } }
        public List<DxfEntity> TempEntities { get { return CADModels?.TempEntities; } }

        public DXFViewer()
		{
			InitializeComponent();
            ZoomIsDisabled = false;
            PanIsDisabled = false;
            IgnoreSelection = true;
            //LastZooms = new Dictionary<string, ZoomData>();
            //PrevZooms = new Dictionary<string, ZoomData>();
            EnableRightMouseZoom = false;
            CADModels = new CADLibModels();
		}

        public void SetDragBoxLeft (double left)
        {
            Canvas.SetLeft( DragSelectionBorder, left );
        }

        public void SetDragBoxTop(double top)
        {
            Canvas.SetTop( DragSelectionBorder, top );
        }

        public void ShowDragSelectionRect()
        {
            DragSelectionCanvas.Visibility = Visibility.Visible;
        }

        public void HideDragSelectionRect()
        {
            DragSelectionCanvas.Visibility = Visibility.Hidden;
            dragSelectionBorder.Width = 0;
            dragSelectionBorder.Height = 0;

        }

        public void SetDragSelectionRectMode(string mode)
        {
            switch (mode.ToUpper())
            {
                case "CONTROL":
                    DragSelectionBorder.BorderBrush = System.Windows.Media.Brushes.Blue;
                    break;
                case "ALT":
                    DragSelectionBorder.BorderBrush = System.Windows.Media.Brushes.Black;
                    break;
                case "SHIFT":
                    DragSelectionBorder.BorderBrush = System.Windows.Media.Brushes.Magenta;
                    break;
                default:
                    DragSelectionBorder.BorderBrush = System.Windows.Media.Brushes.Black;
                    break;
            }
        }

        public void SetDragSelectionBorderColor(System.Windows.Media.Brush aColor)
        =>           DragSelectionBorder.BorderBrush = aColor;
         


        private void CheckBounds()
        {
            CheckBounds(MyBounds);
        }

        public Bounds3D CheckBounds(Bounds3D aBounds)
        {
            if (aBounds == null) return null;
            if (double.IsNaN( aBounds.Delta.GetLength() ) || double.IsInfinity( aBounds.Delta.GetLength() ))
            {
                aBounds.Corner1 = new Point3D( -50, -50, 0 );
                aBounds.Corner2 = new Point3D( 50, 50, 0 );
            }
            return aBounds;
        }

        /// <summary>
        /// Reads a DXF model and creates 2D WPF graphics that are put onto the Canvas area.
        /// </summary>
        public void Init(bool clearOverlays = true)
		{
            WW.Cad.Model.DxfModel.AddShxLookupDirectories( @"Q:\IT\Support\WinTray\Fonts" );
            WW.Cad.Model.DxfModel.FallbackShxFont = @"txt.shx";
            UCSMode = 0;
			viewRotation = 0.0;

            Background = CADModels.Background;

            if (clearOverlays)
            {
                if (overlayHandler != null)
                    overlayHandler.Clear();
                if (staticOverlayHandler != null)
                    staticOverlayHandler.Clear();
            }

            #region calculate the model's _Bounds to determine a proper dots per inch

            double dotsperinch = baseDPI;
            System.Windows.Media.SolidColorBrush b = Background as System.Windows.Media.SolidColorBrush;
            _GraphicsConfig = GetBrightness(b.Color) < 35 ? (GraphicsConfig)GraphicsConfig.BlackBackgroundCorrectForBackColor.Clone() : (GraphicsConfig)GraphicsConfig.WhiteBackgroundCorrectForBackColor.Clone(); ;
            _GraphicsConfig.DotsPerInch = dotsperinch > 0.0 ? dotsperinch : baseDPI;
            _GraphicsConfig.NoOfArcLineSegments = arcResolution;
            _GraphicsConfig.NoOfArcLineSegments = arcResolution;
            // The dots per inch value is important because it determines the eventual pen thickness.

            BoundsCalculator boundsCalculator = new BoundsCalculator();
			if (model.ActiveLayout == null || model.Header.ShowModelSpace)
			{
				boundsCalculator.GetBounds(model);
			}
			else
			{
				paperSpaceLayout = model.ActiveLayout;
				boundsCalculator.GetBounds(model, model.ActiveLayout);
			}
			_Bounds = boundsCalculator.Bounds;
            CheckBounds(_Bounds);
			Vector3D delta = _Bounds.Delta;
            double canvasWidth = canvas.ActualWidth;
            double canvasHeight = canvas.ActualHeight;
            double estimatedScale = Math.Min( canvasWidth / delta.X, canvasHeight / delta.Y );
			double penWidthCorrectionFactor = paperSpaceLayout == null ? estimatedScale : 1d;
			if (!double.IsInfinity(_Bounds.Delta.X))
				_GraphicsConfig.DotsPerInch = baseDPI / penWidthCorrectionFactor;

            #endregion

            graphicsCache = new WireframeGraphics2Cache(true, true) { Config = _GraphicsConfig };

            if (GetBrightness( ((System.Windows.Media.SolidColorBrush)Background).Color ) < 35)
            {
                _GraphicsConfig = (GraphicsConfig)GraphicsConfig.BlackBackgroundCorrectForBackColor.Clone();
            }
            else
            {
                _GraphicsConfig = (GraphicsConfig)GraphicsConfig.WhiteBackgroundCorrectForBackColor.Clone();
            }

            if (paperSpaceLayout == null)
			{
				centerTransform = Transformation4D.Translation(Point3D.Zero - _Bounds.Center);
				graphicsCache.CreateDrawables(model, centerTransform);
				_Bounds.Transform(centerTransform);
			}
			else
			{
				graphicsCache.CreateDrawables(model, model.ActiveLayout);
			}

            wpfGraphics = new WpfWireframeGraphics3DUsingDrawingVisual { Config = _GraphicsConfig };
            wpfGraphics.EnableDrawablesUpdate();

            visualContainer = new WpfWireframeGraphics3DUsingDrawingVisual.VisualContainer();
            canvas.Children.Add( visualContainer );

            UpdateWpfGraphics();

			if (clearOverlays)
            {
                CADModels.ResetOverlays();
				overlayHandler = new OverlayHandler( this, _GraphicsConfig, centerTransform );
				staticOverlayHandler = new OverlayHandler( this, null, centerTransform );
            }
            if (!isReset)
            {
                canvas.SizeChanged += canvas_SizeChanged;
                overlaycanvas.SizeChanged += canvas_SizeChanged;
                staticoverlaycanvas.SizeChanged += canvas_SizeChanged;
            }
        }

        public void ResetImage( UOP.DXFGraphics.dxfImage iImage )
        {
            if (CADModels != null && iImage != null)
            {
                CADModels.GUID = iImage.GUID;
                ResetHeader( iImage );
            }

            //string iGUID = "";
            //if (oImage != null)
            //{
            //    iGUID = oImage.GUID;
            //    //release the image
            //    oImage.EntitiesEvent -= EntitiesEventHandler;
            //    oImage.SettingChange -= SettingsChangeHandler;
            //    oImage.TableEvent -= TableEventHandler;
            //    oImage.TableMemberEvent -= TableMemeberEventHandler;
            //    oImage.ObjectEvent -= ObjectEventHandler;
            //    oImage.ObjectsEvent -= ObjectsEventHandler;
            //    oImage.RenderEvent -= RenderEventHandler;
            //    oImage.StatusChange -= StatusChangeEventHandler;
            //    oImage.EntitiesUpdateEvent -= EntitiesUpdateEventHandler;
            //    oImage.ViewChangeEvent -= ViewChangeEventHandler;
            //    //oImage.OverlayEvent -= OverlayerEventHandler;
            //    //oImage.OverlayBmpEvent -= OverlayBmpEventHandler;
            //    oImage.ScreenRender -= ScreenBmpEventHandler;
            //    oImage.ZoomEvent -= ZoomEventHandler;
            //    oImage.ViewRotateEvent -= RotateEventHandler;
            //    oImage.ViewRegenerate -= ViewRegenerateEventHandler;
            //    oImage.SaveToFileEvent -= SaveToFile;
            //    oImage.ScreenDrawingEvent -= ScreenDrawingEventHandler;
            //    if (iImage == null) oImage.UsingDxfViewer = false;
            //}

            //if(iImage == null)
            //{
            //    oImage = null;
            //    return;
            //}


            //oImage = iImage;
            //oImage.Display.Size = Size;
            //oImage.UsingDxfViewer = true;
            //if(iGUID!= oImage.GUID) ResetHeader();
            //oImage.EntitiesEvent += EntitiesEventHandler;
            //oImage.SettingChange += SettingsChangeHandler;
            //oImage.TableEvent += TableEventHandler;
            //oImage.TableMemberEvent += TableMemeberEventHandler;
            //oImage.ObjectEvent += ObjectEventHandler;
            //oImage.ObjectsEvent += ObjectsEventHandler;
            //oImage.RenderEvent += RenderEventHandler;
            //oImage.StatusChange += StatusChangeEventHandler;
            //oImage.EntitiesUpdateEvent += EntitiesUpdateEventHandler;
            ////oImage.OverlayEvent += OverlayerEventHandler;
            ////oImage.OverlayBmpEvent += OverlayBmpEventHandler;
            //oImage.ScreenRender += ScreenBmpEventHandler;
            //oImage.ZoomEvent += ZoomEventHandler;
            //oImage.ViewRotateEvent += RotateEventHandler;
            //oImage.ViewRegenerate += ViewRegenerateEventHandler;
            //oImage.SaveToFileEvent += SaveToFile;
            //oImage.ScreenDrawingEvent += ScreenDrawingEventHandler;
            //oImage.ViewChangeEvent += ViewChangeEventHandler;
        }

        public void ResetModel()
        {
            //if (model != null && model.Entities != null && model.Entities.Count == 0)
            //    return;

            if (transformUtil != null)
                transformUtil.Dispose();

            //CADModels.modelwh = new ModelWithHandles();
            //model = modelwh.Model;
            transformUtil = new Utils.TransformUtil( this );
            wpfGraphics = null;
            visualContainer = null;

            if (graphicsCache != null)
            {
                canvas.Children.Clear();

                graphicsCache.CreateDrawables( model, centerTransform );
                MyBounds.Transform( centerTransform );

                wpfGraphics = new WpfWireframeGraphics3DUsingDrawingVisual{ Config = MyGraphicsConfig };
                wpfGraphics.EnableDrawablesUpdate();

                visualContainer = new WpfWireframeGraphics3DUsingDrawingVisual.VisualContainer();
                canvas.Children.Add( visualContainer );

                UpdateWpfGraphics();
            }
        }

        public void SetImage(dxfImage iImage) { SetImage(iImage, ZoomIsDisabled, PanIsDisabled); }
        public void SetImage(dxfImage iImage, bool DisableZoom) { SetImage(iImage, DisableZoom, PanIsDisabled); }


        public void SetImage(dxfImage iImage, bool DisableZoom , bool DisablePan)
		{
            ZoomIsDisabled = DisableZoom;
            PanIsDisabled = DisablePan;
            ImageGUID = iImage == null ? string.Empty : iImage.GUID;
            if (CADModels != null)
            {
                CADModels.Clear();
                CADModels.Dispose();
                CADModels = null;
            }
            CADModels = new CADLibModels();
            if (iImage != null) try { CADModels.Background = GetBackground(iImage.Display.BackgroundColor); } 
                catch (Exception e)
                {
                    System.Diagnostics.Debug.Write(e.Message);
                }

            //string iGUID = (oImage != null) ? oImage.GUID : "";


            //if (CADModels != null && CADModels.guid == iImage.GUID)
            //{
            //    ResetModel();
            //    ResetImage(iImage);
            //    Regenerate();
            //    return;
            //}


            //ResetImage( iImage);
            
            ResetModel();

            Init(true);
            if ( iImage != null)
            {
                iImage.Header.ResetDimOverrides();
                CADModels.GUID = iImage.GUID;

                if (iImage.Entities.Count > 0 || isReset)
                {
                    Regenerate(iImage, iImage.Screen.Entities.Count > 0);
                    if (ZoomIsDisabled)
                        ZoomWindow( iImage.Display.ViewRectangle );
                    ZoomLast();
                }
                else { ResetHeader(iImage); }
                //Correct line weight of overlay entities.
                overlayHandler.AdjustDPI();
            }
            else
            {
                RefreshDisplay();
            }
           
        }

        public void SetModel( CADLibModels cadModels ) { SetModel( cadModels, ZoomIsDisabled, PanIsDisabled ); }
        public void SetModel( CADLibModels cadModels, bool DisableZoom ) { SetModel( cadModels, DisableZoom, PanIsDisabled ); }


        public void SetModel( CADLibModels cadModels, bool DisableZoom, bool DisablePan )
        {
            ZoomIsDisabled = DisableZoom;
            PanIsDisabled = DisablePan;

            CADModels = cadModels;
            if (!CADModels.TempApplied)
            {
                model.Entities.AddRange( CADModels.TempEntities );
                CADModels.TempApplied = true;
            }
            if (overlayHandler == null)
                Init();

            //    overlayHandler = new OverlayHandler( this, MyGraphicsConfig, centerTransform );

            //if (staticOverlayHandler == null)
            //    staticOverlayHandler = new OverlayHandler( this, null, centerTransform );

            //if (graphicsCache == null)
            //{
            //    graphicsCache = new WireframeGraphics2Cache( true, true );
            //    graphicsCache.Config = MyGraphicsConfig;
            //}

            overlayHandler.modelwh = CADModels.overlay;
            staticOverlayHandler.modelwh = CADModels.staticOverlay;
            RefreshDisplay();

            //transformUtil = new Utils.TransformUtil( this );
            //wpfGraphics = null;
            //visualContainer = null;

            //if (graphicsCache != null)
            //{
            //    canvas.Children.Clear();

            //    graphicsCache.CreateDrawables( model, centerTransform );
            //    MyBounds.Transform( centerTransform );

            //    wpfGraphics = new WpfWireframeGraphics3DUsingDrawingVisual();
            //    wpfGraphics.Config = MyGraphicsConfig;
            //    wpfGraphics.EnableDrawablesUpdate();

            //    visualContainer = new WpfWireframeGraphics3DUsingDrawingVisual.VisualContainer();
            //    canvas.Children.Add( visualContainer );

            //    UpdateWpfGraphics();
            //}

            //Init();

            //if (iImage.Entities.Count > 0 || isReset)
            //{
            //    Regenerate( iImage, iImage.Screen.Entities.Count > 0 );
            //    if (ZoomIsDisabled)
            //        ZoomWindow( iImage.Display.ViewRectangle );
            //    ZoomLast();
            //}

            ZoomLast();
            //Correct line weight of overlay entities.
            overlayHandler.AdjustDPI();
        }


        private void DrawScreenEntity(dxoScreenEntity sEnt)
        {
            switch (sEnt.Type)
            {
                case dxxScreenEntityTypes.Text:
                    {
                        if (sEnt.Domain == dxxDrawingDomains.Model || (sEnt.Domain == dxxDrawingDomains.Screen && sEnt.Vectors != null && sEnt.Vectors.Count == 1))
                            overlayHandler.DrawText( sEnt );
                        else
                            staticOverlayHandler.DrawText( sEnt );
                        break;
                    }
                case dxxScreenEntityTypes.Axis:
                    {
                        //throw new NotImplementedException( "Screen Axis Not Implemented" );
                        break;
                    }
                case dxxScreenEntityTypes.Rectangle:
                    {
                        if (sEnt.Domain == dxxDrawingDomains.Model)
                        {
                            overlayHandler.DrawRectangle(sEnt);
                        }
                        else
                        {
                            staticOverlayHandler.DrawRectangle(sEnt);
                        }
                        break;
                    }
                case dxxScreenEntityTypes.Point:
                    {
                        //throw new NotImplementedException( "Screen Point Not Implemented" );
                        break;
                    }
                case dxxScreenEntityTypes.Circle:
                    {
                        if (sEnt.Domain == dxxDrawingDomains.Model)
                        {
                            overlayHandler.DrawCircle(sEnt);
                        }
                        else
                        {
                            staticOverlayHandler.DrawCircle(sEnt);
                        }
                        break;
                    }
                case dxxScreenEntityTypes.Triangle:
                    {
                        throw new NotImplementedException( "Screen Triangle Not Implemented" );
                    }
                case dxxScreenEntityTypes.Pointer:
                    {
                        if (sEnt.Domain == dxxDrawingDomains.Model)
                        {
                            overlayHandler.DrawPointer(sEnt);
                        }
                        else
                        {
                            staticOverlayHandler.DrawPointer(sEnt);
                        }
                        break;
                    }
                case dxxScreenEntityTypes.Pill:
                    {
                        if (sEnt.Drawn)
                            throw new NotImplementedException( "Screen Pill Not Implemented" );

                        break;
                    }
                case dxxScreenEntityTypes.Line:
                    {
                        if (sEnt.Domain == dxxDrawingDomains.Model)
                        {
                            overlayHandler.DrawLine( sEnt );
                        }
                        else
                        {
                            staticOverlayHandler.DrawLine( sEnt );
                        }
                        break;
                    }
            }
        }

        private void ScreenDrawingEventHandler( dxxScreenEventTypes aEventType, dxfImageScreenEventArg e )
        {
            switch (aEventType)
            {
                case dxxScreenEventTypes.Clear:
                    {
                        //overlayHandler.Clear();
                        //staticOverlayHandler.Clear();
                        break;
                    }
                case dxxScreenEventTypes.Refresh:
                    {
                        //overlayHandler.ScreenEntitiesRefresh( oImage.Screen.Entities );
                        //staticOverlayHandler.ScreenEntitiesRefresh( oImage.Screen.Entities );
                        break;
                    }
                case dxxScreenEventTypes.ScreenEntityDrawn:
                    {
                        DrawScreenEntity( e.ScreenEntity );
                        break;
                    }
                case dxxScreenEventTypes.ScreenEntityReDrawn:
                    {
                        DrawScreenEntity( e.ScreenEntity );
                        break;
                    }
                case dxxScreenEventTypes.ExtentRectangleDrawn:
                    {
                        throw new NotImplementedException( "Screen ExtentRectangleDrawn Not Implemented" );
                    }
                case dxxScreenEventTypes.EntityOCSDrawn:
                    {
                        throw new NotImplementedException( "Screen EntityOCSDrawn Not Implemented" );
                    }
                case dxxScreenEventTypes.EntityBoundsDrawn:
                    {
                        throw new NotImplementedException( "Screen EntityBoundsDrawn Not Implemented" );
                    }
                case dxxScreenEventTypes.EntityExtentPtsDrawn:
                    {
                        throw new NotImplementedException( "Screen EntityExtentPtsDrawn Not Implemented" );
                    }
                case dxxScreenEventTypes.ScreenEntityErase:
                    {
                        if (e.ScreenEntity.Domain == dxxDrawingDomains.Model)
                            overlayHandler.EraseScreeenEnt( e.ScreenEntity );
                        else
                            staticOverlayHandler.EraseScreeenEnt( e.ScreenEntity );
                        break;
                    }
            }
        }

        //public void Regenerate(dxxEntityTypes entityType)
        //{
        //    for (int i = 1; i <= oImage.Entities.Count; i++)
        //    {
        //        var ent = oImage.Entities.Item( i );
        //        if (ent == null || ent.EntityType != entityType)
        //            continue;

        //        DxfEntity existing = transformUtil.ReadDxfEntityUsingHandle( ent.DxfHandles );
        //        if (existing == null)
        //        {
        //            continue;
        //        }

        //        transformUtil.MatchEntitySettings( existing, ent);
        //    }
        //    RefreshDisplay();
        //}

        public void Regenerate()
        {
            Regenerate( null, false );
        }

        public void Regenerate(UOP.DXFGraphics.dxfImage image, bool clearOverlays)
        {
        
            if (clearOverlays)
            {
                overlayHandler?.Dispose();
                staticOverlayHandler?.Dispose();
                CADModels.ResetOverlays();
                overlayHandler = new OverlayHandler( this, MyGraphicsConfig, centerTransform );
                staticOverlayHandler = new OverlayHandler( this, null, centerTransform );
            }

            ResetHeader( image );
            SupressRefresh = true;
            if (image != null)
            {
                try
                {
                    foreach (var item in image.Entities)
                    {

                        if (item == null) continue;

                        var ent = item;
                        if (item.DxfHandles != null)
                            Array.Clear(item.DxfHandles, 0, item.DxfHandles.Length);

                        try
                        {
                            List<DxfEntity> ents = ConvertDXFGraphicsEntity(ref ent, image);

                            if(ents != null)
                            {
                                if (!ent.SaveToFile) TempEntities.AddRange(ents);
                                graphicsCache.AddDrawables(model, ents.FindAll((x) => x != null), Matrix4D.Identity);
                            }
                            //this adds entities directly
                            
      
                            //this is how we added entities when we were responding to entities added to image in real time
                            // EntitiesEventHandler(true, ref ent, image);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"{ex.Message}");
                            if (ex is System.OutOfMemoryException) break;
                            continue;
                        }
                    }
                 }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                
                }
                finally
                {
                   
                    UpdateWpfGraphics();
                    overlayHandler.RefreshDisplay();

                }
    
            }

            try
            {
                UpdateRenderTransform(false);

                if (image != null)
                {
                    overlayHandler.SupressRefresh = true;
                    staticOverlayHandler.SupressRefresh = true;

                    for (int i = 1; i <= image.Screen.Entities.Count; i++)
                    {
                        var sent = image.Screen.Entities.Item(i);
                        DrawScreenEntity(sent);
                    }

            
                }
            }
            catch (Exception ex) 
            { 
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                overlayHandler.SupressRefresh = false;
                staticOverlayHandler.SupressRefresh = false;
                SupressRefresh = false;
                RefreshDisplay();
            }
        

        }

        public double ProjectedScreenHeight
        {
            get
            {
                var transform = projectionTransform;
                Point3D origin = new Point3D( 0.0, 0.0, 0.0 );
                Point3D top = new Point3D( 0.0, -canvas.ActualHeight, 0.0 );
                Point3D rt = new Point3D( canvas.ActualWidth, 0.0, 0.0 );

                var bpt = transform.GetInverse().Transform( origin );
                var tpt = transform.GetInverse().Transform( top );
                var rpt = transform.GetInverse().Transform( rt );
                return Math.Min( Math.Abs( tpt.Y - bpt.Y ), Math.Abs( rpt.X - bpt.X ) );
            }
        }

        public (double, double) DeviceToWorld(double X, double Y)
        {
            var pt = new Point2D( X, Y );
            var wpt = projectionTransform.GetInverse().Transform( pt );
            return (wpt.X, wpt.Y);
        }

        public dxfVector DeviceToWorld(dxfVector p)
        {
            var pt = new Point2D( p.X, p.Y );
            var wpt = projectionTransform.GetInverse().Transform( pt );
            return new dxfVector( wpt.X, wpt.Y, 0 );
        }

        public dxfVector Transform_WorldToDevice(dxfVector p)
        {
            var pt = new Point2D( p.X, p.Y );
            var dpt = projectionTransform.Transform( pt );
            return new dxfVector( dpt.X, dpt.Y, 0.0 );
        }

        private void RefreshDisplay( DxfEntity ent )
        {
			if (SupressRefresh) return;
            graphicsCache.AddDrawables( model, new List<DxfEntity>() { ent }, Matrix4D.Identity );

            UpdateWpfGraphics();
			overlayHandler.RefreshDisplay();
        }

        private void RefreshDisplay( IEnumerable<DxfEntity> ents, bool saveToFile )
        {
            if (SupressRefresh || ents == null) return;
            if (!saveToFile)  TempEntities.AddRange(ents);
            graphicsCache.AddDrawables( model, ents.ToList(), Matrix4D.Identity );

            UpdateWpfGraphics();
            overlayHandler.RefreshDisplay();
        }

        public void RefreshDisplay(bool resetSupression = true)
        {
            if (model == null) return;

            model.Header.LineTypeScale = ValidateLTScale( model.Header.LineTypeScale );

            //SetBackgroundColor( oImage.Display.BackgroundColor );
			DxfMessage[] messages;
            var vc = new ValidationContext( model );
			model.Validate( vc, out messages );
			if (messages.Count() > 0)
				System.Diagnostics.Debugger.Break();

			if (resetSupression)
				SupressRefresh = false;
            graphicsCache.Clear();
            graphicsCache.AddDrawables( model, model.Entities, Matrix4D.Identity );

            UpdateWpfGraphics();
            overlayHandler.RefreshDisplay();
            staticOverlayHandler.RefreshDisplay();
            UpdateRenderTransform( false );
        }

        public void RefreshDisplay( ulong handle )
        {
            RefreshDisplay( modelwh.GetEntity( handle ) );
        }

        private void UpdateWpfGraphics()
        {
            wpfGraphics.Clear();
            IWireframeGraphicsFactory2 graphicsFactory = wpfGraphics.CreateGraphicsFactory( paperSpaceLayout );
            foreach (IWireframeDrawable2 drawable in graphicsCache.Drawables)
            {
                try
                {
                    drawable.Draw( graphicsFactory );
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.Write( e.Message );
                    return;
                }
            }
            UpdateWpfVisuals();
        }

        private void UpdateWpfVisuals()
        {
            if (visualContainer == null) return;

            try
            {
                visualContainer.Visuals.Clear();
                wpfGraphics.Draw(visualContainer.Visuals);
            }
            catch { }
        }

        /// <summary>
        /// Update the canvas RenderTransform.
        /// </summary>
        public void canvas_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateRenderTransform(false);
		}

        private System.Windows.Point _DownPoint;

        private void Canvas_LeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point p = e.GetPosition((System.Windows.IInputElement)sender);
                mouseDownLocation = new Point2D(p.X, p.Y);

                if (InZoomWindow)
                {
                    //SetDragSelectionRectMode("");
                    _DownPoint = e.GetPosition((System.Windows.IInputElement)sender);
                    ShowDragSelectionRect();
                }
                else
                {
                    PrevZoom.Set(scaling, MyBounds, translation);
                    translationAtMouseClick = translation;
                    mouseDown = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex.Message);
            }
		

            canvas.CaptureMouse();

            OnViewerMouseDown?.Invoke(this, e);
        }

        private void Canvas_RightButtonDown( object sender, MouseButtonEventArgs e )
        {
            if (EnableRightMouseZoom)
            {
                if (e.ClickCount == 2)
                {
                    ZoomExtents(); //  ZoomPrevious();
                    e.Handled = true;
                }
                else if (e.ClickCount == 1)
                {
                    InZoomWindow = true;
                    //SetDragSelectionRectMode( "" );
                    _DownPoint = e.GetPosition( (System.Windows.IInputElement)sender );
                    ShowDragSelectionRect();
                }
            }
            OnViewerMouseDown?.Invoke(this, e);
        }

        private void Canvas_RightButtonUp ( object sender, MouseButtonEventArgs e )
        {
            if (InZoomWindow)
            {
                Point p = e.GetPosition( viewerControl );
                var p1 = DeviceToWorld( new dxfVector( _DownPoint.X, _DownPoint.Y, 0 ) );
                var p2 = DeviceToWorld( new dxfVector( p.X, p.Y, 0 ) );
                var topleft = new dxfVector( Math.Min( p1.X, p2.X ), Math.Max( p1.Y, p2.Y ), 0 );
                var bottomright = new dxfVector( Math.Max( p1.X, p2.X ), Math.Min( p1.Y, p2.Y ), 0 );
                var cen = new dxfVector( topleft.X + ((bottomright.X - topleft.X) / 2), bottomright.Y + ((topleft.Y - bottomright.Y) / 2.0), 0 );
                var rect = new dxfRectangle( cen, bottomright.X - topleft.X, topleft.Y - bottomright.Y );

                HideDragSelectionRect();
                if (rect.Width > 1 && rect.Height > 1)
                    ZoomWindow( rect );

                InZoomWindow = false;
                Cursor = null;
            }
            OnViewerMouseUp?.Invoke(this, e);
        }

        /// <summary>
        /// Pan if the left mouse button is down.
        /// </summary>
        public void Canvas_MouseMove(object sender, MouseEventArgs e)
		{
            if (InZoomWindow)
            {
                Point cp = e.GetPosition( viewerControl );
                var rect = new Rect( _DownPoint, cp );

                SetDragBoxLeft( rect.Left );
                SetDragBoxTop( rect.Top );
                dragSelectionBorder.Width = rect.Width;
                dragSelectionBorder.Height = rect.Height;
                return;
            }

            if (PanIsDisabled) return;

            Point p = e.GetPosition( viewerControl );
            mouseLocation = new Point2D( p.X, p.Y );

            if (mouseDown)
			{
				Vector2D delta = mouseLocation - mouseDownLocation;
				translation =
					translationAtMouseClick +
						new Vector3D(
							delta.X * 2d / canvas.ActualWidth,
							-delta.Y * 2d / canvas.ActualHeight,
							0d);
				UpdateRenderTransform(false);
            }
        }

        private void Canvas_LeftButtonUp(object sender, MouseButtonEventArgs e)
		{
            //string iguid = (oImage == null) ? oImage.GUID : "";
            //string iguid = CADModels.GUID;

            if (InZoomWindow)
            {
                Point p = e.GetPosition( viewerControl );
                var p1 = DeviceToWorld( new dxfVector( _DownPoint.X, _DownPoint.Y, 0 ) );
                var p2 = DeviceToWorld( new dxfVector( p.X, p.Y, 0 ) );
                var topleft = new dxfVector( Math.Min( p1.X, p2.X ), Math.Max( p1.Y, p2.Y ), 0 );
                var bottomright = new dxfVector( Math.Max( p1.X, p2.X ), Math.Min( p1.Y, p2.Y ), 0 );
                var cen = new dxfVector( topleft.X + ((bottomright.X - topleft.X) / 2), bottomright.Y + ((topleft.Y - bottomright.Y) / 2.0), 0 );
                var rect = new dxfRectangle( cen, bottomright.X - topleft.X, topleft.Y - bottomright.Y );

                HideDragSelectionRect();
                if (rect.Width > 1 && rect.Height > 1)
                    ZoomWindow( rect );

                InZoomWindow = false;
                Cursor = null;
              
            }
            // Select entity at mouse location if mouse didn't move
            if (model != null && mouseDown && !IgnoreSelection)
            {
                Point p = e.GetPosition( viewerControl );
                Point2D mouseUpLocation = new Point2D( p.X, p.Y );
                if (mouseUpLocation == mouseDownLocation)
                {
                    IList<RenderedEntityInfo> closestEntities;
                    const int testSquareWidth = 4;
                    if (model.ActiveLayout == null || model.Header.ShowModelSpace)
                    {
                        closestEntities = EntitySelector.GetEntitiesCloseToPoint( model, MyGraphicsConfig, projectionTransform/* * centerTransform*/, mouseDownLocation, testSquareWidth );
                    }
                    else
                    {
                        closestEntities = EntitySelector.GetEntitiesCloseToPoint( model, model.ActiveLayout, MyGraphicsConfig, projectionTransform/* * centerTransform*/, mouseDownLocation, testSquareWidth );
                    }

                    // Unhighlight previously highlighted entity.
                    if (highlightedEntity != null)
                    {
                        UnhighlightEntity( highlightedEntity );
                        highlightedEntity = null;
                    }
                    if (closestEntities.Count > 0)
                    {
                        // Chose the last entity as it is drawn last, so will be on top.
                        highlightedEntity = closestEntities[ closestEntities.Count - 1 ];
                        HighlightEntity( highlightedEntity );
                        DxfEntity entity = highlightedEntity.Entity;
                        OnEntitySelected( entity.Handle );
                    }

                    UpdateWpfVisuals();
                }
            }
            if (mouseDown)
            {
                if (LastZoom != null)
                    LastZoom.Set( scaling, MyBounds, translation );
            }

            mouseDown = false;
			canvas.ReleaseMouseCapture();

            OnViewerMouseUp?.Invoke(this, e);
        }

        private void UnhighlightEntity( RenderedEntityInfo entity )
        {
            IList<IWireframeDrawable2> drawables = graphicsCache.GetDrawables( entity );
            IWireframeGraphicsFactory2 graphicsFactory = null;
            wpfGraphics.UpdateDrawables(
                paperSpaceLayout,
                entity,
                () =>
                {
                    foreach (IWireframeDrawable2 drawable in drawables)
                    {
                        drawable.Draw( graphicsFactory );
                    }
                },
                o => graphicsFactory = o
            );
        }

        private void HighlightEntity( RenderedEntityInfo entity )
        {
            IList<IWireframeDrawable2> drawables = graphicsCache.GetDrawables( entity );
            WireframeGraphicsFactory2ColorChanger graphicsFactoryColorChanger = null;
            wpfGraphics.UpdateDrawables(
                paperSpaceLayout,
                entity,
                () =>
                {
                    foreach (IWireframeDrawable2 drawable in drawables)
                    {
                        drawable.Draw( graphicsFactoryColorChanger );
                    }
                },
                o => graphicsFactoryColorChanger = new WireframeGraphicsFactory2ColorChanger( o, ColorChanger )
            );
        }

        protected virtual void OnEntitySelected( ulong entityHandle )
        {
            SelectedEntityHandle = entityHandle;
            EntitySelected?.Invoke(this, entityHandle);
        }

        /// <summary>
        /// Zoom in/out.
        /// </summary>
        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
		{
            if (ZoomIsDisabled /*|| oImage == null*/) return;
          
            PrevZoom?.Set( scaling, MyBounds, translation );

            int sign = Math.Sign(e.Delta);
			double newScaling = scaling;
            double overlayscale = 1;

			// wheel movement is forward 
			if (sign > 0)
			{
				newScaling *= 1.1d;
                overlayscale /= 1.1;
			}
			// wheel movement is backward 
			else if (sign < 0)
			{
				newScaling /= 1.1d;
                overlayscale *= 1.1;
			}
			Point p = e.GetPosition(viewerControl);

			// This is the post-zoom position.
			Point3D postZoomPosition =
				new Point3D(p.X / canvas.ActualWidth * 2d - 1d, -p.Y / canvas.ActualHeight * 2d + 1, 0d);

			// This is the pre-zoom position.
			Matrix4D zoomTranslation =
				Transformation4D.Translation((Vector3D)translation) *
				Transformation4D.Scaling(scaling);
			Point3D preZoomPosition = zoomTranslation.GetInverse().Transform(postZoomPosition);

			Matrix4D newZoomTranslation =
				Transformation4D.Translation((Vector3D)translation) *
				Transformation4D.Scaling(newScaling);
			Point3D uncorrectedPostZoomPosition = newZoomTranslation.Transform(preZoomPosition);

			translation += postZoomPosition - uncorrectedPostZoomPosition;
			scaling = newScaling;

			UpdateRenderTransform(false);
            overlayHandler.AdjustScale( overlayscale );
            LastZoom.Set( scaling, MyBounds, translation );
            e.Handled = true;
        }

        private void Canvas_LostMouseCapture(object sender, MouseEventArgs e)
		{
			mouseDown = false;
		}

		private void UpdateRenderTransform()
        {
			UpdateRenderTransform( true, true, Point3D.Zero );
        }

		private void UpdateRenderTransform(bool recalculateBounds)
        {
			UpdateRenderTransform( recalculateBounds, false , Point3D.Zero );
        }

		private void UpdateRenderTransform(bool recalulateBounds, bool moveCenter, Point3D newCenter)
		{
			if (recalulateBounds)
            {
                BoundsCalculator boundsCalculator = new BoundsCalculator();
                boundsCalculator.GetBounds( model );
                MyBounds = boundsCalculator.Bounds;
                CheckBounds();
            }

            double canvasWidth = canvas.ActualWidth;
			double canvasHeight = canvas.ActualHeight;

			// Update the WPF graphics from the cache to fix the displayed line width (only needed for model space).
			if (model != null && (model.ActiveLayout == null || model.Header.ShowModelSpace))
			{
				Vector3D delta = MyBounds.Delta;
				double scale = Math.Min(canvasWidth / delta.X, canvasHeight / delta.Y);
				MyGraphicsConfig.DotsPerInch = baseDPI / scale;

				// In model space line weight is independent of zoom level.
				MyGraphicsConfig.DotsPerInch /= scaling;

                UpdateWpfGraphics();
			}

			MatrixTransform baseTransform = DxfUtil.GetScaleWMMatrixTransform(
				(Point2D)MyBounds.Corner1,
				(Point2D)MyBounds.Corner2,
				(Point2D)MyBounds.Center,
				new Point2D(1d, canvasHeight),
				new Point2D(canvasWidth, 1d),
				new Point2D(0.5d * (canvasWidth + 1d), 0.5d * (canvasHeight + 1d))
				);

			TransformGroup transformGroup = new TransformGroup();
			transformGroup.Children.Add( new RotateTransform( viewRotation, MyBounds.Center.X, MyBounds.Center.Y ) );
            transformGroup.Children.Add(baseTransform);
			if (moveCenter)
            {
				var cdelta = newCenter - MyBounds.Center;
				transformGroup.Children.Add( new TranslateTransform()
				{
					X = cdelta.X,
					Y = cdelta.Y
				} );
            }
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

            canvas.RenderTransform = transformGroup;
            overlaycanvas.RenderTransform = transformGroup;
            projectionTransform = new Matrix4D(
												transformGroup.Value.M11, transformGroup.Value.M12, 0, transformGroup.Value.OffsetX,
												transformGroup.Value.M21, transformGroup.Value.M22, 0, transformGroup.Value.OffsetY,
												0, 0, 1, 0,
												0, 0, 0, 1
											  );

            overlayHandler.AdjustDPI();
        }

        private void ZoomLast()
        {
            if (LastZoom == null || !LastZoom.IsValid)
            {
                ZoomExtents();
                return;
            }

            CurrentZoomData = LastZoom;

            //double oscl = MyBounds.Delta.GetLength();

            //MyBounds = LastZoom.bounds;
            //Vector3D delta = MyBounds.Delta;
            //double canvasWidth = canvas.ActualWidth;
            //double canvasHeight = canvas.ActualHeight;
            //double estimatedScale = Math.Min( canvasWidth / delta.X, canvasHeight / delta.Y );
            //MyGraphicsConfig.DotsPerInch = baseDPI / estimatedScale;
            //double nscl = MyBounds.Delta.GetLength();

            //scaling = LastZoom.scale;
            //translation = LastZoom.translation;
            //UpdateWpfGraphics();

            //UpdateRenderTransform( false );
            //overlayHandler.AdjustScale( nscl / oscl );
        }

        public ZoomData CurrentZoomData
        {
            get
            {
                UpdateRenderTransform(true);
                return MyBounds == null ? null : new ZoomData(scaling, MyBounds.Clone(), translation);
            }
            set
            {
                if (value == null || !value.IsValid) return;

                try
                {
                     double oscl = MyBounds.Delta.GetLength();

                    MyBounds = value.bounds;
                    Vector3D delta = MyBounds.Delta;
                    double canvasWidth = canvas.ActualWidth;
                    double canvasHeight = canvas.ActualHeight;
                    double estimatedScale = Math.Min(canvasWidth / delta.X, canvasHeight / delta.Y);
                    MyGraphicsConfig.DotsPerInch = baseDPI / estimatedScale;
                    double nscl = MyBounds.Delta.GetLength();

                    scaling = value.scale;
                    translation = value.translation;
                    UpdateWpfGraphics();

                    UpdateRenderTransform(false);
                    overlayHandler.AdjustScale(nscl / oscl);
                    //Correct line weight of overlay entities.
                    overlayHandler.AdjustDPI(); 
                }
                catch { }
               
             
            }
        }

        public void ZoomPrevious()
        {
            if (PrevZoom == null || !PrevZoom.IsValid) return;

            CurrentZoomData = PrevZoom;
            //MyBounds = PrevZoom.bounds;
            //Vector3D delta = MyBounds.Delta;
            //double canvasWidth = canvas.ActualWidth;
            //double canvasHeight = canvas.ActualHeight;
            //double estimatedScale = Math.Min( canvasWidth / delta.X, canvasHeight / delta.Y );
            //MyGraphicsConfig.DotsPerInch = baseDPI / estimatedScale;
            //double nscl = MyBounds.Delta.GetLength();

            //scaling = PrevZoom.scale;
            //translation = PrevZoom.translation;
            //UpdateWpfGraphics();

            //UpdateRenderTransform( false );
            //overlayHandler.AdjustScale( nscl / oscl );
            ////Correct line weight of overlay entities.
            //overlayHandler.AdjustDPI();
            LastZoom.Set( scaling, MyBounds, translation );
        }

        public void ZoomTo(ZoomData zoom)
        {
            if (zoom == null || !zoom.IsValid)
            {
                ZoomExtents();
                return;
            }


            CurrentZoomData = zoom;
            //double oscl = MyBounds.Delta.GetLength();

            //MyBounds = zoom.bounds;
            //Vector3D delta = MyBounds.Delta;
            //double canvasWidth = canvas.ActualWidth;
            //double canvasHeight = canvas.ActualHeight;
            //double estimatedScale = Math.Min(canvasWidth / delta.X, canvasHeight / delta.Y);
            //MyGraphicsConfig.DotsPerInch = baseDPI / estimatedScale;
            //double nscl = MyBounds.Delta.GetLength();

            //scaling = zoom.scale;
            //translation = zoom.translation;
            //UpdateWpfGraphics();

            //UpdateRenderTransform(false);
            //overlayHandler.AdjustScale(nscl / oscl);
            PrevZoom.Set(scaling, MyBounds, translation);
        }


        private ArgbColor ColorChanger( ArgbColor argbColor )
        {
            ArgbColor result = highlightColor;
            if (argbColor == result)
            {
                result = secondaryHighlightColor;
            }
            return result;
        }

        public void ZoomExtents()
        {
			ZoomExtents( 1.0 );
        }

        public void ZoomExtents(double margin)
        {
            if (_Bounds == null) return;

            try
            {
                PrevZoom.Set(scaling, MyBounds, translation);

                BoundsCalculator boundsCalculator = new BoundsCalculator();
                if (model.ActiveLayout == null || model.Header.ShowModelSpace)
                {
                    boundsCalculator.GetBounds(model);
                }
                else
                {
                    boundsCalculator.GetBounds(model, model.ActiveLayout);
                }
                _Bounds = boundsCalculator.Bounds;
                CheckBounds();
                _Bounds.AdjusteForZoom(margin);

                Vector3D delta = _Bounds.Delta;
                Size estimatedCanvasSize = canvas.RenderSize;
                double estimatedScale = Math.Min(estimatedCanvasSize.Width / delta.X, estimatedCanvasSize.Height / delta.Y);
                MyGraphicsConfig.DotsPerInch = baseDPI / estimatedScale;

                scaling = 1;
                translation = new Vector3D();

                UpdateRenderTransform(false);
                overlayHandler.AdjustScale();
                LastZoom.Set(scaling, _Bounds, translation);
                //Correct line weight of overlay entities.
                overlayHandler.AdjustDPI();
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }

        }

        public void ZoomWindow(dxfRectangle rec)
        {
            if (rec == null) return;
            //if (translation == null) translation = new Vector3D(0, 0, 0);
         
                
            PrevZoom.Set( scaling, MyBounds, translation );

            double oscl = MyBounds.Delta.GetLength();
            var bl = rec.BottomLeft;
			var tr = rec.TopRight;
			_Bounds.Corner1 = new Point3D( bl.X, bl.Y, bl.Z );
		    _Bounds.Corner2 = new Point3D( tr.X, tr.Y, tr.Z );
            Vector3D delta = _Bounds.Delta;
            double canvasWidth = canvas.ActualWidth;
            double canvasHeight = canvas.ActualHeight;
            double estimatedScale = Math.Min( canvasWidth / delta.X, canvasHeight / delta.Y );
            MyGraphicsConfig.DotsPerInch = baseDPI / estimatedScale;
            double nscl = _Bounds.Delta.GetLength();
           
            scaling = 1;

            translation = new Vector3D(); // new Vector3D(rec.X, rec.Y, rec.Z) ;
            UpdateWpfGraphics();

            UpdateRenderTransform(false,true, new Point3D(rec.X, rec.Y, rec.Z));
            overlayHandler.AdjustScale( nscl / oscl );
            LastZoom.Set( scaling, MyBounds, translation );
        }
      
       
        public void ZoomWindow()
        {
            if (ZoomIsDisabled) return;
            InZoomWindow = true;
            Cursor = Cursors.Cross;
        }

        public void Zoom(double zoomFactor)
        {
            PrevZoom.Set( scaling, MyBounds, translation );
            scaling *= zoomFactor;

            UpdateRenderTransform();
            overlayHandler.AdjustScale();
            LastZoom.Set( scaling, MyBounds, translation );
        }

        public void SetCenter(Point3D p)
        {
            PrevZoom.Set( scaling, MyBounds, translation );
            UpdateRenderTransform( false, true, p );
            LastZoom.Set( scaling, MyBounds, translation );
        }

        /// <summary>
        /// If the target file already exists, the existing file is renamed.
        /// </summary>
        /// <param name="FileToCheck"></param>
        public static bool ValidateFileName( string FileToCheck, out string rMessage,out string rBackup)
        {
            rMessage = "";
            rBackup = "";
            if (!System.IO.File.Exists(FileToCheck)) return true;

            if (dxfUtils.FileIsInUse(FileToCheck))
            {

                rMessage = "File is in use!! Close it and try again.";
                return false;
            }
            
            string ext = System.IO.Path.GetExtension(FileToCheck);
            
            //if (MessageBox.Show("Overwrite Existing File ?", "UOP WinTray", button: MessageBoxButton.OKCancel, icon: MessageBoxImage.Question) == MessageBoxResult.Cancel) return false;
                
            string file = FileToCheck.Substring( 0, FileToCheck.Length - 4 );
            var dt = System.DateTime.Now;
            int idx = 0;
            while (string.IsNullOrEmpty( rBackup ) || System.IO.File.Exists( rBackup ))
            {
                rBackup = string.Format( "{0}_{1}-{2}-{3}_{4}-{5}BAK{6}{7}", file,
                    dt.Month, dt.Day, dt.Year, dt.Hour, dt.Minute, (idx == 0) ? "" : string.Format( "{0}", idx ), ext );
                idx++;
            }
            try
            {
                // this will fail if the file is in use
                FileStream fs =
                System.IO.File.Open(FileToCheck, FileMode.OpenOrCreate,
                FileAccess.ReadWrite, FileShare.None);
                fs.Close();
                
                System.IO.File.Move(FileToCheck, rBackup);
                return true;
                

                   
            }
            catch (IOException e)
            {
                rMessage = e.Message;
                return false;
            }
        }

        private void PurgeTempEntities()
        {
            foreach (var ent in CADModels.TempEntities)
            {
                model.Entities.Remove( ent );
            }
            //bool purgedSomething = false;
            //for (int i = 1; i <= oImage.Entities.Count; i++)
            //{
            //    var ent = oImage.Entities.Item( i );
            //    if (ent == null) continue;

            //    if (!ent.SaveToFile)
            //    {
            //        purgedSomething = true;
            //        var handles = ent.DxfHandles.ToList();
            //        if (ent.DxfHandles != null && ent.DxfHandles.Count() > 0)
            //        {
            //            for (int hi = 0; hi < handles.Count(); hi++)
            //            {
            //                var hent = modelwh.GetEntity( handles[ hi ] );
            //                if (hent == null) continue;
            //                model.Entities.Remove( hent );
            //            }
            //        }
            //    }
            //}
            //return purgedSomething;
        }

        private void RestoreTempEntities()
        {
            if (CADModels.TempEntities.Count == 0) return;
            model.Entities.AddRange( CADModels.TempEntities );
        }

        private void SetZoom()
        {
            BoundsCalculator boundsCalculator = new BoundsCalculator();
            boundsCalculator.GetBounds( model );
             _Bounds = boundsCalculator.Bounds;

            var vport = model.Zoom( MyBounds );
            //Adjust the viewport because AutoCAD's aspect ratio is typically different enough to push much of the drawing off screen.
            vport.Height *= 1.25;
            vport.Center = new Point2D( vport.Center.X - (MyBounds.Delta.X * 0.25), vport.Center.Y );
        }

        public bool SaveDwg( string filename, out string rMessage, bool scaleToMetric = false)
        {
            rMessage = "";
            string backupfile = "";
            try
            {
                model.Repair(messages);
                PurgeTempEntities();

                if (scaleToMetric)
                    ScaleToMetric();

                if (!ValidateFileName(filename,out rMessage, out backupfile)) return false;

                SetZoom();

                DwgWriter.Write(filename, model);
                //if (needRegen) Regenerate();

                RestoreTempEntities();
                if (!string.IsNullOrEmpty(backupfile)) { try { System.IO.File.Delete(backupfile); } catch(Exception e) { System.Diagnostics.Debug.Write(e.Message); } }
                return true;

            }
            catch (Exception e)
            { 
                rMessage = e.Message;
               // if (!string.IsNullOrEmpty(backupfile)) { try { File.Move(backupfile,filename); } catch { } }
                return false; 
            }

        }

        public bool SaveDxf( string filename, out string rMessage, bool scaleToMetric = false )
        {
            rMessage = "";
            string backupfile = "";
            try
            {
                model.Repair(messages);

                if (scaleToMetric)
                    ScaleToMetric();

                if (!ValidateFileName(filename, out rMessage, out backupfile)) return false;

                SetZoom();

                DxfWriter.Write(filename, model);
                if (!string.IsNullOrEmpty(backupfile)) { try { System.IO.File.Delete(backupfile); } catch (Exception e) { System.Diagnostics.Debug.Write(e.Message); } }
                return true;

            }
            catch (Exception e)
            {
                rMessage = e.Message;
                //if (!string.IsNullOrEmpty(backupfile)) { try { File.Move(backupfile, filename); } catch (Exception e)
                //{
                //    System.Diagnostics.Debug.Write(e.Message);
                //} }
                return false;
            }

        }

        public bool SavePdf (string filename,out string rMessage)
        {
            rMessage = "";
            string backupfile = "";

            try
            {

                if (!ValidateFileName(filename, out rMessage, out backupfile)) return false;
                model.Repair(messages);
                BoundsCalculator boundsCalculator = new BoundsCalculator();
                boundsCalculator.GetBounds(model);
                Bounds3D curbounds = boundsCalculator.Bounds;
                PaperSize paperSize = PaperSizes.GetPaperSize(PaperKind.A4Rotated);
                float pageWidth = (float)paperSize.Width / 100f;
                float pageHeight = (float)paperSize.Height / 100f;
                float margin = 0.5f;
                Matrix4D to2DTransform = DxfUtil.GetScaleTransform(
                    curbounds.Corner1,
                    curbounds.Corner2,
                    new Point3D(curbounds.Center.X, curbounds.Corner2.Y, 0d),
                    new Point3D(new Vector3D(margin, margin, 0d) * PdfExporter.InchToPixel),
                    new Point3D(new Vector3D(pageWidth - margin, pageHeight - margin, 0d) * PdfExporter.InchToPixel),
                    new Point3D(new Vector3D(pageWidth / 2d, pageHeight - margin, 0d) * PdfExporter.InchToPixel)
                );
                using (Stream stream = System.IO.File.Create(filename))
                {
                    PdfExporter pdf = new PdfExporter(stream);
                    pdf.DrawPage(
                        model,
                        MyGraphicsConfig,
                        to2DTransform,
                        paperSize);
                    pdf.EndDocument();
                }
                if (!string.IsNullOrEmpty(backupfile)) { try { System.IO.File.Delete(backupfile); } catch (Exception e) { System.Diagnostics.Debug.Write(e.Message); } }
                return true;
            }
            catch (Exception e)
            {
                rMessage = e.Message;
                //if (!string.IsNullOrEmpty(backupfile)) { try { File.Move(backupfile, filename); } catch (Exception e)
                //{
                //    System.Diagnostics.Debug.Write(e.Message);
                //} }
                return false;
            }

        }

        public void Save(string name)
		{
            string msg;
            model.Repair( messages );
            try
            {
               
                SaveDwg( $@"C:\work\Honeywell UOP\Test Output\{name}test.dwg",out msg);
                SaveDxf( $@"C:\work\Honeywell UOP\Test Output\{name}test.dxf", out msg);
            }
            catch (DxfException ex)
            {
                MessageBox.Show( string.Join( @"\n", ex.Messages.ToList() ) );
            }

            SavePdf( $@"C:\work\Honeywell UOP\Test Output\{name}test.pdf", out msg);
        }

		public void ZoomIn()
		{
            ZoomByIncrement( 1 );
		}

        public void ZoomOut()
        {
            ZoomByIncrement( -1 );
        }

        private void ZoomByIncrement(int direction)
        {
            PrevZoom.Set( scaling, MyBounds, translation );
            if (ZoomIsDisabled) return;

            double newScaling = scaling;
            double overlayscale = 1;

            // wheel movement is forward 
            if (direction > 0)
            {
                newScaling *= 1.1d;
                overlayscale /= 1.1;
            }
            // wheel movement is backward 
            else if (direction < 0)
            {
                newScaling /= 1.1d;
                overlayscale *= 1.1;
            }
            scaling = newScaling;

            UpdateRenderTransform();
            overlayHandler.AdjustScale( overlayscale );
            LastZoom.Set( scaling, MyBounds, translation );
        }

  //      public void load()
		//{
		//	string filename = null;
		//	Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
		//	dialog.Filter = "AutoCad files (*.DXF, *.DWG)|*.dxf;*.dwg";
		//	if (dialog.ShowDialog() == true)
		//	{
		//		filename = dialog.FileName;
		//	}

		//	if (!string.IsNullOrEmpty(filename))
		//	{
		//		string extension = System.IO.Path.GetExtension(filename);
		//		if (string.Compare(extension, ".dwg", true) == 0)
		//		{
		//			model = DwgReader.Read(filename);
		//		}
		//		else
		//		{
		//			model = DxfReader.Read(filename);
		//		}
  //              modelwh = new ModelWithHandles( model );
		//		RefreshDisplay();
  //              ZoomExtents();
		//	}
		//}

        public void SetAttributes( DxfInsert insert, Dictionary<string, string> attValueMap )
        {
            foreach (var att in insert.Attributes)
            {
                if (attValueMap.ContainsKey( att.TagString ))
                    att.Text = attValueMap[ att.TagString ];
            }
        }

        public void SetAttributes( dxeInsert insert, Dictionary<string, string> attValueMap )
        {
            if (insert.DxfHandles == null) return;
            foreach (ulong ihnd in insert.DxfHandles)
            {
                var bref = modelwh.GetEntity( ihnd ) as DxfInsert;
                foreach (var att in bref.Attributes)
                {
                    if (attValueMap.ContainsKey( att.TagString ))
                        att.Text = attValueMap[ att.TagString ];
                }
            }
        }

        public void SetAttributes( Dictionary<string,string> attValueMap)
        {
            foreach (var ent in model.Entities.Where(e => e is DxfInsert))
            {
                var bref = ent as DxfInsert;
                SetAttributes( bref, attValueMap );
            }
        }

        public void SetAttributes( string blockName, Dictionary<string,string> attValueMap)
        {
            foreach (var ent in model.Entities.Where( e => e is DxfInsert && ((DxfInsert)e).Block.Name == blockName ))
            {
                var bref = ent as DxfInsert;
                SetAttributes( bref, attValueMap );
            }
        }

        public void SetTitleBlock(string tbFile, double scale, double xPosition, double yPosition, Dictionary<string,string> attValueMap )
        {
            if (string.IsNullOrEmpty(tbFile) || !System.IO.File.Exists( tbFile ))
            {
                return;
            }
            Insert( tbFile, scale, xPosition, yPosition, true, attValueMap );
        }

        public void Insert(string filename, double scale, double xPosition, double yPosition, bool explode, Dictionary<string, string> attValueMap )
        {
            if (string.IsNullOrEmpty(filename) || !System.IO.File.Exists(filename))
            {
                Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog() {Filter = "AutoCad files (*.DXF, *.DWG)|*.dxf;*.dwg" };
                
                if (dialog.ShowDialog() == true)
                {
                    filename = dialog.FileName;
                }
            }

            if (!string.IsNullOrEmpty( filename ) && System.IO.File.Exists(filename))
            {
               
                string bname = System.IO.Path.GetFileNameWithoutExtension( filename );
                if (!model.Blocks.TryGetValue( bname, out DxfBlock blk))
                {
                    blk = new DxfBlock( bname );
                    model.Blocks.Add( blk );
                    DxfModel blkmodel = DwgReader.Read( filename );
                    DxfBlock modelSpace = blkmodel.ModelLayout.OwnerBlock;

                    CloneContext cloneContext = new CloneContext( blkmodel, model, ReferenceResolutionType.CloneMissing );

                    foreach (DxfEntity entity in modelSpace.Entities)
                    {
                        DxfEntity clonedEntity = (DxfEntity)entity.Clone( cloneContext );
                        blk.Entities.Add( clonedEntity );
                    }

                    foreach (DxfHandledObject indirectObjects in cloneContext.IndirectlyClonedObjects)
                    {
                        var ent = indirectObjects as DxfEntity;
                        if (ent != null)
                         blk.Entities.Add( ent );
                    }
                    
                    cloneContext.ResolveReferences();
                    //Warning this corrects a duplicate handle at save issue but will likely break the link to dxfGraphics handles
                    //that exist before the insert
                    model.ResetHandles();
                }

                var dxfInsert = new DxfInsert(blk) { ScaleFactor = new Vector3D(scale, scale, scale) , InsertionPoint = new WW.Math.Point3D(xPosition, yPosition, 0.0) };
              
                if (explode)
                {
                    var cloneCtx = new CloneContext( model, model, ReferenceResolutionType.CloneMissing );
                    var btrans = dxfInsert.BlockInsertionTransformations;
                    var tConfig = new TransformConfig();
                    foreach (DxfEntity ent in blk.Entities)
                    {
                        DxfEntity newent = (DxfEntity)ent.Clone( cloneCtx );
                        foreach (var tran in btrans)
                            newent.TransformMe( tConfig, tran );
                        model.Entities.Add( newent );
                        if (newent is DxfInsert && attValueMap != null)
                        {
                            SetAttributes( newent as DxfInsert, attValueMap );
                        }
                    }
                }
                else
                {
                    model.Entities.Add( dxfInsert );
                    var atts = dxfInsert.Block.Entities.Where( e => e is DxfAttributeDefinition ).ToList();
                    if (dxfInsert != null)
                    {
                        foreach (var attdef in blk.GetAttributeDefinitions())
                        {
                            if (attValueMap != null && attValueMap.ContainsKey( attdef.TagString ))
                                dxfInsert.AddAttribute( (DxfAttributeDefinition)attdef, attValueMap[ attdef.TagString ] );
                            else
                                dxfInsert.AddAttribute( (DxfAttributeDefinition)attdef, attdef.Text );
                        }
                    }
                }

                RefreshDisplay();
            }
        }

        public double ValidateLTScale(double ltScale)
        {
            return (ltScale != 0) ? ltScale : ProjectedScreenHeight / canvas.ActualHeight / 0.025;
        
            //seem to get best results by always using this calculation
           // return (ProjectedScreenHeight / canvas.ActualHeight) / 0.025;
        }

        public void ResetHeader( UOP.DXFGraphics.dxfImage iImage )
        {
            if (model == null)
                return;

            //copy table entries
            CopyImageTables(iImage);

            var mheader = model.Header;
            var iheader = iImage.Header;

            mheader.AcadVersion = DxfVersion.Dxf32;
			mheader.CurrentEntityColor = WW.Cad.Model.Color.CreateFromColorIndex( (short)iheader.Color );
            mheader.DimensionStyle = this.GetDimensionStyle( iheader.DimStyleName );
            mheader.CurrentLayer = this.GetLayer( iheader.LayerName );
            mheader.CurrentEntityLineType = this.GetLineType( iheader.Linetype );
            mheader.LineTypeScale = ValidateLTScale( iheader.LineTypeScale);
			mheader.CurrentEntityLinetypeScale = ValidateLTScale( iheader.LineTypeScaleEnt);
			mheader.CurrentEntityLineWeight = (short)iheader.LineWeight;
			mheader.DisplayLineWeight = iheader.LineWeightDisplay;
			mheader.MirrorText = iheader.MirrorText;
            mheader.PointDisplayMode = (PointDisplayMode)iheader.PointMode;
            mheader.PointDisplaySize = iheader.PointSize;
			mheader.PolylineWidthDefault = iheader.PolylineWidth;
            mheader.QuickTextMode = iheader.QuickText;
            mheader.TextHeightDefault = iheader.TextSize;
            mheader.CurrentTextStyle = this.GetTextStyle( iheader.TextStyleName );
			mheader.TraceWidthDefault = iheader.TraceWidth;

            CADModels.UCSMode = (int)iheader.UCSMode;
            CADModels.UCSSize = iheader.UCSSize;
            CADModels.UCSColor = iheader.UCSColor;

          

            //add block definitions
            using (Utils.TransformUtil transformUtil = new Utils.TransformUtil( this, true ))
            {
                List<dxfBlock> iblocks = iImage.Blocks.FindAll((x) => !x.Name.StartsWith("*"));
               
                foreach (var item in iblocks)
                {
                    //if (item.Entities == null || modelwh.Model.Blocks.TryGetValue( item.Name, out DxfBlock existingblk )) continue;
                    if (item.Entities == null || modelwh.Model.Blocks.Contains( item.Name ) ) continue;
                    DxfBlock newblock = new DxfBlock(item.Name);
                    modelwh.Model.Blocks.Add(newblock);
                   
                    colDXFEntities bents = item.Entities;
                    foreach (var ent in bents)
                    {
                        transformUtil.TransformToDXF(ent, newblock.Entities, iImage);
                    }
                }
                

            }


            DxfDimensionStyle cdstyle = this.GetDimensionStyle(iheader.DimStyleName);
            this.SetDimStyleSettings(cdstyle, iImage.DimStyleOverrides);
            DxfHeader hdr = model.Header;
            mheader.DimensionStyleOverrides.CopyFrom(cloneContext:new CloneContext(model,model, ReferenceResolutionType.IgnoreMissing), from:  new DxfDimensionStyleOverrides(cdstyle, model), copyOverridesOnly: false);
        
            hdr.DimensionStyle = cdstyle;
        }

        public void CopyImageTables(dxfImage aImage)
        {
            if (aImage == null) return;

            dxoTables tables = aImage.Tables;
            foreach (var table in tables)
            {
                switch (table.TableType)
                {
                    case dxxReferenceTypes.LTYPE:
                        try
                        {
                            //add linetypes
                            DxfLineTypeCollection mltypes = model.LineTypes;
                            foreach (var entry in table)
                            {
                                if (string.Compare(entry.Name, "Invisible", true) == 0) continue;
                                if (!mltypes.TryGetValue(entry.Name, out DxfLineType ltp))
                                {
                                    ltp = new DxfLineType(entry.Name);
                                    var vc = new ValidationContext(modelwh.Model);
                                    if (ltp.Validate(vc, messages)) mltypes.Add(ltp);
                                }
                                Extensions.SetLineTypeSettings(ltp, (dxoLinetype)entry);
                            }
                        }
                        catch (Exception ex) { Console.WriteLine(ex.Message); }
                        break;
                    case dxxReferenceTypes.LAYER:
                        try
                        {
                            //add layers
                            DxfLayerCollection mlayers = model.Layers;
                            foreach (var entry in table)
                            {
                                dxoLayer ilayer = (dxoLayer)entry;
                                if (!mlayers.TryGetValue(entry.Name, out DxfLayer layer))
                                {
                                    layer = new DxfLayer(entry.Name);
                                    var vc = new ValidationContext(modelwh.Model);
                                    this.SetLayerSettings(layer, ilayer);
                                    if (layer.Validate(vc, messages)) mlayers.Add(layer);
                                }
                                else { this.SetLayerSettings(layer, ilayer); }
                            }

                        }
                        catch (Exception ex) { Console.WriteLine(ex.Message); }
                        break;
                    case dxxReferenceTypes.STYLE:
                        try
                        {
                            //add text styles
                            DxfTextStyleCollection mstyles = model.TextStyles;
                            foreach (var entry in table)
                            {
                                dxoStyle istyle = (dxoStyle)entry;
                                if (!mstyles.TryGetValue(entry.Name, out DxfTextStyle tstyle))
                                {
                                    tstyle = new DxfTextStyle(istyle.Name, istyle.FontFileName) { LastHeightUsed = istyle.LastHeight };
                                    mstyles.Add(tstyle);
                                    this.SetTextStyleSettings(tstyle, istyle);
                                    var vc = new ValidationContext(modelwh.Model);
                                    tstyle.Validate(vc, messages);
                                }
                                else { this.SetTextStyleSettings(tstyle, istyle);}
                           
                            }

                        }
                        catch (Exception ex) { Console.WriteLine(ex.Message); }
                        break;
                    case dxxReferenceTypes.DIMSTYLE:
                        try
                        {
                            //add dimension styles
                            DxfDimensionStyleCollection mdstyles = model.DimensionStyles;
                          
                            foreach (var entry in table)
                            {
                                dxoDimStyle idstyle = (dxoDimStyle)entry;
                                if (!mdstyles.TryGetValue(entry.Name, out DxfDimensionStyle dstyle))
                                {
                                    dstyle = new DxfDimensionStyle(model) { Name = idstyle.Name };
                                    mdstyles.Add(dstyle);
                                    this.SetDimStyleSettings(dstyle, idstyle);
                                    var vc = new ValidationContext(modelwh.Model);
                                    dstyle.Validate(vc, messages);
                                }
                                else { this.SetDimStyleSettings(dstyle, idstyle); }
                                
                            }
                        }
                        catch (Exception ex) { Console.WriteLine(ex.Message); }
                        break;
                    
                    case dxxReferenceTypes.VIEW:
                        try
                        {
                            DxfViewCollection mviews = model.Views;
                            foreach (var entry in table)
                            {
                                dxoView iview = (dxoView)entry;
                                if (!mviews.TryGetValue(entry.Name, out DxfView view))
                                {
                                    view = new DxfView(entry.Name);
                                    this.SetViewSettings(view, iview);
                                    var vc = new ValidationContext(modelwh.Model);
                                    view.Validate(vc, messages);
                                    mviews.Add(view);
                                }else { this.SetViewSettings(view, iview); }
                            }
                        }
                        catch (Exception ex) { Console.WriteLine(ex.Message); }
                        break;
                    
                    case dxxReferenceTypes.UCS:
                        try
                        {
                            DxfUcsCollection mucss = model.UcsCollection;
                            foreach (var entry in table)
                            {
                                dxoUCS iUcs = (dxoUCS)entry;
                                if (!mucss.TryGetValue(entry.Name, out DxfUcs ucs))
                                {
                                    ucs = new DxfUcs(entry.Name);
                                    this.SetUcsSettings(ucs, iUcs);
                                    var vc = new ValidationContext(modelwh.Model);
                                    ucs.Validate(vc, messages);
                                    mucss.Add(ucs);
                                }
                                else { this.SetUcsSettings(ucs, iUcs); }
                            }
                        }
                        catch (Exception ex) { Console.WriteLine(ex.Message); }
                        break;
                    case dxxReferenceTypes.VPORT:
                        try
                        {
                            DxfVPortCollection mvports = model.VPorts;
                            foreach (var entry in table)
                            {
                                if (!mvports.TryGetValue(entry.Name, out DxfVPort vport))
                                {
                                    vport = new DxfVPort(entry.Name);
                                    //need to convert
                                }
                            }
                        }
                        catch (Exception ex) { Console.WriteLine(ex.Message); }
                        break;
                    // we donty copy these
                    case dxxReferenceTypes.APPID:
                    case dxxReferenceTypes.BLOCK_RECORD:
                        break;
                }
            }
        }

        public System.Windows.Media.Brush GetBackground(object color)
        {
          
            System.Windows.Media.Color clr = System.Windows.Media.Color.FromArgb( 0, 0, 0, 0 );
            if (color is System.Drawing.Color)
            {
                var clr2 = (System.Drawing.Color)color;
                clr2.ToArgb();
                clr = System.Windows.Media.Color.FromArgb( clr2.A, clr2.R, clr2.G, clr2.B );
            }
            if (color is int)
            {
                var byteAry = BitConverter.GetBytes( (int)color );
                clr = System.Windows.Media.Color.FromRgb( byteAry[ 0 ], byteAry[ 1 ], byteAry[ 2 ] );
            }
            else if (color is string)
            {
                if (color.ToString().Contains( "A=" ))
                {
                    var values = color.ToString().TrimEnd( ']' ).Split( ',' );
                    int aVal = Convert.ToInt32( values[ 0 ].Substring( values[ 0 ].LastIndexOf( "=" ) + 1 ) );
                    int rVal = Convert.ToInt32( values[ 1 ].Substring( values[ 1 ].LastIndexOf( "=" ) + 1 ) );
                    int gVal = Convert.ToInt32( values[ 2 ].Substring( values[ 2 ].LastIndexOf( "=" ) + 1 ) );
                    int bVal = Convert.ToInt32( values[ 3 ].Substring( values[ 3 ].LastIndexOf( "=" ) + 1 ) );
                    clr = System.Windows.Media.Color.FromArgb( Convert.ToByte( aVal ), Convert.ToByte( rVal ), Convert.ToByte( gVal ), Convert.ToByte( bVal ) );
                }
                else
                {
                    var clr2 = System.Drawing.Color.FromName( "Silver" );
                    clr2.ToArgb();
                    clr = System.Windows.Media.Color.FromArgb( clr2.A, clr2.R, clr2.G, clr2.B );
                }
            }
            if (color is System.Windows.Media.Brush)
                Background = color as System.Windows.Media.Brush;
            else
                try { Background = new System.Windows.Media.SolidColorBrush(clr); } catch (Exception e) { System.Diagnostics.Debug.Write(e.Message); }
                
            return Background;
        }

        public void SetBackgroundColor(object color)
        {
            double dotsperinch = graphicsCache.Config.DotsPerInch;
            System.Windows.Media.Color clr = System.Windows.Media.Color.FromArgb( 0, 0, 0, 0 );
            if (color is System.Drawing.Color)
            {
                var clr2 = (System.Drawing.Color)color;
                clr2.ToArgb();
                clr = System.Windows.Media.Color.FromArgb( clr2.A, clr2.R, clr2.G, clr2.B );
            }
            if (color is int)
            {
                var byteAry = BitConverter.GetBytes( (int)color );
                clr = System.Windows.Media.Color.FromRgb( byteAry[ 0 ], byteAry[ 1 ], byteAry[ 2 ] );
            }
            else if (color is string)
            {
                if (color.ToString().Contains( "A=" ))
                {
                    var values = color.ToString().TrimEnd( ']' ).Split( ',' );
                    int aVal = Convert.ToInt32( values[ 0 ].Substring( values[ 0 ].LastIndexOf( "=" ) + 1 ) );
                    int rVal = Convert.ToInt32( values[ 1 ].Substring( values[ 1 ].LastIndexOf( "=" ) + 1 ) );
                    int gVal = Convert.ToInt32( values[ 2 ].Substring( values[ 2 ].LastIndexOf( "=" ) + 1 ) );
                    int bVal = Convert.ToInt32( values[ 3 ].Substring( values[ 3 ].LastIndexOf( "=" ) + 1 ) );
                    clr = System.Windows.Media.Color.FromArgb( Convert.ToByte( aVal ), Convert.ToByte( rVal ), Convert.ToByte( gVal ), Convert.ToByte( bVal ) );
                }
                else
                {
                    var clr2 = System.Drawing.Color.FromName( "Silver" );
                    clr2.ToArgb();
                    clr = System.Windows.Media.Color.FromArgb( clr2.A, clr2.R, clr2.G, clr2.B );
                }
            }
            if (color is System.Windows.Media.Brush)
                Background = color as System.Windows.Media.Brush;
            else
                Background = new System.Windows.Media.SolidColorBrush( clr );

            if (GetBrightness( clr ) < 35)
            {
                MyGraphicsConfig = (GraphicsConfig)GraphicsConfig.BlackBackgroundCorrectForBackColor.Clone();
            }
            else
            {
                MyGraphicsConfig = (GraphicsConfig)GraphicsConfig.WhiteBackgroundCorrectForBackColor.Clone();
            }
            MyGraphicsConfig.DotsPerInch = dotsperinch;
            graphicsCache.Config = MyGraphicsConfig;
            wpfGraphics.Config = MyGraphicsConfig;
            MyGraphicsConfig.NoOfArcLineSegments = arcResolution;
            CADModels.Background = Background;
        }

        public void Clear()
        {

            try
            {
                transformUtil?.Dispose();
                transformUtil = null;
                try { canvas.Children.Clear(); } catch (Exception e) { System.Diagnostics.Debug.Write(e.Message); }
                canvas.SizeChanged -= canvas_SizeChanged;
                //canvas = null;
                visualContainer = null;
                CADModels?.Clear();
                //if (oImage != null)
                //{
                //    oImage.EntitiesEvent -= EntitiesEventHandler;
                //    oImage.SettingChange -= SettingsChangeHandler;
                //    oImage.TableEvent -= TableEventHandler;
                //    oImage.TableMemberEvent -= TableMemeberEventHandler;
                //    oImage.ObjectEvent -= ObjectEventHandler;
                //    oImage.ObjectsEvent -= ObjectsEventHandler;
                //    oImage.RenderEvent -= RenderEventHandler;
                //    oImage.StatusChange -= StatusChangeEventHandler;
                //    oImage.EntitiesUpdateEvent -= EntitiesUpdateEventHandler;
                //    //oImage.OverlayEvent -= OverlayerEventHandler;
                //    //oImage.OverlayBmpEvent -= OverlayBmpEventHandler;
                //    oImage.ScreenRender -= ScreenBmpEventHandler;
                //    oImage.ZoomEvent -= ZoomEventHandler;
                //    oImage.ViewRotateEvent -= RotateEventHandler;
                //    oImage.ViewRegenerate -= ViewRegenerateEventHandler;
                //    oImage.SaveToFileEvent -= SaveToFile;
                //    oImage.ScreenDrawingEvent -= ScreenDrawingEventHandler;
                //}
                overlaycanvas.SizeChanged -= canvas_SizeChanged;
                staticoverlaycanvas.SizeChanged -= canvas_SizeChanged;
                try { overlayHandler?.Dispose(); } catch (Exception e) { System.Diagnostics.Debug.Write(e.Message); }
                try { staticOverlayHandler?.Dispose(); } catch (Exception e) { System.Diagnostics.Debug.Write(e.Message); }
                }
            catch (Exception e) { System.Diagnostics.Debug.Write(e.Message); }
            finally
            {
                CADModels = null;

                //oImage?.Dispose();
                //oImage = null;

            }
        }

        public void ClearEntities()
        {
            model.Entities.Clear();
        }

        public void ScaleToMetric()
        {
            var tran = Transformation4D.Scaling( 25.4 );
            var tConfig = new TransformConfig();
            var curdimstyle = model.CurrentDimensionStyle;
            var dimorides = model.Header.DimensionStyleOverrides;

            curdimstyle.ScaleFactor *= 25.4;
            curdimstyle.AlternateUnitScaleFactor = 1 / 25.4;
            curdimstyle.LinearScaleFactor /= 25.4;
            curdimstyle.ZeroHandling = ZeroHandling.ShowZeroFeetAndInches;
            curdimstyle.AlternateUnitZeroHandling = ZeroHandling.ShowZeroFeetAndInches;
            curdimstyle.AngularZeroHandling = ZeroHandling.ShowZeroFeetAndInches;

           dimorides.BaseDimensionStyle = curdimstyle;

           dimorides.ScaleFactor = curdimstyle.ScaleFactor;
           dimorides.AlternateUnitScaleFactor = curdimstyle.AlternateUnitScaleFactor;
           dimorides.LinearScaleFactor = curdimstyle.LinearScaleFactor;
           dimorides.ZeroHandling = ZeroHandling.ShowZeroFeetSuppressZeroInches;
           dimorides.AlternateUnitZeroHandling = ZeroHandling.ShowZeroFeetSuppressZeroInches;
           dimorides.AngularZeroHandling = ZeroHandling.ShowZeroFeetSuppressZeroInches;
            model.Header.LineTypeScale *= 25.4;

            model.Entities.ToList().ForEach( e => { 
                e.TransformMe( tConfig, tran );
                if (e is DxfDimension)
                {
                    var d = e as DxfDimension;
                    d.DimensionStyleOverrides.LinearScaleFactor /= 25.4;
                    d.DimensionStyleOverrides.ScaleFactor *= 25.4;
                    d.DimensionStyleOverrides.AlternateUnitScaleFactor = 1 / 25.4;
                    d.DimensionStyleOverrides.ZeroHandling = ZeroHandling.ShowZeroFeetSuppressZeroInches;
                    d.DimensionStyleOverrides.AlternateUnitZeroHandling = ZeroHandling.ShowZeroFeetSuppressZeroInches;
                    d.DimensionStyleOverrides.AngularZeroHandling = ZeroHandling.ShowZeroFeetSuppressZeroInches;
                    d.GenerateBlock();
                }
                if (e is DxfLeader)
                {
                    var l = e as DxfLeader;
                    l.DimensionStyleOverrides.LinearScaleFactor /= 25.4;
                    l.DimensionStyleOverrides.ScaleFactor *= 25.4;
                    l.DimensionStyleOverrides.AlternateUnitScaleFactor = 1 / 25.4;
                    l.DimensionStyleOverrides.ZeroHandling = ZeroHandling.ShowZeroFeetSuppressZeroInches;
                    l.DimensionStyleOverrides.AlternateUnitZeroHandling = ZeroHandling.ShowZeroFeetSuppressZeroInches;
                    l.DimensionStyleOverrides.AngularZeroHandling = ZeroHandling.ShowZeroFeetSuppressZeroInches;
                }
            } );
        }

        public void Dispose()
        {
            model?.Dispose();
            overlayHandler?.Dispose();

        }
    }
}
