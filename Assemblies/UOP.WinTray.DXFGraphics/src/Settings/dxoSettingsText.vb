Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Public Class dxoSettingsText
        Implements idxfSettingsObject
#Region "Members"
        Private _Struc As TTABLEENTRY
#End Region 'Members
#Region "Constructors"
        Friend Sub New(aStructure As TTABLEENTRY)
            _Struc = aStructure
        End Sub
#End Region  'Constructors
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
                Return dxxSettingTypes.TEXTSETTINGS
            End Get
        End Property
        Public ReadOnly Property Name As String Implements idxfSettingsObject.Name
            Get
                Return dxfEnums.DisplayName(SettingType)
            End Get
        End Property
        Public Property Color As dxxColors
            Get
                '^returns the current text color
                Return _Struc.Props.ValueI("Color")
            End Get
            Set(value As dxxColors)
                '^returns the current text color
                SetValue("Color", value)
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
        Public Property LayerColor As dxxColors
            Get
                '^returns the current text layer color
                Return _Struc.Props.ValueI("LayerColor")
            End Get
            Set(value As dxxColors)
                '^returns the current text layer color
                If dxfColors.ColorIsReal(value) Then SetValue("LayerColor", value)
            End Set
        End Property
        Public Property LayerLineWeight As dxxLineWeights
            Get
                '^returns the current text layer lineweight
                Return _Struc.Props.ValueI("LineWeight")
            End Get
            Set(value As dxxLineWeights)
                '^returns the current text layer lineweight
                If value = dxxLineWeights.ByDefault Or value >= 0 Then SetValue("LineWeight", value)
            End Set
        End Property
        Public Property LayerName As String
            Get
                '^the current text layer name. Default = "TEXT"
                '~all text placed on this layer if this name is defined and the layer color is defined
                Return _Struc.Props.ValueStr("LayerName")
            End Get
            Set(value As String)
                value = Trim(value)
                SetValue("LayerName", value)
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
        Private Function SetValue(aName As String, aValue As Object) As Boolean
            Dim _rVal As Boolean = False
            Dim aProp As TPROPERTY = _Struc.Props.Item(aName)
            If aProp.Index = 0 Then Return _rVal
            If aProp.Index > 0 Then
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
            End If
            Return _rVal
        End Function
#End Region 'Methods
    End Class 'dxoSettingsText
End Namespace
