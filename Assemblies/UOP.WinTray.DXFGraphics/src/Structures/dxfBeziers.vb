
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics.Structures

    Friend Structure TBEZIER
        Implements ICloneable
#Region "members"
        Public CP1 As TVECTOR
        Public CP2 As TVECTOR
        Public CP3 As TVECTOR
        Public CP4 As TVECTOR
        Public Plane As TPLANE
        Public Tag As String
#End Region 'members
#Region "Constructors"

        Friend Sub New(Optional aTag As String = "")
            'init =======================================
            CP1 = TVECTOR.Zero
            CP2 = TVECTOR.Zero
            CP3 = TVECTOR.Zero
            CP4 = TVECTOR.Zero
            Plane = TPLANE.World
            Tag = aTag
            'init =======================================
        End Sub

        Friend Sub New(aCP1 As TVECTOR, aCP2 As TVECTOR, aCP3 As TVECTOR, aCP4 As TVECTOR, aPlane As TPLANE)
            'init =======================================
            CP1 = New TVECTOR(aCP1)
            CP2 = New TVECTOR(aCP2)
            CP3 = New TVECTOR(aCP3)
            CP4 = New TVECTOR(aCP4)
            Plane = New TPLANE(aPlane)
            Tag = ""
            'init =======================================
        End Sub

        Friend Sub New(aBezier As TBEZIER)
            'init =======================================
            CP1 = New TVECTOR(aBezier.CP1)
            CP2 = New TVECTOR(aBezier.CP2)
            CP3 = New TVECTOR(aBezier.CP3)
            CP4 = New TVECTOR(aBezier.CP4)
            Plane = New TPLANE(aBezier.Plane)
            Tag = aBezier.Tag
            'init =======================================

        End Sub
#End Region 'Constructors
#Region "Properties"
        Public ReadOnly Property ControlPoints As TVECTORS
            Get
                Dim _rVal As New TVECTORS(4)
                _rVal.SetItem(1, CP1)
                _rVal.SetItem(2, CP2)
                _rVal.SetItem(3, CP3)
                _rVal.SetItem(4, CP4)
                Return _rVal
            End Get
        End Property

