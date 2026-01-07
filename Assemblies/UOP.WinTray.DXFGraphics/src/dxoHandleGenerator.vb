Imports SharpDX.Direct2D1.Effects
Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public Class dxoHandleGenerator
        Implements IDisposable
#Region "Members"
        Private _ImageGUID As String
        Private ImagePtr As WeakReference
        Private _IDS As TGENIDS
        Private _SaveIDS As TGENIDS
        Private _Init As Boolean
        Private _Verbose As Boolean
        Private _TempHandles As Boolean
        Private _TempHndls() As Integer
        Private _Members As List(Of THANDLE)
        Private _TempValues As List(Of Integer)
        Private _Disposed As Boolean
#End Region 'Members
#Region "Constructors"
        Friend Sub New(aImage As dxfImage, Optional aGUID As String = Nothing)
            If aImage IsNot Nothing Then
                If String.IsNullOrWhiteSpace(aGUID) Then aGUID = aImage.GUID
                _TempHandles = False
                ReDim _TempHndls(0)
                _Members = New List(Of THANDLE)
                _TempValues = New List(Of Integer)
                _IDS = New TGENIDS(ImageGUID)
                _ImageGUID = aGUID
                ImagePtr = New WeakReference(aImage)
                _Init = True
            Else
                ImagePtr = Nothing
                Init(aGUID)
            End If
        End Sub
        Friend Sub Init(Optional aImageGUID As String = "")
            If String.IsNullOrWhiteSpace(aImageGUID) Then aImageGUID = "" Else aImageGUID = aImageGUID.Trim
            _TempHandles = False
            _TempValues = New List(Of Integer)
            ReDim _TempHndls(0)
            ImageGUID = aImageGUID
            If Not String.IsNullOrWhiteSpace(aImageGUID) Then
                Dim img As dxfImage = Nothing
                If ImagePtr IsNot Nothing Then
                    If ImagePtr.IsAlive Then img = TryCast(ImagePtr.Target, dxfImage)
                End If
                If img IsNot Nothing Then
                    _ImageGUID = img.GUID
                    ImagePtr = New WeakReference(img)
                Else
                    _ImageGUID = ""
                    ImagePtr = Nothing
                End If
            Else
                ImagePtr = Nothing
            End If
            _Members = New List(Of THANDLE)
            Clear()
        End Sub
#End Region 'Constructors
#Region "Properties"
        Friend ReadOnly Property Count As Integer
            Get
                If _Disposed Or Not _Init Then Return 0
                Return _Members.Count
            End Get
        End Property
        Private _SuppressReuse As Boolean
        Public Property SuppressReuse As Boolean
            Get
                Return _SuppressReuse
            End Get
            Set(value As Boolean)
                _SuppressReuse = value
            End Set
        End Property
        Friend ReadOnly Property MaxHandle As Integer
            Get
                If Not _Init Then Return 0
                If _Members.Count <= 0 Then Return 0
                Dim max = (From member In _Members Order By member.Value Descending).First.Value
                Return max
            End Get
        End Property
        Friend ReadOnly Property Image As dxfImage
            Get
                Dim img As dxfImage = Nothing
                If ImagePtr IsNot Nothing Then
                    If ImagePtr.IsAlive Then
                        img = TryCast(ImagePtr.Target, dxfImage)
                    End If
                End If
                If img Is Nothing Then
                    If Not String.IsNullOrWhiteSpace(ImageGUID) Then img = dxfEvents.GetImage(ImageGUID)
                    If img IsNot Nothing Then
                        ImagePtr = New WeakReference(img)
                    End If
                End If
                If img Is Nothing Then
                    ImagePtr = Nothing
                    ImageGUID = ""
                End If
                Return img
            End Get
        End Property
        Public Property ImageGUID As String
            Get
                Return _ImageGUID
            End Get
            Friend Set(value As String)
                _ImageGUID = value
            End Set
        End Property
        Friend Property Verbose As Boolean
            Get
                Return _Verbose
            End Get
            Set(value As Boolean)
                _Verbose = value
            End Set
        End Property
        Friend Property TempHandles As Boolean
            Get
                Return _TempHandles
            End Get
            Set(value As Boolean)
                If value = _TempHandles Then Return
                ReDim _TempHndls(0)
                _TempHandles = value
            End Set
        End Property
#End Region 'Properties
#Region "Methods"
        Friend Sub ReleaseTempHandles()
            If _TempValues Is Nothing Then Return
            If _TempHndls.Count <= 0 Then Return
            For Each hndl As String In _TempHndls
                ReleaseHandle(hndl)
            Next
            _TempValues.Clear()
        End Sub
        Friend Sub Clear()
            If _Disposed Or Not _Init Then Return
            _Members = New List(Of THANDLE)
            _IDS = New TGENIDS(ImageGUID)
            _TempValues = New List(Of Integer)
            _Init = True
        End Sub

        Public Function AssignTo(aClass As dxfClass) As String
            Dim _rVal As String = String.Empty

            If Not _Init Then Init(ImageGUID)
            Dim hndl As String = aClass.Handle
            Dim created As Boolean
            hndl = NextHandle(aClass.Name, aOldHandle:=hndl, rNewHandleCreated:=created)
            aClass.Handle = hndl
            Return hndl
        End Function

        Friend Function AssignTo(aClass As TDXFCLASS) As String
            Dim _rVal As String = String.Empty

            If Not _Init Then Init(ImageGUID)
            Dim hndl As String = aClass._Props.ValueS(5)
            Dim created As Boolean
            hndl = NextHandle(aClass.Name, aOldHandle:=hndl, rNewHandleCreated:=created)
            aClass._Props.SetValGC(5, hndl, 1)
            Return hndl
        End Function



        Friend Function AssignToEntry(aEntry As dxfTableEntry, Optional aPreferedHandle As String = "") As String
            If aEntry Is Nothing Then Return String.Empty
            If Not _Init Then Init(ImageGUID)
            Dim _rVal As String = String.Empty
            Dim oldHndle As String
            Dim hgc As Integer = 5
            Dim created As Boolean
            If aEntry.EntryType = dxxReferenceTypes.DIMSTYLE Then hgc = 105
            If String.IsNullOrWhiteSpace(aEntry.GUID) Then aEntry.GUID = dxfEvents.NextEntryGUID(aEntry.EntryType)
            oldHndle = aEntry.Handle
            If oldHndle = "" Then oldHndle = aPreferedHandle.Trim
            _rVal = NextHandle(aEntry.GUID, oldHndle, rNewHandleCreated:=created, aOwnerName:=aEntry.Name)
            aEntry.Handle = _rVal
            Return _rVal
        End Function


        Friend Function AssignTo(aOwner As iHandleOwner, Optional bSuppressMembers As Boolean = False, Optional aPreferedHandle As String = "", Optional bSuppressTableHandle As Boolean = False) As String
            If aOwner Is Nothing Then Return String.Empty
            If String.IsNullOrWhiteSpace(aPreferedHandle) Then aPreferedHandle = String.Empty Else aPreferedHandle = aPreferedHandle.Trim()

            Dim iGUID As String = ImageGUID
            Dim _rVal As String = String.Empty
            Dim cnt As Integer
            Dim oldHndl As String = aOwner.Handle
            If String.IsNullOrWhiteSpace(oldHndl) Then
                bSuppressTableHandle = False
                oldHndl = aPreferedHandle.Trim()
            End If
            Dim hidx As Integer
            Dim created As Boolean

            Select Case True
                Case TypeOf aOwner Is dxfTable
