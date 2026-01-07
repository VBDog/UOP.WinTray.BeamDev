Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public Class dxoPoint
        Implements ICloneable
#Region "Members"
        Public Code As Byte
        Public X As Double
        Public Y As Double
#End Region 'Members
#Region "Constructors"
        Public Sub New(Optional aCode As dxxVertexStyles = dxxVertexStyles.MOVETO)
            Code = aCode
        End Sub
        Public Sub New(aX As Double, aY As Double, Optional aCode As dxxVertexStyles = dxxVertexStyles.MOVETO)
            X = aX
            Y = aY
            Code = aCode
        End Sub
        Friend Sub New(aVector As TVECTOR)
            X = aVector.X
            Y = aVector.Y
            Code = aVector.Code
        End Sub

        Friend Sub New(aPoint As TPOINT)
            X = aPoint.X
            Y = aPoint.Y
            Code = aPoint.Code
        End Sub
#End Region 'Constructors
#Region "Properties"
        Public ReadOnly Property CodeString As String
            Get
                Select Case Code
                    Case dxxVertexStyles.CLOSEFIGURE '= &H1
                        Return "CLOSEFIG"
                    Case dxxVertexStyles.LINETO '= &H2
                        Return "LINETO"
                    Case dxxVertexStyles.BEZIER '= &H3
                        Return "BEZIER"
                    Case dxxVertexStyles.BEZIERTO '= &H4
                        Return "BEZTO"
                    Case dxxVertexStyles.MOVETO '= &H6
                        Return "MOVETO"
                    Case dxxVertexStyles.PIXEL '= &H10
                        Return "PIXEL"
                    Case dxxVertexStyles.CLOSEPATH  '= &H128
                        Return "CLOSEPATH"
                    Case Else 'dxxVertexStyles.UNDEFINED  '= &H0
                        Return Code.ToString
                End Select
            End Get
        End Property
