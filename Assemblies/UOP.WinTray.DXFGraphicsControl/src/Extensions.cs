using UOP.DXFGraphicsControl.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WW.Cad.Base;
using WW.Cad.Model;
using WW.Cad.Model.Entities;
using WW.Cad.Model.Tables;
using WW.Math;
using UOP.DXFGraphics;
using static System.Net.WebRequestMethods;

namespace UOP.DXFGraphicsControl
{
    public static class Extensions
    {
        private static List<string> arrowBlocks { get; set; } = new List<string>();

        public static void ClearArrowBlocksCache()
        {
            arrowBlocks?.Clear();
        }

        public static void SetLayerSettings(this DXFViewer viewer, DxfLayer layer, dxoLayer lyr, string propertyName = "")
        {
            List<string> layerPropertyNames;
            if (String.IsNullOrWhiteSpace(propertyName))
            {
                layerPropertyNames = PropertyNameMaps.GetAllLayerPropertyNames();
            }
            else
            {
                layerPropertyNames = new List<string>() { propertyName };
            }

            string layerProperyName;
            Type layerType = typeof(dxoLayer);
            Type cadLibLayerType = typeof(DxfLayer);
            for (int i = 0; i < layerPropertyNames.Count; i++)
            {
                try
                {
                    layerProperyName = layerPropertyNames[i];
                    object propertyValue = layerType.GetProperty(layerProperyName).GetValue(lyr);
                    if (propertyValue != null)
                    {
                        (string cadLibPropName, object cadLibPropValue, bool isValid) = PropertyNameMaps.GetCADLibPropertyNameValueForLayer(layerProperyName, propertyValue, viewer);
                        if (isValid)
                        {
                            PropertyInfo propertyInfo = cadLibLayerType.GetProperty(cadLibPropName);
                            if (propertyInfo != null)
                            {
                                if (propertyInfo.Name == "LineWeight")
                                {
                                    //if ((short)cadLibPropValue < 1)
                                    //    cadLibPropValue = 25;
                                }

                                propertyInfo.SetValue(layer, cadLibPropValue);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.Write(e.Message);
                }
            }
            
            layer.Transparency = Transparency.Opaque;
        }

        public static void SetLineTypeSettings( DxfLineType ltype, dxoLinetype gLtype )
        {
            if (ltype.Name == dxfLinetypes.Continuous || ltype.Name == dxfLinetypes.ByLayer || ltype.Name == dxfLinetypes.ByBlock)
                return;

            ltype.Name = gLtype.Name;
            ltype.Description = gLtype.Description;
            ltype.Elements.Clear();
            var elements = gLtype.Elements;
            DxfLineType.Element element = null;
            foreach (var prop in elements)
            {
                if (prop.Name.EndsWith( "Length" ))
                {
                    element = new DxfLineType.Element( (Double)prop.Value );
                }
                else
                {
                    //var val = prop.Value;
                    //element.ElementType = (WW.Cad.Model.Tables.DxfLineType.ElementType)val;
                    ltype.Elements.Add( element );
                }
            }
        }

        public static void SetTextStyleSettings(this DXFViewer viewer, DxfTextStyle textStyle, dxoStyle gStyle, string propertyName = "")
        {
            List<string> textStylePropertyNames;
            if (String.IsNullOrWhiteSpace(propertyName))
            {
                textStylePropertyNames = PropertyNameMaps.GetAllTextStylePropertyNames();
            }
            else
            {
                textStylePropertyNames = new List<string>() { propertyName };
            }

            string textStylePropertyName;
            Type textStyleType = typeof(dxoStyle);
            Type cadLibTextStyleType = typeof(DxfTextStyle);
            
            for (int i = 0; i < textStylePropertyNames.Count; i++)
            {
                try
                {
                    textStylePropertyName = textStylePropertyNames[i];

                    object propertyValue = textStyleType.GetProperty(textStylePropertyName).GetValue(gStyle);
                    if (propertyValue != null)
                    {
                        (string cadLibPropName, object cadLibPropValue, bool isValid) = PropertyNameMaps.GetCADLibPropertyNameValueForTextStyle(textStylePropertyName, propertyValue, viewer);
                        if (isValid)
                        {
                            PropertyInfo propertyInfo = cadLibTextStyleType.GetProperty(cadLibPropName);
                            if (propertyInfo != null)
                            {
                                var val = propertyInfo.GetValue( textStyle );
                                if (val == null || (val != null && string.Compare(val.ToString(), cadLibPropValue.ToString(), true) != 0))
                                {
                                    propertyInfo.SetValue( textStyle, cadLibPropValue );
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.Write(e.Message);
                }
            }
        }

        public static void SetUcsSettings(this DXFViewer viewer, DxfUcs ucs, dxoUCS iUCS, string propertyName = "")
        {
            if (ucs == null || iUCS == null) return;
            iUCS.UpdateProperties();
            ucs.Elevation = iUCS.Elevation;
            dxfPlane plane = iUCS.Plane;
            dxfVector v1 = plane.Origin;
            dxfDirection d1 = plane.XDirection;
            dxfDirection d2 = plane.YDirection;
            ucs.Origin = new Point3D(v1.X, v1.Y, v1.Z);
            ucs.XAxis = new Vector3D(d1.X, d1.Y, d1.Z);
            ucs.YAxis = new Vector3D(d2.X, d2.Y, d2.Z);
            ucs.OrthographicViewType = (OrthographicType) iUCS.OrthographicType;
        }

        public static void SetViewSettings(this DXFViewer viewer, DxfView view, dxoView iView)
        {
            if (view == null || iView == null) return;
     
            iView.UpdateProperties();
            dxfVector v1 = iView.ViewCenter;
            dxfDirection d1 = iView.Direction;
            view.Center = new Point2D(v1.X,v1.Y);
            view.Direction = new Vector3D(d1.X, d1.Y, d1.Z);
            view.LensLength = iView.LensLength;
            view.Size = new Size2D(iView.ViewWidth, iView.ViewHeight);
            view.ViewMode = (ViewMode) iView.ViewMode;
            view.FrontClippingPlane = iView.FrontClippingPlane;
            view.BackClippingPlane = iView.BackClippingPlane;
            view.Paperspace = iView.IsPaperSpace;
            view.IsExternallyDependent = iView.XRefDependant;
            view.IsResolvedExternalRef = iView.XRefResolved;
            view.Flags = (ViewFlags)iView.ViewFlag;
            view.TwistAngle = iView.TwistAngle;
            view.RenderMode = (RenderMode)iView.RenderMode;

            if (iView.HasUCS)
            {
                view.FollowUcs = true;
                dxfPlane vucs = iView.UCS;
                view.UcsOrthographicType = (OrthographicType)iView.UCSOrthographicType;
                v1 = vucs.Origin;
                d1 = vucs.XDirection;
                dxfDirection d2 = vucs.YDirection;
                view.Ucs = new DxfUcs(vucs.Name, new Point3D(v1.X, v1.Y, v1.Z), new Vector3D(d1.X, d1.Y, d1.Z), new Vector3D(d2.X, d2.Y, d2.Z)) { Elevation = iView.UCSElevation};
                
            }
           
        }

        public static void SetDimStyleSettings( this DXFViewer viewer, DxfDimensionStyle dimStyle, dxoDimStyle gDimStyle, string propertyName = "" )
        {
            List<string> cadLibDimensionStyleCodes;
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                cadLibDimensionStyleCodes = PropertyNameMaps.GetAllAutoCADStandardDimensionStyleCodes();
            }
            else
            {
                cadLibDimensionStyleCodes = new List<string>() { propertyName };
            }

            string code = string.Empty;
             object propertyValue = null;

            for (int i = 0; i < cadLibDimensionStyleCodes.Count; i++)
            {
                try
                {
                    code = cadLibDimensionStyleCodes[i];
                   
                    propertyValue = gDimStyle.GetPropertyValue( code);
               
                    if (propertyValue != null && propertyValue.ToString() != "Undefined")
                    {
                        (string cadLibPropName, object cadLibPropValue, bool isValid) = PropertyNameMaps.GetCADLibPropertyNameValueUsingAutoCADStandardCode(code, propertyValue, viewer);
                        if (isValid)
                        {
                            PropertyInfo propertyInfo = typeof(DxfDimensionStyle).GetProperty(cadLibPropName);
                            if (propertyInfo != null)
                            {
                                propertyInfo.SetValue(dimStyle, cadLibPropValue);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.Write(e.Message);
                }

               
            }

            dimStyle.Name = gDimStyle.Name;
        }

        public static void SetDimStyleSettings(this DXFViewer viewer, DxfDimensionStyle dimStyle, dxsDimOverrides iOverrides, string propertyName = "")
        {
            List<string> cadLibDimensionStyleCodes;
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                cadLibDimensionStyleCodes = PropertyNameMaps.GetAllAutoCADStandardDimensionStyleCodes();
            }
            else
            {
                cadLibDimensionStyleCodes = new List<string>() { propertyName };
            }

            string code = string.Empty;
            object propertyValue = null;

            for (int i = 0; i < cadLibDimensionStyleCodes.Count; i++)
            {
                try
                {
                    code = cadLibDimensionStyleCodes[i];

                    propertyValue = iOverrides.GetPropertyValue(code);

                    if (propertyValue != null && propertyValue.ToString() != "Undefined")
                    {
                        (string cadLibPropName, object cadLibPropValue, bool isValid) = PropertyNameMaps.GetCADLibPropertyNameValueUsingAutoCADStandardCode(code, propertyValue, viewer);
                        if (isValid)
                        {
                            PropertyInfo propertyInfo = typeof(DxfDimensionStyle).GetProperty(cadLibPropName);
                            if (propertyInfo != null)
                            {
                                propertyInfo.SetValue(dimStyle, cadLibPropValue);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.Write(e.Message);
                }


            }

            dimStyle.Name = iOverrides.Name;
        }

        public static void SetDimStyleOverrides(this DXFViewer viewer, DxfDimension dim, dxeDimension edim, dxfImage iImage)
        {
            var oPropertyInfos = typeof(IDimensionStyle).GetProperties();

            //if (viewer.ActiveDimOverrides != null && viewer.ActiveDimOverrides.HasOverrides)
            //{
            //    try
            //    {
            //        foreach (var pi in oPropertyInfos)
            //        {
            //            if (pi != null && pi.CanWrite)
            //            {
            //                string oname = "Override" + pi.Name;
            //                var oPropertyInfo = typeof( DxfDimensionStyleOverrides ).GetProperty( oname );
            //                if ((bool)oPropertyInfo.GetValue( viewer.ActiveDimOverrides ))
            //                {
            //                    pi.SetValue( dim.DimensionStyleOverrides, pi.GetValue( viewer.ActiveDimOverrides ) );
            //                }
            //            }
            //        }
            //    }
            //    catch ((Exception e)
            {
                //   System.Diagnostics.Debug.Write(e.Message);
                //    }
                //}

                dxoDimStyle estyle = (edim != null) ? edim.DimStyle : new dxoDimStyle();
                estyle.UpdateDimPost();

                foreach (var pi in oPropertyInfos)
                {
                    if (pi != null && pi.CanWrite)
                    {
                        object propertyValue = null;
                        string code = PropertyNameMaps.GetCodeByCadlibName(pi.Name);
                        dxfImage dimImage = iImage == null ? edim.Image : iImage;
                        if (code.EndsWith("_NAME"))
                        {
                            code = code.Substring(0, code.LastIndexOf("_"));
                            var hndl = estyle.GetPropertyValue(code);
                            switch (code)
                            {
                                case "DIMBLK":
                                case "DIMBLK1":
                                case "DIMBLK2":
                                case "DIMLDRBLK":

                                    if (dimImage != null)
                                    {
                                        var blknames = dimImage.Blocks.Names;
                                        foreach (var nm in blknames)
                                        {
                                            var blk = dimImage.Blocks.GetByName(nm);
                                            if (blk.BlockRecordHandle == hndl.ToString())
                                            {
                                                propertyValue = nm;
                                            }
                                        }
                                    }


                                    break;
                                case "DIMLTYPE":
                                case "DIMLTEX1":
                                case "DIMLTEX2":
                                    if (dimImage != null)
                                    {

                                        var lt = dimImage.Linetypes.Entry(hndl.ToString());
                                        if (lt != null)
                                            propertyValue = lt.Name;
                                    }

                                    break;
                            }
                        }
                        else
                        {
                            propertyValue = estyle.GetPropertyValue(code);
                        }

                        if (propertyValue != null)
                        {
                            (string cadLibPropName, object cadLibPropValue, bool isValid) = PropertyNameMaps.GetCADLibPropertyNameValueUsingAutoCADStandardCode(code, propertyValue, viewer);
                            if (isValid)
                            {
                                PropertyInfo propertyInfo = typeof(DxfDimensionStyleOverrides).GetProperty(cadLibPropName);
                                if (propertyInfo != null)
                                {
                                    propertyInfo.SetValue(dim.DimensionStyleOverrides, cadLibPropValue);
                                }
                            }
                        }
                    }
                }

                CheckArrowBlock(dim.DimensionStyleOverrides.FirstArrowBlock);
                CheckArrowBlock(dim.DimensionStyleOverrides.SecondArrowBlock);
                CheckArrowBlock(dim.DimensionStyleOverrides.ArrowBlock);
            }
        }

        private static void CheckArrowBlock(DxfBlock arrowBlock)
        {
            if (arrowBlock != null && !arrowBlocks.Contains(arrowBlock.Name))
            {
                foreach (var e in arrowBlock.Entities)
                {
                    e.Color = EntityColor.ByBlock;
                }
                arrowBlocks.Add(arrowBlock.Name);
            }
        }

        public static void SetDimStyleOverrides( this DXFViewer viewer, DxfLeader dim, dxeLeader edim )
        {
            var oPropertyInfos = typeof( IDimensionStyle ).GetProperties();
            dxoDimStyle estyle = (edim != null) ? edim.DimStyle : new dxoDimStyle();
            foreach (var pi in oPropertyInfos)
            {
                if (pi != null && pi.CanWrite)
                {
                    object propertyValue = null;
                    string code = PropertyNameMaps.GetCodeByCadlibName( pi.Name );
                    if (code.EndsWith( "_NAME" ))
                    {
                        code = code.Substring( 0, code.LastIndexOf( "_" ) );
                        var hndl = estyle.GetPropertyValue(  code );
                        switch (code)
                        {
                            case "DIMBLK":
                            case "DIMBLK1":
                            case "DIMBLK2":
                            case "DIMLDRBLK":
                                var blknames = edim.Image.Blocks.Names;
                                foreach (var nm in blknames)
                                {
                                    var blk = edim.Image.Blocks.GetByName( nm );
                                    if (blk.BlockRecordHandle == hndl.ToString())
                                    {
                                        propertyValue = nm;
                                    }
                                }

                                break;
                            case "DIMLTYPE":
                            case "DIMLTEX1":
                            case "DIMLTEX2":
                                var lt = edim.Image.Linetypes.Entry( hndl.ToString());
                                if (lt != null)
                                    propertyValue = lt.Name;
                                break;
                        }
                    }
                    else
                    {
                        propertyValue = estyle.GetPropertyValue(  code );
                    }

                    if (propertyValue != null)
                    {
                        (string cadLibPropName, object cadLibPropValue, bool isValid) = PropertyNameMaps.GetCADLibPropertyNameValueUsingAutoCADStandardCode( code, propertyValue, viewer );
                        if (isValid)
                        {
                            PropertyInfo propertyInfo = typeof( DxfDimensionStyleOverrides ).GetProperty( cadLibPropName );
                            if (propertyInfo != null)
                            {
                                propertyInfo.SetValue( dim.DimensionStyleOverrides, cadLibPropValue );
                            }
                        }
                    }
                }
            }
        }

        public static void SetDimStyleProperty(this DXFViewer viewer, IDimensionStyle dimStyle, string propertyName, object propertyValue)
        {
            try
            {
                string code = propertyName;
                if (propertyValue != null)
                {
                    (string cadLibPropName, object cadLibPropValue, bool isValid) = PropertyNameMaps.GetCADLibPropertyNameValueUsingAutoCADStandardCode( code, propertyValue, viewer );
                    if (isValid)
                    {
                        PropertyInfo propertyInfo = typeof( IDimensionStyle ).GetProperty( cadLibPropName );
                        if (propertyInfo != null)
                        {
                            propertyInfo.SetValue( dimStyle, cadLibPropValue );
                        }
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Write( e.Message );
            }
        }

        public static DxfBlock GetBlock(this DXFViewer viewer, string BlockName)
        {
            if (viewer.modelwh.Model.Blocks.Contains( BlockName ))
                return viewer.modelwh.Model.Blocks[ BlockName ];

            return null;

            //DxfBlock newblock = new DxfBlock( BlockName );
            //viewer.modelwh.Model.Blocks.Add( newblock );

            //if (viewer.oImage.Blocks.BlockExists(BlockName))
            //{
            //    using (TransformUtil transformUtil = new TransformUtil( viewer, true ))
            //    {
            //        dxfBlock block = viewer.oImage.Blocks.GetByName( BlockName );
            //        if (block.Entities != null)
            //        {
            //            dxfEntity entity;
            //            for (int i = 1; i <= block.Entities.Count; i++)
            //            {
            //                entity = block.Entities.Item( i );
            //                transformUtil.TransformToDXF( entity, newblock.Entities );
            //            }
            //        }
            //    }
            //}
            //return newblock;
        }

        public static DxfLayer GetLayer(this DXFViewer viewer, string LayerName)
        {
            return viewer.GetLayer( LayerName, dxxCollectionEventTypes.Suppression );
        }

        public static void UpdateLayer(this DXFViewer viewer, string layerName, string propertyName)
        {
            //int idx = 0;
            //DxfModel model = viewer.modelwh.Model;
            //dxfTableEntry rEntry = null;

            //DxfLayer layer = viewer.modelwh.Model.Layers.Where(l => string.Compare(l.Name, layerName, true) == 0).FirstOrDefault();
            //dxoLayer lyr = viewer.oImage.Layers.Member(layerName, ref idx, ref rEntry) as dxoLayer;

            //if (layer != null && lyr != null)
            //{
            //    SetLayerSettings(viewer, layer, lyr, propertyName);
            //}
        }

        public static DxfLayer GetLayer( this DXFViewer viewer, string LayerName, dxxCollectionEventTypes eventType )
        {
            //int idx = 0;
            DxfModel model = viewer.modelwh.Model;
            //dxfTableEntry rEntry = null;

            DxfLayer layer = model.Layers.Where( l => string.Compare( l.Name, LayerName, true ) == 0 ).FirstOrDefault();
            if (layer != null) return layer;
            //dxoLayer lyr = viewer.oImage.Layers.Member( LayerName, ref idx, ref rEntry ) as dxoLayer;

            //if (layer != null && lyr == null)
            //    return layer;

            //if (layer != null && lyr != null && eventType == dxxCollectionEventTypes.MemberChange)
            //{
            //    SetLayerSettings( viewer, layer, lyr );
            //    return layer;
            //}
            //else if (layer != null)
            //{
            //    return layer;
            //}

            //if (lyr != null)
            //{
            //    layer = new DxfLayer( lyr.Name );
            //    SetLayerSettings( viewer, layer, lyr );
            //    model.Layers.Add( layer );
            //    return layer;
            //}

            //Create a place holder layer with default values
            layer = new DxfLayer( LayerName );
            model.Layers.Add( layer );
            return layer;
        }

        public static DxfLayer GetLayer( this DXFViewer viewer, dxfEntity entity )
        {
            return viewer.GetLayer( entity.LayerName );
        }

        public static DxfLineType GetLineType( this DXFViewer viewer, string LinetypeName )
        {
            DxfModel model = viewer.modelwh.Model;
            //int idx = 0;
            //dxfTableEntry rEntry = null;
            if (string.IsNullOrWhiteSpace(LinetypeName))
            {
                Console.WriteLine("Linetype Is Null");
                LinetypeName = "ByLayer";
            }
            DxfLineType ltype = model.LineTypes.Where( l => string.Compare( l.Name, LinetypeName, true ) == 0 ).FirstOrDefault();
            if (ltype == null)
            {
                dxoLinetype dxfLt = dxfLinetypes.GetCurrentDef(LinetypeName);
                if(dxfLt != null)
                {
                    ltype = new DxfLineType(dxfLt.Name);
                    Extensions.SetLineTypeSettings(ltype, dxfLt);
                    model.LineTypes.Add(ltype);
                }
            }

                if (ltype != null) return ltype;
            //dxoLinetype gLtype = viewer.oImage.Linetypes.Member( LinetypeName, ref idx, ref rEntry ) as dxoLinetype;

            //if (ltype != null && gLtype == null)
            //    return ltype;

            //if (ltype != null && gLtype != null)
            //{
            //    SetLineTypeSettings( ltype, gLtype );
            //    return ltype;
            //}

            //if (gLtype != null)
            //{
            //    ltype = new DxfLineType( gLtype.Name );
            //    SetLineTypeSettings( ltype, gLtype );
            //    if (!model.LineTypes.Contains( ltype.Name ))
            //        model.LineTypes.Add( ltype );
            //    else
            //        model.LineTypes.TryGetValue( ltype.Name, out ltype );

            //    return ltype;
            //}

            //Create a place holder layer with default values
           
                

            ltype = new DxfLineType(LinetypeName);
            model.LineTypes.Add(ltype);
            return ltype;
        }

        public static DxfLineType GetLineType( this DXFViewer viewer, dxfEntity entity )
        {
            return viewer.GetLineType( entity.Linetype );
        }

        public static DxfTextStyle GetTextStyle( this DXFViewer viewer, string TextStyleName )
        {
            DxfModel model = viewer.modelwh.Model;
            List<DxfMessage> messages = new List<DxfMessage>();

            DxfTextStyle txStyle = model.TextStyles.Where( txs => string.Compare( txs.Name, TextStyleName, true ) == 0 ).FirstOrDefault();
            if (txStyle != null) return txStyle;
            //dxoStyle gStyle = viewer.oImage.TextStyle( TextStyleName, false) as dxoStyle;

            //if (txStyle != null && gStyle == null)
            //    return txStyle;

            //if (txStyle != null && gStyle != null)
            //{
            //    SetTextStyleSettings( viewer, txStyle, gStyle );
            //    var vc = new  ValidationContext( viewer.modelwh.Model);
            //    txStyle.Validate( vc, messages );
            //    return txStyle;
            //}

            //if (gStyle != null)
            //{
            //    txStyle = new DxfTextStyle();
            //    SetTextStyleSettings( viewer, txStyle, gStyle );
            //    model.TextStyles.Add( txStyle );
            //    var vc = new ValidationContext(viewer.modelwh.Model);
            //    txStyle.Validate( vc, messages );
            //    return txStyle;
            //}

            //Create a place holder text style with default values
            txStyle = new DxfTextStyle() {Name = TextStyleName };
            model.TextStyles.Add( txStyle );
            return txStyle;
        }

        public static DxfTextStyle GetTextStyle( this DXFViewer viewer, dxfEntity entity )
        {
            return viewer.GetTextStyle( entity.TextStyleName );
        }

        //public static void UpdateTextStyle(this DXFViewer viewer, string textStyleName, string propertyName)
        //{
        //    DxfModel model = viewer.modelwh.Model;
        //    List<DxfMessage> messages = new List<DxfMessage>();

        //    DxfTextStyle txStyle = viewer.modelwh.Model.TextStyles.Where(txs => string.Compare(txs.Name, textStyleName, true) == 0).FirstOrDefault();
        //    dxoStyle gStyle = viewer.oImage.TextStyle(textStyleName, false) as dxoStyle;
            
        //    if (txStyle != null && gStyle != null)
        //    {
        //        SetTextStyleSettings(viewer, txStyle, gStyle, propertyName);
        //    }
        //}

        public static DxfDimensionStyle GetDimensionStyle(this DXFViewer viewer, string DimensionStyleName, string properyName = "")
        {
            DxfModel model = viewer.modelwh.Model;
            List<DxfMessage> messages = new List<DxfMessage>();

            if (string.IsNullOrEmpty(DimensionStyleName))
            {
                DimensionStyleName = "Standard";
            }
            
            DxfDimensionStyle dmStyle = model.DimensionStyles.Where(dms => string.Compare(dms.Name, DimensionStyleName, true) == 0).FirstOrDefault();
            if (dmStyle != null) return dmStyle;
            //dxoDimStyle gStyle = viewer.oImage.DimStyle(DimensionStyleName) as dxoDimStyle;

            //if (dmStyle != null && gStyle == null)
            //    return dmStyle;

            //if (dmStyle != null && gStyle != null)
            //{
            //    viewer.SetDimStyleSettings(dmStyle, gStyle,  properyName);
            //    var vc = new ValidationContext(viewer.modelwh.Model) ;
            //    dmStyle.Validate(vc, messages);
            //    return dmStyle;
            //}
            //else if (dmStyle != null)
            //{
            //    return dmStyle;
            //}

            //if (gStyle != null)
            //{
            //    dmStyle = new DxfDimensionStyle(model);
            //    viewer.SetDimStyleSettings(dmStyle, gStyle);
            //    model.DimensionStyles.Add(dmStyle);
            //    var vc = new ValidationContext( viewer.modelwh.Model);
            //    dmStyle.Validate(vc, messages);
            //    return dmStyle;
            //}

            //Create a place holder dimension style with default values
            dmStyle = new DxfDimensionStyle(model) { Name = DimensionStyleName };
            model.DimensionStyles.Add(dmStyle);
            return dmStyle;
        }

        public static DxfDimensionStyle GetDimensionStyle(this DXFViewer viewer, dxfEntity entity)
        {
            return viewer.GetDimensionStyle(entity.DimStyleName);
        }

        public static void AdjusteForZoom(this Bounds3D bounds, double zoomFactor)
        {
            double wid = bounds.Corner2.X - bounds.Corner1.X;
            double newwid = wid * zoomFactor;
            double offset = (newwid - wid) / 2.0;
            var corner1 = bounds.Corner1;
            var corner2 = bounds.Corner2;
            corner1.X -= offset;
            corner1.Y -= offset;
            corner2.X += offset;
            corner2.Y += offset;
            bounds.Update( corner1 );
            bounds.Update( corner2 );
        }

        public static double Width(this Bounds3D bounds)
        {
            return bounds.Corner2.X - bounds.Corner1.X;
        }

        public static double Height(this Bounds3D bounds)
        {
            return bounds.Corner2.Y - bounds.Corner1.Y;
        }

        public static bool IsEqual(this Point3D pt, Point3D testpt, double tol)
        {
            Vector3D vec = pt - testpt;
            return vec.GetLength() <= tol;
        }

        public static Point3D ConvertToPoint3D( this dxfVector vector )
        {
            return new Point3D( vector.X, vector.Y, vector.Z );
        }

        public static Point2D As2D( this Point3D pt )
        {
            return new Point2D( pt.X, pt.Y);
        }
    }
}
