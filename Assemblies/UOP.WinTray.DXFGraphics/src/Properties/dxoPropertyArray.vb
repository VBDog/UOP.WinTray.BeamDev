Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public Class dxoPropertyArray
        Inherits List(Of dxoProperties)
        Implements IEnumerable(Of dxoProperties)
        Implements ICloneable
#Region "Members"
        Public Instance As Integer
        Public Name As String
        Public Owner As String
#End Region 'Members

        Public Sub New()
            MyBase.New()
        End Sub

        Public Sub New(aName As String)
            MyBase.New()
            Name = aName
        End Sub
        Friend Sub New(aStructure As TPROPERTYARRAY, Optional aName As String = Nothing)
            MyBase.New()
            Structure_Set(aStructure)
            If Not String.IsNullOrWhiteSpace(aName) Then Name = aName.Trim

        End Sub
        Friend Sub New(aProperties As IEnumerable(Of dxoProperties))
            MyBase.New()
            If aProperties Is Nothing Then Return
            If TypeOf aProperties Is dxoPropertyArray Then
                Dim dxf As dxoPropertyArray = DirectCast(aProperties, dxoPropertyArray)
                Name = dxf.Name
                Instance = dxf.Instance
                Owner = dxf.Owner
            End If
            Append(aProperties)
        End Sub
        Friend Sub New(aProperties As dxoProperties)
            MyBase.New()
            If aProperties Is Nothing Then Return

            Add(aProperties)
        End Sub
#Region "Properties"


#End Region 'Properties
#Region "Methods"

        Public Function AddReactor(aArrayName As String, aGroupCode As Integer, aValue As String, Optional bPointer As Boolean = True, Optional bDontAddArray As Boolean = False, Optional aPropName As String = "") As Boolean
            If String.IsNullOrWhiteSpace(aArrayName) Or String.IsNullOrWhiteSpace(aValue) Then Return False
            aArrayName = aArrayName.Trim()

            Dim idx As Integer

            If String.IsNullOrWhiteSpace(aPropName) Then aPropName = "Reactor"
            aPropName = aPropName.Trim()
            Dim aProps As dxoProperties = Item(aArrayName)

            If aProps Is Nothing Then
                If bDontAddArray Then
                    Return False
                Else
                    aProps = New dxoProperties(aArrayName)
                    Add(aProps, aArrayName)
                    idx = Count
                End If
            End If
            Dim pidx As Integer = aProps.FindIndex(Function(x) x.GroupCode = aGroupCode And String.Compare(x.ValueS, aValue, True) = 0) + 1

            If pidx <= 0 Then
                If bPointer Then
                    aProps.Add(New dxoProperty(aGroupCode, aValue, aPropName, dxxPropertyTypes.Pointer))
                    Return True
                Else
                    aProps.Add(New dxoProperty(aGroupCode, aValue, aPropName, dxxPropertyTypes.Undefined))
                    Return True
                End If
            End If
            Return False
        End Function

        Public Shadows Sub Append(aProperties As IEnumerable(Of dxoProperties), Optional bAddClones As Boolean = False, Optional bClear As Boolean = False)
            If bClear Then Clear()
            If aProperties Is Nothing Then Return

            For Each props As dxoProperties In aProperties
                If props Is Nothing Then Continue For
                Dim newprops As dxoProperties = props
                If String.IsNullOrWhiteSpace(newprops.Name) Then newprops.Name = $"GROUP {Count + 1}"
                Dim existing As dxoProperties = Item(newprops.Name)
                If existing Is Nothing Then
                    If Not bAddClones Then Add(newprops) Else Add(New dxoProperties(newprops))
                Else
                    existing.Append(newprops, bAddClones)
                End If

            Next

        End Sub

        Friend Shadows Sub Append(aProperties As TPROPERTYARRAY, Optional bAddClones As Boolean = False, Optional bClear As Boolean = False)
            If bClear Then Clear()
            If aProperties.Count Then Return
            Append(New dxoPropertyArray(aProperties), bAddClones, bClear)
        End Sub

        Public Shadows Sub Add(aProperties As dxoProperties, Optional aName As String = "")
            If aProperties Is Nothing Then Return
            If Not String.IsNullOrWhiteSpace(aName) Then aProperties.Name = aName.Trim()
            If String.IsNullOrWhiteSpace(aProperties.Name) Then aProperties.Name = $"Properties_{Count + 1}"
            MyBase.Add(aProperties)

        End Sub
        Public Function ClearMember(aArrayName As String, Optional bAddIfNotFound As Boolean = False) As Boolean
            Dim _rVal As Boolean
            Dim aProps As dxoProperties = Item(aArrayName)
            If aProps Is Nothing And bAddIfNotFound Then
                aProps = New dxoProperties(aArrayName)
                Add(aProps)
            End If
            If aProps IsNot Nothing Then
                _rVal = aProps.Count <> 0
                aProps.Clear()
            End If
            Return _rVal
        End Function

        Public Shadows Function Item(aIndex As Integer) As dxoProperties
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return Nothing
            End If

            Return MyBase.Item(aIndex - 1)
        End Function
        Public Function RemoveMemberPropertiesByStringValue(aName As String, aStringValue As String, Optional bIgnoreCase As Boolean = True, Optional bAddIfNotFound As Boolean = False) As List(Of dxoProperty)
            Dim mems As dxoProperties = Member(aName, bAddIfNotFound)
            If mems Is Nothing Then Return New List(Of dxoProperty)() Else Return mems.RemoveByStringValue(aStringValue, bIgnoreCase)
        End Function
        Public Shadows Function Item(aName As String) As dxoProperties
            If String.IsNullOrWhiteSpace(aName) Then Return Nothing
            Return Find(Function(x) String.Compare(x.Name, aName, True) = 0)
        End Function

        Public Function Member(aName As String, Optional bAddIfNotFound As Boolean = False) As dxoProperties
            If String.IsNullOrWhiteSpace(aName) Then Return Nothing
            Dim _rVal As dxoProperties = Item(aName)
            If _rVal Is Nothing And bAddIfNotFound Then Add(New dxoProperties(aName))
            Return _rVal
        End Function

        Friend Function Structure_Get() As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY With
            {
            .Name = Name,
            .Instance = Instance,
            .Owner = Owner
            }
            For Each mem As dxoProperties In Me
                _rVal.Add(New TPROPERTIES(mem))
            Next
            Return _rVal
        End Function

        Friend Sub Structure_Set(aStructure As TPROPERTYARRAY)
            Clear()
            Name = aStructure.Name
            Instance = aStructure.Instance
            Owner = aStructure.Owner
            For i As Integer = 1 To aStructure.Count
                Add(New dxoProperties(aStructure.Item(i)))
            Next
        End Sub

        Public Function Clone() As Object Implements ICloneable.Clone
            Return New dxoPropertyArray(Me)
        End Function
#End Region 'Methods


    End Class 'dxoPropertyArray
End Namespace
