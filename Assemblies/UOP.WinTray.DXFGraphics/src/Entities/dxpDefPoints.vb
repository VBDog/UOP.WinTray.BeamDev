Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Friend Class dxpDefPoints
        Implements IDisposable
#Region "Members"
        Private _ImageGUID As String
        Private _BlockGUID As String
        Private _EntityGUID As String
        Private _Vector1 As dxfVector
        Private _Vector2 As dxfVector
        Private _Vector3 As dxfVector
        Private _Vector4 As dxfVector
        Private _Vector5 As dxfVector
        Private _Vector6 As dxfVector
        Private _Vector7 As dxfVector
        Private _Vertices As colDXFVectors
        Private bTransforming As Boolean
        Private disposedValue As Boolean
        Friend OwnerPtr As WeakReference
#End Region 'Members
#Region "Events"
        'Friend Event VectorChange(aVector As dxfVector, aIndex As Integer, ByRef rDontDirty As Boolean)
#End Region 'Events
#Region "Constructors"
        Friend Sub New(aGraphicType As dxxGraphicTypes, aEntity As dxfEntity)
            'init ----------------------------------------------
            _DefPtCnt = 0
            _Vector1 = Nothing
            _Vector2 = Nothing
            _Vector3 = Nothing
            _Vector4 = Nothing
            _Vector5 = Nothing
            _Vector6 = Nothing
            _Vector7 = Nothing
            Vertices = Nothing
            IsDirty = False
            OwnerGUID = ""
            _Plane = TPLANE.World
            SuppressEvents = False
            GraphicType = dxxGraphicTypes.Undefined
            _OwnerGUID = String.Empty
            OwnerPtr = Nothing

            'init ----------------------------------------------
            GraphicType = aGraphicType
            If _DefPtCnt >= 1 Then _Vector1 = dxfVector.Zero
            If _DefPtCnt >= 2 Then _Vector2 = dxfVector.Zero
            If _DefPtCnt >= 3 Then _Vector3 = dxfVector.Zero
            If _DefPtCnt >= 4 Then _Vector4 = dxfVector.Zero
            If _DefPtCnt >= 5 Then _Vector5 = dxfVector.Zero
            If _DefPtCnt >= 6 Then _Vector6 = dxfVector.Zero
            If _DefPtCnt >= 7 Then _Vector7 = dxfVector.Zero
            If HasVertices Then Vertices = New colDXFVectors()

            If aEntity IsNot Nothing Then
                EntityGUID = aEntity.GUID
                OwnerPtr = New WeakReference(aEntity)
            End If
        End Sub

        Friend Sub New(aDefPts As TDEFPOINTS)
            'init ----------------------------------------------
            _DefPtCnt = 0
            _Vector1 = Nothing
            _Vector2 = Nothing
            _Vector3 = Nothing
            _Vector4 = Nothing
            _Vector5 = Nothing
            _Vector6 = Nothing
            _Vector7 = Nothing
            Vertices = Nothing
            IsDirty = False
            OwnerGUID = ""
            Plane = TPLANE.World
            SuppressEvents = False
            GraphicType = dxxGraphicTypes.Undefined
            _OwnerGUID = String.Empty
            OwnerPtr = Nothing
            'init ----------------------------------------------
            GraphicType = aDefPts.GraphicType
            IsDirty = aDefPts.IsDirty
            SuppressEvents = aDefPts.SuppressEvents
            If _DefPtCnt >= 1 Then _Vector1 = New dxfVector(aDefPts.DefPt1)
            If _DefPtCnt >= 2 Then _Vector2 = New dxfVector(aDefPts.DefPt2)
            If _DefPtCnt >= 3 Then _Vector3 = New dxfVector(aDefPts.DefPt3)
            If _DefPtCnt >= 4 Then _Vector4 = New dxfVector(aDefPts.DefPt4)
            If _DefPtCnt >= 5 Then _Vector5 = New dxfVector(aDefPts.DefPt5)
            If _DefPtCnt >= 6 Then _Vector6 = New dxfVector(aDefPts.DefPt6)
            If _DefPtCnt >= 7 Then _Vector7 = New dxfVector(aDefPts.DefPt7)
            If HasVertices Then Vertices = New colDXFVectors(aDefPts.Verts)

        End Sub

        Friend Sub New(aDefPts As dxpDefPoints)
            'init ----------------------------------------------
            _DefPtCnt = 0
            _Vector1 = Nothing
            _Vector2 = Nothing
            _Vector3 = Nothing
            _Vector4 = Nothing
            _Vector5 = Nothing
            _Vector6 = Nothing
            _Vector7 = Nothing
            Vertices = Nothing
            IsDirty = False
            OwnerGUID = ""
            Plane = TPLANE.World
            SuppressEvents = False
            GraphicType = dxxGraphicTypes.Undefined
            _OwnerGUID = String.Empty
            OwnerPtr = Nothing
            'init ----------------------------------------------
            If aDefPts Is Nothing Then Return
            GraphicType = aDefPts.GraphicType
            IsDirty = aDefPts.IsDirty
            SuppressEvents = aDefPts.SuppressEvents



            If _DefPtCnt >= 1 Then _Vector1 = New dxfVector(aDefPts._Vector1)
            If _DefPtCnt >= 2 Then _Vector2 = New dxfVector(aDefPts._Vector2)
            If _DefPtCnt >= 3 Then _Vector3 = New dxfVector(aDefPts._Vector3)
            If _DefPtCnt >= 4 Then _Vector4 = New dxfVector(aDefPts._Vector4)
            If _DefPtCnt >= 5 Then _Vector5 = New dxfVector(aDefPts._Vector5)
            If _DefPtCnt >= 6 Then _Vector6 = New dxfVector(aDefPts._Vector6)
            If _DefPtCnt >= 7 Then _Vector7 = New dxfVector(aDefPts._Vector7)
            If HasVertices Then Vertices = New colDXFVectors(aDefPts._Vertices, bAddClones:=True)

        End Sub

