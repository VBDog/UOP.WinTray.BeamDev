
Imports UOP.DXFGraphics.Utilities

Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics

    Public MustInherit Class dxfPolyline
        Inherits dxfEntity
        Implements iShape

        Public Sub New(aPtype As dxxPolylineTypes, Optional aEntityToCopy As dxfPolyline = Nothing, Optional bCloneInstances As Boolean = False)
            MyBase.New(PolyLineTypeToGraphicType(aPtype), aEntityToCopy:=aEntityToCopy, bCloneInstances:=bCloneInstances)
            _PolylineType = aPtype
        End Sub

        Public Sub New(aPtype As dxxPolylineTypes, aDisplaySettings As dxfDisplaySettings)
            MyBase.New(PolyLineTypeToGraphicType(aPtype), aDisplaySettings)
            _PolylineType = aPtype
        End Sub

        Public Sub New(aPtype As dxxPolylineTypes, aDisplaySettings As dxfDisplaySettings, aVertices As IEnumerable(Of iVector))
            MyBase.New(PolyLineTypeToGraphicType(aPtype), aDisplaySettings, aVertices:=aVertices)
            _PolylineType = aPtype
        End Sub
        Friend Sub New(aPtype As dxxPolylineTypes, aSubEntity As TENTITY, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional bNewGUID As Boolean = False)

            MyBase.New(aSubEntity, aDisplaySettings, bNewGUID)
            _PolylineType = aPtype
        End Sub

        Private _PolylineType As dxxPolylineTypes
        Public ReadOnly Property PolylineType As dxxPolylineTypes
            Get
                Return _PolylineType
            End Get
        End Property
        Public Property Closed As Boolean
            Get
                Closed = PropValueB("*Closed")
            End Get
            Set(value As Boolean)
                SetPropVal("*Closed", value, True)
            End Set
        End Property
        Public Property Segments As colDXFEntities
            Get
                Return New colDXFEntities(PathSegments)
            End Get
            Friend Set(value As colDXFEntities)
                Dim wuz As Boolean = SuppressEvents
                SuppressEvents = True
                If value Is Nothing Then
                    Vertices.Clear()
                    Closed = False
                Else
                    Vertices = value.PolylineVertices()
                End If
                SuppressEvents = wuz
                IsDirty = True
            End Set
        End Property


        Public Property SegmentWidth As Double
            Get
                '^the segment width of the polyline
                Return PropValueD("Constant Width")
            End Get
            Set(value As Double)
                '^the segment width of the polyline
                If value < 0 Then value = -1 Else value = Math.Round(value, 6)
                SetPropVal("Constant Width", value, True)
            End Set
        End Property

        Public Property InsertionPt As dxfVector
            Get
                Return HandlePt
            End Get
            Set(value As dxfVector)
                If PolylineType = dxxPolylineTypes.Polyline Then Return
                MyBase.DefPts.SetVector(value, 1)
            End Set
        End Property

        Friend Property InsertionPtV As TVECTOR
            '^the point where the entity was inserted
            Get
                Return DefPts.VectorGet(1)
            End Get
            Set(value As TVECTOR)
                DefPts.VectorSet(1, value)
            End Set
        End Property





        Public Overridable Property AdditionalSegments As colDXFEntities
            Get

                '^a collection of lines and arcs related to this polyline 
                If _PolylineType <> dxxPolylineTypes.Polygon Then Return New colDXFEntities
                Return AddSegs
            End Get
            Set(value As colDXFEntities)
                If _PolylineType <> dxxPolylineTypes.Polygon Then Return
                Dim asegs As colDXFEntities = AddSegs
                If value IsNot Nothing Then
                    asegs.Populate(value, bAddClones:=True)
                Else
                    asegs.Clear()
                End If
                AddSegs = asegs
            End Set
        End Property
        Public Shadows Property Vertices As colDXFVectors
            Get
                Return MyBase.Vertices
            End Get
            Set(value As colDXFVectors)
                MyBase.Vertices = value
            End Set
        End Property

        Friend Property VerticesV As TVERTICES
            Get
                Return DefPts.Vertexes
            End Get
            Set(value As TVERTICES)
                DefPts.Vertexes = value
            End Set
        End Property

        Public ReadOnly Property VertexCount As Integer
            Get
                Return Vertices.Count
            End Get
        End Property



        Public ReadOnly Property ExtendedVertices As colDXFVectors
            Get
                '^a collection of points (phantoms only) which lie on the Polygon perimeter
                '~used internally to compute area.  places additional vertices along arcs to approximate them as lines
                Return Segments.PhantomPoints(20, 1, True)

            End Get
        End Property
        Public ReadOnly Property HasArcSegments As Boolean
            Get
                '^True if the Polyline has any arc segments defined
                UpdatePath()
                Return Components.Segments.ArcCount > 0
            End Get
        End Property
        Friend Property PlineGen As Boolean
            Get
                Return PropValueB("*PlineGen")
            End Get
            Set(value As Boolean)
                SetPropVal("*PlineGen", value, False)
            End Set
        End Property
        Friend Property VectorsV As TVECTORS
            Get
                Return DefPts.Vectors
            End Get
            Set(value As TVECTORS)
                DefPts.Vectors = value
            End Set
        End Property

        Public ReadOnly Property IsSquare As Boolean
            Get
                Dim _rVal As Boolean = False
                IsRectangular(4, False, _rVal)
                Return _rVal
            End Get
        End Property
        Public ReadOnly Property IsTriangular As Boolean
            Get
                '^True if the Polyline is actually a Triangle
                '~returns True if there are three unequal vertices and no arc segments
                If Vertices.Count <> 2 And Vertices.Count <> 3 Then Return False
                Dim Segs As colDXFEntities
                Segs = Segments()
                If Segs.Count <> 3 Then Return False
                If HasArcSegments > 0 Then Return False
                If Segs.GetByLength(0).Count > 0 Then Return False
                If Not Segs.LastMember.DefinitionPoint(dxxEntDefPointTypes.EndPt).Strukture.Equals(Segs.StartPt.Strukture, 3) Then Return False
                Return True
            End Get
        End Property
        Public ReadOnly Property MaxX As Double
            Get
                '^the maximum X coordinate of the ExtentedVertices collection
                Return ExtendedVertices.GetOrdinate(dxxOrdinateTypes.MaxX)
            End Get
        End Property
        Public ReadOnly Property MaxY As Double
            Get
                '^the maximum Y coordinate of the ExtentedVertices collection
                Return ExtendedVertices.GetOrdinate(dxxOrdinateTypes.MaxY)
            End Get
        End Property
        Public ReadOnly Property MinX As Double
            Get
                '^the minimum X coordinate of the ExtentedVertices collection
                Return ExtendedVertices.GetOrdinate(dxxOrdinateTypes.MinX)
            End Get
        End Property
        Public ReadOnly Property MinY As Double
            Get
                '^the minimum Y coordinate of the ExtentedVertices collection
                Return ExtendedVertices.GetOrdinate(dxxOrdinateTypes.MinY)
            End Get
        End Property

        Public ReadOnly Property FirstRadius As Double
            Get
                '^the radius of the first dxfBginArc vertex found in the Vertices collection
                Dim v1 As dxfVector = Vertices.Find(Function(x) x.Radius <> 0)
                If v1 Is Nothing Then Return 0 Else Return v1.Radius
            End Get
        End Property
        Friend Property ThreeD As Boolean
            Get
                Return PropValueB("*3D")
            End Get
            Set(value As Boolean)
                SetPropVal("*3D", value, True)
            End Set
        End Property

        Private Property iShape_Vertices As IEnumerable(Of iVector) Implements iShape.Vertices
            Get
                Return Vertices
            End Get
            Set(value As IEnumerable(Of iVector))
                Throw New NotImplementedException()
            End Set
        End Property

        Private Property iShape_Plane As dxfPlane Implements iShape.Plane
            Get
                Return Plane
            End Get
            Set(value As dxfPlane)
                Plane = value
            End Set
        End Property

        Private Property iShape_Closed As Boolean Implements iShape.Closed
            Get
                Return Closed
            End Get
            Set(value As Boolean)
                Closed = value
            End Set
        End Property

        Public Function WellFormedArea() As Double

            '^returns the area of the polygon assuming none of any bounding arcs are concave to the polygon
            '^and none of the bounding segments intersect each other

            Dim tot As Double
            Dim verts As colDXFVectors = Vertices

            Dim v1 As dxfVector
            Dim sumation As Double

            Dim v2 As dxfVector
            Dim aSeg As dxeArc
            'On Error Resume Next

            Dim Segs As colDXFEntities = Segments.GetArcs

            For i As Integer = 1 To verts.Count
                v1 = verts.Item(i)
                If i < verts.Count Then
                    v2 = verts.Item(i + 1)
                Else
                    v2 = verts.Item(1)
                End If
                sumation += (v1.X * v2.Y - v2.X * v1.Y)
            Next i
            tot = Math.Abs(0.5 * sumation)
            For i = 1 To Segs.Count
                aSeg = Segs.Item(i)
                tot += aSeg.ChordArea
            Next i
            Return tot

        End Function

        Public Function GetVertex(aTag As String, Optional aFlag As String = Nothing, Optional bReturnClone As Boolean = False, Optional bIgnoreCase As Boolean = True) As dxfVector
            '#1the tag to search for
            '#2the flag to searchf for
            '#3flag to return a clone
            '^returns a point from the polygons vertices collection whose properties or position in the collection match the passed flag and tag combo
            Return Vertices.GetTagged(aTag, aFlag, bReturnClone:=bReturnClone, bIgnoreCase:=bIgnoreCase)
        End Function
        Public Function GetVertex(aControlFlag As dxxPointFilters, Optional aOrdinate As Double = 0.0, Optional bReturnClone As Boolean = False, Optional aPrecis As Integer = 3) As dxfVector

            '#1flag indicating what type of point to search for
            '#2the ordinate to search for if the search is ordinate relative
            '^returns a point from the polygons vertices collection whose properties or position in the collection match the passed control flag
            Return Vertices.GetVector(aControlFlag, aOrdinate, aPlane:=Plane, bReturnClone:=bReturnClone, aPrecis:=aPrecis)

        End Function
        Public Sub Orthoganolize(Optional MergeDistance As Double = 0.001)
            '^aligns the vertices
            If Vertices.Orthoganolize(MergeDistance, True, True, True) Then IsDirty = False
        End Sub
        Public Function ArcSegments(Optional aRadius As Double = 0.0, Optional aPrecis As Integer = 4) As List(Of dxeArc)
            Dim _rVal As New List(Of dxeArc)
            Dim arcs As colDXFEntities = Segments.GetArcs(aRadius, aPrecis)
            For Each ent As dxfEntity In arcs
                _rVal.Add(DirectCast(ent, dxeArc))
            Next
            Return _rVal
        End Function
        Public Function LineSegments(Optional aLength As Double = 0.0, Optional aPrecis As Integer = 4) As List(Of dxeLine)
            Return Segments.GetLines(aLength, aPrecis)
        End Function

        Public Function ArcVertex(aRadius As Double, Optional aOccurance As Integer = 1, Optional aPrecis As Integer = 3, Optional aReturnClone As Boolean = False) As dxfVector
            '#1the radius to search for
            '#2the instance to return
            '#3the precision for the comparison
            '#4flag to return a copy of the matching vector
            '#5returns the collection index of the matching vector
            '^returns the first vector in the collection whose radius property match the passed radius
            Return Vertices.ArcVertex(aRadius, aOccurance, aPrecis, aReturnClone)
        End Function
        Friend Function AddV(aVector As TVECTOR, Optional aBeforeIndex As Long = 0, Optional aAfterIndex As Long = 0) As dxfVector
            Return Vertices.AddV(aVector, aBeforeIndex:=aBeforeIndex, aAfterIndex:=aAfterIndex)
        End Function
        Public Function AddVertex(Optional aX As Double = 0, Optional aY As Double = 0, Optional aZ As Double = 0, Optional aVertexRadius As Double? = Nothing, Optional bClockwise As Boolean? = Nothing, Optional aStartWidth As Double? = Nothing, Optional aEndWidth As Double? = Nothing, Optional aTag As String = "", Optional aFlag As String = "", Optional aBeforeIndex As Long = 0, Optional aAfterIndex As Long = 0) As dxfVector
            '#1the X coordinate of the new vertex
            '#2the Y coordinate of the new vertex
            '#3the Z coordinate of the new vertex
            '#4the radius of the new vertex (only applies to BeginRadius vertex types)
            '#5flag to invert the arc created by a arc vertex
            '^a shorthand way to add a vertex without all the code to create and add one conventionally
            Return Vertices.Add(aX, aY, aZ, aVertexRadius, bClockwise, aStartWidth, aEndWidth, aTag:=aTag, aFlag:=aFlag, aBeforeIndex:=aBeforeIndex, aAfterIndex:=aAfterIndex)
        End Function
        Public Shadows Function Width(Optional aPlane As dxfPlane = Nothing) As Double
            Return BoundingRectangle(aPlane).Width

        End Function
        Public Shadows Function Height(Optional aPlane As dxfPlane = Nothing) As Double
            Return BoundingRectangle(aPlane).Height
        End Function

        Public Shared Function PolyLineTypeToGraphicType(aPType As dxxPolylineTypes) As dxxGraphicTypes
            If aPType = dxxPolylineTypes.Polygon Then
                Return dxxGraphicTypes.Polygon
            Else
                Return dxxGraphicTypes.Polyline
            End If
        End Function
        Public Function FirstVertex(Optional bGetClone As Boolean = False) As dxfVector

            '^returns the last vertex
            Return Vertices.FirstVector(bGetClone)

        End Function

        Public Function LastVertex(Optional bGetClone As Boolean = False) As dxfVector

            '^returns the last vertex
            Return Vertices.LastVector(bGetClone)

        End Function
        Public Function EncompassingRadius() As Double

            '^a radius that defines a circle centered at the polygons center of area that completely encircles the polygons vertices
            '~used for testing gross proximity to the polygon. If a point is not in the circle defined by this radius then the point cannot
            '~be inside the polygon.
            'On Error Resume Next
            Return ExtendedVertices.BoundingCircle(Plane).Radius



        End Function

        Function Center(Optional aPlane As dxfPlane = Nothing) As dxfVector
            Return BoundingRectangle().Center.WithRespectToPlane(aPlane)
        End Function

        Public Function EnclosesPoint(aTestPoint As iVector, Optional aOnBoundIsIn As Boolean = True) As Boolean
            If aTestPoint Is Nothing Then Return False
            UpdatePath()
            Return Components.Segments.EncloseVector(New TVECTOR(aTestPoint), Bounds, aOnBoundIsIn:=aOnBoundIsIn)
        End Function
        Public Function FilletAtVertex(aVertex As dxfVector, aRadius As Double, Optional bApplyChamfer As Boolean = False, Optional aSecondChamfer As Double = 0.0) As Boolean
            '^fillets the polyline intersection with the passed radius if possble
            If aVertex Is Nothing Then Return False
            Dim idx As Integer = Vertices.IndexOf(aVertex, bReturnNearestVector:=True)
            If idx <= 0 Then Return False
            Return FilletAtVertex(idx, aRadius, bApplyChamfer, aSecondChamfer)
        End Function
        Public Function FilletAtVertex(aVertex As Integer, aRadius As Double, Optional bApplyChamfer As Boolean = False, Optional aSecondChamfer As Double = 0.0) As Boolean
            '^fillets the polyline intersection with the passed radius if possble
            If aVertex <= 0 Or aVertex > Vertices.Count Then Return False
            aRadius = Math.Abs(aRadius)
            If aRadius = 0 Then Return False
            Dim verts As TVERTICES
            Dim aFlag As Boolean
            Dim aPl As TPLANE = TPLANE.World
            verts = VerticesV
            verts = dxfUtils.FilletVertices(verts, aPl, aVertex, aRadius, bApplyChamfer, aSecondChamfer, aFlag)
            If aFlag Then
                VerticesV = verts
                IsDirty = True
            End If
            Return aFlag
        End Function
        Public Function FilletAtVertex(aVertIDS As String, aRadiusVals As String, Optional bApplyChamfer As Boolean = False, Optional aSecondChamfer As Double = 0.0) As Boolean
            Dim _rVal As Boolean = False
            '^fillets the polyline intersection with the passed radius if possble
            aVertIDS = Trim(aVertIDS)
            aRadiusVals = Trim(aRadiusVals)
            If aVertIDS = "" Then Return _rVal
            If aRadiusVals = "" Then Return _rVal
            Dim verts As TVERTICES = VerticesV
            Dim aFlag As Boolean
            If verts.Count <= 1 Then Return _rVal
            verts = dxfUtils.FilletVerticesM(verts, aVertIDS, aRadiusVals, aFlag, bApplyChamfer, aSecondChamfer)
            If aFlag Then
                _rVal = True
                VerticesV = verts
                IsDirty = True
            End If
            Return _rVal
        End Function
        Public Function FilletLines(Optional aGap As Double = 0.0) As colDXFEntities
            Return dxfUtils.FilletLines(Vertices, aGap, DisplaySettings, Plane)
        End Function
        Public Function FilletPoints(Optional aRadius As Double = 0.0, Optional bReturnLinearIntersections As Boolean = False) As colDXFVectors
            Return dxfUtils.FilletPoints(Vertices, Closed, aRadius, bReturnLinearIntersections)
        End Function
        Public Function Vertex(aIndex As Integer, Optional bReturnClone As Boolean = False) As dxfVector
            Return Vertices.Item(aIndex, bReturnClone)
        End Function
        Public Function Vertex(aTag As String, Optional bReturnClone As Boolean = False, Optional aTagOccurance As Integer = 1) As dxfVector
            Return Vertices.GetTagged(aTag, bReturnClone:=bReturnClone, aOccur:=aTagOccurance)
        End Function

        Public Function VertexOrdinate(aIndex As Integer, aOrdinateType As dxxOrdinateDescriptors) As Double
            Dim v1 As dxfVector = Vertices.Item(aIndex)
            If v1 IsNot Nothing Then Return v1.Ordinate(aOrdinateType) Else Return 0
        End Function

        Public Function SegmentIntersections(Optional aMaxRadius As Double = 0.0) As colDXFVectors
            Return dxfUtils.UnfilletedVertices(Vertices, Closed, True, aMaxRadius)
        End Function
        Public Function Segment(aIndex As Integer) As dxfEntity

            Dim aSegs As TSEGMENTS = PathSegments
            If aIndex > 0 And aIndex <= aSegs.Count Then
                Return CType(aSegs.Item(aIndex), dxfEntity)
            Else
                Return Nothing
            End If
        End Function
        Public Function IsRectangular(Optional aPrecis As Integer = 4, Optional MustBeOrthogonal As Boolean = False) As Boolean
            Dim issquare As Boolean
            Return IsRectangular(aPrecis, MustBeOrthogonal, issquare)
        End Function
        Public Function IsRectangular(aPrecis As Integer, MustBeOrthogonal As Boolean, ByRef rIsSquare As Boolean) As Boolean
            Dim _rVal As Boolean = False
            '#1the precision to use in the dxeDimension for rectangular
            '#2flag telling if the polygon must be aligned with the X Y axis to be considered rectangular
            '^returns True if the polygon is actually a Rectangle
            rIsSquare = False
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim v3 As TVECTOR
            Dim v4 As TVECTOR
            Dim diag1 As Double
            Dim diag2 As Double
            Dim d1 As TVECTOR
            aPrecis = Math.Abs(aPrecis)
            If aPrecis >= 10 Then aPrecis = 10
            Try
                If Vertices.Count <> 4 Then Return _rVal
                If Vertices.Item(1).Radius <> 0 Then Return _rVal
                If Vertices.Item(2).Radius <> 0 Then Return _rVal
                If Vertices.Item(3).Radius <> 0 Then Return _rVal
                If Vertices.Item(4).Radius <> 0 Then Return _rVal
                v1 = Vertices.ItemVector(1)
                v2 = Vertices.ItemVector(2)
                v3 = Vertices.ItemVector(3)
                v4 = Vertices.ItemVector(4)
                diag1 = dxfProjections.DistanceTo(v1, v3, aPrecis)
                diag2 = dxfProjections.DistanceTo(v2, v4, aPrecis)
                If diag1 <> 0 And diag2 <> 0 Then
                    _rVal = (diag1 = diag2)
                End If
                If _rVal Then
                    rIsSquare = dxfProjections.DistanceTo(v1, v2, aPrecis) = dxfProjections.DistanceTo(v2, v3, aPrecis)
                End If
                If MustBeOrthogonal And _rVal Then
                    d1 = PlaneV.XDirection
                    If Not d1.Equals(v2.DirectionTo(v3), True, aPrecis) Then _rVal = False
                    If _rVal Then
                        d1 = PlaneV.YDirection
                        If Not d1.Equals(v1.DirectionTo(v2), True, aPrecis) Then _rVal = False
                    End If
                End If
            Catch ex As Exception
                _rVal = False
            End Try
            Return _rVal
        End Function
        Public Sub GetLimits(ByRef rMinX As Double, ByRef rMaxX As Double, ByRef rMinY As Double, ByRef rMaxY As Double)
            '^simply returns the 2D ordinate limits of the polygons bounding rectangle
            BoundingRectangle.GetLimits(rMinX, rMaxX, rMinY, rMaxY)
        End Sub
        Public Function GetOrdinate(SearchParameter As dxxOrdinateTypes, Optional SearchExtendedVerts As Boolean = True) As Double
            '#1parameter controling the value returned
            '^returns the requested ordinate based on the search parameter and the members of polygons vertices collection
            If SearchExtendedVerts Then
                Return ExtendedVertices.GetOrdinate(SearchParameter)
            Else
                Return Vertices.GetOrdinate(SearchParameter)
            End If
        End Function
        Public Function VertexCoordinatesGet(Optional aPlane As dxfPlane = Nothing, Optional aDelimiter As String = "¸") As String

            '^a concantonated string of all the vertex coordinates of the polyline
            '~vertex coordinates are the 2D coordinates of the vector with respect to the passed plane augmented with the vertex radius of the vector
            '~ie "(X,Y,VertexRadius)" where the vertex radius indicates the vector is the start of an arc.
            '~the delimitor is "¸" (char 184)
            If TPLANE.IsNull(aPlane) Then
                Return Vertices.VertexCoordinatesGet(Plane, aDelimiter)
            Else
                Return Vertices.VertexCoordinatesGet(aPlane, aDelimiter)
            End If

        End Function
        Public Sub VertexCoordinatesSet(aCoordinates As String, bClosed As Boolean, Optional aPlane As dxfPlane = Nothing, Optional aDelimiter As String = "¸")
            '^a concantonated string of all the vertex coordinates of the polyline
            '~vertex coordinates are the 2D coordinates of the vector with respect to the passed plane augmented with the vertex radius of the vector
            '~ie "(X,Y,VertexRadius)" where the vertex radius indicates the vector is the start of an arc.
            '~the delimitor is "¸" (char 184)
            Vertices.VertexCoordinatesSet(aCoordinates, aPlane, bRetainCurrentMembers:=False, aDelimiter:=aDelimiter)

            Closed = bClosed
        End Sub
    End Class

End Namespace