#End Region 'Properties
#Region "Methods"
        Public Function Bounds(Optional aPlane As dxfPlane = Nothing) As TPLANE

            Dim aPl As TPLANE = Plane
            If Not dxfPlane.IsNull(aPlane) Then aPl = New TPLANE(aPlane)
            Return ControlPoints.Bounds(aPl)

        End Function

        Public Sub GetCoefficients(Optional aX As Double = 0.0, Optional bX As Double = 0.0, Optional cx As Double = 0.0, Optional dX As Double = 0.0, Optional aY As Double = 0.0, Optional bY As Double = 0.0, Optional cy As Double = 0.0, Optional dY As Double = 0.0, Optional aZ As Double = 0.0, Optional bZ As Double = 0.0, Optional cz As Double = 0.0, Optional dZ As Double = 0.0)
            '^returns the polynomial cooefficient for the bezier entity
            dxfMath.BezierCoefficients(CP1, CP2, CP3, CP4, aX, bX, cx, dX, aY, bY, cy, dY, aZ, bZ, cz, dX)
        End Sub
        Public Function ContainsVector(aVector As TVECTOR, Optional aFudgeFactor As Double = 0.001) As Boolean
            Dim rIsStartPt As Boolean = False
            Dim rIsEndPt As Boolean = False
            Return ContainsVector(aVector, aFudgeFactor, rIsStartPt, rIsEndPt)
        End Function
        Public Function ContainsVector(aVector As TVECTOR, aFudgeFactor As Double, ByRef rIsStartPt As Boolean, ByRef rIsEndPt As Boolean) As Boolean
            '^Returns True if the passed vector lies on this entity
            rIsStartPt = False
            rIsEndPt = False
            '^Returns True if the passed vector lies on this entity
            Dim sp As TVECTOR = CP1
            Dim ep As TVECTOR = CP4
            Dim d1 As Double
            Dim f1 As Double = TVALUES.LimitedValue(Math.Abs(aFudgeFactor), 0.1, 0.000001)

            'see if the vector is on my plane
            If Not Plane.ContainsVector(aVector, aFudgeFactor:=f1) Then Return False
            d1 = aVector.DistanceTo(sp)
            If d1 <= f1 Then
                rIsStartPt = True
                Return True
            End If
            d1 = aVector.DistanceTo(ep)
            If d1 <= f1 Then
                rIsEndPt = True
                Return True
            End If
            'see if the vector is outside my bounding rectangle
            Dim aRect As New dxfRectangle(Bounds)
            Dim v1 As TVECTOR = aRect.Center.Strukture
            d1 = v1.DistanceTo(aRect.TopLeftV) + 3 * f1
            If aVector.DistanceTo(v1) > d1 Then Return False
            Dim aX As Double
            Dim bX As Double
            Dim cx As Double
            Dim dX As Double
            Dim aY As Double
            Dim bY As Double
            Dim cy As Double
            Dim dY As Double
            Dim aZ As Double
            Dim bZ As Double
            Dim cz As Double
            Dim dZ As Double
            Dim rootsX As List(Of Double)
            Dim rootsY As List(Of Double)
            Dim T1 As Object
            Dim t2 As Object
            Dim dif As Object
            Dim roots As New TVALUES
            Dim bAddit As Boolean
            GetCoefficients(aX, bX, cx, dX, aY, bY, cy, dY, aZ, bZ, cz, dZ)
            rootsX = dxfMath.CubicRoots(aX, bX, cx, dX - v1.X)
            '    A = -cp1.X + 3 * cp2.X - 3 * cp3.X + 1
            '    B = 3 * cp1.X - 6 * cp2.X + 3 * cp3.X
            '    C = -3 * cp1.X + 3 * cp2.X
            '    D = cp1.X - v1.X
            '     rootsX = dxfMath.CubicRoots(A, B, C, D)
            If rootsX.Count <= 0 Then Return False
            For i As Integer = rootsX.Count To 1 Step -1
                T1 = rootsX(i - 1)
                If T1 < -0.001 Or T1 > 1.001 Or T1 = 0 Then rootsX.Remove(i)
            Next i
            If rootsX.Count <= 0 Then Return False
            rootsY = dxfMath.CubicRoots(aY, bY, cy, dY - v1.Y)
            '    A = -cp1.Y + 3 * cp2.Y - 3 * cp3.Y + 1
            '    B = 3 * cp1.Y - 6 * cp2.Y + 3 * cp3.Y
            '    C = -3 * cp1.Y + 3 * cp2.Y
            '    D = cp1.Y - v1.Y
            '     rootsY = dxfMath.CubicRoots(A, B, C, D)
            For i As Integer = rootsY.Count To 1 Step -1
                T1 = rootsY(i - 1)
                If T1 < -0.001 Or T1 > 1.001 Or T1 = 0 Then rootsY.Remove(i)
            Next i
            If rootsY.Count <= 0 Then Return False
            For i As Integer = 1 To rootsX.Count
                T1 = rootsX(i - 1)
                bAddit = False
                For j As Integer = 1 To rootsY.Count
                    t2 = rootsY(j - 1)
                    dif = 1 - (T1 / t2)
                    If dif <= 0.01 Then
                        bAddit = True
                        Exit For
                    End If
                Next j
                If bAddit Then roots.Add(T1)
            Next i
            Return roots.Count > 0
        End Function
        Public Function BezierPoint(aLengthFactor As Object) As TVECTOR
            Dim _rVal As TVECTOR = CP1.Clone
            '#1a factor from 0 to 1 to retrieve a vector along the length of the entity
            'On Error Resume Next
            Dim P1 As TVECTOR
            Dim t As Double
            Plane.Origin = CP1
            Dim aPl As TPLANE = Plane
            t = TVALUES.To_DBL(aLengthFactor)
            If t < 0 Then t = 0
            If t > 1 Then t = 1
            If t = 0 Then
                _rVal = CP1
            ElseIf t = 1 Then
                _rVal = CP4
            Else
                P1 = (CP1 * (1 - t) ^ 3)
                P1 = CP2 + CP2 * (3 * t * (1 - t) ^ 2)
                P1 += (CP3 * (3 * t ^ 2 * (1 - t)))
                P1 += (CP4 * t ^ 3)
                _rVal = aPl.Vector(P1.X, P1.Y, 0)
            End If
            Return _rVal
        End Function
        Public Function PhantomLines(Optional aDivisions As Integer = 10, Optional bIncludeEndPt As Boolean = True, Optional aPlane As dxfPlane = Nothing, Optional aCollector As colDXFEntities = Nothing) As TLINES
            Dim rLength As Double = 0.0
            If aDivisions < 4 Then aDivisions = 4
            Return PhantomLines(bIncludeEndPt, rLength, aDivisions, aPlane, False, aCollector)
        End Function
        Public Function PhantomLines(bIncludeEndPt As Boolean, ByRef rLength As Double, Optional aDivisions As Integer = 10, Optional aPlane As dxfPlane = Nothing, Optional bComputLength As Boolean = False, Optional aCollector As colDXFEntities = Nothing) As TLINES
            Dim _rVal As New TLINES(0)
            '^returns points along the arc divided int the requested number of segments
            'On Error Resume Next
            rLength = 0
            If aDivisions < 4 Then aDivisions = 4
            aDivisions = TVALUES.LimitedValue(aDivisions, 4, 10000, aDefault:=10)

            Dim aStep As Double
            Dim t As Double
            Dim v0 As TVECTOR
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim v3 As TVECTOR
            Dim RetV As TVECTORS
            Dim sp As TVECTOR
            Dim ep As TVECTOR
            Dim aPl As TPLANE = Plane
            If Not dxfPlane.IsNull(aPlane) Then aPl = New TPLANE(aPlane)
            aStep = 1 / aDivisions
            RetV = New TVECTORS
            v0 = CP1.WithRespectTo(aPl)
            v1 = CP2.WithRespectTo(aPl)
            v2 = CP3.WithRespectTo(aPl)
            v3 = CP4.WithRespectTo(aPl)
            Dim coefX As TVECTOR = TVECTOR.Zero
            Dim coefY As TVECTOR = TVECTOR.Zero
            coefX.Z = 3 * (v1.X - v0.X)
            coefX.Y = 3 * (v2.X - v1.X) - coefX.Z
            coefX.X = v3.X - v0.X - coefX.Z - coefX.Y
            coefY.Z = 3 * (v1.Y - v0.Y)
            coefY.Y = 3 * (v2.Y - v1.Y) - coefY.Z
            coefY.X = v3.Y - v0.Y - coefY.Z - coefY.Y
            sp = aPl.Vector(v0.X, v0.Y, 0)
            t = aStep
            Do While t <= 1
                ep.X = coefX.X * t ^ 3 + coefX.Y * t ^ 2 + coefX.Z * t + v0.X
                ep.Y = coefY.X * t ^ 3 + coefY.Y * t ^ 2 + coefY.Z * t + v0.Y
                ep = aPl.Vector(ep.X, ep.Y, 0)
                _rVal.Add(sp, ep)
                If bComputLength Then rLength += dxfProjections.DistanceTo(sp, ep)
                If aCollector IsNot Nothing Then
                    aCollector.AddLineV(sp, ep)
                End If
                sp = ep
                t += aStep
            Loop
            If bIncludeEndPt Then
                ep = aPl.Vector(v3.X, v3.Y, 0)
                _rVal.Add(sp, ep)
                If bComputLength Then rLength += dxfProjections.DistanceTo(sp, ep)
                If aCollector IsNot Nothing Then aCollector.AddLineV(sp, ep)
            End If
            Return _rVal
        End Function
        Public Function PhantomPoints(Optional aDivisions As Integer = 10, Optional bIncludeEndPt As Boolean = True, Optional aCollector As colDXFVectors = Nothing, Optional aPlane As dxfPlane = Nothing) As TVECTORS
            Dim _rVal As New TVECTORS(0)
            '#1the number of divisions to apply
            '#2flag to return the end point of the curve
            '#3a dxfVector collection to also add the generated points to
            '#4 and alternate plane to project the proints to
            '^returns points along the arc divided into the requested number of segments
            'On Error Resume Next
            If aDivisions < 2 Then aDivisions = 2
            If aDivisions > 10000 Then aDivisions = 10000
            Dim aStep As Double
            Dim t As Double
            Dim v0 As TVECTOR
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim v3 As TVECTOR
            Dim P1 As TVECTOR
            Dim coefX As New TVECTOR()
            Dim coefY As New TVECTOR()
            Dim aPl As TPLANE = Plane
            If Not dxfPlane.IsNull(aPlane) Then aPl = New TPLANE(aPlane)
            aStep = 1 / aDivisions
            _rVal = New TVECTORS
            v0 = CP1.WithRespectTo(aPl)
            v1 = CP2.WithRespectTo(aPl)
            v2 = CP3.WithRespectTo(aPl)
            v3 = CP4.WithRespectTo(aPl)
            coefX.Z = 3 * (v1.X - v0.X)
            coefX.Y = 3 * (v2.X - v1.X) - coefX.Z
            coefX.X = v3.X - v0.X - coefX.Z - coefX.Y
            coefY.Z = 3 * (v1.Y - v0.Y)
            coefY.Y = 3 * (v2.Y - v1.Y) - coefY.Z
            coefY.X = v3.Y - v0.Y - coefY.Z - coefY.Y
            P1 = aPl.Vector(v0.X, v0.Y, 0)
            _rVal.Add(P1)
            If aCollector IsNot Nothing Then aCollector.AddV(P1)
            t = aStep
            Do While t <= 1
                P1.X = coefX.X * t ^ 3 + coefX.Y * t ^ 2 + coefX.Z * t + v0.X
                P1.Y = coefY.X * t ^ 3 + coefY.Y * t ^ 2 + coefY.Z * t + v0.Y
                P1 = aPl.Vector(P1.X, P1.Y, 0)
                _rVal.Add(P1)
                If aCollector IsNot Nothing Then aCollector.AddV(P1)
                t += aStep
            Loop
            If bIncludeEndPt Then
                P1 = aPl.Vector(v3.X, v3.Y, 0)
                _rVal.Add(P1)
                If aCollector IsNot Nothing Then aCollector.AddV(P1)
            End If
            Return _rVal
        End Function

        Public Function Clone() As TBEZIER
            Return New TBEZIER(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TBEZIER(Me)
        End Function
#End Region 'Methods
#Region "Shared Methods"
        Public Shared Function ArcPathP(aArc As TARC) As TPOINTS
            Dim _rVal As New TPOINTS(0)
            If aArc.Radius <= 0 Then Return _rVal
            Dim p0 As TVECTOR
            Dim P1 As TVECTOR
            Dim P2 As TVECTOR
            Dim p3 As TVECTOR
            Dim ep As TVECTOR
            Dim aPlane As TPLANE = aArc.Plane
            Dim cp As TVECTOR = aPlane.Origin
            Dim xDir As TVECTOR = aPlane.XDirection.Clone
            Dim aN As TVECTOR = aPlane.ZDirection
            Dim sa As Double = aArc.StartAngle
            Dim ea As Double = aArc.EndAngle
            Dim rad As Double = aArc.Radius
            Dim qd As Integer
            Dim quads As Integer
            Dim Remainder As Double
            Dim psi As Double
            Dim theta As Double
            Dim Span As Double = aArc.SpannedAngle
            Dim ang1 As Double
            Dim ang2 As Double
            Dim l1 As Double
            Dim f1 As Double
            Dim rLen As Double
            Dim bRemains As Boolean
            _rVal = New TPOINTS(0)
            If Not aArc.ClockWise Then f1 = 1 Else f1 = -1
            If Span = 0 Then Return _rVal
            Remainder = Span
            Do While Remainder >= 90
                quads += 1
                Remainder -= 90
            Loop
            If quads > 4 Then
                quads = 4
                Remainder = 0
            End If
            aPlane.Origin = cp
            rLen = Math.Round(((Remainder * Math.PI) / 180) * rad, 3)
            bRemains = rLen > 0.005
            l1 = Math.Sqrt((dxfGlobals.kappa * rad) ^ 2 + rad ^ 2)
            ang1 = dxfMath.ArcSine((dxfGlobals.kappa * rad) / l1, True)
            ang2 = 90 - 2 * ang1
            p0 = aPlane.PolarVector(rad, sa)
            ep = aPlane.PolarVector(rad, ea)
            If quads > 0 Then
                P1 = aPlane.PolarVector(l1, sa + f1 * ang1)
                P2 = aPlane.PolarVector(l1, sa + f1 * (ang1 + ang2))
                p3 = aPlane.PolarVector(rad, sa + 90 * f1)
                For qd = 1 To quads
                    If qd > 1 Then
                        p0.RotateAbout(cp, aN, f1 * 90, False)
                        P1.RotateAbout(cp, aN, f1 * 90, False)
                        P2.RotateAbout(cp, aN, f1 * 90, False)
                        p3.RotateAbout(cp, aN, f1 * 90, False)
                    End If
                    _rVal.Add(p0, dxxVertexStyles.BEZIERTO)
                    _rVal.Add(P1, dxxVertexStyles.BEZIERTO)
                    _rVal.Add(P2, dxxVertexStyles.BEZIERTO)
                    sa = TVALUES.NormAng(sa + 90 * f1)
                Next qd
                If Not bRemains Then p3 = ep
                _rVal.Add(p3, dxxVertexStyles.BEZIERTO)
            Else
                _rVal.Add(p0, dxxVertexStyles.BEZIERTO)
                p3 = p0
            End If
            If bRemains Then
                psi = Remainder / 2
                theta = psi * Math.PI / 180
                p0.X = Math.Cos(theta)
                p0.Y = -Math.Sin(theta)
                p0.Z = 0
                P1 = New TVECTOR((4 - p0.X) / 3, ((1 - p0.X) * (3 - p0.X)) / (3 * p0.Y), 0) * rad
                p0 *= rad
                p3 = p0
                p3.Y = -p0.Y
                P2 = P1
                P2.Y = -P1.Y
                If P2.X = 0 Or P2.Y = 0 Then
                    P1 = p0.Interpolate(ep, 1 / 3)
                    P2 = p0.Interpolate(ep, 2 / 3)
                    _rVal.Add(P1, dxxVertexStyles.BEZIERTO)
                    _rVal.Add(P2, dxxVertexStyles.BEZIERTO)
                Else
                    l1 = P2.Magnitude
                    ang2 = Math.Atan(P2.Y / P2.X) * 180 / Math.PI
                    ang1 = psi - ang2
                    p0 = aPlane.PolarVector(rad, sa)
                    P1 = aPlane.PolarVector(l1, sa + f1 * ang1)
                    P2 = aPlane.PolarVector(l1, sa + f1 * (ang1 + (2 * ang2)))
                    _rVal.Add(P1, dxxVertexStyles.BEZIERTO)
                    _rVal.Add(P2, dxxVertexStyles.BEZIERTO)
                    _rVal.Add(ep, dxxVertexStyles.BEZIERTO)
                End If
            End If
            If _rVal.Count > 0 Then
                _rVal.SetCode(1, TVALUES.ToByte(dxxVertexStyles.MOVETO))
            End If
            Return _rVal
        End Function
        Public Shared Function ArcPath(aArc As TARC, bFullCircle As Boolean, Optional bNoStart As Boolean = False, Optional bJustMoveTo As Boolean = False) As TVECTORS
            Dim _rVal As New TVECTORS(0)
            If aArc.Radius <= 0 Then Return _rVal
            Dim cp As TVECTOR = aArc.Plane.Origin
            Dim p0 As TVECTOR
            Dim P1 As TVECTOR
            Dim P2 As TVECTOR
            Dim p3 As TVECTOR
            Dim aN As TVECTOR
            Dim ep As TVECTOR
            Dim aPlane As TPLANE = aArc.Plane
            Dim xDir As TVECTOR
            Dim sa As Double = aArc.StartAngle
            Dim ea As Double = aArc.EndAngle
            Dim rad As Double = aArc.Radius
            Dim qd As Integer
            Dim quads As Integer
            Dim Remainder As Double
            Dim psi As Double
            Dim theta As Double
            Dim Span As Double
            Dim ang1 As Double
            Dim ang2 As Double
            Dim l1 As Double
            Dim f1 As Double = 1
            Dim rLen As Double
            Dim bRemains As Boolean
            Dim vType As dxxVertexStyles = dxxVertexStyles.BEZIERTO
            If aArc.ClockWise Then f1 = -1
            If Not bFullCircle Then
                Span = aArc.SpannedAngle 'SpannedAngle(aArc.ClockWise, sa, ea)
                'aArc.SpannedAngle = Span
            Else
                Span = 360
                sa = 0
                ea = 360
            End If
            If Span = 0 Then Return _rVal
            Remainder = Span
            Do While Remainder >= 90
                quads += 1
                Remainder -= 90
            Loop
            If quads > 4 Then
                quads = 4
                Remainder = 0
            End If
            aPlane = aPlane.MovedTo(cp)
            aN = aPlane.ZDirection
            xDir = aPlane.XDirection
            rLen = Math.Round(((Remainder * Math.PI) / 180) * rad, 3)
            bRemains = rLen > 0.005
            l1 = Math.Sqrt((dxfGlobals.kappa * rad) ^ 2 + rad ^ 2)
            ang1 = dxfMath.ArcSine((dxfGlobals.kappa * rad) / l1, bReturnDegrees:=True)
            ang2 = 90 - 2 * ang1
            p0 = aPlane.PolarVector(rad, sa)
            ep = aPlane.PolarVector(rad, ea)
            _rVal = New TVECTORS
            If quads > 0 Then
                P1 = aPlane.PolarVector(l1, sa + f1 * ang1)
                P2 = aPlane.PolarVector(l1, sa + f1 * (ang1 + ang2))
                p3 = aPlane.PolarVector(rad, sa + 90 * f1)
                For qd = 1 To quads
                    If qd > 1 Then
                        p0.RotateAbout(cp, aN, f1 * 90, False)
                        P1.RotateAbout(cp, aN, f1 * 90, False)
                        P2.RotateAbout(cp, aN, f1 * 90, False)
                        p3.RotateAbout(cp, aN, f1 * 90, False)
                    End If
                    If qd > 1 Or (qd = 1 And Not bNoStart) Then
                        _rVal.Add(p0, TVALUES.ToByte(vType))
                    End If
                    _rVal.Add(P1, TVALUES.ToByte(vType))
                    _rVal.Add(P2, TVALUES.ToByte(vType))
                    sa = TVALUES.NormAng(sa + 90 * f1)
                Next qd
                If Not bRemains Then p3 = ep
                _rVal.Add(p3, TVALUES.ToByte(vType))
            Else
                If Not bNoStart Then _rVal.Add(p0, TVALUES.ToByte(vType))
                p3 = p0
            End If
            If bRemains Then
                psi = Remainder / 2
                theta = psi * Math.PI / 180
                p0.X = Math.Cos(theta)
                p0.Y = -Math.Sin(theta)
                p0.Z = 0
                P1 = (New TVECTOR((4 - p0.X) / 3, ((1 - p0.X) * (3 - p0.X)) / (3 * p0.Y), 0) * rad)
                p0 *= rad
                p3 = p0
                p3.Y = -p0.Y
                P2 = P1
                P2.Y = -P1.Y
                If P2.X = 0 Or P2.Y = 0 Then
                    P1 = p0.Interpolate(ep, 1 / 3)
                    P2 = p0.Interpolate(ep, 2 / 3)
                    _rVal.Add(P1, TVALUES.ToByte(vType))
                    _rVal.Add(P2, TVALUES.ToByte(vType))
                    _rVal.Add(ep, TVALUES.ToByte(vType))
                Else
                    l1 = P2.Magnitude
                    ang2 = Math.Atan(P2.Y / P2.X) * 180 / Math.PI
                    ang1 = psi - ang2
                    p0 = aPlane.PolarVector(rad, sa)
                    P1 = aPlane.PolarVector(l1, sa + f1 * ang1)
                    P2 = aPlane.PolarVector(l1, sa + f1 * (ang1 + (2 * ang2)))
                    _rVal.Add(P1, TVALUES.ToByte(vType))
                    _rVal.Add(P2, TVALUES.ToByte(vType))
                    _rVal.Add(ep, TVALUES.ToByte(vType))
                End If
            End If
            If _rVal.Count > 0 And Not bNoStart Then
                _rVal.SetCode(1, TVALUES.ToByte(dxxVertexStyles.MOVETO))
            End If
            Return _rVal
        End Function

        Friend Shared Function ArcPathSimple(aArc As TARC, Optional bJustMoveTo As Boolean = False) As TPOINTS
            Dim _rVal As New TPOINTS(0)
            If aArc.Radius <= 0 Then Return _rVal
            Dim P0 As TVECTOR
            Dim P1 As TVECTOR
            Dim P2 As TVECTOR
            Dim p3 As TVECTOR
            Dim aN As TVECTOR
            Dim ep As TVECTOR
            Dim aPlane As TPLANE = aArc.Plane
            Dim xDir As TVECTOR
            Dim cp As TVECTOR = aPlane.Origin
            Dim sa As Double = aArc.StartAngle
            Dim ea As Double = aArc.EndAngle
            Dim rad As Double = aArc.Radius
            Dim qd As Integer
            Dim quads As Integer
            Dim Remainder As Double
            Dim psi As Double
            Dim theta As Double
            Dim Span As Double = aArc.SpannedAngle
            Dim ang1 As Double
            Dim ang2 As Double
            Dim l1 As Double
            Dim f1 As Double
            Dim rLen As Double
            Dim bRemains As Boolean
            Dim vtype As dxxVertexStyles = dxxVertexStyles.BEZIERTO
            If bJustMoveTo Then vtype = dxxVertexStyles.MOVETO

            If Not aArc.ClockWise Then f1 = 1 Else f1 = -1
            If Span = 0 Then Return _rVal
            Remainder = Span
            Do While Remainder >= 90
                quads += 1
                Remainder -= 90
            Loop
            If quads > 4 Then
                quads = 4
                Remainder = 0
            End If
            aPlane.Origin = cp
            aN = aPlane.ZDirection
            xDir = aPlane.XDirection
            rLen = Math.Round(((Remainder * Math.PI) / 180) * rad, 3)
            bRemains = rLen > 0.005
            l1 = Math.Sqrt((dxfGlobals.kappa * rad) ^ 2 + rad ^ 2)
            ang1 = dxfMath.ArcSine((dxfGlobals.kappa * rad) / l1, True)
            ang2 = 90 - 2 * ang1
            P0 = aPlane.PolarVector(rad, sa)
            ep = aPlane.PolarVector(rad, ea)
            If quads > 0 Then
                P1 = aPlane.PolarVector(l1, sa + f1 * ang1)
                P2 = aPlane.PolarVector(l1, sa + f1 * (ang1 + ang2))
                p3 = aPlane.PolarVector(rad, sa + 90 * f1)
                For qd = 1 To quads
                    If qd > 1 Then
                        P0.RotateAbout(cp, aN, f1 * 90, False)
                        P1.RotateAbout(cp, aN, f1 * 90, False)
                        P2.RotateAbout(cp, aN, f1 * 90, False)
                        p3.RotateAbout(cp, aN, f1 * 90, False)
                    End If
                    _rVal.Add(P0, vtype)
                    _rVal.Add(P1, vtype)
                    _rVal.Add(P2, vtype)
                    sa = TVALUES.NormAng(sa + 90 * f1)
                Next qd
                If Not bRemains Then p3 = ep
                _rVal.Add(p3, vtype)
            Else
                _rVal.Add(P0, vtype)
                p3 = P0
            End If
            If bRemains Then
                psi = Remainder / 2
                theta = psi * Math.PI / 180
                P0.X = Math.Cos(theta)
                P0.Y = -Math.Sin(theta)
                P0.Z = 0
                P1 = (New TVECTOR((4 - P0.X) / 3, ((1 - P0.X) * (3 - P0.X)) / (3 * P0.Y), 0) * rad)
                P0 *= rad
                p3 = P0
                p3.Y = -P0.Y
                P2 = P1
                P2.Y = -P1.Y
                If P2.X = 0 Or P2.Y = 0 Then
                    P1 = P0.Interpolate(ep, 1 / 3)
                    P2 = P0.Interpolate(ep, 2 / 3)
                    _rVal.Add(P1, vtype)
                    _rVal.Add(P2, vtype)
                Else
                    l1 = P2.Magnitude
                    ang2 = Math.Atan(P2.Y / P2.X) * 180 / Math.PI
                    ang1 = psi - ang2
                    P0 = aPlane.PolarVector(rad, sa)
                    P1 = aPlane.PolarVector(l1, sa + f1 * ang1)
                    P2 = aPlane.PolarVector(l1, sa + f1 * (ang1 + (2 * ang2)))
                    _rVal.Add(P1, vtype)
                    _rVal.Add(P2, vtype)
                    _rVal.Add(ep, vtype)
                End If
            End If
            If _rVal.Count > 0 Then
                _rVal.SetCode(1, TVALUES.ToByte(dxxVertexStyles.MOVETO))
            End If
            Return _rVal
        End Function
        Friend Shared Function EllipsePath(aArc As TARC, Optional bFullPath As Boolean = False) As TVECTORS
            Dim plVecs As New TVECTORS
            If aArc.Radius <= 0 Or aArc.MinorRadius <= 0 Then Return plVecs
            Dim aPlane As TPLANE = aArc.Plane
            Dim wPl As New TPLANE("")
            Dim cp As TVECTOR = aPlane.Origin.Clone
            Dim p0 As TVECTOR
            Dim P1 As TVECTOR
            Dim P2 As TVECTOR
            Dim p3 As TVECTOR
            Dim Span As Double = aArc.SpannedAngle
            Dim dX As Double = aArc.Radius
            Dim dY As Double = aArc.MinorRadius
            Dim majD As Double
            Dim minD As Double
            Dim Remainder As Double
            Dim steps As Integer
            Dim ang As Double
            Dim i As Integer
            Dim x1 As Double
            Dim Y1 As Double
            Dim x2 As Double
            Dim Y2 As Double
            Dim sa As Double = aArc.StartAngle
            Dim ea As Double = aArc.EndAngle
            Dim stepp As Double
            If Span > 359.98 Then bFullPath = True
            If bFullPath Then
                Span = 360
                sa = 0
                ea = 360
            End If
            If Span = 0 Then Return plVecs
            majD = 2 * dX
            minD = 2 * dY
            If bFullPath Then
                'first quad
                plVecs.Add(wPl.Vector(dX), TVALUES.ToByte(dxxVertexStyles.MOVETO))
                plVecs.Add(wPl.Vector(dX, dY * dxfGlobals.kappa), TVALUES.ToByte(dxxVertexStyles.BEZIERTO))
                plVecs.Add(wPl.Vector(dX * dxfGlobals.kappa, dY), TVALUES.ToByte(dxxVertexStyles.BEZIERTO))
                plVecs.Add(wPl.Vector(0, dY), TVALUES.ToByte(dxxVertexStyles.BEZIERTO))
                'second quad
                plVecs.Add(wPl.Vector(-(dX * dxfGlobals.kappa), dY), TVALUES.ToByte(dxxVertexStyles.BEZIERTO))
                plVecs.Add(wPl.Vector(-dX, dY * dxfGlobals.kappa), TVALUES.ToByte(dxxVertexStyles.BEZIERTO))
                plVecs.Add(wPl.Vector(-dX, 0), TVALUES.ToByte(dxxVertexStyles.BEZIERTO))
                'third quad
                plVecs.Add(wPl.Vector(-dX, -dY * dxfGlobals.kappa), TVALUES.ToByte(dxxVertexStyles.BEZIERTO))
                plVecs.Add(wPl.Vector(-(dX * dxfGlobals.kappa), -dY), TVALUES.ToByte(dxxVertexStyles.BEZIERTO))
                plVecs.Add(wPl.Vector(0, -dY), TVALUES.ToByte(dxxVertexStyles.BEZIERTO))
                'forth quad
                plVecs.Add(wPl.Vector(dX * dxfGlobals.kappa, -dY), TVALUES.ToByte(dxxVertexStyles.BEZIERTO))
                plVecs.Add(wPl.Vector(dX, -dY * dxfGlobals.kappa), TVALUES.ToByte(dxxVertexStyles.BEZIERTO))
                plVecs.Add(wPl.Vector(dX, 0), TVALUES.ToByte(dxxVertexStyles.BEZIERTO))
            Else
                stepp = 8 / (majD / minD)
                steps = TVALUES.To_INT(Span / stepp)
                ang = sa
                Remainder = Span
                cp = wPl.Origin
                For i = 1 To steps
                    p0 = dxfUtils.EllipsePoint(cp, majD, minD, ang, wPl)
                    P1 = dxfUtils.EllipsePoint(cp, majD, minD, ang + (stepp * 0.25), wPl)
                    P2 = dxfUtils.EllipsePoint(cp, majD, minD, ang + (stepp * 0.75), wPl)
                    p3 = dxfUtils.EllipsePoint(cp, majD, minD, ang + stepp, wPl)
                    x2 = (192 * P2.X + 24 * p0.X - 80 * p3.X - 64 * P1.X) / 72
                    x1 = (64 * P1.X - 27 * p0.X - 9 * x2 - p3.X) / 27
                    Y2 = (192 * P2.Y + 24 * P1.Y - 80 * p3.Y - 64 * P1.Y) / 72
                    Y1 = (64 * P1.Y - 27 * p0.Y - 9 * Y2 - p3.Y) / 27
                    If plVecs.Count <= 0 Then
                        plVecs.Add(wPl.Vector(p0.X, p0.Y), TVALUES.ToByte(dxxVertexStyles.MOVETO))
                        plVecs.Add(wPl.Vector(x1, Y1), TVALUES.ToByte(dxxVertexStyles.BEZIERTO))
                        plVecs.Add(wPl.Vector(x2, Y2), TVALUES.ToByte(dxxVertexStyles.BEZIERTO))
                        plVecs.Add(wPl.Vector(p3.X, p3.Y), TVALUES.ToByte(dxxVertexStyles.BEZIERTO))
                    Else
                        plVecs.Add(wPl.Vector(x1, Y1), TVALUES.ToByte(dxxVertexStyles.BEZIERTO))
                        plVecs.Add(wPl.Vector(x2, Y2), TVALUES.ToByte(dxxVertexStyles.BEZIERTO))
                        plVecs.Add(wPl.Vector(p3.X, p3.Y), TVALUES.ToByte(dxxVertexStyles.BEZIERTO))
                    End If
                    Remainder -= stepp
                    If i < steps Then ang += stepp
                    'GoTo Done1:
                Next i
                If Remainder > 0.01 Then
                    p0 = dxfUtils.EllipsePoint(cp, majD, minD, ang, wPl)
                    P1 = dxfUtils.EllipsePoint(cp, majD, minD, ang + (Remainder * 0.25), wPl)
                    P2 = dxfUtils.EllipsePoint(cp, majD, minD, ang + (Remainder * 0.75), wPl)
                    p3 = dxfUtils.EllipsePoint(cp, majD, minD, ang + Remainder, wPl)
                    x2 = (192 * P2.X + 24 * p0.X - 80 * p3.X - 64 * P1.X) / 72
                    x1 = (64 * P1.X - 27 * p0.X - 9 * x2 - p3.X) / 27
                    Y2 = (192 * P2.Y + 24 * P1.Y - 80 * p3.Y - 64 * P1.Y) / 72
                    Y1 = (64 * P1.Y - 27 * p0.Y - 9 * Y2 - p3.Y) / 27
                    If plVecs.Count <= 0 Then
                        plVecs.Add(wPl.Vector(p0.X, p0.Y), TVALUES.ToByte(dxxVertexStyles.MOVETO))
                        plVecs.Add(wPl.Vector(x1, Y1), TVALUES.ToByte(dxxVertexStyles.BEZIERTO))
                        plVecs.Add(wPl.Vector(x2, Y2), TVALUES.ToByte(dxxVertexStyles.BEZIERTO))
                        plVecs.Add(wPl.Vector(p3.X, p3.Y), TVALUES.ToByte(dxxVertexStyles.BEZIERTO))
                    Else
                        plVecs.Add(wPl.Vector(x1, Y1), TVALUES.ToByte(dxxVertexStyles.BEZIERTO))
                        plVecs.Add(wPl.Vector(x2, Y2), TVALUES.ToByte(dxxVertexStyles.BEZIERTO))
                        plVecs.Add(wPl.Vector(p3.X, p3.Y), TVALUES.ToByte(dxxVertexStyles.BEZIERTO))
                    End If
                End If
            End If
            'convertt world points
            For i = 1 To plVecs.Count
                plVecs.SetItem(i, aPlane.WorldVector(plVecs.Item(i)))
            Next i
            Return plVecs
        End Function


#End Region 'Shared Methods
    End Structure

End Namespace
