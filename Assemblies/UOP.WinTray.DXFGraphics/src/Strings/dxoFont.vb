Imports UOP.DXFGraphics.Fonts.dxfFonts
Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics

    Public Class dxoFont

        Private _Found As Boolean
        Private _ErrorString As String = String.Empty
        Private _FontType As dxxFontTypes
        Private _FontStructure As TFONT
        Private _Styles As List(Of dxoFontStyle)
#Region "Constructors"

        Friend Sub New()
            _Found = False
            _ErrorString = ""
            _FontType = dxxFontTypes.Undefined
            _Styles = New List(Of dxoFontStyle)
        End Sub

        Friend Sub New(aFont As TFONT)
            _Found = False
            _ErrorString = ""
            _FontType = dxxFontTypes.Undefined
            _FontStructure = aFont
            _FontType = aFont.FontType
            _Styles = New List(Of dxoFontStyle)
            For i As Integer = 1 To aFont.StyleArray.Count
                _Styles.Add(New dxoFontStyle(aFont.StyleArray.Item(i)))
            Next
        End Sub


        Friend Sub New(aFont As TFONT, Styles As TFONTSTYLES)
            _Found = False
            _ErrorString = ""
            _FontType = dxxFontTypes.Undefined
            _FontStructure = aFont
            _FontType = aFont.FontType
            _Styles = New List(Of dxoFontStyle)
            For i As Integer = 1 To Styles.Count
                _Styles.Add(New dxoFontStyle(Styles.Item(i)))
            Next
        End Sub
#End Region 'Constructors
#Region "Properties"

        Private _DefaultStyle As dxoFontStyle
        Public ReadOnly Property DefaultStyle As dxoFontStyle
            Get
                If _DefaultStyle Is Nothing Then
                    If _Styles Is Nothing Then Return Nothing
                    If _Styles.Count <= 0 Then Return Nothing
                    _DefaultStyle = _Styles.Find(Function(x) x.TTFStyle = dxxTextStyleFontSettings.Regular)
                    If _DefaultStyle Is Nothing Then _DefaultStyle = _Styles.FirstOrDefault()
                End If

                Return _DefaultStyle
            End Get

        End Property

        Friend Property Styles As List(Of dxoFontStyle)
            Get
                Return _Styles
            End Get
            Set(value As List(Of dxoFontStyle))
                _Styles = value
                Dim tfont As TFONT = FontStructure
                tfont.StyleArray = New TFONTSTYLES(Family)
                If Not value Is Nothing Then
                    For Each sty As dxoFontStyle In value
                        tfont.AddStyle(sty.Struckture)
                    Next
                End If
                _FontStructure = tfont
            End Set
        End Property
        Public Property FontType As dxxFontTypes
            Get
                Return _FontType
            End Get
            Friend Set(value As dxxFontTypes)
                _FontType = value
            End Set
        End Property
        Public Property Index As Integer
            Get
                Return _FontStructure.Index
            End Get
            Friend Set(value As Integer)
                _FontStructure.Index = value
            End Set
        End Property
        Friend Property FontStructure As TFONT
            Get
                Return _FontStructure
            End Get
            Set(value As TFONT)
                _FontStructure = value
            End Set
        End Property
        Public ReadOnly Property Loaded As Boolean
            Get
                Return Index > 0
            End Get
        End Property
        Public Property Name As String
            Get
                Return _FontStructure.Name
            End Get
            Friend Set(value As String)
                _FontStructure.Name = value
            End Set
        End Property
        Public Property Family As String
            Get
                Return _FontStructure.FamilyName
            End Get
            Friend Set(value As String)
                _FontStructure.FamilyName = value
            End Set
        End Property
        Public ReadOnly Property IsShape As Boolean
            Get
                Return FontType = dxxFontTypes.Shape Or FontType = dxxFontTypes.Embedded
            End Get
        End Property
        Public ReadOnly Property StyleNames As List(Of String)
            Get
                Dim _rVal As New List(Of String)
                If IsShape Then
                    _rVal.Add(dxfEnums.Description(dxxTextStyleFontSettings.Regular))
                    Return _rVal
                End If
                For i As Integer = 1 To _FontStructure.StyleArray.Count
                    _rVal.Add(_FontStructure.StyleArray.Item(i).StyleName)
                Next
                Return _rVal
            End Get
        End Property
        Public Function Style(aStyleName As String) As dxoFontStyle
            If String.IsNullOrWhiteSpace(aStyleName) Then Return Nothing Else aStyleName = aStyleName.Trim
            For i As Integer = 1 To _FontStructure.StyleArray.Count
                If String.Compare(aStyleName, _FontStructure.StyleArray.Item(i).StyleName, True) = 0 Then
                    Return New dxoFontStyle(_FontStructure.StyleArray.Item(i))
                End If
            Next
            Return Nothing
        End Function
        Public Function Style(aStyleIndex As Integer) As dxoFontStyle
            If aStyleIndex < 1 Or aStyleIndex > _FontStructure.StyleArray.Count Then Return Nothing
            Return New dxoFontStyle(_FontStructure.StyleArray.Item(aStyleIndex))
        End Function
        Public Property ErrorString As String
            Get
                Return _ErrorString
            End Get
            Friend Set(value As String)
                _ErrorString = value
            End Set
        End Property
        Public Property Found As Boolean
            Get
                Return _Found
            End Get
            Friend Set(value As Boolean)
                _Found = value
            End Set
        End Property
#End Region 'Properties
#Region "Methods"

        Public Overrides Function ToString() As String
            Return Name
        End Function
        Friend Function GetStyleStructure(aStyleName As String, Optional bReturnDefaultIfNotFound As Boolean = True) As TFONTSTYLE
            Dim _rVal As New TFONTSTYLE("", 0)
            If IsShape Then
                _rVal = _FontStructure.StyleArray.Item(1)
            Else
                If _Styles Is Nothing Then Return _rVal
                If _Styles.Count <= 0 Then Return _rVal
                Dim defstyle As String = dxfEnums.Description(dxxTextStyleFontSettings.Regular)
                If String.IsNullOrWhiteSpace(aStyleName) Then aStyleName = defstyle
                Dim idx As Integer = _Styles.FindIndex(Function(x) String.Compare(x.StyleName, aStyleName, True) = 0)
                If idx < 0 And bReturnDefaultIfNotFound And String.Compare(aStyleName, defstyle, True) <> 0 Then
                    aStyleName = defstyle
                    idx = _Styles.FindIndex(Function(x) String.Compare(x.StyleName, aStyleName, True) = 0)
                End If
                If idx < 0 And bReturnDefaultIfNotFound Then
                    idx = 0
                Else
                    Dim style As dxoFontStyle = _Styles.Item(idx)
                    style.StyleIndex = idx + 1
                    _rVal = style.Struckture
                End If
            End If
            Return _rVal
        End Function
#End Region 'Methods
    End Class
End Namespace