#End Region 'Constructors
#Region "Properties"
        Private ReadOnly Property MyOwner As dxfEntity
            Get
                If String.IsNullOrWhiteSpace(_EntityGUID) Or OwnerPtr Is Nothing Then Return Nothing
                Dim _rVal As dxfEntity = TryCast(OwnerPtr.Target, dxfEntity)
                If _rVal IsNot Nothing Then
                    If String.Compare(_EntityGUID, _rVal.GUID, ignoreCase:=True) Then
                        ReleaseOwnerReference()
                        _rVal = Nothing
                    End If
                End If
                Return _rVal
            End Get
        End Property

        Private _DefPtCnt As Integer
        Public Property DefPtCnt As Integer
            Get
                Return _DefPtCnt
            End Get
            Private Set(value As Integer)
                _DefPtCnt = value
            End Set
        End Property

        Friend ReadOnly Property HasVertices As Boolean
            Get
                Return TENTITY.HasVertices(GraphicType)
            End Get
        End Property

        Private _GraphicType As dxxGraphicTypes
        Public Property GraphicType As dxxGraphicTypes
            Get
                Return _GraphicType
            End Get
            Private Set(value As dxxGraphicTypes)
                If value <> _GraphicType Then
                    _GraphicType = value
                    _DefPtCnt = dxfEntity.DefPointCount(value)
                End If
                _GraphicType = value
            End Set
        End Property
        Friend ReadOnly Property GraphicTypeName As String
            Get
                Return dxfEnums.Description(GraphicType)
            End Get
        End Property

        Private _IsDirty As Boolean
        Public Property IsDirty As Boolean
            Get
                Return _IsDirty
            End Get
            Friend Set(value As Boolean)
                _IsDirty = value
            End Set
        End Property

        Private _OwnerGUID As String
        Public Property OwnerGUID As String
            Get
                Return _OwnerGUID
            End Get
            Friend Set(value As String)
                _OwnerGUID = value
            End Set
        End Property

        Private _Plane As TPLANE

        Friend Property Plane As TPLANE
            Get
                If HasVertices And GraphicType = dxxGraphicTypes.Polyline Then
                    _Plane.Origin = Vertices.ItemVector(1, True)
                Else
                    If DefPtCnt >= 1 Then _Plane.Origin = Vector1.Strukture
                End If
                Return _Plane
            End Get
            Set(value As TPLANE)
                Dim bFlag As Boolean
                Dim b1 As Boolean = False
                Dim b2 As Boolean = False
                Dim b3 As Boolean = False
                Dim newPL As TPLANE = _Plane.ReDefined(value.Origin, value.XDirection, value.YDirection, b1, b2, b3, bFlag)
                If bFlag Then IsDirty = True
                _Plane = newPL
            End Set
        End Property
        Public Property PlaneObj As dxfPlane
            Get
                Return New dxfPlane(Plane)
            End Get
            Friend Set(value As dxfPlane)
                Plane = New TPLANE(value)
            End Set
        End Property

        Private _SuppressEvents As Boolean
        Friend Property SuppressEvents As Boolean
            Get
                Return _SuppressEvents
            End Get
            Set(value As Boolean)
                _SuppressEvents = value
            End Set
        End Property
        Friend Property Units As dxxDeviceUnits
            Get
                Return _Plane.Units
            End Get
            Set(value As dxxDeviceUnits)
                _Plane.Units = value
            End Set
        End Property

        Public Function DefiningVectors(Optional bGetClones As Boolean = False) As colDXFVectors

            Dim _rVal As New List(Of dxfVector)
            If HasVertices Then _rVal.AddRange(Vertices)
            If DefPtCnt >= 1 Then _rVal.Add(Vector1)
            If DefPtCnt >= 2 Then _rVal.Add(Vector2)
            If DefPtCnt >= 3 Then _rVal.Add(Vector3)
            If DefPtCnt >= 4 Then _rVal.Add(Vector4)
            If DefPtCnt >= 5 Then _rVal.Add(Vector5)
            If DefPtCnt >= 6 Then _rVal.Add(Vector6)
            If DefPtCnt >= 7 Then _rVal.Add(Vector7)
            Return New colDXFVectors(_rVal, bGetClones)

        End Function

        Friend Property Vector1 As dxfVector
            '^the first defining vector
            Get
                If DefPtCnt < 1 Then _Vector1 = Nothing
                Return GetVector(1)
            End Get
            Set(value As dxfVector)
                SetVector(value, 1)
            End Set
        End Property

        Friend Property Vector2 As dxfVector
            '^the second defining vector
            Get
                If DefPtCnt < 2 Then _Vector2 = Nothing
                Return GetVector(2)
            End Get
            Set(value As dxfVector)
                SetVector(value, 2)
            End Set
        End Property

        Friend Property Vector3 As dxfVector
            '^the third defining vector
            Get
                If DefPtCnt < 3 Then _Vector3 = Nothing
                Return GetVector(3)
            End Get
            Set(value As dxfVector)
                SetVector(value, 3)
            End Set
        End Property
        Friend Property Vector4 As dxfVector
            '^the fourth defining vector
            Get
                If DefPtCnt < 4 Then _Vector4 = Nothing
                Return GetVector(4)
            End Get
            Set(value As dxfVector)
                SetVector(value, 4)
            End Set
        End Property
        Friend Property Vector5 As dxfVector
            '^the fifth defining vector
            Get
                If DefPtCnt < 5 Then _Vector5 = Nothing
                Return GetVector(5)
            End Get
            Set(value As dxfVector)
                SetVector(value, 5)
            End Set
        End Property
        Friend Property Vector6 As dxfVector
            '^the sixth defining vector
            Get
                If DefPtCnt < 6 Then _Vector6 = Nothing
                Return GetVector(6)
            End Get
            Set(value As dxfVector)
                SetVector(value, 6)
            End Set
        End Property
        Friend Property Vector7 As dxfVector
            '^the seventh defining vector
            Get
                If DefPtCnt < 7 Then _Vector7 = Nothing
                Return GetVector(7)
            End Get
            Set(value As dxfVector)
                SetVector(value, 7)
            End Set
        End Property

        Friend Property Vectors As TVECTORS
            Get
                If HasVertices Then
                    Return New TVECTORS(Vertices)
                Else
                    Vectors = New TVECTORS
                    If DefPtCnt >= 1 Then Vectors.Add(VectorGet(1))
                    If DefPtCnt >= 2 Then Vectors.Add(VectorGet(2))
                    If DefPtCnt >= 3 Then Vectors.Add(VectorGet(3))
                    If DefPtCnt >= 4 Then Vectors.Add(VectorGet(4))
                    If DefPtCnt >= 5 Then Vectors.Add(VectorGet(5))
                    If DefPtCnt >= 6 Then Vectors.Add(VectorGet(6))
                    If DefPtCnt >= 7 Then Vectors.Add(VectorGet(7))
                    Return Vectors
                End If
            End Get
            Set(value As TVECTORS)
                If Not HasVertices Then Return
                Vertices.Populate(value)
                IsDirty = True
            End Set
        End Property
        Friend Property Vertexes As TVERTICES
            Get
                If HasVertices Then Return New TVERTICES(Vertices) Else Return New TVERTICES
            End Get
            Set(value As TVERTICES)
                If Not HasVertices Then Return
                Vertices = New colDXFVectors(value)
                IsDirty = True
            End Set
        End Property
        Friend Property Vertices As colDXFVectors
            Get
                If Not HasVertices Then Return Nothing
                If HasVertices And _Vertices Is Nothing Then _Vertices = New colDXFVectors
                _Vertices.SetGUIDS(_ImageGUID, _EntityGUID, _BlockGUID, SuppressEvents)
                Return _Vertices
            End Get
            Set(value As colDXFVectors)
                If Not HasVertices Then value = Nothing
                If value IsNot Nothing Then
                    If _Vertices Is Nothing Then _Vertices = New colDXFVectors
                    _Vertices.SetGUIDS(_ImageGUID, _EntityGUID, _BlockGUID, SuppressEvents)
                    _Vertices = New colDXFVectors(value, bAddClones:=True)
                Else
                    _Vertices = Nothing
                End If
            End Set
        End Property
        Friend ReadOnly Property X As Double
            Get
                Return VectorGet(1).X
            End Get
        End Property
        Friend ReadOnly Property XDirection As TVECTOR
            Get
                Return _Plane.XDirection
            End Get
        End Property
        Friend ReadOnly Property Y As Double
            Get
                Return VectorGet(1).Y
            End Get
        End Property
        Friend ReadOnly Property YDirection As TVECTOR
            Get
                Return _Plane.YDirection
            End Get
        End Property
        Friend ReadOnly Property Z As Double
            Get
                Return VectorGet(1).Z
            End Get
        End Property
        Friend ReadOnly Property ZDirection As TVECTOR
            Get
                Return _Plane.ZDirection
            End Get
        End Property
        Friend Property ImageGUID As String
            '^the guid of the image that this vector is associated to
            Get
                Return _ImageGUID
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = String.Empty Else value = value.Trim
                _ImageGUID = value
            End Set
        End Property
        Friend Property BlockGUID As String
            '^the guid of the image.block that this vector is associated to
            Get
                Return _BlockGUID
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = String.Empty Else value = value.Trim
                _BlockGUID = value
            End Set
        End Property
        Friend Property EntityGUID As String
            '^the guid of the image.Entity that this vector is associated to
            Get
                Return _EntityGUID
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = String.Empty Else value = value.Trim
                _EntityGUID = value
            End Set
        End Property


