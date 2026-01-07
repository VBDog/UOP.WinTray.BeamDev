Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Public Class dxoSettingsDim
        Implements idxfSettingsObject
#Region "Members"
        Private _Struc As TTABLEENTRY
#End Region 'Members
#Region "Constructors"
        Friend Sub New(aStructure As TTABLEENTRY)
            _Struc = aStructure
        End Sub
#End Region 'Constructors
#Region "Properties"
        Public Property Properties As dxoProperties Implements idxfSettingsObject.Properties
            Get
                Return New dxoProperties(_Struc.Props)
            End Get
            Set(value As dxoProperties)
                _Struc.Props.CopyValues(value)
            End Set
        End Property

        Public ReadOnly Property SettingType As dxxReferenceTypes Implements idxfSettingsObject.SettingType
            Get
                Return dxxSettingTypes.DIMSETTINGS
            End Get
        End Property
        Public ReadOnly Property Name As String Implements idxfSettingsObject.Name
            Get
                Return dxfEnums.DisplayName(SettingType)
            End Get
        End Property
        Public Property DimLayer As String
            Get
                '^the current dimension layer name. default = "DIMS"
                '~all dimension entities are placed on this layer unless otherwise indicated.
                Return _Struc.Props.ValueStr("DimLayer")
            End Get
            Set(value As String)
                '^used to set the current dimension layer name. default = "DIMS"
                '~all dimension entities are placed on this layer unless otherwise indicated
                value = Trim(value)
                If value <> "" Then Replace(value, " ", "_")
                ValueSet("Dimlayer", value)
            End Set
        End Property
        Public Property DimLayerColor As dxxColors
            Get
                '^returns the current dimension layer color
                Return _Struc.Props.ValueI("DimLayerColor")
            End Get
            Set(value As dxxColors)
                '^returns the current dimension layer color
                If dxfColors.ColorIsReal(value) Then ValueSet("DimLayerColor", value)
            End Set
        End Property
        Public Property DimTickLength As Double
            Get
                '^the default length used for dimension tick lines (in screen inches)
                '~tick lines can be created along with any linear or ordinate dimensions using dxoDrawingTool.Dimension.
                '~dimension tick lines are not associated to the dimension and are not related to
                '~any AutoCAD dimension properties.  they are intended for clarifying which side of a material
                '~thickness that the dimension is drawn from.
                '~min = 0.02 max = 0.3.
                '~default = 0.05.
                Return _Struc.Props.ValueD("DimTickLength")
            End Get
            Set(value As Double)
                '^the default length used for dimension tick lines (in screen inches)
                '~tick lines can be created along with any linear or ordinate dimensions using dxoDrawingTool.Dimension.
                '~dimension tick lines are not associated to the dimension and are not related to
                '~any AutoCAD dimension properties.  they are intended for clarifying which side of a material
                '~thickness that the dimension is drawn from.
                '~min = 0.02 max = 0.3.
                '~default = 0.05.
                value = Math.Abs(value)
                If value < 0.02 Then value = 0.02
                If value > 0.3 Then value = 0.3
                ValueSet("DimTickLength", value)
            End Set
        End Property
        Public Property DrawingScale As Double
            Get
                Return _Struc.Props.ValueD("DrawingScale")
            End Get
            Set(value As Double)
                If value > 0 Then ValueSet("DrawingScale", value)
            End Set
        End Property
        Public Property DrawingUnits As dxxDrawingUnits
            Get
                Return _Struc.Props.ValueI("DrawingUnits")
            End Get
            Set(value As dxxDrawingUnits)
                If value = dxxDrawingUnits.English Or value = dxxDrawingUnits.Metric Then ValueSet("DrawingUnits", value)
            End Set
        End Property
        Friend ReadOnly Property Image As dxfImage
            Get
                If Not String.IsNullOrWhiteSpace(_Struc.ImageGUID) Then Return dxfEvents.GetImage(_Struc.ImageGUID) Else Return Nothing
            End Get
        End Property
        Friend Property ImageGUID As String
            Get
                Return _Struc.ImageGUID
            End Get
            Set(value As String)
                _Struc.ImageGUID = value
            End Set
        End Property
        Friend Property IsCopied As Boolean
            Get
                Return _Struc.IsCopied
            End Get
            Set(value As Boolean)
                _Struc.IsCopied = value
            End Set
        End Property
        Public Property LeaderLayer As String
            Get
                '^the current leader layer name.
                '~all leaders are placed on this layer unless otherwise indicated.
                Return _Struc.Props.ValueStr("LeaderLayer")
            End Get
            Set(value As String)
                '^used to set the current leader layer name.
                '~all leaders are placed on this layer unless otherwise indicated.
                value = Trim(value)
                ValueSet("LeaderLayer", value)
            End Set
        End Property
        Public Property LeaderLayerColor As dxxColors
            Get
                '^returns the current leader layer color
                Return _Struc.Props.ValueI("LeaderLayerColor")
            End Get
            Set(value As dxxColors)
                '^returns the current dimension layer color
                If dxfColors.ColorIsReal(value) Then ValueSet("LeaderLayerColor", value)
            End Set
        End Property
        Public Property LeaderTextJustification As dxxVerticalJustifications
            Get
                '^a enum controling how new the text is aligned to new leaders created in an image
                '~default is Center
                Return _Struc.Props.ValueI("LeaderTextJustification")
            End Get
            Set(value As dxxVerticalJustifications)
                '^a enum controling how new the text is aligned to new leaders created in an image
                '~default is Center
                If value < dxxVerticalJustifications.Top Then value = dxxVerticalJustifications.Top
                If value > dxxVerticalJustifications.Bottom Then value = dxxVerticalJustifications.Bottom
                ValueSet("LeaderTextJustification", value)
            End Set
        End Property
        Friend Property Strukture As TTABLEENTRY
            Get
                Return _Struc
            End Get
            Set(value As TTABLEENTRY)
                _Struc = value
            End Set
        End Property
#End Region 'Properties
#Region "Methods"
        Private Function ValueSet(aName As String, aValue As Object) As Boolean
            Dim _rVal As Boolean
            Dim aProp As TPROPERTY = _Struc.Props.Item(aName)
            If aProp.Index = 0 Then Return _rVal
            _rVal = aProp.Value <> aValue
            If _rVal Then
                aProp.LastValue = aProp.Value
                aProp.Value = aValue
                _Struc.Props.UpdateProperty = aProp
                If Not String.IsNullOrWhiteSpace(_Struc.ImageGUID) Then
                    Dim aImage As dxfImage = Image
                    If aImage IsNot Nothing Then aImage.RespondToSettingChange(Me, aProp) Else _Struc.ImageGUID = ""
                End If
            End If
            Return _rVal
        End Function
#End Region 'Methods
    End Class 'dxoSettingsDim
End Namespace