#End Region'Properties
#Region "Methods"
        Friend Function WithRespectToPlane(aPlane As TPLANE, Optional aPrecis As Integer = 8, Optional aScaleFactor As Double = 0.0) As dxoPoint
            Dim v1 As TVECTOR = CType(Me, dxfVector).Strukture
            v1 = v1.WithRespectTo(aPlane, aPrecis, aScaleFactor)
            Return New dxoPoint(v1.X, v1.Y, v1.Code)
        End Function
        Public Function Coords(Optional aPrecis As Integer = 0, Optional bSuppressParens As Boolean = False) As String
            If aPrecis > 10 Or aPrecis <= 0 Then aPrecis = 10
            If Not bSuppressParens Then
                Return $"({ Math.Round(X, aPrecis) },{ Math.Round(Y, aPrecis) })"
            Else
                Return $"{Math.Round(X, aPrecis)},{ Math.Round(Y, aPrecis)}"
            End If
        End Function
        Public Overrides Function ToString() As String
            Return $"{ Format(X, "0.0###") }, { Format(Y, "0.0###") }, { CodeString}"
        End Function
        Public Function Clone() As dxoPoint
            Return New dxoPoint(X, Y, Code)
        End Function
        Public Function Scale(aScaleX As Double, Optional aScaleY As Double? = Nothing) As Boolean
            '#1the X factor to scale the point
            '#2the Y factor to scale the point
            '^rescales the pont in space and dimension by the passed factors

            Dim xnew As Double = X
            Dim ynew As Double = Y
            If xnew = 0 And ynew = 0 Then Return False

            Dim scx As Double = aScaleX
            Dim scy As Double = aScaleX
            If aScaleY.HasValue Then scy = aScaleY.Value

            xnew *= scx
            ynew *= scy

            Dim _rVal As Boolean = xnew <> X Or ynew <> Y



            X = xnew
            Y = ynew
            Return _rVal
        End Function
        Friend Sub Project(aDirection As TVECTOR, aDistance As Object, Optional bSuppressNormalize As Boolean = False, Optional bInvertDirection As Boolean = False)
            Dim d1 As Double = TVALUES.To_DBL(aDistance)
            If d1 = 0 Then Return
            If Not bSuppressNormalize Then aDirection.Normalize()
            If bInvertDirection Then d1 *= -1
            Dim v1 As TVECTOR = CType(Me, dxfVector).Strukture + (aDirection * d1)
            X = v1.X
            Y = v1.Y
        End Sub
        Public Function SnapToGrid(aAngle As Double, aDisplacement As Double, ByRef rDX As Double, ByRef rDY As Double) As dxoPoint
            Dim _rVal As dxoPoint = Clone()
            'On Error Resume Next
            Dim vLen As Double
            Dim P1 As dxoPoint
            Dim Y1 As Double
            Dim angVal As Integer
            angVal = Fix(aAngle / 22.5)
            vLen = aDisplacement
            rDX = 0
            rDY = 0
            Select Case angVal
                Case 0 'x 22.5 = 0
                    rDX = vLen
                Case 4 'x 22.5 = 90
                    rDY = vLen
                Case 8 'x 22.5 = 180
                    rDX = -vLen
                Case 12 'x 22.5 = 270
                    rDY = -vLen
                Case 2 'x 22.5 = 45
                    rDX = Math.Round(vLen * Math.Sqrt(2) * Math.Cos(Math.PI / 4), 0)
                    rDY = rDX
                Case 6 'x 22.5 = 135
                    rDX = -Math.Round(vLen * Math.Sqrt(2) * Math.Cos(Math.PI / 4), 0)
                    rDY = -rDX
                Case 10 'x 22.5 = 225
                    rDX = -Math.Round(vLen * Math.Sqrt(2) * Math.Cos(Math.PI / 4), 0)
                    rDY = rDX
                Case 14 'x 22.5 = 315
                    rDX = Math.Round(vLen * Math.Sqrt(2) * Math.Cos(Math.PI / 4), 0)
                    rDY = -rDX
                Case 1 'x 22.5 = 22.5
                    rDX = vLen * Math.Cos(Math.PI / 8)
                    rDY = vLen * Math.Cos(3 * Math.PI / 8)
                    P1 = Me + New dxoPoint(rDX, rDY)
                    Y1 = dxfMath.RoundTo(P1.Y, dxxRoundToLimits.One, False, True)
                    If P1.Y <= Y1 + 0.5 Then
                        rDX = dxfMath.RoundTo(rDX, dxxRoundToLimits.One, True) + 1
                    Else
                        rDX = dxfMath.RoundTo(rDX, dxxRoundToLimits.One, True)
                    End If
                    rDY = dxfMath.RoundTo(rDY, dxxRoundToLimits.One, True)
                Case 3 'x 22.5 = 67.5
                    rDX = vLen * Math.Cos(3 * Math.PI / 8)
                    rDY = vLen * Math.Cos(Math.PI / 8)
                    P1 = Me + New dxoPoint(rDX, rDY)
                    Y1 = dxfMath.RoundTo(P1.Y, dxxRoundToLimits.One, False, True)
                    If P1.Y <= Y1 + 0.5 Then
                        rDX = dxfMath.RoundTo(rDX, dxxRoundToLimits.One, True) '+ 1
                    Else
                        rDX = dxfMath.RoundTo(rDX, dxxRoundToLimits.One, True)
                    End If
                    rDY = dxfMath.RoundTo(rDY, dxxRoundToLimits.One, True)
                Case 7 'x 22.5 = 157.5
                    rDX = vLen * Math.Cos(Math.PI / 8)
                    rDY = vLen * Math.Cos(3 * Math.PI / 8)
                    P1 = Me + New dxoPoint(-rDX, rDY)
                    Y1 = dxfMath.RoundTo(P1.Y, dxxRoundToLimits.One, False, True)
                    If P1.Y <= Y1 + 0.5 Then
                        rDX = -(dxfMath.RoundTo(rDX, dxxRoundToLimits.One, True) + 1)
                    Else
                        rDX = -dxfMath.RoundTo(rDX, dxxRoundToLimits.One, True)
                    End If
                    rDY = dxfMath.RoundTo(rDY, dxxRoundToLimits.One, True)
                Case 5 'x 22.5 = 112.5
                    rDX = vLen * Math.Cos(3 * Math.PI / 8)
                    rDY = vLen * Math.Cos(Math.PI / 8)
                    P1 = Me + New dxoPoint(-rDX, rDY)
                    Y1 = dxfMath.RoundTo(P1.Y, dxxRoundToLimits.One, False, True)
                    If P1.Y <= Y1 + 0.5 Then
                        rDX = -(dxfMath.RoundTo(rDX, dxxRoundToLimits.One, True) + 1)
                    Else
                        rDX = -dxfMath.RoundTo(rDX, dxxRoundToLimits.One, True)
                    End If
                    rDY = dxfMath.RoundTo(rDY, dxxRoundToLimits.One, True)
                Case 9 'x 22.5 = 202.5
                    rDX = vLen * Math.Cos(Math.PI / 8)
                    rDY = vLen * Math.Cos(3 * Math.PI / 8)
                    P1 = Me + New dxoPoint(-rDX, -rDY)
                    Y1 = dxfMath.RoundTo(P1.Y, dxxRoundToLimits.One, False, True)
                    If P1.Y > Y1 + 0.5 Then
                        rDX = -(dxfMath.RoundTo(rDX, dxxRoundToLimits.One, True) + 1)
                    Else
                        rDX = -dxfMath.RoundTo(rDX, dxxRoundToLimits.One, True)
                    End If
                    rDY = -dxfMath.RoundTo(rDY, dxxRoundToLimits.One, True)
                Case 11 'x 22.5 = 247.5
                    rDX = vLen * Math.Cos(3 * Math.PI / 8)
                    rDY = vLen * Math.Cos(Math.PI / 8)
                    P1 = Me + New dxoPoint(-rDX, -rDY)
                    Y1 = dxfMath.RoundTo(P1.Y, dxxRoundToLimits.One, False, True)
                    If P1.Y > Y1 + 0.5 Then
                        rDX = -(dxfMath.RoundTo(rDX, dxxRoundToLimits.One, True) + 1)
                    Else
                        rDX = -dxfMath.RoundTo(rDX, dxxRoundToLimits.One, True)
                    End If
                    rDY = -dxfMath.RoundTo(rDY, dxxRoundToLimits.One, True)
                Case 15 'x 22.5 = 337.5
                    rDX = vLen * Math.Cos(Math.PI / 8)
                    rDY = vLen * Math.Cos(3 * Math.PI / 8)
                    P1 = Me + New dxoPoint(rDX, -rDY)
                    Y1 = dxfMath.RoundTo(P1.Y, dxxRoundToLimits.One, False, True)
                    If P1.Y > Y1 + 0.5 Then
                        rDX = (dxfMath.RoundTo(rDX, dxxRoundToLimits.One, True) + 1)
                    Else
                        rDX = dxfMath.RoundTo(rDX, dxxRoundToLimits.One, True)
                    End If
                    rDY = -dxfMath.RoundTo(rDY, dxxRoundToLimits.One, True)
                Case 13 'x 22.5 = 292.5
                    rDX = vLen * Math.Cos(3 * Math.PI / 8)
                    rDY = vLen * Math.Cos(Math.PI / 8)
                    P1 = Me + New dxoPoint(rDX, -rDY)
                    Y1 = dxfMath.RoundTo(P1.Y, dxxRoundToLimits.One, False, True)
                    If P1.Y > Y1 + 0.5 Then
                        rDX = (dxfMath.RoundTo(rDX, dxxRoundToLimits.One, True) + 1)
                    Else
                        rDX = dxfMath.RoundTo(rDX, dxxRoundToLimits.One, True)
                    End If
                    rDY = -dxfMath.RoundTo(rDY, dxxRoundToLimits.One, True)
            End Select
            _rVal.X += rDX
            _rVal.Y += rDY
            Return _rVal
        End Function