#Region "TABLE"
                    Dim aTable As dxfTable = DirectCast(aOwner, dxfTable)
                    If String.IsNullOrWhiteSpace(aTable.GUID) Then aTable.GUID = dxfEvents.NextTableGUID(aTable.TableType)
                    Dim tGUID As String = aTable.GUID
                    aTable.ImageGUID = iGUID
                    If Not bSuppressTableHandle Then
                        _rVal = NextHandle(aTable.GUID, oldHndl, rNewHandleCreated:=created, aOwnerName:=aTable.TableTypeName)
                        aTable.Handle = _rVal
                    Else
                        _rVal = oldHndl
                    End If
                    hidx = -1

                    For Each aEntry As dxfTableEntry In aTable
                        If Not bSuppressMembers Then AssignTo(aEntry)
                        If aEntry.Domain = dxxDrawingDomains.Model Then
                            If Not aEntry.Suppressed Then cnt += 1
                        End If
                        'set the entries table handle
                        aEntry.Properties.SetVal(330, _rVal, hidx)
                        aEntry.ImageGUID = iGUID
                        aEntry.CollectionGUID = tGUID
                    Next
#End Region 'TABLE

                Case TypeOf aOwner Is colDXFBlocks
#Region "BLOCKS"
                    Dim aBlks As colDXFBlocks = DirectCast(aOwner, colDXFBlocks)
                    _rVal = NextHandle(aBlks.GUID, oldHndl, rNewHandleCreated:=created, aOwnerName:="Blocks")

                    aBlks.Handle = _rVal
                    If Not bSuppressMembers Then
                        For Each blk As dxfBlock In aBlks
                            AssignTo(blk)
                        Next
                    End If
#End Region 'BLOCKS

                Case TypeOf aOwner Is colDXFEntities

#Region "ENTITIES"
                    Dim ents As colDXFEntities = DirectCast(aOwner, colDXFEntities)
                    For Each ent In ents
                        _rVal = AssignTo(ent)
                    Next

#End Region 'ENTITIES

                Case TypeOf aOwner Is dxfAttribute

#Region "ATTRIBS"
                    Dim atrrib As dxfAttribute = DirectCast(aOwner, dxfAttribute)
                    _rVal = NextHandle(atrrib.GUID, atrrib.Handle, rNewHandleCreated:=created, aOwnerName:="Attribute")
