Imports System.IO
Imports UOP.DXFGraphics.Fonts.dxfFonts
Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public Class dxoStyle
        Inherits dxfTableEntry

        Implements ICloneable
#Region "Members"
        Private bSuppressEvents As Boolean
#End Region 'Members
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxReferenceTypes.STYLE)
        End Sub
        Friend Sub New(aEntry As TTABLEENTRY)
            MyBase.New(dxxReferenceTypes.STYLE, aEntry.Name, aGUID:=aEntry.GUID)
            If aEntry.EntryType = dxxReferenceTypes.STYLE Then Properties.CopyVals(aEntry)
        End Sub
        Public Sub New(aName As String)
            MyBase.New(dxxReferenceTypes.STYLE, aName)
        End Sub
        Public Sub New(aEntry As dxoStyle)
            MyBase.New(aEntry)
        End Sub

#End Region 'Constructors
#Region "dxfHandleOwner"
        Friend Overrides Property Suppressed As Boolean
            Get
                Return False
            End Get
            Set(value As Boolean)
                value = False
            End Set
        End Property
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Throw New NotImplementedException
        End Function
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Throw New NotImplementedException
        End Function
        Public Overrides ReadOnly Property FileObjectType As dxxFileObjectTypes
            Get
                Return dxxFileObjectTypes.TableEntry
            End Get
        End Property
