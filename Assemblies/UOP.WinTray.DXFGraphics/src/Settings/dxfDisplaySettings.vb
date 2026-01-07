Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public Class dxfDisplaySettings
#Region "Members"
        Private _Struc As TDISPLAYVARS
        Private tStrucLast As TDISPLAYVARS
        Private bChanged As Boolean
#End Region 'Members
#Region "Constructors"
        Public Sub New()
            'init ------------------------------------------------------------
            _Struc = New TDISPLAYVARS("0", dxfLinetypes.ByLayer, dxxColors.ByLayer, dxxLineWeights.ByLayer, 1, "", "")
            bChanged = False
            tStrucLast = New TDISPLAYVARS(_Struc)
            'init ------------------------------------------------------------
        End Sub
        Friend Sub New(aStructure As TDISPLAYVARS)
            'init ------------------------------------------------------------
            _Struc = aStructure
            bChanged = False
            tStrucLast = New TDISPLAYVARS(_Struc)
            'init ------------------------------------------------------------
        End Sub
        Public Sub New(Optional aLayer As String = "0", Optional aColor As dxxColors = dxxColors.ByLayer, Optional aLinetype As String = "ByLayer", Optional aLTScale As Double = 1.0, Optional aLineWight As dxxLineWeights = dxxLineWeights.ByLayer)
            'init ------------------------------------------------------------
            _Struc = New TDISPLAYVARS(aLayer, aLinetype, aColor, aLineWight, aLTScale)
            tStrucLast = New TDISPLAYVARS(_Struc)
            bChanged = False
            'init ------------------------------------------------------------
        End Sub
        Public Sub New(aDisplayVars As dxfDisplaySettings)
            'init ------------------------------------------------------------
            _Struc = New TDISPLAYVARS(aDisplayVars)
            bChanged = False
            tStrucLast = New TDISPLAYVARS(_Struc)
            'init ------------------------------------------------------------

        End Sub
        Public Sub New(aEntity As dxfEntity)
            'init ------------------------------------------------------------
            _Struc = New TDISPLAYVARS(aEntity)
            bChanged = False
            tStrucLast = New TDISPLAYVARS(_Struc)
            'init ------------------------------------------------------------

        End Sub