#End Region 'ATTRIBS

            End Select




            Return _rVal
        End Function
        Friend Function NextHandle(aHandleOwner As dxfHandleOwner, ByRef rNewHandleCreated As Boolean, Optional bSwapGUID As Boolean = False, Optional bNewHandlesOnly As Boolean = False) As String
            If _Disposed Then Return String.Empty
            If Not _Init Then Init(ImageGUID)
            If aHandleOwner Is Nothing Then Return "0"
            Return NextHandle(aHandleOwner.GUID, aHandleOwner.Handle, rNewHandleCreated:=rNewHandleCreated, aHandleOwner.Name, bSwapGUID, bNewHandlesOnly)
        End Function
        Friend Function NextHandle(aOwnerGUID As String, aOldHandle As String, ByRef rNewHandleCreated As Boolean, Optional aOwnerName As String = "", Optional bSwapGUID As Boolean = False, Optional bNewHandlesOnly As Boolean = False) As String
            If _Disposed Then Return String.Empty
            If Not _Init Then Init(ImageGUID)
            If String.IsNullOrWhiteSpace(aOwnerGUID) Then aOwnerGUID = "" Else aOwnerGUID = aOwnerGUID.Trim
            rNewHandleCreated = False
            Dim aMem As THANDLE = Nothing
            Dim bCreateNew As Boolean = True
            Dim iHndle As Integer = 0
            Dim idx As Integer
            Dim bUsed As Boolean = False
            Dim bChnged As Boolean = False
            iHndle = TVALUES.HexToInteger(aOldHandle)
            If iHndle <= 0 Then iHndle = 0
            'see if the handle was already assigned
            If iHndle > 0 Then
                If TryGetHandle(iHndle, aMem, idx, False) Then
                    If aMem.OwnerGUID <> "" And aMem.OwnerGUID <> aOwnerGUID Then
                        If Not bSwapGUID Then
                            'the existing handle is owned by somebody else so we cant let them keep it
                            idx = 0
                        Else
                            'the existing handle is owned by the same caller so let them keep it
                            aMem.OwnerGUID = aOwnerGUID
                            aMem.OwnerName = aOwnerName
                        End If
                    End If
                End If
            End If
            If bNewHandlesOnly And iHndle > 0 Then
                ReleaseHandle(aOldHandle)
                'System.Diagnostics.Debug.WriteLine("Release-" & ContainsHandle(iHndle).ToString)
            End If
            If idx <= 0 Then 'a new handle is required
                'see if this GUID already has a handle
                If TryGetHandleByGUID(aOwnerGUID, aMem, idx, False) Then
                    iHndle = aMem.Value
                    If iHndle <= 0 Then
                        idx = 0
                        iHndle = 0
                    Else
                        aMem.OwnerGUID = aOwnerGUID
                    End If
                End If
            End If
            rNewHandleCreated = idx <= 0
            If rNewHandleCreated Then  'a new handle is required
                bChnged = True
                iHndle = NextHandle(rUsed:=bUsed, idx)
                If bUsed Then
                    aMem = _Members(idx - 1)
                    aMem.Value = iHndle
                Else
                    aMem = New THANDLE("") With {.Value = iHndle}
                    _Members.Add(aMem)
                    idx = Count
                End If
            End If
            If String.IsNullOrEmpty(aOwnerGUID) Then aOwnerGUID = aMem.OwnerGUID
            If String.IsNullOrEmpty(aOwnerName) Then aOwnerName = aMem.OwnerName
            aMem.OwnerGUID = aOwnerGUID
            aMem.OwnerName = aOwnerName
            If aOwnerGUID <> aMem.OwnerGUID Or aOwnerName <> aMem.OwnerName Then bChnged = True
            _Members(idx - 1) = aMem
            If rNewHandleCreated Then
                If TempHandles Then
                    _TempValues.Add(iHndle)
                End If
                If _Verbose Then
                    Dim aStr As String = $"GUID:{ aMem.OwnerGUID } :: NEW HANDLE:{ aMem.Handle}"
                    If aMem.OwnerName <> "" Then
                        aStr += $" :: NAME:{ aMem.OwnerName}"
                    End If
                    If bUsed Then
                        aStr += $" :: Reused={ bUsed}"
                    End If
                    System.Diagnostics.Debug.WriteLine(aStr)
                End If
            End If
            Return TVALUES.To_Handle(iHndle)
        End Function
        Friend Function NextHandle(ByRef rUsed As Boolean, ByRef rUsedIdex As Integer) As Integer
            rUsed = False
            rUsedIdex = 0
            If _Disposed Then Return 0
            If Not _Init Then Init(ImageGUID)
            Dim _rVal As Integer
            Dim cnt As Integer = _Members.Count
            Dim aMem As THANDLE
            If Count > 0 And Not SuppressReuse Then
                rUsedIdex = _Members.FindIndex(Function(mem) mem.Released = True) + 1
                If rUsedIdex > 0 Then
                    rUsed = True
                    aMem = _Members(rUsedIdex - 1)
                    _rVal = aMem.Value
                    Return _rVal
                End If
            End If
            If Not rUsed Then
                _rVal = MaxHandle + 1
                Do Until Not ContainsHandle(_rVal)
                    _rVal += 1
                Loop
            End If
            Return _rVal
        End Function
        Friend Sub SaveUsedHandles(aOwner As TTABLE)
            If aOwner.Handle <> "" And aOwner.Handle <> "0" Then SaveUsedHandle(aOwner.Handle, aOwner.GUID, aOwner.Name)
            Dim aEntry As TTABLEENTRY
            For i As Integer = 1 To aOwner.Count
                aEntry = aOwner.Item(i)
                If aEntry.Handle <> "" And aEntry.Handle <> "0" Then SaveUsedHandle(aEntry.Handle, aEntry.GUID, aEntry.Name)
            Next
        End Sub

        Friend Sub SaveUsedHandles(aOwner As dxfTable)
            If aOwner Is Nothing Then Return
            If aOwner.Handle <> "" And aOwner.Handle <> "0" Then SaveUsedHandle(aOwner.Handle, aOwner.GUID, aOwner.Name)
            For Each entry As dxfTableEntry In aOwner
                If entry.Handle <> "" And entry.Handle <> "0" Then SaveUsedHandle(entry.Handle, entry.GUID, entry.Name)
            Next

        End Sub

        Friend Sub SaveUsedHandles(aObjects As colDXFObjects)
            If aObjects Is Nothing Then Return
            For Each aObj As dxfObject In aObjects
                If aObj.Handle <> "" And aObj.Handle <> "0" Then SaveUsedHandle(aObj.Handle, aObj.GUID, aObj.Name)
            Next

        End Sub
        Friend Sub SaveUsedHandles(aBlocks As colDXFBlocks)
            If aBlocks Is Nothing Then Return

            For Each blk As dxfBlock In aBlocks
                If blk.Handle <> "" And blk.Handle <> "0" Then SaveUsedHandle(blk.Handle, blk.GUID, blk.Name)
                For Each ent As dxfEntity In blk.Entities
                    If ent.Handle <> "" And ent.Handle <> "0" Then SaveUsedHandle(ent.Handle, ent.GUID, ent.Name)
                Next
            Next


        End Sub
        Friend Function SaveUsedHandle(aHandle As String, aOwnerGUID As String, Optional aOwnerName As String = "") As THANDLE
            Dim aMem As New THANDLE("")
            If _Disposed Then Return aMem
            If Not _Init Then Clear()
            If String.IsNullOrEmpty(aHandle) Then Return aMem
            aHandle = Trim(aHandle)
            If aHandle = "" Then Return Nothing
            Dim iVal As Integer = TVALUES.HexToInteger(aHandle)
            If iVal = 0 Then Return Nothing
            Dim idx As Integer
            If Not TryGetHandle(aHandle, aMem, idx) Then
                SaveHandle(iVal, aOwnerGUID, aOwnerName)
            Else
                aMem.OwnerGUID = aOwnerGUID
                aMem.OwnerName = aOwnerName
                aMem.Index = idx
                _Members(idx - 1) = aMem
            End If
            Return aMem
            'Debug.Print "sAVING hANDLE = " & aVal & "(" & aHandle & ")"
        End Function
        Friend Function SaveHandle(aHandle As Integer, aOwnerGUID As String, Optional aOwnerName As String = "") As Boolean
            Return Add(New THANDLE(aOwnerGUID) With {.OwnerName = aOwnerName, .Value = aHandle})
        End Function
        Friend Function NextAttributeTag() As String
            Dim _rVal As String = String.Empty
            If Not _Init Then Init(ImageGUID)
            _rVal = "ATRRIB_" & _IDS.AttributeIndex
            _IDS.AttributeIndex += 1
            Return _rVal
        End Function
        Public Function HandleIsUsed(aHandle As String, ByRef rOwnerGUID As String) As Boolean
            rOwnerGUID = ""
            Dim aMem As THANDLE = Nothing
            Dim bRels As Boolean = False
            Dim idx As Integer
            If TryGetHandle(aHandle, aMem, idx, bReleased:=bRels) Then
                rOwnerGUID = aMem.OwnerGUID
                Return True
            Else
                Return False
            End If
        End Function
        Friend Function TryGetHandle(aHandle As String, ByRef rHandle As THANDLE, ByRef rIndex As Integer, Optional bReleased As Boolean = False) As Boolean
            rIndex = 0
            rHandle = New THANDLE("")
            If Not _Init Then Return False
            If String.IsNullOrEmpty(aHandle) Then Return False
            Dim iVal As Integer = TVALUES.HexToInteger(aHandle)
            If iVal = 0 Then Return False
            Dim idx As Integer = 0
            If Not ContainsHandle(iVal, rIndex, bReleased) Then Return False
            rHandle.Index = rIndex
            rHandle = _Members(rIndex - 1)
            _Members(rIndex - 1) = rHandle
            Return True
        End Function
        Friend Function NextDimBlockName() As String
            Dim _rVal As String = String.Empty
            If Not _Init Then Init(ImageGUID)
            _rVal = "*D" & _IDS.DimBlockIndex
            _IDS.DimBlockIndex += 1
            Return _rVal
        End Function
        Friend Sub UpdateDimBlockIndex(aDimBlockName As String)
            Dim aFlag As Boolean
            Dim sPrefix As String = String.Empty
            Dim aIndex As Integer = dxfUtils.ExtractTrailingIndex(aDimBlockName, aFlag, sPrefix, sPrefix)
            If aFlag Then Return
            If aIndex > _IDS.DimBlockIndex Then _IDS.DimBlockIndex = aIndex
        End Sub
        Friend Function NextShapeName() As String
            If Not _Init Then Init(ImageGUID)
            Return "SHAPE_" & _IDS.ShapeIndex
            _IDS.ShapeIndex += 1
        End Function
        Friend Function NextSymbolName(aSymbolType As dxxSymbolTypes, aArrowType As dxxArrowTypes) As String
            Dim _rVal As String = String.Empty
            If Not _Init Then Init(ImageGUID)
            If aSymbolType = dxxSymbolTypes.Arrow Then
                If aArrowType = dxxArrowTypes.Axis Then
                    _rVal = "AXIS_" & _IDS.ArrowIDS(0)
                    _IDS.ArrowIDS(0) += 1
                ElseIf aArrowType = dxxArrowTypes.Pointer Then
                    _rVal = "POINTER_" & _IDS.ArrowIDS(1)
                    _IDS.ArrowIDS(1) += 1
                ElseIf aArrowType = dxxArrowTypes.Section Then
                    _rVal = "SECTIONARROWS_" & _IDS.ArrowIDS(2)
                    _IDS.ArrowIDS(2) += 1
                ElseIf aArrowType = dxxArrowTypes.View Then
                    _rVal = "VIEWARROWS_" & _IDS.ArrowIDS(3)
                    _IDS.ArrowIDS(3) += 1
                Else
                    _rVal = "ARROW_" & _IDS.SymbolIDS(0)
                    _IDS.SymbolIDS(0) += 1
                End If
            ElseIf aSymbolType = dxxSymbolTypes.Bubble Then
                _rVal = "BUBBLE_" & _IDS.SymbolIDS(1)
                _IDS.SymbolIDS(1) += 1
            ElseIf aSymbolType = dxxSymbolTypes.DetailBubble Then
                _rVal = "DETAIL_" & _IDS.SymbolIDS(2)
                _IDS.SymbolIDS(2) += 1
            ElseIf aSymbolType = dxxSymbolTypes.Weld Then
                _rVal = "WELD_" & _IDS.SymbolIDS(3)
                _IDS.SymbolIDS(3) += 1
            Else
                _rVal = "SYMBOL_" & _IDS.SymbolIDS(4)
                _IDS.SymbolIDS(4) += 1
            End If
            Return _rVal
        End Function
        Friend Function NextTableName(aBlocks As colDXFBlocks) As String
            Dim _rVal As String = String.Empty
            If Not _Init Then Init(ImageGUID)
            _rVal = "TABLE_" & _IDS.TableIndex
            _IDS.TableIndex += 1
            Dim aBlock As dxfBlock = Nothing
            If aBlocks IsNot Nothing Then
                aBlock = aBlocks.GetByName(_rVal)
                Do Until aBlock Is Nothing
                    _rVal = "Table_" & _IDS.TableIndex
                    _IDS.TableIndex += 1
                    aBlock = aBlocks.GetByName(_rVal)
                Loop
            End If
            Return _rVal
        End Function
        Friend Sub Print()
            If Count <= 0 Then Return
            Dim aMem As THANDLE
            For i As Integer = 1 To Count
                aMem = Get_Item(i)
                System.Diagnostics.Debug.WriteLine(aMem.ToString())
            Next i
        End Sub
        Friend Sub SaveState(Optional bReset As Boolean = False)
            If Not _Init Then Init(ImageGUID)
            If Not bReset Then
                _SaveIDS = _IDS
                _IDS.Pointer = Count
                _IDS.Max = MaxHandle
                SuppressReuse = True
            Else
                ReduceTo(_IDS.Pointer)
                SuppressReuse = False
                _IDS = _SaveIDS
            End If
        End Sub
        Public Sub ReduceTo(aCount As Integer)
            If Not _Init Or _Disposed Then Return
            If aCount < 0 Then aCount = 0
            If aCount > Count Or Count = 0 Then Return
            If aCount = 0 Then
                _Members = New List(Of THANDLE)
            Else
                _Members.RemoveRange(aCount - 1, Count - aCount)
            End If
        End Sub
        Friend Function ReleaseHandles(aEntities As colDXFEntities) As Boolean
            If _Disposed OrElse aEntities Is Nothing Then Return False
            Dim _rVal As Boolean = False
            If Not _Init Then Init(ImageGUID)
            Dim aEntity As dxfEntity
            Dim hndl As String
            Dim i As Integer
            Dim aMem As THANDLE = Nothing
            For i = 1 To aEntities.Count
                aEntity = aEntities.Item(i)
                hndl = aEntity.Handle
                If hndl <> "" Then
                    If ReleaseHandle(hndl) Then _rVal = True
                End If
            Next i
            Return _rVal
        End Function

        Friend Function ReleaseHandles(aBlocks As IEnumerable(Of dxfBlock)) As Boolean
            If _Disposed OrElse aBlocks Is Nothing Then Return False
            Dim _rVal As Boolean = False
            If Not _Init Then Init(ImageGUID)

            If TypeOf aBlocks Is colDXFBlocks Then
                Dim col As colDXFBlocks = DirectCast(aBlocks, colDXFBlocks)
                If ReleaseHandle(col.Handle) Then _rVal = True
            End If
            For Each aMem As dxfBlock In aBlocks
                If ReleaseHandle(aMem.Handle) Then _rVal = True
                If ReleaseHandle(aMem.BlockRecordHandle) Then _rVal = True
                If ReleaseHandle(aMem.EndBlockHandle) Then _rVal = True
                If aMem.HasEntities Then
                    If ReleaseHandles(aMem.Entities) Then _rVal = True
                End If
                aMem.Clear(False, Nothing)
                aMem.ImageGUID = ""
                aMem.CollectionGUID = ""

            Next
            Return _rVal
        End Function

        Friend Function AssignTo(aHandleOwner As dxfHandleOwner, Optional bSwapGUID As Boolean = False, Optional bNewHandlesOnly As Boolean = False,
                                     Optional bSuppressMembers As Boolean = False) As String
            If aHandleOwner Is Nothing Then Return String.Empty
            Dim _rVal As String = String.Empty
            'On Error Resume Next
            Dim aDStyle As dxoDimStyle
            Dim aEnt As dxfEntity
            Dim aEnts As colDXFEntities
            Dim aBlk As dxfBlock

            Dim aFlg As Boolean


            Dim j As Integer

            Dim myimage As dxfImage = Image
            aHandleOwner.SetImage(myimage, False)

            Select Case True
                Case TypeOf aHandleOwner Is dxfEntity
