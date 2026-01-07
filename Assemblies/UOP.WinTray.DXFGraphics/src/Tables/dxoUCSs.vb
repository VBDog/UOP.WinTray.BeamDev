Namespace UOP.DXFGraphics

    Public Class dxoUCSs
        Inherits dxfTable
        Implements IEnumerable(Of dxoUCS)
        Implements ICloneable

        Public Sub New()
            MyBase.New(dxxReferenceTypes.UCS)
        End Sub

        Public Sub New(bAddDefaultMembers As Boolean)
            MyBase.New(dxxReferenceTypes.UCS)
            If bAddDefaultMembers Then dxfTable.LoadDefaultMembers(Me)
        End Sub

        Public Sub New(bAddDefaultMembers As Boolean, aImage As dxfImage)
            MyBase.New(dxxReferenceTypes.UCS)
            If bAddDefaultMembers Then dxfTable.LoadDefaultMembers(Me, aImage)
        End Sub

        Public Sub New(aTable As dxoUCSs)
            MyBase.New(dxxReferenceTypes.UCS, aTable)

        End Sub

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New dxoUCSs(Me)
        End Function
        Public Function Clone() As dxoUCSs
            Return New dxoUCSs(Me)
        End Function
        Public Overloads Sub Add(aEntry As dxoUCS)
            AddEntry(aEntry)
        End Sub

        Public Function Member(aNameOrHandleOrGUID As String) As dxoUCS
            Dim mem As dxoUCS = Nothing
            If Not TryGet(aNameOrHandleOrGUID, mem) Then Return Nothing Else Return mem
        End Function

        Public Function TryGet(aNameOrHandleOrGUID As String, ByRef rMember As dxoUCS) As Boolean
            rMember = Nothing
            Dim entry As dxfTableEntry = Nothing
            If Not TryGetEntry(aNameOrHandleOrGUID, entry) Then Return False
            If entry.EntryType <> TableType Then
                Remove(entry)
                Return False
            End If
            rMember = DirectCast(entry, dxoUCS)
            Return True
        End Function
        Public Overrides Function ToString() As String
            Return $"{TableTypeName} [{Count}]"
        End Function
        Private Function IEnumerable_GetEnumerator() As IEnumerator(Of dxoUCS) Implements IEnumerable(Of dxoUCS).GetEnumerator
            Return DirectCast(Me, IEnumerable).GetEnumerator()
        End Function
    End Class

End Namespace