#End Region 'dxfHandleOwner
#Region "Properties"
        Public Property Backwards As Boolean
            Get
                Return PropValueB(dxxStyleProperties.BACKWARDS)
            End Get
            Set(value As Boolean)

                PropValueSet(dxxStyleProperties.BACKWARDS, TPROPERTY.SwitchValue(value))

            End Set
        End Property

        Public ReadOnly Property FaceName As String
            Get
                '^the font file name less the extension
                Return Path.GetFileNameWithoutExtension(FontName)
            End Get
        End Property

        Public Property FontName As String
            Get
                Return PropValueStr(dxxStyleProperties.FONTNAME)
            End Get
            Set(value As String)
                UpdateFontName(value, "")
            End Set
        End Property

        Public ReadOnly Property FontFileName As String
            Get
                Return Properties.ValueS("*FontFileName")
            End Get
        End Property

        Friend Property FontIndex As Integer
            Get
                Return PropValueI(dxxStyleProperties.FONTINDEX)
            End Get
            Set(value As Integer)
                PropValueSet(dxxStyleProperties.FONTINDEX, value)
            End Set
        End Property

        Public Property FontStyle As String
            Get
                Return PropValueStr(dxxStyleProperties.FONTSTYLE)
            End Get
            Set(value As String)
                UpdateFontName(FontName, value)
            End Set
        End Property

        Public Property FontStyleType As dxxTextStyleFontSettings
            Get
                Return PropValueI(dxxStyleProperties.FONTSTYLETYPE)
            End Get
            Set(value As dxxTextStyleFontSettings)
                UpdateFontName(FontName, value)
            End Set
        End Property


        Public Property IsShape As Boolean
            Get
                Return PropValueB(dxxStyleProperties.SHAPEFLAG)
            End Get
            Set(value As Boolean)
                PropValueSet(dxxStyleProperties.SHAPEFLAG, value)
            End Set
        End Property
        Public Property LastHeight As Double
            Get
                '^the last height used by a text object using this text style
                '~default = 0.2.
                '~this property not actively maintained just included for support
                '~in AutoCAD.
                Return PropValueD(dxxStyleProperties.LASTHT)
            End Get
            Set(value As Double)
                '^the last height used by a text object using this text style
                '~default = 0.2.
                '~this property not actively maintained just included for support
                '~in AutoCAD.
                PropValueSet(dxxStyleProperties.LASTHT, value)
            End Set
        End Property
        Public Property LineSpacingFactor As Double
            Get
                '^all Text created under this style inherits this factor
                '~max = 4 min = 0.25
                Return PropValueD(dxxStyleProperties.LINESPACING)
            End Get
            Set(value As Double)
                '^all Text created under this style inherits this factor
                '~max = 4 min = 0.25
                If value < 0.25 Then value = 0.25
                If value > 4 Then value = 4
                PropValueSet(dxxStyleProperties.LINESPACING, value)
            End Set
        End Property
        Public Property LineSpacingStyle As dxxLineSpacingStyles
            Get
                '^all MText created under this style inherits this style
                Return PropValueI(dxxStyleProperties.LINESPACINGSTYLE)
            End Get
            Set(value As dxxLineSpacingStyles)
                '^all MText created under this style inherits this style
                If value < dxxLineSpacingStyles.AtLeast Or value > dxxLineSpacingStyles.Exact Then Return
                PropValueSet(dxxStyleProperties.LINESPACINGSTYLE, value)
            End Set
        End Property
        Public Property ObliqueAngle As Double
            Get
                Return PropValueD(dxxStyleProperties.OBLIQUE)
            End Get
            Set(value As Double)
                value = TVALUES.ObliqueAngle(value)
                PropValueSet(dxxStyleProperties.OBLIQUE, value)
            End Set
        End Property
        Public Property ShapeStyle As Boolean
            Get
                Return PropValueB(dxxStyleProperties.SHAPESTYLEFLAG)
            End Get
            Set(value As Boolean)
                PropValueSet(dxxStyleProperties.SHAPESTYLEFLAG, value)
            End Set
        End Property
        Public Property SuppressEvents As Boolean
            Get
                Return bSuppressEvents
            End Get
            Set(value As Boolean)
                bSuppressEvents = value
            End Set
        End Property
        Public Property TextHeight As Double
            Get
                '^the height of text defined using this style.
                Return PropValueD(dxxStyleProperties.TEXTHT)
            End Get
            Set(value As Double)
                '^the height of text defined using this style.
                PropValueSet(dxxStyleProperties.TEXTHT, value)
            End Set
        End Property
        Friend ReadOnly Property TypeFlag As Integer
            Get
                '^the entity type flag used in DXF code generation
                '~2 indicates a TTF font is being used 0 means SHX font
                Dim _rVal As Integer = 0
                Dim fnt As String = FontName.ToUpper

                If Not fnt.EndsWith(".SHX") And Not fnt.EndsWith(".SHP") Then
                    _rVal = 2
                End If
                If Vertical Then _rVal += 4
                Return _rVal
            End Get
        End Property
        Public Property UpsideDown As Boolean
            Get
                Return PropValueB(dxxStyleProperties.UPSIDEDOWN)
            End Get
            Set(value As Boolean)
                PropValueSet(dxxStyleProperties.UPSIDEDOWN, TPROPERTY.SwitchValue(value))
            End Set
        End Property
        Public Property Vertical As Boolean
            Get
                Return PropValueB(dxxStyleProperties.VERTICAL)
            End Get
            Set(value As Boolean)

                PropValueSet(dxxStyleProperties.VERTICAL, TPROPERTY.SwitchValue(value))
            End Set
        End Property
        Public Property WidthFactor As Double
            Get
                '^the width factor applied to text defined using this style.
                '~default = 1
                Return PropValueD(dxxStyleProperties.WIDTHFACTOR)
            End Get
            Set(value As Double)
                '^the width factor applied to text defined using this style.
                If value < 0.1 Then value = 0.1
                If value > 10 Then value = 10
                '~default = 1
                PropValueSet(dxxStyleProperties.WIDTHFACTOR, value)
            End Set
        End Property
        Public Property XRefDependant As Boolean
            Get
                Return PropValueB(dxxStyleProperties.XREFDEPENANT)
            End Get
            Set(value As Boolean)
                PropValueSet(dxxStyleProperties.XREFDEPENANT, value)
            End Set
        End Property
        Public Property XRefResolved As Boolean
            Get
                Return PropValueB(dxxStyleProperties.XREFRESOLVED)
            End Get
            Set(value As Boolean)
                PropValueSet(dxxStyleProperties.XREFRESOLVED, value)
            End Set
        End Property
