



Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics.Utilities

#Region "Structures"
    Public Class dxfProjections
#Region "Shared Methods"
        Friend Shared Function LineAcrossLine(aLine As TLINE, bLine As TLINE) As TLINE

            '^mirrors the first line across the second
            Dim sp As TVECTOR = aLine.SPT.Clone
            Dim ep As TVECTOR = aLine.EPT.Clone
            Dim d1 As Double = sp.DistanceTo(ep, 8)
            Dim v1 As TVECTOR
            If d1 > 0 Then
                v1 = dxfProjections.ToLine(sp, bLine, d1)
                If d1 > 0 Then sp += sp.DirectionTo(v1) * (2 * d1)
                v1 = dxfProjections.ToLine(ep, bLine, d1)
                If d1 > 0 Then ep += ep.DirectionTo(v1) * (2 * d1)
            End If
            Return New TLINE(sp, ep)
        End Function
        Friend Shared Function ToPlane(aPoints As TPOINTS, aPlane As TPLANE) As TPOINTS
            Dim ioDirection As dxfDirection = Nothing
            Return ToPlane(aPoints, aPlane, ioDirection)
        End Function
        Friend Shared Function ToPlane(aPoints As TPOINTS, aPlane As TPLANE, ByRef ioDirection As dxfDirection) As TPOINTS
            Dim _rVal As New TPOINTS(aPoints)
            If TPLANE.IsNull(aPlane) Then Return _rVal
            '#1the vector to project
            '#2the structure of the plane to project to
            '#3 the project direction
            '^projects the passeds vector to the passed plane
            '~if the passed direction is a null vector then the direction is assumed as the normal of the plane
            'On Error Resume Next
            _rVal = aPoints
            If aPoints.Count <= 0 Then Return _rVal
            Dim i As Integer
            Dim aDir As TVECTOR
            Dim v1 As TVECTOR
            If ioDirection IsNot Nothing Then
                aDir = ioDirection.Strukture
            Else
                aDir = aPlane.ZDirection * -1
                ioDirection = New dxfDirection With {.Strukture = aDir}
            End If
            For i = 1 To aPoints.Count
                v1 = CType(aPoints.Item(i), TVECTOR)
                v1 = dxfProjections.ToPlane(v1, aPlane, aDir)
                _rVal.SetItem(i, CType(v1, TPOINT))
            Next i
            Return _rVal
        End Function
        Friend Shared Function ToPlane(aVertex As TVERTEX, aPlane As TPLANE, aDirection As TVECTOR) As TVERTEX
            Dim rDistance As Double = 0.0
            Dim rAntiNormal As Boolean = False

            Return ToPlane(aVertex, aPlane, aDirection, rDistance, rAntiNormal)
        End Function
        Friend Shared Function ToPlane(aVertex As TVERTEX, aPlane As TPLANE, aDirection As TVECTOR, ByRef rDistance As Double, ByRef rAntiNormal As Boolean) As TVERTEX
            '#1the vector to project
            '#2the structure of the plane to project to
            '#3 the project direction
            '#4returns the distance to the plane in the passed direction
            '#5returns true if the direction from the vector to the intercept is the opposite of the projection direction
            '#6flag to convert the returned vector to coordinates with repect to the planes origin and direction
            '^projects the passed vector to the passed plane
            '~if the passed direction is a null vector then the direction is assumed as the normal of the plane
            rDistance = 0

            Dim _rVal As TVERTEX = New TVERTEX(aVertex)
            If TPLANE.IsNull(aPlane) Then Return _rVal

            _rVal.Vector = dxfProjections.ToPlane(_rVal.Vector, aPlane, aDirection, rDistance, rAntiNormal)
            Return _rVal
        End Function

        Friend Shared Function ToPlane(aVector As TVECTOR, aPlane As dxfPlane, aDirection As iVector) As TVECTOR
            '#1the vector to project
            '#2the plane to project to
            '#3 the project direction
            '^projects the passed vector to the passed plane using the passed direction
            '~if the passed direction is a null vector then the direction is assumed as the normal of the plane
            Dim _rVal As New TVECTOR(aVector)
            If TPLANE.IsNull(aPlane) Then Return _rVal

            Dim pplane As New TPLANE(aPlane)
            Dim rDistance As Double = 0.0
            Dim rAntiNormal As Boolean = False
            Dim dir As TVECTOR = IIf(aDirection Is Nothing, pplane.ZDirection, New TVECTOR(aDirection, True))
            Return ToPlane(aVector, pplane, dir, rDistance:=rDistance, rAntiNormal:=rAntiNormal)
        End Function

        Friend Shared Function ToPlane(aVector As TVECTOR, aPlane As dxfPlane, aDirection As iVector, ByRef rDistance As Double) As TVECTOR
            '#1the vector to project
            '#2the plane to project to
            '#3 the project direction
            '^projects the passed vector to the passed plane using the passed direction
            '~if the passed direction is a null vector then the direction is assumed as the normal of the plane
            Dim _rVal As New TVECTOR(aVector)
            If TPLANE.IsNull(aPlane) Then Return _rVal

            Dim pplane As New TPLANE(aPlane)

            Dim rAntiNormal As Boolean = False
            Dim dir As TVECTOR = IIf(aDirection Is Nothing, pplane.ZDirection, New TVECTOR(aDirection, True))
            Return ToPlane(aVector, pplane, dir, rDistance:=rDistance, rAntiNormal:=rAntiNormal)
        End Function
        Friend Shared Function ToPlane(aVector As TVECTOR, aPlane As dxfPlane, ByRef rDistance As Double) As TVECTOR
            '#1the vector to project
            '#2the plane to project to
            '#3returns the distance to the plane in the passed direction
            '^projects the passed vector to the passed plane using the normal of the plane as the direction
            Dim _rVal As New TVECTOR(aVector)
            If TPLANE.IsNull(aPlane) Then Return _rVal

            Dim pplane As New TPLANE(aPlane)

            Dim rAntiNormal As Boolean = False
            Dim dir As TVECTOR = pplane.ZDirection
            Return ToPlane(aVector, pplane, dir, rDistance:=rDistance, rAntiNormal:=rAntiNormal)
        End Function
        Friend Shared Function ToPlane(aVector As TVECTOR, aPlane As TPLANE, aDirection As TVECTOR) As TVECTOR
            '#1the vector to project
            '#2the plane to project to
            '#3 the project direction
            '^projects the passed vector to the passed plane using the passed direction
            '~if the passed direction is a null vector then the direction is assumed as the normal of the plane
            Dim rDistance As Double = 0.0
            Dim rAntiNormal As Boolean = False

            Return ToPlane(aVector, aPlane, aDirection, rDistance:=rDistance, rAntiNormal:=rAntiNormal)
        End Function
        Friend Shared Function ToPlane(aVector As TVECTOR, aPlane As TPLANE, ByRef rDistance As Double) As TVECTOR
            '#1the vector to project
            '#2the plane to project to
            '#3returns the distance to the plane in the passed direction
            '^projects the passed vector to the passed plane using the normal of the plane as the direction
            Dim rAntiNormal As Boolean = False
            Return ToPlane(aVector, aPlane, aPlane.ZDirection, rDistance:=rDistance, rAntiNormal:=rAntiNormal)
        End Function

        Friend Shared Function ToPlane(aVector As TVECTOR, aPlane As TPLANE, aDirection As TVECTOR, ByRef rDistance As Double) As TVECTOR
            '#1the vector to project
            '#2the plane to project to
            '#3 the project direction
            '#4returns the distance to the plane in the passed direction
            '^projects the passed vector to the passed plane
            '~if the passed direction is a null vector then the direction is assumed as the normal of the plane

            Dim rAntiNormal As Boolean = False
            Dim rChanged As Boolean = False
            Return ToPlane(aVector, aPlane, aDirection, rDistance:=rDistance, rAntiNormal:=rAntiNormal)
        End Function

        Friend Shared Function ToPlane(aVector As TVECTOR, aPlane As TPLANE, aDirection As TVECTOR, ByRef rDistance As Double, ByRef rAntiNormal As Boolean) As TVECTOR

            rDistance = 0
            rAntiNormal = False
            Dim _rVal As New TVECTOR(aVector)
            If TPLANE.IsNull(aPlane) Then Return _rVal
            '#1the vector to project
            '#2the plane to project to
            '#3 the project direction
            '#4returns the distance to the plane in the passed direction
            '#5returns true if the direction from the vector to the intercept is the opposite of the projection direction

            '^projects the passed vector to the passed plane
            '~if the passed direction is a null vector then the direction is assumed as the normal of the plane


            Dim aDir As TVECTOR
            Dim aFlag As Boolean
            If TPLANE.IsNull(aPlane) Then
                Return _rVal
            Else
                If TVECTOR.IsNull(aDirection) Then
                    aDir = aPlane.ZDirection
                Else
                    aDir = aDirection.Normalized(aFlag)
                    If aFlag Then aDir = aPlane.ZDirection
                End If
            End If
            Dim bFlag As Boolean
            Dim ip As TVECTOR = dxfIntersections.LinePlane(New TLINE(aVector, aVector + (aDir * 100)), aPlane, aFlag, bFlag)
            If Not bFlag Or (bFlag And aFlag) Then
                Dim pDir As TVECTOR = aVector.DirectionTo(ip, False, rDistance)
                If rDistance <> 0 Then
                    _rVal.X = ip.X
                    _rVal.Y = ip.Y
                    _rVal.Z = ip.Z
                    rAntiNormal = Not pDir.Equals(aDir, 3)
                End If

            End If

            Return _rVal
        End Function

        Friend Shared Function ToPlane(aVector As TVECTOR, aPlane As dxfPlane) As TVECTOR

            Dim _rVal As New TVECTOR(aVector)
            If dxfPlane.IsNull(aPlane) Then Return _rVal

            Dim ioDirection As dxfDirection = Nothing
            Dim rDistance As Double = 0.0
            Dim rAntiNormal As Boolean = False
            Return ToPlane(aVector, New TPLANE(aPlane), ioDirection, rDistance, rAntiNormal)
        End Function

        Friend Shared Function ToPlane(aVector As TVECTOR, aPlane As TPLANE) As TVECTOR
            Dim ioDirection As dxfDirection = Nothing
            Dim rDistance As Double = 0.0
            Dim rAntiNormal As Boolean = False
            Return ToPlane(aVector, aPlane, ioDirection, rDistance, rAntiNormal)
        End Function
        Friend Shared Function ToPlane(aVector As TVECTOR, aPlane As TPLANE, ByRef ioDirection As dxfDirection) As TVECTOR
            Dim rDistance As Double = 0.0
            Dim rAntiNormal As Boolean = False
            Return ToPlane(aVector, aPlane, ioDirection, rDistance, rAntiNormal)
        End Function

        Friend Shared Function ToPlane(aVector As TVECTOR, aPlane As TPLANE, ByRef ioDirection As dxfDirection, ByRef rDistance As Double, ByRef rAntiNormal As Boolean) As TVECTOR
            rDistance = 0


            '#1the vector to project
            '#2the structure of the plane to project to
            '#3 the project direction
            '#4returns the distance to the plane in the passed direction
            '#5returns true if the direction from the vector to the intercept is the opposite of the projection direction
            '#6flag to convert the returned vector to coordinates with repect to the planes origin and direction
            '^projects the passed vector to the passed plane
            '~if the passed direction is a null vector then the direction is assumed as the normal of the plane
            If ioDirection Is Nothing Then ioDirection = CType(aPlane.ZDirection, dxfDirection)
            Dim aDir As TVECTOR = ioDirection.Strukture
            Return dxfProjections.ToPlane(aVector, aPlane, aDir, rDistance, rAntiNormal)
            ioDirection.SetStructure(aDir)
        End Function

        Friend Shared Function ToPlane(aVector As TVECTOR, aPlane As dxfPlane, ByRef ioDirection As dxfDirection) As TVECTOR
            Dim rDistance As Double = 0.0
            Dim rAntiNormal As Boolean = False
            If dxfPlane.IsNull(aPlane) Then Return New TVECTOR(aVector)
            Return ToPlane(aVector, aPlane, ioDirection, rDistance, rAntiNormal)
        End Function

        Friend Shared Function ToPlane(aVector As TVECTOR, aPlane As dxfPlane, ByRef ioDirection As dxfDirection, ByRef rDistance As Double, ByRef rAntiNormal As Boolean) As TVECTOR
            rDistance = 0

            If dxfPlane.IsNull(aPlane) Then Return New TVECTOR(aVector)
            '#1the vector to project
            '#2the structure of the plane to project to
            '#3 the project direction
            '#4returns the distance to the plane in the passed direction
            '#5returns true if the direction from the vector to the intercept is the opposite of the projection direction
            '#6flag to convert the returned vector to coordinates with repect to the planes origin and direction
            '^projects the passed vector to the passed plane
            '~if the passed direction is a null vector then the direction is assumed as the normal of the plane
            If ioDirection Is Nothing Then ioDirection = CType(aPlane.ZDirection, dxfDirection)
            Dim aDir As TVECTOR = ioDirection.Strukture
            Dim _rVal As TVECTOR = dxfProjections.ToPlane(aVector, aPlane.Strukture, aDir, rDistance, rAntiNormal)
            ioDirection.SetStructure(aDir)
            Return _rVal
        End Function
        Friend Shared Function ToPlane(aVectors As TVECTORS, aPlane As TPLANE) As TVECTORS
            Dim ioDirection As dxfDirection = Nothing
            Return ToPlane(aVectors, aPlane, ioDirection)
        End Function
        Friend Shared Function ToPlane(aVectors As TVECTORS, aPlane As TPLANE, ByRef ioDirection As dxfDirection) As TVECTORS
            Dim _rVal As New TVECTORS(aVectors)
            If aVectors.Count <= 0 Then Return _rVal
            '#1the vector to project
            '#2the structure of the plane to project to
            '#3 the project direction
            '^projects the passeds vector to the passed plane
            '~if the passed direction is a null vector then the direction is assumed as the normal of the plane

            Dim aDir As TVECTOR
            Dim bDir As Boolean = ioDirection IsNot Nothing
            If bDir Then
                aDir = ioDirection.Strukture
            Else
                aDir = aPlane.ZDirection
                ioDirection = CType(aDir, dxfDirection)
            End If
            For i As Integer = 0 To aVectors.Count - 1

                aVectors.SetItem(i + 1, dxfProjections.ToPlane(_rVal.Item(i + 1), aPlane, aDirection:=aDir))

            Next i
            Return _rVal
        End Function
        Friend Shared Function ToPlane(aLine As TLINE, aPlane As TPLANE) As TLINE
            Dim ioDirection As dxfDirection = Nothing
            Return ToPlane(aLine, aPlane, ioDirection)
        End Function
        Friend Shared Function ToPlane(aLine As TLINE, aPlane As TPLANE, ByRef ioDirection As dxfDirection) As TLINE

            Dim _rVal As New TLINE(aLine)
            If TPLANE.IsNull(aPlane) Then Return _rVal
            '#1the vector to project
            '#2the structure of the plane to project to
            '#3 the project direction
            '#4returns the distance to the plane in the passed direction
            '#5returns true if the direction from the vector to the intercept is the opposite of the projection direction
            '#6flag to convert the returned vector to coordinates with repect to the planes origin and direction
            '^projects the passed vector to the passed plane
            '~if the passed direction is a null vector then the direction is assumed as the normal of the plane
            If ioDirection Is Nothing Then
                _rVal.SPT = aLine.SPT.ProjectedTo(aPlane)
                _rVal.EPT = aLine.EPT.ProjectedTo(aPlane)
            Else
                _rVal.SPT = dxfProjections.ToPlane(aLine.SPT, aPlane, ioDirection.Strukture)
                _rVal.EPT = dxfProjections.ToPlane(aLine.EPT, aPlane, ioDirection.Strukture)
            End If
            Return _rVal
        End Function
        Friend Shared Function ToArc(aVector As TVECTOR, aArc As dxeArc) As TVECTOR
            Dim ioDirection As dxfDirection = Nothing
            Dim rDistance As Double = 0.0
            Dim rPointIsOnArc As Boolean = False
            Return ToArc(aVector, aArc, ioDirection, rDistance, rPointIsOnArc)
        End Function
        Friend Shared Function ToArc(aVector As TVECTOR, aArc As dxeArc, ByRef ioDirection As dxfDirection) As TVECTOR
            Dim rDistance As Double = 0.0
            Dim rPointIsOnArc As Boolean = False
            Return ToArc(aVector, aArc, ioDirection, rDistance, rPointIsOnArc)
        End Function
        Friend Shared Function ToArc(aVector As TVECTOR, aArc As dxeArc, ByRef ioDirection As dxfDirection, ByRef rDistance As Double, ByRef rPointIsOnArc As Boolean) As TVECTOR
            rDistance = 0
            rPointIsOnArc = False

            Dim _rVal As New TVECTOR(aVector)
            If aArc Is Nothing Then Return _rVal
            '#1the vector to project
            '#2the arc to project to
            '#3if passed this is used as the projection direction otherwise the x direction of the arcs plane is used
            '#4returns the distance from the original vector to the vector on the arc
            '#5returns True if the projected vector actual lies on the path of the arc
            '^returns the point on the projected to arc
            Dim aDir As TVECTOR
            Dim aL As TLINE
            Dim iPts As TVECTORS
            Dim aPlane As TPLANE = aArc.PlaneV
            Dim bDir As Boolean = ioDirection IsNot Nothing
            If bDir Then
                aDir = ioDirection.Strukture
            Else
                aDir = aPlane.XDirection
            End If
            'make sure the point is on the arcs plane
            If Not _rVal.LiesOn(aPlane) Then
                _rVal = _rVal.ProjectedTo(aPlane)
            End If
            'see if the vector is already on the arc
            rPointIsOnArc = aArc.ContainsVector(_rVal, 0.001)
            If Not rPointIsOnArc Then
                'make sure the passed direction lies on the plane
                If bDir Then
                    If Not aDir.LiesOn(aPlane, bUseWorldOrigin:=True) Then
                        If aDir.Equals(aPlane.ZDirection, True, 3) Then
                            aDir = aPlane.XDirection
                        Else
                            aDir = aDir.ProjectedTo(aPlane).Normalized
                        End If
                    End If
                Else
                    ioDirection = New dxfDirection With {.Strukture = aDir}
                End If
                'create a line to intersect the arc using our direction
                aL = New TLINE(aVector, aVector + (aDir * (3 * aArc.Radius)))
                'get the intersection points
                iPts = aL.IntersectionPts(aArc.ArcStructure, True, False, True)
                If iPts.Count > 0 Then _rVal = iPts.Item(1)
            End If
            rDistance = aVector.DistanceTo(_rVal)
            Return _rVal
        End Function


        Friend Shared Function ToLine(aVector As dxfVector, aLineXY As iLine) As TVECTOR
            Dim rDistance As Double = 0.0
            Dim rOrthoDirection As dxfDirection = Nothing
            Dim rPointIsOnSegment As Boolean = False
            Dim rDirectionPositive As Boolean = False
            Return ToLine(aVector, aLineXY, rDistance, rOrthoDirection, rPointIsOnSegment, rDirectionPositive)
        End Function
        Friend Shared Function ToLine(aVector As iVector, aLineXY As iLine, ByRef rDistance As Double, ByRef rOrthoDirection As dxfDirection, ByRef rPointIsOnSegment As Boolean, ByRef rDirectionPositive As Boolean) As TVECTOR
            '#1the vector to project
            '#2the segment to project to (segments have start and end vectors)
            '#3returns then orthogal distance to the segment from the vector
            '#4returns the orthogonal direction to the segment from the vector
            '#5returns true if then returned vector lines on the passed segment
            '#6returns true if the returned vector lies in the same direction from the start point as the end point (positive direction)
            '^returns the projection of the passed vector projected orthogonally to the passed line

            rDistance = 0
            rOrthoDirection = New dxfDirection
            rPointIsOnSegment = False
            rDirectionPositive = False
            If aLineXY Is Nothing Or aVector Is Nothing Then Return Nothing
            Dim aLine As New TLINE(aLineXY)
            Dim ivec As New TVECTOR(aVector)

            If Math.Round(aLine.Length, 6) <= 0 Then Return ivec


            Dim _rVal As TVECTOR = dxfProjections.ToLine(ivec, aLine, rDistance, rPointIsOnSegment, rDirectionPositive)
            rOrthoDirection.Strukture = ivec.DirectionTo(_rVal)
            Return _rVal
        End Function
        Friend Shared Function ToLine(aVectors As TVECTORS, aLine As TLINE) As TVECTORS
            Dim _rVal As TVECTORS = New TVECTORS(aVectors)

            Dim i As Integer
            Dim v1 As TVECTOR
            For i = 1 To aVectors.Count
                v1 = aVectors.Item(i)
                _rVal.SetItem(i, ToLine(v1, aLine))
            Next i
            Return _rVal
        End Function
        Friend Shared Function ToLine(aVertex As TVERTEX, aLine As TLINE) As TVERTEX
            Dim rDistance As Double = 0.0
            Dim rPointIsOnSegment As Boolean = False
            Dim rDirectionPositive As Boolean = False
            Return ToLine(aVertex, aLine, rDistance, rPointIsOnSegment, rDirectionPositive)
        End Function
        Friend Shared Function ToLine(aVertex As TVERTEX, aLine As TLINE, ByRef rDistance As Double) As TVERTEX
            Dim rPointIsOnSegment As Boolean = False
            Dim rDirectionPositive As Boolean = False
            Return ToLine(aVertex, aLine, rDistance, rPointIsOnSegment, rDirectionPositive)
        End Function
        Friend Shared Function ToLine(aVertex As TVERTEX, aLine As TLINE, ByRef rDistance As Double, ByRef rPointIsOnSegment As Boolean, ByRef rDirectionPositive As Boolean) As TVERTEX

            '#1the vector to project
            '#2the line
            '#3returns then orthogal distance to the segment from the vector
            '#4returns true if then returned vector lines on the passed segment
            '#5returns true if the returned vector lies in the same direction from the start point as the end point (positive direction)
            '^returns the projection of the passed vector projected orthogonally to the passed line

            rDistance = 0
            rPointIsOnSegment = False
            Dim rOrthoDirection As TVECTOR = TVECTOR.Zero
            rDirectionPositive = True
            Dim v1 As New TVECTOR(aVertex.Vector)
            Dim aDir As TVECTOR
            Dim vComp As TVECTOR
            Dim aFlag As Boolean
            Dim lineLen As Double

            Dim lineDir As TVECTOR = aLine.SPT.DirectionTo(aLine.EPT, False, rDirectionIsNull:=aFlag, rDistance:=lineLen)
            'make sure the line has length
            If Not aFlag Then
                'see if the passed vector is the start pt
                If aLine.SPT.DistanceTo(v1, 4) = 0 Then
                    rPointIsOnSegment = True
                    rDirectionPositive = False
                    rOrthoDirection = TVECTOR.WorldX

                End If
                'see if the passed vector is the end pt
                If aLine.EPT.DistanceTo(v1, 4) = 0 Then
                    rPointIsOnSegment = True
                    rOrthoDirection = TVECTOR.WorldX

                End If
            End If
            If Not aFlag Then
                Dim a As TVECTOR = v1 - aLine.SPT
                Dim b As TVECTOR = aLine.Direction
                vComp = a.ComponentAlong(b)
                v1 = aLine.SPT + vComp
            Else
                rPointIsOnSegment = True
            End If
            If Not aFlag Then
                rDistance = aVertex.Vector.DistanceTo(v1)
                aDir = aLine.SPT.DirectionTo(v1)
                rDirectionPositive = aDir.Equals(lineDir, 3)
                If Not rDirectionPositive Then
                    rPointIsOnSegment = False
                Else
                    rPointIsOnSegment = aLine.SPT.DistanceTo(v1) < lineLen
                End If
            End If
            Dim _rVal As New TVERTEX(aVertex)

            _rVal.Vector = v1
            Return _rVal

        End Function

        Friend Shared Function ToLine(aVector As TVECTOR, aLine As TLINE) As TVECTOR
            '#1the subject vector
            '#2the subject line
            '^returns the projection of the passed vector projected orthogonally to the passed line

            Dim rPointIsOnSegment As Boolean = False
            Dim rDirectionPositive As Boolean = False
            Dim rDistance As Double = 0.0
            Return ToLine(aVector, aLine, rDistance, rPointIsOnSegment, rDirectionPositive)
        End Function
        Friend Shared Function ToLine(aVector As TVECTOR, aLine As TLINE, ByRef rOrthoDirection As TVECTOR) As TVECTOR
            '#1the subject vector
            '#2the subject line
            '#3returns the orthogonal direction to the segment from the vector
            '^returns the projection of the passed vector projected orthogonally to the passed line

            Dim rPointIsOnSegment As Boolean = False
            Dim rDirectionPositive As Boolean = False
            Dim rDistance As Double = 0.0
            Dim _rVal As TVECTOR = ToLine(aVector, aLine, rDistance, rPointIsOnSegment, rDirectionPositive)
            rOrthoDirection = aVector.DirectionTo(_rVal)
            Return _rVal
        End Function

        Friend Shared Function ToLine(aVector As TVECTOR, aLine As TLINE, ByRef rOrthoDirection As TVECTOR, ByRef rDistance As Double) As TVECTOR
            '#1the subject vector
            '#2the subject line
            '#3returns the orthogonal direction to the segment from the vector
            '^returns the projection of the passed vector projected orthogonally to the passed line

            Dim rPointIsOnSegment As Boolean = False
            Dim rDirectionPositive As Boolean = False
            rDistance = 0.0
            Dim _rVal As TVECTOR = ToLine(aVector, aLine, rDistance, rPointIsOnSegment, rDirectionPositive)
            rOrthoDirection = aVector.DirectionTo(_rVal)
            Return _rVal
        End Function

        Friend Shared Function ToLine(aVector As TVECTOR, aLine As TLINE, ByRef rDistance As Double) As TVECTOR
            '#1the subject vector
            '#2the subject line
            '#3returns the orthogal distance to the segment from the vector
            '^returns the projection of the passed vector projected orthogonally to the passed line
            Dim rPointIsOnSegment As Boolean = False
            Dim rDirectionPositive As Boolean = False
            Return ToLine(aVector, aLine, rDistance, rPointIsOnSegment, rDirectionPositive)
        End Function
        Friend Shared Function ToLine(aVector As TVECTOR, aLine As TLINE, ByRef rDistance As Double, ByRef rPointIsOnSegment As Boolean) As TVECTOR
            '#1the subject vector
            '#2the subject line
            '#3returns the orthogal distance to the segment from the vector
            '#4returns true if the returned vector lines on the passed segment
            '^returns the projection of the passed vector projected orthogonally to the passed line
            Dim rDirectionPositive As Boolean = False

            Return ToLine(aVector, aLine, rDistance, rPointIsOnSegment, rDirectionPositive)
        End Function

        Friend Shared Function ToLine(aVector As TVECTOR, aLine As TLINE, ByRef rDistance As Double, ByRef rPointIsOnSegment As Boolean, ByRef rDirectionPositive As Boolean) As TVECTOR

            '#1the subject vector
            '#2the subject line
            '#3returns the orthogal distance to the segment from the vector
            '#4returns true if the returned vector lines on the passed segment
            '#5returns true if the returned vector lies in the same direction from the start point as the end point (positive direction)
            '^returns the projection of the passed vector projected orthogonally to the passed line

            rDistance = 0
            rPointIsOnSegment = False
            Dim rOrthoDirection As TVECTOR = TVECTOR.Zero
            rDirectionPositive = True
            Dim _rVal As New TVECTOR(aVector)
            Dim aDir As TVECTOR
            Dim vComp As TVECTOR
            Dim aFlag As Boolean
            Dim lineLen As Double

            Dim lineDir As TVECTOR = aLine.SPT.DirectionTo(aLine.EPT, False, rDirectionIsNull:=aFlag, rDistance:=lineLen)
            'make sure the line has length
            If Not aFlag Then
                'see if the passed vector is the start pt
                If aLine.SPT.DistanceTo(_rVal, 4) = 0 Then
                    rPointIsOnSegment = True
                    rDirectionPositive = False
                    rOrthoDirection = TVECTOR.WorldX

                End If
                'see if the passed vector is the end pt
                If aLine.EPT.DistanceTo(_rVal, 4) = 0 Then
                    rPointIsOnSegment = True
                    rOrthoDirection = TVECTOR.WorldX

                End If
            End If
            If Not aFlag Then
                Dim a As TVECTOR = _rVal - aLine.SPT
                Dim b As TVECTOR = aLine.Direction
                vComp = a.ComponentAlong(b)
                _rVal = aLine.SPT + vComp
            Else
                rPointIsOnSegment = True
            End If
            If Not aFlag Then
                rDistance = aVector.DistanceTo(_rVal, 15)
                aDir = aLine.SPT.DirectionTo(_rVal)
                rDirectionPositive = aDir.Equals(lineDir, 3)
                If Not rDirectionPositive Then
                    rPointIsOnSegment = False
                Else
                    rPointIsOnSegment = aLine.SPT.DistanceTo(_rVal) < lineLen
                End If
            End If


            Return _rVal

        End Function

        Friend Shared Function DistanceTo(aVector As TVECTOR, aLine As TLINE, Optional aPrecis As Integer = -1) As Double
            Dim _rVal As Double
            '#1the point to find a distance from
            '#2the line to find the distance to
            '^returns the orthogonal (shortest) distance from the vector to the line
            dxfProjections.ToLine(aVector, aLine, _rVal)
            If aPrecis >= 0 Then
                _rVal = Math.Round(_rVal, TVALUES.LimitedValue(aPrecis, 0, 15))
            End If
            Return _rVal
        End Function

        Friend Shared Function DistanceTo(aVector As TVECTOR, aLine As TLINE, ByRef rInterceptPoint As TVECTOR, ByRef rOrthogLine As TLINE, ByRef rInterceptIsOnLine As Boolean) As Double
            Dim _rVal As Double
            '#1the point to find a distance from
            '#2the line to find the distance to
            '#3returns the point on the line where an orthoganal line through the passed point intercepts the passed line
            '#4returns the orthoganal vector from the passed point to the passed line
            '#5returns true if the intercept point of the orthoganol vector lies on the passed line

            '^returns the orthogonal (shortest) distance from the vector to the line
            rInterceptPoint = dxfProjections.ToLine(aVector, aLine, _rVal, rInterceptIsOnLine)
            rOrthogLine = New TLINE(aVector, rInterceptPoint)

            Return _rVal
        End Function
        Friend Shared Function DistanceTo(aVector As TVECTOR, aLine As iLine, ByRef rInterceptPoint As dxfVector, ByRef rOrthogLine As dxeLine, ByRef rInterceptIsOnLine As Boolean, ByRef rError As String) As Double
            Dim _rVal As Double
            '#1the point to find a distance from
            '#2the line to find the distance to
            '#3returns the point on the line where an orthoganal line through the passed point intercepts the passed line
            '#4returns the orthoganal vector from the passed point to the passed line
            '#5returns true if the intercept point of the orthoganol vector lies on the passed line

            '^returns the orthogonal (shortest) distance from the vector to the line

            rError = string.Empty
            rInterceptPoint = Nothing
            rOrthogLine = Nothing
            If aLine Is Nothing Then
                rError = "The Passed Line Is Undefined"
                Return 0
            End If
            Dim iline As New TLINE(aLine)
            If iline.Length <= 0 Then
                rError = "The Passed Line Is Undefined"
                Return 0
            End If

            rInterceptPoint = New dxfVector(dxfProjections.ToLine(aVector, iline, _rVal, rInterceptIsOnLine))
            rOrthogLine = New dxeLine(New TLINE(aVector, rInterceptPoint))

            Return _rVal
        End Function
        Friend Shared Function DistanceTo(aVector As TVECTOR, bVector As TVECTOR, Optional aPrecis As Integer = -1) As Double

            '#1the first point to find a distance from
            '#1the second point to find a distance to
            '#2a rounding precision to apply(0-15)
            '^returns the  distance  between the passed vector
            Dim _rVal As Double
            _rVal = (aVector - bVector).Magnitude
            If aPrecis >= 0 Then _rVal = Math.Round(_rVal, TVALUES.LimitedValue(aPrecis, 0, 15))
            Return _rVal
        End Function
        Friend Shared Function DistanceTo(aVector As TVERTEX, bVector As TVERTEX, Optional aPrecis As Integer = -1) As Double

            '#1the first point to find a distance from
            '#1the second point to find a distance to
            '#2a rounding precision to apply(0-15)
            '^returns the  distance  between the passed vector
            Dim _rVal As Double
            _rVal = (aVector.Vector - bVector.Vector).Magnitude
            If aPrecis >= 0 Then _rVal = Math.Round(_rVal, TVALUES.LimitedValue(aPrecis, 0, 15))
            Return _rVal
        End Function

        Public Shared Function DistanceTo(aVector As iVector, bVector As iVector, Optional aPrecis As Integer = -1) As Double

            '#1the first point to find a distance from
            '#1the second point to find a distance to
            '#2a rounding precision to apply(0-15)
            '^returns the  distance  between the passed vector
            Return dxfProjections.DistanceTo(New TVECTOR(aVector), New TVECTOR(bVector), aPrecis)

        End Function

        Public Shared Function DistanceToLine(aPoint1 As iVector, aLine As iLine, Optional aPrecis As Integer = -1) As Double
            Dim rInterceptPoint As dxfVector = Nothing
            Dim rOrthogLine As dxeLine = Nothing
            Dim rInterceptIsOnLine As Boolean = False
            Dim serr As String = String.Empty
            Dim _rVal As Double = DistanceToLine(aPoint1, aLine, rInterceptPoint, rOrthogLine, rInterceptIsOnLine, serr)
            If aPrecis >= 0 Then _rVal = Math.Round(_rVal, TVALUES.LimitedValue(aPrecis, 0, 15))
            Return _rVal

        End Function
        Public Shared Function DistanceToLine(aPoint1 As iVector, aLine As iLine, ByRef rInterceptPoint As dxfVector, ByRef rOrthogLine As dxeLine, ByRef rInterceptIsOnLine As Boolean, ByRef rError As String) As Double
            '#1the point to find a distance from
            '#2the line to find the distance to
            '#3returns the project of the point to the line
            '^returns the orthogonal distance from the passed point to the passed line
            Dim _rVal As Double
            Try

                _rVal = dxfProjections.DistanceTo(New TVECTOR(aPoint1), aLine, rInterceptPoint, rOrthogLine, rInterceptIsOnLine, rError)
            Catch ex As Exception
                Throw ex
            End Try
            Return _rVal
        End Function

        Friend Shared Function DistanceTo(aVector As TVECTOR, aPlane As TPLANE, aDirection As iVector, ByRef rIntersect As dxfVector, ByRef rAntiNormal As Boolean) As Double
            Dim _rVal As Double
            '#1the vector to find the distance for
            '#2the plane to find the distance to
            '#3the projection direction
            '#4returns the intesect point of the a line thru the vector aligned with the projection direction
            '#5returns true if the direction from the vector to the intercept is the opposite of the projection direction
            '^calculates and returns the orthogonal distance from the given vector to the given plane
            rAntiNormal = False
            rIntersect = Nothing
            If aPlane.ZDirection.Magnitude = 0 Then Return _rVal
            Dim sp As New TVECTOR(aVector)
            Dim pDir As TVECTOR = New TVECTOR(aDirection)
            Dim pN As TVECTOR = aPlane.ZDirection.Normalized() * -1
            'set the projection direction
            If TVECTOR.IsNull(pDir) Then pDir = pN

            Dim coPL As Boolean
            Dim bOnLn As Boolean
            Dim ep As TVECTOR = sp + (pDir * 100)
            Dim ip As TVECTOR = dxfIntersections.LinePlane(New TLINE(sp, ep), aPlane, bOnLn, coPL)
            If Not coPL Or (coPL And bOnLn) Then
                rIntersect = New dxfVector(ip)
                rAntiNormal = Not sp.DirectionTo(ip).Equals(pDir, 3)
                _rVal = sp.DistanceTo(ip)
            End If
            Return _rVal
        End Function

#End Region 'Shared Methods
    End Class
#End Region 'Class
#Region "Methods"
#End Region 'Methods

End Namespace
