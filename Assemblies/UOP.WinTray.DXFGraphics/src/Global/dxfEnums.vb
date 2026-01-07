Imports System.ComponentModel
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics



    Public Class dxfEnums
        Public Shared Sub DecomposeMTextAlignment(aAlignment As dxxMTextAlignments, ByRef rVertical As dxxTextJustificationsVertical, ByRef rHorizontal As dxxTextJustificationsHorizontal, Optional bVertical As Boolean = False)
            rVertical = dxxTextJustificationsVertical.Baseline
            rHorizontal = dxxTextJustificationsHorizontal.Left
            If bVertical Then rHorizontal = dxxTextJustificationsHorizontal.HMiddle
            Select Case aAlignment
             '---------------------------------------------------------------------------------------------------------
                Case dxxMTextAlignments.BaselineLeft
                    If bVertical Then
                        rVertical = dxxTextJustificationsVertical.Top
                    Else
                        rHorizontal = dxxTextJustificationsHorizontal.Left
                    End If
                Case dxxMTextAlignments.BaselineMiddle
                    If bVertical Then
                        rVertical = dxxTextJustificationsVertical.Top
                    Else
                        rHorizontal = dxxTextJustificationsHorizontal.Center
                    End If
                Case dxxMTextAlignments.BaselineRight
                    If bVertical Then rVertical = dxxTextJustificationsVertical.Top Else rHorizontal = dxxTextJustificationsHorizontal.Right
             '---------------------------------------------------------------------------------------------------------
                Case dxxMTextAlignments.BottomLeft
                    If bVertical Then
                        rVertical = dxxTextJustificationsVertical.Bottom
                    Else
                        rHorizontal = dxxTextJustificationsHorizontal.Left
                    End If
                Case dxxMTextAlignments.BottomCenter
                    If bVertical Then
                        rVertical = dxxTextJustificationsVertical.Bottom
                    Else
                        rHorizontal = dxxTextJustificationsHorizontal.Center
                    End If
                Case dxxMTextAlignments.BottomRight
                    If bVertical Then
                        rVertical = dxxTextJustificationsVertical.Bottom
                    Else
                        rHorizontal = dxxTextJustificationsHorizontal.Right
                    End If
             '---------------------------------------------------------------------------------------------------------
                Case dxxMTextAlignments.MiddleLeft
                    If bVertical Then
                        rVertical = dxxTextJustificationsVertical.Middle
                    Else
                        rHorizontal = dxxTextJustificationsHorizontal.Left
                    End If
                Case dxxMTextAlignments.MiddleCenter
                    If bVertical Then
                        rVertical = dxxTextJustificationsVertical.Middle
                    Else
                        rHorizontal = dxxTextJustificationsHorizontal.Center
                    End If
                Case dxxMTextAlignments.MiddleRight
                    If bVertical Then rVertical = dxxTextJustificationsVertical.Middle Else rHorizontal = dxxTextJustificationsHorizontal.Right
             '---------------------------------------------------------------------------------------------------------
                Case dxxMTextAlignments.TopLeft
                    If bVertical Then
                        rVertical = dxxTextJustificationsVertical.Top
                    Else
                        rHorizontal = dxxTextJustificationsHorizontal.Left
                    End If
                Case dxxMTextAlignments.TopCenter
                    If bVertical Then
                        rVertical = dxxTextJustificationsVertical.Top
                    Else
                        rHorizontal = dxxTextJustificationsHorizontal.Center
                    End If
                Case dxxMTextAlignments.TopRight
                    If bVertical Then
                        rVertical = dxxTextJustificationsVertical.Top
                    Else
                        rHorizontal = dxxTextJustificationsHorizontal.Right
                    End If
            End Select
        End Sub

        Public Shared Function GroupCode(EnumConstant As [Enum]) As Integer
            Dim rNotFound As Boolean = False
            Return GroupCode(EnumConstant, rNotFound)
        End Function
        Public Shared Function GroupCode(EnumConstant As [Enum], ByRef rNotFound As Boolean) As Integer
            Dim _rVal As Integer = 0
            rNotFound = False
            Try
                Dim aTyp As Type = EnumConstant.GetType()
                Dim fi As FieldInfo = aTyp.GetField(EnumConstant.ToString())
                Dim attr() As GroupCodeAttribute =
                              DirectCast(fi.GetCustomAttributes(GetType(GroupCodeAttribute),
                              False), GroupCodeAttribute())
                If attr.Length > 0 Then
                    _rVal = TVALUES.To_INT(attr(0).ToString)
                End If
            Catch ex As Exception
                rNotFound = True
            End Try
            Return _rVal
        End Function

        Public Shared Function ObjectTypeByObjectTypeName(aObjectName As String) As dxxObjectTypes
            If String.IsNullOrWhiteSpace(aObjectName) Then Return dxxObjectTypes.Undefined

            Select Case aObjectName.Trim().ToUpper()
                Case "UNDEFINED"
                    Return dxxObjectTypes.Undefined
                Case "ACAD_PROXY_OBJECT"
                    Return dxxObjectTypes.ProxyObject
                Case "ACDBDICTIONARYWDFLT"
                    Return dxxObjectTypes.DictionaryWDFLT
                Case "ACDBPLACEHOLDER"
                    Return dxxObjectTypes.PlaceHolder
                Case "DATATABLE"
                    Return dxxObjectTypes.DataTable
                Case "DICTIONARY"
                    Return dxxObjectTypes.Dictionary
                Case "DICTIONARYVAR"
                    Return dxxObjectTypes.DictionaryVar

                Case "DIMASSOC"
                    Return dxxObjectTypes.DimAssoc

                Case "ACAD_FIELD"
                    Return dxxObjectTypes.Field
                Case "GROUP"
                    Return dxxObjectTypes.Group
                Case "IDBUFFER"
                    Return dxxObjectTypes.IDBuffer

                Case "IMAGEDEF"
                    Return dxxObjectTypes.ImageDef
                Case "IMAGEDEF_REACTOR"
                    Return dxxObjectTypes.ImageDefReactor
                Case "LAYER_INDEX"
                    Return dxxObjectTypes.LayerIndex
                Case "LAYER_FILTER"
                    Return dxxObjectTypes.LayerFilter
                Case "LAYOUT"
                    Return dxxObjectTypes.Layout
                Case "LIGHTLIST"
                    Return dxxObjectTypes.LightList
                Case "MATERIAL"
                    Return dxxObjectTypes.Material
                Case "MLINESTYLE"
                    Return dxxObjectTypes.MLineStyle
                Case "OBJECT_PTR"
                    Return dxxObjectTypes.ObjectPtr
                Case "PLOTSETTINGS"
                    Return dxxObjectTypes.PlotSetting
                Case "RASTERVARIABLES"
                    Return dxxObjectTypes.RasterVariables
                Case "RENDERENVIRONMENT"
                    Return dxxObjectTypes.RenderEnvironment
                Case "SECTIONMANAGER"
                    Return dxxObjectTypes.SectionManager
                Case "SPATIAL_INDEX"
                    Return dxxObjectTypes.SpatialIndex
                Case "SPATIAL_FILTER"
                    Return dxxObjectTypes.SpatialFilter
                Case "SORTENTSTABLE"
                    Return dxxObjectTypes.SortEntsTable
                Case "SUNSTUDY"
                    Return dxxObjectTypes.SunStudy
                Case "TABLESTYLE"
                    Return dxxObjectTypes.TableStyle
                Case "UNDERLAYDEFINITION"
                    Return dxxObjectTypes.UnderlayDefinition
                Case "VISUALSTYLE"
                    Return dxxObjectTypes.VisualStyle
                Case "VBA_PROJECT"
                    Return dxxObjectTypes.VBAProject
                Case "WIPEOUTVARIABLES"
                    Return dxxObjectTypes.WipeoutVariables
                Case "XRECORD"
                    Return dxxObjectTypes.XRecord
                Case "CELLSTYLEMAP"
                    Return dxxObjectTypes.CellStyleMap
                Case "SCALE"
                    Return dxxObjectTypes.Scale
                Case "TABLECELL"
                    Return dxxObjectTypes.TableCell
                Case "MLEADERSTYLE"
                    Return dxxObjectTypes.MLeaderStyle
                Case "USERDEFINED"
                    Return dxxObjectTypes.UserDefined
                Case Else
                    Return dxxObjectTypes.Undefined
            End Select


        End Function




        Public Shared Function HeaderPropertyType(aHeaderVar As dxxHeaderVars, ByRef rIsVector As Boolean, ByRef rIsDirection As Boolean) As dxxPropertyTypes
            rIsVector = False
            rIsDirection = False
            Select Case aHeaderVar
                Case dxxHeaderVars.INSBASE, dxxHeaderVars.PINSBASE, dxxHeaderVars.EXTMIN, dxxHeaderVars.PEXTMIN, dxxHeaderVars.EXTMAX, dxxHeaderVars.PEXTMAX
                    rIsVector = True

                Case dxxHeaderVars.LIMMIN, dxxHeaderVars.PLIMMIN, dxxHeaderVars.LIMMAX, dxxHeaderVars.PLIMMAX
                    rIsVector = True
                Case dxxHeaderVars.UCSORG, dxxHeaderVars.PUCSORG
                    rIsVector = True
                Case dxxHeaderVars.UCSXDIR, dxxHeaderVars.PUCSXDIR, dxxHeaderVars.UCSYDIR, dxxHeaderVars.PUCSYDIR
                    rIsVector = True
                    rIsDirection = True

                Case dxxHeaderVars.UCSORGTOP, dxxHeaderVars.UCSORGBOTTOM, dxxHeaderVars.UCSORGLEFT, dxxHeaderVars.UCSORGRIGHT, dxxHeaderVars.UCSORGFRONT, dxxHeaderVars.UCSORGBACK
                    rIsVector = True
                Case dxxHeaderVars.PUCSORGTOP, dxxHeaderVars.PUCSORGBOTTOM, dxxHeaderVars.PUCSORGLEFT, dxxHeaderVars.PUCSORGRIGHT, dxxHeaderVars.PUCSORGFRONT, dxxHeaderVars.PUCSORGBACK
                    rIsVector = True
            End Select
            Return aHeaderVar.PropertyType()
        End Function

        Public Shared Function GraphicTypeByEntityTypeName(aEntityName As String) As dxxGraphicTypes

            If String.IsNullOrWhiteSpace(aEntityName) Then Return dxxGraphicTypes.Undefined

            Select Case aEntityName.Trim().ToUpper()
         '===========================
                Case "ARC", "CIRCLE"
                    '===========================
                    Return dxxGraphicTypes.Arc
         '===========================
                Case "INSERT"
                    '===========================
                    Return dxxGraphicTypes.Insert

                     '===========================
                Case "HATCH"
                    '===========================
                    Return dxxGraphicTypes.Hatch

         '===========================
                Case "DIMENSION"
                    '===========================
                    Return dxxGraphicTypes.Dimension
         '===========================
                Case "ELLIPSE"
                    '===========================
                    Return dxxGraphicTypes.Ellipse
         '===========================
                Case "ENDBLK"
                    '===========================
                    Return dxxGraphicTypes.EndBlock
         '===========================
                Case "LINE"
                    '===========================
                    Return dxxGraphicTypes.Line
         '===========================
                Case "LWPOLYLINE", "POLYLINE"
                    '===========================
                    Return dxxGraphicTypes.Polyline
         '===========================
                Case "SOLID", "TRACE"
                    '===========================
                    Return dxxGraphicTypes.Solid
         '===========================
                Case "TEXT", "ATTDEF", "ATTRIB"
                    '===========================
                    Return dxxGraphicTypes.Text
         '===========================
                Case "LEADER"
                    '===========================
                    Return dxxGraphicTypes.Leader
         '===========================
                Case "MTEXT"
                    '===========================
                    Return dxxGraphicTypes.MText
         '===========================
                Case "POINT"
                    '===========================
                    Return dxxGraphicTypes.Point
         '===========================
                Case "SEQEND"
                    '===========================
                    Return dxxGraphicTypes.SequenceEnd
         '===========================
                Case "ENDBLK"
                    '===========================
                    Return dxxGraphicTypes.EndBlock
                Case Else
                    Return dxxGraphicTypes.Undefined
            End Select

        End Function

        Public Shared Function ReferenceTypeByName(aEntryTypeName As String) As dxxReferenceTypes
            If String.IsNullOrWhiteSpace(aEntryTypeName) Then Return dxxReferenceTypes.UNDEFINED
            Select Case aEntryTypeName.Trim().ToUpper()
                        '======================================
                Case "APPID"
                    '======================================
                    Return dxxReferenceTypes.APPID
                        '======================================
                Case "VPORT"
                    '======================================
                    Return dxxReferenceTypes.VPORT
                        '======================================
                Case "BLOCK_RECORD"
                    '======================================
                    Return dxxReferenceTypes.BLOCK_RECORD
                        '======================================
                Case "LTYPE"
                    '======================================
                    Return dxxReferenceTypes.LTYPE
                        '======================================
                Case "LAYER"
                    '======================================
                    Return dxxReferenceTypes.LAYER
                        '======================================
                Case "STYLE"
                    '======================================
                    Return dxxReferenceTypes.STYLE
                        '======================================
                Case "DIMSTYLE"
                    '======================================
                    Return dxxReferenceTypes.DIMSTYLE
                        '======================================
                Case "UCS"
                    '======================================
                    Return dxxReferenceTypes.UCS
                        '======================================
                Case "VIEW"
                    '======================================
                    Return dxxReferenceTypes.VIEW
                Case Else
                    Return dxxReferenceTypes.UNDEFINED
            End Select
        End Function

        Public Shared Function CodeValue(EnumConstant As [Enum]) As String
            Dim _rVal As String = String.Empty
            Try
                Dim aTyp As Type = EnumConstant.GetType()
                Dim fi As FieldInfo = aTyp.GetField(EnumConstant.ToString())
                Dim attr() As CodeValueAttribute =
                              DirectCast(fi.GetCustomAttributes(GetType(CodeValueAttribute),
                              False), CodeValueAttribute())
                If attr.Length > 0 Then
                    _rVal = attr(0).ToString
                End If
            Catch ex As Exception
                _rVal = "#ERR#"
            End Try
            Return _rVal
        End Function
        Public Shared Function Description(EnumConstant As [Enum], Optional aPrefix As String = "") As String
            Dim _rVal As String
            Try
                Dim aTyp As Type = EnumConstant.GetType()
                Dim fi As FieldInfo = aTyp.GetField(EnumConstant.ToString())
                Dim attr() As DescriptionAttribute =
                              DirectCast(fi.GetCustomAttributes(GetType(DescriptionAttribute),
                              False), DescriptionAttribute())
                If attr.Length > 0 Then
                    _rVal = attr(0).Description
                Else
                    _rVal = EnumConstant.ToString()
                    Dim i As Integer = _rVal.IndexOf("_") + 1
                    If i > 0 Then _rVal = Right(_rVal, _rVal.Length - i)
                    _rVal = Replace(_rVal, "_", " ")
                End If
            Catch ex As Exception
                _rVal = "#ERR#"
                aPrefix = ""
            End Try
            Return aPrefix & _rVal
        End Function

        Public Shared Function MemberName(EnumConstant As [Enum]) As String
            Dim _rVal As String
            Try
                Dim aTyp As Type = EnumConstant.GetType()
                Dim fi As FieldInfo = aTyp.GetField(EnumConstant.ToString())
                Return fi.Name
            Catch ex As Exception
                _rVal = "#ERR#"
                Return _rVal
            End Try
            Return _rVal
        End Function

        Public Shared Function PropertyName(EnumConstant As [Enum]) As String
            Dim _rVal As String
            Try
                Dim aTyp As Type = EnumConstant.GetType()
                Dim fi As FieldInfo = aTyp.GetField(EnumConstant.ToString())
                Dim attr() As PropertyNameAttribute =
                              DirectCast(fi.GetCustomAttributes(GetType(PropertyNameAttribute),
                              False), PropertyNameAttribute())
                If attr.Length > 0 Then
                    _rVal = attr(0).ToString
                Else
                    _rVal = EnumConstant.ToString()
                    Dim i As Integer = _rVal.IndexOf("_") + 1
                    If i > 0 Then _rVal = Right(_rVal, _rVal.Length - i)
                    _rVal = Replace(_rVal, "_", " ")
                End If
                _rVal = Prefix(EnumConstant) & _rVal
            Catch ex As Exception
                _rVal = "#ERR#"
                Return _rVal
            End Try
            Return _rVal
        End Function
        Public Shared Function Prefix(EnumConstant As [Enum]) As String
            Try
                Dim aTyp As Type = EnumConstant.GetType()
                Dim fi As FieldInfo = aTyp.GetField(EnumConstant.ToString())
                Dim attr() As PrefixAttribute =
                              DirectCast(fi.GetCustomAttributes(GetType(PrefixAttribute),
                              False), PrefixAttribute())
                If attr.Length > 0 Then Return attr(0).ToString Else Return String.Empty
            Catch ex As Exception
                Return String.Empty
            End Try
        End Function
        Public Shared Function DisplayName(EnumConstant As [Enum], Optional bReturnPropertyNameIfNotFound As Boolean = True) As String
            Dim _rVal As String
            Try
                Dim aTyp As Type = EnumConstant.GetType()
                Dim fi As FieldInfo = aTyp.GetField(EnumConstant.ToString())
                Dim attr() As DisplayNameAttribute =
                              DirectCast(fi.GetCustomAttributes(GetType(DisplayNameAttribute),
                              False), DisplayNameAttribute())
                If attr.Length > 0 Then
                    _rVal = attr(0).ToString
                Else
                    If bReturnPropertyNameIfNotFound Then
                        _rVal = dxfEnums.PropertyName(EnumConstant)
                    Else
                        _rVal = ""
                    End If
                End If
            Catch ex As Exception
                _rVal = "#ERR#"
                Return _rVal
            End Try
            Return _rVal
        End Function

        Friend Shared Function TableTypeToRefType(aTableType As dxxTableTypes) As dxxReferenceTypes
            Select Case aTableType
                Case dxxTableTypes.AppID
                    Return dxxReferenceTypes.APPID
                Case dxxTableTypes.BlockRecord
                    Return dxxReferenceTypes.BLOCK_RECORD
                Case dxxTableTypes.DimStyle
                    Return dxxReferenceTypes.DIMSTYLE
                Case dxxTableTypes.Layer
                    Return dxxReferenceTypes.LAYER
                Case dxxTableTypes.LType
                    Return dxxReferenceTypes.LTYPE
                Case dxxTableTypes.Style
                    Return dxxReferenceTypes.STYLE
                Case dxxTableTypes.UCS
                    Return dxxReferenceTypes.UCS
                Case dxxTableTypes.View
                    Return dxxReferenceTypes.VIEW
                Case dxxTableTypes.VPort
                    Return dxxReferenceTypes.VPORT
                Case Else
                    Return dxxReferenceTypes.UNDEFINED

            End Select
        End Function

        Public Shared Function EnumValues(EnumConstant As Type) As Dictionary(Of String, Integer)
            Dim _rVal As New Dictionary(Of String, Integer)

            Try
                Dim MembersNames() As String = System.Enum.GetNames(EnumConstant)
                Dim MembersVals As Array = System.Enum.GetValues(EnumConstant)
                For i As Integer = 0 To MembersNames.Length - 1
                    Dim nm As String = MembersNames(i)
                    Dim vl As Integer = TVALUES.To_INT(MembersVals.GetValue(i))
                    _rVal.Add(nm, vl)
                Next
            Catch ex As Exception
            End Try
            Return _rVal
        End Function
        Public Shared Function EnumValueList(EnumConstant As Type) As List(Of Integer)
            Dim _rVal As New List(Of Integer)
            Try
                Dim MembersNames() As String = System.Enum.GetNames(EnumConstant)
                Dim MembersVals As Array = System.Enum.GetValues(EnumConstant)
                For i As Integer = 0 To MembersNames.Length - 1
                    Dim nm As String = MembersNames(i)
                    Dim vl As Integer = TVALUES.To_INT(MembersVals.GetValue(i))
                    _rVal.Add(vl)
                Next
            Catch ex As Exception
            End Try
            Return _rVal
        End Function
        Public Shared Function ValueNameList(aType As Type) As String
            Try
                Dim ary As Array = System.Enum.GetValues(aType)
                Dim nms() As String = System.Enum.GetNames(aType)
                Dim rVal As String = String.Empty
                Dim i As Integer = 0
                For Each o As Object In ary
                    i += 1
                    Dim ival As Integer = TVALUES.To_INT(o)
                    If i > 1 Then rVal += ","
                    rVal += $"{ival}={ nms(i - 1)}"
                Next

                Return rVal
            Catch ex As Exception
                Return String.Empty
            End Try
        End Function
        Public Shared Function Validate(aType As Type, aValue As Integer, Optional aSkipList As String = Nothing, Optional bSkipNegatives As Boolean = False) As Boolean
            Try
                If Not System.Enum.IsDefined(aType, aValue) Then Return False
                Dim ary As Array = System.Enum.GetValues(aType)
                Dim sSkip As String = String.Empty
                If aSkipList IsNot Nothing Then sSkip = aSkipList.Trim()

                For Each o As Object In ary

                    Dim ival As Integer = TVALUES.To_INT(o)
                    Dim sval As String = ival.ToString()
                    If Not TLISTS.Contains(sSkip, sval, ",") Then
                        If Not bSkipNegatives Or (bSkipNegatives And ival >= 0) Then
                            If ival = aValue Then
                                Return True
                            End If
                        End If
                    End If

                Next


                Return False
            Catch ex As Exception
                Return False
            End Try
        End Function

    End Class
    Friend Class DisplayNameAttribute
        Inherits Attribute
        Private sValue As String
        Public Sub New(aValue As String)
            sValue = aValue
        End Sub
        Public Overrides Function ToString() As String
            Return sValue
        End Function
    End Class
    Friend Class GroupCodeAttribute
        Inherits Attribute
        Private sValue As String
        Public Sub New(aValue As String)
            sValue = aValue
        End Sub
        Public Overrides Function ToString() As String
            Return sValue
        End Function
    End Class
    Friend Class PropertyNameAttribute
        Inherits Attribute
        Private sValue As String
        Public Sub New(aValue As String)
            sValue = aValue
        End Sub
        Public Overrides Function ToString() As String
            Return sValue
        End Function
    End Class
    Friend Class PrefixAttribute
        Inherits Attribute
        Private sValue As String
        Public Sub New(aValue As String)
            sValue = aValue
        End Sub
        Public Overrides Function ToString() As String
            Return sValue
        End Function
    End Class
    Friend Class CodeValueAttribute
        Inherits Attribute
        Private sValue As String
        Public Sub New(aValue As String)
            sValue = aValue
        End Sub
        Public Overrides Function ToString() As String
            Return sValue
        End Function
    End Class
