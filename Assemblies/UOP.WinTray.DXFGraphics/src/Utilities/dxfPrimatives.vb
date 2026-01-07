Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics.Utilities
    Public Class dxfPrimatives
#Region "Members"
        Private _ImageGUID As String
#End Region 'Members
#Region "Constructors"
        Public Sub New()
        End Sub
        Public Sub New(aImageGuid As String)
            _ImageGUID = aImageGuid
        End Sub
#End Region 'Constructors
#Region "Properties"
        Friend Property ImageGUID As String
            Get
                Return _ImageGUID
            End Get
            Set(value As String)
                _ImageGUID = value
            End Set
        End Property
#End Region 'Properties
#Region "Methods"
        Public Function Angle_End(aCenter As iVector, aLegWidth As Double, aThickness As Double, Optional bLegWidth As Double = 0.0, Optional aSuppressFillets As Boolean = False, Optional aRotation As Double = 0.0, Optional bFilletEnds As Boolean = False, Optional aSegmentWidth As Double = 0.0, Optional aPlane As dxfPlane = Nothing, Optional aReturnPolygon As Boolean = False, Optional aXOffset As Double = 0.0, Optional aYOffset As Double = 0.0) As dxfPolyline
            '#1the to center center for the angle
            '#2the top leg width of the desired angle
            '#3the thickness of the material
            '#4the vertical leg width (if different from the top)
            '^returns a L shape
            Dim img As dxfImage = Nothing
            aPlane = GetPlane(aPlane, img)
            Dim v1 As New TVECTOR(aCenter)
            v1.X += aXOffset
            v1.Y += aYOffset
            Dim _rVal As dxfPolyline = Nothing
            Try
                _rVal = dxfPrimatives.CreateAngle_End(v1, aLegWidth, aThickness, bLegWidth, aSuppressFillets, aRotation, bFilletEnds, aSegmentWidth, aPlane, aReturnPolygon)
                xSetDisplay(_rVal, aImage:=img)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod().Name, ex, img)
                Return _rVal
            End Try
            Return Nothing
        End Function
        Public Function Angle_Side(aCenter As iVector, aLegLength As Double, aLength As Double, aThickness As Double, Optional bShowHidden As Boolean = False, Optional aRotation As Double = 0.0, Optional aSegmentWidth As Double = 0.0, Optional aPlane As dxfPlane = Nothing, Optional aReturnPolygon As Boolean = False, Optional aXOffset As Double = 0.0, Optional aYOffset As Double = 0.0) As dxfPolyline
            '#1the center for the angle
            '#2the leg length for the angle
            '#3the length for the angle
            '#4the thickness of the material
            '^returns a L shape
            Dim img As dxfImage = Nothing
            aPlane = GetPlane(aPlane, img)
            Dim v1 As New TVECTOR(aCenter)

            v1.X += aXOffset
            v1.Y += aYOffset
            Dim _rVal As dxfPolyline = Nothing
            Try
                _rVal = dxfPrimatives.CreateAngle_Side(New TVECTOR(aCenter), aLegLength, aLength, aThickness, bShowHidden, aRotation, aSegmentWidth, aPlane, aReturnPolygon, aXOffset, aYOffset)
                xSetDisplay(_rVal, aImage:=img)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod().Name, ex, img)
                Return _rVal
            End Try
        End Function
        Public Function Angle_Top(aCenter As iVector, aLegWidth As Double, aLength As Double, aThickness As Double, Optional aRotation As Double = 0.0, Optional aChamferAngle As Double = 0.0, Optional aSegmentWidth As Double = 0.0, Optional aPlane As dxfPlane = Nothing, Optional aReturnPolygon As Boolean = False, Optional aXOffset As Double = 0.0, Optional aYOffset As Double = 0.0) As dxfPolyline
            '#1the to center center for the angle
            '#2the length for the angle
            '#3the top leg width of the desired angle
            '#4the thickness of the material
            '^returns a L shape
            Dim img As dxfImage = Nothing
            aPlane = GetPlane(aPlane, img)
            Dim v1 As New TVECTOR(aCenter)
            v1.X += aXOffset
            v1.Y += aYOffset
            Dim _rVal As dxfPolyline
            Try
                _rVal = dxfPrimatives.CreateAngle_Top(v1, aLegWidth, aLength, aThickness, aRotation, aChamferAngle, aSegmentWidth, aPlane, aReturnPolygon)
                xSetDisplay(_rVal, aImage:=img)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod().Name, ex, img)
            End Try
            Return Nothing
        End Function
        Public Function Arc(aCenter As iVector, bReturnAsPolygon As Boolean, aRadius As Double, Optional aStartAngle As Double = 0.0, Optional aEndAngle As Double = 360, Optional bClockwise As Boolean = False, Optional aPlane As dxfPlane = Nothing, Optional aSegmentWidth As Double = 0.0, Optional bClosed As Boolean = False) As dxfPolyline
            '#1the center for the new circle
            '#2flag to return a polygon instead of a polyline
            '#3the radius of the desired arc
            '#4the start angle
            '#5the end angle
            '#6clockwise flag
            '#7the coordinate system of definition
            '#8a segment width
            '#9flag to close the polyline
            '^returns a circular polyline
            Dim img As dxfImage = Nothing
            aPlane = GetPlane(aPlane, img)
            Dim _rVal As dxfPolyline = Nothing
            If aCenter IsNot Nothing Then aPlane.OriginV = New TVECTOR(aCenter)
            Try
                _rVal = dxfPrimatives.CreateArc(aPlane.OriginV, bReturnAsPolygon, aRadius, aStartAngle, aEndAngle, bClockwise, aPlane, aSegmentWidth, bClosed)
                xSetDisplay(_rVal, aImage:=img)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod().Name, ex, img)
                Return _rVal
            End Try
        End Function
        Friend Shared Function CreateArc(aCenter As TVECTOR, aReturnPolygon As Boolean, aRadius As Double, Optional aStartAngle As Double = 0.0, Optional aEndAngle As Double = 360, Optional bClockwise As Boolean = False, Optional aPlane As dxfPlane = Nothing, Optional aSegmentWidth As Double = 0.0, Optional bClosed As Boolean = False) As dxfPolyline

            '#1the center for the new arc
            '#2flag to return a polygon instead of a polyline
            '#3the radius of the desired arc
            '#4the start angle
            '#5the end angle
            '#6clockwise flag
            '#7the coordinate system of definition
            '#8a segment width
            '#9flag to close the polyline
            '^returns as circular polyline
            aRadius = Math.Abs(aRadius)
            If aRadius = 0 Then aRadius = 1
            Dim aPl As New TPLANE(aPlane, New dxfVector(aCenter))

            Dim aArc As New dxeArc(aPl, aRadius, aStartAngle, aEndAngle) With {.ClockWise = bClockwise}
            Dim aspn As Double = aArc.SpannedAngle
            Dim sArcs As colDXFEntities = aArc.Divide(90)
            Dim verts As New List(Of dxfVector)
            For i As Integer = 1 To sArcs.Count
                aArc = sArcs.Item(i)
                verts.Add(aArc.StartPt)
                If i = sArcs.Count And aspn < 359.99 Then verts.Add(aArc.EndPt)
            Next i
            Return dxfPrimatives.CreatePGorPL2(aReturnPolygon, aPl, bClosed, verts, aSegmentWidth)

        End Function
        Public Function ArcTrimmedCircle(aCenter As iVector, aTrimCenter As iVector, bReturnAsPolygon As Boolean, aRadius As Double, aTrimRadius As Double, Optional aPlane As dxfPlane = Nothing, Optional bClosed As Boolean = True) As dxfEntity
            '#1the center for the new circle
            '#2the center of the trimming circle
            '#3flag to return a polygon instead of a polyline
            '#3the radius of the new circle
            '#4the radius of the triming circle
            '#5the coordinate system of definition
            '#6flag to close the polyline
            '^returns a polyline that is the boundary of a circle trimmed by another circle
            '~the two circles as assumed to be on the same plane
            Dim img As dxfImage = Nothing
            aPlane = GetPlane(aPlane, img)
            Try
                Dim _rVal As dxfEntity = dxfPrimatives.CreateTrimmedCircle(New TVECTOR(aCenter), New TVECTOR(aTrimCenter), bReturnAsPolygon, aRadius, aTrimRadius, aPlane, bClosed)
                xSetDisplay(_rVal, aImage:=img)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod().Name, ex, img)
            End Try
            Return Nothing
        End Function
        Friend Shared Function CreateAngle_Top(aLegCenter As TVECTOR, aLegWidth As Double, aLength As Double, aThickness As Double, Optional aRotation As Double = 0.0, Optional aChamferAngle As Double = 0.0, Optional aSegmentWidth As Double = 0.0, Optional aPlane As dxfPlane = Nothing, Optional aReturnPolygon As Boolean = False) As dxfPolyline
            Dim _rVal As dxfPolyline
            '#1the to center center for the angle
            '#2the length for the angle
            '#3the top leg width of the desired angle
            '#4the thickness of the material
            '^returns a L shape
            aLegWidth = Math.Abs(aLegWidth)
            aLength = Math.Abs(aLength)
            aThickness = Math.Abs(aThickness)
            If aLegWidth = 0 Then aLegWidth = 1
            If aThickness = 0 Then aThickness = 0.1
            aChamferAngle = Math.Abs(aChamferAngle)
            Dim rVerts As colDXFVectors
            Dim i As Integer
            Dim aPl As TPLANE
            Dim bPl As TPLANE
            Dim wd As Double
            Dim thk As Double
            Dim lg As Double
            Dim d1 As Double
            Dim verts As New TVERTICES(0)
            wd = aLegWidth
            lg = aLength / 2
            thk = aThickness
            If aChamferAngle > 0 And aChamferAngle < 90 Then
                d1 = (wd - thk)
                d1 /= Math.Tan(dxfMath.Deg2Rad(aChamferAngle))
            End If
            verts.Add(-thk, lg)
            verts.Add(-thk, -lg)
            verts.Add(0, -lg)
            verts.Add(0, lg)
            verts.Add(-thk, lg)
            verts.Add(-wd, lg - d1)
            verts.Add(-wd, -lg + d1)
            verts.Add(-thk, -lg)
            If Not dxfPlane.IsNull(aPlane) Then aPl = New TPLANE(aPlane) Else aPl = TPLANE.World
            aPl.Origin = aLegCenter
            bPl = aPl.Clone
            If aRotation <> 0 Then bPl.Revolve(aRotation)
            For i = 0 To verts.Count - 1
                verts.SetVector(bPl.WorldVector(verts.Vector(i + 1)), i + 1)
            Next i
            rVerts = New colDXFVectors(verts)
            rVerts.Item(1).Linetype = dxfLinetypes.Hidden
            rVerts.Item(1).Color = dxxColors.Green
            _rVal = dxfPrimatives.CreatePGorPL2(aReturnPolygon, aPl, False, rVerts, aSegmentWidth)
            Return _rVal
        End Function
        Friend Shared Function CreateAngle_Side(aLegCenter As TVECTOR, aLegLength As Double, aLength As Double, aThickness As Double, Optional bShowHidden As Boolean = False, Optional aRotation As Double = 0.0, Optional aSegmentWidth As Double = 0.0, Optional aPlane As dxfPlane = Nothing, Optional aReturnPolygon As Boolean = False, Optional aXOffset As Double = 0.0, Optional aYOffset As Double = 0.0) As dxfPolyline
            Dim _rVal As dxfPolyline
            '#1the center for the angle
            '#2the leg length for the angle
            '#3the length for the angle
            '#4the thickness of the material
            '^returns the profile view of an angle
            aLegLength = Math.Abs(aLegLength)
            aLength = Math.Abs(aLength)
            aThickness = Math.Abs(aThickness)
            If aLegLength = 0 Then aLegLength = 1
            If aThickness = 0 Then aThickness = 0.1
            Dim rVerts As colDXFVectors
            '**UNUSED VAR** Dim i As Integer
            Dim aPl As New TPLANE("")
            Dim bPl As New TPLANE("")
            Dim wd As Double
            Dim thk As Double
            Dim lg As Double
            wd = aLegLength / 2
            lg = aLength / 2
            thk = aThickness
            If Not dxfPlane.IsNull(aPlane) Then aPl = New TPLANE(aPlane) Else aPl = TPLANE.World
            aPl.Origin = aLegCenter
            bPl = aPl
            If aRotation <> 0 Then aPl.Revolve(aRotation)
            rVerts = New colDXFVectors
            rVerts.AddV(aPl.Vector(-lg + aXOffset, wd - thk + aYOffset))
            If bShowHidden Then rVerts.LastVector.Linetype = dxfLinetypes.Hidden
            If bShowHidden Then rVerts.LastVector.Color = dxxColors.Green
            rVerts.AddV(aPl.Vector(lg + aXOffset, wd - thk + aYOffset))
            rVerts.AddV(aPl.Vector(lg + aXOffset, -wd + aYOffset))
            rVerts.AddV(aPl.Vector(-lg + aXOffset, -wd + aYOffset))
            rVerts.AddV(aPl.Vector(-lg + aXOffset, wd + aYOffset))
            rVerts.AddV(aPl.Vector(lg + aXOffset, wd + aYOffset))
            rVerts.AddV(aPl.Vector(lg + aXOffset, wd - thk + aYOffset))
            _rVal = dxfPrimatives.CreatePGorPL2(aReturnPolygon, bPl, False, rVerts, aSegmentWidth)
            Return _rVal
        End Function
        Friend Shared Function CreateAngle_End(bLegCenter As TVECTOR, aLegWidth As Double, aThickness As Double, Optional bLegWidth As Double = 0.0, Optional aSuppressFillets As Boolean = False, Optional aRotation As Double = 0.0, Optional bFilletEnds As Boolean = False, Optional aSegmentWidth As Double = 0.0, Optional aPlane As dxfPlane = Nothing, Optional aReturnPolygon As Boolean = False) As dxfPolyline

            '#1the to center center for the angle
            '#2the top leg width of the desired angle
            '#3the thickness of the material
            '#4the vertical leg width (if different from the top)
            '^returns a L shape
            aThickness = Math.Abs(aThickness)
            If aLegWidth = 0 Then aLegWidth = 1
            If aThickness = 0 Then aThickness = 0.1
            If bLegWidth = 0 Then bLegWidth = aLegWidth
            Dim verts As TVERTICES
            Dim rVerts As colDXFVectors
            Dim i As Integer
            Dim aPl As TPLANE
            Dim bPl As TPLANE
            Dim wd As Double
            Dim thk As Double
            Dim rad1 As Double
            Dim rad2 As Double
            Dim ht As Double
            Dim swapX As Double
            Dim swapY As Double
            Dim bInvert As Boolean
            Dim v1 As TVECTOR
            ht = Math.Abs(bLegWidth / 2)
            wd = Math.Abs(aLegWidth)
            thk = aThickness
            rad1 = thk
            rad2 = 2 * thk
            bInvert = aLegWidth < 0
            verts = New TVERTICES(0)
            If aSuppressFillets Then
                verts.Add(-wd, ht)
                verts.Add(0, ht)
                verts.Add(0, -ht)
                verts.Add(-thk, -ht)
                verts.Add(-thk, ht - thk)
                verts.Add(-wd, ht - thk)
            Else
                If Not bFilletEnds Then
                    verts.Add(-wd, ht)
                Else
                    verts.Add(-wd + thk / 2, ht)
                End If
                verts.Add(-rad2, ht, aVertexRadius:=rad2, aInverted:=Not bInvert)
                verts.Add(0, ht - rad2)
                If Not bFilletEnds Then
                    verts.Add(0, -ht)
                    verts.Add(-thk, -ht)
                Else
                    verts.Add(0, -ht + thk / 2, aVertexRadius:=thk / 2, aInverted:=Not bInvert)
                    verts.Add(-thk / 2, -ht, aVertexRadius:=thk / 2, aInverted:=Not bInvert)
                    verts.Add(-thk, -ht + thk / 2)
                End If
                verts.Add(-thk, ht - thk - rad1, aVertexRadius:=rad1, aInverted:=bInvert)
                verts.Add(-rad2, ht - thk)
                If Not bFilletEnds Then
                    verts.Add(-wd, ht - thk)
                Else
                    verts.Add(-wd + thk / 2, ht - thk, aVertexRadius:=thk / 2, aInverted:=Not bInvert)
                    verts.Add(-wd, ht - thk / 2, aVertexRadius:=thk / 2, aInverted:=True)
                End If
            End If
            If Not dxfPlane.IsNull(aPlane) Then aPl = New TPLANE(aPlane) Else aPl = TPLANE.World
            aPl.Origin = bLegCenter
            bPl = aPl.Clone
            If aRotation <> 0 Then bPl.Revolve(aRotation)
            If aLegWidth < 0 Then swapX = -1 Else swapX = 1
            If bLegWidth < 0 Then swapY = -1 Else swapY = 1
            For i = 1 To verts.Count
                v1 = verts.Vector(i)
                v1.X *= swapX
                v1.Y *= swapY
                verts.SetVector(bPl.WorldVector(v1), i)
            Next i
            rVerts = New colDXFVectors(verts)
            Return dxfPrimatives.CreatePGorPL2(aReturnPolygon, aPl, True, rVerts, aSegmentWidth)
        End Function
        Public Function Channel_End(aCenter As iVector, aTopWidth As Double, aWebHeight As Double, aThickness As Double, Optional aBottomWidth As Double = 0.0, Optional aJoggleHeight As Double = 0.0, Optional aSuppressFillets As Boolean = False, Optional aRotation As Double = 0.0, Optional bFilletEnds As Boolean = False, Optional aSegmentWidth As Double = 0.0, Optional aPlane As dxfPlane = Nothing, Optional aReturnPolygon As Boolean = False, Optional aXOffset As Double = 0.0, Optional aYOffset As Double = 0.0) As dxfPolyline
            '#1the view to return
            '#2the to center center for the channel
            '#3the height of the desired channel
            '#4the flange width of the desired channel
            '#5the thickness of the material
            '^returns as rectangular c channel
            Dim img As dxfImage = Nothing
            aPlane = GetPlane(aPlane, img)
            Dim _rVal As dxfPolyline = Nothing
            Try
                _rVal = dxfPrimatives.CreateChannel_End(New TVECTOR(aCenter), aTopWidth, aWebHeight, aThickness, aBottomWidth, aJoggleHeight, aSuppressFillets, aRotation, bFilletEnds, aSegmentWidth, aPlane, aReturnPolygon, aXOffset, aYOffset)
                xSetDisplay(_rVal, aImage:=img)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod().Name, ex, img)
                Return _rVal
            End Try
        End Function
        Public Function Channel_Side(aTopCenter As iVector, aLength As Double, aHeight As Double, aThickness As Double, Optional bBackSide As Boolean = False, Optional aRotation As Double = 0.0, Optional aSegmentWidth As Double = 0.0, Optional aPlane As dxfPlane = Nothing, Optional aReturnPolygon As Boolean = False, Optional aXOffset As Double = 0.0, Optional aYOffset As Double = 0.0) As dxfPolyline
            '#1the to center for the channel
            '#2the top flange width of the desired channel
            '#3the height of the desired channel
            '#4the thickness of the material
            '^returns as rectangular c channel
            Dim img As dxfImage = Nothing
            aPlane = GetPlane(aPlane, img)
            Dim v1 As New TVECTOR(aTopCenter)
            v1.X += aXOffset
            v1.Y += aYOffset
            Dim _rVal As dxfPolyline = Nothing
            Try
                _rVal = dxfPrimatives.CreateChannel_Side(v1, aLength, aHeight, aThickness, bBackSide, aRotation, aSegmentWidth, aPlane, aReturnPolygon)
                xSetDisplay(_rVal, aImage:=img)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod().Name, ex, img)
                Return _rVal
            End Try
            Return Nothing
        End Function
        Public Function Channel_Top(aLongCenter As iVector, aTopWidth As Double, aLength As Double, aThickness As Double, Optional aBottomWidth As Double = 0.0, Optional aRotation As Double = 0.0, Optional aSegmentWidth As Double = 0.0, Optional aPlane As dxfPlane = Nothing, Optional aReturnPolygon As Boolean = False, Optional aXOffset As Double = 0.0, Optional aYOffset As Double = 0.0) As dxfPolyline
            '#1the to center for the channel
            '#2the height of the desired channel
            '#3the flange width of the desired channel
            '#4the thickness of the material
            '^returns as rectangular c channel
            Dim img As dxfImage = Nothing
            aPlane = GetPlane(aPlane, img)
            Dim v1 As New TVECTOR(aLongCenter)

            v1.X += aXOffset
            v1.Y += aYOffset
            Dim _rVal As dxfPolyline = Nothing
            Try
                _rVal = dxfPrimatives.CreateChannel_Top(v1, aTopWidth, aLength, aThickness, aBottomWidth, aRotation, aSegmentWidth, aPlane, aReturnPolygon)
                xSetDisplay(_rVal, aImage:=img)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod().Name, ex, img)
                Return _rVal
            End Try
            Return Nothing
        End Function
        Public Function CircleP(aCenter As iVector, aRadius As Double, Optional aPlane As dxfPlane = Nothing, Optional bReturnPolygon As Boolean = False) As dxfPolyline
            '#1the center of the circle
            '#2the radius of the Circle
            '^returns a polygon that is a full cirlce with 4 arc segments
            Dim img As dxfImage = Nothing
            aPlane = GetPlane(aPlane, img)
            Try
                Dim _rVal As dxfPolyline = dxfPrimatives.CreateCircle(aCenter, aRadius, aPlane, bReturnPolygon)
                xSetDisplay(_rVal, aImage:=img)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod().Name, ex, img)
            End Try
            Return Nothing
        End Function
        Public Function CircleSection(aCenter As iVector, aRadius As Double, bReturnAsPolygon As Boolean, Optional aPlane As dxfPlane = Nothing, Optional aRotation As Double = 0.0, Optional aLeftEdge As Double? = Nothing, Optional aRightEdge As Double? = Nothing, Optional aTopEdge As Double? = Nothing, Optional aBottomEdge As Double? = Nothing, Optional sClipDistance As Double = 0.0) As dxfPolyline
            '#1the center of the circle
            '#2the outside radius of the circle
            '#3flag to return as polygon instead of a polyline
            '#4the coordinate system of definition
            '#5a rotation to apply
            '#6the left edge of the desired strip
            '#7the right edge of the desired strip
            '#8the top edge of the desired strip
            '#9the bottom edge of the desired strip
            '#10 a distance to clip off the sharp corners on moon section
            '#11flag indicating if a moon was clipped
            '^creates a polyline that is a strip section of the indicated circle trimmed at the top and bottom
            '~the circle is assumed centered at the origin of the passed coordinate system
            Dim img As dxfImage = Nothing
            aPlane = GetPlane(aPlane, img)
            Try
                Dim _rVal As dxfPolyline = dxfPrimatives.CreateCircleSection(aCenter, aRadius, bReturnAsPolygon, aPlane, aRotation, aLeftEdge, aRightEdge, aTopEdge, aBottomEdge, sClipDistance)
                xSetDisplay(_rVal, aImage:=img)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod().Name, ex, img)
            End Try
            Return Nothing
        End Function
        Public Function CircleSectionVertices(aCenter As iVector, aRadius As Double, Optional aPlane As dxfPlane = Nothing, Optional aRotation As Double = 0.0, Optional aLeftEdge As Double? = Nothing, Optional aRightEdge As Double? = Nothing, Optional aTopEdge As Double? = Nothing, Optional aBottomEdge As Double? = Nothing, Optional sClipDistance As Double = 0.0) As colDXFVectors
            Dim rClipped As Boolean = False
            Return CircleSectionVertices(aCenter, aRadius, aPlane, aRotation, aLeftEdge, aRightEdge, aTopEdge, aBottomEdge, sClipDistance, rClipped)
        End Function
        Public Function CircleSectionVertices(aCenter As iVector, aRadius As Double, aPlane As dxfPlane, aRotation As Double, aLeftEdge As Double?, aRightEdge As Double?, aTopEdge As Double?, aBottomEdge As Double?, sClipDistance As Double, ByRef rClipped As Boolean) As colDXFVectors
            '#1the center of the circle
            '#2the outside radius of the circle
            '#3the coordinate system of definition
            '#4a rotation to apply
            '#5the left edge of the desired strip
            '#6the right edge of the desired strip
            '#7the top edge of the desired strip
            '#8the bottom edge of the desired strip
            '#9 a distance to clip off the sharp corners on moon section
            '#10flag indicating if a moon was clipped
            '^creates polyline vertices that is a strip section of the indicated circle trimmed at the top and bottom
            '~the circle is assumed centered at the origin of the passed coordinate system
            Dim img As dxfImage = Nothing
            aPlane = GetPlane(aPlane, img)
            Dim _rVal As New colDXFVectors
            Dim moon As Boolean = False
            Try
                _rVal = dxfPrimatives.CreateCircleSectionVertices(aCenter, aRadius, aPlane, aRotation, aLeftEdge, aRightEdge, aTopEdge, aBottomEdge, sClipDistance, Nothing, rClipped, moon)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod().Name, ex, img)
                Return _rVal
            End Try
            Return Nothing
        End Function
        Public Function CircleStrip(aCenter As Object, aRadius As Double, bReturnAsPolygon As Boolean, Optional aPlane As dxfPlane = Nothing, Optional aRotation As Double = 0.0, Optional aLeftEdge As Double? = Nothing, Optional aRightEdge As Double? = Nothing, Optional sClipMoon As Double = 0.0, Optional bClipped As Boolean = False) As dxfPolyline
            '#1the center of the circle
            '#2the outside radius of the circle
            '#3flag to return as polygon instead of a polyline
            '#4the coordinate system of definition
            '#5a rotation to apply
            '#6the left edge of the desired strip
            '#7the right edge of the desired strip
            '#8 a distance to clip off the sharp corners on moon section
            '#9flag indicating if a moon was clipped
            '^creates a polyline that is a strip section of the indicated circle
            '~the circe is assumed centered at the origin of the passed coordinate system
            Dim img As dxfImage = Nothing
            aPlane = GetPlane(aPlane, img)
            Dim _rVal As dxfPolyline = Nothing
            Try
                _rVal = dxfPrimatives.CreateCircleStrip(aCenter, aRadius, bReturnAsPolygon, aPlane, aRotation, aLeftEdge, aRightEdge, sClipMoon, bClipped)
                xSetDisplay(_rVal, aImage:=img)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod().Name, ex, img)
                Return _rVal
            End Try
        End Function
        Public Function CreatePGorPL(bReturnPolygon As Boolean, aPlane As dxfPlane, bClosed As Boolean, aVertices As colDXFVectors, Optional aSegmentWidth As Double = 0.0, Optional aIdentifier As String = "", Optional aDisplaySettings As dxfDisplaySettings = Nothing) As dxfPolyline
            '#1flag to return the created polyline as a dxePolygon rather than a dexPolyline
            '#2the plane to assign to the polyline
            '#3value to assign to the polyline's closed property
            '#4the vertices to assign to the polyline
            '#5the segment width to asign to the new polyline
            '#6an Identifier to assign to the new polyline
            '#7the display vriables to assign to the new polyline
            '^creates and returns either a dxePolygon or a dxePolyine based on the pased input
            If aVertices Is Nothing Then Return Nothing
            If aVertices.Count <= 1 Then Return Nothing
            Dim img As dxfImage = Nothing
            aPlane = GetPlane(aPlane, img)
            Try
                Dim aPl As New TPLANE(aPlane)

                aPl.Origin = New TVECTORS(aVertices).PlanarCenter(aPl)
                Return dxfPrimatives.CreatePGorPL2(bReturnPolygon, aPl, bClosed, aVertices, aSegmentWidth, aIdentifier, aDisplaySettings)
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod().Name, ex, img)
            End Try
            Return Nothing
        End Function
        Public Function Donut(aCenter As iVector, bReturnAsPolygon As Boolean, aRadius As Double, Optional aStartAngle As Double = 0.0, Optional aEndAngle As Double = 360, Optional bClockwise As Boolean = False, Optional aPlane As dxfPlane = Nothing, Optional aWidth As Double = 0.0, Optional bClosed As Boolean = True, Optional aTrimToXAxis As Boolean = False, Optional aTrimNearestToYAxis As Boolean = False) As dxfEntity
            Dim _rVal As dxfEntity
            '#1the center for the new donut
            '#2flag to return a polygon instead of a polyline
            '#3the radius of the desired donut
            '#4the start angle
            '#5the end angle
            '#6clockwise flag
            '#7the coordinate system of definition
            '#8a width for the donut
            '#9flag to close the polyline
            '^returns a polyline that is the boundary of a circular donut section
            Dim img As dxfImage = Nothing
            aPlane = GetPlane(aPlane, img)
            Dim ctr As TVECTOR = TVECTOR.Zero
            If aCenter IsNot Nothing Then ctr = New TVECTOR(aCenter)
            aPlane.OriginV = ctr
            Try
                _rVal = dxfPrimatives.CreateDonut(aPlane.OriginV, bReturnAsPolygon, aRadius, aStartAngle, aEndAngle, bClockwise, aPlane, aWidth, bClosed, aTrimToXAxis, aTrimNearestToYAxis)
                xSetDisplay(_rVal, aImage:=img)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod().Name, ex, img)
            End Try
            Return Nothing
        End Function
        Public Function Ellipse(aCenter As iVector, bReturnAsPolygon As Boolean, aMajorAxis As Double, aMinorAxis As Double, Optional aRotation As Double = 0.0, Optional aStartAngle As Double = 0.0, Optional aEndAngle As Double = 360, Optional aSegmentWidth As Double = 0.0, Optional aPlane As dxfPlane = Nothing) As dxfPolyline
            '#1the center for the new ellipse
            '#2the major axis length of the desired ellipse
            '#3the minor axis length of the desired ellipse
            '#4a rotation to apply
            '#5a start angle to apply
            '#6a end angle to apply
            '#7a coordinate system to assign to the polygon
            '^returns a polygon with line segments approximating the desired ellipse
            Dim img As dxfImage = Nothing
            aPlane = GetPlane(aPlane, img)

            aPlane.OriginV = New TVECTOR(aCenter)
            Try
                Dim _rVal As dxfPolyline = dxfPrimatives.CreateEllipse(aPlane.OriginV, bReturnAsPolygon, aMajorAxis, aMinorAxis, aRotation, aStartAngle, aEndAngle, aSegmentWidth, aPlane)
                xSetDisplay(_rVal, aImage:=img)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod().Name, ex, img)
            End Try
            Return Nothing
        End Function
        Public Function FilletArc(aRadius As Double, aSP As iVector, aMP As iVector, aEP As iVector, Optional aPlane As dxfPlane = Nothing) As dxeArc
            '#1the radius for the fillet arc
            '#2the start point of the first line
            '#3the end point of the firs and the start of the second
            '#4the end point of the second line
            '#5returns the coordinate system that is defined based on the two lines
            '^ returns the arc of the requested radius that will fillet the lines defined by the passed points
            Dim img As dxfImage = Nothing
            aPlane = GetPlane(aPlane, img)
            Try
                Dim _rVal As dxeArc = dxfPrimatives.CreateFilletArc(aRadius, aSP, aMP, aEP, aPlane)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod().Name, ex, img)
            End Try
            Return Nothing
        End Function
        Public Function GridPoints(aBoundary As dxfEntity, aPitch As dxxPitchTypes, aHPitch As Double, aVPitch As Double, Optional aIslands As colDXFEntities = Nothing, Optional aXOffset As Double = 0.0, Optional aYOffset As Double = 0.0, Optional aRotation As Double = 0.0, Optional bAllowBoundaryPoints As Boolean = False, Optional aLineCollector As colDXFEntities = Nothing, Optional bSuppressPlaneCheck As Boolean = False, Optional aOriginCollector As dxfVector = Nothing) As colDXFVectors
            Try
                aOriginCollector = New dxfVector
                If aBoundary Is Nothing Then Return New colDXFVectors
                Dim gProps As New TGRID With {
                    .Plane = aBoundary.Plane.Strukture,
                    .HorizontalPitch = Math.Abs(aHPitch),
                    .VerticalPitch = Math.Abs(aVPitch),
                    .OffsetX = aXOffset,
                    .OffsetY = aYOffset,
                    .Rotation = TVALUES.NormAng(aRotation, ThreeSixtyEqZero:=True)
                 }
                If aPitch >= 0 And aPitch <= 2 Then gProps.PitchType = aPitch Else gProps.PitchType = dxxPitchTypes.Rectangular
                Return dxfPrimatives.CreateGridPoints(aBoundary, aIslands, gProps, bAllowBoundaryPoints, aLineCollector, bSuppressPlaneCheck, aOriginCollector)
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod().Name, ex)
            End Try
            Return Nothing
        End Function
        Public Function HexHeadProfile(aHeight As Double, aAcrossPoints As Double, aAcrossFlats As Double, Optional bLongView As Boolean = False, Optional aPlane As dxfPlane = Nothing) As dxePolyline
            Dim img As dxfImage = Nothing
            aPlane = GetPlane(aPlane, img)
            Try
                Dim _rVal As dxePolyline = dxfPrimatives.CreateHexHeadProfile(aHeight, aAcrossPoints, aAcrossFlats, bLongView, aPlane)
                xSetDisplay(_rVal, aImage:=img)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod().Name, ex, img)
            End Try
            Return Nothing
        End Function
        Public Function HexHeadPlan(aCenter As iVector, aAcrossFlats As Double, Optional aRotation As Double? = Nothing, Optional aPlane As dxfPlane = Nothing) As dxePolyline
            Dim img As dxfImage = Nothing
            aPlane = GetPlane(aPlane, img)
            Try
                Dim _rVal As dxePolyline = dxfPrimatives.CreateHexHeadPlan(aCenter, aAcrossFlats, aRotation, aPlane)
                xSetDisplay(_rVal, aImage:=img)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod().Name, ex, img)
            End Try
            Return Nothing
        End Function
        Public Function Pill(aCenter As iVector, aLength As Double, aHeight As Double, Optional aRotation As Double = 0.0, Optional aSegmentWidth As Double = 0.0, Optional aPlane As dxfPlane = Nothing, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional bReturnAsPolygon As Boolean = False, Optional bNoRadius As Boolean = False, Optional aXOffset As Double = 0.0, Optional aYOffset As Double = 0.0) As dxfPolyline
            Dim _rVal As dxfPolyline = Nothing
            Dim img As dxfImage = Nothing
            Try
                _rVal = dxfPrimatives.CreatePill(aCenter, aLength, aHeight, aRotation, aSegmentWidth, aPlane, aDisplaySettings, bReturnAsPolygon, bNoRadius, aXOffset, aYOffset)
                xSetDisplay(_rVal, aDisplaySettings:=aDisplaySettings)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod().Name, ex, img)
                Return _rVal
            End Try
            Return Nothing
        End Function
        ''' <summary>
        ''' ^returns a triangular 3 vertex closed polyline otherwise or a 3 vertext solid based on the passed input
        ''' </summary>
        ''' ''' <param name="aPoint">the tip of the triangular pointer</param>
        ''' <param name="aWidth">the base width of the triangular pointer</param>
        ''' <param name="aHeightFactor">the height factor of the triangular pointer. this times the base width determines the height of  the triangular pointer(default is 1 min is 0.1 max is 5)</param>
        ''' <param name="aRotation">a rotation to apply. if null, the rotation property of the point vector</param>
        ''' <param name="bReturnHollow">if True the function returns a 3 vertex closed polyline otherwise it returns a solid</param>
        ''' <param name="aPlane">a working plane</param>
        Public Function Pointer(aPoint As iVector, aWidth As Double, Optional aHeightFactor As Double = 0.8, Optional aRotation? As Double = Nothing, Optional bReturnHollow As Boolean = False, Optional aPlane As dxfPlane = Nothing, Optional aDisplaySettings As dxfDisplaySettings = Nothing) As dxfEntity

            Dim img As dxfImage = Nothing
            aPlane = GetPlane(aPlane, img)
            Dim _rVal As dxfEntity = Nothing
            Try
                If Not bReturnHollow Then
                    _rVal = dxfPrimatives.CreatePointer_Solid(aPoint, aWidth, aHeightFactor, aRotation, aPlane)
                Else
                    _rVal = dxfPrimatives.CreatePointer_Hollow(aPoint, aWidth, aHeightFactor, aRotation, aPlane)
                End If
                xSetDisplay(_rVal, aDisplaySettings:=aDisplaySettings, aImage:=img)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod().Name, ex, img)
                Return _rVal
            End Try
        End Function
        Public Function Polygon(aCenter As iVector, bReturnAsPolygon As Boolean, aSideCnt As Integer, aRadius As Double, Optional aRotation As Double? = Nothing, Optional bXScribed As Boolean = False, Optional aShearAngle As Double = 0.0, Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aPlane As dxfPlane = Nothing, Optional aSegmentWidth As Double = 0.0) As dxfPolyline
            '#1the center for the polygon
            '#2flag to return a polygon instead of a polyline
            '#3the number of sides (3 to 100)
            '#4the radius of the defining circle
            '#5a rotation to apply
            '#6flag idicating if the polygon should contain the circle or vice versa
            '#7a shear to apply
            '#8 a layer name
            '#9a color
            '#10a linetype
            '#11 the coordinate system of definition
            '#12a segment width
            '^returns a multisided polygon with the passed properties
            Dim img As dxfImage = Nothing

            Try
                Dim _rVal As dxfPolyline = dxfPrimatives.CreatePolygon(New dxfVector(aCenter), bReturnAsPolygon, aSideCnt, aRadius, aRotation, bXScribed, aShearAngle, aLayer, aColor, aLineType, aPlane, aSegmentWidth)
                xSetDisplay(_rVal, aLayer, aColor, aLineType, aImage:=img)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod().Name, ex, img)
            End Try
            Return Nothing
        End Function
        Public Function Rectangle(aCenterXY As iVector, Optional bReturnAsPolygon As Boolean = False, Optional aHeight As Double = 1, Optional aWidth As Double = 1, Optional aFillet As Double = 0.0, Optional ApplyFilletAsChamfer As Boolean = False, Optional aRotation As Double = 0.0, Optional aPlane As dxfPlane = Nothing, Optional aSegmentWidth As Double = 0.0, Optional aXOffset As Double = 0.0, Optional aYOffset As Double = 0.0) As dxfPolyline
            '#1the center for the new rectangle
            '#2flag to return a polygon instead of a polyline
            '#3the height of the desired rectangle
            '#4the width of the desired rectangle
            '#5a fillet radius to apply to the rectangles corners
            '#6flag to return a chamfered rectangle rather than a filleted one
            '^returns as rectangular polyline with desired dimensions centered at the passed point
            Dim img As dxfImage = Nothing
            aPlane = GetPlane(aPlane, img)
            Try
                Dim _rVal As dxfPolyline = dxfPrimatives.CreateRectanglularPerimeter(aCenterXY, bReturnAsPolygon, aHeight, aWidth, aFillet, ApplyFilletAsChamfer, aRotation, aPlane, aSegmentWidth, aXOffset, aYOffset)
                xSetDisplay(_rVal, aImage:=img)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod().Name, ex, img)
            End Try
            Return Nothing
        End Function
        Public Function SawtoothLine(aSP As iVector, aEP As iVector, aToothWd As Double, aToothHt As Double, Optional aShift As Double = 0.0, Optional aPlane As dxfPlane = Nothing) As dxePolyline
            Dim img As dxfImage = Nothing
            aPlane = GetPlane(aPlane, img)
            Try
                Dim _rVal As dxePolyline = dxfPrimatives.CreateSawtoothLine(New TVECTOR(aSP), New TVECTOR(aEP), aToothWd, aToothHt, aShift, aPlane)
                xSetDisplay(_rVal, aImage:=img)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod().Name, ex, img)
            End Try
            Return Nothing
        End Function
        Public Function Trace(aVectors As IEnumerable(Of iVector), aThickness As Double, Optional aClosed As Boolean = False, Optional aApplyFillets As Boolean = False, Optional aFilletEnds As Boolean = False, Optional aRotation As Double = 0.0, Optional aSegmentWidth As Double = 0.0, Optional aPlane As dxfPlane = Nothing, Optional aReturnPolygon As Boolean = False) As dxfPolyline
            Dim _rVal As dxfPolyline
            '#1the xy vectors to use for the trace pattern
            '#2the distance between the trace edges
            '#3flag to close the trace
            '#4flag to apply arc fillets to the corners of the trace
            '#5flag to round the ends of the trace (only valid if fillets are appplied and the trace is not closed)
            '#6a rotation to apply
            '#7the segment width for the trace edges
            '#8the plane for the trace
            '#9flag to return a dxePolygon rather than a dxePolyline
            '^creates a polyline trace along the path of the passed vectors.
            Dim img As dxfImage = Nothing
            aPlane = GetPlane(aPlane, img)
            Try
                _rVal = dxfPrimatives.CreateTrace(New TVERTICES(aVectors), aThickness, aClosed, aApplyFillets, aFilletEnds, aRotation, aSegmentWidth, aPlane, aReturnPolygon)
                xSetDisplay(_rVal, aImage:=img)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod().Name, ex, img)
            End Try
            Return Nothing
        End Function

        Private Function GetImage(Optional aImage As dxfImage = Nothing) As dxfImage
            If aImage IsNot Nothing Then Return aImage
            If String.IsNullOrWhiteSpace(_ImageGUID) Then Return Nothing Else Return dxfEvents.GetImage(_ImageGUID)
        End Function
        Private Function GetPlane(aPlane As dxfPlane, ByRef rImage As dxfImage) As dxfPlane
            Dim _rVal As dxfPlane = IIf(Not dxfPlane.IsNull(aPlane), aPlane, Nothing)
            rImage = GetImage(rImage)

            If rImage IsNot Nothing Then
                If _rVal Is Nothing Then _rVal = New dxfPlane(rImage.UCS)
                _rVal.ImageGUID = ""
            End If
            If _rVal Is Nothing Then _rVal = dxfPlane.World
            Return _rVal
        End Function
        Private Sub xSetDisplay(aEnt As dxfEntity, Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aImage As dxfImage = Nothing)
            aImage = GetImage(aImage)
            If aImage Is Nothing Or aEnt Is Nothing Then Return
            Dim aPl As dxePolyline
            aImage = dxfEvents.GetImage(_ImageGUID)
            aLayer = Trim(aLayer)
            aLineType = Trim(aLineType)
            If aDisplaySettings IsNot Nothing Then
                If aLayer = "" Then aLayer = aDisplaySettings.LayerName
                If aLineType = "" Then aLineType = aDisplaySettings.Linetype
                If aColor = dxxColors.Undefined Then aColor = aDisplaySettings.Color
            End If
            If aImage IsNot Nothing Then
                aDisplaySettings = aImage.GetDisplaySettingsNR(aEnt.EntityType, aLayerName:=aLayer, aColor:=aColor, aLineType:=aLineType)
                aEnt.DisplaySettings = aDisplaySettings
                If aEnt.GraphicType = dxxGraphicTypes.Polyline Then
                    aPl = aEnt
                    If aPl.SegmentWidth < 0 Then
                        aPl.SegmentWidth = aImage.Header.PolylineWidth
                    End If
                End If
            End If
        End Sub
        Private Sub xHandleError(aMethodName As String, e As Exception, Optional aImage As dxfImage = Nothing)
            If e Is Nothing Then Return
            aImage = GetImage(aImage)
            If aImage IsNot Nothing Then
                aImage.HandleError(aMethodName, Me.GetType().Name, e.Message)
            Else
                Debug.Fail($"{ e.GetType().Name }.{ aMethodName }.{ e.Message}")
            End If
        End Sub
