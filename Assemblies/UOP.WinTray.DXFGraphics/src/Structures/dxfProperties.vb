Imports System.Windows.Controls
Imports System.Windows.Documents
Imports SharpDX.Direct2D1
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics.Structures

#Region "Structures"
    Friend Structure TPROPERTY
        Implements ICloneable
#Region "Members"

        Public PropType As dxxPropertyTypes
        Public Value As Object
        Public SuppressedValue As Object
        Public LastValue As Object

        Public Color As Integer
        Public DecodeString As String
        Public GroupCode As Integer
        Public Follows As Integer
        Public Index As Integer

        Public LineNo As Integer
        Public Mark As Boolean
        Public NonDXF As Boolean
        Public Occurance As Integer
        Public Path As String
        Public Prompt As String
        Public Scaleable As Boolean
        Public Suppressed As Boolean
        Public Max As Double?
        Public Min As Double?
        Public ValueControl As mzValueControls
        Public IsOrdinate As Boolean
        Friend _Hidden As Boolean
        Friend _Name As String
        Friend _EnumName As [Enum]
        Public Description As String
        Friend _EnumValueType As Type
        Friend _Key As String

        Friend _Reactors As TDICTIONARYENTRIES
        Friend _GroupName As String
        Friend DoNotCopy As Boolean
#End Region 'Members
#Region "Constructors"



        Public Sub New(Optional aName As String = "")
            'init ------------------------------------
            Color = -1
            DecodeString = ""
            Follows = 0
            GroupCode = -1
            Index = -1

            _GroupName = ""
            LastValue = ""
            LineNo = 0

            Mark = False
            NonDXF = False
            Occurance = 1
            Path = ""
            Prompt = ""
            Scaleable = False
            Suppressed = False
            SuppressedValue = Nothing
            PropType = dxxPropertyTypes.Undefined
            Value = String.Empty
            _EnumName = Nothing
            ValueControl = mzValueControls.None
            Description = ""
            Max = Nothing
            Min = Nothing
            _EnumValueType = Nothing
            _Key = ""
            _Hidden = False
            _Reactors = New TDICTIONARYENTRIES(2, 330)
            IsOrdinate = False
            _IsHeader = Nothing
            _Name = IIf(Not String.IsNullOrWhiteSpace(aName), aName.Trim(), "")
            DoNotCopy = False
            'init ------------------------------------

        End Sub

        Public Sub New(aCode As Integer, aValue As Object, aName As String, Optional aPropType As dxxPropertyTypes = dxxPropertyTypes.Undefined, Optional aLastVal As Object = Nothing, Optional aValControl As mzValueControls = mzValueControls.Undefined, Optional bScalable As Boolean = False, Optional aSuppressedVal As Object = Nothing, Optional bHidden As Boolean = False,
                        Optional bNonDXF As Boolean = False, Optional bSuppressed As Boolean = False, Optional bIsOrdinate As Boolean = False, Optional aDecodeString As String = "", Optional bSetSuppressedValue As Boolean = False, Optional aEnumName As [Enum] = Nothing)

            'init ------------------------------------
            _Name = IIf(Not String.IsNullOrWhiteSpace(aName), aName.Trim(), "")
            PropType = IIf(aPropType <> dxxPropertyTypes.Undefined, aPropType, TPROPERTY.TypeByGC(aCode))
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
            _Reactors = New TDICTIONARYENTRIES(2, 330)
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
            PropType = TPROPERTY.GetSetType(aCode, aValue, isenum, tname, PropType, NonDXF)
            Value = aValue
            LastValue = Value

            If aLastVal IsNot Nothing Then

                TPROPERTY.GetSetType(GroupCode, aLastVal, PropType, NonDXF)
                LastValue = aLastVal
            End If

            If PropType = dxxPropertyTypes.dxf_Single Or PropType = dxxPropertyTypes.dxf_Double Or PropType = dxxPropertyTypes.dxf_Integer Then
                Scaleable = bScalable
            End If



            If PropType = dxxPropertyTypes.Color And DecodeString = "" Then
                DecodeString = dxfGlobals.ColorDecode
            End If

            Suppressed = bSuppressed
            If PropType <> dxxPropertyTypes.dxf_Variant And PropType <> dxxPropertyTypes.Handle Then
                If aSuppressedVal IsNot Nothing Then
                    SuppressedValue = TPROPERTY.SetValueByType(aSuppressedVal, PropType, Value, GroupCode)
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

        Public Sub New(aProperty As TPROPERTY)
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
            PropType = aProperty.PropType
            Value = IIf(aProperty.Value Is Nothing, Nothing, Force.DeepCloner.DeepClonerExtensions.DeepClone(Of Object)(aProperty.Value))
            _EnumName = aProperty._EnumName
            ValueControl = aProperty.ValueControl
            Description = aProperty.Description
            Max = aProperty.Max
            Min = aProperty.Min

            _EnumValueType = aProperty._EnumValueType
            _Key = aProperty.Key
            _Hidden = aProperty._Hidden
            _Reactors = New TDICTIONARYENTRIES(aProperty._Reactors)
            IsOrdinate = aProperty.IsOrdinate
            _IsHeader = aProperty._IsHeader
            _Name = aProperty._Name
            DoNotCopy = aProperty.DoNotCopy
            'init ------------------------------------

        End Sub

        Public Sub New(aProperty As dxoProperty)
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
            PropType = dxxPropertyTypes.Undefined
            Value = String.Empty
            _EnumName = Nothing
            ValueControl = mzValueControls.None
            Description = String.Empty
            Max = Nothing
            Min = Nothing
            _EnumValueType = Nothing
            _Key = String.Empty
            _Hidden = False
            _Reactors = New TDICTIONARYENTRIES(2, 330)
            IsOrdinate = False
            _IsHeader = Nothing
            _Name = String.Empty
            DoNotCopy = False
            'init ------------------------------------
            If aProperty Is Nothing Then Return
            Name = aProperty.Name
            Color = aProperty.Color
            DecodeString = aProperty.DecodeString
            Follows = aProperty.Follows
            GroupCode = aProperty.GroupCode
            Index = aProperty.Index

            _GroupName = aProperty.GroupName
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
            PropType = aProperty.PropertyType
            Value = IIf(aProperty.Value Is Nothing, Nothing, Force.DeepCloner.DeepClonerExtensions.DeepClone(Of Object)(aProperty.Value))
            _EnumName = aProperty._EnumName
            ValueControl = aProperty.ValueControl
            Description = aProperty.Description
            Max = aProperty.Max
            Min = aProperty.Min

            _EnumValueType = aProperty._EnumValueType
            _Key = aProperty.Key
            _Hidden = aProperty._Hidden
            ' _Reactors = New TDICTIONARYENTRIES(aProperty._Reactors)
            IsOrdinate = aProperty.IsOrdinate
            _IsHeader = aProperty._IsHeader
            _Name = aProperty._Name
            DoNotCopy = aProperty.DoNotCopy
            'init ------------------------------------

        End Sub
#End Region 'Constructors
#Region "Properties"
        Friend Property EnumValueType As Type
            Get
                Return _EnumValueType
            End Get
            Set(value As Type)
                _EnumValueType = value
            End Set
        End Property
        Friend Property EnumName As [Enum]
            Get
                Return _EnumName
            End Get
            Set(value As [Enum])
                _EnumName = value
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
        Public ReadOnly Property IsPointer As Boolean
            Get
                Dim _rVal As Boolean
                If PropType = dxxPropertyTypes.Pointer Then
                    _rVal = True
                Else
                    If (Math.Abs(GroupCode) >= 320 And Math.Abs(GroupCode) <= 369) Or (Math.Abs(GroupCode) >= 390 And Math.Abs(GroupCode) <= 399) Or Math.Abs(GroupCode) = 1005 Then
                        _rVal = True 'strings (hex handles)
                    End If
                End If
                Return _rVal
            End Get
        End Property
        Public Property Reactors As TDICTIONARYENTRIES
            Get
                _Reactors.HandleGroupCode = 330
                Return _Reactors
            End Get
            Set(value As TDICTIONARYENTRIES)
                _Reactors = value
            End Set
        End Property
        Friend ReadOnly Property DataTypeCode As Integer
            Get
                Dim _rVal As Integer = 0
                'On Error Resume Next
                Select Case PropType
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
        Public Property Key As String
            Get
                If String.IsNullOrEmpty(Name) Then Name = ""
                If String.IsNullOrEmpty(_Key) Then Return Name.ToUpper Else Return _Key.ToUpper
            End Get
            Set(value As String)
                _Key = value
            End Set
        End Property

        Public Property ValueD As Double
            Get
                Return TVALUES.To_DBL(Value)
            End Get
            Set(value As Double)
                SetVal(value)
            End Set
        End Property


        Public Property ValueI As Integer
            Get
                Return TVALUES.ToInteger(Value)
            End Get
            Set(value As Integer)
                SetVal(value)
            End Set
        End Property

        Public Property ValueL As Long
            Get
                Return TVALUES.ToLong(Value)
            End Get
            Set(value As Long)
                SetVal(value)
            End Set
        End Property
        Public Property ValueB As Boolean
            Get
                Return TVALUES.ToBoolean(Value, False, PropType = dxxPropertyTypes.Switch)
            End Get
            Set(value As Boolean)
                SetVal(value)
            End Set
        End Property

        Public Property ValueS As String
            Get
                Return StringValue()
            End Get
            Set(value As String)
                SetVal(value)
            End Set
        End Property