#End Region 'Properties
#Region "Methods"
        Public Function CreateText(aTextString As String, aTextType As dxxTextTypes, aDirectionFlag As dxxTextDrawingDirections, Optional aTextHeight As Double = 0.0, Optional aAlignment As dxxMTextAlignments = dxxMTextAlignments.AlignUnknown, Optional aWidthFactor As Double = 0.0, Optional aAngle As Double = 0.0, Optional aObliqueAngle As Object = Nothing, Optional aImage As dxfImage = Nothing, Optional aPlane As dxfPlane = Nothing) As dxeText
            If Not GetImage(aImage) Then Return Nothing
            Return aImage.CreateText(Me, aTextString, aTextType, aDirectionFlag, aTextHeight, aAlignment, aWidthFactor, aAngle, aObliqueAngle, aPlane)
        End Function
        Friend Function IsEqual(ByRef Style As dxoStyle, Optional TestName As Boolean = True, Optional TestWidthFactor As Boolean = True) As Boolean
            Dim _rVal As Boolean
            '#1the TextStyle to test
            '#2flag to use Name as a matching criteria
            '^Test to see if the passed TextStyle is equal
            If Style Is Nothing Then Return _rVal
            _rVal = True
            If String.Compare(Style.Name, Name, True) <> 0 Then _rVal = False
            If _rVal Then
                If Style.Vertical <> Vertical Then _rVal = False
            End If
            If _rVal Then
                If Style.Backwards <> Backwards Then _rVal = False
            End If
            If _rVal Then
                If Style.UpsideDown <> Style.UpsideDown Then _rVal = False
            End If
            If _rVal Then
                If String.Compare(Style.FontName, FontName, True) <> 0 Then _rVal = False
            End If
            If TestWidthFactor And _rVal Then
                If Style.WidthFactor <> WidthFactor Then _rVal = False
            End If
            Return _rVal
        End Function
        Public Sub Reset()
            Properties.CopyVals(New TTABLEENTRY(dxxReferenceTypes.STYLE, Name))
        End Sub
        Public Function SelectFont(Optional aOwnerForm As IWin32Window = Nothing) As Boolean
            Dim _rVal As Boolean
            '^show the form to select the font to assign to the text style
            Dim fname As String
            Dim bCanceled As Boolean
            fname = goFontForm.SelectFont(FontName, FontStyle, rCanceled:=bCanceled, aOwner:=aOwnerForm)
            If Not bCanceled Then FontName = fname
            Return _rVal
        End Function
        Public Function GetFontStyle() As dxoFontStyle
            Dim font As dxoFont = dxfGlobals.GetFont(FontName, "Arial.ttf", True)
            Return dxoFonts.Find(FontName).Style(FontStyle)
        End Function
        Public Function UpdateFontName(aFontName As String, aFontStyle As String) As Boolean
            Dim rName As String = String.Empty
            Dim rStyle As String = String.Empty
            Return UpdateFontName(aFontName, aFontStyle, rName, rStyle)
        End Function
        Public Function UpdateFontName(aFontName As String, aFontStyle As String, ByRef rName As String, ByRef rStyle As String) As Boolean
            Dim rOldValue As String = String.Empty
            Dim rNewValue As String = String.Empty
            Return UpdateFontName(aFontName, aFontStyle, rName, rStyle, rOldValue, rNewValue)
        End Function

        Public Function UpdateFontName(aFontName As String, aFontStyle As String, ByRef rName As String, ByRef rStyleName As String, ByRef rOldValue As String, ByRef rNewValue As String) As Boolean
            rName = PropValueStr(dxxStyleProperties.FONTNAME)
            rStyleName = PropValueStr(dxxStyleProperties.FONTSTYLE)
            rOldValue = ""
            rNewValue = ""
            If String.IsNullOrWhiteSpace(aFontName) Then aFontName = rName
            If String.IsNullOrWhiteSpace(aFontName) Then Return "Txt.shx"
            aFontName = aFontName.Trim
            If aFontStyle IsNot Nothing Then aFontStyle = aFontStyle.Trim() Else aFontStyle = rStyleName

            Dim fnt As String = aFontName
            Dim sty As String = aFontStyle
            Dim i As Integer = aFontName.IndexOf(";") + 1
            Dim newstyle As TFONTSTYLE
            If aFontName.Contains(";") Then
                sty = dxfUtils.RightOf(aFontName, ";")
                fnt = dxfUtils.LeftOf(aFontName, ";")
            End If
            Dim ext As String = IO.Path.GetExtension(fnt).ToLower()
            Dim font As dxoFont = dxfGlobals.GetFont(fnt, "", True, bReturnDefault:=False)
            If font Is Nothing And ext = ".ttf" Then
                Dim tryit As String = aFontStyle.Trim
                tryit = $"{fnt.Substring(0, fnt.Length - 4)} {tryit}{ext}"

                font = dxfGlobals.GetFont(tryit, rName, True)

            End If

            If font Is Nothing Then Return False
            newstyle = font.GetStyleStructure(sty, True)
            If fnt.Contains(".") Then
                ext = IO.Path.GetExtension(fnt)
                fnt = IO.Path.GetFileNameWithoutExtension(fnt)

                aFontName = $"{ fnt}{ext}"
            End If
            Dim bIsShape As Boolean
            bIsShape = PropValueB(dxxStyleProperties.SHAPEFLAG)
            rOldValue = $"{rName};{ rStyleName };{ bIsShape}"
            rNewValue = rOldValue
            Dim fInfo As TFONTSTYLEINFO = dxoFonts.GetFontStyleInfo(aFontName, aStyleName:=newstyle.StyleName)
            aFontName = fInfo.FontName
            aFontStyle = fInfo.StyleName
            Dim rChanged As Boolean = PropValueSet(dxxStyleProperties.FONTINDEX, fInfo.FontIndex)
            If String.Compare(aFontName, rName, ignoreCase:=True) <> 0 Or String.Compare(aFontStyle, rStyleName, ignoreCase:=True) <> 0 Or bIsShape <> fInfo.IsShape Then rChanged = True
            If Not rChanged Then Return False
            rName = fInfo.FontName
            rStyleName = fInfo.StyleName
            Dim prop1 As dxoProperty = PropByEnum(dxxStyleProperties.FONTNAME)
            Dim prop2 As dxoProperty = PropByEnum(dxxStyleProperties.FONTSTYLE)
            Dim prop3 As dxoProperty = PropByEnum(dxxStyleProperties.FONTSTYLETYPE)
            Dim prop4 As dxoProperty = PropByEnum(dxxStyleProperties.SHAPEFLAG)
            Dim prop5 As dxoProperty = Prop("*FontFileName")
            Dim prop6 As dxoProperty = PropByEnum(dxxStyleProperties.VERTICAL)

            Dim prop_1 As String = prop1.ValueS
            Dim prop_2 As String = prop2.ValueS
            Dim prop_3 As Object = prop3.Value
            Dim prop_4 As Object = prop4.Value
            Dim prop_5 As String = prop5.ValueS
            Dim prop_6 As Object = prop6.Value
            If prop1.SetVal(rName) Then rChanged = True
            If prop2.SetVal(rStyleName) Then rChanged = True
            If prop3.SetVal(fInfo.TTFStyle) Then rChanged = True
            If prop4.SetVal(TPROPERTY.SwitchValue(fInfo.IsShape)) Then rChanged = True
            If prop5.SetVal(newstyle.FileName) Then rChanged = True
            If rChanged And Not fInfo.IsShape Then
                If prop6.SetVal(False) Then rChanged = True
            End If
            rNewValue = $"{rName};{ rStyleName };{ fInfo.IsShape}"
            If rChanged Then
                Dim aImage As dxfImage = Nothing
                Dim undo As Boolean
                rChanged = Notify(prop5, prop_5, aImage, undo)
                If undo Then
                    rChanged = False
                    rName = prop_1
                    rStyleName = prop_2
                    prop1.SetVal(prop_1)
                    prop2.SetVal(prop_2)
                    prop3.SetVal(prop_3)
                    prop4.SetVal(prop_4)
                    prop5.SetVal(prop_5)
                    prop5.SetVal(prop_6)

                End If
                Return rChanged
            Else
                Return False
            End If


        End Function

        Friend Function GetFontStyleInfo() As TFONTSTYLEINFO
            Return dxoFonts.GetFontStyleInfo(FontName, FontIndex, FontStyle)
        End Function


        Friend Overrides Function PropValueSetByName(aName As String, aValue As Object, Optional aOccur As Integer = 0, Optional bSuppressEvnts As Boolean = False) As Boolean
            If aValue Is Nothing Or String.IsNullOrWhiteSpace(aName) Then Return False
            'reviewv changes for validity before proceding
            Select Case aName.ToUpper()
                Case "a property name here"

            End Select
            Return MyBase.PropValueSetByName(aName, aValue, aOccur, bSuppressEvnts)

        End Function

        Public Shadows Function Clone() As dxoStyle
            Return New dxoStyle(Me)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return Clone()
        End Function
#End Region 'Methods
    End Class 'dxoStyle
End Namespace