#End Region 'Properties
#Region "Methods"
        Public Sub TransferToPlane(aPlane As dxfPlane, aFromPlane As dxfPlane)
            If dxfPlane.IsNull(aPlane) Then Return

            If aFromPlane Is Nothing Then
                aFromPlane = PlaneObj
                If GraphicType = dxxGraphicTypes.Polyline Then
                    aFromPlane.Origin = Vertices.PlanarCenter(aFromPlane)
                End If

            End If
            If _Vector1 IsNot Nothing Then _Vector1 = _Vector1.WithRespectToPlane(aFromPlane, aTransferPlane:=aPlane)
            If _Vector2 IsNot Nothing Then _Vector2 = _Vector2.WithRespectToPlane(aFromPlane, aTransferPlane:=aPlane)
            If _Vector3 IsNot Nothing Then _Vector3 = _Vector3.WithRespectToPlane(aFromPlane, aTransferPlane:=aPlane)
            If _Vector4 IsNot Nothing Then _Vector4 = _Vector4.WithRespectToPlane(aFromPlane, aTransferPlane:=aPlane)
            If _Vector5 IsNot Nothing Then _Vector5 = _Vector5.WithRespectToPlane(aFromPlane, aTransferPlane:=aPlane)
            If _Vector6 IsNot Nothing Then _Vector6 = _Vector6.WithRespectToPlane(aFromPlane, aTransferPlane:=aPlane)
            If _Vector7 IsNot Nothing Then _Vector7 = _Vector7.WithRespectToPlane(aFromPlane, aTransferPlane:=aPlane)
            If _Vertices IsNot Nothing Then _Vertices = _Vertices.WithRespectToPlane(aFromPlane, aTransferPlane:=aPlane)
            Plane = New TPLANE(aPlane)
            IsDirty = True
        End Sub

        Public Function VectorGet(aIndex As Integer) As TVECTOR
            If aIndex > 0 And aIndex <= DefPtCnt Then
                Select Case aIndex
                    Case 1

                        Return New TVECTOR(_Vector1)
                    Case 2
                        Return New TVECTOR(_Vector2)
                    Case 3
                        Return New TVECTOR(_Vector3)
                    Case 4
                        Return New TVECTOR(_Vector4)
                    Case 5
                        Return New TVECTOR(_Vector5)
                    Case 6
                        Return New TVECTOR(_Vector6)
                    Case 7
                        Return New TVECTOR(_Vector7)
                End Select

            Else
                If Not HasVertices Then
                    Return TVECTOR.Zero
                Else

                    Return Vertices.ItemVector(aIndex, True)
                End If
            End If


        End Function

        Public Function VectorSet(aIndex As Integer, value As TVECTOR) As Boolean
            Dim v0 As TVECTOR
            If aIndex > 0 And aIndex <= DefPtCnt Then
                Dim targetvector As dxfVector = Nothing
                Select Case aIndex
                    Case 1
                        If _Vector1 Is Nothing Then _Vector1 = New dxfVector()
                        targetvector = _Vector1
                        'only for vector 1 !
                        _Plane.Origin = New TVECTOR(value)
                    Case 2
                        If _Vector2 Is Nothing Then _Vector2 = New dxfVector()
                        targetvector = _Vector2
                    Case 3
                        If _Vector3 Is Nothing Then _Vector3 = New dxfVector()
                        targetvector = _Vector3
                    Case 4
                        If _Vector4 Is Nothing Then _Vector4 = New dxfVector()
                        targetvector = _Vector4
                    Case 5
                        If _Vector5 Is Nothing Then _Vector5 = New dxfVector()
                        targetvector = _Vector5
                    Case 6
                        If _Vector6 Is Nothing Then _Vector6 = New dxfVector()
                        targetvector = _Vector6
                    Case 7
                        If _Vector7 Is Nothing Then _Vector7 = New dxfVector()
                        targetvector = _Vector7
                End Select

                If targetvector IsNot Nothing Then
                    If targetvector.X <> value.X Or targetvector.Y <> value.Y Or targetvector.Z <> value.Z Then IsDirty = True
                    targetvector.Strukture = value
                End If
            Else
                v0 = value
                If HasVertices Then
                    If aIndex < 1 Or aIndex > Vertices.Count Then Return False

                    v0 = Vertices.ItemVector(aIndex)
                    Vertices.SetItemVector(aIndex, value)
                    _Plane.Origin = Vertices.ItemVector(1, bSuppressIndexErr:=True)
                Else
                    _Plane.Origin = New TVECTOR(_Vector1)
                End If

            End If
            Return v0 <> value
        End Function

        Friend Sub VerticesSet(aVertices As IEnumerable(Of iVector))
            If aVertices Is Nothing Or Not HasVertices Then Return

            _Vertices = New colDXFVectors

            For Each vert As iVector In aVertices
                If vert Is Nothing Then Continue For
                _Vertices.Add(New TVERTEX(vert))
            Next
        End Sub


        Friend Sub SetVector(value As iVector, aIndex As Integer)
            VectorSet(aIndex, New TVECTOR(value))
        End Sub

        Friend Function GetVector(aIndex As Integer, Optional aRad As Double? = Nothing, Optional aRotation As Double? = Nothing) As dxfVector
            If (aIndex < 0 Or aIndex > DefPtCnt) Then Return Nothing
            Select Case aIndex
                Case 1
                    If _Vector1 Is Nothing Then _Vector1 = New dxfVector
                    If aRad.HasValue Then _Vector1.Radius = aRad.Value
                    If aRotation.HasValue Then _Vector1.Rotation = aRotation.Value
                    _Vector1.SetGUIDS(ImageGUID, EntityGUID, BlockGUID, SuppressEvents, 1)
                    Return _Vector1
                Case 2
                    If _Vector2 Is Nothing Then _Vector2 = New dxfVector
                    If aRad.HasValue Then _Vector2.Radius = aRad.Value
                    If aRotation.HasValue Then _Vector2.Rotation = aRotation.Value
                    _Vector2.SetGUIDS(ImageGUID, EntityGUID, BlockGUID, SuppressEvents, 2)
                    Return _Vector2
                Case 3
                    If _Vector3 Is Nothing Then _Vector3 = New dxfVector
                    If aRad.HasValue Then _Vector3.Radius = aRad.Value
                    If aRotation.HasValue Then _Vector3.Rotation = aRotation.Value
                    _Vector3.SetGUIDS(ImageGUID, EntityGUID, BlockGUID, SuppressEvents, 3)
                    Return _Vector3
                Case 4
                    If _Vector4 Is Nothing Then _Vector4 = New dxfVector
                    If aRad.HasValue Then _Vector4.Radius = aRad.Value
                    If aRotation.HasValue Then _Vector4.Rotation = aRotation.Value
                    _Vector4.SetGUIDS(ImageGUID, EntityGUID, BlockGUID, SuppressEvents, 4)
                    Return _Vector4
                Case 5
                    If _Vector5 Is Nothing Then _Vector5 = New dxfVector
                    If aRad.HasValue Then _Vector5.Radius = aRad.Value
                    If aRotation.HasValue Then _Vector5.Rotation = aRotation.Value
                    _Vector5.SetGUIDS(ImageGUID, EntityGUID, BlockGUID, SuppressEvents, 5)
                    Return _Vector5
                Case 6
                    If _Vector6 Is Nothing Then _Vector6 = New dxfVector
                    If aRad.HasValue Then _Vector6.Radius = aRad.Value
                    If aRotation.HasValue Then _Vector6.Rotation = aRotation.Value
                    _Vector6.SetGUIDS(ImageGUID, EntityGUID, BlockGUID, SuppressEvents, 6)
                    Return _Vector6
                Case 7
                    If _Vector7 Is Nothing Then _Vector7 = New dxfVector
                    If aRad.HasValue Then _Vector7.Radius = aRad.Value
                    If aRotation.HasValue Then _Vector7.Rotation = aRotation.Value
                    _Vector7.SetGUIDS(ImageGUID, EntityGUID, BlockGUID, SuppressEvents, 7)
                    Return _Vector7
                Case Else
                    Return Nothing
            End Select

        End Function

        Friend Sub ReleaseOwnerReference()
            _EntityGUID = ""
            OwnerPtr = Nothing
            If _Vector1 IsNot Nothing Then _Vector1.ReleaseOwnerReference()
            If _Vector2 IsNot Nothing Then _Vector2.ReleaseOwnerReference()
            If _Vector3 IsNot Nothing Then _Vector3.ReleaseOwnerReference()
            If _Vector4 IsNot Nothing Then _Vector4.ReleaseOwnerReference()
            If _Vector5 IsNot Nothing Then _Vector5.ReleaseOwnerReference()
            If _Vector6 IsNot Nothing Then _Vector6.ReleaseOwnerReference()
            If _Vector7 IsNot Nothing Then _Vector7.ReleaseOwnerReference()
        End Sub
        Friend Sub SetGUIDS(aImageGUID As String, aEntityGUID As String, aBlockGUID As String, bSuppressEvents As Boolean, Optional aEntity As dxfEntity = Nothing)
            ImageGUID = aImageGUID : EntityGUID = aEntityGUID : BlockGUID = aBlockGUID : SuppressEvents = bSuppressEvents
            If aEntity IsNot Nothing Then
                OwnerPtr = New WeakReference(aEntity)
                If _Vector1 IsNot Nothing Then _Vector1.OwnerPtr = New WeakReference(aEntity)
                If _Vector2 IsNot Nothing Then _Vector2.OwnerPtr = New WeakReference(aEntity)
                If _Vector3 IsNot Nothing Then _Vector3.OwnerPtr = New WeakReference(aEntity)
                If _Vector4 IsNot Nothing Then _Vector4.OwnerPtr = New WeakReference(aEntity)
                If _Vector5 IsNot Nothing Then _Vector5.OwnerPtr = New WeakReference(aEntity)
                If _Vector6 IsNot Nothing Then _Vector6.OwnerPtr = New WeakReference(aEntity)
                If _Vector7 IsNot Nothing Then _Vector7.OwnerPtr = New WeakReference(aEntity)
                GraphicType = aEntity.GraphicType

            End If
        End Sub
        Friend Function AlignToPlane(aPlane As dxfPlane) As Boolean
            If Not dxfPlane.IsNull(aPlane) Then Return AlignToPlaneV(aPlane.Strukture) Else Return AlignToPlaneV(New TPLANE(""))
        End Function
        Friend Function AlignToPlaneV(aPlane As TPLANE) As Boolean
            Dim _rVal As Boolean
            'On Error Resume Next
            'rotate the defining points so they lie on a plane coplanar aligned with the passed plane
            Dim newPL As New TPLANE("")
            Dim aN As dxfVector = Nothing
            Dim aAng As Double
            Dim oldPL As New TPLANE("")
            oldPL = Plane
            newPL = oldPL.AlignedTo(aPlane.ZDirection, dxxAxisDescriptors.Z, aN, aAng)
            If aAng <> 0 Then
                _rVal = True
                bTransforming = True
                Dim i As Long
                Dim v1 As dxfVector
                Dim aAxis As TVECTOR
                aAxis = aN.Strukture
                If DefPtCnt >= 1 Then
                    Vector1.Strukture = Vector1.Strukture.TransferedToPlane(oldPL, newPL, 1, 1, 1, 0)
                End If
                If DefPtCnt >= 2 Then
                    Vector2.Strukture = Vector2.Strukture.TransferedToPlane(oldPL, newPL, 1, 1, 1, 0)
                End If
                If DefPtCnt >= 3 Then
                    Vector3.Strukture = Vector3.Strukture.TransferedToPlane(oldPL, newPL, 1, 1, 1, 0)
                End If
                If DefPtCnt >= 4 Then
                    Vector4.Strukture = Vector4.Strukture.TransferedToPlane(oldPL, newPL, 1, 1, 1, 0)
                End If
                If DefPtCnt >= 5 Then
                    Vector5.Strukture = Vector5.Strukture.TransferedToPlane(oldPL, newPL, 1, 1, 1, 0)
                End If
                If DefPtCnt >= 6 Then
                    Vector6.Strukture = Vector6.Strukture.TransferedToPlane(oldPL, newPL, 1, 1, 1, 0)
                End If
                If DefPtCnt >= 7 Then
                    Vector7.Strukture = Vector7.Strukture.TransferedToPlane(oldPL, newPL, 1, 1, 1, 0)
                End If
                If HasVertices Then
                    For i = 1 To Vertices.Count
                        v1 = _Vertices.Item(i)
                        v1.Strukture = v1.Strukture.TransferedToPlane(oldPL, newPL, 1, 1, 1, 0)
                    Next i
                End If
                bTransforming = False
            End If
            _Plane = newPL
            Return _rVal
        End Function
        Friend Function Clone() As dxpDefPoints
            Return New dxpDefPoints(Me)
        End Function


        Friend Function XZPlane(Optional aOrigin As dxfVector = Nothing) As TPLANE
            Dim _rVal As TPLANE = _Plane.Clone
            Dim org As TVECTOR
            org = _Plane.Origin
            _rVal.Name = "XZ"
            If aOrigin IsNot Nothing Then org = aOrigin.Strukture
            _rVal.Define(org, _Plane.XDirection, _Plane.ZDirection)
            Return _rVal
        End Function
        Friend Function YZPlane(Optional aOrigin As dxfVector = Nothing) As TPLANE
            Dim _rVal As TPLANE = _Plane.Clone
            Dim org As TVECTOR
            org = _Plane.Origin
            _rVal.Name = "YZ"
            If aOrigin IsNot Nothing Then org = aOrigin.Strukture
            _rVal.Define(org, _Plane.YDirection, _Plane.ZDirection * -1)
            Return _rVal
        End Function

        Friend Sub Copy(aDefPts As TDEFPOINTS)
            If _DefPtCnt >= 1 Then VectorSet(1, aDefPts.DefPt1)
            If _DefPtCnt >= 2 Then VectorSet(1, aDefPts.DefPt2)
            If _DefPtCnt >= 3 Then VectorSet(1, aDefPts.DefPt3)
            If _DefPtCnt >= 4 Then VectorSet(1, aDefPts.DefPt4)
            If _DefPtCnt >= 5 Then VectorSet(1, aDefPts.DefPt5)
            If _DefPtCnt >= 6 Then VectorSet(1, aDefPts.DefPt6)
            If _DefPtCnt >= 7 Then VectorSet(1, aDefPts.DefPt7)
            If HasVertices Then Vertices.Populate(aDefPts.Verts)
        End Sub
        Friend Sub Copy(aDefPts As dxpDefPoints)
            If aDefPts Is Nothing Then Return
            If _DefPtCnt >= 1 Then SetVector(aDefPts._Vector1, 1)
            If _DefPtCnt >= 2 Then SetVector(aDefPts._Vector2, 2)
            If _DefPtCnt >= 3 Then SetVector(aDefPts._Vector3, 3)
            If _DefPtCnt >= 4 Then SetVector(aDefPts._Vector4, 4)
            If _DefPtCnt >= 5 Then SetVector(aDefPts._Vector5, 5)
            If _DefPtCnt >= 6 Then SetVector(aDefPts._Vector6, 6)
            If _DefPtCnt >= 7 Then SetVector(aDefPts._Vector7, 7)
            If HasVertices Then Vertices.Populate(aDefPts._Vertices, bAddClones:=True)
        End Sub
        Friend Sub Copy(aEntity As dxfEntity)
            If aEntity IsNot Nothing Then Copy(aEntity.DefPts)
        End Sub
