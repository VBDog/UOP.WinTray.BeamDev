using System;
using System.Collections.Generic;
using System.Linq;
using UOP.DXFGraphics;
using WW.Cad.Model;
using WW.Cad.Model.Tables;

namespace UOP.DXFGraphicsControl.Utils
{
    public static class PropertyNameMaps
    {
        private static Dictionary<string, (string, Type, Func<object, object, (object, bool)>)> dimensionStyleMap;
        private static Dictionary<string, (string, Type, Func<object, object, (object, bool)>)> layerMap;
        private static Dictionary<string, (string, Type, Func<object, object, (object, bool)>)> textStyleMap;
        static PropertyNameMaps()
        {
            InitializeMaps();
        }

        private static void InitializeMaps()
        {
            InitializeDimensionStyleMap();
            InitializeLayerMap();
            InitializeTextStyleMap();
        }

        private static void InitializeDimensionStyleMap()
        {
            dimensionStyleMap = new Dictionary<string, (string, Type, Func<object, object, (object, bool)>)>()
            {
                { "DIMAPOST", ("AlternateDimensioningSuffix", typeof(string), null) },
                { "DIMALTD", ("AlternateUnitDecimalPlaces", typeof(short), ConvertToShort) },
                { "DIMALT", ("AlternateUnitDimensioning", typeof(bool), null) },
                { "DIMALTU", ("AlternateUnitFormat", typeof(AlternateUnitFormat), ConvertToShortBasedEnum<AlternateUnitFormat>) },
                { "DIMALTRND", ("AlternateUnitRounding", typeof(double), null) },
                { "DIMALTF", ("AlternateUnitScaleFactor", typeof(double), null) },
                { "DIMALTTD", ("AlternateUnitToleranceDecimalPlaces", typeof(short), ConvertToShort) },
                { "DIMALTTZ", ("AlternateUnitToleranceZeroHandling", typeof(ZeroHandling), ConvertToByteBasedEnum<ZeroHandling>) },
                { "DIMALTZ", ("AlternateUnitZeroHandling", typeof(ZeroHandling), ConvertToByteBasedEnum<ZeroHandling>) },
                { "DIMALTMZF", ("AltMzf", typeof(double), null) },
                { "DIMALTMZS", ("AltMzs", typeof(string), null) },
                { "DIMADEC", ("AngularDimensionDecimalPlaces", typeof(short), ConvertToShort) },
                { "DIMAUNIT", ("AngularUnit", typeof(AngularUnit), ConvertToShortBasedEnum<AngularUnit>) },
                { "DIMAZIN", ("AngularZeroHandling", typeof(ZeroHandling), ConvertToByteBasedEnum<ZeroHandling>) },
                { "DIMARCSYM", ("ArcLengthSymbolPosition", typeof(ArcLengthSymbolPosition), ConvertToShortBasedEnum<ArcLengthSymbolPosition>) },
                { "DIMBLK_NAME", ("ArrowBlock", typeof(DxfBlock), ConvertToDxfBlock) },
                { "DIMASZ", ("ArrowSize", typeof(double), null) },
                { "DIMATFIT", ("ArrowsTextFit", typeof(ArrowsTextFitType), ConvertToByteBasedEnum<ArrowsTextFitType>) },
                { "DIMCEN", ("CenterMarkSize", typeof(double), null) },
                { "DIMUPT", ("CursorUpdate", typeof(CursorUpdate), ConvertToByteBasedEnum<CursorUpdate>) },
                { "DIMDEC", ("DecimalPlaces", typeof(short), null) },
                { "DIMDSEP", ("DecimalSeparator", typeof(char), null) },
                { "DIMCLRD", ("DimensionLineColor", typeof(Color), ConvertToDxfColorStruct) },
                { "DIMDLE", ("DimensionLineExtension", typeof(double), null) },
                { "DIMGAP", ("DimensionLineGap", typeof(double), null) },
                { "DIMDLI", ("DimensionLineIncrement", typeof(double), null) },
                { "DIMLTYPE_NAME", ("DimensionLineLineType", typeof(DxfLineType), ConvertToDxfLineType) },
                { "DIMLWD", ("DimensionLineWeight", typeof(short), null) },
                { "DIMCLRE", ("ExtensionLineColor", typeof(Color), ConvertToDxfColorStruct) },
                { "DIMEXE", ("ExtensionLineExtension", typeof(double), null) },
                { "DIMEXO", ("ExtensionLineOffset", typeof(double), null) },
                { "DIMLWE", ("ExtensionLineWeight", typeof(short), null) },
                { "DIMBLK1_NAME", ("FirstArrowBlock", typeof(DxfBlock), ConvertToDxfBlock) },
                { "DIMLTEX1_NAME", ("FirstExtensionLineLineType", typeof(DxfLineType), ConvertToDxfLineType) },
                { "DIMFXL", ("FixedExtensionLineLength", typeof(double), null) },
                { "DIMFRAC", ("FractionFormat", typeof(FractionFormat), ConvertToShortBasedEnum<FractionFormat>) },
                { "DIMTOL", ("GenerateTolerances", typeof(bool), null) },
                { "DIMFXLON", ("IsExtensionLineLengthFixed", typeof(bool), null) },
                { "DIMJOGANG", ("JoggedRadiusDimensionTransverseSegmentAngle", typeof(double), null) },
                { "DIMLDRBLK_NAME", ("LeaderArrowBlock", typeof(DxfBlock), ConvertToDxfBlock) },
                { "DIMLIM", ("LimitsGeneration", typeof(bool), null) },
                { "DIMLFAC", ("LinearScaleFactor", typeof(double), null) },
                { "DIMLUNIT", ("LinearUnitFormat", typeof(LinearUnitFormat), ConvertToShortBasedEnum<LinearUnitFormat>) },
                { "DIMTM", ("MinusTolerance", typeof(double), null) },
                { "DIMTP", ("PlusTolerance", typeof(double), null) },
                { "DIMPOST", ("PostFix", typeof(string), null) },
                { "DIMRND", ("Rounding", typeof(double), null) },
                { "DIMSCALE", ("ScaleFactor", typeof(double), null) },
                { "DIMBLK2_NAME", ("SecondArrowBlock", typeof(DxfBlock), ConvertToDxfBlock) },
                { "DIMLTEX2_NAME", ("SecondExtensionLineLineType", typeof(DxfLineType), ConvertToDxfLineType) },
                { "DIMSAH", ("SeparateArrowBlocks", typeof(bool), null) },
                { "DIMSD1", ("SuppressFirstDimensionLine", typeof(bool), null) },
                { "DIMSE1", ("SuppressFirstExtensionLine", typeof(bool), null) },
                { "DIMSOXD", ("SuppressOutsideExtensions", typeof(bool), null) },
                { "DIMSD2", ("SuppressSecondDimensionLine", typeof(bool), null) },
                { "DIMSE2", ("SuppressSecondExtensionLine", typeof(bool), null) },
                //{ "DIMTAD", ("TextAboveDimensionLine", typeof(bool), null) }, This property is obsolete
                { "DIMTFILLCLR", ("TextBackgroundColor", typeof(Color), ConvertToDxfColorStruct) },
                { "DIMTFILL", ("TextBackgroundFillMode", typeof(DimensionTextBackgroundFillMode), ConvertToShortBasedEnum<DimensionTextBackgroundFillMode>) },
                { "DIMCLRT", ("TextColor", typeof(Color), ConvertToDxfColorStruct) },
                { "DIMTXTDIRECTION", ("TextDirection", typeof(TextDirection), ConvertToByteBasedEnum<TextDirection>) },
                { "DIMTXT", ("TextHeight", typeof(double), null) },
                { "DIMJUST", ("TextHorizontalAlignment", typeof(DimensionTextHorizontalAlignment), ConvertToByteBasedEnum<DimensionTextHorizontalAlignment>) },
                { "DIMTIX", ("TextInsideExtensions", typeof(bool), null) },
                { "DIMTIH", ("TextInsideHorizontal", typeof(bool), null) },
                { "DIMTMOVE", ("TextMovement", typeof(TextMovement), ConvertToShortBasedEnum<TextMovement>) },
                { "DIMTOFL", ("TextOutsideExtensions", typeof(bool), null) },
                { "DIMTOH", ("TextOutsideHorizontal", typeof(bool), null) },
                { "DIMTXSTY_NAME", ("TextStyle", typeof(DxfTextStyle), ConvertToDxfTextStyle) },
                { "DIMTAD", ("TextVerticalAlignment", typeof(DimensionTextVerticalAlignment), ConvertToByteBasedEnum<DimensionTextVerticalAlignment>) },
                { "DIMTVP", ("TextVerticalPosition", typeof(double), null) },
                { "DIMTSZ", ("TickSize", typeof(double), null) },
                { "DIMTOLJ", ("ToleranceAlignment", typeof(ToleranceAlignment), ConvertToByteBasedEnum<ToleranceAlignment>) },
                { "DIMTDEC", ("ToleranceDecimalPlaces", typeof(short), null) },
                { "DIMTFAC", ("ToleranceScaleFactor", typeof(double), null) },
                { "DIMTZIN", ("ToleranceZeroHandling", typeof(ZeroHandling), ConvertToByteBasedEnum<ZeroHandling>) },
                { "DIMZIN", ("ZeroHandling", typeof(ZeroHandling), ConvertToByteBasedEnum<ZeroHandling>) }
            };
        }

