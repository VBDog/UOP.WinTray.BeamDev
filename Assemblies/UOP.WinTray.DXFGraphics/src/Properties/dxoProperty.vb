Imports System.Windows.Controls
Imports SharpDX.Direct2D1
Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public Class dxoProperty
        Implements ICloneable
#Region "Members"


#End Region 'Members
#Region "Constructors"
        Public Sub New()
            'init ------------------------------------
            Color = -1
            DecodeString = String.Empty
            Follows = 0
            GroupCode = -1
            Index = -1

            _GroupName = String.Empty
            LastValue = String.Empty
            LineNo = 0

            Mark = False
            NonDXF = False
            Occurance = 1
            Path = String.Empty
            Prompt = String.Empty
            Scaleable = False
            Suppressed = False
            SuppressedValue = Nothing
            PropertyType = dxxPropertyTypes.Undefined
            Value = String.Empty
            _EnumName = Nothing
            ValueControl = mzValueControls.None
            Description = String.Empty
            Max = Nothing
            Min = Nothing
            _EnumValueType = Nothing
            _Key = ""
            _Hidden = False
            _Reactors = New dxoDictionaryEntries(2, 330)
            IsOrdinate = False
            _IsHeader = Nothing
            _Name = String.Empty
            DoNotCopy = False
            'init ------------------------------------
        End Sub
        Public Sub New(Optional aName As String = "")
            'init ------------------------------------
            Color = -1
            DecodeString = String.Empty
            Follows = 0
            GroupCode = -1
            Index = -1

            _GroupName = String.Empty
            LastValue = String.Empty
            LineNo = 0

            Mark = False
            NonDXF = False
            Occurance = 1
            Path = String.Empty
            Prompt = String.Empty
            Scaleable = False
            Suppressed = False
            SuppressedValue = Nothing
            PropertyType = dxxPropertyTypes.Undefined
            Value = String.Empty
            _EnumName = Nothing
            ValueControl = mzValueControls.None
            Description = String.Empty
            Max = Nothing
            Min = Nothing
            _EnumValueType = Nothing
            _Key = String.Empty
            _Hidden = False
            _Reactors = New dxoDictionaryEntries(2, 330)
            IsOrdinate = False
            _IsHeader = Nothing
            _Name = IIf(Not String.IsNullOrWhiteSpace(aName), aName.Trim(), String.Empty)
            DoNotCopy = False
            'init ------------------------------------

        End Sub
        Public Sub New(aGroupCode As Integer, aName As String)
            'init ------------------------------------
            Color = -1
            DecodeString = String.Empty
            Follows = 0
            GroupCode = aGroupCode
            Index = -1

            _GroupName = String.Empty
            LastValue = String.Empty
            LineNo = 0

            Mark = False
            NonDXF = False
            Occurance = 1
            Path = String.Empty
            Prompt = String.Empty
            Scaleable = False
            Suppressed = False
            SuppressedValue = Nothing
            PropertyType = dxxPropertyTypes.Undefined
            Value = String.Empty
            _EnumName = Nothing
            ValueControl = mzValueControls.None
            Description = ""
            Max = Nothing
            Min = Nothing
            _EnumValueType = Nothing
            _Key = ""
            _Hidden = False
            _Reactors = New dxoDictionaryEntries(2, 330)
            IsOrdinate = False
            _IsHeader = Nothing
            _Name = IIf(Not String.IsNullOrWhiteSpace(aName), aName.Trim(), String.Empty)
            DoNotCopy = False
            'init ------------------------------------

        End Sub

        Public Sub New(aCode As Integer, aValue As Object, aName As String, Optional aPropType As dxxPropertyTypes = dxxPropertyTypes.Undefined, Optional aLastVal As Object = Nothing, Optional aValControl As mzValueControls = mzValueControls.Undefined, Optional bScalable As Boolean = False, Optional aSuppressedVal As Object = Nothing, Optional bHidden As Boolean = False,
                        Optional bNonDXF As Boolean = False, Optional bSuppressed As Boolean = False, Optional bIsOrdinate As Boolean = False, Optional aDecodeString As String = "", Optional bSetSuppressedValue As Boolean = False, Optional aEnumName As [Enum] = Nothing)
            'init ------------------------------------
            _Name = IIf(Not String.IsNullOrWhiteSpace(aName), aName.Trim(), "")
            PropertyType = IIf(aPropType <> dxxPropertyTypes.Undefined, aPropType, TPROPERTY.TypeByGC(aCode))
            Value = ""

            Color = -1
            DecodeString = IIf(Not String.IsNullOrWhiteSpace(aDecodeString), aDecodeString.Trim(), "")
            Follows = 0
            GroupCode = -1
            Index = -1

            _GroupName = ""
            LastValue = ""
            LineNo = 0

            Mark = False
            NonDXF = bNonDXF
            Occurance = 1
            Path = ""
            Prompt = ""
            Scaleable = False
            Suppressed = False
            SuppressedValue = Nothing

            _EnumName = aEnumName
            ValueControl = aValControl
            Description = ""
            Max = Nothing
            Min = Nothing
            _EnumValueType = Nothing
            _Key = ""
            _Hidden = False
            _Reactors = New dxoDictionaryEntries(2, 330)
            IsOrdinate = bIsOrdinate
            _IsHeader = Nothing
            DoNotCopy = False
            'init ------------------------------------

            Hidden = bHidden Or aCode < 0 Or Name.StartsWith("*")
            NonDXF = bNonDXF Or aCode < 0 Or Name.StartsWith("*")
            GroupCode = Math.Abs(aCode)

            If aValue Is Nothing Then aValue = ""
            Dim isenum As Boolean
            Dim tname As String = String.Empty
            PropertyType = TPROPERTY.GetSetType(aCode, aValue, isenum, tname, PropertyType, NonDXF)
            Value = aValue
            LastValue = Value

            If aLastVal IsNot Nothing Then

                TPROPERTY.GetSetType(GroupCode, aLastVal, PropertyType, NonDXF)
                LastValue = aLastVal
            End If

            If PropertyType = dxxPropertyTypes.dxf_Single Or PropertyType = dxxPropertyTypes.dxf_Double Or PropertyType = dxxPropertyTypes.dxf_Integer Then
                Scaleable = bScalable
            End If



            If PropertyType = dxxPropertyTypes.Color And DecodeString = "" Then
                DecodeString = dxfGlobals.ColorDecode
            End If

            Suppressed = bSuppressed
            If PropertyType <> dxxPropertyTypes.dxf_Variant And PropertyType <> dxxPropertyTypes.Handle Then
                If aSuppressedVal IsNot Nothing Then
                    SuppressedValue = TPROPERTY.SetValueByType(aSuppressedVal, PropertyType, Value, GroupCode)
                Else
                    If bSetSuppressedValue Then
                        SuppressedValue = Value
                    End If
                End If
            End If


            Try
                If isenum Then
                    Dim exAssembly = Reflection.Assembly.GetExecutingAssembly

                    _EnumValueType = exAssembly.GetTypes.First(Function(f) f.Name = tname)


                    If DecodeString = "" Then
                        DecodeString = dxfEnums.ValueNameList(_EnumValueType)
                    End If
                End If
            Catch ex As Exception
                If dxfUtils.RunningInIDE Then
                    MessageBox.Show("Property Enum Value Detection Error")
                End If
                Return
            End Try
        End Sub

        Friend Sub New(aProperty As TPROPERTY)

            Color = aProperty.Color
            DecodeString = aProperty.DecodeString
            Follows = aProperty.Follows
            GroupCode = aProperty.GroupCode
            Index = aProperty.Index

            _GroupName = aProperty._GroupName
            LastValue = aProperty.LastValue
            LineNo = aProperty.LineNo

            Mark = aProperty.Mark
            NonDXF = aProperty.NonDXF
            Occurance = aProperty.Occurance
            Path = aProperty.Path
            Prompt = aProperty.Prompt
            Scaleable = aProperty.Scaleable
            Suppressed = aProperty.Suppressed
            SuppressedValue = aProperty.SuppressedValue
            PropertyType = aProperty.PropType
            Value = IIf(aProperty.Value Is Nothing, Nothing, Force.DeepCloner.DeepClonerExtensions.DeepClone(Of Object)(aProperty.Value))
            _EnumName = aProperty._EnumName
            ValueControl = aProperty.ValueControl
            Description = aProperty.Description
            Max = aProperty.Max
            Min = aProperty.Min

            _EnumValueType = aProperty._EnumValueType
            Key = aProperty.Key
            _Hidden = aProperty._Hidden
            _Reactors = New dxoDictionaryEntries(aProperty._Reactors)
            IsOrdinate = aProperty.IsOrdinate
            _IsHeader = aProperty._IsHeader
            _Name = aProperty._Name
            DoNotCopy = aProperty.DoNotCopy
        End Sub

        Public Sub New(aProperty As dxoProperty)
            If aProperty Is Nothing Then
                'init ------------------------------------
                Color = -1
                DecodeString = String.Empty
                Follows = 0
                GroupCode = -1
                Index = -1

                _GroupName = String.Empty
                LastValue = String.Empty
                LineNo = 0

                Mark = False
                NonDXF = False
                Occurance = 1
                Path = String.Empty
                Prompt = String.Empty
                Scaleable = False
                Suppressed = False
                SuppressedValue = Nothing
                PropertyType = dxxPropertyTypes.Undefined
                Value = String.Empty
                _EnumName = Nothing
                ValueControl = mzValueControls.None
                Description = String.Empty
                Max = Nothing
                Min = Nothing
                _EnumValueType = Nothing
                _Key = String.Empty
                _Hidden = False
                _Reactors = New dxoDictionaryEntries(2, 330)
                IsOrdinate = False
                _IsHeader = Nothing
                _Name = IIf(aProperty IsNot Nothing, aProperty.Name, String.Empty)
                DoNotCopy = False
                'init ------------------------------------
            Else
                'init ------------------------------------

                Color = aProperty.Color
                DecodeString = aProperty.DecodeString
                Follows = aProperty.Follows
                GroupCode = aProperty.GroupCode
                Index = aProperty.Index

                _GroupName = aProperty._GroupName
                LastValue = aProperty.LastValue
                LineNo = aProperty.LineNo

                Mark = aProperty.Mark
                NonDXF = aProperty.NonDXF
                Occurance = aProperty.Occurance
                Path = aProperty.Path
                Prompt = aProperty.Prompt
                Scaleable = aProperty.Scaleable
                Suppressed = aProperty.Suppressed
                SuppressedValue = aProperty.SuppressedValue
                PropertyType = aProperty.PropertyType
                Value = IIf(aProperty.Value Is Nothing, Nothing, Force.DeepCloner.DeepClonerExtensions.DeepClone(Of Object)(aProperty.Value))
                _EnumName = aProperty._EnumName
                ValueControl = aProperty.ValueControl
                Description = aProperty.Description
                Max = aProperty.Max
                Min = aProperty.Min

                _EnumValueType = aProperty._EnumValueType
                Key = aProperty.Key
                _Hidden = aProperty._Hidden
                _Reactors = New dxoDictionaryEntries(aProperty._Reactors)
                IsOrdinate = aProperty.IsOrdinate
                _IsHeader = aProperty._IsHeader
                _Name = aProperty._Name
                DoNotCopy = aProperty.DoNotCopy
                'init ------------------------------------
            End If



        End Sub

        Friend Sub New(dxfEntry1 As dxfFileEntry)
            'init ------------------------------------
            Color = -1
            DecodeString = String.Empty
            Follows = 0
            GroupCode = -1
            Index = -1

            _GroupName = String.Empty
            LastValue = String.Empty
            LineNo = 0

            Mark = False
            NonDXF = False
            Occurance = 1
            Path = String.Empty
            Prompt = String.Empty
            Scaleable = False
            Suppressed = False
            SuppressedValue = Nothing
            PropertyType = dxxPropertyTypes.Undefined
            Value = String.Empty
            _EnumName = Nothing
            ValueControl = mzValueControls.None
            Description = String.Empty
            Max = Nothing
            Min = Nothing
            _EnumValueType = Nothing
            _Key = ""
            _Hidden = False
            _Reactors = New dxoDictionaryEntries(2, 330)
            IsOrdinate = False
            _IsHeader = Nothing
            _Name = String.Empty
            DoNotCopy = False
            'init ------------------------------------
            If dxfEntry1 Is Nothing Then Return

            GroupCode = dxfEntry1.GroupCode

            Value = dxfEntry1.Value
            PropertyType = TPROPERTY.GetSetType(GroupCode, Value, dxfEntry1.PropertyType, NonDXF)
            LineNo = dxfEntry1.LineNo
            Name = $"Line_{LineNo}"

        End Sub