#End Region 'Methods
#Region "_Vector_EventHandlers"
        Friend Sub RespondToDefPtChange(aEvent As dxfDefPtEvent, bDontDirty As Boolean)
            If aEvent Is Nothing AndAlso aEvent.Vertex Is Nothing Then Return
            If aEvent.EventType <> dxxVertexEventTypes.Position Then Return
            If aEvent.Vertex.DefPntIndex < 1 Or aEvent.Vertex.DefPntIndex > DefPtCnt Then Return
            Select Case aEvent.Vertex.DefPntIndex
                Case 1
                    If Not HasVertices Then
                        _Plane.Origin = _Vector1.Strukture
                    End If
                    VectorSet(1, New TVECTOR(aEvent.Vertex))
                Case 2
                    VectorSet(2, New TVECTOR(aEvent.Vertex))
                Case 3
                    VectorSet(3, New TVECTOR(aEvent.Vertex))
                Case 4
                    VectorSet(4, New TVECTOR(aEvent.Vertex))
                Case 5
                    VectorSet(5, New TVECTOR(aEvent.Vertex))
                Case 6
                    VectorSet(6, New TVECTOR(aEvent.Vertex))
                Case 7
                    VectorSet(6, New TVECTOR(aEvent.Vertex))
            End Select
            If Not bTransforming And Not bDontDirty Then
                IsDirty = True
            End If
        End Sub