        private static void InitializeLayerMap()
        {
            layerMap = new Dictionary<string, (string, Type, Func<object, object, (object, bool)>)>()
            {
                { "Color", ("Color", typeof(Color), ConvertToDxfColorStruct) },
                { "Frozen", ("Frozen", typeof(bool), null) },
                { "FrozenInNewViewports", ("FrozenInNewViewport", typeof(bool), null) },
                { "Linetype", ("LineType", typeof(DxfLineType), ConvertToDxfLineType) },
                { "LineWeight", ("LineWeight", typeof(short), ConvertIntBasedEnumToShort) },
                { "Locked", ("Locked", typeof(bool), null) },
                { "PlotFlag", ("PlotEnabled", typeof(bool), null) }
            };
        }

        private static void InitializeTextStyleMap()
        {
            textStyleMap = new Dictionary<string, (string, Type, Func<object, object, (object, bool)>)>()
            {
                { "Name", ("Name", typeof(string), null) },
                { "FontFileName", ("FontFilename", typeof(string), null) },
                { "Backwards", ("IsBackwards", typeof(bool), null) },
                { "UpsideDown", ("IsUpsideDown", typeof(bool), null) },
                { "Vertical", ("IsVertical", typeof(bool), null) },
                { "ObliqueAngle", ("ObliqueAngle", typeof(double), null) },
                { "WidthFactor", ("WidthFactor", typeof(double), null) },
                { "TextHeight", ("FixedHeight", typeof(double), null) },
                { "LastHeight", ("LastHeightUsed", typeof(double), null) }
            };
        }

