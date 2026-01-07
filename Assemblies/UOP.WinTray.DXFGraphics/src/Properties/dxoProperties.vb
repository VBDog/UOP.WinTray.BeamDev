Imports System.Reflection
Imports System.Windows.Controls
Imports SharpDX.Direct2D1
Imports SharpDX.Direct2D1.Effects
Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities
Imports Vanara
Imports Vanara.PInvoke.Kernel32



Namespace UOP.DXFGraphics
    Public Class dxoProperties
        Inherits List(Of dxoProperty)
        Implements ICloneable
        Implements IEnumerable(Of dxoProperty)

        Public Delegate Sub PropertyChangedDelegate(aProperty As dxoProperty)
        Public Event PropertyChanged As PropertyChangedDelegate


#Region "Members"

#End Region 'Members
#Region "Constructors"
        Public Sub New()
            MyBase.New()
            FileObjectType = dxxFileObjectTypes.Undefined
        End Sub
        Public Sub New(aName As String, Optional bNonDXF As Boolean = False)
            MyBase.New()
            FileObjectType = dxxFileObjectTypes.Undefined
            Name = aName
            NonDXF = bNonDXF
        End Sub
        Public Sub New(aName As String, aProperties As List(Of dxoProperty))
            MyBase.New()
            FileObjectType = dxxFileObjectTypes.Undefined
            Name = aName
            If aProperties IsNot Nothing Then AddRange(aProperties)
        End Sub
        Friend Sub New(aProps As TPROPERTIES, Optional aName As String = Nothing)

            MyBase.New()
            FileObjectType = dxxFileObjectTypes.Undefined
            For i As Integer = 1 To aProps.Count
                Add(New dxoProperty(aProps.Item(i)))
            Next
            Name = aProps.Name
            NonDXF = aProps.NonDXF
            If Not String.IsNullOrWhiteSpace(aName) Then Name = aName.Trim()
        End Sub

        Public Sub New(aProperties As IEnumerable(Of dxoProperty))
            MyBase.Clear()
            FileObjectType = dxxFileObjectTypes.Undefined
            If aProperties Is Nothing Then Return
            If TypeOf aProperties Is dxoProperties Then
                Dim dxp As dxoProperties = DirectCast(aProperties, dxoProperties)
                Name = dxp.Name
                IsDirty = dxp.IsDirty
                NonDXF = dxp.NonDXF
                FileObjectType = dxp.FileObjectType
            End If
            For Each prop As dxoProperty In aProperties
                Add(New dxoProperty(prop))
            Next
        End Sub

#End Region 'Constructors
#Region "Properties"

        Public Property Name As String = String.Empty
        Friend Property IsDirty As Boolean = False
        Friend NonDXF As Boolean
        Friend Owner As String
        Friend Delimiter As Char
        Friend FileObjectType As dxxFileObjectTypes
        Public Property Handle As String
            Get

                Dim aMem As dxoProperty = Nothing
                If TryGet("Handle", aMem) Then
                    Return aMem.ValueS
                End If
                Dim hndls As List(Of dxoProperty) = FindAll(Function(x) x.PropertyType = dxxPropertyTypes.Handle)
                If hndls.Count > 0 Then
                    Return hndls(0).ValueS
                End If

                Add(New dxoProperty(5, "", "Handle", dxxPropertyTypes.Handle, ""))
                Return String.Empty
            End Get

            Friend Set(value As String)
                Dim aMem As dxoProperty = Nothing
                If TryGet("Handle", aMem) Then
                    SetVal(aMem, value)
                    Return
                End If
                Dim hndls As List(Of dxoProperty) = FindAll(Function(x) x.PropertyType = dxxPropertyTypes.Handle)
                If hndls.Count > 0 Then
                    aMem = hndls(0)
                    SetVal(aMem, value)
                    Return
                End If
                Add(New dxoProperty(5, value, "Handle", dxxPropertyTypes.Handle, ""))

            End Set
        End Property

        Public Property GUID As String
            Get

                Dim aMem As dxoProperty = Nothing
                If TryGet("*GUID", aMem) Then
                    Return aMem.ValueS
                ElseIf TryGet("GUID", aMem) Then
                    Return aMem.ValueS
                Else
                    Return String.Empty
                End If
            End Get

            Friend Set(value As String)
                Dim aMem As dxoProperty = Nothing
                If TryGet("*GUID", aMem) Then
                    SetVal(aMem, value)
                ElseIf TryGet("GUID", aMem) Then
                    SetVal(aMem, value)
                End If
            End Set
        End Property