#End Region '_Vector_EventHandlers
#Region "_Vertices_EventHandlers"
        Friend Sub RespondToVectorsChange(aEvent As dxfVectorsEvent)
            If aEvent Is Nothing Or Not HasVertices Then Return
            If Not bTransforming Then IsDirty = True
            If GraphicType = dxxGraphicTypes.Polyline Then _Plane.Origin = Vertices.ItemVector(1, True)
        End Sub
        Friend Sub RespondToVectorsMemberChange(aEvent As dxfVertexEvent)
            If aEvent Is Nothing Or Not HasVertices Then Return
            If Not bTransforming Then IsDirty = True
            If aEvent.Vertex IsNot Nothing Then
                _Plane.Origin = Vertices.ItemVector(1)
                If aEvent.Vertex.Index > 0 And aEvent.Vertex.Index <= Vertices.Count Then
                    Vertices.SetItemVertex(aEvent.Vertex.Index, aEvent.Vertex.VertexV)
                End If
            End If
        End Sub
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    If _Vector1 IsNot Nothing Then _Vector1.ReleaseOwnerReference()
                    If _Vector2 IsNot Nothing Then _Vector2.ReleaseOwnerReference()
                    If _Vector3 IsNot Nothing Then _Vector3.ReleaseOwnerReference()
                    If _Vector4 IsNot Nothing Then _Vector4.ReleaseOwnerReference()
                    If _Vector5 IsNot Nothing Then _Vector5.ReleaseOwnerReference()
                    If _Vector6 IsNot Nothing Then _Vector6.ReleaseOwnerReference()
                    If _Vector7 IsNot Nothing Then _Vector7.ReleaseOwnerReference()
                    ReleaseOwnerReference()
                    _Vector1 = Nothing
                    _Vector2 = Nothing
                    _Vector3 = Nothing
                    _Vector4 = Nothing
                    _Vector5 = Nothing
                    _Vector6 = Nothing
                    _Vector7 = Nothing
                    If _Vertices IsNot Nothing Then
                        _Vertices.ReleaseOwnerReference()
                        _Vertices.Dispose()
                    End If
                    _Vertices = Nothing
                    disposedValue = True
                End If
                disposedValue = True
            End If
        End Sub
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region '_Vertices_EventHandlers
    End Class  'dxpDefPoints
End Namespace
