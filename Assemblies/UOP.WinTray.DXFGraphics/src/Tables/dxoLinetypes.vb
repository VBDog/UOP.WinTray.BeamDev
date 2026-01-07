Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics

    Public Class dxoLineTypes
        Inherits dxfTable
        Implements ICloneable
        Implements IEnumerable(Of dxoLinetype)

        Public Sub New()
            MyBase.New(dxxReferenceTypes.LTYPE)
        End Sub

        Public Sub New(bAddDefaultMembers As Boolean)
            MyBase.New(dxxReferenceTypes.LTYPE)
            If bAddDefaultMembers Then dxfTable.LoadDefaultMembers(Me)
        End Sub

        Public Sub New(bAddDefaultMembers As Boolean, aImage As dxfImage)
            MyBase.New(dxxReferenceTypes.LTYPE)
            If bAddDefaultMembers Then dxfTable.LoadDefaultMembers(Me, aImage)
        End Sub

        Public Sub New(aTable As dxoLineTypes)
            MyBase.New(dxxReferenceTypes.LTYPE, aTable)
        End Sub



        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New dxoLineTypes(Me)
        End Function
        Public Function Clone() As dxoLineTypes
            Return New dxoLineTypes(Me)
        End Function
        Public Overloads Sub Add(aEntry As dxoLinetype)
            AddEntry(aEntry)
        End Sub

        ''' <summary>
        ''' used to add a linetype to the table
        ''' </summary>
        ''' <param name="aName"></param>
        ''' <param name="bMakeCurrent"></param>
        ''' <returns></returns>
        Public Overloads Function Add(aName As String, Optional bMakeCurrent As Boolean? = False) As dxoLinetype

            Dim aImage As dxfImage = MyImage
            Try
                Dim bGoodName As Boolean
                Dim existing As dxoLinetype = Nothing
                Dim exists As Boolean = TryGet(aName, existing)
                If exists Then
                    If bMakeCurrent.HasValue And aImage IsNot Nothing Then
                        aImage.Header.SetCurrentReferenceName(dxxReferenceTypes.LTYPE, existing.Name)
                    End If
                    Return existing
                End If

                If String.IsNullOrWhiteSpace(aName) Then Throw New Exception("Invalid Linetype Name Detected")
                aName = aName.Trim()
                Dim aEntry As TTABLEENTRY = dxfLinetypes.DefaultDefinition(aName, False, bGoodName, aName)
                If Not bGoodName Then Throw New Exception($"An attempt was made to add an undefined linetype '{ aName }' to table '{TableTypeName()}'")
                Return AddToCollection(New dxoLinetype(aEntry), bOverrideExisting:=False, bSetCurrent:=bMakeCurrent, ioImage:=aImage)
            Catch ex As Exception
                If aImage IsNot Nothing Then
                    aImage.HandleError(Reflection.MethodBase.GetCurrentMethod(), Me.GetType(), ex)
                Else
                    Throw ex
                End If
                Return Nothing
            End Try
        End Function

        Public Function Member(aNameOrHandleOrGUID As String) As dxoLinetype
            Dim mem As dxoLinetype = Nothing
            If Not TryGet(aNameOrHandleOrGUID, mem) Then Return Nothing Else Return mem
        End Function

        Public Overrides Function ToString() As String
            Return $"{TableTypeName} [{Count}]"
        End Function

        Public Function TryGet(aNameOrHandleOrGUID As String, ByRef rMember As dxoLinetype) As Boolean
            rMember = Nothing
            Dim entry As dxfTableEntry = Nothing
            If Not TryGetEntry(aNameOrHandleOrGUID, entry) Then Return False
            If entry.EntryType <> TableType Then
                Remove(entry)
                Return False
            End If
            rMember = DirectCast(entry, dxoLinetype)
            Return True
        End Function
        Private Function IEnumerable_GetEnumerator() As IEnumerator(Of dxoLinetype) Implements IEnumerable(Of dxoLinetype).GetEnumerator
            Return DirectCast(Me, IEnumerable).GetEnumerator()
        End Function
    End Class

End Namespace