#End Region 'Constructors
#Region "Properties"
        Private _Changed As Boolean
        Public Property Max As Double?
        Public Property Min As Double?

        Public Property Occurance As Integer
        Public Property Prompt As String
        Public Property Follows As Integer
        Public Property Index As Integer

        Friend Property Mark As Boolean
        Friend Property Color As Integer
        Public Property GroupCode As Integer
        Friend Property Path As String
        Public ValueControl As mzValueControls
        Public Property IsOrdinate As Boolean
        Public Property Description As String
        Public Property PropertyType As dxxPropertyTypes
        Public Property LineNo As Integer
        Friend Property _EnumName As [Enum]
        Friend Property _EnumValueType As Type

        Friend Property DoNotCopy As Boolean
        Friend _Key As String
        Public Property Key As String
            Get
                If String.IsNullOrEmpty(Name) Then Name = ""
                If String.IsNullOrEmpty(_Key) Then Return Name.ToUpper Else Return _Key.ToUpper
            End Get
            Set(value As String)
                _Key = value
            End Set
        End Property

        Friend ReadOnly Property DataTypeCode As Integer
            Get
                Dim _rVal As Integer = 0
                'On Error Resume Next
                Select Case PropertyType
                    Case dxxPropertyTypes.dxf_Variant
                        _rVal = 1000
                    Case dxxPropertyTypes.dxf_Integer
                        _rVal = 1070
                    Case dxxPropertyTypes.Switch
                        _rVal = 1070
                    Case dxxPropertyTypes.dxf_Long, dxxPropertyTypes.Color, dxxPropertyTypes.BitCode
                        _rVal = 1070
                    Case dxxPropertyTypes.dxf_Single
                        _rVal = 1040
                    Case dxxPropertyTypes.dxf_Double
                        _rVal = 1040
                    Case dxxPropertyTypes.dxf_Boolean
                        _rVal = -1070
                    Case dxxPropertyTypes.dxf_String, dxxPropertyTypes.ClassMarker
                        _rVal = 1000
                    Case dxxPropertyTypes.Handle, dxxPropertyTypes.Pointer
                        _rVal = 1005
                End Select
                Return _rVal
            End Get
        End Property

        Private _GroupName As String
        Public Property GroupName As String
            Get
                Return _GroupName
            End Get
            Set(value As String)
                If value Is Nothing Then value = String.Empty
                _GroupName = value.Trim()
                If Not String.IsNullOrWhiteSpace(value) Then
                    If value.StartsWith("$") Then _IsHeader = True
                End If

            End Set
        End Property
        Public ReadOnly Property IsHandle As Boolean
            Get
                Return PropertyType = dxxPropertyTypes.Handle
            End Get
        End Property
        Public Property IsHeaderProperty As Boolean
            Get
                If Not _IsHeader.HasValue Then Return Name.StartsWith("$") Else Return _IsHeader.Value
            End Get
            Friend Set(value As Boolean)
                _IsHeader = value
            End Set
        End Property
        Private _IsPointer As Boolean
        Public Property IsPointer As Boolean
            Get
                Return _IsPointer
            End Get
            Set(value As Boolean)
                _IsPointer = value
            End Set
        End Property

        Friend _Name As String
        Public Property Name As String
            Get
                Return _Name
            End Get
            Set(value As String)
                If value Is Nothing Then value = String.Empty
                _Name = value.Trim()

                If Not String.IsNullOrWhiteSpace(value) Then
                    If value.StartsWith("$") Then _IsHeader = True
                End If
            End Set
        End Property



        Friend _Hidden As Boolean
        Public Property Hidden As Boolean
            Get
                Return GroupCode < 0 Or (GroupCode <> 0 And Name.StartsWith("*")) Or _Hidden
            End Get
            Set(value As Boolean)
                _Hidden = value
                'If value And GroupCode = 213 Then
                '    Beep()
                'End If
            End Set
        End Property

        Friend _IsHeader As Boolean?
        Public Property IsHeader As Boolean
            Get
                If Not _IsHeader.HasValue Then Return Name.StartsWith("$") Else Return _IsHeader.Value
            End Get
            Set(value As Boolean)
                _IsHeader = value
            End Set
        End Property


        Private _Scaleable As Boolean

        Public Property Scaleable As Boolean
            Get
                Return _Scaleable
            End Get
            Friend Set(value As Boolean)
                _Scaleable = value
            End Set
        End Property
        Friend NonDXF As Boolean

        Private _Suppressed As Boolean
        Public Property Suppressed As Boolean
            Get
                Dim _rVal As Boolean
                'undefined
                If GroupCode < 0 Or Value Is Nothing Or Value Is Nothing Or Suppressed Then
                    _rVal = True
                Else
                    'matches defined supressing value (default)
                    If SuppressedValue IsNot Nothing And Value IsNot Nothing Then
                        If String.Compare(TypeName(Value), "String", True) = 0 Or Not TVALUES.IsNumber(Value) Then
                            _rVal = String.Compare(Value, SuppressedValue, True) = 0
                        Else
                            _rVal = (Value = SuppressedValue)
                        End If
                    Else
                        'explicitly suppressed or a negative group code
                        _rVal = Suppressed Or GroupCode < 0
                    End If
                End If
                Return _rVal
            End Get
            Set(value As Boolean)
                _Suppressed = value
            End Set
        End Property
        Public SuppressedValue As Object

        Public ReadOnly Property ValueString As String
            Get
                If _Value IsNot Nothing Then Return _Value.ToString Else Return "Nothing"
            End Get
        End Property

        Private _Value As Object
        Public Property Value As Object
            Get
                Return _Value
            End Get
            Set(value As Object)
                _Changed = SetVal(value)
            End Set
        End Property
        Private _LastValue As Object

        Public Property LastValue As Object
            Get
                Return _LastValue
            End Get
            Friend Set(value As Object)
                _LastValue = value
            End Set
        End Property

        Private _DecodeString As String
        Public Property DecodeString As String
            Get
                Return _DecodeString
            End Get
            Friend Set(value As String)
                _DecodeString = value
            End Set
        End Property



        Public ReadOnly Property DisplayName As String
            Get
                Dim _rVal As String = String.Empty
                If _EnumName IsNot Nothing Then
                    _rVal = dxfEnums.DisplayName(_EnumName, False)
                End If
                If _rVal = "" Then _rVal = Name
                Return _rVal
            End Get
        End Property

        Public Property ValueD As Double
            Get
                Return TVALUES.To_DBL(_Value)
            End Get
            Set(value As Double)
                _Changed = SetVal(value)
            End Set
        End Property


        Public Property ValueI As Integer
            Get
                Return TVALUES.ToInteger(_Value)
            End Get
            Set(value As Integer)
                _Changed = SetVal(value)
            End Set
        End Property

        Public Property ValueL As Long
            Get
                Return TVALUES.ToLong(_Value)
            End Get
            Set(value As Long)
                _Changed = SetVal(value)
            End Set
        End Property
        Public Property ValueB As Boolean
            Get
                Return TVALUES.ToBoolean(_Value, False, PropertyType = dxxPropertyTypes.Switch)
            End Get
            Set(value As Boolean)
                _Changed = SetVal(value)
            End Set
        End Property

        Public Property ValueS As String
            Get
                Return StringValue()
            End Get
            Set(value As String)
                _Changed = SetVal(value)
            End Set
        End Property



        Friend _Reactors As dxoDictionaryEntries
        Friend Property Reactors As dxoDictionaryEntries
            Get

                Return _Reactors
            End Get
            Set(value As dxoDictionaryEntries)
                _Reactors = New dxoDictionaryEntries(value)
            End Set
        End Property
