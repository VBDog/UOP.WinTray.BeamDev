Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Public Class dxoSettingsSymbol
        Implements idxfSettingsObject
#Region "Members"
        Private _Struc As TTABLEENTRY
#End Region 'Members
#Region "Constructors"
        Friend Sub New(aStructure As TTABLEENTRY, Optional bIsCopied As Boolean = False)
            _Struc = aStructure
            _Struc.IsCopied = bIsCopied
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
                Return dxxSettingTypes.SYMBOLSETTINGS
            End Get
        End Property
        Public ReadOnly Property Name As String Implements idxfSettingsObject.Name
            Get
                Return dxfEnums.DisplayName(SettingType)
            End Get
        End Property
        Public Property ArrowHead As dxxArrowHeadTypes
            Get
                Return _Struc.Props.ValueI("ArrowHead", dxxArrowHeadTypes.ClosedFilled)
            End Get
            Set(value As dxxArrowHeadTypes)
                If Not dxfEnums.Validate(GetType(dxxArrowHeadTypes), value, bSkipNegatives:=True) Then Return
                SetValue("ArrowHead", value)
            End Set
        End Property
        Public ReadOnly Property ArrowHeadName As String
            Get
                Return dxfEnums.Description(ArrowHead)
            End Get
        End Property
        Public Property ArrowSize As Double
            Get
                Return _Struc.Props.ValueD("ArrowSize")
            End Get
            Set(value As Double)
                If value >= 0 Then
                    SetValue("ArrowSize", value)
                End If
            End Set
        End Property
        Public Property ArrowStyle As dxxArrowStyles
            Get
                Return _Struc.Props.ValueI("ArrowStyle")
            End Get
            Set(value As dxxArrowStyles)
                If value < 0 Or value > dxxArrowStyles.StraightFullOpen Then Return
                SetValue("ArrowStyle", value)
            End Set
        End Property
        Public Property ArrowTails As dxxArrowTails
            Get
                Return _Struc.Props.ValueI("ArrowTails", dxxArrowTails.Undefined)
            End Get
            Set(value As dxxArrowTails)
                If value >= 0 And value <= 2 Then
                    SetValue("ArrowTails", value)
                End If
            End Set
        End Property
        Public Property ArrowTextAlignment As dxxRectangularAlignments
            Get
                Return _Struc.Props.ValueI("ArrowTextAlignment", dxxRectangularAlignments.TopLeft)
            End Get
            Set(value As dxxRectangularAlignments)
                If value >= dxxRectangularAlignments.TopLeft And value <= dxxRectangularAlignments.BottomRight Then
                    SetValue("ArrowTextAlignment", value)
                End If
            End Set
        End Property
        Public Property AxisStyle As Integer
            Get
                Return _Struc.Props.ValueI("AxisStyle")
            End Get
            Set(value As Integer)
                SetValue("AxisStyle", value)
            End Set
        End Property
        Public Property BoxText As Boolean
            Get
                Return _Struc.Props.ValueB("BoxText")
            End Get
            Set(value As Boolean)
                SetValue("BoxText", value)
            End Set
        End Property
        Public Property FeatureScale As Double
            Get
                Return _Struc.Props.ValueD("FeatureScale", True, True, 1)
            End Get
            Set(value As Double)
                SetValue("FeatureScale", value)
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
        Friend Property IsDirty As Boolean
            Get
                Return _Struc.IsDirty
            End Get
            Set(value As Boolean)
                _Struc.IsDirty = value
            End Set
        End Property
        Public Property LayerColor As dxxColors
            Get
                Return _Struc.Props.ValueI("LayerColor")
            End Get
            Set(value As dxxColors)
                If value <> dxxColors.Undefined And value <> dxxColors.ByBlock And value <> dxxColors.ByLayer Then
                    SetValue("LayerColor", value)
                End If
            End Set
        End Property
        Public Property LayerName As String
            Get
                Return _Struc.Props.ValueStr("LayerName")
            End Get
            Set(value As String)
                SetValue("LayerName", value.Trim())
            End Set
        End Property
        Public Property LineColor As dxxColors
            Get
                Return _Struc.Props.ValueI("LineColor")
            End Get
            Set(value As dxxColors)
                If value <> dxxColors.Undefined Then
                    SetValue("LineColor", value)
                End If
            End Set
        End Property
        Friend Property Props As TPROPERTIES
            Get
                Return _Struc.Props
            End Get
            Set(value As TPROPERTIES)
                If value.Count > 0 Then _Struc.Props = value
            End Set
        End Property
        Friend Property Strukture As TTABLEENTRY
            Get
                _Struc.EntryType = dxxSettingTypes.SYMBOLSETTINGS
                Return _Struc
            End Get
            Set(value As TTABLEENTRY)
                If value.EntryType = dxxSettingTypes.SYMBOLSETTINGS And value.Props.Count > 0 Then
                    _Struc = value
                    _Struc.EntryType = dxxSettingTypes.SYMBOLSETTINGS
                End If
            End Set
        End Property


        Public Property LineDisplaySettings As dxfDisplaySettings
            Get
                Return New dxfDisplaySettings(LayerName, LineColor, dxfLinetypes.Continuous)
            End Get
            Set(value As dxfDisplaySettings)
                If value IsNot Nothing Then
                    LineColor = value.Color
                    LayerName = value.LayerName
                End If
            End Set
        End Property

        Public Property TextDisplaySettings As dxfDisplaySettings
            Get
                Return New dxfDisplaySettings(LayerName, TextColor, dxfLinetypes.Continuous)
            End Get
            Set(value As dxfDisplaySettings)
                If value IsNot Nothing Then
                    TextColor = value.Color
                End If
            End Set
        End Property

        Public Property TextColor As dxxColors
            Get
                Return _Struc.Props.ValueI("TextColor")
            End Get
            Set(value As dxxColors)
                If value <> dxxColors.Undefined Then
                    SetValue("TextColor", value)
                End If
            End Set
        End Property
        Public Property TextGap As Double
            Get
                Return _Struc.Props.ValueD("TextGap", bNonZero:=True)
            End Get
            Set(value As Double)
                If value < 0 Then BoxText = True
                SetValue("TextGap", Math.Abs(value))
            End Set
        End Property
        Public Property TextHeight As Double
            Get
                Return _Struc.Props.ValueD("TextHeight", True)
            End Get
            Set(value As Double)
                SetValue("TextHeight", Math.Abs(value))
            End Set
        End Property
        Public Property TextStyleName As String
            Get
                Return _Struc.Props.ValueStr("TextStyle", "Standard")
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = String.Empty Else value = value.Trim
                If value <> "" Then SetValue("TextStyle", value)
            End Set
        End Property