#End Region 'Methods
#Region "Shared Methods"
        Public Shared Function CreateHexHeadPlan(aCenter As iVector, aAcrossFlats As Double, Optional aRotation As Double? = Nothing, Optional aPlane As dxfPlane = Nothing, Optional aDisplaySettings As dxfDisplaySettings = Nothing) As dxePolyline
            Dim _rVal As dxePolyline
            Dim plane As New TPLANE(aPlane, aCenter)
            Dim rad As Double = 0.5 * aAcrossFlats
            _rVal = dxfPrimatives.CreatePolygon(New dxfVector(plane.Origin), False, 6, 0.5 * aAcrossFlats, aRotation, True)
            _rVal.Identifier = "HexHead"
            _rVal.Closed = False
            _rVal.PlaneV = plane
            Dim v1 As TVECTOR = _rVal.Vertices.ItemVector(1)
            Dim v2 As TVECTOR = v1.Interpolate(_rVal.Vertices.ItemVector(2), 0.5)
            _rVal.Vertices.AddV(v1)
            _rVal.Vertices.AddV(v2, rad)
            v1 = v2
            v2 = _rVal.Vertices.ItemVector(2).Interpolate(_rVal.Vertices.ItemVector(3), 0.5)
            _rVal.Vertices.AddV(v2, rad)
            v2 = _rVal.Vertices.ItemVector(3).Interpolate(_rVal.Vertices.ItemVector(4), 0.5)
            _rVal.Vertices.AddV(v2, rad)
            v2 = _rVal.Vertices.ItemVector(4).Interpolate(_rVal.Vertices.ItemVector(5), 0.5)
            _rVal.Vertices.AddV(v2, rad)
            v2 = _rVal.Vertices.ItemVector(5).Interpolate(_rVal.Vertices.ItemVector(6), 0.5)
            _rVal.Vertices.AddV(v2, rad)
            v2 = _rVal.Vertices.ItemVector(6).Interpolate(_rVal.Vertices.ItemVector(1), 0.5)
            _rVal.Vertices.AddV(v2, rad)
            _rVal.Vertices.AddV(v1)
            _rVal.DisplaySettings = aDisplaySettings
            Return _rVal
        End Function
        Public Shared Function Create_Hatch_ByFillStyle(aFillStyle As dxxFillStyles, aEntity As dxfEntity, Optional aRotation As Double = 0.0, Optional aLineSpace As Double = 1, Optional aScaleFactor As Double = 0.0, Optional aDisplaySettings As dxfDisplaySettings = Nothing) As dxeHatch
            Dim _rVal As dxeHatch = Nothing

            '#1the entity to hatch
            '#2the angle of the hatch lines
            '#3the spacing between the hatch lines
            '#4a scale factor to apply to the hatch (0 means scale to current display)
            '#5flag indicating if the hatch is a solid
            '#6flag indicating if the hatch is a crossing pattern (a net)
            '#7a layer to put the entity on instead of the current layer
            '#8a color to apply to the entity instead of the current color setting
            '#9a line type to apply to the entity instead of the current line type setting
            '^used to add a hatch to the last drawn entity or a passed entity.
            '~if the entity is passed as nothing an attempt is made to hatch the last draw entity.
            '~if the entity does not have a defineable hatch region nothing is drawn
            Try
                Dim aEnt As dxfEntity = aEntity

                If aEnt Is Nothing Then Throw New Exception("No Hatchable Entity Found")
                If Not dxfEnums.Validate(GetType(dxxFillStyles), TVALUES.To_INT(aFillStyle), bSkipNegatives:=True) Then
                    Throw New Exception("Invalid Fill Style Passed")
                End If
                If aLineSpace <= 0 Then
                    aEnt.UpdatePath()
                    aLineSpace = 0.01 * aEnt.BoundingRectangle.Width
                End If
                If aLineSpace <= 0 Then Throw New Exception("Invalid Line Step Passed")
                aEnt.UpdatePath()
                Dim bCrvs As TBOUNDLOOPS = dxfHatches.BoundLoops_Entity(aEnt, aEnt.Plane.Strukture)
                If bCrvs.Count <= 0 Then Throw New Exception("Un-Hatchable Enity Passed")
                If aScaleFactor <= 0 Then
                    aScaleFactor = 1
                End If
                If aScaleFactor <= 0 Then aScaleFactor = 1
                _rVal = New dxeHatch With {
                    .PlaneV = aEnt.PlaneV,
                    .DisplayStructure = New TDISPLAYVARS(aDisplaySettings),
                    .HatchStyle = dxxHatchStyle.dxfHatchUserDefined,
                    .LineStep = aLineSpace * aScaleFactor
                }
                _rVal.BoundingEntities.Add(aEnt.CloneAll(Nothing, True))
                _rVal.Rotation = aRotation
                Select Case aFillStyle
                    Case dxxFillStyles.Cross
                        _rVal.Doubled = True
                    Case dxxFillStyles.DiagonalCross
                        _rVal.Doubled = True
                        aRotation += 45
                    Case dxxFillStyles.DownwardDiagonal
                        _rVal.Doubled = False
                        aRotation -= 45
                    Case dxxFillStyles.HorizontalLine
                        _rVal.Doubled = False
                    Case dxxFillStyles.Solid
                        _rVal.HatchStyle = dxxHatchStyle.dxfHatchSolidFill
                    Case dxxFillStyles.UpwardDiagonal
                        _rVal.Doubled = False
                        aRotation += 45
                    Case dxxFillStyles.VerticalLine
                        _rVal.Doubled = False
                        aRotation += 90
                End Select
                _rVal.Rotation = aRotation
                Return _rVal
            Catch ex As Exception
                Throw ex
                Return _rVal
            End Try
        End Function
        Public Shared Function CreateDot(aDotType As dxxDotShapes, aCenter As iVector, aRadius As Double, Optional aRotation As Double = 0, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aPlane As dxfPlane = Nothing) As List(Of dxfEntity)
            Dim _rVal As New List(Of dxfEntity)

            Dim border As dxfPolyline = Nothing
            Dim fill As dxeHatch = Nothing
            Select Case aDotType
                Case dxxDotShapes.Circle
                    border = dxfPrimatives.CreateCircularPerimeter(aCenter, aRadius, 20, aRotation, aPlane, aDisplaySettings)

                Case dxxDotShapes.Square
                    Dim w As Double = Math.Sqrt(Math.Pow(aRadius, 2) / 2) * 2
                    border = dxfPrimatives.CreateRectanglularPerimeter(aCenter, False, w, w, aPlane:=aPlane)
                    border.DisplaySettings = aDisplaySettings

            End Select

            If border IsNot Nothing Then
                _rVal.Add(border)
                fill = dxfPrimatives.Create_Hatch_ByFillStyle(dxxFillStyles.Solid, border, aDisplaySettings:=border.DisplaySettings)

                _rVal.Add(fill)
            End If
            Return _rVal

        End Function
        Public Shared Function CreateCircularPerimeter(aCenter As iVector, aRadius As Double, Optional aCurveDivisions As Integer = 20, Optional aRotation As Double? = Nothing, Optional aPlane As dxfPlane = Nothing, Optional aDisplaySettings As dxfDisplaySettings = Nothing) As dxePolyline

            aRadius = Math.Abs(aRadius)
            If aRadius = 0 Then Return Nothing


            Dim plane As TPLANE = New TPLANE(aPlane, aCenter)
            If aRotation.HasValue Then plane.Rotate(aRotation.Value)
            Dim arc As New TARC(plane, plane.Origin, aRadius)
            Return New dxePolyline(arc.PhantomPoints(aCurveDivisions), True, aDisplaySettings, New dxfPlane(plane))

        End Function

        ''' <summary>
        ''' returns the solid pointer entity defined by the input
        ''' </summary>
        ''' <param name="aPoint">the tip of the triangular pointer</param>
        ''' <param name="aWidth">the base width of the triangular pointer</param>
        ''' <param name="aHeightFactor">the height factor of the triangular pointer. this times the base width determines the height of  the triangular pointer(default is 1 min is 0.1 max is 5)</param>
        ''' <param name="aRotation">a rotation to apply. if null, the rotation property of the point vector</param>
        ''' <param name="aPlane">a working plane</param>
        ''' <returns></returns>
        Public Shared Function CreatePointer_Solid(aPoint As iVector, aWidth As Double, Optional aHeightFactor As Double = 1, Optional aRotation As Double? = Nothing, Optional aPlane As dxfPlane = Nothing) As dxeSolid

            If aWidth = 0 Then Return Nothing
            aWidth = Math.Abs(aWidth)
            Dim ht As Double = aWidth * dxfUtils.LimitedValue(aHeightFactor, 0.1, 5)
            Dim aPl As New TPLANE(aPlane, aPoint)
            Dim v1 As New TVERTEX(aPoint)
            Dim v2 As TVECTOR = v1.Vector + aPl.YDirection * ht
            v2 += aPl.XDirection * -0.5 * aWidth
            Dim v3 As TVECTOR = v2 + aPl.XDirection * aWidth
            Dim verts As New TVECTORS(v1.Vector, v2, v3)
            Dim rot = v1.Rotation
            If aRotation.HasValue Then rot = aRotation.Value

            If rot <> 0 Then verts.Rotate(aPl.Origin, aPl.ZDirection, rot)
            Return New dxeSolid(verts, New dxfPlane(aPl)) With
            {
                .Tag = v1.Tag,
                .Flag = v1.Flag,
                .Value = v1.Value
            }
        End Function

        ''' <summary>
        ''' returns the polyline pointer entity defined by the input
        ''' </summary>
        ''' <param name="aPoint">the tip of the triangular pointer</param>
        ''' <param name="aWidth">the base width of the triangular pointer</param>
        ''' <param name="aHeightFactor">the height factor of the triangular pointer. this times the base width determines the height of  the triangular pointer(default is 1 min is 0.1 max is 5)</param>
        ''' <param name="aRotation">a rotation to apply. if null, the rotation property of the point vector</param>
        ''' <param name="aPlane">a working plane</param>
        ''' <returns></returns>
        Public Shared Function CreatePointer_Hollow(aPoint As iVector, aWidth As Double, Optional aHeightFactor As Double = 1, Optional aRotation As Double? = Nothing, Optional aPlane As dxfPlane = Nothing) As dxePolyline
            If aWidth = 0 Then Return Nothing
            aWidth = Math.Abs(aWidth)
            Dim ht As Double = aWidth * dxfUtils.LimitedValue(aHeightFactor, 0.1, 5)
            Dim aPl As New TPLANE(aPlane, aPoint)
            Dim v1 As New TVERTEX(aPoint)
            Dim v2 As TVECTOR = v1.Vector + aPl.YDirection * ht
            v2 += aPl.XDirection * -0.5 * aWidth
            Dim v3 As TVECTOR = v2 + aPl.XDirection * aWidth
            Dim verts As New TVECTORS(v1.Vector, v2, v3)
            Dim rot = v1.Rotation
            If aRotation.HasValue Then rot = aRotation.Value

            If rot <> 0 Then verts.Rotate(aPl.Origin, aPl.ZDirection, rot)
            Return New dxePolyline(verts, True, aPlane:=New dxfPlane(aPl)) With
            {
                .Tag = v1.Tag,
                .Flag = v1.Flag,
                .Value = v1.Value
            }
        End Function
        Public Shared Function CreateCircleStrip(aCenter As iVector, aRadius As Double, aReturnPolygon As Boolean, Optional aPlane As dxfPlane = Nothing, Optional aRotation As Double = 0.0, Optional aLeftEdge As Double? = Nothing, Optional aRightEdge As Double? = Nothing, Optional sClipMoon As Double = 0.0, Optional bClipped As Boolean = False) As dxfPolyline
            Dim _rVal As dxfPolyline = Nothing
            '#1the center of the circle
            '#2the outside radius of the circle
            '#3flag to return as polygon instead of a polyline
            '#4the coordinate system of definition
            '#5a rotation ot apply
            '#6the left edge of the desired strip
            '#7the right edge of the desired strip
            '#8returns True if the returned section is a hemispher that is less than half the circle (a moon)
            '#9returns True if the returned section is the right side (positive X) of the circle trimmed on the left only
            '#10returns True if the returned section contains the center of the circle
            '#11a length to trim off the end of a moon
            '^creates a polyline that is a strip section of the indicated circle
            '~the circle is assumed centered at the origin of the passed coordinate system
            Try
                Dim aPl As TPLANE = New TPLANE(aPlane, aCenter)
                Dim bMoon As Boolean
                Dim aID As String = String.Empty
                Dim bPlane As New dxfPlane
                'set the system
                Dim PlineVerts As colDXFVectors = dxfPrimatives.CreateCircleSectionVertices(aCenter, aRadius, aPlane, aRotation, aLeftEdge, aRightEdge, Nothing, Nothing, sClipMoon, bPlane, bClipped, bMoon)
                If bMoon Then aID = "MOON"
                If bPlane IsNot Nothing Then aPl = New TPLANE(bPlane)
                _rVal = dxfPrimatives.CreatePGorPL2(aReturnPolygon, aPl, True, PlineVerts, aIdentifier:=aID)
                '_rVal.UpdatePath(True)
                'Dim aEnt As dxfEntity = _rVal
                Return _rVal
            Catch ex As Exception
                Throw ex
                Return _rVal
            End Try
        End Function
        Public Shared Function CreatePolygon(aCenter As iVector, aReturnPolygon As Boolean, aSideCnt As Integer, aRadius As Double, Optional aRotation As Double? = Nothing, Optional bXScribed As Boolean = False, Optional aShearAngle As Double = 0.0, Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aPlane As dxfPlane = Nothing, Optional aSegmentWidth As Double = 0.0) As dxfPolyline

            '#1the center for the polygon
            '#2flag to return a polygon instead of a polyline
            '#3the number of sides (3 to 100)
            '#4the radius of the defining circle
            '#5a rotation to apply
            '#6flag idicating if the polygon should contain the circle or vice versa
            '#7a shear to apply
            '#8 a layer name
            '#9a color
            '#10a linetype
            '#11the coordinate system of definition
            '#12a segment width
            '^returns a multisided polygon with the passed properties
            Dim aPl As TPLANE = New TPLANE(aPlane, aCenter)
            Dim verts As colDXFVectors = New colDXFVectors(dxfPrimatives.CreateVertices_Polygon(aPl.Origin, aPl, aSideCnt, aRadius, aRotation, bXScribed, aShearAngle))
            Dim _rVal As dxfPolyline = dxfPrimatives.CreatePGorPL2(aReturnPolygon, aPl, True, verts, aSegmentWidth)
            If _rVal IsNot Nothing Then
                _rVal.LCLSet(aLayer, aColor, aLineType)
            End If
            Return _rVal
        End Function
        Public Shared Function CreateRectanglularPerimeter(aCenter As iVector, Optional aReturnPolygon As Boolean = False, Optional aHeight As Double = 1, Optional aWidth As Double = 1, Optional aFillet As Double = 0.0, Optional bAppyFilletAtChamfer As Boolean = False, Optional aRotation As Double = 0.0, Optional aPlane As dxfPlane = Nothing, Optional aSegmentWidth As Double = 0.0, Optional aXOffset As Double = 0.0, Optional aYOffset As Double = 0.0) As dxfPolyline
            '#1the center for the new rectangle
            '#2flag to return a polygon instead of a polyline
            '#3the height of the desired rectangle
            '#4the width of the desired rectangle
            '#5a fillet radius to apply to the rectangles corners
            '#6flag to return a chamfered rectangle rather than a filleted one
            '^returns as rectangular polyline with desired dimensions centered at the passed point

            Dim aPl As New TPLANE(aPlane, aCenter)
            Dim verts As colDXFVectors = New colDXFVectors(dxfPrimatives.CreateVertices_Rectangle(aPl.Origin, aPl, aHeight, aWidth, aFillet, bAppyFilletAtChamfer, aRotation, aXOffset, aYOffset))
            Return dxfPrimatives.CreatePGorPL2(aReturnPolygon, aPl, True, verts, aSegmentWidth)
        End Function
        Friend Shared Function CreateSawtoothLine(aSP As TVECTOR, aEP As TVECTOR, aToothWd As Double, aToothHt As Double, Optional aShift As Double = 0.0, Optional aPlane As dxfPlane = Nothing) As dxePolyline
            Dim _rVal As New dxePolyline With {.Closed = False}
            Try
                Dim d1 As Double
                Dim wd As Double = Math.Abs(aToothWd)
                Dim ht As Double = Math.Abs(aToothHt)
                Dim xDir As TVECTOR = aSP.DirectionTo(aEP, False, rDistance:=d1)
                Dim ang As Double
                Dim bPlane As dxfPlane
                Dim v1 As TVECTOR
                Dim v2 As TVECTOR
                Dim v3 As TVECTOR
                Dim verts As New colDXFVectors
                Dim aLn As TLINE
                Dim bLn As TLINE
                Dim aPl As New TPLANE("")
                Dim f1 As Double
                Dim stps As Double
                Dim remain As Double
                Dim i As Integer
                Dim aFlg As Boolean
                Dim bFlg As Boolean
                Dim aTr As New TTRANSFORMS
                If dxfPlane.IsNull(aPlane) Then bPlane = New dxfPlane Else bPlane = aPlane
                bPlane.AlignXToV(xDir)
                If d1 <= 0 Or wd = 0 Then
                    _rVal.Vertices.Clear()
                    _rVal.Vertices.AddV(aSP)
                    _rVal.Vertices.AddV(aEP)
                Else
                    If aToothHt = 0 Then aToothHt = wd
                    If aToothHt < 0 Then f1 = -1 Else f1 = 1
                    stps = d1 / wd
                    remain = stps - Fix(stps)
                    stps -= remain
                    remain = Math.Round(remain, 3)
                    v1 = New TVECTOR
                    For i = 1 To stps
                        v2 = v1 + aPl.XDirection * (wd / 2)
                        v2 += aPl.YDirection * (ht * f1)
                        If aShift <> 0 Then v2 += aPl.XDirection * aShift
                        v3 = v1 + aPl.XDirection * wd
                        If i = 1 Then verts.AddV(v1)
                        verts.AddV(v2)
                        verts.AddV(v3)
                        v1 = v3
                    Next i
                    If remain > 0 Then
                        aLn = New TLINE With {
                            .SPT = aPl.Origin + aPl.XDirection * d1
                        }
                        aLn.EPT = aLn.SPT + aPl.YDirection * (-2 * d1)
                        aLn.SPT = aLn.EPT + aPl.YDirection * (4 * d1)
                        v2 = v1 + aPl.XDirection * (wd / 2)
                        v2 += aPl.YDirection * (ht * f1)
                        If aShift <> 0 Then v2 += aPl.XDirection * aShift
                        v3 = v1 + aPl.XDirection * wd
                        If stps = 0 Then verts.AddV(v1)
                        bLn = New TLINE(v1, v2)
                        Dim rInterceptExists As Boolean = False
                        Dim rLinesAreParallel As Boolean = False
                        Dim rLinesAreCoincident As Boolean = False
                        v1 = aLn.IntersectionPt(bLn, rLinesAreParallel, rLinesAreCoincident, rIsOnFirstLine:=aFlg, rIsOnSecondLine:=bFlg, rInterceptExists:=rInterceptExists)
                        If aFlg Then
                            verts.AddV(v1)
                        Else
                            verts.AddV(v2)
                            bLn = New TLINE(v2, v3)
                            v1 = aLn.IntersectionPt(bLn, rLinesAreParallel, rLinesAreCoincident, rIsOnFirstLine:=aFlg, rIsOnSecondLine:=bFlg, rInterceptExists:=rInterceptExists)
                            verts.AddV(v1)
                        End If
                    End If
                End If
                aTr = New TTRANSFORMS()
                If Not TVECTOR.IsNull(aSP) Then
                    aTr.Add(TTRANSFORM.CreateTranslation(aSP, True))
                End If
                If Not aPl.XDirection.Equals(xDir, 3) Then
                    v1 = aPl.XDirection.CrossProduct(xDir)
                    ang = aPl.XDirection.AngleTo(xDir, v1)
                    aTr.Add(TTRANSFORM.CreateRotation(aSP, ang, False, v1, True))
                End If
                verts.Transform(aTr, True)
                If Not aEP.IsEqualTo(verts.LastVector, 3) Then
                    verts.AddV(aEP)
                End If
                _rVal.Plane = bPlane
                _rVal.Vertices = verts
                Return _rVal
            Catch ex As Exception
                Throw ex
                Return _rVal
            End Try
        End Function
        Public Shared Function CreateTrace(aVectors As IEnumerable(Of iVector), aThickness As Double, Optional aClosed As Boolean = False, Optional aApplyFillets As Boolean = False, Optional aFilletEnds As Boolean = False, Optional aRotation As Double = 0.0, Optional aSegmentWidth As Double = 0.0, Optional aPlane As dxfPlane = Nothing, Optional aReturnPolygon As Boolean = False) As dxfPolyline
            If aVectors Is Nothing Then Return Nothing
            If aVectors.Count < 2 Then Return Nothing
            '#1the xy vectors to use for the trace pattern
            '#2the distance between the trace edges
            '#3flag to close the trace
            '#4flag to apply arc fillets to the corners of the trace
            '#5flag to round the ends of the trace (only valid if fillets are appplied and the trace is not closed)
            '#6a rotation to apply
            '#7the segment width for the trace edges
            '#8the plane for the trace
            '#9flag to return a dxePolygon rather than a dxePolyline
            '^creates a polyline trace along the path of the passed vectors.
            Return CreateTrace(New TVERTICES(aVectors), aThickness, aClosed, aApplyFillets, aFilletEnds, aRotation, aSegmentWidth, aPlane, aReturnPolygon)
        End Function
        Friend Shared Function CreateTrace(aVectors As TVERTICES, aThickness As Double, Optional aClosed As Boolean = False, Optional aApplyFillets As Boolean = False, Optional aFilletEnds As Boolean = False, Optional aRotation As Double = 0.0, Optional aSegmentWidth As Double = 0.0, Optional aPlane As dxfPlane = Nothing, Optional aReturnPolygon As Boolean = False) As dxfPolyline
            Dim _rVal As dxfPolyline
            '#1the xy vectors to use for the trace pattern
            '#2the distance between the trace edges
            '#3flag to close the trace
            '#4flag to apply arc fillets to the corners of the trace
            '#5flag to round the ends of the trace (only valid if fillets are appplied and the trace is not closed)
            '#6a rotation to apply
            '#7the segment width for the trace edges
            '#8the plane for the trace
            '#9flag to return a dxePolygon rather than a dxePolyline
            '^creates a polyline trace along the path of the passed vectors.
            Dim tcnt As Integer
            If dxfPlane.IsNull(aPlane) Then aPlane = New dxfPlane
            Dim verts As TVERTICES = dxfPrimatives.CreateVertices_Trace(aVectors, aThickness, aClosed, aApplyFillets, aFilletEnds, aRotation, aPlane, tcnt)
            _rVal = dxfPrimatives.CreatePGorPL2(aReturnPolygon, aPlane.Strukture, Not aClosed, CType(verts, colDXFVectors), aSegmentWidth)
            '_rVal.InsertionPt = _rVal.Vertex(1)
            If aClosed And tcnt > 1 And aReturnPolygon Then
                _rVal.Vertex(verts.Count).Linetype = dxfLinetypes.Invisible
            End If
            Return _rVal
        End Function
        Public Shared Function CreateHexHeadProfile(aHeight As Double, aAcrossPoints As Double, aAcrossFlats As Double, Optional bLongView As Boolean = False, Optional aPlane As dxfPlane = Nothing) As dxePolyline
            Dim _rVal As New dxePolyline With {
                .Plane = aPlane,
                .Closed = True,
                .Identifier = "HexHead"
                }
            If bLongView Then
                _rVal.Vertices = dxfPrimatives.CreateVertices_HexHeadProfile3Sides(aHeight, aAcrossPoints, aAcrossFlats, aPlane)
            Else
                _rVal.Vertices = dxfPrimatives.CreateVertices_HexHeadProfile2Sides(aHeight, aAcrossPoints, aAcrossFlats, aPlane)
            End If
            Return _rVal
        End Function
        Public Shared Function CreatePill(aCenter As iVector, aLength As Double, aHeight As Double, Optional aRotation As Double = 0.0, Optional aSegmentWidth As Double = 0.0, Optional aPlane As dxfPlane = Nothing, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional bReturnAsPolygon As Boolean = False, Optional bNoRadius As Boolean = False, Optional aXOffset As Double = 0.0, Optional aYOffset As Double = 0.0) As dxfPolyline
            Dim _rVal As dxfPolyline = Nothing
            Try
                Dim aPl As TPLANE = New TPLANE(aPlane, aCenter)
                Dim verts As colDXFVectors = New colDXFVectors(dxfPrimatives.CreateVertices_Pill(aPl.Origin, aPl, aLength, aHeight, aRotation, bNoRadius, aXOffset, aYOffset))
                _rVal = dxfPrimatives.CreatePGorPL2(bReturnAsPolygon, aPl, True, verts, aSegmentWidth, aDisplaySettings:=aDisplaySettings)
            Catch ex As Exception
                Throw ex

            End Try
            Return _rVal
        End Function
        Friend Shared Function CreateVertices_Pill(aCenter As TVECTOR, aPlane As TPLANE, aLength As Double, aHeight As Double, Optional aRotation As Double = 0.0, Optional bNoRadius As Boolean = False, Optional aXOffset As Double = 0.0, Optional aYOffset As Double = 0.0) As TVERTICES
            Dim _rVal As TVERTICES
            Dim rad As Double
            Dim alen As Double
            Dim bPlane As New TPLANE(aPlane)
            Dim d1 As Double
            Dim d2 As Double
            Dim verts As New TVERTICES
            Dim arad As Double
            rad = 0.5 * Math.Abs(aHeight)

            alen = Math.Abs(aLength)
            If alen < 2 * rad Then alen = 2 * rad
            bPlane.Origin = aCenter
            If aRotation <> 0 Then bPlane.Revolve(aRotation, False)
            bPlane.Origin = bPlane.Vector(aXOffset, aYOffset)
            d1 = 0.5 * alen
            d2 = d1 - rad
            arad = rad
            If bNoRadius Then arad = 0
            If Math.Round(alen, 6) > Math.Round(2 * rad, 6) Then
                verts = New TVERTICES(bPlane.Vertex(-d2, rad, 0, arad))
                verts.Add(bPlane.Vertex(-d1, 0, 0, arad))
                verts.Add(bPlane.Vertex(-d2, -rad))
                verts.Add(bPlane.Vertex(d2, -rad, 0, arad))
                verts.Add(bPlane.Vertex(d1, 0, 0, arad))
                verts.Add(bPlane.Vertex(d2, rad))
            Else
                verts = New TVERTICES(bPlane.Vertex(0, rad, 0, arad))
                verts.Add(bPlane.Vertex(-rad, 0, 0, arad))
                verts.Add(bPlane.Vertex(0, -rad, 0, arad))
                verts.Add(bPlane.Vertex(rad, 0, 0, arad))
            End If
            _rVal = verts
            Return _rVal
        End Function
        Public Shared Function CreateCircleSectionVertices(aCenter As iVector, aRadius As Double, Optional aPlane As dxfPlane = Nothing, Optional aRotation As Double = 0.0, Optional aLeftEdge As Double? = Nothing, Optional aRightEdge As Double? = Nothing, Optional aTopEdge As Double? = Nothing, Optional aBottomEdge As Double? = Nothing, Optional sClipMoon As Double = 0.0, Optional aPlaneCollector As dxfPlane = Nothing) As colDXFVectors
            Dim rClipped As Boolean = False
            Dim rMoon As Boolean = False
            Return CreateCircleSectionVertices(aCenter, aRadius, aPlane, aRotation, aLeftEdge, aRightEdge, aTopEdge, aBottomEdge, sClipMoon, aPlaneCollector, rClipped, rMoon)
        End Function
        Public Shared Function CreateCircleSectionVertices(aCenter As iVector, aRadius As Double, aPlane As dxfPlane, aRotation As Double, aLeftEdge As Double?, aRightEdge As Double?, aTopEdge As Double?, aBottomEdge As Double?, sClipMoon As Double, aPlaneCollector As dxfPlane, ByRef rClipped As Boolean, ByRef rMoon As Boolean) As colDXFVectors
            Dim _rVal As New colDXFVectors
            '#1the center of the circle
            '#2the outside radius of the circle
            '#3flag to return as polygon instead of a polyline
            '#4the coordinate system of definition
            '#5a rotation to apply
            '#6the left edge of the desired section
            '#7the right edge of the desired section
            '#8the top edge of the desired section
            '#9the bottom edge of the desired section
            '#11a length to trim off the end of a moon
            '#12returns True if the section vertices represents a moon section of the circle
            '^creates the vertices of a polyline that is a section of the indicated circle
            '~the circle is assumed centered at the origin of the passed coordinate system
            rMoon = False
            rClipped = False
            Try
                Dim rad As Double = Math.Abs(aRadius)
                Dim limL As Double = -2 * rad
                Dim limR As Double = 2 * rad
                Dim LimT As Double = 2 * rad
                Dim limB As Double = -2 * rad
                Dim sClip As Double = Math.Abs(sClipMoon)
                Dim aPl As New TPLANE("")
                Dim v1 As TVECTOR
                Dim v2 As TVECTOR
                Dim verts As New TVERTICES
                Dim aVert As New TVERTEX
                Dim iCrn As Integer = 0
                Dim sCrn As String = ","
                Dim i As Integer
                Dim d1 As Double
                Dim bStradY As Boolean
                Dim bStradX As Boolean
                Dim trec As dxfRectangle = Nothing



                If rad = 0 Then Throw New Exception("Zero Radius Passed")
                If aLeftEdge.HasValue Then limL = aLeftEdge.Value
                If aRightEdge.HasValue Then limR = aRightEdge.Value


                'make sure left is less than right etc.
                TVALUES.SortTwoValues(True, limL, limR)
                If Math.Round(limL, 5) <= -Math.Round(rad, 5) Then limL = -2 * rad
                If Math.Round(limR, 5) >= Math.Round(rad, 5) Then limR = 2 * rad
                If Math.Round(limL, 5) = 0 Then limL = 0
                If Math.Round(limR, 5) = 0 Then limR = 0
                If aBottomEdge.HasValue Then limB = aBottomEdge.Value
                If aTopEdge.HasValue Then LimT = aTopEdge.Value
                'make sure bottom is less than top etc.
                TVALUES.SortTwoValues(True, limB, LimT)
                If Math.Round(limB, 5) <= -Math.Round(rad, 5) Then limB = -2 * rad
                If Math.Round(LimT, 5) >= Math.Round(rad, 5) Then LimT = 2 * rad
                If Math.Round(limB, 5) = 0 Then limB = 0
                If Math.Round(LimT, 5) = 0 Then LimT = 0
                Dim UL As New TVERTEX(limL, LimT, 0, aTag:="UL")
                Dim LL As New TVERTEX(limL, limB, 0, aTag:="LL")
                Dim UR As New TVERTEX(limR, LimT, 0, aTag:="UR")
                Dim LR As New TVERTEX(limR, limB, 0, aTag:="LR")
                v1 = TVECTOR.Zero
                UL.Value = v1.DistanceTo(UL.Vector)
                UR.Value = v1.DistanceTo(UR.Vector)
                LL.Value = v1.DistanceTo(LL.Vector)
                LR.Value = v1.DistanceTo(LR.Vector)
                Dim Q1 As New TVERTEX(0, aRadius, 0, aRadius)
                Dim Q2 As New TVERTEX(-aRadius, 0, 0, aRadius)
                Dim Q3 As New TVERTEX(0, -aRadius, 0, aRadius)
                Dim Q4 As New TVERTEX(aRadius, 0, 0, aRadius)
                Dim plane As New TPLANE("")
                Dim arc As New TARC(plane, plane.Origin, aRadius)
                Dim vrt As TVERTEX
                Dim inside As New TVERTICES(0)
                Dim vrts As TVERTICES = arc.RectangleIntersections(aLeftEdge, aRightEdge, aTopEdge, aBottomEdge, bReturnInteriorCorners:=True, bReturnQuadrantPt:=True, rInsinside:=inside, rTrimRectangle:=trec)
                iCrn = inside.Count
                For i = 1 To inside.Count
                    vrt = inside.Item(i)
                    If vrt.Tag = "UL" Then
                        sCrn += "UL,"
                    ElseIf vrt.Tag = "LL" Then
                        sCrn += "LL,"
                    ElseIf vrt.Tag = "UR" Then
                        sCrn += "UR,"
                    ElseIf vrt.Tag = "LR" Then
                        sCrn += "LR,"
                    End If
                Next
                If iCrn = 0 Then sCrn += ","
                bStradY = limL < -0.01 And limR > 0.01
                bStradX = limB < -0.01 And LimT > 0.01
                Select Case iCrn
         '===============================================================================
                    Case 4 'internal rectangle
                        '===============================================================================
                        verts = inside
         '===============================================================================
                    Case 0 'whole circle or a strip
                        '===============================================================================
                        '-----------------------------------------------------------------------------------------------------
                        'whole circle
                        '-----------------------------------------------------------------------------------------------------
                        If (limB <= -rad And LimT >= rad) And (limL <= -rad And limR >= rad) Then
                            verts.Add(Q1)
                            verts.Add(Q2)
                            verts.Add(Q3)
                            verts.Add(Q4)
                        End If
                        '-----------------------------------------------------------------------------------------------------
                        'vertical strip section
                        '-----------------------------------------------------------------------------------------------------
                        If (limB <= -rad And LimT >= rad) And (limL > -rad And limR < rad) Then
                            If limL = 0 Then v1 = New TVECTOR(limL, aRadius, 0) Else v1 = New TVECTOR(limL, Math.Sqrt(aRadius ^ 2 - limL ^ 2), 0)
                            If limR = 0 Then v2 = New TVECTOR(limR, aRadius, 0) Else v2 = New TVECTOR(limR, Math.Sqrt(aRadius ^ 2 - limR ^ 2), 0)
                            verts.Add(New TVERTEX(v1.X, v1.Y))
                            verts.Add(New TVERTEX(v1.X, -v1.Y, aVertexRadius:=aRadius))
                            If bStradY Then verts.Add(Q3)
                            verts.Add(New TVERTEX(v2.X, -v2.Y))
                            verts.Add(New TVERTEX(v2.X, v2.Y, aVertexRadius:=aRadius))
                            If bStradY Then verts.Add(Q1)
                        End If
                        '-----------------------------------------------------------------------------------------------------
                        'horizontal strip section
                        '-----------------------------------------------------------------------------------------------------
                        If (limL <= -rad And limR >= rad) And (limB > -rad And LimT < rad) Then
                            If limB = 0 Then v1 = New TVECTOR(aRadius, limB, 0) Else v1 = New TVECTOR(Math.Sqrt(aRadius ^ 2 - limB ^ 2), limB, 0)
                            If LimT = 0 Then v2 = New TVECTOR(aRadius, LimT, 0) Else v2 = New TVECTOR(Math.Sqrt(aRadius ^ 2 - LimT ^ 2), LimT, 0)
                            verts.Add(New TVERTEX(-v2.X, v2.Y, aVertexRadius:=aRadius))
                            If LimT > 0 And limB < 0 Then verts.Add(Q2)
                            verts.Add(New TVERTEX(-v1.X, v1.Y))
                            verts.Add(New TVERTEX(v1.X, v1.Y, aVertexRadius:=aRadius))
                            If LimT > 0 And limB < 0 Then verts.Add(Q4)
                            verts.Add(New TVERTEX(v2.X, v2.Y))
                        End If
                        '-----------------------------------------------------------------------------------------------------
                        If LimT > rad And limB > -rad And limB < rad Then
                            'top side moon
                            '-----------------------------------------------------------------------------------------------------
                            If limB = 0 Then v1 = New TVECTOR(aRadius, limB, 0) Else v1 = New TVECTOR(Math.Sqrt(aRadius ^ 2 - limB ^ 2), limB, 0)
                            rMoon = (limB >= 0)
                            rClipped = sClip > 0 And limB > 0 And sClip <= 0.9 * v1.X
                            If rClipped Then v1.X -= sClip
                            If rMoon Then 'semi-circle or less
                                If rClipped Then
                                    d1 = Math.Sqrt(aRadius ^ 2 - v1.X ^ 2)
                                    verts.Add(New TVERTEX(-v1.X, d1))
                                    verts.Add(New TVERTEX(-v1.X, limB))
                                    verts.Add(New TVERTEX(v1.X, limB))
                                    verts.Add(New TVERTEX(v1.X, d1, aVertexRadius:=aRadius))
                                    verts.Add(Q1)
                                Else
                                    verts.Add(New TVERTEX(-v1.X, limB))
                                    verts.Add(New TVERTEX(v1.X, limB, aVertexRadius:=aRadius))
                                    verts.Add(Q1)
                                End If
                            Else
                                'more than half
                                verts.Add(New TVERTEX(-v1.X, limB))
                                verts.Add(New TVERTEX(v1.X, limB, aVertexRadius:=aRadius))
                                verts.Add(Q4)
                                verts.Add(Q1)
                                verts.Add(Q2)
                            End If
                        End If
                        '-----------------------------------------------------------------------------------------------------
                        'bottom side moon
                        '-----------------------------------------------------------------------------------------------------
                        If limB < -rad And LimT > -rad And LimT < rad Then
                            rMoon = (LimT <= 0)
                            If LimT = 0 Then v1 = New TVECTOR(aRadius, LimT, 0) Else v1 = New TVECTOR(Math.Sqrt(aRadius ^ 2 - LimT ^ 2), LimT, 0)
                            rClipped = sClip > 0 And LimT < 0 And sClip <= 0.9 * v1.X
                            If rClipped Then v1.X -= sClip
                            If rMoon Then 'semi-circle or less
                                If rClipped Then
                                    d1 = -Math.Sqrt(aRadius ^ 2 - v1.X ^ 2)
                                    verts.Add(New TVERTEX(-v1.X, LimT))
                                    verts.Add(New TVERTEX(-v1.X, d1, aVertexRadius:=aRadius))
                                    verts.Add(Q3)
                                    verts.Add(New TVERTEX(v1.X, d1))
                                    verts.Add(New TVERTEX(v1.X, LimT))
                                Else
                                    verts.Add(New TVERTEX(-v1.X, LimT, aVertexRadius:=aRadius))
                                    verts.Add(Q3)
                                    verts.Add(New TVERTEX(v1.X, LimT))
                                End If
                            Else
                                'more than half
                                verts.Add(New TVERTEX(-v1.X, LimT, aVertexRadius:=aRadius))
                                verts.Add(Q2)
                                verts.Add(Q3)
                                verts.Add(Q4)
                                verts.Add(New TVERTEX(v1.X, LimT))
                            End If
                        End If
                        '-----------------------------------------------------------------------------------------------------
                        'right side moon
                        '-----------------------------------------------------------------------------------------------------
                        If (limB <= -rad And LimT >= rad) And limR > rad And limL > -rad And limL < rad Then
                            If limL = 0 Then v1 = New TVECTOR(limL, aRadius, 0) Else v1 = New TVECTOR(limL, Math.Sqrt(aRadius ^ 2 - limL ^ 2), 0)
                            rMoon = (limL >= 0)
                            rClipped = sClip > 0 And limL > 0 And sClip <= 0.9 * v1.Y
                            If rClipped Then v1.Y -= sClip
                            If rMoon Then 'semi-circle or less
                                If rClipped Then
                                    d1 = Math.Sqrt(aRadius ^ 2 - v1.Y ^ 2)
                                    verts.Add(New TVERTEX(limL, v1.Y))
                                    verts.Add(New TVERTEX(limL, -v1.Y))
                                    verts.Add(New TVERTEX(d1, -v1.Y, aVertexRadius:=aRadius))
                                    verts.Add(Q4)
                                    verts.Add(New TVERTEX(d1, v1.Y))
                                Else
                                    verts.Add(New TVERTEX(limL, v1.Y))
                                    verts.Add(New TVERTEX(limL, -v1.Y, aVertexRadius:=aRadius))
                                    verts.Add(Q4)
                                End If
                            Else
                                'more than half
                                verts.Add(New TVERTEX(limL, v1.Y))
                                verts.Add(New TVERTEX(limL, -v1.Y, aVertexRadius:=aRadius))
                                verts.Add(Q3)
                                verts.Add(Q4)
                                verts.Add(Q1)
                            End If
                        End If
                        '-----------------------------------------------------------------------------------------------------
                        'left side moon
                        '-----------------------------------------------------------------------------------------------------
                        If (limB <= -rad And LimT >= rad) And limL < -rad And limR > -rad And limR < rad Then
                            rMoon = (limR <= 0)
                            If limR = 0 Then v1 = New TVECTOR(limR, aRadius, 0) Else v1 = New TVECTOR(limR, Math.Sqrt(aRadius ^ 2 - limR ^ 2), 0)
                            rClipped = sClip > 0 And limR < 0 And sClip <= 0.9 * v1.Y
                            If rClipped Then v1.Y -= sClip
                            If rMoon Then 'semi-circle or less
                                If rClipped Then
                                    d1 = -Math.Sqrt(aRadius ^ 2 - v1.Y ^ 2)
                                    verts.Add(New TVERTEX(limR, v1.Y))
                                    verts.Add(New TVERTEX(d1, v1.Y, aVertexRadius:=aRadius))
                                    verts.Add(Q2)
                                    verts.Add(New TVERTEX(d1, -v1.Y))
                                    verts.Add(New TVERTEX(limR, -v1.Y))
                                Else
                                    verts.Add(New TVERTEX(limR, v1.Y, aVertexRadius:=aRadius))
                                    verts.Add(Q2)
                                    verts.Add(New TVERTEX(limR, -v1.Y))
                                End If
                            Else
                                'more than half
                                verts.Add(New TVERTEX(limR, v1.Y, aVertexRadius:=aRadius))
                                verts.Add(Q1)
                                verts.Add(Q2)
                                verts.Add(Q3)
                                verts.Add(New TVERTEX(limR, -v1.Y))
                            End If
                        End If
         '===============================================================================
                    Case 1 'single corner within circle
                        '===============================================================================
                        Select Case sCrn
             '-----------------------------------------------------------------------------------------------------
                            Case ",UL,"
                                '-----------------------------------------------------------------------------------------------------
                                verts.Add(UL)
                                aVert = New TVERTEX(limL, -Math.Sqrt(aRadius ^ 2 - limL ^ 2), aVertexRadius:=aRadius)
                                verts.Add(aVert)
                                If limL < 0 Then verts.Add(Q3)
                                If LimT > 0 Then verts.Add(Q4)
                                aVert = New TVERTEX(Math.Sqrt(aRadius ^ 2 - LimT ^ 2), LimT)
                                If sClip > 0 And LimT < 0 Then
                                    If aVert.X - sClip > UL.X Then
                                        aVert.X -= sClip
                                        aVert.Y = -Math.Sqrt(aRadius ^ 2 - aVert.X ^ 2)
                                        verts.Add(aVert)
                                        aVert.Y = UL.Y
                                    End If
                                End If
                                verts.Add(aVert)
             '-----------------------------------------------------------------------------------------------------
                            Case ",LL,"
                                '-----------------------------------------------------------------------------------------------------
                                verts.Add(New TVERTEX(limL, Math.Sqrt(aRadius ^ 2 - limL ^ 2)))
                                verts.Add(LL)
                                aVert = New TVERTEX(Math.Sqrt(aRadius ^ 2 - limB ^ 2), limB, aVertexRadius:=aRadius)
                                If sClip > 0 And limB > 0 Then
                                    If aVert.X - sClip > LL.X Then
                                        verts.Add(New TVERTEX(aVert.X - sClip, LL.Y))
                                        aVert.X -= sClip
                                        aVert.Y = Math.Sqrt(aRadius ^ 2 - aVert.X ^ 2)
                                        rClipped = True
                                    End If
                                End If
                                verts.Add(aVert)
                                If LimT < 0 Then verts.Add(Q4)
                                If limL < 0 Then verts.Add(Q1)
             '-----------------------------------------------------------------------------------------------------
                            Case ",LR,"
                                '-----------------------------------------------------------------------------------------------------
                                verts.Add(New TVERTEX(limR, Math.Sqrt(aRadius ^ 2 - limR ^ 2), aVertexRadius:=aRadius))
                                If limR > 0.1 Then verts.Add(Q1)
                                If limB < -0.1 Then verts.Add(Q2)
                                aVert = New TVERTEX(-Math.Sqrt(aRadius ^ 2 - limB ^ 2), limB)
                                If sClip > 0 And limB > 0 Then
                                    If aVert.X + sClip < LR.X Then
                                        verts.Add(New TVERTEX(aVert.X + sClip, Math.Sqrt(aRadius ^ 2 - (aVert.X + sClip) ^ 2)))
                                        aVert.X += sClip
                                        rClipped = True
                                    End If
                                End If
                                verts.Add(aVert)
                                verts.Add(LR)
             '-----------------------------------------------------------------------------------------------------
                            Case ",UR,"
                                '-----------------------------------------------------------------------------------------------------
                                aVert = New TVERTEX(-Math.Sqrt(aRadius ^ 2 - LimT ^ 2), LimT, aVertexRadius:=aRadius)
                                If sClip > 0 And LimT < 0 Then
                                    If aVert.X + sClip < UR.X Then
                                        verts.Add(New TVERTEX(aVert.X + sClip, UR.Y))
                                        aVert.X += sClip
                                        aVert.Y = -Math.Sqrt(aRadius ^ 2 - aVert.X ^ 2)
                                        rClipped = True
                                    End If
                                End If
                                verts.Add(aVert)
                                If LimT > 0 Then verts.Add(Q2)
                                If limR > 0 Then verts.Add(Q3)
                                verts.Add(New TVERTEX(limR, -Math.Sqrt(aRadius ^ 2 - limR ^ 2)))
                                verts.Add(UR)
                        End Select
         '===============================================================================
                    Case 2 'two corners within circle
                        '===============================================================================
                        Select Case sCrn
             '-----------------------------------------------------------------------------------------------------
                            Case ",UL,LL,"
                                '-----------------------------------------------------------------------------------------------------
                                verts.Add(UL)
                                verts.Add(LL)
                                verts.Add(New TVERTEX(Math.Sqrt(aRadius ^ 2 - limB ^ 2), limB, aVertexRadius:=aRadius))
                                If limR < aRadius And (bStradX) Then
                                    'clipped corners
                                    verts.Add(New TVERTEX(limR, -Math.Sqrt(aRadius ^ 2 - limR ^ 2)))
                                    verts.Add(New TVERTEX(limR, Math.Sqrt(aRadius ^ 2 - limR ^ 2), aVertexRadius:=aRadius))
                                Else
                                    If bStradX Then verts.Add(Q4)
                                End If
                                verts.Add(New TVERTEX(Math.Sqrt(aRadius ^ 2 - LimT ^ 2), LimT))
             '-----------------------------------------------------------------------------------------------------
                            Case ",LR,UR,"
                                '-----------------------------------------------------------------------------------------------------
                                verts.Add(UR)
                                verts.Add(New TVERTEX(-Math.Sqrt(aRadius ^ 2 - LimT ^ 2), LimT, aVertexRadius:=aRadius))
                                If limL > -aRadius And (bStradX) Then
                                    'clipped corners
                                    verts.Add(New TVERTEX(limL, Math.Sqrt(aRadius ^ 2 - limL ^ 2)))
                                    verts.Add(New TVERTEX(limL, -Math.Sqrt(aRadius ^ 2 - limL ^ 2), aVertexRadius:=aRadius))
                                Else
                                    If bStradX Then verts.Add(Q2)
                                End If
                                verts.Add(New TVERTEX(-Math.Sqrt(aRadius ^ 2 - limB ^ 2), limB))
                                verts.Add(LR)
             '-----------------------------------------------------------------------------------------------------
                            Case ",LL,LR,"
                                '-----------------------------------------------------------------------------------------------------
                                verts.Add(New TVERTEX(limL, Math.Sqrt(aRadius ^ 2 - limL ^ 2)))
                                verts.Add(LL)
                                verts.Add(LR)
                                aVert = New TVERTEX(limR, Math.Sqrt(aRadius ^ 2 - limR ^ 2), aVertexRadius:=aRadius)
                                verts.Add(aVert)
                                If LimT < aRadius And (bStradY) Then
                                    'clipped corners
                                    verts.Add(New TVERTEX(Math.Sqrt(aRadius ^ 2 - LimT ^ 2), LimT))
                                    verts.Add(New TVERTEX(-Math.Sqrt(aRadius ^ 2 - LimT ^ 2), LimT, aVertexRadius:=aRadius))
                                Else
                                    If bStradY Then verts.Add(Q1)
                                End If
             '-----------------------------------------------------------------------------------------------------
                            Case ",UL,UR,"
                                '-----------------------------------------------------------------------------------------------------
                                verts.Add(UL)
                                verts.Add(New TVERTEX(limL, -Math.Sqrt(aRadius ^ 2 - limL ^ 2), aVertexRadius:=aRadius))
                                If limB > -aRadius And (bStradY) Then
                                    'clipped corners
                                    verts.Add(New TVERTEX(-Math.Sqrt(aRadius ^ 2 - limB ^ 2), limB))
                                    verts.Add(New TVERTEX(Math.Sqrt(aRadius ^ 2 - limB ^ 2), limB, aVertexRadius:=aRadius))
                                Else
                                    If bStradY Then verts.Add(Q3)
                                End If
                                verts.Add(New TVERTEX(limR, -Math.Sqrt(aRadius ^ 2 - limR ^ 2)))
                                verts.Add(UR)
                        End Select
         '===============================================================================
                    Case 3 'three corners within circle
                        '===============================================================================
                        Select Case sCrn
             '-----------------------------------------------------------------------------------------------------
                            Case ",UL,LL,UR,"
                                '-----------------------------------------------------------------------------------------------------
                                verts.Add(UL)
                                verts.Add(LL)
                                verts.Add(New TVERTEX(Math.Sqrt(aRadius ^ 2 - limB ^ 2), limB, aVertexRadius:=aRadius))
                                verts.Add(New TVERTEX(limR, -Math.Sqrt(aRadius ^ 2 - limR ^ 2)))
                                verts.Add(UR)
             '-----------------------------------------------------------------------------------------------------
                            Case ",UL,LL,LR,"
                                '-----------------------------------------------------------------------------------------------------
                                verts.Add(UL)
                                verts.Add(LL)
                                verts.Add(LR)
                                verts.Add(New TVERTEX(limR, Math.Sqrt(aRadius ^ 2 - limR ^ 2), aVertexRadius:=aRadius))
                                verts.Add(New TVERTEX(Math.Sqrt(aRadius ^ 2 - LimT ^ 2), LimT))
             '-----------------------------------------------------------------------------------------------------
                            Case ",LL,LR,UR,"
                                '-----------------------------------------------------------------------------------------------------
                                verts.Add(New TVERTEX(limL, Math.Sqrt(aRadius ^ 2 - limL ^ 2)))
                                verts.Add(LL)
                                verts.Add(LR)
                                verts.Add(UR)
                                verts.Add(New TVERTEX(-Math.Sqrt(aRadius ^ 2 - LimT ^ 2), LimT, aVertexRadius:=aRadius))
             '-----------------------------------------------------------------------------------------------------
                            Case ",UL,LR,UR,"
                                '-----------------------------------------------------------------------------------------------------
                                verts.Add(UL)
                                verts.Add(New TVERTEX(limL, -Math.Sqrt(aRadius ^ 2 - limL ^ 2), aVertexRadius:=aRadius))
                                verts.Add(New TVERTEX(-Math.Sqrt(aRadius ^ 2 - limB ^ 2), limB))
                                verts.Add(LR)
                                verts.Add(UR)
                        End Select
                End Select
                'apply the rotation
                If aRotation <> 0 Then
                    v1 = TVECTOR.WorldZ
                    v2 = TVECTOR.Zero
                    For i = 1 To verts.Count
                        aVert = verts.Item(i)
                        aVert.Vector.RotateAbout(v2, v1, aRotation, False)
                        verts.SetItem(i, aVert)
                    Next i
                End If
                're center or aplly plane
                If Not dxfPlane.IsNull(aPlane) Or aCenter IsNot Nothing Then
                    If Not dxfPlane.IsNull(aPlane) Then aPl = New TPLANE(aPlane)
                    If aCenter IsNot Nothing Then aPl.Origin = New TVECTOR(aCenter)
                    For i = 1 To verts.Count
                        aVert = verts.Item(i)
                        aVert.Vector = aPl.Vector(aVert.X, aVert.Y)
                        verts.SetItem(i, aVert)
                    Next i
                End If
                If aPlaneCollector IsNot Nothing Then aPlaneCollector.Strukture = CType(verts, TVECTORS).Bounds(aPl)
                Return New colDXFVectors(verts)
            Catch ex As Exception
                Throw New Exception("[CircleSectionVertices] " & ex.Message)
                Return _rVal
            End Try
        End Function
        Public Shared Function CreateCircleSegments(aRadius As Double,
                                      Optional aLeftLim As Double? = Nothing,
                                      Optional aRightLim As Double? = Nothing,
                                      Optional aTopLim As Double? = Nothing,
                                      Optional aBottomLim As Double? = Nothing,
                                      Optional aPlane As dxfPlane = Nothing,
                                      Optional aDisplaySettings As dxfDisplaySettings = Nothing,
                                      Optional bReturnInteriors As Boolean = True,
                                      Optional bReturnExteriors As Boolean = True) As List(Of dxeArc)
            Dim _rVal As New List(Of dxeArc)
            Try
                If Not bReturnInteriors And Not bReturnExteriors Then Return _rVal
                Dim rad As Double = Math.Round(Math.Abs(aRadius), 8)
                If rad = 0 Then Return _rVal
                Dim dArc As New dxeArc
                Dim plane As TPLANE = TPLANE.World
                If Not dxfPlane.IsNull(aPlane) Then
                    If aPlane.IsDefined Then
                        plane = New TPLANE(aPlane).Clone
                    End If
                End If
                Dim arc As New TARC(plane, plane.Origin, rad)
                Dim inside As New TVERTICES(0)
                Dim trect As dxfRectangle = Nothing
                Dim vrts As TVERTICES = arc.RectangleIntersections(aLeftLim, aRightLim, aTopLim, aBottomLim, True, False, inside, trect)
                'the rectangle contains the full circle so return it
                If vrts.Count <= 0 And inside.Count <= 0 Then
                    dArc = New dxeArc(New dxfPlane(plane), rad, 0, 360, aDisplaySettings) With {
                        .Name = "FULL CIRCLE"
                    }
                    _rVal.Add(dArc)
                Else
                    If vrts.Count <= 1 Then Return _rVal

                    Dim ep As TVERTEX
                    Dim bounds As dxfRectangle = plane.ToRectangle()
                    For i As Integer = 1 To vrts.Count
                        Dim sp As TVERTEX = vrts.Item(i)
                        If i < vrts.Count Then ep = vrts.Item(i + 1) Else ep = vrts.Item(1)
                        arc = TARC.DefineWithPoints(plane, plane.Origin, sp.Vector, ep.Vector, False, True)
                        dArc = New dxeArc(arc, aDisplaySettings) With {.Name = $"SEGMENT {i}", .Radius = Math.Abs(aRadius)}
                        If Math.Round(sp.X, 8) = Math.Round(ep.X, 8) And Math.Round(sp.Y, 8) = Math.Round(ep.Y, 8) Then
                            Continue For
                        End If
                        Dim keep As Boolean = True
                        If Not bReturnInteriors Or Not bReturnExteriors Then
                            Dim mpt As dxfVector = dArc.MidPt

                            If Not bReturnInteriors Then
                                keep = Not trect.ContainsVector(mpt)
                            Else
                                keep = trect.ContainsVector(mpt)
                            End If

                        End If
                        If keep Then
                            _rVal.Add(dArc)
                        End If
                    Next
                End If
                Return _rVal
            Catch ex As Exception
                Throw ex
            End Try
        End Function
        Public Shared Function CreateCircleStrip(aCenter As iVector, aRadius As Double, bReturnAsPolygon As Boolean, Optional aPlane As dxfPlane = Nothing, Optional aRotation As Double = 0.0, Optional aLeftEdge As Double? = Nothing, Optional aRightEdge As Double? = Nothing) As dxfPolyline
            Dim sClipMoon As Double = 0.0
            Return CreateCircleStrip(aCenter, aRadius, bReturnAsPolygon, aPlane, aRotation, aLeftEdge, aRightEdge, sClipMoon)
        End Function
        Public Shared Function CreateCircleStrip(aCenter As iVector, aRadius As Double, bReturnAsPolygon As Boolean, aPlane As dxfPlane, aRotation As Double, aLeftEdge As Double?, aRightEdge As Double?, ByRef rClipMoon As Double) As dxfPolyline
            '#1the center of the circle
            '#2the outside radius of the circle
            '#3flag to return as polygon instead of a polyline
            '#4the coordinate system of definition
            '#5a rotation to apply
            '#6the left edge of the desired strip
            '#7the right edge of the desired strip
            '#8 a distance to clip off the sharp corners on moon section
            '#9flag indicating if a moon was clipped
            '^creates a polyline that is a strip section of the indicated circle
            '~the circe is assumed centered at the origin of the passed coordinate system
            Try
                Return dxfPrimatives.CreateCircleSection(aCenter, aRadius, bReturnAsPolygon, aPlane, aRotation, aLeftEdge, aRightEdge, Nothing, Nothing, rClipDistance:=rClipMoon)
            Catch ex As Exception
                Throw ex
            End Try
            Return Nothing
        End Function
        Public Shared Function CreateCircleSection(aCenter As iVector, aRadius As Double, aReturnPolygon As Boolean, Optional aPlane As dxfPlane = Nothing, Optional aRotation As Double = 0.0, Optional aLeftEdge As Double? = Nothing, Optional aRightEdge As Double? = Nothing, Optional aTopEdge As Double? = Nothing, Optional aBottomEdge As Double? = Nothing) As dxfPolyline
            Dim rClipDistance As Double = 0.0
            Return CreateCircleSection(aCenter, aRadius, aReturnPolygon, aPlane, aRotation, aLeftEdge, aRightEdge, aTopEdge, aBottomEdge, rClipDistance)
        End Function
        Public Shared Function CreateCircleSection(aCenter As iVector, aRadius As Double, aReturnPolygon As Boolean, aPlane As dxfPlane, aRotation As Double, aLeftEdge As Double?, aRightEdge As Double?, aTopEdge As Double?, aBottomEdge As Double?, ByRef rClipDistance As Double) As dxfPolyline
            Dim _rVal As dxfPolyline
            Dim bClipped As Boolean = False
            '#1the center of the circle
            '#2the outside radius of the circle
            '#3flag to return as polygon instead of a polyline
            '#4the coordinate system of definition
            '#5a rotation to apply
            '#6the left edge of the desired strip
            '#7the right edge of the desired strip
            '#8the top edge of the desired strip
            '#9the bottom edge of the desired strip
            '#10 a distance to clip off the sharp corners on moon section
            '#11flag indicating if a moon was clipped
            '^creates a polyline that is a strip section of the indicated circle trimmed at the top and bottom
            '~the circle is assumed centered at the origin of the passed coordinate system
            Try
                Dim aPl As TPLANE
                Dim bPlane As dxfPlane = Nothing
                Dim PlineVerts As colDXFVectors
                Dim bMoon As Boolean
                Dim aID As String = String.Empty
                'set the system
                If dxfPlane.IsNull(aPlane) Then aPl = TPLANE.World Else aPl = New TPLANE(aPlane)
                If aCenter IsNot Nothing Then aPl.Origin = New TVECTOR(aCenter)
                PlineVerts = dxfPrimatives.CreateCircleSectionVertices(aCenter, aRadius, aPlane, aRotation, aLeftEdge, aRightEdge, aTopEdge, aBottomEdge, rClipDistance, bPlane, bClipped, bMoon)
                If bMoon Then aID = "MOON"
                If bPlane IsNot Nothing Then aPl = New TPLANE(bPlane)
                _rVal = dxfPrimatives.CreatePGorPL2(aReturnPolygon, aPl, True, PlineVerts, aIdentifier:=aID)
                If aReturnPolygon Then
                    _rVal.InsertionPt = CType(aPl.Origin, dxfVector)
                End If
                Return _rVal
            Catch ex As Exception
                Throw New Exception("[dxfPrimative.CircleSection] " & ex.Message)
                Return _rVal
            End Try
        End Function
        Public Shared Function CreateCircle(aCenter As iVector, aRadius As Double, Optional aPlane As dxfPlane = Nothing, Optional aReturnPolygon As Boolean = False, Optional aSegmentWidth As Double = 0.0, Optional aDisplaySettings As dxfDisplaySettings = Nothing) As dxfPolyline
            Dim _rVal As dxfPolyline
            '#1the center of the circle
            '#2the radius of the Circle
            '^returns a polyline that is a full circle with 4 arc segments
            Dim rad As Double = Math.Abs(aRadius)
            If rad = 0 Then rad = 1
            Dim verts As colDXFVectors
            Dim aPl As New TPLANE(aPlane, aCenter)

            verts = New colDXFVectors(dxfPrimatives.CreateVertices_Circle(aPl.Origin, aPl, aRadius))
            _rVal = dxfPrimatives.CreatePGorPL2(aReturnPolygon, aPl, False, verts, aSegmentWidth, aDisplaySettings:=aDisplaySettings)
            Return _rVal
        End Function

        Public Shared Function CreateEllipse(aEllipse As dxeEllipse, Optional aCurveDivisions As Integer = 20) As dxePolyline
            '^returns the ellipse converted to a polyline with line segments
            If aEllipse Is Nothing Then Return Nothing
            Dim _rVal As New dxePolyline With {
                .SuppressEvents = True,
                .DisplayStructure = aEllipse.DisplayStructure,
                .PlaneV = aEllipse.PlaneV,
                .Closed = False,
                .Vertices = aEllipse.PhantomPoints(aCurveDivisions)
            }
            _rVal.SuppressEvents = False
            Return _rVal
        End Function
        Friend Shared Function CreateArc(aArc As dxeArc, Optional bAsLines As Boolean = False, Optional aSegmentCount As Integer = 20) As dxePolyline
            '^returns the arc converted to a polyline
            If aArc Is Nothing Then Return Nothing

            aArc.UpdatePath()
            Dim subArcs As TSEGMENTS
            Dim sArc As TARC
            Dim i As Integer
            Dim v1 As New TVERTEX
            Dim lVerts As New TVERTICES
            Dim bOneWidth As Boolean
            Dim bHasWd As Boolean
            bHasWd = aArc.HasWidth
            bOneWidth = aArc.StartWidth = aArc.EndWidth
            If bOneWidth Then
                v1.StartWidth = aArc.StartWidth
                v1.EndWidth = v1.StartWidth
            End If
            Dim _rVal As New dxePolyline With
            {
            .DisplayStructure = aArc.DisplayStructure(),
            .Instances = aArc.Instances,
            .GroupName = aArc.GroupName,
            .PlaneV = aArc.PlaneV
            }
            If bAsLines Then
                If aSegmentCount < 10 Then aSegmentCount = 10
                _rVal.Vertices = aArc.PhantomPoints(aSegmentCount, 1)
                If bOneWidth Then _rVal.SegmentWidth = aArc.StartWidth
            Else
                If aArc.SpannedAngle <= 90 Then
                    v1.Vector = aArc.StartPtV
                    v1.Radius = aArc.Radius
                    v1.Inverted = aArc.ClockWise
                    If bHasWd And Not bOneWidth Then
                        v1.StartWidth = aArc.StartWidth
                        v1.EndWidth = aArc.EndWidth
                    End If
                    lVerts.Add(v1)
                    v1.Vector = aArc.EndPtV
                    v1.StartWidth = 0
                    v1.EndWidth = 0
                    lVerts.Add(v1)
                Else
                    subArcs = dxfUtils.ArcDivide(aArc.ArcStructure, 90)
                    For i = 1 To subArcs.Count
                        sArc = subArcs.Item(i).ArcStructure
                        v1.Vector = sArc.StartPt
                        v1.Radius = aArc.Radius
                        v1.Inverted = sArc.ClockWise
                        If bHasWd And Not bOneWidth Then
                            v1.StartWidth = sArc.StartWidth
                            v1.EndWidth = sArc.EndWidth
                        End If
                        lVerts.Add(v1)
                        If i = subArcs.Count Then
                            v1.Vector = sArc.EndPt
                            If bHasWd And Not bOneWidth Then
                                v1.StartWidth = sArc.EndWidth
                                v1.EndWidth = v1.StartWidth
                            End If
                            lVerts.Add(v1)
                        End If
                    Next i
                End If
                _rVal.VerticesV = lVerts
            End If
            Return _rVal
        End Function
        Friend Shared Function CreatePGorPL2(aReturnPolygon As Boolean, aPlane As TPLANE, bClosed As Boolean, aVertices As IEnumerable(Of iVector), Optional aSegmentWidth As Double = 0.0, Optional aIdentifier As String = "", Optional aDisplaySettings As dxfDisplaySettings = Nothing) As dxfPolyline
            '#1flag to return the created polyline as a dxePolygon rather than a dexPolyline
            '#2the plane to assign to the polyline
            '#3value to assign to the polyline's closed property
            '#4the vertices to assign to the polyline
            '#5the segment width to asign to the new polyline
            '#6an Identifier to assign to the new polyline
            '#7the display vriables to assign to the new polyline
            '^creates and returns either a dxePolygon or a dxePolyine based on the pased input
            If aReturnPolygon Then
                Dim aRec As TPLANE = New TVECTORS(aVertices).BoundingRectangle(aPlane, bSuppressProjection:=True)
                Dim aPg As New dxePolygon(aVertices, New dxfVector(aRec.Origin), bClosed, aDisplaySettings:=aDisplaySettings, aPlane:=New dxfPlane(aRec), aSegWidth:=aSegmentWidth) With {.Identifier = aIdentifier}
                Return aPg
            Else
                Dim aP As New dxePolyline(aVertices, bClosed, aDisplaySettings, New dxfPlane(aPlane), aSegWidth:=aSegmentWidth) With {.Identifier = aIdentifier}
                Return aP
            End If
        End Function
        Friend Shared Function CreateHoleVertices(aRadius As Double, aMinorRadius As Double, aLength As Double, bSquare As Boolean, aRotation As Double, aPlane As TPLANE, Optional aXOffset As Double = 0.0, Optional aYOffset As Double = 0.0, Optional aZOffset As Double = 0.0, Optional aMirrorLine As dxeLine = Nothing) As TVERTICES
            Dim _rVal As New TVERTICES(0)
            '^returns the vertices that define the bounds of a hole on its plane
            aRadius = Math.Abs(aRadius)
            If aRadius <= 0 Then Return _rVal
            Dim aPl As TPLANE = aPlane
            Dim oset As New TVECTOR(aXOffset, aYOffset, aZOffset)
            Dim org As TVECTOR = aPl.Origin + oset
            Dim rad As Double = aRadius
            Dim mr As Double = Math.Abs(aMinorRadius)
            If mr >= rad Then mr = 0
            Dim dia As Double = 2 * rad
            Dim lng As Double = aLength
            Dim l1 As Double
            Dim ang As Double
            If lng <> dia Then mr = 0
            If aMirrorLine IsNot Nothing Then
                If aMirrorLine.Length <> 0 Then org.Mirror(New TLINE(aMirrorLine), bSuppressCheck:=True)
            End If
            aPl.Origin = org
            aPl.Revolve(aRotation, False)
            l1 = 0.5 * lng - rad
            If bSquare Then
                'square hole
                _rVal.Add(aPl.Vector(-0.5 * lng, rad))
                _rVal.Add(aPl.Vector(-0.5 * lng, -rad))
                _rVal.Add(aPl.Vector(0.5 * lng, -rad))
                _rVal.Add(aPl.Vector(0.5 * lng, rad))
            Else
                If mr = 0 Then
                    If lng = dia Then
                        'round hole
                        _rVal.Add(aPl.Vector(-rad, 0), rad)
                        _rVal.Add(aPl.Vector(0, -rad), rad)
                        _rVal.Add(aPl.Vector(rad, 0), rad)
                        _rVal.Add(aPl.Vector(0, rad), rad)
                    Else
                        'round end slot
                        _rVal.Add(aPl.Vector(-l1, rad), rad)
                        _rVal.Add(aPl.Vector(-0.5 * lng, 0), rad)
                        _rVal.Add(aPl.Vector(-l1, -rad))
                        _rVal.Add(aPl.Vector(l1, -rad), rad)
                        _rVal.Add(aPl.Vector(0.5 * lng, 0), rad)
                        _rVal.Add(aPl.Vector(l1, rad))
                    End If
                Else
                    ang = Math.Atan(Math.Sqrt(rad ^ 2 - mr ^ 2) / mr) * 180 / Math.PI
                    _rVal.Add(aPl.AngleVector(ang, rad, False), rad)
                    _rVal.Add(aPl.Vector(0, rad), rad)
                    _rVal.Add(aPl.Vector(-rad, 0), rad)
                    _rVal.Add(aPl.Vector(0, -rad), rad)
                    _rVal.Add(aPl.AngleVector(-ang, rad, False))
                End If
            End If
            Return _rVal
        End Function
        Public Shared Function CreateVertices_HexHeadProfile3Sides(aHeight As Double, aAcrossPoints As Double, aAcrossFlats As Double, Optional aPlane As dxfPlane = Nothing) As colDXFVectors
            Dim _rVal As New colDXFVectors
            Dim aPl As TPLANE
            If Not dxfPlane.IsNull(aPlane) Then aPl = New TPLANE(aPlane) Else aPl = TPLANE.World
            If aAcrossFlats = 0 Or aAcrossPoints = 0 Then Return _rVal
            Dim aFC As Double = (0.5 * aAcrossFlats) / Math.Cos(30 * Math.PI / 180)
            Dim x1 As Double = 0.5 * Math.Abs(aAcrossPoints)
            Dim x2 As Double = Math.Abs(0.5 * aFC)
            Dim Y1 As Double = Math.Abs(aHeight)
            Dim d1 As Double = (x1 - x2)
            Dim rad As Double = 0.4 * d1
            If rad >= Y1 Then rad = 0.25 * Y1
            Dim Y2 As Double = Y1 - rad
            _rVal.AddV(aPl.Vector(x1, 0))
            _rVal.AddV(aPl.Vector(x1, Y2 + rad / 3))
            _rVal.AddV(aPl.Vector(x1 - rad, Y1))
            _rVal.AddV(aPl.Vector(x2 + rad, Y1), aVertexRadius:=rad)
            _rVal.AddV(aPl.Vector(x2, Y2))
            _rVal.AddV(aPl.Vector(x2, 0))
            _rVal.AddV(aPl.Vector(x2, Y2), aVertexRadius:=rad)
            _rVal.AddV(aPl.Vector(x2 - rad, Y1))
            _rVal.AddV(aPl.Vector(x2 + rad, Y1))
            _rVal.AddV(aPl.Vector(-x2 + rad, Y1), aVertexRadius:=rad)
            _rVal.AddV(aPl.Vector(-x2, Y2))
            _rVal.AddV(aPl.Vector(-x2, 0))
            _rVal.AddV(aPl.Vector(-x2, Y2), aVertexRadius:=rad)
            _rVal.AddV(aPl.Vector(-x2 - rad, Y1))
            _rVal.AddV(aPl.Vector(-x2 + rad, Y1))
            _rVal.AddV(aPl.Vector(-x1 + rad, Y1))
            _rVal.AddV(aPl.Vector(-x1, Y2 + rad / 3))
            _rVal.AddV(aPl.Vector(-x1, 0))
            Return _rVal
        End Function
        Public Shared Function CreateVertices_HexHeadProfile2Sides(aHeight As Double, aAcrossPoints As Double, aAcrossFlats As Double, Optional aPlane As dxfPlane = Nothing) As colDXFVectors
            Dim _rVal As New colDXFVectors
            If aAcrossFlats = 0 Or aAcrossPoints = 0 Then Return _rVal
            Dim aPl As TPLANE
            If Not dxfPlane.IsNull(aPlane) Then aPl = New TPLANE(aPlane) Else aPl = TPLANE.World
            Dim aFC As Double = (0.5 * aAcrossFlats) / Math.Cos(30 * Math.PI / 180)
            Dim x1 As Double = 0.5 * Math.Abs(aAcrossFlats)
            Dim x2 As Double = 0
            Dim Y1 As Double = Math.Abs(aHeight)
            Dim d1 As Double = (x1 - x2)
            Dim rad As Double = 0.3 * d1
            If rad >= Y1 Then rad = 0.25 * Y1
            Dim Y2 As Double = Y1 - rad
            _rVal.AddV(aPl.Vector(x1, 0))
            _rVal.AddV(aPl.Vector(x1, Y2 + rad / 3))
            _rVal.AddV(aPl.Vector(x1 - rad, Y1))
            _rVal.AddV(aPl.Vector(x2 + rad, Y1), aVertexRadius:=rad)
            _rVal.AddV(aPl.Vector(x2, Y2))
            _rVal.AddV(aPl.Vector(x2, 0))
            _rVal.AddV(aPl.Vector(x2, Y2), aVertexRadius:=rad)
            _rVal.AddV(aPl.Vector(x2 - rad, Y1))
            _rVal.AddV(aPl.Vector(x2 + rad, Y1))
            _rVal.AddV(aPl.Vector(-x1 + rad, Y1))
            _rVal.AddV(aPl.Vector(-x1, Y2 + rad / 3))
            _rVal.AddV(aPl.Vector(-x1, 0))
            Return _rVal
        End Function
        Friend Shared Function CreateHoleSegments(aHole As dxeHole, Optional bSuppressDepth As Boolean = False) As TSEGMENTS
            '#1the basis of the hole to build the three D wireframe of the hole for
            '^returns the three D edges of the passed hole
            'On Error Resume Next
            Dim _rVal As New TSEGMENTS(0)
            If aHole Is Nothing Then Return _rVal
            Dim i As Integer
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim dsp As TDISPLAYVARS = aHole.DisplayStructure
            Dim tverts As New TVERTICES
            Dim bverts As New TVERTICES

            Dim midPl As TPLANE = aHole.PlaneV
            Dim aDir As TVECTOR = midPl.ZDirection
            Dim rotPl As New TPLANE(midPl)
            Dim aCirc As New TARC
            Dim aSeg As New TSEGMENT
            Dim bCircs As Boolean
            Dim dpth As Double = Math.Round(aHole.Depth, 6)
            Dim eGUID As String = aHole.GUID
            Dim rad As Double = aHole.Radius

            If aHole.Rotation <> 0 Then
                rotPl.Revolve(aHole.Rotation, False)
            End If
            Dim botPl As New TPLANE(rotPl)
            Dim topPl As New TPLANE(rotPl)

            If dpth > 0 And Not bSuppressDepth Then
                botPl.Origin += botPl.ZDirection * (-0.5 * dpth)
                topPl.Origin += topPl.ZDirection * (0.5 * dpth)
            End If
            bCircs = (Math.Round(Math.Abs((2 * rad) - aHole.Length), 3) = 0)
            If bCircs And (aHole.MinorRadius > 0 And aHole.MinorRadius < rad) Then bCircs = False
            tverts = New TVERTICES(0)
            bverts = tverts
            'create the top edge
            If bCircs Then
                aCirc.Plane = topPl
                aCirc.Radius = rad
                aCirc.Identifier = "TOP EDGE"
                aCirc.StartAngle = 0
                aCirc.EndAngle = 360
                '.SpannedAngle = 360
                '.StartPt = aCirc.Plane.Vector(.Radius, 0)
                '.EndPt = aCirc.StartPt
                tverts.Add(aCirc.StartPt)
                tverts.Add(aCirc.Plane.Vector(0, aCirc.Radius))
                tverts.Add(aCirc.Plane.Vector(-aCirc.Radius, 0))
                tverts.Add(aCirc.Plane.Vector(0, -aCirc.Radius))
                _rVal.Add(aCirc, eGUID, "TOP EDGE")
                _rVal.SetDisplayStructure(1, dsp)
                If dpth > 0 And Not bSuppressDepth Then
                    aCirc.Plane = botPl
                    'aCirc.StartPt = aCirc.Plane.Vector(.Radius, 0)
                    'aCirc.EndPt = aCirc.StartPt
                    bverts.Add(aCirc.StartPt)
                    bverts.Add(aCirc.Plane.Vector(0, aCirc.Radius))
                    bverts.Add(aCirc.Plane.Vector(-aCirc.Radius, 0))
                    bverts.Add(aCirc.Plane.Vector(0, -aCirc.Radius))
                    _rVal.Add(aCirc, eGUID, "BOTTOM EDGE")
                    _rVal.SetDisplayStructure(2, dsp)
                End If
            Else
                tverts = dxfPrimatives.CreateHoleVertices(rad, aHole.MinorRadius, aHole.Length, aHole.IsSquare, 0, topPl)
                Dim aSegs As List(Of TSEGMENT) = dxfSegments.PolylineSegments(tverts, topPl, True, aHole.DisplaySettings, dsp.LTScale, aIdentifier:="TOP EDGE")
                For i = 1 To aSegs.Count
                    _rVal.Add(aSegs(i - 1), eGUID, "TOP EDGE")
                Next i
                If dpth > 0 And Not bSuppressDepth Then
                    Dim trans As TVECTOR = aDir * -dpth
                    For i = 1 To aSegs.Count
                        aSeg = New TSEGMENT(aSegs(i - 1))
                        aSeg.Translate(trans)
                        _rVal.Add(aSeg, eGUID, "BOTTOM EDGE")
                    Next i
                    For i = 1 To tverts.Count
                        v1 = tverts.Vector(i) + trans
                        bverts.Add(v1)
                    Next i
                End If
            End If
            If bverts.Count > 0 Then
                aSeg = New TSEGMENT("") With
                {
                .DisplayStructure = dsp,
                .Identifier = "DEPTH LINE",
                .OwnerGUID = eGUID
                }

                For i = 1 To tverts.Count
                    v1 = tverts.Vector(i)
                    v2 = bverts.Vector(i)
                    aSeg.LineStructure.SPT = v1
                    aSeg.LineStructure.EPT = v2
                    _rVal.Add(New TSEGMENT(aSeg), eGUID, "DEPTH LINE")
                Next i
            End If
            Return _rVal
        End Function
        Friend Shared Function CreateVertices_Rectangle(aCenter As TVECTOR, aPlane As TPLANE, Optional aHeight As Double = 1, Optional aWidth As Double = 1, Optional aFillet As Double = 0.0, Optional bApplyFilletAsChamfer As Boolean = False, Optional aRotation As Double = 0.0, Optional aXOffset As Double = 0.0, Optional aYOffset As Double = 0.0) As TVERTICES
            '#1the center for the new rectangle
            '#2flag to return a polygon instead of a polyline
            '#3the height of the desired rectangle
            '#4the width of the desired rectangle
            '#5a fillet radius to apply to the rectangles corners
            '#6flag to return a chamfered rectangle rather than a filleted one
            '^returns as rectangular polyline with desired dimensions centered at the passed point
            aHeight = Math.Abs(aHeight)
            aWidth = Math.Abs(aWidth)
            If aHeight = 0 Then aHeight = 1
            If aWidth = 0 Then aHeight = 1
            aFillet = Math.Abs(aFillet)
            If aFillet >= 0.5 * aWidth Then aFillet = 0
            If aFillet >= 0.5 * aHeight Then aFillet = 0
            Dim x1 As Double
            Dim Y1 As Double
            Dim x2 As Double
            Dim Y2 As Double
            Dim verts As New TVERTICES
            Dim bPlane As TPLANE = aPlane.Clone
            bPlane.Origin = aCenter
            If aRotation <> 0 Then bPlane.Revolve(aRotation, False)
            verts = New TVERTICES(0)
            x1 = aXOffset - 0.5 * aWidth
            x2 = aXOffset + 0.5 * aWidth
            Y1 = aYOffset - 0.5 * aHeight
            Y2 = aYOffset + 0.5 * aHeight
            If aFillet = 0 Then
                verts.Add(bPlane.Vertex(x1, Y1))
                verts.Add(bPlane.Vertex(x2, Y1))
                verts.Add(bPlane.Vertex(x2, Y2))
                verts.Add(bPlane.Vertex(x1, Y2))
            Else
                If Not bApplyFilletAsChamfer Then
                    verts.Add(bPlane.Vertex(x1, Y1 + aFillet, aRadius:=aFillet))
                    verts.Add(bPlane.Vertex(x1 + aFillet, Y1))
                    verts.Add(bPlane.Vertex(x2 - aFillet, Y1, aRadius:=aFillet))
                    verts.Add(bPlane.Vertex(x2, Y1 + aFillet))
                    verts.Add(bPlane.Vertex(x2, Y2 - aFillet, aRadius:=aFillet))
                    verts.Add(bPlane.Vertex(x2 - aFillet, Y2))
                    verts.Add(bPlane.Vertex(x1 + aFillet, Y2, aRadius:=aFillet))
                    verts.Add(bPlane.Vertex(x1, Y2 - aFillet))
                Else
                    verts.Add(bPlane.Vertex(x1, Y1 + aFillet))
                    verts.Add(bPlane.Vertex(x1 + aFillet, Y1))
                    verts.Add(bPlane.Vertex(x2 - aFillet, Y1))
                    verts.Add(bPlane.Vertex(x2, Y1 + aFillet))
                    verts.Add(bPlane.Vertex(x2, Y2 - aFillet))
                    verts.Add(bPlane.Vertex(x2 - aFillet, Y2))
                    verts.Add(bPlane.Vertex(x1 + aFillet, Y2))
                    verts.Add(bPlane.Vertex(x1, Y2 - aFillet))
                End If
            End If
            Return verts
        End Function

        Public Shared Function CreatePolygonVertices(aCenter As iVector, aSideCnt As Integer, aRadius As Double, Optional aRotation As Double? = Nothing, Optional bXScribed As Boolean = False, Optional aShearAngle As Double = 0.0, Optional aPlane As dxfPlane = Nothing) As colDXFVectors
            '#1the center for the polygon
            '#2the plane of the polygon
            '#3the number of sides (3 to 100)
            '#4the radius of the defining circle
            '#5a rotation to apply
            '#6flag idicating if the polygon should contain the circle or vice versa
            '#7a shear to apply
            '^returns a multisided polygon with the passed properties
            Return New colDXFVectors(CreateVertices_Polygon(New TVECTOR(aCenter), New TPLANE(aPlane), aSideCnt, aRadius, aRotation, bXScribed, aShearAngle))
        End Function

        Friend Shared Function CreateVertices_Polygon(aCenter As TVECTOR, aPlane As TPLANE, aSideCnt As Integer, aRadius As Double, Optional aRotation As Double? = Nothing, Optional bXScribed As Boolean = False, Optional aShearAngle As Double = 0.0) As TVERTICES
            Dim _rVal As New TVERTICES(0)
            '#1the center for the polygon
            '#2the plane of the polygon
            '#3the number of sides (3 to 100)
            '#4the radius of the defining circle
            '#5a rotation to apply
            '#6flag idicating if the polygon should contain the circle or vice versa
            '#7a shear to apply
            '^returns a multisided polygon with the passed properties
            aRadius = Math.Abs(aRadius)
            If aRadius <= 0 Then aRadius = 1
            If aSideCnt < 3 Then aSideCnt = 3
            If aSideCnt > 100 Then aSideCnt = 100
            Dim aAng As Double = 360 / aSideCnt
            Dim verts As New TVECTORS(0)
            Dim aScl As Double = 1
            Dim v1 As TVECTOR
            Dim aPl As New TPLANE(aPlane) With {.Origin = aCenter}
            Dim bPl As New TPLANE("")
            Dim vrt As TVERTEX
            bPl.Origin = aCenter

            For i As Integer = 1 To aSideCnt
                v1 = aPl.AngleVector((i - 1) * aAng, aRadius, False)
                verts.Add(v1)
            Next i
            If dxfUtils.IsOdd(aSideCnt) Then
                v1 = verts.Item(verts.Count - 2).DirectionTo(verts.Item(verts.Count - 1))
            Else
                v1 = verts.Item(verts.Count - 1).DirectionTo(verts.Item(verts.Count))
            End If
            aAng = v1.AngleTo(aPl.XDirection, aPl.ZDirection)
            If aAng <> 0 Then verts.Rotate(aPl.Origin, aPl.ZDirection, aAng, False)
            If bXScribed Then
                Dim v3 As TVECTOR = verts.Item(1).Interpolate(verts.Item(2), 0.5)
                aScl = aRadius / v3.DistanceTo(aPl.Origin)
            End If
            aShearAngle = TVALUES.ObliqueAngle(aShearAngle)
            If aRotation.HasValue Then aPl.Revolve(aRotation.Value)
            For i = 1 To verts.Count
                v1 = verts.Item(i).WithRespectTo(aPl)
                If aShearAngle <> 0 Then
                    v1 = aPl.Vector(v1.X * aScl, v1.Y * aScl, v1.Z * aScl, aShearAngle)
                Else
                    v1 = aPl.Vector(v1.X * aScl, v1.Y * aScl, v1.Z * aScl)
                End If
                ''v2 = bPl.Vector(v1.X, v1.Y, v1.Z)
                vrt = New TVERTEX(v1.X, v1.Y, v1.Z)
                _rVal.Add(vrt, dxxVertexStyles.MOVETO)
            Next i
            Return _rVal
        End Function

        Friend Shared Function CreateVertices_Circle(aCenter As TVECTOR, aPlane As TPLANE, aRadius As Double) As TVERTICES
            Dim _rVal As TVERTICES
            '#1the center of the circle
            '#2the radius of the Circle
            '^returns a polygon that is a full cirlce with 4 arc segments
            Dim rad As Double = Math.Abs(aRadius)
            If rad = 0 Then rad = 1
            Dim bPlane As TPLANE = aPlane.Clone
            bPlane.Origin = aCenter
            _rVal = New TVERTICES(bPlane.Vertex(0, rad, 0, 0, rad, aTag:="Q2"))
            _rVal.Add(bPlane.Vertex(-rad, 0, 0, 0, rad, aTag:="Q3"))
            _rVal.Add(bPlane.Vertex(0, -rad, 0, 0, rad, aTag:="Q4"))
            _rVal.Add(bPlane.Vertex(rad, 0, 0, 0, rad, aTag:="Q1"))
            Return _rVal
        End Function

        Public Shared Function CreateVertices_Trace(aVerticesXY As IEnumerable(Of iVector), aThickness As Double, Optional aClosed As Boolean = False, Optional aApplyFillets As Boolean = False, Optional aFilletEnds As Boolean = False, Optional aRotation As Double = 0.0, Optional aPlane As dxfPlane = Nothing) As colDXFVectors
            '#1the xy vectors to use for the trace pattern
            '#2the distance between the trace edges
            '#3flag to close the trace
            '#4flag to apply arc fillets to the corners of the trace
            '#5flag to round the ends of the trace (only valid if fillets are appplied and the trace is not closed)
            '#6a rotation to apply
            '#7the plane for the trace
            '^creates a polyline trace along the path of the passed vectors.
            Try

                If aVerticesXY Is Nothing Then Return New colDXFVectors()
                If aPlane Is Nothing Then aPlane = New dxfPlane

                Return New colDXFVectors(dxfPrimatives.CreateVertices_Trace(dxfVectors.GetTVERTICES(aVerticesXY, aPlane, bJustProject:=True), aThickness, aClosed, aApplyFillets, aFilletEnds, aRotation, aPlane))

            Catch ex As Exception
                Throw (ex)
            End Try
            Return Nothing
        End Function

        Friend Shared Function CreateVertices_Trace(aVectors As TVERTICES, aThickness As Double, Optional aClosed As Boolean = False, Optional aApplyFillets As Boolean = False, Optional aFilletEnds As Boolean = False, Optional aRotation As Double = 0.0, Optional aPlane As dxfPlane = Nothing) As TVERTICES
            Dim rTopCount As Integer = 0
            Return CreateVertices_Trace(aVectors, aThickness, aClosed, aApplyFillets, aFilletEnds, aRotation, aPlane, rTopCount)
        End Function

        Friend Shared Function CreateVertices_Trace(aVectors As TVERTICES, aThickness As Double, aClosed As Boolean, aApplyFillets As Boolean, aFilletEnds As Boolean, aRotation As Double, aPlane As dxfPlane, ByRef rTopCount As Integer) As TVERTICES
            '#1the xy vectors to use for the trace pattern
            '#2the distance between the trace edges
            '#3flag to close the trace
            '#4flag to apply arc fillets to the corners of the trace
            '#5flag to round the ends of the trace (only valid if fillets are appplied and the trace is not closed)
            '#6a rotation to apply
            '#7the segment width for the trace edges
            '#8the plane for the trace
            '#9flag to return a dxePolygon rather than a dxePolyline
            '^creates a polyline trace along the path of the passed vectors.
            rTopCount = 0
            Dim verts As New TVERTICES(0)
            Dim aPl As New TPLANE(aPlane)
            Dim ip As TVECTOR
            Dim bVecs As TVERTICES = aVectors
            Dim lTops As New TLINES(0)
            Dim lBots As New TLINES(0)
            Dim sp As TVERTEX
            Dim ep As TVERTEX
            Dim aLn As TLINE
            Dim bLn As TLINE
            Dim aDir As TVECTOR
            Dim zDir As TVECTOR = TVECTOR.WorldZ
            Dim tverts As New TVERTICES
            Dim bverts As New TVERTICES(0)
            Dim v1 As TVERTEX
            Dim v2 As TVERTEX
            Dim v3 As TVERTEX
            Dim lv As TVERTEX
            Dim aArc As New TARC
            Dim i As Integer
            Dim thk As Double = Math.Abs(aThickness)
            Dim rad1 As Double = thk
            Dim rad2 As Double = 2 * thk
            Dim t As Double = thk / 2
            Dim bParel As Boolean
            Dim exst As Boolean
            Dim cnt As Integer
            Dim ang As Double
            Dim bMitered As Boolean

            bVecs = bVecs.UniqueMembers(3)
            If aClosed Or thk = 0 Or Not aApplyFillets Then aFilletEnds = False

            If aRotation <> 0 Then aPl.Rotate(aRotation, bInRadians:=False, bRotateOrigin:=False, bRotateDirections:=True, bSuppressNorm:=True)
            zDir = aPl.ZDirection
            'create top and bottom trace lines for each vector
            If bVecs.Count > 1 Then
                For i = 1 To bVecs.Count
                    sp = bVecs.Item(i)
                    If i + 1 <= bVecs.Count Then
                        ep = bVecs.Item(i + 1)
                    Else
                        If Not aClosed Then Exit For
                        ep = bVecs.Item(1)
                    End If
                    sp.Vector = aPl.WorldVector(sp.Vector)
                    ep.Vector = aPl.WorldVector(ep.Vector)
                    aDir = sp.Vector.DirectionTo(ep.Vector).RotatedAbout(zDir, 90, False)
                    lTops.Add(sp.Vector + (aDir * t), ep.Vector + (aDir * t))
                    lBots.Add(sp.Vector + (aDir * -t), ep.Vector + (aDir * -t))
                    If lTops.Count > 1 Then
                        aLn = lTops.Item(lTops.Count - 1)
                        bLn = lTops.Item(lTops.Count)
                        ip = aLn.IntersectionPt(bLn, exst, bParel)
                        If Not bParel Then
                            bLn.SPT = ip
                            lTops.SetEndPt(lTops.Count - 1, ip)
                            lTops.SetStartPt(lTops.Count, ip)
                            aLn = lBots.Item(lBots.Count - 1)
                            bLn = lBots.Item(lBots.Count)
                            ip = aLn.IntersectionPt(bLn, bParel)
                            lBots.SetEndPt(lBots.Count - 1, ip)
                            lBots.SetStartPt(lBots.Count, ip)
                        End If
                    End If
                Next i
            End If
            cnt = lTops.Count
            'miter the last intersection
            If aClosed And cnt > 1 Then
                aLn = lTops.Item(1)
                bLn = lTops.Item(cnt)
                ip = aLn.IntersectionPt(bLn, exst, bParel)
                If Not bParel Then
                    lTops.SetStartPt(1, ip)
                    lTops.SetEndPt(cnt, ip)
                    ip = lBots.Item(1).IntersectionPt(lBots.Item(cnt), bParel)
                    lBots.SetStartPt(1, ip)
                    lBots.SetEndPt(cnt, ip)
                    bMitered = True
                End If
            End If
            For i = 1 To cnt
                aLn = lTops.Item(i)
                tverts.Add(aLn.SPT)
                If i = cnt Then tverts.Add(aLn.EPT)
            Next i
            For i = cnt To 1 Step -1
                aLn = lBots.Item(i)
                bverts.Add(aLn.EPT)
                If i = 1 Then bverts.Add(aLn.SPT)
            Next i
            'apply fillets
            If aApplyFillets And cnt > 1 And thk > 0 Then
                lv = tverts.Last
                If Not aClosed Then
                    If Not aFilletEnds Then
                        verts = New TVERTICES(tverts.Item(1).Clone)
                    Else
                        v1 = bverts.Last
                        v2 = tverts.Item(2)
                        v3 = tverts.Item(3)
                        aArc = dxfPrimatives.CreateFilletArc(thk / 2, v1.Vector, v2.Vector, v3.Vector, aPl)
                        If aArc.Radius > 0 Then
                            v2.Vector = aArc.StartPt
                            v2.Radius = aArc.Radius
                            v2.Inverted = aArc.ClockWise
                            verts = New TVERTICES(v2.Clone)
                            verts.Add(aArc.EndPt.Clone)
                        Else
                            verts = New TVERTICES(tverts.Item(2).Clone)
                        End If
                    End If
                Else
                    v1 = tverts.Item(tverts.Count - 1)
                    v2 = tverts.Item(2)
                    v3 = tverts.Item(3)
                    ang = v1.Vector.DirectionTo(v2.Vector).AngleTo(v2.Vector.DirectionTo(v3.Vector), zDir)
                    If ang <> 0 And ang <> 180 Then
                        aArc.Radius = 0
                        If ang > 180 Then
                            aArc = dxfPrimatives.CreateFilletArc(rad2, v1.Vector, v2.Vector, v3.Vector, aPl)
                        Else
                            aArc = dxfPrimatives.CreateFilletArc(rad1, v1.Vector, v2.Vector, v3.Vector, aPl)
                        End If
                        If aArc.Radius > 0 Then
                            v2.Vector = aArc.StartPt
                            v2.Radius = aArc.Radius
                            v2.Inverted = aArc.ClockWise
                            verts.Add(v2)
                            lv = v2
                            verts.Add(aArc.EndPt)
                        Else
                            verts = New TVERTICES(tverts.Item(2).Clone)
                        End If
                    Else
                        verts = New TVERTICES(tverts.Item(2).Clone)
                    End If
                End If
                For i = 1 To tverts.Count - 2
                    v1 = tverts.Item(i)
                    v2 = tverts.Item(i + 1)
                    v3 = tverts.Item(i + 2)
                    ang = v1.Vector.DirectionTo(v2.Vector).AngleTo(v2.Vector.DirectionTo(v3.Vector), zDir)
                    If ang <> 0 And ang <> 180 Then
                        aArc.Radius = 0
                        If ang > 180 Then
                            aArc = dxfPrimatives.CreateFilletArc(rad2, v1.Vector, v2.Vector, v3.Vector, aPl)
                        Else
                            aArc = dxfPrimatives.CreateFilletArc(rad1, v1.Vector, v2.Vector, v3.Vector, aPl)
                        End If
                        If aArc.Radius > 0 Then
                            v2.Vector = aArc.StartPt
                            v2.Radius = aArc.Radius
                            v2.Inverted = aArc.ClockWise
                            verts.Add(v2)
                            verts.Add(aArc.EndPt.Clone)
                        Else
                            verts.Add(v2.Clone)
                        End If
                    Else
                        verts.Add(v2.Clone)
                    End If
                Next i
                verts.Add(lv.Clone)
                tverts = verts.Clone
                '======================================================================
                verts.Clear()
                lv = bverts.Last
                If Not aClosed Then
                    If Not aFilletEnds Then
                        verts = New TVERTICES(bverts.Item(1).Clone)
                    Else
                        v1 = tverts.Item(tverts.Count)
                        v2 = bverts.Item(2)
                        v3 = bverts.Item(3)
                        aArc = dxfPrimatives.CreateFilletArc(thk / 2, v1.Vector, v2.Vector, v3.Vector, aPl)
                        If aArc.Radius > 0 Then
                            v2.Vector = aArc.StartPt
                            v2.Radius = aArc.Radius
                            v2.Inverted = aArc.ClockWise
                            verts = New TVERTICES(v2)
                            verts.Add(aArc.EndPt)
                        Else
                            verts = New TVERTICES(bverts.Item(2).Clone)
                        End If
                    End If
                Else
                    v1 = bverts.Item(bverts.Count - 1)
                    v2 = bverts.Item(2)
                    v3 = bverts.Item(3)
                    ang = v1.Vector.DirectionTo(v2.Vector).AngleTo(v2.Vector.DirectionTo(v3.Vector), zDir)
                    If ang <> 0 And ang <> 180 Then
                        aArc.Radius = 0
                        If ang > 180 Then
                            aArc = dxfPrimatives.CreateFilletArc(rad2, v1.Vector, v2.Vector, v3.Vector, aPl)
                        Else
                            aArc = dxfPrimatives.CreateFilletArc(rad1, v1.Vector, v2.Vector, v3.Vector, aPl)
                        End If
                        If aArc.Radius > 0 Then
                            v2.Vector = aArc.StartPt
                            v2.Radius = aArc.Radius
                            v2.Inverted = aArc.ClockWise
                            verts.Add(v2.Clone)
                            verts.Add(aArc.EndPt.Clone)
                            lv = v2
                        Else
                            verts = New TVERTICES(bverts.Item(2).Clone)
                        End If
                    Else
                        verts = New TVERTICES(bverts.Item(2).Clone)
                    End If
                End If
                For i = 1 To bverts.Count - 2
                    v1 = bverts.Item(i)
                    v2 = bverts.Item(i + 1)
                    v3 = bverts.Item(i + 2)
                    ang = v1.Vector.DirectionTo(v2.Vector).AngleTo(v2.Vector.DirectionTo(v3.Vector), zDir)
                    If ang <> 0 And ang <> 180 Then
                        aArc.Radius = 0
                        If ang > 180 Then
                            aArc = dxfPrimatives.CreateFilletArc(rad2, v1.Vector, v2.Vector, v3.Vector, aPl)
                        Else
                            aArc = dxfPrimatives.CreateFilletArc(rad1, v1.Vector, v2.Vector, v3.Vector, aPl)
                        End If
                        If aArc.Radius > 0 Then
                            v2.Vector = aArc.StartPt
                            v2.Radius = aArc.Radius
                            v2.Inverted = aArc.ClockWise
                            verts.Add(v2)
                            verts.Add(aArc.EndPt.Clone)
                        Else
                            verts.Add(v2.Clone)
                        End If
                    Else
                        verts.Add(v2.Clone)
                    End If
                Next i
                verts.Add(lv)
                bverts = verts.Clone
            End If
            If aFilletEnds Then
                v1 = bverts.Item(bverts.Count - 1)
                v2 = bverts.Item(bverts.Count)
                v3 = tverts.Item(2)
                aArc = dxfPrimatives.CreateFilletArc(thk / 2, v1.Vector, v2.Vector, v3.Vector, aPl)
                If aArc.Radius > 0 Then
                    v2.Vector = aArc.StartPt
                    v2.Radius = aArc.Radius
                    v2.Inverted = aArc.ClockWise
                    bverts.SetItem(bverts.Count, v2)
                End If
                v1 = tverts.Item(bverts.Count - 1)
                v2 = tverts.Item(bverts.Count)
                v3 = bverts.Item(2)
                aArc = dxfPrimatives.CreateFilletArc(thk / 2, v1.Vector, v2.Vector, v3.Vector, aPl)
                If aArc.Radius > 0 Then
                    v2.Vector = aArc.StartPt
                    v2.Radius = aArc.Radius
                    v2.Inverted = aArc.ClockWise
                    tverts.SetItem(tverts.Count, v2)
                End If
            End If
            rTopCount = lTops.Count
            Dim _rVal As TVERTICES = tverts
            If thk > 0 Then
                For i = 1 To bverts.Count
                    _rVal.Add(bverts.Item(i).Clone)
                Next i
            End If
            Return _rVal
        End Function

        Friend Shared Function CreateDonut(aCenter As TVECTOR, aReturnPolygon As Boolean, aRadius As Double, Optional aStartAngle As Double = 0.0, Optional aEndAngle As Double = 360, Optional bClockwise As Boolean = False, Optional aPlane As dxfPlane = Nothing, Optional aWidth As Double = 0.0, Optional bClosed As Boolean = True, Optional aTrimToXAxis As Boolean = False, Optional aTrimNearestToYAxis As Boolean = False) As dxfPolyline
            Dim _rVal As dxfPolyline
            '#1the center for the new donut
            '#2flag to return a polygon instead of a polyline
            '#3the radius of the desired donut
            '#4the start angle
            '#5the end angle
            '#6clockwise flag
            '#7the coordinate system of definition
            '#8a width for the donut
            '#9flag to close the polyline
            '^returns a polyline that is the boundary of a circular donut section
            aRadius = Math.Abs(aRadius)
            If aRadius = 0 Then aRadius = 1
            aWidth = Math.Abs(aWidth)
            Dim aArc As dxeArc
            Dim sArcs As colDXFEntities
            Dim verts As colDXFVectors
            Dim aspn As Double
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim ctr As TVECTOR
            Dim rad2 As Double
            Dim aAxisLine As dxeLine
            Dim aDir As TVECTOR
            Dim d1 As Double
            Dim aLn As New TLINE
            Dim bArc As New TARC
            Dim bLn As New TLINE
            Dim iPts As TVECTORS
            Dim aPl As New TPLANE(aPlane)
            Dim rVerts As colDXFVectors = Nothing
            If aTrimToXAxis And aTrimNearestToYAxis Then aTrimNearestToYAxis = False

            aPl.Origin = aCenter
            aArc = New dxeArc With {
                .Radius = aRadius,
                .PlaneV = aPl,
                .CenterV = aCenter,
                .StartAngle = aStartAngle,
                .EndAngle = aEndAngle,
                .ClockWise = bClockwise
                }
            aspn = aArc.SpannedAngle
            sArcs = aArc.Divide(90)
            verts = New colDXFVectors
            For i As Integer = 1 To sArcs.Count
                aArc = sArcs.Item(i)
                verts.AddV(aArc.StartPtV, aRadius, bClockwise)
                If i = sArcs.Count And aspn < 359.99 Then verts.AddV(aArc.EndPtV)
            Next i
            If aWidth > 0 And aWidth < aRadius Then
                ctr = aArc.CenterV
                rad2 = aRadius - aWidth
                If aTrimToXAxis Then
                    aAxisLine = aArc.Plane.XAxis
                    aLn = New TLINE(aAxisLine)
                    bArc = TARC.ArcStructure(aArc.PlaneV, ctr, rad2, 0, 360, False)
                ElseIf aTrimNearestToYAxis Then
                    aAxisLine = aArc.Plane.YAxis
                    aLn = New TLINE(aAxisLine)
                    bArc = TARC.ArcStructure(aArc.PlaneV, ctr, rad2, 0, 360, False)
                Else
                    aAxisLine = Nothing
                End If
                rVerts = verts.Clone
                For i = verts.Count To 1 Step -1
                    v1 = verts.ItemVector(i)
                    If aAxisLine Is Nothing Or (i > 1 And i < verts.Count) Then
                        d1 = aWidth
                        aDir = v1.DirectionTo(ctr)
                    Else
                        bLn.SPT = v1
                        bLn.EPT = dxfProjections.ToLine(v1, aLn)
                        iPts = bLn.IntersectionPts(bArc, True)
                        If iPts.Count > 0 Then
                            v2 = iPts.Nearest(v1, TVECTOR.Zero, d1)
                            aDir = v1.DirectionTo(v2)
                        Else
                            d1 = aWidth
                            aDir = v1.DirectionTo(ctr)
                        End If
                    End If
                    v1 += aDir * d1
                    If i > 1 Then
                        rVerts.AddV(v1, rad2, Not bClockwise)
                    Else
                        rVerts.AddV(v1)
                    End If
                Next i
            End If
            _rVal = dxfPrimatives.CreatePGorPL2(aReturnPolygon, aPl, bClosed, rVerts)
            Return _rVal
        End Function

        Friend Shared Function CreateEllipse(aCenter As TVECTOR, aReturnPolygon As Boolean, aMajorAxis As Double, aMinorAxis As Double, Optional aRotation As Double = 0.0, Optional aStartAngle As Double = 0.0, Optional aEndAngle As Double = 360, Optional aSegmentWidth As Double = 0.0, Optional aPlane As dxfPlane = Nothing) As dxfPolyline
            Dim _rVal As dxfPolyline
            '#1the center for the new ellipse
            '#2the major axis length of the desired ellipse
            '#3the minor axis length of the desired ellipse
            '#4a rotation to apply
            '#5a start angle to apply
            '#6a end angle to apply
            '#7a coordinate system to assign to the polygon
            '^returns a polygon with line segments approximating the desired ellipse
            Try
                If aMinorAxis = 0 Or aMajorAxis = 0 Then Throw New Exception("The Passed Axis Lengths Are Invalid")
                aMajorAxis = Math.Abs(aMajorAxis)
                aMinorAxis = Math.Abs(aMinorAxis)
                TVALUES.SortTwoValues(True, aMinorAxis, aMajorAxis)
                aRotation = TVALUES.NormAng(aRotation, False, True, True)

                Dim aPl As New TPLANE(aPlane, New dxfVector(aCenter))

                Dim aEllipse As New dxeEllipse With {
                    .SuppressEvents = True,
                    .PlaneV = aPl,
                    .CenterV = aCenter,
                    .Diameter = aMajorAxis,
                    .MinorDiameter = aMinorAxis,
                    .StartAngle = aStartAngle,
                    .EndAngle = aEndAngle
                    }

                aEllipse.RotateAbout(aEllipse.Center, aRotation, False)
                aEllipse.SuppressEvents = False
                _rVal = dxfPrimatives.CreatePGorPL2(aReturnPolygon, aPl, True, aEllipse.PhantomPoints(30), aSegmentWidth)
                Return _rVal
            Catch ex As Exception
                Throw ex
                Return _rVal
            End Try
        End Function

        Friend Shared Function CreateTrimmedCircle(aCenter As TVECTOR, aTrimCenter As TVECTOR, aReturnPolygon As Boolean, aRadius As Double, aTrimRadius As Double, Optional aPlane As dxfPlane = Nothing, Optional bClosed As Boolean = True) As dxfPolyline
            Dim _rVal As dxfPolyline
            '#1the center for the new circle
            '#2the center of the trimming circle
            '#3flag to return a polygon instead of a polyline
            '#3the radius of the new circle
            '#4the radius of the triming circle
            '#5the coordinate system of definition
            '#6flag to close the polyline
            '^returns a polyline that is the boundary of a circle trimmed by another circle
            '~the two circles as assumed to be on the same plane
            aRadius = Math.Abs(aRadius)
            If aRadius = 0 Then aRadius = 1
            aTrimRadius = Math.Abs(aTrimRadius)
            If aTrimRadius = 0 Then aTrimRadius = 1
            Dim wPl As New TPLANE("")
            Dim cp As TVECTOR = wPl.Vector(aCenter.X, aCenter.Y)
            Dim tp As TVECTOR = wPl.Vector(aTrimCenter.X, aTrimCenter.Y)
            Dim iPts As TVECTORS
            Dim v1 As dxfVector
            Dim rVerts As New colDXFVectors
            Dim verts As TVERTICES
            Dim aVert As TVERTEX
            Dim sang As Double
            Dim eang As Double
            Dim arPl As New TPLANE("") With {
                .Origin = cp
            }
            Dim bKeep As Boolean
            Dim ip1 As New TVERTEX
            Dim ip2 As TVERTEX
            Dim aArc As New TARC(arPl, cp, aRadius)
            Dim bArc As New TARC(wPl, tp, aTrimRadius)
            If Not aArc.Plane.Origin.Equals(bArc.Plane.Origin, 4) Then
                iPts = aArc.IntersectionPts(bArc, True)
            Else
                iPts = New TVECTORS(0)
            End If
            If iPts.Count > 1 Then
                'create sub arcs for the primary circle
                ip1 = New TVERTEX(iPts.Item(1)) With {
                    .Radius = bArc.Radius
                }
                aVert = ip1
                ip2 = ip1.Clone
                ip2.Vector = iPts.Item(2)
                ip2.Radius = aArc.Radius
                verts = New TVERTICES(ip1, ip2)
                sang = 0
                eang = 90
                For i As Integer = 0 To 3
                    aArc = TARC.ArcStructure(arPl, cp, aRadius, sang, eang, False)
                    bKeep = aArc.StartPt.DistanceTo(tp, 4) <= aTrimRadius
                    If bKeep Then
                        aVert.Vector = aArc.StartPt
                        aVert.Radius = aRadius
                        verts.Add(aVert)
                    End If
                    bKeep = aArc.EndPt.DistanceTo(tp, 4) <= aTrimRadius
                    If bKeep Then
                        aVert.Vector = aArc.EndPt
                        aVert.Radius = aRadius
                        verts.Add(aVert)
                    End If
                    sang = eang
                    eang += 90
                Next i
                verts.RemoveCoincident()
                For i = 0 To verts.Count - 1
                    aVert = verts.Item(i + 1)
                    rVerts.AddV(aVert.Vector, aVert.Radius)
                Next i
                rVerts.SortClockWiseV(cp, wPl.XDirection.AngleTo(aCenter.DirectionTo(rVerts.ItemVector(1)), wPl.ZDirection), True, wPl)
                If Not dxfPlane.IsNull(aPlane) Then
                    arPl = New TPLANE(aPlane)
                    For i = 1 To rVerts.Count
                        v1 = rVerts.Item(i)
                        v1.Strukture = arPl.WorldVector(v1.Strukture)
                        v1.Z = aCenter.Z
                    Next i
                End If
            Else
                arPl = New TPLANE("") With {.Origin = aCenter}
                rVerts = New colDXFVectors(dxfPrimatives.CreateVertices_Circle(aCenter, arPl, aRadius))
            End If
            _rVal = dxfPrimatives.CreatePGorPL2(aReturnPolygon, arPl, bClosed, rVerts)
            If aReturnPolygon Then _rVal.InsertionPt = New dxfVector(cp)
            Return _rVal
        End Function

        Friend Shared Function CreateGridPoints(aBoundary As dxfEntity, aIslands As colDXFEntities, gProps As TGRID, Optional bAllowBoundaryPoints As Boolean = False, Optional aLineCollector As colDXFEntities = Nothing, Optional bSuppressPlaneCheck As Boolean = False, Optional aOriginCollector As dxfVector = Nothing) As colDXFVectors
            Dim _rVal As colDXFVectors
            '#1the bounding entity
            '#2entities to treat as islands within the bounds
            '#3the grid structure which carries the pitches etc. for the layout
            '#4flag to keep points which lie on the boundary segments
            '#5returns the grid lines (if passed)
            '#6flag to suppress for checking for plane equality on the islands
            '^lays out a grid of of points within the bounding entity's hatch boundary
            _rVal = New colDXFVectors
            Dim aEnt As dxfEntity
            Dim iPl As dxfPolyline
            Dim iAr As dxfArc
            Dim aPl As dxePolyline
            Dim aSld As dxeSolid
            Dim gPts As colDXFVectors
            Dim aArc As New TSEGMENT
            Dim eps As TVECTORS
            Dim bndLoops As New TBOUNDLOOPS
            Dim bRec As New TPLANE("")
            Dim aPlane As New TPLANE("")
            Dim v1 As TVECTOR
            Dim cp As TVECTOR
            Dim hLn As TLINE

            Dim aExtPts As TVECTORS
            Dim aProps As New TGRID
            Dim aDir As TVECTOR
            Dim eRec As New TPLANE("")
            Dim eBnds As New TSEGMENTS
            Dim eBndLoops As New TBOUNDLOOPS
            Dim iCnt As Integer
            Dim i As Integer
            Dim j As Integer
            Dim k As Integer
            Dim d1 As Double
            Dim d2 As Double
            Dim d3 As Double
            Dim iRow As Integer
            Dim aPitch As dxxPitchTypes
            Dim aHPitch As Double
            Dim aVPitch As Double
            Dim aFlg As Boolean
            Dim diag As Double
            Dim diag2 As Double
            Dim bOnPlane As Boolean
            Dim gType As dxxGraphicTypes
            Dim org As TVECTOR
            aProps = gProps
            If aBoundary Is Nothing Then Return _rVal
            'get the entity plane
            aPlane = aBoundary.Plane.Strukture
            'get the extent points of the entity
            aExtPts = New TVECTORS(aBoundary.ExtentPoints)
            If aExtPts.Count <= 1 Then Return _rVal
            'get the segments of the hatch boundary of the entity on its own plane
            bndLoops = dxfHatches.BoundLoops_Entity(aBoundary, aPlane)
            'no edges no hatch!
            If bndLoops.Count <= 0 Then Return _rVal
            If aProps.Rotation <> 0 Then
                'apply the big rotation for the whole hatch to the plane
                aPlane.Revolve(aProps.Rotation)
            End If
            'get a rectangle on our plane around the boundary
            bRec = aExtPts.Bounds(aPlane)
            If bRec.Height <= 0 Or bRec.Width <= 0 Then Return _rVal
            bRec.Stretch(1, True, True, False, True)
            diag = Math.Round(Math.Sqrt(bRec.Width ^ 2 + bRec.Height ^ 2) / 2, 6)
            'get the pattern which describes the hatch line family
            aHPitch = Math.Round(Math.Abs(aProps.HorizontalPitch), 6)
            aVPitch = Math.Round(Math.Abs(aProps.VerticalPitch), 6)
            aPitch = aProps.PitchType
            If aPitch < 0 Or aPitch > 2 Then aPitch = dxxPitchTypes.Rectangular
            If aHPitch = 0 Then aHPitch = 1
            If aVPitch = 0 Then aVPitch = aHPitch
            If aPitch = dxxPitchTypes.InvertedTriangular Then aProps.OffsetX += 0.5 * aHPitch
            If aProps.OffsetX <> 0 Then
                bRec.Stretch(Math.Abs(aProps.OffsetX) * 2, True, False, False, True)
            End If
            If aProps.OffsetY <> 0 Then
                bRec.Stretch(Math.Abs(aProps.OffsetY) * 2, False, True, False, True)
            End If
            org = bRec.Vector(aProps.OffsetX, aProps.OffsetY)
            If aOriginCollector IsNot Nothing Then aOriginCollector.Strukture = org
            'create the horizontal lines
            d1 = (Fix(0.5 * bRec.Height / aVPitch) * aVPitch) + aVPitch + Math.Abs(aProps.OffsetY)
            Dim hLns As New TLINES(0)
            d2 = d1
            d3 = -d1
            iCnt = 0
            Do While d2 >= d3
                hLn = bRec.LineH(d2, bRec.Width)
                eps = hLn.IntersectionPts(bndLoops)
                eps.SortNearestToFarthest(hLn.SPT, False, True, 3)
                If eps.Count >= 2 Then
                    iCnt = eps.Count
                    For i = 0 To eps.Count - 2
                        hLn.SPT = eps.Item(i + 1)
                        hLn.EPT = eps.Item(i + 2)
                        hLns.Add(hLn)
                        i += 1
                    Next i
                Else
                    If iCnt > 0 Then Exit Do
                End If
                d2 -= aVPitch
            Loop
            'create the grid points
            gPts = New colDXFVectors
            For i = 0 To hLns.Count
                hLn = hLns.Item(i)
                'project the origin onto the line
                'v1 = bRec.Vector( Abs(aProps.OffsetX), 0)
                v1 = org
                v1 = dxfProjections.ToLine(v1, hLn, d1)
                d2 = hLn.SPT.DistanceTo(v1, 6)
                d3 = (Fix(d2 / aHPitch) * aHPitch)
                'move the point as close to the start pt of the line without going past
                v1 += bRec.XDirection * -d3
                d2 -= d3
                'offset for triangular pitchs
                If aPitch <> dxxPitchTypes.Rectangular Then
                    If i = 1 Then
                        If d1 <> 0 Then iRow = Fix(Math.Abs(d1) / aVPitch) Else iRow = 1
                    Else
                        iRow -= 1
                    End If
                    If Math.Abs(iRow) Mod (2 <> 0) Then
                        If d2 >= 0.5 * aHPitch Then
                            v1 += bRec.XDirection * (-0.5 * aHPitch)
                        Else
                            v1 += bRec.XDirection * (0.5 * aHPitch)
                        End If
                    End If
                End If
                Do
                    aDir = hLn.SPT.DirectionTo(v1, False, d3)
                    d3 = Math.Round(d3, 5)
                    If d3 = 0 Then
                        If bAllowBoundaryPoints Then
                            gPts.AddV(v1)
                            gPts.LastVector.Row = iRow
                        End If
                    Else
                        aDir = v1.DirectionTo(hLn.EPT, False, d3)
                        d3 = Math.Round(d3, 5)
                        If d3 = 0 Then
                            If bAllowBoundaryPoints Then
                                gPts.AddV(v1)
                                gPts.LastVector.Row = iRow
                                Exit Do
                            End If
                        Else
                            If aDir.Equals(bRec.XDirection, 3) Then
                                gPts.AddV(v1)
                                gPts.LastVector.Row = iRow
                            Else
                                Exit Do
                            End If
                        End If
                    End If
                    v1 += bRec.XDirection * aHPitch
                    'Application.DoEvents()
                Loop
                If aLineCollector IsNot Nothing Then aLineCollector.AddLineV2(hLn)
                'Application.DoEvents()
            Next i
            'trim out the islands
            If aIslands IsNot Nothing And gPts.Count > 0 Then
                For k = 1 To aIslands.Count
                    aEnt = aIslands.Item(k)
                    gType = aEnt.GraphicType
                    If gType = dxxGraphicTypes.Line Or gType = dxxGraphicTypes.Point Or gType = dxxGraphicTypes.Leader Or gType = dxxGraphicTypes.Dimension Or gType = dxxGraphicTypes.Bezier Or gType = dxxGraphicTypes.EndBlock Or gType = dxxGraphicTypes.Hatch Or gType = dxxGraphicTypes.SequenceEnd Then
                        'these entities dont affect grid
                        bOnPlane = False
                    Else
                        bOnPlane = True
                    End If
                    If bOnPlane And (gType = dxxGraphicTypes.Arc Or gType = dxxGraphicTypes.Ellipse) Then
                        'only full arcs affect the grid
                        iAr = aEnt
                        aArc = dxfEntity.ArcLine(aEnt)
                        cp = aArc.ArcStructure.Plane.Origin
                        If iAr.SpannedAngle < 359.99 Then
                            bOnPlane = False
                        End If
                    End If
                    If bOnPlane Then
                        If gType = dxxGraphicTypes.Solid Then
                            'use a solids bounding perimeter
                            aSld = aEnt
                            aEnt = aSld.Perimeter
                            gType = dxxGraphicTypes.Polyline
                        End If
                        If gType = dxxGraphicTypes.Polygon Or gType = dxxGraphicTypes.Polyline Then
                            'use polygons bounding perimeter
                            iPl = aEnt
                            aPl = New dxePolyline With {
                            .ImageGUID = aBoundary.ImageGUID,
                            .PlaneV = iPl.Plane.Strukture,
                            .VerticesV = New TVERTICES(iPl.Vertices),
                            .Closed = True}
                            gType = dxxGraphicTypes.Polyline
                        End If
                    End If
                    If bOnPlane Then
                        'make sure the entity has area
                        eRec = aEnt.BoundingRectangle.Strukture
                        bOnPlane = (eRec.Width > 0 And eRec.Height > 0)
                    End If
                    If bOnPlane Then
                        If Not bSuppressPlaneCheck Then
                            'check for planarity
                            bOnPlane = TPLANES.Compare(aPlane, aEnt.Plane.Strukture, 3, False, False)
                        End If
                        'gross check
                        diag2 = Math.Round(Math.Sqrt(eRec.Width ^ 2 + eRec.Height ^ 2) / 2, 6)
                        If eRec.Origin.DistanceTo(bRec.Origin, 6) >= diag + diag2 Then bOnPlane = False
                    End If
                    If bOnPlane Then
                        If gType <> dxxGraphicTypes.Arc And gType <> dxxGraphicTypes.Ellipse And gType <> dxxGraphicTypes.Polyline Then
                            eBndLoops = dxfHatches.BoundLoops_Entity(aEnt, aPlane)
                            If eBndLoops.Count <= 0 Then
                                bOnPlane = False
                            Else
                                eRec = eBndLoops.ExtentPts.Bounds(aPlane)
                                If Not (eRec.Width > 0 And eRec.Height > 0) Then
                                    bOnPlane = False
                                End If
                            End If
                        End If
                    End If
                    If bOnPlane Then
                        'check points to se if they are with then entiy
                        For j = 1 To gPts.Count
                            v1 = gPts.ItemVector(j)
                            If v1.Code = 0 Then
                                'gross check
                                If eRec.Origin.DistanceTo(v1, 6) <= diag2 Then
                                    Select Case aEnt.GraphicType
                                        Case dxxGraphicTypes.Ellipse, dxxGraphicTypes.Arc
                                            Dim issp As Boolean = False
                                            Dim isep As Boolean = False
                                            If aArc.ContainsVector(v1, 0.001, isep, isep, bTreatAsInfinite:=True, bSuppressPlaneCheck:=True, rWithin:=aFlg) Then
                                                If Not bAllowBoundaryPoints Then gPts.Item(j).VertexCode = 1
                                            Else
                                                If aFlg Then
                                                    gPts.Item(j).VertexCode = 1
                                                End If
                                            End If
                                        Case dxxGraphicTypes.Polyline
                                            aPl = aEnt
                                            aPl.UpdatePath()
                                            If aPl.PathSegments.EncloseVector(v1, aPl.Bounds, aOnBoundIsIn:=bAllowBoundaryPoints) Then
                                                gPts.Item(j).VertexCode = 1
                                            End If
                                        Case Else
                                            If eBnds.Count > 0 Then
                                                If eBnds.EncloseVector(v1, eRec, aOnBoundIsIn:=bAllowBoundaryPoints) Then
                                                    gPts.Item(j).VertexCode = 1
                                                End If
                                            End If
                                    End Select
                                End If
                            End If
                        Next j
                    End If
                    'Application.DoEvents()
                Next k
            End If
            For j = 1 To gPts.Count
                v1 = gPts.ItemVector(j)
                If v1.Code = 0 Then
                    _rVal.AddV(v1)
                Else
                    '             GridPoints.AddV v1
                    '            If v1.Code = 2 Then iCnt = iCnt + 1
                End If
                'Application.DoEvents()
            Next j
            Return _rVal
        End Function

        ''' <summary>
        '''returns the arc of the requested radius that will fillet the lines defined by the passed points
        ''' </summary>
        '''><remarks>if the arc can't be created nothing is returned</remarks>
        ''' <param name="aRadius">the radius for the fillet arc</param>
        ''' <param name="aSP">the start point of the first line</param>
        ''' <param name="aMP">the end point of the first and the start of the second</param>
        ''' <param name="aEP">the end point of the second line</param>
        ''' <param name="aPlane">the coordinate system that is defined based on the two lines</param>
        ''' <param name="bReturnCircle"> flag to return the full circle</param>
        ''' <returns></returns>
        Public Shared Function CreateFilletArc(aRadius As Double, aSP As iVector, aMP As iVector, aEP As iVector, Optional aPlane As dxfPlane = Nothing, Optional bReturnCircle As Boolean = False) As dxeArc
            Dim _rVal As dxeArc = Nothing


            Dim bArcTouches1 As Boolean
            Dim bArcTouches2 As Boolean
            Dim d1 As Double = 0
            Dim aAr As TARC = dxfPrimatives.CreateFilletArc(aRadius, New TVECTOR(aSP), New TVECTOR(aMP), New TVECTOR(aEP), New TPLANE(aPlane), bArcTouches1, bArcTouches2, d1)
            If aAr.Radius > 0 Then
                _rVal = New dxeArc(aAr)
            End If
            Return _rVal
        End Function


        ''' <summary>
        '''returns the arc of the requested radius that will fillet the lines defined by the passed points
        ''' </summary>
        '''><remarks>if the arc can't be created nothing is returned</remarks>
        ''' <param name="aRadius">the radius for the fillet arc</param>
        ''' <param name="aSP">the start point of the first line</param>
        ''' <param name="aMP">the end point of the first and the start of the second</param>
        ''' <param name="aEP">the end point of the second line</param>
        ''' <param name="aPlane">the coordinate system that is defined based on the two lines</param>
        ''' <param name="bReturnCircle"> flag to return the full circle</param>
        ''' <returns></returns>
        Friend Shared Function CreateFilletArc(aRadius As Double, aSP As TVECTOR, aMP As TVECTOR, aEP As TVECTOR, aPlane As TPLANE, Optional bReturnCircle As Boolean = False) As TARC
            Dim rArcTouches1 As Boolean = False
            Dim rArcTouches2 As Boolean = False
            Dim rIntersectDistance As Double = 0.0
            Return CreateFilletArc(aRadius, aSP, aMP, aEP, aPlane, rArcTouches1, rArcTouches2, rIntersectDistance, bReturnCircle)
        End Function


        ''' <summary>
        '''returns the arc of the requested radius that will fillet the lines defined by the passed points
        ''' </summary>
        '''><remarks>if the arc can't be created nothing is returned</remarks>
        ''' <param name="aRadius">the radius for the fillet arc</param>
        ''' <param name="aSP">the start point of the first line</param>
        ''' <param name="aMP">the end point of the first and the start of the second</param>
        ''' <param name="aEP">the end point of the second line</param>
        ''' <param name="aPlane">the coordinate system that is defined based on the two lines</param>
        ''' <returns></returns>
        Friend Shared Function CreateFilletArc(aRadius As Double, aSP As TVECTOR, aMP As TVECTOR, aEP As TVECTOR, aPlane As TPLANE, ByRef rArcTouches1 As Boolean, ByRef rArcTouches2 As Boolean, ByRef rIntersectDistance As Double, Optional bReturnCircle As Boolean = False) As TARC
            Dim _rVal As New TARC(0)
            aRadius = Math.Round(Math.Abs(aRadius), 4)
            rArcTouches1 = False
            rArcTouches2 = False
            rIntersectDistance = 0
            If aRadius = 0 Then Return New TARC(0)
            Dim bFlag As Boolean
            Dim aDir As TVECTOR = aMP.DirectionTo(aEP, False, bFlag)
            If bFlag Then Return _rVal 'the direction is null
            Dim bDir As TVECTOR = aMP.DirectionTo(aSP, False, bFlag)
            If bFlag Then Return _rVal 'the direction is null
            If bDir.Equals(aDir, True, 3) Then Return _rVal 'the directions are parallel
            Dim v1 As TVECTOR = aMP + (aDir * 10)
            Dim v2 As TVECTOR = aMP + (bDir * 10)
            Dim ang As Double = aDir.AngleTo(bDir, aPlane.ZDirection)
            If ang > 180 Then
                ang = 360 - ang
                aPlane = TPLANE.ArbitraryCS(bDir.CrossProduct(aDir))
            Else
                aPlane = TPLANE.ArbitraryCS(aDir.CrossProduct(bDir))
            End If
            Dim halfang As Double = (ang * Math.PI / 180) / 2
            Dim cDir As TVECTOR = aMP.DirectionTo(v1.MidPt(v2))

            aPlane.Origin = aMP
            Dim x1 As Double = aRadius / Math.Tan(halfang)
            Dim hyp As Double = aRadius / Math.Cos(halfang)
            Dim arSP As TVECTOR = aMP + (bDir * x1)
            Dim arEP As TVECTOR = aMP + (aDir * x1)
            rIntersectDistance = x1
            ' Dim possibles As TSEGMENTS = dxfPrimatives.ArcsBetweenPoints(aRadius, arSP, arEP, aPlane, bReturnLargerArcs:=False)
            Dim fArcs As List(Of dxeArc) = dxfUtils.ArcsBetweenPoints(aRadius, arSP, arEP, aPlane, bReturnLargerArcs:=False)
            Dim idx As Integer = -1
            Dim fAr As dxeArc


            For i As Integer = 0 To fArcs.Count - 1
                fAr = fArcs(i)
                Dim arMP As TVECTOR = fAr.MidPtV
                v1 = fAr.MidPtV.DirectionTo(fAr.CenterV)
                If v1.Equals(cDir, 2) Then

                    idx = i
                    Exit For
                End If
            Next i
            If idx >= 0 Then
                fAr = fArcs(idx)
                _rVal = fAr.ArcStructure
            Else
                If fArcs.Count > 0 Then
                    fAr = fArcs(0)
                    _rVal = fAr.ArcStructure

                End If
            End If

            rArcTouches1 = dxfProjections.DistanceTo(aMP, _rVal.StartPt, 4) <= dxfProjections.DistanceTo(aMP, aSP, 4)
            rArcTouches2 = dxfProjections.DistanceTo(aMP, _rVal.EndPt, 4) <= dxfProjections.DistanceTo(aMP, aEP, 4)
            If bReturnCircle Then
                _rVal = New TARC(_rVal.Plane, _rVal.Plane.Origin, _rVal.Radius)
            End If
            Return _rVal
        End Function

        Public Shared Function CreateChannel_EndView(aWebCenter As iVector, aTopWidth As Double, aWebHeight As Double, aThickness As Double, Optional aBottomWidth As Double = 0.0, Optional aJoggleHeight As Double = 0.0, Optional aSuppressFillets As Boolean = False, Optional aRotation As Double = 0.0, Optional bFilletEnds As Boolean = False, Optional aSegmentWidth As Double = 0.0, Optional aPlane As dxfPlane = Nothing, Optional aReturnPolygon As Boolean = False, Optional aXOffset As Double = 0.0, Optional aYOffset As Double = 0.0) As dxfPolyline

            Dim v1 As New TVECTOR(aWebCenter)

            If Not dxfPlane.IsNull(aPlane) Then v1 = dxfProjections.ToPlane(v1, aPlane)
            Return CreateChannel_End(v1, aTopWidth, aWebHeight, aThickness, aBottomWidth, aJoggleHeight, aSuppressFillets, aRotation, bFilletEnds, aSegmentWidth, aPlane, aReturnPolygon, aXOffset, aYOffset)
        End Function

        Friend Shared Function CreateChannel_End(aWebCenter As TVECTOR, aTopWidth As Double, aWebHeight As Double, aThickness As Double, Optional aBottomWidth As Double = 0.0, Optional aJoggleHeight As Double = 0.0, Optional aSuppressFillets As Boolean = False, Optional aRotation As Double = 0.0, Optional bFilletEnds As Boolean = False, Optional aSegmentWidth As Double = 0.0, Optional aPlane As dxfPlane = Nothing, Optional aReturnPolygon As Boolean = False, Optional aXOffset As Double = 0.0, Optional aYOffset As Double = 0.0) As dxfPolyline
            '#1the to center center for the channel
            '#2the flange width of the desired channel
            '#3the height of the desired channel
            '#4the thickness of the material
            '^returns as rectangular c channel
            aWebHeight = Math.Abs(aWebHeight)
            aTopWidth = Math.Abs(aTopWidth)
            aBottomWidth = Math.Abs(aBottomWidth)
            aThickness = Math.Abs(aThickness)
            If aWebHeight = 0 Then aWebHeight = 1
            If aThickness = 0 Then aThickness = 0.1
            If aTopWidth <= 0 Then aTopWidth = aWebHeight
            If aBottomWidth <= 0 Then aBottomWidth = aTopWidth
            aJoggleHeight = Math.Abs(aJoggleHeight)
            If aJoggleHeight > aWebHeight - 4 * aThickness Then aJoggleHeight = 0
            Dim rVerts As New colDXFVectors
            Dim aPl As TPLANE
            Dim bPl As TPLANE
            Dim w1 As Double
            Dim w2 As Double
            Dim ht As Double
            Dim thk As Double
            Dim rad1 As Double
            Dim rad2 As Double
            Dim jog As Double
            Dim d1 As Double
            Dim tan22pt5 As Double
            '**UNUSED VAR** Dim v1 As TVERTEX
            '**UNUSED VAR** Dim P1 As dxfVector
            Dim aPln As dxfPlane
            ht = 0.5 * aWebHeight
            w1 = aTopWidth
            w2 = aBottomWidth
            thk = aThickness
            rad1 = thk
            rad2 = 2 * thk
            jog = aJoggleHeight
            tan22pt5 = 0.4142135624
            If Not dxfPlane.IsNull(aPlane) Then aPl = New TPLANE(aPlane) Else aPl = TPLANE.World
            aPl.Origin = aWebCenter
            bPl = aPl
            If aRotation <> 0 Then bPl.RotateAbout(aPl.Origin, aPl.ZDirection, aRotation, False, False, True, True)
            aPln = New dxfPlane(bPl)
            If aSuppressFillets Then
                If jog = 0 Then
                    rVerts.Add(aPln, -w1 + aXOffset, ht + aYOffset)
                    rVerts.Add(aPln, aXOffset, ht + aYOffset)
                    rVerts.Add(aPln, aXOffset, -ht + aYOffset)
                    rVerts.Add(aPln, -w2 + aXOffset, -ht + aYOffset)
                    rVerts.Add(aPln, -w2 + aXOffset, -ht + thk + aYOffset)
                    rVerts.Add(aPln, -thk + aXOffset, -ht + thk + aYOffset)
                    rVerts.Add(aPln, -thk + aXOffset, ht - thk + aYOffset)
                    rVerts.Add(aPln, -w1 + aXOffset, ht - thk + aYOffset)
                Else
                    d1 = tan22pt5 * thk
                    rVerts.Add(aPln, -w1 + aXOffset, ht + aYOffset)
                    rVerts.Add(aPln, aXOffset, ht + aYOffset)
                    rVerts.Add(aPln, aXOffset, -ht + jog + thk + aYOffset)
                    rVerts.Add(aPln, -thk + aXOffset, -ht + jog + aYOffset)
                    rVerts.Add(aPln, -thk + aXOffset, -ht + aYOffset)
                    rVerts.Add(aPln, -w2 - thk + aXOffset, -ht + aYOffset)
                    rVerts.Add(aPln, -w2 - thk + aXOffset, -ht + thk + aYOffset)
                    rVerts.Add(aPln, -2 * thk + aXOffset, -ht + thk + aYOffset)
                    rVerts.Add(aPln, -2 * thk + aXOffset, -ht + jog + d1 + aYOffset)
                    rVerts.Add(aPln, -thk + aXOffset, -ht + jog + thk + d1 + aYOffset)
                    rVerts.Add(aPln, -thk + aXOffset, ht - thk + aYOffset)
                    rVerts.Add(aPln, -w1 + aXOffset, ht - thk + aYOffset)
                End If
            Else
                If jog = 0 Then
                    If Not bFilletEnds Then
                        rVerts.Add(aPln, -w1 + aXOffset, ht + aYOffset)
                    Else
                        rVerts.Add(aPln, -w1 + thk / 2 + aXOffset, ht + aYOffset)
                    End If
                    rVerts.Add(aPln, -rad2 + aXOffset, ht + aYOffset, aVertexRadius:=-rad2)
                    rVerts.Add(aPln, aXOffset, ht - rad2 + aYOffset)
                    rVerts.Add(aPln, aXOffset, -ht + rad2 + aYOffset, aVertexRadius:=-rad2)
                    rVerts.Add(aPln, -rad2 + aXOffset, -ht + aYOffset)
                    If Not bFilletEnds Then
                        rVerts.Add(aPln, -w2 + aXOffset, -ht + aYOffset)
                        rVerts.Add(aPln, -w2 + aXOffset, -ht + thk + aYOffset)
                    Else
                        rVerts.Add(aPln, -w2 + thk / 2 + aXOffset, -ht + aYOffset, aVertexRadius:=-rad1 / 2)
                        rVerts.Add(aPln, -w2 + aXOffset, -ht + 0.5 * thk + aYOffset, aVertexRadius:=-rad1 / 2)
                        rVerts.Add(aPln, -w2 + thk / 2 + aXOffset, -ht + thk + aYOffset)
                    End If
                    rVerts.Add(aPln, -thk - rad1 + aXOffset, -ht + thk + aYOffset, aVertexRadius:=rad1)
                    rVerts.Add(aPln, -thk + aXOffset, -ht + 2 * thk + aYOffset)
                    rVerts.Add(aPln, -thk + aXOffset, ht - 2 * thk + aYOffset, aVertexRadius:=rad1)
                    rVerts.Add(aPln, -thk - rad1 + aXOffset, ht - thk + aYOffset)
                    If Not bFilletEnds Then
                        rVerts.Add(aPln, -w1 + aXOffset, ht - thk + aYOffset)
                    Else
                        rVerts.Add(aPln, -w1 + thk / 2 + aXOffset, ht - thk + aYOffset, aVertexRadius:=-rad1 / 2)
                        rVerts.Add(aPln, -w1 + aXOffset, ht - thk / 2 + aYOffset, aVertexRadius:=-rad1 / 2)
                        rVerts.Add(aPln, -w1 + thk / 2 + aXOffset, ht + aYOffset)
                    End If
                Else
                    d1 = tan22pt5 * thk
                    If Not bFilletEnds Then
                        rVerts.Add(aPln, -w1 + aXOffset, ht + aYOffset)
                    Else
                        rVerts.Add(aPln, -w1 + thk / 2 + aXOffset, ht + aYOffset)
                    End If
                    rVerts.Add(aPln, -rad2 + aXOffset, ht + aYOffset, aVertexRadius:=-rad2)
                    rVerts.Add(aPln, aXOffset, ht - rad2 + aYOffset)
                    rVerts.Add(aPln, aXOffset, -ht + jog + thk + aYOffset)
                    rVerts.Add(aPln, -thk + aXOffset, -ht + jog + aYOffset)
                    rVerts.Add(aPln, -thk + aXOffset, -ht + rad2 + aYOffset, aVertexRadius:=-rad2)
                    rVerts.Add(aPln, -thk - rad2 + aXOffset, -ht + aYOffset)
                    If Not bFilletEnds Then
                        rVerts.Add(aPln, -w2 - thk + aXOffset, -ht + aYOffset)
                        rVerts.Add(aPln, -w2 - thk + aXOffset, -ht + thk + aYOffset)
                    Else
                        rVerts.Add(aPln, -w2 - thk + thk / 2 + aXOffset, -ht + aYOffset, aVertexRadius:=-thk / 2)
                        rVerts.Add(aPln, -w2 - thk + aXOffset, -ht + thk / 2 + aYOffset, aVertexRadius:=-thk / 2)
                        rVerts.Add(aPln, -w2 - thk + thk / 2 + aXOffset, -ht + thk + aYOffset)
                    End If
                    rVerts.Add(aPln, -2 * thk - rad1 + aXOffset, -ht + thk + aYOffset, aVertexRadius:=rad1)
                    rVerts.Add(aPln, -2 * thk + aXOffset, -ht + thk + rad1 + aYOffset)
                    rVerts.Add(aPln, -2 * thk + aXOffset, -ht + jog + d1 + aYOffset)
                    rVerts.Add(aPln, -thk + aXOffset, -ht + jog + thk + d1 + aYOffset)
                    rVerts.Add(aPln, -thk + aXOffset, ht - thk - rad1 + aYOffset, aVertexRadius:=rad1)
                    rVerts.Add(aPln, -thk - rad1 + aXOffset, ht - thk + aYOffset)
                    If Not bFilletEnds Then
                        rVerts.Add(aPln, -w1 + aXOffset, ht - thk + aYOffset)
                    Else
                        rVerts.Add(aPln, -w1 + thk / 2 + aXOffset, ht - thk + aYOffset, aVertexRadius:=-thk / 2)
                        rVerts.Add(aPln, -w1 + aXOffset, ht - thk / 2 + aYOffset, aVertexRadius:=-thk / 2)
                    End If
                End If
            End If
            Return dxfPrimatives.CreatePGorPL2(aReturnPolygon, bPl, True, rVerts, aSegmentWidth)
        End Function

        Friend Shared Function CreateChannel_Side(aTopCenter As TVECTOR, aLength As Double, aHeight As Double, aThickness As Double, Optional bBackSide As Boolean = False, Optional aRotation As Double = 0.0, Optional aSegmentWidth As Double = 0.0, Optional aPlane As dxfPlane = Nothing, Optional aReturnPolygon As Boolean = False) As dxfPolyline
            Dim _rVal As dxfPolyline
            '#1the to center for the channel
            '#2the top flange width of the desired channel
            '#3the height of the desired channel
            '#4the thickness of the material
            '^returns as rectangular c channel
            aLength = Math.Abs(aLength)
            aHeight = Math.Abs(aHeight)
            aThickness = Math.Abs(aThickness)
            If aLength = 0 Then aLength = 1
            If aThickness = 0 Then aThickness = 0.1
            Dim verts As New TVERTICES
            Dim rVerts As colDXFVectors
            Dim i As Integer
            Dim aPl As New TPLANE("")
            Dim bPl As New TPLANE("")
            Dim lg As Double
            Dim thk As Double
            Dim ht As Double
            lg = 0.5 * aLength
            thk = aThickness
            ht = 0.5 * aHeight
            verts = New TVERTICES(0)
            verts.Add(-lg, ht - thk)
            verts.Add(-lg, ht)
            verts.Add(lg, ht)
            verts.Add(lg, -ht)
            verts.Add(-lg, -ht)
            verts.Add(-lg, ht - thk)
            verts.Add(lg, ht - thk)
            verts.Add(lg, -ht + thk)
            verts.Add(-lg, -ht + thk)
            If Not dxfPlane.IsNull(aPlane) Then aPl = New TPLANE(aPlane) Else aPl = TPLANE.World
            aPl.Origin = aTopCenter
            bPl = aPl
            If aRotation <> 0 Then bPl.RotateAbout(aPl.Origin, aPl.ZDirection, aRotation, False, False, True, True)
            For i = 1 To verts.Count
                verts.SetVector(bPl.WorldVector(verts.Vector(i)), i)
            Next i
            rVerts = New colDXFVectors(verts)
            If bBackSide Then
                rVerts.Item(6).Linetype = dxfLinetypes.Hidden
                rVerts.Item(8).Linetype = dxfLinetypes.Hidden
            End If
            _rVal = dxfPrimatives.CreatePGorPL2(aReturnPolygon, aPl, False, rVerts, aSegmentWidth)
            Return _rVal
        End Function

        Friend Shared Function CreateArcThreePoint(aSP As TVECTOR, aMP As TVECTOR, aEP As TVECTOR, bSuppressErrs As Boolean, aPlane As TPLANE) As dxeArc
            Dim rError As String = String.Empty
            Return CreateArcThreePoint(aSP, aMP, aEP, bSuppressErrs, aPlane, rError)
        End Function

        Friend Shared Function CreateArcThreePoint(aSP As TVECTOR, aMP As TVECTOR, aEP As TVECTOR, bSuppressErrs As Boolean, aPlane As TPLANE, ByRef rError As String) As dxeArc
            rError = String.Empty
            '#1the first vector
            '#2the second vector
            '#3the third vector
            '^returns a dxeArc passing through all three of the passed vectors
            '~no errors raised if the arc can't be defined.
            '~the first vector is assumed as the arc start vector and the last vector is assummed as the end.

            Try
                Dim aArc As TARC = dxfPrimatives.ArcThreePointV(aSP, aMP, aEP, True, aPlane, False, rError:=rError)
                If Not String.IsNullOrWhiteSpace(rError) Then Throw New Exception(rError)
                Return New dxeArc(aArc)
            Catch ex As Exception
                rError = ex.Message
                If Not bSuppressErrs Then
                    Throw ex
                End If
            End Try
            Return Nothing
        End Function

        Friend Shared Function ArcThreePointV(aSP As TVECTOR, aMP As TVECTOR, aEP As TVECTOR, bSuppressErrs As Boolean, aPlane As TPLANE, Optional bSuppressProjection As Boolean = False) As TARC
            Dim rError As String = String.Empty
            Return ArcThreePointV(aSP, aMP, aEP, bSuppressErrs, aPlane, bSuppressProjection, rError)
        End Function

        Friend Shared Function ArcThreePointV(aSP As TVECTOR, aMP As TVECTOR, aEP As TVECTOR, bSuppressErrs As Boolean, aPlane As TPLANE, bSuppressProjection As Boolean, ByRef rError As String) As TARC
            Dim _rVal As TARC
            rError = String.Empty
            '#1the first vector
            '#2the second vector
            '#3the third vector
            '^returns a TARC passing through all three of the passed vectors
            '~no errors raised if the arc can't be defined.
            '~the first vector is assumed as the arc start vector and the last vector is assummed as the end.
            Dim m1 As TVECTOR
            Dim m2 As TVECTOR
            Dim rad As Double
            Dim eStr As String = String.Empty
            Dim d1 As TVECTOR
            Dim d2 As TVECTOR
            Dim cp As TVECTOR
            Dim ang1 As Double
            Dim ang2 As Double
            Dim pN As TVECTOR = aPlane.ZDirection
            Dim sp As TVECTOR = aSP
            Dim ep As TVECTOR = aEP
            Dim mp As TVECTOR = aMP
            Dim aAl As New TSEGMENT
            Try
                If Not bSuppressProjection Then
                    sp = sp.ProjectedTo(aPlane)
                    ep = ep.ProjectedTo(aPlane)
                    mp = mp.ProjectedTo(aPlane)
                End If
                If sp.Equals(mp, -1) Then Throw New Exception("Coincident Point Passed")
                If sp.Equals(ep, -1) Then Throw New Exception("Coincident Point Passed")
                If ep.Equals(mp, -1) Then Throw New Exception("Coincident Point Passed")
                Dim l1 As New TLINE(sp, ep)
                Dim l2 As New TLINE(mp, ep)
                d1 = l1.Direction(True) '.EPT.DirectionTo(l1.SPT)
                d2 = l2.Direction(True) ' .EPT.DirectionTo(l2.SPT)
                m1 = l1.SPT.Interpolate(l1.EPT, 0.5)
                m2 = l2.SPT.Interpolate(l2.EPT, 0.5)
                d1 = d1.RotatedAbout(pN, 90, bSuppressNorm:=True)
                d2 = d2.RotatedAbout(pN, 90, bSuppressNorm:=True)
                Dim R1 As New TLINE(m1, m1 + d1 * 100)
                Dim R2 As New TLINE(m2, m2 + d2 * 100)
                cp = R1.IntersectionPt(R2)
                'get the radius
                d1 = cp.DirectionTo(sp, False, rad)
                d2 = cp.DirectionTo(ep)
                ep = cp + d2 * rad
                mp = cp + cp.DirectionTo(mp) * rad
                sp = cp + d1 * rad
                ang1 = aPlane.XDirection.AngleTo(d1, pN)
                ang2 = aPlane.XDirection.AngleTo(d2, pN)
                aPlane.Origin = cp
                _rVal = New TARC With {
                   .Plane = aPlane,
                   .StartAngle = ang1,
                   .EndAngle = ang2,
                   .Radius = rad
                }
                '.SpannedAngle = SpannedAngle(False, ang1, ang2)
                aAl.IsArc = True
                '.StartPt = sp
                '.EndPt = ep
                aAl.ArcStructure = _rVal
                If Not aAl.ContainsVector(mp, aFudgeFactor:=0.01, bSuppressPlaneCheck:=True) Then
                    _rVal.ClockWise = True
                End If
                Return _rVal
            Catch ex As Exception
                rError = ex.Message
                If Not bSuppressErrs Then Throw ex
                Return New TARC()
            End Try
        End Function

        ''' <summary>
        ''' returns an arc  starting at the first vector ending at the second vector with the requested radius
        ''' </summary>
        ''' <remarks>an error is raised if the requested arc cannot be defined</remarks>
        ''' <param name="aRadius">the radius of the arc to create</param>
        ''' <param name="aSP">the first vector</param>
        ''' <param name="aEP">the second vector</param>
        ''' <param name="aPlane">the plane of the arc </param>
        ''' <param name="bClockwise">flag to return the clockwise version of the arc</param>
        ''' <param name="aReturnLargerArc">flag to return the larger arc of the possible returns</param>
        ''' <param name="bSuppressErrs">flag to suppress any exceptions from being thrown</param>
        ''' <param name="aDisplaySettings">a display settings to apply</param>
        ''' <returns></returns>
        Friend Shared Function ArcBetweenPointsV(aRadius As Double, aSP As TVECTOR, aEP As TVECTOR, aPlane As TPLANE, Optional bClockwise As Boolean? = Nothing, Optional aReturnLargerArc As Boolean = False, Optional bSuppressErrs As Boolean = False, Optional aDisplaySettings As dxfDisplaySettings = Nothing) As TSEGMENT
            Dim rArcDefined As Boolean = False
            Return ArcBetweenPointsV(aRadius, aSP, aEP, aPlane, bClockwise, aReturnLargerArc, bSuppressErrs, rArcDefined, aDisplaySettings)
        End Function

        ''' <summary>
        ''' returns an arc  starting at the first vector ending at the second vector with the requested radius
        ''' </summary>
        ''' <remarks>an error is raised if the requested arc cannot be defined</remarks>
        ''' <param name="aRadius">the radius of the arc to create</param>
        ''' <param name="aSP">the first vector</param>
        ''' <param name="aEP">the second vector</param>
        ''' <param name="aPlane">the plane of the arc </param>
        ''' <param name="bClockwise">flag to return the clockwise version of the arc</param>
        ''' <param name="aReturnLargerArc">flag to return the larger arc of the possible returns</param>
        ''' <param name="bSuppressErrs">flag to suppress any exceptions from being thrown</param>
        ''' <param name="rArcDefined"> returns true if the arc  exists</param>
        ''' <param name="aDisplaySettings">a display settings to apply</param>
        ''' <returns></returns>
        Friend Shared Function ArcBetweenPointsV(aRadius As Double, aSP As TVECTOR, aEP As TVECTOR, aPlane As TPLANE, bClockwise As Boolean?, aReturnLargerArc As Boolean, bSuppressErrs As Boolean, ByRef rArcDefined As Boolean, Optional aDisplaySettings As dxfDisplaySettings = Nothing) As TSEGMENT

            Dim sErr As String = String.Empty
            Dim _rVal As New TSEGMENT()
            aRadius = Math.Abs(aRadius)
            rArcDefined = False
            Try
                If aRadius = 0 Then
                    sErr = "Invalid Radius Passed"
                    Return _rVal

                End If
                Dim span1 As Double = dxfProjections.DistanceTo(aSP, aEP)
                If span1 <= 0.0001 Then
                    sErr = "The Passed Vectors Are Coincident"
                    Return _rVal
                End If
                If span1 > 2 * aRadius + 0.00001 Then
                    sErr = "Arc Doesn't Exist"
                    Return _rVal
                End If
                Dim aArcs As TSEGMENTS = dxfPrimatives.ArcsBetweenPoints(aRadius, aSP, aEP, aPlane, bClockwise, aReturnLargerArc, aDisplaySettings)
                rArcDefined = aArcs.Count > 0
                If Not rArcDefined Then
                    sErr = "Arc Doesn't Exist"
                    Return _rVal
                Else
                    _rVal = aArcs.Item(1)
                End If
            Catch ex As Exception
                sErr = ex.Message
                _rVal = New TSEGMENT()
            Finally
                If Not bSuppressErrs And sErr <> "" Then Throw New Exception(sErr)
            End Try
            Return _rVal

        End Function
        ''' <summary>
        ''' returns all the possible arcs bewteen the two passed vectors with the passed radius.
        ''' </summary>
        ''' <remarks>an error is raised if the requested arcs cannot be defined</remarks>
        ''' <param name="aRadius">the radius of the arcs to create </param>
        ''' <param name="aSP">the first vector</param>
        ''' <param name="aEP">the second vector</param>
        ''' <param name="aPlane">the arc plane</param>
        ''' <param name="bClockwise">flag to return clockwise arcs otherwise counterclockwise arcs are returned</param>
        ''' <param name="bReturnLargerArcs">flag to return the longer of the possible arcs</param>
        ''' <param name="aDisplaySettings">a display settings to apply</param>
        ''' <returns></returns>
        Public Shared Function ArcsBetweenPoints(aRadius As Double, aSP As iVector, aEP As iVector, Optional aPlane As dxfPlane = Nothing, Optional bClockwise As Boolean? = Nothing, Optional bReturnLargerArcs As Boolean = False, Optional aDisplaySettings As dxfDisplaySettings = Nothing) As List(Of dxeArc)
            Dim rArcs As New List(Of dxeArc)

            Try
                Dim arcs As TSEGMENTS = ArcsBetweenPoints(aRadius, New TVECTOR(aSP), New TVECTOR(aEP), New TPLANE(aPlane), bClockwise, bReturnLargerArcs, aDisplaySettings)
                For i As Integer = 1 To arcs.Count
                    rArcs.Add(New dxeArc(arcs.Item(i).ArcStructure, aDisplaySettings))
                Next
            Catch ex As Exception
                Throw ex
            End Try
            Return rArcs
        End Function
        ''' <summary>
        ''' returns all the possible arcs bewteen the two passed vectors with the passed radius.
        ''' </summary>
        ''' <remarks>an error is raised if the requested arcs cannot be defined</remarks>
        ''' <param name="aRadius">the radius of the arcs to create </param>
        ''' <param name="aSP">the first vector</param>
        ''' <param name="aEP">the second vector</param>
        ''' <param name="aPlane">the arc plane</param>
        ''' <param name="aClockwise">flag to return clockwise arcs otherwise counterclockwise arcs are returned</param>
        ''' <param name="bReturnLargerArcs">flag to return the longer of the possible arcs</param>
        ''' <param name="aDisplaySettings">a display settings to apply</param>
        ''' <returns></returns>
        Friend Shared Function ArcsBetweenPoints(aRadius As Double, aSP As TVECTOR, aEP As TVECTOR, aPlane As TPLANE, Optional aClockwise As Boolean? = Nothing, Optional bReturnLargerArcs As Boolean = False, Optional aDisplaySettings As dxfDisplaySettings = Nothing) As TSEGMENTS
            Dim rArcs As New TSEGMENTS(0)
            Dim rad As Double = Math.Abs(aRadius)
            If rad = 0 Then Return rArcs

            Dim aPl As New TPLANE(aPlane)
            Dim sp = dxfProjections.ToPlane(aSP, aPl)
            Dim ep = dxfProjections.ToPlane(aEP, aPl)

            Dim span1 As Double
            Dim vDir As TVECTOR = sp.DirectionTo(ep, False, rDistance:=span1)
            If span1 <= 0.0001 Then Return rArcs
            If span1 > 2 * rad + 0.00001 Then Return rArcs
            Dim aN As TVECTOR = aPl.ZDirection
            Dim xDir As TVECTOR = aPl.XDirection
            Dim aArc As TARC
            Dim bArc As TARC
            Dim ips As TVECTORS
            Dim c1 As TVECTOR = sp + vDir * (0.5 * span1)
            aPl.Origin = c1
            Dim Arc1 As TSEGMENT
            Dim Arc2 As TSEGMENT
            Dim dsp As TDISPLAYVARS = New TDISPLAYVARS("0", dxfLinetypes.ByLayer, dxxColors.ByLayer, dxxLineWeights.ByLayer, 1)
            If aDisplaySettings IsNot Nothing Then dsp = aDisplaySettings.Strukture
            Dim bClockwise As Boolean
            Dim bAll As Boolean = True

            If aClockwise.HasValue Then
                bAll = False
                bClockwise = aClockwise.Value
            End If


            Try

                'there is only one center (2 or 4 Arcs)
                If span1 >= 2 * rad Then
                    'one center at the midpt between the arcs
                    'so there are only 2 arcs (semi-circles) that span the pts
                    'with the radius. one clock wise one counter clockwise
                    'rad = 0.5 * span1
                    aArc = TARC.DefineWithPoints(aPl, c1, sp, ep, bClockwise)

                    rArcs.Add(aArc)
                    rArcs.SetDisplayStructure(1, dsp)
                Else
                    'put an arc on each point and get their intersections
                    aArc = New TARC(aPl, sp, rad)
                    bArc = New TARC(aArc)
                    bArc.Plane.Origin = ep
                    ips = aArc.IntersectionPts(bArc, True)
                    'the two returned intersection pts are the centers for the four possible
                    'arcs that span the points with the radius.one clockwise and one counter
                    'clockwise at each center.
                    rArcs.Clear()
                    For i As Integer = 1 To ips.Count
                        c1 = ips.Item(i)
                        aArc = TARC.DefineWithPoints(aPl, c1, sp, ep, bClockwise, bSuppressProjection:=True)
                        Arc1 = New TSEGMENT(aArc)
                        rArcs.Add(Arc1)
                        rArcs.SetDisplayStructure(rArcs.Count, dsp)
                    Next i
                    If rArcs.Count = 2 Then
                        Arc1 = rArcs.Item(1)
                        Arc2 = rArcs.Item(2)
                        aArc = Arc1.ArcStructure
                        bArc = Arc2.ArcStructure
                        rArcs.Clear()
                        If bReturnLargerArcs Then
                            If aArc.SpannedAngle > bArc.SpannedAngle Then
                                rArcs.Add(Arc1)
                            Else
                                rArcs.Add(Arc2)
                            End If
                        Else
                            If aArc.SpannedAngle < bArc.SpannedAngle Then
                                rArcs.Add(Arc1)
                            Else
                                rArcs.Add(Arc2)
                            End If
                        End If
                    End If
                End If
                If bAll Then
                    For i As Integer = 1 To rArcs.Count
                        Arc1 = New TSEGMENT(rArcs.Item(i))
                        Arc1.ArcStructure.ClockWise = Not Arc1.ArcStructure.ClockWise
                        'als_UpdateArcPoints(Arc1)
                        rArcs.Add(Arc1)
                    Next i
                End If

            Catch ex As Exception
                Throw ex
            End Try
            Return rArcs

        End Function

        Friend Shared Function CreateChannel_Top(aLongCenter As TVECTOR, aTopWidth As Double, aLength As Double, aThickness As Double, Optional aBottomWidth As Double = 0.0, Optional aRotation As Double = 0.0, Optional aSegmentWidth As Double = 0.0, Optional aPlane As dxfPlane = Nothing, Optional aReturnPolygon As Boolean = False) As dxfPolyline
            Dim _rVal As dxfPolyline
            '#1the to center for the channel
            '#2the top flange width of the desired channel
            '#3the height of the desired channel
            '#4the thickness of the material
            '^returns as rectangular c channel
            aLength = Math.Abs(aLength)
            aTopWidth = Math.Abs(aTopWidth)
            aBottomWidth = Math.Abs(aBottomWidth)
            aThickness = Math.Abs(aThickness)
            If aLength = 0 Then aLength = 1
            If aThickness = 0 Then aThickness = 0.1
            If aTopWidth <= 0 Then aTopWidth = aLength
            If aBottomWidth <= 0 Then aBottomWidth = aTopWidth
            Dim verts As New TVERTICES
            Dim rVerts As colDXFVectors
            Dim i As Integer
            Dim aPl As New TPLANE("")
            Dim bPl As New TPLANE("")
            Dim w1 As Double
            Dim w2 As Double
            Dim lg As Double
            Dim thk As Double
            lg = 0.5 * aLength
            w1 = aTopWidth
            w2 = aBottomWidth
            thk = aThickness
            verts = New TVERTICES(0)
            TVALUES.SortTwoValues(True, w2, w1)
            verts.Add(-thk, lg)
            verts.Add(-thk, -lg)
            verts.Add(0, -lg)
            verts.Add(0, lg)
            verts.Add(-w1, lg)
            verts.Add(-w1, -lg)
            verts.Add(-thk, -lg)
            If w2 <> w1 Then
                verts.Add(-w2, -lg)
                verts.Add(-w2, lg)
            End If
            If Not dxfPlane.IsNull(aPlane) Then aPl = New TPLANE(aPlane) Else aPl = TPLANE.World
            aPl.Origin = aLongCenter
            bPl = aPl
            If aRotation <> 0 Then bPl.RotateAbout(aPl.Origin, aPl.ZDirection, aRotation, False, False, True, True)
            For i = 1 To verts.Count
                verts.SetVector(bPl.WorldVector(verts.Vector(i)), i)
            Next i
            rVerts = New colDXFVectors(verts)
            rVerts.Item(1).Linetype = dxfLinetypes.Hidden
            If aBottomWidth < aTopWidth Then
                rVerts.Item(8).Linetype = dxfLinetypes.Hidden
            End If
            _rVal = dxfPrimatives.CreatePGorPL2(aReturnPolygon, aPl, False, rVerts, aSegmentWidth)
            Return _rVal
        End Function
        Public Shared Function FilletArc(aLine As iLine, bLine As iLine, aRadius As Double) As dxeArc
            Dim rErrorString As String = String.Empty
            Return FilletArc(aLine, bLine, aRadius, rErrorString)

        End Function
        Public Shared Function FilletArc(aLine As iLine, bLine As iLine, aRadius As Double, ByRef rErrorString As String) As dxeArc

            Dim rInterceptExists As Boolean = False
            Dim rLinesAreParallel As Boolean = False
            Dim rLinesAreCoincident As Boolean = False
            If aLine Is Nothing Or bLine Is Nothing Then
                rErrorString = $"One or both of the passed lines are null"
                Return Nothing
            End If
            Dim aFlag As Boolean
            Dim bFlag As Boolean
            Dim l1 As New TLINE(aLine)
            Dim l2 As New TLINE(bLine)


            Dim ip As TVECTOR = l1.IntersectionPt(l2, rLinesAreParallel, rLinesAreCoincident, aFlag, bFlag, rInterceptExists)
            If rLinesAreParallel Then
                rErrorString = $"The passed lines are parrallel"
                Return Nothing
            End If
            If rLinesAreCoincident Then
                rErrorString = $"The passed lines are coincindent"
                Return Nothing
            End If
            If Not rInterceptExists Then
                rErrorString = $"The passed lines do not intersect"
                Return Nothing
            End If

            If l1.SPT.DistanceTo(ip) < l1.EPT.DistanceTo(ip) Then l1.Invert()
            If l2.SPT.DistanceTo(ip) < l2.EPT.DistanceTo(ip) Then l2.Invert()
            Dim d1 As TVECTOR = l1.Direction
            Dim d2 As TVECTOR = l2.Direction
            Dim zDir As TVECTOR = d1.CrossProduct(d2, True)
            If Math.Abs(zDir.Z) = 1 Then zDir = TVECTOR.WorldZ
            'both lines are on this plane
            Dim plane As TPLANE = TPLANE.ArbitraryCS(zDir, True, False)
            ' plane.Define(ip, d1, zDir.CrossProduct(d1, True))
            Dim sp1 As TVECTOR = ip + d1 * -aRadius
            Dim sp2 As TVECTOR = ip + d2 * -aRadius
            Dim d As Double
            Dim aAr As TARC = dxfPrimatives.CreateFilletArc(aRadius, sp1, ip, sp2, plane, aFlag, bFlag, d)
            If aAr.Radius <= 0 Then
                rErrorString = $"the fillet arc could not be created"
                Return Nothing 'the fillet arc could not be created
            End If
            'If Not aFlag Or Not bFlag Then Return Nothing 'the arcs end points don't don't lie on the lines formed by the points

            Return New dxeArc(aAr)
        End Function
#End Region 'Shared Methods
    End Class 'dxfPrimatives
End Namespace
