Namespace UOP.DXFGraphics

    Public Class dxoViewPorts
        Inherits dxfTable
        Implements IEnumerable(Of dxoViewPort)
        Implements ICloneable

        Public Sub New()
            MyBase.New(dxxReferenceTypes.VPORT)
        End Sub

        Public Sub New(bAddDefaultMembers As Boolean)
            MyBase.New(dxxReferenceTypes.VPORT)
            If bAddDefaultMembers Then dxfTable.LoadDefaultMembers(Me)
        End Sub

        Public Sub New(bAddDefaultMembers As Boolean, aImage As dxfImage)
            MyBase.New(dxxReferenceTypes.VPORT)
            If bAddDefaultMembers Then dxfTable.LoadDefaultMembers(Me, aImage)
        End Sub

        Public Sub New(aTable As dxoViewPorts)
            MyBase.New(dxxReferenceTypes.VPORT, aTable)
        End Sub

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New dxoViewPorts(Me)
        End Function
        Public Function Clone() As dxoViewPorts
            Return New dxoViewPorts(Me)
        End Function
        Public Overloads Sub Add(aEntry As dxoViewPort)
            AddEntry(aEntry)
        End Sub

        Public Function Member(aNameOrHandleOrGUID As String) As dxoViewPort
            Dim mem As dxoViewPort = Nothing
            If Not TryGet(aNameOrHandleOrGUID, mem) Then Return Nothing Else Return mem
        End Function

        Public Function TryGet(aNameOrHandleOrGUID As String, ByRef rMember As dxoViewPort) As Boolean
            rMember = Nothing
            Dim entry As dxfTableEntry = Nothing
            If Not TryGetEntry(aNameOrHandleOrGUID, entry) Then Return False
            If entry.EntryType <> TableType Then
                Remove(entry)
                Return False
            End If
            rMember = DirectCast(entry, dxoViewPort)
            Return True
        End Function
        Public Overrides Function ToString() As String
            Return $"{TableTypeName} [{Count}]"
        End Function

        Private Function IEnumerable_GetEnumerator() As IEnumerator(Of dxoViewPort) Implements IEnumerable(Of dxoViewPort).GetEnumerator
            Return DirectCast(Me, IEnumerable).GetEnumerator()
        End Function
    End Class

End Namespace