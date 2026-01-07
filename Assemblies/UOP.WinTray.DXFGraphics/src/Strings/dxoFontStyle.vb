Imports UOP.DXFGraphics.Fonts.dxfFonts
Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Public Class dxoFontStyle

        Private _Struc As TFONTSTYLE
#Region "Constructors"
        Friend Sub New()
            _Struc = New TFONTSTYLE("", 0)
            _FontInfo = New TFONTINFO()
        End Sub
        Friend Sub New(aStructure As TFONTSTYLE)
            _Struc = New TFONTSTYLE(aStructure)
            _FontInfo = New TFONTINFO()
        End Sub


#End Region 'Constructors

#Region "Properties"

        Public ReadOnly Property FileName As String
            Get
                Return _Struc.FileName
            End Get
        End Property

        Private _FontInfo As TFONTINFO

        Friend Property FontInfo As TFONTINFO
            Get
                Return _FontInfo
            End Get
            Set(value As TFONTINFO)
                _FontInfo = value
            End Set
        End Property

        Friend Property Struckture As TFONTSTYLE
            Get
                Return _Struc
            End Get
            Set(value As TFONTSTYLE)
                _Struc = value
            End Set
        End Property

        Public ReadOnly Property FontName As String
            Get
                Return _Struc.FontName
            End Get
        End Property

        Public ReadOnly Property FontIndex As String
            Get
                Return _Struc.FontIndex
            End Get
        End Property



        Public Property StyleIndex As Integer
            Get
                Return _Struc.StyleIndex
            End Get
            Friend Set(value As Integer)
                _Struc.StyleIndex = value
            End Set
        End Property

        Public ReadOnly Property Ascent
            Get
                Return _Struc.Ascent
            End Get
        End Property

        Public ReadOnly Property Descent
            Get
                Return _Struc.Descent
            End Get
        End Property

        Public ReadOnly Property DescentFactor
            Get
                Return _Struc.DescentFactor
            End Get
        End Property

        Public ReadOnly Property CellHeight
            Get
                Return _Struc.CellHeight
            End Get
        End Property


        Public Property TTFStyle As dxxTextStyleFontSettings
            Get
                Return _Struc.TTFStyle
            End Get
            Set(value As dxxTextStyleFontSettings)
                _Struc.TTFStyle = value
            End Set
        End Property

        Public ReadOnly Property Bold As Boolean
            Get
                Return TTFStyle = dxxTextStyleFontSettings.Bold Or TTFStyle = dxxTextStyleFontSettings.BoldItalic

            End Get
        End Property

        Public ReadOnly Property Italic As Boolean
            Get
                Return TTFStyle = dxxTextStyleFontSettings.Italic Or TTFStyle = dxxTextStyleFontSettings.BoldItalic
            End Get
        End Property

        Public ReadOnly Property StyleName As String
            Get
                Return dxfEnums.Description(TTFStyle)
            End Get
        End Property
#End Region 'Properties

#Region "Methods"
        Public Overrides Function ToString() As String
            Dim _rVal As String = FontName
            If Not String.IsNullOrWhiteSpace(StyleName) Then
                If Not String.IsNullOrWhiteSpace(_rVal) Then _rVal += $" - { StyleName}" Else _rVal = StyleName
            End If
            _rVal = $"dxoFontStyle [{ _rVal }]"
            Return _rVal

        End Function
#End Region 'Methods
    End Class

End Namespace