#End Region 'Properties
#Region "Methods"
        Public Function Signature(Optional bSuppressName As Boolean = False, Optional bReturnHandleLongs As Boolean = False, Optional aPS As String = "", Optional bDecoded As Boolean = False, Optional bSwitchesAsBooleans As Boolean = False) As String
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
            If aPS <> "" Then _rVal += $"({ aPS })"
            Return _rVal
        End Function

        Public Function StringValue(Optional bSwitchesAsBooleans As Boolean = True, Optional bReturnHandleLongs As Boolean = False, Optional bDecoded As Boolean = False, Optional bReturnLastValue As Boolean = False) As String
            Dim _rVal As String
            Dim aVal As Object
            Dim aGC As Integer
            Dim ptype As dxxPropertyTypes
            aGC = GroupCode

            If Not bReturnLastValue Then
                aVal = Value
            Else
                aVal = LastValue
            End If
            If DecodeString = "" Then bDecoded = False

            ptype = PropType
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

        Public Function Clone(Optional bCloneReactors As Boolean = False) As TPROPERTY
            Return New TPROPERTY(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TPROPERTY(Me)
        End Function
        Public Overrides Function ToString() As String
            Return Signature(bDecoded:=True)
        End Function
        Public Sub Print(Optional bDXFFormat As Boolean = False, Optional aStream As IO.StreamWriter = Nothing, Optional bShowIndices As Boolean = False, Optional aPrefix As String = "")
            Dim aStr As String = String.Empty
            If Not bDXFFormat Then
                If Name <> "" Then aStr = $"[{ Name }]"
                If bShowIndices Then aStr = $"{Index } - { aStr}"
                If aStream IsNot Nothing Then
                    aStream.WriteLine(aPrefix & aStr & Signature(True))
                Else
                    System.Diagnostics.Debug.WriteLine(aPrefix & aStr & Signature(True))
                End If
            Else
                If IsHeader Then
                    If aStream IsNot Nothing Then
                        aStream.WriteLine($"{aPrefix}9")
                        aStream.WriteLine($"{aPrefix}{Name}")
                    Else
                        System.Diagnostics.Debug.WriteLine($"{aPrefix}9")
                        System.Diagnostics.Debug.WriteLine($"{aPrefix}{Name}")
                    End If
                End If
                If aStream IsNot Nothing Then
                    aStream.WriteLine($"{aPrefix}{GroupCode}")
                    aStream.WriteLine($"{aPrefix}{StringValue()}")
                Else
                    System.Diagnostics.Debug.WriteLine($"{aPrefix}{GroupCode}")
                    System.Diagnostics.Debug.WriteLine($"{aPrefix}{StringValue()}")
                End If

            End If
        End Sub
        Public Function Compare(bProp As TPROPERTY, Optional aPrecis As Integer = 5, Optional bCompareNames As Boolean = False, Optional bCompareHandles As Boolean = True, Optional bComparePointers As Boolean = True) As Boolean
            If GroupCode <> bProp.GroupCode Then Return False
            If bCompareNames And String.Compare(Name, bProp.Name, True) <> 0 Then Return False
            If Not bCompareHandles And PropType = dxxPropertyTypes.Handle Then Return True
            If Not bComparePointers And PropType = dxxPropertyTypes.Pointer Then Return True
            Dim _rVal As Boolean
            Dim tname As String
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            If PropType > 0 Then
                tname = TPROPERTY.TypeNameByPropertyType(PropType)
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

        Public Function CopyValue(bProp As TPROPERTY) As Boolean
            If Not CopyValue Then Return False
            If bProp.PropType > 0 Then
                If PropType <= 0 Then PropType = bProp.PropType
            End If
            Return CopyValue(bProp)
        End Function
        Friend Function SetVal(aValue As Object, Optional aLastValue As Object = Nothing, Optional bSuppress As Double? = Nothing, Optional bIgnoreValueControl As Boolean = False) As Boolean
            If aValue Is Nothing Then Return False

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

            If aValue Is Nothing Then aValue = Value
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
            If _EnumValueType IsNot Nothing And PropType = dxxPropertyTypes.dxf_Integer Then
                If _EnumValueType <> GetType(dxxColors) Then
                    aValue = TVALUES.To_INT(aValue, TVALUES.To_INT(Value))
                    If Not dxfEnums.Validate(_EnumValueType, aValue) Then
                        Return False
                    End If
                End If
            End If
            aValue = TPROPERTY.SetValueByType(aValue, PropType, Value, vc)
            LastValue = Value
            LastValue = TPROPERTY.SetValueByType(LastValue, PropType, Value, vc)
            Value = aValue
            If PropType >= dxxPropertyTypes.dxf_String And PropType <= dxxPropertyTypes.ClassMarker Then
                Try
                    If LastValue Is Nothing Or Value Is Nothing Then
                        _rVal = True
                    ElseIf Value.ToString().Length <> LastValue.ToString().Length Then
                        _rVal = True
                    Else
                        _rVal = String.Compare(TVALUES.To_STR(Value), TVALUES.To_STR(LastValue), True) <> 0
                    End If
                Catch ex As Exception
                    _rVal = True
                End Try
            Else
                Try
                    If TVALUES.IsNumber(LastValue) Then
                        _rVal = LastValue.ToString() <> Value.ToString()
                    Else
                        _rVal = True
                    End If
                Catch ex As Exception
                    Try
                        If LastValue Is Nothing Or Value Is Nothing Then
                            _rVal = True
                        ElseIf Value.ToString().Length <> LastValue.ToString().Length Then
                            _rVal = True
                        Else
                            _rVal = String.Compare(TVALUES.To_STR(Value), TVALUES.To_STR(LastValue), True) <> 0
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
                aLastValue = TPROPERTY.SetValueByType(aLastValue, PropType)
                LastValue = aLastValue
            Else
                LastValue = TPROPERTY.SetValueByType(LastValue, PropType)
            End If
            Return _rVal
        End Function
#End Region 'Methods
#Region "Shared Methods"



        Public Shared ReadOnly Property Null
            Get
                Return New TPROPERTY("") With {.GroupCode = -1}
            End Get
        End Property
        Public Shared Function ReactorAdd(aProperty As TPROPERTY, aName As String, aHandle As String) As Boolean
            If String.IsNullOrEmpty(aHandle) Then Return False
            If aHandle = "0" Then Return False
            Dim reacts As TDICTIONARYENTRIES = aProperty.Reactors
            Dim _rVal As Boolean
            Dim aentry As TDICTIONARYENTRY = TDICTIONARYENTRY.Null
            If Not reacts.TryGet(aName, aHandle, aentry) Then
                _rVal = reacts.Add(aName, aHandle)
            End If
            aentry.Handle = aHandle
            aentry.Name = aName
            reacts.SetItem(aentry.Index, aentry)
            If _rVal Then aProperty.Reactors = reacts
            Return _rVal
        End Function
        Public Shared Function ReactorRemove(aProperty As TPROPERTY, aHandleorName As String) As Boolean
            Dim reacts As TDICTIONARYENTRIES = aProperty.Reactors
            If reacts.Count <= 0 Then Return False
            Dim aentry As TDICTIONARYENTRY = TDICTIONARYENTRY.Null
            If Not reacts.TryGet(aHandleorName, aentry) Then Return False
            reacts.Remove(aentry.Index)
            aProperty.Reactors = reacts
            Return True
        End Function
        Public Shared Function IsVector(aGC As Integer, ByRef rIsDirection As Boolean, Optional aOwnerType As String = "") As Boolean
            Dim _rVal As Boolean
            rIsDirection = False
            If aOwnerType Is Nothing Then aOwnerType = ""
            aOwnerType = aOwnerType.Trim().ToUpper()
            If aGC >= 10 And aGC <= 18 Then
                _rVal = True
                If aOwnerType = "UCS" And (aGC = 11 Or aGC = 12) Then rIsDirection = True
                If aOwnerType = "VIEW" And aGC = 11 Then rIsDirection = True
                If aOwnerType = "VPORT" And aGC = 16 Then
                    rIsDirection = True
                End If
            End If
            If aGC = 110 Or aGC = 111 Or aGC = 112 Or aGC = 210 Then
                _rVal = True
                If aGC = 111 Or aGC = 112 Or aGC = 210 Then rIsDirection = True
            End If
            Return _rVal
        End Function
        Public Shared Narrowing Operator CType(aProperty As TPROPERTY) As dxoProperty

            Return New dxoProperty(aProperty)
        End Operator



        Public Shared Function Decode(aStringValue As String, aDecodeString As String, ByRef rDecoded As Boolean, Optional aDelimiter As String = ",") As String
            rDecoded = False
            If Not aDecodeString.Contains("=") Or aStringValue = "" Then Return aStringValue
            Dim eqVals As TVALUES = TLISTS.ToValues(aDecodeString, aDelimiter)
            Dim compVals As TVALUES
            Dim i As Integer
            For i = 1 To eqVals.Count
                compVals = TLISTS.ToValues(eqVals.Item(i).ToString(), "=")
                If compVals.Count >= 2 Then
                    If String.Compare(aStringValue.Trim(), compVals.Member(0).ToString().Trim(), True) = 0 Then
                        rDecoded = True
                        Return aStringValue & "{" + compVals.Member(1).ToString() + "}"
                        Exit For
                    End If
                End If
            Next i
            Return aStringValue
        End Function
        Public Shared Function SwitchValue(aValue As Object) As Integer
            Dim _rVal As Integer
            If aValue Is Nothing Then Return _rVal
            If TypeOf aValue Is Boolean Then
                Dim bVal As Boolean = aValue
                If bVal Then _rVal = 1 Else _rVal = 0
            ElseIf TypeOf aValue Is String Then
                Dim sVal As String = aValue.ToString().Trim
                If sVal = "1" Then
                    _rVal = 1
                ElseIf sVal = "0" Then
                    _rVal = 0
                ElseIf String.Compare(sVal, "True", True) = 0 Then
                    _rVal = 1
                Else
                    _rVal = 0
                End If
            ElseIf TypeOf (aValue) Is Integer Or TypeOf (aValue) Is Long Then
                If aValue = 1 Then _rVal = 1 Else _rVal = 0
            Else
                If TVALUES.To_INT(aValue) = 1 Then _rVal = 1 Else _rVal = 0
            End If
            Return _rVal
        End Function
        Public Shared Function SetTypeByGroupCode(aGC As Integer, aValue As Object) As Object
            Dim arDataTypeName As String = String.Empty
            Dim rIsString As Boolean = False
            Return SetTypeByGroupCode(aGC, aValue, arDataTypeName, rIsString)
        End Function
        Public Shared Function SetTypeByGroupCode(aGC As Integer, aValue As Object, ByRef ioDataTypeName As String, ByRef rIsString As Boolean) As Object
            Dim _rVal As Object
            rIsString = False

            If aValue Is Nothing Then aValue = ""
            If aGC < 0 Then
                _rVal = aValue
                Return _rVal
            End If
            ioDataTypeName = ioDataTypeName.Trim().ToUpper()
            If ioDataTypeName = "" Then ioDataTypeName = TPROPERTY.TypeNameByGC(aGC)
            Select Case ioDataTypeName
                Case "STRING"
                    rIsString = True
                    _rVal = aValue.ToString 'strings
                Case "DOUBLE"
                    _rVal = TVALUES.To_DBL(aValue) 'doubles
                Case "INTEGER"
                    _rVal = TVALUES.To_INT(aValue, 0)
                Case "LONG"
                    _rVal = TVALUES.To_LNG(aValue, 0)
                Case Else
                    If TVALUES.IsNumber(aValue) Then
                        _rVal = TVALUES.To_DBL(aValue)
                    Else
                        rIsString = True
                        _rVal = aValue.ToString
                    End If
            End Select
            If aGC = 0 Then
                rIsString = True
                _rVal = _rVal.ToString().Trim()
            End If
            Return _rVal
        End Function
        Public Shared Function TypeNameByPropertyType(aPropertyType As dxxPropertyTypes) As String
            Try
                Select Case aPropertyType
                    Case dxxPropertyTypes.dxf_Variant
                        Return "VARIANT"
                    Case dxxPropertyTypes.dxf_Integer
                        Return "INTEGER"
                    Case dxxPropertyTypes.Switch
                        Return "SWITCH"
                    Case dxxPropertyTypes.dxf_Long, dxxPropertyTypes.Color, dxxPropertyTypes.BitCode
                        Return "LONG"
                    Case dxxPropertyTypes.dxf_Single
                        Return "SINGLE"
                    Case dxxPropertyTypes.Angle, dxxPropertyTypes.dxf_Double
                        Return "DOUBLE"
                    Case dxxPropertyTypes.dxf_Boolean
                        Return "BOOLEAN"
                    Case dxxPropertyTypes.dxf_String
                        Return "STRING"
                    Case dxxPropertyTypes.ClassMarker
                        Return "STRING"
                    Case dxxPropertyTypes.Handle, dxxPropertyTypes.Pointer
                        Return "STRING"

                    Case Else
                        Return "STRING"
                End Select
            Catch ex As Exception
                Return "STRING"
            End Try
        End Function
        Public Shared Function SetValueByType(aValue As Object, aPropertyType As dxxPropertyTypes, Optional aOldValue As Object = Nothing, Optional aValueControl As mzValueControls = mzValueControls.None) As Object
            Try
                If aValue Is Nothing Then aValue = String.Empty
                Dim _rVal As Object = aValue

                Select Case aPropertyType
                    Case dxxPropertyTypes.dxf_Variant
                        If aOldValue IsNot Nothing And aValueControl > 0 Then
                            If aOldValue IsNot Nothing Then
                                If aValueControl = mzValueControls.Positive And aValue < 0 Then _rVal = aOldValue
                                If aValueControl = mzValueControls.PositiveNonZero And aValue <= 0 Then _rVal = aOldValue
                            End If
                        End If
                    Case dxxPropertyTypes.dxf_Integer
                        _rVal = TVALUES.ToInteger(aValue, aDefault:=aOldValue, aValueControl:=aValueControl)
                    Case dxxPropertyTypes.Switch
                        If TPROPERTY.SwitchValue(_rVal) Then _rVal = 1 Else _rVal = 0
                    Case dxxPropertyTypes.dxf_Long, dxxPropertyTypes.Color, dxxPropertyTypes.BitCode
                        If aPropertyType = dxxPropertyTypes.BitCode Then aValueControl = mzValueControls.Positive
                        _rVal = TVALUES.ToLong(aValue, aDefault:=aOldValue, aValueControl:=aValueControl)
                    Case dxxPropertyTypes.dxf_Single
                        _rVal = TVALUES.ToSingle(aValue, aDefault:=aOldValue, aValueControl:=aValueControl)
                    Case dxxPropertyTypes.Angle
                        _rVal = TVALUES.ToDouble(aValue, aDefault:=aOldValue, aValueControl:=aValueControl)
                        _rVal = TVALUES.NormAng(TVALUES.To_DBL(_rVal), False, bReturnPosive:=True)
                    Case dxxPropertyTypes.dxf_Double
                        _rVal = TVALUES.ToDouble(aValue, aDefault:=aOldValue, aValueControl:=aValueControl)
                    Case dxxPropertyTypes.dxf_Boolean
                        _rVal = TPROPERTY.SwitchValue(_rVal) = 1
                    Case dxxPropertyTypes.dxf_String
                        _rVal = aValue.ToString()
                    Case dxxPropertyTypes.ClassMarker
                        _rVal = aValue.ToString().Trim()
                    Case dxxPropertyTypes.Handle, dxxPropertyTypes.Pointer
                        _rVal = aValue.ToString().Trim()
                        If _rVal = "" Then aValue = "0"

                End Select
                Return _rVal
            Catch ex As Exception
                Return aValue
            End Try
        End Function
        Public Shared Function TypeByGC(aGC As Integer) As String
            Dim rIsPointer As Boolean = False
            Return TypeByGC(aGC, rIsPointer)
        End Function
        Public Shared Function TypeByGC(aGC As Integer, ByRef rIsPointer As Boolean) As dxxPropertyTypes
            Dim _rVal As dxxPropertyTypes = dxxPropertyTypes.dxf_Variant
            If (aGC >= 0 And aGC <= 9) Or (aGC >= 100 And aGC <= 102) Or aGC = 105 Or
(aGC >= 300 And aGC <= 319) Or (aGC >= 410 And aGC <= 419) Or (aGC >= 430 And aGC <= 439) _
     Or (aGC >= 470 And aGC <= 479) Or (aGC >= 1000 And aGC <= 1009) Or aGC = 999 Then
                _rVal = dxxPropertyTypes.dxf_String
            ElseIf (aGC >= 10 And aGC <= 39) Or (aGC >= 40 And aGC <= 59) Or (aGC >= 110 And aGC <= 149) Or
(aGC >= 210 And aGC <= 239) Or (aGC >= 460 And aGC <= 469) Or (aGC >= 1010 And aGC <= 1059) Then
                _rVal = dxxPropertyTypes.dxf_Double
            ElseIf (aGC >= 60 And aGC <= 79) Or (aGC >= 170 And aGC <= 179) Or (aGC >= 270 And aGC <= 289) Or
(aGC >= 370 And aGC <= 389) Or (aGC >= 400 And aGC <= 409) Or (aGC >= 1060 And aGC <= 1070) Then
                _rVal = dxxPropertyTypes.dxf_Integer
            ElseIf (aGC >= 90 And aGC <= 99) Or (aGC >= 420 And aGC <= 429) Or (aGC >= 440 And aGC <= 449) Or
aGC = 1071 Then
                _rVal = dxxPropertyTypes.dxf_Long
            ElseIf aGC >= 290 And aGC <= 299 Then
                _rVal = dxxPropertyTypes.Switch
            ElseIf (aGC >= 320 And aGC <= 369) Or (aGC >= 390 And aGC <= 399) Or (aGC >= 1000 And aGC <= 1009) Then
                _rVal = dxxPropertyTypes.dxf_String
            End If
            rIsPointer = (aGC >= 320 And aGC <= 369) Or (aGC >= 390 And aGC <= 399) Or aGC = 1005
            Return _rVal
        End Function

        Public Shared Function TypeNameByGC(aGC As Integer) As String
            Dim rIsPointer As Boolean = False
            Return TypeNameByGC(aGC, rIsPointer)
        End Function
        Public Shared Function TypeNameByGC(aGC As Integer, ByRef rIsPointer As Boolean) As String
            Dim _rVal As String = String.Empty
            If (aGC >= 0 And aGC <= 9) Or (aGC >= 100 And aGC <= 102) Or aGC = 105 Or
(aGC >= 300 And aGC <= 319) Or (aGC >= 410 And aGC <= 419) Or (aGC >= 430 And aGC <= 439) _
     Or (aGC >= 470 And aGC <= 479) Or (aGC >= 1000 And aGC <= 1009) Or aGC = 999 Then
                _rVal = "STRING"
            ElseIf (aGC >= 10 And aGC <= 39) Or (aGC >= 40 And aGC <= 59) Or (aGC >= 110 And aGC <= 149) Or
(aGC >= 210 And aGC <= 239) Or (aGC >= 460 And aGC <= 469) Or (aGC >= 1010 And aGC <= 1059) Then
                _rVal = "DOUBLE"
            ElseIf (aGC >= 60 And aGC <= 79) Or (aGC >= 170 And aGC <= 179) Or (aGC >= 270 And aGC <= 289) Or
(aGC >= 370 And aGC <= 389) Or (aGC >= 400 And aGC <= 409) Or (aGC >= 1060 And aGC <= 1070) Then
                _rVal = "INTEGER"
            ElseIf (aGC >= 90 And aGC <= 99) Or (aGC >= 420 And aGC <= 429) Or (aGC >= 440 And aGC <= 449) Or
aGC = 1071 Then
                _rVal = "LONG"
            ElseIf (aGC >= 290 And aGC <= 299) Then
                _rVal = "SWITCH"
            ElseIf (aGC >= 320 And aGC <= 369) Or (aGC >= 390 And aGC <= 399) Or (aGC >= 1000 And aGC <= 1009) Then
                _rVal = "STRING"
            End If
            rIsPointer = (aGC >= 320 And aGC <= 369) Or (aGC >= 390 And aGC <= 399) Or aGC = 1005
            Return _rVal
        End Function
        Public Shared Function GetSetType(aGC As Integer, ByRef ioValue As Object, aType As dxxPropertyTypes, bNonNDXF As Boolean) As dxxPropertyTypes

            Return GetSetType(aGC, ioValue, False, "", aType, bNonNDXF)

        End Function
        Public Shared Function GetSetType(aGC As Integer, ByRef ioValue As Object, ByRef rIsEnumType As Boolean, ByRef rTypeName As String, aType As dxxPropertyTypes, bNonNDXF As Boolean) As dxxPropertyTypes
            Dim _rVal As dxxPropertyTypes = aType
            Try
                rIsEnumType = TypeOf ioValue Is [Enum]
                rTypeName = TypeName(ioValue)
                _rVal = aType
                If ioValue Is Nothing Then ioValue = ""
                If aGC < 0 Then bNonNDXF = True
                Dim tname As String = String.Empty
                Dim bPntr As Boolean
                Dim bHndl As Boolean
                If _rVal = dxxPropertyTypes.Undefined Then
                    If bNonNDXF Then
                        tname = rTypeName
                    Else
                        tname = TPROPERTY.TypeNameByGC(aGC, bPntr)
                        bHndl = aGC = 5
                    End If
                    Select Case tname.ToUpper()
                        Case "STRING", "DATE"
                            If bHndl Then
                                _rVal = dxxPropertyTypes.Handle
                            ElseIf bPntr Then
                                _rVal = dxxPropertyTypes.Pointer
                            Else
                                _rVal = dxxPropertyTypes.dxf_String
                            End If
                            'If bSuppressHandles Then
                            '    _rVal = dxxPropertyTypes.dxf_String
                            'End If
                        Case "SINGLE", "CURRENCY"
                            _rVal = dxxPropertyTypes.dxf_Single
                        Case "DOUBLE", "DECIMAL"
                            _rVal = dxxPropertyTypes.dxf_Double
                        Case "INTEGER"
                            _rVal = dxxPropertyTypes.dxf_Integer
                        Case "LONG"
                            _rVal = dxxPropertyTypes.dxf_Long
                        Case "SWITCH"
                            _rVal = dxxPropertyTypes.Switch
                        Case "BOOLEAN"
                            _rVal = dxxPropertyTypes.dxf_Boolean
                        Case Else
                            If rIsEnumType Then
                                _rVal = dxxPropertyTypes.dxf_Integer
                            Else
                                _rVal = dxxPropertyTypes.dxf_Variant
                            End If

                    End Select
                End If
                If _rVal <> dxxPropertyTypes.dxf_Variant Then
                    ioValue = TPROPERTY.SetValueByType(ioValue, _rVal)
                End If
            Catch ex As Exception
            End Try
            Return _rVal
        End Function

        Public Shared Function IsSuppressed(aProp As TPROPERTY) As Boolean
            Dim _rVal As Boolean
            'undefined
            If aProp.GroupCode < 0 Or aProp.Value Is Nothing Or aProp.Value Is Nothing Or aProp.Suppressed Then
                _rVal = True
            Else
                'matches defined supressing value (default)
                If aProp.SuppressedValue IsNot Nothing And aProp.Value IsNot Nothing Then
                    If String.Compare(TypeName(aProp.Value), "String", True) = 0 Or Not TVALUES.IsNumber(aProp.Value) Then
                        _rVal = String.Compare(aProp.Value, aProp.SuppressedValue, True) = 0
                    Else
                        _rVal = (aProp.Value = aProp.SuppressedValue)
                    End If
                Else
                    'explicitly suppressed or a negative group code
                    _rVal = aProp.Suppressed Or aProp.GroupCode < 0
                End If
            End If
            Return _rVal
        End Function
#End Region
    End Structure 'TPROPERTY

    Friend Structure TPROPERTIES
        Implements ICloneable
#Region "Members"
        Public Delimiter As Char
        Public IsDirty As Boolean
        Public Name As String
        Public NonDXF As Boolean
        Public Owner As String
        Public SuppressIndices As Boolean
        Friend _Members() As TPROPERTY
        Private _Init As Boolean

        'Private _Members As TDICTIONARY_PROP
        'Private _GroupCodes As TDICTIONARY_GCS
#End Region 'Members
#Region "Constructors"
        Public Sub New(Optional aName As String = "", Optional bNonDXF As Boolean = False)

            'init ------------------
            Name = ""
            NonDXF = False
            SuppressIndices = False
            IsDirty = False
            Owner = ""
            Delimiter = ","
            _Init = True
            ReDim _Members(-1)
            '_Members = New TDICTIONARY_PROP("")
            ' _GroupCodes = New TDICTIONARY_GCS("")
            'init ------------------
            If aName Is Nothing Then aName = ""
            Name = aName.Trim
            NonDXF = bNonDXF
        End Sub


        Public Sub New(aName As String, aMembers As IEnumerable(Of dxoProperty), Optional bDontCloneMembers As Boolean = False)
            'init ------------------
            Name = ""
            NonDXF = False
            SuppressIndices = False
            IsDirty = False
            Owner = ""
            Delimiter = ","
            _Init = True
            ReDim _Members(-1)
            '_Members = New TDICTIONARY_PROP("")
            '_GroupCodes = New TDICTIONARY_GCS("")
            'init ------------------
            If aName Is Nothing Then aName = ""

            Name = aName.Trim()
            NonDXF = False

            If aMembers Is Nothing Then Return
            If TypeOf aMembers Is dxoProperties Then
                Dim dprops As dxoProperties = DirectCast(aMembers, dxoProperties)
                Name = dprops.Name
                NonDXF = dprops.NonDXF
                IsDirty = dprops.IsDirty
                Owner = dprops.Owner
                Delimiter = dprops.Delimiter
            End If
            If bDontCloneMembers Then Return
            For Each prop As dxoProperty In aMembers
                Add(prop)
            Next

        End Sub
        Public Sub New(aMembers As IEnumerable(Of dxoProperty), Optional bDontCloneMembers As Boolean = False)
            'init ------------------
            Name = ""
            NonDXF = False
            SuppressIndices = False
            IsDirty = False
            Owner = ""
            Delimiter = ","
            _Init = True
            ReDim _Members(-1)
            '_Members = New TDICTIONARY_PROP("")
            '_GroupCodes = New TDICTIONARY_GCS("")
            'init ------------------

            If aMembers Is Nothing Then Return
            If TypeOf aMembers Is dxoProperties Then
                Dim dprops As dxoProperties = DirectCast(aMembers, dxoProperties)
                Name = dprops.Name
                NonDXF = dprops.NonDXF
                IsDirty = dprops.IsDirty
                Owner = dprops.Owner
                Delimiter = dprops.Delimiter
            End If
            If bDontCloneMembers Then Return
            For Each prop As dxoProperty In aMembers
                Add(prop)
            Next

        End Sub
        Public Sub New(aName As String, aMembers As IEnumerable(Of TPROPERTY))
            'init ------------------
            Name = ""
            NonDXF = False
            SuppressIndices = False
            IsDirty = False
            Owner = ""
            Delimiter = ","
            _Init = True
            ReDim _Members(-1)
            '_Members = New TDICTIONARY_PROP("")
            '_GroupCodes = New TDICTIONARY_GCS("")
            'init ------------------
            If aName Is Nothing Then aName = ""

            Name = aName.Trim()
            NonDXF = False

            If aMembers Is Nothing Then Return

            For Each prop As TPROPERTY In aMembers
                Add(prop)
            Next

        End Sub
        Public Sub New(aProperties As TPROPERTIES, Optional bDontCloneMembers As Boolean = False)
            'init ------------------
            Name = aProperties.Name
            NonDXF = aProperties.NonDXF
            SuppressIndices = aProperties.SuppressIndices
            IsDirty = aProperties.IsDirty
            Owner = aProperties.Owner
            Delimiter = aProperties.Delimiter
            _Init = True
            ReDim _Members(-1)
            '_Members = New TDICTIONARY_PROP("")
            '_GroupCodes = New TDICTIONARY_GCS("")
            'init ------------------
            If Not bDontCloneMembers Then
                Repopulate(aProperties.ToList(), True)
                '_Members = New TDICTIONARY_PROP(aProperties._Members)
                ' _GroupCodes = New TDICTIONARY_GCS(aProperties._GroupCodes)
            End If


        End Sub


#End Region 'Constructors
#Region "Properties"
        Public ReadOnly Property Count As Integer
            Get
                If Not _Init Then
                    ReDim _Members(-1)
                    _Init = True
                End If
                Return _Members.Count
            End Get
        End Property
        Public ReadOnly Property IsEmpty As Boolean
            Get
                Return Not _Init Or String.IsNullOrEmpty(Name)
            End Get
        End Property

        Public Property GUID As String
            Get
                Dim aMem As TPROPERTY = TPROPERTY.Null
                If TryGet("*GUID", aMem) Then
                    Return aMem.Value.ToString
                ElseIf TryGet("GUID", aMem) Then
                    Return aMem.Value.ToString
                Else
                    Return String.Empty
                End If
            End Get
            Set(value As String)
                Dim aMem As TPROPERTY = TPROPERTY.Null
                If TryGet("*GUID", aMem) Then
                    aMem.SetVal(value)
                    _Members(aMem.Index - 1) = aMem
                ElseIf TryGet("GUID", aMem) Then
                    aMem.SetVal(value)
                    _Members(aMem.Index - 1) = aMem
                End If
            End Set
        End Property
        Public Property Handle As String
            Get
                Dim occr As Integer = 1
                Dim pcnt As Integer
                Dim i As Integer
                Dim aMem As TPROPERTY = TPROPERTY.Null
                If occr <= 1 Then
                    If TryGet("Handle", aMem) Then
                        Return aMem.Value.ToString
                    End If
                End If
                For i = 1 To Count
                    aMem = _Members(i - 1)
                    If aMem.PropType = dxxPropertyTypes.Handle Then
                        pcnt += 1
                        If pcnt = occr Then
                            Return aMem.Value.ToString
                        End If
                    End If
                Next i
                Add(New TPROPERTY(5, "", "Handle", dxxPropertyTypes.Handle, ""))
                Return String.Empty
            End Get
            Set(value As String)
                Dim occr As Integer = 1
                Dim pcnt As Integer
                Dim aMem As TPROPERTY = TPROPERTY.Null
                If occr <= 1 Then
                    If TryGet("Handle", aMem) Then
                        aMem.SetVal(value)
                        _Members(aMem.Index - 1) = aMem
                        Return
                    End If
                End If
                For i = 1 To Count
                    aMem = _Members(i - 1)
                    If aMem.PropType = dxxPropertyTypes.Handle Then
                        pcnt += 1
                        If pcnt = occr Then
                            aMem.SetVal(value)
                            _Members(i - 1) = aMem
                        End If
                    End If
                Next i
            End Set
        End Property
        Public WriteOnly Property UpdateProperty As TPROPERTY
            Set(value As TPROPERTY)
                If IsEmpty Then Return
                If value.Index > 0 And value.Index < _Members.Count Then
                    If String.Compare(value.Key, _Members(value.Index - 1).Key, True) = 0 Then
                        _Members(value.Index - 1) = value
                        Return
                    End If
                End If
                If Not SetItem(value.Key, value) Then SetItem(value.Name, value)
            End Set
        End Property
#End Region 'Properties
#Region "Methods"
        Public Function HandleKey(Optional aOccurance As Integer = 1) As String
            Dim occr As Integer = Math.Abs(aOccurance)
            Dim pcnt As Integer
            Dim aMem As TPROPERTY
            For i As Integer = 1 To Count
                aMem = _Members(i - 1)
                If aMem.Key = "HANDLE" Or aMem.PropType = dxxPropertyTypes.Handle Then
                    pcnt += 1
                    If pcnt >= occr Then
                        Return aMem.Key
                    End If
                End If
            Next
            Return 0
        End Function
        Public Function Vector(aIndexOrKey As Object) As TVECTOR

            Dim aMem As TPROPERTY = TPROPERTY.Null
            If Not TryGetVector(aIndexOrKey, aMem) Then Return TVECTOR.Zero

            Dim idx As Integer = aMem.Index
            If idx <= 0 Then Return TVECTOR.Zero
            If aMem.IsHeader Then
                idx += 1
                aMem = Item(idx)
                If aMem.Index <= 0 Then Return TVECTOR.Zero
            End If
            Dim _rVal As New TVECTOR(0, 0, 0)
            If aMem.PropType = dxxPropertyTypes.dxf_Double Then _rVal.X = TVALUES.To_DBL(aMem.Value)
            Dim yMem As TPROPERTY = Item(idx + 1)
            If yMem.Index >= 0 And yMem.PropType = dxxPropertyTypes.dxf_Double And yMem.GroupCode = aMem.GroupCode + 10 Then _rVal.Y = TVALUES.To_DBL(yMem.Value)
            Dim zMem As TPROPERTY = Item(idx + 2)
            If zMem.Index >= 0 And zMem.PropType = dxxPropertyTypes.dxf_Double And zMem.GroupCode = aMem.GroupCode + 20 Then _rVal.Z = TVALUES.To_DBL(zMem.Value)

            Return _rVal
        End Function
        Public Function PropNames(Optional aSkipList As String = "", Optional bIncludeHidden As Boolean = False) As String
            Dim mynames As List(Of String) = Names(aSkipList, bIncludeHidden)

            Dim _rVal As String = String.Empty

            For i As Integer = 1 To mynames.Count
                If i > 1 Then _rVal += ","
                _rVal += mynames(i - 1)
            Next i
            Return _rVal
        End Function
        Public Function Names(Optional aSkipList As String = "", Optional bIncludeHidden As Boolean = False) As List(Of String)
            Dim _rVal As New List(Of String)
            Dim aMem As TPROPERTY

            For i As Integer = 1 To Count
                aMem = _Members(i - 1)
                If bIncludeHidden Or (Not bIncludeHidden And Not aMem.Hidden) Then
                    Dim str As String = aMem.Name
                    If _rVal.FindIndex(Function(x) String.Compare(x, str, True) = 0) < 0 Then
                        _rVal.Add(str)
                    End If
                End If
            Next i
            Return _rVal
        End Function
        Public Function StringValues(Optional aSkipList As String = "", Optional bIncludeHidden As Boolean = False) As List(Of String)
            Dim _rVal As New List(Of String)

            Dim str As String
            For i As Integer = 1 To Count
                Dim aMem As TPROPERTY = _Members(i - 1)
                If bIncludeHidden Or (Not bIncludeHidden And Not aMem.Hidden) Then
                    str = aMem.Name
                    If Not TLISTS.Contains(str, aSkipList, bReturnTrueForNullList:=True) Then
                        _rVal.Add(aMem.Value.ToString)
                    End If
                End If
            Next i
            Return _rVal
        End Function
        Public Function MemberType(aIndex As Integer) As dxxPropertyTypes
            '1 based
            If aIndex < 1 Or aIndex > Count Then Return dxxPropertyTypes.Undefined
            Return Item(aIndex).PropType
        End Function
        Public Function MemberTypeSet(aIndex As Integer, value As dxxPropertyTypes) As Boolean
            If aIndex < 1 Or aIndex > Count Then Return False
            Dim aMem As TPROPERTY = Item(aIndex)
            Dim _rVal As Boolean = aMem.PropType <> value
            aMem.PropType = value
            _Members(aIndex - 1) = aMem
            Return _rVal
        End Function
        Public Function GroupCode(aKey As String) As Integer
            Dim aMem As TPROPERTY = TPROPERTY.Null
            If Not TryGet(aKey.ToUpper, aMem) Then Return -1
            Return aMem.GroupCode
        End Function
        Public Function GroupCodeSet(aKey As String, value As Integer) As Boolean
            Dim aMem As TPROPERTY = TPROPERTY.Null
            If Not TryGet(aKey.ToUpper, aMem) Then Return False
            Dim _rVal As Boolean = aMem.GroupCode <> value
            aMem.GroupCode = value
            _Members(aMem.Index - 1) = aMem
            Return _rVal
        End Function
        Public Function GCValue(aGroupCode As Integer, Optional aOccurance As Integer = 1) As Object
            Dim aProp As TPROPERTY = TPROPERTY.Null
            If Not TryGet(aGroupCode, aProp, aOccur:=aOccurance) Then Return Nothing
            Return aProp.Value
        End Function
        Public Function GCValueSet(aGroupCode As Integer, value As Object, Optional aOccurance As Integer = 1)
            If value Is Nothing Then Return False
            Dim aProp As TPROPERTY = TPROPERTY.Null
            If Not TryGet(aGroupCode, aProp, aOccur:=aOccurance) Then Return False
            Dim _rVal As Boolean = aProp.SetVal(value)
            _Members(aProp.Index - 1) = aProp
            Return _rVal
        End Function
        Public Function Value(aIndexOrKey As Object, Optional aReturnType As dxxPropertyTypes = dxxPropertyTypes.Undefined) As Object
            'base 1
            Dim aMem As TPROPERTY = TPROPERTY.Null
            If TypeOf (aIndexOrKey) Is String Then
                If TryGet(TVALUES.To_STR(aIndexOrKey, bTrim:=True).ToUpper, aMem) Then
                    Return aMem.Value
                End If
                Return dxxPropertyTypes.Undefined
            End If
            aMem = Item(TVALUES.To_INT(aIndexOrKey))
            If aMem.Index >= 0 Then Return aMem.Value
            Return dxxPropertyTypes.Undefined
        End Function
        Public Function LastValue(aIndexOrKey As Object, Optional aReturnType As dxxPropertyTypes = dxxPropertyTypes.Undefined) As Object
            'base 1
            Dim aMem As TPROPERTY = TPROPERTY.Null
            If TypeOf (aIndexOrKey) Is String Then
                If TryGet(TVALUES.To_STR(aIndexOrKey, bTrim:=True).ToUpper, aMem) Then
                    Return aMem.LastValue
                End If
            Else
                aMem = Item(TVALUES.To_INT(aIndexOrKey))
                If aMem.Index < 0 Then Return String.Empty
                Return aMem.LastValue
            End If
            Return String.Empty
        End Function
        Public Function LastProperty(Optional bHidden As Boolean? = Nothing) As TPROPERTY
            'base 1
            If Not bHidden.HasValue Then
                If IsEmpty Then Return New TPROPERTY("") Else Return Item(Count)
            End If
            Dim hdn As Boolean = bHidden.Value
            Dim aMem As TPROPERTY
            For i As Integer = Count To 1 Step -1
                aMem = Item(i)
                If aMem.Hidden = hdn Then
                    Return aMem
                End If
            Next
            Return New TPROPERTY("")
        End Function

        ''' <summary>
        ''' used to see if one properties collection matches another
        ''' </summary>
        ''' <param name="bProperties">the properties to compare with</param>
        ''' <param name="aSkipList">a list of group codes or names to not include in the comparison</param>
        ''' <returns></returns>
        Public Function IsEqual(bProperties As TPROPERTIES, Optional aSkipList As String = "") As Boolean

            Return dxoProperties.IsEqual(Me, bProperties, aSkipList)

        End Function
        Public Function GroupCodes(Optional bUniquesOnly As Boolean = True, Optional aGCListToOmmit As String = "") As List(Of Integer)

            Dim _rVal As New List(Of Integer)
            Dim aProp As TPROPERTY
            If aGCListToOmmit Is Nothing Then aGCListToOmmit = ""
            aGCListToOmmit = aGCListToOmmit.Trim()

            For i As Integer = 1 To Count
                aProp = _Members(i - 1)

                Dim skip As Boolean = TLISTS.Contains(aProp.GroupCode, aGCListToOmmit)
                If Not skip Then
                    If bUniquesOnly Then
                        If _rVal.FindIndex(Function(x) x = aProp.GroupCode) >= 0 Then
                            Continue For
                        End If
                    End If
                    _rVal.Add(aProp.GroupCode)
                End If
            Next i
            Return _rVal

        End Function

        Public Function GroupCodeKey(aGroupCode As Integer, Optional aOccurance As Integer = 1) As String
            Dim mem As TPROPERTY = TPROPERTY.Null
            If Not TryGet(aGroupCode, mem, aOccurance) Then Return String.Empty Else Return mem.Key
        End Function
        Public Function GroupCodeIndex(aGroupCode As Integer, Optional aOccurance As Integer = 1) As Integer
            Return GetIndex(aGroupCode, True, False, aOccurance)
        End Function
        Public Function GroupCodeMembers(aGroupCode As Integer) As List(Of TPROPERTY)
            Dim _rVal As New List(Of TPROPERTY)

            Dim aMem As TPROPERTY
            For i As Integer = 1 To Count
                aMem = _Members(i - 1)
                If aMem.GroupCode = aGroupCode Then
                    aMem.Index = i
                    _rVal.Add(aMem)
                End If
            Next
            Return _rVal
        End Function
        Public Sub SetLast(aProperty As TPROPERTY, Optional bHidden As Boolean? = Nothing)

            If aProperty.Name = "" Then Return
            Dim aMem As TPROPERTY
            If Not bHidden.HasValue Then
                If IsEmpty Then Return Else SetItem(Count, aProperty)
                ReIndex()
                Return
            End If
            Dim hdn As Boolean = bHidden.Value
            For i As Integer = Count To 1 Step -1
                aMem = Item(i)
                If aMem.Hidden = hdn Then
                    aProperty.Name = aMem.Name
                    _Members(i - 1) = aProperty
                    ReIndex()
                    Return
                End If
            Next
        End Sub
        Public Function Item(aKeyOrGC As String) As TPROPERTY
            'base 1
            Dim _rVal As TPROPERTY = TPROPERTY.Null
            If Not TryGet(aKeyOrGC, _rVal) Then Return _rVal
            _rVal.NonDXF = NonDXF
            _Members(_rVal.Index - 1) = _rVal
            Return _rVal
        End Function
        Public Function SetItem(aKeyOrGC As String, value As TPROPERTY) As Boolean

            Dim aMem As TPROPERTY = TPROPERTY.Null
            If TryGet(aKeyOrGC, aMem) Then
                value.Name = aMem.Name
                value.Key = aMem.Key
                _Members(aMem.Index - 1) = value
                Return True
            Else
                Return False
            End If
        End Function
        Public Function Item(aIndex As Integer) As TPROPERTY
            'base 1
            If aIndex < 1 Or aIndex > Count Then Return TPROPERTY.Null
            Dim _rVal As TPROPERTY = _Members(aIndex - 1)
            If Not SuppressIndices Then
                _rVal.Index = aIndex
                _Members(aIndex - 1) = _rVal
            End If
            _rVal.NonDXF = NonDXF
            Return _rVal
        End Function
        Public Sub SetItem(aIndex As Integer, value As TPROPERTY)

            If aIndex < 1 Or aIndex > Count Then Return
            Dim _rVal As TPROPERTY = _Members(aIndex - 1)
            If Not SuppressIndices Then value.Index = aIndex
            value.Key = _rVal.Key
            _Members(aIndex - 1) = value
        End Sub
        Public Function Clone(Optional bReturnEmpty As Boolean = False) As TPROPERTIES
            Return New TPROPERTIES(Me, bReturnEmpty)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TPROPERTIES(Me)
        End Function
        Public Overrides Function ToString() As String
            Dim _rVal As String = $"TPROPERTIES({ Count })"
            If Name <> "" Then _rVal += $"[{ Name }]"
            Return _rVal
        End Function
        Public Function ContainsKey(aKey As String) As Boolean
            Dim mem As TPROPERTY = TPROPERTY.Null
            Return TryGetByKey(aKey, mem, bSearchByNameOnly:=True)
        End Function

        Public Function TryGetByKey(aKey As String, ByRef rMember As TPROPERTY, Optional bSearchByNameOnly As Boolean = False) As Boolean
            rMember = TPROPERTY.Null
            If IsEmpty Then Return False
            Dim idx As Integer = _Members.ToList().FindIndex(Function(x) String.Compare(x.Key, aKey, True) = 0)
            If idx >= 0 Then
                _Members(idx).Index = idx + 1
                rMember = _Members(idx)
            End If
            Return idx >= 0
        End Function

        Public Function TryGet(aKey As String, ByRef rMember As TPROPERTY, Optional bSearchByNameOnly As Boolean = False) As Boolean
            rMember = TPROPERTY.Null
            If IsEmpty Then Return False

            Try
                Dim mems = _Members.Where(Function(x) String.Compare(x.Key, aKey, True) = 0 Or String.Compare(x.Name, aKey, True) = 0)
                If mems.Count > 0 Then
                    Dim idx = Array.IndexOf(Of TPROPERTY)(_Members, mems(0))
                    _Members(idx).Index = idx + 1
                    rMember = _Members(idx)
                    Return True
                Else
                    If bSearchByNameOnly Then Return False
                End If



                If TVALUES.IsNumber(aKey) Then
                    mems = _Members.Where(Function(x) x.GroupCode = TVALUES.To_INT(aKey))
                    If mems.Count <= 0 Then
                        Return False
                    Else
                        Dim idx = Array.IndexOf(Of TPROPERTY)(_Members, mems(0))
                        _Members(idx).Index = idx + 1
                        rMember = _Members(idx)
                        Return True
                    End If

                Else
                    Return False
                End If

            Catch ex As Exception
                rMember = TPROPERTY.Null
                Return False
            End Try

        End Function

        Public Function TryGet(aGC As Integer, ByRef rMember As TPROPERTY, Optional aOccur As Integer = 1) As Boolean
            rMember = TPROPERTY.Null
            If IsEmpty Then Return False

            Dim mems = _Members.Where(Function(x) x.GroupCode = aGC)
            If mems.Count <= 0 Then Return False

            If aOccur <= 1 Then
                Dim idx = Array.IndexOf(Of TPROPERTY)(_Members, mems(0))
                _Members(idx).Index = idx + 1
                rMember = _Members(idx)
                Return True
            Else
                If aOccur > mems.Count Then Return False
                Dim idx = Array.IndexOf(Of TPROPERTY)(_Members, mems(aOccur - 1))
                _Members(idx).Index = idx + 1
                rMember = _Members(idx)

                Return True
            End If


        End Function

        Public Function TryGetVector(aKeyOrGC As String, ByRef rMember As TPROPERTY, Optional bSearchByNameOnly As Boolean = False) As Boolean
            rMember = TPROPERTY.Null
            If IsEmpty Then Return False

            Dim vname As String = aKeyOrGC.ToUpper().Trim()
            Dim _rVal As Boolean = TryGet(vname, rMember, bSearchByNameOnly)
            If Not _rVal And Not vname.EndsWith("_X") Then
                _rVal = TryGet(vname + "_X", rMember, bSearchByNameOnly)
            End If


            Return _rVal
        End Function

        Public Function GetIndex(aKeyOrGC As String, Optional bGroupCodePassed As Boolean = False, Optional bSearchByNameOnly As Boolean = False, Optional aOccurance As Integer = 1) As Integer
            If IsEmpty Or String.IsNullOrWhiteSpace(aKeyOrGC) Then Return 0

            aKeyOrGC = aKeyOrGC.Trim().ToUpper()
            If aKeyOrGC = "" Then Return 0
            Dim aMem As TPROPERTY = TPROPERTY.Null
            Dim _rVal As Integer = 0
            'search byName
            If Not bGroupCodePassed Then
                If Not TryGet(aKeyOrGC, aMem, bSearchByNameOnly) Then
                    If bSearchByNameOnly Then Return 0
                Else
                    Return aMem.Index
                End If
            End If
            If _rVal = 0 And TVALUES.IsNumber(aKeyOrGC) Then
                If Not TryGet(TVALUES.To_INT(aKeyOrGC), aMem, aOccurance) Then
                    Return 0
                Else
                    Return aMem.Index
                End If
            End If

            Return _rVal
        End Function
        Public Function HiddenMembers() As TPROPERTIES
            Dim _rVal As TPROPERTIES = Clone(bReturnEmpty:=True)

            For i As Integer = 1 To Count
                Dim aMem As TPROPERTY = _Members(i - 1)
                If aMem.Hidden Then _rVal.Add(aMem)
            Next i
            Return _rVal
        End Function

        Public Function HiddenMemberCount() As Integer
            Dim _rVal As Integer = 0

            For i As Integer = 1 To Count
                Dim aMem As TPROPERTY = _Members(i - 1)
                If aMem.Hidden Then _rVal += 1
            Next i
            Return _rVal
        End Function


        Public Function Seek(aGroupCode As Integer, ByRef rIndex As Integer, Optional aOccurance As Integer = 1, Optional aStartID As Integer = 1, Optional bReverseSearch As Boolean = False) As Boolean
            rIndex = 0
            If IsEmpty Then Return False
            If aOccurance <= 0 Then aOccurance = 1
            Dim igc As Integer
            Dim si As Integer = 1
            Dim cnt As Integer = Count
            Dim ei As Integer = cnt
            Dim stp As Integer = 1
            Dim aMem As TPROPERTY
            'search forward
            If Not bReverseSearch Then
                If aStartID >= si And aStartID <= ei Then si = aStartID
            Else
                'search backwards
                stp = -1
                si = cnt
                ei = 1
                If aStartID >= ei And aStartID <= si Then ei = aStartID
            End If
            For i As Integer = si To ei Step stp
                aMem = _Members(i - 1)
                If aMem.GroupCode = aGroupCode Then
                    igc += 1
                    If igc = aOccurance Then
                        rIndex = i
                        Return True
                    End If
                End If
            Next i
            Return False
        End Function
        Public Function Add(aProperty As dxoProperty, Optional bHidden As Boolean? = Nothing, Optional bIsHeader As Boolean? = Nothing) As TPROPERTY
            If aProperty Is Nothing Then Return New TPROPERTY("")
            Return Add(New TPROPERTY(aProperty), bHidden, bIsHeader)
        End Function

        Friend Function Add(aProperty As TPROPERTY, Optional bHidden As Boolean? = False, Optional bIsHeader As Boolean? = Nothing) As TPROPERTY


            Dim idx As Integer
            Dim pname As String = aProperty.Name.Trim()
            Dim sufx As String = String.Empty
            If pname = "" Then pname = "Prop"
            aProperty.Name = pname
            Dim exisiting As TPROPERTY = TPROPERTY.Null
            If Count > 0 Then
                Do While TryGetByKey($"{pname }{ sufx}", exisiting) '' _Members.ContainsKey($"{pname }{ sufx}")
                    idx += 1
                    sufx = $"_{idx}"
                Loop
            End If
            If idx > 0 Then
                pname += sufx
                aProperty.Key = pname
            End If
            If pname.StartsWith("*") Or bHidden Then aProperty.Hidden = True
            aProperty.Index = _Members.Count + 1
            If bIsHeader.HasValue Then aProperty.IsHeader = bIsHeader.Value

            System.Array.Resize(_Members, _Members.Length + 1)
            _Members(_Members.Count - 1) = aProperty
            '_GroupCodes.Add(aProperty)

            Return _Members(_Members.Count - 1)

        End Function
        Public Function GetMember(aGCOrKey As Object, Optional aOccurance As Integer = 1) As TPROPERTY
            Dim aMem As TPROPERTY = TPROPERTY.Null
            TryGet(TVALUES.To_STR(aGCOrKey), aMem)
            Return aMem
        End Function
        Public Function Add(aGroupCode As Integer, aValue As Object, aName As String, aPropertyType As dxxPropertyTypes, bScalable As Boolean) As TPROPERTY
            Dim rHidden As Boolean = False
            Return Add(aCode:=aGroupCode, aValue:=aValue, aName:=aName, aSuppressedValue:=Nothing, aPropertyType:=aPropertyType, rHidden:=rHidden, bSuppressed:=False, bScalable:=False, bSetToSuppressed:=False, aLastVal:=Nothing, bIsHeader:=False)
        End Function
        Public Function Add(aGroupCode As Integer, aValue As Object, aName As String, Optional aPropertyType As dxxPropertyTypes = dxxPropertyTypes.Undefined, Optional bIsHeader As Boolean? = Nothing) As TPROPERTY
            Dim rHidden As Boolean = False
            Return Add(aCode:=aGroupCode, aValue:=aValue, aName:=aName, aSuppressedValue:=Nothing, aPropertyType:=aPropertyType, rHidden:=rHidden, bSuppressed:=False, bScalable:=False, bSetToSuppressed:=False, aLastVal:=Nothing, bIsHeader:=bIsHeader)

        End Function
        Public Function Add(aGroupCode As Integer, aValue As Object, aName As String, aSuppressedValue As Object, aPropertyType As dxxPropertyTypes, Optional bSuppressed As Boolean = False, Optional bScalable As Boolean = False, Optional bSetToSuppressed As Boolean = False, Optional aLastVal As Object = Nothing, Optional bIsHeader As Boolean? = Nothing) As TPROPERTY

            Dim rHidden As Boolean = False
            Return Add(aCode:=aGroupCode, aValue:=aValue, aName:=aName, aSuppressedValue:=aSuppressedValue, aPropertyType:=aPropertyType, rHidden:=rHidden, bSuppressed:=bSuppressed, bScalable:=bScalable, bSetToSuppressed:=bSetToSuppressed, aLastVal:=aLastVal, bIsHeader:=bIsHeader)
        End Function
        Public Function Add(aCode As Integer, aValue As Object, aName As String, aSuppressedValue As Object, aPropertyType As dxxPropertyTypes, ByRef rHidden As Boolean, bSuppressed As Boolean, bScalable As Boolean, bSetToSuppressed As Boolean, aLastVal As Object, bIsHeader As Boolean?) As TPROPERTY ', Optional aHandleGC As Integer = 5,

            Dim bNonDXF As Boolean = NonDXF
            aName = aName.Trim()
            rHidden = aCode < 0 Or aName.StartsWith("*")
            If aCode < 0 Or aName.StartsWith("*") Then bNonDXF = True


            Dim _rVal As New TPROPERTY(aCode:=aCode, aValue:=aValue, aName:=aName, aPropType:=aPropertyType, aLastVal:=aLastVal, aValControl:=mzValueControls.None, bScalable:=bScalable, aSuppressedVal:=Nothing, bNonDXF:=bNonDXF)

            If bSetToSuppressed And _rVal.SuppressedValue Is Nothing Then _rVal.SuppressedValue = _rVal.Value
            rHidden = _rVal.Hidden

            If Not rHidden Then
                _rVal.Suppressed = bSuppressed
            Else
                If _rVal.GroupCode = 0 Then _rVal.GroupCode = 1
            End If

            Return Add(_rVal, rHidden, bIsHeader)

        End Function
        Friend Function AddPoint(aVector As TVECTOR, aName As String, aGroupCode As Integer,
                                    Optional bIsHeader As Boolean = False, Optional bSuppressed As Boolean = False,
                                     Optional bScaleable As Boolean = False, Optional bHidden As Boolean = False) As TPROPERTY

            Dim _rVal As TPROPERTY
            If String.IsNullOrWhiteSpace(aName) Then aName = "" Else aName = aName.Trim()
            If aName = "" Then aName = $"Prop_{Count + 1}"

            If bIsHeader Then
                If Not aName.StartsWith("$") Then aName = $"${aName}"
                _rVal = New TPROPERTY(9, aName, aName, dxxPropertyTypes.dxf_String, Nothing, bScalable:=bScaleable) With {.Suppressed = bSuppressed}
                Add(_rVal, bHidden, bIsHeader)
                Dim p1 As New TPROPERTY(aGroupCode, aVector.X, $"{aName}_X", dxxPropertyTypes.dxf_Double, bScalable:=bScaleable, bSuppressed:=bSuppressed, bIsOrdinate:=True)
                Dim p2 As New TPROPERTY(aGroupCode + 10, aVector.Y, $"{aName}_Y", dxxPropertyTypes.dxf_Double, bScalable:=bScaleable, bSuppressed:=bSuppressed, bIsOrdinate:=True)

                Add(p1, bHidden)
                Add(p2, bHidden)

            Else
                _rVal = New TPROPERTY(aGroupCode, aVector.X, $"{aName}_X", dxxPropertyTypes.dxf_Double, bScalable:=bScaleable, bSuppressed:=bSuppressed, bIsOrdinate:=True)
                Dim p1 As New TPROPERTY(aGroupCode + 10, aVector.Y, $"{aName}_Y", dxxPropertyTypes.dxf_Double, bScalable:=bScaleable, bSuppressed:=bSuppressed, bIsOrdinate:=True)
                Add(_rVal, bHidden)
                Add(p1, bHidden)
            End If




            Return _rVal
        End Function
        Public Function AddVector(aValue As Double, bValue As Double, cValue As Double, Optional aName As String = "", Optional aGroupCode As Integer = 10, Optional bIsHeader As Boolean = False, Optional bSuppressed As Boolean = False, Optional bIsDirection As Boolean = False, Optional bScaleable As Boolean = False, Optional bHidden As Boolean = False) As TPROPERTY

            Return AddVector(aGroupCode, New TVECTOR(aValue, bValue, cValue), aName, False, bSuppressed, bIsDirection, bScaleable, bHidden, bIsHeader)
        End Function

        Friend Function AddVector(aGroupCode As Integer, aVector As TVECTOR, Optional aName As String = "", Optional bTwoD As Boolean = False, Optional bSuppressed As Boolean = False, Optional bIsDirection As Boolean = False, Optional bScaleable As Boolean = False, Optional bHidden As Boolean = False, Optional bIsHeader As Boolean = False) As TPROPERTY

            If bIsDirection Then
                bScaleable = False
                aVector = aVector.Normalized
            End If
            Dim _rVal As TPROPERTY
            If String.IsNullOrWhiteSpace(aName) Then aName = "" Else aName = aName.Trim()
            If aName = "" Then aName = $"Prop_{Count + 1}"

            If bIsHeader Then
                If Not aName.StartsWith("$") Then aName = $"${aName}"
                _rVal = New TPROPERTY(9, aName, aName, dxxPropertyTypes.dxf_String, Nothing, bScalable:=bScaleable, bSuppressed:=bSuppressed) With {.Key = aName}
                Add(_rVal, bHidden, bIsHeader)
                Dim p1 As New TPROPERTY(aGroupCode, aVector.X, $"{aName}_X", dxxPropertyTypes.dxf_Double, Nothing, bScalable:=bScaleable, bSuppressed:=bSuppressed, bIsOrdinate:=True)
                Dim p2 As New TPROPERTY(aGroupCode + 10, aVector.Y, $"{aName}_Y", dxxPropertyTypes.dxf_Double, Nothing, bScalable:=bScaleable, bSuppressed:=bSuppressed, bIsOrdinate:=True)

                Add(p1, bHidden)
                Add(p2, bHidden)
                If Not bTwoD Then
                    Dim p3 As New TPROPERTY(aGroupCode + 20, aVector.Z, $"{aName}_Z", dxxPropertyTypes.dxf_Double, Nothing, bScalable:=bScaleable, bSuppressed:=bSuppressed, bIsOrdinate:=True)
                    Add(p3, bHidden)

                End If

            Else

                _rVal = New TPROPERTY(aGroupCode, aVector.X, $"{aName}_X", dxxPropertyTypes.dxf_Double, Nothing, bScalable:=bScaleable, bSuppressed:=bSuppressed, bIsOrdinate:=True)
                Dim p1 As New TPROPERTY(aGroupCode + 10, aVector.Y, $"{aName}_Y", dxxPropertyTypes.dxf_Double, Nothing, bScalable:=bScaleable, bSuppressed:=bSuppressed, bIsOrdinate:=True)

                Add(_rVal, bHidden)
                Add(p1, bHidden)
                If Not bTwoD Then
                    Dim p2 As New TPROPERTY(aGroupCode + 20, aVector.Z, $"{aName}_Z", dxxPropertyTypes.dxf_Double, Nothing, bScalable:=bScaleable, bSuppressed:=bSuppressed, bIsOrdinate:=True)
                    Add(p2, bHidden)

                End If

            End If
            Return _rVal

        End Function

        Public Sub Append(bProps As TPROPERTIES, Optional bExcludeHidden As Boolean = False, Optional aSkipList As String = "")

            Dim aProp As TPROPERTY
            If String.IsNullOrWhiteSpace(aSkipList) Then aSkipList = "" Else aSkipList = aSkipList.Trim
            Dim testname As Boolean = aSkipList <> ""
            For i As Integer = 1 To bProps.Count
                aProp = bProps.Item(i)
                If Not bExcludeHidden Or (bExcludeHidden And Not aProp.Hidden) Then
                    If Not testname Then
                        Add(New TPROPERTY(aProp))
                    Else
                        If Not TLISTS.Contains(aProp.Name, aSkipList) Then
                            Add(aProp.Clone)
                        End If
                    End If
                End If
            Next i
        End Sub
        Public Sub Clear()
            ReDim _Members(-1)
            _Init = True
            '_GroupCodes = New TDICTIONARY_GCS("")

        End Sub
        Public Sub Print(Optional bShowIndices As Boolean = False, Optional bSuppressHidden As Boolean = False, Optional aGCList As String = "", Optional bDXFFormat As Boolean = False, Optional aStream As IO.StreamWriter = Nothing, Optional aPrefix As String = "")
            '#1the subject property array structure
            '#2flag to include the indices of the members in the output string
            '#3flag to suppress the output of the property arrays hidden members
            '#4a list of group codes to print
            '#5flag to print the property as it would be printed in a DXF file
            '#6a text stream to print to
            '#7a string to append to the beginning of the each output string
            '^write the sigantures of the property array to the debug screen and or a
            'On Error Resume Next
            If IsEmpty Then Return
            Dim bDoIt As Boolean
            Dim aProp As TPROPERTY
            Dim bTestGC As Boolean
            Dim Mems(0) As TPROPERTY
            Dim cnt As Integer = Count
            Dim hProps As New List(Of TPROPERTY)
            If aGCList Is Nothing Then aGCList = ""
            aGCList = aGCList.Trim()
            bTestGC = aGCList <> ""
            For i As Integer = 1 To cnt
                aProp = _Members(i - 1)
                bDoIt = True
                If bTestGC Then bDoIt = TLISTS.Contains(aProp.GroupCode, aGCList, bReturnTrueForNullList:=True)
                If bDoIt Then
                    aProp.Index = i
                    If aProp.Hidden Then
                        bDoIt = False
                        If Not bSuppressHidden Then hProps.Add(aProp)
                    End If
                End If
                If bDoIt Then
                    aProp.Print(bDXFFormat, aStream, bShowIndices, aPrefix)
                End If
            Next i
            'just put the hiddens lats
            For i = 1 To hProps.Count
                aProp = hProps(i - 1)
                aProp.Print(bDXFFormat, aStream, bShowIndices, aPrefix)
            Next i
        End Sub
        Public Function SetVector(aIndexOrKey As Object, aVector As iVector, Optional arHidden As Boolean? = Nothing, Optional bSuppressed As Boolean? = Nothing, Optional bDirection As Boolean = False) As Boolean
            Dim rLastValue As TVECTOR = TVECTOR.Zero
            Return SetVector(aIndexOrKey, aVector, arHidden, bSuppressed, bDirection, rLastValue)
        End Function
        Public Function SetVector(aIndexOrKey As Object, aVector As iVector, arHidden As Boolean?, bSuppressed As Boolean?, bDirection As Boolean, ByRef rLastValue As TVECTOR) As Boolean

            Dim nevec As New TVECTOR(aVector)
            Dim _rVal As Boolean
            rLastValue = TVECTOR.Zero
            Dim xMem As TPROPERTY = TPROPERTY.Null
            Dim yMem As TPROPERTY
            Dim zMem As TPROPERTY
            If TypeOf aIndexOrKey Is String Then
                If Not TryGetVector(aIndexOrKey, xMem) Then
                    Console.WriteLine($"Vector {aIndexOrKey} Not Found")
                    Return False

                End If
            Else
                xMem = Item(TVALUES.To_INT(aIndexOrKey))
                If xMem.Index <= 0 Then Return False
            End If
            If xMem.IsHeader Then
                If arHidden.HasValue Then xMem.Hidden = arHidden.Value
                If bSuppressed IsNot Nothing Then
                    If bSuppressed.HasValue Then xMem.Suppressed = bSuppressed.Value
                End If

                SetItem(xMem.Index, xMem)
                xMem = Item(xMem.Index + 1)
                If xMem.PropType <> dxxPropertyTypes.dxf_Double Then Return False

                rLastValue.X = TVALUES.To_DBL(xMem.Value)
                xMem.Value = nevec.X
                If arHidden.HasValue Then xMem.Hidden = arHidden.Value
                If TVALUES.IsBoolean(bSuppressed) Then xMem.Suppressed = TVALUES.ToBoolean(bSuppressed)
                SetItem(xMem.Index, xMem)
                yMem = Item(xMem.Index + 1)
                If yMem.PropType = dxxPropertyTypes.dxf_Double And yMem.GroupCode = xMem.GroupCode + 10 Then
                    rLastValue.Y = TVALUES.To_DBL(yMem.Value)
                    yMem.Value = nevec.Y
                    yMem.Hidden = xMem.Hidden
                    yMem.Suppressed = xMem.Suppressed
                    SetItem(yMem.Index, yMem)
                End If
                zMem = Item(xMem.Index + 2)
                If yMem.PropType = dxxPropertyTypes.dxf_Double And zMem.GroupCode = xMem.GroupCode + 20 Then
                    rLastValue.Z = TVALUES.To_DBL(zMem.Value)
                    zMem.Value = nevec.Z
                    zMem.Hidden = xMem.Hidden
                    zMem.Suppressed = xMem.Suppressed
                    SetItem(zMem.Index, zMem)
                End If

            Else
                If xMem.PropType <> dxxPropertyTypes.dxf_Double Then Return False
                rLastValue.X = TVALUES.To_DBL(xMem.Value)
                xMem.Value = nevec.X
                If arHidden.HasValue Then xMem.Hidden = arHidden.Value
                If TVALUES.IsBoolean(bSuppressed) Then xMem.Suppressed = TVALUES.ToBoolean(bSuppressed)
                SetItem(xMem.Index, xMem)
                yMem = Item(xMem.Index + 1)
                If yMem.PropType = dxxPropertyTypes.dxf_Double And yMem.GroupCode = xMem.GroupCode + 10 Then
                    rLastValue.Y = TVALUES.To_DBL(yMem.Value)
                    yMem.Value = nevec.Y
                    yMem.Hidden = xMem.Hidden
                    yMem.Suppressed = xMem.Suppressed
                    SetItem(yMem.Index, yMem)
                End If
                zMem = Item(xMem.Index + 2)
                If yMem.PropType = dxxPropertyTypes.dxf_Double And zMem.GroupCode = xMem.GroupCode + 20 Then
                    rLastValue.Z = TVALUES.To_DBL(zMem.Value)
                    zMem.Value = nevec.Z
                    zMem.Hidden = xMem.Hidden
                    zMem.Suppressed = xMem.Suppressed
                    SetItem(zMem.Index, zMem)
                End If
            End If
            _rVal = Not rLastValue.Equals(nevec, 4)
            SetItem(xMem.Index, xMem)

            Return _rVal
        End Function

        Friend Function SetVector(aIndexOrKey As Object, aVector As TVECTOR, Optional arHidden As Boolean? = Nothing, Optional bSuppressed As Boolean? = Nothing, Optional bDirection As Boolean = False) As Boolean
            Dim rLastValue As TVECTOR = TVECTOR.Zero
            Return SetVector(aIndexOrKey, New dxfVector(aVector), arHidden, bSuppressed, bDirection, rLastValue)
        End Function
        Friend Function SetVector(aIndexOrKey As Object, aVector As TVECTOR, arHidden As Boolean?, bSuppressed As Boolean?, bDirection As Boolean, ByRef rLastValue As TVECTOR) As Boolean

            Dim nevec As New TVECTOR(aVector)
            Dim _rVal As Boolean
            rLastValue = TVECTOR.Zero
            Dim xMem As New TPROPERTY("")
            Dim yMem As TPROPERTY
            Dim zMem As TPROPERTY
            If TypeOf aIndexOrKey Is String Then
                If Not TryGetVector(TVALUES.To_STR(aIndexOrKey, bTrim:=True).ToUpper, xMem) Then
                    Return False
                End If
            Else
                xMem = Item(TVALUES.To_INT(aIndexOrKey))
                If xMem.Index <= 0 Then Return False
            End If
            If xMem.IsHeader Then
                If arHidden.HasValue Then xMem.Hidden = arHidden.Value
                If bSuppressed IsNot Nothing Then
                    If bSuppressed.HasValue Then xMem.Suppressed = bSuppressed.Value
                End If

                SetItem(xMem.Index, xMem)
                xMem = Item(xMem.Index + 1)
                If xMem.PropType <> dxxPropertyTypes.dxf_Double Then Return False

                rLastValue.X = TVALUES.To_DBL(xMem.Value)
                xMem.Value = nevec.X
                If arHidden.HasValue Then xMem.Hidden = arHidden.Value
                If TVALUES.IsBoolean(bSuppressed) Then xMem.Suppressed = TVALUES.ToBoolean(bSuppressed)
                SetItem(xMem.Index, xMem)
                yMem = Item(xMem.Index + 1)
                If yMem.PropType = dxxPropertyTypes.dxf_Double And yMem.GroupCode = xMem.GroupCode + 10 Then
                    rLastValue.Y = TVALUES.To_DBL(yMem.Value)
                    yMem.Value = nevec.Y
                    yMem.Hidden = xMem.Hidden
                    yMem.Suppressed = xMem.Suppressed
                    SetItem(yMem.Index, yMem)
                End If
                zMem = Item(xMem.Index + 2)
                If yMem.PropType = dxxPropertyTypes.dxf_Double And zMem.GroupCode = xMem.GroupCode + 20 Then
                    rLastValue.Z = TVALUES.To_DBL(zMem.Value)
                    zMem.Value = nevec.Z
                    zMem.Hidden = xMem.Hidden
                    zMem.Suppressed = xMem.Suppressed
                    SetItem(zMem.Index, zMem)
                End If

            Else
                If xMem.PropType <> dxxPropertyTypes.dxf_Double Then Return False
                rLastValue.X = TVALUES.To_DBL(xMem.Value)
                xMem.Value = nevec.X
                If arHidden.HasValue Then xMem.Hidden = arHidden.Value
                If TVALUES.IsBoolean(bSuppressed) Then xMem.Suppressed = TVALUES.ToBoolean(bSuppressed)
                SetItem(xMem.Index, xMem)
                yMem = Item(xMem.Index + 1)
                If yMem.PropType = dxxPropertyTypes.dxf_Double And yMem.GroupCode = xMem.GroupCode + 10 Then
                    rLastValue.Y = TVALUES.To_DBL(yMem.Value)
                    yMem.Value = nevec.Y
                    yMem.Hidden = xMem.Hidden
                    yMem.Suppressed = xMem.Suppressed
                    SetItem(yMem.Index, yMem)
                End If
                zMem = Item(xMem.Index + 2)
                If yMem.PropType = dxxPropertyTypes.dxf_Double And zMem.GroupCode = xMem.GroupCode + 20 Then
                    rLastValue.Z = TVALUES.To_DBL(zMem.Value)
                    zMem.Value = nevec.Z
                    zMem.Hidden = xMem.Hidden
                    zMem.Suppressed = xMem.Suppressed
                    SetItem(zMem.Index, zMem)
                End If
            End If
            _rVal = Not rLastValue.Equals(nevec, 4)
            SetItem(xMem.Index, xMem)

            Return _rVal
        End Function


        Public Function SetVal(aIndexOrKey As Object, aValue As Object, Optional bSuppress As Boolean? = Nothing, Optional bIgnoreValueControl As Boolean = False) As Boolean
            Dim rLastValue As Object = Nothing
            Return SetVal(aIndexOrKey, aValue, rLastValue, bSuppress, bIgnoreValueControl)
        End Function

        Public Function SetVal(aIndexOrKey As Object, aValue As Object, ByRef rLastValue As Object, Optional bSuppress As Boolean? = Nothing, Optional bIgnoreValueControl As Boolean = False) As Boolean
            '1 based
            If TypeOf aValue Is iVector Then
                Dim iv As iVector = DirectCast(aValue, iVector)
                Return SetVector(aIndexOrKey, iv, Nothing, bSuppress, False, rLastValue)
            ElseIf TypeOf aValue Is dxfVector Then
                Dim v1 As dxfVector = DirectCast(aValue, dxfVector)
                Return SetVector(aIndexOrKey, v1, Nothing, bSuppress, False, rLastValue)
            End If
            Dim aMem As TPROPERTY = TPROPERTY.Null
            rLastValue = Nothing
            If aIndexOrKey Is Nothing Then Return False
            Dim _rVal As Boolean
            If TypeOf (aIndexOrKey) Is String Then
                If Not TryGet(aIndexOrKey.ToString(), aMem) Then Return False
            Else
                aMem = Item(TVALUES.To_INT(aIndexOrKey))
                If aMem.Key = "" Then Return False
            End If
            rLastValue = aMem.Value
            _rVal = aMem.SetVal(aValue, bSuppress:=bSuppress, bIgnoreValueControl:=bIgnoreValueControl)
            _Members(aMem.Index - 1) = aMem
            Return _rVal
        End Function
        Public Function SetValGC(aGroupCode As Integer, aValue As Object, Optional aOccurance As Integer = 1, Optional bSuppress As Boolean? = Nothing, Optional bIgnoreValueControl As Boolean = False) As Boolean
            Dim rLastValue As Object = Nothing
            Return SetValGC(aGroupCode, aValue, aOccurance, rLastValue, bSuppress, bIgnoreValueControl)
        End Function
        Public Function SetValGC(aGroupCode As Object, aValue As Object, aOccurance As Integer, ByRef rLastValue As Object, Optional bSuppress As Boolean? = Nothing, Optional bIgnoreValueControl As Boolean = False) As Boolean
            rLastValue = Nothing
            If aGroupCode Is Nothing Or aValue Is Nothing Then Return False
            If Not TVALUES.IsNumber(aGroupCode) Then Return False
            Dim aIndex As Integer
            Dim iGC As Integer = TVALUES.To_INT(aGroupCode)
            Dim _rVal As Boolean
            Dim aProp As TPROPERTY = TPROPERTY.Null
            If Not TryGet(iGC, aProp, aOccur:=aOccurance) Then Return False
            rLastValue = aProp.Value
            If TypeOf (aValue) Is TVECTOR Then
                Dim v1 As TVECTOR = aValue
                _rVal = SetVector(aIndex, v1, Nothing, bSuppressed:=bSuppress, False, rLastValue:=rLastValue)
            ElseIf TypeOf (aValue) Is dxfVector Then
                Dim v1 As dxfVector = DirectCast(aValue, dxfVector)
                _rVal = SetVector(aIndex, v1, Nothing, bSuppressed:=bSuppress, False, rLastValue:=rLastValue)

            Else

                _rVal = aProp.SetVal(aValue)
            End If
            SetItem(aProp.Index, aProp)

            Return _rVal
        End Function
        Public Function SetValueGC(aGroupCode As Integer, aValue As Object, Optional aOccurance As Integer = 1) As Boolean
            Dim rIndex As Integer = 0
            Dim rLastValue As Object = ""
            Dim rPropName As String = String.Empty
            Return SetValueGC(aGroupCode, aValue, rIndex, aOccurance, rLastValue, rPropName)
        End Function
        Public Function SetValueGC(aGroupCode As Integer, aValue As Object, ByRef rIndex As Integer, aOccurance As Integer, ByRef rLastValue As Object, ByRef rPropName As String) As Boolean
            rIndex = GetIndex(aGroupCode.ToString, True, False, aOccurance)
            Dim _rVal As Boolean
            Dim aProp As TPROPERTY = Item(rIndex)
            rLastValue = aProp.Value
            rPropName = aProp.Name
            If (TypeOf aValue Is TVECTOR) Then
                Dim tv As TVECTOR = DirectCast(aValue, TVECTOR)
                _rVal = SetVector(rIndex, New dxfVector(tv), Nothing, Nothing, False, rLastValue:=rLastValue)

            ElseIf (TypeOf aValue Is dxfVector) Then
                Dim v1 As dxfVector = DirectCast(aValue, dxfVector)

                _rVal = SetVector(rIndex, v1, Nothing, Nothing, False, rLastValue:=rLastValue)

            Else
                _rVal = aProp.SetVal(aValue)
            End If
            SetItem(rIndex, aProp)
            Return _rVal
        End Function
        Public Function SetVectorGC(aGroupCode As Object, aVector As TVECTOR, Optional aOccurance As Integer = 1, Optional bSuppress As Boolean? = Nothing, Optional bNormalize As Boolean = False) As Boolean
            Dim rLastValue As New TVECTOR(0)
            Return SetVectorGC(aGroupCode, aVector, aOccurance, rLastValue, bSuppress, bNormalize)
        End Function
        Public Function SetVectorGC(aGroupCode As Object, aVector As TVECTOR, aOccurance As Integer, ByRef rLastValue As TVECTOR, Optional bSuppress As Boolean? = Nothing, Optional bNormalize As Boolean = False) As Boolean
            rLastValue = TVECTOR.Zero
            If aGroupCode Is Nothing Then Return False
            If Not TVALUES.IsNumber(aGroupCode) Then Return False
            Dim iGC As Integer = TVALUES.To_INT(aGroupCode)

            Dim idx As Integer = GetIndex(aGroupCode, True, False, aOccurance)
            If idx <= 0 Then Return False
            Return SetVector(idx, aVector, Nothing, bSuppress, bNormalize)
        End Function
        Public Function GetByGC(aGroupCode As Integer, Optional aOccurance As Integer = 1) As TPROPERTY
            Dim _rVal As TPROPERTY = TPROPERTY.Null
            TryGet(aGroupCode, _rVal, aOccur:=aOccurance)
            Return _rVal
        End Function
        Public Function GCValueL(aGroupCode As Integer, Optional aDefault As Long = 0, Optional aOccurance As Integer = 1) As Long
            Dim rPropName As String = String.Empty
            Return GCValueL(aGroupCode, aDefault, aOccurance, rPropName)
        End Function
        Public Function GCValueL(aGroupCode As Integer, aDefault As Long, aOccurance As Integer, ByRef rPropName As String) As Long
            rPropName = ""
            Dim aProp As TPROPERTY = TPROPERTY.Null
            If Not TryGet(aGroupCode, aProp, aOccur:=aOccurance) Then Return aDefault
            rPropName = aProp.Name
            Return TVALUES.To_LNG(aProp.Value, aDefault)
        End Function
        Public Function GCValueI(aGroupCode As Integer, Optional aDefault As Integer = 0, Optional aOccurance As Integer = 1) As Integer
            Dim rPropName As String = String.Empty
            Return GCValueI(aGroupCode, aDefault, aOccurance, rPropName)
        End Function
        Public Function GCValueI(aGroupCode As Integer, aDefault As Integer, aOccurance As Integer, ByRef rPropName As String) As Integer
            Dim aProp As TPROPERTY = TPROPERTY.Null
            rPropName = ""
            If Not TryGet(aGroupCode, aProp, aOccur:=aOccurance) Then Return aDefault
            rPropName = aProp.Name
            Return TVALUES.To_INT(aProp.Value, aDefault)
        End Function
        Public Function GCValueB(aGroupCode As Integer, Optional aDefault As Boolean = False, Optional aOccurance As Integer = 1) As Boolean
            Dim rPropName As String = String.Empty
            Return GCValueB(aGroupCode, aDefault, aOccurance, rPropName)
        End Function
        Public Function GCValueB(aGroupCode As Integer, aDefault As Boolean, aOccurance As Integer, ByRef rPropName As String) As Boolean
            Dim aProp As TPROPERTY = TPROPERTY.Null
            rPropName = ""
            If Not TryGet(aGroupCode, aProp, aOccur:=aOccurance) Then Return aDefault
            rPropName = aProp.Name
            Return TVALUES.ToBoolean(aProp.Value, aDefault)
        End Function
        Public Function GCValueD(aGroupCode As Integer, Optional aDefault As Double = 0, Optional aOccurance As Integer = 1) As Double
            Dim rPropName As String = String.Empty
            Return GCValueD(aGroupCode, aDefault, aOccurance, rPropName)
        End Function
        Public Function GCValueD(aGroupCode As Integer, aDefault As Double, aOccurance As Integer, ByRef rPropName As String) As Double
            Dim aProp As TPROPERTY = TPROPERTY.Null
            rPropName = ""
            If Not TryGet(aGroupCode, aProp, aOccur:=aOccurance) Then Return aDefault
            rPropName = aProp.Name
            Return TVALUES.To_DBL(aProp.Value, aDefault:=aDefault)
        End Function
        Public Function GCValueStr(aGroupCode As Integer, Optional aDefault As String = "", Optional aOccurance As Integer = 1, Optional bReturnDefaultIfNullString As Boolean = False) As String
            Dim rPropName As String = String.Empty
            Dim rIndex As Integer = 0
            Return GCValueStr(aGroupCode, aDefault, aOccurance, rPropName, bReturnDefaultIfNullString, rIndex)
        End Function
        Public Function GCValueStr(aGroupCode As Integer, aDefault As String, aOccurance As Integer, ByRef rPropName As String, bReturnDefaultIfNullString As Boolean, ByRef rIndex As Integer) As String
            Dim aProp As TPROPERTY = TPROPERTY.Null
            rPropName = ""
            If Not TryGet(aGroupCode, aProp, aOccur:=aOccurance) Then Return aDefault
            rPropName = aProp.Name
            Dim _rVal As String = TVALUES.To_STR(aProp.Value, aDefault)
            If bReturnDefaultIfNullString And Trim(_rVal) = "" Then _rVal = aDefault
            Return _rVal
        End Function
        Public Function GCValueV(aGroupCode As Integer, Optional aOccurance As Integer = 1) As TVECTOR
            Dim rIndex As Integer = 1
            Dim aDefault As New TVECTOR(0)
            Dim rPropName As String = String.Empty
            Return GCValueV(aGroupCode, aDefault, aOccurance, rPropName, rIndex)
        End Function
        Public Function GCValueV(aGroupCode As Integer, aDefault As TVECTOR, Optional aOccurance As Integer = 1) As TVECTOR
            Dim rIndex As Integer = 1
            Dim rPropName As String = String.Empty
            Return GCValueV(aGroupCode, aDefault, aOccurance, rPropName, rIndex)
        End Function
        Public Function GCValueV(aGroupCode As Integer, aDefault As TVECTOR, aOccurance As Integer, ByRef rPropName As String, ByRef rIndex As Integer) As TVECTOR
            Dim xMem As New TPROPERTY("")
            rPropName = ""
            rIndex = 0
            If Not TryGet(aGroupCode, xMem, aOccur:=aOccurance) Then Return aDefault
            rPropName = xMem.Name
            rIndex = xMem.Index

            Dim _rVal As TVECTOR = TVECTOR.Zero


            If xMem.IsHeader Then xMem = Item(xMem.Index + 1)

            Dim yMem As TPROPERTY = Item(xMem.Index + 1)
            Dim zMem As TPROPERTY = Item(xMem.Index + 2)

            If xMem.PropType = dxxPropertyTypes.dxf_Double Then _rVal.X = TVALUES.To_DBL(xMem.Value) Else Return _rVal
            If yMem.PropType = dxxPropertyTypes.dxf_Double And yMem.GroupCode = xMem.GroupCode + 10 Then _rVal.Y = TVALUES.To_DBL(yMem.Value)
            If zMem.PropType = dxxPropertyTypes.dxf_Double And zMem.GroupCode = xMem.GroupCode + 20 Then _rVal.Z = TVALUES.To_DBL(zMem.Value)
            Return _rVal


        End Function

        Public Function GCVector(aGroupCode As Integer, Optional aOccurance As Integer = 1) As dxfVector
            Dim rIndex As Integer = 1
            Dim aDefault As New TVECTOR(0)
            Dim rPropName As String = String.Empty
            Return GCVector(aGroupCode, aDefault, aOccurance, rPropName, rIndex)
        End Function
        Public Function GCVector(aGroupCode As Integer, aDefault As dxfVector, Optional aOccurance As Integer = 1) As dxfVector
            Dim rIndex As Integer = 1
            Dim rPropName As String = String.Empty
            Return GCVector(aGroupCode, aDefault, aOccurance, rPropName, rIndex)
        End Function
        Public Function GCVector(aGroupCode As Integer, aDefault As dxfVector, aOccurance As Integer, ByRef rPropName As String, ByRef rIndex As Integer) As dxfVector
            Dim xMem As New TPROPERTY("")
            rPropName = ""
            rIndex = 0
            If Not TryGet(aGroupCode, xMem, aOccur:=aOccurance) Then Return aDefault
            rPropName = xMem.Name
            rIndex = xMem.Index

            Dim _rVal As dxfVector = Nothing


            If xMem.IsHeader Then xMem = Item(xMem.Index + 1)

            Dim yMem As TPROPERTY = Item(xMem.Index + 1)
            Dim zMem As TPROPERTY = Item(xMem.Index + 2)

            If xMem.PropType = dxxPropertyTypes.dxf_Double Then _rVal.X = TVALUES.To_DBL(xMem.Value) Else Return _rVal
            If yMem.PropType = dxxPropertyTypes.dxf_Double And yMem.GroupCode = xMem.GroupCode + 10 Then _rVal.Y = TVALUES.To_DBL(yMem.Value)
            If zMem.PropType = dxxPropertyTypes.dxf_Double And zMem.GroupCode = xMem.GroupCode + 20 Then _rVal.Z = TVALUES.To_DBL(zMem.Value)
            Return _rVal


        End Function
        Public Function SetSuppressed(aIndexOrKey As Object, aValue As Boolean) As Boolean
            'base 1
            Dim aMem As TPROPERTY = TPROPERTY.Null
            If TypeOf (aIndexOrKey) Is String Then
                If Not TryGet(TVALUES.To_STR(aIndexOrKey, bTrim:=True).ToUpper, aMem) Then Return False
            Else
                aMem = Item(TVALUES.To_INT(aIndexOrKey))
                If aMem.Index < 0 Then Return False
            End If
            Dim _rVal As Boolean = aMem.Suppressed <> aValue
            aMem.Suppressed = aValue
            SetItem(aMem.Index, aMem)
            Return _rVal
        End Function
        Public Function SetSuppressed(aNameOrKeyList As String, aDelimitor As String, aValue As Boolean) As Boolean
            Dim sVals As TVALUES = TLISTS.ToValues(aNameOrKeyList, aDelimitor, False, True, True)
            If sVals.Count <= 0 Then Return False
            Dim _rVal As Boolean

            For i As Integer = 1 To sVals.Count
                Dim pname As String = sVals.Member(i - 1).ToString().Trim()
                If SetSuppressed(pname, aValue) Then _rVal = True
            Next
            Return _rVal
        End Function
        Public Function RemoveByGroupCode(aGroupCode As Integer) As TPROPERTIES
            Dim _rVal As TPROPERTIES = Clone(bReturnEmpty:=True)
            Dim aProp As TPROPERTY
            Dim nMems As New List(Of TPROPERTY)
            For i As Integer = 1 To Count
                aProp = _Members(i - 1)
                If aProp.GroupCode = aGroupCode Then
                    _rVal.Add(aProp)
                Else
                    nMems.Add(aProp)
                End If
            Next i
            If _rVal.Count > 0 Then Repopulate(nMems)


            Return _rVal
        End Function
        Public Function RemoveByStringValue(aStringValue As String) As TPROPERTIES
            Dim _rVal As TPROPERTIES = Clone(bReturnEmpty:=True)
            Dim aProp As TPROPERTY
            Dim nMems As New List(Of TPROPERTY)
            For i As Integer = 1 To Count
                aProp = _Members(i - 1)
                If String.Compare(aProp.Value, aStringValue, True) = 0 Then
                    _rVal.Add(aProp)
                Else
                    nMems.Add(aProp)
                End If
            Next i
            If _rVal.Count > 0 Then Repopulate(nMems)
            Return _rVal
        End Function
        Public Sub CopyCommonProps(aFromProps As TPROPERTIES, Optional aStartID As Integer = 2)
            If Count <= dxfGlobals.CommonProps Or aFromProps.Count <= dxfGlobals.CommonProps Then Return


            Dim si As Integer
            If aStartID > 0 Then si = aStartID
            For i As Integer = si To dxfGlobals.CommonProps
                SetItem(i, aFromProps.Item(i).Clone)
            Next i
        End Sub

        Public Function CopyValues(bProperties As dxoProperties, Optional bCopyNewMembers As Boolean = False, Optional aGCsToSkip As String = "") As Boolean
            If bProperties Is Nothing Then Return False
            If bProperties.Count <= 0 Then Return False
            Return CopyValues(New TPROPERTIES(bProperties), bCopyNewMembers, aGCsToSkip)
        End Function

        Public Function CopyValues(bProperties As TPROPERTIES, Optional bCopyNewMembers As Boolean = False, Optional aGCsToSkip As String = "") As Boolean
            If bProperties.Count <= 0 Then Return False
            Dim _rVal As Boolean = False
            Dim gCodes As List(Of Integer) = GroupCodes(bUniquesOnly:=True, aGCListToOmmit:=aGCsToSkip)


            For Each aGC As Integer In gCodes
                Dim bProps As List(Of TPROPERTY) = bProperties.GroupCodeMembers(aGC)
                Dim myProps As List(Of TPROPERTY) = GroupCodeMembers(aGC)
                For j As Integer = 1 To bProps.Count
                    Dim bProp As TPROPERTY = bProps.Item(j - 1)
                    If j > myProps.Count Then
                        If bCopyNewMembers Then Add(bProp) Else Exit For
                    Else
                        Dim aProp As TPROPERTY = myProps.Item(j - 1)

                        If aProp.CopyValue(bProp) Then
                            _rVal = True
                        End If


                        SetItem(aProp.Index, aProp)
                    End If


                Next j
            Next
            Return _rVal
        End Function

        Private Sub Repopulate(aMems As List(Of TPROPERTY), Optional bAddClones As Boolean = False)
            ReDim _Members(-1)
            _Init = True
            If aMems Is Nothing Then Return
            If aMems.Count <= 0 Then Return
            Dim j As Integer
            System.Array.Resize(_Members, aMems.Count)
            For Each mem As TPROPERTY In aMems
                Dim newmem As TPROPERTY = mem
                If bAddClones Then newmem = New TPROPERTY(mem)
                j += 1
                If Not SuppressIndices Then newmem.Index = j
                _Members(j - 1) = newmem
            Next

        End Sub
        Public Sub Remove(aIndex As Integer)
            If aIndex < 0 Or aIndex > Count Then Return
            Dim allmems As List(Of TPROPERTY) = _Members.ToList()
            allmems.RemoveAt(aIndex - 1)
            Repopulate(allmems)
        End Sub
        Public Function ReplaceByGC(aSubSet As TPROPERTIES, aGC As Integer, Optional bKeepMem As Boolean = False, Optional aOccurance As Integer = 0) As TPROPERTIES
            Dim _rVal As TPROPERTIES = Clone(bReturnEmpty:=True)
            If aSubSet.Count <= 0 Then Return _rVal
            If aOccurance <= 0 Then aOccurance = 1
            Dim cnt As Integer
            Dim i As Integer
            Dim bDone As Boolean
            Dim bDoIt As Boolean
            Dim j As Integer
            Dim aMem As TPROPERTY
            Dim bMem As TPROPERTY
            For i = 1 To Count
                aMem = _Members(i - 1)
                If bDone Then
                    _rVal.Add(aMem.Clone)
                Else
                    If aMem.GroupCode = aGC Then
                        cnt += 1
                        bDoIt = cnt = aOccurance
                    End If
                    If bDoIt Then
                        If bKeepMem Then _rVal.Add(New TPROPERTY(aMem))
                        For j = 1 To aSubSet.Count
                            bMem = aSubSet.Item(j)
                            _rVal.Add(New TPROPERTY(bMem))
                        Next j
                        bDone = True
                    Else
                        _rVal.Add(New TPROPERTY(aMem))
                    End If
                End If
            Next i
            ReIndex()
            Return _rVal
        End Function
        Public Function RemoveToCount(aCount As Integer) As TPROPERTIES
            Dim _rVal As TPROPERTIES = Clone(bReturnEmpty:=True)
            Dim aMem As TPROPERTY
            Dim cnt As Integer
            Dim hProps As New List(Of TPROPERTY)
            Dim keepMems As New List(Of TPROPERTY)
            For i As Integer = 1 To Count
                aMem = _Members(i - 1)
                If Not aMem.Hidden Then
                    cnt += 1
                    If cnt <= aCount Then
                        keepMems.Add(aMem)
                    Else
                        _rVal.Add(aMem)
                    End If
                Else
                    hProps.Add(aMem)
                End If
            Next i
            For i As Integer = 1 To hProps.Count
                aMem = hProps(i - 1)
                keepMems.Add(aMem)
            Next i
            Repopulate(keepMems)
            Return _rVal
        End Function
        Public Function RemoveToKey(aKey As String) As TPROPERTIES
            Dim bMem As TPROPERTY = Item(aKey)
            If bMem.Key = "" Then Return Clone(False)
            Dim _rVal As TPROPERTIES = Clone(bReturnEmpty:=True)
            Dim aMem As TPROPERTY
            Dim hProps As New List(Of TPROPERTY)
            Dim keepMems As New List(Of TPROPERTY)
            Dim bKeep As Boolean = True
            For i As Integer = 1 To Count
                aMem = _Members(i - 1)
                If Not aMem.Hidden Then
                    If bKeep Then
                        keepMems.Add(aMem)
                    Else
                        _rVal.Add(aMem)
                    End If
                Else
                    hProps.Add(aMem)
                End If
                If aMem.Key = bMem.Key Then bKeep = False
            Next i
            For i As Integer = 1 To hProps.Count
                aMem = hProps(i - 1)
                keepMems.Add(aMem)
            Next i
            Repopulate(keepMems)

            Return _rVal
        End Function
        Friend Sub ReduceToGC(aGC As Integer)
            Dim nMems As New List(Of TPROPERTY)
            Dim bKeep As Boolean
            Dim aMem As TPROPERTY
            Dim bFound As Boolean = False
            Dim cnt As Integer = 0
            For i As Integer = Count To 1 Step -1
                aMem = _Members(i - 1)
                bKeep = True
                If Not aMem.Hidden And Not bFound Then
                    If aMem.GroupCode <> aGC Then
                        bKeep = False
                    Else
                        bKeep = True
                        bFound = True
                    End If
                End If
                If bKeep Then
                    cnt += 1
                    nMems.Add(aMem)
                End If
            Next i
            If bFound Then Repopulate(nMems)

        End Sub
        Public Sub UpdateByString(aPropList As String, Optional bAssignNames As Boolean = False, Optional bAppendUnfound As Boolean = False)
            If String.IsNullOrWhiteSpace(aPropList) Then Return
            aPropList = aPropList.Trim()
            Dim bProps As New TPROPERTIES("")
            Dim bProp As TPROPERTY
            Dim aProp As TPROPERTY = TPROPERTY.Null
            Dim i As Integer
            Dim idx As Integer
            Dim nProps As New TPROPERTIES("")
            bProps.AddByString(aPropList)
            bProps.SetOccurances()
            For i = 1 To bProps.Count
                bProp = bProps.Item(i)
                If TryGet(bProp.GroupCode, aProp, bProp.Occurance) Then
                    nProps.Add(bProp)
                Else
                    aProp.CopyValue(bProp)
                    If bAssignNames And bProp.Name <> "" Then aProp.Name = bProp.Name
                    SetItem(idx, aProp)
                End If
            Next i
            If bAppendUnfound And nProps.Count > 0 Then
                Append(nProps)
            End If
        End Sub
        Public Sub SetOccurances(Optional bMarkValue As Boolean = False, Optional bSuppressHiddenProps As Boolean = False)
            Dim i As Integer
            Dim j As Integer
            Dim aProp As TPROPERTY
            Dim bProp As TPROPERTY
            Dim k As Integer
            For i = 1 To Count
                aProp = _Members(i - 1)
                If Not aProp.Hidden Then
                    aProp.Mark = bMarkValue
                    aProp.Occurance = 0
                    If i = 1 Then
                        aProp.Follows = -1
                    Else
                        aProp.Follows = _Members(i - 2).GroupCode
                    End If
                    _Members(i - 1) = aProp
                End If
            Next i
            For i = 1 To Count
                aProp = _Members(i - 1)
                If Not aProp.Hidden Then
                    If aProp.Occurance = 0 Then
                        aProp.Occurance = 1
                        k = 2
                        For j = i + 1 To Count
                            bProp = _Members(j - 1)
                            If bProp.GroupCode = aProp.GroupCode Then
                                bProp.Occurance = k
                                _Members(j - 1) = bProp
                                k += 1
                            End If
                        Next j
                    End If
                    _Members(i - 1) = aProp
                End If
            Next i
            If bSuppressHiddenProps Then Return
            For i = 1 To Count
                aProp = _Members(i - 1)
                If aProp.Hidden Then
                    aProp.Mark = bMarkValue
                    aProp.Occurance = 0
                    If i = 1 Then
                        aProp.Follows = -1
                    Else
                        aProp.Follows = _Members(i - 2).GroupCode
                    End If
                    _Members(i - 1) = aProp
                End If
            Next i
            For i = 1 To Count
                aProp = _Members(i - 1)
                If aProp.Hidden Then
                    If aProp.Occurance = 0 Then
                        aProp.Occurance = 1
                        k = 2
                        For j = i + 1 To Count
                            bProp = _Members(j - 1)
                            If bProp.GroupCode = aProp.GroupCode Then
                                bProp.Occurance = k
                                _Members(j - 1) = bProp
                                k += 1
                            End If
                        Next j
                    End If
                    _Members(i - 1) = aProp
                End If
            Next i
        End Sub
        Public Function ValueStr(aIndexOrKey As Object, Optional aDefault As String = Nothing, Optional bReturnDefaultForNullString As Boolean = False) As String
            '1 based
            Dim aProp As TPROPERTY = Item(aIndexOrKey)
            Dim _rVal As String = String.Empty
            Dim sDefault As String = String.Empty
            If aDefault IsNot Nothing Then sDefault = sDefault.Trim()
            If aProp.Index > 0 Then
                If aProp.Value IsNot Nothing Then _rVal = aProp.Value.ToString()
            Else
                _rVal = sDefault
            End If
            If bReturnDefaultForNullString And _rVal.Trim() = "" Then _rVal = sDefault
            Return _rVal
        End Function
        Public Function ValueI(aIndexOrKey As Object, Optional bNonNegative As Boolean = False, Optional bNonZero As Boolean = False, Optional aDefault As Integer? = Nothing) As Integer
            '1 based
            Dim ptype As dxxPropertyTypes = dxxPropertyTypes.dxf_Integer
            Dim _rVal As Integer = Value(aIndexOrKey, aReturnType:=ptype)
            Dim vcontrol As mzValueControls = mzValueControls.None
            If bNonNegative Or bNonZero Then
                If bNonNegative And _rVal < 0 Then
                    If aDefault.HasValue Then
                        If aDefault.Value >= 0 Then _rVal = aDefault.Value Else vcontrol = mzValueControls.Positive
                    End If

                End If
                If bNonZero And _rVal = 0 Then
                    If aDefault.HasValue Then
                        If aDefault.Value <> 0 Then _rVal = aDefault.Value Else vcontrol += mzValueControls.NonZero
                    End If

                End If
                If vcontrol <> mzValueControls.None Then _rVal = TPROPERTY.SetValueByType(_rVal, ptype, aValueControl:=vcontrol)
            End If
            Return _rVal
        End Function
        Friend Function ValueL(aIndexOrKey As Object, Optional bNonNegative As Boolean = False, Optional bNonZero As Boolean = False, Optional aDefault As Long? = Nothing) As Long
            '1 based
            Dim ptype As dxxPropertyTypes = dxxPropertyTypes.dxf_Long
            Dim _rVal As Long = Value(aIndexOrKey, aReturnType:=ptype)
            Dim vcontrol As mzValueControls = mzValueControls.None
            If bNonNegative Or bNonZero Then
                If bNonNegative And _rVal < 0 Then
                    If aDefault.HasValue Then
                        If aDefault.Value >= 0 Then _rVal = aDefault Else vcontrol = mzValueControls.Positive
                    End If

                End If
                If bNonZero And _rVal = 0 Then
                    If aDefault.HasValue Then
                        If aDefault.Value <> 0 Then _rVal = aDefault.Value Else vcontrol += mzValueControls.NonZero
                    End If

                End If

            End If
            If vcontrol <> mzValueControls.None Then _rVal = TPROPERTY.SetValueByType(_rVal, ptype, aValueControl:=vcontrol)
            Return _rVal
        End Function
        Friend Function ValueB(aIndexOrKey As Object, Optional aDefault As Boolean = False) As Boolean
            Dim aMem As TPROPERTY = TPROPERTY.Null
            If TypeOf (aIndexOrKey) Is String Then
                If Not TryGet(TVALUES.To_STR(aIndexOrKey, bTrim:=True).ToUpper, aMem) Then Return aDefault
            Else
                aMem = Item(TVALUES.To_INT(aIndexOrKey))
                If aMem.Index < 0 Then Return aDefault
            End If
            Return TVALUES.ToBoolean(aMem.Value, aDefault, bSwitchVal:=aMem.PropType = dxxPropertyTypes.Switch)
        End Function
        Friend Function ValueD(aIndexOrKey As Object, Optional bNonNegative As Boolean = False, Optional bNonZero As Boolean = False, Optional aDefault As Double? = Nothing, Optional aMultiplier As Double? = Nothing) As Double
            Dim ptype As dxxPropertyTypes = dxxPropertyTypes.dxf_Double
            Dim _rVal As Double = Value(aIndexOrKey, aReturnType:=ptype)
            Dim vcontrol As mzValueControls = mzValueControls.None
            If bNonNegative Or bNonZero Then
                If bNonNegative And _rVal < 0 Then
                    If aDefault IsNot Nothing Then
                        If aDefault.HasValue Then
                            aDefault = TPROPERTY.SetValueByType(aDefault.Value, ptype)
                            If aDefault.Value >= 0 Then _rVal = aDefault.Value Else vcontrol = mzValueControls.Positive
                        End If
                    End If


                End If
                If bNonZero And _rVal = 0 Then
                    If aDefault IsNot Nothing Then
                        If aDefault.HasValue Then
                            aDefault = TPROPERTY.SetValueByType(aDefault.Value, ptype)
                            If aDefault.Value <> 0 Then _rVal = aDefault.Value Else vcontrol += mzValueControls.NonZero
                        End If
                    End If

                End If
                If vcontrol > 0 Then _rVal = TPROPERTY.SetValueByType(_rVal, ptype, aValueControl:=vcontrol)
            End If
            If aMultiplier IsNot Nothing Then
                If aMultiplier.HasValue Then _rVal *= aMultiplier.Value
            End If
            Return _rVal
        End Function
        Friend Function ValueD(aKeyOrGC As String, ByRef rValueChanged As Boolean, aDefault As Double, Optional bNonNegative As Boolean = False, Optional bNonZero As Boolean = False, Optional bSaveChanges As Boolean = True) As Double
            rValueChanged = False
            Dim aMem As New TPROPERTY("")
            If Not TryGet(aKeyOrGC, aMem) Then Return aDefault

            If aMem.PropType <> dxxPropertyTypes.dxf_Double Then
                aMem.PropType = dxxPropertyTypes.dxf_Double
                aMem.Value = TVALUES.To_DBL(aMem.Value)

                SetItem(aMem.Index, aMem)
            End If


            Dim _rVal As Double = TVALUES.To_DBL(aMem.Value)
            Dim oldval As Double = _rVal
            Dim vcontrol As mzValueControls = mzValueControls.None
            If bNonNegative Or bNonZero Then
                If bNonNegative And _rVal < 0 Then

                    If aDefault >= 0 Then _rVal = aDefault Else vcontrol = mzValueControls.Positive
                End If
                If bNonZero And _rVal = 0 Then

                    If aDefault <> 0 Then _rVal = aDefault Else vcontrol += mzValueControls.NonZero
                End If
                If vcontrol > 0 Then _rVal = TPROPERTY.SetValueByType(_rVal, dxxPropertyTypes.dxf_Double, aValueControl:=vcontrol)
            End If
            rValueChanged = oldval <> _rVal
            If rValueChanged Then

            End If
            If bSaveChanges And rValueChanged Then
                aMem.Value = _rVal
                SetItem(aMem.Index, aMem)
            End If
            Return _rVal
        End Function
        Friend Function ValueS(aIndexOrKey As Object, Optional bNonNegative As Boolean = False, Optional bNonZero As Boolean = False, Optional aDefault As Single? = Nothing) As Single
            '1 based
            Dim ptype As dxxPropertyTypes = dxxPropertyTypes.dxf_Single
            Dim _rVal As Single = Value(aIndexOrKey, aReturnType:=ptype)
            Dim vcontrol As mzValueControls = mzValueControls.None
            If bNonNegative Or bNonZero Then
                If bNonNegative And _rVal < 0 Then
                    If aDefault.HasValue Then
                        If aDefault.Value >= 0 Then _rVal = aDefault.Value Else vcontrol = mzValueControls.Positive
                    End If

                End If
                If bNonZero And _rVal = 0 Then
                    If aDefault.HasValue Then
                        If aDefault.Value <> 0 Then _rVal = aDefault.Value Else vcontrol += mzValueControls.NonZero
                    End If

                End If
                If vcontrol <> mzValueControls.None Then _rVal = TPROPERTY.SetValueByType(_rVal, ptype, aValueControl:=vcontrol)
            End If
            Return _rVal
        End Function
        Public Sub ReIndex(Optional bHiddenToEnd As Boolean = False)
            Dim aMem As TPROPERTY
            Dim idx As Integer = 0
            '_GroupCodes = New TDICTIONARY_GCS("")
            If bHiddenToEnd Then
                Dim bMems As New List(Of TPROPERTY)
                Dim vProps As New List(Of TPROPERTY)
                Dim hProps As New List(Of TPROPERTY)
                For i As Integer = 1 To Count
                    aMem = _Members(i - 1)
                    If Not aMem.Hidden Then
                        vProps.Add(aMem)
                    Else
                        hProps.Add(aMem)
                    End If
                Next i
                For i As Integer = 1 To vProps.Count
                    idx += 1
                    aMem = vProps(i - 1)
                    If Not SuppressIndices Then aMem.Index = idx
                    bMems.Add(aMem)
                    '_GroupCodes.Add(aMem)
                Next
                For i As Integer = 1 To hProps.Count
                    idx += 1
                    aMem = hProps(i - 1)
                    If Not SuppressIndices Then aMem.Index = idx
                    bMems.Add(aMem)
                    '_GroupCodes.Add(aMem)
                Next
                Repopulate(bMems)
            Else
                For i As Integer = 1 To Count
                    aMem = _Members(i - 1)
                    '_GroupCodes.Add(aMem)
                    If Not SuppressIndices Then aMem.Index = i
                    _Members(i - 1) = aMem
                Next i
            End If
        End Sub
        Public Function Multiply(aNameOrKeyList As String, aDelimitor As String, aFactor As Double) As String
            Dim _rVal As String = String.Empty
            If aFactor = 1 Then Return String.Empty

            Dim sVals As TVALUES = TLISTS.ToValues(aNameOrKeyList, aDelimitor, False, True, True)
            If sVals.Count <= 0 Then Return String.Empty
            Dim pname As String
            Dim dval As Double
            Dim aMem As TPROPERTY = TPROPERTY.Null
            For i As Integer = 1 To sVals.Count
                pname = sVals.Item(i).ToString().Trim()
                If TryGet(pname, aMem) Then
                    dval = TVALUES.To_DBL(aMem.Value) * aFactor
                    If aMem.SetVal(dval) Then
                        TLISTS.Add(_rVal, aMem.Name)
                        _Members(i - 1) = aMem
                    End If
                End If
            Next
            Return _rVal
        End Function
        Public Function Scale(aMultiplier As Double, Optional aSkipList As String = "") As Boolean
            Dim _rVal As Boolean

            Dim aMem As TPROPERTY

            For i As Integer = 1 To Count
                aMem = _Members(i - 1)
                If aMem.Scaleable Then
                    If Not TLISTS.Contains(aMem.Name, aSkipList) Then

                        If aMem.SetVal(TVALUES.To_DBL(aMem.Value) * aMultiplier) Then _rVal = True

                    End If
                End If
                _Members(i - 1) = aMem
            Next i
            Return _rVal
        End Function
        Public Function Wrapped(aGC As Integer, aValue As Object, aName As String, bGC As Integer, bValue As Object, bName As String, Optional bExcludeHidden As Boolean = True) As TPROPERTIES
            Dim _rVal As TPROPERTIES = Clone(bReturnEmpty:=True)
            If aValue Is Nothing Or bValue Is Nothing Then Return _rVal
            _rVal.Add(New TPROPERTY(aGC, aValue, aName, dxxPropertyTypes.Undefined))
            _rVal.Append(Me, bExcludeHidden)
            _rVal.Add(New TPROPERTY(bGC, bValue, bName, dxxPropertyTypes.Undefined))
            Return _rVal
        End Function
        Public Function ValueList(Optional bSuppressNames As Boolean = False, Optional aSkipList As String = "", Optional aStartID As Integer = 0, Optional aEndID As Integer = 0, Optional aDelimiter As String = ",") As String
            Dim _rVal As String = String.Empty
            If IsEmpty Then Return String.Empty
            Dim aProp As TPROPERTY
            Dim bDoIt As Boolean
            Dim bListPassed As Boolean = Not String.IsNullOrWhiteSpace(aSkipList)
            Dim aStr As String
            Dim si As Integer
            Dim ei As Integer
            If Not dxfUtils.LoopIndices(Count, aStartID, aEndID, si, ei) Then Return String.Empty
            If bListPassed Then aSkipList = aSkipList.Trim()
            For i As Integer = si To ei
                If i > Count Then Exit For
                aProp = _Members(i - 1)
                bDoIt = True
                If bListPassed Then
                    If TLISTS.Contains(aProp.GroupCode, aSkipList) Then bDoIt = False
                End If
                If bDoIt Then
                    If Not bSuppressNames Then
                        aStr = "[" & aProp.Name & "]"
                    Else
                        aStr = ""
                    End If
                    aStr += aProp.GroupCode & "=" & aProp.Value
                    If _rVal <> "" Then _rVal += aDelimiter
                    _rVal += aStr
                End If
            Next i
            Return _rVal
        End Function
        Public Function ValueListGC(aGC As Integer, bUniqueOnly As Boolean, Optional bSuppressNulls As Boolean = False, Optional aDelimiter As String = ",") As String
            Dim _rVal As String = String.Empty
            Dim i As Integer
            Dim aProp As TPROPERTY
            Dim cnt As Integer = Count
            For i = 1 To cnt
                aProp = _Members(i - 1)
                If aProp.GroupCode = aGC Then
                    TLISTS.Add(_rVal, aProp.Value, bAllowDuplicates:=Not bUniqueOnly, aDelimitor:=aDelimiter, bAllowNulls:=Not bSuppressNulls)
                End If
            Next i
            Return _rVal
        End Function
        Public Function SubSet(aStart As Integer, aEnd As Integer, Optional bSuppressFirst As Boolean = False, Optional bSuppressLast As Boolean = False) As TPROPERTIES
            Dim _rVal As TPROPERTIES = Clone(bReturnEmpty:=True)
            _rVal.SuppressIndices = True
            Dim i As Integer
            Dim si As Integer
            Dim ei As Integer
            Dim bKeep As Boolean
            si = aStart
            ei = aEnd
            TVALUES.SortTwoValues(True, si, ei)
            For i = 1 To Count
                bKeep = i + 1 >= si And i + 1 <= ei
                If bKeep Then
                    If bSuppressFirst Then
                        If i + 1 = si Then bKeep = False
                    End If
                End If
                If bKeep Then
                    If bSuppressLast Then
                        If i + 1 = ei Then bKeep = False
                    End If
                End If
                If bKeep Then
                    _rVal.Add(New TPROPERTY(Item(i)))
                End If
            Next i
            Return _rVal
        End Function
        Public Function CopyValue(aFromProps As TPROPERTIES, aIndexOrKey As Object, Optional aDefStringVal As String = Nothing) As Boolean
            'base 1
            ' Dim rLastValue As Object
            Dim _rVal As Boolean
            If aFromProps.Count <= 0 Then Return False
            Dim aProp As TPROPERTY = Item(aIndexOrKey)
            If aProp.Index < 0 Then Return False
            Dim bProp As TPROPERTY = aFromProps.Item(aIndexOrKey)
            If bProp.Index < 0 Then Return False
            If aProp.GroupCode <> bProp.GroupCode Then Return False
            If aProp.PropType >= dxxPropertyTypes.dxf_String And aProp.PropType <= dxxPropertyTypes.ClassMarker And aDefStringVal IsNot Nothing Then
                If aProp.Value.ToString() = "" Then aProp.Value = aDefStringVal
                If bProp.Value.ToString() = "" Then
                    bProp.Value = aDefStringVal
                    aFromProps.SetItem(bProp.Index, bProp)
                End If
            End If
            _rVal = aProp.SetVal(bProp.Value)
            '  rLastValue = aProp.LastValue
            SetItem(aProp.Index, aProp)
            Return _rVal
        End Function
        Public Function HandleList(Optional bSuppressPointers As Boolean = True, Optional bUniqueOnly As Boolean = True, Optional bSuppressNulls As Boolean = True, Optional aDelimiter As String = ",") As String
            Dim _rVal As String = String.Empty
            Dim i As Integer
            Dim aProp As TPROPERTY
            Dim cnt As Integer = Count
            For i = 1 To cnt
                aProp = _Members(i - 1)
                If aProp.PropType = dxxPropertyTypes.Handle Or (aProp.PropType = dxxPropertyTypes.Pointer And Not bSuppressPointers) Then
                    TLISTS.Add(_rVal, aProp.Value, bAllowDuplicates:=Not bUniqueOnly, aDelimitor:=aDelimiter, bAllowNulls:=Not bSuppressNulls)
                End If
            Next i
            Return _rVal
        End Function
        Public Function SetSuppressionByGC(aGroupCodeList As String, aSuppressValue As Boolean, Optional bToggleNonMembers As Boolean = False, Optional aSkipList As String = "", Optional aStartID As Integer = 0) As Integer
            Dim _rVal As Integer
            If String.IsNullOrWhiteSpace(aGroupCodeList) Then Return _rVal
            aGroupCodeList = aGroupCodeList.Trim()
            Dim i As Integer
            Dim aGC As Integer
            Dim bDoIt As Boolean
            Dim bListPassed As Boolean = Not String.IsNullOrWhiteSpace(aSkipList)
            Dim si As Integer
            Dim cnt As Integer = Count
            Dim aMem As TPROPERTY
            If aStartID > 1 Then si = aStartID Else si = 1
            If bListPassed Then aSkipList = aSkipList.Trim()
            For i = si To cnt
                aMem = _Members(i - 1)
                aGC = aMem.GroupCode
                bDoIt = aMem.PropType <> dxxPropertyTypes.Handle
                If bDoIt And bListPassed Then
                    If TLISTS.Contains(aGC, aSkipList) Then bDoIt = False
                End If
                If bDoIt Then
                    If TLISTS.Contains(aGC, aGroupCodeList) Then
                        If aMem.Suppressed <> aSuppressValue Then _rVal += 1
                        aMem.Suppressed = aSuppressValue
                    Else
                        If bToggleNonMembers Then
                            If aMem.Suppressed = aSuppressValue Then _rVal += 1
                            aMem.Suppressed = Not aSuppressValue
                        End If
                    End If
                End If
                _Members(i - 1) = aMem
            Next i
            Return _rVal
        End Function
        Public Function InvertSwitch(aGCOrKey As Object, Optional aOccurance As Integer = 1) As Boolean
            Dim _rVal As Boolean
            Dim aProp As TPROPERTY = GetMember(aGCOrKey, aOccurance) ', arHidden)
            If aProp.Name = "" Then Return False
            If aProp.Value = 1 Then aProp.Value = 0 Else aProp.Value = 1
            _rVal = aProp.Value = 1
            UpdateProperty = aProp
            Return _rVal
        End Function
        Public Function GetByGroupCode(aGroupCode As Integer, Optional aMatchValue As Object = Nothing, Optional aFollowerCount As Integer = 0, Optional bRemove As Boolean = False, Optional aName As String = "", Optional bJustOne As Boolean = False, Optional aNameList As String = "") As TPROPERTIES
            Dim rFirstIndex As Integer = 0
            Return GetByGroupCode(aGroupCode, aMatchValue, aFollowerCount, bRemove, aName, bJustOne, rFirstIndex, aNameList)
        End Function
        Public Function GetByGroupCode(aGroupCode As Integer, aMatchValue As Object, aFollowerCount As Integer, bRemove As Boolean, aName As String, bJustOne As Boolean, ByRef rFirstIndex As Integer, Optional aNameList As String = "") As TPROPERTIES
            Dim _rVal As New TPROPERTIES(aName) With {.SuppressIndices = True}

            Dim cnt As Integer = Count
            If cnt <= 0 Then Return _rVal


            Dim bTestA As Boolean
            Dim strTestA As String = String.Empty
            Dim bKeep As Boolean

            Dim rIDs As List(Of Integer) = Nothing

            Dim bNamesPassed As Boolean
            Dim pNames(0) As String
            Dim ncnt As Integer
            If Not String.IsNullOrWhiteSpace(aNameList) Then aNameList = aNameList.Trim() Else aNameList = ""
            bNamesPassed = aNameList <> ""
            If bNamesPassed Then
                pNames = aNameList.Split(",")
                ncnt = pNames.Length - 1
            End If
            rFirstIndex = 0
            If bRemove Then rIDs = New List(Of Integer)
            If aMatchValue IsNot Nothing Then
                strTestA = aMatchValue.ToString()
                bTestA = True

            End If

            For i As Integer = 1 To cnt
                Dim aProp As TPROPERTY = Item(i)
                If aProp.GroupCode = aGroupCode Then
                    bKeep = True
                    If bTestA Then
                        If String.Compare(aProp.Value.ToString(), strTestA, True) <> 0 Then
                            bKeep = False
                        End If
                    End If
                    If bKeep Then
                        If rFirstIndex = 0 Then rFirstIndex = i
                        If bNamesPassed Then
                            aProp.Name = pNames(0)
                        End If
                        _rVal.Add(aProp)
                        If bRemove Then
                            rIDs.Add(i - 1)
                        End If
                        If aFollowerCount > 0 Then
                            Dim iskip As Integer = 0
                            For j As Integer = 1 To aFollowerCount
                                If i + j <= cnt Then
                                    iskip += 1
                                    aProp = _Members(i + j - 1)
                                    If bRemove Then rIDs.Add(i + j)
                                    If bNamesPassed Then
                                        If j <= ncnt Then
                                            aProp.Name = pNames(j)
                                        End If
                                    End If
                                    _rVal.Add(aProp)
                                End If
                            Next j
                            i += iskip
                            If i >= cnt Then Exit For
                        End If
                    End If
                End If
                If bJustOne And _rVal.Count > 0 Then Exit For
            Next i
            If bRemove Then
                If rIDs.Count > 0 Then
                    Dim aNewMems As New List(Of TPROPERTY)
                    For i As Integer = 1 To cnt
                        Dim aProp As TPROPERTY = _Members(i - 1)
                        Dim srch As Integer = i
                        bKeep = rIDs.FindIndex(Function(x) x = srch) < 0
                        If bKeep Then aNewMems.Add(aProp)
                    Next i
                    Repopulate(aNewMems)
                End If
            End If
            Return _rVal
        End Function


        Public Sub AddHeader(aDimStyleProps As TPROPERTIES, aPropName As String, aGroupCode As Integer, aValue As Object, Optional bIsSwitch As Boolean = False, Optional bTwoD As Boolean = False, Optional bThreeD As Boolean = False, Optional bIsDirection As Boolean = False, Optional bSuppressed As Boolean = False, Optional aPropertyType As dxxPropertyTypes = dxxPropertyTypes.Undefined, Optional aDecodeString As String = "")
            '#1the descriptor string for the header property to add
            '#2flag to indicate if the property should be included in dxo file output
            '^adds a header property bassed on the passed string
            'On Error Resume Next
            If String.IsNullOrWhiteSpace(aPropName) Then Throw New Exception("Header Properties Must Have Names")
            aPropName = aPropName.Trim().ToUpper()
            Dim v1 As TVECTOR
            Dim ptype As dxxPropertyTypes
            Dim dsProp As TPROPERTY
            Dim pname As String
            Dim supVal As Object
            ptype = aPropertyType
            If bIsSwitch Then ptype = dxxPropertyTypes.Switch
            If Not aPropName.StartsWith("$") Then aPropName = "$" + aPropName
            If bTwoD Or bThreeD Then
                v1 = TVECTOR.DefineByString(aValue.ToString(), v1)
                AddVector(aGroupCode, v1, aPropName, bTwoD, bSuppressed, bIsDirection)
            Else
                If Left(aPropName, 4) = "$DIM" Then
                    If aPropName <> "$DIMSTYLE" Then
                        pname = aPropName.Substring(1, aPropName.Length - 1)
                        If pname <> "DIMASSOC" And pname <> "DIMASO" And pname <> "DIMSHO" Then
                            Select Case pname
                                Case "DIMTXSTY"
                                    pname = $"*{ pname}"
                                Case "DIMBLK", "DIMBLK1", "DIMBLK2"
                                    pname = $"*{ pname}"
                                Case "DIMLTYPE", "DIMLTEX1", "DIMLTEX2"
                                    pname = $"*{ pname}"
                            End Select
                            dsProp = aDimStyleProps.GetMember(pname)
                            If dsProp.Name <> "" Then
                                ptype = dsProp.PropType
                                aValue = dsProp.Value
                                supVal = dsProp.SuppressedValue
                            End If
                        End If
                    End If
                End If
                Dim aProp As TPROPERTY = Add(aGroupCode, aValue, aPropName, Nothing, ptype, bSuppressed)
                aProp.DecodeString = aDecodeString
                SetItem(Count, aProp)
            End If
        End Sub
        Public Sub CopyValuesByGC(aFromProps As TPROPERTIES, Optional aSkipList As String = "", Optional bSuppressOcurrances As Boolean = False, Optional bCollate As Boolean = False, Optional bIgnoreHandles As Boolean = False, Optional bIgnoreVectors As Boolean = False)
            Dim rCollatedCnt As Integer = 0
            Dim rCollatedList As String = String.Empty
            CopyValuesByGC(aFromProps, aSkipList, bSuppressOcurrances, bCollate, rCollatedCnt, rCollatedList, bIgnoreHandles, bIgnoreVectors)
        End Sub
        Public Sub CopyValuesByGC(aFromProps As TPROPERTIES, aSkipList As String, bSuppressOcurrances As Boolean, bCollate As Boolean, ByRef rCollatedCnt As Integer, ByRef rCollatedList As String, Optional bIgnoreHandles As Boolean = False, Optional bIgnoreVectors As Boolean = False)
            Dim aProp As TPROPERTY
            Dim bProp As TPROPERTY = TPROPERTY.Null

            Dim rProps As TPROPERTIES = Clone(bReturnEmpty:=True)
            Dim bKeep As Boolean
            rCollatedCnt = 0
            rCollatedList = ""
            If aSkipList Is Nothing Then aSkipList = "" Else aSkipList = aSkipList.Trim()
            aFromProps.SetOccurances()
            SetOccurances()
            For i As Integer = 1 To aFromProps.Count
                aProp = aFromProps.Item(i)
                bKeep = Not TLISTS.Contains(aProp.GroupCode, aSkipList)
                If bKeep Then
                    bProp = GetByGC(aProp.GroupCode, aProp.Occurance)
                End If
                If bIgnoreHandles Then
                    If aProp.PropType = dxxPropertyTypes.Handle Or bProp.PropType = dxxPropertyTypes.Handle Then bKeep = False
                    If aProp.PropType = dxxPropertyTypes.Pointer Or bProp.PropType = dxxPropertyTypes.Pointer Then bKeep = False
                End If
                If bIgnoreVectors Then
                    If aProp.IsOrdinate Then bKeep = False
                End If
                If bKeep Then
                    If aProp.Index > 0 Then

                        bProp = Item(aProp.Index)
                        bProp.CopyValue(aProp)
                        UpdateProperty = bProp
                    Else
                        rProps.SuppressIndices = True
                        rProps.Add(aProp)
                    End If
                End If
            Next i
            If rProps.Count > 0 And bCollate Then
                rCollatedCnt = rProps.Count
                For i = 1 To rProps.Count
                    aProp = rProps.Item(i)
                    If aProp.Follows > 0 Then
                        If TryGet(aProp.Follows, bProp) Then
                            Add(aProp)

                        Else
                            Add(aProp)
                        End If
                    Else
                        Add(aProp)
                    End If
                    TLISTS.Add(rCollatedList, $"{aProp.GroupCode }={ aProp.Value}", bAllowDuplicates:=True)
                Next i
            End If
        End Sub
        Public Function Compare(bProps As TPROPERTIES, ByRef rDifferences As TPROPERTIES, ByRef rNotFoundInA As TPROPERTIES, ByRef rNotFoundInB As TPROPERTIES, Optional aSkipList As String = "", Optional bSuppressHiddenProps As Boolean = False, Optional bCompareHandles As Boolean = False, Optional bComparePointers As Boolean = False, Optional bCompareHidden As Boolean = True) As Boolean
            Dim _rVal As Boolean
            SetOccurances(True, bSuppressHiddenProps)
            bProps.SetOccurances(False, bSuppressHiddenProps)
            rDifferences = New TPROPERTIES("Differences")
            rNotFoundInA = rDifferences
            rNotFoundInB = rDifferences
            _rVal = True
            Dim bTestGC As Boolean
            Dim aProp As TPROPERTY
            Dim bProp As TPROPERTY = TPROPERTY.Null
            Dim bMems(0) As TPROPERTY
            Dim bDoIt As Boolean
            Dim k As Integer
            Dim aCnt As Integer = Count
            Dim bcnt As Integer = bProps.Count
            Dim ocnt As Integer
            Dim bEqual As Boolean
            Dim idx As Integer
            If aSkipList Is Nothing Then aSkipList = "" Else aSkipList = aSkipList.Trim()
            bTestGC = aSkipList <> ""
            For i As Integer = 1 To aCnt
                If i > bcnt Then Exit For
                aProp = _Members(i - 1)
                bDoIt = True
                If Not bCompareHidden And aProp.Hidden Then bDoIt = False
                'skip the props in the passed group code list
                If bTestGC And bDoIt Then
                    If TLISTS.Contains(aProp.GroupCode, aSkipList) Then
                        bDoIt = False
                    End If
                End If
                If bDoIt Then
                    idx = -1
                    ocnt = 0
                    For k = 1 To bcnt
                        If bMems(k - 1).GroupCode = aProp.GroupCode Then
                            ocnt += 1
                            If ocnt = aProp.Occurance Then
                                idx = k
                                bMems(k - 1).Mark = True
                                bProp = bMems(k - 1)
                                Exit For
                            End If
                        End If
                    Next k
                    If idx >= 0 Then
                        'the a property was found in the b array
                        bEqual = aProp.Compare(bProp, bCompareHandles:=bCompareHandles, bComparePointers:=bComparePointers)
                        If Not bEqual Then
                            aProp.LastValue = bProp.Value
                            rDifferences.Add(aProp)
                        End If
                    Else
                        'the property was not found in b
                        rNotFoundInB.Add(aProp)
                    End If
                End If
            Next i
            'return thr props in b that were not found in the a array
            For k = 1 To bcnt
                aProp = bMems(k - 1)
                bDoIt = True
                If Not bCompareHidden And aProp.Hidden Then bDoIt = False
                'skip the props in the passed group code list
                If Not bCompareHandles And aProp.PropType = dxxPropertyTypes.Handle Then bDoIt = False
                If Not bComparePointers And aProp.PropType = dxxPropertyTypes.Pointer Then bDoIt = False
                'skip the props in the passed group code list
                If bTestGC And bDoIt Then
                    If TLISTS.Contains(aProp.GroupCode, aSkipList) Then
                        bDoIt = False
                    End If
                End If
                If Not aProp.Mark And bDoIt Then
                    rNotFoundInA.Add(aProp)
                End If
            Next k
            If rDifferences.Count > 0 Or rNotFoundInA.Count > 0 Or rNotFoundInB.Count > 0 Then
                _rVal = False
            End If
            Return _rVal
        End Function
        Public Sub AddMatrix(aGroupCode As Integer, aName As String, aMatrix As TMATRIX4)
            If String.IsNullOrWhiteSpace(aName) Then aName = aMatrix.Name
            If String.IsNullOrWhiteSpace(aName) Then aName = "Matrix"
            aName = aName.Trim()
            Dim ptype As dxxPropertyTypes = dxxPropertyTypes.Undefined
            Add(New TPROPERTY(aGroupCode, aMatrix.A.X, $"{aName}-(1,1)", dxxPropertyTypes.Undefined))
            If Count > 0 Then ptype = _Members(Count - 1).PropType
            Add(New TPROPERTY(aGroupCode, aMatrix.A.Y, $"{aName}-(1,2)", ptype))
            Add(New TPROPERTY(aGroupCode, aMatrix.A.Z, $"{aName}-(1,3)", ptype))
            Add(New TPROPERTY(aGroupCode, aMatrix.A.s, $"{aName}-(1,4)", ptype))
            Add(New TPROPERTY(aGroupCode, aMatrix.B.X, $"{aName}-(2,1)", ptype))
            Add(New TPROPERTY(aGroupCode, aMatrix.B.Y, $"{aName}-(2,2)", ptype))
            Add(New TPROPERTY(aGroupCode, aMatrix.B.Z, $"{aName}-(2,3)", ptype))
            Add(New TPROPERTY(aGroupCode, aMatrix.B.s, $"{aName}-(2,4)", ptype))
            Add(New TPROPERTY(aGroupCode, aMatrix.C.X, $"{aName}-(3,1)", ptype))
            Add(New TPROPERTY(aGroupCode, aMatrix.C.Y, $"{aName}-(3,2)", ptype))
            Add(New TPROPERTY(aGroupCode, aMatrix.C.Z, $"{aName}-(3,3)", ptype))
            Add(New TPROPERTY(aGroupCode, aMatrix.C.s, $"{aName}-(3,4)", ptype))
            Add(New TPROPERTY(aGroupCode, aMatrix.D.X, $"{aName}-(4,1)", ptype))
            Add(New TPROPERTY(aGroupCode, aMatrix.D.Y, $"{aName}-(4,2)", ptype))
            Add(New TPROPERTY(aGroupCode, aMatrix.D.Z, $"{aName}-(4,3)", ptype))
            Add(New TPROPERTY(aGroupCode, aMatrix.D.s, $"{aName}-(4,4)", ptype))
        End Sub
        Public Sub AddByString(aPropList As String, Optional aDelimiter As String = ",", Optional aHandleGC As Integer = 5, Optional bSuppressPointers As Boolean = False)
            If String.IsNullOrWhiteSpace(aPropList) Then Return
            aPropList = aPropList.Trim()

            Dim sVals() As String
            Dim pVals() As String
            Dim i As Integer
            Dim pStr As String
            Dim pnamegc As String
            Dim pname As String = String.Empty
            Dim pval As String
            Dim aFlg As Boolean
            Dim aGC As Integer
            Dim bIsVec As Boolean
            Dim b2D As Boolean
            Dim v1 As TVECTOR
            Dim iCnt As Integer
            Dim j As Integer
            sVals = aPropList.Split(aDelimiter)
            For i = 0 To sVals.Length - 1
                pStr = sVals(i)
                If pStr.Contains("=") Then
                    pVals = pStr.Split("=")
                    If pVals.Length - 1 > 1 Then
                        For j = 2 To pVals.Length - 1
                            pVals(1) = $"{pVals(1) }={ pVals(j)}"
                        Next j
                    End If
                Else
                    ReDim pVals(0 To 1)
                    pVals(0) = pStr
                End If
                pnamegc = pVals(0).Trim()
                pval = pVals(1).Trim()
                If pnamegc.Length > 0 Then
                    aGC = dxfUtils.ExtractTrailingIndex(pnamegc, aFlg, True, pname)
                    If Not aFlg Then
                        pname = dxfUtils.StripParens(pval, "[", "]")

                        bIsVec = False

                        If pval.StartsWith("(") Then

                            Dim zset As Boolean
                            v1 = TVECTOR.DefineByString(pval, Nothing, ",", zset, iCnt)
                            If iCnt = 2 Or iCnt = 3 Then
                                bIsVec = True
                                b2D = iCnt = 2
                            End If

                        End If
                        If bIsVec Then
                            AddVector(aGC, v1, pname, b2D)
                        Else
                            Dim aProp As TPROPERTY = Add(New TPROPERTY(aGC, pval, pname.Trim(), dxxPropertyTypes.Undefined))
                            If aProp.GroupCode = aHandleGC Then aProp.PropType = dxxPropertyTypes.Handle
                            If bSuppressPointers Then
                                If aProp.IsPointer Then aProp.PropType = dxxPropertyTypes.dxf_String
                            End If
                            SetItem(Count, aProp)
                        End If
                    End If
                End If
            Next i
        End Sub
        Public Function GetByStringValue(aString As String, Optional iOccurance As Integer = 1, Optional aGC As Integer = -9999, Optional bPointersOnly As Boolean = False) As TPROPERTY
            Dim rIndex As Integer
            Return GetByStringValue(aString, iOccurance, rIndex, aGC, bPointersOnly)
        End Function
        Public Function GetByStringValue(aString As String, iOccurance As Integer, ByRef rIndex As Integer, Optional aGC As Integer = -9999, Optional bPointersOnly As Boolean = False) As TPROPERTY
            Dim _rVal As TPROPERTY = TPROPERTY.Null
            If String.IsNullOrWhiteSpace(aString) Then aString = ""
            Dim aProp As TPROPERTY
            Dim aVal As String
            Dim cnt As Integer
            Dim bTestGC As Boolean
            bTestGC = aGC <> -9999
            aString = aString.Trim()
            If iOccurance <= 0 Then iOccurance = 1
            rIndex = 0
            For i As Integer = 1 To Count
                aProp = Item(i)
                aVal = TVALUES.To_STR(aProp.Value)
                If String.Compare(aString, aVal, True) = 0 Then
                    If (bTestGC And aProp.GroupCode = aGC) Or Not bTestGC Then
                        If Not bPointersOnly Or (bPointersOnly And aProp.PropType = dxxPropertyTypes.Pointer) Then
                            cnt += 1
                            If cnt = iOccurance Then
                                _rVal = aProp
                                rIndex = i
                                Exit For
                            End If
                        End If
                    End If
                End If
                'Application.DoEvents()
            Next i
            Return _rVal
        End Function
        Public Function StringValueKey(aString As String, Optional iOccurance As Integer = 1, Optional aGC As Integer = -9999, Optional bPointersOnly As Boolean = False) As String
            Dim _rVal As String = String.Empty
            If String.IsNullOrWhiteSpace(aString) Then aString = ""
            Dim i As Integer
            Dim aProp As TPROPERTY
            Dim aVal As String
            Dim cnt As Integer
            Dim bTestGC As Boolean
            bTestGC = aGC <> -9999
            aString = aString.Trim()
            If iOccurance <= 0 Then iOccurance = 1
            For i = 1 To Count
                aProp = Item(i)
                aVal = aProp.StringValue
                If String.Compare(aString, aVal, True) = 0 Then
                    If (bTestGC And aProp.GroupCode = aGC) Or Not bTestGC Then
                        If Not bPointersOnly Or (bPointersOnly And aProp.PropType = dxxPropertyTypes.Pointer) Then
                            cnt += 1
                            If cnt = iOccurance Then
                                _rVal = aProp.Key
                                Exit For
                            End If
                        End If
                    End If
                End If
                'Application.DoEvents()
            Next i
            Return _rVal
        End Function
        Public Function VectorByName(aName As String, aOccur As Integer, ByRef rFound As Boolean, Optional aDefaultX As Double = 0.0, Optional aDefaultY As Double = 0.0, Optional aDefaultZ As Double = 0.0) As TVECTOR


            Dim aMem As TPROPERTY = TPROPERTY.Null
            rFound = TryGetVector(aName, aMem)
            If Not rFound Then Return New TVECTOR(aDefaultX, aDefaultY, aDefaultZ)
            Return Vector(aMem.Index)

        End Function
        Public Function PlaneByName(aBasePlane As TPLANE, aOriginName As String, aXDirectionName As String, aYDirectionName As String) As TPLANE
            Dim _rVal As TPLANE = aBasePlane
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim v3 As TVECTOR
            Dim bFlag As Boolean
            Dim bFound As Boolean
            If aOriginName <> "" Then
                v1 = VectorByName(aOriginName, 1, bFound, aBasePlane.Origin.X, aBasePlane.Origin.Y, aBasePlane.Origin.Z)
                If bFound Then _rVal.Origin = v1
            End If
            If aXDirectionName <> "" And aYDirectionName <> "" Then
                v1 = VectorByName(aXDirectionName, 1, bFound, aBasePlane.XDirection.X, aBasePlane.XDirection.Y, aBasePlane.XDirection.Z).Normalized(bFlag)
                If bFound And Not bFlag Then
                    v2 = VectorByName(aYDirectionName, 1, bFound, aBasePlane.YDirection.X, aBasePlane.YDirection.Y, aBasePlane.YDirection.Z).Normalized(bFlag)
                    If bFound And Not bFlag Then
                        v3 = v1.CrossProduct(v2, True)
                        If v1.AngleTo(v2, v3, 2) = 90 Then
                            _rVal.Define(_rVal.Origin, v1, v2)
                        End If
                    End If
                End If
            End If
            Return _rVal
        End Function
        Public Function Vertices(aGroupCode As Integer) As TVERTICES
            Dim _rVal As New TVERTICES
            Dim aProp As TPROPERTY
            Dim bProp As TPROPERTY
            Dim bFlag As Boolean
            Dim j As Integer
            Dim cnt As Integer = Count
            For i As Integer = 1 To cnt
                aProp = _Members(i - 1)
                If aProp.GroupCode = aGroupCode Then
                    Dim v1 As New TVERTEX(Vector(aProp.Index))

                    j = i
                    bFlag = False
                    Do Until bFlag
                        j += 1
                        If j <= cnt - 1 Then
                            bProp = _Members(j - 1)
                            If (bProp.GroupCode = 40) Or (bProp.GroupCode = 41) Or (bProp.GroupCode = 42) Then
                                If bProp.GroupCode = 40 Then
                                    v1.StartWidth = TVALUES.To_DBL(bProp.Value)
                                ElseIf bProp.GroupCode = 41 Then
                                    v1.EndWidth = TVALUES.To_DBL(bProp.Value)
                                ElseIf bProp.GroupCode = 42 Then
                                    v1.Bulge = TVALUES.To_DBL(bProp.Value)
                                End If
                                i = j
                            Else
                                bFlag = True
                                i = j - 1
                            End If
                        Else
                            bFlag = True
                            i = j - 1
                        End If
                    Loop
                    _rVal.Add(v1)
                End If
                'Application.DoEvents()
                If i >= cnt Then Exit For
            Next i
            Return _rVal
        End Function
        Public Function GetGroupCodeRange(aLowGC As Integer, aHighGC As Integer, Optional bRetainIndices As Boolean = False) As TPROPERTIES
            Dim _rVal As TPROPERTIES = Clone(bReturnEmpty:=True)
            _rVal.SuppressIndices = bRetainIndices
            Dim i As Integer
            Dim lVal As Integer
            Dim hVal As Integer
            Dim cnt As Integer = Count
            lVal = aLowGC
            hVal = aHighGC
            TVALUES.SortTwoValues(True, lVal, hVal)
            _rVal.SuppressIndices = bRetainIndices
            For i = 1 To cnt
                If _Members(i - 1).GroupCode >= lVal And _Members(i - 1).GroupCode <= hVal Then
                    _rVal.Add(New TPROPERTY(_Members(i - 1)))
                End If
            Next i
            Return _rVal
        End Function
        Public Function GetGroupCodePair(aGC As Integer, bGC As Integer, aMatchValue As Object, bMatchValue As Object, ByRef rIndex As Integer, Optional aName As String = "") As TPROPERTIES
            Dim _rVal As TPROPERTIES = Clone(bReturnEmpty:=True)
            If String.IsNullOrWhiteSpace(aName) Then aName = ""
            If aName <> "" Then _rVal.Name = aName.Trim()
            rIndex = -1
            _rVal.SuppressIndices = True
            If Count <= 1 Then Return _rVal
            Dim i As Integer
            Dim bTestA As Boolean
            Dim strTestA As String = String.Empty
            Dim bTestB As Boolean
            Dim strTestB As String = String.Empty
            Dim bKeep As Boolean
            Dim aProp As TPROPERTY
            Dim bProp As TPROPERTY
            Dim cnt As Integer = Count
            If aMatchValue IsNot Nothing Then
                bTestA = True
                strTestA = TVALUES.To_STR(aMatchValue)
            End If
            If bMatchValue IsNot Nothing Then
                bTestB = True
                strTestB = TVALUES.To_STR(bMatchValue)
            End If
            For i = 1 To cnt - 1
                aProp = _Members(i - 1)
                bProp = _Members(i)
                If aProp.GroupCode = aGC And bProp.GroupCode = bGC Then
                    bKeep = True
                    If bTestA Then
                        If String.Compare(aProp.Value, strTestA, True) <> 0 Then
                            bKeep = False
                        End If
                    End If
                    If bTestB Then
                        If String.Compare(bProp.Value, strTestB, True) <> 0 Then
                            bKeep = False
                        End If
                    End If
                    If bKeep Then
                        _rVal.Add(aProp)
                        _rVal.Add(bProp)
                        rIndex = i
                        Exit For
                    End If
                    i += 1
                End If
                If i + 1 > cnt Then Exit For
            Next i
            Return _rVal
        End Function
        Public Function GetDifferences(bProps As TPROPERTIES, ByRef rDifCount As Integer, Optional aGCSkipList As String = "", Optional aNameSkipList As String = "", Optional bTestHandles As Boolean = False, Optional bSuppressHiddenProps As Boolean = False, Optional bUsePropCompare As Boolean = True, Optional aPrecis As Integer = 5) As TPROPERTIES
            Dim _rVal As TPROPERTIES = Clone(bReturnEmpty:=True)
            _rVal.SuppressIndices = True
            If bProps.Count <= 0 Then Return _rVal
            Dim aProp As TPROPERTY
            Dim bProp As TPROPERTY
            Dim cProp As TPROPERTY
            Dim bDoIt As Boolean
            Dim i As Integer
            Dim aCnt As Integer = Count
            Dim bEqual As Boolean
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            rDifCount = 0
            For i = 1 To aCnt
                aProp = Item(i)
                bProp = bProps.Item(aProp.Name)
                bDoIt = bProps.TryGet(aProp.Name, bProp, bSearchByNameOnly:=True)
                If bDoIt And bSuppressHiddenProps And aProp.Hidden Then bDoIt = False
                If Not bTestHandles Then
                    bDoIt = Not (aProp.PropType = dxxPropertyTypes.Handle Or aProp.PropType = dxxPropertyTypes.Pointer)
                End If
                If TLISTS.Contains(aProp.GroupCode, aGCSkipList, bReturnTrueForNullList:=True) Then bDoIt = False
                If bDoIt Then
                    If TLISTS.Contains(aProp.Name, aNameSkipList, bReturnTrueForNullList:=True) Then bDoIt = False
                End If
                If bDoIt Then
                    If bUsePropCompare Then
                        bEqual = aProp.Compare(bProp, aPrecis, True, bTestHandles, bTestHandles)
                    Else
                        bEqual = (aProp.Value = bProp.Value)
                    End If
                    If Not bEqual Then
                        rDifCount += 1
                        cProp = bProp.Clone
                        cProp.LastValue = aProp.Value
                        cProp.LineNo = aProp.LineNo
                        _rVal.Add(cProp)
                    End If
                End If
            Next i
            Return _rVal
        End Function
        Public Function GetAfter(aGroupCode As Integer, Optional aOccur As Integer = 0, Optional bReturnFirst As Boolean = False, Optional bRemove As Boolean = False) As TPROPERTIES
            Dim rIndex As Integer = 0
            Dim _rVal As TPROPERTIES = Clone(bReturnEmpty:=True)
            If Not Seek(aGroupCode, rIndex:=rIndex, aOccurance:=aOccur) Then Return _rVal
            _rVal = SubSet(rIndex, Count, Not bReturnFirst)
            If bRemove Then
                Dim aProps As TPROPERTIES = SubSet(1, rIndex, bSuppressLast:=bReturnFirst)
                Clear()
                Append(aProps)
            End If
            Return _rVal
        End Function
        Public Function CopyByName(aFromProps As TPROPERTIES, Optional bSetSuppressions As Boolean = False, Optional bCopyHidden As Boolean = False, Optional aSkipGCList As String = "", Optional aNamesList As List(Of String) = Nothing) As List(Of TPROPERTY)
            Dim rChanged As Boolean = False
            Return CopyByName(aFromProps, bSetSuppressions, bCopyHidden, aSkipGCList, rChanged, aNamesList)
        End Function
        Public Function CopyByName(aFromProps As dxoProperties, Optional bSetSuppressions As Boolean = False, Optional bCopyHidden As Boolean = False, Optional aSkipGCList As String = "", Optional aNamesList As List(Of String) = Nothing) As List(Of TPROPERTY)
            Dim rChanged As Boolean = False
            If aFromProps Is Nothing Then Return New List(Of TPROPERTY)

            Return CopyByName(New TPROPERTIES(aFromProps), bSetSuppressions, bCopyHidden, aSkipGCList, rChanged, aNamesList)
        End Function

        Public Function CopyByName(aFromProps As TPROPERTIES, bSetSuppressions As Boolean, bCopyHidden As Boolean, aSkipGCList As String, ByRef rChanged As Boolean, Optional aNamesList As List(Of String) = Nothing) As List(Of TPROPERTY)
            Dim _rVal As New List(Of TPROPERTY)
            If aFromProps.Count <= 0 Then Return _rVal
            Dim i As Integer
            Dim aProp As TPROPERTY
            Dim bProp As TPROPERTY = TPROPERTY.Null
            Dim bListPassed As Boolean
            Dim bDoIt As Boolean
            Dim cnt As Integer = Count
            Dim bcnt As Integer = aFromProps.Count

            bListPassed = Not String.IsNullOrWhiteSpace(aSkipGCList)
            If bListPassed Then aSkipGCList = aSkipGCList.Trim()
            rChanged = False
            If bSetSuppressions Then
                For i = 1 To cnt
                    bDoIt = True
                    aProp = Item(i)
                    If aNamesList IsNot Nothing Then
                        If aNamesList.FindIndex(Function(x) String.Compare(aProp.Name, x, True)) < 0 Then Continue For
                    End If
                    If Not aProp.Hidden Then
                        If bListPassed Then
                            If TLISTS.Contains(aProp.GroupCode, aSkipGCList) Then bDoIt = False
                        End If
                        If bDoIt Then
                            aProp.Suppressed = True
                            _Members(i - 1) = aProp
                        End If
                    End If
                Next i
            End If
            For i = 1 To cnt
                aProp = Item(i)
                If String.IsNullOrWhiteSpace(aProp.Name) Then Continue For

                If Not aFromProps.TryGet(aProp.Name, bProp, bSearchByNameOnly:=True) Then
                    Continue For
                End If

                If aNamesList IsNot Nothing Then
                    If aNamesList.FindIndex(Function(x) String.Compare(aProp.Name, x, True)) <= 0 Then
                        Continue For
                    End If
                End If
                If Not (bCopyHidden Or (Not bCopyHidden And Not aProp.Hidden)) Then Continue For

                If bListPassed Then
                    If TLISTS.Contains(aProp.GroupCode, aSkipGCList) Then
                        Continue For
                    End If

                End If


                aProp.Suppressed = False
                If aProp.SetVal(bProp.Value) Then
                    rChanged = True
                    _Members(i - 1) = aProp
                    _rVal.Add(New TPROPERTY(aProp))
                End If

            Next i
            Return _rVal
        End Function
        Public Function ExtractGroupCodeGroups(aGroupCode As Integer) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY("")
            If IsEmpty Then Return _rVal
            Dim leadProps As List(Of TPROPERTY) = GroupCodeMembers(aGroupCode)
            If leadProps.Count <= 0 Then Return _rVal
            Dim rProps As TPROPERTIES = Clone(bReturnEmpty:=True)
            Dim i As Integer
            Dim aProp As TPROPERTY
            'Dim bIn As Boolean
            Dim rGroup As TPROPERTIES = TPROPERTIES.Null
            Dim si As Integer
            Dim ei As Integer
            Dim k As Integer
            Dim props As TPROPERTIES
            Dim cnt As Integer = Count
            si = leadProps.Item(0).Index
            k = 2
            If leadProps.Count > 1 Then
                ei = leadProps.Item(k - 1).Index
            Else
                ei = cnt
            End If
            For i = 1 To cnt
                aProp = _Members(i - 1)
                If i < si Then
                    rProps.Add(aProp)
                ElseIf i = si Then
                    rGroup = New TPROPERTIES("Group " & k)
                    rGroup.Add(aProp)
                    'bIn = True
                ElseIf i = ei Then
                    rGroup.Add(aProp)
                    props = New TPROPERTIES(rGroup.Name, rGroup.NonDXF)
                    props.Append(rGroup)
                    _rVal.Add(props, "", True)
                    si = ei + 1
                    k += 1
                    If k > leadProps.Count Then
                        ei = cnt
                    Else
                        ei = leadProps.Item(k - 1).Index
                    End If
                Else
                    rGroup.Add(aProp)
                End If
            Next i
            Clear()
            Append(rProps)
            Return _rVal
        End Function
        Friend Function ExtractReactorGroups() As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY("")
            Dim reaProps As TPROPERTIES = GetByGroupCode(102)

            If IsEmpty Then Return _rVal
            Dim rProps As TPROPERTIES
            Dim i As Integer
            Dim aProp As TPROPERTY
            Dim bProp As TPROPERTY
            Dim j As Integer
            Dim rGroup As TPROPERTIES
            Dim si As Integer
            Dim props As TPROPERTIES

            If reaProps.Count <= 0 Then Return _rVal
            rProps = Clone(bReturnEmpty:=True)
            si = 1
            For i = 1 To reaProps.Count
                aProp = reaProps.Item(i)
                If i + 1 > reaProps.Count Then Exit For
                bProp = reaProps.Item(i + 1)
                rGroup = New TPROPERTIES(aProp.Value.ToString())
                For j = si To aProp.Index - 1
                    rProps.Add(_Members(j - 1))
                Next j
                si = bProp.Index + 1
                For j = aProp.Index + 1 To bProp.Index - 1
                    rGroup.Add(_Members(j - 1))
                Next j
                props = New TPROPERTIES(rGroup.Name, rGroup.NonDXF)
                props.Append(rGroup)
                _rVal.Add(props, aProp.Value.ToString(), True)
                i += 1
            Next i
            If si <= Count Then
                For j = si To Count
                    rProps.Add(_Members(j - 1))
                Next j
            End If
            Clear()
            Append(rProps)
            Return _rVal
        End Function

        Public Function ToList() As List(Of TPROPERTY)
            If Count <= 0 Then Return New List(Of TPROPERTY)
            Return _Members.ToList()
        End Function
#End Region 'Methods
#Region "Shared Methods"

        Friend Shared ReadOnly Property Null As TPROPERTIES
            Get
                Return New TPROPERTIES("")
            End Get
        End Property
        Friend Shared Function CopyDisplayProps(ByRef aProperties As TPROPERTIES, aSet As TDISPLAYVARS) As Boolean
            Dim _rVal As Boolean = False
            If aProperties.SetVal(dxfLinetypes.Invisible, aSet.Suppressed) Then _rVal = True
            If aSet.Color <> dxxColors.Undefined Then
                If aProperties.SetVal("Color", aSet.Color) Then _rVal = True
            End If
            If Not String.IsNullOrWhiteSpace(aSet.LayerName) Then
                If aProperties.SetVal("LayerName", aSet.LayerName) Then _rVal = True
            End If
            If Not String.IsNullOrWhiteSpace(aSet.Linetype) Then
                If aProperties.SetVal("LineType", aSet.Linetype) Then _rVal = True
            End If
            If aSet.LTScale > 0 Then
                If aProperties.SetVal("LT Scale", aSet.LTScale) Then _rVal = True
            End If
            If aSet.LineWeight <> dxxLineWeights.Undefined Then
                If aProperties.SetVal("LineWeight", aSet.LineWeight) Then _rVal = True
            End If
            Return _rVal
        End Function

        Friend Shared Function CopyDisplayProps(ByRef aProperties As TPROPERTIES, aSet As dxfDisplaySettings) As Boolean
            If aSet Is Nothing Then Return False
            Dim _rVal As Boolean = False
            If aProperties.SetVal(dxfLinetypes.Invisible, aSet.Suppressed) Then _rVal = True
            If aSet.Color <> dxxColors.Undefined Then
                If aProperties.SetVal("Color", aSet.Color) Then _rVal = True
            End If
            If Not String.IsNullOrWhiteSpace(aSet.LayerName) Then
                If aProperties.SetVal("LayerName", aSet.LayerName) Then _rVal = True
            End If
            If Not String.IsNullOrWhiteSpace(aSet.Linetype) Then
                If aProperties.SetVal("LineType", aSet.Linetype) Then _rVal = True
            End If
            If aSet.LTScale > 0 Then
                If aProperties.SetVal("LT Scale", aSet.LTScale) Then _rVal = True
            End If
            If aSet.LineWeight <> dxxLineWeights.Undefined Then
                If aProperties.SetVal("LineWeight", aSet.LineWeight) Then _rVal = True
            End If
            Return _rVal
        End Function
        Public Shared Function CopyDimStyleProps(aToProperties As TPROPERTIES, aFromProperties As TPROPERTIES, aHeaderPassed As Boolean, bHeaderPassed As Boolean, Optional aSkipList As String = Nothing) As TPROPERTIES
            Dim rProps As TPROPERTIES = aToProperties
            Dim enumVals As System.Collections.Generic.Dictionary(Of String, Integer) = dxfEnums.EnumValues(GetType(dxxDimStyleProperties))
            Dim nm As String
            Dim ival As dxxDimStyleProperties
            Dim bProp As TPROPERTY = TPROPERTY.Null
            Dim aProp As TPROPERTY = TPROPERTY.Null
            Dim skipVals As New TLIST(",", aSkipList)
            Dim aprefx As String = String.Empty
            Dim bprefx As String = String.Empty
            If bHeaderPassed Then bprefx = "$"
            If aHeaderPassed Then aprefx = "$"
            For Each nm In enumVals.Keys
                ival = enumVals.Item(nm)
                If Not skipVals.Contains(aprefx & nm) Then
                    If aFromProperties.TryGet(bprefx & nm, rMember:=bProp, bSearchByNameOnly:=True) Then
                        If rProps.TryGet(aprefx & nm, rMember:=aProp, bSearchByNameOnly:=True) Then
                            If aProp.SetVal(bProp.Value) Then rProps.IsDirty = True
                            rProps.SetItem(aProp.Index, aProp)
                        End If
                    End If
                End If
            Next
            Return rProps
        End Function
        Public Shared Function ReactorAdd(aProperties As TPROPERTIES, aKeyOrGC As String, aName As String, aHandle As String) As Boolean
            Dim aProperty As TPROPERTY = TPROPERTY.Null
            If Not aProperties.TryGet(aKeyOrGC, aProperty) Then Return False
            Dim _rVal As Boolean = TPROPERTY.ReactorAdd(aProperty, aName, aHandle)
            aProperties.UpdateProperty = aProperty
            Return _rVal
        End Function
        Public Shared Function ReactorRemove(aProperties As TPROPERTIES, aKeyOrGC As String, aString As String) As Boolean
            Dim aProperty As TPROPERTY = TPROPERTY.Null
            If Not aProperties.TryGet(aKeyOrGC, aProperty) Then Return False
            Dim _rVal As Boolean = TPROPERTY.ReactorRemove(aProperty, aString)
            aProperties.UpdateProperty = aProperty
            Return _rVal
        End Function
        Public Shared Function OneProp(aName As String, aValue As Object, Optional aLastVal As Object = Nothing, Optional aGroupCode As Integer = 0) As TPROPERTIES
            Dim _rVal As New TPROPERTIES("ONE PROP", True)
            _rVal.Add(aGroupCode, aValue, aName, Nothing, dxxPropertyTypes.Undefined, aLastVal:=aLastVal)
            Return _rVal
        End Function
        Public Shared Function FromString(aPropList As String, Optional aName As String = "", Optional aDelimiter As String = ",", Optional aHandleGC As Integer = 5, Optional bSuppressPointers As Boolean = False, Optional bNonDXF As Boolean = False) As TPROPERTIES
            Dim _rVal As New TPROPERTIES(aName, bNonDXF)
            If String.IsNullOrWhiteSpace(aPropList) Then Return _rVal
            aPropList = aPropList.Trim()
            Dim pVals() As String
            Dim pnamegc As String
            Dim pname As String = String.Empty
            Dim pval As String
            Dim aFlg As Boolean
            Dim aGC As Integer
            Dim bIsVec As Boolean
            Dim b2D As Boolean
            Dim pidx As Integer = 0
            Dim iCnt As Integer
            Dim sVals() As String = aPropList.Split(aDelimiter)

            For i As Integer = 0 To sVals.Length - 1
                Dim pStr As String = sVals(i)
                If pStr.Contains("=") Then
                    pVals = pStr.Split("=")
                    If pVals.Length > 1 Then
                        For j As Integer = 2 To pVals.Length - 1
                            pVals(1) = $"{pVals(1)}={ pVals(j)}"
                        Next j
                    End If
                Else
                    ReDim pVals(0 To 1)
                    pVals(0) = pStr
                End If
                pnamegc = pVals(0).Trim()
                If pnamegc.Length <= 0 Then Continue For
                pval = pVals(1).Trim()

                aGC = dxfUtils.ExtractTrailingIndex(pnamegc, rNoIndex:=aFlg, bAllowNegatives:=True, rLeadString:=pname)
                If aFlg Then Continue For

                pname = dxfUtils.StripParens(pname, "[", "]")
                bIsVec = False
                If pval.StartsWith("(") Then

                    Dim zset As Boolean
                    Dim v1 As TVECTOR = TVECTOR.DefineByString(dxfUtils.StripParens(pval), Nothing, ",", zset, iCnt)
                    If iCnt = 2 Or iCnt = 3 Then
                        bIsVec = True
                        b2D = iCnt = 2
                        _rVal.AddVector(aGC, v1, pname, b2D)
                        Continue For
                    End If
                End If

                If String.IsNullOrWhiteSpace(pname) Then
                    pidx += 1
                    pname = $"Property_{pidx}"
                End If

                Dim aProp As TPROPERTY = _rVal.Add(New TPROPERTY(aGC, pval, pname.Trim(), dxxPropertyTypes.Undefined))
                If aProp.GroupCode = aHandleGC Then aProp.PropType = dxxPropertyTypes.Handle
                If bSuppressPointers Then
                    If aProp.IsPointer Then aProp.PropType = dxxPropertyTypes.dxf_String
                End If
                _rVal.SetItem(_rVal.Count, aProp)


            Next i
            Return _rVal
        End Function
        Public Shared Function DecodeDIMZIN(aProperties As TPROPERTIES) As TPROPERTIES
            Dim _rVal As TPROPERTIES = aProperties
            'On Error Resume Next
            Dim aValue As Integer
            Dim aZSuppress As dxxZeroSuppression
            Dim aASuppress As dxxZeroSuppressionsArchitectural
            aValue = aProperties.ValueI(dxfEnums.PropertyName(dxxDimStyleProperties.DIMZIN))
            aZSuppress = dxxZeroSuppression.None
            aASuppress = dxxZeroSuppressionsArchitectural.ZeroFeetAndZeroInches
            If aValue <> 0 Then
                If aValue >= 12 Then
                    aZSuppress = dxxZeroSuppression.LeadingAndTrailing
                    Do While aValue >= 12
                        aValue -= 12
                    Loop
                End If
                If aValue >= 8 Then
                    aZSuppress = dxxZeroSuppression.Trailing
                    Do While aValue >= 8
                        aValue -= 8
                    Loop
                End If
                If aValue >= 4 Then
                    aZSuppress = dxxZeroSuppression.Leading
                    Do While aValue >= 4
                        aValue -= 4
                    Loop
                End If
                If aValue >= 3 Then
                    aASuppress = dxxZeroSuppressionsArchitectural.ZeroFeetAndIncludeZeroInches
                    Do While aValue >= 3
                        aValue -= 3
                    Loop
                End If
                If aValue >= 2 Then
                    aASuppress = dxxZeroSuppressionsArchitectural.IncludeZeroFeetAndSuppressZeroInches
                    Do While aValue >= 2
                        aValue -= 2
                    Loop
                End If
                If aValue >= 1 Then
                    aASuppress = dxxZeroSuppressionsArchitectural.IncludeZeroFeetAndZeroInches
                End If
            End If
            _rVal.SetVal(dxfEnums.PropertyName(dxxDimStyleProperties.DIMZEROSUPPRESSION), aZSuppress)
            _rVal.SetVal(dxfEnums.PropertyName(dxxDimStyleProperties.DIMZEROSUPPRESSION_ARCH), aASuppress)
            Return _rVal
        End Function
        Public Shared Function ArcLineStructure(aEntity As TENTITY, Optional arInfinite As Boolean? = Nothing) As TSEGMENT
            Dim _rVal As New TSEGMENT
            If aEntity.Props.Count < dxfGlobals.CommonProps Then Return _rVal
            Dim gType As dxxGraphicTypes = aEntity.GraphicType
            If gType <> dxxGraphicTypes.Arc And gType <> dxxGraphicTypes.Ellipse And gType <> dxxGraphicTypes.Line Then Return _rVal
            If arInfinite IsNot Nothing Then _rVal.INFINITE = arInfinite.Value
            _rVal.Flag = aEntity.TagFlagValue.Tag
            _rVal.Tag = aEntity.TagFlagValue.Flag
            _rVal.Value = aEntity.TagFlagValue.Value
            _rVal.DisplayStructure = TENTITY.DisplayVarsFromProperties(aEntity.Props)
            _rVal.Identifier = aEntity.Props.ValueStr("*Identifier")
            _rVal.OwnerGUID = aEntity.GUID
            Select Case gType
                Case dxxGraphicTypes.Arc
                    _rVal.IsArc = True
                    _rVal.ArcStructure.ClockWise = aEntity.Props.ValueB("*Clockwise")
                    _rVal.ArcStructure.Elliptical = False
                    _rVal.ArcStructure.Radius = aEntity.Props.ValueD("Radius", True, True, 1)
                    _rVal.ArcStructure.Plane = aEntity.DefPts.Plane
                    If arInfinite Then
                        _rVal.ArcStructure.StartAngle = 0
                        _rVal.ArcStructure.EndAngle = 360
                        '    .SpannedAngle = 360
                        '    .StartPt = .PLane.AngleVector( 0, .Radius, False)
                        '    .EndPt = .StartPt
                    Else
                        _rVal.ArcStructure.StartAngle = aEntity.Props.ValueD("Start Angle")
                        _rVal.ArcStructure.EndAngle = aEntity.Props.ValueD("End Angle")
                        _rVal.ArcStructure.StartWidth = aEntity.Props.ValueD("*StartWidth")
                        _rVal.ArcStructure.EndWidth = aEntity.Props.ValueD("*EndWidth")
                        If _rVal.ArcStructure.SpannedAngle >= 359.99 Then arInfinite = True
                    End If
                Case dxxGraphicTypes.Ellipse
                    _rVal.IsArc = True
                    _rVal.ArcStructure.Plane = aEntity.DefPts.Plane
                    _rVal.ArcStructure.ClockWise = aEntity.Props.ValueB("*Clockwise")
                    _rVal.ArcStructure.Elliptical = True
                    _rVal.ArcStructure.Radius = aEntity.Props.ValueD("*MajorRadius")
                    _rVal.ArcStructure.MinorRadius = aEntity.Props.ValueD("*MinorRadius")
                    If arInfinite Then
                        _rVal.ArcStructure.StartAngle = 0
                        _rVal.ArcStructure.EndAngle = 360
                        '    _rVal.ArcStructure.SpannedAngle = 360
                        '    _rVal.ArcStructure.StartPt = dxfUtils.EllipsePoint(_rVal.ArcStructure.Plane.Origin, _rVal.ArcStructure.Radius * 2, _rVal.ArcStructure.MinorRadius * 2, 0, .Plane)
                        '    _rVal.ArcStructure.EndPt = _rVal.ArcStructure.StartPt
                    Else
                        _rVal.ArcStructure.StartAngle = aEntity.Props.ValueD("*StartAngle")
                        _rVal.ArcStructure.EndAngle = aEntity.Props.ValueD("*EndAngle")
                        '_rVal.ArcStructure.SpannedAngle = dxfMath.SpannedAngle(_rVal.ArcStructure.ClockWise, _rVal.ArcStructure.StartAngle, _rVal.ArcStructure.EndAngle)
                        '.StartPt = dxfUtils.EllipsePoint(_rVal.ArcStructure.Plane.Origin, _rVal.ArcStructure.Radius * 2, _rVal.ArcStructure.MinorRadius * 2, _rVal.ArcStructure.StartAngle, _rVal.ArcStructure.Plane)
                        '.EndPt = dxfUtils.EllipsePoint(_rVal.ArcStructure.Plane.Origin, _rVal.ArcStructure.Radius * 2, _rVal.ArcStructure.MinorRadius * 2, _rVal.ArcStructure.EndAngle, _rVal.ArcStructure.Plane)
                        If _rVal.ArcStructure.SpannedAngle >= 359.99 Then
                            arInfinite = True
                        End If
                    End If
                Case dxxGraphicTypes.Line
                    _rVal.IsArc = False

                    _rVal.LineStructure.SPT = aEntity.DefPts.DefPt1
                    _rVal.LineStructure.EPT = aEntity.DefPts.DefPt2
                    _rVal.LineStructure.StartWidth = aEntity.Props.ValueD("*StartWidth")
                    _rVal.LineStructure.EndWidth = aEntity.Props.ValueD("*EndWidth")
            End Select
            If arInfinite.HasValue Then _rVal.INFINITE = arInfinite.Value
            Return _rVal
        End Function

        Public Shared Function ArcLineStructure(aEntity As dxfEntity, Optional arInfinite As Boolean? = Nothing) As TSEGMENT
            Dim _rVal As New TSEGMENT("")
            If aEntity Is Nothing Then Return _rVal
            If aEntity.Properties.Count < dxfGlobals.CommonProps Then Return _rVal
            Dim gType As dxxGraphicTypes = aEntity.GraphicType
            If gType <> dxxGraphicTypes.Arc And gType <> dxxGraphicTypes.Ellipse And gType <> dxxGraphicTypes.Line Then Return _rVal
            If arInfinite IsNot Nothing Then _rVal.INFINITE = arInfinite.Value
            _rVal.Flag = aEntity.TagFlagValue.Tag
            _rVal.Tag = aEntity.TagFlagValue.Flag
            _rVal.Value = aEntity.TagFlagValue.Value
            _rVal.DisplayStructure = aEntity.DisplayStructure
            _rVal.Identifier = aEntity.Identifier
            _rVal.OwnerGUID = aEntity.GUID
            Select Case gType
                Case dxxGraphicTypes.Arc
                    _rVal.IsArc = True
                    Dim arc As dxeArc = DirectCast(aEntity, dxeArc)
                    _rVal.ArcStructure.ClockWise = arc.ClockWise
                    _rVal.ArcStructure.Elliptical = False
                    _rVal.ArcStructure.Radius = arc.Radius
                    _rVal.ArcStructure.Plane = arc.PlaneV
                    If arInfinite Or arc.SpannedAngle >= 359.9999 Then
                        arInfinite = True
                        _rVal.ArcStructure.StartAngle = 0
                        _rVal.ArcStructure.EndAngle = 360
                        '    .SpannedAngle = 360
                        '    .StartPt = .PLane.AngleVector( 0, .Radius, False)
                        '    .EndPt = .StartPt
                    Else
                        _rVal.ArcStructure.StartAngle = arc.StartAngle
                        _rVal.ArcStructure.EndAngle = arc.EndAngle
                        _rVal.ArcStructure.StartWidth = arc.StartWidth
                        _rVal.ArcStructure.EndWidth = arc.EndWidth
                    End If
                Case dxxGraphicTypes.Ellipse
                    Dim ellipse As dxeEllipse = DirectCast(aEntity, dxeEllipse)
                    _rVal.IsArc = True
                    _rVal.ArcStructure.Plane = aEntity.DefPts.Plane
                    _rVal.ArcStructure.ClockWise = ellipse.ClockWise
                    _rVal.ArcStructure.Elliptical = True
                    _rVal.ArcStructure.Radius = ellipse.Radius
                    _rVal.ArcStructure.MinorRadius = ellipse.MinorRadius
                    If arInfinite Or ellipse.SpannedAngle >= 359.9999 Then
                        _rVal.ArcStructure.StartAngle = 0
                        _rVal.ArcStructure.EndAngle = 360
                        arInfinite = True
                        '    _rVal.ArcStructure.SpannedAngle = 360
                        '    _rVal.ArcStructure.StartPt = dxfUtils.EllipsePoint(_rVal.ArcStructure.Plane.Origin, _rVal.ArcStructure.Radius * 2, _rVal.ArcStructure.MinorRadius * 2, 0, .Plane)
                        '    _rVal.ArcStructure.EndPt = _rVal.ArcStructure.StartPt
                    Else
                        _rVal.ArcStructure.StartAngle = ellipse.StartAngle
                        _rVal.ArcStructure.EndAngle = ellipse.EndAngle
                        '_rVal.ArcStructure.SpannedAngle = dxfMath.SpannedAngle(_rVal.ArcStructure.ClockWise, _rVal.ArcStructure.StartAngle, _rVal.ArcStructure.EndAngle)
                        '.StartPt = dxfUtils.EllipsePoint(_rVal.ArcStructure.Plane.Origin, _rVal.ArcStructure.Radius * 2, _rVal.ArcStructure.MinorRadius * 2, _rVal.ArcStructure.StartAngle, _rVal.ArcStructure.Plane)
                        '.EndPt = dxfUtils.EllipsePoint(_rVal.ArcStructure.Plane.Origin, _rVal.ArcStructure.Radius * 2, _rVal.ArcStructure.MinorRadius * 2, _rVal.ArcStructure.EndAngle, _rVal.ArcStructure.Plane)
                        If _rVal.ArcStructure.SpannedAngle >= 359.99 Then
                            arInfinite = True
                        End If
                    End If
                Case dxxGraphicTypes.Line
                    _rVal.IsArc = False
                    Dim l1 As dxeLine = DirectCast(aEntity, dxeLine)
                    _rVal.LineStructure.SPT = New TVECTOR(aEntity.DefPts.Vector1)
                    _rVal.LineStructure.EPT = New TVECTOR(aEntity.DefPts.Vector2)
                    _rVal.LineStructure.StartWidth = l1.StartWidth
                    _rVal.LineStructure.EndWidth = l1.EndWidth
            End Select
            If arInfinite.HasValue Then _rVal.INFINITE = arInfinite.Value
            Return _rVal
        End Function
        Friend Function CopyDisplayProperties(aEntitySet As dxfDisplaySettings) As Boolean

            Dim rChangeString As String = String.Empty
            If aEntitySet Is Nothing Then Return False
            Return CopyDisplayProperties(aEntitySet, rChangeString)
        End Function
        Friend Function CopyDisplayProperties(aEntitySet As dxfDisplaySettings, ByRef rChangeString As String) As Boolean
            rChangeString = ""
            If aEntitySet Is Nothing Then Return False
            Return CopyDisplayProperties(aEntitySet.Strukture, rChangeString)
        End Function
        Friend Function CopyDisplayProperties(aEntitySet As TDISPLAYVARS) As Boolean

            Dim rChangeString As String = String.Empty
            Return CopyDisplayProperties(aEntitySet, rChangeString)
        End Function
        Friend Function CopyDisplayProperties(aEntitySet As TDISPLAYVARS, ByRef rChangeString As String) As Boolean

            Dim rChanged As Boolean = False
            rChangeString = ""

            Dim aSet As TDISPLAYVARS = aEntitySet
            Dim lval As Object = Nothing

            'If aSet.LayerName = "" Then aSet.LayerName = "0"
            'If aSet.Color = dxxColors.Undefined Then aSet.Color = dxxColors.ByLayer
            'If aSet.Linetype = "" Then aSet.Linetype = dxfLinetypes.ByLayer
            If SetVal(dxfLinetypes.Invisible, lval, aSet.Suppressed) Then
                rChanged = True

                TLISTS.Add(rChangeString, $"Suppressed from {lval} to  {aSet.Suppressed}")
            End If

            If aSet.Color <> dxxColors.Undefined Then
                If SetVal("Color", aSet.Color, lval) Then
                    rChanged = True
                    TLISTS.Add(rChangeString, $"Color from {lval} to {aSet.Color}")
                End If
            End If
            If Not String.IsNullOrWhiteSpace(aSet.LayerName) Then
                If SetVal("LayerName", aSet.LayerName, lval) Then
                    rChanged = True
                    TLISTS.Add(rChangeString, $"LayerName from {lval} to  {aSet.LayerName}")
                End If
            End If

            If Not String.IsNullOrWhiteSpace(aSet.Linetype) Then
                If SetVal("LineType", aSet.Linetype, lval) Then
                    rChanged = True
                    TLISTS.Add(rChangeString, $"LineType from {lval} to  {aSet.Linetype}")
                End If
            End If

            If aSet.LTScale > 0 Then
                If SetVal("LT Scale", aSet.LTScale, lval) Then
                    rChanged = True
                    TLISTS.Add(rChangeString, $"LTScale from {lval} to  { aSet.LTScale}")
                End If
            End If
            If aSet.LineWeight <> dxxLineWeights.Undefined Then
                If SetVal("LineWeight", aSet.LineWeight, lval) Then
                    rChanged = True
                    TLISTS.Add(rChangeString, $"LineWeight from {lval} to  {aSet.LineWeight}")
                End If
            End If
            Return rChanged
        End Function
#End Region 'Shared Methods
    End Structure 'TPROPERTIES
    Friend Structure TPROPERTYARRAY
        Implements ICloneable
#Region "Members"
        Public Instance As Integer
        Public Name As String
        Public Owner As String
        Private _Init As Boolean
        Private _Members() As TPROPERTIES
#End Region 'Members
#Region "Constructors"
        Public Sub New(Optional aName As String = "", Optional aInstance As Integer = 0, Optional aOwner As String = "")
            'init ----------------------
            Name = aName.Trim()
            ReDim _Members(-1)
            _Init = True
            Owner = aOwner
            Instance = aInstance
            'init ----------------------
        End Sub


        Public Sub New(aArray As TPROPERTYARRAY)
            'init ----------------------
            Name = aArray.Name
            ReDim _Members(-1)
            _Init = True
            Owner = aArray.Owner
            Instance = aArray.Instance
            'init ----------------------
            _Members = Force.DeepCloner.DeepClonerExtensions.DeepClone(Of TPROPERTIES())(aArray._Members)
        End Sub

        Public Sub New(aArray As IEnumerable(Of dxoProperties))
            'init ----------------------
            Name = String.Empty
            ReDim _Members(-1)
            _Init = True
            Owner = String.Empty
            Instance = 0
            'init ----------------------
            If aArray Is Nothing Then Return
            If TypeOf aArray Is dxoPropertyArray Then
                Dim dxa As dxoPropertyArray = DirectCast(aArray, dxoPropertyArray)
                Name = dxa.Name
                Instance = dxa.Instance
                Owner = dxa.Owner
            End If

            For Each item As dxoProperties In aArray
                Add(New TPROPERTIES(item))
            Next

        End Sub

        Public Sub New(aMember As dxoProperties)
            'init ----------------------
            Name = String.Empty
            ReDim _Members(-1)
            _Init = True
            Owner = String.Empty
            Instance = 0
            'init ----------------------
            If aMember Is Nothing Then Return
            Add(New TPROPERTIES(aMember))
        End Sub

#End Region 'Constructors
#Region "Properties"
        Public ReadOnly Property Count As Integer
            Get
                If _Members Is Nothing And _Init Then
                    _Init = False
                End If
                If Not _Init Then
                    _Init = True
                    ReDim _Members(-1)
                End If
                Return _Members.Count
            End Get
        End Property
#End Region 'Properties
#Region "Methods"
        Public Function ToList() As List(Of TPROPERTIES)
            If Count <= 0 Then Return New List(Of TPROPERTIES)
            Return _Members.ToList()
        End Function
        Public Function Clone() As TPROPERTYARRAY
            Return New TPROPERTYARRAY(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TPROPERTYARRAY(Me)
        End Function
        Public Function Item(aIndex As Integer, Optional bSuppressIndexError As Boolean = False) As TPROPERTIES
            'BAse 1
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE And Not bSuppressIndexError Then Throw New IndexOutOfRangeException()
                Return New TPROPERTIES("")
            End If
            If Not String.IsNullOrWhiteSpace(Owner) Then _Members(aIndex - 1).Owner = Owner
            Return _Members(aIndex - 1)
        End Function
        Public Sub SetItem(aIndex As Integer, value As TPROPERTIES)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1) = value
        End Sub
        Public Sub Clear()

            _Init = True
            ReDim _Members(-1)

        End Sub
        Public Function ClearMember(aArrayName As String, Optional bAddIfNotFound As Boolean = False) As Boolean
            Dim _rVal As Boolean
            Dim idx As Integer
            Dim aProps As TPROPERTIES = GetProps(aArrayName, idx, bAddIfNotFound)
            If idx >= 0 Then
                _rVal = aProps.Count <> 0
                aProps.Clear()
                SetItem(idx, aProps)
            End If
            Return _rVal
        End Function

        Public Sub Add(aMembers As List(Of TPROPERTY), Optional aName As String = "", Optional bSuppressSearch As Boolean = False, Optional bDontAddEmpty As Boolean = False, Optional bClearExisting As Boolean = False)
            Dim rIndex As Integer = 0
            Add(New TPROPERTIES("", aMembers), aName, bSuppressSearch, bDontAddEmpty, bClearExisting, rIndex)
        End Sub

        Public Sub Add(aMember As TPROPERTIES, Optional aName As String = "", Optional bSuppressSearch As Boolean = False, Optional bDontAddEmpty As Boolean = False, Optional bClearExisting As Boolean = False)
            Dim rIndex As Integer = 0
            Add(aMember, aName, bSuppressSearch, bDontAddEmpty, bClearExisting, rIndex)
        End Sub
        Public Sub Add(aMember As TPROPERTIES, aName As String, bSuppressSearch As Boolean, bDontAddEmpty As Boolean, bClearExisting As Boolean, ByRef rIndex As Integer)

            If Not String.IsNullOrEmpty(aName) Then aName = aName.Trim Else aName = ""
            If aName = "" Then aName = $"SUBPROPS_{ Count + 1}"
            If bSuppressSearch Then
                rIndex = 0
            Else
                GetProps(aName, rIndex)
            End If
            If rIndex = 0 Then
                If Count >= Integer.MaxValue Then Return
                If bDontAddEmpty And aMember.Count <= 0 Then Return
                Array.Resize(_Members, Count + 1)
                _Members(_Members.Count - 1) = aMember
                _Members(_Members.Count - 1).Name = aName
                If Not String.IsNullOrWhiteSpace(Owner) And Count > 0 Then _Members(_Members.Count - 1).Owner = Owner
            Else
                If bClearExisting Then
                    _Members(rIndex - 1).Clear()
                End If
                If bDontAddEmpty And aMember.Count <= 0 Then Return
                _Members(rIndex - 1).Append(aMember)
                If Owner <> "" And Count > 0 Then _Members(Count - 1).Owner = Owner
            End If
        End Sub
        Public Function GetProps(aName As String, Optional bAddIfNotFound As Boolean = False) As TPROPERTIES
            Dim rIndex As Integer = 0
            Return GetProps(aName, rIndex, bAddIfNotFound)
        End Function
        Public Function GetProps(aName As String, ByRef rIndex As Integer, Optional bAddIfNotFound As Boolean = False) As TPROPERTIES
            Dim _rVal As New TPROPERTIES(aName)
            rIndex = 0
            Dim aProps As TPROPERTIES
            For i As Integer = 1 To Count
                aProps = Item(i)
                If String.Compare(aProps.Name, aName, True) = 0 Then
                    _rVal = aProps
                    rIndex = i
                    Exit For
                End If
            Next i
            If rIndex = 0 And bAddIfNotFound And aName <> "" Then
                Add(_rVal, aName)
                rIndex = Count
            End If
            Return _rVal
        End Function
        Public Sub Append(bPropArray As TPROPERTYARRAY, Optional bSuppressSearch As Boolean = False, Optional bDontAddEmpty As Boolean = False)

            If bPropArray.Count <= 0 Then Return
            For i As Integer = 1 To bPropArray.Count
                Add(bPropArray.Item(i), bPropArray.Item(i).Name, bSuppressSearch, bDontAddEmpty)
            Next i
        End Sub
        Public Sub AddReactor(aArrayName As String, aGroupCode As Integer, aValue As String, Optional bPointer As Boolean = True, Optional bDontAddArray As Boolean = False, Optional aPropName As String = "")
            If String.IsNullOrWhiteSpace(aArrayName) Or String.IsNullOrWhiteSpace(aValue) Then Return
            aArrayName = aArrayName.Trim()

            Dim idx As Integer
            Dim pidx As Integer
            If String.IsNullOrWhiteSpace(aPropName) Then aPropName = "Reactor"
            aPropName = aPropName.Trim()
            Dim aProps As TPROPERTIES = GetProps(aArrayName, idx)
            If idx < 0 Then
                If bDontAddArray Then
                    Return
                Else
                    aProps = New TPROPERTIES(aArrayName)
                    Add(aProps, aArrayName, True)
                    idx = Count
                End If
            End If
            aProps.GetByStringValue(aValue, 1, rIndex:=pidx, aGC:=aGroupCode)
            If pidx <= 0 Then
                If bPointer Then
                    aProps.Add(New TPROPERTY(aGroupCode, aValue, aPropName, dxxPropertyTypes.Pointer))
                Else
                    aProps.Add(New TPROPERTY(aGroupCode, aValue, aPropName, dxxPropertyTypes.Undefined))
                End If
            End If
            SetItem(idx, aProps)
        End Sub
        Public Sub AddMemberPair(aArrayName As String, aGroupCode As Integer, aValue As Object, bGroupCode As Integer, bValue As Object, Optional aName As String = "", Optional bName As String = "", Optional bDontAddArray As Boolean = False)
            If String.IsNullOrWhiteSpace(aArrayName) Then Return
            aArrayName = aArrayName.Trim()
            Dim idx As Integer
            Dim aProps As TPROPERTIES = GetProps(aArrayName, idx)
            Dim pidx As Integer
            Dim aIsPointer As Boolean
            Dim bIsPointer As Boolean
            Dim aVal As Object = Nothing
            Dim bVal As Object = Nothing
            If String.IsNullOrWhiteSpace(aName) Then aName = "Pointer"
            If String.IsNullOrWhiteSpace(bName) Then bName = "Pointer"

            aName = aName.Trim()
            bName = bName.Trim()
            If idx < 0 Then
                If bDontAddArray Then
                    Return
                Else
                    aProps = New TPROPERTIES(aArrayName)
                    Add(aProps, aArrayName, True)
                    idx = Count - 1
                End If
            End If
            If (aGroupCode >= 320 And aGroupCode <= 369) Or (aGroupCode >= 390 And aGroupCode <= 399) Or bGroupCode = 1005 Then
                aIsPointer = True 'strings (hex handles)
                aVal = aValue.ToString()

            End If
            If (bGroupCode >= 320 And bGroupCode <= 369) Or (bGroupCode >= 390 And bGroupCode <= 399) Or bGroupCode = 1005 Then
                bIsPointer = True 'strings (hex handles)
                bVal = bValue.ToString()

            End If
            aProps.GetGroupCodePair(aGroupCode, bGroupCode, aVal, bVal, pidx)
            If pidx <= 0 Then
                If aIsPointer Then
                    aProps.Add(New TPROPERTY(aGroupCode, aValue, aName, dxxPropertyTypes.Pointer))
                Else
                    aProps.Add(New TPROPERTY(aGroupCode, aValue, aName, dxxPropertyTypes.Undefined))
                End If
                If bIsPointer Then
                    aProps.Add(New TPROPERTY(bGroupCode, bValue, bName, dxxPropertyTypes.Pointer))
                Else
                    aProps.Add(New TPROPERTY(bGroupCode, bValue, bName, dxxPropertyTypes.Undefined))
                End If
            Else
                Dim aProp As TPROPERTY
                Dim bProp As TPROPERTY
                aProp = aProps.Item(pidx)
                bProp = aProps.Item(pidx + 1)
                If aIsPointer Then aProp.PropType = dxxPropertyTypes.Pointer
                aProp.Value = aValue
                If bIsPointer Then bProp.PropType = dxxPropertyTypes.Pointer
                bProp.Value = bValue
                aProps.SetItem(pidx, aProp)
                aProps.SetItem(pidx + 1, bProp)
            End If
            SetItem(idx, aProps)
        End Sub
#End Region 'Methods
    End Structure 'TPROPERTYARRAY
    Friend Structure TPROPERTYMATRIX
        Implements ICloneable
#Region "Members"
        Public Block As TBLOCK
        Public ExtendedData As TPROPERTYARRAY
        Public GroupName As String
        Public Name As String
        Private _Init As Boolean
        Private _Members() As TPROPERTYARRAY
#End Region 'Members
#Region "Constructors"
        Public Sub New(Optional aName As String = "")
            'init --------------------------------------------------------
            Block = New TBLOCK("")
            ExtendedData = New TPROPERTYARRAY("Extended Data")
            GroupName = ""
            Name = IIf(Not String.IsNullOrWhiteSpace(aName), aName.Trim(), "")
            _Init = True
            ReDim _Members(-1)
            'init --------------------------------------------------------

        End Sub

        Public Sub New(aMatrix As TPROPERTYMATRIX, Optional bDontCloneMembers As Boolean = False)
            'init --------------------------------------------------------
            Block = New TBLOCK(aMatrix.Block)
            ExtendedData = New TPROPERTYARRAY(aMatrix.ExtendedData)
            GroupName = aMatrix.GroupName
            Name = aMatrix.Name
            _Init = True
            ReDim _Members(-1)
            'init --------------------------------------------------------
            If aMatrix.Count > 0 And Not bDontCloneMembers Then _Members = aMatrix._Members.Clone()
        End Sub

#End Region 'Constructors
#Region "Properties"
        Public ReadOnly Property Count As Integer
            Get
                If Not _Init Then
                    _Init = True
                    ReDim _Members(-1)
                End If
                Return _Members.Count
            End Get
        End Property
#End Region 'Properties
#Region "Methods"

        Public Function Clone(Optional bReturnEmpty As Boolean = False) As TPROPERTYMATRIX
            Return New TPROPERTYMATRIX(Me, bReturnEmpty)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TPROPERTYMATRIX(Me)
        End Function
        Public Function Item(aIndex As Integer) As TPROPERTYARRAY
            'base 1
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return New TPROPERTYARRAY("")
            End If
            Return _Members(aIndex - 1)
        End Function
        Public Sub SetItem(aIndex As Integer, value As TPROPERTYARRAY)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1) = value
        End Sub
        Public Sub Add(aArray As TPROPERTYARRAY, Optional aName As String = Nothing)
            If Count >= Integer.MaxValue Then Return
            Array.Resize(_Members, Count + 1)
            _Members(_Members.Count - 1) = aArray
            If aName IsNot Nothing Then _Members(_Members.Count - 1).Name = aName
        End Sub
        Public Sub Add(aProperties As TPROPERTIES, Optional aArrayName As String = "", Optional aPropsName As String = "")
            If Count >= Integer.MaxValue Then Return
            Dim aPropArray As New TPROPERTYARRAY(aArrayName)
            aPropArray.Add(aProperties, aPropsName)
            Add(aPropArray, aArrayName)
        End Sub
        Public Sub Append(bMatrix As TPROPERTYMATRIX, Optional bFlatten As Boolean = False)
            Dim aPropArray As TPROPERTYARRAY
            For i As Integer = 1 To bMatrix.Count
                aPropArray = bMatrix.Item(i)
                If aPropArray.Count > 0 Then
                    If bFlatten Then
                        For j As Integer = 1 To aPropArray.Count
                            Add(aPropArray.Item(j))
                        Next j
                    Else
                        Add(aPropArray)
                    End If
                End If
            Next i
        End Sub