        public static string GetCodeByCadlibName(string propertyName)
        {
            return dimensionStyleMap.Where( pair => pair.Value.Item1 == propertyName ).Select( pair => pair.Key ).FirstOrDefault();
        }

        public static (string, object, bool) GetCADLibPropertyNameValueUsingAutoCADStandardCode(string code, object propertyValueInput, DXFViewer dXFViewer)
        {
            (string, object, bool) result;

            try
            {
                code = code.Trim().ToUpper().Trim('*');
                (string, Type, Func<object, object, (object, bool)>) value;

                bool found = false;

                if (!(found = dimensionStyleMap.TryGetValue( code, out value )))
                    found = dimensionStyleMap.TryGetValue( code + "_NAME", out value );

                if (found)
                {
                    string propertyName = value.Item1;
                    Type propertyType = value.Item2;
                    Func<object, object, (object, bool)> customConverter = value.Item3;
                    object propertyValue;
                    if (customConverter == null)
                    {
                        propertyValue = Convert.ChangeType(propertyValueInput, propertyType);
                        result = (propertyName, propertyValue, true);
                    }
                    else
                    {
                        (object propertyValueTemp, bool isValid) = customConverter(propertyValueInput, dXFViewer);
                        if (isValid)
                        {
                            propertyValue = Convert.ChangeType(propertyValueTemp, propertyType);
                            result = (propertyName, propertyValue, true);
                        }
                        else
                        {
                            result = ("", null, false);
                        }
                    }
                }
                else
                {
                    result = ("", null, false);
                }
            }
            catch (Exception)
            {
                result = ("", null, false);
            }

            return result;
        }