#Region "Entity"
                    aEnt = DirectCast(aHandleOwner, dxfEntity)
                    '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                    ''AssignEnt:
                    If aEnt.GraphicType = dxxGraphicTypes.Dimension Then
                        Dim aDim As dxeDimension = aEnt
                        If aDim.BlockName = "" Then
                            Dim blknm As String = NextDimBlockName()
                            aDim.Name = blknm
                            aDim.BlockName = blknm
                        End If
                    End If
                    aEnt.Handle = NextHandle(aEnt, rNewHandleCreated:=aFlg, bSwapGUID:=bSwapGUID, bNewHandlesOnly:=bNewHandlesOnly)
                    If aEnt.HasDimStyle Then
                        aDStyle = aEnt.DimStyle
                        If aDStyle IsNot Nothing Then aDStyle.OwnerGUID = aEnt.GUID
                    End If
                    Dim subEnts As List(Of dxfEntity) = aEnt.PersistentSubEntities
                    Dim bEnt As dxfEntity = aEnt.ReactorEntity
                    If subEnts IsNot Nothing Then
                        For k As Integer = 1 To subEnts.Count
                            bEnt = subEnts.Item(k - 1)
                            AssignTo(bEnt, bNewHandlesOnly:=aFlg)
                        Next
                    End If
                    _rVal = aEnt.Handle

                    If aEnt.GraphicType = dxxGraphicTypes.Insert Then
                        Dim isert As dxeInsert = DirectCast(aEnt, dxeInsert)
                        Dim attrs As dxfAttributes = isert.Attributes
                        If attrs.Count > 0 Then
                            For Each attr As dxfAttribute In attrs
                                AssignTo(attr, aPreferedHandle:=attr.Handle, bSuppressTableHandle:=True)
                            Next

                        End If
                    End If

                    '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                    _rVal = aHandleOwner.Handle