#End Region 'Methods
    End Structure 'TPROPERTYMATRIX
    Friend Structure TPROPERTYMATRIXARRAY
        Implements ICloneable
#Region "Members"
        Private _Name As String
        Private _Init As Boolean
        Private _Members() As TPROPERTYMATRIX
#End Region 'Members
#Region "Constructors"
        Public Sub New(Optional aName As String = "")
            'init ------------------------------------------------
            _Name = IIf(Not String.IsNullOrEmpty(aName), aName.Trim(), "")
            _Init = True
            ReDim _Members(-1)
            'init ------------------------------------------------
        End Sub
        Public Sub New(aArray As TPROPERTYMATRIXARRAY, Optional bDontCloneMembers As Boolean = False)
            'init ------------------------------------------------
            _Name = aArray.Name
            _Init = True
            ReDim _Members(-1)
            'init ------------------------------------------------
            If Not bDontCloneMembers And aArray.Count > 0 Then _Members = aArray._Members.Clone()
        End Sub
#End Region 'Constructors
#Region "Properties"
        Public ReadOnly Property Count As Integer
            Get
                If Not _Init Then
                    _Init = True
                    Name = ""
                    ReDim _Members(-1)
                End If
                Return _Members.Count
            End Get
        End Property
        Public Property Name As String
            Get
                Return _Name
            End Get
            Set(value As String)
                _Name = value
            End Set
        End Property
#End Region 'Properties
#Region "Methods"
        Public Function ToList() As List(Of TPROPERTYMATRIX)
            If Count <= 0 Then Return New List(Of TPROPERTYMATRIX)
            Return _Members.ToList()
        End Function
        Public Sub Add(aArray As TPROPERTYMATRIX, Optional aName As String = "")
            If Count >= Integer.MaxValue Then Return
            Array.Resize(_Members, Count + 1)
            _Members(_Members.Count - 1) = aArray
            If aName <> "" Then _Members(_Members.Count - 1).Name = aName
        End Sub
        Public Function Item(aIndex As Integer) As TPROPERTYMATRIX
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return New TPROPERTYMATRIX("")
            End If
            Return _Members(aIndex - 1)
        End Function
        Public Sub SetItem(aIndex As Integer, value As TPROPERTYMATRIX)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1) = value
        End Sub
        Public Function Clone(Optional bReturnEmpty As Boolean = False) As TPROPERTYMATRIXARRAY
            Return New TPROPERTYMATRIXARRAY(Me, bReturnEmpty)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TPROPERTYMATRIXARRAY(Me)
        End Function
#End Region 'Methods
    End Structure 'TPROPERTYMATRIXARRAY
#End Region 'Structure

End Namespace
