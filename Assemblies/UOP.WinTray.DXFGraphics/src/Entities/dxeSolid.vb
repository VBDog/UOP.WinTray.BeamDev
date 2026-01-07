Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Public Class dxeSolid
        Inherits dxfEntity
        Implements iShape

#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxGraphicTypes.Solid)
        End Sub

        Public Sub New(aEntity As dxeSolid, Optional bCloneInstances As Boolean = False)
            MyBase.New(dxxGraphicTypes.Solid, aEntityToCopy:=aEntity, bCloneInstances:=bCloneInstances)
            If aEntity IsNot Nothing Then IsTrace = aEntity.IsTrace
        End Sub



        Friend Sub New(aSubEntity As TENTITY, Optional bNewGUID As Boolean = False)
            MyBase.New(aSubEntity, bNewGUID:=bNewGUID)
        End Sub

        Friend Sub New(aObject As TOBJECT)
            MyBase.New(dxxGraphicTypes.Solid)
            DefineByObject(aObject)
        End Sub


        Friend Sub New(aVertices As TVECTORS, Optional aPlane As dxfPlane = Nothing)
            MyBase.New(dxxGraphicTypes.Solid)
            If Not dxfPlane.IsNull(aPlane) Then Plane = aPlane
            VerticesV = aVertices
        End Sub
        Public Sub New(aVertices As colDXFVectors, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aPlane As dxfPlane = Nothing)
            MyBase.New(dxxGraphicTypes.Solid)
            If Not dxfPlane.IsNull(aPlane) Then PlaneV = New TPLANE(aPlane)
            If aVertices IsNot Nothing Then
                DefPts.VectorSet(1, aVertices.ItemVector(1, True))
                DefPts.VectorSet(2, aVertices.ItemVector(2, True))
                DefPts.VectorSet(3, aVertices.ItemVector(3, True))
                If aVertices.Count >= 4 Then
                    DefPts.VectorSet(4, aVertices.ItemVector(4))
                    Triangular = False
                End If
            End If
            If aDisplaySettings IsNot Nothing Then DisplayStructure = aDisplaySettings.Strukture
        End Sub
        Friend Sub New(aVertices As TVECTORS, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aPlane As dxfPlane = Nothing)
            MyBase.New(dxxGraphicTypes.Solid)
            If Not dxfPlane.IsNull(aPlane) Then PlaneV = New TPLANE(aPlane)

            DefPts.VectorSet(1, aVertices.Item(1, True))
            DefPts.VectorSet(2, aVertices.Item(2, True))
            DefPts.VectorSet(3, aVertices.Item(3, True))
            If aVertices.Count >= 4 Then
                DefPts.VectorSet(4, aVertices.Item(4))
                Triangular = False
            End If
            If aDisplaySettings IsNot Nothing Then DisplayStructure = aDisplaySettings.Strukture
        End Sub
        Public Sub New(Optional aVertex1 As iVector = Nothing, Optional aVertex2 As iVector = Nothing, Optional aVertex3 As iVector = Nothing, Optional aVertex4 As iVector = Nothing, Optional aPlane As dxfPlane = Nothing, Optional aDisplaySettings As dxfDisplaySettings = Nothing)
            MyBase.New(dxxGraphicTypes.Solid)
            If Not dxfPlane.IsNull(aPlane) Then PlaneV = New TPLANE(aPlane)
            If aVertex1 IsNot Nothing Then Vertex1V = New TVECTOR(aVertex1)
            If aVertex2 IsNot Nothing Then Vertex2V = New TVECTOR(aVertex2)
            If aVertex3 IsNot Nothing Then Vertex3V = New TVECTOR(aVertex3)
            If aVertex4 IsNot Nothing Then Vertex4V = New TVECTOR(aVertex4)
            If aDisplaySettings IsNot Nothing Then DisplayStructure = aDisplaySettings.Strukture
        End Sub

