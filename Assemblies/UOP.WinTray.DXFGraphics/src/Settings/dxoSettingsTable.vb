Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Public Class dxoSettingsTable
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
                Return dxxSettingTypes.TABLESETTINGS
            End Get
        End Property
        Public ReadOnly Property Name As String Implements idxfSettingsObject.Name
            Get
                Return dxfEnums.DisplayName(SettingType)
            End Get
        End Property
        Public Property Alignment As dxxRectangularAlignments
            '^used to set the tables alignment value
            '~controls how the table is aligned with respect to its insertion point.
            '~default = dxfTopLeft.
            Get
                Return Props.ValueI("Alignment")
            End Get
            Set(value As dxxRectangularAlignments)
                If value >= 1 And value <= 9 Then
                    SetPropVal("Alignment", value, True)
                End If
            End Set
        End Property

        Public Property CellAlignment As dxxRectangularAlignments
            '^used to set the tables alignment value
            '~controls how the table cell text is aligned with respect to its cell boundary.
            '~default = dxfMiddleCenter.
            Get
                Return Props.ValueI("CellAlignment")
            End Get
            Set(value As dxxRectangularAlignments)
                If value >= 1 And value <= 9 Then
                    SetPropVal("CellAlignment", value, True)
                End If
            End Set
        End Property

        Public Property BorderColor As dxxColors
            '^the color to apply to the border lines
            Get
                Return Props.ValueI("BorderColor")
            End Get
            Set(value As dxxColors)
                If dxfColors.ColorIsReal(value) Then SetPropVal("BorderColor", value, True)
            End Set
        End Property
        Public Property BorderLineThickness As Double
            '^the default thickness for table border lines
            '~scaled by the current feature scale when applied
            Get
                Return Props.ValueD("BorderLineThickness")
            End Get
            Set(value As Double)
                If value >= 0 Then SetPropVal("BorderLineThickness", value, True)
            End Set
        End Property
        Public Property ColumnGap As Double
            '^a length to add to the column cells to stretch the table lengthwise
            Get
                Return Props.ValueD("ColumnGap")
            End Get
            Set(value As Double)
                If value < 0 Then value = 0
                SetPropVal("ColumnGap", value, True)
            End Set
        End Property
        Public Property FeatureScale As Double
            '^the scale factor to apply to the entities of the table when they are created
            Get
                Return Props.ValueD("FeatureScale", bNonNegative:=True, aDefault:=1)
            End Get
            Set(value As Double)
                If value > 0 Then SetPropVal("FeatureScale", value, True)
            End Set
        End Property
        Public Property FooterAlignment As dxxHorizontalJustifications
            '^controls the horizontal text aligment of the footer text
            Get
                Return Props.ValueI("FooterAlignment")
            End Get
            Set(value As dxxHorizontalJustifications)
                If value >= 0 And value <= 3 Then SetPropVal("FooterAlignment", value, True)
            End Set
        End Property
        Public Property FooterTextColor As dxxColors
            '^the color to apply to footer text
            '~-1 (dxxColors.Undefined) means the grid text color is applied
            Get
                Return Props.ValueI("FooterTextColor")
            End Get
            Set(value As dxxColors)
                If dxfColors.ColorIsReal(value) Or value = dxxColors.Undefined Then SetPropVal("FooterTextColor", value, True)
            End Set
        End Property
        Public Property FooterTextScale As Double
            '^used to set the current table footer text scale
            '~table footer text size is equal to the grid text size multiplied by this factor.
            '~default = 1.
            Get
                Return Props.ValueD("FooterTextScale")
            End Get
            Set(value As Double)
                If value > 0 Then SetPropVal("FooterTextScale", value, True)
            End Set
        End Property
        Public ReadOnly Property FooterTextSize As Double
            '^returns the current table footer text size
            '~table footer text size is equal to the grid text size multiplied FooterTextScale.
            Get
                Return FooterTextScale * TextSize
            End Get
        End Property
        Public Property FooterTextStyle As String
            '^the text style that will be applied to the footer text
            Get
                Return Props.ValueStr("FooterTextStyle")
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = "Standard" Else value = value.Trim
                SetPropVal("HeaderTextStyle", value, True)
            End Set
        End Property
        Public Property FooterWidthFactor As Double
            '^the width factor to apply to footer text
            '~0 is default and means to use the current width factor of the grid text style
            Get
                Return Props.ValueD("FooterWidthFactor")
            End Get
            Set(value As Double)
                If value <> 0 Then value = TVALUES.LimitedValue(Math.Abs(value), 0.1, 100)
                SetPropVal("FooterWidthFactor", value, True)
            End Set
        End Property
        Public Property GridColor As dxxColors
            '^the color to apply to the grid lines
            Get
                Return Props.ValueI("GridColor")
            End Get
            Set(value As dxxColors)
                If dxfColors.ColorIsReal(value) Then SetPropVal("GridColor", value, True)
            End Set
        End Property
        Public Property GridLineThickness As Double
            '^the default thickness for table grid lines
            '~scaled by the current feature scale when applied
            Get
                Return Props.ValueD("GridLineThickness", True, aDefault:=1)
            End Get
            Set(value As Double)
                If value >= 0 Then SetPropVal("GridLineThickness", value, True)
            End Set
        End Property
        Public Property GridStyle As dxxTableGridStyles
            '^controls how horizontal and vertical grid lines are displayed in the table
            Get
                Return Props.ValueI("GridStyle")
            End Get
            Set(value As dxxTableGridStyles)
                If value >= 0 And value <= 3 Then
                    SetPropVal("GridStyle", value, True)
                End If
            End Set
        End Property
        Public Property HeaderCol As Integer
            '^the row which will be treated as the header column
            Get
                Return Props.ValueI("HeaderCol")
            End Get
            Set(value As Integer)
                If value < 0 Then value = 0
                SetPropVal("HeaderCol", value, True)
            End Set
        End Property
        Public Property HeaderRow As Integer
            '^the row which will be treated as the header row
            Get
                Return Props.ValueI("HeaderRow")
            End Get
            Set(value As Integer)
                If value < 0 Then value = 0
                SetPropVal("HeaderRow", value, True)
            End Set
        End Property
        Public Property HeaderTextColor As dxxColors
            '^the color to apply to header text
            '~-1 (dxxColors.Undefined) means the grid text color is applied
            Get
                Return Props.ValueI("HeaderTextColor")
            End Get
            Set(value As dxxColors)
                If dxfColors.ColorIsReal(value) Or value = dxxColors.Undefined Then SetPropVal("HeaderTextColor", value, True)
            End Set
        End Property
        Public Property HeaderTextScale As Double
            '^used to set the current table header text scale
            '~table header text size is equal to the grid text size multiplied by this factor.
            '~default = 1.
            Get
                Return Props.ValueD("HeaderTextScale")
            End Get
            Set(value As Double)
                If value > 0 Then SetPropVal("HeaderTextScale", value, True)
            End Set
        End Property
        Public ReadOnly Property HeaderTextSize As Double
            '^returns the current table header text size
            '~table header text size is equal to the grid text size multiplied HeaderTextScale.
            Get
                Return HeaderTextScale * TextSize
            End Get
        End Property
        Public Property HeaderTextStyle As String
            '^the text stype to apply to the header text
            Get
                Return Props.ValueStr("HeaderTextStyle")
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = "Standard" Else value = value.Trim
                SetPropVal("HeaderTextStyle", value, True)
            End Set
        End Property
        Public Property HeaderWidthFactor As Double
            '^the width factor to apply to header text
            '~0 is default and means to use the current width factor of the grid text style
            Get
                Return Props.ValueD("HeaderWidthFactor")
            End Get
            Set(value As Double)
                If value <> 0 Then value = TVALUES.LimitedValue(Math.Abs(value), 0.1, 100)
                SetPropVal("HeaderWidthFactor", value, True)
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
                '^returns the current table layer color
                Return Props.ValueI("LayerColor")
            End Get
            Set(value As dxxColors)
                '^returns the current table layer color
                If dxfColors.ColorIsReal(value) Then SetPropVal("LayerColor", value, True)
            End Set
        End Property
        Public Property LayerName As String
            Get
                '^the current table layer name
                '~all tables are placed on this layer if this name is defined and the layer color is defined
                Return Props.ValueStr("LayerName")
            End Get
            Set(value As String)
                '^the current table layer name
                '~all tables are placed on this layer if this name is defined and the layer color is defined
                SetPropVal("LayerName", Trim(value))
            End Set
        End Property
        Friend Property Props As TPROPERTIES
            Get
                Return _Struc.Props
            End Get
            Set(value As TPROPERTIES)
                If value.Count > 0 Then
                    _Struc.Props = value
                End If
            End Set
        End Property
        Public Property RotationAngle As Double
            '^the rotation to apply to table
            Get
                Return Props.ValueD("RotationAngle")
            End Get
            Set(value As Double)
                SetPropVal("RotationAngle", TVALUES.NormAng(value, ThreeSixtyEqZero:=True), True)
            End Set
        End Property
        Public Property RowGap As Double
            '^a length to add to the row heights to stretch the table height wise
            Get
                Return Props.ValueD("RowGap")
            End Get
            Set(value As Double)
                If value < 0 Then value = 0
                SetPropVal("RowGap", value, True)
            End Set
        End Property
        Public Property SaveAsBlock As Boolean
            Get
                '^controls how the table is saved to file
                Return Props.ValueB("SaveAsBlock")
            End Get
            Set(value As Boolean)
                '^controls how the table is saved to file
                SetPropVal("SaveAsBlock", value, True)
            End Set
        End Property
        Public Property SaveAsGroup As Boolean
            Get
                '^controls how the table is saved to file
                Return Props.ValueB("SaveAsGroup")
            End Get
            Set(value As Boolean)
                '^controls how the table is saved to file
                SetPropVal("SaveAsGroup", value, True)
            End Set
        End Property
        Public Property SaveAttributes As Boolean
            '^controls how the table is saved to file
            '~if true and the table is saved as a block then the cell text is
            '~saved as attributes
            Get
                Return Props.ValueB("SaveAttributes")
            End Get
            Set(value As Boolean)
                SetPropVal("SaveAttributes", value, True)
            End Set
        End Property
        Friend Property Strukture As TTABLEENTRY
            Get
                _Struc.EntryType = dxxSettingTypes.TABLESETTINGS
                Return _Struc
            End Get
            Set(value As TTABLEENTRY)
                If value.EntryType = dxxSettingTypes.TABLESETTINGS And value.Props.Count > 0 Then
                    _Struc = value
                    _Struc.EntryType = dxxSettingTypes.TABLESETTINGS
                End If
            End Set
        End Property
        Public Property SuppressBorder As Boolean
            Get
                '^False if the bounding rectangle should be drawn
                Return Props.ValueB("SuppressBorder")
            End Get
            Set(value As Boolean)
                '^False if the bounding rectangle should be drawn
                SetPropVal("SuppressBorder", value, True)
            End Set
        End Property
        Public Property TextColor As dxxColors
            '^the color to apply to grid text
            '~-1 (dxxColors.Undefined) means the grid text color is applied
            Get
                Return Props.ValueI("TextColor")
            End Get
            Set(value As dxxColors)
                If dxfColors.ColorIsReal(value) Then SetPropVal("TextColor", value, True)
            End Set
        End Property
        Public Property TextGap As Double
            '^the gap around text in the table as a fraction of the width of a single character
            '~0 to 6 default = 0.5. Scaled by the current feature scale
            Get
                Return Props.ValueD("TextGap")
            End Get
            Set(value As Double)
                value = TVALUES.LimitedValue(value, 0, 6)
                SetPropVal("TextGap", value, True)
            End Set
        End Property
        Public Property TextSize As Double
            '^the text height for the grid text
            '~Default = 0.2
            '~the current feature scale is applied
            Get
                Return Props.ValueD("TextSize")
            End Get
            Set(value As Double)
                If value > 0 Then SetPropVal("TextSize", value, True)
            End Set
        End Property
        Public Property TextStyleName As String
            '^the text style that will be applied to the cell text
            Get
                Return Props.ValueStr("TextStyleName")
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = "Standard" Else value = value.Trim
                SetPropVal("TextStyleName", Trim(value))
            End Set
        End Property
        Public Property TextWidthFactor As Double
            '^the width factor to apply to grid text
            '~0 is default and means to use the current width factor of the grid text style
            Get
                Return Props.ValueD("TextWidthFactor")
            End Get
            Set(value As Double)
                If value <> 0 Then value = TVALUES.LimitedValue(Math.Abs(value), 0.1, 100)
                SetPropVal("TextWidthFactor", value, True)
            End Set
        End Property
        Public Property TitleAlignment As dxxHorizontalJustifications
            '^controls the horizontal text aligment  of the title text
            Get
                Return Props.ValueI("TitleAlignment")
            End Get
            Set(value As dxxHorizontalJustifications)
                If value >= 0 And value <= 3 Then SetPropVal("TitleAlignment", value, True)
            End Set
        End Property
        Public Property TitleTextColor As dxxColors
            '^the color to apply to title text
            '~-1 (dxxColors.Undefined) means the grid text color is applied
            Get
                Return Props.ValueI("TitleTextColor")
            End Get
            Set(value As dxxColors)
                If dxfColors.ColorIsReal(value) Or value = dxxColors.Undefined Then SetPropVal("TitleTextColor", value, True)
            End Set
        End Property
        Public Property TitleTextScale As Double
            '^used to set the current table title text scale
            '~table title text size is equal to the grid text size multiplied by this factor.
            '~default = 1.
            Get
                Return Props.ValueD("TitleTextScale")
            End Get
            Set(value As Double)
                If value > 0 Then SetPropVal("TitleTextScale", value, True)
            End Set
        End Property
        Public ReadOnly Property TitleTextSize As Double
            '^returns the current table title text size
            '~table title text size is equal to the grid text size multiplied TitleTextScale.
            Get
                Return TitleTextScale * TextSize
            End Get
        End Property
        Public Property TitleTextStyle As String
            '^the text style that will be applied to the title text
            Get
                Return Props.ValueStr("TitleTextStyle")
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = "Standard" Else value = value.Trim
                SetPropVal("TitleTextStyle", value, True)
            End Set
        End Property
        Public Property TitleWidthFactor As Double
            '^the width factor to apply to title text
            '~0 is default and means to use the current width factor of the title text style
            Get
                Return Props.ValueD("TitleWidthFactor")
            End Get
            Set(value As Double)
                If value <> 0 Then value = TVALUES.LimitedValue(Math.Abs(value), 0.1, 100)
                SetPropVal("TitleWidthFactor", value, True)
            End Set
        End Property
