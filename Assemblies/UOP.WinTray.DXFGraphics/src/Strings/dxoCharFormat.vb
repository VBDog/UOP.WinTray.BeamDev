Imports UOP.DXFGraphics.Fonts.dxfFonts
Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public Class dxoCharFormat
        Implements ICloneable


#Region "Constructors"
        Public Sub New(Optional aAligment As dxxCharacterAlignments = dxxCharacterAlignments.Bottom)
            Init()
            CharAlign = aAligment


        End Sub

        Friend Sub New(aFormats As TCHARFORMAT)
            Init()
            Backwards = aFormats.Backwards
            CharAlign = aFormats.CharAlign
            CharHeight = aFormats.CharHeight
            Color = aFormats.Color
            FontIndex = aFormats.FontIndex
            IsShape = aFormats.IsShape
            Overline = aFormats.Overline
            Rotation = aFormats.Rotation
            StackBelow = aFormats.StackBelow
            StackID = aFormats.StackID
            StackStyle = aFormats.StackStyle
            StyleIndex = aFormats.StyleIndex
            Underline = aFormats.Underline
            UpsideDown = aFormats.UpsideDown
            Vertical = aFormats.Vertical
            WidthFactor = aFormats.WidthFactor
            HeightFactor = aFormats.HeightFactor
            StrikeThru = aFormats.StrikeThru
            _Tracking = aFormats.Tracking
            ObliqueAngle = aFormats.ObliqueAngle
            TextStyleName = aFormats.TextStyleName
        End Sub

        Friend Sub New(aFormats As dxoCharFormat)
            Init()
            If aFormats Is Nothing Then Return

            Backwards = aFormats.Backwards
            CharAlign = aFormats.CharAlign
            CharHeight = aFormats.CharHeight
            Color = aFormats.Color
            FontIndex = aFormats.FontIndex
            IsShape = aFormats.IsShape
            Overline = aFormats.Overline
            Rotation = aFormats.Rotation
            StackBelow = aFormats.StackBelow
            StackID = aFormats.StackID
            StackStyle = aFormats.StackStyle
            StyleIndex = aFormats.StyleIndex
            Underline = aFormats.Underline
            UpsideDown = aFormats.UpsideDown
            Vertical = aFormats.Vertical
            WidthFactor = aFormats.WidthFactor
            HeightFactor = aFormats.HeightFactor
            StrikeThru = aFormats.StrikeThru
            Tracking = aFormats.Tracking
            ObliqueAngle = aFormats.ObliqueAngle
            TextStyleName = aFormats.TextStyleName
        End Sub

        Private Sub Init()
            Backwards = False
            CharAlign = dxxCharacterAlignments.Bottom
            CharHeight = 1
            Color = dxxColors.BlackWhite
            FontIndex = 0
            IsShape = False
            Overline = False
            Rotation = 0
            StackBelow = False
            StackID = 0
            StackStyle = dxxCharacterStackStyles.None
            StyleIndex = 0
            Underline = False
            UpsideDown = False
            Vertical = False
            WidthFactor = 1
            HeightFactor = 1
            TextStyleName = ""
            StrikeThru = False
            _Tracking = 1
            _ObliqueAngle = 0
        End Sub
#End Region 'Constructors