#End Region 'Constructors
#Region "Properties"

        Public Overrides ReadOnly Property EntityType As dxxEntityTypes
            Get
                If IsTrace Then Return dxxEntityTypes.Trace Else Return dxxEntityTypes.Solid
            End Get
        End Property
        Public Property Filled As Boolean
            Get
                Return PropValueB("*Filled")
            End Get
            Set(value As Boolean)
                SetPropVal("*Filled", value, True)
            End Set
        End Property
        Public Property Triangular As Boolean
            Get
                If EntityType = dxxEntityTypes.Trace Then SetPropVal("*Triangular", False, True)
                Return PropValueB("*Triangular")
            End Get
            Set(value As Boolean)
                SetPropVal("*Triangular", value, True)
            End Set
        End Property
        Public Property Vertex1 As dxfVector
            Get
                'the first vertex of the path
                Return DefPts.Vector1
            End Get
            Set(value As dxfVector)
                'the first vertex of the path
                MyBase.DefPts.SetVector(value, 1)
            End Set
        End Property
        Friend Property Vertex1V As TVECTOR
            Get
                '^the vector structure of the first vertex
                Return DefPts.VectorGet(1)
            End Get
            Set(value As TVECTOR)
                '^the vector structure of the first vertex
                DefPts.VectorSet(1, value)
            End Set
        End Property

        Public Property IsTrace As Boolean

        Public Property Vertex2 As dxfVector
            Get
                'the second vertex of the path
                Return DefPts.Vector2
            End Get
            Set(value As dxfVector)
                'the second vertex of the path
                DefPts.Vector2 = value
            End Set
        End Property
        Friend Property Vertex2V As TVECTOR
            Get
                '^the vector structure of the second vertex
                Return DefPts.VectorGet(2)
            End Get
            Set(value As TVECTOR)
                '^the vector structure of the second vertex
                DefPts.VectorSet(2, value)
            End Set
        End Property
        Public Property Vertex3 As dxfVector
            Get
                'the third vertex of the path
                Return DefPts.Vector3
            End Get
            Set(value As dxfVector)
                'the third vertex of the path
                DefPts.Vector3 = value
            End Set
        End Property
        Friend Property Vertex3V As TVECTOR
            Get
                '^the vector structure of the third vertex
                Return DefPts.VectorGet(3)
            End Get
            Set(value As TVECTOR)
                '^the vector structure of the third vertex
                DefPts.VectorSet(3, value)
            End Set
        End Property
        Public Property Vertex4 As dxfVector
            Get
                'the fourth vertex of the path
                If Triangular Then
                    Return DefPts.Vector3.Clone
                Else
                    Return DefPts.Vector4
                End If
            End Get
            Set(value As dxfVector)
                'the forth vertex of the path
                DefPts.Vector4 = value
            End Set
        End Property
        Friend Property Vertex4V As TVECTOR
            Get
                '^the vector structure of the fourth vertex
                Return DefPts.VectorGet(4)
            End Get
            Set(value As TVECTOR)
                '^the vector structure of the fourth vertex
                DefPts.VectorSet(4, value)
            End Set
        End Property
        Public ReadOnly Property VertexCount As Integer
            Get
                If Triangular Then Return 3 Else Return 4
            End Get
        End Property
        Public Overrides Property Vertices As colDXFVectors
            Get
                '^collection of points that define the boundary of the Solid
                If Not Triangular Then Return New colDXFVectors(Vertex1, Vertex2, Vertex3, Vertex4) Else Return New colDXFVectors(Vertex1, Vertex2, Vertex3)
            End Get
            Friend Set(value As colDXFVectors)
                '^collection of points that define the boundary of the Solid
                If value Is Nothing Then Return
                Dim wuz As Boolean
                wuz = SuppressEvents
                SuppressEvents = True
                Triangular = value.Count <= 3
                Vertex1V = value.ItemVector(1, True)
                Vertex2V = value.ItemVector(2, True)
                Vertex3V = value.ItemVector(3, True)
                Vertex4V = value.ItemVector(4, True)
                If Triangular Then Vertex4V = Vertex3V
                SuppressEvents = wuz
            End Set
        End Property
        Friend Property VerticesV As TVECTORS
            Get
                '^collection of points that define the boundary of the Solid
                If Not Triangular Then Return New TVECTORS(Vertex1V, Vertex2V, Vertex3V, Vertex4V) Else Return New TVECTORS(Vertex1V, Vertex2V, Vertex3V)
            End Get
            Set(value As TVECTORS)
                '^collection of points that define the boundary of the Solid

                Dim wuz As Boolean
                wuz = SuppressEvents
                SuppressEvents = True
                Triangular = value.Count <= 3
                Vertex1V = value.Item(1)
                Vertex2V = value.Item(2)
                Vertex3V = value.Item(3)
                If Not Triangular Then Vertex4V = value.Item(4) Else Vertex4V = Vertex3V
                SuppressEvents = wuz
            End Set
        End Property

        Private Property iShape_Vertices As IEnumerable(Of iVector) Implements iShape.Vertices
            Get
                Return Vertices
            End Get
            Set(value As IEnumerable(Of iVector))
                If value IsNot Nothing Then Vertices = New colDXFVectors(value)

            End Set
        End Property

        Public Overrides Property Plane As dxfPlane Implements iShape.Plane
            Get
                Return MyBase.Plane
            End Get
            Set(value As dxfPlane)
                MyBase.Plane = value
            End Set
        End Property

        Public Property Closed As Boolean Implements iShape.Closed
            Get
                Return True
            End Get
            Set(value As Boolean)
                'Throw New NotImplementedException()
            End Set
        End Property
