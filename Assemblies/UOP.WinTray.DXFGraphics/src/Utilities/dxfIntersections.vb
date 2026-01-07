Imports System.Numerics
Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics.Utilities
    Public Class dxfIntersections
#Region "Methods"
        Friend Shared Function ArcEllipse(aArc As TARC, aEllipse As TARC, Optional aCurveDivisions As Integer = 1000, Optional aCollector As colDXFVectors = Nothing) As TVECTORS
            Dim _rVal As New TVECTORS
            aCurveDivisions = TVALUES.LimitedValue(aCurveDivisions, 10, 10000, 1000)
            '#1the arc segment
            '#2the intersecting ellipse entity
            '#3the number of divisions to break the ellipse into to for intersection
            '^returns the vector where passed entity and line segments intersect (if any exist)
            '~returns any empty collection if the two segments don't intersect.
            Dim i As Long
            Dim bLine As TLINE
            Dim ips As TVECTORS
            Dim tLns As TLINES
            Dim bCpl As Boolean
            Dim aDir As TVECTOR
            Dim v1 As TVECTOR
            Dim j As Integer
            Dim aCreateLines As Boolean
            If aArc.Plane.Origin.DistanceTo(aEllipse.Plane.Origin, 6) > Math.Round(aArc.Radius + aEllipse.Radius, 6) Then
                Return _rVal
            End If
            bCpl = TPLANES.Compare(aArc.Plane, aEllipse.Plane, 3, False, False)
            tLns = aEllipse.EllipseSegments
            aCreateLines = tLns.Count = 0 Or tLns.Count <> aCurveDivisions
            If aCreateLines Then
                tLns = aEllipse.PhantomLines(aCurveDivisions, True)
            End If
            aEllipse.EllipseSegments = tLns
            For i = 1 To tLns.Count
                bLine = tLns.Item(i)
                ips = bLine.IntersectionPts(aArc, bCpl, False, True)
                If ips.Count > 0 Then
                    For j = 1 To ips.Count
                        v1 = ips.Item(j)
                        aDir = aArc.Plane.Origin.DirectionTo(v1)
                        v1 = aArc.Plane.Origin + (aDir * aArc.Radius)
                        _rVal.Add(v1)
                        If aCollector IsNot Nothing Then aCollector.AddV(v1)
                    Next j
                End If
            Next i
            Return _rVal
        End Function
        Friend Shared Function EllipseEllipse(aEllipse As TARC, bEllipse As TARC, Optional aCurveDivisions As Integer = 1000, Optional bCurveDivisions As Integer = 1000, Optional aCollector As colDXFVectors = Nothing) As TVECTORS
            Dim _rVal As New TVECTORS
            '#1the subject ellipse segment
            '#2the intersecting ellipse entity
            '#3the number of divisions to break the first ellipse into to for intersection
            '#4the number of divisions to break the second ellipse into to for intersection
            '#5returns then lines from the first ellipse used for intersection
            '#6returns then lines from the first second used for intersection
            '^returns the vectors where passed ellipses intersect
            '~returns any empty collection if the two segments don't intersect.
            aCurveDivisions = TVALUES.LimitedValue(aCurveDivisions, 10, 10000, 1000)
            bCurveDivisions = TVALUES.LimitedValue(bCurveDivisions, 10, 10000, 1000)
            If aEllipse.Plane.Origin.DistanceTo(bEllipse.Plane.Origin, 6) > Math.Round(aEllipse.Radius + bEllipse.Radius, 6) Then
                Return _rVal
            End If
            Dim aFlag As Boolean
            Dim bFlag As Boolean
            Dim aTestLines As TLINES = aEllipse.EllipseSegments
            Dim bTestLines As TLINES = bEllipse.EllipseSegments
            Dim aCreateLines As Boolean = aTestLines.Count = 0 Or aTestLines.Count <> aCurveDivisions
            Dim bCreateLines As Boolean = bTestLines.Count = 0 Or bTestLines.Count <> bCurveDivisions
            If aCreateLines Then
                aTestLines = aEllipse.PhantomLines(aCurveDivisions, True)
            End If
            If bCreateLines Then
                bTestLines = bEllipse.PhantomLines(bCurveDivisions, True)
            End If
            aEllipse.EllipseSegments = aTestLines
            bEllipse.EllipseSegments = bTestLines
            Dim rIsOnFirstLine As Boolean = False
            Dim rIsOnSecondLine As Boolean = False
            Dim rInterceptExists As Boolean = False
            For i As Integer = 1 To aTestLines.Count
                Dim aL As TLINE = aTestLines.Item(i)
                For j As Integer = 1 To bTestLines.Count
                    Dim bL As TLINE = bTestLines.Item(j)
                    Dim v1 As TVECTOR = aL.IntersectionPt(bL, aFlag, bFlag, rIsOnFirstLine, rIsOnSecondLine, rInterceptExists)
                    If Not aFlag And Not bFlag Then
                        _rVal.Add(v1)
                        If aCollector IsNot Nothing Then aCollector.AddV(v1)
                    End If
                Next j
            Next i
            Return _rVal
        End Function
        Friend Shared Function LAES_LAES(aArcsLines1 As TSEGMENTS, aArcsLines2 As TSEGMENTS, Optional aEntIsInfinite As Boolean = False, Optional bEntIsInfinite As Boolean = False, Optional aCurveDivisions As Integer = 1000, Optional bCurveDivisions As Integer = 1000, Optional aCollector As colDXFVectors = Nothing) As TVECTORS
            Dim _rVal As New TVECTORS
            ', Optional bNoSort As Boolean, Optional bColRow As Boolean) As colDXFVectors
            '#1the first entity or collection of entities
            '#2the second entity or collection of entities
            '#3flag to treat the first entity as infinitely long
            '#4flag to treat the second entity as infinitely long
            '#5returns true if the two entities are tangent to each other
            '#6flag to only return the points that lie on both entities
            '#7the number of segment divisions to divide the first entity into to find the intersection points if the first entity is a bezier curve
            '#8the number of segment divisions to divide the second entity into to find the intersection points if the second entity is a bezier curve
            '^returns the intersection points of the passed entities
            Try
                Dim ips As TVECTORS
                Dim aLAE As TSEGMENT
                Dim bLAE As TSEGMENT
                aCurveDivisions = TVALUES.LimitedValue(aCurveDivisions, 10, 10000, 1000)
                Dim tangent As Boolean = False
                For i As Integer = 1 To aArcsLines1.Count
                    aLAE = aArcsLines1.Item(i)
                    For j As Integer = 1 To aArcsLines2.Count
                        bLAE = aArcsLines2.Item(j)
                        ips = dxfIntersections.LAE_LAE(aLAE, bLAE, tangent, True, aEntIsInfinite, bEntIsInfinite, aCurveDivisions, bCurveDivisions, False, aCollector)
                        _rVal.Append(ips)
                    Next j
                Next i
                Return _rVal
            Catch
                Return _rVal
            End Try
        End Function
        Friend Shared Function LAE_LAE(aEntity As TSEGMENT, bEntity As TSEGMENT, Optional bOnlyOnBoth As Boolean = False, Optional aLineIsInfinite As Boolean = False, Optional bLineIsInfinite As Boolean = False, Optional aCurveDivisions As Integer = 1000, Optional bCurveDivisions As Integer = 1000, Optional bDontReturnEndToStart As Boolean = False) As TVECTORS
            Dim rIsTangent As Boolean = False
            Return dxfIntersections.LAE_LAE(aEntity, bEntity, bOnlyOnBoth, rIsTangent, aLineIsInfinite, bLineIsInfinite, aCurveDivisions, bCurveDivisions, bDontReturnEndToStart, Nothing)
        End Function
        Friend Shared Function LAE_LAE(aEntity As TSEGMENT, bEntity As TSEGMENT, ByRef rIsTangent As Boolean, bOnlyOnBoth As Boolean, aLineIsInfinite As Boolean, bLineIsInfinite As Boolean, aCurveDivisions As Integer, bCurveDivisions As Integer, bDontReturnEndToStart As Boolean, Optional aCollector As colDXFVectors = Nothing) As TVECTORS
            Dim _rVal As New TVECTORS
            Dim eType1 As dxxGraphicTypes = dxxGraphicTypes.Line
            Dim eType2 As dxxGraphicTypes = dxxGraphicTypes.Line
            Dim ip As TVECTOR
            Dim aFlag As Boolean
            Dim i As Long
            Dim k As Long
            Dim j As Long
            Dim v1 As TVECTOR
            Dim rVecs As TVECTORS
            Dim eps1 As New TVECTORS
            Dim eps2 As New TVECTORS
            Dim bKeep As Boolean
            rVecs = New TVECTORS
            rIsTangent = False
            aLineIsInfinite = aEntity.INFINITE
            bLineIsInfinite = bEntity.INFINITE
            If aCurveDivisions < 10 Then aCurveDivisions = 10
            If aCurveDivisions > 10000 Then aCurveDivisions = 10000
            If aEntity.IsArc Then
                If aEntity.ArcStructure.Elliptical Then eType1 = dxxGraphicTypes.Ellipse Else eType1 = dxxGraphicTypes.Arc
            End If
            If bEntity.IsArc Then
                If bEntity.ArcStructure.Elliptical Then eType2 = dxxGraphicTypes.Ellipse Else eType2 = dxxGraphicTypes.Arc
            End If
            If bDontReturnEndToStart Then
                If eType1 = dxxGraphicTypes.Ellipse Or eType2 = dxxGraphicTypes.Ellipse Then
                    bDontReturnEndToStart = False
                Else
                    eps1 = New TVECTORS
                    eps2 = eps1
                End If
            End If
            '---- LINE and arc,line or ellipse
            Select Case eType1
                Case dxxGraphicTypes.Line
                    If bDontReturnEndToStart Then
                        eps1.Add(aEntity.LineStructure.EPT)
                        eps1.Add(aEntity.LineStructure.SPT)
                    End If
                    Select Case eType2
                        Case dxxGraphicTypes.Line 'LINE-LINE
                            If bDontReturnEndToStart Then
                                eps2.Add(bEntity.LineStructure.EPT)
                                eps2.Add(bEntity.LineStructure.SPT)
                            End If
                            ip = aEntity.LineStructure.IntersectionPt(bEntity.LineStructure, aFlag)
                            If aFlag Then
                                rVecs.Add(ip)
                            End If
                        Case dxxGraphicTypes.Arc 'LINE-ARC
                            rVecs = aEntity.LineStructure.IntersectionPts(bEntity.ArcStructure)
                            rIsTangent = rVecs.Count = 1
                            If bDontReturnEndToStart Then
                                eps2.Add(bEntity.ArcStructure.Plane.AngleVector(bEntity.ArcStructure.StartAngle, bEntity.ArcStructure.Radius, False))
                                eps2.Add(bEntity.ArcStructure.Plane.AngleVector(bEntity.ArcStructure.EndAngle, bEntity.ArcStructure.Radius, False))
                            End If
                        Case dxxGraphicTypes.Ellipse 'LINE-ELLIPSE
                            rVecs = aEntity.LineStructure.IntersectionPts(bEntity.ArcStructure, aCurveDivisions:=bCurveDivisions)
                            rIsTangent = rVecs.Count = 1
                    End Select
         '---- ARC and arc,line or ellipse
                Case dxxGraphicTypes.Arc
                    If bDontReturnEndToStart Then
                        eps1.Add(aEntity.ArcStructure.Plane.AngleVector(aEntity.ArcStructure.StartAngle, aEntity.ArcStructure.Radius, False))
                        eps1.Add(aEntity.ArcStructure.Plane.AngleVector(aEntity.ArcStructure.EndAngle, aEntity.ArcStructure.Radius, False))
                    End If
                    Select Case eType2
                        Case dxxGraphicTypes.Line 'ARC-LINE
                            If bDontReturnEndToStart Then
                                eps2.Add(bEntity.LineStructure.EPT)
                                eps2.Add(bEntity.LineStructure.SPT)
                            End If
                            rVecs = bEntity.LineStructure.IntersectionPts(aEntity.ArcStructure)
                            rIsTangent = rVecs.Count = 1
                        Case dxxGraphicTypes.Arc 'ARC-ARC
                            If bDontReturnEndToStart Then
                                eps2.Add(bEntity.ArcStructure.Plane.AngleVector(bEntity.ArcStructure.StartAngle, bEntity.ArcStructure.Radius, False))
                                eps2.Add(bEntity.ArcStructure.Plane.AngleVector(bEntity.ArcStructure.EndAngle, bEntity.ArcStructure.Radius, False))
                            End If
                            rVecs = aEntity.ArcStructure.IntersectionPts(bEntity.ArcStructure)
                            rIsTangent = rVecs.Count = 1
                        Case dxxGraphicTypes.Ellipse 'ARC-ElLLIPSE
                            rVecs = dxfIntersections.ArcEllipse(aEntity.ArcStructure, bEntity.ArcStructure, bCurveDivisions)
                            rIsTangent = rVecs.Count = 1
                    End Select
         '---- ELLIPSE and arc,line or ellipse
                Case dxxGraphicTypes.Ellipse
                    Select Case eType2
                        Case dxxGraphicTypes.Line 'ELLIPSE-LINE
                            rVecs = bEntity.LineStructure.IntersectionPts(aEntity.ArcStructure, aCurveDivisions:=aCurveDivisions)
                            rIsTangent = rVecs.Count = 1
                        Case dxxGraphicTypes.Arc 'ELLIPSE-ARC
                            rVecs = dxfIntersections.ArcEllipse(bEntity.ArcStructure, aEntity.ArcStructure, aCurveDivisions)
                            rIsTangent = rVecs.Count = 1
                        Case dxxGraphicTypes.Ellipse 'ELLIPSE-ElLLIPSE
                            rVecs = dxfIntersections.EllipseEllipse(aEntity.ArcStructure, bEntity.ArcStructure, aCurveDivisions, bCurveDivisions)
                            rIsTangent = rVecs.Count = 1
                    End Select
            End Select
            If rVecs.Count > 0 Then
                For i = 1 To rVecs.Count
                    v1 = rVecs.Item(i)
                    bKeep = True
                    If bOnlyOnBoth Then
                        If Not bEntity.ContainsVector(v1, bTreatAsInfinite:=bEntity.INFINITE) Then bKeep = False
                        If bKeep Then
                            If Not aEntity.ContainsVector(v1, bTreatAsInfinite:=aEntity.INFINITE) Then bKeep = False
                        End If
                    Else
                        If Not bEntity.INFINITE Then
                            If Not bEntity.ContainsVector(v1) Then bKeep = False
                        End If
                        If Not aEntity.INFINITE And bKeep Then
                            If Not aEntity.ContainsVector(v1) Then bKeep = False
                        End If
                    End If
                    If bKeep Then
                        If bDontReturnEndToStart Then
                            For j = 1 To eps1.Count
                                For k = 1 To eps2.Count
                                    If eps1.Item(j).DistanceTo(eps2.Item(k), 3) <= 0 Then
                                        bKeep = False
                                        Exit For
                                    End If
                                Next k
                            Next j
                        End If
                    End If
                    If bKeep Then
                        _rVal.Add(v1.Clone)
                        If aCollector IsNot Nothing Then aCollector.AddV(v1.Clone)
                    End If
                Next i
            End If
            Return _rVal
        End Function
        Friend Shared Function LAE_LAES(aSegment As TSEGMENT, aArcsLines As TSEGMENTS, Optional aEntIsInfinite As Boolean = False, Optional bEntIsInfinite As Boolean = False, Optional aCurveDivisions As Integer = 1000, Optional bCurveDivisions As Integer = 1000, Optional aCollector As colDXFVectors = Nothing) As TVECTORS
            Dim _rVal As New TVECTORS
            ', Optional bNoSort As Boolean, Optional bColRow As Boolean) As colDXFVectors
            '#1the first entity or collection of entities
            '#2the second entity or collection of entities
            '#3flag to treat the first entity as infinitely long
            '#4flag to treat the second entity as infinitely long
            '#5returns true if the two entities are tangent to each other
            '#6flag to only return the points that lie on both entities
            '#7the number of segment divisions to divide the first entity into to find the intersection points if the first entity is a bezier curve
            '#8the number of segment divisions to divide the second entity into to find the intersection points if the second entity is a bezier curve
            '^returns the intersection points of the passed entities
            Try


                For i As Integer = 1 To aArcsLines.Count
                    Dim aLAE As TSEGMENT = aArcsLines.Item(i)
                    Dim ips As TVECTORS = dxfIntersections.LAE_LAE(aSegment, aLAE, False, True, aEntIsInfinite, bEntIsInfinite, aCurveDivisions, bCurveDivisions, False, aCollector)
                    _rVal.Append(ips)
                Next i
                Return _rVal
            Catch
                Return _rVal
            End Try
        End Function
        Friend Shared Function LinePlane(aLine As TLINE, aPlane As TPLANE) As TVECTOR
            Dim rIntersectIsOnLine As Boolean = False
            Dim rCoplanar As Boolean = False
            Return dxfIntersections.LinePlane(aLine, aPlane, rIntersectIsOnLine, rCoplanar)
        End Function
        Friend Shared Function LinePlane(aLine As TLINE, aPlane As TPLANE, ByRef rIntersectIsOnLine As Boolean, ByRef rCoplanar As Boolean) As TVECTOR
            Dim _rVal As TVECTOR = TVECTOR.Zero
            '#1the subject line
            '#2the subject plane
            '#3returns true if the intersection point lines on the passed line
            '#4returns true if the passed line lies on the passed plane
            '^returns the intersection of the plane and the passed line
            rIntersectIsOnLine = False
            rCoplanar = True
            Dim lDir As TVECTOR
            lDir = aLine.EPT - aLine.SPT
            If lDir.Magnitude = 0 Then Return _rVal
            If aPlane.ZDirection.Magnitude = 0 Then Return _rVal
            Dim u As Double
            Dim denom As Double
            Dim numer As Double
            '    pln_Cooefficients PlaneNormal, PlaneOrigin, a, B, C, D
            '    denom = a * (sp.X - ep.X) + B * (sp.Y - ep.Y) + C * (sp.Z - ep.Z)
            denom = aPlane.ZDirection.DotProduct(lDir)
            If denom = 0 Then
                'the line is either coplanar or parallel to the plane
                rCoplanar = aLine.SPT.DistanceTo(aPlane, -1) <= 0.0001
                If Not rCoplanar Then
                    _rVal = aLine.SPT
                    rIntersectIsOnLine = True
                End If
                Return _rVal
            Else
                rCoplanar = False
                'the line intersects the plane
                '        numer = a * sp.X + B * sp.Y + C * sp.Z + D
                numer = aPlane.ZDirection.DotProduct((aPlane.Origin - aLine.SPT))
                u = numer / denom
                'deterime if the intersect lines on the passed line
                rIntersectIsOnLine = u > -0.0001 And u < 1.0001
                _rVal = aLine.SPT + ((aLine.EPT - aLine.SPT) * u)
            End If
            Return _rVal
        End Function
        Friend Shared Function LineSphere(aLine As TLINE, CenterPt As TVECTOR, aRadius As Double, Optional aCollector As colDXFVectors = Nothing) As TVECTORS
            Dim rLength As Double = 0.0
            Return dxfIntersections.LineSphere(aLine, CenterPt, aRadius, rLength, aCollector)
        End Function
        Friend Shared Function LineSphere(aLine As TLINE, CenterPt As TVECTOR, aRadius As Double, ByRef rLength As Double, Optional aCollector As colDXFVectors = Nothing) As TVECTORS
            Dim _rVal As New TVECTORS
            '#1 the line
            '#2the center of the sphere
            '#3the radius of the sphere
            '^returns the intersection vectors of the defined line and the defined sphere
            Dim aSP As TVECTOR = aLine.SPT
            Dim aEP As TVECTOR = aLine.EPT
            _rVal = New TVECTORS
            rLength = dxfProjections.DistanceTo(aSP, aEP)
            If rLength <= 0.00000001 Then Return _rVal
            If aRadius <= 0 Then Return _rVal
            Dim A As Double
            Dim B As Double
            Dim C As Double
            Dim D As Double
            Dim Sq As Double
            Dim a2 As Double
            Dim mu As Double
            Dim i1 As TVECTOR
            Dim i2 As TVECTOR
            Dim aDir As TVECTOR
            Dim vRel As TVECTOR
            Dim cp As TVECTOR
            cp = CenterPt
            'calculate vectors
            aDir = aEP - aSP
            vRel = aSP - cp
            'set up quadratic equation
            A = aDir.SquareSum
            B = 2 * aDir.MultiSum(vRel)
            C = cp.SquareSum + aSP.SquareSum - (2 * cp.MultiSum(aSP)) - aRadius ^ 2
            'calculate discriminant
            D = (B * B) - (4 * A * C)
            If D < 0 Or A = 0 Then
                'no intersection
                Return _rVal
                'determine number of solutions
            Else
                If D = 0 Then
                    'one intersection
                    'calculate intersect vector
                    mu = -B / (2 * A)
                    i1.X = aSP.X + (mu * aDir.X)
                    i1.Y = aSP.Y + (mu * aDir.Y)
                    i1.Z = aSP.Z + (mu * aDir.Z)
                    _rVal.Add(i1)
                    If aCollector IsNot Nothing Then aCollector.AddV(i1)
                Else
                    'pre-calculate some values
                    Sq = Math.Sqrt(D)
                    a2 = 2 * A
                    'calculate first intersect vector
                    mu = (-B + Sq) / a2
                    i1.X = aSP.X + (mu * aDir.X)
                    i1.Y = aSP.Y + (mu * aDir.Y)
                    i1.Z = aSP.Z + (mu * aDir.Z)
                    _rVal.Add(i1)
                    If aCollector IsNot Nothing Then aCollector.AddV(i1)
                    'calculate second intersect vector
                    mu = (-B - Sq) / a2
                    i2.X = aSP.X + (mu * aDir.X)
                    i2.Y = aSP.Y + (mu * aDir.Y)
                    i2.Z = aSP.Z + (mu * aDir.Z)
                    _rVal.Add(i2)
                    If aCollector IsNot Nothing Then aCollector.AddV(i2)
                End If
            End If
            Return _rVal
        End Function
        Friend Shared Function PlanarArcArc(aArc As TARC, bArc As TARC, Optional aArcIsInfinite As Boolean = True, Optional bArcIsInfinite As Boolean = True, Optional aCollector As colDXFVectors = Nothing) As TVECTORS
            '^Finds the intersections of the two passed arcs
            '~assume  Circular arcs
            Dim _rVal As New TVECTORS(0)
            If aArc.SpannedAngle >= 359.99 Then aArcIsInfinite = True
            If bArc.SpannedAngle >= 359.99 Then bArcIsInfinite = True
            Dim aPl As TPLANE = aArc.Plane
            Dim bPl As TPLANE = bArc.Plane
            Dim aFlg As Boolean
            Dim r0 As Double = aArc.Radius
            Dim r1 As Double = bArc.Radius
            Dim dst As Double
            If Not aPl.DirectionsAreDefined Or r0 <= 0 Then Return _rVal 'null arc
            If Not bPl.DirectionsAreDefined Or r1 <= 0 Then Return _rVal 'null arc
            Dim aDir As TVECTOR = aPl.Origin.DirectionTo(bPl.Origin, False, aFlg, dst)
            If aFlg Then Return _rVal 'centers are coincident
            If Math.Round(dst, 15) > Math.Round(r0 + r1, 15) Then Return _rVal 'too far apart
            Dim v1 As TVECTOR
            Dim ips As New TVECTORS(0)
            If (Math.Round(dst, 15) = Math.Round(r0 + r1, 15)) Or (Math.Round(dst + r1, 15) = Math.Round(r0, 15)) Then
                'arcs are tangent one intersection
                v1 = aPl.Origin + (aDir * r0)
                ips.Add(v1.Clone)
            Else
                Dim C As TVECTOR = aPl.Origin.Clone
                Dim P1 As TVECTOR
                Dim P2 As TVECTOR
                Dim p3 As TVECTOR
                Dim xDir As TVECTOR = aDir
                Dim yDir As TVECTOR = xDir.RotatedAbout(aPl.ZDirection, 90, False)
                Dim c1 As New TVECTOR(dst, 0, 0)
                Dim u1 As New TVECTOR(dst, 0, 0) ' dir_Subtract(C1, C0)  'U
                Dim u3 As New TVECTOR(u1.Y, -u1.X, 0) 'V
                Dim d1 As Double = u1.Magnitude
                Dim d2 As Double
                Dim d3 As Double
                '        C0 = dir_Input(0, 0, 0, , True)
                If d1 <> 0 Then
                    d2 = 0.5 * (((r0 ^ 2 - r1 ^ 2) / d1 ^ 2) + 1) 's
                    d3 = (r0 ^ 2 / d1 ^ 2)
                    If d3 >= d2 ^ 2 Then
                        d3 = Math.Sqrt(d3 - d2 ^ 2) 't
                        P1 = u1 * d2
                        P1 += u3 * d3
                        P2 = C + (xDir * P1.X)
                        p3 = P2 + (yDir * -P1.Y)
                        P2 += yDir * P1.Y
                        ips.Add(P2.X, P2.Y, P2.Z)
                        ips.Add(p3.X, p3.Y, p3.Z)
                    End If
                End If
            End If
            If aArcIsInfinite And bArcIsInfinite Then
                _rVal = ips
                If aCollector IsNot Nothing Then aCollector.Append(ips)
            Else
                For i As Integer = 1 To ips.Count
                    v1 = ips.Item(i)
                    aFlg = True
                    If Not aArcIsInfinite Then
                        If Not aArc.ContainsVector(v1, aFudgeFactor:=0.001, bSuppressPlaneCheck:=True) Then aFlg = False
                    End If
                    If aFlg And Not bArcIsInfinite Then
                        If Not bArc.ContainsVector(v1, aFudgeFactor:=0.001, bSuppressPlaneCheck:=True) Then aFlg = False
                    End If
                    If aFlg Then
                        _rVal.Add(v1)
                        If aCollector IsNot Nothing Then aCollector.AddV(v1)
                    End If
                Next i
            End If
            Return _rVal
        End Function

        Public Shared Function LineArc(aLineStartPt As iVector, aLineEndPt As iVector, aArcCentPt As iVector, aArcRadius As Double, Optional aLineIsInfinite As Boolean = True, Optional aArcIsInfinite As Boolean = True, Optional aStartAngle As Double = 0, Optional aEndAngle As Double = 360, Optional aPrecis As Integer = 15) As colDXFVectors

            Dim line As New TLINE(New TVECTOR(aLineStartPt) With {.Z = 0}, New TVECTOR(aLineEndPt) With {.Z = 0})
            Dim arc As New TARC(New dxfVector(aArcCentPt) With {.Z = 0}, Math.Abs(aArcRadius))
            arc.StartAngle = IIf(aArcIsInfinite, 0, aStartAngle)
            arc.EndAngle = IIf(aArcIsInfinite, 360, aEndAngle)
            Return New colDXFVectors(PlanarLineArc(line, arc, aLineIsInfinite:=aLineIsInfinite, aArcIsInfinite, aPrecis:=aPrecis))
        End Function

        Friend Shared Function PlanarLineArc(aLine As TLINE, aArc As TARC, Optional aLineIsInfinite As Boolean = True, Optional aArcIsInfinite As Boolean = True, Optional aCollector As colDXFVectors = Nothing, Optional aPrecis As Integer = 15) As TVECTORS
            Dim _rVal As New TVECTORS
            Dim aFlg As Boolean
            aPrecis = TVALUES.LimitedValue(aPrecis, 1, 15)

            If Math.Round(aArc.Radius, aPrecis) = 0 Then Return _rVal 'null arc
            Dim aPl As TPLANE = aArc.Plane
            Dim aDir As TVECTOR = aLine.SPT.DirectionTo(aLine.EPT, aFlg)
            If aFlg Then Return _rVal 'null line
            Dim ips As New TVECTORS(0)

            Dim t(0 To 1) As Double


            If aArc.SpannedAngle >= 359.99 Then aArcIsInfinite = True
            Dim v2 As TVECTOR = aLine.EPT.WithRespectTo(aPl, aPrecis)
            Dim P As TVECTOR = aLine.SPT.WithRespectTo(aPl, aPrecis)
            aDir = New TVECTOR(v2.X - P.X, v2.Y - P.Y).Normalized
            Dim delta As TVECTOR = P

            Dim d1 As Double = aDir.Magnitude ^ 2
            Dim d2 As Double = delta.Magnitude ^ 2 - aArc.Radius ^ 2

            Dim omega As Double = aDir.DotProduct(delta) ^ 2 - d1 * d2
            Dim xDir As TVECTOR = aPl.XDirection.Normalized
            Dim yDir As TVECTOR = aPl.YDirection.Normalized
            Dim C As TVECTOR = aPl.Origin
            Select Case omega
                Case Is < 0
                    'no intersections
                Case Else
                    Dim d3 As Double = d1
                    d1 = (aDir * -1).DotProduct(delta)
                    d2 = Math.Sqrt(omega)
                    t(0) = (d1 + d2) / d3
                    Dim P1 As TVECTOR = P + (aDir * t(0))
                    Dim P2 As TVECTOR = C + (xDir * P1.X)
                    P2 += (yDir * P1.Y)
                    ips.Add(P2.X, P2.Y, P2.Z)
                    If omega <> 0 Then 'zero means tangent
                        t(1) = (d1 - d2) / d3
                        P1 = P + (aDir * t(1))
                        P2 = C + (xDir * P1.X)
                        P2 += (yDir * P1.Y)
                        ips.Add(P2.X, P2.Y, P2.Z)
                    End If
            End Select
            If aArcIsInfinite And aLineIsInfinite Then
                _rVal = ips
            Else
                Dim v1 As TVECTOR
                Dim bLine As New TSEGMENT(aLine)
                Dim bArc As New TSEGMENT(aArc)
                For i As Integer = 1 To ips.Count
                    v1 = ips.Item(i)
                    aFlg = True
                    If Not aLineIsInfinite Then
                        If Not aLine.ContainsVector(v1, aFudgeFactor:=0.001, bSuppressPlaneCheck:=True) Then aFlg = False
                        'If t(i - 1) > 1 Then aFlg = False
                    End If
                    If aFlg And Not aArcIsInfinite Then
                        If Not aArc.ContainsVector(v1, aFudgeFactor:=0.001, bSuppressPlaneCheck:=True) Then aFlg = False
                    End If
                    If aFlg Then
                        _rVal.Add(v1)
                        If aCollector IsNot Nothing Then aCollector.AddV(v1)
                    End If
                Next i
            End If
            Return _rVal
        End Function



        Friend Shared Function Point(aEntity As dxfEntity, bEntity As dxfEntity, Optional aEntIsInfinite As Boolean = False, Optional bEntIsInfinite As Boolean = False, Optional bMustBeOnBoth As Boolean = True) As dxfVector
            Dim rAllPoints As colDXFVectors = Nothing
            Dim rIsTangent As Boolean = False
            If aEntity Is Nothing Or bEntity Is Nothing Then Return Nothing
            Return dxfIntersections.Point(aEntity, bEntity, aEntIsInfinite, bEntIsInfinite, bMustBeOnBoth, rAllPoints, rIsTangent)
        End Function
        Friend Shared Function Point(aEntity As dxfEntity, bEntity As dxfEntity, aEntIsInfinite As Boolean, bEntIsInfinite As Boolean, bMustBeOnBoth As Boolean, ByRef rAllPoints As colDXFVectors, ByRef rIsTangent As Boolean) As dxfVector
            '#1the first entity to intersect
            '#2the second entity to intersect
            '#3flag to indicate that the first item should be treated as infinite
            '#4flag to indicate that the second item should be treated as infinite
            '#5flag to indicate that returned points must lie on both then passed segments
            '#6returns all the intersects
            '#7returns true if the entities are tangent
            '^returns the first of all intersection vectors of the passed entities
            If aEntity Is Nothing Or bEntity Is Nothing Then Return Nothing
            rAllPoints = New colDXFVectors
            Dim vpts As TVECTORS = dxfIntersections.EntityPoints(aEntity, bEntity, aEntIsInfinite, bEntIsInfinite, rIsTangent, bMustBeOnBoth, aCollector:=rAllPoints)
            Return rAllPoints.FirstOrDefault
        End Function

        Friend Shared Function Points(aEntities As IEnumerable(Of dxfEntity), bEntities As IEnumerable(Of dxfEntity), Optional aEntsAreInfinite As Boolean = False, Optional bEntsAreInfinite As Boolean = False, Optional bMustBeOnBoth As Boolean = True, Optional aCurveDivisions As Integer = 1000, Optional bCurveDivisions As Integer = 1000, Optional bNoSort As Boolean = False, Optional aCollector As colDXFVectors = Nothing) As TVECTORS
            '#1the first entity
            '#2a collection of entities
            '#3flag to treat the first entity as infinitely long
            '#4flag to treat the second entity as infinitely long
            '#5returns true if the two entities are tangent to each other
            '#6flag to only return the points that lie on both entities
            '#7the number of segment divisions to divide the first entity into to find the intersection points if the first entity is a bezier curve
            '#8the number of segment divisions to divide the second entity into to find the intersection points if the second entity is a bezier curve
            '^returns the intersection points of the passed entities
            Dim rIsTangent As Boolean = False
            Dim _rVal As New TVECTORS(0)
            If bEntities Is Nothing Or aEntities Is Nothing Then Return _rVal
            For Each ent As dxfEntity In aEntities
                If ent Is Nothing Then Continue For
                For Each bent As dxfEntity In bEntities
                    If bent Is Nothing Then Continue For
                    _rVal.Append(dxfIntersections.EntityPoints(ent, bent, aEntsAreInfinite, bEntsAreInfinite, rIsTangent, bMustBeOnBoth, aCurveDivisions, bCurveDivisions, bNoSort, aCollector))
                Next

            Next

            Return _rVal
        End Function

        Friend Shared Function Points(aEntity As dxfEntity, aEntities As IEnumerable(Of dxfEntity), Optional aEntIsInfinite As Boolean = False, Optional bEntsAreInfinite As Boolean = False, Optional bMustBeOnBoth As Boolean = True, Optional aCurveDivisions As Integer = 1000, Optional bCurveDivisions As Integer = 1000, Optional bNoSort As Boolean = False, Optional aCollector As colDXFVectors = Nothing) As TVECTORS
            '#1the first entity
            '#2a collection of entities
            '#3flag to treat the first entity as infinitely long
            '#4flag to treat the second entity as infinitely long
            '#5returns true if the two entities are tangent to each other
            '#6flag to only return the points that lie on both entities
            '#7the number of segment divisions to divide the first entity into to find the intersection points if the first entity is a bezier curve
            '#8the number of segment divisions to divide the second entity into to find the intersection points if the second entity is a bezier curve
            '^returns the intersection points of the passed entities
            Dim rIsTangent As Boolean = False
            Dim _rVal As New TVECTORS(0)
            If aEntity Is Nothing Or aEntities Is Nothing Then Return _rVal
            For Each ent As dxfEntity In aEntities
                If ent Is Nothing Then Continue For
                _rVal.Append(dxfIntersections.EntityPoints(aEntity, ent, aEntIsInfinite, bEntsAreInfinite, rIsTangent, bMustBeOnBoth, aCurveDivisions, bCurveDivisions, bNoSort, aCollector))
            Next

            Return _rVal
        End Function
        Friend Shared Function EntityPoints(aEntity As dxfEntity, bEntity As dxfEntity, aEntIsInfinite As Boolean, bEntIsInfinite As Boolean, ByRef rIsTangent As Boolean, bMustBeOnBoth As Boolean, Optional aCurveDivisions As Integer = 1000, Optional bCurveDivisions As Integer = 1000, Optional bNoSort As Boolean = False, Optional aCollector As colDXFVectors = Nothing) As TVECTORS
            Dim _rVal As New TVECTORS
            '#1the first entity or collection of entities
            '#2the second entity or collection of entities
            '#3flag to treat the first entity as infinitely long
            '#4flag to treat the second entity as infinitely long
            '#5returns true if the two entities are tangent to each other
            '#6flag to only return the points that lie on both entities
            '#7the number of segment divisions to divide the first entity into to find the intersection points if the first entity is a bezier curve
            '#8the number of segment divisions to divide the second entity into to find the intersection points if the second entity is a bezier curve
            '^returns the intersection points of the passed entities
            rIsTangent = False
            If aEntity Is Nothing Or bEntity Is Nothing Then Return _rVal
            If aEntity Is bEntity Then Return _rVal
            Dim bFlag As Boolean
            Dim aType As dxxGraphicTypes = aEntity.GraphicType
            Dim bType As dxxGraphicTypes = bEntity.GraphicType
            Dim aSegs As TSEGMENTS
            Dim bSegs As TSEGMENTS
            Try
                'get lines and arcs
                aSegs = dxfIntersections.IntersectionSegments(aEntity, aCurveDivisions, aEntIsInfinite)
                If aSegs.Count <= 0 Then Return _rVal
                bSegs = dxfIntersections.IntersectionSegments(bEntity, bCurveDivisions, bEntIsInfinite)
                If bSegs.Count <= 0 Then Return _rVal

                Dim nEnt As TSEGMENT
                Dim nPts As TVECTORS
                Dim iPts As New TVECTORS
                Dim v1 As TVECTOR
                'get all the intersection points
                For m As Integer = 1 To aSegs.Count
                    Dim mEnt As TSEGMENT = aSegs.Item(m)
                    For n As Integer = 1 To bSegs.Count
                        nEnt = bSegs.Item(n)
                        If mEnt.OwnerGUID <> nEnt.OwnerGUID Or mEnt.OwnerGUID = "" Or nEnt.OwnerGUID = "" Then
                            nPts = dxfIntersections.LAE_LAE(mEnt, nEnt, bFlag, True, mEnt.INFINITE, nEnt.INFINITE, aCurveDivisions, bCurveDivisions)
                            If nPts.Count > 0 Then
                                iPts.AppendUnique(nPts, 3)
                            End If
                            If aType = dxxGraphicTypes.Line And (bType = dxxGraphicTypes.Arc Or bType = dxxGraphicTypes.Ellipse) And bFlag Then rIsTangent = True
                            If bType = dxxGraphicTypes.Line And (aType = dxxGraphicTypes.Arc Or aType = dxxGraphicTypes.Ellipse) And bFlag Then rIsTangent = True
                        End If
                        'Application.DoEvents()
                    Next n
                    'Application.DoEvents()
                Next m
                If iPts.Count > 1 Then
                    If Not bNoSort Then
                        If aType = dxxGraphicTypes.Line Then
                            Dim mEnt As TSEGMENT = aSegs.Item(1)
                            v1 = iPts.Farthest(mEnt.LineStructure.SPT, mEnt.LineStructure.EPT.DirectionTo(mEnt.LineStructure.SPT))
                            iPts.SortNearestToFarthest(v1, False, False)
                        ElseIf bType = dxxGraphicTypes.Line Then
                            Dim mEnt As TSEGMENT = bSegs.Item(1)
                            v1 = iPts.Farthest(mEnt.LineStructure.SPT, mEnt.LineStructure.EPT.DirectionTo(mEnt.LineStructure.SPT))
                            iPts.SortNearestToFarthest(mEnt.LineStructure.SPT, False, False)
                        ElseIf aType = dxxGraphicTypes.Arc Or aType = dxxGraphicTypes.Ellipse Then
                            Dim mEnt As TSEGMENT = aSegs.Item(1)
                            iPts.Clockwise(mEnt.ArcStructure.Plane, 0, True)
                        ElseIf bType = dxxGraphicTypes.Arc Or bType = dxxGraphicTypes.Ellipse Then
                            Dim mEnt As TSEGMENT = bSegs.Item(1)
                            iPts.Clockwise(mEnt.ArcStructure.Plane, 0, True)
                        End If
                    End If
                End If
                _rVal = iPts
                If aCollector IsNot Nothing And iPts.Count > 0 Then
                    aCollector.Append(iPts)
                End If
            Catch ex As Exception
                Throw New Exception($"dxfIntersections.Points - { ex.Message}")
            End Try
            Return _rVal
        End Function
        Public Shared Function Entities(aEntity As dxfEntity, bEntity As dxfEntity, aEntIsInfinite As Boolean, bEntIsInfinite As Boolean, ByRef rIsTangent As Boolean, bMustBeOnBoth As Boolean, Optional aCurveDivisions As Integer = 1000, Optional bCurveDivisions As Integer = 1000) As colDXFVectors
            '#1the first entity or collection of entities
            '#2the second entity or collection of entities
            '#3flag to treat the first entity as infinitely long
            '#4flag to treat the second entity as infinitely long
            '#5returns true if the two entities are tangent to each other
            '#6flag to only return the points that lie on both entities
            '#7the number of segment divisions to divide the first entity into to find the intersection points if the first entity is a bezier curve
            '#8the number of segment divisions to divide the second entity into to find the intersection points if the second entity is a bezier curve
            '^returns the intersection points of the passed entities
            Dim _rVal As New colDXFVectors()
            If aEntity Is Nothing Or bEntity Is Nothing Then Return _rVal
            Dim vpts As TVECTORS = dxfIntersections.EntityPoints(aEntity, bEntity, aEntIsInfinite, rIsTangent, bEntIsInfinite, bMustBeOnBoth, aCurveDivisions, bCurveDivisions, False, _rVal)


            Return _rVal
        End Function

        Friend Shared Function IntersectionSegments(aEntities As IEnumerable(Of dxfEntity), aCurveDivisions As Integer, Optional bInfinite As Boolean = False) As TSEGMENTS
            Dim _rVal As New TSEGMENTS(0)
            If aEntities Is Nothing Then Return _rVal
            If aEntities IsNot Nothing Then
                For Each ent As dxfEntity In aEntities
                    If ent Is Nothing Then Continue For
                    _rVal.Append(dxfIntersections.IntersectionSegments(ent, aCurveDivisions, bInfinite))
                Next

            End If

            Return _rVal
        End Function
        Friend Shared Function IntersectionSegments(aEntity As dxfEntity, aCurveDivisions As Integer, Optional arInfinite As Boolean? = Nothing) As TSEGMENTS
            If aEntity Is Nothing Then Return New TSEGMENTS(0)
            aEntity.UpdatePath()
            Dim rRet As New TSEGMENTS(0)
            Dim aLns As New TLINES(0)
            Dim i As Integer
            Dim j As Integer
            Dim s As Integer
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim eGUID As String = aEntity.GUID
            Dim aSet As TSEGMENTS
            Dim aPaths As New TPATHS(dxxDrawingDomains.Model)
            Dim aVecs As New TVECTORS(0)
            Dim aSubEnts As colDXFEntities = Nothing
            Dim aSeg As dxfEntity

            Select Case aEntity.GraphicType
                Case dxxGraphicTypes.Arc
                    rRet.Add(dxfEntity.ArcLine(aEntity, arInfinite), eGUID)
                Case dxxGraphicTypes.Bezier
                    Dim aB As dxeBezier
                    aB = aEntity
                    aLns = aB.BezierStructure.PhantomLines(aCurveDivisions, True)
                Case dxxGraphicTypes.Dimension
                    Dim aD As dxeDimension
                    aD = aEntity
                    aSubEnts = aD.Entities
                Case dxxGraphicTypes.Ellipse
                    rRet.Add(dxfEntity.ArcLine(aEntity, arInfinite), eGUID)
                Case dxxGraphicTypes.Hatch
                    Dim aH As dxeHatch
                    aH = aEntity
                    If aH.HatchStyle <> dxxHatchStyle.dxfHatchSolidFill Then
                        aPaths = aH.Paths
                    End If
                Case dxxGraphicTypes.Hole
                    Dim aHl As dxeHole
                    aHl = aEntity
                    aPaths = aHl.Paths
                Case dxxGraphicTypes.Insert
                    Dim aI As dxeInsert
                    aI = aEntity
                    aPaths = aI.Paths
                Case dxxGraphicTypes.Leader
                    Dim aL As dxeLeader = aEntity
                    aVecs = aL.VectorsV
                    aSubEnts = New colDXFEntities
                    If aL.LeaderType = dxxLeaderTypes.LeaderText Then
                        aSubEnts.Add(aL.MText)
                    ElseIf aL.EntityType = dxxEntityTypes.LeaderBlock Then
                        aSubEnts.Add(aL.Insert)
                    End If
                Case dxxGraphicTypes.Line
                    rRet.Add(dxfEntity.ArcLine(aEntity, arInfinite), eGUID)
                Case dxxGraphicTypes.Point
                   ' Dim aP As dxePoint = aEntity
         'no intersections
                Case dxxGraphicTypes.Polygon
                    Dim aPg As dxePolygon
                    aPg = aEntity
                    rRet = aPg.PathSegments
                    If Not aPg.SuppressAdditionalSegments Then
                        For idx As Integer = 1 To aPg.AdditionalSegments.Count
                            aSeg = aPg.AdditionalSegments.Item(idx)
                            aSet = aSeg.IntersectionSegments(aCurveDivisions)
                            For j = 1 To aSet.Count
                                rRet.Add(aSet.Item(j), eGUID)
                            Next j
                        Next
                    End If
                Case dxxGraphicTypes.Polyline

                    rRet = aEntity.PathSegments
                Case dxxGraphicTypes.Solid
                    rRet = aEntity.PathSegments
                Case dxxGraphicTypes.Symbol
                    Dim aSy As dxeSymbol
                    aSy = aEntity
                    aSubEnts = aSy.Entities
                Case dxxGraphicTypes.Table
                    Dim aTB As dxeTable
                    aTB = aEntity
                    aSubEnts = aTB.Entities
                Case dxxGraphicTypes.Text
                    Dim aTx As dxeText
                    aTx = aEntity
                    aPaths = aTx.Paths
                Case dxxGraphicTypes.Shape
                    Dim aShp As dxeShape
                    aShp = aEntity
                    aPaths = aShp.Paths
            End Select
            If aSubEnts IsNot Nothing Then
                For s = 1 To aSubEnts.Count
                    aSet = aSubEnts.Item(s).IntersectionSegments(aCurveDivisions, False)
                    For i = 1 To aSet.Count
                        rRet.Add(aSet.Item(i), eGUID)
                    Next i
                Next s
            End If
            If aLns.Count > 0 Then
                For i = 1 To aLns.Count
                    rRet.Add(aLns.Item(i), eGUID)
                Next i
            End If
            If aVecs.Count > 0 Then
                For i = 1 To aVecs.Count - 1
                    v1 = aVecs.Item(i)
                    If i + 1 <= aVecs.Count Then
                        v2 = aVecs.Item(i + 1)
                        rRet.Add(v1, v2, aOwnerGUID:=eGUID)
                    Else
                        Exit For
                    End If
                Next i
            End If
            If aPaths.Count > 0 Then
                For i = 1 To aPaths.Count
                    aSet = TPATH.ToArcLines(aPaths.Item(i), aCurveDivisions)
                    For j = 1 To aSet.Count
                        rRet.Add(aSet.Item(j), eGUID)
                    Next j
                Next i
            End If
            Return rRet
        End Function
#End Region 'Methods

    End Class

End Namespace