#End Region 'Properties
#Region "Methods"
        Public Function IsNamed(aNamesOrKeys As List(Of String), Optional aPrefixsToConsider As List(Of String) = Nothing) As Boolean
            If aNamesOrKeys Is Nothing Then Return False
            For Each nameorkey In aNamesOrKeys
                If IsNamed(Name, aPrefixsToConsider) Then Return True
            Next
            Return False

        End Function
        Public Function IsNamed(aNameOrKey As String, Optional aPrefixsToConsider As List(Of String) = Nothing) As Boolean
            If String.IsNullOrWhiteSpace(aNameOrKey) Then Return False Else aNameOrKey = aNameOrKey.Trim()
            If String.Compare(aNameOrKey, Name, True) = 0 Or String.Compare(aNameOrKey, Key, True) = 0 Then Return True
            If aPrefixsToConsider Is Nothing Then Return False
            For Each prefix In aPrefixsToConsider
                If String.Compare($"{prefix}{aNameOrKey}", Name, True) = 0 Or String.Compare($"{prefix}{aNameOrKey}", Key, True) = 0 Then Return True
            Next
            Return False

        End Function

        Public Function ReactorAdd(aName As String, aHandle As String) As Boolean

            Return Reactors.AddUnique(aName, aHandle)
        End Function

        Friend Function SetVal(aValue As Object, Optional aLastValue As Object = Nothing, Optional bSuppress As Double? = Nothing, Optional bIgnoreValueControl As Boolean = False) As Boolean
            If aValue Is Nothing Then Return False
            If _Value Is Nothing Then _Value = String.Empty


            Dim tname As String = TVALUES.GetTypeName(aValue)
            If String.Compare(tname, "String", True) = 0 Then
                Dim sval As String = aValue.ToString()
                If String.Compare(sval, dxfLinetypes.ByBlock, True) = 0 Then
                    sval = dxfLinetypes.ByBlock
                End If
                If String.Compare(sval, dxfLinetypes.ByLayer, True) = 0 Then
                    sval = dxfLinetypes.ByLayer
                End If

                aValue = sval
            End If
            Dim _rVal As Boolean
            Dim vVal As New TVECTOR(0)

            If aValue Is Nothing Then aValue = _Value
            If TVALUES.IsNumber(aValue) Then
                If Max.HasValue Then
                    If aValue > Max.Value Then aValue = Max.Value
                End If
                If Min.HasValue Then
                    If aValue < Min.Value Then aValue = Min.Value
                End If
            End If
            Dim vc As mzValueControls = ValueControl
            If bIgnoreValueControl Then vc = mzValueControls.None
            'if the enum vlaue type is set, make sure the new value is a member of the enum family
            If _EnumValueType IsNot Nothing And PropertyType = dxxPropertyTypes.dxf_Integer Then
                If _EnumValueType <> GetType(dxxColors) Then
                    aValue = TVALUES.To_INT(aValue, TVALUES.To_INT(Value))
                    If Not dxfEnums.Validate(_EnumValueType, aValue) Then
                        Return False
                    End If
                End If
            End If
            aValue = TPROPERTY.SetValueByType(aValue, PropertyType, _Value, vc)
            _LastValue = _Value
            _LastValue = TPROPERTY.SetValueByType(_LastValue, PropertyType, _Value, vc)
            _Value = aValue
            If PropertyType >= dxxPropertyTypes.dxf_String And PropertyType <= dxxPropertyTypes.ClassMarker Then
                Try
                    If _LastValue Is Nothing Or _Value Is Nothing Then
                        _rVal = True
                    ElseIf _Value.ToString().Length <> _LastValue.ToString().Length Then
                        _rVal = True
                    Else
                        _rVal = String.Compare(TVALUES.To_STR(_Value), TVALUES.To_STR(_LastValue), True) <> 0
                    End If
                Catch ex As Exception
                    _rVal = True
                End Try
            Else
                Try
                    If TVALUES.IsNumber(_LastValue) Then
                        _rVal = _LastValue.ToString() <> _Value.ToString()
                    Else
                        _rVal = True
                    End If
                Catch ex As Exception
                    Try
                        If _LastValue Is Nothing Or _Value Is Nothing Then
                            _rVal = True
                        ElseIf _Value.ToString().Length <> _LastValue.ToString().Length Then
                            _rVal = True
                        Else
                            _rVal = String.Compare(TVALUES.To_STR(_Value), TVALUES.To_STR(_LastValue), True) <> 0
                        End If
                    Catch ex2 As Exception
                        _rVal = True
                    End Try
                End Try
            End If
            If bSuppress IsNot Nothing Then
                If bSuppress.HasValue Then Suppressed = bSuppress.Value
            End If
            If aLastValue IsNot Nothing Then
                _LastValue = TPROPERTY.SetValueByType(aLastValue, PropertyType)

            Else
                _LastValue = TPROPERTY.SetValueByType(_LastValue, PropertyType)
            End If
            Return _rVal
        End Function
        Public Function CopyValue(bProp As dxoProperty) As Boolean
            If bProp Is Nothing Or DoNotCopy Then Return False
            If bProp.PropertyType > 0 Then
                If PropertyType <= 0 Then PropertyType = bProp.PropertyType
            End If
            Return SetVal(bProp.Value)

        End Function


        Friend Function CopyValue(bProp As TPROPERTY) As Boolean
            If DoNotCopy Then Return False
            If bProp.PropType > 0 Then
                If PropertyType <= 0 Then PropertyType = bProp.PropType
            End If
            Return SetVal(bProp.Value)

        End Function
        Public Function StringValue(Optional bSwitchesAsBooleans As Boolean = True, Optional bReturnHandleLongs As Boolean = False, Optional bDecoded As Boolean = False, Optional bReturnLastValue As Boolean = False) As String
            Dim _rVal As String
            Dim aVal As Object = IIf(bReturnLastValue, LastValue, Value)
            Dim aGC As Integer = GroupCode
            Dim ptype As dxxPropertyTypes = PropertyType
            If String.IsNullOrWhiteSpace(DecodeString) Then bDecoded = False

            If ptype = dxxPropertyTypes.Undefined Then
                ptype = TPROPERTY.GetSetType(aGC, aVal, ptype, NonDXF)
            End If
            aVal = TPROPERTY.SetValueByType(aVal, ptype)
            Select Case ptype
                Case dxxPropertyTypes.dxf_Variant
                    _rVal = TVALUES.To_STR(aVal)
                Case dxxPropertyTypes.dxf_Integer
                    _rVal = TVALUES.To_STR(aVal)
                Case dxxPropertyTypes.Switch
                    If bSwitchesAsBooleans Then
                        If aVal = 1 Then _rVal = "True" Else _rVal = "False"
                    Else
                        _rVal = TVALUES.To_STR(aVal)
                    End If
                Case dxxPropertyTypes.dxf_Long, dxxPropertyTypes.Color, dxxPropertyTypes.BitCode
                    _rVal = TVALUES.To_STR(aVal)
                Case dxxPropertyTypes.dxf_Single
                    If TVALUES.To_STR(aVal).IndexOf("E", StringComparison.OrdinalIgnoreCase) < 0 Then
                        _rVal = Format(aVal, "0.0#########")
                    Else
                        _rVal = TVALUES.To_STR(aVal)
                    End If
                Case dxxPropertyTypes.dxf_Single
                    If aVal <> 0 Then
                        If TVALUES.To_STR(aVal).IndexOf("E", StringComparison.OrdinalIgnoreCase) < 0 Then
                            _rVal = Format(aVal, "0.0#########")
                        Else
                            _rVal = TVALUES.To_STR(aVal)
                        End If
                    Else
                        _rVal = "0.0"
                    End If
                Case dxxPropertyTypes.Angle
                    If TVALUES.To_STR(aVal).IndexOf("E", StringComparison.OrdinalIgnoreCase) < 0 Then
                        _rVal = Format(aVal, "0.0#########")
                    Else
                        _rVal = TVALUES.To_STR(aVal)
                    End If
                Case dxxPropertyTypes.dxf_Double
                    If TVALUES.To_STR(aVal).IndexOf("E", StringComparison.OrdinalIgnoreCase) < 0 Then
                        _rVal = Format(aVal, "0.0#########")
                    Else
                        _rVal = TVALUES.To_STR(aVal)
                    End If
                Case dxxPropertyTypes.dxf_Boolean
                    If bSwitchesAsBooleans Then
                        _rVal = TVALUES.To_STR(aVal)
                    Else
                        If aVal = True Then _rVal = 1 Else _rVal = 0
                    End If
                Case dxxPropertyTypes.dxf_String
                    _rVal = aVal
                Case dxxPropertyTypes.ClassMarker
                    _rVal = aVal
                Case dxxPropertyTypes.Handle, dxxPropertyTypes.Pointer
                    If bReturnHandleLongs Then
                        _rVal = aVal.ToString().Trim()
                        _rVal += "{" + TVALUES.HexToInteger(_rVal) + "}"
                    Else
                        _rVal = aVal
                    End If
                Case Else
                    _rVal = TVALUES.To_STR(aVal)
            End Select
            If bDecoded Then
                _rVal = TPROPERTY.Decode(_rVal, DecodeString, bDecoded)
            End If
            Return _rVal
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New dxoProperty(Me)
        End Function

        Friend Function Clone() As dxoProperty
            '^returns a new object with properties matching those of the cloned object
            Return New dxoProperty(Me)
        End Function

        Public Function Signature(Optional bSuppressName As Boolean = False, Optional bReturnHandleLongs As Boolean = False, Optional aPostScript As String = Nothing, Optional bDecoded As Boolean = False, Optional bSwitchesAsBooleans As Boolean = False) As String
            Dim _rVal As String
            Dim aStr As String

            If DecodeString = "" Then bDecoded = False
            aStr = StringValue(bSwitchesAsBooleans:=bSwitchesAsBooleans, bReturnHandleLongs:=bReturnHandleLongs, bDecoded:=bDecoded)
            If Name = "" Or bSuppressName Then
                _rVal = $"{GroupCode } = { aStr}"
            Else
                If Not NonDXF Then
                    _rVal = $"{GroupCode }({ Name }) = { aStr}"
                Else
                    _rVal = $"{Name }={ aStr}"
                End If
            End If
            If Not String.IsNullOrWhiteSpace(aPostScript) Then _rVal += $"({ aPostScript })"
            Return _rVal
        End Function
        Public Overrides Function ToString() As String
            Return Signature(bDecoded:=True)
        End Function
        Public Function Compare(bProp As dxoProperty, Optional aPrecis As Integer = 5, Optional bCompareNames As Boolean = False, Optional bCompareHandles As Boolean = True, Optional bComparePointers As Boolean = True) As Boolean
            If bProp Is Nothing Then Return False
            If GroupCode <> bProp.GroupCode Then Return False
            If bCompareNames And String.Compare(Name, bProp.Name, True) <> 0 Then Return False
            If Not bCompareHandles And PropertyType = dxxPropertyTypes.Handle Then Return True
            If Not bComparePointers And PropertyType = dxxPropertyTypes.Pointer Then Return True
            Dim _rVal As Boolean
            Dim tname As String
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            If PropertyType > 0 Then
                tname = TPROPERTY.TypeNameByPropertyType(PropertyType)
            Else
                tname = TPROPERTY.TypeNameByGC(GroupCode)
            End If
            If tname = "STRING" Or GroupCode < 0 Then
                _rVal = String.Compare(Value, bProp.Value, True) = 0
            Else
                _rVal = TVALUES.CompareNumbers(Value, bProp.Value, aPrecis)
            End If

            Return _rVal
        End Function

#End Region 'Methods


    End Class 'dxoProperty
End Namespace
