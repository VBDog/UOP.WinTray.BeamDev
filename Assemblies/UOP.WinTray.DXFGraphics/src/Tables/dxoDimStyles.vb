Namespace UOP.DXFGraphics

    Public Class dxoDimStyles
        Inherits dxfTable
        Implements IEnumerable(Of dxoDimStyle)
        Implements ICloneable

        Public Sub New()
            MyBase.New(dxxReferenceTypes.DIMSTYLE)
        End Sub

        Public Sub New(bAddDefaultMembers As Boolean)
            MyBase.New(dxxReferenceTypes.DIMSTYLE)
            If bAddDefaultMembers Then dxfTable.LoadDefaultMembers(Me)
        End Sub

        Public Sub New(bAddDefaultMembers As Boolean, aImage As dxfImage)
            MyBase.New(dxxReferenceTypes.DIMSTYLE)
            If bAddDefaultMembers Then dxfTable.LoadDefaultMembers(Me, aImage)
        End Sub

        Public Sub New(aTable As dxoDimStyles)
            MyBase.New(dxxReferenceTypes.DIMSTYLE, aTable)
        End Sub

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New dxoDimStyles(Me)
        End Function
        Public Function Clone() As dxoDimStyles
            Return New dxoDimStyles(Me)
        End Function
        Public Overloads Sub Add(aEntry As dxoDimStyle)
            AddEntry(aEntry)
        End Sub

        '''<summary>
        ''' used to add a new entry to current table
        ''' </summary>
        ''' <param name="aName">the name to apply to the new member</param>
        ''' <param name="aTextStyleName">the text stype name to asign to the new dim style</param>
        ''' <param name="bMakeCurrent">flag to make the name style the current active style in the parent image</param>
        ''' <param name="bOverrideExisting">flag to update the existing style by the given name to the passed properties</param>
        ''' <returns></returns>
        Public Overloads Function Add(aName As String, Optional aTextStyleName As String = "Standard", Optional bMakeCurrent As Boolean = False, Optional bOverrideExisting As Boolean = True)
            Dim aImage As dxfImage = MyImage
            Try
                If String.IsNullOrWhiteSpace(aName) Then
                    Throw New Exception("Invalid Name Detected")
                End If
                If String.IsNullOrWhiteSpace(aTextStyleName) Then aTextStyleName = "Standard" Else aTextStyleName = aTextStyleName.Trim()

                If aImage IsNot Nothing Then
                    aTextStyleName = aImage.GetOrAddReference(aTextStyleName, dxxReferenceTypes.STYLE).Name
                End If

                Dim existing As dxoDimStyle = Nothing
                Dim exists As Boolean = TryGet(aName, existing)
                If exists Then
                    If Not bOverrideExisting Then Throw New Exception($"{TableTypeName}'{aName }' Already Exists")
                    existing.TextStyleName = aTextStyleName
                    If bMakeCurrent And aImage IsNot Nothing Then
                        aImage.Header.SetCurrentReferenceName(dxxReferenceTypes.DIMSTYLE, existing.Name)
                    End If
                    Return existing
                End If
                Dim aEntry As New dxoDimStyle(aName, aTextStyleName)
                Return DirectCast(AddToCollection(aEntry, bOverrideExisting:=bOverrideExisting, bSetCurrent:=bMakeCurrent, ioImage:=aImage), dxoDimStyle)

            Catch ex As Exception
                If aImage IsNot Nothing Then
                    aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
                Else
                    Throw ex
                End If
                Return Nothing
            End Try



        End Function

        Public Function Member(aNameOrHandleOrGUID As String) As dxoDimStyle
            Dim mem As dxoDimStyle = Nothing
            If Not TryGet(aNameOrHandleOrGUID, mem) Then Return Nothing Else Return mem
        End Function

        Public Function TryGet(aNameOrHandleOrGUID As String, ByRef rMember As dxoDimStyle) As Boolean
            rMember = Nothing
            Dim entry As dxfTableEntry = Nothing
            If Not TryGetEntry(aNameOrHandleOrGUID, entry) Then Return False
            If entry.EntryType <> TableType Then
                Remove(entry)
                Return False
            End If
            rMember = DirectCast(entry, dxoDimStyle)
            Return True
        End Function
        Public Overrides Function ToString() As String
            Return $"{TableTypeName} [{Count}]"
        End Function

        Public Function ReferencesBlock(aBlockName As String, Optional aReturnJustOne As Boolean = False, Optional aBlockRecordHandle As String = "") As Boolean
            Dim rIndices As List(Of Integer) = Nothing
            Return ReferencesBlock(aBlockName, rIndices, aReturnJustOne, aBlockRecordHandle)
        End Function
        Public Function ReferencesBlock(aBlockName As String, ByRef rIndices As List(Of Integer), Optional aReturnJustOne As Boolean = False, Optional aBlockRecordHandle As String = "") As Boolean
            Dim _rVal As Boolean = False
            '#1the block name to search for
            '#2returns the members that have a reference to the passed block name
            '#3flag to terminate the search iList(Of Integer)            '^returns True if any of the memebrs has a reference to the passed block name
            '~only valid for dimstyle table
            rIndices = New List(Of Integer)
            If String.IsNullOrWhiteSpace(aBlockName) Or Count <= 0 Then Return False

            aBlockName = aBlockName.Trim()
            Dim i As Integer = 0
            For Each aDStyle As dxoDimStyle In Me
                i += 1
                If aDStyle.ReferencesBlock(aBlockName, aBlockRecordHandle) Then
                    rIndices.Add(i)
                    If aReturnJustOne Then Exit For
                End If
            Next
            Return rIndices.Count > 0
        End Function

        Private Function IEnumerable_GetEnumerator() As IEnumerator(Of dxoDimStyle) Implements IEnumerable(Of dxoDimStyle).GetEnumerator
            Return DirectCast(Me, IEnumerable).GetEnumerator()
        End Function
    End Class

End Namespace