#End Region 'Methods
#Region "Shared Methods"
        Public Shared Function Compare(A As dxoPoint, B As dxoPoint, Optional aPrecis As Integer = 0) As Boolean
            Dim _rVal As Boolean
            If A Is Nothing Or B Is Nothing Then Return False
            If aPrecis <= 0 Then
                If A.X = B.X Then
                    _rVal = A.Y = B.Y
                End If
            Else
                aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
                If Math.Round(A.X, aPrecis) = Math.Round(B.X, aPrecis) Then
                    _rVal = Math.Round(A.Y, aPrecis) = Math.Round(B.Y, aPrecis)
                End If
            End If
            Return _rVal
        End Function
        Friend Shared Function Compare(A As dxoPoint, B As TPOINT, Optional aPrecis As Integer = 0) As Boolean
            Dim _rVal As Boolean
            If A Is Nothing Then Return False
            If aPrecis <= 0 Then
                If A.X = B.X Then
                    _rVal = A.Y = B.Y
                End If
            Else
                aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
                If Math.Round(A.X, aPrecis) = Math.Round(B.X, aPrecis) Then
                    _rVal = Math.Round(A.Y, aPrecis) = Math.Round(B.Y, aPrecis)
                End If
            End If
            Return _rVal
        End Function
        Public Shared Function Interpolate(a As dxoPoint, B As dxoPoint, aAlpha As Double) As dxoPoint
            Return New dxoPoint With {
                    .X = a.X + aAlpha * (B.X - a.X),
                    .Y = a.Y + aAlpha * (B.Y - a.Y),
                    .Code = a.Code
                }
        End Function
        Friend Shared Function Direction(A As dxoPoint, B As dxoPoint, bReturnInverse As Boolean, ByRef rDirectionIsNull As Boolean, ByRef rDistance As Double) As TVECTOR
            Dim _rVal As TVECTOR = CType((A - B), dxfVector).Strukture().Normalized(rDirectionIsNull, rDistance)
            If bReturnInverse Then _rVal *= -1
            Return _rVal
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return Clone()
        End Function
