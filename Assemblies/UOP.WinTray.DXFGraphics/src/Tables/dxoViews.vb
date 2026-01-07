Namespace UOP.DXFGraphics

    Public Class dxoViews
        Inherits dxfTable
        Implements ICloneable
        Implements IEnumerable(Of dxoView)

        Public Sub New()
            MyBase.New(dxxReferenceTypes.VIEW)
        End Sub

        Public Sub New(bAddDefaultMembers As Boolean)
            MyBase.New(dxxReferenceTypes.VIEW)
            If bAddDefaultMembers Then dxfTable.LoadDefaultMembers(Me)
        End Sub

        Public Sub New(bAddDefaultMembers As Boolean, aImage As dxfImage)
            MyBase.New(dxxReferenceTypes.VIEW)
            If bAddDefaultMembers Then dxfTable.LoadDefaultMembers(Me, aImage)
        End Sub

        Public Sub New(aTable As dxoViews)
            MyBase.New(dxxReferenceTypes.VIEW, aTable)
        End Sub

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New dxoViews(Me)
        End Function
        Public Function Clone() As dxoViews
            Return New dxoViews(Me)
        End Function
        Public Overloads Sub Add(aEntry As dxoView)
            AddEntry(aEntry)
        End Sub

        Public Function Member(aNameOrHandleOrGUID As String) As dxoView
            Dim mem As dxoView = Nothing
            If Not TryGet(aNameOrHandleOrGUID, mem) Then Return Nothing Else Return mem
        End Function

        Public Function TryGet(aNameOrHandleOrGUID As String, ByRef rMember As dxoView) As Boolean
            rMember = Nothing
            Dim entry As dxfTableEntry = Nothing
            If Not TryGetEntry(aNameOrHandleOrGUID, entry) Then Return False
            If entry.EntryType <> TableType Then
                Remove(entry)
                Return False
            End If
            rMember = DirectCast(entry, dxoView)
            Return True
        End Function
        Public Overrides Function ToString() As String
            Return $"{TableTypeName} [{Count}]"
        End Function
        Private Function IEnumerable_GetEnumerator() As IEnumerator(Of dxoView) Implements IEnumerable(Of dxoView).GetEnumerator
            Return DirectCast(Me, IEnumerable).GetEnumerator()
        End Function
    End Class

End Namespace