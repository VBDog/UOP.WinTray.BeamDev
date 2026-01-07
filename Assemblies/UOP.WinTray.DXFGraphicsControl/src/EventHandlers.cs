using UOP.DXFGraphicsControl.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using WW.Cad.Model;
using WW.Cad.Model.Entities;
using WW.Cad.Model.Tables;
using WW.Math;
using UOP.DXFGraphics;

namespace UOP.DXFGraphicsControl
{
    public partial class DXFViewer : UserControl
    {
        private TransformUtil transformUtil { get; set; }

        private WriteableBitmap FromBitmap( System.Drawing.Bitmap image )
        {
            if (image == null)
            {
                return null;
            }
            var ms = new System.IO.MemoryStream();
            image.Save( ms, System.Drawing.Imaging.ImageFormat.Png );
            ms.Position = 0;
            var bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();
            return new WriteableBitmap( bi );
        }

        private int GetBrightness(System.Windows.Media.Color clr)
        {
            //Adapted from color_RGBToHSB
            int R = clr.R;
            int G = clr.G;
            int B = clr.B;
            int lMax = Math.Max( Math.Max( R, G ), B );
            return lMax * 100 / 255;
        }

        private System.Windows.Media.ImageSource ConvertImage( System.Drawing.Image image )
        {
            try
            {
                if (image != null)
                {
                    var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                    bitmap.BeginInit();
                    System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
                    image.Save( memoryStream, System.Drawing.Imaging.ImageFormat.Png );
                    memoryStream.Seek( 0, System.IO.SeekOrigin.Begin );
                    bitmap.StreamSource = memoryStream;
                    bitmap.EndInit();
                    return bitmap;
                }
            }
            catch { }
            return null;
        }

        private void ScreenBmpEventHandler( dxfBitmap aBitmap)
        {
            //staticbmpoverlaycanvas.Source = ConvertImage(aBitmap.Image);
        }

        private void EntitiesUpdateEventHandler( ref bool Remove, ref dxfEntity Entity )
        {
            if (transformUtil == null || transformUtil.isDisposed)
                transformUtil = new TransformUtil( this );

            if (Remove && Entity.DxfHandles != null)
            {
                var handles = Entity.DxfHandles.ToList();
                if (Entity.DxfHandles != null && Entity.DxfHandles.Count() > 0)
                {
                    for (int i = 0; i < handles.Count(); i++)
                    {
                        var ent = modelwh.GetEntity( handles[ i ] );
                       if(ent != null)  model.Entities.Remove( ent );
                    }
                }
                Array.Clear( Entity.DxfHandles, 0, Entity.DxfHandles.Length );
            }

            var entities = transformUtil.TransformToDXF( Entity );
            RefreshDisplay( false );
        }

        public void UpdateEntity( ref bool Remove, ref dxfEntity Entity )
        {
            UpdateEntity( Entity, Remove );
        }

        public void UpdateEntity(dxfEntity Entity, bool Remove = false)
        {
            if (transformUtil == null || transformUtil.isDisposed)
                transformUtil = new TransformUtil( this );

            if (Remove)
            {
                var handles = Entity.DxfHandles.ToList();
                if (Entity.DxfHandles != null && Entity.DxfHandles.Count() > 0)
                {
                    for (int i = 0; i < handles.Count(); i++)
                    {
                        var ent = modelwh.GetEntity( handles[ i ] );
                        model.Entities.Remove( ent );
                    }
                }
                RefreshDisplay( false );
            }

            var entities = transformUtil.TransformToDXF( Entity );
            RefreshDisplay( false );
        }