#End Region 'Properties
#Region "Methods"
        Friend Function Clone() As dxoSettingsTable
            Return New dxoSettingsTable(_Struc)
        End Function
        Public Sub RestoreValues()
            '^sets the properties back to their last value
            Dim i As Integer
            For i = 1 To _Struc.Props.Count
                SetPropVal(_Struc.Props.Item(i).Name, _Struc.Props.Item(i).LastValue)
            Next i
        End Sub
        Public Sub SaveValues()
            '^stores the current property values
            '~values can be restored by executing RestoreValues
            Dim i As Integer
            Dim aProp As TPROPERTY
            For i = 1 To _Struc.Props.Count
                aProp = _Struc.Props.Item(i)
                aProp.LastValue = aProp.Value
                _Struc.Props.SetItem(i, aProp)
            Next i
        End Sub
        Friend Function SetPropVal(aName As String, aValue As Object, Optional bDirtyOnChange As Boolean = False) As Boolean

            Dim aProp As TPROPERTY = TPROPERTY.Null
            If aValue Is Nothing Or Not _Struc.Props.TryGet(aName, aProp, bSearchByNameOnly:=True) Then Return False

            Dim pname As String = aProp.Name.ToUpper
            Dim newval As Object = aValue
            Dim _rVal As Boolean = False


            If aProp.PropType = dxxPropertyTypes.Switch Then aValue = TPROPERTY.SwitchValue(aValue)
            _rVal = aProp.Value <> newval
            If Not _rVal Then Return _rVal
            Dim aImage As dxfImage = Nothing
            Dim bDontChange As Boolean
            Dim aError As String = String.Empty
            aImage = Image
            If aImage IsNot Nothing Then
                bDontChange = Not dxfImageTool.ValidateSettings(aImage, Strukture, aProp, newval, aError)
            End If
            If bDontChange Then
                _rVal = False
                Return _rVal
            End If
            aProp.LastValue = aProp.Value
            aProp.Value = aValue
            If pname = "SAVEASBLOCK" Then
                If aProp.Value = 1 Then
                    _Struc.Props.SetVal("SAVEASGROUP", False)
                End If
            ElseIf pname = "SAVEASGROUP" Then
                If aProp.Value = 1 Then
                    _Struc.Props.SetVal("SAVEASBLOCK", False)
                End If
            End If
            _Struc.Props.UpdateProperty = aProp
            IsDirty = True
            If aImage IsNot Nothing And Not _Struc.IsCopied Then
                aImage.RespondToSettingChange(Me, aProp)
            End If
            Return _rVal
        End Function
#End Region 'Methods
    End Class 'dxoSettingsTable
End Namespace
