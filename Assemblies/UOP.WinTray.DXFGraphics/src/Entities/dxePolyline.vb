Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public Class dxePolyline
        Inherits dxfPolyline
        Implements ICloneable
#Region "Members"
#End Region 'Members
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxPolylineTypes.Polyline)
        End Sub

        Public Sub New(aEntity As dxePolyline, Optional bCloneInstances As Boolean = False)
            MyBase.New(dxxGraphicTypes.Polyline, aEntityToCopy:=aEntity, bCloneInstances:=bCloneInstances)
        End Sub

        Public Sub New(aShape As iShape, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aSegWidth As Double = 0)
            MyBase.New(dxxPolylineTypes.Polyline, aDisplaySettings)
            If aShape Is Nothing Then Return
            Vertices = New colDXFVectors(aShape.Vertices)
            PlaneV = New TPLANE(aShape.Plane)
            Closed = aShape.Closed

            If aSegWidth > 0 Then SegmentWidth = aSegWidth
        End Sub

        Public Sub New(aVertices As IEnumerable(Of iVector), Optional bClosed As Boolean = False, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aPlane As dxfPlane = Nothing, Optional aSegWidth As Double = 0)
            MyBase.New(dxxPolylineTypes.Polyline, aDisplaySettings, aVertices)
            PlaneV = New TPLANE(aPlane)
            Closed = bClosed

            If aSegWidth > 0 Then SegmentWidth = aSegWidth
        End Sub
        Friend Sub New(aSubEntity As TENTITY, Optional bNewGUID As Boolean = False)
            MyBase.New(dxxPolylineTypes.Polyline, aSubEntity, bNewGUID:=bNewGUID)
        End Sub

        Friend Sub New(aObject As TOBJECT)
            MyBase.New(dxxPolylineTypes.Polyline)
            DefineByObject(aObject)
        End Sub

        Friend Sub New(aVertices As TVECTORS, Optional bClosed As Boolean = False, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aPlane As dxfPlane = Nothing, Optional aSegWidth As Double = 0)
            MyBase.New(dxxPolylineTypes.Polyline, aDisplaySettings, New colDXFVectors(aVertices))
            If Not dxfPlane.IsNull(aPlane) Then PlaneV = New TPLANE(aPlane)
            Closed = bClosed
            If aDisplaySettings IsNot Nothing Then DisplaySettings = aDisplaySettings
            If aSegWidth > 0 Then SegmentWidth = aSegWidth
        End Sub
        Friend Sub New(aVertices As TVERTICES, Optional bClosed As Boolean = False, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aPlane As dxfPlane = Nothing, Optional aSegWidth As Double = 0)
            MyBase.New(dxxPolylineTypes.Polyline, aDisplaySettings, New colDXFVectors(aVertices))
            If Not dxfPlane.IsNull(aPlane) Then PlaneV = New TPLANE(aPlane)
            VerticesV = aVertices
            Closed = bClosed

            If aSegWidth > 0 Then SegmentWidth = aSegWidth
        End Sub

        Public Sub New(aCoordinates As String, bClosed As Boolean, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aSegmentWidth As Double = 0.0, Optional aPlane As dxfPlane = Nothing, Optional aDelimiter As Char = "¸")
            MyBase.New(dxxPolylineTypes.Polyline, aDisplaySettings, New colDXFVectors(aCoordinates, aDelimitor:=aDelimiter))

            '^a concantonated string of all the vertex coordinates of the polyline
            '~vertex coordinates are the 2D coordinates of the vector with respect to the passed plane augmented with the vertex radius of the vector
            '~ie "(X,Y,VertexRadius)" where the vertex radius indicates the vector is the start of an arc.
            '~the delimitor is "¸" (char 184))
            SegmentWidth = aSegmentWidth
            Closed = bClosed
            PlaneV = New TPLANE(aPlane)


        End Sub

        Public Sub New(aRectangle As dxfRectangle)
            MyBase.New(dxxPolylineTypes.Polyline)

            If aRectangle Is Nothing Then Return
        End Sub
#End Region 'Constructors

#Region "Properties"