        private List<DxfEntity> ConvertDXFGraphicsEntity(ref UOP.DXFGraphics.dxfEntity Entity, dxfImage iImage)
        {
            if(Entity == null) return new List<DxfEntity>();

            if (transformUtil == null || transformUtil.isDisposed)
                transformUtil = new TransformUtil(this);

          
                switch (Entity.EntityType)
                {
                    case UOP.DXFGraphics.dxxEntityTypes.Character:
                    case UOP.DXFGraphics.dxxEntityTypes.SequenceEnd:
                    case UOP.DXFGraphics.dxxEntityTypes.EndBlock:
                    case UOP.DXFGraphics.dxxEntityTypes.Undefined:
                        return new List<DxfEntity>();
                case UOP.DXFGraphics.dxxEntityTypes.LeaderTolerance:
                    //Not used in WinTray no examples
                    return new List<DxfEntity>();
                case UOP.DXFGraphics.dxxEntityTypes.Line:
                       return   transformUtil.TransformToDXF(Entity, iImage);
                    case UOP.DXFGraphics.dxxEntityTypes.Polyline:
                        return transformUtil.TransformToDXF(Entity, iImage);
                    case UOP.DXFGraphics.dxxEntityTypes.Arc:
                        return transformUtil.TransformToDXF(Entity, iImage);
                    case UOP.DXFGraphics.dxxEntityTypes.Circle:
                        return transformUtil.TransformToDXF(Entity, iImage);
                    case UOP.DXFGraphics.dxxEntityTypes.Ellipse:
                        return transformUtil.TransformToDXF(Entity, iImage);
                    case UOP.DXFGraphics.dxxEntityTypes.Bezier:
                        return transformUtil.TransformToDXF(Entity, iImage);
                    case UOP.DXFGraphics.dxxEntityTypes.Point:
                        return transformUtil.TransformToDXF(Entity, iImage);
                    case UOP.DXFGraphics.dxxEntityTypes.Trace:
                    case UOP.DXFGraphics.dxxEntityTypes.Solid:
                        return transformUtil.TransformToDXF(Entity, iImage);
                    case UOP.DXFGraphics.dxxEntityTypes.Insert:
                        return transformUtil.TransformToDXF(Entity, iImage);
                    case UOP.DXFGraphics.dxxEntityTypes.Table:
                        return transformUtil.TransformToDXF(Entity, iImage);
                    case UOP.DXFGraphics.dxxEntityTypes.Polygon:
                        return transformUtil.TransformToDXF(Entity, iImage);
                    case UOP.DXFGraphics.dxxEntityTypes.Hole:
                    case UOP.DXFGraphics.dxxEntityTypes.Slot:
                        return transformUtil.TransformToDXF(Entity, iImage);
                    case UOP.DXFGraphics.dxxEntityTypes.Text:
                        return transformUtil.TransformToDXF(Entity, iImage);
                    case UOP.DXFGraphics.dxxEntityTypes.Attdef:
                        return transformUtil.TransformToDXF(Entity, iImage);
                    case UOP.DXFGraphics.dxxEntityTypes.Attribute:
                        return transformUtil.TransformToDXF(Entity, iImage);
                    case UOP.DXFGraphics.dxxEntityTypes.MText:
                        return transformUtil.TransformToDXF(Entity, iImage);
                    case UOP.DXFGraphics.dxxEntityTypes.Hatch:
                        return transformUtil.TransformToDXF(Entity, iImage);
                    case UOP.DXFGraphics.dxxEntityTypes.DimLinearH:
                        return transformUtil.TransformToDXF(Entity, iImage);
                    case UOP.DXFGraphics.dxxEntityTypes.DimLinearV:
                        return transformUtil.TransformToDXF(Entity, iImage);
                    case UOP.DXFGraphics.dxxEntityTypes.DimLinearA:
                        return transformUtil.TransformToDXF(Entity, iImage);
                    case UOP.DXFGraphics.dxxEntityTypes.DimOrdinateH:
                        return transformUtil.TransformToDXF(Entity, iImage);
                    case UOP.DXFGraphics.dxxEntityTypes.DimOrdinateV:
                        return transformUtil.TransformToDXF(Entity, iImage);
                    case UOP.DXFGraphics.dxxEntityTypes.DimRadialR:
                        return transformUtil.TransformToDXF(Entity, iImage);
                    case UOP.DXFGraphics.dxxEntityTypes.DimRadialD:
                        return transformUtil.TransformToDXF(Entity, iImage);
                    case UOP.DXFGraphics.dxxEntityTypes.DimAngular:
                        return transformUtil.TransformToDXF(Entity, iImage);
                    case UOP.DXFGraphics.dxxEntityTypes.DimAngular3P:
                        return transformUtil.TransformToDXF(Entity, iImage);
                    case UOP.DXFGraphics.dxxEntityTypes.LeaderBlock:
                    case UOP.DXFGraphics.dxxEntityTypes.Leader:
                    case UOP.DXFGraphics.dxxEntityTypes.LeaderText:
                        return transformUtil.TransformToDXF(Entity, iImage);
                case UOP.DXFGraphics.dxxEntityTypes.Symbol:
                    return transformUtil.TransformToDXF(Entity, iImage);
                case UOP.DXFGraphics.dxxEntityTypes.Shape:
                    return transformUtil.TransformToDXF(Entity, iImage);
                default:
                    return new List<DxfEntity>();
            }
          
        }


