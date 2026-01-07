Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Public MustInherit Class dxfHandleOwner
#Region "Members"
        Private _Handlez As THANDLES
        Friend OwnerPtr As WeakReference
        Friend BlockPtr As WeakReference
        Friend CollectionPtr As WeakReference
        Friend BlockCollectionPtr As WeakReference
        Friend ImagePtr As WeakReference(Of dxfImage)

#End Region 'Members
#Region "Constructors"
        Friend Sub New(aStruc As THANDLES)
            Init()
            _Handlez = aStruc

        End Sub
        Friend Sub New(aGUID As String, Optional aGraphicType As dxxGraphicTypes = dxxGraphicTypes.Undefined, Optional aGUIDPrefix As String = Nothing)
            Init()
            If String.IsNullOrWhiteSpace(aGUID) Then
                aGUID = dxfEvents.NextEntityGUID(aGraphicType, aGUIDPrefix:=aGUIDPrefix)
            End If
            _Handlez = New THANDLES(aGUID)

        End Sub

        Friend Sub Init(Optional aGUID As String = "")
            _Handlez = New THANDLES(aGUID)
            OwnerPtr = Nothing
            BlockPtr = Nothing
            CollectionPtr = Nothing
            BlockCollectionPtr = Nothing
            ImagePtr = Nothing
        End Sub