#End Region 'Constructors
#Region "Properties"
        Public ReadOnly Property Changed As Boolean
            Get
                Return bChanged
            End Get
        End Property
        Public Property Color As dxxColors
            '^the color setting
            Get
                Return _Struc.Color
            End Get
            Set(value As dxxColors)
                If value = dxxColors.Undefined Then Return
                bChanged = _Struc.Color <> value
                If bChanged Then
                    tStrucLast = New TDISPLAYVARS(_Struc)
                    _Struc.Color = value
                    _Struc.IsDirty = True
                End If
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
        Public ReadOnly Property LastColor As dxxColors
            Get
                Return tStrucLast.Color
            End Get
        End Property
        Public ReadOnly Property LastLayer As String
            Get
                Return tStrucLast.LayerName
            End Get
        End Property
        Public ReadOnly Property LastLinetype As String
            Get
                Return tStrucLast.Linetype
            End Get
        End Property
        Public ReadOnly Property LastLTScale As String
            Get
                Return tStrucLast.LTScale
            End Get
        End Property
        Friend Property LastStructure As TDISPLAYVARS
            Get
                Return tStrucLast
            End Get
            Set(value As TDISPLAYVARS)
                tStrucLast = value
            End Set
        End Property
        Public Property LayerName As String
            '^the layer name associated to the entity
            '~this layer is used for color and linetype info for ByLayer values and the visiblity of the enity in an image
            Get
                Return _Struc.LayerName
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = String.Empty Else value = value.Trim
                bChanged = String.Compare(_Struc.LayerName, value, True) <> 0
                If bChanged Then
                    tStrucLast = _Struc
                    _Struc.LayerName = value
                    _Struc.IsDirty = True
                End If
            End Set
        End Property
        Public Property Linetype As String
            '^the linetype name assigned to the entity
            Get
                Return _Struc.Linetype
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = String.Empty Else value = value.Trim
                bChanged = String.Compare(_Struc.Linetype, value, True) <> 0
                If bChanged Then
                    tStrucLast = _Struc
                    _Struc.Linetype = value
                    _Struc.IsDirty = True
                End If
            End Set
        End Property
        Public Property LineWeight As dxxLineWeights
            Get
                '^the plotter line weight for the entity
                Return _Struc.LineWeight
            End Get
            Set(value As dxxLineWeights)
                bChanged = _Struc.LineWeight <> value
                If bChanged Then
                    tStrucLast = _Struc
                    _Struc.LineWeight = value
                    _Struc.IsDirty = True
                End If
            End Set
        End Property
        Public Property TextStyleName As String
            '^the text style name assigned to the entity
            Get
                Return _Struc.TextStyle
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = String.Empty Else value = value.Trim
                bChanged = String.Compare(_Struc.TextStyle, value, True) <> 0
                If bChanged Then
                    tStrucLast = _Struc
                    _Struc.TextStyle = value
                    _Struc.IsDirty = True
                End If
            End Set
        End Property
        Public Property DimStyleName As String
            '^the dim style name assigned to the entity
            Get
                Return _Struc.DimStyle
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = String.Empty Else value = value.Trim
                bChanged = String.Compare(_Struc.DimStyle, value, True) <> 0
                If bChanged Then
                    tStrucLast = _Struc
                    _Struc.DimStyle = value
                    _Struc.IsDirty = True
                End If
            End Set
        End Property
        Public Property LTScale As Double
            '^the linetype scale factor of the entity
            '~affects the dispaly of non-continuous lines
            Get
                Return _Struc.LTScale
            End Get
            Set(value As Double)
                value = Math.Abs(value)
                If value > 0 Then
                    bChanged = _Struc.LTScale <> value
                    If bChanged Then
                        tStrucLast = _Struc
                        _Struc.LTScale = value
                        _Struc.IsDirty = True
                    End If
                End If
            End Set
        End Property
        Friend Property Strukture As TDISPLAYVARS
            Get
                Return _Struc
            End Get
            Set(value As TDISPLAYVARS)
                If _Struc <> value Then IsDirty = True
                tStrucLast = _Struc
                _Struc = value
                _Struc.IsDirty = False
            End Set
        End Property
        Public Property Suppressed As Boolean
            Get
                Return _Struc.Suppressed Or String.Compare(_Struc.Linetype, dxfLinetypes.Invisible, True) = 0
            End Get
            Friend Set(value As Boolean)
                bChanged = _Struc.Suppressed <> value
                If bChanged Then
                    tStrucLast = _Struc
                    _Struc.Suppressed = value
                    _Struc.IsDirty = True
                End If
            End Set
        End Property