        private void EntitiesEventHandler( bool Added, ref UOP.DXFGraphics.dxfEntity Entity, dxfImage iImage )
        {
            if (transformUtil == null || transformUtil.isDisposed)
                transformUtil = new TransformUtil(this);

            if (Added)
            {
                switch (Entity.EntityType)
                {
                    case UOP.DXFGraphics.dxxEntityTypes.SequenceEnd:
                        //Not Supported / used
                        break;
                    case UOP.DXFGraphics.dxxEntityTypes.EndBlock:
                        //Not Supported / used
                        break;
                    case UOP.DXFGraphics.dxxEntityTypes.Undefined:
                        //Not Supported / used
                        break;
                    case UOP.DXFGraphics.dxxEntityTypes.Line:
                        List<DxfEntity> lines = transformUtil.TransformToDXF(Entity, iImage);
                        RefreshDisplay(lines, Entity.SaveToFile);
                        break;
                    case UOP.DXFGraphics.dxxEntityTypes.Polyline:
                        List<DxfEntity> polyLines = transformUtil.TransformToDXF(Entity, iImage);
                        RefreshDisplay(polyLines, Entity.SaveToFile );
                        break;
                    case UOP.DXFGraphics.dxxEntityTypes.Arc:
                        List<DxfEntity> arcs = transformUtil.TransformToDXF(Entity, iImage);
                        RefreshDisplay(arcs, Entity.SaveToFile );
                        break;
                    case UOP.DXFGraphics.dxxEntityTypes.Circle:
                        List<DxfEntity> circles = transformUtil.TransformToDXF(Entity, iImage);
                        RefreshDisplay(circles, Entity.SaveToFile );
                        break;
                    case UOP.DXFGraphics.dxxEntityTypes.Ellipse:
                        List<DxfEntity> ellipses = transformUtil.TransformToDXF(Entity);
                        RefreshDisplay(ellipses, Entity.SaveToFile );
                        break;
                    case UOP.DXFGraphics.dxxEntityTypes.Bezier:
                        List<DxfEntity> beziers = transformUtil.TransformToDXF( Entity );
                        RefreshDisplay( beziers, Entity.SaveToFile );
                        break;
                      case UOP.DXFGraphics.dxxEntityTypes.Point:
                        List<DxfEntity> points = transformUtil.TransformToDXF(Entity);
                        RefreshDisplay(points, Entity.SaveToFile );
                        break;
                    case UOP.DXFGraphics.dxxEntityTypes.Trace:
                    case UOP.DXFGraphics.dxxEntityTypes.Solid:
                        List<DxfEntity> solids = transformUtil.TransformToDXF(Entity);
                        RefreshDisplay(solids, Entity.SaveToFile );
                        break;
                   
                    case UOP.DXFGraphics.dxxEntityTypes.Insert:
                        List<DxfEntity> inserts = transformUtil.TransformToDXF(Entity, iImage);
                        RefreshDisplay(inserts, Entity.SaveToFile );
                        break;
                    case UOP.DXFGraphics.dxxEntityTypes.Table:
                        List<DxfEntity> tables = transformUtil.TransformToDXF(Entity, iImage );
                        RefreshDisplay(tables, Entity.SaveToFile );
                        break;
                    case UOP.DXFGraphics.dxxEntityTypes.Polygon:
                        List<DxfEntity> polygons = transformUtil.TransformToDXF(Entity, iImage );
                        RefreshDisplay(polygons, Entity.SaveToFile );
                        break;
                       
                    case UOP.DXFGraphics.dxxEntityTypes.Hole:
                        List<DxfEntity> holes = transformUtil.TransformToDXF(Entity, iImage );
                        RefreshDisplay(holes, Entity.SaveToFile );
                        break;
                    case UOP.DXFGraphics.dxxEntityTypes.Slot:
                        List<DxfEntity> slots = transformUtil.TransformToDXF(Entity, iImage );
                        RefreshDisplay(slots, Entity.SaveToFile );
                        break;
                    case UOP.DXFGraphics.dxxEntityTypes.Text:
                        List<DxfEntity> texts = transformUtil.TransformToDXF(Entity);
                        RefreshDisplay(texts, Entity.SaveToFile);
                        break;
                    case UOP.DXFGraphics.dxxEntityTypes.Attdef:
                        transformUtil.TransformToDXF( Entity );
                        break;
                    case UOP.DXFGraphics.dxxEntityTypes.Attribute:
                        //Handled by inserts
                        break;
                    case UOP.DXFGraphics.dxxEntityTypes.MText:
                        List<DxfEntity> mtexts = transformUtil.TransformToDXF(Entity);
                        RefreshDisplay(mtexts, Entity.SaveToFile );
                        break;
                    case UOP.DXFGraphics.dxxEntityTypes.Character:
                        //Not supported
                        break;
                    case UOP.DXFGraphics.dxxEntityTypes.Hatch:
                        List<DxfEntity> hatches = transformUtil.TransformToDXF( Entity );
                        RefreshDisplay( hatches, Entity.SaveToFile );
                        break;
                    case UOP.DXFGraphics.dxxEntityTypes.DimLinearH:
                        List<DxfEntity> dimensions_LH = transformUtil.TransformToDXF(Entity);
                        RefreshDisplay(dimensions_LH, Entity.SaveToFile );
                        break;
                    case UOP.DXFGraphics.dxxEntityTypes.DimLinearV:
                        List<DxfEntity> dimensions_LV = transformUtil.TransformToDXF(Entity);
                        RefreshDisplay(dimensions_LV, Entity.SaveToFile );
                        break;
                    case UOP.DXFGraphics.dxxEntityTypes.DimLinearA:
                        List<DxfEntity> dimensions_LA = transformUtil.TransformToDXF(Entity);
                        RefreshDisplay(dimensions_LA, Entity.SaveToFile );
                        break;
                    case UOP.DXFGraphics.dxxEntityTypes.DimOrdinateH:
                        List<DxfEntity> dimensions_OH = transformUtil.TransformToDXF(Entity);
                        RefreshDisplay(dimensions_OH, Entity.SaveToFile );
                        break;
                    case UOP.DXFGraphics.dxxEntityTypes.DimOrdinateV:
                        List<DxfEntity> dimensions_OV = transformUtil.TransformToDXF(Entity);
                        RefreshDisplay(dimensions_OV, Entity.SaveToFile );
                        break;
                    case UOP.DXFGraphics.dxxEntityTypes.DimRadialR:
                        List<DxfEntity> dimensions_RR = transformUtil.TransformToDXF(Entity);
                        RefreshDisplay(dimensions_RR, Entity.SaveToFile );
                        break;
                    case UOP.DXFGraphics.dxxEntityTypes.DimRadialD:
                        List<DxfEntity> dimensions_RD = transformUtil.TransformToDXF(Entity);
                        RefreshDisplay(dimensions_RD, Entity.SaveToFile );
                        break;
                    case UOP.DXFGraphics.dxxEntityTypes.DimAngular:
                        List<DxfEntity> dimensions_Ang4P = transformUtil.TransformToDXF(Entity);
                        RefreshDisplay(dimensions_Ang4P, Entity.SaveToFile );
                        break;
                    case UOP.DXFGraphics.dxxEntityTypes.DimAngular3P:
                        List<DxfEntity> dimensions_Ang3P = transformUtil.TransformToDXF(Entity);
                        RefreshDisplay(dimensions_Ang3P, Entity.SaveToFile );
                        break;
                    case UOP.DXFGraphics.dxxEntityTypes.LeaderBlock:
                    case UOP.DXFGraphics.dxxEntityTypes.LeaderText:
                        List<DxfEntity> leaders_text = transformUtil.TransformToDXF( Entity );
                        RefreshDisplay( leaders_text, Entity.SaveToFile );
                        break;
                    case UOP.DXFGraphics.dxxEntityTypes.LeaderTolerance:
                        //Not used in WinTray no examples
                        break;
                    
                    case UOP.DXFGraphics.dxxEntityTypes.Leader:
                        List<DxfEntity> leaders_noref = transformUtil.TransformToDXF( Entity );
                        RefreshDisplay( leaders_noref, Entity.SaveToFile );
                        break;
                    case UOP.DXFGraphics.dxxEntityTypes.Symbol:
                        List<DxfEntity> symbols = transformUtil.TransformToDXF( Entity, iImage );
                        RefreshDisplay( symbols, Entity.SaveToFile );
                        break;
                    case UOP.DXFGraphics.dxxEntityTypes.Shape:
                        List<DxfEntity> shapes = transformUtil.TransformToDXF( Entity, iImage );
                        RefreshDisplay( shapes, Entity.SaveToFile );
                        break;
                    default:
                        break;
                }
            }
            else
            {
                transformUtil.RemoveDxfEntity(Entity);
            }
        }