#End Region 'Properties
#Region "MustOverride Entity Methods"

        Friend Overrides Sub DefineByObject(aObj As TOBJECT, Optional bNoHandles As Boolean = False, Optional aStyle As dxfTableEntry = Nothing, Optional aBlock As dxfBlock = Nothing)
            Reactors.Append(aObj.Reactors, bClear:=True)
            ExtendedData.Append(aObj.ExtendedData, bClear:=True)


            Dim bVal As Object = PropValueI(90)
            Dim elev As Double = PropValueD(38)
            Dim aPlane As TPLANE = TPLANE.ArbitraryCS(aObj.Properties.GCValueV(210, TVECTOR.WorldZ))


            Dim gWd As Double
            Dim bWds As Boolean




            DisplayStructure = aObj.DisplayVars
            If elev <> 0 Then aPlane.Origin += aPlane.ZDirection * -elev
            PlaneV = aPlane
            If bVal > 0 Then
                Dim aVal As Object = PropValueI(70)
                If aVal > 128 Then
                    PlineGen = True
                    aVal -= 128
                End If
                Closed = (aVal = 1)
                gWd = PropValueD(43, -1)
                If gWd < 0 Then gWd = -1
                bWds = (gWd = 0)
                'get the vertices
                Dim tverts As TVERTICES = New TPROPERTIES(ActiveProperties()).Vertices(10)
                VerticesV = New TVERTICES(0)
                For i As Integer = 1 To tverts.Count
                    Dim v1 As TVERTEX = tverts.Item(i)
                    v1.Vector = aPlane.WorldVector(v1.Vector)
                    If v1.Bulge <> 0 Then
                        Dim v2 As TVERTEX
                        If i <= tverts.Count - 1 Then v2 = tverts.Item(i + 1) Else v2 = tverts.Item(1)
                        v2.Vector = aPlane.WorldVector(v2.Vector)

                        Dim aA As TARC = TARC.ByBulge(v1.Vector, v2.Vector, v1.Bulge, True, aPlane)
                        If aA.Radius > 0 Then
                            v1.Radius = aA.Radius
                            v1.Inverted = aA.ClockWise
                        Else
                            v1.Radius = 0
                        End If
                    End If

                    If gWd > 0 Then
                        v1.StartWidth = gWd
                        v1.EndWidth = gWd
                    End If

                    Vertices.Add(New dxfVector(v1))
                Next i
            End If
            If gWd > 0 Then SegmentWidth = gWd
            If Not bNoHandles Then MyBase.Handle = PropValueStr(5)
        End Sub

        ''' <summary>
        ''' returns a clone of the entity transfered to the passed plane
        ''' </summary>
        ''' <remarks>the entities defining vectors are converted to vectors with respect to the entities OCS plane and then redefines them based on the passed plane </remarks>
        ''' <param name="aPlane"></param>
        '''<param name="aFromPlane">the plane to define the definition points with respect to prior to transfer to the new plane. If not passed the entities OCS is used </param>
        ''' <returns></returns>
        Public Overloads Function TransferedToPlane(aPlane As dxfPlane, Optional aFromPlane As dxfPlane = Nothing) As dxePolyline
            Dim _rVal As dxePolyline = Me.Clone()
            _rVal.TransferToPlane(aPlane, aFromPlane)
            Return _rVal
        End Function

        ''' <summary>
        ''' Returns a new object with properties matching those of the cloned object
        ''' </summary>
        ''' <returns></returns>
        Public Overloads Function Clone() As dxePolyline
            Return New dxePolyline(Me)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return Clone()
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
            Dim aOCS As New TPLANE("")
            Dim tname As String = String.Empty
            If aInstances Is Nothing Then
                aInstances = Instances
                aInstances.Plane = Bounds
            End If
            If bSuppressInstances Then iCnt = 1 Else iCnt = aInstances.Count + 1
            For i = 1 To iCnt
                If aInstance <= 0 Or i = aInstance Then
                    _rVal.Add(DXFProps(aInstances, i, aOCS, tname))
                End If
            Next i
            If iCnt > 1 Then
                _rVal.Name = tname & "-" & iCnt & " INSTANCES"
            End If
            Return _rVal
        End Function
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY

            '#1the instance to create the properties for
            '#2the OCS plane of the entity (created for first instance)
            '^returns the entities properties updated for output to a DXF file
            'On Error Resume Next
            aInstance = Math.Abs(aInstance)
            If aInstance <= 0 Then aInstance = 1
            Dim _rVal As New TPROPERTYARRAY(aInstance:=aInstance)
            Dim myProps As New TPROPERTIES(Properties)

            Dim aPl As TPLANE = Bounds
            Dim scl As Double
            Dim ang As Double
            Dim bInv As Boolean
            Dim bLft As Boolean
            Dim gw As Double
            Dim cnt As Integer
            Dim bClosed As Boolean
            Dim bOCS As TPLANE
            Dim vrts As New TVERTICES(Vertices)
            scl = 1
            ang = 0
            gw = SegmentWidth

            If vrts.Count <= 1 Then Return _rVal
            Dim vProps As TPROPERTIES
            Dim elev As Double
            Dim bFlag As Boolean
            Dim vgw As Double
            bClosed = Closed
            If vgw < 0 Then vgw = 0
            If aInstance > 1 Then
                If aInstances Is Nothing Then aInstances = Instances
                Dim aTrs As TTRANSFORMS = aInstances.Transformations(aInstance - 1, scl, ang, bInv, bLft)
                TTRANSFORMS.Apply(aTrs, vrts)
                TTRANSFORMS.Apply(aTrs, aPl)
                bOCS = TPLANE.ArbitraryCS(aPl.ZDirection)
                vgw = myProps.ValueD("Constant Width")
                vProps = vrts.DXFProperties(aPl.ZDirection, SegmentWidth, cnt, vgw)
                rTypeName = Trim(myProps.Item(1).Value)
            Else
                myProps.Handle = Handle
                aOCS = TPLANE.ArbitraryCS(aPl.ZDirection)
                bOCS = aOCS
                vProps = vrts.DXFProperties(aPl.ZDirection, SegmentWidth, cnt, vgw)
                myProps.SetVectorGC(210, aOCS.ZDirection, bNormalize:=Math.Round(aOCS.ZDirection.Z, 6) = 1)
                vgw = gw
                If vgw < 0 Then vgw = 0
                myProps.SetVal("Constant Width", vgw)
                myProps.SetVal("Vertex Count", cnt)
                SetProps(myProps)
                UpdateCommonProperties("LWPOLYLINE")
                rTypeName = "LWPOLYLINE"
                myProps = New TPROPERTIES(Properties)
            End If
            Dim plf As Integer = 0
            If bClosed Then plf += 1
            If PlineGen Then plf += 128
            myProps.SetVal("Polyline Flag(Bit coded)", plf)
            Dim v1 As TVECTOR = dxfProjections.ToPlane(TVECTOR.Zero, aPl, aPl.ZDirection, rDistance:=elev, rAntiNormal:=bFlag)
            If Not bFlag Then elev = -elev
            myProps.SetVectorGC(210, bOCS.ZDirection, bNormalize:=Math.Round(bOCS.ZDirection.Z, 6) = 1)
            myProps.SetVal("Constant Width", vgw)
            myProps.SetVal("Elevation", elev)
            myProps.SetVal("Vertex Count", cnt)
            myProps = myProps.ReplaceByGC(vProps, 39, True, 1)
            _rVal.Add(myProps, rTypeName, True, True)
            'If aInstance = 1 Then Props = myProps
            Return _rVal
        End Function





        Public Function Perimeter(Optional bAsLines As Boolean = False, Optional aCurveDivisions As Double = 20, Optional bClosed As Boolean = False) As dxePolyline

            '^returns the bounding segments as a Polyline
            '^returns the entity as a Polyline
            Dim _rVal As New dxePolyline With {
                .SuppressEvents = True,
                .DisplayStructure = DisplayStructure,
                .PlaneV = PlaneV,
                .Vertices = Segments.PolylineVertices(bAsLines, aCurveDivisions, bClosed),
                .Identifier = "Polyline"}
            _rVal.SuppressEvents = False
            Return _rVal
        End Function
        Public Function ToPolygon(Optional bAsLines As Boolean = False, Optional aCurveDivisions As Double = 20, Optional bClosed As Boolean? = Nothing) As dxePolygon

            '^returns the bounding segments as a Polygon
            '^returns the entity as a Polygon
            UpdatePath()
            Dim _rVal = New dxePolygon With {
                .SuppressEvents = True,
                .DisplayStructure = DisplayStructure,
                .PlaneV = PlaneV,
                .Closed = Closed
            }

            If bClosed.HasValue Then
                _rVal.Closed = bClosed.Value
            End If

            If bAsLines Then
                _rVal.Vertices = Segments.PolylineVertices(True, aCurveDivisions, _rVal.Closed)
            Else
                _rVal.Vertices = Vertices
            End If
            _rVal.TFVCopy(Me)
            _rVal.Identifier = "Polygon"
            _rVal.SuppressEvents = False
            Return _rVal
        End Function

        Public Function TrimWithArc(aTrimArc As dxeArc, Optional aKeepPoint As iVector = Nothing, Optional bTrimmerIsInfinite As Boolean = False, Optional bDoAddSegs As Boolean = True, Optional bDoSubPGons As Boolean = False) As Boolean
            '#1the arc to trim with
            '#2a point to determine which side of the arc to keep
            '#3flag indicating the passed arc should be treated as infinite (360 degree span)
            '^trims the polyline with the passed arc
            Return dxfBreakTrimExtend.trim_Polyline_Arc(Me, aTrimArc, aKeepPoint, bTrimmerIsInfinite, bDoAddSegs, bDoSubPGons)
        End Function
        Public Function TrimWithLine(aTrimLine As iLine, aKeepPoint As iVector, Optional bTrimmerIsInfinite As Boolean = False, Optional bDoAddSegs As Boolean = True, Optional bDoSubPGons As Boolean = False) As Boolean

            '#1line to trim with
            '#2a point to determine which side of the line to keep
            '#3flag indicating the passed line should be treated as infinite
            '^trims the polyline with the passed line
            Return dxfBreakTrimExtend.trim_Polyline_Line(Me, aTrimLine, New TVECTOR(aKeepPoint), bTrimmerIsInfinite, bDoAddSegs, bDoSubPGons)

        End Function
        Public Function TrimWithOrthoLine(aTrimType As dxxTrimTypes, TrimCoordinate As Double) As Boolean
            '#1the type of trim to perform
            '#2the x or y coordinate to trim at
            '#3return flag indicating if any actual trimming was performed
            '^trims the current polygon at the indicted coordinate
            '~only allows vertical or horizontal line trimming.
            Dim newPline As dxePolyline
            Dim rTrimPerformed As Boolean = False
            Try
                newPline = dxfBreakTrimExtend.trim_Polyline_Ortho(Me, aTrimType, TrimCoordinate, rTrimPerformed)
                If rTrimPerformed Then
                    IsDirty = True
                    VerticesV = newPline.VerticesV
                End If
                newPline = Nothing
            Catch ex As Exception
                Throw ex
            End Try
            Return rTrimPerformed
        End Function
        Public Function TrimWithSegments(TrimSegments As colDXFEntities, Optional SegmentsAreInfinite As Boolean = False, Optional refPt As dxfVector = Nothing) As Boolean
            '#1a collection of segments to trim then polygon with
            '#2flag to treat the passed segments as infinite
            '^trims the polygon with the passed collection of polygon segments (lines & arcs)
            If TrimSegments Is Nothing Then Return False
            If TrimSegments.Count <= 0 Then Return False
            If refPt Is Nothing Then refPt = Center()

            Dim bWasTrimmed As Boolean
            Dim rTrimPerformed As Boolean = False
            For i As Integer = 1 To TrimSegments.Count
                Dim aSeg As dxfEntity = TrimSegments.Item(i)
                If aSeg.GraphicType = dxxGraphicTypes.Arc Or aSeg.GraphicType = dxxGraphicTypes.Line Then
                    If aSeg.GraphicType = dxxGraphicTypes.Arc Then
                        bWasTrimmed = TrimWithArc(aSeg, refPt, SegmentsAreInfinite)
                    Else
                        bWasTrimmed = TrimWithLine(aSeg, refPt, SegmentsAreInfinite)
                    End If
                    If bWasTrimmed Then rTrimPerformed = True
                End If
            Next i
            Return rTrimPerformed
        End Function


#End Region 'Methods

    End Class 'dxePolyline
End Namespace