#End Region 'Shared Methods
#Region "Operators"
        Public Shared Operator +(A As dxoPoint, B As dxoPoint) As dxoPoint
            If A IsNot Nothing And B IsNot Nothing Then
                Return New dxoPoint(A.X + B.X, A.Y + B.Y, A.Code)
            Else
                If A IsNot Nothing Then
                    Return A.Clone
                ElseIf B IsNot Nothing Then
                    Return B.Clone
                Else
                    Return New dxoPoint(0, 0, dxxVertexStyles.MOVETO)
                End If
            End If
        End Operator
        Public Shared Operator -(A As dxoPoint, B As dxoPoint) As dxoPoint
            If A IsNot Nothing And B IsNot Nothing Then
                Return New dxoPoint(A.X - B.X, A.Y - B.Y, A.Code)
            Else
                If A IsNot Nothing Then
                    Return A.Clone
                ElseIf B IsNot Nothing Then
                    Return B.Clone
                Else
                    Return New dxoPoint(0, 0, dxxVertexStyles.MOVETO)
                End If
            End If
        End Operator
        Public Shared Operator =(A As dxoPoint, B As dxoPoint) As Boolean
            If A IsNot Nothing And B IsNot Nothing Then
                Return (A.X = B.X) And (A.Y = B.Y)
            Else
                Return False
            End If
        End Operator
        Public Shared Operator <>(A As dxoPoint, B As dxoPoint) As Boolean
            If A IsNot Nothing And B IsNot Nothing Then
                Return (A.X <> B.X) Or (A.Y <> B.Y)
            Else
                Return False
            End If
        End Operator

        Public Shared Widening Operator CType(aPoint As dxoPoint) As dxfVector
            Return New dxfVector(aPoint.X, aPoint.Y, 0) With {.VertexCode = aPoint.Code}
        End Operator
#End Region 'Operators
    End Class
End Namespace