#End Region 'Properties
#Region "Methods"
        Friend Function Clone() As dxoSettingsSymbol
            Return New dxoSettingsSymbol(_Struc)
        End Function
        Public Sub RestoreValues()
            '^sets the properties back to their last value
            Dim i As Integer
            For i = 1 To _Struc.Props.Count
                SetValue(_Struc.Props.Item(i).Name, _Struc.Props.Item(i).LastValue)
            Next i
        End Sub


        Public Sub SaveValues()
            '^stores the current property values
            '~values can be restored by executing RestoreValues
            Dim i As Integer
            Dim aProp As TPROPERTY
            For i = 1 To _Struc.Props.Count
                aProp = _Struc.Props.Item(i)
                aProp.LastValue = _Struc.Props.Item(i).Value
                _Struc.Props.SetItem(i, aProp)
            Next i
        End Sub
        Friend Function SetValue(aName As String, aValue As Object, Optional bIgnoreValueControl As Boolean = False) As Boolean
            Dim _rVal As Boolean
            Dim aProp As TPROPERTY = _Struc.Props.Item(aName)
            Dim pname As String
            Dim newval As Object
            _rVal = False
            aProp = _Struc.Props.Item(aName)
            If aProp.Index = 0 Then Return _rVal
            If aValue Is Nothing Then Return _rVal
            pname = aProp.Name.ToUpper
            newval = aValue
            _rVal = aProp.SetVal(aValue, bIgnoreValueControl:=bIgnoreValueControl)
            If Not _rVal Then Return _rVal
            Dim aImage As dxfImage = Nothing
            Dim bDontChange As Boolean
            Dim aError As String = String.Empty
            aImage = Image
            If aImage IsNot Nothing Then
                bDontChange = Not dxfImageTool.ValidateSettings(aImage, Strukture, aProp, aProp.Value, aError)
            End If
            If bDontChange Then
                _rVal = False
                Return _rVal
            End If
            _Struc.Props.UpdateProperty = aProp
            If _Struc.IsCopied Then IsDirty = True
            If aImage IsNot Nothing And Not _Struc.IsCopied Then
                aImage.RespondToSettingChange(Me, aProp)
            End If
            Return _rVal
        End Function
#End Region 'Methods
    End Class 'dxoSettingsSymbol
End Namespace