        private void SettingsChangeHandler( string aSource, dxoProperty aProperty )
        {
            string propName = aProperty.Name;

            switch (aSource)
            {
                case "Header":
                    {
                        switch (propName)
                        {
                            case "$LTSCALE":
                                model.Header.LineTypeScale = ValidateLTScale((double)aProperty.Value);
                                break;
                            case "$DIMSTYLE":
                                model.Header.DimensionStyle = this.GetDimensionStyle( aProperty.ValueString );
                                ActiveDimOverrides = new DxfDimensionStyleOverrides( model.Header.DimensionStyle, model );
                                break;
                            case "$CLAYER":
                                model.Header.CurrentLayer = this.GetLayer( aProperty.ValueString );
                                break;
                            case "$CELTYPE":
                                model.Header.CurrentEntityLineType = this.GetLineType( aProperty.ValueString );
                                break;
                            case "$TEXTSTYLE":
                                model.Header.CurrentTextStyle = this.GetTextStyle( aProperty.ValueString );
                                break;
                            case "$TEXTSIZE":
                                model.Header.TextHeightDefault = (double)aProperty.Value;
                                break;
                            case "*UCSMODE":
                                viewerControl.UCSMode = (int)aProperty.Value;
                                break;
                            case "*UCSCOLOR":
                                //Not supported by CADLib
                                break;
                            case "*UCSSIZE":
                                //Not supported by CADLib
                                break;
                            case "$PDMODE":
                                short pdmode = (short)(int)aProperty.Value;
                                model.Header.PointDisplayMode = (PointDisplayMode)pdmode;
                                break;
                            case "$PDSIZE":
                                model.Header.PointDisplaySize = (double)aProperty.Value;
                                break;
                            case "$QTEXTMODE":
                                model.Header.QuickTextMode = (bool)aProperty.Value;
                                break;
                            case "$STYLESHEET":
                                //Not supported by CADLib
                                break;
                        }
                        break;
                    }
                case "UCS":
                    {
                        switch (propName)
                        {
                            case "Origin/Orientation":
                                break;
                            case "Origin":
                                {
                                    string cords = aProperty.ValueString;
                                    cords = cords.Substring( 1, cords.Length - 2 );
                                    var dcords = cords.Split( ',' );
                                    model.Header.Ucs.Origin = new Point3D( Convert.ToDouble( dcords[ 0 ] ), Convert.ToDouble( dcords[ 1 ] ), Convert.ToDouble( dcords[ 2 ] ) );
                                    break;
                                }
                            case "Orientation":
                                break;
                        }
                        break;
                    }
                case "DimStyleOverrides":
                    {
                        if (ActiveDimOverrides == null)
                            ActiveDimOverrides = new DxfDimensionStyleOverrides( model.Header.DimensionStyle, model );
                        this.SetDimStyleProperty( ActiveDimOverrides, propName, aProperty.Value );
                        break;
                    }
                case "DimSettings":
                    {
                        switch (propName)
                        {
                            case "DimLayer":
                            case "DimColor":
                                //not supported by cadlib and handled by the dimension object
                                break;
                        }
                        break;
                    }
                case "LinetypeSettings":
                    {
                        break;
                    }
                case "SymbolSettings":
                    {
                        switch (propName)
                        {
                            case "FeatureScale":
                                //Not supported by CADLib
                                break;
                        }
                        break;
                    }
                case "TableSettings":
                    {
                        switch (propName)
                        {
                            case "FeatureScale":
                                //Not supported by CADLib
                                break;
                        }
                        break;
                    }
                case "TextSettings":
                    {
                        break;
                    }
                case "Display Setting":
                    {
                        if (propName.EndsWith( "BACKCOLOR" ))
                        {
                            SetBackgroundColor( aProperty.Value );
                            RefreshDisplay( false );
                        }
                        //if (propName.StartsWith( "VIEW" ))
                        //{
                        //    switch (propName.Substring( propName.IndexOf( "." ) + 1 ))
                        //    {
                        //        case "ZOOMFACTOR":
                        //            var vrec = oImage.Display.ViewRectangle;
                        //            ZoomWindow( oImage.Display.ViewRectangle );
                        //            break;
                        //        case "FOCALPOINT":
                        //            string cords = aProperty.ValueString;
                        //            cords = cords.Substring( 1, cords.Length - 2 );
                        //            var dcords = cords.Split( ',' );
                        //            SetCenter( new Point3D( Convert.ToDouble( dcords[ 0 ] ), Convert.ToDouble( dcords[ 1 ] ), Convert.ToDouble( dcords[ 2 ] ) ) );
                        //            break;
                        //    }
                        //}
                        break;
                    }
                default:
                    {
                        if (aSource.StartsWith( "DimStyle[" ))
                        {
                            string dimStyleName = aSource.Substring( aSource.IndexOf( "[" ) ).Substring( 0, aSource.IndexOf( "]" ) - 2 );
                            if (model.DimensionStyles.Contains( dimStyleName ))
                            {
                                this.SetDimStyleProperty( model.DimensionStyles[ dimStyleName ], propName, aProperty.Value );
                            }
                        }
                        break;
                    }
            }
            RefreshDisplay( false );
        }