#End Region 'Properties
#Region "Methods"
        Public Overrides Function ToString() As String
            Dim _rVal As String = $"{LayerName},{IIf(Color <> dxxColors.Undefined, Color, "") },{Linetype}"
            If _rVal = ",," Then Return $"dxfDisplaySettings" Else Return _rVal
        End Function
        Public Function Clone() As dxfDisplaySettings
            Return New dxfDisplaySettings(Me)
            '^returns a new object with properties matching those of the cloned object
        End Function

        Public Function Copy(Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLinetype As String = "", Optional aLTScale As Double = 0, Optional aLineWight As dxxLineWeights = dxxLineWeights.Undefined, Optional aTextStyle As String = "", Optional aDimStyle As String = "") As dxfDisplaySettings
            Dim _rVal As dxfDisplaySettings = Clone()
            If Not String.IsNullOrWhiteSpace(aLinetype) Then _rVal.Linetype = aLinetype.Trim()
            If Not String.IsNullOrWhiteSpace(aLayer) Then _rVal.LayerName = aLayer.Trim()
            If Not String.IsNullOrWhiteSpace(aTextStyle) Then _rVal.TextStyleName = aTextStyle.Trim()
            If Not String.IsNullOrWhiteSpace(aDimStyle) Then _rVal.DimStyleName = aDimStyle.Trim()
            If aColor <> dxxColors.Undefined Then _rVal.Color = aColor
            If aLTScale <> 0 Then _rVal.LTScale = aLTScale

            Return _rVal
        End Function

        Public Function CopyDisplayValues(aEntitySet As dxfDisplaySettings, Optional aMatchLayer As String = Nothing, Optional aMatchColor As dxxColors = dxxColors.Undefined, Optional aMatchLineType As String = Nothing) As Boolean
            If aEntitySet Is Nothing Then Return False
            Return CopyDisplayValuesV(aEntitySet.Strukture, aMatchLayer, aMatchColor, aMatchLineType)
        End Function
        Friend Function CopyDisplayValuesV(aEntitySet As TDISPLAYVARS, Optional aMatchLayer As String = Nothing, Optional aMatchColor As dxxColors = dxxColors.Undefined, Optional aMatchLineType As String = Nothing) As Boolean
            Dim _rVal As Boolean
            tStrucLast = _Struc
            Dim aStr As String = String.Empty
            Dim bStr As String = String.Empty
            bChanged = False
            If _Struc.Suppressed <> aEntitySet.Suppressed Then _rVal = True
            If aMatchColor = dxxColors.Undefined Then
                If _Struc.Color <> aEntitySet.Color Then
                    If aStr <> "" Then aStr += dxfGlobals.Delim
                    aStr += "Color" & dxfGlobals.subDelim & _Struc.Color
                    _rVal = True
                    _Struc.Color = aEntitySet.Color
                    If bStr <> "" Then bStr += dxfGlobals.Delim
                    bStr += "Color" & dxfGlobals.subDelim & _Struc.Color
                End If
            Else
                If _Struc.Color = aMatchColor Then
                    If _Struc.Color <> aEntitySet.Color Then
                        If aStr <> "" Then aStr += dxfGlobals.Delim
                        aStr += "Color" & dxfGlobals.subDelim & _Struc.Color
                        _rVal = True
                        _Struc.Color = aEntitySet.Color
                        If bStr <> "" Then bStr += dxfGlobals.Delim
                        bStr += "Color" & dxfGlobals.subDelim & _Struc.Color
                    End If
                End If
            End If
            If String.IsNullOrWhiteSpace(aMatchLayer) Then
                If String.Compare(aEntitySet.LayerName, _Struc.LayerName, True) <> 0 Then
                    If aStr <> "" Then aStr += dxfGlobals.Delim
                    aStr += "LayerName" & dxfGlobals.subDelim & _Struc.LayerName
                    _Struc.LayerName = aEntitySet.LayerName
                    _rVal = True
                    If bStr <> "" Then bStr += dxfGlobals.Delim
                    bStr += "LayerName" & dxfGlobals.subDelim & _Struc.LayerName
                End If
            Else
                aMatchLayer = aMatchLayer.Trim
                If String.Compare(aMatchLayer, _Struc.LayerName, True) = 0 Then
                    If String.Compare(aEntitySet.LayerName, _Struc.LayerName, True) <> 0 Then
                        If aStr <> "" Then aStr += dxfGlobals.Delim
                        aStr += "LayerName" & dxfGlobals.subDelim & _Struc.LayerName
                        _Struc.LayerName = aEntitySet.LayerName
                        _rVal = True
                        If bStr <> "" Then bStr += dxfGlobals.Delim
                        bStr += "LayerName" & dxfGlobals.subDelim & _Struc.LayerName
                    End If
                End If
            End If
            If String.IsNullOrWhiteSpace(aMatchLineType) Then
                If String.Compare(aEntitySet.Linetype, _Struc.Linetype, True) <> 0 Then
                    If aStr <> "" Then aStr += dxfGlobals.Delim
                    aStr += "LineType" & dxfGlobals.subDelim & _Struc.Linetype
                    _Struc.Linetype = aEntitySet.Linetype
                    _rVal = True
                    If bStr <> "" Then bStr += dxfGlobals.Delim
                    bStr += "LineType" & dxfGlobals.subDelim & _Struc.Linetype
                End If
            Else
                aMatchLineType = aMatchLineType.Trim
                If String.Compare(aMatchLineType, _Struc.Linetype, True) = 0 Then
                    If String.Compare(aEntitySet.Linetype, _Struc.Linetype, True) <> 0 Then
                        If aStr <> "" Then aStr += dxfGlobals.Delim
                        aStr += "LineType" & dxfGlobals.subDelim & _Struc.Linetype
                        _Struc.Linetype = aEntitySet.Linetype
                        _rVal = True
                        If bStr <> "" Then bStr += dxfGlobals.Delim
                        bStr += "LineType" & dxfGlobals.subDelim & _Struc.Linetype
                    End If
                End If
            End If
            If _Struc.LTScale <> aEntitySet.LTScale Then
                If aStr <> "" Then aStr += dxfGlobals.Delim
                aStr += "LTScale" & dxfGlobals.subDelim & _Struc.LTScale
                _rVal = True
                _Struc.LTScale = aEntitySet.LTScale
                If bStr <> "" Then bStr += dxfGlobals.Delim
                bStr += "LTScale" & dxfGlobals.subDelim & _Struc.LTScale
            End If
            If _Struc.LineWeight <> aEntitySet.LineWeight Then
                If aStr <> "" Then aStr += dxfGlobals.Delim
                aStr += "LineWeight" & dxfGlobals.subDelim & _Struc.LineWeight
                _rVal = True
                _Struc.LineWeight = aEntitySet.LineWeight
                If bStr <> "" Then bStr += dxfGlobals.Delim
                bStr += "LineWeight" & dxfGlobals.subDelim & _Struc.LineWeight
            End If
            If aStr <> "" Then
                bChanged = True
                _Struc.IsDirty = True
            End If
            Return _rVal
        End Function
        Public Function Defined(Optional bColorDefined As Boolean = False, Optional bLayerDefined As Boolean = False, Optional bLinetypeDefined As Boolean = False) As Boolean
            Dim _rVal As Boolean
            bColorDefined = Color <> dxxColors.Undefined
            bLayerDefined = LayerName <> ""
            bLinetypeDefined = Linetype <> ""
            _rVal = bColorDefined And bLayerDefined And bLinetypeDefined
            Return _rVal
        End Function
        Public Function GetEntityTypeName(aEntType As dxxEntityTypes) As String
            Return dxfUtils.GetEntityTypeName(aEntType)
        End Function
        Public Function PropertyValueGet(aPropType As dxxDisplayProperties) As Object
            Select Case aPropType
                Case dxxDisplayProperties.Color
                    Return Color
                Case dxxDisplayProperties.DimStyle
                    Return _Struc.DimStyle
                Case dxxDisplayProperties.IsDirty
                    Return IsDirty
                Case dxxDisplayProperties.LayerName
                    Return LayerName
                Case dxxDisplayProperties.Linetype
                    Return Linetype
                Case dxxDisplayProperties.LineWeight
                    Return LineWeight
                Case dxxDisplayProperties.LTScale
                    Return LTScale
                Case dxxDisplayProperties.Suppressed
                    Return Suppressed
                Case dxxDisplayProperties.TextStyle
                    Return _Struc.TextStyle
                Case Else
                    Return String.Empty
            End Select
        End Function
        Public Function PropertyValueSet(aPropType As dxxDisplayProperties, aValue As Object) As Boolean
            If aValue Is Nothing Then Return False
            bChanged = False
            Select Case aPropType
                Case dxxDisplayProperties.Color
                    If TVALUES.IsNumber(aValue) Then Color = TVALUES.ToInteger(aValue)
                Case dxxDisplayProperties.DimStyle
                    Dim sVal As String = aValue.ToString
                    bChanged = sVal <> _Struc.DimStyle
                    If bChanged Then
                        tStrucLast = New TDISPLAYVARS(_Struc)
                        _Struc.DimStyle = sVal
                    End If
                Case dxxDisplayProperties.IsDirty
                    If Not TVALUES.IsBoolean(aValue) Then Return False
                    Dim bVal As Boolean = TVALUES.ToBoolean(aValue)
                    bChanged = bVal <> _Struc.IsDirty
                    If bChanged Then
                        tStrucLast = New TDISPLAYVARS(_Struc)
                        _Struc.IsDirty = bVal
                    End If
                Case dxxDisplayProperties.LayerName
                    LayerName = aValue.ToString
                Case dxxDisplayProperties.Linetype
                    Linetype = aValue.ToString
                Case dxxDisplayProperties.LineWeight
                    Dim aLW As dxxLineWeights = TVALUES.To_INT(aValue, LineWeight)
                    If Not dxfEnums.Validate(GetType(dxxLineWeights), aLW) Then Return False
                    LineWeight = aLW
                Case dxxDisplayProperties.LTScale
                    LTScale = TVALUES.ToDouble(aValue, True, LTScale, aValueControl:=mzValueControls.PositiveNonZero)
                Case dxxDisplayProperties.Suppressed
                    If Not TVALUES.IsBoolean(aValue) Then Return False
                    Suppressed = TVALUES.ToBoolean(aValue)
                Case dxxDisplayProperties.TextStyle
                    Dim sVal As String = aValue.ToString
                    bChanged = sVal <> _Struc.TextStyle
                    If bChanged Then
                        tStrucLast = New TDISPLAYVARS(_Struc)
                        _Struc.TextStyle = sVal
                    End If
                Case Else
                    Return False
            End Select
            Return bChanged
        End Function
        Public Function SetDisplayValues(Optional aLayerName As Object = Nothing, Optional aColor As Object = Nothing, Optional aLineType As Object = Nothing, Optional aLTScale As Object = Nothing) As Object
            Dim _rVal As Object = Nothing
            bChanged = False
            tStrucLast = New TDISPLAYVARS(_Struc)
            Dim aStr As String = String.Empty
            Dim aclr As dxxColors
            Dim aVal As Double
            Dim vStr As String
            vStr = $"{_Struc.LayerName},{ _Struc.Color },{ _Struc.Linetype},{ _Struc.LTScale}"
            If aLayerName IsNot Nothing Then
                aStr = aLayerName.ToString().Trim()
                If aStr <> "" Then
                    bChanged = String.Compare(_Struc.LayerName, aStr, True) <> 0
                    _Struc.LayerName = aStr
                End If
            End If
            If aLineType IsNot Nothing Then
                aStr = aLineType.Trim().Replace(  " ", "")
                If aStr <> "" Then
                    bChanged = String.Compare(_Struc.Linetype, aStr, True) <> 0
                    _Struc.Linetype = aStr
                End If
            End If
            If aColor IsNot Nothing Then
                If TVALUES.IsNumber(aColor) Then
                    aclr = TVALUES.ToInteger(aColor, aDefault:=-1, aMinVal:=0, aMaxVal:=256)
                    If aclr <> dxxColors.Undefined Then
                        bChanged = aclr <> _Struc.Color
                        _Struc.Color = aclr
                    End If
                End If
            End If
            If aLTScale IsNot Nothing Then
                If TVALUES.IsNumber(aLTScale) Then
                    aVal = TVALUES.To_DBL(aLTScale)
                    If aVal > 0 Then
                        bChanged = _Struc.LTScale <> aVal
                        _Struc.LTScale = aVal
                    End If
                End If
            End If
            _rVal = bChanged
            Return _rVal
        End Function

#End Region 'Methods
#Region "Shared Methods"
        Public Shared Function Null(Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLinetype As String = "", Optional aLTScale As Double = 0, Optional aLineWight As dxxLineWeights = dxxLineWeights.Undefined, Optional aTextStyle As String = "", Optional aDimStyle As String = "") As dxfDisplaySettings
            If aLinetype Is Nothing Then aLinetype = String.Empty
            If aLayer Is Nothing Then aLayer = String.Empty
            If aTextStyle Is Nothing Then aTextStyle = String.Empty
            If aDimStyle Is Nothing Then aDimStyle = String.Empty
            Dim struc As New TDISPLAYVARS(aLayer) With {
                .Linetype = aLinetype,
                .Color = aColor,
                .LineWeight = aLineWight,
                .LTScale = aLTScale,
                .DimStyle = aDimStyle,
                .TextStyle = aTextStyle
                }
            Return New dxfDisplaySettings(struc)
        End Function
#End Region 'Shared MEthods
    End Class 'dxfDisplaySettings
End Namespace