#End Region 'Entity

                Case TypeOf aHandleOwner Is dxfTableEntry
#Region "TableEntry"
                    Dim aEntry As dxfTableEntry = DirectCast(aHandleOwner, dxfTableEntry)
                    Dim oldHndle As String
                    Dim hgc As Integer = 5
                    Dim created As Boolean
                    If aEntry.EntryType = dxxReferenceTypes.DIMSTYLE Then hgc = 105
                    If String.IsNullOrWhiteSpace(aEntry.GUID) Then aEntry.GUID = dxfEvents.NextEntryGUID(aEntry.EntryType)
                    oldHndle = aEntry.Handle
                    _rVal = NextHandle(aEntry.GUID, oldHndle, rNewHandleCreated:=created, aOwnerName:=aEntry.Name)
                    aEntry.Handle = _rVal

#End Region 'TableEntry
                Case TypeOf aHandleOwner Is dxfBlock

#Region "Block"
                    aBlk = DirectCast(aHandleOwner, dxfBlock)
                    aBlk.Handle = NextHandle(aBlk, rNewHandleCreated:=aFlg, bNewHandlesOnly:=bNewHandlesOnly)
                    'If Not aBlk.Suppressed Then
                    Dim created As Boolean
                    Dim aBR As dxoBlockRecord = aBlk.BlockRecord
                    If aFlg Then bNewHandlesOnly = True
                    aBR.Handle = NextHandle(aBR.GUID, aBR.Handle, rNewHandleCreated:=created, aOwnerName:=$"{aBlk.Name }.BlockRecord", bNewHandlesOnly:=bNewHandlesOnly)
                    aBlk.EndBlockHandle = NextHandle($"{aBlk.GUID }.EndBlock", aBlk.EndBlockHandle, rNewHandleCreated:=created, aOwnerName:=$"{aBlk.Name }.EndBlock", bNewHandlesOnly:=bNewHandlesOnly)
                    If aBlk.HasEntities Then
                        aEnts = aBlk.Entities
                        For j = 1 To aEnts.Count
                            aEnt = aEnts.Item(j)
                            '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                            ''AssignEnt:
                            If aEnt.Domain <> dxxDrawingDomains.Screen Then
                                aEnt.Handle = NextHandle(aEnt, rNewHandleCreated:=aFlg, bSwapGUID:=bSwapGUID, bNewHandlesOnly:=bNewHandlesOnly)
                            End If
                            If aEnt.HasDimStyle Then
                                aDStyle = aEnt.DimStyle
                                If aDStyle IsNot Nothing Then aDStyle.OwnerGUID = aEnt.GUID
                            End If
                            _rVal = aEnt.Handle
                            '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                        Next j
                    End If
                    _rVal = aBlk.Handle