#End Region 'Constructors
#Region "Properties"
        Public Overridable Property BlockGUID As String
            '^the GUID of the dxfBlock that the object is associated to
            Get
                Return _Handlez.BlockGUID
            End Get
            Friend Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = String.Empty
                _Handlez.BlockGUID = value
            End Set
        End Property



        Friend Overridable Property CollectionGUID As String
            '^the GUID of the the collection that the object is associated to
            Get
                Return _Handlez.CollectionGUID
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = String.Empty
                _Handlez.CollectionGUID = value
            End Set
        End Property
        Friend Overridable Property BlockCollectionGUID As String
            '^the GUID of the the collection that the object is associated to
            Get
                Return _Handlez.BlockCollectionGUID
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = String.Empty
                _Handlez.BlockCollectionGUID = value
            End Set
        End Property
        Public Property Domain As dxxDrawingDomains
            '^the drawing domain of the object
            Get
                Return _Handlez.Domain
            End Get
            Friend Set(value As dxxDrawingDomains)
                _Handlez.Domain = value
            End Set
        End Property
        Public Overridable Property GUID As String
            '^the unique identifier assigned to the object
            Get
                Return _Handlez.GUID
            End Get
            Friend Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = String.Empty
                _Handlez.GUID = value
            End Set
        End Property
        Public Overridable Property Name As String
            '^the name assigned to the object for unique identification in DXF code generation
            Get
                Return _Handlez.Name
            End Get
            Friend Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = String.Empty
                _Handlez.Name = value
            End Set
        End Property
        Public Overridable Property Handle As String
            '^the handle assigned to the object for unique identification in DXF code generation
            Get
                Return _Handlez.Handle
            End Get
            Friend Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = String.Empty
                _Handlez.Handle = value
            End Set
        End Property
        Public Overridable Property Identifier As String
            '^a private identifier used to locate the object in a collection
            '~used internally for complex entity block creation
            Get
                Return _Handlez.Identifier
            End Get
            Friend Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = String.Empty
                _Handlez.Identifier = value
            End Set
        End Property
        Public Property Index As Integer
            '^the index of the object when it is a member of a collection
            Get
                Return _Handlez.Index
            End Get
            Friend Set(value As Integer)
                _Handlez.Index = value
            End Set
        End Property
        '^the type of the object
        Public MustOverride ReadOnly Property FileObjectType As dxxFileObjectTypes
        Public Property OwnerGUID As String
            '^the GUID of the object that owns this object
            Get
                Return _Handlez.OwnerGUID
            End Get
            Friend Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = String.Empty
                _Handlez.OwnerGUID = value
            End Set
        End Property
        Public Property OwnerType As dxxFileObjectTypes
            Get
                Return _Handlez.OwnerType
            End Get
            Set(value As dxxFileObjectTypes)
                _Handlez.OwnerType = value
            End Set
        End Property
        Public Overridable Property ImageGUID As String
            '^the GUID of the image that owns this object
            Get
                Return _Handlez.ImageGUID
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = String.Empty Else value = value.Trim
                Dim newval As Boolean = String.Compare(value, ImageGUID, ignoreCase:=True)
                If Not newval Then Return
                _Handlez.ImageGUID = value
                ImagePtr = dxfEvents.GetImagePtr(_Handlez.ImageGUID)
            End Set
        End Property

        Public Overridable Property ObjectType As dxxObjectTypes
            Get
                Return _Handlez.ObjectType
            End Get
            Friend Set(value As dxxObjectTypes)
                _Handlez.ObjectType = value
            End Set
        End Property
        Public ReadOnly Property BelongsToBlock As Boolean
            Get
                If FileObjectType <> dxxFileObjectTypes.Entity Then Return False
                Return Not String.IsNullOrWhiteSpace(ImageGUID) And Not String.IsNullOrWhiteSpace(BlockGUID)
            End Get
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
        Public Overridable Property ReactorGUID As String
            '^the GUID the associated object that is affected by changes to the owning object
            Get
                Return _Handlez.ReactorGUID
            End Get
            Friend Set(value As String)
                _Handlez.ReactorGUID = value
            End Set
        End Property
        Public Property SourceGUID As String
            Get
                Return _Handlez.SourceGUID
            End Get
            Friend Set(value As String)
                _Handlez.SourceGUID = value
            End Set
        End Property
        Friend Property HStrukture As THANDLES
            Get
                Return _Handlez
            End Get
            Set(value As THANDLES)
                _Handlez = New THANDLES(value, GUID)
            End Set
        End Property
        '^a flag indicating the object should be suppressed (invisible)
        Friend Overridable Property Suppressed As Boolean
        Friend MustOverride Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
        Friend MustOverride Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
        Friend Overridable Sub SetGUIDS(aImageGUID As String, aOwnerGUID As String, aBlockGUID As String, aOwnerType As dxxFileObjectTypes, Optional aOwner As dxfHandleOwner = Nothing, Optional aBlock As dxfBlock = Nothing, Optional aImage As dxfImage = Nothing)
            ImageGUID = aImageGUID : OwnerGUID = aOwnerGUID : BlockGUID = aBlockGUID : OwnerType = aOwnerType
            If aOwner IsNot Nothing Then
                OwnerType = aOwner.FileObjectType
                OwnerGUID = aOwner.GUID
                OwnerPtr = New WeakReference(aOwner)
            End If
            If aBlock IsNot Nothing Then
                BlockGUID = aBlock.GUID
                BlockPtr = New WeakReference(aBlock)
            End If
            If aImage IsNot Nothing Then
                ImageGUID = aImage.GUID
                ImagePtr = New WeakReference(Of dxfImage)(aImage)
            ElseIf Not String.IsNullOrWhiteSpace(ImageGUID) And Not HasReferenceTo_Image Then
                Dim img As dxfImage = dxfEvents.GetImage(ImageGUID)
                SetImage(img, False)
            End If
        End Sub
        Friend ReadOnly Property HasReferenceTo_Owner As Boolean
            Get
                If String.IsNullOrWhiteSpace(OwnerGUID) Or OwnerPtr Is Nothing Then Return False
                Return OwnerPtr.IsAlive
            End Get
        End Property
        Friend Overridable ReadOnly Property MyOwner As dxfHandleOwner
            Get
                If Not HasReferenceTo_Owner Then Return Nothing
                Dim _rVal As dxfHandleOwner = TryCast(OwnerPtr.Target, dxfHandleOwner)
                If _rVal IsNot Nothing Then
                    If String.Compare(OwnerGUID, _rVal.GUID, ignoreCase:=True) <> 0 Then SetOwner(Nothing, False)
                End If
                Return _rVal
            End Get
        End Property
        Friend ReadOnly Property HasReferenceTo_Image As Boolean
            Get
                If String.IsNullOrWhiteSpace(ImageGUID) Or ImagePtr Is Nothing Then Return False
                Dim img As dxfImage = Nothing
                Return ImagePtr.TryGetTarget(img)
            End Get
        End Property
        Friend Overridable ReadOnly Property MyImage As dxfImage
            Get

                If Not HasReferenceTo_Image Then Return Nothing
                Dim _rVal As dxfImage = Nothing
                Try
                    ImagePtr.TryGetTarget(_rVal)
                    If _rVal IsNot Nothing AndAlso _rVal.Disposed Then _rVal = Nothing
                    If _rVal IsNot Nothing Then
                        If String.IsNullOrWhiteSpace(ImageGUID) Or String.Compare(ImageGUID, _rVal.GUID, ignoreCase:=True) <> 0 Then SetImage(Nothing, False)
                    End If
                Catch ex As Exception
                    ImagePtr = Nothing

                End Try

                Return _rVal
            End Get
        End Property
        Friend ReadOnly Property HasReferenceTo_Collection As Boolean
            Get
                If String.IsNullOrWhiteSpace(CollectionGUID) Or CollectionPtr Is Nothing Then Return False
                Return CollectionPtr.IsAlive
            End Get
        End Property
        Friend Overridable ReadOnly Property myCollection As colDXFEntities
            Get
                If Not HasReferenceTo_Collection Then Return Nothing
                Dim _rVal As colDXFEntities = TryCast(CollectionPtr.Target, colDXFEntities)
                If _rVal IsNot Nothing Then
                    If String.Compare(CollectionGUID, _rVal.GUID, ignoreCase:=True) <> 0 Then SetCollection(Nothing, False)
                End If
                Return _rVal
            End Get
        End Property
        Friend ReadOnly Property HasReferenceTo_Block As Boolean
            Get
                If String.IsNullOrWhiteSpace(BlockGUID) Or BlockPtr Is Nothing Then Return False
                Return BlockPtr.IsAlive
            End Get
        End Property
        Friend Overridable ReadOnly Property MyBlock As dxfBlock
            Get
                If Not HasReferenceTo_Block Then Return Nothing
                Dim _rVal As dxfBlock = TryCast(BlockPtr.Target, dxfBlock)
                If _rVal IsNot Nothing Then
                    If String.Compare(BlockGUID, _rVal.GUID, ignoreCase:=True) <> 0 Then SetBlock(Nothing, False)
                End If
                Return _rVal
            End Get
        End Property
        Friend ReadOnly Property HasReferenceTo_BlockCollection As Boolean
            Get
                If String.IsNullOrWhiteSpace(BlockCollectionGUID) Or BlockCollectionPtr Is Nothing Then Return False
                Return BlockCollectionPtr.IsAlive
            End Get
        End Property
        Friend Overridable ReadOnly Property MyBlockCollection As colDXFBlocks
            Get
                If Not HasReferenceTo_BlockCollection Then Return Nothing
                Dim _rVal As colDXFBlocks = TryCast(BlockCollectionPtr.Target, colDXFBlocks)
                If _rVal IsNot Nothing Then
                    If String.Compare(BlockCollectionGUID, _rVal.GUID, ignoreCase:=True) <> 0 Then SetBlock(Nothing, False)
                End If
                Return _rVal
            End Get
        End Property
