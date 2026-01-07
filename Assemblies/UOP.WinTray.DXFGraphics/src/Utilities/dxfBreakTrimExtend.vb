
Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics.Utilities
    Public Class dxfBreakTrimExtend
#Region "Methods"

        Public Shared Function break_Arc(aArc As dxeArc, aBreaker As dxfEntity, bBreakersAreInfinite As Boolean, Optional aIntersects As colDXFVectors = Nothing) As colDXFEntities
            Dim rWasBroken As Boolean
            Return dxfBreakTrimExtend.break_Arc(aArc, aBreaker, bBreakersAreInfinite, rWasBroken, aIntersects)
        End Function

        Public Shared Function break_Arc(aArc As dxeArc, aBreaker As dxfEntity, bBreakersAreInfinite As Boolean, ByRef rWasBroken As Boolean, Optional aIntersects As colDXFVectors = Nothing) As colDXFEntities
            Dim _rVal As New colDXFEntities
            '#1the arc to break
            '#2the entity  use to break the arc
            '#3flag indicating that the breaker(s) are of infinite length
            '#4returns true if the arc was broken
            '#5returns the points of intersection that were used for the break points
            '^returns the arc broken into parts at the intersection of its self and the passed segment or segments
            'On Error Resume Next
            rWasBroken = False
            If aArc Is Nothing Then Return _rVal
            aArc.Value = False
            If aBreaker Is Nothing Then
                _rVal.Add(aArc, bAddClone:=True)
                Return _rVal
            End If
            Dim iPts As colDXFVectors = aArc.Intersections(aBreaker, False, bBreakersAreInfinite)
            If aIntersects IsNot Nothing Then aIntersects.Append(iPts)

            If iPts.Count <= 0 Then
                _rVal.Add(aArc, bAddClone:=True)
                Return _rVal
            End If
            iPts.Clockwise(aArc.Center, aArc.StartAngle, Not aArc.ClockWise)

            rWasBroken = True
            Dim sp As dxfVector = aArc.StartPt
            Dim ep As dxfVector = iPts.Item(1)

            Dim aA As dxeArc = aArc.Clone
            Dim aCS As dxfPlane = Nothing
            Dim xDir As dxfDirection
            Dim zDir As dxfDirection
            aCS = aArc.Plane
            xDir = aCS.XDirection
            zDir = aCS.ZDirection

            aA.Value = True
            aA.EndAngle = xDir.AngleTo(aArc.Center.DirectionTo(ep), zDir)
            If aA.SpannedAngle <> 0 Then _rVal.Add(aA)
            iPts.Remove(1)
            Do Until iPts.Count = 0
                sp = ep
                ep = iPts.Item(1)
                aA = aArc.Clone
                aA.Value = True
                aA.StartAngle = xDir.AngleTo(aArc.Center.DirectionTo(sp), zDir)
                aA.EndAngle = xDir.AngleTo(aArc.Center.DirectionTo(ep), zDir)
                If aA.SpannedAngle <> 0 Then _rVal.Add(aA)
                iPts.Remove(1)
            Loop
            If _rVal.Count > 0 Then
                sp = _rVal.LastMember.DefinitionPoint(dxxEntDefPointTypes.EndPt)
                ep = aArc.EndPt
                If Not sp.IsEqual(ep) Then
                    aA = aArc.Clone
                    aA.Value = True
                    aA.StartAngle = xDir.AngleTo(aArc.Center.DirectionTo(sp), zDir)
                    aA.EndAngle = xDir.AngleTo(aArc.Center.DirectionTo(ep), zDir)
                    If aA.SpannedAngle <> 0 Then _rVal.Add(aA)
                End If
            Else
                _rVal.Add(aArc, bAddClone:=True)
            End If
            Return _rVal
        End Function

        Public Shared Function break_Arc(aArc As dxeArc, aBreakers As IEnumerable(Of dxfEntity), bBreakersAreInfinite As Boolean, Optional aIntersects As colDXFVectors = Nothing) As colDXFEntities
            Dim rWasBroken As Boolean
            Return dxfBreakTrimExtend.break_Arc(aArc, aBreakers, bBreakersAreInfinite, rWasBroken, aIntersects)
        End Function

        Public Shared Function break_Arc(aArc As dxeArc, aBreakers As IEnumerable(Of dxfEntity), bBreakersAreInfinite As Boolean, ByRef rWasBroken As Boolean, Optional aIntersects As colDXFVectors = Nothing) As colDXFEntities
            Dim _rVal As New colDXFEntities
            '#1the arc to break
            '#2the  entities to use to break the arc
            '#3flag indicating that the breaker(s) are of infinite length
            '#4returns true if the arc was broken
            '#5returns the points of intersection that were used for the break points
            '^returns the arc broken into parts at the intersection of its self and the passed segment or segments
            'On Error Resume Next
            rWasBroken = False
            If aArc Is Nothing Then Return _rVal
            aArc.Value = False
            If aBreakers Is Nothing Then
                _rVal.Add(aArc, bAddClone:=True)
                Return _rVal
            End If
            Dim iPts As colDXFVectors = aArc.Intersections(aBreakers, False, bBreakersAreInfinite)
            If aIntersects IsNot Nothing Then aIntersects.Append(iPts)

            If iPts.Count <= 0 Then
                _rVal.Add(aArc, bAddClone:=True)
                Return _rVal
            End If
            iPts.Clockwise(aArc.Center, aArc.StartAngle, Not aArc.ClockWise)

            rWasBroken = True
            Dim sp As dxfVector = aArc.StartPt
            Dim ep As dxfVector = iPts.Item(1)

            Dim aA As dxeArc = aArc.Clone
            Dim aCS As dxfPlane = Nothing
            Dim xDir As dxfDirection
            Dim zDir As dxfDirection
            aCS = aArc.Plane
            xDir = aCS.XDirection
            zDir = aCS.ZDirection

            aA.Value = True
            aA.EndAngle = xDir.AngleTo(aArc.Center.DirectionTo(ep), zDir)
            If aA.SpannedAngle <> 0 Then _rVal.Add(aA)
            iPts.Remove(1)
            Do Until iPts.Count = 0
                sp = ep
                ep = iPts.Item(1)
                aA = aArc.Clone
                aA.Value = True
                aA.StartAngle = xDir.AngleTo(aArc.Center.DirectionTo(sp), zDir)
                aA.EndAngle = xDir.AngleTo(aArc.Center.DirectionTo(ep), zDir)
                If aA.SpannedAngle <> 0 Then _rVal.Add(aA)
                iPts.Remove(1)
            Loop
            If _rVal.Count > 0 Then
                sp = _rVal.LastMember.DefinitionPoint(dxxEntDefPointTypes.EndPt)
                ep = aArc.EndPt
                If Not sp.IsEqual(ep) Then
                    aA = aArc.Clone
                    aA.Value = True
                    aA.StartAngle = xDir.AngleTo(aArc.Center.DirectionTo(sp), zDir)
                    aA.EndAngle = xDir.AngleTo(aArc.Center.DirectionTo(ep), zDir)
                    If aA.SpannedAngle <> 0 Then _rVal.Add(aA)
                End If
            Else
                _rVal.Add(aArc, bAddClone:=True)
            End If
            Return _rVal
        End Function

        Friend Shared Function break_ArcLineV(aSegment As TSEGMENT, aBreakers As TSEGMENTS, Optional bBreakersAreInfinite As Boolean = False, Optional bWasBroken As Boolean = False, Optional aIntersects As colDXFVectors = Nothing, Optional aDisplaySettings As dxfDisplaySettings = Nothing) As colDXFEntities
            Dim _rVal As colDXFEntities = Nothing
            If aSegment.IsArc Then
                If aSegment.ArcStructure.Elliptical Then
                    _rVal = break_EllipseV(aSegment, aBreakers, bBreakersAreInfinite, bWasBroken, aIntersects, aDisplaySettings)
                Else
                    _rVal = break_ArcV(aSegment, aBreakers, bBreakersAreInfinite, bWasBroken, aIntersects, aDisplaySettings)
                End If
            Else
                _rVal = break_LineV(aSegment, aBreakers, bBreakersAreInfinite, bWasBroken, aIntersects, aDisplaySettings)
            End If
            Return _rVal
        End Function
        Friend Shared Function break_ArcV(aArc As TSEGMENT, aBreakers As TSEGMENTS, bBreakersAreInfinite As Boolean, ByRef rWasBroken As Boolean, Optional aIntersects As colDXFVectors = Nothing, Optional aDisplaySettings As dxfDisplaySettings = Nothing) As colDXFEntities
            Dim _rVal As colDXFEntities = Nothing
            '#1the arc to break
            '#2the entity or entities to use to break the arc
            '#3flag indicating that the breaker(s) are of infinite length
            '#4returns true if the arc was broken
            '#5returns the points of intersection that were used for the break points
            '^returns the arc broken into parts at the intersection of its self and the passed segment or segments
            rWasBroken = False
            Try
                aIntersects = New colDXFVectors
                _rVal = New colDXFEntities
                aArc.Marker = False
                aArc.ArcStructure.Elliptical = False
                _rVal.AddArcLineV(aArc, aDisplaySettings)
                If aArc.ArcStructure.SpannedAngle <= 0 Then Return _rVal
                dxfIntersections.LAE_LAES(aArc, aBreakers, False, bBreakersAreInfinite, 1000, 1000, aIntersects)
                rWasBroken = aIntersects.Count > 0
                If Not rWasBroken Then Return _rVal
                Dim aRet As colDXFEntities
                Dim sp As TVECTOR
                Dim ep As TVECTOR
                '**UNUSED VAR** Dim idx As Long
                Dim aA As New TARC
                Dim bA As New TSEGMENT
                Dim cA As New TARC
                Dim xDir As TVECTOR
                Dim zDir As TVECTOR
                Dim cp As TVECTOR
                Dim lEP As TVECTOR
                aA = aArc.ArcStructure
                xDir = aA.Plane.XDirection
                zDir = aA.Plane.ZDirection
                cp = aA.Plane.Origin
                aRet = New colDXFEntities
                aIntersects.SortClockWiseV(aA.Plane.Origin, aA.StartAngle, Not aA.ClockWise, aA.Plane, True)
                aA.Plane.Origin = cp
                sp = aA.Plane.AngleVector(aA.StartAngle, aA.Radius, False)
                ep = aIntersects.ItemVector(1)
                bA.IsArc = True
                bA.Marker = True
                cA = aA
                cA.EndAngle = xDir.AngleTo(cp.DirectionTo(ep), zDir)
                'cA.SpannedAngle = dxfMath.SpannedAngle(cA.ClockWise, cA.StartAngle, cA.EndAngle)
                bA.ArcStructure = cA
                If cA.SpannedAngle <> 0 Then
                    aRet.AddArcLineV(bA, aDisplaySettings)
                    lEP = ep
                End If
                aIntersects.Remove(1)
                Do Until aIntersects.Count = 0
                    sp = ep
                    ep = aIntersects.ItemVector(1)
                    cA = aA
                    cA.StartAngle = xDir.AngleTo(cp.DirectionTo(sp), zDir)
                    cA.EndAngle = xDir.AngleTo(cp.DirectionTo(ep), zDir)
                    'cA.SpannedAngle = dxfMath.SpannedAngle(aA.ClockWise, cA.StartAngle, cA.EndAngle)
                    bA.ArcStructure = cA
                    If cA.SpannedAngle <> 0 Then
                        aRet.AddArcLineV(bA, aDisplaySettings)
                        lEP = ep
                    End If
                    aIntersects.Remove(1)
                Loop
                If _rVal.Count > 0 Then
                    sp = lEP
                    ep = aA.Plane.AngleVector(aA.EndAngle, aA.Radius, False)
                    If dxfProjections.DistanceTo(sp, ep) > 0.0001 Then
                        cA = aA
                        cA.StartAngle = xDir.AngleTo(cp.DirectionTo(sp), zDir)
                        cA.EndAngle = xDir.AngleTo(cp.DirectionTo(ep), zDir)
                        'cA.SpannedAngle = dxfMath.SpannedAngle(aA.ClockWise, cA.StartAngle, cA.EndAngle)
                        bA.ArcStructure = cA
                        If cA.SpannedAngle <> 0 Then
                            aRet.AddArcLineV(bA, aDisplaySettings)
                            lEP = ep
                        End If
                    End If
                Else
                    aRet.AddArcLineV(aArc)
                    rWasBroken = False
                End If
                _rVal = aRet
                Return _rVal
            Catch ex As Exception
                rWasBroken = False
            End Try
            Return _rVal
        End Function
        Public Shared Function break_Ellipse(aEllipse As dxeEllipse, aBreaker As Object, Optional bBreakersAreInfinite As Boolean = False, Optional bWasBroken As Boolean = False, Optional aIntersects As colDXFVectors = Nothing) As colDXFEntities
            Dim _rVal As colDXFEntities = Nothing
            '#1the ellipse to break
            '#2the entity or entities to use to break the ellipse
            '#3flag indicating that the breaker(s) are of infinite length
            '#4returns true if the ellipse was broken
            '#5returns the points of intersection that were used for the break points
            '^returns the ellipse broken into parts at the intersection of its self and the passed segment or segments
            'On Error Resume Next
            bWasBroken = False
            _rVal = New colDXFEntities
            If aEllipse Is Nothing Then Return _rVal
            aEllipse.Value = False
            If aBreaker Is Nothing Then
                _rVal.Add(aEllipse, bAddClone:=True)
                Return _rVal
            End If
            Dim iPts As TVECTORS
            iPts = dxfIntersections.Points(aEllipse, aBreaker, False, bBreakersAreInfinite)
            If iPts.Count <= 0 Then
                _rVal.Add(aEllipse, bAddClone:=True)
                Return _rVal
            End If
            bWasBroken = True
            Dim sp As dxfVector
            Dim ep As dxfVector
            '**UNUSED VAR** Dim idx As Long
            Dim aA As dxeEllipse
            Dim aCS As dxfPlane = Nothing
            Dim xDir As dxfDirection
            Dim zDir As dxfDirection
            aCS = aEllipse.Plane
            xDir = aCS.XDirection
            zDir = aCS.ZDirection
            iPts.Clockwise(aEllipse.PlaneV, aEllipse.StartAngle, Not aEllipse.ClockWise)
            sp = aEllipse.StartPt
            ep = New dxfVector With {.Strukture = iPts.Item(1)}
            aA = aEllipse.Clone
            aA.Value = True
            aA.EndAngle = xDir.AngleTo(aEllipse.Center.DirectionTo(ep), zDir)
            If aA.SpannedAngle <> 0 Then _rVal.Add(aA)
            iPts.Remove(1)
            Do Until iPts.Count = 0
                sp = ep
                ep = New dxfVector With {.Strukture = iPts.Item(1)}
                aA = aEllipse.Clone
                aA.Value = True
                aA.StartAngle = xDir.AngleTo(aEllipse.Center.DirectionTo(sp), zDir)
                aA.EndAngle = xDir.AngleTo(aEllipse.Center.DirectionTo(ep), zDir)
                If aA.SpannedAngle <> 0 Then _rVal.Add(aA)
                iPts.Remove(1)
            Loop
            If _rVal.Count > 0 Then
                sp = _rVal.LastMember.DefinitionPoint(dxxEntDefPointTypes.EndPt)
                ep = aEllipse.EndPt.Clone
                If Not sp.IsEqual(ep) Then
                    aA = aEllipse.Clone
                    aA.Value = True
                    aA.StartAngle = xDir.AngleTo(aEllipse.Center.DirectionTo(sp), zDir)
                    aA.EndAngle = xDir.AngleTo(aEllipse.Center.DirectionTo(ep), zDir)
                    If aA.SpannedAngle <> 0 Then _rVal.Add(aA)
                End If
            Else
                _rVal.Add(aEllipse, bAddClone:=True)
            End If
            Return _rVal
        End Function
        Friend Shared Function break_EllipseV(aEllipse As TSEGMENT, aBreakers As TSEGMENTS, bBreakersAreInfinite As Boolean, ByRef rWasBroken As Boolean, Optional rIntersects As colDXFVectors = Nothing, Optional aDisplaySettings As dxfDisplaySettings = Nothing) As colDXFEntities
            Dim _rVal As colDXFEntities = Nothing
            '#1the ellipse to break
            '#2the entity or entities to use to break the ellipse
            '#3flag indicating that the breaker(s) are of infinite length
            '#4returns true if the ellipse was broken
            '#5returns the points of intersection that were used for the break points
            '^returns the ellipse broken into parts at the intersection of its self and the passed segment or segments
            rWasBroken = False
            Try
                _rVal = New colDXFEntities
                aEllipse.Marker = False
                aEllipse.ArcStructure.Elliptical = True
                _rVal.AddArcLineV(aEllipse, aDisplaySettings)
                If aEllipse.ArcStructure.SpannedAngle <= 0 Then Return _rVal
                Dim ipts As New colDXFVectors
                dxfIntersections.LAE_LAES(aEllipse, aBreakers, False, bBreakersAreInfinite, 1000, 1000, ipts)
                rWasBroken = ipts.Count > 0
                If Not rWasBroken Then Return _rVal
                If rIntersects IsNot Nothing Then rIntersects.Append(ipts)
                Dim aRet As colDXFEntities
                Dim sp As TVECTOR
                Dim ep As TVECTOR
                '**UNUSED VAR** Dim idx As Long
                Dim aA As New TARC
                Dim bA As New TSEGMENT
                Dim cA As New TARC
                Dim xDir As TVECTOR
                Dim zDir As TVECTOR
                Dim cp As TVECTOR
                Dim lEP As TVECTOR
                aA = aEllipse.ArcStructure
                xDir = aA.Plane.XDirection
                zDir = aA.Plane.ZDirection
                cp = aA.Plane.Origin
                aRet = New colDXFEntities
                rIntersects.SortClockWiseV(aA.Plane.Origin, aA.StartAngle, Not aA.ClockWise, aA.Plane, True)
                aA.Plane.Origin = cp
                sp = aA.Plane.AngleVector(aA.StartAngle, aA.Radius, False)
                ep = rIntersects.ItemVector(1)
                bA.IsArc = True
                bA.Marker = True
                cA = aA
                cA.EndAngle = xDir.AngleTo(cp.DirectionTo(ep), zDir)
                'cA.SpannedAngle = dxfMath.SpannedAngle(cA.ClockWise, cA.StartAngle, cA.EndAngle)
                bA.ArcStructure = cA
                If cA.SpannedAngle <> 0 Then
                    aRet.AddArcLineV(bA, aDisplaySettings)
                    lEP = ep
                End If
                rIntersects.Remove(1)
                Do Until rIntersects.Count = 0
                    sp = ep
                    ep = rIntersects.ItemVector(1)
                    cA = aA
                    cA.StartAngle = xDir.AngleTo(cp.DirectionTo(sp), zDir)
                    cA.EndAngle = xDir.AngleTo(cp.DirectionTo(ep), zDir)
                    'cA.SpannedAngle = dxfMath.SpannedAngle(aA.ClockWise, cA.StartAngle, cA.EndAngle)
                    bA.ArcStructure = cA
                    If cA.SpannedAngle <> 0 Then
                        aRet.AddArcLineV(bA, aDisplaySettings)
                        lEP = ep
                    End If
                    rIntersects.Remove(1)
                Loop
                If _rVal.Count > 0 Then
                    sp = lEP
                    ep = aA.Plane.AngleVector(aA.EndAngle, aA.Radius, False)
                    If dxfProjections.DistanceTo(sp, ep) > 0.0001 Then
                        cA = aA
                        cA.StartAngle = xDir.AngleTo(cp.DirectionTo(sp), zDir)
                        cA.EndAngle = xDir.AngleTo(cp.DirectionTo(ep), zDir)
                        'cA.SpannedAngle = dxfMath.SpannedAngle(aA.ClockWise, cA.StartAngle, cA.EndAngle)
                        bA.ArcStructure = cA
                        If cA.SpannedAngle <> 0 Then
                            aRet.AddArcLineV(bA, aDisplaySettings)
                            lEP = ep
                        End If
                    End If
                Else
                    aRet.AddArcLineV(aEllipse, aDisplaySettings)
                    rWasBroken = False
                End If
                _rVal = aRet
            Catch ex As Exception
                rWasBroken = False
            End Try
            Return _rVal
        End Function

        Public Shared Function break_Line(aLine As dxeLine, aBreaker As dxfEntity, bBreakersAreInfinite As Boolean, Optional aIntercepts As colDXFVectors = Nothing) As colDXFEntities
            Dim rWasBroken As Boolean
            Return dxfBreakTrimExtend.break_Line(aLine, aBreaker, bBreakersAreInfinite, rWasBroken, aIntercepts)
        End Function

        Public Shared Function break_Line(aLine As dxeLine, aBreaker As dxfEntity, bBreakersAreInfinite As Boolean, ByRef rWasBroken As Boolean, Optional aIntercepts As colDXFVectors = Nothing) As colDXFEntities
            Dim _rVal As New colDXFEntities
            '#1the line to break
            '#2the entity or entities to use to break the line
            '#3flag indicating that the breaker(s) are of infinite length
            '#4returns true if the line was broken
            '#5returns the points of intersection that were used for the break points
            '^returns the line broken into parts at the intersection of its self and the passed segment or segments
            'On Error Resume Next
            rWasBroken = False

            If aLine Is Nothing Then Return _rVal
            If aBreaker Is Nothing Then
                _rVal.Add(aLine, bAddClone:=True)
                Return _rVal
            End If
            aLine.Value = False
            Dim ipts As colDXFVectors = aLine.Intersections(aBreaker, False, bBreakersAreInfinite)
            If ipts.Count <= 0 Then
                _rVal.Add(aLine, bAddClone:=True)
                Return _rVal
            End If
            rWasBroken = True
            Dim sp As TVECTOR
            Dim ep As TVECTOR

            Dim aL As dxeLine
            Dim i As Long
            ipts.Sort(dxxSortOrders.NearestToFarthest, aLine.StartPt)
            sp = aLine.StartPt.Strukture
            ep = ipts.ItemVector(1)
            If dxfProjections.DistanceTo(sp, ep) > 0.001 Then
                aL = aLine.Clone
                aL.Value = True
                aL.SetVectors(sp, ep)
                _rVal.Add(aL)
            End If
            For i = 1 To ipts.Count
                If i + 1 > ipts.Count Then Exit For
                sp = ipts.ItemVector(i)
                ep = ipts.ItemVector(i + 1)
                aL = aLine.Clone
                If dxfProjections.DistanceTo(sp, ep) > 0.001 Then
                    aL.SetVectors(sp, ep)
                    aL.Value = True
                    _rVal.Add(aL)
                End If
            Next i
            sp = ipts.LastVector.Strukture
            ep = aLine.EndPt.Strukture
            If dxfProjections.DistanceTo(sp, ep) > 0.001 Then
                aL = aLine.Clone
                aL.Value = True
                aL.SetVectors(sp, ep)
                _rVal.Add(aL)
            End If
            If aIntercepts IsNot Nothing Then aIntercepts.Append(ipts)
            Return _rVal
        End Function
        Public Shared Function break_Line(aLine As dxeLine, aBreakers As IEnumerable(Of dxfEntity), bBreakersAreInfinite As Boolean, Optional aIntercepts As colDXFVectors = Nothing) As colDXFEntities
            Dim rWasBroken As Boolean
            Return dxfBreakTrimExtend.break_Line(aLine, aBreakers, bBreakersAreInfinite, rWasBroken, aIntercepts)
        End Function
        Public Shared Function break_Line(aLine As dxeLine, aBreakers As IEnumerable(Of dxfEntity), bBreakersAreInfinite As Boolean, ByRef rWasBroken As Boolean, Optional aIntercepts As colDXFVectors = Nothing) As colDXFEntities
            Dim _rVal As New colDXFEntities
            '#1the line to break
            '#2the entity or entities to use to break the line
            '#3flag indicating that the breaker(s) are of infinite length
            '#4returns true if the line was broken
            '#5returns the points of intersection that were used for the break points
            '^returns the line broken into parts at the intersection of its self and the passed segment or segments
            'On Error Resume Next
            rWasBroken = False

            If aLine Is Nothing Then Return _rVal
            If aBreakers Is Nothing Then
                _rVal.Add(aLine, bAddClone:=True)
                Return _rVal
            End If
            aLine.Value = False
            Dim ipts As colDXFVectors = aLine.Intersections(aBreakers, False, bBreakersAreInfinite)
            If ipts.Count <= 0 Then
                _rVal.Add(aLine, bAddClone:=True)
                Return _rVal
            End If
            rWasBroken = True
            Dim sp As TVECTOR
            Dim ep As TVECTOR

            Dim aL As dxeLine
            Dim i As Long
            ipts.Sort(dxxSortOrders.NearestToFarthest, aLine.StartPt)
            sp = aLine.StartPt.Strukture
            ep = ipts.ItemVector(1)
            If dxfProjections.DistanceTo(sp, ep) > 0.001 Then
                aL = aLine.Clone
                aL.Value = True
                aL.SetVectors(sp, ep)
                _rVal.Add(aL)
            End If
            For i = 1 To ipts.Count
                If i + 1 > ipts.Count Then Exit For
                sp = ipts.ItemVector(i)
                ep = ipts.ItemVector(i + 1)
                aL = aLine.Clone
                If dxfProjections.DistanceTo(sp, ep) > 0.001 Then
                    aL.SetVectors(sp, ep)
                    aL.Value = True
                    _rVal.Add(aL)
                End If
            Next i
            sp = ipts.LastVector.Strukture
            ep = aLine.EndPt.Strukture
            If dxfProjections.DistanceTo(sp, ep) > 0.001 Then
                aL = aLine.Clone
                aL.Value = True
                aL.SetVectors(sp, ep)
                _rVal.Add(aL)
            End If
            If aIntercepts IsNot Nothing Then aIntercepts.Append(ipts)
            Return _rVal
        End Function

        Friend Shared Function break_LineByDashes(aLine As TLINE, aOrigin As TVECTOR, aDashCount As Integer, aDashes() As Double, Optional aOffset As Double = 0.0, Optional aScaler As Double = 1, Optional bSuppressProj As Boolean = False, Optional aCollector As colDXFEntities = Nothing) As colDXFEntities
            Dim _rVal As colDXFEntities = Nothing
            Try
                If aCollector Is Nothing Then _rVal = New colDXFEntities Else _rVal = aCollector
                If aDashCount <= 0 Then Return _rVal
                Dim v0 As TVECTOR
                Dim xDir As TVECTOR
                Dim aSP As TVECTOR
                Dim aEP As TVECTOR
                '**UNUSED VAR** Dim v1 As TVECTOR
                Dim d1 As Double
                Dim totPat As Double
                Dim i As Integer
                Dim aDir As TVECTOR
                '**UNUSED VAR** Dim eps As colDXFVectors
                Dim d2 As Double
                Dim aDash As Double
                Dim aStep As Double
                Dim llen As Double
                Dim mpt As TVECTOR
                Dim dmpt As Double
                Dim bBeforMidPt As Boolean
                Dim bOnLine As Boolean
                Dim bLine As New TLINE
                Dim halfLen As Double
                'get the direction of the line
                xDir = aLine.SPT.DirectionTo(aLine.EPT, False, llen)
                llen = Math.Round(llen, 5)
                If llen <= 0.0001 Then Return _rVal
                halfLen = llen / 2
                'get the length of one pattern
                For i = 0 To aDashCount - 1
                    totPat += Math.Abs(aDashes(i)) * aScaler
                Next i
                If Math.Round(totPat, 5) <= 0.0001 Then Return _rVal
                'get the origin that must be on the path of the line but
                'not necessarily within the start and end points
                v0 = aOrigin
                If Not bSuppressProj Then
                    v0 = dxfProjections.ToLine(v0, aLine)

                End If
                'apply the offset to the origin
                If aOffset <> 0 Then v0 += xDir * (aScaler * aOffset)
                'get the direction from the lines start pt to the origin
                aDir = aLine.SPT.DirectionTo(v0, False, d1)
                'determine how many patterns between the origin and the line start pt
                i = Fix(d1 / totPat) + 1
                'set the dash start back to somewhere before the line start
                If aDir.Equals(xDir, 2) Then
                    'if the origin is in line with the direction of the line
                    bLine.EPT = v0 + (xDir * (-i * totPat))
                Else
                    'if the origin is the opposite direction the direction of the line
                    bLine.EPT = v0 + xDir * (i * totPat)
                End If
                bLine.SPT = bLine.EPT + (xDir * -totPat)
                'get the total distance to create dashes on
                d1 = bLine.SPT.DistanceTo(aLine.EPT)
                d2 = 0
                bBeforMidPt = True
                mpt = aLine.SPT.Interpolate(aLine.EPT, 0.5)
                'leap frog from the dash start pt down the line making the dashes as you go
                'we are done when d2 exceeds the total dashing distance
                aSP = bLine.SPT
                dmpt = aSP.DistanceTo(mpt)
                Do Until d2 >= d1
                    'loop on the dash length
                    For i = 0 To aDashCount - 1
                        aDash = aDashes(i)
                        aStep = Math.Abs(aDash) * aScaler
                        'increment d2 with the current dash length
                        d2 += aStep
                        'jump the ep forward from the current start
                        If aDash < 0 Then
                            '(negatives mean pen up - no line added!)
                            aEP = aSP + xDir * aStep
                        ElseIf aDash = 0 Then
                            aEP = aSP
                            If mpt.DistanceTo(aSP, 5) <= halfLen Then
                                _rVal.AddLineV(aSP, aSP)
                            End If
                        Else
                            aEP = aSP + (xDir * aStep)
                            If bBeforMidPt Then
                                'before we get to the midpt the line is only kept if the end pt is on the line
                                bOnLine = mpt.DistanceTo(aEP, 5) < halfLen
                                If bOnLine Then
                                    If mpt.DistanceTo(aSP, 5) > halfLen Then
                                        aSP = aLine.SPT
                                    End If
                                    _rVal.AddLineV(aSP, aEP)
                                End If
                            Else
                                'after we get to the midpt the line is only kept if the start pt is on the line
                                bOnLine = mpt.DistanceTo(aSP, 5) < halfLen
                                If bOnLine Then
                                    If mpt.DistanceTo(aEP, 5) > halfLen Then
                                        aEP = aLine.EPT
                                    End If
                                    _rVal.AddLineV(aSP, aEP)
                                End If
                            End If
                        End If
                        'move the start pt to the end of the last dash
                        aSP = aEP
                        'use the distance to the mid pt to determine when we cross half way
                        dmpt -= aStep
                        If dmpt < 0 Then bBeforMidPt = False
                    Next i
                Loop
            Catch ex As Exception
                _rVal = New colDXFEntities
                _rVal.AddLineV2(aLine)
            End Try
            Return _rVal
        End Function
        Friend Shared Function break_LineIntoSegments(aLine As TLINE, aSegLengths As TVALUES) As TLINES
            Dim _rVal As New TLINES(0)

            Try
                Dim patlen As Double = aSegLengths.Total(True)
                If patlen <= 0 Then Throw New Exception("Null Pattern Length Found")
                Dim aDir As TVECTOR
                Dim aFlg As Boolean
                Dim bFlg As Boolean
                Dim cFlg As Boolean
                Dim d1 As Double
                aDir = aLine.SPT.DirectionTo(aLine.EPT, False, aFlg, d1)
                If aFlg Then Throw New Exception("Direction Is Null")
                'aSegLengths.Print()
                Dim d2 As Double
                '**UNUSED VAR** Dim spath As TPATH
                Dim i As Long
                Dim j As Integer
                Dim mp As TVECTOR
                '**UNUSED VAR** Dim aLoop As TVECTORS
                '**UNUSED VAR** Dim bLoop As TVECTORS
                Dim Segs As Long
                Dim seg As Double
                Dim sp As TVECTOR
                Dim ep As TVECTOR
                Dim d3 As Double
                Dim v1 As TVECTOR
                Dim v2 As TVECTOR
                Dim sid As Long
                Dim sDir As TVECTOR
                '    d2 = -((segs * patlen) - d1) / 2
                d2 = 0
                For i = aSegLengths.Count To 1 Step -1
                    seg = aSegLengths.Member(i - 1)
                    d2 += Math.Abs(seg)
                    If seg > 0 Then
                        d2 -= seg / 2
                        Exit For
                    End If
                Next i
                mp = aLine.SPT.MidPt(aLine.EPT)
                d3 = -d2
                Segs = 0
                Do Until d3 >= (d1 / 2)
                    If d3 + patlen <= (d1 / 2) Then
                        d3 += patlen
                        Segs += 1
                    Else
                        d3 += patlen
                        Segs += 1
                        Exit Do
                    End If
                Loop
                sp = mp + (aDir * -d3)
                Segs = 2 * Segs + 1
                aFlg = False
                cFlg = False
                d3 = aLine.EPT.DistanceTo(sp)
                For sid = 1 To Segs
                    For j = 1 To aSegLengths.Count
                        seg = aSegLengths.Member(j - 1)
                        d3 -= Math.Abs(seg)
                        ep = sp + (aDir * Math.Abs(seg))
                        If Not aFlg Then
                            sDir = aLine.SPT.DirectionTo(sp, False, bFlg)
                            If bFlg Then
                                aFlg = True
                            Else
                                aFlg = aDir.Equals(sDir, 3)
                            End If
                        End If
                        If Not cFlg Then
                            sDir = ep.DirectionTo(aLine.EPT, False, bFlg)
                            If bFlg Then
                                cFlg = True
                            Else
                                cFlg = aDir.Equals(sDir, 3)
                            End If
                        End If
                        v1 = sp
                        v2 = ep
                        If aFlg And Not cFlg Then
                            v2 = aLine.EPT
                            cFlg = True
                        End If
                        If Not aFlg And cFlg Then
                            v1 = aLine.SPT
                            aFlg = True
                        End If
                        '            If cFlg And d3 >= d1 Then
                        '                v2 = aLine.EPT
                        '            End If
                        If seg > 0 Then
                            If aFlg And cFlg Then
                                _rVal.Add(v1, v2)
                            End If
                        End If
                        sp = ep
                        If d3 <= 0 Then Exit For
                    Next j
                    If d3 <= 0 Then Exit For
                Next sid
                If _rVal.Count > 0 Then
                    ep = _rVal.Item(_rVal.Count).EPT
                    sDir = ep.DirectionTo(aLine.EPT, False, bFlg)
                    If Not bFlg Then
                        bFlg = aDir.Equals(sDir, 3)
                        If Not bFlg Then _rVal.SetEndPt(_rVal.Count, aLine.EPT)
                    End If
                End If
            Catch ex As Exception
                _rVal = New TLINES(0)
                _rVal.Add(aLine)
            End Try
            Return _rVal
        End Function
        Friend Shared Function break_LineV(aLine As TSEGMENT, aBreakers As TSEGMENTS, Optional bBreakersAreInfinite As Boolean = False, Optional Intercepts As colDXFVectors = Nothing, Optional aDisplaySettings As dxfDisplaySettings = Nothing) As colDXFEntities
            Dim rWasBroken As Boolean = False
            Return dxfBreakTrimExtend.break_LineV(aLine, aBreakers, bBreakersAreInfinite, rWasBroken, Intercepts)
        End Function
        Friend Shared Function break_LineV(aLine As TSEGMENT, aBreakers As TSEGMENTS, bBreakersAreInfinite As Boolean, ByRef rWasBroken As Boolean, Optional Intercepts As colDXFVectors = Nothing, Optional aDisplaySettings As dxfDisplaySettings = Nothing) As colDXFEntities
            Dim _rVal As colDXFEntities = Nothing
            '#1the line to break
            '#2the entity or entities to use to break the line
            '#3flag indicating that the breaker(s) are of infinite length
            '#4returns true if the line was broken
            '#5returns the points of intersection that were used for the break points
            '^returns the line broken into parts at the intersection of its self and the passed segment or segments
            rWasBroken = False
            Try
                _rVal = New colDXFEntities
                aLine.IsArc = False
                Intercepts = New colDXFVectors
                aLine.Marker = False
                _rVal.AddArcLineV(aLine, aDisplaySettings)
                If aLine.LineStructure.SPT.DistanceTo(aLine.LineStructure.EPT) <= 0.000001 Then Return _rVal
                dxfIntersections.LAE_LAES(aLine, aBreakers, False, bBreakersAreInfinite, 1000, 1000, Intercepts)
                rWasBroken = Intercepts.Count > 0
                If Not rWasBroken Then Return _rVal
                Dim sp As TVECTOR
                Dim ep As TVECTOR
                '**UNUSED VAR** Dim idx As Long
                Dim aL As New TSEGMENT
                Dim i As Long
                Dim RetCol As colDXFEntities
                RetCol = New colDXFEntities
                sp = aLine.LineStructure.SPT
                aL = aLine
                Intercepts.Sort(dxxSortOrders.NearestToFarthest, CType(sp, dxfVector))
                ep = Intercepts.ItemVector(1)
                If dxfProjections.DistanceTo(sp, ep) > 0.0001 Then
                    aL.LineStructure.SPT = sp
                    aL.LineStructure.EPT = ep
                    aL.Marker = True
                    RetCol.AddArcLineV(aL, aDisplaySettings)
                End If
                For i = 1 To Intercepts.Count
                    If i + 1 > Intercepts.Count Then Exit For
                    sp = Intercepts.ItemVector(i)
                    ep = Intercepts.ItemVector(i + 1)
                    If dxfProjections.DistanceTo(sp, ep) > 0.001 Then
                        aL.LineStructure.SPT = sp
                        aL.LineStructure.EPT = ep
                        aL.Marker = True
                        RetCol.AddArcLineV(aL, aDisplaySettings)
                    End If
                Next i
                sp = Intercepts.LastVector.Strukture
                ep = aLine.LineStructure.EPT
                If dxfProjections.DistanceTo(sp, ep) > 0.001 Then
                    aL.LineStructure.SPT = sp
                    aL.LineStructure.EPT = ep
                    aL.Marker = True
                    RetCol.AddArcLineV(aL, aDisplaySettings)
                End If
                _rVal = RetCol
            Catch ex As Exception
                rWasBroken = False
            End Try
            Return _rVal
        End Function
        Public Shared Function break_Polyline(aPline As dxfPolyline, aBreaker As Object, Optional bBreakersAreInfinite As Boolean = False) As colDXFEntities
            Dim rWasBroken As Boolean
            Dim rIntersects As colDXFVectors = Nothing
            Return dxfBreakTrimExtend.break_Polyline(aPline, aBreaker, bBreakersAreInfinite, rWasBroken, rIntersects)
        End Function
        Public Shared Function break_Polyline(aPline As dxfPolyline, aBreaker As Object, bBreakersAreInfinite As Boolean, ByRef rWasBroken As Boolean, ByRef rIntersects As colDXFVectors) As colDXFEntities
            Dim _rVal As New colDXFEntities
            '#1the polyline to break
            '#1the entity or entities to use to break the polyline
            '#2flag indicating that the breaker(s) are of infinite length
            '#3returns true if the polyline was broken
            '#4returns the points of intersection that were used for the break points
            '^returns the polyline broken into parts at the intersection of its self and the passed segment or segments
            rWasBroken = False
            rIntersects = New colDXFVectors
            If aBreaker Is Nothing Or aPline.Vertices.Count <= 1 Or aPline Is Nothing Then
                _rVal.Add(aPline, bAddClone:=True)
                Return _rVal
            End If
            Dim Segs As colDXFEntities
            Dim bFlag As Boolean
            Dim sSegs As colDXFEntities
            aPline.UpdatePath()
            Segs = New colDXFEntities(aPline.Segments)
            sSegs = break_Segments(Segs, aBreaker, bBreakersAreInfinite, bFlag, rIntersects)
            If Not bFlag Then
                _rVal.Add(aPline, bAddClone:=True)
            Else
                rIntersects.RemoveCoincidentVectors(3)
                _rVal = sSegs
            End If
            Return _rVal
        End Function
        Public Shared Function break_Segments(aEnts As IEnumerable(Of dxfEntity), aBreaker As dxfEntity, Optional bBreakersAreInfinite As Boolean = False) As colDXFEntities
            Dim rBreakSegsWereBroken As Boolean
            Dim rIntersects As colDXFVectors = Nothing
            Return dxfBreakTrimExtend.break_Segments(aEnts, aBreaker, bBreakersAreInfinite, rBreakSegsWereBroken, rIntersects)
        End Function
        Public Shared Function break_Segments(aEnts As IEnumerable(Of dxfEntity), aBreaker As dxfEntity, bBreakersAreInfinite As Boolean, ByRef rBreakSegsWereBroken As Boolean) As colDXFEntities
            Dim rIntersects As colDXFVectors = Nothing
            Return dxfBreakTrimExtend.break_Segments(aEnts, aBreaker, bBreakersAreInfinite, rBreakSegsWereBroken, rIntersects)
        End Function
        Public Shared Function break_Segments(aEnts As IEnumerable(Of dxfEntity), aBreaker As dxfEntity, bBreakersAreInfinite As Boolean, ByRef rBreakSegsWereBroken As Boolean, rIntersects As colDXFVectors) As colDXFEntities
            '#1the segments to break
            '#2the entity or entities to use to break the segments
            '#3flag indicating that the breakers are of infinite length
            '#4returns true if any of the segments were broken
            '#5returns the points of intersection that were used for the break points
            '^breaks the passed collection of segments (arcs and lines) broken at their intersection points with the breaking object(s)
            Dim _rVal As New colDXFEntities
            rBreakSegsWereBroken = False

            Dim bFlag As Boolean
            Dim ePts As colDXFVectors = Nothing


            Dim subSegs As colDXFEntities
            If rIntersects Is Nothing Then rIntersects = New colDXFVectors
            For Each aEnt As dxfEntity In aEnts
                If aEnt Is Nothing Then Continue For
                If aEnt.GraphicType = dxxGraphicTypes.Arc Then
                    Dim aA As dxeArc = DirectCast(aEnt, dxeArc)
                    subSegs = break_Arc(aA, aBreaker, bBreakersAreInfinite, bFlag, ePts)
                    _rVal.Append(subSegs, False)
                    rIntersects.Append(ePts, False)
                    If bFlag Then rBreakSegsWereBroken = True
                ElseIf aEnt.GraphicType = dxxGraphicTypes.Line Then
                    Dim aL As dxeLine = DirectCast(aEnt, dxeLine)
                    subSegs = break_Line(aL, aBreaker, bBreakersAreInfinite, bFlag, ePts)
                    _rVal.Append(subSegs, False)
                    rIntersects.Append(ePts, False)
                    If bFlag Then rBreakSegsWereBroken = True
                End If
            Next
            Return _rVal
        End Function



        Public Shared Function split_Polygon(aPolygon As dxfPolyline, aSplitType As dxxSplitTypes, aSplitCenter As dxfVector, Optional aGap As Double = 0.0, Optional aAngle As Double = 0.0, Optional bReturnGap As Boolean = False) As colDXFEntities
            Dim rSplitOccured As Boolean
            Return dxfBreakTrimExtend.split_Polygon(aPolygon, aSplitType, aSplitCenter, aGap, aAngle, bReturnGap, rSplitOccured)
        End Function
        Public Shared Function split_Polygon(aPolygon As dxfPolyline, aSplitType As dxxSplitTypes, aSplitCenter As dxfVector, aGap As Double, aAngle As Double, bReturnGap As Boolean, ByRef rSplitOccured As Boolean) As colDXFEntities
            Dim _rVal As New colDXFEntities
            rSplitOccured = False
            If aPolygon Is Nothing Then Return _rVal
            If aSplitType <> dxxSplitTypes.Horizontal And aSplitType <> dxxSplitTypes.Vertical And aSplitType <> dxxSplitTypes.ByAngle Then aSplitType = dxxSplitTypes.Vertical

            If aSplitType = dxxSplitTypes.Horizontal Then
                aAngle = 0
            ElseIf aSplitType = dxxSplitTypes.Vertical Then
                aAngle = 90
            Else
                aAngle = TVALUES.NormAng(aAngle, False, True, True)
            End If

            Dim aSegs As colDXFEntities = aPolygon.Segments
            Dim v1 As TVECTOR
            Dim lSegs As New colDXFEntities
            Dim gSegs As New colDXFEntities
            Dim rSegs As New colDXFEntities
            Dim aPl As New TPLANE("")
            Dim iLine1 As dxeLine
            Dim iLine2 As dxeLine

            Dim iLines As New colDXFEntities
            Dim bCS As dxfPlane = aPolygon.Plane
            Dim aPg As dxfEntity

            bCS.Rotate(aAngle)
            bCS.OriginV = v1
            aPl = bCS.Strukture

            If aSplitCenter Is Nothing Then
                v1 = aPolygon.Center.Strukture
            Else
                v1 = aSplitCenter.Strukture.ProjectedTo(aPl)
            End If

            aGap = Math.Abs(aGap)
            If aGap > 0.01 Then
                iLine1 = bCS.HorizontalLine(0.5 * aGap, 10)
                iLine2 = bCS.HorizontalLine(-0.5 * aGap, 10)
            Else
                iLine1 = bCS.HorizontalLine(0, 10)
                iLine2 = Nothing
            End If
            iLines.Add(iLine1)
            iLines.Add(iLine2)
            aPg = aPolygon.Clone
            v1 = iLine1.MidPtV + aPl.YDirection * 10
            rSplitOccured = trim_Polyline_Line(aPg, iLine1, v1, True)
            If aSplitType = dxxSplitTypes.Horizontal Then aPg.Flag = "TOP" Else aPg.Flag = "RIGHT"
            _rVal.Add(aPg)
            If rSplitOccured Then
                If iLine2 IsNot Nothing Then
                    aPg = aPolygon.Clone
                    v1 = iLine2.MidPtV + aPl.YDirection * -10
                    trim_Polyline_Line(aPg, iLine2, v1, True)
                    _rVal.Add(aPg)
                    If aSplitType = dxxSplitTypes.Horizontal Then aPg.Flag = "BOTTOM" Else aPg.Flag = "LEFT"
                    If bReturnGap Then
                        aPg = aPolygon.Clone
                        v1 = iLine1.MidPtV + aPl.YDirection * -10
                        trim_Polyline_Line(aPg, iLine1, v1, True)
                        _rVal.Add(aPg)
                        v1 = iLine2.MidPtV + aPl.YDirection * 10
                        trim_Polyline_Line(aPg, iLine2, v1, True)
                        aPg.Flag = "GAP"
                    End If
                End If
            End If
            'split_Polygon.Append iLines
            Return _rVal
        End Function
        Public Shared Function trim_Bounds_Arc(aArc As dxeArc, aBounders As IEnumerable(Of dxfEntity), Optional bReturnExteriors As Boolean = False, Optional bTrimmed As Boolean = False, Optional aDiscards As colDXFEntities = Nothing, Optional bUseClosedSegments As Boolean = True) As colDXFEntities
            Dim _rVal As colDXFEntities = Nothing
            '#1the arc to trim
            '#2the bounding segments
            '#3flag to return the segments that lie outside the bounding segments
            '#4returns True if the arc is actually trimmed
            '#5returns the parts that were discarded
            '#6flag to treat gaps as lines
            '^Return the segments of the arc which lie with the bounding segments

            _rVal = New colDXFEntities
            aDiscards = New colDXFEntities
            bTrimmed = False
            If aArc Is Nothing Then Return _rVal
            If aBounders Is Nothing Then
                If bReturnExteriors Then _rVal.Add(aArc, bAddClone:=True) Else aDiscards.Add(aArc, bAddClone:=True)
                Return _rVal
            End If
            If aBounders.Count <= 0 Then
                If bReturnExteriors Then _rVal.Add(aArc, bAddClone:=True) Else aDiscards.Add(aArc, bAddClone:=True)

                Return _rVal
            End If
            Dim tSegs As colDXFEntities
            Dim bParts As colDXFEntities
            Dim aA As dxeArc
            Dim v1 As TVECTOR
            'use the closed segments
            If bUseClosedSegments Then
                tSegs = dxfEntities.GetClosedSegments(aBounders)
            Else
                tSegs = aBounders
            End If
            bParts = break_Arc(aArc, tSegs, False, bTrimmed)
            If Not bTrimmed Then
                _rVal.Add(aArc, bAddClone:=True)
            Else
                For i As Integer = 1 To bParts.Count
                    aA = bParts.Item(i)
                    If aA.SpannedAngle > 0 Then
                        v1 = aA.MidPtV
                        Dim srch As iVector = New dxfVector(v1)
                        Dim enclosed As Boolean = dxfEntities.EncloseVector(tSegs, srch, 0.001, Nothing, False, True)
                        If Not bReturnExteriors Then
                            If enclosed Then _rVal.Add(aA) Else aDiscards.Add(aA)
                        Else
                            If Not enclosed Then _rVal.Add(aA) Else aDiscards.Add(aA)
                        End If
                    End If
                Next i
            End If
            Return _rVal
        End Function
        Public Shared Function trim_Bounds_Line(aLine As dxeLine, aBounders As colDXFEntities, Optional bReturnExteriors As Boolean = False, Optional bTrimmed As Boolean = False, Optional aDiscards As colDXFEntities = Nothing, Optional bUseClosedSegments As Boolean = True) As colDXFEntities
            Dim _rVal As New colDXFEntities
            '#1the line to trim
            '#2the bounding segments
            '#3flag to return the segments that lie outside the bounding segments
            '#4returns True if the line is actually trimmed
            '#5returns the parts that were discarded
            '#6flag to treat gaps as lines
            '^Return the segments of the line which lie with the bounding segments
            'On Error Resume Next

            aDiscards = New colDXFEntities
            bTrimmed = False
            If aLine Is Nothing Then Return _rVal
            If aBounders Is Nothing Then
                If bReturnExteriors Then _rVal.Add(aLine, bAddClone:=True) Else aDiscards.Add(aLine, bAddClone:=True)
                Return _rVal
            End If
            If aBounders.Count <= 0 Then
                If bReturnExteriors Then _rVal.Add(aLine, bAddClone:=True) Else aDiscards.Add(aLine, bAddClone:=True)
                Return _rVal
            End If
            Dim tSegs As colDXFEntities
            Dim bParts As colDXFEntities
            Dim aL As dxeLine
            Dim v1 As TVECTOR
            'use the closed segments
            If bUseClosedSegments Then
                tSegs = dxfEntities.GetClosedSegments(aBounders)
            Else
                tSegs = aBounders
            End If
            bParts = break_Line(aLine, tSegs, False, bTrimmed)
            If Not bTrimmed Then
                _rVal.Add(aLine, bAddClone:=True)
            Else
                For i As Integer = 1 To bParts.Count
                    aL = bParts.Item(i)
                    If aL.Length > 0 Then
                        v1 = aL.MidPtV
                        Dim srch As iVector = New dxfVector(v1)
                        Dim enclosed As Boolean = dxfEntities.EncloseVector(tSegs, srch, 0.001, Nothing, False, True)
                        If Not bReturnExteriors Then
                            If enclosed Then _rVal.Add(aL) Else aDiscards.Add(aL)
                        Else
                            If Not enclosed Then _rVal.Add(aL) Else aDiscards.Add(aL)
                        End If
                    End If
                Next i
            End If
            Return _rVal
        End Function
        Friend Shared Function trim_Line_Line(aLine As dxeLine, aTrimLine As dxeLine, aKeepPoint As TVECTOR, Optional bTrimmerIsInfinite As Boolean = False) As Boolean
            Dim _rVal As Boolean
            '#1the line to trim
            '#2the line to trim with
            '#3a point to determine which side of the line to keep
            '#4flag indicating the passed trim line should be treated as infinite
            '^trims the first line with the second line
            If aLine Is Nothing Then Return _rVal
            If aTrimLine Is Nothing Then Return _rVal
            Dim bFlag As Boolean
            Dim _Struc As New TLINE
            _Struc = trim_Line_LineV(New TLINE(aLine), New TLINE(aTrimLine), aKeepPoint, bTrimmerIsInfinite, bFlag)
            If bFlag Then
                aLine.LineStructure = _Struc
                _rVal = True
            End If
            Return _rVal
        End Function
        Friend Shared Function trim_Line_LineV(aLine As TLINE, aTrimLine As TLINE, aKeepPoint As TVECTOR, bTrimmerIsInfinite As Boolean, ByRef rTrimOcurred As Boolean) As TLINE
            Dim _rVal As New TLINE
            '#1the line to trim
            '#2the line to trim with
            '#3a point to determine which side of the line to keep
            '#4flag indicating the passed trim line should be treated as infinite
            '^trims the first line with the second line
            Dim ip As TVECTOR
            Dim aFlag As Boolean
            Dim bFlag As Boolean
            Dim cFlag As Boolean
            Dim kDir As TVECTOR = TVECTOR.Zero
            Dim aDir As TVECTOR = TVECTOR.Zero
            Dim mpt As TVECTOR
            Dim d1 As Double
            Dim v1 As TVECTOR
            Dim eFlag As Boolean
            Dim dpos As Boolean
            rTrimOcurred = False
            _rVal = aLine
            If aLine.SPT.DistanceTo(aLine.EPT) <= 0.00001 Then Return _rVal
            If aTrimLine.SPT.DistanceTo(aTrimLine.EPT) <= 0.00001 Then Return _rVal
            Dim rLinesAreCoincident As Boolean = False
            ip = aLine.IntersectionPt(aTrimLine, aFlag, rLinesAreCoincident, bFlag, cFlag, eFlag)
            If Not bTrimmerIsInfinite And Not cFlag Then Return _rVal
            If Not eFlag Then Return _rVal
            If aLine.SPT.DistanceTo(ip) <= 0.00001 Then Return _rVal
            If aLine.EPT.DistanceTo(ip) <= 0.00001 Then Return _rVal
            v1 = dxfProjections.ToLine(aKeepPoint, aTrimLine, d1, bFlag, dpos)
            If d1 < 0.001 Then
                kDir = aTrimLine.SPT.DirectionTo(aTrimLine.EPT)
                kDir.RotateAbout(TVECTOR.WorldZ, 90, False)
            Else
                kDir = aKeepPoint.DirectionTo(v1)
            End If
            rTrimOcurred = True
            mpt = dxfProjections.ToLine(aLine.SPT.Interpolate(ip, 0.5), aTrimLine, aDir)
            kDir.Equals(aDir, True, 1, bFlag)
            If bFlag Then _rVal = New TLINE(ip, aLine.EPT) Else _rVal = New TLINE(aLine.SPT, ip)
            Return _rVal
        End Function
        Public Shared Function trim_Polygon_Line(Polygon As dxePolygon, TrimLineObj As iLine, RefPtXY As iVector, LineIsInfinite As Boolean, ByRef rTrimPerformed As Boolean, Optional keepSegs As colDXFEntities = Nothing, Optional DiscardSegs As colDXFEntities = Nothing, Optional bUseLineForGaps As Boolean = False, Optional bTrimAddSegs As Boolean = True) As dxePolygon
            Dim _rVal As dxePolygon
            '#1the polygon to trim
            '#2the line to trim the polygon with
            '#3the point that indicates the side of the line to keep
            '#4flag to treat the passed line as infinite
            '#5return flag indicating if any actual trimming was performed
            '^returns the trimmed version of the passed polygon
            Dim eStr As String
            rTrimPerformed = False
            Try
                If Polygon Is Nothing Then Throw New Exception("The Passed Polygon Is Nothing")
                If Polygon.Vertices.Count < 2 Then Throw New Exception("The Passed Polygon Is Undefined")
                If TrimLineObj Is Nothing Then Throw New Exception("The Passed Trim Line Is Undefined")
                If RefPtXY Is Nothing Then Throw New Exception("The Passed Reference Point Is Undefined")
                Dim trimLine As New TLINE(TrimLineObj)
                If trimLine.Length = 0 Then Throw New Exception("The Passed Trim Line Is Undefined")
                Dim aCS As New TPLANE(Polygon.Plane)
                Dim RefPt As New TVECTOR(RefPtXY)
                RefPt.ProjectTo(aCS)
                Dim KeepLine As New TLINE(RefPt, RefPt.ProjectedTo(trimLine))
                If KeepLine.Length <= 0 Then Throw New Exception("Invalide Trim Point Detected")
                Dim bWasBroken As Boolean
                Dim Segs As colDXFEntities = break_Segments(Polygon.Segments, TrimLineObj, LineIsInfinite, bWasBroken)
                Dim aSeg As dxfEntity
                Dim Discard As New colDXFEntities
                Dim Keep As New colDXFEntities
                Dim v1 As dxfVector
                Dim dsp As New dxfDisplaySettings
                If TypeOf TrimLineObj Is dxfEntity Then
                    Dim ent As dxfEntity = DirectCast(TrimLineObj, dxfEntity)
                    dsp = ent.DisplaySettings
                End If



                Dim kDir As dxfDirection
                Dim aDir As dxfDirection
                Dim cGapIds As New List(Of Integer)
                'intialize

                rTrimPerformed = False
                'decide which segments to keep
                Segs = break_Segments(Polygon.Segments, TrimLineObj, LineIsInfinite, bWasBroken)
                If Not bWasBroken Then
                    Keep = Segs
                Else
                    rTrimPerformed = True
                    kDir = KeepLine.Direction
                    For i As Integer = 1 To Segs.Count
                        aSeg = Segs.Item(i)
                        v1 = aSeg.DefinitionPoint(dxxEntDefPointTypes.MidPt)
                        aDir = dxeLine.FromTo(v1, v1.ProjectedToLine(TrimLineObj)).Direction
                        If aDir.IsEqual(kDir, 2) Then
                            Keep.Add(aSeg)
                        Else
                            Discard.Add(aSeg)
                        End If
                    Next i
                End If
                _rVal = Polygon.Clone
                If rTrimPerformed Then
                    _rVal.Vertices = dxfUtils.ExtractVertices(Keep, 0, dxfLinetypes.Invisible, False, False, rGapIDs:=cGapIds, "")
                    For i = 1 To cGapIds.Count
                        v1 = _rVal.Vertices.Item(cGapIds.Item(i - 1))
                        If bUseLineForGaps Then v1.CopyDisplayValues(dsp) Else v1.CopyDisplayValues(Polygon.DisplaySettings)
                    Next i
                End If
                If keepSegs IsNot Nothing Then keepSegs.Append(Keep, True)
                If DiscardSegs IsNot Nothing Then DiscardSegs.Append(Discard, True)
                If bTrimAddSegs Then
                    Keep.Clear()
                    Discard.Clear()
                    Segs = break_Segments(Polygon.AdditionalSegments, TrimLineObj, LineIsInfinite, bWasBroken)
                    If Not bWasBroken Then
                        Keep = Segs
                    Else
                        rTrimPerformed = True
                        kDir = KeepLine.Direction
                        For i = 1 To Segs.Count
                            aSeg = Segs.Item(i)
                            v1 = aSeg.DefinitionPoint(dxxEntDefPointTypes.MidPt)
                            aDir = dxeLine.FromTo(v1, v1.ProjectedToLine(TrimLineObj)).Direction
                            If aDir.IsEqual(kDir, 2) Then
                                Keep.Add(aSeg)
                            Else
                                Discard.Add(aSeg)
                            End If
                        Next i
                    End If
                    _rVal.AdditionalSegments = Keep
                    If keepSegs IsNot Nothing Then keepSegs.Append(Keep, True)
                    If DiscardSegs IsNot Nothing Then DiscardSegs.Append(Discard, True)
                End If
            Catch ex As Exception
                eStr = ex.Message
                Throw New Exception("Trim_Polygon_Line - " & eStr)
            End Try
            Return _rVal
        End Function
        Public Shared Function trim_Polygon_Ortho(aPolygon As dxePolygon, aTrimType As dxxTrimTypes, ByRef TrimCoordinate As Double, Optional rTrimPerformed As Boolean? = Nothing, Optional bDoAddSegs As Boolean = True, Optional bDoSubPGons As Boolean = False) As dxePolygon
            If rTrimPerformed IsNot Nothing Then rTrimPerformed = False
            If aPolygon Is Nothing Then Return Nothing
            If aTrimType < 0 Then aTrimType = 0
            If aTrimType > 4 Then aTrimType = 4
            Dim _rVal As dxePolygon = aPolygon.Clone
            Dim trimLine As dxeLine
            Dim aDir As TVECTOR
            Dim kpt As TVECTOR
            Dim bCS As dxfPlane
            bCS = aPolygon.Plane.Clone
            bCS.OriginV = New TVECTOR
            If aTrimType = dxxTrimTypes.Above Or aTrimType = dxxTrimTypes.Below Then
                trimLine = bCS.HorizontalLine(TrimCoordinate, 100)
            Else
                trimLine = bCS.VerticalLine(TrimCoordinate, 100)
            End If
            kpt = trimLine.MidPtV
            If aTrimType = dxxTrimTypes.Above Then
                aDir = bCS.YDirection.Inverse.Strukture
            ElseIf aTrimType = dxxTrimTypes.Below Then
                aDir = bCS.YDirectionV
            ElseIf aTrimType = dxxTrimTypes.Left Then
                aDir = bCS.XDirectionV
            ElseIf aTrimType = dxxTrimTypes.Right Then
                aDir = bCS.XDirection.Inverse.Strukture
            End If
            kpt = trimLine.MidPtV + (aDir * 1000)
            Dim rtrimmed As Boolean = trim_Polyline_Line(_rVal, trimLine, kpt, True, bDoAddSegs, bDoSubPGons)
            If rTrimPerformed IsNot Nothing Then rTrimPerformed = rtrimmed
            Return _rVal
        End Function
        Public Shared Sub trim_Polygons_Ortho(aPgons As colDXFEntities, aTrimType As dxxTrimTypes, ByRef TrimCoordinate As Double, Optional rTrimPerformed As Boolean? = Nothing, Optional bDoAddSegs As Boolean = True, Optional bDoSubPGons As Boolean = False)
            '#1the type of trim to perform
            '#2the x or y coordinate to trim at
            '#3return flag indicating if any actual trimming was performed
            '^trims the current polygon at the indicted coordinate
            '~only allows vertical or horizontal line trimming.
            If aPgons Is Nothing Then Return
            Dim aPGon As dxePolygon
            Dim aEnt As dxfEntity
            Dim aFlag As Boolean
            For i As Integer = 1 To aPgons.Count
                aEnt = aPgons.Item(i)
                If aEnt.GraphicType = dxxGraphicTypes.Polygon Then
                    aPGon = aEnt
                    aPGon.TrimWithOrthoLine(aTrimType, TrimCoordinate, aFlag, bDoAddSegs, bDoSubPGons)
                    If aFlag And rTrimPerformed IsNot Nothing Then rTrimPerformed = True
                End If
            Next i
            Return
