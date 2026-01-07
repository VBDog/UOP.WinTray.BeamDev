

Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics.Utilities
    Public Class dxfMath
#Region "Shared Methods"
        Public Shared Function Determinant2By2(a As Double, b As Double, ByRef C As Double, D As Double) As Double
            Return a * D - b * C
        End Function
        Public Shared Function Determinant3by3(a As Double, b As Double, ByRef C As Double, D As Double, ByRef e As Double, ByRef F As Double, ByRef G As Double, ByRef H As Double, ByRef i As Double) As Double
            Return a * dxfMath.Determinant2By2(e, F, H, i) - b * dxfMath.Determinant2By2(D, F, G, i) + C * dxfMath.Determinant2By2(D, e, G, H)
        End Function
        Public Shared Function Determinant4By4(a As Double, b As Double, ByRef C As Double, D As Double, ByRef e As Double, ByRef F As Double, ByRef G As Double, ByRef H As Double, ByRef i As Double, ByRef j As Double, ByRef k As Double, ByRef l As Double, ByRef m As Double, ByRef n As Double, ByRef O As Double, ByRef P As Double) As Double
            Dim val1 As Double = dxfMath.Determinant3by3(F, G, H, j, k, l, n, O, P)
            Dim val2 As Double = dxfMath.Determinant3by3(e, G, H, i, k, l, m, O, P)
            Dim val3 As Double = dxfMath.Determinant3by3(e, F, H, i, j, l, m, n, P)
            Dim val4 As Double = dxfMath.Determinant3by3(e, F, G, i, j, k, m, n, O)
            val1 = a * val1
            val2 = b * val2
            val3 = C * val3
            val4 = D * val4
            Dim R As Double = val1 - val2 + val3 - val4
            Return R
        End Function
        Public Shared Function ArcSine(aVal As Double, Optional bReturnDegrees As Boolean = False) As Double
            Dim _rVal As Double = aVal
            '#1the angle in radians to get the ArcSine for
            '#2flag to return value in degrees
            '^used to calculate the ArcSine of an angle expressed in radians
            If Math.Abs(_rVal) = 1 Then
                If _rVal = 1 Then _rVal = Math.PI / 2 Else _rVal = -Math.PI / 2
            Else
                Dim dVal As Double = _rVal
                Dim bVal As Double = -dVal * dVal + 1
                If bVal <> 0 Then
                    _rVal = Math.Atan(dVal / Math.Sqrt(Math.Abs(bVal)))
                End If
            End If
            If bReturnDegrees Then _rVal *= 180 / Math.PI
            Return _rVal
        End Function
        Public Shared Function ArcTan2(Y As Double, X As Double, Optional bReturnDegrees As Boolean = False) As Double
            Dim _rVal As Double
            Dim signy As Integer
            signy = Math.Sign(Y)
            If signy = 0 Then signy = 1 ' removes the problem when Y=0
            If Math.Abs(X) < 0.00001 Then
                ' (direct comparison with zero doesn't always work)
                _rVal = Math.Sign(Y) * (Math.PI / 2)
            ElseIf X < 0 Then
                _rVal = Math.Atan(Y / X) + signy * Math.PI
            Else
                _rVal = Math.Atan(Y / X)
            End If
            If bReturnDegrees Then _rVal *= 180 / Math.PI
            Return _rVal
        End Function
        Public Shared Function ArcCosine(aVal As Double, Optional bReturnDegrees As Boolean = False) As Double
            Dim _rVal As Double = aVal
            '#1the angle in radians to get the ArcCosine for
            '#2flag to return the value in degrees
            '^used to calculate the ArcCosine of an angle expressed in radians
            If Math.Abs(_rVal) = 1 Then
                If _rVal = 1 Then _rVal = Math.PI / 2 Else _rVal = -Math.PI / 2
            Else
                Dim dVal As Double = _rVal
                Dim bVal As Double = -dVal * dVal + 1
                If Math.Round(bVal, 6) <> 0 Then
                    _rVal = Math.Atan(-dVal / Math.Sqrt(Math.Abs(bVal))) + 2 * Math.Atan(1)
                End If
            End If
            If bReturnDegrees Then _rVal *= 180 / Math.PI
            Return _rVal
        End Function
        Friend Shared Sub BezierCoefficients(b0 As TVECTOR, b1 As TVECTOR, b2 As TVECTOR, b3 As TVECTOR, Optional aX As Double = 0.0, Optional bX As Double = 0.0, Optional cx As Double = 0.0, Optional dX As Double = 0.0, Optional aY As Double = 0.0, Optional bY As Double = 0.0, Optional cy As Double = 0.0, Optional dY As Double = 0.0, Optional aZ As Double = 0.0, Optional bZ As Double = 0.0, Optional cz As Double = 0.0, Optional dZ As Double = 0.0)
            '^returns the polynomial cooefficient for the bezier entity
            cx = 3 * (b1.X - b0.X)
            bX = 3 * (b2.X - b1.X) - cx
            aX = b3.X - b0.X - cx - bX
            dX = b0.X
            cy = 3 * (b1.Y - b0.Y)
            bY = 3 * (b2.Y - b1.Y) - cy
            aY = b3.Y - b0.Y - cy - bY
            dY = b0.Y
            cz = 3 * (b1.Z - b0.Z)
            bZ = 3 * (b2.Z - b1.Z) - cz
            aZ = b3.Z - b0.Z - cz - bZ
            dZ = b0.Z
        End Sub
        Public Shared Function CubicRoots(A As Object, B As Object, C As Object, D As Object) As List(Of Double)
            Dim _rVal As New List(Of Double)
            '#1the first cooefficient
            '#2the second cooefficient
            '#3the third cooefficient
            '#4the forth cooefficient
            '^returns the roots of the cubic polynomial Ax3 + Bx2 + Cx + D = 0

            Dim Ain As Double = TVALUES.ToDouble(A)
            Dim Bin As Double = TVALUES.ToDouble(B)
            Dim Cin As Double = TVALUES.ToDouble(C)
            Dim Din As Double = TVALUES.ToDouble(D)
            Dim a1 As Double
            Dim a2 As Double
            Dim a3 As Double


            Dim x1 As Double
            Dim x2 As Double
            Dim x3 As Double
            Dim sqrtQ As Double
            Dim theta As Double
            Dim e As Double

            If Ain <> 0 Then
                a1 = TVALUES.To_DBL(Bin / Ain)
                a2 = TVALUES.To_DBL(Cin / Ain)
                a3 = TVALUES.To_DBL(Din / Ain)
            End If
            Dim q As Double = TVALUES.To_DBL((a1 * a1 - 3 * a2) / 9)
            Dim R As Double = TVALUES.To_DBL((2 * a1 * a1 * a1 - 9 * a1 * a2 + 27 * a3) / 54)
            Dim Qcubed As Double = TVALUES.To_DBL(q * q * q)
            Dim d1 As Double = TVALUES.To_DBL(Qcubed - R * R)
            '/* Three real roots */
            If (d1 >= 0) Then
                If Qcubed > 0 Then theta = dxfMath.ArcCosine(R / Math.Sqrt(Qcubed))
                sqrtQ = Math.Sqrt(q)
                x1 = TVALUES.To_DBL(-2 * sqrtQ * Math.Cos(theta / 3) - a1 / 3)
                x2 = TVALUES.To_DBL(-2 * sqrtQ * Math.Cos((theta + 2 * Math.PI) / 3) - a1 / 3)
                x3 = TVALUES.To_DBL(-2 * sqrtQ * Math.Cos((theta + 4 * Math.PI) / 3) - a1 / 3)
                _rVal.Add(x1)
                If x2 <> x1 Then _rVal.Add(x2)
                If x3 <> x2 And x3 <> x1 Then _rVal.Add(x3)
            Else
                '/* One real root */
                e = Math.Pow(Math.Sqrt(-d1) + Math.Abs(R), 1.0# / 3.0#)
                If (R > 0) Then e = -e
                x1 = (e + q / e) - (a1 / 3.0#)
                _rVal.Add(x1)
            End If
            Return _rVal
        End Function
        Public Shared Function Deg2Rad(aAngleDegrees As Double) As Double
            Return aAngleDegrees * Math.PI / 180
        End Function
        Public Shared Function Rad2Deg(aAngleRadians As Double) As Double
            Return aAngleRadians * 180 / Math.PI
        End Function
        Public Shared Function HoleArea(aRadius As Double, aMinorRadius As Double, aLength As Double, bIsSquare As Boolean) As Double
            Dim _rVal As Double
            '^the area of the hole
            Dim rad As Double = aRadius
            Dim mr As Double = Math.Abs(aMinorRadius)
            If mr >= rad - 0.0001 Then mr = 0
            Dim dia As Double = 2 * rad
            Dim lng As Double = aLength
            Dim ang As Double
            Dim are As Double
            Dim x1 As Double

            If lng <> dia Then mr = 0
            If bIsSquare Then
                _rVal = lng * dia
            Else
                If mr = 0 Then
                    _rVal = Math.PI * rad ^ 2
                    If lng > dia Then
                        _rVal += (lng - dia) * dia
                    End If
                Else
                    _rVal = Math.PI * rad ^ 2
                    x1 = Math.Sqrt(rad ^ 2 - mr ^ 2)
                    ang = dxfMath.ArcCosine(x1 / rad) * Math.PI
                    are = ((2 * rad) ^ 2) / 8
                    _rVal -= (are * (ang - Math.Sin(ang)))
                End If
            End If
            Return _rVal
        End Function
        Public Shared Function PolylineArea(aPolyline As dxfPolyline) As Double
            Dim aEnt As dxfEntity = aPolyline
            aPolyline.UpdatePath()
            Dim _rVal As Double
            Dim aArc As TARC
            Dim aRec As TPLANE

            Dim aPln As TPLANE = aEnt.PlaneV
            Dim aArcLn As TSEGMENT
            Dim bArcLn As TSEGMENT
            Dim aSegs As TSEGMENTS = aEnt.Components.Segments

            Dim sArcs As TSEGMENTS
            Dim v1 As TVECTOR
            Dim aLn As New TLINE
            Dim aDir As TVECTOR = TVECTOR.Zero
            Dim bDir As TVECTOR = TVECTOR.Zero

            Dim j As Integer
            Dim d1 As Double
            Dim d2 As Double


            If aSegs.Count > 0 Then
                Dim ePts As New TVECTORS
                Dim mPts As New TVECTORS
                Dim aArcs As TSEGMENTS = aSegs.Clone()
                aArcs.Clear()
                For i As Integer = 1 To aSegs.Count
                    aArcLn = aSegs.Item(i)
                    If aArcLn.IsArc Then
                        'als_UpdateArcPoints(aArcLn)
                        aArc = aArcLn.ArcStructure
                        sArcs = New TSEGMENTS(0)
                        If Math.Round(aArc.SpannedAngle, 2) <= 90 Then
                            sArcs.Add(aArcLn)
                        Else
                            sArcs = dxfUtils.ArcDivide(aArc, 90, True)
                        End If
                        For j = 1 To sArcs.Count
                            bArcLn = sArcs.Item(j)
                            'If sArcs.Count > 1 Then als_UpdateArcPoints(bArcLn)
                            aArc = bArcLn.ArcStructure
                            If i = 1 Then ePts.Add(aArc.StartPt)
                            v1 = aArc.MidPoint
                            ePts.Add(v1)
                            If j < sArcs.Count Or i < aSegs.Count Then
                                ePts.Add(aArc.EndPt)
                            Else
                                If ePts.Item(1).DistanceTo(aArc.EndPt, 3) > 0 Then
                                    ePts.Add(aArc.EndPt)
                                End If
                            End If
                            mPts.Add(v1)
                            aArcs.Add(bArcLn)
                        Next j
                    Else
                        If i = 1 Then ePts.Add(aArcLn.LineStructure.SPT)
                        If i < aSegs.Count Then
                            ePts.Add(aArcLn.LineStructure.EPT)
                        Else
                            If ePts.Item(1).DistanceTo(aArcLn.LineStructure.EPT, 3) > 0 Then
                                ePts.Add(aArcLn.LineStructure.EPT)
                            End If
                        End If
                    End If
                Next i
                aRec = ePts.Bounds(aPln)
                _rVal = ePts.AreaSummation(aRec, True)
                If aArcs.Count > 0 Then
                    For i = 1 To aArcs.Count
                        aArc = aArcs.Arc(i)
                        aLn.SPT = aArc.StartPt
                        aLn.EPT = aArc.EndPt
                        dxfProjections.ToLine(aRec.Origin, aLn, rOrthoDirection:=aDir, rDistance:=d1)

                        dxfProjections.ToLine(mPts.Item(i), aLn, rOrthoDirection:=bDir, rDistance:=d2)

                        If aDir.Equals(bDir, False, 3) Then
                            _rVal -= (2 * dxfUtils.ArcArea(aArc.Radius, aArc.SpannedAngle / 2, False))
                        Else
                            _rVal += (2 * dxfUtils.ArcArea(aArc.Radius, aArc.SpannedAngle / 2, False))
                        End If
                    Next i
                End If
            End If
            Return _rVal
        End Function

        Public Shared Function RoundTo(aNum As Object, aNearest As dxxRoundToLimits, Optional bRoundUp As Boolean = False, Optional bRoundDown As Boolean = False) As Double
            Dim _rVal As Double = TVALUES.To_DBL(aNum)
            If aNearest = dxxRoundToLimits.Undefined Then Return _rVal
            '#1the number to round
            '#2the limit to round to
            '#3flag to indicate that the value should only be rounded up
            '#4flag to indicate that the value should only be rounded down
            '^used to round a numeric value to the indicated limit.
            '~limits are equated to enums for convenience and clarity and are transformed to numeric values.
            '~i.e. Eighth = 1 means round to the nearest 0.125
            '~if Millimeter or Centimeter is passed then the passed number is
            '~assumed to be in inches and is returned in inches rounded to the metric equivaliet
            Dim Num As Double
            Dim Factor As Double
            Dim remain As Double
            Dim whole As Long
            Dim multi As Integer
            Dim multafter As Double
            Num = _rVal
            Select Case aNearest
                Case dxxRoundToLimits.Eighth
                    Factor = 0.125
                Case dxxRoundToLimits.Half
                    Factor = 0.5
                Case dxxRoundToLimits.Quarter
                    Factor = 0.25
                Case dxxRoundToLimits.Third
                    Factor = 1 / 3
                Case dxxRoundToLimits.Sixteenth
                    Factor = 0.0625
                Case dxxRoundToLimits.ThirtySeconds
                    Factor = 0.03125
                Case dxxRoundToLimits.One
                    Factor = 1
                Case dxxRoundToLimits.Millimeter
                    multafter = 1 / 25.4
                    Num *= 25.4
                    Factor = 1
                Case dxxRoundToLimits.Centimeter
                    multafter = 1 / 2.54
                    Num *= 2.54
                    Factor = 1
            End Select
            If bRoundUp And bRoundDown Then bRoundDown = False
            If Factor = 0 Then Factor = 0.125
            whole = Int(Num)
            remain = Num - whole
            If Not bRoundDown And Not bRoundUp Then
                _rVal = dxfMath.Rounder(Num, Factor)
            Else
                multi = Fix(remain / Factor)
                If bRoundUp Then
                    If remain - (multi * Factor) <> 0 Then multi += 1
                End If
                _rVal = whole + multi * Factor
            End If
            If multafter <> 0 Then
                _rVal *= multafter
            End If
            Return _rVal
        End Function
        Friend Shared Function Rounder(aNum As Object, aFraction As Double) As Double
            Dim _rVal As Double = TVALUES.To_DBL(aNum)
            If aFraction <= 0 Then Return _rVal
            '#1the number to round
            '#2the limit to round to
            '^used to round a numeric value to the indicated limit.
            Dim whole As Long = TVALUES.To_LNG(Math.Round(_rVal, 0))
            Dim remain As Double = _rVal - whole
            Dim multi As Integer = TVALUES.To_INT(remain / Math.Abs(aFraction))
            Return whole + multi * aFraction
        End Function
        Public Shared Function SpannedAngle(bClockwise As Boolean, aStartAngle As Double, aEndAngle As Double) As Double
            Dim _rVal As Double
            '^the angle spanned by the entity
            '~dynamically calculated based on current entity properties
            'return the angle covered by the entity
            'useful in determining midpoint and entity length
            'On Error Resume Next
            Dim sa As Double
            Dim ea As Double
            sa = aStartAngle
            ea = aEndAngle
            Do While sa < 0
                sa += 360
            Loop
            Do While ea < 0
                ea += 360
            Loop
            If Not bClockwise Then
                _rVal = Math.Round(ea - sa, 4)
            Else
                _rVal = Math.Round(sa - ea, 4)
            End If
            Do While _rVal < 0
                _rVal += 360
            Loop
            If _rVal = 0 And (aEndAngle <> aStartAngle) Then _rVal = 360
            Return _rVal
        End Function
#End Region 'shared methods
    End Class
End Namespace
