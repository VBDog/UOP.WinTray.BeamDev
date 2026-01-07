Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public Class dxfBlock
        Inherits dxfHandleOwner
        Implements IDisposable
        Implements ICloneable
#Region "Members"
        'Private _Events As dxpEventHandler
        Private _Entities As colDXFEntities
        Private _Struc As TBLOCK
        Private bSuppressEvents As Boolean
        Private _Plane As dxfPlane
        Private _Disposed As Boolean
#End Region 'Members
#Region "Constructors"
        Private Overloads Sub Init(Optional aGUID As String = "", Optional aPlane As dxfPlane = Nothing, Optional aName As String = Nothing)
            If String.IsNullOrWhiteSpace(aGUID) Or GUID = "" Then aGUID = dxfEvents.NextBlockGUID
            MyBase.Init(aGUID)
            _Struc = New TBLOCK(aName)
            _Plane = New dxfPlane(aPlane) With {
                .OwnerPtr = New WeakReference(Me)
            }
            'AddHandler _Plane.PlaneChange, AddressOf _Plane_PlaneChange
            Domain = dxxDrawingDomains.Model
            Description = String.Empty
            Flag = String.Empty
            _Entities = New colDXFEntities
            _Entities.SetGUIDS(String.Empty, String.Empty, aBlockGUID:=GUID, aOwnerType:=dxxFileObjectTypes.Block, aBlock:=Me)
            _Disposed = False
            bSuppressEvents = False
        End Sub

        Public Sub New()
            MyBase.New(dxfEvents.NextBlockGUID)
            Init(GUID)

        End Sub
        Public Sub New(aEntities As IEnumerable(Of dxfEntity), aName As String, Optional aDescription As String = Nothing, Optional bCloneEntities As Boolean = True, Optional bCloneInstances As Boolean = True)
            MyBase.New(dxfEvents.NextBlockGUID)
            Init(GUID, Nothing, aName)
            If aDescription Is Nothing Then Description = "" Else Description = aDescription
            If aEntities IsNot Nothing Then
                _Entities.Append(aEntities, bAddClones:=bCloneEntities, bCloneInstances:=bCloneInstances)
            End If
        End Sub
        Public Sub New(aName As String, Optional aDomain As dxxDrawingDomains = dxxDrawingDomains.Model, Optional aPlane As dxfPlane = Nothing, Optional aEntities As IEnumerable(Of dxfEntity) = Nothing, Optional aInsertionPt As iVector = Nothing)
            MyBase.New(dxfEvents.NextBlockGUID)
            Init(GUID, aPlane, aName)

            Domain = aDomain
            InsertionPtV = New TVECTOR(aInsertionPt)
            If aEntities IsNot Nothing Then
                _Entities.Populate(aEntities, bAddClones:=True, bCopyInstances:=True)
            End If
        End Sub


        Public Sub New(aBlock As dxfBlock)
            MyBase.New(dxfEvents.NextBlockGUID)
            Init(GUID)
            If aBlock Is Nothing Then Return
            _Struc = New TBLOCK(aBlock.Strukture)
            _Plane = New dxfPlane(aBlock.Plane)
            Entities.Populate(True, aBlock.Entities, bAddClones:=True, bCopyInstances:=True)
            BlockRecordHandle = String.Empty
            EndBlockHandle = String.Empty
            _Instances = New dxoInstances(aBlock.Instances)
        End Sub
#End Region 'Constructors
#Region "dxfHandleOwner"
        Friend Overrides Property Suppressed As Boolean
            Get
                Return _Struc.Suppressed
            End Get
            Set(value As Boolean)
                _Struc.Suppressed = value

            End Set
        End Property
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Throw New NotImplementedException
        End Function
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Throw New NotImplementedException
        End Function
        Public Overrides ReadOnly Property FileObjectType As dxxFileObjectTypes
            Get
                Return dxxFileObjectTypes.Block
            End Get
        End Property
