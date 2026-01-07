

Namespace UOP.DXFGraphics.Structures


#Region "Structures"
    Friend Structure THANDLE
        Implements ICloneable
#Region "Members"
        Public Value As Integer
        Public OwnerGUID As String
        Public OwnerName As String
        Public Index As Integer
        Public Released As Boolean
#End Region 'Members
#Region "Constructors"
        Public Sub New(Optional aOwnerGUID As String = "")
            'init -----------------------------
            OwnerGUID = aOwnerGUID.Trim()
            Value = 0
            OwnerName = ""
            Index = -1
            Released = False
            'init -----------------------------
        End Sub
        Public Sub New(aHandle As THANDLE)
            'init -----------------------------
            OwnerGUID = aHandle.OwnerGUID
            Value = aHandle.Value
            OwnerName = aHandle.OwnerName
            Index = aHandle.Index
            Released = aHandle.Released
            'init -----------------------------
        End Sub
#End Region 'Constructors
#Region "Properties"
        Public ReadOnly Property Handle As String
            Get
                If Value > 0 Then Return TVALUES.To_Handle(Value) Else Return String.Empty
            End Get
        End Property
#End Region 'Properties
#Region "Methods"
        Public Function Clone() As THANDLE
            Return New THANDLE(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New THANDLE(Me)
        End Function

        Public Overrides Function ToString() As String
            Dim _rVal As String = Value.ToString & " [" & TVALUES.To_Handle(Value) & "]"
            If Value <= 0 Then
                Return _rVal & " {OPEN}"
            End If
            _rVal += ":" & OwnerGUID & ":"
            If OwnerName <> "" Then _rVal += " {" & OwnerName & "}"
            Return _rVal
        End Function
#End Region 'Methods
    End Structure 'THANDLE
    Friend Structure TGENIDS
        Public AttributeIndex As Integer
        Public DimBlockIndex As Integer
        Public ArrowIDS() As Integer
        Public SymbolIDS() As Integer
        Public ShapeIndex As Integer
        Public TableIndex As Integer
        Public Pointer As Integer
        Public Max As Integer
#Region "Contructors"
        Public Sub New(aImageGUID As String)
            'init -------------------------------------
            AttributeIndex = 0
            DimBlockIndex = 2
            ShapeIndex = 1
            TableIndex = 0
            Pointer = 0
            Max = 0

            ReDim SymbolIDS(0 To 5)
            ReDim ArrowIDS(0 To 4)
            'init -------------------------------------
        End Sub
#End Region 'Constructurs
    End Structure
    Friend Structure THANDLEGENERATOR
#Region "Members"
        Public ImageGUID As String
        Private _IDS As TGENIDS
        Private _SaveIDS As TGENIDS
        Private _Init As Boolean
        Private _Mems As TDICTIONARY_HANDLES
#End Region 'Members
#Region "Constructors"
        Public Sub New(Optional aImageGUID As String = "")
            'init ---------------------------------------------------------- 
            ImageGUID = IIf(Not String.IsNullOrEmpty(aImageGUID), aImageGUID.Trim(), "")
            _Mems = New TDICTIONARY_HANDLES(ImageGUID)
            _Init = True
            _SaveIDS = New TGENIDS(ImageGUID)
            _IDS = New TGENIDS(ImageGUID)
            'init ---------------------------------------------------------- 

        End Sub

        Public Sub New(aGen As THANDLEGENERATOR)
            'init ---------------------------------------------------------- 
            ImageGUID = aGen.ImageGUID
            _Mems = New TDICTIONARY_HANDLES(aGen._Mems)
            _Init = True
            _SaveIDS = New TGENIDS(ImageGUID)
            _IDS = New TGENIDS(ImageGUID)
            'init ---------------------------------------------------------- 
        End Sub

        Public Sub Init(Optional aImageGUID As String = Nothing)
            ImageGUID = IIf(Not String.IsNullOrWhiteSpace(aImageGUID), aImageGUID.Trim(), "")
            _Mems = New TDICTIONARY_HANDLES(ImageGUID)
            _Init = True
            _SaveIDS = New TGENIDS(ImageGUID)
            _IDS = New TGENIDS(ImageGUID)
        End Sub
#End Region 'Constructors
#Region "Properties"
        Public Property Inited As Boolean
            Get
                Return _Init
            End Get
            Set(value As Boolean)
                _Init = value
            End Set
        End Property
        Public Property Members As TDICTIONARY_HANDLES
            Get
                Return _Mems
            End Get
            Set(value As TDICTIONARY_HANDLES)
                _Mems = value
            End Set
        End Property
        Public Property SaveIDS As TGENIDS
            Get
                Return _SaveIDS
            End Get
            Set(value As TGENIDS)
                _SaveIDS = value
            End Set
        End Property
        Public Property IDS As TGENIDS
            Get
                Return _IDS
            End Get
            Set(value As TGENIDS)
                _IDS = value
            End Set
        End Property
        Public ReadOnly Property Count As Integer
            Get
                If Not _Init Then Return 0
                Return _Mems.Count
            End Get
        End Property
        Public ReadOnly Property MaxHandle As Integer
            Get
                Return _Mems.MaxHandle
            End Get
        End Property
        Friend ReadOnly Property Image As dxfImage
            Get
                If ImageGUID <> "" Then Return dxfEvents.GetImage(ImageGUID) Else Return Nothing
            End Get
        End Property
#End Region 'Properties
#Region "Methods"
        Public Sub Clear()

            _Mems = New TDICTIONARY_HANDLES(ImageGUID)
            _Init = True
            _SaveIDS = New TGENIDS(ImageGUID)
            _IDS = New TGENIDS(ImageGUID)

        End Sub
        Public Function Assign(aSubEnts As TENTITIES, Optional aImage As dxfImage = Nothing) As String
            Dim _rVal As String = String.Empty
            If aSubEnts.Count <= 0 Then Return String.Empty
            If Not _Init Then Init()

            Dim hndl As String

            For i As Integer = 1 To aSubEnts.Count
                Dim sent As TENTITY = aSubEnts.Item(i)
                hndl = sent.Handle
                sent.Handle = NextHandle(aSubEnts.Item(i).Props.GUID, hndl, aImage:=aImage)
                aSubEnts.SetItem(i, sent)
            Next i
            'If aImage Is Nothing Then aImage = Image
            'If aImage IsNot Nothing Then aImage.obj_HANDLES = Me
            Return _rVal
        End Function
        Public Function AssignToEntry(aEntry As TTABLEENTRY, Optional aImage As dxfImage = Nothing, Optional aPreferedHandle As String = "", Optional bDontUpdateImage As Boolean = False) As String
            If aEntry.Index < 0 Then Return String.Empty
            If Not _Init Then Init()
            Dim _rVal As String = String.Empty
            Dim oldHndle As String
            Dim hgc As Integer = 5
            If aEntry.EntryType = dxxReferenceTypes.DIMSTYLE Then hgc = 105
            If aEntry.GUID = "" Then aEntry.GUID = dxfEvents.NextEntryGUID(aEntry.EntryType)
            oldHndle = aEntry.Handle
            If oldHndle = "" Then oldHndle = Trim(aPreferedHandle)
            _rVal = NextHandle(aEntry.GUID, oldHndle, aOwnerName:=aEntry.Props.GCValueStr(2), aImage:=aImage, bDontUpdateImage:=bDontUpdateImage)
            aEntry.Handle = _rVal
            'If Not bDontUpdateImage Then
            '    If aImage Is Nothing Then aImage = Image
            '    If aImage IsNot Nothing Then aImage.obj_HANDLES = Me
            'End If
            Return _rVal
        End Function

        Public Function AssignToEntry(aEntry As dxfTableEntry, Optional aImage As dxfImage = Nothing, Optional aPreferedHandle As String = "", Optional bDontUpdateImage As Boolean = False) As String
            If aEntry Is Nothing Then Return String.Empty
            If Not _Init Then Init()
            Dim _rVal As String = String.Empty
            Dim oldHndle As String
            Dim hgc As Integer = 5
            If aEntry.EntryType = dxxReferenceTypes.DIMSTYLE Then hgc = 105
            If aEntry.GUID = "" Then aEntry.GUID = dxfEvents.NextEntryGUID(aEntry.EntryType)
            oldHndle = aEntry.Handle
            If oldHndle = "" Then oldHndle = Trim(aPreferedHandle)
            _rVal = NextHandle(aEntry.GUID, oldHndle, aOwnerName:=aEntry.Name, aImage:=aImage, bDontUpdateImage:=bDontUpdateImage)
            aEntry.Handle = _rVal
            'If Not bDontUpdateImage Then
            '    If aImage Is Nothing Then aImage = Image
            '    If aImage IsNot Nothing Then aImage.obj_HANDLES = Me
            'End If
            Return _rVal
        End Function

        Public Function AssignToTable(aTable As TTABLE, aImage As dxfImage, Optional bSuppressMembers As Boolean = False, Optional aPreferedHandle As String = "", Optional bSuppressTableHandle As Boolean = False, Optional bDontUpdateImage As Boolean = False) As String
            If aImage Is Nothing Then aImage = Image
            If String.IsNullOrEmpty(aPreferedHandle) Then aPreferedHandle = ""
            Dim iGUID As String = String.Empty
            If aImage IsNot Nothing Then iGUID = aImage.GUID
            If aTable.Props.Count <= 0 Then
                aTable = dxfImageTool.TableCreate(aImage, aTable.TableType, True, False)
            End If
            Dim _rVal As String = String.Empty
            Dim cnt As Integer
            Dim oldHndl As String = aTable.Handle
            If oldHndl = "" Then oldHndl = Trim(aPreferedHandle)
            Dim hidx As Integer
            aTable.GUID = dxfEvents.NextTableGUID(aTable.TableType, aTable.GUID)
            aTable.ImageGUID = iGUID
            If Not bSuppressTableHandle Or oldHndl = "" Then
                _rVal = NextHandle(aTable.GUID, oldHndl, aImage:=aImage, aOwnerName:=aTable.TableTypeName, bDontUpdateImage:=bDontUpdateImage)
                aTable.Handle = _rVal
            Else
                _rVal = oldHndl
            End If
            hidx = -1
            Dim aEntry As TTABLEENTRY = TTABLEENTRY.Null
            For i As Integer = 1 To aTable.Count
                aEntry = aTable.Item(i)
                If Not bSuppressMembers Then AssignToEntry(aEntry, aImage:=aImage, bDontUpdateImage:=True)
                If aEntry.Domain = dxxDrawingDomains.Model Then
                    If Not aEntry.Suppressed Then cnt += 1
                End If
                'set the entries table handle
                aEntry.Props.SetValueGC(330, _rVal, hidx)
                aEntry.ImageGUID = iGUID
                aTable.SetItem(i, aEntry)
            Next i
            aTable.MemberCount = cnt
            'If aImage IsNot Nothing Then
            '    If Not bDontUpdateImage Then
            '        aImage.obj_HANDLES = Me
            '        aImage.BaseTable(aTable.TableType) = aTable
            '    End If
            'End If
            Return _rVal
        End Function
        Public Function NextHandle(aOwnerGUID As String, aOldHandle As String, Optional rNewHandleCreated As Boolean? = Nothing, Optional aOwnerName As String = "", Optional bSwapGUID As Boolean = False, Optional aImage As dxfImage = Nothing, Optional bDontUpdateImage As Boolean = False) As String
            If Not _Init Then Init()
            aOwnerGUID = Trim(aOwnerGUID)
            If String.IsNullOrEmpty(aOldHandle) Then aOldHandle = "" Else aOldHandle = Trim(aOldHandle)
            If rNewHandleCreated IsNot Nothing Then rNewHandleCreated = False
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
                If _Mems.TryGetHandle(iHndle, aMem, idx, False) Then
                    If aMem.OwnerGUID <> "" And aMem.OwnerGUID <> aOwnerGUID Then
                        If Not bSwapGUID Then
                            idx = 0
                        Else
                            aMem.OwnerGUID = aOwnerGUID
                        End If
                    End If
                End If
            End If
            If idx <= 0 Then  'a new handle is required
                bChnged = True
                If rNewHandleCreated IsNot Nothing Then rNewHandleCreated = True
                iHndle = _Mems.NextHandle(bUsed)
                If bUsed Then _Mems.Release(iHndle, True)
                _Mems.SaveHandle(iHndle, aOwnerGUID, aOwnerName)
                idx = _Mems.Count
                aMem = _Mems.Item(idx)
            End If
            If String.IsNullOrEmpty(aOwnerGUID) Then aOwnerGUID = aMem.OwnerGUID
            If String.IsNullOrEmpty(aOwnerName) Then aOwnerName = aMem.OwnerName
            aMem.OwnerGUID = aOwnerGUID
            aMem.OwnerName = aOwnerName
            If aOwnerGUID <> aMem.OwnerGUID Or aOwnerName <> aMem.OwnerName Then bChnged = True
            _Mems.SetItem(idx, aMem)
            If bChnged And Not bDontUpdateImage Then
                If aImage Is Nothing Then aImage = Image
            End If
            'If Not bDontUpdateImage Then
            '    If aImage IsNot Nothing Then aImage.obj_HANDLES = Me
            'End If
            Return TVALUES.To_Handle(iHndle)
        End Function
        Public Function SaveUsedHandle(aHandle As String, aOwnerGUID As String, Optional aOwnerName As String = "") As THANDLE
            If Not _Init Then Clear()
            If String.IsNullOrEmpty(aHandle) Then Return Nothing
            aHandle = Trim(aHandle)
            If aHandle = "" Then Return Nothing
            Dim iVal As Integer = TVALUES.HexToInteger(aHandle)
            If iVal = 0 Then Return Nothing
            Dim aMem As THANDLE = Nothing
            Dim idx As Integer
            If Not _Mems.TryGetHandle(aHandle, aMem, idx, False) Then
                _Mems.SaveHandle(iVal, aOwnerGUID, aOwnerName)
            Else
                aMem.OwnerGUID = aOwnerGUID
                aMem.OwnerName = aOwnerName
                _Mems.SetItem(idx, aMem)
            End If
            _Mems.Handle_Set(iVal, False)
            Return aMem
            'Debug.Print "sAVING hANDLE = " & aVal & "(" & aHandle & ")"
        End Function
        Public Function NextAttributeTag(Optional aImage As dxfImage = Nothing) As String
            Dim _rVal As String = String.Empty
            If Not _Init Then Init()
            _rVal = "ATRRIB_" & _IDS.AttributeIndex
            _IDS.AttributeIndex += 1
            'If aImage Is Nothing Then aImage = Image
            'If aImage IsNot Nothing Then aImage.obj_HANDLES = Me
            Return _rVal
        End Function
        Public Function NextDimBlockName(Optional aImage As dxfImage = Nothing, Optional bDontUpdateImage As Boolean = False) As String
            Dim _rVal As String = String.Empty
            If Not _Init Then Init()
            _rVal = "*D" & _IDS.DimBlockIndex
            _IDS.DimBlockIndex += 1
            'If aImage Is Nothing Then
            '    If Not bDontUpdateImage Then aImage = Image
            'End If
            'If aImage IsNot Nothing Then
            '    If Not bDontUpdateImage Then aImage.obj_HANDLES = Me
            'End If
            Return _rVal
        End Function
        Public Function NextSymbolName(aSymbolType As dxxSymbolTypes, aArrowType As dxxArrowTypes, Optional aImage As dxfImage = Nothing) As String
            Dim _rVal As String = String.Empty
            If Not _Init Then Init()
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
            'If aImage Is Nothing Then aImage = Image
            'If aImage IsNot Nothing Then aImage.obj_HANDLES = Me
            Return _rVal
        End Function
        Public Function NextTableName(aBlocks As colDXFBlocks, Optional aImage As dxfImage = Nothing) As String
            Dim _rVal As String = String.Empty
            If Not _Init Then Init()
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
            'If aImage Is Nothing Then aImage = Image
            'If aImage IsNot Nothing Then aImage.obj_HANDLES = Me
            Return _rVal
        End Function
        Friend Sub Print()
            If Count <= 0 Then Return
            Dim aMem As THANDLE
            For i As Integer = 1 To Count
                aMem = _Mems.Item(i)
                System.Diagnostics.Debug.WriteLine(aMem.ToString())
            Next i
        End Sub
        Public Sub SaveState(Optional aImage As dxfImage = Nothing, Optional bReset As Boolean = False)
            If Not _Init Then Init()
            If Not bReset Then
                _SaveIDS = _IDS
                _IDS.Pointer = _Mems.Count
                _IDS.Max = _Mems.MaxHandle
                _Mems.SuppressReuse = True
            Else
                _Mems.ReduceTo(_IDS.Pointer)
                _Mems.SuppressReuse = False
                _Mems.MaxHandle = _IDS.Max
                _IDS = _SaveIDS
            End If
            'If aImage Is Nothing Then aImage = Image
            'If aImage IsNot Nothing Then aImage.obj_HANDLES = Me
        End Sub

#End Region 'Methods
    End Structure 'THANDLEGENERATOR
#End Region 'Structures

End Namespace