        private string GetNameFromDescription(string description)
        {
            var flds = description.Split( '\'' );
            return flds[ 1 ].Trim();
        }

        private void TableEventHandler( string TableName, dxxCollectionEventTypes EventType, string EventDescription )
        {
            string tableMemberName = GetNameFromDescription( EventDescription );
            switch (TableName.ToUpper())
            {
                case "LAYER":
                    switch (EventType)
                    {
                        case dxxCollectionEventTypes.Add:
                            {
                                this.GetLayer( tableMemberName, EventType );
                                break;
                            }
                        case dxxCollectionEventTypes.Remove:
                            {
                                modelwh.Model.Layers.Remove( tableMemberName );
                                break;
                            }
                        default:
                            return;
                    }
                    break;
                case "STYLES":
                    switch (EventType)
                    {
                        case dxxCollectionEventTypes.Add:
                            {
                                this.GetTextStyle( tableMemberName );
                                break;
                            }
                        case dxxCollectionEventTypes.Remove:
                            {
                                DxfTextStyle txStyle = model.TextStyles.Where( txs => string.Compare( txs.Name, tableMemberName, true ) == 0 ).FirstOrDefault();
                                model.TextStyles.Remove( txStyle );
                                break;
                            }
                        default:
                            return;
                    }
                    break;
                case "DIMSTYLE":
                    switch (EventType)
                    {
                        case dxxCollectionEventTypes.Add:
                            {
                                this.GetDimensionStyle( tableMemberName );
                                break;
                            }
                        case dxxCollectionEventTypes.Remove:
                            {
                                model.DimensionStyles.Remove( tableMemberName );
                                break;
                            }
                        default:
                            return;
                    }
                    break;
                case "LINETYPE":
                    switch (EventType)
                    {
                        case dxxCollectionEventTypes.Add:
                            {
                                this.GetLineType( tableMemberName );
                                break;
                            }
                        case dxxCollectionEventTypes.Remove:
                            {
                                model.LineTypes.Remove( tableMemberName );
                                break;
                            }
                        default:
                            return;
                    }
                    break;
                case "STYLE":
                    switch (EventType)
                    {
                        case dxxCollectionEventTypes.Add:
                            {
                                this.GetTextStyle( tableMemberName );
                                break;
                            }
                        case dxxCollectionEventTypes.Remove:
                            {
                                DxfTextStyle txStyle = model.TextStyles.Where( txs => string.Compare( txs.Name, tableMemberName, true ) == 0 ).FirstOrDefault();
                                model.TextStyles.Remove( txStyle );
                                break;
                            }
                        default:
                            return;
                    }
                    break;
            }
        }