#End Region 'dxfHandleOwner
#Region "Properties"

        Public Property Properties As dxoProperties
            Get
                Return New dxoProperties(_Struc.Props)
            End Get
            Friend Set(value As dxoProperties)
                _Struc.Props = New TPROPERTIES(value)
            End Set
        End Property

        Private _BlockRecord As dxoBlockRecord
        Friend Property BlockRecord As dxoBlockRecord
            Get
                If _BlockRecord Is Nothing Then
                    _BlockRecord = New dxoBlockRecord(Name)
                Else
                    _BlockRecord.Name = Name
                End If
                _BlockRecord.OwnerGUID = GUID
                Return _BlockRecord
            End Get
            Set(value As dxoBlockRecord)

                If value Is Nothing Then
                    _BlockRecord = Nothing
                Else
                    If _BlockRecord Is Nothing Then _BlockRecord = New dxoBlockRecord(Name) Else _BlockRecord.Name = Name
                    _BlockRecord.Suppressed = Suppressed
                    _BlockRecord.IsDefault = IsDefault
                    Dim g As String = _BlockRecord.GUID
                    _BlockRecord.Properties.CopyVals(value.Properties, bSkipHandles:=False, bSkipPointers:=False, bCopyNewMembers:=True)
                    _BlockRecord.GUID = g
                End If
            End Set
        End Property


        Public ReadOnly Property HasEntities As Boolean
            Get
                If _Entities Is Nothing Then Return False
                Return _Entities.Count > 0
            End Get
        End Property

        Public ReadOnly Property HasAttributes As Boolean
            Get
                If _Entities Is Nothing Then Return False
                Return _Entities.ContainsEntityType(dxxEntityTypes.Attdef)
            End Get
        End Property

        Public Property BlockRecordHandle As String
            Get
                Return BlockRecord.Handle
            End Get
            Friend Set(value As String)
                BlockRecord.Handle = value
            End Set
        End Property
        Public Property Description As String
            Get
                Return _Struc.Description
            End Get
            Set(value As String)
                _Struc.Description = value
            End Set
        End Property
        Public ReadOnly Property EditableAttributes As Boolean
            Get
                Dim aAtts As colDXFEntities
                Dim aTxt As dxeText
                Dim cnt As Integer
                aAtts = Entities.GetByEntityType(dxxEntityTypes.Attdef)
                For i As Integer = 1 To aAtts.Count
                    aTxt = aAtts.Item(i)
                    If Not aTxt.Constant Then cnt += 1
                Next i
                EditableAttributes = cnt > 0
                Return EditableAttributes
            End Get
        End Property
        Friend Property EndBlockHandle As String
            Get
                Return _Struc.EndBlockHandle
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = String.Empty
                _Struc.EndBlockHandle = value.Trim
            End Set
        End Property

        Public ReadOnly Property Attributes As dxfAttributes
            Get
                Dim _rVal As New dxfAttributes()
                Dim atts As List(Of dxeText) = Entities.AttDefs()
                '^returns the attribute objects of all the att definitions in the block
                For Each aTxt As dxeText In atts
                    _rVal.Add(New dxfAttribute(aTxt))
                Next

                Return _rVal
            End Get
        End Property

        Public Property Entities As colDXFEntities
            Get
                If _Entities Is Nothing And Not _Disposed Then
                    _Entities = New colDXFEntities
                    _Entities.SetGUIDS(ImageGUID, String.Empty, GUID, aOwnerType:=dxxFileObjectTypes.Block, Nothing, Me, MyImage)
                End If
                If _Entities IsNot Nothing Then
                    _Entities.SetGUIDS(ImageGUID, String.Empty, GUID, aOwnerType:=dxxFileObjectTypes.Block, Nothing, Me, MyImage)
                    _Entities.BlockName = Name
                    _Entities.Suppressed = Suppressed
                End If
                Return _Entities
            End Get
            Set(value As colDXFEntities)
                Entities.Populate(value, True)
                _Struc.PathsDefined = False
            End Set
        End Property
        Public Property Explodable As Boolean
            Get
                Return BlockRecord.Properties.ValueB(280)
            End Get
            Set(value As Boolean)
                BlockRecord.Properties.SetVal(280, TPROPERTY.SwitchValue(value))
            End Set
        End Property
        Friend Property ExtentPts As TVECTORS
            Get
                If Not PathsDefined Then TPATHS.BLOCK(Me, True)
                Return _Struc.RelativePaths.ExtentVectors
            End Get
            Set(value As TVECTORS)
                _Struc.RelativePaths.ExtentVectors = value
            End Set
        End Property
        Public Property Flag As String
            Get
                '^provided to carry additional info about the entity
                Return _Struc.Flag
            End Get
            Set(value As String)
                '^provided to carry additional info about the entity
                _Struc.Flag = value
            End Set
        End Property
        Public ReadOnly Property HandlePt As dxfVector
            Get
                Return InsertionPt
            End Get
        End Property
        Friend Property Handlez As THANDLES
            Get
                Return MyBase.HStrukture
            End Get
            Set(value As THANDLES)
                MyBase.HStrukture = New THANDLES(value, GUID)
            End Set
        End Property
        Public Property IsDefault As Boolean
            Get
                Return _Struc.IsDefault
            End Get
            Friend Set(value As Boolean)
                _Struc.IsDefault = value

            End Set
        End Property
        Public Property InsertionPt As dxfVector
            Get
                '^the point where the entity was inserted
                Return _Plane.Origin
            End Get
            Set(value As dxfVector)
                '^the point where the entity was inserted
                If value IsNot Nothing Then
                    _Plane.SetCoordinates(value.X, value.Y, value.Z)
                Else
                    _Plane.SetCoordinates(0, 0, 0)
                End If
            End Set
        End Property
        Friend Property InsertionPtV As TVECTOR
            Get
                '^the point where the entity was inserted
                Return _Plane.OriginV
            End Get
            Set(value As TVECTOR)
                _Plane.OriginV = value
            End Set
        End Property
        Friend Property IsArrowHead As Boolean
            Get
                Return _Struc.IsArrowHead
            End Get
            Set(value As Boolean)
                _Struc.IsArrowHead = value
            End Set
        End Property
        Friend Property IsDirty As Boolean
            Get
                Return Not _Struc.PathsDefined
            End Get
            Set(value As Boolean)
                If value Then _Struc.PathsDefined = False
            End Set
        End Property
        Public Property LayerName As String
            Get
                If String.IsNullOrWhiteSpace(_Struc.LayerName) Then _Struc.LayerName = "0"
                Return _Struc.LayerName
            End Get
            Set(value As String)
                _Struc.LayerName = value
            End Set
        End Property
        Public ReadOnly Property LayerNames As String
            Get
                '^all of the layer names referenced by the entities in block
                Return Entities.LayerNames(_Struc.LayerName)
            End Get
        End Property
        Friend Property LayoutHandle As String
            Get
                '^the handle of the layout object associated to the block
                Return BlockRecord.Properties.ValueS("Layout Handle")
            End Get
            Set(value As String)
                '^the handle of the layout object associated to the block
                If value Is Nothing Then value = String.Empty
                BlockRecord.Properties.SetVal("Layout Handle", value.Trim)
            End Set
        End Property
        Friend Property LayoutName As String
            Get
                '^the handle of the layout object associated to the block
                Return BlockRecord.Properties.ValueS("*LayoutName")
            End Get
            Set(value As String)
                '^the handle of the layout object associated to the block
                BlockRecord.Properties.SetVal("*LayoutName", Trim(value))
            End Set
        End Property
        Public ReadOnly Property Linetypes As String
            Get
                '^all of the lintetypes names referenced by the entities in block
                Return Entities.Linetypes
            End Get
        End Property

        Public ReadOnly Property StyleNames As String
            Get
                '^all of the style names referenced by the entities in block
                Return Entities.StyleNames
            End Get
        End Property
        Public ReadOnly Property DimStyleNames As String
            Get
                '^all of the dim style names referenced by the entities in block
                Return Entities.DimStyleNames
            End Get
        End Property

        ''' <summary>
        ''' the name of the block
        ''' </summary>
        ''' <returns></returns>
        Public Shadows Property Name As String
            Get

                MyBase.Name = _Struc.Name
                Return MyBase.Name
            End Get
            Set(value As String)
                dxfUtils.ValidateBlockName(value, "", True, True)
                If _Struc.Name <> value Then
                    Dim lVal As String = _Struc.Name
                    _Struc.Name = value
                    MyBase.Name = _Struc.Name
                    BlockRecord.Properties.SetVal("Name", value)
                    RaiseChangeEvents("Name", lVal, _Struc.Name)
                End If
            End Set
        End Property
        Friend ReadOnly Property PathsDefined As Boolean
            Get
                Return _Struc.PathsDefined And _Struc.RelativePaths.Count > 0
            End Get
        End Property
        Friend Property ReadFrom As String
            Get
                Return _Struc.ReadFrom
            End Get
            Set(value As String)
                _Struc.ReadFrom = value
            End Set
        End Property
        Friend Property RelativePaths As TPATHS
            Get
                If Not PathsDefined Then TPATHS.BLOCK(Me, False)
                _Struc.PathsDefined = True
                Return _Struc.RelativePaths
            End Get
            Set(value As TPATHS)
                _Struc.RelativePaths = value
            End Set
        End Property
        Friend Property Strukture As TBLOCK
            Get
                If _Plane Is Nothing Then
                    _Plane = New dxfPlane With {.OwnerPtr = New WeakReference(Me)}
                    'AddHandler _Plane.PlaneChange, AddressOf _Plane_PlaneChange
                End If
                _Struc.Plane = Plane.Strukture
                Return _Struc
            End Get
            Set(value As TBLOCK)
                _Struc = value
                MyBase.Name = _Struc.Name
                Plane.Strukture = value.Plane
                Entities.BlockName = _Struc.Name
            End Set
        End Property
        Friend Property SubBlockNames As String
            Get
                Return _Struc.SubBlockNames.ToString
            End Get
            Set(value As String)
                _Struc.SubBlockNames = New TLIST(value)
            End Set
        End Property
        Friend Property SuppressEvents As Boolean
            Get
                Return bSuppressEvents
            End Get
            Set(value As Boolean)
                Entities.SuppressEvents = value
                bSuppressEvents = value
            End Set
        End Property
        Friend Property SuppressInstances As Boolean
            Get
                Return _Struc.SuppressInstances
            End Get
            Set(value As Boolean)
                _Struc.SuppressInstances = value
            End Set
        End Property
        Public Property Tag As String
            Get
                '^the dxf entity's tag
                '~all dxf entities have a tag property to allow users to assign strings to a particular entity.
                Return _Struc.Tag
            End Get
            Set(value As String)
                '^the dxf entity's tag
                '~all dxf entities have a tag property to allow users to assign strings to a particular entity.
                _Struc.Tag = value
            End Set
        End Property
        Public Property TypeFlag As Integer
            Get
                '^the entity type flag used in DXF code generation
                Dim _rVal As Integer
                If _Struc.IsAnonomous Then _rVal = 1
                If EditableAttributes Then _rVal += 2
                If _Struc.IsExref Then _rVal += 4
                If _Struc.IsOverlay Then _rVal += 8
                If _Struc.IsExDependant Then _rVal += 16
                If _Struc.IsResolved Then _rVal += 32
                If _Struc.IsExrefed Then _rVal += 64
                Return _rVal
            End Get
            Set(value As Integer)
                '^the entity type flag used in DXF code generation
                Dim valu As Integer
                _Struc.IsAnonomous = False
                _Struc.IsExDependant = False
                _Struc.IsExref = False
                _Struc.IsExrefed = False
                _Struc.IsOverlay = False
                _Struc.IsResolved = False
                valu = value
                If valu >= 64 Then
                    _Struc.IsExrefed = True
                    valu -= 64
                End If
                If valu >= 32 Then
                    _Struc.IsResolved = True
                    valu -= 32
                End If
                If valu >= 16 Then
                    _Struc.IsExDependant = True
                    valu -= 16
                End If
                If valu >= 8 Then
                    _Struc.IsOverlay = True
                    valu -= 8
                End If
                If valu >= 4 Then
                    _Struc.IsExref = True
                    valu -= 4
                End If
                If valu >= 2 Then
                    valu -= 2
                End If
                If valu >= 1 Then
                    _Struc.IsAnonomous = True
                    valu -= 1
                End If
            End Set
        End Property
        Public Property UniformScale As Boolean
            Get
                Return BlockRecord.Properties.ValueS("UniformScale")
            End Get
            Set(value As Boolean)
                BlockRecord.Properties.SetVal("UniformScale", value)
            End Set
        End Property
        Friend Property VIEWPORT As TTABLEENTRY
            Get
                Return _Struc.VIEWPORT
            End Get
            Set(value As TTABLEENTRY)
                _Struc.VIEWPORT = value
            End Set
        End Property
        Public ReadOnly Property X As Double
            Get
                Return Plane.X
            End Get
        End Property
        Public Property XRefPath As String
            Get
                Return _Struc.XRefPath
            End Get
            Set(value As String)
                _Struc.XRefPath = value
            End Set
        End Property
        Public ReadOnly Property Y As Double
            Get
                Return Plane.Y
            End Get
        End Property
        Public ReadOnly Property Z As Double
            Get
                Return Plane.Z
            End Get
        End Property
        Public Property Plane As dxfPlane
            Get
                If _Plane Is Nothing Then
                    _Plane = New dxfPlane() With {.OwnerPtr = New WeakReference(Me)}

                    'AddHandler _Plane.PlaneChange, AddressOf _Plane_PlaneChange
                End If

                Return _Plane
            End Get
            Set(value As dxfPlane)
                'If _Plane  IsNot Nothing Then RemoveHandler _Plane.PlaneChange, AddressOf _Plane_PlaneChange
                If value Is Nothing Then
                    If _Plane IsNot Nothing Then _Plane.OwnerPtr = Nothing
                    _Plane = Nothing
                    Return
                End If
                If _Plane.AlignTo(value, bMoveTo:=True) Then
                    _Struc.Plane = _Plane.Strukture
                    _Struc.PathsDefined = False
                End If
                'If _Plane  IsNot Nothing Then AddHandler _Plane.PlaneChange, AddressOf _Plane_PlaneChange
            End Set
        End Property
        Friend Property PlaneV As TPLANE
            Get
                Return Plane.Strukture
            End Get
            Set(value As TPLANE)
                If TPLANE.IsNull(value) Then value = New TPLANE(Plane.Name)
                If Plane.Define(value.Origin, value.XDirection, value.YDirection, bSuppressEvnts:=True) Then
                    _Struc.Plane = Plane.Strukture
                    _Struc.PathsDefined = False
                End If
            End Set
        End Property

        Private _Instances As dxoInstances

        ''' <summary>
        '''provided for clients to associate instance to the block which can be used to insert multible instances of the block into an image.
        ''' </summary>
        ''' <remarks> See dxoDrawingTool.aInserts (aBlock as dxfBlock .... etc)</remarks>
        ''' <returns></returns>
        Public Property Instances As dxoInstances
            Get
                If _Instances Is Nothing Then _Instances = New dxoInstances()
                Return _Instances
            End Get
            Set(value As dxoInstances)
                _Instances = value

            End Set
        End Property
#End Region 'Properties
#Region "Methods"
        Public Function AttDefs(Optional bConvertToAttribs As Boolean = False) As List(Of dxeText)
            Dim _rVal As List(Of dxeText) = Entities.AttDefs()
            '^returns the attribute objects of all the att definitions in the block
            If bConvertToAttribs Then

                Dim aRet As New List(Of dxeText)
                For Each aTxt As dxeText In _rVal
                    aRet.Add(aTxt.Clone(Nothing, aNewTextType:=dxxTextTypes.Attribute))
                Next
                _rVal = aRet
            End If
            Return _rVal
        End Function
        Public Function BoundingRectangle(aPlane As dxfPlane) As dxfRectangle
            If dxfPlane.IsNull(aPlane) Then
                Return New dxfRectangle(BoundingRectangleV(Plane.Strukture))
            Else
                Return New dxfRectangle(BoundingRectangleV(aPlane.Strukture))
            End If
        End Function
        Friend Function BoundingRectangleV(aPlane As TPLANE) As TPLANE
            Return Entities.BoundingRectangleV(aPlane)
        End Function
        Friend Sub Clear(bDestroy As Boolean, aImage As dxfImage)
            If Not bDestroy Then
                _Entities.Clear(bSuppressEvnts:=True)
                Return
            End If
            'If _Events IsNOt Nothing Then RemoveHandler _Events.BlockRequest, AddressOf _Events_BlockRequest
            '_Events = Nothing
            Dim HG As dxoHandleGenerator = Nothing
            Dim bhndls As Boolean = aImage IsNot Nothing
            If bhndls Then HG = aImage.HandleGenerator
            If bhndls Then
                HG.ReleaseHandle(Handle)
                HG.ReleaseHandle(Me.EndBlockHandle)
                HG.ReleaseHandle(Me.BlockRecordHandle)
                If HasEntities Then
                    HG.ReleaseHandles(_Entities)
                End If
            End If
            If _Entities IsNot Nothing Then
                'RemoveHandler _Entities.EntitiesChange, AddressOf _Entities_EntitiesChange
                ' RemoveHandler _Entities.EntitiesMemberChange, AddressOf _Entities_EntitiesMemberChange
                '_Entities.Clear(True, aImage)
                _Entities.Dispose()
            End If
            _Entities = Nothing
            ReleaseReferences()
            If _Plane IsNot Nothing Then
                _Plane.OwnerPtr = Nothing
                'RemoveHandler _Plane.PlaneChange, AddressOf _Plane_PlaneChange
            End If
            _Plane = Nothing
            _BlockRecord = Nothing
            _Struc.RelativePaths.Clear()
            _Struc.VIEWPORT.Clear()
            _Struc.SubBlockNames.Clear()
            _Struc = Nothing
        End Sub
        Public Function Clone() As dxfBlock
            '^returns a new object with properties matching those of the cloned object
            Return New dxfBlock(Me)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New dxfBlock(Me)
        End Function
        Friend Function CloneAll(aImage As dxfImage) As dxfBlock
            '^returns a new object with properties matching those of the cloned object
            Dim _rVal As New dxfBlock With {.Strukture = _Struc}
            _rVal.Handlez = Handlez
            _rVal.SetEntities(Entities.CloneAll(aImage, False))
            Return _rVal
        End Function
        Public Function Entity(aIndex As Integer) As dxfEntity
            '#1the index of the requested entity
            '^used to get a DXF entity from the blocks entities collection.
            '~returns Nothing if the passed index is outside of the bounds of the entities collection.
            '~no error raised.
            Return Entities.Item(aIndex)
        End Function



        ''' <summary>
        ''' Returns copies of the blocks entities that will be transformed to create the path entities for an insert of the block
        ''' </summary>
        ''' <param name="aImage">the hositng image</param>
        ''' <param name="aInsert">the subject insert entity</param>
        ''' <returns></returns>
        Friend Function InsertEntities(aImage As dxfImage, aInsert As dxeInsert, Optional aTransforms As TTRANSFORMS? = Nothing) As List(Of dxfEntity)
            Dim _rVal As New List(Of dxfEntity)

            Dim atts_Insert As List(Of dxeText) = Nothing
            Dim trans As dxfVector = Nothing
            Dim scaleX As Double = 1
            Dim myPlane As dxfPlane = Plane
            Dim insPlane As dxfPlane = Nothing
            Dim rot As Double = 0
            GetImage(aImage)
            If aInsert IsNot Nothing Then
                atts_Insert = aInsert.GetAttributes(aImage, Me, False)

                If Not aTransforms.HasValue Then
                    aTransforms = aInsert.Transforms(Nothing, Me)
                End If
                trans = aTransforms.Value.Item(dxxTransformationTypes.Translation).Translation

                scaleX = aTransforms.Value.Item(dxxTransformationTypes.Scale).ScaleFactor
                insPlane = aInsert.Plane
                rot = aTransforms.Value.Item(dxxTransformationTypes.Rotation).Angle
                If rot <> 0 Then insPlane.Rotate(rot)
            End If

            Try

                Dim regents As List(Of dxfEntity) = Entities.ToList()
                Dim attribs As List(Of dxfEntity) = regents.FindAll(Function(x) x.EntityType = dxxEntityTypes.Attdef)

                If attribs.Count > 0 Then
                    regents.RemoveAll(Function(x) x.EntityType = dxxEntityTypes.Attdef)



                    'just add the non atribute entities
                    _rVal.AddRange(regents)
                    If aInsert IsNot Nothing Then

                        For Each att As dxeText In atts_Insert
                            _rVal.Add(att)
                        Next


                    Else
                        'format the attributes
                        For Each ent In attribs

                            'convert the attdefs to attributes
                            Dim txt As dxeText = DirectCast(ent, dxeText)
                            Dim attrib As dxeText = txt.Clone(Nothing, aNewTextType:=dxxTextTypes.Attribute)


                            'If atts_Insert IsNot Nothing Then
                            '    Dim iatt As dxfAttribute = atts_Insert.Find(Function(x) String.Compare(x.SourceGUID, txt.GUID, True) = 0)
                            '    If iatt Is Nothing Then iatt = atts_Insert.Item(txt.AttributeTag)
                            '    If iatt Is Nothing Then
                            '        iatt = New dxfAttribute(txt, aInsert.GUID)
                            '        aInsert.Attributes.Add(iatt)
                            '    End If
                            '    '    Dim htdif As Double = iatt.TextHeight - txt.TextHeight

                            '    '    Dim v1 As dxfVector = ent.Properties.ValueV(10).WithRespectTo(myPlane)
                            '    '    Dim v2 As dxfVector = ent.Properties.ValueV(11).WithRespectTo(myPlane)

                            '    '    iatt.Properties.SetVector(10, insPlane.Vector(v1.X * scaleX, v1.Y * scaleX))
                            '    '    iatt.Properties.SetVector(11, insPlane.Vector(v2.X * scaleX, v2.Y * scaleX))
                            '    '    iatt.TextHeight = txt.TextHeight * scaleX
                            '    '    iatt.Rotation = txt.Rotation + rot
                            '    attrib.Properties.CopyVals(dxfAttributes.IndependantGroupCodes, iatt.Properties)
                            'End If

                            _rVal.Add(attrib)
                        Next
                    End If


                Else
                    _rVal.AddRange(regents)
                End If
                Return _rVal



            Catch ex As Exception

            Finally

            End Try

            Return _rVal



        End Function

        Friend Function InsertPaths(aImage As dxfImage, aInsert As dxeInsert, bRegen As Boolean) As TPATHS
            Dim _rVal As TPATHS = TPATHS.NullEnt(aInsert)
            If Not GetImage(aImage) Or aInsert Is Nothing Then Return _rVal

            Dim rot As Double = aInsert.RotationAngle
            Dim scales As TVECTOR = aInsert.ScaleVector()
            Dim iGUID As String = aImage.GUID
            Dim idsp As TDISPLAYVARS = aInsert.DisplayStructure
            Dim iPlane As TPLANE = aInsert.PlaneV
            Dim myPlane As TPLANE = PlaneV
            'get the entities
            Dim aEnts As List(Of dxfEntity) = InsertEntities(aImage, aInsert)


            For Each aEnt As dxfEntity In aEnts
                'get the entities paths
                Dim regen As Boolean = False
                If aEnt.GraphicType = dxxGraphicTypes.Text Then
                    If aEnt.EntityType = dxxEntityTypes.Attribute Then
                        regen = True
                    End If
                End If
                aEnt.UpdatePath(bRegen Or regen, aImage)
                Dim entPaths As TPATHS = New TPATHS(aEnt.Paths)
                _rVal.ExtentVectors.Append(entPaths.ExtentVectors)
                For ip = 1 To entPaths.Count
                    Dim aPth As TPATH = entPaths.Item(ip)
                    Dim bAddit As Boolean = False
                    If aPth.LoopCount > 0 Then
                        If aPth.Looop(1).Count > 0 Then bAddit = True
                    End If
                    If bAddit Then
                        If aPth.Color = dxxColors.ByBlock Then aPth.Color = idsp.Color
                        If aPth.LineWeight = dxxLineWeights.ByBlock Then aPth.LineWeight = idsp.LineWeight
                        If String.Compare(aPth.Linetype, dxfLinetypes.ByBlock, True) = 0 Then aPth.Linetype = idsp.Linetype
                        If aPth.Relative Then
                            aPth.ConvertToWorld()
                        End If
                        _rVal.AddOrJoin(aPth)
                        '_rVal.Add(aPth)
                    End If
                    '            pths_AddOrJoin InsertPaths, aPth
                Next ip

            Next
            'get the paths with respect to the insert plane
            'InsertPaths.Members(0).Loops(0).Print
            _rVal.TransferToPlane(myPlane, iPlane, scales.X, scales.Y, scales.Z, rot, bKeepOrigin:=False, bReturnRelativePaths:=False)

            Return _rVal
        End Function
        Friend Function InsertPaths(aDomain As dxxDrawingDomains, aScaleFactor As Double, aRotation As Double, aPlane As TPLANE, aImage As dxfImage, bUniformScale As Boolean, aDisplayVars As TDISPLAYVARS, bRegen As Boolean) As TPATHS
            Dim _rVal As New TPATHS(aDomain)
            If Not GetImage(aImage) Then Return _rVal
            If aScaleFactor = 0 Then aScaleFactor = 1

            Dim aPth As TPATH
            Dim entPaths As TPATHS
            Dim iGUID As String = aImage.GUID
            Dim ip As Integer
            Dim aAng As Double
            Dim bAddit As Boolean
            Dim iClr As dxxColors = aDisplayVars.Color
            Dim iLt As String = aDisplayVars.Linetype
            Dim iLWt As dxxLineWeights = aDisplayVars.LineWeight
            Dim Scls As TVECTOR
            Dim iPl As TPLANE = aPlane
            Dim bPl As New TPLANE(InsertionPtV, TVECTOR.WorldX, TVECTOR.WorldY)
            'init
            Scls = New TVECTOR(aScaleFactor, aScaleFactor, aScaleFactor)
            aAng = TVALUES.NormAng(aRotation, False, True, True)
            'get the entities

            Dim aEnts As List(Of dxfEntity) = InsertEntities(aImage, Nothing)

            For Each aEnt As dxfEntity In aEnts
                aEnt.UpdatePath(bRegen, aImage)
                entPaths = New TPATHS(aEnt.Paths)
                _rVal.ExtentVectors.Append(entPaths.ExtentVectors)
                For ip = 1 To entPaths.Count
                    aPth = entPaths.Item(ip)
                    If aPth.LoopCount > 0 Then
                        If aPth.Looop(1).Count > 0 Then bAddit = True
                    End If
                    If bAddit Then
                        If aPth.Color = dxxColors.ByBlock Then aPth.Color = iClr
                        If aPth.LineWeight = dxxLineWeights.ByBlock Then aPth.LineWeight = iLWt
                        If String.Compare(aPth.Linetype, dxfLinetypes.ByBlock, True) = 0 Then aPth.Linetype = iLt
                        If aPth.Relative Then
                            aPth.ConvertToWorld()
                        End If
                        _rVal.AddOrJoin(aPth)
                        '_rVal.Add(aPth)
                    End If
                    '            pths_AddOrJoin InsertPaths, aPth
                Next ip
            Next
            'get the paths with respect to the insert plane
            'InsertPaths.Members(0).Loops(0).Print
            _rVal.TransferToPlane(bPl, iPl, Scls.X, Scls.Y, Scls.Z, aAng, bKeepOrigin:=False, bReturnRelativePaths:=False)
            'format the attributes
            'If rAttributes.Count > 0 Then
            '    Dim bTrs As New TTRANSFORMS
            '    'add translation to block origin and to the insert pt
            '    bTrs.Add(TTRANSFORM.CreateTranslation(iPl.Origin - bPl.Origin, True))
            '    'add insert rotation
            '    If aAng <> 0 Then
            '        bTrs.Add(TTRANSFORM.CreateRotation(iPl.Origin, aAng, False, bPl.ZDirection, True))
            '    End If
            '    'add the scales
            '    bTrs.Add(TTRANSFORM.CreateScale(iPl.Origin, Scls.X, Scls.Y, Scls.Z, Nothing, True))
            '    For i As Integer = 1 To rAttributes.Count
            '        aTxt = rAttributes.Item(i)
            '        aTxt.Transform(bTrs, True)
            '        aTxt.UpdatePath(False, aImage)

            '    Next i
            'End If
            '   aImage.HandleGenerator.AssignTo(rAttribs)
            Return _rVal
        End Function
        Public Function Move(Optional aXChange As Double = 0.0, Optional aYChange As Double = 0.0, Optional aZChange As Double = 0.0, Optional aPlane As dxfPlane = Nothing) As Boolean
            Return Translate(New TVECTOR(aXChange, aYChange, aZChange), aPlane)
            '^can be used to move an existing Block so it will appear somewhere else in the outputed DXF file
        End Function
        Friend Function Translate(aVector As TVECTOR, Optional aPlane As dxfPlane = Nothing) As Boolean
            Dim _rVal As Boolean = False
            '^can be used to move an existing Block so it will appear somewhere else in the outputed DXF file
            If Plane.Translate(aVector, bSuppressEvnts:=True, aPlane) Then _rVal = True
            If Entities.TranslateV(aVector, aPlane) Then _rVal = True
            If _rVal Then _Struc.PathsDefined = False
            Return _rVal
        End Function
        Public Function MoveFromTo(aBasePoint As iVector, aDestinationPoint As iVector) As Boolean
            Dim v1 As TVECTOR = New TVECTOR(aBasePoint)
            Dim v2 As TVECTOR = New TVECTOR(aDestinationPoint)
            '^used to move the block from one reference point to another
            Return Translate(v1 - v2)
        End Function
        Public Function MoveTo(DestinationObject As iVector, Optional ChangeX As Double = 0.0, Optional ChangeY As Double = 0.0, Optional ChangeZ As Double = 0.0) As Boolean
            Dim v1 As New TVECTOR(DestinationObject)
            If ChangeX <> 0 Or ChangeY <> 0 Or ChangeZ Then
                v1 += New TVECTOR(ChangeX, ChangeY, ChangeZ)
            End If

            Return Translate(v1 - InsertionPtV)
        End Function
        Private Sub RaiseChangeEvents(aName As String, aOldValue As Object, aNewValue As Object)
            If (ImageGUID <> "") And Not bSuppressEvents Then
                Dim aImage As dxfImage = Nothing
                Dim err As String = String.Empty
                If GetImage(aImage) Then aImage.RespondToBlockChange(Me, aName, aOldValue, aNewValue, False, err) Else ImageGUID = ""
            End If
        End Sub
        Friend Sub ReferenceAdd(aHandle As String)
            BlockRecord.Reactors.AddReactor("{ACAD_REACTORS", 331, aHandle, True)
        End Sub
        Friend Sub ReferenceRemove(aHandle As String)

            Dim aProps As dxoProperties = BlockRecord.Reactors.Item("{ACAD_REACTORS")
            If aProps Is Nothing Then Return
            aProps.RemoveAll(Function(x) String.Compare(x.ValueS, aHandle, True) = 0)
        End Sub
        Public Function ReferencesBlock(aBlockName As String) As Boolean
            Return Entities.ReferencesBlock(aBlockName)
        End Function
        Friend Sub ResetReferences()
            BlockRecord.Reactors.ClearMember("{BLKREFS", True)
        End Sub
        Public Function RotateAbout(aLineOrPointObj As iLine, aAngle As Double, Optional bInRadians As Boolean = False, Optional aCS As dxfPlane = Nothing) As Boolean

            Dim _rVal As Boolean = False
            '#1the line or point to rotate about
            '#2the angle to rotate
            '#3flag indicating the passed angle is in radians
            '#4the coordinate system which is used as the Z axis of rotation if the paased object is a point
            '^rotates the entity about the passed axis the requested angle
            '~if the passed object is a point the entity is rotated about the Z axis of the passed coordinated system
            aAngle = TVALUES.NormAng(aAngle, bInRadians, True)
            If aAngle = 0 Then Return _rVal
            Dim P1 As dxfPlane = Plane.Clone
            Dim aAxis As dxeLine = Nothing
            If P1.RotateAbout(aLineOrPointObj, aAngle, bInRadians:=bInRadians, bRotateOrigin:=True, bRotateDirections:=False, aAxis:=aAxis) Then
                Plane = P1
                _Struc.PathsDefined = False
                _rVal = True
                Entities.Move(X - P1.X, Y - P1.Y, Z - P1.Z)
            End If
            If _rVal Then _Struc.PathsDefined = False
            Return _rVal
        End Function
        Public Function RotateAbout(aPoint As iVector, aAngle As Double, Optional bInRadians As Boolean = False, Optional aCS As dxfPlane = Nothing) As Boolean

            Dim _rVal As Boolean = False
            '#1the line or point to rotate about
            '#2the angle to rotate
            '#3flag indicating the passed angle is in radians
            '#4the coordinate system which is used as the Z axis of rotation if the paased object is a point
            '^rotates the entity about the passed axis the requested angle
            '~if the passed object is a point the entity is rotated about the Z axis of the passed coordinated system
            aAngle = TVALUES.NormAng(aAngle, bInRadians, True)
            If aAngle = 0 Then Return _rVal
            Dim P1 As dxfPlane = Plane.Clone
            Dim zdir As TVECTOR = IIf(aCS Is Nothing, P1.ZDirectionV, aCS.ZDirectionV)
            Dim sp As TVECTOR = New TVECTOR(aPoint)
            Dim line As New TLINE(sp, sp + zdir * 10)
            Dim aAxis As dxeLine = Nothing
            If P1.RotateAboutLine(line, aAngle, bInRadians, True, False) Then
                Plane = P1
                _Struc.PathsDefined = False
                _rVal = True
                Entities.Move(X - P1.X, Y - P1.Y, Z - P1.Z)
            End If
            If _rVal Then _Struc.PathsDefined = False
            Return _rVal
        End Function

        Friend Sub SetEntities(ByRef newobj As colDXFEntities)
            If newobj IsNot Nothing Then _Entities = newobj Else _Entities = New colDXFEntities
            _Entities.SetGUIDS(aImageGUID:=ImageGUID, aOwnerGUID:=String.Empty, aBlockGUID:=GUID, aOwnerType:=dxxFileObjectTypes.Block, aBlock:=Me)
            _Struc.PathsDefined = False
        End Sub
        Public Overrides Function ToString() As String
            Return $"{GUID} [{Name }]"
        End Function
        Friend Overrides Sub ReleaseReferences()
            MyBase.ReleaseReferences()
            If _Entities IsNot Nothing Then
                _Entities.ReleaseReferences()
            End If
        End Sub
#End Region 'Methods
#Region "IDisposable Implementation"
        'Protected Overrides Sub Finalize()
        '    MyBase.Finalize()
        'End Sub
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not _Disposed Then
                If disposing Then
                    ' dispose managed state (managed objects)
                    If _Entities IsNot Nothing Then
                        _Entities.SetBlock(Nothing, False)
                        _Entities.SetImage(Nothing, False)
                        _Entities.Clear(True, Nothing, False)
                    End If
                    ReleaseReferences()
                    _Plane = Nothing
                    _Entities = Nothing
                    _Disposed = True
                End If
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
#Region "_Entities_EventHandlers"
        Friend Overrides Sub RespondToEntitiesChangeEvent(aEvent As dxfEntitiesEvent)
            If aEvent Is Nothing Then Return
            aEvent.OwnerNotified = True
            Dim aEnt As dxfEntity
            Dim aInsert As dxeInsert
            Select Case aEvent.EventType
                Case dxxCollectionEventTypes.PreAdd
                    aEnt = aEvent.Member
                    If Not aEnt Is Nothing Then
                        If aEnt.EntityType = dxxEntityTypes.Attribute Then aEvent.Undo = True
                    End If
                    If aEvent.Undo Then Return
                Case dxxCollectionEventTypes.Add
                    aEnt = aEvent.Member
                    If Not aEnt Is Nothing Then
                        If aEnt.GraphicType = dxxGraphicTypes.Insert Then
                            aInsert = aEnt
                            _Struc.SubBlockNames.Add(aInsert.BlockName)
                        End If
                    End If
                Case dxxCollectionEventTypes.Remove
                    aEnt = aEvent.Member
                    If Not aEnt Is Nothing Then
                        If aEnt.GraphicType = dxxGraphicTypes.Insert Then
                            aInsert = aEnt
                            _Struc.SubBlockNames.Remove(aInsert.BlockName)
                        End If
                    End If
            End Select
            _Struc.PathsDefined = False
            If aEvent.CountChange And Not aEvent.Undo Then
                RaiseChangeEvents("BlockChange", _Entities.Count - 1, _Entities.Count)
            End If
        End Sub
        Friend Sub RespondToEntitiesMemberChangeEvent(aEvent As dxfEntityEvent)
            If aEvent Is Nothing Then Return
            IsDirty = True
            RaiseChangeEvents("BlockChange." & aEvent.PropertyName, aEvent.OldValue, aEvent.NewValue)
        End Sub
#End Region '_Entities_EventHandlers
#Region "oEvents_EventHandlers"
        'Private Sub _Events_BlockRequest(aGUID As String, ByRef rBlock As dxfBlock)
        '    If aGUID = GUID Then rBlock = Me
        'End Sub
#End Region 'oEvents_EventHandlers
#Region "_Plane_EventHandlers"
        Friend Overrides Sub RespondToPlaneChangeEvent(aEvent As dxfPlaneEvent)
            If aEvent Is Nothing Then Return
            aEvent.OwnerNotified = True
            _Struc.Plane = _Plane.Strukture
            If TVALUES.BitCode_FindSubCode(TVALUES.To_INT(aEvent.EventType), dxxCoordinateSystemEventTypes.Orientation) Or TVALUES.BitCode_FindSubCode(TVALUES.To_INT(aEvent.EventType), dxxCoordinateSystemEventTypes.Origin) Then
                _Struc.PathsDefined = False
            End If
        End Sub
#End Region '_Plane_EventHandlers

#Region "Shared Methods"


#End Region 'Shared Methods

    End Class 'dxfBlock
End Namespace
