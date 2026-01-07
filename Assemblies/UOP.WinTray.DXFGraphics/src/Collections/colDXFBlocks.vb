Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public Class colDXFBlocks
        Inherits List(Of dxfBlock)
        Implements iHandleOwner
        Implements IEnumerable(Of dxfBlock)
        Implements ICloneable
        Implements IDisposable

#Region "Members"
        Private disposedValue As Boolean
#End Region 'Members
#Region "Constructors"
        Public Sub New()
            MyBase.New()

        End Sub
        Friend Sub New(aImage As dxfImage, Optional aGUID As String = "")
            MyBase.New()
            If aImage IsNot Nothing Then
                If String.IsNullOrWhiteSpace(aGUID) Then aGUID = dxfEvents.NextBlocksGUID
                ImageGUID = aGUID
                SetImage(aImage, False)
                MonitorMembers = True
            End If
        End Sub

        Public Sub New(aBlocks As colDXFBlocks)
            MyBase.New()

            If aBlocks Is Nothing Then Return
            For Each block As dxfBlock In aBlocks
                Add(New dxfBlock(block))
            Next
        End Sub
#End Region 'Constructors
#Region "dxfHandleOwner"


#End Region 'dxfHandleOwner
#Region "Properties"

        Friend Property CollectionObj As List(Of dxfBlock)
            Get
                Return MyBase.ToList()
            End Get
            Set(value As List(Of dxfBlock))
                Populate(value)
            End Set
        End Property
        Friend Property Handlez As THANDLES
            Get
                Return _Handlez
            End Get
            Set(value As THANDLES)
                _Handlez = New THANDLES(value, GUID)
            End Set
        End Property
        Public ReadOnly Property NamesList As String
            Get
                '^returns a comma delimited list of the names of the current members
                Dim _rVal As String = String.Empty
                For Each blk In Me
                    TLISTS.Add(_rVal, blk.Name, bAllowDuplicates:=True)
                Next
                Return _rVal
            End Get
        End Property

        Public ReadOnly Property Names As List(Of String)
            Get
                '^returns a comma delimited list of the names of the current members
                Dim _rVal As New List(Of String)
                For Each blk In Me
                    _rVal.Add(blk.Name)
                Next
                Return _rVal
            End Get
        End Property
        Public Shadows Function Item(aName As String) As dxfBlock
            '#1the requested item name or number
            '^returns the object from the collection at the requested index in the collection.
            '~returns nothing if the passed index is outside the bounds of the current collection
            If String.IsNullOrWhiteSpace(aName) Then Return Nothing
            Dim _rVal As dxfBlock = SetMemberInfo(MyBase.Find(Function(mem) String.Compare(mem.Name, aName, True) = 0))
            Return _rVal
        End Function
        Friend Property MonitorMembers As Boolean
            Get
                Return Not String.IsNullOrWhiteSpace(GUID)
            End Get
            Set(value As Boolean)
                If value Then
                    If String.IsNullOrWhiteSpace(GUID) Then
                        GUID = dxfEvents.NextBlocksGUID
                        For Each mem As dxfBlock In Me
                            SetMemberInfo(mem)
                        Next
                    End If
                    'If _Events IS Nothing Then _Events = goEvents()
                    'AddHandler _Events.EntititiesRequest, AddressOf _Events_EntitiesRequest
                Else
                    If Not String.IsNullOrWhiteSpace(GUID) Then
                        For Each mem As dxfBlock In Me
                            mem.SetBlockCollection(Nothing, False)
                        Next
                    End If
                    GUID = ""
                    'If _Events  IsNot Nothing Then RemoveHandler _Events.EntititiesRequest, AddressOf _Events_EntitiesRequest
                    '_Events = Nothing
                End If
            End Set
        End Property