#End Region 'Block

'                Case TypeOf aHandleOwner Is colDXFEntities
'#Region "Entities"
'                    aEnts = DirectCast(aHandleOwner, colDXFEntities)
'                    aEnts.Handle = NextHandle(aEnts, rNewHandleCreated:=aFlg, bSwapGUID, bNewHandlesOnly:=bNewHandlesOnly)
'                    For i = 1 To aEnts.Count
'                        aEnt = aEnts.Item(i)
'                        AssignTo(aEnt, bSwapGUID, bNewHandlesOnly, bSuppressMembers:=False)  'recursion
'                    Next i
'#End Region 'Entities
                Case TypeOf aHandleOwner Is dxfObject
#Region "Objects"
                    aHandleOwner.Handle = NextHandle(aHandleOwner, rNewHandleCreated:=aFlg, bSwapGUID:=bSwapGUID)
#End Region 'Objects
                Case Else
                    aHandleOwner.Handle = NextHandle(aHandleOwner, rNewHandleCreated:=aFlg, bSwapGUID:=bSwapGUID)
            End Select


            '            Select Case aHandleOwner.FileObjectType
            '                Case dxxFileObjectTypes.Entity
            '#Region "Entity"
            '                    aEnt = TryCast(aHandleOwner, dxfEntity)
            '                    '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            '                    ''AssignEnt:
            '                    If aEnt.GraphicType = dxxGraphicTypes.Dimension Then
            '                        Dim aDim As dxeDimension = aEnt
            '                        If aDim.BlockName = "" Then
            '                            Dim blknm As String = NextDimBlockName()
            '                            aDim.Name = blknm
            '                            aDim.BlockName = blknm
            '                        End If
            '                    End If
            '                    aEnt.Handle = NextHandle(aEnt, rNewHandleCreated:=aFlg, bSwapGUID:=bSwapGUID, bNewHandlesOnly:=bNewHandlesOnly)
            '                    If aEnt.HasDimStyle Then
            '                        aDStyle = aEnt.DimStyle
            '                        If aDStyle IsNot Nothing Then aDStyle.OwnerGUID = aEnt.GUID
            '                    End If
            '                    Dim subEnts As List(Of dxfEntity) = aEnt.PersistentSubEntities
            '                    Dim bEnt As dxfEntity = aEnt.ReactorEntity
            '                    If subEnts IsNot Nothing Then
            '                        For k As Integer = 1 To subEnts.Count
            '                            bEnt = subEnts.Item(k - 1)
            '                            AssignTo(bEnt, bNewHandlesOnly:=aFlg)
            '                        Next
            '                    End If
            '                    _rVal = aEnt.Handle

            '                    If aEnt.GraphicType = dxxGraphicTypes.Insert Then
            '                        Dim isert As dxeInsert = DirectCast(aEnt, dxeInsert)
            '                        Dim attrs As dxfAttributes = isert.Attributes
            '                        If attrs.Count > 0 Then
            '                            For Each attr As dxfAttribute In attrs
            '                                AssignTo(attr, aPreferedHandle:=attr.Handle, bSuppressTableHandle:=True)
            '                            Next

            '                        End If
            '                    End If

            '                    '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            '                    _rVal = aHandleOwner.Handle
            '#End Region 'Entity

            '                Case dxxFileObjectTypes.TableEntry
            '#Region "TableEntry"
            '                    Dim aTBlE As dxfTableEntry = aHandleOwner

            '                    AssignToEntry(aHandleOwner)

            '#End Region 'TableEntry
            '                Case dxxFileObjectTypes.Block

            '#Region "Block"
            '                    aBlk = TryCast(aHandleOwner, dxfBlock)
            '                    aBlk.Handle = NextHandle(aBlk, rNewHandleCreated:=aFlg, bNewHandlesOnly:=bNewHandlesOnly)
            '                    'If Not aBlk.Suppressed Then
            '                    Dim created As Boolean
            '                    Dim aBR As dxoBlockRecord = aBlk.BlockRecord
            '                    If aFlg Then bNewHandlesOnly = True
            '                    aBR.Handle = NextHandle(aBR.GUID, aBR.Handle, rNewHandleCreated:=created, aOwnerName:=$"{aBlk.Name }.BlockRecord", bNewHandlesOnly:=bNewHandlesOnly)
            '                    aBlk.EndBlockHandle = NextHandle($"{aBlk.GUID }.EndBlock", aBlk.EndBlockHandle, rNewHandleCreated:=created, aOwnerName:=$"{aBlk.Name }.EndBlock", bNewHandlesOnly:=bNewHandlesOnly)
            '                    If aBlk.HasEntities Then
            '                        aEnts = aBlk.Entities
            '                        For j = 1 To aEnts.Count
            '                            aEnt = aEnts.Item(j)
            '                            '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            '                            ''AssignEnt:
            '                            If aEnt.Domain <> dxxDrawingDomains.Screen Then
            '                                aEnt.Handle = NextHandle(aEnt, rNewHandleCreated:=aFlg, bSwapGUID:=bSwapGUID, bNewHandlesOnly:=bNewHandlesOnly)
            '                            End If
            '                            If aEnt.HasDimStyle Then
            '                                aDStyle = aEnt.DimStyle
            '                                If aDStyle IsNot Nothing Then aDStyle.OwnerGUID = aEnt.GUID
            '                            End If
            '                            _rVal = aEnt.Handle
            '                            '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            '                        Next j
            '                    End If
            '                    _rVal = aBlk.Handle

            '#End Region 'Block

            '                Case dxxFileObjectTypes.Entities
            '#Region "Entities"
            '                    aEnts = TryCast(aHandleOwner, colDXFEntities)
            '                    aEnts.Handle = NextHandle(aEnts, rNewHandleCreated:=aFlg, bSwapGUID, bNewHandlesOnly:=bNewHandlesOnly)
            '                    For i = 1 To aEnts.Count
            '                        aEnt = aEnts.Item(i)
            '                        AssignTo(aEnt, bSwapGUID, bNewHandlesOnly, bSuppressMembers:=False)  'recursion
            '                    Next i
            '#End Region 'Entities
            '                Case dxxFileObjectTypes.DXFObject
            '#Region "Objects"
            '                    aHandleOwner.Handle = NextHandle(aHandleOwner, rNewHandleCreated:=aFlg, bSwapGUID:=bSwapGUID)
            '#End Region 'Objects
            '                Case Else
            '                    aHandleOwner.Handle = NextHandle(aHandleOwner, rNewHandleCreated:=aFlg, bSwapGUID:=bSwapGUID)
            '            End Select
            Return _rVal
        End Function
        Friend Function Release(aHandleOwner As dxfHandleOwner, Optional bSuppressMembers As Boolean = False) As Boolean
            If aHandleOwner Is Nothing Then Return String.Empty
            Dim _rVal As Boolean = ReleaseHandle(aHandleOwner.Handle)
            Dim aEnt As dxfEntity
            Dim aEnts As colDXFEntities
            Dim aBlk As dxfBlock

            Dim hndl As String = String.Empty
            aHandleOwner.SetImage(Nothing, False)
            Select Case aHandleOwner.FileObjectType
                Case dxxFileObjectTypes.Entity