        public static (string, object, bool) GetCADLibPropertyNameValueForLayer(string layerPropertyName, object propertyValueInput, DXFViewer dXFViewer)
        {
            (string, object, bool) result;

            try
            {
                layerPropertyName = layerPropertyName.Trim();
                (string, Type, Func<object, object, (object, bool)>) value;

                if (layerMap.TryGetValue(layerPropertyName, out value))
                {
                    string propertyName = value.Item1;
                    Type propertyType = value.Item2;
                    Func<object, object, (object, bool)> customConverter = value.Item3;
                    object propertyValue;
                    if (customConverter == null)
                    {
                        propertyValue = Convert.ChangeType(propertyValueInput, propertyType);
                        result = (propertyName, propertyValue, true);
                    }
                    else
                    {
                        (object propertyValueTemp, bool isValid) = customConverter(propertyValueInput, dXFViewer);
                        if (isValid)
                        {
                            propertyValue = Convert.ChangeType(propertyValueTemp, propertyType);
                            result = (propertyName, propertyValue, true);
                        }
                        else
                        {
                            result = ("", null, false);
                        }
                    }
                }
                else
                {
                    result = ("", null, false);
                }
            }
            catch (Exception)
            {
                result = ("", null, false);
            }

            return result;
        }

        public static (string, object, bool) GetCADLibPropertyNameValueForTextStyle(string textStylePropertyName, object propertyValueInput, DXFViewer dXFViewer)
        {
            (string, object, bool) result;

            try
            {
                textStylePropertyName = textStylePropertyName.Trim();
                (string, Type, Func<object, object, (object, bool)>) value;

                if (textStyleMap.TryGetValue(textStylePropertyName, out value))
                {
                    string propertyName = value.Item1;
                    Type propertyType = value.Item2;
                    Func<object, object, (object, bool)> customConverter = value.Item3;
                    object propertyValue;
                    if (customConverter == null)
                    {
                        propertyValue = Convert.ChangeType(propertyValueInput, propertyType);
                        result = (propertyName, propertyValue, true);
                    }
                    else
                    {
                        (object propertyValueTemp, bool isValid) = customConverter(propertyValueInput, dXFViewer);
                        if (isValid)
                        {
                            propertyValue = Convert.ChangeType(propertyValueTemp, propertyType);
                            result = (propertyName, propertyValue, true);
                        }
                        else
                        {
                            result = ("", null, false);
                        }
                    }
                }
                else
                {
                    result = ("", null, false);
                }
            }
            catch (Exception)
            {
                result = ("", null, false);
            }

            return result;
        }

        public static List<string> GetAllAutoCADStandardDimensionStyleCodes()
        {
            return dimensionStyleMap.Keys.ToList();
        }

        public static List<string> GetAllLayerPropertyNames()
        {
            return layerMap.Keys.ToList();
        }

        public static List<string> GetAllTextStylePropertyNames()
        {
            return textStyleMap.Keys.ToList();
        }

        private static (object, bool) ConvertToShort(object input, object viewer = null)
        {
            short result;

            if (input is short)
            {
                result = (short)input;
                return (result, true);
            }

            if (input is int)
            {
                result = (short)(int)input;
                return (result, true);
            }

            if (input is long)
            {
                result = (short)(long)input;
                return (result, true);
            }

            result = default;
            return (result, false);
        }

        private static (object, bool) ConvertToByte(object input, object viewer = null)
        {
            byte result;

            if (input is byte)
            {
                result = (byte)input;
                return (result, true);
            }

            if (input is short)
            {
                result = (byte)input;
                return (result, true);
            }

            if (input is int)
            {
                result = (byte)(int)input;
                return (result, true);
            }

            if (input is long)
            {
                result = (byte)(long)input;
                return (result, true);
            }

            result = default;
            return (result, false);
        }

