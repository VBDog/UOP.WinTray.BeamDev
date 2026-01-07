Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics

    Public Class dxoAPPIDs
        Inherits dxfTable
        Implements IEnumerable(Of dxoAPPID)
        Implements ICloneable

        Public Sub New()
            MyBase.New(dxxReferenceTypes.APPID)
        End Sub

        Public Sub New(bAddDefaultMembers As Boolean)
            MyBase.New(dxxReferenceTypes.APPID)
            If bAddDefaultMembers Then dxfTable.LoadDefaultMembers(Me)
        End Sub

        Public Sub New(bAddDefaultMembers As Boolean, aImage As dxfImage)
            MyBase.New(dxxReferenceTypes.APPID)
            If bAddDefaultMembers Then dxfTable.LoadDefaultMembers(Me, aImage)
        End Sub

        Public Sub New(aTable As dxoAPPIDs)
            MyBase.New(dxxReferenceTypes.APPID, aTable)

        End Sub

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New dxoAPPIDs(Me)
        End Function
        Public Function Clone() As dxoAPPIDs
            Return New dxoAPPIDs(Me)
        End Function
        Public Overloads Sub Add(aEntry As dxoAPPID)
            AddEntry(aEntry)
        End Sub
        Public Function Member(aNameOrHandleOrGUID As String) As dxoAPPID
            Dim mem As dxoAPPID = Nothing
            If Not TryGet(aNameOrHandleOrGUID, mem) Then Return Nothing Else Return mem
        End Function

        Public Function TryGet(aNameOrHandleOrGUID As String, ByRef rMember As dxoAPPID) As Boolean
            rMember = Nothing
            Dim entry As dxfTableEntry = Nothing
            If Not TryGetEntry(aNameOrHandleOrGUID, entry) Then Return False
            If entry.EntryType <> TableType Then
                Remove(entry)
                Return False
            End If
            rMember = DirectCast(entry, dxoAPPID)
            Return True
        End Function
        Public Overrides Function ToString() As String
            Return $"{TableTypeName} [{Count}]"
        End Function

        Private Function IEnumerable_GetEnumerator() As IEnumerator(Of dxoAPPID) Implements IEnumerable(Of dxoAPPID).GetEnumerator
            Return DirectCast(Me, IEnumerable).GetEnumerator()
        End Function
    End Class

End Namespace