        private void TableMemeberEventHandler( string TableName, string MemberName, dxoProperty aProperty )
        {
            switch (TableName.ToUpper())
            {
                case "STYLE":
                    //this.UpdateTextStyle( MemberName, aProperty.Name );
                    break;
                case "DIMSTYLE":
                    this.GetDimensionStyle( MemberName, aProperty.Name );
                    break;
                case "LAYER":
                    this.UpdateLayer( MemberName, aProperty.Name );
                    break;
            }
            return;
        }

        private void ObjectEventHandler( string ObjectTypeName, string ObjectName, string PropertyName, object NewValue, object LastValue )
        {
            return;
        }

        private void ObjectsEventHandler( string ObjectTypeName, dxxCollectionEventTypes EventType, string EventDescription )
        {
            return;
        }

        private void RenderEventHandler( dxfImageRenderEventArg e )
        {
            if (!e.Begin && e.ZoomExtents)
            {
                ZoomExtents( e.ExtentBufferPercentage );
            }
            return;
        }

        private void ViewChangeEventHandler(dxfViewChangeEventArg e)
        {
            ZoomWindow(e.ViewRectangle);
        }

        private void StatusChangeEventHandler( string StatusDescription )
        {
            return;
        }

        private void ZoomEventHandler( bool extents, double zoomFactor )
        {
            if (extents)
            {
                //ZoomExtents(zoomFactor);
            }
            else
            {
                //Zoom( zoomFactor );
            }
        }

        private void RotateEventHandler( double rotation )
        {
            viewRotation += -rotation;
            UpdateRenderTransform();
        }

        private void ViewRegenerateEventHandler(dxfImage aImage)
        {
            Regenerate( aImage, false );
        }

        private void SaveToFile( string aFileName )
        {
            string ucFileName = aFileName.ToUpper();
            if (ucFileName.EndsWith(".DWG"))
            {
                SaveDwg( aFileName,out string msgDWG );
            }
            if (ucFileName.EndsWith(".DXF"))
            {
                SaveDxf( aFileName, out string msgDXF);
            }
            if (ucFileName.EndsWith(".PDF"))
            {
                SavePdf( aFileName, out string msgPDF);
            }
        }
    }
}