#Region "Enums"

    Public Enum dxxBorderPointers
        All = -1
        Left = 0
        Bottom = 1
        Right = 2
        Top = 3
    End Enum

    Public Enum dxxCellTextTypes
        All = -1
        Field = 0
        Header = 1
        Title = 2
        Footer = 3
    End Enum
    '
    ' Summary:
    '     Color index used to get a system color from GetSysColor.
    Public Enum dxxReferenceTypes
        <DisplayName("Undefined")> UNDEFINED = 0
        <DisplayName("APPID")> APPID = 1
        <DisplayName("Block Record")> BLOCK_RECORD = 2
        <DisplayName("DimStyle")> DIMSTYLE = 4
        <DisplayName("Layer")> LAYER = 8
        <DisplayName("Linetype")> LTYPE = 16
        <DisplayName("Style")> STYLE = 32
        <DisplayName("UCS")> UCS = 64
        <DisplayName("View")> VIEW = 128
        <DisplayName("Viewport")> VPORT = 256
        <DisplayName("Dimension Setting")> DIMSETTINGS = 512
        <DisplayName("Table Setting")> TABLESETTINGS = 1024
        <DisplayName("Symbol Setting")> SYMBOLSETTINGS = 2048
        <DisplayName("Header")> HEADER = 4096
        <DisplayName("DimStyle Override")> DIMOVERRIDES = 8192
        <DisplayName("Text Setting")> TEXTSETTINGS = 16384
        <DisplayName("Linetype Setting")> LINETYPESETTINGS = 32768
        <DisplayName("Screen Setting")> SCREENSETTINGS = 65536
        <DisplayName("Display Setting")> DISPLAYSETTINGS = 131072
    End Enum 'dxxReferenceTypes

    Public Enum dxxSettingTypes
        <DisplayName("Undefined")> UNDEFINED = 0
        <DisplayName("Dimension Setting")> DIMSETTINGS = 512
        <DisplayName("Table Setting")> TABLESETTINGS = 1024
        <DisplayName("Symbol Setting")> SYMBOLSETTINGS = 2048
        <DisplayName("Header")> HEADER = 4096
        <DisplayName("DimStyle Override")> DIMOVERRIDES = 8192
        <DisplayName("Text Setting")> TEXTSETTINGS = 16384
        <DisplayName("Linetype Setting")> LINETYPESETTINGS = 32768
        <DisplayName("Screen Setting")> SCREENSETTINGS = 65536
        <DisplayName("Display Setting")> DISPLAYSETTINGS = 131072
    End Enum 'dxxReferenceTypes

    Public Enum dxxSystemColorIndex
        '
        ' Summary:
        '     Scroll bar gray area.
        COLOR_SCROLLBAR = 0
        '
        ' Summary:
        '     Desktop.
        COLOR_BACKGROUND = 1
        '
        ' Summary:
        '     Desktop.
        COLOR_DESKTOP = 1
        '
        ' Summary:
        '     Active window title bar. The associated foreground color is COLOR_CAPTIONTEXT.
        '     Specifies the left side color in the color gradient of an active window's title
        '     bar if the gradient effect is enabled.
        COLOR_ACTIVECAPTION = 2
        '
        ' Summary:
        '     Inactive window caption. The associated foreground color is COLOR_INACTIVECAPTIONTEXT.
        '     Specifies the left side color in the color gradient of an inactive window's title
        '     bar if the gradient effect is enabled.
        COLOR_INACTIVECAPTION = 3
        '
        ' Summary:
        '     Menu background. The associated foreground color is COLOR_MENUTEXT.
        COLOR_MENU = 4
        '
        ' Summary:
        '     Window background. The associated foreground colors are COLOR_WINDOWTEXT and
        '     COLOR_HOTLITE.
        COLOR_WINDOW = 5
        '
        ' Summary:
        '     Window frame.
        COLOR_WINDOWFRAME = 6
        '
        ' Summary:
        '     Text in menus. The associated background color is COLOR_MENU.
        COLOR_MENUTEXT = 7
        '
        ' Summary:
        '     Text in windows. The associated background color is COLOR_WINDOW.
        COLOR_WINDOWTEXT = 8
        '
        ' Summary:
        '     Text in caption, size box, and scroll bar arrow box. The associated background
        '     color is COLOR_ACTIVECAPTION.
        COLOR_CAPTIONTEXT = 9
        '
        ' Summary:
        '     Active window border.
        COLOR_ACTIVEBORDER = 10
        '
        ' Summary:
        '     Inactive window border.
        COLOR_INACTIVEBORDER = 11
        '
        ' Summary:
        '     Background color of multiple document interface (MDI) applications.
        COLOR_APPWORKSPACE = 12
        '
        ' Summary:
        '     Item(s) selected in a control. The associated foreground color is COLOR_HIGHLIGHTTEXT.
        COLOR_HIGHLIGHT = 13
        '
        ' Summary:
        '     Text of item(s) selected in a control. The associated background color is COLOR_HIGHLIGHT.
        COLOR_HIGHLIGHTTEXT = 14
        '
        ' Summary:
        '     Face color for three-dimensional display elements and for dialog box backgrounds.
        '     The associated foreground color is COLOR_BTNTEXT.
        COLOR_BTNFACE = 15
        '
        ' Summary:
        '     Face color for three-dimensional display elements and for dialog box backgrounds.
        COLOR_3DFACE = 15
        '
        ' Summary:
        '     Shadow color for three-dimensional display elements (for edges facing away from
        '     the light source).
        COLOR_BTNSHADOW = 16
        '
        ' Summary:
        '     Shadow color for three-dimensional display elements (for edges facing away from
        '     the light source).
        COLOR_3DSHADOW = 16
        '
        ' Summary:
        '     Grayed (disabled) text. This color is set to 0 if the current display driver
        '     does not support a solid gray color.
        COLOR_GRAYTEXT = 17
        '
        ' Summary:
        '     Text on push buttons. The associated background color is COLOR_BTNFACE.
        COLOR_BTNTEXT = 18
        '
        ' Summary:
        '     Inactive window caption. The associated foreground color is COLOR_INACTIVECAPTIONTEXT.
        '     Specifies the left side color in the color gradient of an inactive window's title
        '     bar if the gradient effect is enabled.
        COLOR_INACTIVECAPTIONTEXT = 19
        '
        ' Summary:
        '     Highlight color for three-dimensional display elements (for edges facing the
        '     light source.)
        COLOR_BTNHIGHLIGHT = 20
        '
        ' Summary:
        '     Highlight color for three-dimensional display elements (for edges facing the
        '     light source.)
        COLOR_3DHIGHLIGHT = 20
        '
        ' Summary:
        '     Highlight color for three-dimensional display elements (for edges facing the
        '     light source.)
        COLOR_3DHILIGHT = 20
        '
        ' Summary:
        '     Highlight color for three-dimensional display elements (for edges facing the
        '     light source.)
        COLOR_BTNHILIGHT = 20
        '
        ' Summary:
        '     Dark shadow for three-dimensional display elements.
        COLOR_3DDKSHADOW = 21
        '
        ' Summary:
        '     Light color for three-dimensional display elements (for edges facing the light
        '     source.)
        COLOR_3DLIGHT = 22
        '
        ' Summary:
        '     Text color for tooltip controls. The associated background color is COLOR_INFOBK.
        COLOR_INFOTEXT = 23
        '
        ' Summary:
        '     Background color for tooltip controls. The associated foreground color is COLOR_INFOTEXT.
        COLOR_INFOBK = 24
        '
        ' Summary:
        '     Item(s) selected in a control. The associated foreground color is COLOR_HIGHLIGHTTEXT.
        COLOR_HOTLIGHT = 26
        '
        ' Summary:
        '     Right side color in the color gradient of an active window's title bar. COLOR_ACTIVECAPTION
        '     specifies the left side color. Use SPI_GETGRADIENTCAPTIONS with the SystemParametersInfo
        '     function to determine whether the gradient effect is enabled.
        COLOR_GRADIENTACTIVECAPTION = 27
        '
        ' Summary:
        '     Right side color in the color gradient of an inactive window's title bar. COLOR_INACTIVECAPTION
        '     specifies the left side color.
        COLOR_GRADIENTINACTIVECAPTION = 28
        '
        ' Summary:
        '     The color used to highlight menu items when the menu appears as a flat menu (see
        '     SystemParametersInfo). The highlighted menu item is outlined with COLOR_HIGHLIGHT.
        '     Windows 2000: This value is not supported.
        COLOR_MENUHILIGHT = 29
        '
        ' Summary:
        '     The background color for the menu bar when menus appear as flat menus (see SystemParametersInfo).
        '     However, COLOR_MENU continues to specify the background color of the menu popup.
        '     Windows 2000: This value is not supported.
        COLOR_MENUBAR = 30
    End Enum
    Public Enum RasterOpConstants
        vbDstInvert = &H550009 'Inverts the destination bitmap
        vbMergeCopy = &HC000CA 'Combines the pattern and the source bitmap
        vbMergePaint = &HBB0226 'Combines the inverted source bitmap with the destination bitmap by using Or
        vbNotSrcCopy = &H330008 'Copies the inverted source bitmap to the destination
        vbNotSrcErase = &H1100A6 'Inverts the result of combining the destination and source bitmaps by using Or
        vbPatCopy = &HF00021L 'Copies the pattern to the destination bitmap
        vbPatInvert = &H5A0049L 'Combines the destination bitmap with the pattern by using Xor
        vbPatPaint = &HFB0A09L 'Combines the inverted source bitmap with the pattern by using Or. Combines the result of this operation with the destination bitmap by using Or
        vbSrcAnd = &H8800C6 'Combines pixels of the destination and source bitmaps by using And
        vbSrcCopy = &HCC0020 'Copies the source bitmap to the destination bitmap
        vbSrcErase = &H440328 'Inverts the destination bitmap and combines the result with the source bitmap by using And
        vbSrcInvert = &H660046 'Combines pixels of the destination and source bitmaps by using Xor
        vbSrcPaint = &HEE0086 'Combines pixels of the destination and source bitmaps by using Or
    End Enum
    Public Enum dxxAttributeVisibilityModes
        None = 0
        Normal = 1
        ViewAll = 2
    End Enum 'dxxAttributeVisibilityModes
    Public Enum dxxSplineTypes
        <Description("Quadratic B-spline")>
        Quadratic = 5
        <Description("Cubic B-spline")>
        Cubic = 6
    End Enum 'dxxSplineTypes
    Public Enum dxxShadeEdgeSettings
        <Description("Faces shaded, edges not highlighted")>
        FaceNoEdges = 0
        <Description("Faces shaded, edges highlighted in black")>
        FaceAndEdgesBlack = 1
        <Description("Faces not filled, edges in entity color")>
        EdgesOnly = 2
        <Description("Faces in entity color, edges in black")>
        FaceAndEdges = 3
    End Enum 'dxxShadeEdgeSettings
    Public Enum dxxDrawingUnitBasis
        Unitless = 0
        Inches = 1
        Feet = 2
        Miles = 3
        Millimeters = 4
        Centimeters = 5
        Meters = 6
        Kilometers = 7
        MIcoinches = 8
        Mils = 9
        Yards = 10
        Angstroms = 11
        Nanometers = 12
        Microns = 13
        Decimeters = 14
        Decameters = 15
        Hectometers = 16
        Gigameters = 17
        AstroUnits = 18
        LightYears = 19
        Parsects = 20
    End Enum 'dxxDrawingUntis
    Public Enum dxxEndCaps
        None = 0
        Round = 1
        Angle = 2
        Square = 3
    End Enum 'dxxEndCaps
    Public Enum dxxJoinStyles
        None = 0
        Rounds = 1
        Angle = 2
        Flat = 3
    End Enum
    Public Enum dxxPlotStyleTypes
        ByLayer = 0
        ByBlock = 1
        ByDictionaryDefault = 2
        ByObjectID = 3
    End Enum 'dxxPlotStyleTypes
    Public Enum dxxSortEntsCode
        Disabled = 0
        ForObjectSelection = 1
        ForObjectSnap = 2
        ForRedraws = 4
        ForMSLIDECration = 8
        ForRegen = 16
        ForPlotting = 32
        ForPostScripting = 64
        All = 127
    End Enum 'dxxSortEntsCode
    Public Enum dxxLineStyles
        Off = 0
        Solid = 1
        Dashed = 2
        Dotted = 3
        ShortDash = 4
        MediumDash = 5
        LongDash = 6
        DoubleShortDash = 7
        DoubleMediumDash = 8
        DoubleLongDash = 9
        MediumLongDash = 10
        SparseDot = 11
    End Enum 'dxxLineStyles
    Public Enum dxxLoftParams
        <Description("Minimizes the twist between cross sections")>
        NoTwist = 1
        <Description("Aligns the start to end direction of each cross section curve")>
        Align = 2
        <Description("Produces simple solids and surfaces, such as a cylinder or plane, instead of spline solids and surfacess")>
        Simplify = 4
        <Description("closes the surface or solid between the first and the last cross sections")>
        Closed = 8
    End Enum 'dxxLoftParams
    Public Enum dxxLoftNormals
        Ruled = 0
        SmoothFit = 1
        StartCross = 2
        EndCross = 3
        StartAndEndCross = 4
        AllCross = 5
        DraftAngleMagnitude = 6
    End Enum 'dxxLoftNormals
    Public Enum dxxTimeZones
        IDLWest = -12000
        MidaySamoa = -11000
        Hawaii = -10000
        Alaska = -9000
        Pacific = -8000
        Mountain = -7000
        Arizona = -7001
        MAzatlan = -7002
        CentralAmerica = -6000
        Central = -6001
        Monterrey = -6002
        Saskatcewan = -6003
        Eastern = -5000
        Indiana = -5001
        Bogata = -5002
        Atlantic = -4000
        Caracas = -4001
        Santiago = -4002
        NewFoundland = -3300
        Brasilia = -3000
        BuenosAires = -3001
        Greenland = -3002
        MidAtlantic = -2000
        Azores = -1000
        CapeVerde = -1001
        UCT = 0
        GMT = 1
        Casablanca = 2
        Amsterdam = 1000
        Madrid = 1001
        Prague = 1003
        CentralAfrica = 1004
        Athens = 2000
        Bucharest = 2001
        Cairo = 2002
        Pretoria = 2003
        Helsinki = 2004
        Jerusalem = 2005
        Moscow = 3000
        Kuwait = 3001
        Bagdad = 3002
        Nairobi = 3003
        Tehran = 3300
        AbuDhabi = 4000
        Ekaterinburg = 5000
        Islamabad = 5001
        Mumbai = 5300
        Kathmandu = 5450
        Novosibirsk = 6000
        Dhaka = 6001
        Jayawardenepura = 6002
        Rangoon = 6300
        Bangkok = 7000
        Krasnoyarsk = 7001
        Beijing = 8000
        Singapore = 8001
        Taipei = 8002
        Irkutsk = 8003
        Perth = 8004
        Osaka = 9000
        Seoul = 9001
        Yakutsk = 9002
        Adelaide = 9300
        Darwon = 9301
        Sydney = 10000
        Guam = 10001
        Brisbane = 10002
        Hobart = 10003
        Vladivostok = 10004
        Magadan = 11000
        Aukland = 12000
        Fiji = 12001
        Tanga = 13000
    End Enum 'dxxTimeZones
    Public Enum dxxHeaderVars
        <Description("The AutoCAD drawing database version number.")> <PropertyName("$ACADVER")> <GroupCode("1")> ACADVER = 1
        <Description("Maintenance version number (should be ignored)")> <PropertyName("$ACADMAINTVER")> <GroupCode("70")> ACADMAINTVER = 2
        <Description("Drawing code page; set to the system code page when a new drawing is created, but not otherwise maintained by AutoCAD")>
        <PropertyName("$DWGCODEPAGE")> <GroupCode("3")> DWGCODEPAGE = 3
        <Description("Name of the last user to save the current drawing")> <PropertyName("$LASTSAVEDBY")> <GroupCode("1")> LASTSAVEDBY = 4
        <Description("Insertion base set by BASE command (in WCS)")> <PropertyName("$INSBASE")> <GroupCode("10")> INSBASE = 5
        <Description("X, Y, and Z drawing extents lower-left corner (in WCS)")> <PropertyName("$EXTMIN")> <GroupCode("10")> EXTMIN = 6
        <Description("X, Y, And Z drawing extents upper-right corner (In WCS)")> <PropertyName("$EXTMAX")> <GroupCode("10")> EXTMAX = 7
        <Description("XY drawing limits lower-left corner (in WCS)")> <PropertyName("$LIMMIN")> <GroupCode("10")> LIMMIN = 8
        <Description("XY drawing limits upper-right corner (in WCS)")> <PropertyName("$LIMMAX")> <GroupCode("10")> LIMMAX = 9
        <Description("Ortho mode on if nonzero")> <PropertyName("$ORTHOMODE")> <GroupCode("70")> ORTHOMODE = 10
        <Description("REGENAUTO mode on if nonzero")> <PropertyName("$REGENMODE")> REGENMODE = 11
        <Description("Fill mode on if nonzero")> <PropertyName("$FILLMODE")> <GroupCode("70")> FILLMODE = 12
        <Description("Quick Text mode on if nonzero")> <PropertyName("$QTEXTMODE")> <GroupCode("70")> QTEXTMODE = 13
        <Description("Mirror text if nonzero")> <PropertyName("$MIRRTEXT")> <GroupCode("70")> MIRRTEXT = 14
        <Description("Global linetype scale")> <PropertyName("$LTSCALE")> <GroupCode("40")> LTSCALE = 15
        <Description("Attribute visibility")> <PropertyName("$ATTMODE")> <GroupCode("70")> ATTMODE = 16
        <Description("Default text height")> <PropertyName("$TEXTSIZE")> <GroupCode("40")> TEXTSIZE = 17
        <Description("Default trace width")> <PropertyName("$TRACEWID")> <GroupCode("40")> TRACEWID = 18
        <Description("Current text style name")> <PropertyName("$TEXTSTYLE")> <GroupCode("7")> TEXTSTYLE = 19
        <Description("Current layer name")> <PropertyName("$CLAYER")> <GroupCode("8")> CLAYER = 20
        <Description("Entity linetype name, or BYBLOCK or BYLAYER")> <PropertyName("$CELTYPE")> <GroupCode("6")> CELTYPE = 21
        <Description("Current entity color number")> <PropertyName("$CECOLOR")> <GroupCode("62")> CECOLOR = 22
        <Description("Current entity linetype scale")> <PropertyName("$CELTSCALE")> <GroupCode("40")> CELTSCALE = 23
        <Description("Controls the display of silhouette curves of body objects in Wireframe mode")> <PropertyName("$DISPSILH")> DISPSILH = 24
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMSCALE")> <GroupCode("40")> DIMSCALE = 25
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMASZ")> <GroupCode("40")> DIMASZ = 26
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMEXO")> <GroupCode("40")> DIMEXO = 27
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMDLI")> <GroupCode("40")> DIMDLI = 28
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMRND")> <GroupCode("40")> DIMRND = 29
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMDLE")> <GroupCode("40")> DIMDLE = 30
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMEXE")> <GroupCode("40")> DIMEXE = 31
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMTP")> <GroupCode("40")> DIMTP = 32
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMTM")> <GroupCode("40")> DIMTM = 33
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMTXT")> <GroupCode("40")> DIMTXT = 34
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMCEN")> <GroupCode("40")> DIMCEN = 35
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMTSZ")> <GroupCode("40")> DIMTSZ = 36
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMTOL")> <GroupCode("70")> DIMTOL = 37
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMLIM")> <GroupCode("70")> DIMLIM = 38
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMTIH")> <GroupCode("70")> DIMTIH = 39
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMTOH")> <GroupCode("70")> DIMTOH = 40
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMSE1")> <GroupCode("70")> DIMSE1 = 41
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMSE2")> <GroupCode("70")> DIMSE2 = 42
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMTAD")> <GroupCode("70")> DIMTAD = 43
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMZIN")> <GroupCode("70")> DIMZIN = 44
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMBLK")> <GroupCode("1")> DIMBLK = 45
        <Description("Controls the associativity of dimension objects. OBSOLETE use DIMASSOC")> <PropertyName("$DIMASO")> <GroupCode("70")> DIMASO = 46
        <Description("Controls redefinition of dimension objects while dragging")> <PropertyName("$DIMSHO")> <GroupCode("70")> DIMSHO = 47
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMPOST")> <GroupCode("1")> DIMPOST = 48
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMAPOST")> <GroupCode("1")> DIMAPOST = 49
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMALT")> <GroupCode("70")> DIMALT = 50
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMALTD")> <GroupCode("70")> DIMALTD = 51
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMALTF")> <GroupCode("40")> DIMALTF = 52
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMLFAC")> <GroupCode("40")> DIMLFAC = 53
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMTOFL")> <GroupCode("70")> DIMTOFL = 54
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMTVP")> <GroupCode("40")> DIMTVP = 55
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMTIX")> <GroupCode("70")> DIMTIX = 56
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMSOXD")> <GroupCode("70")> DIMSOXD = 57
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMSAH")> <GroupCode("70")> DIMSAH = 58
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMBLK1")> <GroupCode("1")> DIMBLK1 = 59
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMBLK2")> <GroupCode("1")> DIMBLK2 = 60
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMSTYLE")> <GroupCode("2")> DIMSTYLE = 61
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMCLRD")> <GroupCode("70")> DIMCLRD = 62
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMCLRE")> <GroupCode("70")> DIMCLRE = 63
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMCLRT")> <GroupCode("70")> DIMCLRT = 64
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMTFAC")> <GroupCode("40")> DIMTFAC = 65
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMGAP")> <GroupCode("40")> DIMGAP = 66
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMJUST")> <GroupCode("70")> DIMJUST = 67
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMSD1")> <GroupCode("70")> DIMSD1 = 68
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMSD2")> <GroupCode("70")> DIMSD2 = 69
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMTOLJ")> <GroupCode("70")> DIMTOLJ = 70
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMTZIN")> <GroupCode("70")> DIMTZIN = 71
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMALTZ")> <GroupCode("70")> DIMALTZ = 72
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMALTTZ")> <GroupCode("70")> DIMALTTZ = 73
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMUPT")> <GroupCode("70")> DIMUPT = 74
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMDEC")> <GroupCode("70")> DIMDEC = 75
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMTDEC")> <GroupCode("70")> DIMTDEC = 76
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMALTU")> <GroupCode("70")> DIMALTU = 77
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMALTTD")> <GroupCode("70")> DIMALTTD = 78
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMTXSTY")> <GroupCode("7")> DIMTXSTY = 79
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMAUNIT")> <GroupCode("70")> DIMAUNIT = 80
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMADEC")> <GroupCode("70")> DIMADEC = 81
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMALTRND")> <GroupCode("40")> DIMALTRND = 82
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMAZIN")> <GroupCode("70")> DIMAZIN = 83
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMDSEP")> <GroupCode("70")> DIMDSEP = 84
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMATFIT")> <GroupCode("70")> DIMATFIT = 85
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMFRAC")> <GroupCode("70")> DIMFRAC = 86
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMLDRBLK")> <GroupCode("1")> DIMLDRBLK = 87
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMLUNIT")> <GroupCode("70")> DIMLUNIT = 88
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMLWD")> <GroupCode("70")> DIMLWD = 89
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMLWE")> <GroupCode("70")> DIMLWE = 90
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMTMOVE")> <GroupCode("70")> DIMTMOVE = 91
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMFXL")> <GroupCode("40")> DIMFXL = 92
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMFXLON")> <GroupCode("70")> DIMFXLON = 93
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMJOGANG")> <GroupCode("40")> DIMJOGANG = 94
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMTFILL")> <GroupCode("70")> DIMTFILL = 95
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMTFILLCLR")> <GroupCode("70")> DIMTFILLCLR = 96
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMARCSYM")> <GroupCode("70")> DIMARCSYM = 97
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMLTYPE")> <GroupCode("6")> DIMLTYPE = 98
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMLTEX1")> <GroupCode("6")> DIMLTEX1 = 99
        <Description("Refer to DIMSTYLE Properties")> <PropertyName("$DIMLTEX2")> <GroupCode("6")> DIMLTEX2 = 100
        <Description("Units format for coordinates and distances")> <PropertyName("$LUNITS")> <GroupCode("70")> LUNITS = 101
        <Description("Units precision for coordinates and distances")> <PropertyName("$LUPREC")> <GroupCode("70")> LUPREC = 102
        <Description("Sketch record increment")> <PropertyName("$SKETCHINC")> <GroupCode("40")> SKETCHINC = 103
        <Description("Fillet radius")> <PropertyName("$FILLETRAD")> <GroupCode("40")> FILLETRAD = 104
        <Description("Units format for angles")> <PropertyName("$AUNITS")> <GroupCode("70")> AUNITS = 105
        <Description("Units precision for angles")> <PropertyName("$AUPREC")> <GroupCode("70")> AUPREC = 106
        <Description("Name of menu file")> <PropertyName("$MENU")> <GroupCode("1")> MENU = 107
        <Description("Current elevation set by ELEV command")> <PropertyName("$ELEVATION")> <GroupCode("40")> ELEVATION = 108
        <Description("Current paper space elevation")> <PropertyName("$PELEVATION")> <GroupCode("40")> PELEVATION = 109
        <Description("Current thickness set by ELEV command")> <PropertyName("$THICKNESS")> <GroupCode("40")> THICKNESS = 110
        <Description("Nonzero if limits checking is on")> <PropertyName("$LIMCHECK")> <GroupCode("70")> LIMCHECK = 111
        <Description("First chamfer distance")> <PropertyName("$CHAMFERA")> <GroupCode("40")> CHAMFERA = 112
        <Description("Second chamfer distance")> <PropertyName("$CHAMFERB")> <GroupCode("40")> CHAMFERB = 113
        <Description("Chamfer length")> <PropertyName("$CHAMFERC")> <GroupCode("40")> CHAMFERC = 114
        <Description("Chamfer angle")> <PropertyName("$CHAMFERD")> <GroupCode("40")> CHAMFERD = 115
        <Description("0 = Sketch lines; 1 = Sketch polylines")> <PropertyName("$SKPOLY")> <GroupCode("70")> SKPOLY = 116
        <Description("Local date/time of drawing creation")> <PropertyName("$TDCREATE")> <GroupCode("40")> TDCREATE = 117
        <Description("Universal date/time the drawing was created")> <PropertyName("$TDUCREATE")> <GroupCode("40")> TDUCREATE = 118
        <Description("Local date/time of last drawing update")> <PropertyName("$TDUPDATE")> <GroupCode("40")> TDUPDATE = 119
        <Description("Universal date/time of the last update/save")> <PropertyName("$TDUUPDATE")> <GroupCode("40")> TDUUPDATE = 120
        <Description("Cumulative editing time for this drawing")> <PropertyName("$TDINDWG")> <GroupCode("40")> TDINDWG = 121
        <Description("User-elapsed timer")> <PropertyName("$TDUSRTIMER")> <GroupCode("40")> TDUSRTIMER = 122
        <Description("0 = Timer off; 1 = Timer on")> <PropertyName("$USRTIMER")> <GroupCode("70")> USRTIMER = 123
        <Description("Angle 0 direction")> <PropertyName("$ANGBASE")> <GroupCode("50")> ANGBASE = 124
        <Description("Default Angular direction")> <PropertyName("$ANGDIR")> <GroupCode("70")> ANGDIR = 125
        <Description("Point display mode")> <PropertyName("$PDMODE")> <GroupCode("70")> PDMODE = 126
        <Description("Point display size")> <PropertyName("$PDSIZE")> <GroupCode("40")> PDSIZE = 127
        <Description("Default polyline width")> <PropertyName("$PLINEWID")> <GroupCode("40")> PLINEWID = 128
        <Description("Spline control polygon display")> <PropertyName("$SPLFRAME")> SPLFRAME = 129
        <Description("Spline curve type for PEDIT Spline")> <PropertyName("$SPLINETYPE")> <GroupCode("70")> SPLINETYPE = 130
        <Description("Number of line segments per spline patch")> <PropertyName("$SPLINESEGS")> <GroupCode("70")> SPLINESEGS = 131
        <Description("Next available handle")> <PropertyName("$HANDSEED")> <GroupCode("5")> HANDSEED = 133
        <Description("Number of mesh tabulations in first direction")> <PropertyName("$SURFTAB1")> <GroupCode("70")> SURFTAB1 = 134
        <Description("Number of mesh tabulations in second direction")> <PropertyName("$SURFTAB2")> <GroupCode("70")> SURFTAB2 = 135
        <Description("Surface type for PEDIT Smooth")> <PropertyName("$SURFTYPE")> <GroupCode("70")> SURFTYPE = 136
        <Description("surface density (for PEDIT Smooth) in M direction")> <PropertyName("$SURFU")> <GroupCode("70")> SURFU = 137
        <Description("Surface density (for PEDIT Smooth) in N direction")> <PropertyName("$SURFV")> <GroupCode("70")> SURFV = 138
        <Description("Stores the name of the UCS that defines the origin and orientation of orthographic UCS settings.")> <PropertyName("$UCSBASE")> <GroupCode("2")> UCSBASE = 139
        <Description("Name of current UCS")> <PropertyName("$UCSNAME")> <GroupCode("2")> UCSNAME = 140
        <Description("Origin of current UCS (in WCS)")> <PropertyName("$UCSORG")> <GroupCode("10")> UCSORG = 141
        <Description("Direction of the current UCS X axis (in WCS)")> <PropertyName("$UCSXDIR")> <GroupCode("10")> UCSXDIR = 142
        <Description("Direction of the current UCS Y axis (in WCS)")> <PropertyName("$UCSYDIR")> <GroupCode("10")> UCSYDIR = 143
        <Description("If model space UCS is orthographic (UCSORTHOVIEW not equal to 0), this is the name of the UCS that the orthographic UCS is relative to. If blank, UCS is relative to WORLD")> <PropertyName("$UCSORTHOREF")> <GroupCode("2")> UCSORTHOREF = 144
        <Description("Orthographic view type of model space UCS")> <PropertyName("$UCSORTHOVIEW")> <GroupCode("70")> UCSORTHOVIEW = 145
        <Description("Point which becomes the new UCS origin after changing model space UCS to TOP when UCSBASE is set to WORLD")> <PropertyName("$UCSORGTOP")> <GroupCode("10")> UCSORGTOP = 146
        <Description("Point which becomes the new UCS origin after changing model space UCS to BOTTOM when UCSBASE is set to WORLD")> <PropertyName("$UCSORGBOTTOM")> <GroupCode("10")> UCSORGBOTTOM = 147
        <Description("Point which becomes the new UCS origin after changing model space UCS to LEFT when UCSBASE is set to WORLD")> <PropertyName("$UCSORGLEFT")> <GroupCode("10")> UCSORGLEFT = 148
        <Description("Point which becomes the new UCS origin after changing model space UCS to RIGHT when UCSBASE is set to WORLD")> <PropertyName("$UCSORGRIGHT")> <GroupCode("10")> UCSORGRIGHT = 149
        <Description("Point which becomes the new UCS origin after changing model space UCS to FRONT when UCSBASE is set to WORLD")> <PropertyName("$UCSORGFRONT")> <GroupCode("10")> UCSORGFRONT = 150
        <Description("Point which becomes the new UCS origin after changing model space UCS to BACK when UCSBASE is set to WORLD")> <PropertyName("$UCSORGBACK")> <GroupCode("10")> UCSORGBACK = 151
        <Description("Name of the UCS that defines the origin and orientation of orthographic UCS settings (paper space only)")> <PropertyName("$PUCSBASE")> <GroupCode("2")> PUCSBASE = 152
        <Description("Current paper space UCS name")> <PropertyName("$PUCSNAME")> <GroupCode("2")> PUCSNAME = 153
        <Description("Current paper space UCS origin")> <PropertyName("$PUCSORG")> <GroupCode("10")> PUCSORG = 154
        <Description("Current paper space UCS X axis")> <PropertyName("$PUCSXDIR")> <GroupCode("10")> PUCSXDIR = 155
        <Description("Current paper space UCS Y axis")> <PropertyName("$PUCSYDIR")> <GroupCode("10")> PUCSYDIR = 156
        <Description("If paper space UCS is orthographic (PUCSORTHOVIEW not equal to 0), this is the name of the UCS that the orthographic UCS is relative to. If blank, UCS is relative to WORLD")> <PropertyName("$PUCSORTHOREF")> <GroupCode("2")> PUCSORTHOREF = 157
        <Description("Orthographic view type of paper space UCS")> <PropertyName("$PUCSORTHOVIEW")> <GroupCode("70")> PUCSORTHOVIEW = 158
        <Description("Point which becomes the new UCS origin after changing paper space UCS to TOP when PUCSBASE is set to WORLD")> <PropertyName("$PUCSORGTOP")> <GroupCode("10")> PUCSORGTOP = 159
        <Description("Point which becomes the new UCS origin after changing paper space UCS to BOTTOM when PUCSBASE is set to WORLD")> <PropertyName("$PUCSORGBOTTOM")> <GroupCode("10")> PUCSORGBOTTOM = 160
        <Description("Point which becomes the new UCS origin after changing paper space UCS to LEFT when PUCSBASE is set to WORLD")> <PropertyName("$PUCSORGLEFT")> <GroupCode("10")> PUCSORGLEFT = 161
        <Description("Point which becomes the new UCS origin after changing paper space UCS to RIGHT when PUCSBASE is set to WORLD")> <PropertyName("$PUCSORGRIGHT")> <GroupCode("10")> PUCSORGRIGHT = 162
        <Description("Point which becomes the new UCS origin after changing paper space UCS to FRONT when PUCSBASE is set to WORLD")> <PropertyName("$PUCSORGFRONT")> <GroupCode("10")> PUCSORGFRONT = 163
        <Description("Point which becomes the new UCS origin after changing paper space UCS to BACK when PUCSBASE is set to WORLD")> <PropertyName("$PUCSORGBACK")> <GroupCode("10")> PUCSORGBACK = 164
        <Description("integer variable intended for use by third-party developers")> <PropertyName("$USERI1")> <GroupCode("70")> USERI1 = 165
        <Description("integer variable intended for use by third-party developers")> <PropertyName("$USERI2")> <GroupCode("70")> USERI2 = 166
        <Description("integer variable intended for use by third-party developers")> <PropertyName("$USERI3")> <GroupCode("70")> USERI3 = 167
        <Description("integer variable intended for use by third-party developers")> <PropertyName("$USERI4")> <GroupCode("70")> USERI4 = 168
        <Description("integer variable intended for use by third-party developers")> <PropertyName("$USERI5")> <GroupCode("70")> USERI5 = 169
        <Description("real variable intended for use by third-party developers")> <PropertyName("$USERR1")> <GroupCode("40")> USERR1 = 170
        <Description("real variable intended for use by third-party developers")> <PropertyName("$USERR2")> <GroupCode("40")> USERR2 = 171
        <Description("real variable intended for use by third-party developers")> <PropertyName("$USERR3")> <GroupCode("40")> USERR3 = 172
        <Description("real variable intended for use by third-party developers")> <PropertyName("$USERR4")> <GroupCode("40")> USERR4 = 173
        <Description("real variable intended for use by third-party developers")> <PropertyName("$USERR5")> <GroupCode("40")> USERR5 = 174
        <Description("1 = Set UCS to WCS during DVIEW/VPOINT, 0 = Don't change UCS")> <PropertyName("$WORLDVIEW")> <GroupCode("70")> WORLDVIEW = 175
        <Description("Controls face rendering style")> <PropertyName("$SHADEDGE")> <GroupCode("70")> SHADEDGE = 176
        <Description("Percent ambient/diffuse light; range 1-100")> <PropertyName("$SHADEDIF")> <GroupCode("70")> SHADEDIF = 177
        <Description("1 for previous release compatibility mode; 0 otherwise")> <PropertyName("$TILEMODE")> <GroupCode("70")> TILEMODE = 178
        <Description("Sets maximum number of viewports to be regenerated")> <PropertyName("$MAXACTVP")> <GroupCode("70")> MAXACTVP = 179
        <Description("Paper space insertion base point")> <PropertyName("$PINSBASE")> <GroupCode("10")> PINSBASE = 180
        <Description("Limits checking in paper space when nonzero")> <PropertyName("$PLIMCHECK")> <GroupCode("70")> PLIMCHECK = 181
        <Description("Minimum X, Y, and Z extents for paper space")> <PropertyName("$PEXTMIN")> <GroupCode("10")> PEXTMIN = 182
        <Description("Maximum X, Y, and Z extents for paper space")> <PropertyName("$PEXTMAX")> <GroupCode("10")> PEXTMAX = 183
        <Description("Minimum X and Y limits in paper space")> <PropertyName("$PLIMMIN")> <GroupCode("10")> PLIMMIN = 184
        <Description("Maximum X and Y limits in paper space")> <PropertyName("$PLIMMAX")> <GroupCode("10")> PLIMMAX = 185
        <Description("Low bit set = Display fractions, feet-and-inches, and surveyor's angles in input format")> <PropertyName("$UNITMODE")> <GroupCode("70")> UNITMODE = 186
        <Description("retain xref-dependent visibility settings")> <PropertyName("$VISRETAIN")> <GroupCode("70")> VISRETAIN = 187
        <Description("Governs the generation of linetype patterns around the vertices of a 2D polyline")> <PropertyName("$PLINEGEN")> <GroupCode("70")> PLINEGEN = 188
        <Description("Controls paper space linetype scaling")> <PropertyName("$PSLTSCALE")> <GroupCode("70")> PSLTSCALE = 189
        <Description("Specifies the maximum depth of the spatial index")> <PropertyName("$TREEDEPTH")> <GroupCode("70")> TREEDEPTH = 190
        <Description("Current multiline style name")> <PropertyName("$CMLSTYLE")> <GroupCode("2")> CMLSTYLE = 191
        <Description("Current multiline justification")> <PropertyName("$CMLJUST")> <GroupCode("70")> CMLJUST = 192
        <Description("Current multiline scale")> <PropertyName("$CMLSCALE")> <GroupCode("40")> CMLSCALE = 193
        <Description("Controls the saving of proxy object images")> <PropertyName("$PROXYGRAPHICS")> <GroupCode("70")> PROXYGRAPHICS = 194
        <Description("Sets drawing units: 0 = English; 1 = Metric")> <PropertyName("$MEASUREMENT")> <GroupCode("70")> MEASUREMENT = 195
        <Description("Lineweight of new objects")> <PropertyName("$CELWEIGHT")> <GroupCode("370")> CELWEIGHT = 196
        <Description("Lineweight endcaps setting for new objects")> <PropertyName("$ENDCAPS")> <GroupCode("280")> ENDCAPS = 197
        <Description("Lineweight joint setting for new objects")> <PropertyName("$JOINSTYLE")> <GroupCode("280")> JOINSTYLE = 198
        <Description("Controls the display of lineweights on the Model or Layout tab")> <PropertyName("$LWDISPLAY")> <GroupCode("290")> LWDISPLAY = 199
        <Description("Default drawing units for AutoCAD DesignCenter blocks")> <PropertyName("$INSUNITS")> <GroupCode("70")> INSUNITS = 200
        <Description("Path for all relative hyperlinks in the drawing. If null, the drawing path is used")> <PropertyName("$HYPERLINKBASE")> <GroupCode("1")> HYPERLINKBASE = 201
        <Description("Styleheet path")> <PropertyName("$STYLESHEET")> <GroupCode("1")> STYLESHEET = 202
        <Description("Controls whether the current drawing can be edited in-place when being referenced by another drawing.")> <PropertyName("$XEDIT")> <GroupCode("290")> XEDIT = 203
        <Description("Plotstyle handle of new objects; if CEPSNTYPE is 3, then this value indicates the handle")> <PropertyName("$CEPSNID")> <GroupCode("390")> CEPSNID = 132
        <Description("Plot style type of new objects")> <PropertyName("$CEPSNTYPE")> <GroupCode("380")> CEPSNTYPE = 204
        <Description("Indicates whether the current drawing is in a Color-Dependent or Named Plot Style mode")> <PropertyName("$PSTYLEMODE")> <GroupCode("290")> PSTYLEMODE = 205
        <Description("Set at creation time, uniquely identifies a particular drawing")> <PropertyName("$FINGERPRINTGUID")> <GroupCode("2")> FINGERPRINTGUID = 206
        <Description("Uniquely identifies a particular version of a drawing. Updated when the drawing is modified")> <PropertyName("$VERSIONGUID")> <GroupCode("2")> VERSIONGUID = 207
        <Description("Controls symbol table naming")> <PropertyName("$EXTNAMES")> <GroupCode("290")> EXTNAMES = 208
        <Description("View scale factor for new viewports")> <PropertyName("$PSVPSCALE")> <GroupCode("40")> PSVPSCALE = 209
        <Description("Loading the OLE source application may improve the plot quality.")> <PropertyName("$OLESTARTUP")> <GroupCode("290")> OLESTARTUP = 210
        <Description("Controls the object sorting methods; accessible from the Options dialog box User Preferences tab")> <PropertyName("$SORTENTS")> <GroupCode("280")> SORTENTS = 211
        <Description("Controls whether layer and spatial indexes are created and saved in drawing files")> <PropertyName("$INDEXCTL")> <GroupCode("280")> INDEXCTL = 212
        <Description("Specifies HIDETEXT system variable")> <PropertyName("$HIDETEXT")> <GroupCode("280")> HIDETEXT = 213
        <Description("Determines whether xref clipping boundaries are visible or plotted in the current drawing.")> <PropertyName("$XCLIPFRAME")> XCLIPFRAME = 214
        <Description("Specifies a gap to be displayed where an object is hidden by another object; the value is specified as a percent of one unit and is independent of the zoom level.")> <PropertyName("$HALOGAP")> <GroupCode("280")> HALOGAP = 215
        <Description("Specifies the color of obscured lines. An obscured line is a hidden line made visible by changing its color and linetype and is visible only when the HIDE or SHADEMODE command is used.")> <PropertyName("$OBSCOLOR")> <GroupCode("70")> OBSCOLOR = 216
        <Description("Specifies the linetype of obscured lines.")> <PropertyName("$OBSLTYPE")> <GroupCode("280")> OBSLTYPE = 217
        <Description("Specifies the display of intersection polylines")> <PropertyName("$INTERSECTIONDISPLAY")> <GroupCode("280")> INTERSECTIONDISPLAY = 218
        <Description("Specifies the entity color of intersection polylines")> <PropertyName("$INTERSECTIONCOLOR")> <GroupCode("70")> INTERSECTIONCOLOR = 219
        <Description("Controls the associativity of dimension objects")> <PropertyName("$DIMASSOC")> <GroupCode("280")> DIMASSOC = 220
        <Description("Assigns a project name to the current drawing")> <PropertyName("$PROJECTNAME")> <GroupCode("1")> PROJECTNAME = 221
        <Description("The value changes to 1 (to display cameras) when you use the CAMERA command.")> <PropertyName("$CAMERADISPLAY")> <GroupCode("290")> CAMERADISPLAY = 222
        <Description("Stores the length of the lens in millimeters used in perspective viewing.")> <PropertyName("$LENSLENGTH")> <GroupCode("40")> LENSLENGTH = 223
        <Description("Specifies the default height for new camera objects.")> <PropertyName("$CAMERAHEIGHT")> <GroupCode("40")> CAMERAHEIGHT = 224
        <Description("Specifies the number of steps taken per second when you are in walk or fly mode.")> <PropertyName("$STEPSPERSEC")> <GroupCode("40")> STEPSPERSEC = 225
        <Description("Specifies the size of each step when in walk or fly mode, in drawing units.?")> <PropertyName("$STEPSIZE")> <GroupCode("40")> STEPSIZE = 226
        <Description("Controls the precision of 3D DWF or 3D DWFx publishing.")> <PropertyName("$3DDWFPREC")> <GroupCode("40")> THREEDDWFPREC = 227
        <Description("Controls the default width for a swept solid object created with the POLYSOLID command.")> <PropertyName("$PSOLWIDTH")> <GroupCode("40")> PSOLWIDTH = 228
        <Description("Controls the default heighta swept solid object created with the POLYSOLID command.?")> <PropertyName("$PSOLHEIGHT")> <GroupCode("40")> PSOLHEIGHT = 229
        <Description("Sets the draft angle through the first cross section in a loft operation.")> <PropertyName("$LOFTANG1")> <GroupCode("40")> LOFTANG1 = 230
        <Description("Sets the draft angle through the last cross section in a loft operation.")> <PropertyName("$LOFTANG2")> <GroupCode("40")> LOFTANG2 = 231
        <Description("Sets the magnitude of the draft angle through the first cross section in a loft operation.")> <PropertyName("$LOFTMAG1")> <GroupCode("40")> LOFTMAG1 = 232
        <Description("Sets the magnitude of the draft angle through the first cross section in a loft operation.")> <PropertyName("$LOFTMAG2")> <GroupCode("40")> LOFTMAG2 = 233
        <Description("Controls the shape of lofted solids and surfaces.")> <PropertyName("$LOFTPARAM")> <GroupCode("70")> LOFTPARAM = 234
        <Description("Controls the normals of a lofted object where it passes through cross sections.")> <PropertyName("$LOFTNORMALS")> <GroupCode("280")> LOFTNORMALS = 235
        <Description("Specifies the latitude of the geographic marker.")> <PropertyName("$LATITUDE")> <GroupCode("40")> LATITUDE = 236
        <Description("Specifies the longiture of the geographic marker.")> <PropertyName("$LONGITUDE")> <GroupCode("40")> LONGITUDE = 237
        <Description("Specifies the angle between the Y axis of WCS and the grid north.")> <PropertyName("$NORTHDIRECTION")> <GroupCode("40")> NORTHDIRECTION = 238
        <Description("Sets the time zone for the sun in the drawing.?")> <PropertyName("$TIMEZONE")> <GroupCode("70")> TIMEZONE = 239
        <Description("Turns on and off the display of light glyphs.")> <PropertyName("$LIGHTGLYPHDISPLAY")> <GroupCode("280")> LIGHTGLYPHDISPLAY = 240
        <Description("Control lighting in tile mode")> <PropertyName("$TILEMODELIGHTSYNCH")> <GroupCode("280")> TILEMODELIGHTSYNCH = 241
        <Description("Sets the material of new objects.")> <PropertyName("$CMATERIAL")> <GroupCode("347")> CMATERIAL = 242
        <Description("Controls whether new composite solids retain a history of their original components.")> <PropertyName("$SOLIDHIST")> <GroupCode("280")> SOLIDHIST = 243
        <Description("Controls the Show History property for solids in a drawing.")> <PropertyName("$SHOWHIST")> <GroupCode("280")> SHOWHIST = 244
        <Description("Determines whether DWF or DWFx underlay frames are visible or plotted in the current drawing.")> <PropertyName("$DWFFRAME")> <GroupCode("280")> DWFFRAME = 245
        <Description("Determines whether DGN underlay frames are visible or plotted in the current drawing.")> <PropertyName("$DGNFRAME")> DGNFRAME = 246
        <Description("Scale for the real world")> <PropertyName("$REALWORLDSCALE")> <GroupCode("290")> REALWORLDSCALE = 247
        <Description("Sets the color for interference objects.")> <PropertyName("$INTERFERECOLOR")> <GroupCode("62")> INTERFERECOLOR = 248
        <Description("Sets the visual style for interference objects.")> <PropertyName("$INTERFEREOBJVS")> <GroupCode("345")> INTERFEREOBJVS = 249
        <Description("Specifies the visual style for the viewport during interference checking.")> <PropertyName("$INTERFEREVPVS")> <GroupCode("346")> INTERFEREVPVS = 250
        <Description("Shadow mode for a 3D object")> <PropertyName("$CSHADOW")> <GroupCode("280")> CSHADOW = 251
        <Description("Location of the ground shadow plane. This is a Z axis ordinate.")> <PropertyName("$SHADOWPLANELOCATION")> <GroupCode("40")> SHADOWPLANELOCATION = 252
        <Description("Controls how the UCS is displayed in the current image")> <PropertyName("*UCSMODE")> <GroupCode("-1")> UCSMODE = -1
        <Description("Controls the size of the UCS as displayed in the current image. (Screen Fraction)")> <PropertyName("*UCSSIZE")> <GroupCode("-2")> UCSSIZE = (-1 * 2)
        <Description("Controls the color of the UCS as displayed in the current image")> <PropertyName("*UCSCOLOR")> <GroupCode("-3")> UCSCOLOR = (-1 * 3)
        <Description("Stores the current default lineweight")> <PropertyName("*LWDEFAULT")> <GroupCode("-4")> LWDEFAULT = (-1 * 4)
        <Description("Stores the current line weight scale")> <PropertyName("*LWSCALE")> <GroupCode("-5")> LWSCALE = (-1 * 5)
    End Enum 'dxxHeaderVars
    Public Enum dxxLayerProperties
        <DisplayName("Name")> <PropertyName("Name")> Name = 6
        <DisplayName("Status")> <PropertyName("Bit Code")> Status = 7
        <DisplayName("Color")> <PropertyName("Color")> Color = 8
        <DisplayName("Linetype")> <PropertyName("Linetype")> Linetype = 9
        <DisplayName("Plot Flag")> <PropertyName("Plot Flag")> PlotFlag = 10
        <DisplayName("Line Weight")> <PropertyName("Line Weight")> LineWeight = 11
        <DisplayName("Plot Style Pointer")> <PropertyName("Plot Style Pointer")> PlotStyleHandle = 12
        <DisplayName("Material Pointer")> <PropertyName("Material Pointer")> MaterialHandle = 13
        <DisplayName("Visible")> <PropertyName("Color")> Visible = 103  'negative colors control visibility
        <DisplayName("Transparency")> <PropertyName("*Transparency")> Transparency = 1000
        <DisplayName("Description")> <PropertyName("*Description")> Description = 1001
        <DisplayName("Frozen")> <PropertyName("Bit Code")> Frozen = -1
        <DisplayName("Frozen In Viewport")> <PropertyName("Bit Code")> FrozenViewport = (-1 * 2)
        <DisplayName("Locked")> <PropertyName("Bit Code")> Locked = (-1 * 4)
        <DisplayName("XRef Dependant")> <PropertyName("Bit Code")> XRefDependant = (-1 * 16)
        <DisplayName("XRef Resolved")> <PropertyName("Bit Code")> XRefResolved = (-1 * 32)
        PropertyName
    End Enum 'dxxLayerProperties
    Public Enum dxxLinetypeProperties
        <DisplayName("Name")> <PropertyName("Name")> Name = 5
        <DisplayName("Status")> <PropertyName("Bit Code")> Status = 6
        <DisplayName("Description")> <PropertyName("Description")> Description = 7
        <DisplayName("Alignment Code")> <PropertyName("Alignment Code")> AlignmentCode = 8
        <DisplayName("Element Count")> <PropertyName("Element Count")> Elements = 9
        <DisplayName("Pattern Length")> <PropertyName("Pattern Length")> PatternLength = 10
        <DisplayName("XRef Dependant")> <PropertyName("Bit Code")> XRefDependant = (-1 * 16)
        <DisplayName("XRef Resolved")> <PropertyName("Bit Code")> XRefResolved = (-1 * 32)
    End Enum 'dxxLinetypeProperties
    Public Enum dxxStyleProperties
        <DisplayName("Name")> <PropertyName("Name")> NAME = 5
        <DisplayName("Text Height")> <PropertyName("Text Height")> TEXTHT = 7
        <DisplayName("Width Factor")> <PropertyName("Width Factor")> WIDTHFACTOR = 8
        <DisplayName("Oblique Angle")> <PropertyName("Oblique Angle")> OBLIQUE = 9
        <DisplayName("Generation Flag")> <PropertyName("Generation Flag")> GENFLAG = 10
        <DisplayName("Last Height Used")> <PropertyName("Last Height")> LASTHT = 11
        <DisplayName("Font Name")> <PropertyName("Font")> FONTNAME = 12
        <DisplayName("Big Font File")> <PropertyName("Big Font File")> BIGFONT = 13
        <DisplayName("Font Style")> <PropertyName("*FontStyle")> FONTSTYLE = 101
        <DisplayName("Font Style Type")> <PropertyName("*FontStyleType")> FONTSTYLETYPE = 102
        <DisplayName("Shape Flag")> <PropertyName("*IsShape")> SHAPEFLAG = 103
        <DisplayName("Line Spacing Factor")> <PropertyName("*LineSpacingFactor")> LINESPACING = 104
        <DisplayName("Line Spacing Style")> <PropertyName("*LineSpacingStyle")> LINESPACINGSTYLE = 105
        <DisplayName("Font Index")> <PropertyName("*FontIndex")> FONTINDEX = 106
        <DisplayName("Backwards")> <PropertyName("*Backwards")> BACKWARDS = 107
        <DisplayName("Upsidedown")> <PropertyName("*Upsidedown")> UPSIDEDOWN = 108
        <DisplayName("Shape Style Flag")> <PropertyName("*ShapeStyleFlag")> SHAPESTYLEFLAG = 109
        <DisplayName("Vertical Text")> <PropertyName("*Vertical")> VERTICAL = 110
        <DisplayName("XRef Dependant")> <PropertyName("*XRefDependant")> XREFDEPENANT = 111
        <DisplayName("XRef Resolved")> <PropertyName("*XRefResolved")> XREFRESOLVED = 112
        <DisplayName("Is Referenced")> <PropertyName("*IsReferenced")> ISREFERENCED = 113
    End Enum 'dxxStyleProperties.
    Public Enum dxxScreenProperties
        <Description("Suppressed")> Suppressed = 1
        <Description("BoundingRectangles")> BoundingRectangles = 2
        <Description("OCSs")> OCSs = 3
        <Description("ExtentPts")> ExtentPts = 4
        <Description("ExtentRectangle")> ExtentRectangle = 5
        <Description("TextBoxes")> TextBoxes = 6
        <Description("EntitySymbolColor")> EntitySymbolColor = 7
        <Description("TextColor")> TextColor = 8
        <Description("AxisColor")> AxisColor = 9
        <Description("RectangleColor")> RectangleColor = 10
        <Description("ExtentRectangleColor")> ExtentRectangleColor = 11
        <Description("PointColor")> PointColor = 12
        <Description("OCSSize")> OCSSize = 13
        <Description("TextSize")> TextSize = 14
        <Description("PointSize")> PointSize = 15
        <Description("CircleColor")> CircleColor = 16
        <Description("PointerColor")> PointerColor = 17
        <Description("LineColor")> LineColor = 18
        <Description("LineType")> LineType = 19
        <Description("LTScale")> LTScale = 20
    End Enum 'dxxScreenProperties
    Public Enum dxxDimStyleProperties
        <Description("The name of the DimStyle")>
        <PropertyName("Name")> DIMNAME = 5
        <PropertyName("Bit Code")> DIMSTATUS = 6
        <Description("Specifies a text prefix or suffix (or both) to the dimension measurement.")>
        DIMPOST = 7
        <Description("Specifies a text prefix or suffix (or both) to the alternate dimension measurement for all types of dimensions except angular.")>
        DIMAPOST = 8
        <Description("Sets the overall scale factor applied to dimensioning variables that specify sizes, distances, or offsets.")>
        DIMSCALE = 9
        <Description("Controls the size of dimension line and leader line arrowheads. Also controls the size of hook lines.")>
        DIMASZ = 10
        <Description("Specifies how far extension lines are offset from origin points.")>
        DIMEXO = 11
        <Description("Controls the spacing of the dimension lines in baseline dimensions.")>
        DIMDLI = 12
        <Description("Specifies how far to extend the extension line beyond the dimension line.")>
        DIMEXE = 13
        <Description("Rounds all dimensioning distances to the specified value.")>
        DIMRND = 14
        <Description("Sets the distance the dimension line extends beyond the extension line when oblique strokes are drawn instead of arrowheads.")>
        DIMDLE = 15
        <Description("Sets the maximum (or upper) tolerance limit for dimension text when DIMTOL or DIMLIM is on.")>
        DIMTP = 16
        <Description("Sets the minimum (or lower) tolerance limit for dimension text when DIMTOL or DIMLIM is on.")>
        DIMTM = 17
        <Description("Sets the total length of the extension lines starting from the dimension line toward the dimension origin.")>
        DIMFXL = 18
        <Description("Determines the angle of the transverse segment of the dimension line in a jogged radius dimension.")>
        DIMJOGANG = 19
        <Description("Controls the background of dimension text. (0=Off,1=On)")>
        DIMTFILL = 20
        <Description("Sets the color for the text background in dimensions.")>
        DIMTFILLCLR = 21
        <Description("Appends tolerances to dimension text. (0=Off,1=On)")>
        DIMTOL = 22
        <Description("Generates dimension limits as the default text. (0=Off,1=On)")>
        DIMLIM = 23
        <Description("Controls the position of dimension text inside the extension lines for all dimension types except Ordinate. (1=On Draws text horizontally, 0=Off Aligns text with the dimension line)")>
        DIMTIH = 24
        <Description("Controls the position of dimension text outside the extension lines for all dimension types except Ordinate. (1=On Draws text horizontally, 0=Off Aligns text with the dimension line)")>
        DIMTOH = 25
        <Description("Suppresses display of the first extension line. (0=Off,1=On)")>
        DIMSE1 = 26
        <Description("Suppresses display of the second extension line. (0=Off,1=On)")>
        DIMSE2 = 27
        <Description("Controls the vertical position of text in relation to the dimension line.(0=Centered, 1=above dimension line, 2=farthest from def points, 3=perJIS standards, 4=below dimension line")>
        DIMTAD = 28
        <Description("Controls the suppression of zeros in the primary unit value. (0=Suppresses zero feet and precisely zero inches, 1=Includes zero feet and precisely zero inches,
        2=Includes zero feet and suppresses zero inches, 3=Includes zero inches and suppresses zero feet, 4=Suppresses leading zeros in decimal dimensions, 8=Suppresses trailing zeros in decimal dimensions ,
        12=Suppresses both leading and trailing zeros)")>
        DIMZIN = 29
        <Description("Controls the suppression of zeros in the angular unit value.0=Suppresses zero feet and precisely zero inches, 1=Includes zero feet and precisely zero inches,
        2=Includes zero feet and suppresses zero inches, 3=Includes zero inches and suppresses zero feet, 4=Suppresses leading zeros in decimal dimensions, 8=Suppresses trailing zeros in decimal dimensions ,
        12=Suppresses both leading and trailing zeros")>
        DIMAZIN = 30
        <Description("Controls display of the arc symbol in an arc length dimension. 0=before dimension text,1=above dimension text")>
        DIMARCSYM = 31
        <Description("Specifies the height of dimension text, unless the current text style has a fixed height.")>
        DIMTXT = 32
        <Description("Controls drawing of circle or arc center marks and centerlines by the DIMCENTER, DIMDIAMETER, and DIMRADIUS commands.")>
        DIMCEN = 33
        <Description("Specifies the size of oblique strokes drawn instead of arrowheads for linear, radius, and diameter dimensioning.")>
        DIMTSZ = 34
        <Description("Controls the multiplier for alternate units.")>
        DIMALTF = 35
        <Description("Sets a scale factor for linear dimension measurements.")>
        DIMLFAC = 36
        <Description("Controls the vertical position of dimension text above or below the dimension line when DIMTAD is Off")>
        DIMTVP = 37
        <Description("Specifies a scale factor for the text height of fractions and tolerance values relative to the dimension text height, as set by DIMTXT.")>
        DIMTFAC = 38
        <Description("Sets the distance around the dimension text when the dimension line breaks to accommodate dimension text. Negative values draws a rectangle around the dim text")>
        DIMGAP = 39
        <Description("Rounds off the alternate dimension units.")>
        DIMALTRND = 40
        <Description("Controls the display of alternate units in dimensions. (0=Off,1=On)")>
        DIMALT = 41
        <Description("Controls the number of decimal places in alternate units.")>
        DIMALTD = 42
        <Description("Controls whether a dimension line is drawn between the extension lines even when the text is placed outside. (0=Off,1=On)")>
        DIMTOFL = 43
        <Description("Controls the display of dimension line arrowhead blocks. (0=Off,1=On)")>
        DIMSAH = 44
        <Description("Draws text between extension lines.(0=Off between if the text fits,1=On force text between the extension lines)")>
        DIMTIX = 45
        <Description("Suppresses arrowheads if not enough space is available inside the extension lines. Ignored if DIMTIX is off")>
        DIMSOXD = 46
        <Description("Assigns colors to dimension lines, arrowheads, and dimension leader lines.")>
        DIMCLRD = 47
        <Description("Assigns colors to extension lines, center marks, and centerlines.")>
        DIMCLRE = 48
        <Description("Assigns colors to dimension text.")>
        DIMCLRT = 49
        <Description("Controls the number of precision places displayed in angular dimensions.")>
        DIMADEC = 50
        <Description("Obsolete. Has no effect in AutoCAD 2000 except to preserve the integrity of pre-AutoCAD 2000 scripts and AutoLISP routines. In AutoCAD 2000, DIMUNIT is replaced by DIMLUNIT and DIMFRAC.")>
        DIMUNIT = 51
        <Description("Sets the number of decimal places displayed for the primary units of a dimension.")>
        DIMDEC = 52
        <Description("Sets the number of decimal places to display in tolerance values for the primary units in a dimension.")>
        DIMTDEC = 53
        <Description("Sets the number of decimal places to display for alternate units of all dimension substyles except Angular.")>
        DIMALTU = 54
        <Description("Sets the number of decimal places for the tolerance values in the alternate units of a dimension.")>
        DIMALTTD = 55
        <Description("Sets units for angular dimension types.")>
        DIMAUNIT = 56
        <Description("Sets the units format for angular dimensions. (0=Decimal degrees,1=Degrees/minutes/seconds,2=Gradians,3=Radians")>
        DIMFRAC = 57
        <Description("Sets units for all dimension types except Angular.")>
        DIMLUNIT = 58
        <Description("Specifies a single-character decimal separator to use when creating dimensions whose unit format is decimal.")>
        DIMDSEP = 59
        <Description("Sets dimension text movement rules. (0=dimension line moves with text,1=adds a leader when the dim text is moved,2=allows text to be freely moved with no leader)")>
        DIMTMOVE = 60
        <Description("Controls the horizontal positioning of dimension text. (0=Positions the text above the dimension line and center-justifies it between the extension lines,
1=Positions the text next to the first extension line,2=Positions the text next to the second extension line,3=	Positions the text above and aligned with the first extension line,
4=Positions the text above and aligned with the second extension line")>
        DIMJUST = 61
        <Description("Controls suppression of the first dimension line and arrowhead. (0=Off,1=On)")>
        DIMSD1 = 62
        <Description("Controls suppression of the second dimension line and arrowhead. (0=Off,1=On)")>
        DIMSD2 = 63
        <Description("Sets the vertical justification for tolerance values relative to the nominal dimension text. Ignored if DIMTOL is off (0=Botton,1=Middle,2=Top")>
        DIMTOLJ = 64
        <Description("Controls the suppression of zeros in the tolerance values. (0=Suppresses zero feet and precisely zero inches, 1=Includes zero feet and precisely zero inches,
        2=Includes zero feet and suppresses zero inches, 3=Includes zero inches and suppresses zero feet, 4=Suppresses leading zeros in decimal dimensions, 8=Suppresses trailing zeros in decimal dimensions ,
        12=Suppresses both leading and trailing zeros)")>
        DIMTZIN = 65
        <Description("Controls the suppression of zeros in the alternate unit values. (0=Suppresses zero feet and precisely zero inches, 1=Includes zero feet and precisely zero inches,
        2=Includes zero feet and suppresses zero inches, 3=Includes zero inches and suppresses zero feet, 4=Suppresses leading zeros in decimal dimensions, 8=Suppresses trailing zeros in decimal dimensions ,
        12=Suppresses both leading and trailing zeros)")>
        DIMALTZ = 66
        <Description("Controls the suppression of zeros in the alternate unit tolerance values. (0=Suppresses zero feet and precisely zero inches, 1=Includes zero feet and precisely zero inches,
        2=Includes zero feet and suppresses zero inches, 3=Includes zero inches and suppresses zero feet, 4=Suppresses leading zeros in decimal dimensions, 8=Suppresses trailing zeros in decimal dimensions ,
        12=Suppresses both leading and trailing zeros)")>
        DIMALTTZ = 67
        <Description("Obsolete, use DIMATFIT and DIMTMOVE instead.DIMFIT is replaced by DIMATFIT and DIMTMOVE. However, if DIMFIT is set to 0 - 3, then DIMATFIT is also set to 0 - 3 and DIMTMOVE is set to 0.
        If DIMFIT is set to 4 or 5, then DIMATFIT is set to 3 and DIMTMOVE is set to 1 or 2 respectively.")>
        DIMFIT = 68
        <Description("Controls options for user-positioned text. (0=Off,1=On)")>
        DIMUPT = 69
        <Description("Determines how dimension text and arrows are arranged when space is not sufficient to place both within the extension lines. (0=Places both text and arrows outside extension lines,
1=Moves arrows first, then text,2=Moves text first then arrows,3=Moves either text or arrows whichever fits best")>
        DIMATFIT = 70
        <Description("Controls whether extension lines are set to a fixed length.When DIMFXLON is on, extension lines are set to the length specified by DIMFXL. (0=Off,1=On)")>
        DIMFXLON = 71
        <Description("Stores the handle of text style of the dimension.")>
        DIMTXSTY = 72
        <Description("Stores the handle of the arrowhead block for leaders.")>
        DIMLDRBLK = 73
        <Description("Stores the handle of the arrowhead block displayed at the ends of dimension lines or leader lines.")>
        DIMBLK = 74
        <Description("Stores the handle of the arrowhead block for the first end of the dimension line when DIMSAH is on.")>
        DIMBLK1 = 75
        <Description("Stores the handle of the arrowhead block for the second end of the dimension line when DIMSAH is on.")>
        DIMBLK2 = 76
        <Description("Stores the handle of the linetype of the dimension line.")>
        DIMLTYPE = 77
        <Description("Stores the handle of the linetype of the first extension line.")>
        DIMLTEX1 = 78
        <Description("Stores the handle of the linetype of the second extension line.")>
        DIMLTEX2 = 79
        <Description("Assigns lineweight to dimension lines.")>
        DIMLWD = 80
        <Description("Assigns lineweight to extension lines.")>
        DIMLWE = 81
        <Description("Specifies the reading direction of the dimension text.")>
        DIMTXTDIRECTION = 82
        <Description("Stores the dxxZeroSuppression zero suppression enum value based on current settings")>
        <PropertyName("*ZeroSuppression")> DIMZEROSUPPRESSION = 90
        <Description("Stores the dxxZeroSuppressionsArchitectural  zero suppression enum value based on current settings")>
        <PropertyName("*ZeroSuppressionTol")> DIMTOLZEROSUPPRESSION = 91
        <Description("Stores the dxxZeroSuppressionsArchitectural  zero suppression enum value based on current settings")>
        <PropertyName("*ZeroSuppressionArch")> DIMZEROSUPPRESSION_ARCH = 92
        <Description("Stores the current prefix for linear dimesions.")>
        <PropertyName("*Prefix")> DIMPREFIX = 93
        <Description("Stores the current suffix for linear dimesions.")>
        <PropertyName("*Suffix")> DIMSUFFIX = 94
        <Description("Stores the current prefix for angular dimesions.")>
        <PropertyName("*APrefix")> DIMAPREFIX = 95
        <Description("Stores the current suffix for angular dimesions.")>
        <PropertyName("*ASuffix")> DIMASUFFIX = 96
        <Description("Stores the current text angle for new dimensions in the stytle")>
        <PropertyName("*Text Angle")> DIMTANGLE = 97
        <Description("Stores the name of text style of the dimension.")>
        <PropertyName("*DIMTXSTY_NAME")> DIMTXSTY_NAME = DIMTXSTY + 100
        <Description("Stores the name of the arrowhead block for leaders.")>
        <PropertyName("*DIMLDRBLK_NAME")> DIMLDRBLK_NAME = DIMLDRBLK + 100
        <Description("Stores the name of the arrowhead block displayed at the ends of dimension lines or leader lines.")>
        <PropertyName("*DIMBLK_NAME")> DIMBLK_NAME = DIMBLK + 100
        <Description("Stores the name of the arrowhead block for the first end of the dimension line when DIMSAH is on.")>
        <PropertyName("*DIMBLK1_NAME")> DIMBLK1_NAME = DIMBLK1 + 100
        <Description("Stores the name of the arrowhead block for the send end of the dimension line when DIMSAH is on.")>
        <PropertyName("*DIMBLK2_NAME")> DIMBLK2_NAME = DIMBLK2 + 100
        <Description("Stores the name of the linetype of the dimension line.")>
        <PropertyName("*DIMLTYPE_NAME")> DIMLTYPE_NAME = DIMLTYPE + 100
        <Description("Stores the handle of the linetype of the first extension line.")>
        <PropertyName("*DIMLTEX1_NAME")> DIMLTEX1_NAME = DIMLTEX1 + 100
        <Description("Stores the handle of the linetype of the second extension line.")>
        <PropertyName("*DIMLTEX2_NAME")> DIMLTEX2_NAME = DIMLTEX2 + 100
        <DisplayName("XRef Dependant")> <PropertyName("Bit Code")> XREFDEPENDANT = (-1 * 16)
        <DisplayName("XRef Resolved")> <PropertyName("Bit Code")> XREFRESOLVED = (-1 * 32)
        <DisplayName("Is Referenced")> <PropertyName("Bit Code")> ISREFERENCED = (-1 * 64)
    End Enum 'dxxDimStyleProperties
    Public Enum dxxPropertyTypes
        <Description("Undefined")> Undefined = -1
        <Description("Variant")> dxf_Variant = 0
        <Description("Integer")> dxf_Integer = 1
        <Description("Switch")> Switch = 2
        <Description("Long")> dxf_Long = 3
        <Description("Color")> Color = 4
        <Description("BitCode")> BitCode = 5
        <Description("Single")> dxf_Single = 6
        <Description("Double")> dxf_Double = 7
        <Description("Angle")> Angle = 8
        <Description("Boolean")> dxf_Boolean = 9
        <Description("String")> dxf_String = 11
        <Description("Handle")> Handle = 12
        <Description("Pointer")> Pointer = 13
        <Description("Class Marker")> ClassMarker = 14
        <Description("2D Vector")> Vector2D = 15 'vectors must be this value or greater
        <Description("3D Vector")> Vector3D = 16
        <Description("Direction")> VectorDir = 17
    End Enum 'dxxPropertyTypes
    Public Enum dxxBackgroundFillModes
        Off = 0
        BackFillColor = 1
        WindowFillColor = 2
    End Enum 'dxxBackgroundFillModes
    Public Enum dxxShadowModes
        CastReceives = 0
        Cast = 1
        Receives = 2
        Ignores = 3
    End Enum 'dxxShadowModes
    Public Enum dxxEntityStates
        Steady = 0
        GeneratingPath = 1
        Transforming = 2
        Cloning = 3
    End Enum 'dxxEntityStates
    Public Enum dxxRenderModes
        <Description("2D Optimized")> TwoDOptimized = 0
        <Description("Wire Frame")> WireFrame = 1
        <Description("Hidden Line")> HiddenLine = 2
        <Description("Flat Shaded")> FlatShaded = 3
        <Description("Gourard Shaded")> GourardShaded = 4
        <Description("Flat Shaded Wireframe")> FlatShadedWireframe = 5
        <Description("Gourard Shaded Wireframe")> GourardShadedWireframe = 6

    End Enum
    Public Enum dxxOrthoGraphicTypes
        <Description("Non-Orthographic")> NonOrthographic = 0
        <Description("Top")> Top = 1
        <Description("Bottom")> Bottom = 2
        <Description("Front")> Front = 3
        <Description("Back")> Back = 4
        <Description("Left")> Left = 5
        <Description("Right")> Right = 6
    End Enum

    Public Enum dxxShadePlotModes
        <Description("As Displayed")> AsDisplayed = 0
        <Description("Wire Frame")> WireFrame = 1
        <Description("Hidden")> Hidden = 2
        <Description("Rendered")> Rendered = 3
    End Enum

    Public Enum dxxShadePlotResolutions
        <Description("Draft")> Draft = 0
        <Description("Preview")> Preview = 1
        <Description("Normal")> Normal = 2
        <Description("Presentation")> Presentation = 3
        <Description("Maximum")> Maximum = 4
        <Description("Custom")> Custom = 6
    End Enum
    Public Enum dxxLightingTypes
        <Description("One Distant")> OneDistant = 0
        <Description("Two Distant")> TwoDistant = 1
    End Enum

    Public Enum dxxSnapStyles
        <Description("Rectangular")> Rectangular = 0
        <Description("Isometrict")> Isometric = 1
    End Enum
    Public Enum dxxPaperUnits
        <Description("Inches")> Inches = 0
        <Description("Millimeter")> Millimeter = 1
        <Description("Pixels")> Pixels = 2
    End Enum
    Public Enum dxxPaperRotations
        <Description("No rotation")> Zero = 0
        <Description("90 degrees counterclockwise")> NinetyCounterClockwise = 1
        <Description("Upside-down")> UpsideDown = 2
        <Description("90 degrees clockwise")> NinetyClockwise = 3
    End Enum
    Public Enum dxxPaperPlotTypes
        <Description("Last screen display")> LastDisplay = 0
        <Description("Drawing extents")> Extents = 1
        <Description("Drawing limits")> Limits = 2
        <Description("View specified by code 6")> ByView = 3
        <Description("Window specified by codes 48, 49, 140, and 141")> ByWindow = 4
        <Description("BY Layout")> ByLayout = 5
    End Enum
    Public Enum dxxDrawingDomains
        Screen = -1
        Model = 0
        Paper = 1
    End Enum 'dxxDrawingDomains

    Public Enum dxxViewModes
        <Description("Off")> Off = 0
        <Description("Perspective View")> Perspective = 1
        <Description("Front Clipping Plane")> FrontClipping = 2
        <Description("Rear Clipping Plane")> RearClipping = 4
        <Description("UCS Follow Mode")> UCSFollow = 8
        <Description("Front Clipping Not At Camera")> NotAtCamera = 16

    End Enum

    Public Enum dxxVectorProperties
        Undefined = 0
        X = 1 'As Double
        Y = 2 'As Double
        Z = 4 'As Double
        Radius = 8 'As Double
        Inverted = 16 'As Boolean
        StartWidth = 32 'As Double
        EndWidth = 64 'As Double
        Rotation = 128 'As Double
        Tag = 256 'As String
        Flag = 512 'As String
        Value = 1024 'As Variant
        Mark = 2048 'As Boolean
        LayerName = 4096 'As String
        Color = 8192 'As Long
        Linetype = 16384 'As String
        LTScale = 32768 'As Double
        Suppressed = 65536 'As Boolean
        Row = 131072 'As Integer
        Col = 262144 'As integer
        Coordinates = X + Y + Z
    End Enum 'dxxVectorProperties
    Public Enum dxxShapeCommands
        Undefined = -1
        EndShape = 0
        PenDown = 1
        PenUp = 2
        DivideBy = 3
        MultiplyBy = 4
        PushToStack = 5
        PopFromStack = 6
        SubShape = 7
        XYDisplacement = 8
        MultiXYDisplacement = 9
        OctantArc = 10
        FractionalArc = 11
        BulgeArc = 12
        MultiBulgeArc = 13
        VerticalOnly = 14
        VectorLengthAndDirection = 15
    End Enum 'dxxShapeCommands
    Public Enum dxxPrinterMarginStyles
        prntMg_Default = 0
        prntMg_Current = 1
        prntMg_Minimum = 2
    End Enum 'dxxPrinterMarginStyles
    Public Enum dxxPrinterProperties
        DM_COLLATE = &H8000
        DM_COLOR = &H800&
        DM_COPIES = &H100&
        DM_COPY = 2
        DM_DEFAULTSOURCE = &H200&
        DM_DUPLEX = &H1000&
        DM_MODIFY = 8
        DM_ORIENTATION = &H1&
        DM_PAPERLENGTH = &H4&
        DM_PAPERSIZE = &H2&
        DM_PAPERWIDTH = &H8&
        DM_PRINTQUALITY = &H400&
        DM_SCALE = &H10&
        DM_IN_BUFFER = DM_MODIFY
        DM_OUT_BUFFER = DM_COPY
    End Enum 'dxxPrinterProperties
    Public Enum dxxPlotLayoutFlags
        pltF_UNdefined = -1
        pltF_ViewportBorders = 1
        pltF_ShowPlotStyles = 2
        pltF_PlotCentered = 4
        pltF_PlotHidden = 8
        pltF_UseStandardScale = 16
        pltF_PlotPlotStyle = 32
        pltF_ScaleLineWidths = 64
        pltF_PrintLineweights = 128
        pltF_PDrawViewportsFirst = 512
        pltF_PModelType = 1024
        pltF_PUpdatePaper = 2048
        pltF_PZoomToPaperOnUpdate = 4096
        pltF_PIntitializing = 8182
        pltF_PPrevPlotInit = 16348
    End Enum 'dxxPlotLayoutFlags
    Public Enum dxxPlotPaperUnits
        pltU_Inches = 1
        pltU_Millimeters = 2
        pltU_Pixels = 3
    End Enum 'dxxPlotPaperUnits
    Public Enum dxxPlotPaperRotations
        pltR_Undefined = -1
        pltR_None = 0
        pltR_90CCW = 1
        pltR_UpsideDown = 2
        pltR_90CW = 3
    End Enum 'dxxPlotPaperRotations
    Public Enum dxxPlotAreas
        pltA_Undefined = -1
        pltA_LastDisplay = 0
        pltA_Extents = 1
        pltA_Limits = 2
        pltA_ByViewName = 3
        pltA_ByWindow = 4
        pltA_ByLayoutInfo = 5
    End Enum 'dxxPlotAreas
    Public Enum dxxPlotShadeModes
        pltShade_AsDisplayed = 0
        pltShade_Wireframe = 1
        pltShade_Hidden = 2
        pltShade_Rendered = 3
    End Enum 'dxxPlotShadeModes
    Public Enum dxxPlotShadeResolutions
        Draft = 0
        Preview = 1
        Normal = 2
        Presentation = 3
        Maximimum = 4
        Custom = 5
    End Enum 'dxxPlotShadeResolutions
    Public Enum dxxPlotStandardScales
        pltS_Undefined = -1
        pltS_ScaledToFit = 0
        pltS_1_128_f = 1
        pltS_1_64_f = 2
        pltS_1_32_f = 3
        pltS_1_16_f = 4
        pltS_3_32_f = 5
        pltS_1_8_f = 6
        pltS_3_16_f = 7
        pltS_1_4_f = 8
        pltS_3_8_f = 9
        pltS_1_2_f = 10
        pltS_3_4_f = 11
        pltS_1_f = 12
        pltS_3_f = 13
        pltS_6_f = 14
        pltS_12_f = 15
        pltS_1to1 = 16
        pltS_1to2 = 17
        pltS_1to4 = 18
        pltS_1to8 = 19
        pltS_1to10 = 20
        pltS_1to16 = 21
        pltS_1to20 = 22
        pltS_1to30 = 23
        pltS_1to40 = 24
        pltS_1to50 = 25
        pltS_1to100 = 26
        pltS_2to1 = 27
        pltS_4to1 = 28
        pltS_8to1 = 29
        pltS_10to1 = 30
        pltS_100to1 = 31
        pltS_1000to1 = 32
    End Enum 'dxxPlotStandardScales
    Public Enum dxxTextDrawingDirections
        Horizontal = 1
        Vertical = 3
        ByStyle = 5
    End Enum 'dxxTextDrawingDirections
    Public Enum dxxScreenEventTypes
        Clear = 0
        Refresh = 1
        ScreenEntityDrawn = 2
        ScreenEntityReDrawn = 8
        ExtentRectangleDrawn = 16
        EntityOCSDrawn = 32
        EntityBoundsDrawn = 64
        EntityExtentPtsDrawn = 128
        UCSDrawn = 256
        ScreenEntityErase = 512
    End Enum
    Public Enum dxxScreenEntityTypes
        All = -1
        Undefined = 0
        Text = 1
        Axis = 2
        Rectangle = 4
        Point = 8
        Circle = 16
        Triangle = 32
        Pointer = 64
        Pill = 128
        Line = 256
    End Enum 'dxxScreenEntityTypes
    Public Enum dxxFileObjectTypes
        <Description("Undefined")> Undefined = -1
        <Description("Header")> Header = 1
        <Description("Entity")> Entity = 2
        <Description("Block")> Block = 4
        <Description("Table Entry")> TableEntry = 8
        <Description("Class")> DXFClass = 16
        <Description("Object")> DXFObject = 32
        <Description("ACSData")> ACSData = 64
        <Description("Entities")> Entities = 128
        <Description("Classes")> DXFClasses = 256
        <Description("Objects")> DXFObjects = 512
        <Description("Blocks")> Blocks = 1024
        <Description("Table")> Table = 2048
        <Description("Attribute")> Attribute = 4096
    End Enum 'dxxFileObjectTypes

    Public Enum dxxDotShapes
        <Description("Undefined")> Undefined = -1
        <Description("Circle")> Circle = 0
        <Description("Square")> Square = 1
    End Enum 'dxxFileObjectTypes


    Public Enum dxxDomains
        World = 0
        Viewport = 1
        Device = 2
        UCS = 3
    End Enum 'dxxDomains
    Public Enum dxxBorderSizes
        dxfSize_Undefined = 0
        dxfASize_Landscape = 1
        dxfBSize_Landscape = 2
        dxfCSize_Landscape = 3
        dxfDSize_Landscape = 4
        dxfMDFunctional = 5
    End Enum 'dxxBorderSizes
    Public Enum dxxFileErrorTypes
        <Description("Header")> HeaderError = 0
        <Description("Entity")> EntityError = 1
        <Description("Block")> BlockError = 2
    End Enum 'dxxFileErrorTypes
    Public Enum dxxVertexEventTypes
        Undefined = 0
        Position = 1
        Variable = 2
        Display = 3
    End Enum 'dxxVertexEventTypes
    Public Enum dxxBlockReferenceTypes
        Undefined = 0
        Name = 1
        Handle = 2
        GUID = 3
        LayoutHandle = 4
        LayoutName = 5
    End Enum
    Public Enum dxxEntityEventTypes
        Undefined = (-1 * 999)
        DefPoint = 0
        OCS = 1
        Font = 2
        DisplaySettings = 3
        Vertex = 4
        PropertyValue = 5
        Delete = 7
        DefPoints = 8
    End Enum 'dxxEntityEventTypes
    Public Enum dxxCoordinateSystemEventTypes
        Origin = 8
        Orientation = 16
        Units = 32
        Dimensions = 64
    End Enum 'dxxCoordinateSystemEventTypes
    Public Enum dxxCollectionEventTypes
        Clear = (-1 * 3)
        RemoveSet = (-1 * 2)
        Remove = -1
        Add = 1
        MemberChange = 2
        PreAdd = 3
        PreRemove = 4
        Append = 5
        MemberMove = 6
        Suppression = 7
        Populate = 8
        Invalidate = 9
        CollectionMove = 100
        CollectionMoveUndo = (-1 * 100)
    End Enum 'dxxCollectionEventTypes
    Public Enum dxxDimTextFitTypes
        MoveTextAndArrows = 0 'Places both text and arrows outside extension lines
        MoveArrowsFirst = 1 'Moves arrows first, then text
        MoveTextFirst = 2 'Moves text first, then arrows
        BestFit = 3 'Moves either text or arrows, whichever fits best
    End Enum 'dxxDimTextFitTypes
    Public Enum dxxSegmentTypes
        Line = 1
        Arc = 2
    End Enum 'dxxSegmentTypes
    Public Enum dxxAttributeTypes
        None = 0
        Invisible = 1
        Constant = 2
        VerifyOnInput = 4
        Preset = 8
    End Enum 'dxxAttributeTypes
    Public Enum dxxTextTypes
        <CodeValue("UNDEF")> Undefined = -1
        <CodeValue("TEXT")> DText = 50
        <CodeValue("ATTDEF")> AttDef = 51
        <CodeValue("ATTRIB")> Attribute = 52
        <CodeValue("MTEXT")> Multiline = 53
    End Enum 'dxxTextTypes
    Public Enum dxxTransformationTypes
        <CodeValue("Undefined")> Undefined = 0
        <CodeValue("Mirror")> Mirror = 1
        <CodeValue("Translation")> Translation = 2
        <CodeValue("Rotation")> Rotation = 4
        <CodeValue("Scale")> Scale = 8
    End Enum 'dxxTransformationTypes
    Public Enum dxxCharFormatCodes
        <Description("END FORMAT")> EndFormat = (-1 * 3)
        <Description("NULL")> Null = (-1 * 2)
        <Description("BASE")> Base = -1
        <Description("NONE")> None = 0
        <Description("OPEN GROUP BRACKET")> OpenGroupBracket = 1
        <Description("CLOSE GROUP BRACKET")> CloseGroupBracket = 2
        <Description("NBS")> NBS = 3
        <Description("BACKSLASH")> Backslash = 4
        <Description("OPEN BRACKET")> OpenBracket = 5
        <Description("CLOSE BRACKET")> CloseBracket = 6
        <Description("STACK")> Stack = 7
        <Description("STACK BREAK")> StackBreak = 8
        <Description("LINE FEED")> LineFeed = 9
        <Description("DEGREE SYMBOL")> DegreeSymbol = 10
        <Description("PLUS MINUS SYMBOL")> PlusMinusSymbol = 11
        <Description("DIAMETER SYMBOL")> DiameterSymbol = 12
        <Description("END STACK")> EndStack = 13
        <Description("OVERLINE ON")> OverlineOn = 20
        <Description("OVERLINE OFF")> OverlineOff = 21
        <Description("UNDERLINE ON")> UnderlineOn = 22
        <Description("UNDERLINE OFF")> UnderlineOff = 23
        <Description("STRIKE THRU ON")> StrikeThruOn = 24
        <Description("STRIKE THRU OFF")> StrikeThruOff = 25
        <Description("DECIMAL STACK LEADER")> DecimalStackLeader = 26
        <Description("COLOR")> Color = 100
        <Description("FONT")> Font = 101
        <Description("HEIGHT")> Height = 102
        <Description("HEIGHT FACTOR")> HeightFactor = 103
        <Description("TRACKING")> Tracking = 104
        <Description("OBLIQUE")> Oblique = 105
        <Description("WIDTH FACTOR")> WidthFactor = 106
        <Description("ALIGNMENT")> Alignment = 107
    End Enum 'dxxCharFormatCodes
    Public Enum dxxCharacterAlignments
        Undefined = -1
        Bottom = 0
        Center = 1
        Top = 2
    End Enum 'dxxCharacterAlignments
    Public Enum dxxCharacterStackStyles
        <Description("None")> None = 0
        <Description("Horizontal")> Horizontal = 1
        <Description("None")> Diagonal = 2
        <Description("Tolerance")> Tolerance = 3
        <Description("Decimal")> DecimalStack = 4
        <Description("Superscript")> SuperScript = 5
        <Description("Subscript")> SubScript = 6
    End Enum 'dxxCharacterStackStyles
    Public Enum dxxPointModes
        Undefined = -1
        Dot = 0
        None = 1
        Cross = 2
        X = 3
        Tick = 4
        CircDot = 32
        Circ = 33
        CircCross = 34
        CircX = 35
        CircTick = 36
        SqrDot = 64
        Sqr = 65
        SqrCross = 66
        SqrX = 67
        SqrTick = 68
        CircSqrDot = 96
        CircSqr = 97
        CircSqrCross = 98
        CircSqrX = 99
        CircSqrTick = 100
    End Enum 'dxxPointModes

    Public Enum dxxSegmentPointTypes
        Undefined = -1
        StartPt = 0
        MidPt = 1
        EndPt = 2
    End Enum 'dxxSegmentPointTypes

    Public Enum dxxEntDefPointTypes
        Undefined = -1
        StartPt = 0
        MidPt = 1
        Center = 2
        HandlePt = 3
        EndPt = 4
        TopLeft = 101
        TopCenter = 102
        TopRight = 103
        MiddleLeft = 104
        MiddleCenter = 105
        MiddleRight = 106
        BottomLeft = 107
        BottomCenter = 108
        BottomRight = 109
    End Enum 'dxxEntDefPointTypes
    Public Enum dxxPointFilters
        GetTopLeft = 1 'must be first
        GetTopRight = 2
        GetBottomLeft = 3
        GetBottomRight = 4
        GetLeftTop = 5
        GetRightTop = 6
        GetLeftBottom = 7
        GetRightBottom = 8 'must be last
        AtX = 9
        AtY = 10
        AtZ = 11
        AtMaxX = 12
        AtMaxY = 13
        AtMaxZ = 14
        AtMinX = 15
        AtMinY = 16
        AtMinZ = 17
        GreaterThanX = 18
        LessThanX = 19
        GreaterThanY = 20
        LessThanY = 21
        GreaterThanZ = 22
        LessThanZ = 23
        NearestToX = 24
        NearestToY = 25
        NearestToZ = 26
        FarthestFromX = 27
        FarthestFromY = 28
        FarthestFromZ = 29
    End Enum 'dxxPointFilters
    Public Enum dxxLoopTypes
        Undefined = -1
        Line = 1
        CircularArc = 2
        EllipticalArc = 3
        Bezier = 4
        Spline = 5
        Point = 6
        Polyline = 7
    End Enum 'dxxLoopTypes
    Public Enum dxxHatchTypes
        User = 0
        PreDefined = 1
        Custom = 2
    End Enum 'dxxHatchTypes
    Public Enum dxxHatchStyle
        <Description("User Defined")> dxfHatchUserDefined = 0
        <Description("Solid")> dxfHatchSolidFill = 1
        <Description("Pre Defined")> dxfHatchPreDefined = 2
    End Enum 'dxxHatchStyle
    Public Enum dxxPitchTypes
        Undefined = -1
        Rectangular = 0
        Triangular = 1
        InvertedTriangular = 2
    End Enum 'dxxPitchTypes
    Public Enum dxxLayerStatus
        dxfLayerNormal = 0
        dxfLayerFrozen = 1
        dxfLayerFrozenInNewVPorts = 2
        dxfLayerLocked = 4
    End Enum 'dxxLayerStatus
    Public Enum dxxDeviceUnits
        <DisplayName("Undefined")> <Description("Undefined")> Undefined = -1
        <DisplayName("pxs")> <Description("Pixels")> Pixels = 1
        <DisplayName("pts")> <Description("Points")> Points = 2
        <DisplayName("chr")> <Description("Characters")> Characters = 3
        <DisplayName("in")> <Description("Inches")> Inches = 4
        <DisplayName("mm")> <Description("Millimeters")> Millimeters = 5
        <DisplayName("cm")> <Description("Centimeters")> Centimeters = 6
    End Enum 'dxxDeviceUnits
    Public Enum dxxVertexStyles
        <Description("UNDEF")> UNDEFINED = 0
        <Description("CLOSEFIG")> CLOSEFIGURE = 1
        <Description("BEZIER")> BEZIER = 3
        <Description("LINETO")> LINETO = 2
        <Description("BEZIERTO")> BEZIERTO = 4
        <Description("MOVETO")> MOVETO = 6
        <Description("PIXEL")> PIXEL = 10
        <Description("CLOSEPATH")> CLOSEPATH = 128
    End Enum 'dxxVertexStyles
    Public Enum dxxRoundToLimits
        Undefined = -1
        Sixteenth = 0
        Eighth = 1
        Quarter = 2
        Half = 3
        One = 4
        ThirtySeconds = 5
        Third = 6
        Millimeter = 7
        Centimeter = 8

    End Enum 'dxxRoundToLimits

    Public Enum dxxRoundingTypes
        Natural = 0
        Up = 1
        Down = 2
    End Enum 'dxxRoundingTypes

    Public Enum dxxAxisDescriptors
        X = 1
        Y = 2
        Z = 3
    End Enum 'dxxAxisDescriptors

    Public Enum dxx2DOrdinateDescriptors
        X = 0
        Y = 1
    End Enum 'dxxOrdinateDescriptorsr

    Public Enum dxxOrdinateDescriptors
        X = 0
        Y = 1
        Z = 2
    End Enum 'dxxOrdinateDescriptors
    Public Enum dxxSortOrders
        LeftToRight = 0
        RightToLeft = 1
        TopToBottom = 2
        BottomToTop = 3
        Clockwise = 4
        CounterClockwise = 5
        NearestToFarthest = 6
        FarthestToNearest = 7
        TopToBottomAndLeftToRight = 8
    End Enum 'dxxSortOrders
    Public Enum dxxOrdinateTypes
        MinX = 0
        MinY = 1
        MinZ = 2
        MaxX = 3
        MaxY = 4
        MaxZ = 5
        MidX = 6
        MidY = 7
        MidZ = 8
    End Enum 'dxxOrdinateTypes
    Public Enum dxxRectangularAlignments
        General = -1
        TopLeft = 1
        TopCenter = 2
        TopRight = 3
        MiddleLeft = 4
        MiddleCenter = 5
        MiddleRight = 6
        BottomLeft = 7
        BottomCenter = 8
        BottomRight = 9
    End Enum 'dxxRectangularAlignments
    Public Enum dxxRectanglePts
        TopLeft = 1
        TopCenter = 2
        TopRight = 3
        BottomLeft = 4
        BottomCenter = 5
        BottomRight = 6
        MiddleLeft = 7
        MiddleCenter = 8
        MiddleRight = 9
        BaselineLeft = 10
        BaselineCenter = 11
        BaselineRight = 12
        Center = 13
        UnderlineLeft = 14
        UnderlineRight = 15
        OverlineLeft = 16
        OverlineRight = 17
    End Enum 'dxxRectanglePts
    Public Enum dxxRectangleLines
        LeftEdge = 1
        BottomEdge = 2
        RightEdge = 3
        TopEdge = 4
        Baseline = 5
        Diagonal1 = 6
        Diagonal2 = 7
    End Enum 'dxxRectangleLines
    Public Enum dxxHorizontalJustifications
        Undefined = -1
        Left = 0
        Center = 1
        Right = 2
    End Enum 'dxxHorizontalJustifications
    Public Enum dxxVerticalJustifications
        Undefined = -1
        Bottom = 0
        Center = 1
        Top = 2
    End Enum 'dxxVerticalJustifications
    Public Enum dxxTextJustificationsHorizontal
        Left = 0
        Center = 1
        Right = 2
        Align = 3
        HMiddle = 4
        Fit = 5
    End Enum 'dxxTextJustificationsHorizontal
    Public Enum dxxTextJustificationsVertical
        Baseline = 0
        Bottom = 1
        Middle = 2
        Top = 3
    End Enum 'dxxTextJustificationsVertical
    Public Enum dxxFillStyles
        Undefined = -1
        Solid = 0
        HorizontalLine = 1
        VerticalLine = 2
        UpwardDiagonal = 3
        DownwardDiagonal = 4
        Cross = 5
        DiagonalCross = 6
    End Enum 'dxxFillStyles
    Public Enum dxxLineSpacingStyles
        <Description("Undefined")> Undefined = 0
        <Description("At Least")> AtLeast = 1
        <Description("Exact")> Exact = 2
    End Enum 'dxxLineSpacingStyles
    Public Enum dxxMTextAlignments
        <Description("Unknown")> AlignUnknown = (-1 * 99)
        <Description("Top Left")> TopLeft = 1
        <Description("Top Center")> TopCenter = 2
        <Description("Top Right")> TopRight = 3
        <Description("Middle Left")> MiddleLeft = 4
        <Description("Middle Center")> MiddleCenter = 5
        <Description("Middle Right")> MiddleRight = 6
        <Description("Bottom Left")> BottomLeft = 7
        <Description("Bottom Center")> BottomCenter = 8
        <Description("Bottom Right")> BottomRight = 9
        <Description("Baseline Left")> BaselineLeft = 10
        <Description("Baseline Middle")> BaselineMiddle = 11
        <Description("Baseline Right")> BaselineRight = 12
        <Description("Fit")> Fit = 13
        <Description("Aligned")> Aligned = 14
    End Enum 'dxxMTextAlignments
    Public Enum dxxFontTypes
        Undefined = 0
        Embedded = 1
        Shape = 2
        TTF = 3
    End Enum
    Public Enum dxxFontStyle
        Normal = 0
        Italic = 10
        Underline = 100
        Bold = 1000
    End Enum 'dxxFontStyle

    Public Enum dxxArcTypes
        <Description("Arc")> Arc = 4
        <Description("Ellipse")> Ellipse = 8
    End Enum

    Public Enum dxxPolylineTypes
        <Description("Polyline")> Polyline = 64
        <Description("Polygon")> Polygon = 128
    End Enum
    Public Enum dxxInsertUnits
        <Description("Unitless")> Unitless = 0
        <Description("Inches")> Inches = 1
        <Description("Feet")> Feet = 2
        <Description("Miles")> Miles = 3
        <Description("Millimeters")> Millimeters = 4
        <Description("Centimeters")> Centimeters = 5
        <Description("Meters")> Meters = 6
        <Description("Kilometers")> Kilometers = 7
        <Description("Microinches")> Microinches = 8
        <Description("Mils")> Mils = 9
        <Description("Yards")> Yards = 10
        <Description("Angstroms")> Angstroms = 11
        <Description("Nanometers")> Nanometers = 12
        <Description("Microns")> Microns = 13
        <Description("Decimeters")> Decimeters = 14
        <Description("Decameters")> Decameters = 15
        <Description("Hectometers")> Hectometers = 16
        <Description("Gigameters")> Gigameters = 17
        <Description("Astronomical")> Astronomical = 18
        <Description("Light Years")> LightYears = 19
        <Description("Parsecs")> Parsecs = 20
    End Enum

    Public Enum dxxGraphicTypes
        <Description("End Block")> EndBlock = (-1 * 2)
        <Description("Squence End")> SequenceEnd = -1
        <Description("Undefined")> Undefined = 0
        <Description("Point")> Point = 1
        <Description("Line")> Line = 2
        <Description("Arc")> Arc = 4
        <Description("Ellipse")> Ellipse = 8
        <Description("Bezier")> Bezier = 16
        <Description("Solid")> Solid = 32
        <Description("Polyline")> Polyline = 64
        <Description("Polygon")> Polygon = 128
        <Description("Text")> Text = 256
        <Description("Insert")> Insert = 512
        <Description("Leader")> Leader = 1024
        <Description("Dimension")> Dimension = 2048
        <Description("Hatch")> Hatch = 4096
        <Description("Hole")> Hole = 8192
        <Description("Symbol")> Symbol = 16384
        <Description("Table")> Table = 32768
        <Description("Shape")> Shape = 2 * Table
        <Description("MText")> MText = 2 * Shape
        <Description("Viewport")> Viewport = 2 * MText
    End Enum 'dxxGraphicTypes
    Public Enum dxxTextStyleFontSettings
        <Description("Undefined")> Undefined = 0
        <Description("Regular")> Regular = 34
        <Description("Bold")> Bold = 33554466
        <Description("Bold Italic")> BoldItalic = 50331682
        <Description("Italic")> Italic = 16777250
    End Enum 'dxxTextStyleFontSettings
    Public Enum dxxACADVersions
        DefaultVersion = -1
        <CodeValue("UNKNOWN")> <Description("UNKNOWN")> UnknownVersion = 0
        <CodeValue("AC1004")> <Description("AutoCAD Release 9")> R9 = 1004
        <CodeValue("AC1006")> <Description("AutoCAD Release 10")> R10 = 1006
        <CodeValue("AC1009")> <Description("AutoCAD Release 11/12")> R11 = 1009
        <CodeValue("AC1012")> <Description("AutoCAD Release 13")> R13 = 1012
        <CodeValue("AC1014")> <Description("AutoCAD Release 14")> R14 = 1014
        <CodeValue("AC1015")> <Description("AutoCAD 2000/2000i/2002")> R2000 = 1015
        <CodeValue("AC1018")> <Description("AutoCAD 2004/2005/2006")> R2004 = 1018
        <CodeValue("AC1021")> <Description("AutoCAD 2007/2008/2009")> R2007 = 1021
        <CodeValue("AC1024")> <Description("AutoCAD 2010/2011")> R2010 = 1024
        <CodeValue("AC1027")> <Description("AutoCAD 2013")> R2013 = 1027
        <CodeValue("AC1032")> <Description("AutoCAD 2018")> R2018 = 1032
    End Enum 'dxxACADVersions
    Public Enum dxxFileSections
        <Description("UNKNOWN")> Unknown = 0
        <Description("HEADER")> Header = 1
        <Description("CLASSES")> Classes = 2
        <Description("TABLES")> Tables = 3
        <Description("BLOCKS")> Blocks = 4
        <Description("ENTITIES")> Entities = 5
        <Description("OBJECTS")> Objects = 6
        <Description("THUMBNAILIMAGE")> Thumbnail = 7
        <Description("SETTINGS")> Settings = 100
    End Enum


    Public Enum dxxTableTypes
        <Description("UNKNOWN")> Unknown = 0
        <Description("APPID")> AppID = 1
        <Description("BLOCK_RECORD")> BlockRecord = 2
        <Description("DIMSTYLE")> DimStyle = 3
        <Description("LAYER")> Layer = 4
        <Description("LTYPE")> LType = 5
        <Description("STYLE")> Style = 6
        <Description("UCS")> UCS = 7
        <Description("VIEW")> View = 8
        <Description("VPORT")> VPort = 9
    End Enum

    Public Enum dxxHoleTypes
        Undefined = -1
        Round = 0
        Square = 1
        RoundSlot = 2
        SquareSlot = 3
    End Enum 'dxxHoleTypes
    Public Enum dxxEntityTypes
        SequenceEnd = (-1 * 101)
        EndBlock = (-1 * 100)
        Undefined = -1
        Line = 1
        Polyline = 2
        Arc = 3
        Circle = 4
        Ellipse = 5
        Bezier = 6
        Shape = 7
        Point = 10
        Solid = 12
        Insert = 16
        Table = 17
        Polygon = 18
        Trace = 19
        Hole = 20
        Slot = 21
        Text = 50 'must match text types
        Attdef = 51
        Attribute = 52
        MText = 53
        Character = 54
        Hatch = 101
        DimLinearH = 1001 'must be lowest
        DimLinearV = 1002
        DimLinearA = 1003
        DimOrdinateH = 1004
        DimOrdinateV = 1005
        DimRadialR = 1006
        DimRadialD = 1007
        DimAngular = 1008
        DimAngular3P = 1050 'must be highest
        LeaderText = 1100
        LeaderTolerance = 1101
        LeaderBlock = 1102
        Leader = 1103
        Symbol = 1200
    End Enum 'dxxEntityTypes
    Public Enum dxxDimensionTypes
        <DisplayName("Undefined")> Undefined = -1
        <DisplayName("Linear")> Linear = 0
        <DisplayName("Ordinate")> Ordinate = 1
        <DisplayName("Radial")> Radial = 2
        <DisplayName("Angular")> Angular = 3
    End Enum 'dxxDimensionTypes
    Public Enum dxxDisplayProperties
        <Description("Undefined")> Undefined = 0
        <Description("LayerName")> LayerName = 1
        <Description("Color")> Color = 2
        <Description("Linetype")> Linetype = 4
        <Description("LTScale")> LTScale = 8
        <Description("LineWeight")> LineWeight = 16
        <Description("DimStyle")> DimStyle = 32
        <Description("TextStyle")> TextStyle = 64
        <Description("IsDirty")> IsDirty = 128
        <Description("Suppressed")> Suppressed = 256
    End Enum
    Public Enum dxxDimTypes
        <DisplayName("Undefined")> Undefined = -1
        <DisplayName("Linear(H)")> LinearHorizontal = 0
        <DisplayName("Linear(V)")> LinearVertical = 1
        <DisplayName("Linear(A)")> LinearAligned = 2
        <DisplayName("Ordinate(V)")> OrdVertical = 10
        <DisplayName("Ordinate(H)")> OrdHorizontal = 11
        <DisplayName("Radial")> Radial = 20
        <DisplayName("Diametric")> Diametric = 21
        <DisplayName("Angular")> Angular = 30
        <DisplayName("Angular(3P)")> Angular3P = 31
    End Enum 'dxxDimTypes
    Public Enum dxxLinearDimTypes
        <DisplayName("Undefined")> Undefined = -1
        <DisplayName("Linear(H)")> LinearHorizontal = 0
        <DisplayName("Linear(V)")> LinearVertical = 1
        <DisplayName("Linear(A)")> LinearAligned = 4
    End Enum 'dxxLinearDimTypes
    Public Enum dxxOrdinateDimTypes
        <DisplayName("Undefined")> Undefined = -1
        <DisplayName("Ordinate(V)")> OrdVertical = 2
        <DisplayName("Ordinate(H)")> OrdHorizontal = 3
    End Enum 'dxxOrdinateDimTypes
    Public Enum dxxRadialDimensionTypes
        <DisplayName("Undefined")> Undefined = -1
        <DisplayName("Radial")> Radial = 0
        <DisplayName("Diametric")> Diametric = 1
    End Enum 'dxxRadialDimensionTypes
    Public Enum dxxAngularDimTypes
        <DisplayName("Undefined")> Undefined = -1
        <DisplayName("Angular")> Angular = 0
        <DisplayName("Angular(3P)")> Angular3P = 1
    End Enum 'dxxAngularDimTypes
    Public Enum dxxLeaderTypes
        <DisplayName("Leader with Text")> LeaderText = 0
        <DisplayName("Leader with Tolerance")> LeaderTolerance = 1
        <DisplayName("Leader with Block")> LeaderBlock = 2
        <DisplayName("Leader")> NoReactor = 3
    End Enum 'dxxLeaderTypes
    Public Enum dxxBackGroundFillStyles
        Off = 0
        EntColor = 1
        Window = 2
    End Enum
    Public Enum dxxColors
        <Description("Undefined")> Undefined = -1
        <Description("ByBlock")> ByBlock = 0
        <Description("Red")> Red = 1
        <Description("Yellow")> Yellow = 2
        <Description("Green")> Green = 3
        <Description("Cyan")> Cyan = 4
        <Description("Blue")> Blue = 5
        <Description("Magenta")> Magenta = 6
        <Description("Black/White")> BlackWhite = 7
        <Description("Grey")> Grey = 8
        <Description("Light Grey")> LightGrey = 9
        <Description("Color 10")> z_10 = 10
        <Description("Light Red")> LightRed = 11
        <Description("Color 12")> z_12 = 12
        <Description("Color 13")> z_13 = 13
        <Description("Color 14")> z_14 = 14
        <Description("Color 15")> z_15 = 15
        <Description("Color 16")> z_16 = 16
        <Description("Color 17")> z_17 = 17
        <Description("Color 18")> z_18 = 18
        <Description("Color 19")> z_19 = 19
        <Description("Color 20")> z_20 = 20
        <Description("Color 21")> z_21 = 21
        <Description("Color 22")> z_22 = 22
        <Description("Light Brown")> LightBrown = 23
        <Description("Color 24")> z_24 = 24
        <Description("Brown")> Brown = 25
        <Description("Color 26")> z_26 = 26
        <Description("Color 27")> z_27 = 27
        <Description("Color 28")> z_28 = 28
        <Description("Color 29")> z_29 = 29
        <Description("Orange")> Orange = 30
        <Description("Color 31")> z_31 = 31
        <Description("Color 32")> z_32 = 32
        <Description("Color 33")> z_33 = 33
        <Description("Color 34")> z_34 = 34
        <Description("Color 35")> z_35 = 35
        <Description("Color 36")> z_36 = 36
        <Description("Color 37")> z_37 = 37
        <Description("Color 38")> z_38 = 38
        <Description("Color 39")> z_39 = 39
        '<Description("Color x0")> z_x0 = 30
        '<Description("Color x1")> z_x1 = 31
        '<Description("Color x2")> z_x2 = 32
        '<Description("Color x3")> z_x3 = 33
        '<Description("Color x4")> z_x4 = 34
        '<Description("Color x5")> z_x5 = 35
        '<Description("Color x6")> z_x6 = 36
        '<Description("Color x7")> z_x7 = 37
        '<Description("Color x8")> z_x8 = 38
        '<Description("Color x9")> z_x9 = 39
        <Description("Color 40")> z_40 = 40
        <Description("Color 41")> z_41 = 41
        <Description("Color 42")> z_42 = 42
        <Description("Color 43")> z_43 = 43
        <Description("Color 44")> z_44 = 44
        <Description("Color 45")> z_45 = 45
        <Description("Color 46")> z_46 = 46
        <Description("Color 47")> z_47 = 47
        <Description("Color 48")> z_48 = 48
        <Description("Color 49")> z_49 = 49
        <Description("Color 50")> z_50 = 50
        <Description("Light Yellow")> LightYellow = 51
        <Description("Color 52")> z_52 = 52
        <Description("Color 53")> z_53 = 53
        <Description("Color 54")> z_54 = 54
        <Description("Color 55")> z_55 = 55
        <Description("Color 56")> z_56 = 56
        <Description("Color 57")> z_57 = 57
        <Description("Color 58")> z_58 = 58
        <Description("Color 59")> z_59 = 59
        <Description("Color 60")> z_60 = 60
        <Description("Color 61")> z_61 = 61
        <Description("Color 62")> z_62 = 62
        <Description("Color 63")> z_63 = 63
        <Description("Color 64")> z_64 = 64
        <Description("Color 65")> z_65 = 65
        <Description("Color 66")> z_66 = 66
        <Description("Color 67")> z_67 = 67
        <Description("Color 68")> z_68 = 68
        <Description("Color 69")> z_69 = 69
        <Description("Color 70")> z_70 = 70
        <Description("Color 71")> z_71 = 71
        <Description("Color 72")> z_72 = 72
        <Description("Color 73")> z_73 = 73
        <Description("Color 74")> z_74 = 74
        <Description("Color 75")> z_75 = 75
        <Description("Color 76")> z_76 = 76
        <Description("Color 77")> z_77 = 77
        <Description("Color 78")> z_78 = 78
        <Description("Color 79")> z_79 = 79
        <Description("Color 80")> z_80 = 80
        <Description("Light Green")> LightGreen = 81
        <Description("Color 82")> z_82 = 82
        <Description("Color 83")> z_83 = 83
        <Description("Dark Green")> DarkGreen = 84
        <Description("Color 85")> z_85 = 85
        <Description("Color 86")> z_86 = 86
        <Description("Color 87")> z_87 = 87
        <Description("Color 88")> z_88 = 88
        <Description("Color 89")> z_89 = 89
        <Description("Color 90")> z_90 = 90
        <Description("Color 91")> z_91 = 91
        <Description("Color 92")> z_92 = 92
        <Description("Color 93")> z_93 = 93
        <Description("Color 94")> z_94 = 94
        <Description("Color 95")> z_95 = 95
        <Description("Color 96")> z_96 = 96
        <Description("Color 97")> z_97 = 97
        <Description("Color 98")> z_98 = 98
        <Description("Color 99")> z_99 = 99
        <Description("Color 100")> z_100 = 100
        <Description("Color 101")> z_101 = 101
        <Description("Color 102")> z_102 = 102
        <Description("Color 103")> z_103 = 103
        <Description("Color 104")> z_104 = 104
        <Description("Color 105")> z_105 = 105
        <Description("Color 106")> z_106 = 106
        <Description("Color 107")> z_107 = 107
        <Description("Color 108")> z_108 = 108
        <Description("Color 109")> z_109 = 109
        <Description("Color 110")> z_110 = 110
        <Description("Color 111")> z_111 = 111
        <Description("Color 112")> z_112 = 112
        <Description("Color 113")> z_113 = 113
        <Description("Color 114")> z_114 = 114
        <Description("Color 115")> z_115 = 115
        <Description("Color 116")> z_116 = 116
        <Description("Color 117")> z_117 = 117
        <Description("Color 118")> z_118 = 118
        <Description("Color 119")> z_119 = 119
        <Description("Color 120")> z_120 = 120
        <Description("Color 121")> z_121 = 121
        <Description("Color 122")> z_122 = 122
        <Description("Color 123")> z_123 = 123
        <Description("Color 124")> z_124 = 124
        <Description("Color 125")> z_125 = 125
        <Description("Color 126")> z_126 = 126
        <Description("Color 127")> z_127 = 127
        <Description("Color 128")> z_128 = 128
        <Description("Color 129")> z_129 = 129
        <Description("Color 130")> z_130 = 130
        <Description("Light Cyan")> LightCyan = 131
        <Description("Color 132")> z_132 = 132
        <Description("Color 133")> z_133 = 133
        <Description("Color 134")> z_134 = 134
        <Description("Color 135")> z_135 = 135
        <Description("Color 136")> z_136 = 136
        <Description("Color 137")> z_137 = 137
        <Description("Color 138")> z_138 = 138
        <Description("Color 139")> z_139 = 139
        <Description("Color 140")> z_140 = 140
        <Description("Color 141")> z_141 = 141
        <Description("Color 142")> z_142 = 142
        <Description("Color 143")> z_143 = 143
        <Description("Color 144")> z_144 = 144
        <Description("Color 145")> z_145 = 145
        <Description("Color 146")> z_146 = 146
        <Description("Color 147")> z_147 = 147
        <Description("Color 148")> z_148 = 148
        <Description("Color 149")> z_149 = 149
        <Description("Color 150")> z_150 = 150
        <Description("Light Blue")> LightBlue = 151
        <Description("Color 152")> z_152 = 152
        <Description("Color 153")> z_153 = 153
        <Description("Color 154")> z_154 = 154
        <Description("Color 155")> z_155 = 155
        <Description("Color 156")> z_156 = 156
        <Description("Color 157")> z_157 = 157
        <Description("Color 158")> z_158 = 158
        <Description("Color 159")> z_159 = 159
        <Description("Color 160")> z_160 = 160
        <Description("Color 161")> z_161 = 161
        <Description("Color 162")> z_162 = 162
        <Description("Color 163")> z_163 = 163
        <Description("Color 164")> z_164 = 164
        <Description("Color 165")> z_165 = 165
        <Description("Color 166")> z_166 = 166
        <Description("Color 167")> z_167 = 167
        <Description("Color 168")> z_168 = 168
        <Description("Color 169")> z_169 = 169
        <Description("Color 170")> z_170 = 170
        <Description("Color 171")> z_171 = 171
        <Description("Color 172")> z_172 = 172
        <Description("Color 173")> z_173 = 173
        <Description("Color 174")> z_174 = 174
        <Description("Color 175")> z_175 = 175
        <Description("Color 176")> z_176 = 176
        <Description("Color 177")> z_177 = 177
        <Description("Color 178")> z_178 = 178
        <Description("Color 179")> z_179 = 179
        <Description("Color 180")> z_180 = 180
        <Description("Color 181")> z_181 = 181
        <Description("Color 182")> z_182 = 182
        <Description("Color 183")> z_183 = 183
        <Description("Color 184")> z_184 = 184
        <Description("Color 185")> z_185 = 185
        <Description("Color 186")> z_186 = 186
        <Description("Color 187")> z_187 = 187
        <Description("Color 188")> z_188 = 188
        <Description("Color 189")> z_189 = 189
        <Description("Purple")> Purple = 190
        <Description("Color 191")> z_191 = 191
        <Description("Dark Purple")> DarkPurple = 192
        <Description("Color 193")> z_193 = 193
        <Description("Color 194")> z_194 = 194
        <Description("Color 195")> z_195 = 195
        <Description("Color 196")> z_196 = 196
        <Description("Color 197")> z_197 = 197
        <Description("Color 198")> z_198 = 198
        <Description("Color 199")> z_199 = 199
        <Description("Color 200")> z_200 = 200
        <Description("Light Purple")> LightPurple = 201
        <Description("Color 202")> z_202 = 202
        <Description("Color 203")> z_203 = 203
        <Description("Color 204")> z_204 = 204
        <Description("Color 205")> z_205 = 205
        <Description("Color 206")> z_206 = 206
        <Description("Color 207")> z_207 = 207
        <Description("Color 208")> z_208 = 208
        <Description("Color 209")> z_209 = 209
        <Description("Color 210")> z_210 = 210
        <Description("Light Magenta")> LightMagenta = 211
        <Description("Color 212")> z_212 = 212
        <Description("Color 213")> z_213 = 213
        <Description("Color 214")> z_214 = 214
        <Description("Color 215")> z_215 = 215
        <Description("Color 216")> z_216 = 216
        <Description("Color 217")> z_217 = 217
        <Description("Color 218")> z_218 = 218
        <Description("Color 219")> z_219 = 219
        <Description("Color 220")> z_220 = 220
        <Description("Color 221")> z_221 = 221
        <Description("Color 222")> z_222 = 222
        <Description("Color 223")> z_223 = 223
        <Description("Color 224")> z_224 = 224
        <Description("Color 225")> z_225 = 225
        <Description("Color 226")> z_226 = 226
        <Description("Color 227")> z_227 = 227
        <Description("Color 228")> z_228 = 228
        <Description("Color 229")> z_229 = 229
        <Description("Color 230")> z_230 = 230
        <Description("Color 231")> z_231 = 231
        <Description("Color 232")> z_232 = 232
        <Description("Color 233")> z_233 = 233
        <Description("Color 234")> z_234 = 234
        <Description("Color 235")> z_235 = 235
        <Description("Color 236")> z_236 = 236
        <Description("Color 237")> z_237 = 237
        <Description("Color 238")> z_238 = 238
        <Description("Color 239")> z_239 = 239
        <Description("Color 240")> z_240 = 240
        <Description("Color 241")> z_241 = 241
        <Description("Color 242")> z_242 = 242
        <Description("Color 243")> z_243 = 243
        <Description("Color 244")> z_244 = 244
        <Description("Color 245")> z_245 = 245
        <Description("Color 246")> z_246 = 246
        <Description("Color 247")> z_247 = 247
        <Description("Color 248")> z_248 = 248
        <Description("Color 249")> z_249 = 249
        <Description("Black")> Black = 250
        <Description("Color 251")> z_251 = 251
        <Description("Dark Grey")> DarkGrey = 252
        <Description("Color 253")> z_253 = 253
        <Description("Color 254")> z_254 = 254
        <Description("White")> White = 255
        <Description("ByLayer")> ByLayer = 256
    End Enum 'dxxColors




    Public Enum dxxHoleProperties
        PropUndefined = 0
        Radius = 1
        Length = 2
        MinorRadius = 3
        Rotation = 4
        Depth = 5
        Downset = 6
        Inset = 7
        WeldedBolt = 8
        IsSquare = 9
    End Enum 'dxxHoleProperties
    Public Enum dxxOrthoDirections
        Up = 0
        Down = 1
        Left = 2
        Right = 3
    End Enum 'dxxOrthoDirections
    Public Enum dxxRadialDirections
        TowardsCenter = 0
        AwayFromCenter = 1
    End Enum 'dxxRadialDirections
    Public Enum dxxStandardPlanes
        Undefined = -1
        XY = 0
        XZ = 1
        YZ = 2
    End Enum 'dxxStandardPlanes
    Public Enum dxxUCSIconModes
        None = 0 ' No Icon Is displayed
        LowerLeft = 1 'icon at lower left if not visible at actual location
        Origin = 2 ' icin only at its actual location
    End Enum 'dxxUCSIconModes
    Public Enum dxxColorModes
        ByImage = -1
        Full = 0
        BlackWhite = 1
        GreyScales = 2
    End Enum 'dxxColorModes
    Public Enum dxxSelectionTypes
        Linetype = 1
        Layer = 2
        Color = 4
        SelectAll = 8
        CurrentSet = 16
        Type = 32
        dxfSelectDimsAndLeaders = 64
    End Enum 'dxxSelectionTypes
    Public Enum dxxSheetSizes
        ASize = 0
        BSize = 1
        CSize = 2
        DSize = 3
    End Enum 'dxxSheetSizes
    Public Enum dxxDrawingUnits
        English = 0
        Metric = 1
    End Enum 'dxxDrawingUnits
    Public Enum dxxOrientations
        Horizontal = 0
        Vertical = 1
    End Enum 'dxxOrientations
    Public Enum dxxPaperOrientations
        ByAspect = -1
        OrUndefined = 0
        Portrait = 1
        Landscape = 2
    End Enum 'dxxPaperOrientations
    Public Enum dxxPrinterScaleModes
        CurrentView = 0
        Extents = 1
        ToScale = 3
    End Enum 'dxxPrinterScaleModes
    Public Enum dxxAngularDirections
        CounterClockwise = 0
        Clockwise = 1
    End Enum 'dxxAngularDirections
    Public Enum dxxTrimTypes
        Left = 0
        Right = 1
        Above = 2
        Below = 4
    End Enum 'dxxTrimTypes
    Public Enum dxxSplitTypes
        Vertical = 0
        Horizontal = 1
        ByAngle = 2
    End Enum 'dxxSplitTypes
    Public Enum dxxLineWeights
        <Description("Undefined")> Undefined = (-1 * 4)
        <Description("ByDefault")> ByDefault = (-1 * 3)
        <Description("ByBlock")> ByBlock = (-1 * 2)
        <Description("ByLayer")> ByLayer = -1
        <Description("0.00 mm")> LW_000 = 0
        <Description("0.05 mm")> LW_005 = 5
        <Description("0.09 mm")> LW_009 = 9
        <Description("0.13 mm")> LW_013 = 13
        <Description("0.15 mm")> LW_015 = 15
        <Description("0.18 mm")> LW_018 = 18
        <Description("0.20 mm")> LW_020 = 20
        <Description("0.25 mm")> LW_025 = 25
        <Description("0.30 mm")> LW_030 = 30
        <Description("0.35 mm")> LW_035 = 35
        <Description("0.40 mm")> LW_040 = 40
        <Description("0.50 mm")> LW_050 = 50
        <Description("0.53 mm")> LW_053 = 53
        <Description("0.60 mm")> LW_060 = 60
        <Description("0.70 mm")> LW_070 = 70
        <Description("0.80 mm")> LW_080 = 80
        <Description("0.90 mm")> LW_090 = 90
        <Description("1.00 mm")> LW_100 = 100
        <Description("1.06 mm")> LW_106 = 106
        <Description("1.20 mm")> LW_120 = 120
        <Description("1.40 mm")> LW_140 = 140
        <Description("1.58 mm")> LW_158 = 158
        <Description("2.00 mm")> LW_200 = 200
        <Description("2.11 mm")> LW_211 = 211
    End Enum 'dxxLineWeights
    Public Enum dxxFileHandlerEvents
        Undefined = 0
        BeginFile = 1
        EndFile = 2
        BeginSection = 3
        EndSection = 4
        BeginTable = 5
        EndTable = 6
        BeginTableEntry = 7
        EndTableEntry = 8
        BeginClass = 9
        EndClass = 10
        BeginBlock = 11
        EndBlock = 12
        BeginEntity = 13
        EndEntity = 14
        BeginObject = 15
        EndObject = 16
        BeginPropertyGroup = 17
        EndPropertyGroup = 18
        PathIncrease = 100
        PathDecrease = 101
    End Enum
    Public Enum dxxRectangleMethods
        ByCorner = 0
        ByCenter = 1
    End Enum 'dxxRectangleMethods
    Public Enum dxxWeldTypes
        Fillet = 0
    End Enum 'dxxWeldTypes
    Public Enum dxxSymbolTypes
        Undefined = 0
        Arrow = 1
        Bubble = 2
        DetailBubble = 3
        Weld = 4
    End Enum 'dxxSymbolTypes
    Public Enum dxxArrowTypes
        Pointer = 0
        View = 1
        Section = 2
        Axis = 3
    End Enum 'dxxArrowTypes
    Public Enum dxxArrowStyles
        Undefined = -1
        StdBlocks = 0
        AngledHalf = 1
        StraightHalf = 2
        AngledFull = 3
        StraightFull = 4
        AngledHalfOpen = 5
        StraightHalfOpen = 6
        AngledFullOpen = 7
        StraightFullOpen = 8
    End Enum 'dxxArrowStyles
    Public Enum dxxArrowTails
        Undefined = 0
        Filled = 1
        Open = 2
    End Enum 'dxxArrowTails
    Public Enum dxxBubbleTypes
        Circular = 0
        Pill = 1
        Hexagonal = 2
        Rectangular = 3
    End Enum 'dxxBubbleTypes
    Public Enum dxxTableGridStyles
        Undefined = -1
        None = 0
        RowLines = 1
        ColumnLines = 2
        All = 3
    End Enum 'dxxTableGridStyles
    Public Enum dxxAngularUnits
        Undefined = -1
        DegreesDecimal = 0 'Decimal degrees
        DegreesMinSec = 1 'Degrees/minutes/seconds
        Gradians = 2 'Gradians
        Radians = 3 ' Radians
    End Enum 'dxxAngularUnits
    Public Enum dxxDimFractionStyles
        Horizontal = 0
        Diagonal = 1
        NoStack = 2
    End Enum 'dxxDimFractionStyles
    Public Enum dxxDimFit
        <Description("Move Arrows First")> MoveArrowsFirst = 1
        <Description("Move Text First")> MoveTextFirst = 2
        <Description("Best Fit (No Leader)")> BestFitNoLeader = 3
        <Description("Best Fit (with Leader)")> BestFitWithLeader = 4
        <Description("Freeform")> dxfFreeForm = 5
    End Enum 'dxxDimFit
    Public Enum dxxDimTextMovementTypes
        <Description("Moves the dimension line with dimension text")>
        DimLineWithText = 0
        <Description("Adds a leader when dimension text is moved")>
        TextWithLeader = 1
        <Description("Allows text to be moved freely without a leader")>
        FreeWithNoLeader = 2
    End Enum 'dxxDimTextMovementTypes
    Public Enum dxxDimTadSettings
        <Description("Centers the dimension text between the extension lines.")>
        Centered = 0
        <Description("Places the dimension text above the dimension line except when the dimension line is not horizontal and text inside the extension lines is forced horizontal (DIMTIH = 1). The distance from the dimension line to the baseline of the lowest line of text is the current DIMGAP value.")>
        Above = 1
        <Description("Places the dimension text on the side of the dimension line farthest away from the defining points.")>
        OppositeSide = 2
        <Description(" Places the dimension text to conform to Japanese Industrial Standards (JIS).")>
        JIS = 3
    End Enum 'dxxDimTadSettings
    Public Enum dxxDimJustSettings
        <Description("Positions the text above the dimension line and center-justifies it between the extension lines")>
        Centered = 0
        <Description("Positions the text next to the first extension line")>
        Ext1 = 1
        <Description("Positions the text next to the second extension line")>
        Ext2 = 2
        <Description("Positions the text above and aligned with the first extension line")>
        AlignExt1 = 3
        <Description("Positions the text above and aligned with the second extension line")>
        AlignExt2 = 4
    End Enum 'dxxDimJustSettings
    Public Enum dxxArrowHeadTypes
        Various = (-1 * 3)
        Suppressed = (-1 * 2)
        ByStyle = -1
        <Description("_ClosedFilled")> ClosedFilled = 0
        <Description("_ClosedBlank")> ClosedBlank = 1
        <Description("_Closed")> Closed = 2
        <Description("_Dot")> Dot = 3
        <Description("_ArchTick")> ArchTick = 4
        <Description("_Oblique")> Oblique = 5
        <Description("_Open")> Open = 6
        <Description("_Origin")> Origin = 7
        <Description("_Origin2")> Origin2 = 8
        <Description("_Open90")> Open90 = 9
        <Description("_Open30")> Open30 = 10
        <Description("_DotSmall")> DotSmall = 11
        <Description("_Small")> Small = 12
        <Description("_DotBlank")> DotBlank = 13
        <Description("_BoxBlank")> BoxBlank = 14
        <Description("_BoxFilled")> BoxFilled = 15
        <Description("_DatumBlank")> DatumBlank = 16
        <Description("_DataumFilled")> DatumFilled = 17
        <Description("_Integral")> Integral = 18
        <Description("_None")> None = 19
        UserDefined = 20
    End Enum 'dxxArrowHeadTypes
    Public Enum dxxArrowIndicators
        One = 1
        Two = 2
        Leader = 3
    End Enum 'dxxArrowIndicators
    Public Enum dxxZeroSuppressionsArchitectural
        ZeroFeetAndZeroInches = 0
        IncludeZeroFeetAndZeroInches = 1
        IncludeZeroFeetAndSuppressZeroInches = 2
        ZeroFeetAndIncludeZeroInches = 3
    End Enum 'dxxZeroSuppressionsArchitectural
    Public Enum dxxZeroSuppression
        None = 0
        Leading = 4
        Trailing = 8
        LeadingAndTrailing = 12
    End Enum 'dxxZeroSuppression
    Public Enum dxxLinearUnitFormats
        Undefined = -1
        Scientific = 1
        Decimals = 2
        Engineering = 3
        Architectural = 4 '(always displayed stacked)
        Fractional = 5 '(always displayed stacked)
        WindowsDesktop = 6 '(decimal format using Control Panel settings for decimal separator and number grouping symbols)
    End Enum 'dxxLinearUnitFormats
    Public Enum dxxHatchMethods
        Normal = 0
        Outer = 1
        Ignore = 2
    End Enum 'dxxHatchMethods
    Public Enum dxxSides
        Undefined = -1
        Top = 0
        Bottom = 1
        Left = 2
        Right = 3
        Back = 4
        Front = 5
    End Enum 'dxxSides
    Public Enum dxxOrthograpicViews
        Undefined = 0
        Top = 1
        Bottom = 2
        Front = 3
        Back = 4
        Left = 5
        Right = 6
    End Enum 'dxxOrthograpicViews
    Public Enum dxxObjectTypes
        <Description("UNDEFINED")> Undefined = 0
        <Description("ACAD_PROXY_OBJECT")> ProxyObject = 1
        <Description("ACDBDICTIONARYWDFLT")> DictionaryWDFLT = 2
        <Description("ACDBPLACEHOLDER")> PlaceHolder = 3
        <Description("DATATABLE")> DataTable = 4
        <Description("DICTIONARY")> Dictionary = 5
        <Description("DICTIONARYVAR")> DictionaryVar = 6
        <Description("DIMASSOC")> DimAssoc = 7
        <Description("ACAD_FIELD")> Field = 8
        <Description("GROUP")> Group = 9
        <Description("IDBUFFER")> IDBuffer = 10
        <Description("IMAGEDEF")> ImageDef = 11
        <Description("IMAGEDEF_REACTOR")> ImageDefReactor = 12
        <Description("LAYER_INDEX")> LayerIndex = 13
        <Description("LAYER_FILTER")> LayerFilter = 14
        <Description("LAYOUT")> Layout = 15
        <Description("LIGHTLIST")> LightList = 16
        <Description("MATERIAL")> Material = 17
        <Description("MLINESTYLE")> MLineStyle = 18
        <Description("OBJECT_PTR")> ObjectPtr = 19
        <Description("PLOTSETTINGS")> PlotSetting = 20
        <Description("RASTERVARIABLES")> RasterVariables = 21
        <Description("RENDERENVIRONMENT")> RenderEnvironment = 22
        <Description("SECTIONMANAGER")> SectionManager = 23
        <Description("SPATIAL_INDEX")> SpatialIndex = 24
        <Description("SPATIAL_FILTER")> SpatialFilter = 25
        <Description("SORTENTSTABLE")> SortEntsTable = 26
        <Description("SUNSTUDY")> SunStudy = 27
        <Description("TABLESTYLE")> TableStyle = 28
        <Description("UNDERLAYDEFINITION")> UnderlayDefinition = 29
        <Description("VISUALSTYLE")> VisualStyle = 30
        <Description("VBA_PROJECT")> VBAProject = 31
        <Description("WIPEOUTVARIABLES")> WipeoutVariables = 32
        <Description("XRECORD")> XRecord = 33
        <Description("CELLSTYLEMAP")> CellStyleMap = 34
        <Description("SCALE")> Scale = 35
        <Description("TABLECELL")> TableCell = 36
        <Description("MLEADERSTYLE")> MLeaderStyle = 37
        <Description("USERDEFINED")> UserDefined = 38
    End Enum 'dxxObjectTypes
    Public Enum dxxLinetypeLayerFlag
        Undefined = -1
        ForceToLayer = 0
        ForceToColor = 1
        Suppressed = 2
    End Enum 'dxxLinetypeLayerFlag
    Public Enum dxxLineDescripts
        Normal = 0
        Infinite = 2
        InfinitePositive = 3
        InfiniteNegative = 4
    End Enum 'dxxLineDescripts
    Public Enum dxxFileTypes
        Undefined = -1
        DXF = 0
        DWG = 1
        DXT = 2
        DWF = 3
    End Enum 'dxxFileTypes
    Public Enum mzValueControls
        Undefined = -1
        None = 0
        Positive = 1
        Negative = 2
        NonZero = 4
        PositiveNonZero = 5 'Positive + NonZero
        NegativeNonZero = 6  'Negative + NonZero
    End Enum 'mzValueControls
    Public Enum mzRegistryRoots
        HKEY_CLASSES_ROOT = 0 '-2147483648 '&H80000000
        HKEY_CURRENT_USER = 1 '(-1 * 2147483647) '&H80000001
        HKEY_LOCAL_MACHINE = 2 ' (-1 * 2147483646) '&H80000002
        HKEY_USERS = 3 '(-1 * 2147483645) '&H80000003
        HKEY_CURRENT_CONFIG = 4 '(-1 * 2147483643) '&H80000005
        HKEY_DYN_DATA = 5 '(-1 * 2147483642) '&H80000006
        HKEY_PERFORMANCE_DATA = 6 '(-1 * 2147483644) '&H80000004
    End Enum 'mzRegistryRoots

    Friend Enum Printer_Status
        PRINTER_STATUS_PAUSED = &H1
        PRINTER_STATUS_ERROR = &H2
        PRINTER_STATUS_PENDING_DELETION = &H4
        PRINTER_STATUS_PAPER_JAM = &H8
        PRINTER_STATUS_PAPER_OUT = &H10
        PRINTER_STATUS_MANUAL_FEED = &H20
        PRINTER_STATUS_PAPER_PROBLEM = &H40
        PRINTER_STATUS_OFFLINE = &H80
        PRINTER_STATUS_IO_ACTIVE = &H100
        PRINTER_STATUS_BUSY = &H200
        PRINTER_STATUS_PRINTING = &H400
        PRINTER_STATUS_OUTPUT_BIN_FULL = &H800
        PRINTER_STATUS_NOT_AVAILABLE = &H1000
        PRINTER_STATUS_WAITING = &H2000
        PRINTER_STATUS_PROCESSING = &H4000
        PRINTER_STATUS_INITIALIZING = &H8000
        PRINTER_STATUS_WARMING_UP = &H10000
        PRINTER_STATUS_TONER_LOW = &H20000
        PRINTER_STATUS_NO_TONER = &H40000
        PRINTER_STATUS_PAGE_PUNT = &H80000
        PRINTER_STATUS_USER_INTERVENTION = &H100000
        PRINTER_STATUS_OUT_OF_MEMORY = &H200000
        PRINTER_STATUS_DOOR_OPEN = &H400000
    End Enum 'Printer_Status

#End Region 'Enums
End Namespace