#End Region 'Properties
#Region "Methods"
        Public Function Scale(aMultiplier As Double, Optional aSkipList As List(Of String) = Nothing, Optional bSuppressEvents As Boolean = False) As Boolean
            Dim _rVal As Boolean = False
            If aMultiplier = 1 Then Return False
            For Each mem In Me
                If Not mem.Scaleable Then Continue For
                If aSkipList IsNot Nothing Then
                    If mem.IsNamed(aSkipList) Then Continue For
                End If
                Dim dval As Double = mem.ValueD * aMultiplier
                If SetVal(mem, dval, bSuppressEvents:=bSuppressEvents) Then _rVal = True
            Next


            Return _rVal
        End Function

        Public Function Multiply(aGC As Integer, aFactor As Double, Optional aOccur As Integer = 1, Optional bSuppressEvents As Boolean = False) As Boolean

            Dim _rVal As String = String.Empty
            If aFactor = 1 Then Return False
            Dim aMem As dxoProperty = Nothing
            If Not TryGet(aGroupCode:=aGC, aMem, aOccur) Then Return False
            Dim dval As Double = aMem.ValueD * aFactor
            Return SetVal(aMem, dval, bSuppressEvents)

        End Function

        Public Function AddToValue(aGC As Integer, aAdder As Double, Optional aOccur As Integer = 1, Optional bSuppressEvents As Boolean = False) As Boolean

            Dim _rVal As String = String.Empty
            If aAdder = 0 Then Return False
            Dim aMem As dxoProperty = Nothing
            If Not TryGet(aGroupCode:=aGC, aMem, aOccur) Then Return False
            Dim dval As Double = aMem.ValueD + aAdder
            Return SetVal(aMem, dval, bSuppressEvents)

        End Function


        Public Function Multiply(aNameOrKeyList As String, aDelimitor As String, aFactor As Double) As String
            Dim _rVal As String = String.Empty
            If aFactor = 1 Then Return String.Empty

            Dim sVals As TVALUES = TLISTS.ToValues(aNameOrKeyList, aDelimitor, False, True, True)
            If sVals.Count <= 0 Then Return String.Empty
            Dim pname As String
            Dim dval As Double
            Dim aMem As dxoProperty = Nothing
            For i As Integer = 1 To sVals.Count
                pname = sVals.Item(i).ToString().Trim()
                If TryGet(pname, aMem) Then
                    dval = TVALUES.To_DBL(aMem.Value) * aFactor
                    If aMem.SetVal(dval) Then
                        TLISTS.Add(_rVal, aMem.Name)

                    End If
                End If
            Next
            Return _rVal
        End Function

        Public Shadows Sub Add(aProperty As dxoProperty)
            If aProperty Is Nothing Then Return
            Add(aProperty, Nothing, Nothing)
        End Sub
        Friend Shadows Sub Add(aProperty As TPROPERTY, Optional bHidden As Boolean? = Nothing, Optional bIsHeader As Boolean? = Nothing)
            Add(New dxoProperty(aProperty), bHidden, bIsHeader)
        End Sub

        Friend Shadows Sub Add(aProperty As dxoProperty, Optional bHidden As Boolean? = False, Optional bIsHeader As Boolean? = Nothing)

            If aProperty Is Nothing Then Return


            Dim idx As Integer
            Dim pname As String = aProperty.Name.Trim()
            Dim sufx As String = String.Empty
            If pname = "" Then pname = $"Property_{Count + 1}"
            aProperty.Name = pname
            'If Count > 0 Then
            '    Do While Me.FindIndex(Function(x) String.Compare(x.Key, $"{pname }{ sufx}", True) = 0) >= 0
            '        idx += 1
            '        sufx = $"_{idx}"
            '    Loop
            'End If
            If idx > 0 Then
                pname += sufx
                aProperty.Key = pname
            End If
            If pname.StartsWith("*") Or bHidden Then aProperty.Hidden = True
            aProperty.Index = Count + 1


            If bIsHeader.HasValue Then aProperty.IsHeader = bIsHeader.Value

            MyBase.Add(aProperty)
            '_GroupCodes.Add(aProperty)


        End Sub
        Friend Shadows Sub Add(aProperty As TPROPERTY)

            MyBase.Add(New dxoProperty(aProperty))
        End Sub

        Public Function AddTo(aGroupCode As Integer, aValue As Object, aName As String, aPropertyType As dxxPropertyTypes, bScalable As Boolean) As dxoProperty
            Return AddTo(aGroupCode, aValue, aName, Nothing, aPropertyType, Nothing, bScalable, False, Nothing)
        End Function

        Public Function AddTo(aGroupCode As Integer, aValue As Object, aName As String, Optional aPropertyType As dxxPropertyTypes = dxxPropertyTypes.Undefined) As dxoProperty
            Return AddTo(aGroupCode, aValue, aName, Nothing, aPropertyType, False, False, False, Nothing)
        End Function

        Public Function AddTo(aGroupCode As Integer, aValue As Object, aName As String, aSuppressedValue As Object,
                                  aPropertyType As dxxPropertyTypes, Optional bSuppressed As Boolean = False,
                                Optional bScalable As Boolean = False, Optional bSetToSuppressed As Boolean = False, Optional aLastVal As Object = Nothing) As dxoProperty

            Dim rHidden As Boolean = False
            Return AddTo(aGroupCode, aValue, aName, aSuppressedValue, aPropertyType, bSuppressed, bScalable, bSetToSuppressed, aLastVal, rHidden)
        End Function
        Friend Function AddPoint(aVector As TVECTOR, aName As String, aGroupCode As Integer,
                                    Optional bIsHeader As Boolean = False, Optional bSuppressed As Boolean = False,
                                     Optional bScaleable As Boolean = False, Optional bHidden As Boolean = False) As dxoProperty

            Dim _rVal As dxoProperty
            If String.IsNullOrWhiteSpace(aName) Then aName = "" Else aName = aName.Trim()
            If aName = "" Then aName = $"Prop_{Count + 1}"

            If bIsHeader Then
                If Not aName.StartsWith("$") Then aName = $"${aName}"
                _rVal = New dxoProperty(9, aName, aName, dxxPropertyTypes.dxf_String, Nothing, bScalable:=bScaleable) With {.Suppressed = bSuppressed}
                Add(_rVal, bHidden, bIsHeader)
                Dim p1 As New dxoProperty(aGroupCode, aVector.X, $"{aName}_X", dxxPropertyTypes.dxf_Double, bScalable:=bScaleable, bSuppressed:=bSuppressed, bIsOrdinate:=True)
                Dim p2 As New dxoProperty(aGroupCode + 10, aVector.Y, $"{aName}_Y", dxxPropertyTypes.dxf_Double, bScalable:=bScaleable, bSuppressed:=bSuppressed, bIsOrdinate:=True)

                Add(p1, bHidden)
                Add(p2, bHidden)

            Else
                _rVal = New dxoProperty(aGroupCode, aVector.X, $"{aName}_X", dxxPropertyTypes.dxf_Double, bScalable:=bScaleable, bSuppressed:=bSuppressed, bIsOrdinate:=True)
                Dim p1 As New dxoProperty(aGroupCode + 10, aVector.Y, $"{aName}_Y", dxxPropertyTypes.dxf_Double, bScalable:=bScaleable, bSuppressed:=bSuppressed, bIsOrdinate:=True)
                Add(_rVal, bHidden)
                Add(p1, bHidden)
            End If

            Return _rVal
        End Function

        Friend Function AddPoint(aVector As iVector, aName As String, aGroupCode As Integer,
                                    Optional bIsHeader As Boolean = False, Optional bSuppressed As Boolean = False,
                                     Optional bScaleable As Boolean = False, Optional bHidden As Boolean = False) As dxoProperty

            Return AddPoint(New TVECTOR(aVector), aName, aGroupCode, bIsHeader, bSuppressed, bScaleable, bHidden)
        End Function

        Public Function AddVector(aGroupCode As Integer, aVector As iVector, Optional aName As String = "", Optional bTwoD As Boolean = False, Optional bSuppressed As Boolean = False, Optional bIsDirection As Boolean = False, Optional bScaleable As Boolean = False, Optional bHidden As Boolean = False, Optional bIsHeader As Boolean = False) As dxoProperty
            Return AddVector(aGroupCode, New TVECTOR(aVector), aName, bTwoD, bSuppressed, bIsDirection, bScaleable, bHidden, bIsHeader)
        End Function


        Friend Function AddVector(aGroupCode As Integer, aVector As TVECTOR, Optional aName As String = "", Optional bTwoD As Boolean = False, Optional bSuppressed As Boolean = False, Optional bIsDirection As Boolean = False, Optional bScaleable As Boolean = False, Optional bHidden As Boolean = False, Optional bIsHeader As Boolean = False) As dxoProperty

            If bIsDirection Then
                bScaleable = False
                aVector = aVector.Normalized
            End If
            Dim _rVal As dxoProperty
            If String.IsNullOrWhiteSpace(aName) Then aName = "" Else aName = aName.Trim()
            If aName = "" Then aName = $"Prop_{Count + 1}"

            If bIsHeader Then
                If Not aName.StartsWith("$") Then aName = $"${aName}"
                _rVal = New dxoProperty(9, aName, aName, dxxPropertyTypes.dxf_String, Nothing, bScalable:=bScaleable, bSuppressed:=bSuppressed) With {.Key = aName}
                Add(_rVal, bHidden, bIsHeader)
                Dim p1 As New dxoProperty(aGroupCode, aVector.X, $"{aName}_X", dxxPropertyTypes.dxf_Double, Nothing, bScalable:=bScaleable, bSuppressed:=bSuppressed, bIsOrdinate:=True)
                Dim p2 As New dxoProperty(aGroupCode + 10, aVector.Y, $"{aName}_Y", dxxPropertyTypes.dxf_Double, Nothing, bScalable:=bScaleable, bSuppressed:=bSuppressed, bIsOrdinate:=True)

                Add(p1, bHidden)
                Add(p2, bHidden)
                If Not bTwoD Then
                    Dim p3 As New dxoProperty(aGroupCode + 20, aVector.Z, $"{aName}_Z", dxxPropertyTypes.dxf_Double, Nothing, bScalable:=bScaleable, bSuppressed:=bSuppressed, bIsOrdinate:=True)
                    Add(p3, bHidden)

                End If

            Else

                _rVal = New dxoProperty(aGroupCode, aVector.X, $"{aName}_X", dxxPropertyTypes.dxf_Double, Nothing, bScalable:=bScaleable, bSuppressed:=bSuppressed, bIsOrdinate:=True)
                Dim p1 As New dxoProperty(aGroupCode + 10, aVector.Y, $"{aName}_Y", dxxPropertyTypes.dxf_Double, Nothing, bScalable:=bScaleable, bSuppressed:=bSuppressed, bIsOrdinate:=True)

                Add(_rVal, bHidden)
                Add(p1, bHidden)
                If Not bTwoD Then
                    Dim p2 As New dxoProperty(aGroupCode + 20, aVector.Z, $"{aName}_Z", dxxPropertyTypes.dxf_Double, Nothing, bScalable:=bScaleable, bSuppressed:=bSuppressed, bIsOrdinate:=True)
                    Add(p2, bHidden)

                End If

            End If
            Return _rVal

        End Function


        Public Function AddTo(aGroupCode As Integer, aValue As Object, aName As String, aSuppressedValue As Boolean?,
                                 ByRef ioPropertyType As dxxPropertyTypes, bSuppressed As Boolean,
                                 bScalable As Boolean, bSetToSuppressed As Boolean, aLastVal As Object, ByRef rHidden As Boolean) As dxoProperty

            If aName Is Nothing Then aName = ""
            aName = aName.Trim
            Dim _rVal As New dxoProperty(aGroupCode, aName)
            Dim bSupVal As Boolean = aSuppressedValue.HasValue
            rHidden = aGroupCode < 0 Or aName.StartsWith("*")
            If aGroupCode < 0 Then aGroupCode = Math.Abs(aGroupCode)

            Dim cnt As Integer = Count
            ' If rHidden And aGroupCode = 0 Then aGroupCode = -(cnt + 1)

            'If bNonDXF And aGroupCode = 0 Then aGroupCode = cnt + 1
            ioPropertyType = TPROPERTY.GetSetType(aGroupCode, aValue, ioPropertyType, False)
            If ioPropertyType = dxxPropertyTypes.Handle Then bSupVal = False
            If ioPropertyType <> dxxPropertyTypes.dxf_Variant And bSupVal Then
                aSuppressedValue = TPROPERTY.SetValueByType(aSuppressedValue.Value, ioPropertyType)
            End If
            aValue = TPROPERTY.SetValueByType(aValue, ioPropertyType)

            _rVal.PropertyType = ioPropertyType
            _rVal.GroupCode = aGroupCode
            _rVal.Name = aName
            _rVal.Hidden = rHidden
            If _rVal.PropertyType = dxxPropertyTypes.dxf_Single Or _rVal.PropertyType = dxxPropertyTypes.dxf_Double Or _rVal.PropertyType = dxxPropertyTypes.dxf_Integer Then
                _rVal.Scaleable = bScalable
            End If
            _rVal.Value = aValue
            If bSetToSuppressed Then _rVal.SuppressedValue = _rVal.Value
            If bSupVal Then _rVal.SuppressedValue = aSuppressedValue.Value
            If Not _rVal.Hidden Then
                _rVal.Suppressed = bSuppressed
            Else
                If _rVal.GroupCode = 0 Then _rVal.GroupCode = 1
            End If
            If aLastVal IsNot Nothing Then
                aLastVal = TPROPERTY.SetValueByType(aLastVal, ioPropertyType)
                _rVal.LastValue = aLastVal
            Else
                _rVal.LastValue = _rVal.Value
            End If
            If _rVal.PropertyType = dxxPropertyTypes.Color And _rVal.DecodeString = "" Then
                _rVal.DecodeString = dxfGlobals.ColorDecode
            End If
            'If Not _rVal.Hidden And rHidden Then _rVal.GroupCode *= -1

            rHidden = _rVal.Hidden
            Add(_rVal)
            Return _rVal
        End Function

        Friend Sub Append(aProperties As TPROPERTIES)
            For i = 1 To aProperties.Count
                Add(aProperties.Item(i))
            Next
        End Sub

        Friend Sub Append(aProperties As IEnumerable(Of dxoProperty), Optional bAddClones As Boolean = False)
            If aProperties Is Nothing Then Return
            For Each prop In aProperties
                If Not bAddClones Then Add(prop) Else Add(New dxoProperty(prop))
            Next
        End Sub

        Friend Function ReactorAdd(aPropName As String, aName As String, aHandle As String, Optional aOccur As Integer = 1) As Boolean
            Dim aProperty As dxoProperty = Nothing
            If Not TryGet(aPropName, aProperty, aOccur) Then Return False
            Dim _rVal As Boolean = aProperty.ReactorAdd(aName, aHandle)
            Return _rVal
        End Function

        Friend Function ReactorAdd(aPropGroupCode As Integer, aName As String, aHandle As String, Optional aOccur As Integer = 1) As Boolean
            Dim aProperty As dxoProperty = Nothing
            If Not TryGet(aPropGroupCode, aProperty, aOccur) Then Return False
            Dim _rVal As Boolean = aProperty.ReactorAdd(aName, aHandle)
            Return _rVal
        End Function

        Friend Function SetSuppressed(aPropName As String, aValue As Boolean, Optional aOccur As Integer = 1) As Boolean
            'base 1
            Dim aMem As dxoProperty = Nothing
            If Not TryGet(aPropName, aMem, aOccur) Then Return False
            Dim _rVal As Boolean = aMem.Suppressed <> aValue
            aMem.Suppressed = aValue
            Return _rVal
        End Function

        Friend Function SetSuppressed(aPropGroupCode As Integer, aValue As Boolean, Optional aOccur As Integer = 1) As Boolean
            'base 1
            Dim aMem As dxoProperty = Nothing
            If Not TryGet(aPropGroupCode, aMem, aOccur) Then Return False
            Dim _rVal As Boolean = aMem.Suppressed <> aValue
            aMem.Suppressed = aValue
            Return _rVal
        End Function

        Public Function Clone() As dxoProperties
            Return New dxoProperties(Me)
        End Function

        Public Function CopyValue(aFromProps As dxoProperties, aName As String, Optional aDefStringVal As String = Nothing, Optional aOccur As Integer = 1) As Boolean
            If aFromProps Is Nothing Then Return False
            Dim aProp As dxoProperty = Nothing
            Dim bProp As dxoProperty = Nothing
            If Not TryGet(aName, aProp, aOccur) Then Return False
            If Not aFromProps.TryGet(aName, bProp, aOccur) Then Return False

            If aProp.PropertyType >= dxxPropertyTypes.dxf_String And aProp.PropertyType <= dxxPropertyTypes.ClassMarker And aDefStringVal IsNot Nothing Then
                If aProp.ValueS = "" Then aProp.Value = aDefStringVal
                If bProp.ValueS = "" Then
                    bProp.Value = aDefStringVal
                End If
            End If
            Return SetVal(aProp, bProp.Value)
        End Function

        Public Function CopyValue(aFromProps As dxoProperties, aGroupCode As Integer, Optional aDefStringVal As String = Nothing, Optional aOccur As Integer = 1) As Boolean
            If aFromProps Is Nothing Then Return False
            Dim aProp As dxoProperty = Nothing
            Dim bProp As dxoProperty = Nothing
            If Not TryGet(aGroupCode, aProp, aOccur) Then Return False
            If Not aFromProps.TryGet(aGroupCode, bProp, aOccur) Then Return False

            If aProp.PropertyType >= dxxPropertyTypes.dxf_String And aProp.PropertyType <= dxxPropertyTypes.ClassMarker And aDefStringVal IsNot Nothing Then
                If aProp.ValueS = "" Then aProp.Value = aDefStringVal
                If bProp.ValueS = "" Then
                    bProp.Value = aDefStringVal
                End If
            End If
            Return SetVal(aProp, bProp.Value)
        End Function

        Friend Function CopyValue(aFromProps As TPROPERTIES, aName As String, Optional aDefStringVal As String = Nothing, Optional aOccur As Integer = 1) As Boolean
            If aFromProps.Count <= 0 Then Return False
            Dim aProp As dxoProperty = Nothing
            If Not TryGet(aName, aProp, aOccur) Then Return False
            Dim bProp As TPROPERTY = Nothing
            If Not aFromProps.TryGet(aName, bProp, True) Then Return False

            If aProp.PropertyType >= dxxPropertyTypes.dxf_String And aProp.PropertyType <= dxxPropertyTypes.ClassMarker And aDefStringVal IsNot Nothing Then
                If aProp.ValueS = "" Then aProp.Value = aDefStringVal
                If bProp.StringValue = "" Then
                    bProp.Value = aDefStringVal
                    aFromProps.UpdateProperty = bProp
                End If
            End If
            Return SetVal(aProp, bProp.Value)
        End Function

        Public Function CopyDisplayProperties(aSet As dxfDisplaySettings) As Boolean
            If aSet Is Nothing Then Return False
            Dim _rVal As Boolean = False
            If SetVal(dxfLinetypes.Invisible, aSet.Suppressed) Then _rVal = True
            If aSet.Color <> dxxColors.Undefined Then
                If SetVal("Color", aSet.Color) Then _rVal = True
            End If
            If Not String.IsNullOrWhiteSpace(aSet.LayerName) Then
                If SetVal("LayerName", aSet.LayerName) Then _rVal = True
            End If
            If Not String.IsNullOrWhiteSpace(aSet.Linetype) Then
                If SetVal("LineType", aSet.Linetype) Then _rVal = True
            End If
            If aSet.LTScale > 0 Then
                If SetVal("LT Scale", aSet.LTScale) Then _rVal = True
            End If
            If aSet.LineWeight <> dxxLineWeights.Undefined Then
                If SetVal("LineWeight", aSet.LineWeight) Then _rVal = True
            End If
            Return _rVal
        End Function

        Friend Function CopyDisplayProperties(aSet As TDISPLAYVARS) As Boolean
            Dim _rVal As Boolean = False
            If SetVal(dxfLinetypes.Invisible, aSet.Suppressed) Then _rVal = True
            If aSet.Color <> dxxColors.Undefined Then
                If SetVal("Color", aSet.Color) Then _rVal = True
            End If
            If Not String.IsNullOrWhiteSpace(aSet.LayerName) Then
                If SetVal("LayerName", aSet.LayerName) Then _rVal = True
            End If
            If Not String.IsNullOrWhiteSpace(aSet.Linetype) Then
                If SetVal("LineType", aSet.Linetype) Then _rVal = True
            End If
            If aSet.LTScale > 0 Then
                If SetVal("LT Scale", aSet.LTScale) Then _rVal = True
            End If
            If aSet.LineWeight <> dxxLineWeights.Undefined Then
                If SetVal("LineWeight", aSet.LineWeight) Then _rVal = True
            End If
            Return _rVal
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New dxoProperties(Me)
        End Function

        Public Shadows Function Item(aIndex As Integer, Optional bSuppressIndexError As Boolean = False) As dxoProperty


            If aIndex < 1 Or aIndex > Count Then
                If Not bSuppressIndexError And dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return Nothing
            End If


            Return MyBase.Item(aIndex - 1)
        End Function

        Public Shadows Function Item(aName As String, Optional aPrefixsToConsider As List(Of String) = Nothing) As dxoProperty
            Return Member(aName, aPrefixsToConsider:=aPrefixsToConsider)

        End Function


        Public Function TryGet(aName As String, ByRef rMember As dxoProperty, Optional aOccur As Integer = 1, Optional aPrefixsToConsider As List(Of String) = Nothing) As Boolean
            rMember = Nothing
            If String.IsNullOrWhiteSpace(aName) Then Return False
            rMember = Member(aName, aOccur, aPrefixsToConsider)
            Return rMember IsNot Nothing
        End Function

        Public Function TryGet(aGroupCode As Integer, ByRef rMember As dxoProperty, Optional aOccur As Integer = 1) As Boolean
            rMember = Member(aGroupCode, aOccur)
            Return rMember IsNot Nothing
        End Function

        Public Function TryGet(aHeaderVar As dxxHeaderVars, ByRef rMember As dxoProperty, Optional aOccur As Integer = 1) As Boolean


            rMember = Nothing
            Try

                Return TryGet(dxfEnums.PropertyName(aHeaderVar), rMember, aOccur)
            Catch ex As Exception
                'add an error
                'Throw ex
                Return False
            End Try
        End Function

        Public Function TryGet(aDimStyleVar As dxxDimStyleProperties, ByRef rMember As dxoProperty, Optional aOccur As Integer = 1) As Boolean

            rMember = Nothing
            Try

                Return TryGet(dxfEnums.PropertyName(aDimStyleVar), rMember, aOccur)
            Catch ex As Exception
                'add an error
                'Throw ex
                Return False
            End Try
        End Function

        Public Function TryGet(aStyleVar As dxxStyleProperties, ByRef rMember As dxoProperty, Optional aOccur As Integer = 1) As Boolean

            rMember = Nothing
            Try

                Return TryGet(dxfEnums.PropertyName(aStyleVar), rMember, aOccur)
            Catch ex As Exception
                'add an error
                'Throw ex
                Return False
            End Try
        End Function

        Public Function TryGet(aLayerVar As dxxLayerProperties, ByRef rMember As dxoProperty, Optional aOccur As Integer = 1) As Boolean

            rMember = Nothing
            Try

                Return TryGet(dxfEnums.PropertyName(aLayerVar), rMember, aOccur)
            Catch ex As Exception
                'add an error
                'Throw ex
                Return False
            End Try
        End Function

        Public Function TryGet(aLTVar As dxxLinetypeProperties, ByRef rMember As dxoProperty, Optional aOccur As Integer = 1) As Boolean

            rMember = Nothing
            Try

                Return TryGet(dxfEnums.PropertyName(aLTVar), rMember, aOccur)
            Catch ex As Exception
                'add an error
                'Throw ex
                Return False
            End Try
        End Function

        Private Function TryGet(aIndex As [Enum], ByRef rMember As dxoProperty, Optional aOccur As Integer = 1) As Boolean

            rMember = Nothing
            Try

                Return TryGet(dxfEnums.PropertyName(aIndex), rMember, aOccur)
            Catch ex As Exception
                'add an error
                'Throw ex
                Return False
            End Try
        End Function



        Public Function TryGetProperty(aNameOrGroupCode As Object, ByRef rMember As dxoProperty, Optional aOccur As Integer = 1)
            rMember = Nothing
            If aNameOrGroupCode Is Nothing Then Return False

            If TypeOf aNameOrGroupCode Is String Then
                Return TryGet(aNameOrGroupCode.ToString(), rMember, aOccur)
            ElseIf TVALUES.IsNumber(aNameOrGroupCode) Then
                Return TryGet(TVALUES.To_INT(aNameOrGroupCode), rMember, aOccur)
            ElseIf TypeOf aNameOrGroupCode Is dxxHeaderVars Then
                Dim enumval As dxxHeaderVars = aNameOrGroupCode
                Return TryGet(aHeaderVar:=enumval, rMember:=rMember, aOccur:=aOccur)
            ElseIf TypeOf aNameOrGroupCode Is dxxDimStyleProperties Then
                Dim enumval As dxxDimStyleProperties = aNameOrGroupCode
                Return TryGet(aDimStyleVar:=enumval, rMember:=rMember, aOccur:=aOccur)
            ElseIf TypeOf aNameOrGroupCode Is dxxStyleProperties Then
                Dim enumval As dxxStyleProperties = aNameOrGroupCode
                Return TryGet(aStyleVar:=enumval, rMember:=rMember, aOccur:=aOccur)
            Else
                Return False
            End If
        End Function

        Public Function TryGet(aGroupCode As Integer, aStringVal As String, ByRef rMember As dxoProperty, Optional aOccur As Integer = 1, Optional bIgnoreCase As Boolean = True) As Boolean
            rMember = Nothing
            If aOccur <= 0 Then aOccur = 1
            Dim mems As List(Of dxoProperty) = FindAll(Function(x) x.GroupCode = aGroupCode And String.Compare(x.StringValue, aStringVal, bIgnoreCase) = 0)
            If mems.Count <= 0 Or mems.Count < aOccur Then Return False
            rMember = mems.Item(aOccur - 1)
            Return True

        End Function


        Public Function ValueS(aName As String, Optional aOccur As Integer = 1, Optional aDefault As String = "") As String
            Dim prop As dxoProperty = Nothing
            If Not TryGet(aName, prop, aOccur:=aOccur) Then Return aDefault
            Return prop.ValueS
        End Function

        Public Function ValueD(aName As String, Optional aOccur As Integer = 1, Optional aDefault As Double = 0.0) As Double
            Dim prop As dxoProperty = Nothing
            If Not TryGet(aName, prop, aOccur:=aOccur) Then Return aDefault
            Return prop.ValueD
        End Function

        Public Function ValueI(aName As String, Optional aOccur As Integer = 1, Optional aDefault As Integer = 0) As Integer
            Dim prop As dxoProperty = Nothing
            If Not TryGet(aName, prop, aOccur:=aOccur) Then Return aDefault
            Return prop.ValueI
        End Function

        Friend Function ValueB(aName As String, Optional aOccur As Integer = 1, Optional aDefault As Boolean = False) As Boolean
            Dim prop As dxoProperty = Nothing
            If Not TryGet(aName, prop, aOccur:=aOccur) Then Return aDefault
            Return prop.ValueB
        End Function

        Public Function Vector(aName As String, Optional aOccur As Integer = 1, Optional aDefault As dxfVector = Nothing) As dxfVector

            Dim prop As dxoProperty = Nothing
            If Not TryGet(aName, prop, aOccur:=aOccur) Then Return aDefault

            Dim _rVal As New dxfVector(prop.ValueD, 0)

            Dim idx As Integer = IndexOf(prop)

            Dim prop2 As dxoProperty = Item(idx + 1)
            If prop2 IsNot Nothing Then
                If prop2.GroupCode = prop.GroupCode + 10 Then
                    _rVal.Y = prop2.ValueD
                Else
                    Return _rVal
                End If
            Else
                Return _rVal
            End If

            prop2 = Item(idx + 2)
            If prop2 IsNot Nothing Then
                If prop2.GroupCode = prop.GroupCode + 20 Then
                    _rVal.Z = prop2.ValueD
                Else
                    Return _rVal
                End If
            Else
                Return _rVal
            End If

            Return _rVal
        End Function

        Friend Function ValueV(aName As String, Optional aOccur As Integer = 1, Optional aDefault As dxfVector = Nothing) As TVECTOR
            Dim prop As dxoProperty = Nothing

            If Not TryGet(aName, prop, aOccur:=aOccur) Then Return New TVECTOR(aDefault)

            Dim _rVal As New TVECTOR(prop.ValueD)



            Dim prop2 As dxoProperty = Item(IndexOf(prop) + 1)
            If prop2 IsNot Nothing Then
                If prop2.GroupCode = prop.GroupCode + 10 Then
                    _rVal.Y = prop2.ValueD
                Else
                    Return _rVal
                End If
            Else
                Return _rVal
            End If

            prop2 = Item(IndexOf(prop) + 2)
            If prop2 IsNot Nothing Then
                If prop2.GroupCode = prop.GroupCode + 20 Then
                    _rVal.Z = prop2.ValueD
                Else
                    Return _rVal
                End If
            Else
                Return _rVal
            End If

            Return _rVal
        End Function

        Public Function ValueS(aGroupCode As Integer, Optional aOccur As Integer = 1, Optional aDefault As String = "") As String
            Dim prop As dxoProperty = Nothing
            If Not TryGet(aGroupCode, prop, aOccur:=aOccur) Then Return aDefault
            Return prop.ValueS
        End Function

        Public Function ValueD(aGroupCode As Integer, Optional aOccur As Integer = 1, Optional aDefault As Double = 0.0) As Double
            Dim prop As dxoProperty = Nothing
            If Not TryGet(aGroupCode, prop, aOccur:=aOccur) Then Return aDefault
            Return prop.ValueD
        End Function

        Public Function ValueI(aGroupCode As Integer, Optional aOccur As Integer = 1, Optional aDefault As Integer = 0) As Integer
            Dim prop As dxoProperty = Nothing
            If Not TryGet(aGroupCode, prop, aOccur:=aOccur) Then Return aDefault
            Return prop.ValueI
        End Function


        Friend Function ValueB(aGroupCode As Integer, Optional aOccur As Integer = 1, Optional aDefault As Boolean = False) As Boolean
            Dim prop As dxoProperty = Nothing
            If Not TryGet(aGroupCode, prop, aOccur:=aOccur) Then Return aDefault
            Return prop.ValueB
        End Function

        Public Function Vector(aGroupCode As Integer, Optional aOccur As Integer = 1, Optional aDefault As dxfVector = Nothing) As dxfVector

            Dim prop As dxoProperty = Nothing
            If Not TryGet(aGroupCode, prop, aOccur:=aOccur) Then Return aDefault

            Dim _rVal As New dxfVector(prop.ValueD, 0)

            Dim idx As Integer = IndexOf(prop)

            Dim prop2 As dxoProperty = Item(idx + 1)
            If prop2 IsNot Nothing Then
                If prop2.GroupCode = prop.GroupCode + 10 Then
                    _rVal.Y = prop2.ValueD
                Else
                    Return _rVal
                End If
            Else
                Return _rVal
            End If

            prop2 = Item(idx + 2)
            If prop2 IsNot Nothing Then
                If prop2.GroupCode = prop.GroupCode + 20 Then
                    _rVal.Z = prop2.ValueD
                Else
                    Return _rVal
                End If
            Else
                Return _rVal
            End If

            Return _rVal
        End Function

        Public Function Direction(aGroupCode As Integer, Optional aOccur As Integer = 1, Optional aDefault As dxfDirection = Nothing) As dxfDirection

            Dim v1 As dxfVector = Vector(aGroupCode, aOccur)
            If v1 Is Nothing And Not aDefault Is Nothing Then v1 = New dxfVector(aDefault)
            If v1 Is Nothing Then Return Nothing Else Return New dxfDirection(v1)

        End Function


        Friend Function ValueV(aGroupCode As Integer, Optional aOccur As Integer = 1, Optional aDefault As dxfVector = Nothing) As TVECTOR
            Dim prop As dxoProperty = Nothing

            If Not TryGet(aGroupCode, prop, aOccur:=aOccur) Then Return New TVECTOR(aDefault)

            Dim _rVal As New TVECTOR(prop.ValueD)



            Dim prop2 As dxoProperty = Item(IndexOf(prop) + 1)
            If prop2 IsNot Nothing Then
                If prop2.GroupCode = prop.GroupCode + 10 Then
                    _rVal.Y = prop2.ValueD
                Else
                    Return _rVal
                End If
            Else
                Return _rVal
            End If

            prop2 = Item(IndexOf(prop) + 2)
            If prop2 IsNot Nothing Then
                If prop2.GroupCode = prop.GroupCode + 20 Then
                    _rVal.Z = prop2.ValueD
                Else
                    Return _rVal
                End If
            Else
                Return _rVal
            End If

            Return _rVal
        End Function




        Public Function Vertices(aGroupCode As Integer) As colDXFVectors
            Return New colDXFVectors(VerticesV(aGroupCode))
        End Function


        Friend Function VerticesV(aGroupCode As Integer, Optional aGlobalWidth As Double = 0, Optional bConfirmArcs As Boolean = False, Optional aPlane As dxfPlane = Nothing) As TVERTICES
            Dim _rVal As New TVERTICES
            Dim gcProps As List(Of dxoProperty) = GroupCodeMembers(aGroupCode)
            If gcProps.Count <= 0 Then Return _rVal

            Dim j As Integer
            For Each prop As dxoProperty In gcProps
                j = IndexOf(prop)
                Dim v2 As New TVERTEX(TVALUES.To_DBL(prop.Value))
                Dim prop2 As dxoProperty = Item(j + 1, True)
                If prop2 IsNot Nothing Then
                    If prop2.GroupCode = prop.GroupCode + 10 Then
                        v2.Y = TVALUES.To_DBL(prop2.Value)
                        j += 2

                    Else
                        j += 1
                    End If
                End If
                Dim prop3 As dxoProperty = Item(j, True)
                Dim prop4 As dxoProperty = Item(j + 1, True)
                Dim prop5 As dxoProperty = Item(j + 2, True)
                If prop3 IsNot Nothing Then
                    If prop3.GroupCode = 40 Or prop3.GroupCode = 41 Or prop3.GroupCode = 42 Then
                        Select Case prop3.GroupCode
                            Case 40
                                v2.StartWidth = TVALUES.To_DBL(prop3.Value)
                            Case 41
                                v2.EndWidth = TVALUES.To_DBL(prop3.Value)
                            Case 42
                                v2.Bulge = TVALUES.To_DBL(prop3.Value)
                        End Select

                    End If
                End If
                If prop4 IsNot Nothing Then
                    If prop4.GroupCode = 40 Or prop4.GroupCode = 41 Or prop4.GroupCode = 42 Then
                        Select Case prop4.GroupCode
                            Case 40
                                v2.StartWidth = TVALUES.To_DBL(prop4.Value)
                            Case 41
                                v2.EndWidth = TVALUES.To_DBL(prop4.Value)
                            Case 42
                                v2.Bulge = TVALUES.To_DBL(prop4.Value)
                        End Select

                    End If
                End If
                If prop5 IsNot Nothing Then
                    If prop5.GroupCode = 40 Or prop5.GroupCode = 41 Or prop5.GroupCode = 42 Then
                        Select Case prop5.GroupCode
                            Case 40
                                v2.StartWidth = TVALUES.To_DBL(prop5.Value)
                            Case 41
                                v2.EndWidth = TVALUES.To_DBL(prop5.Value)
                            Case 42
                                v2.Bulge = TVALUES.To_DBL(prop5.Value)
                        End Select

                    End If
                End If
                If aGlobalWidth > 0 Then
                    v2.StartWidth = aGlobalWidth
                    v2.EndWidth = aGlobalWidth
                End If
                _rVal.Add(v2)

            Next
            If bConfirmArcs Then
                Dim plane As New TPLANE(aPlane)
                For i As Integer = 1 To _rVal.Count
                    Dim v1 As TVERTEX = _rVal.Item(i)
                    v1.Vector = plane.WorldVector(v1.Vector)
                    If v1.Bulge <> 0 Then

                        Dim v2 As TVERTEX
                        Dim mpt As TVECTOR


                        If i <= _rVal.Count - 1 Then v2 = _rVal.Item(i + 1) Else v2 = _rVal.Item(1)
                        'confirm arc vectors
                        Dim aA As TARC = TARC.ByBulge(v1.Vector, plane.WorldVector(v2.Vector), aBulge:=v1.Bulge, rMP:=mpt, bSuppressErrs:=True, aPlane:=plane)
                        If aA.Radius > 0 Then
                            v1.Radius = aA.Radius
                            v1.Inverted = aA.ClockWise
                        Else
                            v1.Radius = 0
                        End If
                    End If



                    _rVal.SetItem(i, v1)
                Next i
            End If



            Return _rVal
        End Function

        Public Function Members(aGroupCode As Integer, aStringVal As String, Optional bIgnoreCase As Boolean = True) As dxoProperties
            Dim _rVal As New dxoProperties(Name)
            _rVal.Append(FindAll(Function(x) x.GroupCode = aGroupCode And String.Compare(x.StringValue, aStringVal, bIgnoreCase) = 0))
            Return _rVal
        End Function

        Public Function Member(aGroupCode As Integer, Optional aOccur As Integer = 1) As dxoProperty

            Dim rMembers As List(Of dxoProperty) = Members(aGroupCode)

            If rMembers.Count <= 0 Then Return Nothing
            If aOccur <= 1 Then
                Return rMembers(0)
            Else
                Return rMembers.Find(Function(x) x.Occurance = aOccur)
            End If


        End Function
        Public Function RemoveByStringValue(aStringValue As String, Optional bIgnoreCase As Boolean = True) As List(Of dxoProperty)
            Dim _rVal As List(Of dxoProperty) = FindAll(Function(x) String.Compare(x.ValueS, aStringValue, bIgnoreCase) = 0)
            For Each mem As dxoProperty In _rVal
                Remove(mem)
            Next
            If _rVal.Count > 0 Then ReIndex()
            Return _rVal
        End Function
        Public Function Members(aGroupCode As Integer) As List(Of dxoProperty)
            Dim _rVal As List(Of dxoProperty) = FindAll(Function(x) x.GroupCode = aGroupCode)

            For i = 1 To _rVal.Count
                _rVal(i - 1).Occurance = i
            Next

            Return _rVal
        End Function


        Public Function Members(aName As String, Optional aPrefixsToConsider As List(Of String) = Nothing) As List(Of dxoProperty)


            If aPrefixsToConsider Is Nothing Then
                aPrefixsToConsider = New List(Of String)({"*"}) ' for hidden prperties
            End If

            If String.IsNullOrWhiteSpace(aName) Then Return New List(Of dxoProperty)() Else aName = aName.Trim()
            Dim _rVal As List(Of dxoProperty) = FindAll(Function(x) x.IsNamed(aName, aPrefixsToConsider))

            For i = 1 To _rVal.Count
                _rVal(i - 1).Occurance = i
            Next

            Return _rVal
        End Function

        Public Function Member(aName As String, Optional aOccur As Integer = 1, Optional aPrefixsToConsider As List(Of String) = Nothing) As dxoProperty
            Dim rMembers As List(Of dxoProperty) = Members(aName, aPrefixsToConsider)

            If rMembers.Count <= 0 Then Return Nothing
            If aOccur <= 1 Then
                Return rMembers(0)
            Else
                Return rMembers.Find(Function(x) x.Occurance = aOccur)
            End If

        End Function

        Public Function Value(aName As String, Optional aOccur As Integer = 1) As Object
            Dim member As dxoProperty = Nothing
            If TryGet(aName, member, aOccur) Then Return member.Value Else Return Nothing
        End Function

        Public Function Value(aGroupCode As Integer, Optional aOccur As Integer = 1) As Object
            Dim member As dxoProperty = Nothing
            If TryGet(aGroupCode, member, aOccur) Then Return member.Value Else Return String.Empty
        End Function

        Public Shadows Function IndexOf(aMember As dxoProperty) As Integer
            Return MyBase.IndexOf(aMember) + 1
        End Function

        Public Function GetSubSet(aGroupCode As Integer, Optional aName As String = "", Optional aStartIndex As Integer = 1) As dxoProperties

            If String.IsNullOrWhiteSpace(aName) Then aName = Name
            Dim _rVal As New dxoProperties(aName)
            Dim keep As Boolean = False
            Dim si As Integer = 1
            If aStartIndex > Count Then Return _rVal
            If aStartIndex <= 0 Then aStartIndex = 1

            For i As Integer = aStartIndex To Count
                If i >= aStartIndex Then

                    Dim prop As dxoProperty = MyBase.Item(i - 1)

                    If prop.GroupCode = aGroupCode Then
                        If Not keep Then
                            keep = True
                            _rVal.Add(prop)
                        Else
                            Exit For
                        End If
                    Else
                        If keep Then _rVal.Add(prop)
                    End If

                End If

            Next
            Return _rVal

        End Function

        Public Function SubSet(aStartIndex As Integer, aEndIndex As Integer, Optional bGetClones As Boolean = False, Optional bRemove As Boolean = False) As dxoProperties
            Dim _rVal As New dxoProperties(Name)
            TVALUES.SortTwoValues(True, aStartIndex, aEndIndex)

            If aStartIndex < 1 Or aStartIndex > Count Then Return _rVal
            If aEndIndex < 1 Or aEndIndex > Count Then Return _rVal
            Dim cnt As Integer = aEndIndex - aStartIndex + 1
            Dim range As List(Of dxoProperty) = GetRange(aStartIndex, cnt)

            _rVal.Append(range, bAddClones:=bGetClones)
            If bRemove Then RemoveRange(aStartIndex, cnt)
            Return _rVal
        End Function

        Public Sub ReIndex(Optional bHiddenToEnd As Boolean = False)

            If bHiddenToEnd Then
                Dim idx As Integer = 0
                Dim vProps As New List(Of dxoProperty)
                Dim hProps As New List(Of dxoProperty)
                Dim aMem As dxoProperty
                For i As Integer = 1 To Count
                    aMem = Me(i - 1)
                    If Not aMem.Hidden Then
                        vProps.Add(aMem)
                    Else
                        hProps.Add(aMem)
                    End If
                Next i
                Clear()
                For i As Integer = 1 To vProps.Count
                    idx += 1
                    aMem = vProps(i - 1)
                    aMem.Index = idx
                    MyBase.Add(aMem)

                Next
                For i As Integer = 1 To hProps.Count
                    idx += 1
                    aMem = hProps(i - 1)
                    aMem.Index = idx
                    MyBase.Add(aMem)

                Next
            Else
                For i As Integer = 1 To Count
                    Me(i - 1).Index = i
                Next i
            End If
        End Sub

        Public Shadows Function GetRange(aStartIndex As Integer, aCount As Integer) As List(Of dxoProperty)

            Return MyBase.GetRange(aStartIndex - 1, aCount)
        End Function

        Public Shadows Sub RemoveRange(aStartIndex As Integer, aCount As Integer)

            MyBase.RemoveRange(aStartIndex - 1, aCount)
        End Sub

        Public Function GetSubSet(aGroupCode As Integer, aEndGroupCode As Integer, aEndString As String, Optional aName As String = "", Optional aStartIndex As Integer = 1) As dxoProperties

            If String.IsNullOrWhiteSpace(aName) Then aName = Name
            Dim _rVal As New dxoProperties(aName)
            Dim keep As Boolean = False
            Dim si As Integer = 1
            If aStartIndex > Count Then Return _rVal
            If aStartIndex <= 0 Then aStartIndex = 1

            For i As Integer = aStartIndex To Count
                If i >= aStartIndex Then

                    Dim prop As dxoProperty = MyBase.Item(i - 1)

                    If prop.GroupCode = aGroupCode Then
                        If Not keep Then
                            keep = True
                            _rVal.Add(prop)
                        End If
                    Else
                        If keep Then
                            _rVal.Add(prop)
                            If prop.GroupCode = aEndGroupCode Then
                                If String.Compare(prop.StringValue, aEndString, True) = 0 Then
                                    Exit For
                                End If
                            End If

                        End If
                    End If

                End If

            Next
            Return _rVal

        End Function


        Friend Function ExtractReactorGroups() As dxoPropertyArray
            Dim _rVal As New dxoPropertyArray("Reactors")
            Dim reaProps As dxoProperties = GetByGroupCode(102)

            If reaProps.Count <= 0 Then Return _rVal

            If Count <= 0 Then Return _rVal


            Dim si As Integer = 1


            For i As Integer = 1 To reaProps.Count
                Dim aProp As dxoProperty = reaProps.Item(i)
                If i + 1 > reaProps.Count Then Exit For
                Dim bProp As dxoProperty = reaProps.Item(i + 1)
                i += 1
                Dim rGroup As New dxoProperties(aProp.StringValue)


                For j = IndexOf(aProp) To IndexOf(bProp)
                    rGroup.Add(Item(j))
                Next j
                _rVal.Add(rGroup)

            Next i
            For Each sprops As dxoProperties In _rVal
                For Each prop As dxoProperty In sprops
                    Remove(prop)
                Next
            Next

            Return _rVal
        End Function

        Public Function GetByGroupCode(aGroupCode As Integer, Optional aMatchValue As Object = Nothing, Optional aFollowerCount As Integer = 0, Optional bRemove As Boolean = False, Optional aName As String = "", Optional bJustOne As Boolean = False, Optional aNameList As String = "") As dxoProperties
            Dim rFirstIndex As Integer = 0
            Return GetByGroupCode(aGroupCode, aMatchValue, aFollowerCount, bRemove, aName, bJustOne, rFirstIndex, aNameList)
        End Function
        Public Function GetByGroupCode(aGroupCode As Integer, aMatchValue As Object, aFollowerCount As Integer, bRemove As Boolean, aName As String, bJustOne As Boolean, ByRef rFirstIndex As Integer, Optional aNameList As String = "") As dxoProperties
            Dim _rVal As New dxoProperties()

            Dim cnt As Integer = Count
            If cnt <= 0 Then Return _rVal


            Dim bTestA As Boolean
            Dim strTestA As String = String.Empty
            Dim bKeep As Boolean



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
            bTestA = aMatchValue IsNot Nothing
            If bTestA Then
                strTestA = aMatchValue.ToString()

            End If

            For i As Integer = 1 To cnt
                Dim aProp As dxoProperty = Item(i)
                If aProp.GroupCode = aGroupCode Then
                    bKeep = True
                    If bTestA Then
                        If String.Compare(aProp.ValueString, strTestA, True) <> 0 Then
                            bKeep = False
                        End If
                    End If
                    If bKeep Then
                        If rFirstIndex = 0 Then rFirstIndex = i
                        If bNamesPassed Then
                            aProp.Name = pNames(0)
                        End If
                        _rVal.Add(aProp)

                        If aFollowerCount > 0 Then
                            Dim iskip As Integer = 0
                            For j As Integer = 1 To aFollowerCount
                                If i + j <= cnt Then
                                    iskip += 1
                                    aProp = Item(i + j)

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
                For Each prop As dxoProperty In _rVal
                    Remove(prop)
                Next
            End If
            Return _rVal
        End Function


        Public Function GroupCodes(Optional bUniquesOnly As Boolean = True, Optional aGCListToOmmit As String = "") As List(Of Integer)
            Return dxoProperties.GetGroupCodes(Me, bUniquesOnly, aGCListToOmmit)

        End Function

        Public Function Keys(Optional bExcludeHandles As Boolean = False, Optional bExcludePointers As Boolean = False, Optional bIncludeHiddenProps As Boolean = False, Optional aPrefixFilter As String = Nothing, Optional bExcludeDoNotCopy As Boolean = True, Optional aGCSkipList As List(Of Integer) = Nothing) As List(Of String)
            Return dxoProperties.GetKeys(Me, bExcludeHandles, bExcludePointers, bIncludeHiddenProps, aPrefixFilter, bExcludeDoNotCopy, aGCSkipList)

        End Function

        Public Function Names(Optional bUniquesOnly As Boolean = False, Optional aGCListToOmmit As String = "") As List(Of String)

            Dim _rVal As New List(Of String)

            If Not String.IsNullOrWhiteSpace(aGCListToOmmit) Then aGCListToOmmit = aGCListToOmmit.Trim() Else aGCListToOmmit = ""

            For Each prop As dxoProperty In Me
                Dim skip As Boolean = TLISTS.Contains(prop.GroupCode, aGCListToOmmit)
                If Not skip Then

                    If bUniquesOnly Then
                        If _rVal.FindIndex(Function(x) String.Compare(x, prop.Name, True) = 0) >= 0 Then
                            Continue For
                        End If
                    End If
                    _rVal.Add(prop.Name)
                End If
            Next

            Return _rVal

        End Function


        Public Function GroupCodeMembers(aGroupCode As Integer) As dxoProperties
            Return New dxoProperties(Name, FindAll(Function(x) x.GroupCode = aGroupCode))

        End Function
        Public Function NamedMembers(aName As String) As dxoProperties
            Return New dxoProperties(Name, FindAll(Function(x) String.Compare(x.Name, aName, True) = 0))

        End Function
        Public Function CopyValues(bProperties As List(Of dxoProperty), Optional bCopyNewMembers As Boolean = False, Optional aGCsToSkip As String = "", Optional bByName As Boolean = False, Optional aNamesToSkip As String = "") As Boolean
            If bProperties Is Nothing Then Return False
            Return CopyVals(bProperties, aGCsToSkip:=TLISTS.ToIntegerList(aGCsToSkip, bNoDupes:=True), bByName:=bByName, aNamesToSkip:=TLISTS.ToStringList(aNamesToSkip), bCopyNewMembers:=bCopyNewMembers, bSkipHandles:=False, bSkipPointers:=False)
        End Function

        Public Function CopyVals(bProperties As List(Of dxoProperty), Optional aGCsToSkip As List(Of Integer) = Nothing, Optional bByName As Boolean = False, Optional aNamesToSkip As List(Of String) = Nothing, Optional bCopyNewMembers As Boolean = False, Optional bSkipHandles As Boolean = True, Optional bSkipPointers As Boolean = True) As Boolean
            If bProperties Is Nothing Then Return False
            Dim _rVal As Boolean
            Dim changers As New List(Of dxoProperty)

            For Each item As dxoProperty In bProperties
                item.Mark = False
            Next
            If aGCsToSkip Is Nothing Then aGCsToSkip = New List(Of Integer)
            If aNamesToSkip Is Nothing Then aNamesToSkip = New List(Of String)
            If aNamesToSkip.FindIndex(Function(x) String.Compare(x, "*GUID", True) = 0) < 0 Then aNamesToSkip.Add("*GUID")

            If Not bByName Then
                Dim gCodes As List(Of Integer) = GroupCodes(True)


                For Each gc As Integer In gCodes
                    If aGCsToSkip.Contains(gc) Then Continue For

                    Dim bProps As List(Of dxoProperty) = bProperties.FindAll(Function(x) x.GroupCode = gc)
                    Dim myProps As List(Of dxoProperty) = FindAll(Function(x) x.GroupCode = gc)
                    Dim j As Integer = 0
                    For Each bprop As dxoProperty In bProps
                        j += 1
                        If j > myProps.Count Then Exit For
                        bprop.Mark = True
                        Dim myprop As dxoProperty = myProps(j - 1)
                        If Not myprop.CopyValue(bprop) Then Continue For

                        If myprop.PropertyType = dxxPropertyTypes.ClassMarker Then Continue For
                        If bSkipHandles And myprop.PropertyType = dxxPropertyTypes.Handle Then Continue For
                        If bSkipPointers And myprop.PropertyType = dxxPropertyTypes.Pointer Then Continue For

                        If aGCsToSkip.Contains(myprop.GroupCode) Then Continue For
                        If aNamesToSkip.FindIndex(Function(x) String.Compare(x, myprop.Name, True) = 0) >= 0 Then Continue For
                        If myprop.CopyValue(bprop) Then
                            _rVal = True
                            changers.Add(myprop)
                        End If

                    Next
                Next
            Else
                Dim mynames As List(Of String) = Names(True)

                For Each pname As String In mynames
                    If aNamesToSkip.FindIndex(Function(x) String.Compare(x, pname, True) = 0) >= 0 Then Continue For

                    Dim bProps As List(Of dxoProperty) = bProperties.FindAll(Function(x) String.Compare(x.Name, pname, True) = 0)
                    Dim myProps As List(Of dxoProperty) = FindAll(Function(x) String.Compare(x.Name, pname, True) = 0)
                    Dim j As Integer = 0
                    For Each bprop As dxoProperty In bProps
                        j += 1
                        If j > myProps.Count Then
                            Exit For
                        Else
                            bprop.Mark = True
                            Dim myprop As dxoProperty = myProps.Item(j - 1)
                            If Not myprop.CopyValue(bprop) Then Continue For

                            If bSkipHandles And myprop.PropertyType = dxxPropertyTypes.Handle Then Continue For
                            If bSkipPointers And myprop.PropertyType = dxxPropertyTypes.Pointer Then Continue For
                            If aGCsToSkip.Contains(myprop.GroupCode) Then Continue For
                            If aNamesToSkip.FindIndex(Function(x) String.Compare(x, myprop.Name, True) = 0) >= 0 Then Continue For

                            If myprop.CopyValue(bprop) Then
                                _rVal = True
                                changers.Add(myprop)
                            End If
                        End If
                    Next
                Next

            End If

            If bCopyNewMembers Then
                Append(bProperties.FindAll(Function(x) x.Mark = False), bAddClones:=True)
            End If

            For Each prop As dxoProperty In changers
                RaiseEvent PropertyChanged(prop)
            Next
            Return _rVal
        End Function
        Friend Function CopyVals(aEntry As TTABLEENTRY, Optional aGCsToSkip As List(Of Integer) = Nothing, Optional aNamesToSkip As List(Of String) = Nothing, Optional bCopyNewMembers As Boolean = False, Optional bSkipHandles As Boolean = False, Optional bSkipPointers As Boolean = False) As Boolean
            Return CopyVals(aEntry.Props, aGCsToSkip, bByName:=False, aNamesToSkip:=aNamesToSkip, bCopyNewMembers:=bCopyNewMembers, bSkipHandles:=bSkipHandles, bSkipPointers:=bSkipPointers)
        End Function

        Friend Function CopyVals(bProperties As TPROPERTIES, Optional aGCsToSkip As List(Of Integer) = Nothing, Optional bByName As Boolean = False, Optional aNamesToSkip As List(Of String) = Nothing, Optional bCopyNewMembers As Boolean = False, Optional bSkipHandles As Boolean = True, Optional bSkipPointers As Boolean = True) As Boolean
            If bProperties.Count <= 0 Then Return False
            Dim _rVal As Boolean
            Dim changers As New List(Of dxoProperty)
            Dim props As List(Of TPROPERTY) = bProperties.ToList()
            For Each item As dxoProperty In props
                item.Mark = False
            Next
            If aGCsToSkip Is Nothing Then aGCsToSkip = New List(Of Integer)
            If aNamesToSkip Is Nothing Then aNamesToSkip = New List(Of String)
            If aNamesToSkip.FindIndex(Function(x) String.Compare(x, "*GUID", True) = 0) < 0 Then aNamesToSkip.Add("*GUID")

            If Not bByName Then
                Dim gCodes As List(Of Integer) = GroupCodes(True)


                For Each gc As Integer In gCodes
                    If aGCsToSkip.Contains(gc) Then Continue For

                    Dim bProps As List(Of TPROPERTY) = props.FindAll(Function(x) x.GroupCode = gc)
                    Dim myProps As List(Of dxoProperty) = FindAll(Function(x) x.GroupCode = gc)
                    Dim j As Integer = 0
                    For Each bprop As TPROPERTY In bProps
                        j += 1
                        If j > myProps.Count Then Exit For
                        bprop.Mark = True
                        Dim myprop As dxoProperty = myProps(j - 1)
                        If Not myprop.CopyValue(bprop) Then Continue For

                        If myprop.PropertyType = dxxPropertyTypes.ClassMarker Then Continue For
                        If bSkipHandles And myprop.PropertyType = dxxPropertyTypes.Handle Then Continue For
                        If bSkipPointers And myprop.PropertyType = dxxPropertyTypes.Pointer Then Continue For

                        If aGCsToSkip.Contains(myprop.GroupCode) Then Continue For
                        If aNamesToSkip.FindIndex(Function(x) String.Compare(x, myprop.Name, True) = 0) >= 0 Then Continue For
                        If myprop.CopyValue(bprop) Then
                            _rVal = True
                            changers.Add(myprop)
                        End If

                    Next
                Next
            Else
                Dim mynames As List(Of String) = Names(True)

                For Each pname As String In mynames
                    If aNamesToSkip.FindIndex(Function(x) String.Compare(x, pname, True) = 0) >= 0 Then Continue For

                    Dim bProps As List(Of TPROPERTY) = props.FindAll(Function(x) String.Compare(x.Name, pname, True) = 0)
                    Dim myProps As List(Of dxoProperty) = FindAll(Function(x) String.Compare(x.Name, pname, True) = 0)
                    Dim j As Integer = 0
                    For Each bprop As dxoProperty In bProps
                        j += 1
                        If j > myProps.Count Then
                            Exit For
                        Else
                            bprop.Mark = True
                            Dim myprop As dxoProperty = myProps.Item(j - 1)
                            If Not myprop.CopyValue(bprop) Then Continue For

                            If bSkipHandles And myprop.PropertyType = dxxPropertyTypes.Handle Then Continue For
                            If bSkipPointers And myprop.PropertyType = dxxPropertyTypes.Pointer Then Continue For
                            If aGCsToSkip.Contains(myprop.GroupCode) Then Continue For
                            If aNamesToSkip.FindIndex(Function(x) String.Compare(x, myprop.Name, True) = 0) >= 0 Then Continue For

                            If myprop.CopyValue(bprop) Then
                                _rVal = True
                                changers.Add(myprop)
                            End If
                        End If
                    Next
                Next

            End If

            If bCopyNewMembers Then
                Append(props.FindAll(Function(x) x.Mark = False), bAddClones:=True)
            End If

            For Each prop As dxoProperty In changers
                RaiseEvent PropertyChanged(prop)
            Next
            Return _rVal
        End Function

        Public Function CopyVals(aGroupCodes As List(Of Integer), bProperties As List(Of dxoProperty)) As Boolean
            If bProperties Is Nothing Or aGroupCodes Is Nothing Then Return False
            Dim _rVal As Boolean
            Dim changers As New List(Of dxoProperty)
            Dim gCodes As New List(Of Integer)

            For Each gc As Integer In aGroupCodes
                If gCodes.Contains(gc) Then Continue For
                gCodes.Add(gc) ' only do them once

                Dim bProps As List(Of dxoProperty) = bProperties.FindAll(Function(x) x.GroupCode = gc And Not x.Hidden)
                Dim myProps As List(Of dxoProperty) = FindAll(Function(x) x.GroupCode = gc And Not x.Hidden)
                Dim j As Integer = 0
                For Each bprop As dxoProperty In bProps
                    j += 1
                    If j > myProps.Count Then Exit For

                    Dim myprop As dxoProperty = myProps(j - 1)
                    If myprop.CopyValue(bprop) Then
                        _rVal = True
                        changers.Add(myprop)
                    End If
                Next
            Next

            For Each prop As dxoProperty In changers
                RaiseEvent PropertyChanged(prop)
            Next


            Return _rVal
        End Function



        Friend Function CopyValues(bProperties As TPROPERTIES, Optional bCopyNewMembers As Boolean = False, Optional aGCsToSkip As String = "", Optional bByName As Boolean = False, Optional aNamesToSkip As String = "") As Boolean
            Return CopyValues(New dxoProperties(bProperties), bCopyNewMembers, aGCsToSkip, bByName, aNamesToSkip)
        End Function

        Public Function SetDirection(aName As String, aDirection As dxfDirection, Optional aOccur As Integer = 1) As Boolean

            Return SetVector(aName, New TVECTOR(aDirection), True, aOccur)
        End Function

        Friend Function SetVector(aName As String, aVector As iVector, Optional aOccur As Integer = 1, Optional bDirection As Boolean = False) As Boolean

            Return SetVector(aName, New TVECTOR(aVector), bDirection, aOccur)
        End Function
        Friend Function SetVector(aGroupCode As Integer, aVector As iVector, Optional bDirection As Boolean = False, Optional aOccur As Integer = 1) As Boolean

            Return SetVector(aGroupCode, New TVECTOR(aVector), bDirection, aOccur)
        End Function
        Public Function SetVector(aName As String, aVector As iVector, bSuppressed As Boolean?, bDirection As Boolean, ByRef rLastValue As dxfVector, Optional aOccur As Integer = 1) As Boolean


            Return SetVector(aName, New TVECTOR(aVector), bSuppressed, bDirection, rLastValue, aOccur)

        End Function
        Public Function SetVector(aGroupCode As Integer, aVector As iVector, bSuppressed As Boolean?, bDirection As Boolean, ByRef rLastValue As dxfVector, Optional aOccur As Integer = 1) As Boolean


            Return SetVector(aGroupCode, New TVECTOR(aVector), bSuppressed, bDirection, rLastValue, aOccur)

        End Function
        Friend Function SetVector(aName As String, aVector As TVECTOR, Optional bDirection As Boolean = False, Optional aOccur As Integer = 1, Optional bDirtyOnChange As Boolean = False, Optional bSuppressEvents As Boolean = False) As Boolean

            Dim rLastValue As dxfVector = Nothing
            Dim xMem As dxoProperty = Nothing

            If Not TryGet(aName, xMem, aOccur) Then Return False
            Return SetVector(aFirstMember:=xMem, aVector:=aVector, bSuppressed:=Nothing, bDirection:=bDirection, rLastValue:=rLastValue, bDirtyOnChange:=bDirtyOnChange, bSuppressEvents:=bSuppressEvents)

        End Function
        Friend Function SetVector(aGroupCode As Integer, aVector As TVECTOR, Optional bDirection As Boolean = False, Optional aOccur As Integer = 1, Optional bDirtyOnChange As Boolean = False, Optional bSuppressEvents As Boolean = False) As Boolean


            Dim xMem As dxoProperty = Nothing
            If Not TryGet(aGroupCode, xMem, aOccur) Then Return False

            Return SetVector(aFirstMember:=xMem, aVector:=aVector, bSuppressed:=Nothing, bDirection:=bDirection, rLastValue:=Nothing, bDirtyOnChange:=bDirtyOnChange, bSuppressEvents:=bSuppressEvents)


        End Function

        Friend Function SetVector(aGroupCode As Integer, aVector As TVECTOR, bSuppressed As Boolean?, bDirection As Boolean, ByRef rLastValue As dxfVector, Optional aOccur As Integer = 1, Optional bDirtyOnChange As Boolean = False, Optional bSuppressEvents As Boolean = False) As Boolean
            rLastValue = Nothing
            Dim xMem As dxoProperty = Nothing
            If Not TryGet(aGroupCode, xMem, aOccur) Then Return False

            Return SetVector(aFirstMember:=xMem, aVector:=aVector, bSuppressed:=bSuppressed, bDirection:=bDirection, rLastValue:=rLastValue, bDirtyOnChange:=bDirtyOnChange, bSuppressEvents:=bSuppressEvents)
        End Function

        Friend Function SetVector(aName As String, aVector As TVECTOR, bSuppressed As Boolean?, bDirection As Boolean, ByRef rLastValue As dxfVector, Optional aOccur As Integer = 1, Optional bDirtyOnChange As Boolean = False, Optional bSuppressEvents As Boolean = False) As Boolean
            rLastValue = Nothing
            Dim xMem As dxoProperty = Nothing

            If Not TryGet(aName, xMem, aOccur) Then Return False
            Return SetVector(aFirstMember:=xMem, aVector:=aVector, bSuppressed:=bSuppressed, bDirection:=bDirection, rLastValue:=rLastValue, bDirtyOnChange:=bDirtyOnChange, bSuppressEvents:=bSuppressEvents)


        End Function

        Private Function SetVector(aFirstMember As dxoProperty, aVector As TVECTOR, bSuppressed As Boolean?, bDirection As Boolean, ByRef rLastValue As dxfVector, Optional bDirtyOnChange As Boolean = False, Optional bSuppressEvents As Boolean = False) As Boolean

            rLastValue = Nothing
            If aFirstMember Is Nothing Then Return False

            Dim nevec As New TVECTOR(aVector)
            If bDirection Then nevec.Normalize()

            Dim _rVal As Boolean = False
            rLastValue = TVECTOR.Zero


            Dim idx As Integer = IndexOf(aFirstMember)
            If bSuppressed.HasValue Then aFirstMember.Suppressed = bSuppressed.Value
            If aFirstMember.IsHeader Then idx += 1

            aFirstMember = Item(idx)
            If aFirstMember Is Nothing Then Return False
            If bSuppressed.HasValue Then aFirstMember.Suppressed = bSuppressed.Value
            If aFirstMember.PropertyType <> dxxPropertyTypes.dxf_Double Then Return False

            rLastValue.X = aFirstMember.ValueD
            If rLastValue.X <> aVector.X Then _rVal = True
            aFirstMember.Value = aVector.X

            Dim yMem As dxoProperty = Item(idx + 1)

            If yMem IsNot Nothing Then
                If yMem.PropertyType = dxxPropertyTypes.dxf_Double And yMem.GroupCode = aFirstMember.GroupCode + 10 Then

                    If bSuppressed.HasValue Then yMem.Suppressed = bSuppressed.Value

                    rLastValue.Y = yMem.ValueD
                    If rLastValue.Y <> aVector.Y Then _rVal = True

                    yMem.Value = aVector.Y
                End If

            End If


            Dim zMem As dxoProperty = Item(idx + 2)
            If zMem IsNot Nothing Then
                If zMem.PropertyType = dxxPropertyTypes.dxf_Double And zMem.GroupCode = aFirstMember.GroupCode + 20 Then

                    If bSuppressed.HasValue Then zMem.Suppressed = bSuppressed.Value

                    rLastValue.Z = zMem.ValueD
                    If rLastValue.Z <> aVector.Z Then _rVal = True
                    zMem.Value = aVector.Z
                End If

            End If

            If _rVal And bDirtyOnChange Then IsDirty = True
            If Not bSuppressEvents Then
                Dim changedProp As New dxoProperty(aFirstMember.GroupCode, aVector.Coordinates(5), aFirstMember.Name, dxxPropertyTypes.dxf_String, rLastValue.CoordinatesR(5))
                RaiseEvent PropertyChanged(changedProp)
            End If
            Return _rVal
        End Function


        Public Function GroupCode(aName As Integer, Optional aOccur As Integer = 1) As Integer
            Dim prop As dxoProperty = Nothing
            If Not TryGet(aName, rMember:=prop, aOccur:=aOccur) Then Return prop.GroupCode Else Return -1
        End Function
        Public Function SetVal(aName As String, aValue As Object, Optional aOccur As Integer = 1, Optional bSuppress As Boolean? = Nothing, Optional bIgnoreValueControl As Boolean = False, Optional bDirtyOnChange As Boolean = False, Optional bSuppressEvents As Boolean = False, Optional aPrefixsToConsider As List(Of String) = Nothing) As Boolean
            Dim rLastValue As Object = Nothing
            If aValue Is Nothing Then Return False
            Return SetVal(aName, aValue, rLastValue, bSuppress, bIgnoreValueControl, aOccur, bDirtyOnChange, bSuppressEvents:=bSuppressEvents, aPrefixsToConsider:=aPrefixsToConsider)
        End Function

        Public Function SetVal(aName As String, aValue As Object, ByRef rLastValue As Object, Optional bSuppress As Boolean? = Nothing, Optional bIgnoreValueControl As Boolean = False, Optional aOccur As Integer = 1, Optional bDirtyOnChange As Boolean = False, Optional bSuppressEvents As Boolean = False, Optional aPrefixsToConsider As List(Of String) = Nothing) As Boolean
            rLastValue = Nothing
            If aValue Is Nothing Then Return False
            Dim aMem As dxoProperty = Nothing
            If Not TryGet(aName, aMem, aOccur, aPrefixsToConsider) Then Return False
            rLastValue = aMem.Value
            Return SetVal(aMem, aValue, bSuppress:=bSuppress, bIgnoreValueControl:=bIgnoreValueControl, bDirtyOnChange:=bDirtyOnChange, bSuppressEvents:=bSuppressEvents)

        End Function

        Public Function SetVal(aGroupCode As Integer, aValue As Object, Optional aOccur As Integer = 1, Optional bSuppress As Boolean? = Nothing, Optional bIgnoreValueControl As Boolean = False, Optional bDirtyOnChange As Boolean = False, Optional bSuppressEvents As Boolean = False) As Boolean
            If aValue Is Nothing Then Return False
            Dim aMem As dxoProperty = Nothing
            If Not TryGet(aGroupCode, aMem, aOccur) Then Return False
            Return SetVal(aMem, aValue, bSuppress:=bSuppress, bIgnoreValueControl:=bIgnoreValueControl, bDirtyOnChange:=bDirtyOnChange, bSuppressEvents:=bSuppressEvents)

        End Function

        Friend Function SetVal(aMember As dxoProperty, aValue As Object, Optional bSuppress As Boolean? = Nothing, Optional bIgnoreValueControl As Boolean = False, Optional bDirtyOnChange As Boolean = False, Optional bSuppressEvents As Boolean = False) As Boolean
            If aMember Is Nothing Or aValue Is Nothing Then Return False

            Dim idx As Integer = IndexOf(aMember)
            Dim _rVal As Boolean = False


            Dim rLastValue As Object = Nothing
            If dxfVectors.TypeIsVector(aValue) Then
                If (idx > 0) Then _rVal = SetVector(aMember, dxfVectors.GetVector(aValue), bSuppress, False, rLastValue, bDirtyOnChange)
            Else
                rLastValue = aMember.Value
                _rVal = aMember.SetVal(aValue, bSuppress:=bSuppress, bIgnoreValueControl:=bIgnoreValueControl)
            End If
            If _rVal Then
                If bDirtyOnChange And idx > 0 Then IsDirty = True
                If Not bSuppressEvents And idx > 0 Then RaiseEvent PropertyChanged(aMember)
            End If
            Return _rVal
        End Function

        Friend Function ExtractExtendedData(Optional bDontRemove As Boolean = False) As dxoPropertyArray


            Dim aProps As dxoProperties

            Dim _rVal As New dxoPropertyArray("ExtendedData")
            Dim markers As List(Of dxoProperty) = FindAll(Function(x) x.GroupCode = 1001)
            If markers.Count <= 0 Then Return _rVal
            For Each marker As dxoProperty In markers
                aProps = New dxoProperties(marker.StringValue)
                aProps.Add(marker)
                Dim si As Integer = IndexOf(marker)
                Dim ei As Integer = si

                For j As Integer = si + 1 To Count
                    Dim prop As dxoProperty = Item(j)
                    If prop.GroupCode = 0 Or prop.StringValue = "}" Then
                        ei = j
                        _rVal.Add(aProps)
                        Exit For
                    Else
                        aProps.Add(prop)
                    End If
                Next
            Next
            If bDontRemove Then Return _rVal

            For Each aProps In _rVal
                For Each prop As dxoProperty In aProps
                    Remove(prop)
                Next
            Next

            Return _rVal
        End Function

        Friend Function CopyDisplayProps(aSet As TDISPLAYVARS) As Boolean
            If Count <= 0 Then Return False

            Dim _rVal As Boolean = False
            If SetVal(dxfLinetypes.Invisible, aSet.Suppressed) Then _rVal = True
            If aSet.Color <> dxxColors.Undefined Then
                If SetVal("Color", aSet.Color) Then _rVal = True
            End If
            If Not String.IsNullOrWhiteSpace(aSet.LayerName) Then
                If SetVal("LayerName", aSet.LayerName) Then _rVal = True
            End If
            If Not String.IsNullOrWhiteSpace(aSet.Linetype) Then
                If SetVal("LineType", aSet.Linetype) Then _rVal = True
            End If
            If aSet.LTScale > 0 Then
                If SetVal("LTScale", aSet.LTScale) Then _rVal = True
            End If
            If aSet.LineWeight <> dxxLineWeights.Undefined Then
                If SetVal("LineWeight", aSet.LineWeight) Then _rVal = True
            End If
            Return _rVal
        End Function

        ''' <summary>
        ''' used to see if one properties collection matches another
        ''' </summary>
        ''' <param name="bProperties">the properties to compare with</param>
        ''' <param name="aGCSkipList">a list of group codes to exclude from the comparison</param>
        ''' <param name="aNameSkipList">a list of names  to exclude from the comparison</param>
        ''' <returns></returns>
        Public Function IsEqual(bProperties As IEnumerable(Of dxoProperty), Optional aGCSkipList As String = "", Optional aNameSkipList As String = "") As Boolean

            Return dxoProperties.Compare(Me, bProperties, aGCSkipList, aNameSkipList)

        End Function


        Friend Function PropValueI(aIndex As [Enum], Optional aOccur As Integer = 0, Optional bAbsVal As Boolean = False) As Integer
            Dim prop As dxoProperty = Nothing
            If Not TryGet(dxfEnums.PropertyName(aIndex), prop, aOccur) Then
                Return 0
            End If
            If Not bAbsVal Then Return prop.ValueI Else Return Math.Abs(prop.ValueI)

        End Function


        Friend Function PropValueStr(aIndex As [Enum], Optional aOccur As Integer = 0) As String
            Dim prop As dxoProperty = Nothing
            If Not TryGet(dxfEnums.PropertyName(aIndex), prop, aOccur) Then
                Return String.Empty
            End If
            Return prop.ValueS

        End Function

        Friend Function PropValueB(aIndex As [Enum], Optional aOccur As Integer = 0) As Boolean
            Dim prop As dxoProperty = Nothing
            If Not TryGet(dxfEnums.PropertyName(aIndex), prop, aOccur) Then
                Return False
            End If
            Return prop.ValueB
        End Function


        Friend Function PropValueD(aIndex As [Enum], Optional aOccur As Integer = 0) As Double
            Dim prop As dxoProperty = Nothing
            If Not TryGet(dxfEnums.PropertyName(aIndex), prop, aOccur) Then
                Return 0
            End If
            Return prop.ValueD
        End Function

        Friend Overridable Function PropValueSet(aIndex As [Enum], aValue As Object, Optional aOccur As Integer = 0, Optional bSuppressEvnts As Boolean = False) As Boolean
            Dim rFound As Boolean = False
            Dim rProp As TPROPERTY = TPROPERTY.Null
            Return PropValueSet(aIndex, aValue, aOccur, rFound, rProp, bSuppressEvnts)
        End Function
        Friend Overridable Function PropValueSet(aIndex As [Enum], aValue As Object, aOccur As Integer, ByRef rFound As Boolean, ByRef rProp As TPROPERTY, Optional bSuppressEvnts As Boolean = False, Optional bDirtyOnChange As Boolean = False) As Boolean

            rFound = False
            rProp = TPROPERTY.Null

            Try
                Dim pname As String = dxfEnums.PropertyName(aIndex)
                Dim myprop As dxoProperty = Nothing
                rFound = TryGet(pname, myprop, aOccur)


                If Not rFound Then
                    Return False
                Else
                    rProp = New TPROPERTY(myprop)
                    Return SetVal(myprop, aValue, bDirtyOnChange:=bDirtyOnChange)
                End If
            Catch ex As Exception
                'add an error
                Return False

            End Try
        End Function

        Public Function GetByPropertyValue(aGC As Integer, aValue As Object, aStringCompare As Boolean, Optional aOccur As Integer = 0, Optional aSecondaryValue As Object = Nothing) As dxoProperty
            Dim props As List(Of dxoProperty) = FindAll((Function(x) x.GroupCode = aGC))
            If aValue Is Nothing Or props.Count <= 0 Then Return Nothing
            Dim prop As dxoProperty
            If aOccur <= 0 Then
                prop = props(0)
            Else
                If aOccur > props.Count Then Return Nothing Else prop = props(aOccur - 1)
            End If
            If Not aStringCompare Then
                If prop.Value = aValue Then Return prop
                If aSecondaryValue IsNot Nothing Then
                    If prop.Value = aSecondaryValue Then Return prop Else Return Nothing
                Else
                    Return Nothing
                End If
            Else
                If String.Compare(prop.Value, aValue, True) = 0 Then Return prop
                If aSecondaryValue IsNot Nothing Then
                    If String.Compare(prop.Value, aSecondaryValue, True) = 0 Then Return prop Else Return Nothing
                Else
                    Return Nothing
                End If
            End If
            Return Nothing
        End Function
        Public Function GetByPropertyValue(aName As String, aValue As Object, aStringCompare As Boolean, Optional aOccur As Integer = 0, Optional aSecondaryValue As Object = Nothing) As dxoProperty
            Dim props As List(Of dxoProperty) = FindAll((Function(x) String.Compare(x.Name, aName, True) = 0))
            If aValue Is Nothing Or props.Count <= 0 Then Return Nothing
            Dim prop As dxoProperty
            If aOccur <= 0 Then
                prop = props(0)
            Else
                If aOccur > props.Count Then Return Nothing Else prop = props(aOccur - 1)
            End If
            If Not aStringCompare Then
                If prop.Value = aValue Then Return prop
                If aSecondaryValue IsNot Nothing Then
                    If prop.Value = aSecondaryValue Then Return prop Else Return Nothing
                Else
                    Return Nothing
                End If
            Else
                If String.Compare(prop.Value, aValue, True) = 0 Then Return prop
                If aSecondaryValue IsNot Nothing Then
                    If String.Compare(prop.Value, aSecondaryValue, True) = 0 Then Return prop Else Return Nothing
                Else
                    Return Nothing
                End If
            End If
            Return Nothing
        End Function
        Public Function NamesByGroupCode(aGroupCodes As List(Of Integer), Optional bHidden As Boolean? = False) As List(Of String)
            If aGroupCodes Is Nothing Then Return New List(Of String)
            Dim _rVal As New List(Of String)

            Dim gCodes As New List(Of Integer)

            For Each gc As Integer In aGroupCodes
                If gCodes.Contains(gc) Then Continue For
                gCodes.Add(gc) ' only do them once
                Dim myProps As List(Of dxoProperty)
                If bHidden.HasValue Then
                    myProps = FindAll(Function(x) x.GroupCode = gc And x.Hidden = bHidden.Value)
                Else
                    myProps = FindAll(Function(x) x.GroupCode = gc)
                End If
                For Each bprop As dxoProperty In myProps
                    _rVal.Add(bprop.Name)
                Next
            Next



            Return _rVal
        End Function

        Public Function SetSuppressionByGC(aGroupCodeList As List(Of Integer), aSuppressValue As Boolean, Optional bToggleNonMembers As Boolean = False, Optional aSkipList As List(Of String) = Nothing, Optional aStartID As Integer = 0) As Integer
            Dim _rVal As Integer
            If aGroupCodeList Is Nothing Then Return 0

            Dim i As Integer
            Dim aGC As Integer

            Dim bListPassed As Boolean = aSkipList IsNot Nothing
            Dim si As Integer
            Dim cnt As Integer = Count
            Dim aMem As dxoProperty
            If aStartID > 1 Then si = aStartID Else si = 1
            If bListPassed Then bListPassed = aSkipList.Count > 0
            For i = si To cnt
                aMem = Item(i)
                aGC = aMem.GroupCode

                If aMem.PropertyType = dxxPropertyTypes.Handle Then Continue For
                If bListPassed Then
                    If aSkipList.FindIndex(Function(x) String.Compare(x, aMem.Name, True) = 0) >= 0 Then Continue For
                End If

                If aGroupCodeList.Contains(aGC) Then
                    If aMem.Suppressed <> aSuppressValue Then _rVal += 1
                    aMem.Suppressed = aSuppressValue
                Else
                    If bToggleNonMembers Then
                        If aMem.Suppressed = aSuppressValue Then _rVal += 1
                        aMem.Suppressed = Not aSuppressValue
                    End If
                End If


            Next i
            Return _rVal
        End Function
#End Region 'Methods

#Region "Shared Methods"
        ''' <summary>
        ''' used to see if one properties collection matches another
        ''' </summary>
        ''' <param name="aProperties">the properties to compare to</param>
        ''' <param name="bProperties">the properties to compare with</param>
        ''' <param name="aSkipList">a list of group codes to not include in the comparison</param>
        ''' <returns></returns>
        Friend Shared Function IsEqual(aProperties As TPROPERTIES, bProperties As TPROPERTIES, Optional aSkipList As String = "") As Boolean


            Dim bProp As TPROPERTY = TPROPERTY.Null
            'aCodes = props_GroupCodes(aProperties, bProperties, bCodes)
            Dim aCodes As List(Of Integer) = aProperties.GroupCodes(True, aSkipList)
            For Each iGC As Integer In aCodes
                Dim aProps As List(Of TPROPERTY) = aProperties.GroupCodeMembers(iGC)
                For k As Integer = 1 To aProps.Count
                    Dim aProp As TPROPERTY = aProps.Item(k - 1)
                    If Not bProperties.TryGet(iGC, bProp, aOccur:=k) Then Return False
                    If Not aProp.Compare(bProp) Then Return False
                Next k
            Next
            Return True

        End Function
        ''' <summary>
        ''' used to see if one properties collection matches another
        ''' </summary>
        ''' <param name="aProperties">the properties to compare to</param>
        ''' <param name="bProperties">the properties to compare with</param>
        ''' <param name="aGCSkipList">a list of group codes to not include in the comparison</param>
        ''' <param name="aNameSkipList">a list of names  to not include in the comparison</param>
        ''' <returns></returns>
        Public Shared Function Compare(aProperties As IEnumerable(Of dxoProperty), bProperties As IEnumerable(Of dxoProperty), Optional aGCSkipList As String = "", Optional aNameSkipList As String = "") As Boolean

            If aProperties Is Nothing And bProperties Is Nothing Then Return True
            If aProperties Is Nothing And bProperties IsNot Nothing Then Return False
            If aProperties IsNot Nothing And bProperties Is Nothing Then Return False


            'aCodes = props_GroupCodes(aProperties, bProperties, bCodes)
            Dim aCodes As List(Of Integer) = GetGroupCodes(aProperties, True, aGCSkipList)
            Dim skippers As List(Of String) = TLISTS.ToStringList(aNameSkipList, bTrim:=True, bNoDupes:=True, bUCase:=True)
            For Each iGC As Integer In aCodes
                Dim aProps As List(Of dxoProperty) = GetByGroupCode(aProperties, iGC)
                Dim bProps As List(Of dxoProperty) = GetByGroupCode(bProperties, iGC)
                For k As Integer = 1 To aProps.Count
                    Dim aProp As dxoProperty = aProps(k - 1)
                    If skippers.Contains(aProp.Name.ToUpper) Then Continue For
                    If k > bProps.Count Then Return False
                    Dim bProp As dxoProperty = bProps(k - 1)
                    If bProp Is Nothing Then Return False
                    If Not aProp.Compare(bProp) Then Return False
                Next k
            Next
            Return True

        End Function


        Public Shared Function GetByGroupCode(aProperties As IEnumerable(Of dxoProperty), aGroupCode As Integer) As List(Of dxoProperty)
            If aProperties Is Nothing Then Return New List(Of dxoProperty)
            Return aProperties.ToList().FindAll(Function(x) x.GroupCode = aGroupCode)
        End Function

        Public Shared Function GetGroupCodes(aProperties As IEnumerable(Of dxoProperty), Optional bUniqueValues As Boolean = True, Optional aGCSkipList As String = "", Optional aNamesSkipList As String = "") As List(Of Integer)
            Dim _rVal As New List(Of Integer)
            If aProperties Is Nothing Then Return _rVal
            Dim gcskippers As List(Of String) = TLISTS.ToStringList(aGCSkipList, bReturnNulls:=False, bTrim:=True, bNoDupes:=True)
            Dim nameskippers As List(Of String) = TLISTS.ToStringList(aNamesSkipList, bReturnNulls:=False, bTrim:=True, bNoDupes:=True)
            Dim p1 As List(Of dxoProperty) = aProperties.ToList()
            For Each pname As String In nameskippers
                p1.RemoveAll(Function(x) String.Compare(x.Name, pname, True) = 0)
            Next

            For Each prop In aProperties
                If gcskippers.Contains(prop.GroupCode.ToString()) Then Continue For
                If bUniqueValues Then
                    If _rVal.Contains(prop.GroupCode) Then Continue For
                End If

                _rVal.Add(prop.GroupCode)
            Next

            Return _rVal
        End Function

        Public Shared Function GetNames(aProperties As IEnumerable(Of dxoProperty), Optional bUniqueValues As Boolean = True, Optional aGCSkipList As String = "", Optional aNamesSkipList As String = "") As List(Of String)
            Dim _rVal As New List(Of String)
            If aProperties Is Nothing Then Return _rVal
            Dim gcskippers As List(Of String) = TLISTS.ToStringList(aGCSkipList, bReturnNulls:=False, bTrim:=True, bNoDupes:=True)
            Dim nameskippers As List(Of String) = TLISTS.ToStringList(aNamesSkipList, bReturnNulls:=False, bTrim:=True, bNoDupes:=True)
            Dim p1 As List(Of dxoProperty) = aProperties.ToList()
            For Each gcstr As String In gcskippers
                p1.RemoveAll(Function(x) String.Compare(x.GroupCode.ToString(), gcstr, True) = 0)
            Next

            For Each prop In aProperties
                If nameskippers.Contains(prop.Name) Then Continue For
                If bUniqueValues Then
                    If _rVal.Contains(prop.Name) Then Continue For
                End If

                _rVal.Add(prop.Name)
            Next

            Return _rVal
        End Function

        Public Shared Function GetKeys(aProperties As IEnumerable(Of dxoProperty), Optional bExcludeHandles As Boolean = False, Optional bExcludePointers As Boolean = False, Optional bIncludeHiddenProps As Boolean = False, Optional aPrefixFilter As String = Nothing, Optional bExcludeDoNotCopy As Boolean = True, Optional aGCSkipList As List(Of Integer) = Nothing) As List(Of String)
            Dim _rVal As New List(Of String)
            If aProperties Is Nothing Then Return _rVal
            Dim props As List(Of dxoProperty) = aProperties.ToList()
            If Not bIncludeHiddenProps Then props.RemoveAll(Function(x) x.Hidden)
            If bExcludeHandles Then props.RemoveAll(Function(x) x.IsHandle)
            If bExcludePointers Then props.RemoveAll(Function(x) x.IsPointer)
            If bExcludeDoNotCopy Then
                props.RemoveAll(Function(x) x.DoNotCopy)
            End If
            If aGCSkipList IsNot Nothing Then
                For Each gc As Integer In aGCSkipList
                    props.RemoveAll(Function(x) x.GroupCode = gc)
                Next
            End If
            If Not String.IsNullOrWhiteSpace(aPrefixFilter) Then
                props.RemoveAll(Function(x) Not x.Key.ToUpper().StartsWith(aPrefixFilter.ToUpper()))
            End If

            For Each prop As dxoProperty In props
                _rVal.Add(prop.Key)
            Next

            Return _rVal

        End Function
#End Region 'Shared Methods
    End Class 'dxoProperties
End Namespace