        private static (object, bool) ConvertToDxfBlock(object input, object viewer = null)
        {
            if (!(viewer is DXFViewer))
            {
                return (null, false);
            }

            if (!(input is string))
            {
                return (null, false);
            }

            string blockName = (string)input;
            if (string.IsNullOrWhiteSpace(blockName))
            {
                return (null, false);
            }

            DXFViewer dXFViewer = (DXFViewer)viewer;
            DxfBlock dxfBlock = dXFViewer.GetBlock(blockName);
            return (dxfBlock, true);
        }

        private static (object, bool) ConvertToDxfLineType(object input, object viewer = null)
        {
            if (!(viewer is DXFViewer))
            {
                return (null, false);
            }

            if (!(input is string))
            {
                return (null, false);
            }

            string lineTypeName = (string)input;
            if (string.IsNullOrWhiteSpace(lineTypeName))
            {
                return (null, false);
            }

            DXFViewer dXFViewer = (DXFViewer)viewer;
            DxfLineType dxfLineType = dXFViewer.GetLineType(lineTypeName);
            return (dxfLineType, true);
        }

        private static (object, bool) ConvertToDxfTextStyle(object input, object viewer = null)
        {
            if (!(viewer is DXFViewer))
            {
                return (null, false);
            }

            if (!(input is string))
            {
                return (null, false);
            }

            string textStyleName = (string)input;
            if (string.IsNullOrWhiteSpace(textStyleName))
            {
                return (null, false);
            }

            DXFViewer dXFViewer = (DXFViewer)viewer;
            DxfTextStyle dxfTextStyle = dXFViewer.GetTextStyle(textStyleName);
            return (dxfTextStyle, true);
        }

        private static (object, bool) ConvertToDxfColorStruct(object input, object viewer = null)
        {
            if (!(viewer is DXFViewer))
            {
                return (null, false);
            }

            if (input is dxxColors)
            {
                dxxColors dxxColor = (dxxColors)input;
                Color color = Color.CreateFromColorIndex((short)dxxColor);
                return (color, true);
            }

            if (input is long)
            {
                Color color = Color.CreateFromColorIndex((short)(long)input);
                return (color, true);
            }

            if (input is int)
            {
                Color color = Color.CreateFromColorIndex((short)(int)input);
                return (color, true);
            }

            if (input is short)
            {
                Color color = Color.CreateFromColorIndex((short)input);
                return (color, true);
            }
            
            return (default, false);
        }

        private static (object, bool) ConvertToByteBasedEnum<T>(object input, object viewer = null)
        {
            T none = default(T);
            try
            {
                byte byteVal;
                (object byteValObj, bool isValid) = ConvertToByte(input);
                if (!isValid)
                {
                    return (none, false);
                }
                else
                {
                    byteVal = (byte)byteValObj;
                }

                if (Enum.IsDefined(typeof(T), byteVal))
                {
                    return ((T)byteValObj, true);
                }
                else
                {
                    return (none, false);
                }
            }
            catch (Exception)
            {
                return (none, false);
            }
        }

        private static (object, bool) ConvertToShortBasedEnum<T>(object input, object viewer = null)
        {
            T none = default(T);
            try
            {
                short shortVal;
                (object shortValObj, bool isValid) = ConvertToShort(input);
                if (!isValid)
                {
                    return (none, false);
                }
                else
                {
                    shortVal = (short)shortValObj;
                }

                if (Enum.IsDefined(typeof(T), shortVal))
                {
                    return ((T)shortValObj, true);
                }
                else
                {
                    return (none, false);
                }
            }
            catch (Exception)
            {
                return (none, false);
            }
        }

        private static (object, bool) ConvertIntBasedEnumToShort(object input, object viewer = null)
        {
            try
            {
                short shortVal = (short)(int)input;
                return (shortVal, true);
            }
            catch (Exception)
            {
                return (default, false);
            }
        }
    }
}