Err:
        End Sub
        Public Shared Function trim_Polyline_Arc(aPline As dxfPolyline, aTrimArc As dxeArc, Optional aKeepPoint As iVector = Nothing, Optional bTrimmerIsInfinite As Boolean = False, Optional bDoAddSegs As Boolean = True, Optional bDoSubPGons As Boolean = False) As Boolean
            Dim _rVal As Boolean
            '#1polyline to trim
            '#2the arc to trim with
            '#3a point to determine which side of the arc to keep
            '#4flag indicating the passed arc should be treated as infinite (360 degree span)
            '^trims the passed polyline with the passed arc
            Dim aPl As dxfPlane = aPline.Plane
            Dim kpt As TVECTOR

            If aTrimArc Is Nothing Or aPline.Vertices.Count <= 1 Then Return _rVal
            If aTrimArc.SpannedAngle <= 0 Then Return _rVal
            Dim Segs As colDXFEntities
            Dim bFlag As Boolean
            Dim aIntersects As colDXFVectors = Nothing
            Dim aSeg As dxfEntity
            Dim bSeg As dxfEntity
            Dim verts As colDXFVectors
            Dim mpt As TVECTOR
            Dim ep As dxfVector
            Dim sp As dxfVector
            Dim bClosed As Boolean
            Dim d1 As Double
            Dim bKeepInside As Boolean
            Dim kDist As Double
            Dim cp As TVECTOR
            If aKeepPoint Is Nothing Then aKeepPoint = aTrimArc.Center
            kpt = New TVECTOR(aPl, aKeepPoint)
            kDist = aTrimArc.Radius + 0.000001
            cp = aTrimArc.CenterV
            bKeepInside = cp.DistanceTo(kpt) <= kDist
            aPline.UpdatePath()
            Segs = break_Segments(aPline.Segments, aTrimArc, bTrimmerIsInfinite, bFlag, aIntersects)
            If Not bFlag Then Return _rVal
            bClosed = Segs.LastMember.DefinitionPoint(dxxEntDefPointTypes.EndPt).IsEqual(Segs.FirstMember.DefinitionPoint(dxxEntDefPointTypes.StartPt), 3)
            _rVal = True
            For i As Integer = Segs.Count To 1 Step -1
                aSeg = Segs.Item(i)
                mpt = aSeg.DefinitionPoint(dxxEntDefPointTypes.MidPt).Strukture
                d1 = cp.DistanceTo(mpt)
                If bKeepInside Then
                    If d1 > kDist Then Segs.Remove(aSeg.GUID)
                Else
                    If d1 <= kDist Then Segs.Remove(aSeg.GUID)
                End If
            Next i
            verts = New colDXFVectors
            For i = 1 To Segs.Count
                aSeg = Segs.Item(i)
                If i + 1 <= Segs.Count Then bSeg = Segs.Item(i + 1) Else bSeg = Segs.Item(i)
                verts.Add(aSeg.DefinitionPoint(dxxEntDefPointTypes.StartPt))
                ep = aSeg.DefinitionPoint(dxxEntDefPointTypes.EndPt).Clone
                sp = bSeg.DefinitionPoint(dxxEntDefPointTypes.StartPt)
                If Not ep.Strukture.Equals(sp.Strukture, 3) Then
                    If i < Segs.Count Then
                        ep.Radius = aTrimArc.Radius
                        verts.Add(ep)
                    Else
                        If Not ep.Strukture.Equals(sp.Strukture, 3) Then
                            If aSeg.Value Then
                                ep.Radius = aTrimArc.Radius
                                verts.Add(ep)
                            Else
                                '                       If Segs.Item(1).Value Then
                                '                            bClosed = False
                                '                        End If
                            End If
                        End If
                    End If
                End If
            Next i
            Dim wuz As Boolean
            wuz = aPline.SuppressEvents
            aPline.SuppressEvents = True
            aPline.Vertices.Populate(verts, False)
            aPline.Closed = bClosed
            '    If bDoAddSegs And aPline.GraphicType = dxxGraphicTypes.Polygon Then
            '        If Not aPline.SubEntities Is Nothing Then
            '            aPline.SubEntities
            aPline.SuppressEvents = wuz
            Return _rVal
        End Function
        Friend Shared Function trim_Polyline_Line(aPline As dxfPolyline, aTrimLine As iLine, aKeepPoint As TVECTOR, Optional bTrimmerIsInfinite As Boolean = False, Optional bDoAddSegs As Boolean = True, Optional bDoSubPGons As Boolean = False) As Boolean
            Dim _rVal As Boolean
            '#1polyline to trim
            '#2the line to trim with
            '#3a point to determine which side of the line to keep
            '#4flag indicating the passed line should be treated as infinite
            '^trims the passed polyline with the passed line
            If aPline Is Nothing Then Return _rVal
            If aTrimLine Is Nothing Then Return _rVal
            If aPline.Vertices.Count <= 1 Then Return _rVal
            Dim aPl As dxfPlane = aPline.Plane

            Dim trmL As New TLINE(aTrimLine, aPl)
            If trmL.Length <= 0 Then Return _rVal
            Dim kpt As TVECTOR = aKeepPoint.ProjectedTo(aPl.Strukture)
            Dim Segs As colDXFEntities
            Dim bFlag As Boolean
            Dim aIntersects As colDXFVectors = Nothing
            Dim kDir As TVECTOR
            Dim aDir As TVECTOR
            Dim aSeg As dxfEntity
            Dim mpt As TVECTOR
            Dim kLn As New TLINE(trmL)
            Dim d1 As Double
            Dim v1 As TVECTOR = dxfProjections.ToLine(kpt, kLn, d1, bFlag)
            Dim ppt As TVECTOR
            Dim keepSegs As colDXFEntities
            'define the keep direction
            'segments to one side are kept others are discarded
            If d1 < 0.001 Then
                kDir = trmL.Direction
                kDir.RotateAbout(aPl.ZDirectionV, 90, False)
            Else
                kDir = kpt.DirectionTo(v1)
            End If
            aPline.UpdatePath()
            Segs = aPline.Segments
            'break the poyline segment with the trim line
            keepSegs = break_Segments(Segs, New dxeLine(trmL), bTrimmerIsInfinite, bFlag, aIntersects)
            If Not bFlag Then Return _rVal
            _rVal = True
            For i As Integer = keepSegs.Count To 1 Step -1
                aSeg = keepSegs.Item(i)
                'project the mdipoint of the segment to the trim line
                'if projection direction is not equal to the keep direction then remove it
                mpt = aSeg.DefinitionPoint(dxxEntDefPointTypes.MidPt).Strukture
                ppt = dxfProjections.ToLine(mpt, kLn, aDir, d1)
                'kDir.Equals(aDir, True, 1, bFlag)
                If aDir.Equals(kDir * -1, False, 1) And d1 <> 0 Then
                    keepSegs.Remove(aSeg.GUID)
                End If
            Next i
            If aPline.GraphicType = dxxGraphicTypes.Polygon Then
                If keepSegs.Count > 1 Then
                    If Not keepSegs.LastMember.DefinitionPoint(dxxEntDefPointTypes.EndPt).IsEqual(keepSegs.Item(1).DefinitionPoint(dxxEntDefPointTypes.StartPt), aPrecis:=3) Then
                        keepSegs.AddLine(keepSegs.LastMember.DefinitionPoint(dxxEntDefPointTypes.EndPt), keepSegs.Item(1).DefinitionPoint(dxxEntDefPointTypes.StartPt), aPline.DisplaySettings)
                    End If
                End If
            End If
            aPline.Segments = keepSegs
            Return _rVal
        End Function
        Public Shared Function trim_Polyline_Ortho(aPolyline As dxePolyline, aTrimType As dxxTrimTypes, ByRef TrimCoordinate As Double, ByRef rTrimPerformed As Boolean?) As dxePolyline
            Dim _rVal As dxePolyline = Nothing
            rTrimPerformed = False
            If aPolyline Is Nothing Then Return _rVal
            If aTrimType < 0 Then aTrimType = 0
            If aTrimType > 4 Then aTrimType = 4
            _rVal = aPolyline.Clone
            Dim trmLine As dxeLine
            Dim aDir As TVECTOR
            Dim kpt As TVECTOR
            If aTrimType = dxxTrimTypes.Above Or aTrimType = dxxTrimTypes.Below Then
                trmLine = aPolyline.Plane.HorizontalLine(TrimCoordinate, 100)
            Else
                trmLine = aPolyline.Plane.VerticalLine(TrimCoordinate, 100)
            End If
            kpt = trmLine.MidPtV
            If aTrimType = dxxTrimTypes.Above Then aDir = aPolyline.Plane.YDirection.Inverse.Strukture
            If aTrimType = dxxTrimTypes.Below Then aDir = aPolyline.PlaneV.YDirection
            If aTrimType = dxxTrimTypes.Left Then aDir = aPolyline.PlaneV.XDirection
            If aTrimType = dxxTrimTypes.Right Then aDir = (aPolyline.PlaneV.XDirection * -1)
            kpt = trmLine.MidPtV + (aDir * 1000)
            Dim btrimed As Boolean = trim_Polyline_Line(_rVal, trmLine, kpt, True)
            If rTrimPerformed IsNot Nothing Then rTrimPerformed = btrimed
            Return _rVal
        End Function
        Public Shared Function trim_SegmentsWithPolyline(aSegments As colDXFEntities, aPolyline As dxfPolyline, Optional rTrimmedParts As colDXFEntities = Nothing) As colDXFEntities
            Dim _rVal As New colDXFEntities
            '#1the polygon to use to trim the segments in the collection
            '#2the trimmed part of the segments that are external to the passed polygon
            '^returns the parts of the segments that lie with the passed polygon
            Try
                If aSegments Is Nothing Then Throw New Exception("The Passed Segments are Undefined")
                If aPolyline Is Nothing Then Throw New Exception("The Passed Polygon Is Undefined")

                Dim aSeg As dxfEntity
                Dim aBnd As colDXFEntities

                Dim aA As dxeArc
                Dim aL As dxeLine
                Dim sParts As colDXFEntities
                Dim aFlag As Boolean
                Dim v1 As TVECTOR
                Dim tSegs As List(Of dxfEntity) = aSegments.ArcsAndLines
                If tSegs.Count <= 0 Then Return _rVal
                aPolyline.UpdatePath()
                Dim aEnt As dxfEntity = aPolyline
                aBnd = New colDXFEntities(aEnt.Components.Segments)
                If aBnd.Count <= 0 Then Return _rVal
                For i As Integer = 1 To tSegs.Count
                    aSeg = tSegs.Item(i - 1)
                    If aSeg.GraphicType = dxxGraphicTypes.Arc Then
                        aA = aSeg
                        sParts = aA.Break(aBnd, aFlag)
                    Else
                        aL = aSeg
                        sParts = aL.Break(aBnd, aFlag)
                    End If
                    For j As Integer = 1 To sParts.Count
                        aSeg = sParts.Item(j)
                        If aSeg.Length > 0 Then
                            v1 = New TVECTOR(aSeg.DefinitionPoint(dxxEntDefPointTypes.MidPt))
                            Dim bOn As Boolean
                            Dim bept As Boolean
                            If dxfEntities.EncloseVector(aBnd, v1, 0.001, bOn, bept, bOnBoundIsIn:=True) Then

                                _rVal.Add(aSeg)
                            Else
                                If rTrimmedParts IsNot Nothing Then rTrimmedParts.Add(aSeg)
                                End If
                            End If
                    Next j
                Next i
            Catch ex As Exception
                Throw New Exception($"trim_SegmentsWithPolyline - {ex.Message} ")
            End Try
            Return _rVal
        End Function
        Public Shared Function trim_SegmentsWithPolylines(aSegments As colDXFEntities, aPolygons As Object, Optional KeepInteriors As Boolean = True, Optional KeepExteriors As Boolean = False) As colDXFEntities
            Dim _rVal As colDXFEntities = Nothing
            '#1the polygon to us to trim the member segments
            '#2flag to retain the sub segments that are interior to the passed polygons
            '#3the parts of the segments that where discarded
            '^trims each member with the passed polygons and reatins the interior or exterior sub segments
            '~returns the trimmed parts
            Try
                _rVal = New colDXFEntities
                If aSegments Is Nothing Then Return _rVal
                If aPolygons Is Nothing Then Return _rVal
                '**UNUSED VAR** Dim aMem As dxfEntity
                Dim aEnt As dxfEntity
                Dim i As Long
                Dim aPl As dxfPolyline
                Dim inParts As colDXFEntities
                Dim outParts As colDXFEntities
                Dim lParts As colDXFEntities = Nothing
                inParts = New colDXFEntities
                outParts = New colDXFEntities
                inParts.Populate(aSegments.ArcsAndLines(True))
                For i = 1 To aPolygons.Count
                    aEnt = aPolygons.Item(i)
                    If aEnt.EntityType = dxxEntityTypes.Polygon Or aEnt.EntityType = dxxEntityTypes.Polyline Then
                        aPl = aEnt
                        inParts = trim_SegmentsWithPolyline(inParts, aPl, lParts)
                        outParts.Append(lParts)
                    End If
                Next i
                If Not KeepInteriors Then
                    _rVal.Append(inParts, False, "INSIDE")
                End If
                If Not KeepExteriors Then
                    _rVal.Append(outParts, False, "OUTSIDE")
                End If
            Catch ex As Exception
                Throw New Exception("trim_SegmentsWithPolylines - " & ex.Message)
            End Try
            Return _rVal
        End Function
#End Region 'Methods
    End Class 'dxfBreakTrimExtend
End Namespace