#Region "Entity"
                    aEnt = TryCast(aHandleOwner, dxfEntity)
                    Dim bEnt As dxfEntity
                    Dim subEnts As List(Of dxfEntity) = aEnt.PersistentSubEntities
                    If subEnts IsNot Nothing Then
                        For i As Integer = 1 To subEnts.Count
                            bEnt = subEnts.Item(i - 1)
                            bEnt.SetImage(Nothing, False)
                            If ReleaseHandle(bEnt.Handle) Then _rVal = True
                        Next
                    End If
#End Region 'Entity

                Case dxxFileObjectTypes.TableEntry
#Region "TableEntry"
#End Region 'TableEntry
                Case dxxFileObjectTypes.Block
#Region "Block"
                    aBlk = TryCast(aHandleOwner, dxfBlock)
                    If ReleaseHandle(aBlk.BlockRecordHandle) Then _rVal = True
                    If ReleaseHandle(aBlk.EndBlockHandle) Then _rVal = True
                    aEnts = aBlk.Entities
                    For i As Integer = 1 To aEnts.Count
                        aEnt = aEnts.Item(i)
                        If Release(aEnt) Then _rVal = True
                    Next
#End Region 'Block


                Case dxxFileObjectTypes.DXFObject
#Region "Objects"
#End Region 'Objects
                Case Else
            End Select
            Return _rVal
        End Function

        Friend Function Release(aHandleOwner As iHandleOwner, Optional bSuppressMembers As Boolean = False) As Boolean
            If aHandleOwner Is Nothing Then Return String.Empty
            Dim _rVal As Boolean = ReleaseHandle(aHandleOwner.Handle)


            Dim hndl As String = String.Empty
            Select Case aHandleOwner.ObjectType
                Case dxxFileObjectTypes.Entity
                Case dxxFileObjectTypes.Table
#Region "Table"
                    Dim aTBl As dxfTable = TryCast(aHandleOwner, dxfTable)
                    For i As Integer = 1 To aTBl.Count
                        If ReleaseHandle(aTBl.Item(i).Handle) Then _rVal = True
                    Next
#End Region 'Table
                Case dxxFileObjectTypes.Blocks
                    Dim blocks As colDXFBlocks = DirectCast(aHandleOwner, colDXFBlocks)
                    For Each block In blocks
                        If Release(block) Then _rVal = True
                    Next
#Region "Blocks"
#End Region 'Blocks
                Case dxxFileObjectTypes.Entities
#Region "Entities"
                    Dim aEnts As colDXFEntities = DirectCast(aHandleOwner, colDXFEntities)
                    For Each ent In aEnts
                        If Release(ent) Then _rVal = True
                    Next

#End Region 'Entities
                Case dxxFileObjectTypes.DXFObject