#End Region 'Properties
#Region "Methods"

        Public Function Clone() As colDXFBlocks
            Return New colDXFBlocks(Me)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New colDXFBlocks(Me)
        End Function
        Public Function BlockRecordHandle(aBlockNameOrHandle As String) As String

            Dim aMem As dxfBlock = GetByName(aBlockNameOrHandle, True)
            If aMem Is Nothing Then Return "0"
            Return aMem.BlockRecordHandle

        End Function

        Public Overrides Function ToString() As String
            Return $"colDXFBlocks[{ Count }]"
        End Function
        Public Shadows Function Item(aIndex As Integer) As dxfBlock
            '#1the requested item name or number
            '^returns the object from the collection at the requested index in the collection.
            '~returns nothing if the passed index is outside the bounds of the current collection
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return Nothing
            End If
            Dim _rVal As dxfBlock = MyBase.ElementAt(aIndex - 1)
            If _rVal IsNot Nothing Then
                If Not String.IsNullOrWhiteSpace(ImageGUID) Then _rVal.ImageGUID = ImageGUID
                If MonitorMembers Then _rVal.Index = aIndex
            End If
            Return _rVal
        End Function
        Private Function SetMemberInfo(aMember As dxfBlock, Optional bReturnClone As Boolean = False) As dxfBlock
            If aMember Is Nothing Then Return Nothing
            If MonitorMembers Then
                aMember.SetBlockCollection(Me, False)
            End If
            'If _Suppressed Then aMember.Suppressed = True
            'If ImageGUID <> "" Then aMember.ImageGUID = ImageGUID
            'If OwnerGUID <> "" Then aMember.OwnerGUID = OwnerGUID
            'If BlockGUID <> "" Then aMember.BlockGUID = BlockGUID
            'aMember.CollectionGUID = CollectionGUID
            If bReturnClone Then Return aMember.Clone Else Return aMember
        End Function

        ''' <summary>
        ''' used to add dxfBlock objects to the collection
        ''' </summary>
        ''' <remarks   >
        ''' won't add nothing to the collection (no error is raised)
        ''' doesn't allow duplicate blocks (same name) to be added so first in wins unless the override existing is passed as true.
        ''' </remarks>
        ''' <param name="aBlock">the subject block</param>
        ''' <param name="bOverrideExisting">flag to replace an existing block with the same name with the passed block</param>
        ''' <param name="aAddClone">to add a clone of the passed block, not the block itself</param>
        ''' <returns></returns>
        Public Overloads Function Add(aBlock As dxfBlock, Optional bOverrideExisting As Boolean = False, Optional aAddClone As Boolean = False) As dxfBlock

            If aBlock Is Nothing Then Return Nothing Else Return AddToCollection(aBlock, bOverrideExisting:=bOverrideExisting, aAddClone:=aAddClone)
        End Function
        Friend Function AddToCollection(aBlock As dxfBlock, Optional aBeforeIndex As Integer = 0, Optional aAfterIndex As Integer = 0, Optional bSuppressEvnts As Boolean = False, Optional bOverrideExisting As Boolean = False, Optional aAddClone As Boolean = False, Optional aImage As dxfImage = Nothing) As dxfBlock
            Dim _rVal As dxfBlock = Nothing
            If aBlock Is Nothing Then Return Nothing
            If String.IsNullOrWhiteSpace(aBlock.Name) Then Return Nothing

            Dim exstidx As Integer = IndexOf(aBlock)
            Dim rmvidx As Integer = 0
            'it's already a member
            If exstidx > 0 Then Return aBlock

            exstidx = FindIndex(Function(mem) String.Compare(mem.GUID, aBlock.GUID, True) = 0)
            'no duplicate guids  (this should never happen due to the previous trap on membership)
            If exstidx >= 0 Then
                If Not bOverrideExisting Then Return Item(exstidx + 1)
                rmvidx = exstidx

            End If

            If aAddClone Then _rVal = aBlock.Clone Else _rVal = aBlock


            If Not bSuppressEvnts Then
                If aImage Is Nothing Then aImage = MyImage
            End If
            If aImage Is Nothing Then bSuppressEvnts = True


            'no duplicate names
            exstidx = MyBase.FindIndex(Function(x) String.Compare(x.Name, _rVal.Name, True) = 0)
            If exstidx >= 0 Then
                If Not bOverrideExisting Then Return Item(exstidx + 1)
                rmvidx = exstidx
            End If

            Dim tempremove As dxfBlock = Nothing
            If rmvidx > 0 Then
                tempremove = Item(rmvidx + 1)
                MyBase.RemoveAt(rmvidx)
            End If

            Dim bBail As Boolean
            If Not bSuppressEvnts Then
                aImage.RespondToCollectionEvent(Me, dxxCollectionEventTypes.PreAdd, _rVal, bBail)
                If bBail Then
                    If tempremove IsNot Nothing Then MyBase.Add(tempremove)
                    Return Nothing
                End If
            End If
            If tempremove IsNot Nothing Then
                tempremove.SetBlockCollection(Nothing, False)
            End If



            _rVal.SetBlockCollection(Me, False)
            Dim cnt As Integer = Count
            Dim idx As Integer = 0
            If cnt = 0 Then
                aBeforeIndex = 0
                aAfterIndex = 0
                idx = 1
            Else
                If aBeforeIndex < 1 Then aBeforeIndex = 0
                If aAfterIndex < 1 Then aAfterIndex = 0
                If aBeforeIndex > 0 Then
                    aAfterIndex = 0
                    If aBeforeIndex >= cnt Then aBeforeIndex = 0
                    If aBeforeIndex > 0 Then idx = aBeforeIndex - 1
                ElseIf aAfterIndex > 0 Then
                    aBeforeIndex = 0
                    If aAfterIndex >= cnt Then aAfterIndex = 0
                    If aAfterIndex > 0 Then idx = aAfterIndex + 1
                End If
            End If
            If aBeforeIndex = 0 And aAfterIndex = 0 Then
                idx = cnt + 1
                MyBase.Add(_rVal)
            Else
                If aBeforeIndex <> 0 Then
                    MyBase.Insert(idx - 1, _rVal)
                Else
                    MyBase.Insert(idx - 1, _rVal)
                End If
            End If
            _rVal = Item(idx)
            If Not bSuppressEvnts Then
                aImage.RespondToCollectionEvent(Me, dxxCollectionEventTypes.Add, _rVal, bBail)
            End If
            Return _rVal
        End Function
        Friend Sub Append(aBlocks As colDXFBlocks, bOverrideExisting As Boolean, bAddClones As Boolean)
            If aBlocks Is Nothing Then Return
            For i As Integer = 1 To aBlocks.Count
                Add(aBlocks.Item(i), bOverrideExisting, bAddClones)
            Next i
        End Sub
        Public Function BlockExists(aNameOrHandle As String) As Boolean
            Dim aMem As dxfBlock = Nothing
            '#1the block name to search for
            '^tests to see if a block with the same name as the passed block name is present in the collection
            '~is not case dependant and lead and trailing spaces are ignored.
            If TryGet(aNameOrHandle, aMem, aPassedType:=dxxBlockReferenceTypes.Name) Then Return True
            If TryGet(aNameOrHandle, aMem, aPassedType:=dxxBlockReferenceTypes.Handle) Then Return True
            Return False
        End Function
        Friend Function GetArrowHead(ByRef ioBRHandle As String, ByRef ioBlockName As String, Optional bTryHandleFirst As Boolean = False, Optional bReturnDefaultIfNotFound As Boolean = True, Optional aImage As dxfImage = Nothing) As dxfBlock
            If String.IsNullOrWhiteSpace(ioBlockName) Then ioBlockName = "" Else ioBlockName = ioBlockName.Trim
            If String.IsNullOrWhiteSpace(ioBRHandle) Then ioBRHandle = "" Else ioBRHandle = ioBRHandle.Trim
            Dim _rVal As dxfBlock = Nothing
            Dim populate As Boolean
            Dim newblock As dxfBlock
            If bTryHandleFirst Then
                If _rVal Is Nothing And Not String.IsNullOrWhiteSpace(ioBRHandle) Then
                    _rVal = GetByBlockRecordHandle(ioBRHandle)
                    If _rVal IsNot Nothing Then
                        ioBRHandle = _rVal.BlockRecordHandle
                        ioBlockName = _rVal.Name
                        If _rVal.Entities.Count > 0 Then
                            'Return _rVal
                        Else
                            populate = True
                        End If
                    End If
                End If
                If _rVal Is Nothing And Not String.IsNullOrWhiteSpace(ioBlockName) Then
                    _rVal = GetByName(ioBlockName, True)
                    If _rVal IsNot Nothing Then
                        ioBRHandle = _rVal.BlockRecordHandle
                        ioBlockName = _rVal.Name
                        If _rVal.Entities.Count > 0 Then
                            'Return _rVal
                        Else
                            populate = True
                        End If
                    End If
                End If
            Else
                If _rVal Is Nothing And Not String.IsNullOrWhiteSpace(ioBlockName) Then
                    _rVal = GetByName(ioBlockName, True)
                    If _rVal IsNot Nothing Then
                        ioBRHandle = _rVal.BlockRecordHandle
                        ioBlockName = _rVal.Name
                        If _rVal.Entities.Count > 0 Then
                            'Return _rVal
                        Else
                            populate = True
                        End If
                    End If
                End If
                If _rVal Is Nothing And Not String.IsNullOrWhiteSpace(ioBRHandle) Then
                    _rVal = GetByBlockRecordHandle(ioBRHandle)
                    If _rVal IsNot Nothing Then
                        ioBRHandle = _rVal.BlockRecordHandle
                        ioBlockName = _rVal.Name
                        If _rVal.Entities.Count > 0 Then
                            'Return _rVal
                        Else
                            populate = True
                        End If
                    End If
                End If
            End If
            If _rVal IsNot Nothing And Not populate Then
                Return _rVal
            End If
            If bReturnDefaultIfNotFound Then
                GetImage(aImage)
                If ioBlockName = "" Then ioBlockName = "_ClosedFilled"
                If dxfArrowheads.IsDefault(ioBlockName) Then
                    newblock = dxfArrowheads.CreateArrowHeadBlock(aImage, ioBlockName)
                    If newblock Is Nothing Then
                        ioBlockName = ""
                        ioBRHandle = ""
                        Return Nothing
                    Else
                        If populate Then
                            _rVal.Entities.Append(newblock.Entities)
                        Else
                            _rVal = AddToCollection(newblock, aAfterIndex:=1, aImage:=aImage)
                        End If
                        If _rVal IsNot Nothing Then
                            ioBRHandle = _rVal.BlockRecordHandle
                            ioBlockName = _rVal.Name
                            Return _rVal
                        Else
                            ioBRHandle = ""
                            'ioBlockName = _rVal.Name
                        End If
                    End If
                    Return Nothing
                End If
            Else
                Return Nothing
            End If
            Return _rVal
        End Function
        Friend Overloads Sub Clear()
            '^removes all the members from the collection
            Dim HG As dxoHandleGenerator = Nothing
            Dim aImage As dxfImage = Nothing
            Dim bhndls As Boolean = GetImage(aImage)
            Try
                If bhndls Then aImage.HandleGenerator.ReleaseHandles(Me)

                MyBase.Clear()

            Catch
            End Try

        End Sub
        Friend Function GetBlockAndSubBlocks(aImage As dxfImage, aBlockName As String, Optional bReturnClones As Boolean = False) As List(Of dxfBlock)

            '#1the block name to search for
            '#2returns the index of the block in the collection if it is found
            '^returns the requested blcok if the passed block name is found
            '~is not case dependant and lead and trailing spaces are ignored.
            '~returns nothing if the block can't be found.
            Dim _rVal As New List(Of dxfBlock)
            If String.IsNullOrWhiteSpace(aBlockName) Then Return _rVal Else aBlockName = aBlockName.Trim()

            GetImage(aImage)
            Dim aMem As dxfBlock
            Dim bMem As dxfBlock
            Dim aEnt As dxfEntity
            Dim aIns As dxeInsert
            aMem = GetByName(aBlockName)
            If aMem Is Nothing Then Return _rVal
            If bReturnClones Then
                If aImage Is Nothing Then
                    aMem = aMem.Clone
                Else
                    aMem = aMem.CloneAll(aImage)
                End If
            End If
            _rVal.Add(aMem)
            For i As Integer = 1 To aMem.Entities.Count
                aEnt = aMem.Entities.Item(i)
                If aEnt.GraphicType = dxxGraphicTypes.Insert Then
                    aIns = aEnt
                    bMem = GetByName(aIns.BlockName)
                    If bMem IsNot Nothing Then
                        If bReturnClones Then
                            If aImage Is Nothing Then
                                bMem = bMem.Clone
                            Else
                                bMem = bMem.CloneAll(aImage)
                            End If
                        End If
                    End If
                    _rVal.Add(bMem)
                End If
            Next i
            Return _rVal
        End Function
        Friend Function GetByBlockRecordHandle(aHandle As Object) As dxfBlock
            Dim _rVal As dxfBlock = Nothing
            Dim rBid As Integer = 0
            '#1the block handle to search for
            '#2returns the index of the block in the collection if it is found
            '^returns the requested block if the passed handle is found
            '~is not case dependant and lead and trailing spaces are ignored.
            '~returns nothing if the block can't be found.
            If aHandle = "" Then Return _rVal
            Dim aMem As dxfBlock
            For i As Integer = 1 To Count
                aMem = Item(i)
                If String.Compare(aMem.Handle, aHandle, True) = 0 Then
                    _rVal = aMem
                    rBid = i
                    Exit For
                End If
            Next i
            Return _rVal
        End Function
        Friend Function GetByGUID(aGUID As String) As dxfBlock
            '#1the block guid to search for
            '#2returns the index of the block in the collection if it is found
            '^returns the requested block if the passed handle is found
            '~is not case dependant and lead and trailing spaces are ignored.
            '~returns nothing if the block can't be found.
            If String.IsNullOrWhiteSpace(aGUID) Then Return Nothing
            Dim aMem As dxfBlock = Nothing
            TryGet(aGUID, aMem, aPassedType:=dxxBlockReferenceTypes.GUID)
            Return aMem
        End Function
        Public Function GetByHandle(aHandle As String) As dxfBlock
            '#1the block handle to search for
            '#2returns the index of the block in the collection if it is found
            '^returns the requested block if the passed handle is found
            '~is not case dependant and lead and trailing spaces are ignored.
            '~returns nothing if the block can't be found.
            Dim aMem As dxfBlock = Nothing
            TryGet(aHandle, aMem, aPassedType:=dxxBlockReferenceTypes.Handle)
            Return aMem
        End Function
        Public Function GetByName(aBlockNameOrHandle As String, Optional bLoadDefaults As Boolean = False) As dxfBlock
            If (String.IsNullOrWhiteSpace(aBlockNameOrHandle)) Then Return Nothing
            '#1the block name to search for
            '#2returns the index of the block in the collection if it is found
            '^returns the requested blcok if the passed block name is found
            '~is not case dependant and lead and trailing spaces are ignored.
            '~returns nothing if the block can't be found.
            Dim _rVal As dxfBlock = Nothing
            Dim bname As String = aBlockNameOrHandle.Trim
            TryGet(aBlockNameOrHandle, _rVal, aPassedType:=dxxBlockReferenceTypes.Name)
            If _rVal Is Nothing And ImageGUID <> "" And bLoadDefaults Then
                _rVal = LoadDefaultBlock(bname)
            End If
            If _rVal IsNot Nothing And ImageGUID <> "" Then
                _rVal.ImageGUID = ImageGUID
            End If
            Return _rVal
        End Function
        Friend Function GetByNamesLike(aBlockName As String, Optional aCollector As List(Of dxfBlock) = Nothing, Optional bRemove As Boolean = False) As List(Of dxfBlock)
            Dim _rVal As List(Of dxfBlock)
            '#1the block name to search for
            '#2returns the index of the block in the collection if it is found
            '^returns the requested block if the passed block name is found
            '~is not case dependant and lead and trailing spaces are ignored.
            '~returns nothing if the block can't be found.
            If aCollector Is Nothing Then _rVal = New List(Of dxfBlock) Else _rVal = aCollector

            If String.IsNullOrEmpty(aBlockName) Then Return _rVal
            aBlockName = aBlockName.Trim()
            Dim aMem As dxfBlock
            Dim sln As Integer
            Dim bname As String
            sln = Len(aBlockName)
            For i As Integer = Count To 1 Step -1
                aMem = Item(i)
                aMem.Index = i
                bname = aMem.Name
                If Len(bname) >= sln Then
                    If String.Compare(Left(bname, sln), aBlockName, True) = 0 Then
                        _rVal.Add(aMem)
                        If bRemove Then
                            aMem.CollectionGUID = ""
                            Remove(i)
                        End If
                    End If
                End If
            Next i
            Return _rVal
        End Function
        Friend Function GetEntities() As colDXFEntities
            Dim _rVal As New colDXFEntities With {.MaintainIndices = False}
            Dim aMem As dxfBlock
            Dim aEnts As colDXFEntities
            Dim aEnt As dxfEntity
            For i As Integer = 1 To Count
                aMem = Item(i)
                aEnts = aMem.Entities
                For j As Integer = 1 To aEnts.Count
                    aEnt = aEnts.Item(j)
                    _rVal.AddToCollection(aEnt, bSuppressEvnts:=True)
                Next j
            Next i
            Return _rVal
        End Function
        Public Function GetHandle(aBlockName As String, Optional aReturnBlockRecordHandle As Boolean = False) As String
            Dim _rVal As String = String.Empty
            '#1the block name to search for
            '#2flag to return the block record handle of the block rather than the handle of the block
            '^returns the handle of the requested block if found
            Dim aMem As dxfBlock
            For i As Integer = 1 To Count
                aMem = Item(i)
                If String.Compare(aMem.Name, aBlockName, True) = 0 Then
                    If Not aReturnBlockRecordHandle Then _rVal = aMem.Handle Else _rVal = aMem.BlockRecordHandle
                    Exit For
                End If
            Next i
            Return _rVal
        End Function
        Friend Function GetLayoutBlocks(bAddDefaults As Boolean) As List(Of dxfBlock)
            Dim rBlockNames As String = String.Empty
            Dim rLayoutNames As String = String.Empty
            Dim rMaxSpaceIndex As Integer = 0
            Return GetLayoutBlocks(bAddDefaults, rBlockNames, rLayoutNames, Nothing, rMaxSpaceIndex)
        End Function
        Friend Function GetLayoutBlocks(bAddDefaults As Boolean, ByRef rBlockNames As String, ByRef rLayoutNames As String, aImage As dxfImage, ByRef rMaxSpaceIndex As Integer) As List(Of dxfBlock)
            Dim _rVal As New List(Of dxfBlock)
            '^returns the blocks that are associated to layout objects
            rBlockNames = ""
            rLayoutNames = ""
            rMaxSpaceIndex = 0
            Dim aMem As dxfBlock
            Dim bname As String
            Dim lname As String
            Dim spcidx As Integer
            Dim idx As Integer
            Dim bNames As List(Of String) = Names
            spcidx = 1
            If bAddDefaults Then
                If Not bNames.Contains("*Model_Space", StringComparer.OrdinalIgnoreCase) Then
                    aMem = New dxfBlock("*Model_Space")
                    If aImage IsNot Nothing Then aImage.HandleGenerator.AssignTo(aMem)
                    AddToCollection(aMem, aBeforeIndex:=1, bSuppressEvnts:=True)
                End If
                If Not bNames.Contains("*Paper_Space", StringComparer.OrdinalIgnoreCase) Then
                    aMem = New dxfBlock("*Paper_Space", dxxDrawingDomains.Paper)
                    If aImage IsNot Nothing Then aImage.HandleGenerator.AssignTo(aMem)
                    AddToCollection(aMem, aBeforeIndex:=2, bSuppressEvnts:=True)
                End If
            End If
            Dim cnt As Integer = Count
            For i As Integer = 1 To cnt
                aMem = Item(i)
                aMem.Index = i
                bname = aMem.Name
                If Len(bname) >= 12 Then
                    If TLISTS.Contains(bname, "*Model_Space,*Paper_Space") Then
                        If String.Compare(Left(bname, 12), "*Model_Space", True) = 0 Then
                            aMem.Domain = dxxDrawingDomains.Model
                            lname = "Model"
                        Else
                            aMem.Domain = dxxDrawingDomains.Paper
                            lname = aMem.LayoutName
                            If lname = "" Then
                                lname = "Layout" & spcidx
                                spcidx += 1
                            Else
                                idx = dxfUtils.ExtractTrailingIndex(bname)
                                If idx > rMaxSpaceIndex Then rMaxSpaceIndex = idx
                            End If
                        End If
                        aMem.LayoutName = lname
                        _rVal.Add(aMem)
                        TLISTS.Add(rBlockNames, bname)
                        TLISTS.Add(rLayoutNames, lname, bAllowDuplicates:=True, bAllowNulls:=True)
                    End If
                End If
            Next i
            Return _rVal
        End Function
        Public Overloads Function IndexOf(aMember As dxfBlock) As Integer
            If aMember Is Nothing Or Count <= 0 Then Return 0
            Return MyBase.IndexOf(aMember) + 1
        End Function
        Friend Overloads Function Insert(aBlock As dxfBlock, aBeforeIndex As Integer) As dxfBlock
            If aBlock Is Nothing Then Return Nothing
            '#1the block to add to the collection
            '#2the index to insert the block before
            '^used to insert a block into the collection at a position other than the end of the collection
            '~the passed block is added at end of the collection if the passed BeforIndex is outside the bounds of the
            '~current collection dimensions. If the BeforIndex <= 1 then the block is placed at the begining of the collection.
            Return AddToCollection(aBlock, aBeforeIndex:=aBeforeIndex)
        End Function
        Friend Function LoadDefaultBlock(aBlockName As String) As dxfBlock
            Dim _rVal As dxfBlock = Nothing
            Dim bname As String
            bname = aBlockName.Trim
            If bname = "" Then Return _rVal
            If Not bname.StartsWith("_") Then bname = "_" & bname
            If TryGet(bname, _rVal) Then Return _rVal
            If Not dxfArrowheads.IsDefault(bname) Then Return Nothing
            Dim aImage As dxfImage = Nothing
            If Not GetImage(aImage) Then Return Nothing
            _rVal = dxfArrowheads.CreateArrowHeadBlock(aImage, bname)
            If _rVal Is Nothing Then Return Nothing
            AddToCollection(_rVal, aAfterIndex:=2, bSuppressEvnts:=False)
            Return _rVal
        End Function
        Public Function GetNames(Optional bReturnUpperCase As Boolean = False) As List(Of String)
            Dim _rVal As New List(Of String)
            For i As Integer = 1 To Count
                If Not bReturnUpperCase Then _rVal.Add(Item(i).Name) Else _rVal.Add(Item(i).Name.ToUpper)
            Next
            Return _rVal
        End Function
        Public Function NextBlockName(aBlockName As String) As String
            aBlockName = Trim(aBlockName)
            If String.IsNullOrEmpty(aBlockName) Then Return String.Empty
            Dim _rVal As String = aBlockName
            Dim idx As Integer
            idx = 1
            Do Until Not BlockExists(_rVal)
                _rVal += "_" & idx
                idx += 1
            Loop
            Return _rVal
        End Function
        Friend Sub Populate(ByRef newMembers As IEnumerable(Of dxfBlock), Optional bAddClones As Boolean = True, Optional bSuppressEvents As Boolean = False)
            '#1a collection of objects to define this one with
            '#2flag to add clones of the passed objects
            '^clears the current collection and adds the new members

            Clear()
            For Each block As dxfBlock In newMembers
                AddToCollection(block, aAddClone:=bAddClones, bSuppressEvnts:=bSuppressEvents)
            Next
        End Sub
        Public Function ReferencesBlock(aBlockName As String, Optional aReturnJustOne As Boolean = False) As Boolean
            Dim rBlocks As List(Of dxfBlock) = Nothing
            Return ReferencesBlock(aBlockName, rBlocks, aReturnJustOne)
        End Function
        Public Function ReferencesBlock(aBlockName As String, ByRef rBlocks As List(Of dxfBlock), Optional aReturnJustOne As Boolean = False) As Boolean
            Dim _rVal As Boolean = False
            '#1the block name to search for
            '#2returns the members that have a reference to the passed block name
            '#3flag to terminate the search if one is found
            '^returns True if any of the memebrs has a reference to the passed block name

            rBlocks = New List(Of dxfBlock)
            If String.IsNullOrEmpty(aBlockName) Then Return False
            aBlockName = aBlockName.Trim

            For i As Integer = 1 To Count
                Dim aBlock As dxfBlock = Item(i)
                If String.Compare(aBlockName, aBlock.Name, True) <> 0 Then
                    If aBlock.ReferencesBlock(aBlockName) Then
                        _rVal = True
                        rBlocks.Add(aBlock)
                        If aReturnJustOne Then Exit For
                    End If
                End If
            Next i
            Return _rVal
        End Function
        Public Function ReferencesDimStyle(aStyleName As String) As Boolean
            Dim rMemberName As String = String.Empty
            Return ReferencesDimStyle(aStyleName, rMemberName)
        End Function
        Public Function ReferencesDimStyle(aStyleName As String, ByRef rMemberName As String) As Boolean
            rMemberName = ""
            '#1the dim style name to search for
            '#2returns the index of the first member which references the search dim style
            '^returns True if any of the memebrs has a reference to the passed dim style
            aStyleName = Trim(aStyleName)
            If aStyleName = "" Then Return False
            Dim aMem As dxfBlock
            For i As Integer = 1 To Count
                aMem = Item(i)
                If aMem.Entities.ReferencesDimStyle(aStyleName) Then
                    rMemberName = aMem.Name
                    Return True
                End If
            Next i
            Return False
        End Function
        Public Function ReferencesLayer(aName As String) As Boolean
            Dim rMemberName As String = String.Empty
            Return ReferencesLayer(aName, rMemberName)
        End Function
        Public Function ReferencesLayer(aName As String, ByRef rMemberName As String) As Boolean
            '#1the layer name to search for
            '#2returns the index of the first member which references the search layer
            '^returns True if any of the memebrs has a reference to the passed layer
            rMemberName = ""
            If (String.IsNullOrWhiteSpace(aName)) Then Return False
            aName = aName.Trim
            Dim aMem As dxfBlock = Find(Function(mem) String.Compare(mem.LayerName, aName, ignoreCase:=True) = 0)
            If (aMem IsNot Nothing) Then
                rMemberName = aMem.Name
                Return True
            End If
            For Each aMem In Me
                If aMem.Entities.ReferencesLayer(aName) Then
                    rMemberName = aMem.Name
                    Return True
                End If
            Next
            Return False
        End Function
        Public Function ReferencesLinetype(aName As String) As Boolean
            Dim rMemberName As String = String.Empty
            Return ReferencesLinetype(aName, rMemberName)
        End Function
        Public Function ReferencesLinetype(aName As String, ByRef rMemberName As String) As Boolean
            '#1the linetype name to search for
            '#2returns the index of the first member which references the search linetype
            '^returns True if any of the memebrs has a reference to the passed linetype
            rMemberName = ""
            If (String.IsNullOrWhiteSpace(aName)) Then Return False
            aName = aName.Trim
            For Each aMem In Me
                If aMem.Entities.ReferencesLinetype(aName) Then
                    rMemberName = aMem.Name
                    Return True
                End If
            Next
            Return False
        End Function
        Public Function ReferencesStyle(aStyleName As String) As Boolean
            Dim rMemberName As String = String.Empty
            Return ReferencesStyle(aStyleName, rMemberName)
        End Function
        Public Function ReferencesStyle(aStyleName As String, ByRef rMemberName As String) As Boolean
            rMemberName = ""
            '#1the style name to search for
            '#2returns the index of the first member which references the search style
            '^returns True if any of the memebrs has a reference to the passed style
            aStyleName = Trim(aStyleName)
            If aStyleName = "" Then Return False
            For Each aMem As dxfBlock In Me
                If aMem.Entities.ReferencesStyle(aStyleName) Then
                    rMemberName = aMem.Name
                    Return True
                End If
            Next
            Return False
        End Function
        Public Overloads Function Remove(aMember As dxfBlock) As dxfBlock
            Return Remove(IndexOf(aMember))
        End Function
        Public Overloads Function Remove(aIndex As Integer) As dxfBlock
            Dim rAbort As Boolean = False
            Return Remove(aIndex, rAbort)
        End Function
        Public Overloads Function Remove(aIndex As Integer, ByRef rAbort As Boolean) As dxfBlock
            Dim rBlock As dxfBlock = Nothing
            '^removes the object from then collection at the indicated index
            rAbort = False
            If aIndex < 1 Or aIndex > Count Then Return rBlock
            rBlock = Item(aIndex)
            If rBlock Is Nothing Then Return rBlock
            Dim aImage As dxfImage = Nothing
            If GetImage(aImage) Then
                aImage.RespondToCollectionEvent(Me, dxxCollectionEventTypes.PreRemove, rBlock, rAbort)
                If rAbort Then Return Nothing
            End If
            rBlock.CollectionGUID = ""
            MyBase.Remove(rBlock)
            If aImage IsNot Nothing Then
                Dim canc As Boolean
                aImage.RespondToCollectionEvent(Me, dxxCollectionEventTypes.Remove, rBlock, canc)
            End If
            Return rBlock
        End Function
        Public Overloads Function Remove(aName As String) As dxfBlock
            Dim rAbort As Boolean = False
            Return Remove(aName, rAbort)
        End Function
        Public Overloads Function Remove(aName As String, ByRef rAbort As Boolean) As dxfBlock
            If Count <= 0 Then Return Nothing
            '^removes the object from then collection at the indicated index
            '#1the item number to remove
            Dim rBlock As dxfBlock = Nothing
            rAbort = False
            If Not TryGet(aName, rBlock, aPassedType:=dxxBlockReferenceTypes.Name) Then Return Nothing
            Dim aImage As dxfImage = Nothing
            If GetImage(aImage) Then
                aImage.RespondToCollectionEvent(Me, dxxCollectionEventTypes.PreRemove, rBlock, rAbort)
                If rAbort Then Return Nothing
            End If
            rBlock.CollectionGUID = ""
            MyBase.Remove(rBlock)
            If aImage IsNot Nothing Then
                Dim canc As Boolean
                aImage.RespondToCollectionEvent(Me, dxxCollectionEventTypes.Remove, rBlock, canc)
            End If
            Return rBlock
        End Function
        Friend Function RemoveByGUID(aGUID As String) As dxfBlock
            Dim aMem As dxfBlock = Nothing
            If Not TryGet(aGUID, aMem, aPassedType:=GUID) Then Return Nothing
            MyBase.Remove(aMem)
            Return aMem
        End Function
        Public Function RemoveByHandle(aHandle As String) As dxfBlock
            '#1the block handle to search for
            '#2returns the index of the block in the collection if it is found
            '^returns and removes the requested block if the passed handle is found
            Dim aMem = GetByHandle(aHandle)
            If aMem Is Nothing Then Return Nothing
            Dim aImage As dxfImage = Nothing
            If GetImage(aImage) Then
                Dim bAbort As Boolean
                aImage.RespondToCollectionEvent(Me, dxxCollectionEventTypes.PreRemove, aMem, bAbort)
                If bAbort Then Return Nothing
            End If
            aMem.CollectionGUID = ""
            MyBase.Remove(aMem)
            If aImage IsNot Nothing Then
                Dim canc As Boolean
                aImage.RespondToCollectionEvent(Me, dxxCollectionEventTypes.Remove, aMem, canc)
            End If
            Return aMem
        End Function
        Friend Function RespondToLayerChange(aImage As dxfImage, aLayer As dxoLayer, aLayerProp As dxoProperty) As Integer
            If aLayerProp Is Nothing Or aLayer Is Nothing Then Return 0
            Dim _rVal As Integer = 0
            Dim i As Integer
            Dim aMem As dxfBlock
            If aLayerProp.GroupCode = 2 Then
                SetDisplayVariable(dxxDisplayProperties.LayerName, aLayerProp.Value, aLayerProp.LastValue)
            Else
                For i = 1 To Count
                    aMem = Item(i)
                    aMem.Entities.RespondToLayerChange(aImage, aLayer, aLayerProp)
                Next i
            End If
            Return _rVal
        End Function
        Friend Function SetDisplayVariable(aPropertyType As dxxDisplayProperties, aNewValue As Object, Optional aSearchValue As Object = Nothing, Optional aSearchType As dxxEntityTypes = dxxEntityTypes.Undefined) As Integer
            Dim _rVal As Integer = 0
            '#1the name of the display variable to affect (LayerName, Color, Linetype etc.)
            '#2the new value for the  display variable
            '#3a variable value to match
            '#4flag to set any undefined variable to the new value
            '^sets the members indicated display variable to the new value
            '~if a search value is passed then only members with a current value equal to the search value will be affected.
            '~returns the affected members.
            If aPropertyType = dxxDisplayProperties.Undefined Then Return _rVal
            Dim aMem As dxfBlock
            Dim bTestIt As Boolean = aSearchValue IsNot Nothing
            Dim sValue As String = String.Empty
            Dim sTest As String = String.Empty
            Dim bLyr As Boolean = aPropertyType = dxxDisplayProperties.LayerName
            If bLyr Then
                bLyr = True
                sValue = TVALUES.To_STR(aNewValue, bTrim:=True)
                If sValue = "" Then sValue = "0"
                If bTestIt Then
                    sTest = TVALUES.To_STR(aSearchValue, bTrim:=True)
                    bTestIt = sTest <> ""
                End If
            End If
            For i As Integer = 1 To Count
                aMem = Item(i)
                If bLyr Then
                    If Not bTestIt Then
                        aMem.LayerName = sValue
                    Else
                        If String.Compare(aMem.LayerName, sTest, True) = 0 Then
                            aMem.LayerName = sValue
                        End If
                    End If
                End If
                _rVal += aMem.Entities.SetDisplayVariable(aPropertyType, aNewValue, aSearchValue, aSearchType).Count
            Next i
            Return _rVal
        End Function

        Public Function TryGet(aNameOrHandleOrGUID As String, ByRef rBlock As dxfBlock, Optional aPassedType As dxxBlockReferenceTypes = dxxBlockReferenceTypes.Undefined) As Boolean
            rBlock = Nothing
            If String.IsNullOrWhiteSpace(aNameOrHandleOrGUID) Then Return False

            If Not colDXFBlocks.TryGet(Me, aNameOrHandleOrGUID, rBlock, aPassedType) Then
                If rBlock Is Nothing And aNameOrHandleOrGUID.StartsWith("_") Then
                    Dim myImage As dxfImage = Nothing
                    If GetImage(myImage) Then
                        If dxfArrowheads.IsDefault(aNameOrHandleOrGUID) Then
                            rBlock = dxfArrowheads.CreateArrowHeadBlock(myImage, aNameOrHandleOrGUID, True)
                            If rBlock IsNot Nothing Then
                                rBlock = AddToCollection(rBlock, bSuppressEvnts:=True, aImage:=myImage)
                                rBlock.ImageGUID = ImageGUID
                                Return True
                            End If
                        End If
                    End If
                End If
                Return False
            End If
            If Not String.IsNullOrEmpty(ImageGUID) Then rBlock.ImageGUID = ImageGUID


            Return True
        End Function
        Public Shared Function TryGet(aMembers As List(Of dxfBlock), aNameOrHandleOrGUID As String, ByRef rBlock As dxfBlock, aPassedType As dxxBlockReferenceTypes) As Boolean
            rBlock = Nothing
            If String.IsNullOrWhiteSpace(aNameOrHandleOrGUID) Then Return False
            If aMembers Is Nothing Then Return False
            If aMembers.Count <= 0 Then Return False
            aNameOrHandleOrGUID = aNameOrHandleOrGUID.Trim
            Select Case aPassedType
                Case dxxBlockReferenceTypes.Name
                    rBlock = aMembers.Find(Function(mem) String.Compare(mem.Name, aNameOrHandleOrGUID, ignoreCase:=True) = 0)
                Case dxxBlockReferenceTypes.Handle
                    rBlock = aMembers.Find(Function(mem) String.Compare(mem.Handle, aNameOrHandleOrGUID, ignoreCase:=True) = 0)
                Case dxxBlockReferenceTypes.GUID
                    rBlock = aMembers.Find(Function(mem) String.Compare(mem.GUID, aNameOrHandleOrGUID, ignoreCase:=True) = 0)
                Case dxxBlockReferenceTypes.LayoutHandle
                    rBlock = aMembers.Find(Function(mem) String.Compare(mem.LayoutHandle, aNameOrHandleOrGUID, ignoreCase:=True) = 0)
                Case dxxBlockReferenceTypes.LayoutName
                    rBlock = aMembers.Find(Function(mem) String.Compare(mem.LayoutName, aNameOrHandleOrGUID, ignoreCase:=True) = 0)
                Case Else
                    rBlock = aMembers.Find(Function(mem) String.Compare(mem.Name, aNameOrHandleOrGUID, ignoreCase:=True) = 0)
                    If rBlock Is Nothing Then rBlock = aMembers.Find(Function(mem) String.Compare(mem.Handle, aNameOrHandleOrGUID, ignoreCase:=True) = 0)
                    If rBlock Is Nothing Then rBlock = aMembers.Find(Function(mem) String.Compare(mem.GUID, aNameOrHandleOrGUID, ignoreCase:=True) = 0)
            End Select


            Return rBlock IsNot Nothing
        End Function

        Public Sub UpdatePaths(Optional bRegen As Boolean = False, Optional aImage As dxfImage = Nothing)
            If Not GetImage(aImage) Then Return
            Dim aMem As dxfBlock
            For i As Integer = 1 To Count
                aMem = Item(i)
                aMem.SetImage(aImage, False)
                aMem.Entities.UpdatePaths(bRegen)
            Next i
        End Sub
        Friend Sub ReleaseReferences()
            _ImagePtr = Nothing
            For Each block As dxfBlock In Me
                block.ReleaseReferences()
            Next
            'myBase.Clear()
        End Sub


#End Region 'Methods
#Region "IDisposable Implementation"
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' dispose managed state (managed objects)
                    'Clear(True, Nothing)
                    'free unmanaged resources (unmanaged objects) and override finalizer
                    'set large fields to null
                    ReleaseReferences()
                    Clear()

                End If
                disposedValue = True
            End If
        End Sub
        ' override finalizer only if 'Dispose(disposing As Boolean)' has code to free unmanaged resources
        Protected Overrides Sub Finalize()
            ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
            Dispose(disposing:=False)
            MyBase.Finalize()
        End Sub
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region 'IDisposable Implementation
#Region "IEnumerable Implementation"
        'Public Function GetEnumerator() As IEnumerator(Of dxfBlock) Implements IEnumerable(Of dxfBlock).GetEnumerator
        '    Return DirectCast(_Members, IEnumerable(Of dxfBlock)).GetEnumerator()
        'End Function
        'Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        '    Return DirectCast(_Members, IEnumerable).GetEnumerator()
        'End Function
#End Region 'IEnumerable Implementation
#Region "iHandleOwner"

        Public Function GetImage(ByRef aImage As dxfImage) As Boolean
            If aImage Is Nothing Then aImage = MyImage
            Return aImage IsNot Nothing

        End Function

        Friend ReadOnly Property HasReferenceTo_Image As Boolean
            Get
                If String.IsNullOrWhiteSpace(ImageGUID) Or _ImagePtr Is Nothing Then Return False
                Dim img As dxfImage = Nothing
                Return _ImagePtr.TryGetTarget(img)
            End Get
        End Property
        Friend Overridable ReadOnly Property MyImage As dxfImage
            Get

                If Not HasReferenceTo_Image Then Return Nothing
                Dim _rVal As dxfImage = Nothing
                Try
                    _ImagePtr.TryGetTarget(_rVal)
                    If _rVal IsNot Nothing AndAlso _rVal.Disposed Then _rVal = Nothing
                    If _rVal IsNot Nothing Then
                        If String.IsNullOrWhiteSpace(ImageGUID) Or String.Compare(ImageGUID, _rVal.GUID, ignoreCase:=True) <> 0 Then SetImage(Nothing, False)
                    End If
                Catch ex As Exception
                    _ImagePtr = Nothing

                End Try

                Return _rVal
            End Get
        End Property
        Dim _ImagePtr As WeakReference(Of dxfImage) = Nothing
        Friend Function SetImage(aImage As dxfImage, bDontReleaseOnNull As Boolean) As Boolean
            Dim img As dxfImage = aImage
            If img IsNot Nothing AndAlso img.Disposed Then
                img = Nothing
                Return False
            End If
            If img IsNot Nothing Then
                ImageGUID = img.GUID
                _ImagePtr = New WeakReference(Of dxfImage)(img)
                Return True
            Else
                If Not bDontReleaseOnNull Then
                    ImageGUID = ""
                    _ImagePtr = Nothing
                End If
                Return False
            End If
        End Function

        Private _Handlez As New THANDLES
        Public Property ReactorGUID As String Implements iHandleOwner.ReactorGUID
            Get
                Return _Handlez.ReactorGUID
            End Get
            Set(value As String)
                _Handlez.ReactorGUID = value
            End Set
        End Property

        Public Property BlockGUID As String Implements iHandleOwner.BlockGUID
            Get
                Return _Handlez.BlockGUID
            End Get
            Set(value As String)
                _Handlez.BlockGUID = value
            End Set
        End Property

        Public Property CollectionGUID As String Implements iHandleOwner.CollectionGUID
            Get
                Return _Handlez.CollectionGUID
            End Get
            Set(value As String)
                _Handlez.CollectionGUID = value
            End Set
        End Property

        Public Property BlockCollectionGUID As String Implements iHandleOwner.BlockCollectionGUID
            Get
                Return _Handlez.BlockCollectionGUID
            End Get
            Set(value As String)
                _Handlez.BlockCollectionGUID = value
            End Set
        End Property

        Public Property Handle As String Implements iHandleOwner.Handle
            Get
                Return _Handlez.Handle
            End Get
            Set(value As String)
                _Handlez.Handle = value

            End Set
        End Property

        Public Property Image As dxfImage
            '^the parent image asssociated to this entity
            Get
                If String.IsNullOrWhiteSpace(ImageGUID) Then Return Nothing
                Dim img As dxfImage = MyImage
                If img IsNot Nothing Then Return img
                img = dxfEvents.GetImage(ImageGUID)
                SetImage(img, False)
                Return img
            End Get
            Set(value As dxfImage)
                If value IsNot Nothing Then
                    ImageGUID = value.GUID
                Else
                    ImageGUID = ""
                End If
            End Set
        End Property

        Public Property ImageGUID As String Implements iHandleOwner.ImageGUID
            Get
                Return _Handlez.ImageGUID
            End Get
            Friend Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = String.Empty Else value = value.Trim
                _Handlez.ImageGUID = value
                If Not String.IsNullOrWhiteSpace(value) Then
                    Dim img As dxfImage = dxfEvents.GetImage(value)
                    If img Is Nothing Then
                        _Handlez.ImageGUID = String.Empty
                        _ImagePtr = Nothing
                    Else
                        _ImagePtr = New WeakReference(Of dxfImage)(img)
                    End If
                    MonitorMembers = img IsNot Nothing
                Else
                    _ImagePtr = Nothing
                    MonitorMembers = False
                End If

            End Set
        End Property

        Public Property Index As Integer Implements iHandleOwner.Index
            Get
                Return _Handlez.Index
            End Get
            Set(value As Integer)
                _Handlez.Index = value

            End Set
        End Property

        Public Property OwnerGUID As String Implements iHandleOwner.OwnerGUID
            Get
                Return _Handlez.OwnerGUID
            End Get
            Set(value As String)
                _Handlez.OwnerGUID = value
            End Set
        End Property

        Public Property SourceGUID As String Implements iHandleOwner.SourceGUID
            Get
                Return _Handlez.SourceGUID
            End Get
            Set(value As String)
                _Handlez.SourceGUID = value
            End Set
        End Property

        Public Property Domain As dxxDrawingDomains Implements iHandleOwner.Domain
            Get
                Return _Handlez.Domain
            End Get
            Set(value As dxxDrawingDomains)
                _Handlez.Domain = value
            End Set
        End Property

        Public Property Identifier As String Implements iHandleOwner.Identifier
            Get
                Return _Handlez.Identifier
            End Get
            Set(value As String)
                _Handlez.Identifier = value
            End Set
        End Property

        Public Property ObjectType As dxxFileObjectTypes Implements iHandleOwner.ObjectType
            Get
                Return dxxFileObjectTypes.Blocks
            End Get
            Set(value As dxxFileObjectTypes)
                value = dxxFileObjectTypes.Block
            End Set
        End Property

        Public Property OwnerType As dxxFileObjectTypes Implements iHandleOwner.OwnerType
            Get
                Return _Handlez.OwnerType
            End Get
            Set(value As dxxFileObjectTypes)
                _Handlez.OwnerType = value
            End Set
        End Property

        Public Property Name As String Implements iHandleOwner.Name
            Get
                Return _Handlez.Name
            End Get
            Friend Set(value As String)
                _Handlez.Name = value
            End Set
        End Property

        Public Property GUID As String Implements iHandleOwner.GUID
            Get
                Return _Handlez.GUID
            End Get
            Friend Set(value As String)
                _Handlez.GUID = value

            End Set
        End Property

        Friend Property Suppressed As Boolean Implements iHandleOwner.Suppressed
            Get
                Return False
            End Get
            Set
            End Set
        End Property


        Friend Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix Implements iHandleOwner.DXFFileProperties
            Throw New NotImplementedException
        End Function

        Friend Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As dxfPlane, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As dxoPropertyArray Implements iHandleOwner.DXFProps
            Throw New NotImplementedException
        End Function

#End Region 'iHandleOwner
    End Class 'colDXFBlocks
End Namespace