#Region "Properties"


        Private _CharHeight As Double
        Public Property CharHeight As Double
            Get
                Return _CharHeight
            End Get
            Friend Set(value As Double)
                _CharHeight = Math.Abs(value)
            End Set
        End Property

        Private _Rotation As Double
        Public Property Rotation As Double
            Get
                Return _Rotation
            End Get
            Friend Set(value As Double)
                _Rotation = dxfUtils.NormalizeAngle(value, False, True, True)
            End Set
        End Property

        Private _FontIndex As Integer
        Public Property FontIndex As Integer
            Get
                Return _FontIndex
            End Get
            Friend Set(value As Integer)
                _FontIndex = value
            End Set
        End Property

        Private _CharAlign As dxxCharacterAlignments
        Public Property CharAlign As dxxCharacterAlignments
            Get
                Return _CharAlign
            End Get
            Friend Set(value As dxxCharacterAlignments)
                _CharAlign = value
            End Set
        End Property

        Private _Backwards As Boolean
        Public Property Backwards As Boolean
            Get
                Return _Backwards
            End Get
            Friend Set(value As Boolean)
                _Backwards = value
            End Set
        End Property

        Private _TextStyleName As String
        Public Property TextStyleName As String
            Get
                Return _TextStyleName
            End Get
            Friend Set(value As String)
                _TextStyleName = value
            End Set
        End Property

        Private _IsShape As Boolean
        Public Property IsShape As Boolean
            Get
                Return _IsShape
            End Get
            Friend Set(value As Boolean)
                _IsShape = value
            End Set
        End Property

        Private _Overline As Boolean
        Public Property Overline As Boolean
            Get
                Return _Overline
            End Get
            Friend Set(value As Boolean)
                _Overline = value
            End Set
        End Property

        Private _StackBelow As Boolean
        Public Property StackBelow As Boolean
            Get
                Return _StackBelow
            End Get
            Friend Set(value As Boolean)
                _StackBelow = value
            End Set
        End Property
        Private _StackID As Integer

        Public Property StackID As Integer
            Get
                Return _StackID
            End Get
            Friend Set(value As Integer)
                _StackID = value
            End Set
        End Property

        Private _StackStyle As dxxCharacterStackStyles
        Public Property StackStyle As dxxCharacterStackStyles
            Get
                Return _StackStyle
            End Get
            Friend Set(value As dxxCharacterStackStyles)
                _StackStyle = value
            End Set
        End Property

        Private _StyleIndex As Integer

        Public Property StyleIndex As Integer
            Get
                Return _StyleIndex
            End Get
            Friend Set(value As Integer)
                _StyleIndex = value
            End Set
        End Property

        Private _Underline As Boolean
        Public Property Underline As Boolean
            Get
                Return _Underline
            End Get
            Friend Set(value As Boolean)
                _Underline = value
            End Set
        End Property

        Private _UpsideDown As Boolean
        Public Property UpsideDown As Boolean
            Get
                Return _UpsideDown
            End Get
            Friend Set(value As Boolean)
                _UpsideDown = value
            End Set
        End Property


        Private _Vertical As Boolean
        Public Property Vertical As Boolean
            Get
                Return _Vertical
            End Get
            Friend Set(value As Boolean)
                _Vertical = value
            End Set
        End Property


        Private _WidthFactor As Double
        Public Property WidthFactor As Double
            Get
                Return _WidthFactor
            End Get
            Friend Set(value As Double)
                _WidthFactor = TVALUES.LimitedValue(value, 0.1, 10)
            End Set
        End Property

        Private _HeightFactor As Double
        Public Property HeightFactor As Double
            Get
                Return _HeightFactor
            End Get
            Friend Set(value As Double)
                _HeightFactor = Math.Abs(value)
            End Set
        End Property


        Private _StrikeThru As Boolean
        Public Property StrikeThru As Boolean
            Get
                Return _StrikeThru
            End Get
            Friend Set(value As Boolean)
                _StrikeThru = value
            End Set
        End Property

        Private _Color As dxxColors
        Public Property Color As dxxColors
            Get
                Return _Color
            End Get
            Friend Set(value As dxxColors)
                _Color = value
            End Set
        End Property

        Friend Property FontStyleInfo As TFONTSTYLEINFO
            Get
                Dim _rVal As New TFONTSTYLEINFO With {.FontIndex = FontIndex, .StyleIndex = StyleIndex, .IsShape = IsShape}
                Return _rVal
            End Get
            Set(value As TFONTSTYLEINFO)
                FontIndex = value.FontIndex
                StyleIndex = value.StyleIndex
                IsShape = value.IsShape
                If Tracking <= 0 Then Tracking = 1
                If WidthFactor <= 0 Then WidthFactor = 1
            End Set
        End Property

        Private _ObliqueAngle As Double
        Public Property ObliqueAngle As Double
            Get
                Return _ObliqueAngle
            End Get
            Set(value As Double)
                _ObliqueAngle = TVALUES.ObliqueAngle(value)
            End Set
        End Property

        Private _Tracking As Double

        Public Property Tracking As Double
            Get
                If _Tracking <= 0 Then _Tracking = 1
                Return _Tracking
            End Get
            Set(value As Double)

                If _Tracking <= 0 Then _Tracking = 1
                If value = 0 Then Return

                _Tracking = TVALUES.LimitedValue(Math.Abs(value), 0.75, 4)
            End Set
        End Property

        Public ReadOnly Property IsTrueType As Boolean
            Get
                Return Not IsShape
            End Get
        End Property

#End Region 'Properties

#Region "Methods"

        Friend Function UpdateFontStyleInfo() As TFONTSTYLEINFO
            If FontIndex <= 0 Then
                FontStyleInfo = dxoFonts.GetFontStyleInfo(aFontName:="Txt.shx", aStyleIndex:=1)
            Else
                FontStyleInfo = dxoFonts.GetFontStyleInfo(aIndex:=FontIndex, aStyleIndex:=StyleIndex)
                If FontIndex <= 0 Then FontStyleInfo = dxoFonts.GetFontStyleInfo(aFontName:="Txt.shx", aStyleIndex:=1)
            End If

            Return FontStyleInfo



        End Function
        Friend Function Structure_Get() As TCHARFORMAT

            Return New TCHARFORMAT() With
            {
            .Backwards = Backwards,
            .CharAlign = CharAlign,
            .CharHeight = CharHeight,
            .Color = Color,
            .FontIndex = FontIndex,
            .IsShape = IsShape,
            .Overline = Overline,
            .Rotation = Rotation,
            .StackBelow = StackBelow,
            .StackID = StackID,
            .StackStyle = StackStyle,
            .StyleIndex = StyleIndex,
            .Underline = Underline,
            .UpsideDown = UpsideDown,
            .Vertical = Vertical,
            .WidthFactor = WidthFactor,
            .HeightFactor = HeightFactor,
            .StrikeThru = StrikeThru,
            .Tracking = Tracking,
            .ObliqueAngle = ObliqueAngle
                }

        End Function

        Public Function Clone() As dxoCharFormat
            Return New dxoCharFormat With
            {
            .Backwards = Backwards,
        .CharAlign = CharAlign,
            .CharHeight = CharHeight,
            .Color = Color,
            .FontIndex = FontIndex,
            .IsShape = IsShape,
            .Overline = Overline,
            .Rotation = Rotation,
            .StackBelow = StackBelow,
            .StackID = StackID,
            .StackStyle = StackStyle,
            .StyleIndex = StyleIndex,
            .Underline = Underline,
            .UpsideDown = UpsideDown,
            .Vertical = Vertical,
            .WidthFactor = WidthFactor,
            .HeightFactor = HeightFactor,
            .StrikeThru = StrikeThru,
            .Tracking = Tracking,
            .ObliqueAngle = ObliqueAngle,
            .TextStyleName = TextStyleName
            }

        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return Clone()
        End Function

#End Region  'Methods
    End Class
End Namespace