#Region "Objects"
#End Region 'Objects
                Case Else
            End Select
            Return _rVal
        End Function
        Friend Function ReleaseHandle(aHandle As String) As Boolean
            If _Disposed OrElse Not _Init OrElse String.IsNullOrWhiteSpace(aHandle) Then Return False
            Dim iVal As Integer = TVALUES.HexToInteger(aHandle)
            If iVal = 0 Then Return False
            Dim idx As Integer = _Members.FindIndex(Function(mem) mem.Value = iVal)
            If idx < 0 Then Return False
            Dim hndl As THANDLE = _Members(idx)
            hndl.Released = True
            _Members(idx) = hndl
            Return True
        End Function
        Friend Function Add(aHandle As THANDLE) As Boolean
            If _Disposed OrElse Not _Init OrElse aHandle.Value <= 0 Then Return False
            If aHandle.OwnerGUID = "" Then
                If dxfUtils.RunningInIDE Then
                    'Throw New Exception("TDICTIONARY_HANDLES.Add: Owner GUID Cannot Be Null")
                    Return False
                End If
            End If
            If Not _Init Then Clear()
            Dim cnt As Integer = _Members.Count
            Dim hndl As THANDLE = Nothing
            Dim idx As Integer = 0
            Dim aFlg As Boolean = TryGetHandle(aHandle.Value, hndl, idx, False)
            Try
                If aFlg Then
                    Throw New Exception("The Passed Handle Is already Used")
                End If
                Dim newCnt As Integer = cnt + 1
                aHandle.Index = newCnt
                aHandle.Released = False
                _Members.Add(aHandle)
                Return True
            Catch ex As Exception
                Throw ex
                Return False
            End Try
        End Function
        Friend Function TryGetHandle(aHandle As Integer, ByRef rHandle As THANDLE, ByRef rIndex As Integer, Optional bReleased As Boolean = False) As Boolean
            rIndex = 0
            rHandle = New THANDLE("")
            If Not _Init Then Return False
            Dim idx As Integer = 0
            If Not ContainsHandle(aHandle, rIndex, bReleased) Then Return False
            rHandle = _Members.Item(rIndex - 1)
            Return True
        End Function
        Friend Function TryGetHandleByGUID(aGUID As String, ByRef rHandle As THANDLE, ByRef rIndex As Integer, Optional bReleased As Boolean = False) As Boolean
            rIndex = 0
            rHandle = New THANDLE("")
            If Not _Init Or _Disposed Or String.IsNullOrWhiteSpace(aGUID) Then Return False
            Dim idx As Integer = 0
            If Not ContainsGUID(aGUID, rIndex, bReleased) Then Return False
            rHandle = _Members(rIndex - 1)
            Return True
        End Function
        Friend Function ContainsGUID(aGUID As String, ByRef rIndex As Integer, Optional bReleased As Boolean = False) As Boolean
            rIndex = 0
            If Not _Init Or _Disposed Then Return False
            If _Members.Count <= 0 Then Return False
            Dim idx As Integer = _Members.FindIndex(Function(mem) String.Compare(mem.OwnerGUID, aGUID, ignoreCase:=True) = 0 And mem.Released = bReleased)
            If idx < 0 Then Return False
            rIndex = idx + 1
            Return True
        End Function
        Public Function ContainsHandle(aHandle As Integer, Optional bReleased As Boolean = False) As Boolean
            Dim rIndex As Integer = 0
            Return ContainsHandle(aHandle, rIndex, bReleased)
        End Function
        Public Function ContainsHandle(aHandle As Integer, ByRef rIndex As Integer, Optional bReleased As Boolean = False) As Boolean
            rIndex = 0
            If Not _Init Then Return False
            If _Members.Count <= 0 Then Return False
            Dim idx As Integer = _Members.FindIndex(Function(mem) mem.Value = aHandle And mem.Released = bReleased)
            If idx < 0 Then Return False
            rIndex = idx + 1
            Return True
        End Function
        Friend Function Get_Item(aIndex As Integer) As THANDLE
            If aIndex < 1 Or aIndex > Count Then Return New THANDLE("")
            Dim _rVal As THANDLE = _Members(aIndex - 1)
            _rVal.Index = aIndex
            _Members(aIndex - 1) = _rVal
            Return _rVal
        End Function
        Friend Sub Set_Item(aIndex As Integer, aMember As THANDLE)
            If aIndex < 1 Or aIndex > Count Then Return
            aMember.Index = aIndex
            _Members(aIndex - 1) = aMember
            Return
        End Sub
        Friend Function UpdateGUIDS(aHandleOwner As dxfHandleOwner, bHandleOwner As dxfHandleOwner) As Object
            Dim _rVal As Object = Nothing
            If aHandleOwner Is Nothing Or bHandleOwner Is Nothing Then Return _rVal
            If Not _Init Then Init(ImageGUID)
            'On Error Resume Next
            Dim hndl As String
            Dim idx As Integer
            Dim aMem As New THANDLE
            bHandleOwner.ImageGUID = ImageGUID
            Select Case aHandleOwner.FileObjectType
                Case dxxFileObjectTypes.Entity
                    Dim aEntity As dxfEntity = aHandleOwner
                    Dim bEntity As dxfEntity = bHandleOwner
                    hndl = aEntity.Handle
                    If TryGetHandle(aEntity.Handle, aMem, idx) Then
                        aMem.OwnerGUID = bEntity.GUID
                        Set_Item(idx, aMem)
                    End If
                Case dxxFileObjectTypes.Block
                    Dim aBlk As dxfBlock = aHandleOwner
                    Dim bBlk As dxfBlock = bHandleOwner
                    Dim aBR As dxfTableEntry = aBlk.BlockRecord
                    hndl = aBR.Handle
                    If TryGetHandle(aBR.Handle, aMem, idx) Then
                        aMem.OwnerGUID = aBR.GUID
                        Set_Item(idx, aMem)
                    End If
                    If TryGetHandle(aBlk.Handle, aMem, idx) Then
                        aMem.OwnerGUID = aBlk.GUID
                        Set_Item(idx, aMem)
                    End If
                    If TryGetHandle(aBlk.EndBlockHandle, aMem, idx) Then
                        aMem.OwnerGUID = aBlk.GUID & ".EndBlock"
                        Set_Item(idx, aMem)
                    End If
            End Select
            Return _rVal
        End Function
#End Region 'Methods
#Region "IDisposable Implementation"
        'Protected Overrides Sub Finalize()
        '    MyBase.Finalize()
        'End Sub
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not _Disposed Then
                If disposing Then
                    ' dispose managed state (managed objects)
                    _ImageGUID = ""
                    If _Members IsNot Nothing Then _Members.Clear()
                    If _TempValues IsNot Nothing Then _TempValues.Clear()
                End If
                ImagePtr = Nothing
                _IDS = Nothing
                _SaveIDS = Nothing
                _Members = Nothing
                _Disposed = True
                _TempValues = Nothing
                _TempHndls = Nothing
                _Init = False
            End If
        End Sub
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
    End Class
End Namespace