#End Region 'Properties
#Region "Methods"
        Friend Overridable Sub ReleaseReferences()
            OwnerGUID = String.Empty
            OwnerPtr = Nothing
            BlockGUID = String.Empty
            BlockPtr = Nothing
            CollectionGUID = String.Empty
            CollectionPtr = Nothing
            ImageGUID = String.Empty
            ImagePtr = Nothing

        End Sub
        Friend Sub ReleaseCollectionReference()
            CollectionGUID = String.Empty
            CollectionPtr = Nothing
        End Sub
        Friend Overridable Sub ProcessEvent(aEvent As dxfEvent)
            If aEvent Is Nothing Then Return
            Select Case aEvent.BaseType
                Case dxxEventTypes.EntityCollection
                    Dim evnt As dxfEntitiesEvent = TryCast(aEvent, dxfEntitiesEvent)
                    RespondToEntitiesChangeEvent(evnt)
                Case dxxEventTypes.PropertyChange
                    Dim evnt As dxfPropertyChangeEvent = TryCast(aEvent, dxfPropertyChangeEvent)
                    RespondToDimStylePropertyChange(evnt)
                Case dxxEventTypes.Vertex
                    Dim evnt As dxfVertexEvent = TryCast(aEvent, dxfVertexEvent)
                    RespondToVectorsMemberChange(evnt)
                Case dxxEventTypes.DefPt
                    Dim evnt As dxfDefPtEvent = TryCast(aEvent, dxfDefPtEvent)
                    RespondToDefPtChange(evnt)
                Case dxxEventTypes.PlaneChange
                    Dim evnt As dxfPlaneEvent = TryCast(aEvent, dxfPlaneEvent)
                    RespondToPlaneChangeEvent(evnt)
            End Select
        End Sub
        Friend Overridable Sub RespondToPlaneChangeEvent(aEvent As dxfPlaneEvent)
            If aEvent Is Nothing Then Return
            aEvent.OwnerNotified = True
        End Sub
        Friend Overridable Sub RespondToDimStylePropertyChange(aEvent As dxfPropertyChangeEvent)
            If aEvent Is Nothing Then Return
            aEvent.OwnerNotified = True
        End Sub
        Friend Overridable Sub RespondToSettingChange(aEvent As dxfPropertyChangeEvent)
            If aEvent Is Nothing Then Return
            aEvent.OwnerNotified = True
        End Sub
        Friend Overridable Sub RespondToEntitiesChangeEvent(aEvent As dxfEntitiesEvent)
            If aEvent Is Nothing Then Return
            aEvent.OwnerNotified = True
        End Sub
        Friend Overridable Sub RespondToVectorsMemberChange(aEvent As dxfVertexEvent)
            If aEvent Is Nothing Then Return
            If String.Compare(aEvent.OwnerGUID, GUID, ignoreCase:=True) Then Return
            aEvent.OwnerNotified = True
        End Sub
        Friend Overridable Sub RespondToVectorsChange(aEvent As dxfVectorsEvent)
            If aEvent Is Nothing Then Return
            If String.Compare(aEvent.OwnerGUID, GUID, ignoreCase:=True) Then Return
            aEvent.OwnerNotified = True
        End Sub
        Friend Overridable Sub RespondToDefPtChange(aEvent As dxfDefPtEvent)
            If aEvent Is Nothing Then Return
            If aEvent.Vertex Is Nothing Then Return
            If String.Compare(aEvent.OwnerGUID, GUID, ignoreCase:=True) Then Return
            aEvent.OwnerNotified = True
        End Sub
        Friend Overridable Sub RespondToReactorChangeEvent(aEvent As dxfEntityEvent)
            aEvent.OwnerNotified = True
        End Sub
        Friend Function GetImage(ByRef rImage As dxfImage) As Boolean
            If rImage IsNot Nothing Then
                If rImage.Disposed Then rImage = Nothing
            End If
            If rImage Is Nothing Then
                If Not String.IsNullOrWhiteSpace(ImageGUID) Then
                    rImage = MyImage
                    If rImage Is Nothing Then rImage = dxfEvents.GetImage(ImageGUID)

                End If
                Return SetImage(rImage, True)
            End If

            Return SetImage(rImage, True)

        End Function


        Public Overrides Function ToString() As String
            Return $"{GUID}[{ Handle }]"
        End Function
        Friend Sub SetOwner(aOwner As dxfHandleOwner, bDontReleaseOnNull As Boolean)
            If aOwner IsNot Nothing Then
                OwnerType = aOwner.FileObjectType
                OwnerGUID = aOwner.GUID
                OwnerPtr = New WeakReference(aOwner)
            Else
                If Not bDontReleaseOnNull Then
                    OwnerType = dxxFileObjectTypes.Undefined
                    OwnerGUID = ""
                    OwnerPtr = Nothing
                End If
            End If
        End Sub
        Friend Sub SetBlock(aBlock As dxfBlock, bDontReleaseOnNull As Boolean)
            If aBlock IsNot Nothing Then
                BlockGUID = aBlock.GUID
                BlockPtr = New WeakReference(aBlock)
                If aBlock.HasReferenceTo_Image Then
                    SetImage(aBlock.Image, True)
                End If
            Else
                If Not bDontReleaseOnNull Then
                    BlockGUID = ""
                    BlockPtr = Nothing
                End If
            End If
        End Sub
        Friend Sub SetBlocks(aBlocks As colDXFBlocks, bDontReleaseOnNull As Boolean)
            If aBlocks IsNot Nothing Then
                BlockCollectionGUID = aBlocks.GUID
                BlockCollectionPtr = New WeakReference(aBlocks)
                If aBlocks.HasReferenceTo_Image Then
                    SetImage(aBlocks.Image, True)
                End If
            Else
                If Not bDontReleaseOnNull Then
                    BlockCollectionGUID = ""
                    BlockCollectionPtr = Nothing
                End If
            End If
        End Sub
        Friend Sub SetCollection(aCollection As colDXFEntities, bDontReleaseOnNull As Boolean)
            If aCollection IsNot Nothing Then
                CollectionGUID = aCollection.GUID
                CollectionPtr = New WeakReference(aCollection)
                If aCollection.HasReferenceTo_Block Then
                    SetBlock(aCollection.MyBlock, False)
                End If
                If aCollection.HasReferenceTo_Owner Then
                    SetOwner(aCollection.MyOwner, False)
                End If
                If aCollection.HasReferenceTo_Image Then
                    SetImage(aCollection.Image, True)
                End If
            Else
                If Not bDontReleaseOnNull Then
                    CollectionGUID = ""
                    CollectionPtr = Nothing
                End If
            End If
        End Sub
        Friend Sub SetBlockCollection(aCollection As colDXFBlocks, bDontReleaseOnNull As Boolean)
            If aCollection IsNot Nothing Then
                CollectionGUID = aCollection.GUID
                BlockCollectionPtr = New WeakReference(aCollection)
                If aCollection.HasReferenceTo_Image Then
                    SetImage(aCollection.MyImage, False)
                Else
                    ImageGUID = aCollection.ImageGUID
                End If
            Else
                If Not bDontReleaseOnNull Then
                    SetCollection(Nothing, False)
                    SetImage(Nothing, False)
                End If
            End If
        End Sub
        Friend Function SetImage(aImage As dxfImage, bDontReleaseOnNull As Boolean) As Boolean
            Dim img As dxfImage = aImage
            If img IsNot Nothing AndAlso img.Disposed Then
                img = Nothing
                Return False
            End If
            If img IsNot Nothing Then
                ImageGUID = img.GUID
                ImagePtr = New WeakReference(Of dxfImage)(img)
                Return True
            Else
                If Not bDontReleaseOnNull Then
                    ImageGUID = ""
                    ImagePtr = Nothing
                End If
                Return False
            End If
        End Function
#End Region 'Methods
    End Class


End Namespace