#End Region 'Properties
#Region "MustOverride Entity Methods"

        Friend Overrides Sub DefineByObject(aObj As TOBJECT, Optional bNoHandles As Boolean = False, Optional aStyle As dxfTableEntry = Nothing, Optional aBlock As dxfBlock = Nothing)
            Reactors.Append(aObj.Reactors, bClear:=True)
            ExtendedData.Append(aObj.ExtendedData, bClear:=True)
            Dim aPlane As TPLANE = TPLANE.ArbitraryCS(aObj.Properties.GCValueV(210, TVECTOR.WorldZ))
            Dim v4 As TVECTOR
            Dim v3 As TVECTOR
            Dim idx As Integer
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            DisplayStructure = aObj.DisplayVars

            v1 = aObj.Properties.GCValueV(10)
            v2 = aObj.Properties.GCValueV(11)
            v4 = aObj.Properties.GCValueV(13, aDefault:=v3, 1, "", idx)
            v3 = aObj.Properties.GCValueV(12)
            Vertex1V = New TVECTOR(aPlane, v1.X, v1.Y, v1.Z)
            Vertex2V = New TVECTOR(aPlane, v2.X, v2.Y, v1.Z)
            Vertex4V = New TVECTOR(aPlane, v4.X, v4.Y, v1.Z)
            If String.Compare(aObj.Properties.GCValueStr(0, "SOLID"), "TRACE", True) = 0 Then
                Vertex3V = New TVECTOR(aPlane, v3.X, v3.Y)
            Else
                If idx = 0 Then v3 = v4
                Vertex3V = New TVECTOR(aPlane, v3.X, v3.Y, v1.Z)
                Vertex4V = New TVECTOR(aPlane, v4.X, v4.Y, v1.Z)
                Triangular = v3.Equals(v4, 4)
            End If
            aPlane.Origin = v1
            PlaneV = aPlane
            If Not bNoHandles Then Handle = aObj.Properties.GCValueStr(5)
        End Sub

        ''' <summary>
        ''' returns a clone of the entity transfered to the passed plane
        ''' </summary>
        ''' <remarks>the entities defining vectors are converted to vectors with respect to the entities OCS plane and then redefines them based on the passed plane </remarks>
        ''' <param name="aPlane"></param>
        '''<param name="aFromPlane">the plane to define the definition points with respect to prior to transfer to the new plane. If not passed the entities OCS is used </param>
        ''' <returns></returns>
        Public Overloads Function TransferedToPlane(aPlane As dxfPlane,optional aFromPlane as dxfPlane = Nothing) As dxeSolid
            Dim _rVal As dxeSolid = Me.Clone()
            _rVal.TransferToPlane(aPlane, aFromPlane)
            Return _rVal
        End Function

        ''' <summary>
        ''' Returns a new object with properties matching those of the cloned object
        ''' </summary>
        ''' <returns></returns>
        Public Overloads Function Clone() As dxeSolid
            Return New dxeSolid(Me)
        End Function
#End Region 'MustOverride Entity Methods
#Region "Methods"
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix(GroupName, ExtendedData)
            'On Error Resume Next
            rBlock = Nothing
            If Not GetImage(aImage) Then Return _rVal
            If bUpdatePath Or IsDirty Then UpdatePath(False, aImage)
            Dim i As Integer
            Dim iCnt As Integer
            Dim aOCS As TPLANE = TPLANE.World
            Dim tname As String = String.Empty
            If aInstances Is Nothing Then aInstances = Instances
            If bSuppressInstances Then iCnt = 1 Else iCnt = aInstances.Count + 1
            For i = 1 To iCnt
                If aInstance <= 0 Or i = aInstance Then _rVal.Add(DXFProps(aInstances, i, aOCS, tname))
            Next i
            If iCnt > 1 Then
                _rVal.Name = tname & "-" & iCnt & " INSTANCES"
            End If
            Return _rVal
        End Function
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As TPROPERTYARRAY
            '#1the instance to create the properties for
            '#2the OCS plane of the entity (created for first instance)
            '^returns the entities properties updated for output to a DXF file
            'On Error Resume Next
            If Not Filled Then
                Dim aPln As dxePolyline
                aPln = Perimeter()
                _rVal = aPln.DXFProps(aInstances, aInstance, aOCS, rTypeName)
            Else
                aInstance = Math.Abs(aInstance)
                If aInstance <= 0 Then aInstance = 1
                _rVal = New TPROPERTYARRAY(GUID & "-" & aInstance.ToString, aInstance)
                Dim myProps As New TPROPERTIES(Properties)
                Dim aTrs As New TTRANSFORMS
                Dim aPl As TPLANE = PlaneV
                Dim scl As Double = 1
                Dim ang As Double = 0
                Dim bInv As Boolean
                Dim bLft As Boolean
                If aInstance > 1 Then
                    If aInstances Is Nothing Then aInstances = Instances
                    aTrs = aInstances.Transformations(aInstance - 1, scl, ang, bInv, bLft)
                    TTRANSFORMS.Apply(aTrs, aPl)
                    rTypeName = myProps.Item(1).Value
                    aOCS = TPLANE.ArbitraryCS(aPl.ZDirection)
                Else
                    myProps.Handle = Handle
                    aOCS = TPLANE.ArbitraryCS(aPl.ZDirection)
                    myProps.SetVectorGC(210, aOCS.ZDirection, bSuppress:=Math.Round(aOCS.ZDirection.Z, 6) = 1)
                    If EntityType = dxxEntityTypes.Trace Then
                        rTypeName = "TRACE"
                    Else
                        rTypeName = "SOLID"
                    End If
                    SetProps(myProps)
                    UpdateCommonProperties(rTypeName)
                    myProps = New TPROPERTIES(Properties)
                    myProps.SetVal("Entity Type", rTypeName)
                End If
                Dim ePts As TVECTORS
                ePts = Vectors(bForFile:=True)
                If aInstance > 1 Then TTRANSFORMS.Apply(aTrs, ePts)
                myProps.SetVal("Entity Type", rTypeName)
                myProps.SetVectorGC(10, ePts.Item(1).WithRespectTo(aOCS))
                myProps.SetVectorGC(11, ePts.Item(2).WithRespectTo(aOCS))
                myProps.SetVectorGC(12, ePts.Item(3).WithRespectTo(aOCS))
                myProps.SetVectorGC(13, ePts.Item(4).WithRespectTo(aOCS))
                _rVal.Add(myProps, rTypeName, True, True)
            End If
            Return _rVal
        End Function
        Public Function Perimeter(Optional aLayerName As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "") As dxePolyline
            Dim _rVal As New dxePolyline With {
                .ImageGUID = ImageGUID,
                .Closed = True,
                .PlaneV = PlaneV,
                .Instances = Instances,
                .DisplayStructure = DisplayStructure
                }
            If Triangular Then
                _rVal.Vertices.Add(Vertex1, bAddClone:=True)
                _rVal.Vertices.Add(Vertex2, bAddClone:=True)
                _rVal.Vertices.Add(Vertex3, bAddClone:=True)
            Else
                _rVal.Vertices.Add(Vertex1, bAddClone:=True)
                _rVal.Vertices.Add(Vertex2, bAddClone:=True)
                _rVal.Vertices.Add(Vertex3, bAddClone:=True)
                _rVal.Vertices.Add(Vertex4, bAddClone:=True)
            End If
            _rVal.LCLSet(aLayerName, aColor, aLineType)
            Return _rVal
        End Function
        Friend Sub Planarize()
            'On Error Resume Next
            Dim aPl As TPLANE = PlaneV
            Vertex1V = Vertex1V.ProjectedTo(aPl)
            Vertex2V = Vertex2V.ProjectedTo(aPl)
            Vertex3V = Vertex3V.ProjectedTo(aPl)
            Vertex4V = Vertex4V.ProjectedTo(aPl)
        End Sub
        Public Function Rotated(aAngle As Double, Optional aXChange As Double = 0.0, Optional aYChange As Double = 0.0, Optional aZChange As Double = 0.0, Optional aAxis As Object = Nothing) As dxeSolid
            Dim _rVal As dxeSolid = Clone()
            '#1the angle to rotate
            '#2an optional X displacement to add after the rotation
            '#3an optional Y displacement to add after the rotation
            '#4an optional Z displacement to add after the rotation
            '#5an optional axis for the rotation
            '^returns a copy of the entity rotated and/or moved
            If aAngle <> 0 Then
                If aAxis IsNot Nothing Then
                    _rVal.RotateAbout(aAxis, aAngle)
                Else
                    _rVal.Rotate(aAngle)
                End If
            End If
            If aXChange <> 0 Or aYChange <> 0 Or aZChange <> 0 Then
                _rVal.Move(aXChange, aYChange, aZChange)
            End If
            Return _rVal
        End Function
        Friend Function Vectors(Optional bForFile As Boolean = False) As TVECTORS
            Dim _rVal As New TVECTORS
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim v3 As TVECTOR
            Dim v4 As TVECTOR
            v1 = Vertex1V
            v2 = Vertex2V
            If bForFile Then
                If Triangular Then
                    v3 = Vertex3V
                    v4 = v3
                Else
                    v3 = Vertex4V
                    v4 = Vertex3V
                End If
                _rVal.Add(v1)
                _rVal.Add(v2)
                _rVal.Add(v3)
                _rVal.Add(v4)
            Else
                v3 = Vertex3V
                v4 = Vertex4V
                If Triangular Then
                    _rVal.Add(v1, TVALUES.ToByte(dxxVertexStyles.MOVETO))
                    _rVal.Add(v2, TVALUES.ToByte(dxxVertexStyles.LINETO))
                    _rVal.Add(v3, TVALUES.ToByte(dxxVertexStyles.LINETO))
                    _rVal.Add(v1, TVALUES.ToByte(dxxVertexStyles.LINETO))
                Else
                    _rVal.Add(v1, TVALUES.ToByte(dxxVertexStyles.MOVETO))
                    _rVal.Add(v2, TVALUES.ToByte(dxxVertexStyles.LINETO))
                    _rVal.Add(v3, TVALUES.ToByte(dxxVertexStyles.LINETO))
                    _rVal.Add(v4, TVALUES.ToByte(dxxVertexStyles.LINETO))
                    _rVal.Add(v1, TVALUES.ToByte(dxxVertexStyles.LINETO))
                End If
            End If
            Return _rVal
        End Function
        Public Function Vertex(aIndex As Integer) As dxfVector
            Dim _rVal As dxfVector = Nothing
            '#1 a value from 1 to 4
            '^returns the desired vertex
            _rVal = Vertices.Item(aIndex)
            Return _rVal
        End Function
        Public Function VertexCoordinatesGet(Optional aPlane As dxfPlane = Nothing, Optional aDelimiter As String = "¸") As String
            Dim _rVal As String = String.Empty
            '^a concantonated string of all the vertex coordinates of the polyline
            '~vertex coordinates are the 2D coordinates of the vector with respect to the passed plane augmented with the vertex radius of the vector
            '~ie "(X,Y,VertexRadius)" where the vertex radius indicates the vector is the start of an arc.
            '~the delimitor is "¸" (char 184)
            If TPLANE.IsNull(aPlane) Then
                _rVal = Vertices.VertexCoordinatesGet(Plane, aDelimiter, True)
            Else
                _rVal = Vertices.VertexCoordinatesGet(aPlane, aDelimiter, True)
            End If
            Return _rVal
        End Function
        Public Sub VertexCoordinatesSet(aCoordinates As String, Optional aPlane As dxfPlane = Nothing, Optional aDelimiter As String = "¸")
            '^a concantonated string of all the vertex coordinates of the polyline
            '~vertex coordinates are the 2D coordinates of the vector with respect to the passed plane augmented with the vertex radius of the vector
            '~ie "(X,Y,VertexRadius)" where the vertex radius indicates the vector is the start of an arc.
            '~the delimitor is "¸" (char 184)
            Dim averts As New colDXFVectors
            If TPLANE.IsNull(aPlane) Then
                averts.VertexCoordinatesSet(aCoordinates, Plane, aDelimiter:=aDelimiter, aMaxAdd:=4)
            Else
                averts.VertexCoordinatesSet(aCoordinates, aPlane, aDelimiter:=aDelimiter, aMaxAdd:=4)
            End If
            If averts.Count >= 4 Then Triangular = False
            If averts.Count >= 1 Then Vertex1V = averts.ItemVector(1)
            If averts.Count >= 2 Then Vertex2V = averts.ItemVector(2)
            If averts.Count >= 3 Then Vertex3V = averts.ItemVector(3)
            If Triangular Then
                Vertex4V = Vertex3V
            Else
                Vertex4V = averts.ItemVector(4, True)
            End If
            Vertices = averts
        End Sub

#End Region 'Methods
    End Class 'dxeSolid
End Namespace
