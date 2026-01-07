Namespace UOP.DXFGraphics

    Public Class dxoBlockRecords
        Inherits dxfTable
        Implements IEnumerable(Of dxoBlockRecord)
        Implements ICloneable

        Public Sub New()
            MyBase.New(dxxReferenceTypes.BLOCK_RECORD)
        End Sub

        Public Sub New(bAddDefaultMembers As Boolean)
            MyBase.New(dxxReferenceTypes.BLOCK_RECORD)
            If bAddDefaultMembers Then dxfTable.LoadDefaultMembers(Me)
        End Sub

        Public Sub New(bAddDefaultMembers As Boolean, aImage As dxfImage)
            MyBase.New(dxxReferenceTypes.BLOCK_RECORD)
            If bAddDefaultMembers Then dxfTable.LoadDefaultMembers(Me, aImage)
        End Sub

        Public Sub New(aTable As dxoBlockRecords)
            MyBase.New(dxxReferenceTypes.BLOCK_RECORD, aTable)
        End Sub

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New dxoBlockRecords(Me)
        End Function
        Public Function Clone() As dxoBlockRecords
            Return New dxoBlockRecords(Me)
        End Function

        Public Overloads Sub Add(aEntry As dxoBlockRecord)
            AddEntry(aEntry)
        End Sub

        Public Function Member(aNameOrHandleOrGUID As String) As dxoBlockRecord
            Dim mem As dxoBlockRecord = Nothing
            If Not TryGet(aNameOrHandleOrGUID, mem) Then Return Nothing Else Return mem
        End Function

        Public Function TryGet(aNameOrHandleOrGUID As String, ByRef rMember As dxoBlockRecord) As Boolean
            rMember = Nothing
            Dim entry As dxfTableEntry = Nothing
            If Not TryGetEntry(aNameOrHandleOrGUID, entry) Then Return False
            If entry.EntryType <> TableType Then
                Remove(entry)
                Return False
            End If
            rMember = DirectCast(entry, dxoBlockRecord)
            Return True
        End Function
        Public Overrides Function ToString() As String
            Return $"{TableTypeName} [{Count}]"
        End Function

        Private Function IEnumerable_GetEnumerator() As IEnumerator(Of dxoBlockRecord) Implements IEnumerable(Of dxoBlockRecord).GetEnumerator
            Return DirectCast(Me, IEnumerable).GetEnumerator()
        End Function
    End Class

End Namespace
