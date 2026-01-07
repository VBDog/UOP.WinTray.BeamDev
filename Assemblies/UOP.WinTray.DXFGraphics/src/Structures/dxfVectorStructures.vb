Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics.Structures

    Friend Structure TPOINT
        Implements ICloneable
#Region "Members"
        Public Code As Byte
        Public X As Double
        Public Y As Double
#End Region 'Members
#Region "Constructors"
        Public Sub New(Optional aCode As dxxVertexStyles = dxxVertexStyles.MOVETO)
            'init ---------------------------
            X = 0
            Y = 0
            Code = 0
            'init ---------------------------
            Code = aCode
        End Sub
        Public Sub New(aX As Double, aY As Double, Optional aCode As dxxVertexStyles = dxxVertexStyles.MOVETO)
            'init ---------------------------
            X = aX
            Y = aY
            Code = TVALUES.ToByte(aCode)
            'init ---------------------------

        End Sub
        Public Sub New(aVector As TVECTOR)
            'init ---------------------------
            X = aVector.X
            Y = aVector.Y
            Code = aVector.Code
            'init ---------------------------
        End Sub
        Public Sub New(aVector As dxfVector)
            'init ---------------------------
            X = aVector.X
            Y = aVector.Y
            Code = aVector.VertexCode
            'init ---------------------------
        End Sub

        Public Sub New(aPoint As TPOINT)
            'init ---------------------------
            X = aPoint.X
            Y = aPoint.Y
            Code = aPoint.Code
            'init ---------------------------
        End Sub
        Public Sub New(aPoint As dxoPoint)
            'init ---------------------------
            X = 0
            Y = 0
            Code = 0
            'init ---------------------------
            If aPoint Is Nothing Then Return
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
                        Return Code.ToString()
                End Select
            End Get
        End Property
#End Region'Properties
#Region "Methods"
        Public Function WithRespectToPlane(aPlane As TPLANE, Optional aPrecis As Integer = 8, Optional aScaleFactor As Double = 0.0) As TPOINT
            Dim v1 As New TVECTOR(Me)
            v1 = v1.WithRespectTo(aPlane, aPrecis, aScaleFactor)
            Return New TPOINT(v1.X, v1.Y, v1.Code)
        End Function
        Public Function Coords(Optional aPrecis As Integer = 0, Optional bSuppressParens As Boolean = False) As String
            If aPrecis > 10 Or aPrecis <= 0 Then aPrecis = 10
            If Not bSuppressParens Then
                Return $"({ Math.Round(X, aPrecis) },{ Math.Round(Y, aPrecis) })"
            Else
                Return $"{Math.Round(X, aPrecis) },{ Math.Round(Y, aPrecis)}"
            End If
        End Function
        Public Overrides Function ToString() As String
            Return $"{Format(X, "0.0###")}, { Format(Y, "0.0###") },{ CodeString }"
        End Function
        Public Function Clone() As TPOINT
            Return New TPOINT(Me)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TPOINT(Me)
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
        Public Sub Project(aDirection As TVECTOR, aDistance As Double, Optional bSuppressNormalize As Boolean = False, Optional bInvertDirection As Boolean = False)
            Dim d1 As Double = TVALUES.To_DBL(aDistance)
            If d1 = 0 Then Return
            If Not bSuppressNormalize Then aDirection.Normalize()
            If bInvertDirection Then d1 *= -1
            Dim v1 As New TVECTOR(Me)
            v1 += aDirection * d1
            X = v1.X
            Y = v1.Y
        End Sub
        Public Function SnapToGrid(aAngle As Double, aDisplacement As Double, ByRef rDX As Double, ByRef rDY As Double) As TPOINT
            Dim _rVal As New TPOINT(Me)
            'On Error Resume Next
            Dim vLen As Double
            Dim P1 As TPOINT
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
                    P1 = Me + New TPOINT(rDX, rDY)
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
                    P1 = Me + New TPOINT(rDX, rDY)
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
                    P1 = Me + New TPOINT(-rDX, rDY)
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
                    P1 = Me + New TPOINT(-rDX, rDY)
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
                    P1 = Me + New TPOINT(-rDX, -rDY)
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
                    P1 = Me + New TPOINT(-rDX, -rDY)
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
                    P1 = Me + New TPOINT(rDX, -rDY)
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
                    P1 = Me + New TPOINT(rDX, -rDY)
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
        Public Shared Function Compare(a As TPOINT, B As TPOINT, Optional aPrecis As Integer = -1) As Boolean
            Dim _rVal As Boolean

            If aPrecis < 0 Then
                If a.X = B.X Then
                    _rVal = a.Y = B.Y
                End If
            Else
                aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
                If Math.Round(a.X, aPrecis) = Math.Round(B.X, aPrecis) Then
                    _rVal = Math.Round(a.Y, aPrecis) = Math.Round(B.Y, aPrecis)
                End If
            End If
            Return _rVal
        End Function
        Public Shared Function Interpolate(a As TPOINT, B As TPOINT, aAlpha As Double) As TPOINT
            Return New TPOINT With {
                    .X = a.X + aAlpha * (B.X - a.X),
                    .Y = a.Y + aAlpha * (B.Y - a.Y),
                    .Code = a.Code
                }
        End Function
        Public Shared Function Direction(A As TPOINT, B As TPOINT, bReturnInverse As Boolean, ByRef rDirectionIsNull As Boolean, ByRef rDistance As Double) As TVECTOR
            Dim _rVal As New TVECTOR(A - B)
            _rVal = _rVal.Normalized(rDirectionIsNull, rDistance)
            If bReturnInverse Then _rVal *= -1
            Return _rVal
        End Function

#End Region 'Shared Methods
#Region "Operators"
        Public Shared Operator +(A As TPOINT, B As TPOINT) As TPOINT
            Return New TPOINT(A.X + B.X, A.Y + B.Y, A.Code)
        End Operator
        Public Shared Operator -(A As TPOINT, B As TPOINT) As TPOINT
            Return New TPOINT(A.X - B.X, A.Y - B.Y, A.Code)
        End Operator
        Public Shared Operator =(A As TPOINT, B As TPOINT) As Boolean
            Return (A.X = B.X) And (A.Y = B.Y)
        End Operator
        Public Shared Operator <>(A As TPOINT, B As TPOINT) As Boolean
            Return (A.X <> B.X) Or (A.Y <> B.Y)
        End Operator
        Public Shared Widening Operator CType(aPoint As TPOINT) As TVECTOR
            Return New TVECTOR(aPoint.X, aPoint.Y, 0) With {.Code = aPoint.Code}
        End Operator
        Public Shared Widening Operator CType(aPoint As TPOINT) As dxfVector
            Return New dxfVector(aPoint.X, aPoint.Y, 0) With {.VertexCode = aPoint.Code}
        End Operator
#End Region 'Operators
    End Structure 'TPOINT
    Friend Structure TPOINTS
        Implements ICloneable
#Region "Members"
        Private _Init As Boolean
        Private _Members() As TPOINT
#End Region 'Members
#Region "Constructors"

        Public Sub New(aPoints As TPOINTS)
            'init ------------------------------------
            _Init = True
            ReDim _Members(-1)
            'init ------------------------------------

            For i As Integer = 1 To aPoints.Count
                Add(New TPOINT(aPoints.Item(i)))
            Next
        End Sub

        Public Sub New(aPoints As TVECTORS)
            'init ------------------------------------
            _Init = True
            ReDim _Members(-1)
            'init ------------------------------------
            For i As Integer = 1 To aPoints.Count
                Add(New TPOINT(aPoints.Item(i)))
            Next
        End Sub

        Public Sub New(aPoints As dxoPoints)
            'init ------------------------------------
            _Init = True
            ReDim _Members(-1)
            'init ------------------------------------
            If aPoints Is Nothing Then Return
            For Each pt As dxoPoint In aPoints
                Add(New TPOINT(pt.X, pt.Y, pt.Code))
            Next

        End Sub

        Public Sub New(aPoints As colDXFVectors)
            'init ------------------------------------
            _Init = True
            ReDim _Members(-1)
            'init ------------------------------------
            If aPoints Is Nothing Then Return
            For Each pt As dxfVector In aPoints
                Add(New TPOINT(pt))
            Next

        End Sub

        Public Sub New(aCount As Integer)
            'init ------------------------------------
            _Init = True
            ReDim _Members(-1)
            'init ------------------------------------
            If aCount > 0 Then
                Array.Resize(_Members, aCount)
            End If
        End Sub

#End Region 'Constructors
#Region "Properties"
        Public ReadOnly Property Count As Integer
            Get
                If Not _Init Then
                    _Init = True
                    ReDim _Members(-1)
                End If
                Return _Members.Count
            End Get
        End Property
#End Region 'Properties
#Region "Methods"
        Public Function Item(aIndex As Integer) As TPOINT
            If aIndex < 0 Or aIndex > Count Then Return Nothing
            Return _Members(aIndex - 1)
        End Function

        Public Function Point(aIndex As Integer) As dxoPoint
            If aIndex < 0 Or aIndex > Count Then Return Nothing
            Return New dxoPoint(_Members(aIndex - 1))
        End Function

        Public Sub SetItem(aIndex As Integer, value As TPOINT)
            If aIndex < 0 Or aIndex > Count Then Return
            _Members(aIndex - 1) = value
        End Sub
        Public Overrides Function ToString() As String
            Return $"TPOINTS[{ Count }]"
        End Function
        Public Function Clone() As TPOINTS
            Return New TPOINTS(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TPOINTS(Me)
        End Function
        Public Sub Add(Optional aX As Double = 0, Optional aY As Double = 0, Optional aCode As Byte? = Nothing)
            Add(New TPOINT(aX, aY), aCode)
        End Sub
        Public Sub Add(aPoint As TPOINT, Optional aCode As Byte? = Nothing)

            If Count >= Integer.MaxValue Then Return
            Array.Resize(_Members, Count + 1)
            _Members(_Members.Count - 1) = aPoint
            If aCode.HasValue Then _Members(_Members.Length - 1).Code = aCode.Value
        End Sub

        Public Sub Add(aPoint As TPOINT, aCode As dxxVertexStyles)

            If Count >= Integer.MaxValue Then Return
            Array.Resize(_Members, Count + 1)
            _Members(_Members.Count - 1) = aPoint
            _Members(_Members.Length - 1).Code = TVALUES.ToByte(aCode)
        End Sub

        Public Sub Add(aVector As TVECTOR, Optional aCode As Byte? = Nothing)
            Add(CType(aVector, TPOINT), aCode)
        End Sub
        Public Sub Add(aLine As TLINE)

            AddLine(CType(aLine.SPT, TPOINT), CType(aLine.EPT, TPOINT))
        End Sub
        Public Sub Print()
            Dim P1 As TPOINT
            Dim aStr As String
            Dim bStr As String
            For i As Integer = 1 To Count
                P1 = Item(i)
                If P1.Code = dxxVertexStyles.MOVETO Then
                    aStr = "MOVETO"
                ElseIf P1.Code = dxxVertexStyles.CLOSEFIGURE Then
                    aStr = "CLOSEFIGURE"
                ElseIf P1.Code = dxxVertexStyles.LINETO Then
                    aStr = "LINETO"
                ElseIf P1.Code = dxxVertexStyles.BEZIERTO Then
                    aStr = "BEZIERTO"
                ElseIf P1.Code = dxxVertexStyles.PIXEL Then
                    aStr = "PIXEL"
                Else
                    aStr = "UNDEF"
                End If
                bStr = P1.Coords(4)
                bStr = bStr.Replace(") ", $",{ aStr})")
                Console.WriteLine($"{ i } - {bStr}")
            Next i
        End Sub
        Public Sub Scale(aScaleX As Double, Optional aScaleY As Double? = Nothing)
            '#1the vectors to scale
            '#2the scale factor to apply
            '#3the center to scale with resect to
            '#4the y scale to apply
            '^moves the current coordinates of the vectors in the collection to a vector scaled with respect to the passed center

            Dim yScl As Double
            Dim p1 As TPOINT
            If aScaleX <= 0 Then aScaleX = 1
            If aScaleY.HasValue Then yScl = aScaleY.Value Else yScl = aScaleX
            If yScl <= 0 Then yScl = aScaleX
            'aPoints.Print()
            For i As Integer = 1 To Count
                p1 = _Members(i - 1)
                p1.Scale(aScaleX, yScl)
                _Members(i - 1) = p1
            Next i
            'aPoints.Print()
        End Sub
        Public Sub Remove(aIndex As Integer)
            If aIndex <= 0 Or aIndex > Count Then Return
            If aIndex = Count Then
                Array.Resize(_Members, Count - 1)
                Return
            End If

            Dim idx As Integer
            Dim nMems(0 To Count - 1) As TPOINT
            idx = 0
            For i As Integer = 1 To Count
                If i <> aIndex Then
                    nMems(idx) = _Members(i - 1)
                    idx += 1
                End If
            Next i
            _Members = nMems
        End Sub


        Public Function Bounds(aPlane As TPLANE, Optional aShearAngle As Double = 0) As TPLANE
            Return ToPlaneVectors(aPlane, aShearAngle).BoundingRectangle(aPlane, Nothing, True)
        End Function

        Public Function ToPlaneVectors(aPlane As TPLANE, Optional aShearAngle As Double = 0, Optional aRelativeToPlane As dxfPlane = Nothing) As TVECTORS
            Dim _rVal As New TVECTORS(0)
            Dim withrespc As Boolean = aRelativeToPlane IsNot Nothing
            For i As Integer = 1 To Count
                Dim pt As TPOINT = Item(i)
                Dim vpt As TVECTOR = aPlane.Vector(pt.X, pt.Y, 0, aShearXAngle:=aShearAngle)
                If withrespc Then
                    Dim relpt As TVECTOR = vpt.WithRespectTo(aRelativeToPlane)
                    _rVal.Add(relpt, aCode:=pt.Code)
                Else
                    _rVal.Add(vpt, aCode:=pt.Code)
                End If

            Next
            Return _rVal
        End Function


        Public Sub SetCode(aIndex As Integer, aCode As Byte)
            If aIndex > 0 And aIndex <= Count Then
                _Members(aIndex - 1).Code = aCode
            End If
        End Sub
        Public Sub Append(bPoints As TPOINTS, Optional aStartID As Integer = 0)
            'On Error Resume Next
            If Not _Init Then Clear()
            If bPoints.Count <= 0 Then Return
            Dim i As Integer
            Dim sid As Integer
            If aStartID > 0 Then sid = aStartID
            If sid > bPoints.Count - 1 Then sid = bPoints.Count - 1
            If sid <= 0 Then
                _Members = _Members.Concat(bPoints._Members).ToArray
            Else
                For i = sid To bPoints.Count
                    If Count + 1 > Integer.MaxValue Then Exit For
                    Add(bPoints.Item(i))
                Next i
            End If
        End Sub

        Public Sub Append(bPoints As List(Of dxoPoint), Optional aStartID As Integer = 1)
            'On Error Resume Next
            If Not _Init Then Clear()
            If bPoints Is Nothing Then Return
            If bPoints.Count <= 0 Then Return
            Dim i As Integer
            Dim sid As Integer = aStartID
            If sid <= 0 Then sid = 1
            If sid > bPoints.Count Then sid = bPoints.Count
            For i = sid To bPoints.Count
                If Count + 1 > Integer.MaxValue Then Exit For
                If i > bPoints.Count Then Exit For
                Add(New TPOINT(bPoints.Item(i - 1)))
            Next i
        End Sub
        Public Sub Append(aVectors As TVECTORS, Optional aStartID As Integer = 1)
            'On Error Resume Next

            If aVectors.Count <= 0 Then Return
            Dim i As Integer
            Dim sid As Integer
            If aStartID > 0 Then sid = aStartID
            If sid > aVectors.Count Then sid = aVectors.Count
            For i = sid To aVectors.Count
                If Count >= Integer.MaxValue Then Exit For
                If i >= 1 And i <= aVectors.Count Then
                    Add(aVectors.Item(i))
                End If
            Next i
        End Sub
        Public Function LastMember(Optional bRemove As Boolean = False) As TPOINT
            If Count <= 0 Then Return New TPOINT(0, 0, 0)
            Dim _rVal As TPOINT = Item(Count)
            If bRemove Then
                System.Array.Resize(_Members, Count - 1)
            End If
            Return _rVal
        End Function
        Public Sub Clear()
            _Init = True
            ReDim _Members(-1)

        End Sub
        Public Sub Print(Optional aTag As String = "")
            System.Diagnostics.Debug.WriteLine("")
            System.Diagnostics.Debug.WriteLine($"{ aTag } TPOINTS[{ Count  }]".Trim())
            For i As Integer = 1 To Count
                System.Diagnostics.Debug.WriteLine($"   { i } - {Item(i)}")
            Next
        End Sub
        Public Sub Scale(aXScale As Double?, aYScale As Double?)
            Try
                Dim sX As Double
                Dim sY As Double
                If aXScale.HasValue Then sX = aXScale.Value Else sX = 1
                If sX <= 0 Then sX = 1

                If aYScale.HasValue Then sY = aYScale.Value Else sY = 1
                If sY <= 0 Then sY = 1
                If sX = 1 And sY = 1 Then Return
                For i As Integer = 1 To Count
                    _Members(i - 1).X *= sX
                    _Members(i - 1).Y *= sY
                Next i
            Catch ex As Exception
            End Try
        End Sub

        Public ReadOnly Property Limits As TLIMITS
            Get

                Dim _rVal As New TLIMITS()
                If Count <= 0 Then Return _rVal
                For i As Integer = 1 To Count
                    Dim pt As TPOINT = Item(i)
                    If i = 1 Then
                        _rVal.Update(aLeft:=pt.X, aRight:=pt.X, aBottom:=pt.Y, aTop:=pt.Y)
                    Else
                        _rVal.Update(pt)
                    End If

                Next
                Return _rVal
            End Get
        End Property
        Public Sub AddTwo(aPoint As TPOINT, bPoint As TPOINT, Optional aCode As Byte? = Nothing, Optional bCode As Byte? = Nothing)
            Add(aPoint)
            If aCode.HasValue Then SetCode(Count, TVALUES.ToByte(aCode.Value))
            Add(bPoint)
            If bCode.HasValue Then SetCode(Count, TVALUES.ToByte(bCode.Value))
        End Sub
        Public Sub AddLine(aLine As TLINE, Optional bTestLast As Boolean = False, Optional aPrecis As Integer = -1)
            AddLine(aLine.SPT, aLine.EPT, bTestLast, aPrecis)
        End Sub
        Public Sub AddLine(P1 As TPOINT, P2 As TPOINT, Optional bTestLast As Boolean = False, Optional aPrecis As Integer = -1, Optional bJustMoveTo As Boolean = False)
            If Not bTestLast Or Count <= 0 Then
                If Not bJustMoveTo Then
                    AddTwo(P1, P2, TVALUES.ToByte(dxxVertexStyles.MOVETO), TVALUES.ToByte(dxxVertexStyles.LINETO))
                Else
                    AddTwo(P1, P2, TVALUES.ToByte(dxxVertexStyles.MOVETO), TVALUES.ToByte(dxxVertexStyles.MOVETO))
                End If

            Else
                If TPOINT.Compare(Item(Count), P1, aPrecis) Then
                    If Not bJustMoveTo Then
                        Add(P2, dxxVertexStyles.LINETO)
                    Else
                        Add(P2, dxxVertexStyles.MOVETO)
                    End If
                Else
                    If Not bJustMoveTo Then
                        AddTwo(P1, P2, TVALUES.ToByte(dxxVertexStyles.MOVETO), TVALUES.ToByte(dxxVertexStyles.LINETO))
                    Else
                        AddTwo(P1, P2, TVALUES.ToByte(dxxVertexStyles.MOVETO), TVALUES.ToByte(dxxVertexStyles.MOVETO))
                    End If

                End If
            End If
        End Sub
        Public Sub AddLine(V1 As TVECTOR, V2 As TVECTOR, Optional bTestLast As Boolean = False, Optional aPrecis As Integer = -1)
            AddLine(CType(V1, TPOINT), CType(V2, TPOINT), bTestLast, aPrecis)
        End Sub
        Public Sub Translate(aXChange As Double, aYChange As Double)
            For i As Integer = 1 To Count
                _Members(i - 1).X += aXChange
                _Members(i - 1).Y += aYChange
            Next i
        End Sub
        Public Function First(aCount As Integer, Optional bRemove As Boolean = False) As TPOINTS
            Dim _rVal As New TPOINTS(0)
            '#1the number of vectors to return
            '#2flag to return copies
            '#3flag to remove the subset from the collection
            '^returns the first members of the collection up to the passed count
            '~i.e. pnts_First(4) returns the first 4 members
            If Count <= 0 Then Return _rVal
            If aCount <= 0 Then Return _rVal
            If aCount > Count Then aCount = Count
            Dim P1 As TPOINT
            Dim bPoints As New TPOINTS(Me)
            Dim newmems() As TPOINT
            ReDim newmems(-1)
            Dim cnt As Integer = 0
            For i As Integer = 1 To Count
                P1 = Item(i)
                If i <= aCount Then
                    _rVal.Add(P1, P1.Code)
                    If Not bRemove Then
                        cnt += 1
                        Array.Resize(newmems, cnt)
                        newmems(cnt - 1) = P1
                    End If
                Else
                    cnt += 1
                    Array.Resize(newmems, cnt)
                    newmems(cnt - 1) = P1
                End If
            Next i
            _Members = newmems
            Return _rVal
        End Function

        Public Function ToList() As List(Of TPOINT)
            If Count <= 0 Then Return New List(Of TPOINT)
            Return _Members.ToList()
        End Function
#End Region 'Methods
#Region "Shared Methods"
        Public Shared Function FromAPIPoints(aPointArray() As System.Drawing.Point, aPointTypes() As Byte, Optional aXScaler As Double = 1, Optional aYScaler As Double = 1, Optional aXOffset As Double = 0.0, Optional aYOffset As Double = 0.0) As TPOINTS
            Dim _rVal As New TPOINTS(0)
            Try
                Dim lb1 As Integer
                Dim ub1 As Integer
                Dim lb2 As Integer
                Dim ub2 As Integer
                Dim i As Integer
                Dim pAPI As New System.Drawing.Point
                Dim P1 As TPOINT
                lb1 = 0
                ub1 = aPointArray.Length - 1
                lb2 = 0
                ub2 = aPointTypes.Length - 1
                For i = lb1 To ub1
                    pAPI = aPointArray(i)
                    P1 = New TPOINT(pAPI.X * aXScaler + aXOffset, pAPI.Y * aYScaler + aYOffset, dxxVertexStyles.MOVETO)
                    If i >= lb2 And i <= ub2 Then P1.Code = aPointTypes(i)
                    _rVal.Add(P1)
                Next i
                Return _rVal
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine(ex.Message)
                Return _rVal
            End Try
        End Function
#End Region 'Shared Methods
#Region "Operators"
        Public Shared Widening Operator CType(aPoints As TPOINTS) As TVECTORS
            Dim _rVal As New TVECTORS
            For i As Integer = 1 To aPoints.Count
                _rVal.Add(CType(aPoints.Item(i), TVECTOR))
            Next i
            Return _rVal
        End Operator
#End Region 'Operators
    End Structure 'TPOINTS
    Friend Structure TVECTOR
        Implements ICloneable
#Region "Members"
        Public Code As Byte
        Public Rotation As Double
        Public X As Double
        Public Y As Double
        Public Z As Double
#End Region 'Members
#Region "Constructors"

        Public Sub New(aDirection As dxfDirection)
            'init -----------------------
            X = 1
            Y = 0
            Z = 0
            Rotation = 0
            Code = 0
            'init -----------------------
            If aDirection Is Nothing Then Return
            X = aDirection.X
            Y = aDirection.Y
            Z = aDirection.Z

        End Sub

        Public Sub New(aVector As iVector, Optional bNormalize As Boolean = False)
            'init -----------------------
            X = 0
            Y = 0
            Z = 0
            Rotation = 0
            Code = 0
            'init -----------------------
            If aVector Is Nothing Then Return
            X = aVector.X
            Y = aVector.Y
            Z = aVector.Z
            If bNormalize Then

                TVECTOR.NormalizeOrdinates(X, Y, Z)
            End If
            If TypeOf aVector Is dxfVector Then
                Dim v1 As dxfVector = DirectCast(aVector, dxfVector)
                Rotation = v1.Rotation
                Code = v1.VertexCode
            Else
                Dim obj As Object = aVector
                If dxfUtils.CheckProperty(obj, "Rotation") Then
                    Try
                        Rotation = TVALUES.To_DBL(obj.Rotation)
                    Catch

                    End Try

                End If
            End If


        End Sub


        Public Sub New(aVector As TVECTOR)
            'init -----------------------
            X = aVector.X
            Y = aVector.Y
            Z = aVector.Z
            Rotation = aVector.Rotation
            Code = aVector.Code
            'init -----------------------

        End Sub

        Public Sub New(aVector As TVERTEX)
            'init -----------------------
            X = aVector.X
            Y = aVector.Y
            Z = aVector.Z
            Rotation = aVector.Rotation
            Code = aVector.Code
            'init -----------------------

        End Sub

        Public Sub New(Optional aX As Double = 0, Optional aY As Double = 0, Optional aZ As Double = 0)
            'init -----------------------
            X = aX
            Y = aY
            Z = aZ
            Rotation = 0
            Code = &H6
            'init -----------------------
        End Sub
        Public Sub New(aCoords As String)
            'init -----------------------
            X = 0
            Y = 0
            Z = 0
            Rotation = 0
            Code = 0
            'init -----------------------

            Dim v1 As TVECTOR = TVECTOR.FromString(aCoords)
            X = v1.X
            Y = v1.Y
            Z = v1.Z
        End Sub

        Public Sub New(aPlane As dxfPlane, Optional aX As Double = 0, Optional aY As Double = 0, Optional aZ As Double = 0, Optional aRotation As Double = 0)
            'init -----------------------
            X = aX
            Y = aY
            Z = aZ
            Rotation = aRotation
            Code = 0
            'init -----------------------
            If dxfPlane.IsNull(aPlane) Then Return
            Dim v1 As TVECTOR = aPlane.OriginV
            If aX <> 0 Then v1 += aPlane.XDirectionV * aX
            If aY <> 0 Then v1 += aPlane.YDirectionV * aY
            If aZ <> 0 Then v1 += aPlane.ZDirectionV * aZ
            X = v1.X
            Y = v1.Y
            Z = v1.Z

        End Sub

        Public Sub New(aPlane As dxfPlane, aVector As iVector)
            'init -----------------------
            X = 0
            Y = 0
            Z = 0
            Rotation = 0
            Code = 0
            'init -----------------------
            Dim v1 As dxfVector
            If aVector IsNot Nothing Then
                X = aVector.X
                Y = aVector.Y
                Z = aVector.Z
                If TypeOf aVector Is dxfVector Then
                    v1 = DirectCast(aVector, dxfVector)
                    Rotation = v1.Rotation
                    Code = v1.VertexCode
                End If

            End If
            If dxfPlane.IsNull(aPlane) Then Return
            Dim v2 As TVECTOR = aPlane.OriginV
            If X <> 0 Then v2 += aPlane.XDirectionV * X
            If Y <> 0 Then v2 += aPlane.YDirectionV * Y
            If Z <> 0 Then v2 += aPlane.ZDirectionV * Z
            X = v2.X
            Y = v2.Y
            Z = v2.Z

        End Sub

        Friend Sub New(aPlane As TPLANE, Optional aX As Double = 0, Optional aY As Double = 0, Optional aZ As Double = 0, Optional aRotation As Double = 0)
            'init -----------------------
            X = aX
            Y = aY
            Z = aZ
            Rotation = aRotation
            Code = 0
            'init -----------------------
            If TPLANE.IsNull(aPlane) Then
                Return
            End If
            Dim v1 As New TVECTOR(aPlane.Origin)
            If aX <> 0 Then v1 += aPlane.XDirection * aX
            If aY <> 0 Then v1 += aPlane.YDirection * aY
            If aZ <> 0 Then v1 += aPlane.ZDirection * aZ
            X = v1.X
            Y = v1.Y
            Z = v1.Z

        End Sub
#End Region 'Constructors
#Region "Properties"
        Public ReadOnly Property Magnitude As Double
            Get
                Return TVECTOR.SquareRoot(X, Y, Z)

            End Get
        End Property
        Public ReadOnly Property Squared As Double
            '^returns the sum of the components squared
            Get

                Return TVECTOR.Square(X, Y, Z)

            End Get
        End Property


        Public ReadOnly Property CodeString As String
            Get
                Select Case Code
                    Case dxxVertexStyles.CLOSEFIGURE '= &H1
                        Return "CLOSEFIG"
                    Case dxxVertexStyles.LINETO '= &H2
                        Return "LINETO"
                    Case dxxVertexStyles.BEZIERTO '= &H4
                        Return "BEZTO"
                    Case dxxVertexStyles.MOVETO '= &H6
                        Return "MOVETO"
                    Case dxxVertexStyles.PIXEL '= &H10
                        Return "PIXEL"
                    Case Else 'dxxVertexStyles.UNDEFINED  '= &H0
                        Return Code.ToString
                End Select
            End Get
        End Property


        Public ReadOnly Property SquareSum As Double
            Get
                '^returns the sum A.X ^ 2 + A.Y ^ 2 + A.Z ^ 2
                Return X ^ 2 + Y ^ 2 + Z ^ 2
            End Get
        End Property

#End Region 'Properties
#Region "Methods"

        Public Function Dot(B As TVECTOR) As Double

            Return (X * B.X) + (Y * B.Y) + (Z * B.Z)
        End Function

        Public Function MultiSum(A As TVECTOR) As Double

            '^returns the sum (A.X * X) + (A.Y * Y) + (A.Z * Z)
            Return (A.X * X) + (A.Y * Y) + (A.Z * Z)

        End Function

        Public Function IsNull(Optional aPrecis As Integer = 0) As Boolean
            Return TVECTOR.IsNull(Me, aPrecis)

        End Function

        Public Function IsUnit(Optional aPrecis As Integer = 0) As Boolean

            If aPrecis <= 0 Then
                Return X = 1 And Y = 1 And Z = 1
            Else
                aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
                Return Math.Round(X, aPrecis) = 1 And Math.Round(Y, aPrecis) = 1 And Math.Round(Z, aPrecis) = 1
            End If

        End Function

        Public Function IsDefined(Optional aPrecis As Integer = 0) As Boolean

            Return Not TVECTOR.IsNull(Me, aPrecis)
        End Function
        Public Function Clone() As TVECTOR
            Return New TVECTOR(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TVECTOR(Me)
        End Function
        Public Function Rounded(aPrecis As Integer) As TVECTOR
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            Return New TVECTOR(Math.Round(X, aPrecis), Math.Round(Y, aPrecis), Math.Round(Z, aPrecis))
        End Function

        ''' <summary>
        ''' returns a text string containing the vector's coordinates rounded to the passed precisions i.e. '(X.Y,Z)'
        ''' </summary>
        ''' <param name="aPrecis">the precision to apply (0-15)</param>
        ''' <param name="bSuppressParens">flag to not return open and colose parenthesis around the coordinate string</param>
        ''' <param name="bSuppressZ">flag to only reurn X and Y</param>
        ''' <returns></returns>
        Public Function Coordinates(Optional aPrecis As Integer = 0, Optional bSuppressParens As Boolean = False, Optional bSuppressZ As Boolean = False) As String
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            Dim _rVal As String = $"{ Math.Round(X, aPrecis)},{ Math.Round(Y, aPrecis) }"
            If Not bSuppressZ Then _rVal += $",{ Math.Round(Z, aPrecis)}"
            If Not bSuppressParens Then _rVal = $"({ _rVal })"
            Return _rVal
        End Function

        ''' <summary>
        ''' returns a text string containing the vector's coordinates rounded to the passed precisions i.e. '(X.Y,Z, Style Code)'
        ''' </summary>
        ''' <remarks>the coordinates are augmented with the name of the vectors vertex style</remarks>
        ''' <param name="aPrecis">the precision to apply (0-15)</param>
        ''' <returns></returns>
        Public Function CoordinatesP(Optional aPrecis As Integer = 3) As String
            aPrecis = TVALUES.LimitedValue(aPrecis, 1, 15)
            Return $"({ Math.Round(X, aPrecis)},{ Math.Round(Y, aPrecis) },{ Math.Round(Z, aPrecis)},{ TVERTEX.StyleName(Code) })"
        End Function

        ''' <summary>
        ''' returns a text string containing the vector's coordinates rounded to the passed precisions i.e. '(X.Y,Z)'
        ''' </summary>
        ''' <param name="aPrecis">the precision to apply (0-15)</param>
        ''' <returns></returns>
        Public Function Coordinates(aPrecis As Integer?) As String
            If Not aPrecis.HasValue Then Return $"({ X },{ Y },{ Z })" Else Return Coordinates(aPrecis.Value, False)
        End Function
        Public Function ComponentAlong(a As TVECTOR) As TVECTOR
            Dim _rVal As New TVECTOR(a)
            '^returns the component of this vector along the  passed vector
            Dim numer As Double = (X * a.X + Y * a.Y + Z * a.Z) 'VectorDotProductDBL(A, A)
            Dim denom As Double = (a.X * a.X + a.Y * a.Y + a.Z * a.Z) 'VectorDotProductDBL(A, A)
            If denom <> 0 Then _rVal *= numer / denom
            Return _rVal
        End Function

        Public Function DistanceTo(aLine As TLINE) As Double
            '#1the line to find the distance to
            '^returns the orthogonal (shortest) distance from the vector to the line
            Return dxfProjections.DistanceTo(Me, aLine)
        End Function

        Public Function DistanceTo(aLine As iLine) As Double
            '#1the line to find the distance to
            '^returns the orthogonal (shortest) distance from the vector to the line
            Return dxfProjections.DistanceTo(Me, New TLINE(aLine))
        End Function

        Public Function DistanceTo(aVector As iVector, Optional aPrecis As Integer = -1) As Double

            '#1the point to find a distance to
            '#2a rounding precision to apply(0-15)
            '^returns the  distance from the vector to passed
            Dim _rVal As Double = DistanceTo(New TVECTOR(aVector))
            If aPrecis >= 0 Then
                If aPrecis > 15 Then aPrecis = 15
                _rVal = Math.Round(_rVal, aPrecis)
            End If
            Return _rVal
        End Function

        Public Function DistanceTo(aArc1 As dxeArc, rInterceptPoint As dxfVector, ByRef rOrthogLine As dxeLine, ByRef rInterceptIsOnArc As Boolean) As Double
            Dim _rVal As Double = 0
            '#1the point to find a distance from
            '#2the Arc to find the distance to
            '#3returns the point on the arc where a line through the passed point intercepts the passed Arc
            '#4returns the orthoganal vector from the passed point to the passed Arc
            '#5returns True if the intercept point lines on the arc
            rOrthogLine = Nothing
            rInterceptPoint = Nothing
            rInterceptIsOnArc = False
            rOrthogLine = Nothing
            If aArc1 Is Nothing Then Return _rVal
            '^returns the orthogonal distance from the passed point to the passed Arc
            Dim v1 As New TVECTOR(Me)
            Dim radLine As dxeLine = New dxeLine(aArc1.CenterV, v1)
            Dim iPts As TVECTORS
            Dim v2 As TVECTOR
            Dim bFlag As Boolean
            Dim alist As New List(Of dxfEntity)({aArc1})
            Dim blist As New List(Of dxfEntity)({radLine})
            Try


                iPts = dxfIntersections.Points(alist, blist, True, True)
                If iPts.Count = 1 Then
                    bFlag = True
                    v2 = iPts.Item(1)
                ElseIf iPts.Count = 2 Then
                    bFlag = True
                    If v1.DistanceTo(iPts.Item(1)) < v1.DistanceTo(iPts.Item(2)) Then v2 = iPts.Item(1) Else v2 = iPts.Item(2)
                End If
                If bFlag Then
                    rInterceptPoint = New dxfVector With {.Strukture = v2}
                    rOrthogLine = New dxeLine(v1, v2)
                    _rVal = dxfProjections.DistanceTo(v1, v2)
                    rInterceptIsOnArc = aArc1.ContainsVector(v2)
                End If
                Return _rVal
            Catch ex As Exception
                Throw ex
                Return _rVal
            End Try
        End Function
        Public Overrides Function ToString() As String
            If Z <> 0 Then
                Return $"TVECTOR[{X:0.000###},{ Y:0.000###},{ Z:0.000###}]"
            Else
                Return $"TVECTOR[{ X:0.000###},{ Y:0.000###}]"
            End If
        End Function
        Public Function MidPt(A As TVECTOR) As TVECTOR
            '^returns a vector projected 0.5 times the distance between this vector and the passed vector in the direction towards the passed vecor
            Return Interpolate(A, 0.5)
        End Function
        Public Function Interpolate(A As TVECTOR, Alpha As Double) As TVECTOR
            '^returns a vector projected Alpha times the distance between this vector and the passed vector in the direction towards the passed vecor
            Dim _rVal As New TVECTOR(Me)
            _rVal.X += Alpha * (A.X - _rVal.X)
            _rVal.Y += Alpha * (A.Y - _rVal.Y)
            _rVal.Z += Alpha * (A.Z - _rVal.Z)
            Return _rVal
        End Function
        Public Function PlanarAngle(aPlane As TPLANE, Optional bSuppressRespectTo As Boolean = False) As Double
            Dim v1 = New TVECTOR(Me)

            If Not aPlane.DirectionsAreDefined Then bSuppressRespectTo = True
            If Not bSuppressRespectTo Then v1 = v1.WithRespectTo(aPlane, aPrecis:=15)
            If v1.X = 0 And v1.Y = 0 Then Return 0
            If v1.X = 0 Then
                If v1.Y > 0 Then Return 90 Else Return 270
            ElseIf v1.Y = 0 Then
                If v1.X > 0 Then Return 0 Else Return 180
            Else
                Dim ang1 As Double
                Dim ang2 As Double
                ang1 = Math.Atan(Math.Abs(v1.Y) / Math.Abs(v1.X))
                If v1.X > 0 Then
                    If v1.Y > 0 Then
                        ang2 = 0
                    Else
                        ang1 = Math.PI / 2 - ang1
                        ang2 = 1.5 * Math.PI
                    End If
                Else
                    If v1.Y > 0 Then
                        ang1 = -ang1
                        ang2 = Math.PI
                    Else
                        ang2 = Math.PI
                    End If
                End If
                Return Math.Round((ang1 + ang2) * 180 / Math.PI, 6)
            End If
        End Function
        Public Function RotatedAbout(aAxis As TVECTOR, aAngle As Double, Optional bInRadians As Boolean = False, Optional bSuppressNorm As Boolean = False) As TVECTOR
            Dim _rVal As New TVECTOR(Me)
            _rVal.RotateAbout(aAxis, aAngle, bInRadians, True)
            Return _rVal
        End Function
        Public Function RotatedAbout(aOrigin As TVECTOR, aAxis As TVECTOR, aAngle As Double, Optional bInRadians As Boolean = False, Optional bSuppressNorm As Boolean = False) As TVECTOR
            Dim _rVal As New TVECTOR(Me)
            _rVal.RotateAbout(aOrigin, aAxis, aAngle, bInRadians, bSuppressNorm)
            Return _rVal
        End Function
        Public Function RotateAbout(aAxis As TVECTOR, aAngle As Double, Optional bInRadians As Boolean = False, Optional bSuppressNorm As Boolean = False) As Boolean
            Return RotateAbout(New TVECTOR(0, 0, 0), aAxis, aAngle, bInRadians, bSuppressNorm)
        End Function
        Public Function RotateAbout(aAxis As TLINE, aAngle As Double, Optional bInRadians As Boolean = False) As Boolean
            Return RotateAbout(New TVECTOR(0, 0, 0), aAxis.Direction, aAngle, bInRadians, True)
        End Function
        Public Function RotateAbout(aOrigin As TVECTOR, aAxis As TVECTOR, aAngle As Double, Optional bInRadians As Boolean = False, Optional bSuppressNorm As Boolean = False) As Boolean
            Dim v1 As New TVECTOR(Me)
            Dim _rVal As Boolean = False '= v1.RotateAbout(aAxis, aAngle, bInRadians, bSuppressNorm)
            If Math.Abs(aAngle) <= 0.00000001 Or TVECTOR.IsNull(aAxis, 6) Then Return False

            If Not bInRadians Then aAngle = aAngle * Math.PI / 180

            Dim aXs As TVECTOR
            Dim aFlg As Boolean
            If Not bSuppressNorm Then
                aXs = aAxis.Normalized(aFlg)
                If aFlg Then Return _rVal
            Else
                aXs = aAxis
            End If
            Dim A As Double = aOrigin.X
            Dim B As Double = aOrigin.Y
            Dim C As Double = aOrigin.Z

            If TVECTOR.IsNull(aOrigin, 6) Then
                A = 0 : B = 0 : C = 0
            End If

            Dim iX As Double = v1.X
            Dim iY As Double = v1.Y
            Dim iZ As Double = v1.Z
            Dim u As Double = aXs.X
            Dim V As Double = aXs.Y
            Dim W As Double = aXs.Z

            Dim c1 As Double = u * iX + V * iY + W * iZ
            Dim C2 As Double = Math.Sqrt(u ^ 2 + V ^ 2 + W ^ 2)

            Dim denom As Double = u ^ 2 + V ^ 2 + W ^ 2
            If denom = 0 Then Return _rVal
            'the X component
            Dim T1 As Double = A * (V ^ 2 + W ^ 2)
            Dim t2 As Double = u * (-B * V - C * W + c1)
            Dim t3 As Double = ((iX - A) * (V ^ 2 + W ^ 2) + u * (B * V + C * W - V * iY - W * iZ)) * Math.Cos(aAngle)
            Dim t4 As Double = C2 * (-C * V + B * W - W * iY + V * iZ) * Math.Sin(aAngle)
            Dim aOrd As Double = (T1 + t2 + t3 + t4) / denom

            If Math.Round(Math.Abs(v1.X - aOrd), 15) <> 0 Then _rVal = True
            v1.X = aOrd

            'the iY component
            T1 = B * (u ^ 2 + W ^ 2)
            t2 = V * (-A * u - C * W + c1)
            '    t3 = (-B * (u ^ 2 + W ^ 2) + V * (A * u + C * W - u * iX - W * iZ) + iY * (u ^ 2 + W ^ 2)) * Cos(aAngle)
            t3 = ((iY - B) * (u ^ 2 + W ^ 2) + V * (A * u + C * W - u * iX - W * iZ)) * Math.Cos(aAngle)
            t4 = C2 * (C * u - A * W + W * iX - u * iZ) * Math.Sin(aAngle)
            aOrd = (T1 + t2 + t3 + t4) / denom
            If Math.Round(Math.Abs(v1.Y - aOrd), 15) <> 0 Then _rVal = True

            v1.Y = aOrd
            'the iZ component
            T1 = C * (u ^ 2 + V ^ 2)
            t2 = W * (-A * u - B * V + c1)
            '    t3 = (-C * (u ^ 2 + V ^ 2) + W * (A * u + B * V - u * iX - V * iY) + iZ * (u ^ 2 + V ^ 2)) * Cos(aAngle)
            t3 = ((iZ - C) * (u ^ 2 + V ^ 2) + W * (A * u + B * V - u * iX - V * iY)) * Math.Cos(aAngle)
            t4 = C2 * (-B * u + A * V - V * iX + u * iY) * Math.Sin(aAngle)
            aOrd = (T1 + t2 + t3 + t4) / denom
            If Math.Round(Math.Abs(v1.Z - aOrd), 15) <> 0 Then _rVal = True
            v1.Z = aOrd
            X = v1.X : Y = v1.Y : Z = v1.Z
            Return _rVal
        End Function
        Public Function AngleTo(A As TVECTOR, Optional bReturnDegrees As Boolean = False) As Double
            '^returns the angle between this vector and the passed vectors in radians
            Dim denom As Double = Magnitude * A.Magnitude
            If denom = 0 Then Return 0

            Dim numer As Double = DotProduct(A)
            Dim _rVal As Double = numer / denom
            _rVal = Math.Acos(_rVal)

            If bReturnDegrees Then _rVal = _rVal * 180 / Math.PI
            Return _rVal
        End Function
        Public Function AngleTo(A As TVECTOR, aNormal As TVECTOR, Optional aPrecis As Integer = -1) As Double
            '#2the to vector
            '#3the normal (cross product of the two passed vectors)
            '^the angle between the two passed vectors in degrees
            '~if the known normal is passed then the counter clockwise angle between the vectors is returned (0 to 360)
            '~otherwise the angle is returned (0 to 180) and the normal is returned Dim denom As Double = Magnitude * A.Magnitude
            Return VectorAngle(Me, A, aNormal, aPrecis)
        End Function
        Public Function DirectionTo(A As TVECTOR, Optional bReturnInverse As Boolean = False) As TVECTOR
            Dim rDirectionIsNull As Boolean = False
            Dim rDistance As Double = 0.0
            Return DirectionTo(A, bReturnInverse, rDirectionIsNull, rDistance)
        End Function
        Public Function DirectionTo(A As TVECTOR, bReturnInverse As Boolean, ByRef rDistance As Double) As TVECTOR
            Dim rDirectionIsNull As Boolean = False
            Return DirectionTo(A, bReturnInverse, rDirectionIsNull, rDistance)
        End Function
        Public Function DirectionTo(A As TVECTOR, bReturnInverse As Boolean, ByRef rDirectionIsNull As Boolean) As TVECTOR
            Dim rDistance As Double = 0.0
            Return DirectionTo(A, bReturnInverse, rDirectionIsNull, rDistance)
        End Function
        Public Function DirectionTo(A As TVECTOR, ByRef rDistance As Double) As TVECTOR
            Dim rDirectionIsNull As Boolean = False
            Return DirectionTo(A, False, rDirectionIsNull, rDistance)
        End Function
        Public Function DirectionTo(A As TVECTOR, bReturnInverse As Boolean, ByRef rDirectionIsNull As Boolean, ByRef rDistance As Double) As TVECTOR
            Dim _rVal As TVECTOR = A - Me
            rDistance = _rVal.Magnitude
            rDirectionIsNull = rDistance = 0
            '_rVal = (A - _rVal).Normalized(rDirectionIsNull, rDistance)
            If rDistance = 0 Then
                _rVal.X = 1
                _rVal.Y = 0
                _rVal.Z = 0
            Else
                _rVal.X /= rDistance
                _rVal.Y /= rDistance
                _rVal.Z /= rDistance
            End If
            If Math.Round(_rVal.Squared, 8) <> 1 Then
                If _rVal.X <> 0 Then
                    If Math.Abs(_rVal.X) > 0 Then _rVal.X = 1 Else _rVal.X = -1
                ElseIf _rVal.Y <> 0 Then
                    If Math.Abs(_rVal.Y) > 0 Then _rVal.Y = 1 Else _rVal.Y = -1
                Else
                    If Math.Abs(_rVal.Z) > 0 Then _rVal.Z = 1 Else _rVal.Z = -1
                End If
            End If
            If bReturnInverse Then _rVal *= -1
            Return _rVal
        End Function
        Public Function DotProduct(A As TVECTOR) As Double
            Return X * A.X + Y * A.Y + Z * A.Z
        End Function
        Public Function CrossProduct(A As TVECTOR, Optional bNormalize As Boolean = False) As TVECTOR
            Dim _rVal As New TVECTOR(Me) With
            {
                .X = dxfMath.Determinant2By2(Y, Z, A.Y, A.Z),
                .Y = -dxfMath.Determinant2By2(X, Z, A.X, A.Z),
                .Z = dxfMath.Determinant2By2(X, Y, A.X, A.Y)
            }

            If bNormalize Then _rVal.Normalize()
            Return _rVal
        End Function
        Public Function DistanceTo(A As TVECTOR, Optional aPrecis As Integer = -1) As Double
            Return dxfProjections.DistanceTo(Me, A, aPrecis)
        End Function
        Public Function DistanceTo(aLine As TLINE, Optional aPrecis As Integer = -1) As Double
            Return dxfProjections.DistanceTo(Me, aLine, aPrecis)
        End Function
        Public Function DistanceTo(aLine As TLINE, aPrecis As Integer, ByRef rOrthoDir As TVECTOR) As Double
            rOrthoDir = TVECTOR.Zero
            If aLine.Length <= 0 Then Return 0
            Dim _rVal As Double
            Dim v1 As TVECTOR = dxfProjections.ToLine(Me, aLine, rOrthoDir, _rVal)
            If aPrecis >= 0 Then
                aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
                _rVal = Math.Round(_rVal, aPrecis)
            End If
            Return _rVal
        End Function
        Public Function DistanceTo(aPlane As TPLANE, Optional aPrecis As Integer = -1) As Double
            If TPLANE.IsNull(aPlane) Then Return 0
            Dim _rVal As Double
            Dim v1 As TVECTOR = dxfProjections.ToPlane(Me, aPlane, rDistance:=_rVal)
            If aPrecis >= 0 And _rVal <> 0 Then
                _rVal = Math.Round(_rVal, TVALUES.LimitedValue(aPrecis, 0, 15))
            End If
            Return _rVal
        End Function
        Public Shadows Function Equals(A As TVECTOR, Optional aPrecis As Integer = 4) As Boolean
            aPrecis = TVALUES.LimitedValue(aPrecis, -1, 15)
            If aPrecis >= 0 Then
                Return (Math.Round(A.X, aPrecis) = Math.Round(X, aPrecis)) And (Math.Round(A.Y, aPrecis) = Math.Round(Y, aPrecis)) And (Math.Round(A.Z, aPrecis) = Math.Round(Z, aPrecis))
            Else
                Return (A.X = X) And (A.Y = Y) And (A.Z = Z)
            End If
        End Function
        Public Shadows Function Equals(A As TVECTOR, bCompareInverse As Boolean, Optional aPrecis As Integer = 0) As Boolean
            Dim rIsInverseEqual As Boolean = False
            Return Equals(A, bCompareInverse, aPrecis, rIsInverseEqual)
        End Function
        Public Shadows Function Equals(A As TVECTOR, bCompareInverse As Boolean, aPrecis As Integer, ByRef rIsInverseEqual As Boolean) As Boolean
            rIsInverseEqual = False
            Dim _rVal As Boolean = Equals(A, aPrecis)
            If bCompareInverse And Not _rVal Then
                If Equals(A * -1, aPrecis) Then
                    _rVal = True
                    rIsInverseEqual = True
                End If
            End If
            Return _rVal
        End Function

        Public Function IsEqualTo(A As iVector, Optional aPrecis As Integer = 0) As Boolean
            Return Equals(New TVECTOR(A), aPrecis)

        End Function

        Public Function IsEqualTo(A As iVector, bCompareInverse As Boolean, aPrecis As Integer, ByRef rIsInverseEqual As Boolean) As Boolean

            Return Equals(New TVECTOR(A), bCompareInverse, aPrecis, rIsInverseEqual)

        End Function

        Public Function LiesOn(aPlane As TPLANE, Optional aFudgeFactor As Double = 0.001, Optional bUseWorldOrigin As Boolean = False) As Boolean
            Dim rDistance As Double = 0.0
            Return LiesOn(aPlane, aFudgeFactor, rDistance, bUseWorldOrigin)
        End Function
        Public Function LiesOn(aPlane As TPLANE, aFudgeFactor As Double, ByRef rDistance As Double, Optional bUseWorldOrigin As Boolean = False) As Boolean
            rDistance = 0
            If Not aPlane.DirectionsAreDefined Then Return False
            '#1the vector to test
            '#2the plane
            '#3a tolerance for the test
            '#4returns the distace of the vector to the plane
            '5flag to a plane equal to the passed plane but center on 0,0,0 for the test
            '^returns True if the perpendicular distance of the vector to the plane is less than or equal to the fudge factor
            aFudgeFactor = TVALUES.LimitedValue(Math.Abs(aFudgeFactor), 0.000001, 0.1, 0.001)
            Dim aPl As TPLANE = aPlane
            If bUseWorldOrigin Then
                aPl = New TPLANE(aPlane, dxfVector.Zero)

            End If
            rDistance = DistanceTo(aPl, -1)
            Return rDistance <= aFudgeFactor
        End Function
        Public Function Mirror(aLine As TLINE, Optional bSuppressCheck As Boolean = False) As Boolean
            Dim rDirection As New TVECTOR(0) : Dim rDistance As Double = 0.0
            Return Mirror(aLine, rDirection, rDistance, bSuppressCheck)
        End Function
        Public Function Mirror(aSP As TVECTOR, aEP As TVECTOR, Optional bSuppressCheck As Boolean = False) As Boolean
            Dim rDirection As New TVECTOR(0) : Dim rDistance As Double = 0.0
            Return Mirror(New TLINE(aSP, aEP), rDirection, rDistance, bSuppressCheck)
        End Function
        Public Function Mirror(aLine As TLINE, ByRef rDirection As TVECTOR, ByRef rDistance As Double, Optional bSuppressCheck As Boolean = False) As Boolean
            Dim _rVal As Boolean
            rDirection = TVECTOR.Zero
            rDistance = 0
            '#1the vector to mirror
            '#2the start pt of the mirror line
            '#3the end pt of the mirror line
            '#4the distance the vector moves
            '^mirrors the vector across the passed line
            Dim dst As Double
            Dim v2 As TVECTOR
            Dim v1 As New TVECTOR(X, Y, Z)
            If Not bSuppressCheck Then
                If aLine.Length < 0.00001 Then Return _rVal
            End If
            v2 = dxfProjections.ToLine(Me, aLine, rDirection, dst)
            If dst = 0 Then Return False
            rDistance = 2 * dst
            v1 += rDirection * rDistance
            X = v1.X : Y = v1.Y : Z = v1.Z
            Return True
        End Function
        Public Function Mirror(aLineObj As iLine) As Boolean
            Dim rDirection As New TVECTOR(0)
            Dim rDistance As Double = 0.0
            Return Mirror(aLineObj, rDirection, rDistance)
        End Function
        Public Function Mirror(aLineObj As iLine, ByRef rDirection As TVECTOR, ByRef rDistance As Double) As Boolean
            rDistance = 0
            rDirection = TVECTOR.Zero
            If aLineObj Is Nothing Then Return False

            Dim aLine As New TLINE(aLineObj)

            If aLine.Length <= 0 Then Return False
            Return Mirror(aLine, rDirection, rDistance)
        End Function
        Public Function Mirrored(aLineObj As iLine) As TVECTOR
            Dim rDirection As New TVECTOR(0)
            Dim rDistance As Double = 0.0
            Return Mirrored(aLineObj, rDirection, rDistance)
        End Function
        Public Function Mirrored(aLineObj As iLine, ByRef rDirection As TVECTOR, ByRef rDistance As Double) As TVECTOR
            Dim _rVal As New TVECTOR(Me)
            If aLineObj Is Nothing Then Return _rVal
            _rVal.Mirror(aLineObj, rDirection, rDistance)
            Return _rVal
        End Function
        Public Function Mirrored(aLine As TLINE, Optional bSuppressCheck As Boolean = False) As TVECTOR
            Dim rDirection As New TVECTOR(0)
            Dim rDistance As Double = 0.0
            Return Mirrored(aLine, rDirection, rDistance, bSuppressCheck)
        End Function
        Public Function Mirrored(aLine As TLINE, ByRef rDirection As TVECTOR, rDistance As Double, Optional bSuppressCheck As Boolean = False) As TVECTOR
            rDistance = 0
            rDirection = TVECTOR.Zero
            Dim _rVal As New TVECTOR(Me)
            _rVal.Mirror(aLine, rDirection, rDistance, bSuppressCheck)
            rDistance = 0
            '#1the vector to mirror
            '#2the start pt of the mirror line
            '#3the end pt of the mirror line
            '#4the distance the vector moves
            '^mirrors the vector across the passed line
            Return _rVal
        End Function
        Public Sub Normalize()
            Dim rVectorIsNull As Boolean = False
            Dim rLength As Double = 0.0
            Normalize(rVectorIsNull, rLength)
        End Sub
        Public Sub Normalize(ByRef rVectorIsNull As Boolean)
            Dim rLength As Double = 0.0
            Normalize(rVectorIsNull, rLength)
        End Sub
        Public Sub Normalize(ByRef rLength As Double)
            Dim rVectorIsNull As Boolean = False
            Normalize(rVectorIsNull, rLength)
        End Sub
        Public Sub Normalize(ByRef rVectorIsNull As Boolean, ByRef rLength As Double)
            TVECTOR.NormalizeOrdinates(X, Y, Z, rVectorIsNull, rLength)

        End Sub
        Public Function Normalized() As TVECTOR
            Dim rVectorIsNull As Boolean = False
            Dim rLength As Double = 0.0
            Return Normalized(rVectorIsNull, rLength)
        End Function
        Public Function Normalized(ByRef rVectorIsNull As Boolean) As TVECTOR
            Dim rLength As Double = 0.0
            Return Normalized(rVectorIsNull, rLength)
        End Function
        Public Function Normalized(ByRef rVectorIsNull As Boolean, ByRef rLength As Double) As TVECTOR
            Dim _rVal As New TVECTOR(Me)
            _rVal.Normalize(rVectorIsNull, rLength)
            Return _rVal
        End Function



        Public Function ProjectTo(aPlane As TPLANE) As Boolean
            '#1the plane to project to
            '^moves the vector to the passed plane
            '~the direction is assumed as the normal of the plane
            Dim vnew As TVECTOR = dxfProjections.ToPlane(Me, aPlane)
            Return Update(vnew.X, vnew.Y, vnew.Z)
        End Function
        Public Function ProjectedTo(aPlane As TPLANE) As TVECTOR
            '#1the plane to project to
            '^returns a clone of the vector projected to the passed plane
            '~the direction is assumed as the normal of the plane

            Return dxfProjections.ToPlane(Me, aPlane)
        End Function

        Public Function ProjectTo(aPlane As TPLANE, aDirection As TVECTOR) As Boolean
            '#1the plane to project to
            '#2the direction to use for the projects
            '^moves the vector to the passed plane
            '~the direction is assumed as the normal of the plane if the passed direction is null
            Dim vnew As TVECTOR = dxfProjections.ToPlane(Me, aPlane, aDirection)
            Return Update(vnew.X, vnew.Y, vnew.Z)
        End Function



        Public Function ProjectedTo(aPlane As TPLANE, aDirection As TVECTOR) As TVECTOR
            '#1the plane to project to
            '#2the direction to use for the projects
            '^returns a clone of the vector projected to the passed plane
            '~the direction is assumed as the normal of the plane if the passed direction is null
            Return dxfProjections.ToPlane(Me, aPlane, aDirection)
        End Function


        Public Function ProjectTo(aLine As TLINE) As Boolean
            '#1the subject line
            '^projects this vector orthogonally onto the passed  line
            Dim newv As TVECTOR = dxfProjections.ToLine(Me, aLine)
            Return Update(newv.X, newv.Y, newv.Z)
        End Function

        Public Function ProjectedTo(aLine As TLINE) As TVECTOR
            Return dxfProjections.ToLine(Me, aLine)
        End Function

        Public Function SetCoordinates(Optional aX As Double? = Nothing, Optional aY As Double? = Nothing, Optional aZ As Double? = Nothing, Optional aPrecis As Integer = -1) As Boolean

            Dim newX As Double = IIf(aX.HasValue, aX.Value, X)
            Dim newY As Double = IIf(aY.HasValue, aY.Value, Y)
            Dim newZ As Double = IIf(aZ.HasValue, aZ.Value, Z)

            If aPrecis >= 0 Then
                aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
                newX = Math.Round(newX, aPrecis)
                newY = Math.Round(newY, aPrecis)
                newZ = Math.Round(newZ, aPrecis)
            End If

            Dim _rVal As Boolean = newX <> X Or newY <> Y Or newZ <> Z
            X = newX
            Y = newY
            Z = newZ
            Return _rVal
        End Function
        Public Function Scale(aScaler As TVECTOR, aReference As TVECTOR, Optional aPlane As dxfPlane = Nothing) As Boolean
            Dim rChanged As Boolean = False
            Return Scale(aScaler, aReference, aPlane, rChanged)
        End Function
        Public Function Scale(aScaler As TVECTOR, aReference As TVECTOR, aPlane As dxfPlane, ByRef rChanged As Boolean) As Boolean
            Return Scale(aScaler.X, aReference, aScaler.Y, aScaler.Z, aPlane)
        End Function
        Public Function Scale(aScaleX As Double, aReference As TVECTOR, Optional aScaleY As Double? = Nothing, Optional aScaleZ As Double? = Nothing, Optional aPlane As dxfPlane = Nothing) As Boolean
            Dim _rVal As Boolean
            '#2the factor to scale the entity by
            '#3the reference point to rescale the entities position with respect to
            '^rescales the Vector in space and dimension by the passed factors
            _rVal = False
            Dim scx As Double = aScaleX
            If scx = 0 Then scx = 1

            Dim scy As Double = scx
            Dim scz As Double = scx
            Dim cd As Byte = Code
            If aScaleY.HasValue Then
                If aScaleY.Value <> 0 Then scy = aScaleY.Value
            End If
            If aScaleZ.HasValue Then
                If aScaleZ.Value <> 0 Then scz = aScaleZ.Value
            End If
            If (scx = 1 And scy = 1 And scz = 1) Then Return _rVal
            If Me = aReference Then Return _rVal

            Dim aDir As TVECTOR
            Dim v1 As TVECTOR
            Dim d1 As Double
            If (scx = scy And scx = scz) Then
                'uniform scaling
                v1 = New TVECTOR(Me)
                aDir = aReference.DirectionTo(Me, False, d1)
                If d1 <> 0 Then
                    v1 = aReference + aDir * scx * d1
                    'X = aReference.X + aDir.X * d1 * scx
                    'Y = aReference.Y + aDir.Y * d1 * scx
                    'Z = aReference.Z + aDir.Z * d1 * scx
                End If
            Else
                Dim aPl As New TPLANE(aPlane) With {.Origin = aReference}
                v1 = WithRespectTo(aPl)
                v1 = aPl.Vector(v1.X * scx, v1.Y * scy, v1.Z * scz)

            End If

            _rVal = X <> v1.X Or Y <> v1.Y Or Z <> v1.Z
            X = v1.X
            Y = v1.Y
            Z = v1.Z
            Code = cd
            Return _rVal
        End Function
        Public Function Scaled(aScaleX As Double, aReference As TVECTOR, Optional aScaleY As Double = 0.0, Optional aScaleZ As Double = 0.0, Optional aPlane As dxfPlane = Nothing) As TVECTOR
            Dim rChanged As Boolean = False
            Return Scaled(aScaleX, aReference, aScaleY, aScaleZ, aPlane, rChanged)
        End Function
        Public Function Scaled(aScaleX As Double, aReference As TVECTOR, aScaleY As Double, aScaleZ As Double, aPlane As dxfPlane, ByRef rChanged As Boolean) As TVECTOR
            Dim _rVal As New TVECTOR(Me)
            rChanged = _rVal.Scale(aScaleX, aReference, aScaleY, aScaleZ, aPlane)
            Return _rVal
            '#2the factor to scale the entity by
            '#3the reference point to rescale the entities position with respect to
            '^rescales the Vector in space and dimension by the passed factors
        End Function
        Public Function Translated(aTranslation As TVECTOR, Optional aPlane As dxfPlane = Nothing) As TVECTOR
            Dim rChanged As Boolean = False
            Return Translated(aTranslation, aPlane, rChanged)
        End Function
        Public Function Translated(aTranslation As TVECTOR, aPlane As dxfPlane, ByRef rChanged As Boolean) As TVECTOR
            '^ adds the passed vector to this vector and returns the resulting vector
            '~if a plane is passed the translations is with respect to the planes axes
            Dim v1 As New TVECTOR(Me)
            rChanged = v1.Translate(aTranslation, aPlane)
            Return v1
        End Function
        Public Function Translate(aTranslation As TVECTOR, Optional aPlane As dxfPlane = Nothing) As Boolean
            '^ adds the passed vector to this vector
            '~if a plane is passed the translations is with respect to the planes axes
            Dim v1 As New TVECTOR(Me)
            Dim aPl As TPLANE
            If Not dxfPlane.IsNull(aPlane) Then
                v1 += aTranslation
            Else
                aPl = New TPLANE(aPlane)
                If aTranslation.X <> 0 Then v1 += aPl.XDirection * aTranslation.X
                If aTranslation.Y <> 0 Then v1 += aPl.YDirection * aTranslation.Y
                If aTranslation.Z <> 0 Then v1 += aPl.ZDirection * aTranslation.Z
            End If
            Dim _rVal = v1.X <> X Or v1.Y <> Y Or v1.Z <> Z
            X = v1.X : Y = v1.Y : Z = v1.Z
            Return _rVal
        End Function
        Public Function Update(Optional aNewX As Double? = Nothing, Optional aNewY As Double? = Nothing, Optional aNewZ As Double? = Nothing, Optional aPrecis As Integer = -1) As Boolean
            Dim v1 As New TVECTOR(X, Y, Z)
            aPrecis = TVALUES.LimitedValue(aPrecis, -1, 15)
            If aNewX.HasValue Then
                If aPrecis >= 0 Then v1.X = Math.Round(aNewX.Value, aPrecis) Else v1.X = aNewX.Value
            End If
            If aNewY.HasValue Then
                If aPrecis >= 0 Then v1.Y = Math.Round(aNewY.Value, aPrecis) Else v1.Y = aNewY.Value
            End If
            If aNewZ.HasValue Then
                If aPrecis >= 0 Then v1.Z = Math.Round(aNewZ.Value, aPrecis) Else v1.Z = aNewZ.Value
            End If
            Dim _rVal As Boolean = v1.X <> X Or v1.Y <> Y Or v1.Z <> Z
            X = v1.X
            Y = v1.Y
            Z = v1.Z
            Return _rVal
        End Function
        Public Function Updated(Optional aNewX As Double? = Nothing, Optional aNewY As Double? = Nothing, Optional aNewZ As Double? = Nothing, Optional aPrecis As Integer = -1) As TVECTOR
            Dim _rVal As New TVECTOR(Me)
            _rVal.Update(aNewX, aNewY, aNewZ, aPrecis)
            Return _rVal
        End Function
        Public Function WithRespectTo(aPlane As TPLANE, aXScale As Double, aYScale As Double, aZScale As Double, Optional aPrecis As Integer = 8, Optional aRotation As Double? = 0) As TVECTOR
            Dim _rVal As New TVECTOR(Me)
            If TPLANE.IsNull(aPlane) Then Return _rVal
            '#1the structure of the plane
            '#2 returns the distance away form the plane the source vector is
            '#3the number of decimals to round the returned vertices coordinates to
            '4flag to control if the depth is returned
            '#5a scale factor to apply 0 means no scaling)
            '^returns the coordinates of the passed vector with respect to the center and origin of the passed plane
            'If bGetDepth Then rDepth = DistanceTo(aPlane) Else rDepth = 0
            Dim v1 As TVECTOR
            Dim bFlag As Boolean
            Dim xAx As TLINE = aPlane.XAxis
            Dim yAx As TLINE = aPlane.YAxis
            Dim zAx As TLINE = aPlane.ZAxis
            Dim d1 As Double = 0
            Dim onseg As Boolean
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            'get the coordinates by projecting the vector to the lines
            'if the point on the line lies directionally before the start of the line
            'the coordinate is negative
            v1 = dxfProjections.ToLine(Me, xAx, d1, onseg, bFlag)
            _rVal.X = TVALUES.To_DBL(aPlane.Origin.DistanceTo(v1) * aXScale)
            If aPrecis <> 0 Then _rVal.X = Math.Round(_rVal.X, aPrecis)
            If Not bFlag Then _rVal.X *= -1
            v1 = dxfProjections.ToLine(Me, yAx, d1, onseg, bFlag)
            _rVal.Y = TVALUES.To_DBL(aPlane.Origin.DistanceTo(v1) * aYScale)
            If aPrecis <> 0 Then _rVal.Y = Math.Round(_rVal.Y, aPrecis)
            If Not bFlag Then _rVal.Y *= -1
            v1 = dxfProjections.ToLine(Me, zAx, d1, onseg, bFlag)
            _rVal.Z = TVALUES.To_DBL(aPlane.Origin.DistanceTo(v1) * aZScale)
            If aPrecis <> 0 Then _rVal.Z = Math.Round(_rVal.Z, aPrecis)
            If Not bFlag Then _rVal.Z *= -1
            If aRotation <> 0 Then _rVal.RotateAbout(zAx, aRotation)
            Return _rVal
        End Function
        Public Function WithRespectTo(aPlane As dxfPlane, Optional aPrecis As Integer = 8, Optional aScaleFactor As Double = 0.0, Optional bSuppressZ As Boolean = False) As TVECTOR
            Dim rDepth As Double
            Dim bGetDepth As Boolean = False
            If dxfPlane.IsNull(aPlane) Then Return New TVECTOR(Me)
            Return WithRespectTo(aPlane.Strukture, rDepth, aPrecis, bGetDepth, aScaleFactor, bSuppressZ)
        End Function

        Public Function WithRespectTo(aPlane As TPLANE, Optional aPrecis As Integer = 8, Optional aScaleFactor As Double = 0.0, Optional bSuppressZ As Boolean = False) As TVECTOR
            Dim rDepth As Double
            Dim bGetDepth As Boolean = False
            Return WithRespectTo(aPlane, rDepth, aPrecis, bGetDepth, aScaleFactor, bSuppressZ)
        End Function
        Public Function WithRespectTo(aPlane As TPLANE, ByRef rDepth As Double, Optional aPrecis As Integer = 8, Optional bGetDepth As Boolean = False, Optional aScaleFactor As Double = 0.0, Optional bSuppressZ As Boolean = False) As TVECTOR
            rDepth = 0
            Dim _rVal As New TVECTOR(Me)

            If TPLANE.IsNull(aPlane) Then Return _rVal
            '#1the structure of the plane
            '#2 returns the distance away form the plane the source vector is
            '#3the number of decimals to round the returned vertices coordinates to
            '4flag to control if the depth is returned
            '#5a scale factor to apply 0 means no scaling)
            '^returns the coordinates of the passed vector with respect to the center and origin of the passed plane
            If bGetDepth Then rDepth = DistanceTo(aPlane, -1) Else rDepth = 0
            Dim bFlag As Boolean
            Dim xAx As TLINE = aPlane.XAxis
            Dim yAx As TLINE = aPlane.YAxis
            Dim SF As Double
            Dim d1 As Double = 0
            Dim onseg As Boolean
            If aPrecis >= 0 Then aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            If aScaleFactor <> 0 Then
                If aPrecis >= 0 Then SF = Math.Round(aScaleFactor, aPrecis)
            Else
                SF = 1
            End If
            'get the coordinates by projecting the vector to the lines
            'if the point on the line lies directionally before the start of the line
            'the coordinate is negative
            Dim zval As Double = 0
            Dim negz As Boolean = False
            Dim p1 As TVECTOR = dxfProjections.ToPlane(Me, aPlane:=aPlane, Nothing, zval, negz)
            Dim v1 As TVECTOR = dxfProjections.ToLine(p1, xAx, d1, onseg, rDirectionPositive:=bFlag)
            _rVal.X = dxfProjections.DistanceTo(xAx.SPT, v1) * SF
            If aPrecis >= 0 Then _rVal.X = Math.Round(_rVal.X, aPrecis)
            If Not bFlag Then _rVal.X *= -1
            v1 = dxfProjections.ToLine(p1, yAx, d1, onseg, rDirectionPositive:=bFlag)
            _rVal.Y = dxfProjections.DistanceTo(yAx.SPT, v1) * SF
            If aPrecis >= 0 Then _rVal.Y = Math.Round(_rVal.Y, aPrecis)
            If Not bFlag Then _rVal.Y *= -1
            If Not bSuppressZ Then
                If negz Then
                    _rVal.Z = -zval
                Else
                    _rVal.Z = zval
                End If
                'v1 = dxfProjections.ToLine(Me, aPlane.ZAxis, d1, onseg, rDirectionPositive:=bFlag)
                '_rVal.Z = aPlane.Origin.DistanceTo(v1) * SF
                If aPrecis >= 0 Then _rVal.Z = Math.Round(_rVal.Z, aPrecis)
                If Not bFlag Then _rVal.Z *= -1
            Else
                _rVal.Z = 0
            End If
            Return _rVal
        End Function

        Public Function WithRespectTo(aCharBox As dxoCharBox, Optional aPrecis As Integer = 8, Optional aScaleFactor As Double = 0.0, Optional bSuppressZ As Boolean = False) As TVECTOR
            '#1the charbox to get the plane from
            '#2 returns the distance away form the plane the source vector is
            '#3the number of decimals to round the returned vertices coordinates to
            '4flag to control if the depth is returned
            '#5a scale factor to apply 0 means no scaling)
            '^returns the coordinates of the passed vector with respect to the center and origin of the passed plane
            If aCharBox Is Nothing Then Return Clone()
            Return WithRespectTo(aCharBox.Plane, aPrecis, aScaleFactor, bSuppressZ)
        End Function

        Public Function WithRespectTo(aCharBox As TCHARBOX, Optional aPrecis As Integer = 8, Optional aScaleFactor As Double = 0.0, Optional bSuppressZ As Boolean = False) As TVECTOR
            '#1the charbox to get the plane from
            '#2 returns the distance away form the plane the source vector is
            '#3the number of decimals to round the returned vertices coordinates to
            '4flag to control if the depth is returned
            '#5a scale factor to apply 0 means no scaling)
            '^returns the coordinates of the passed vector with respect to the center and origin of the passed plane
            Return WithRespectTo(aCharBox.Plane, aPrecis, aScaleFactor, bSuppressZ)
        End Function
        Public Function TransferedToPlane(aVectorPlane As TPLANE, aNewPlane As TPLANE, aXScale As Double, aYScale As Double, aZScale As Double, aRotation As Double) As TVECTOR
            Dim _rVal As New TVECTOR(Me)
            If TPLANE.IsNull(aVectorPlane) Or TPLANE.IsNull(aNewPlane) Then Return _rVal
            _rVal = _rVal.WithRespectTo(aVectorPlane, aXScale, aYScale, aZScale, aRotation:=aRotation)
            Return _rVal
        End Function
#End Region 'Methods
#Region "Shared Methods"
        Public Shared ReadOnly Property Zero As TVECTOR
            Get
                Return New TVECTOR(0, 0, 0)
            End Get
        End Property

        Public Shared Function SquareRoot(aX As Double, aY As Double, aZ As Double) As Double
            Try
                If aX = 0 And aY = 0 And aZ = 0 Then Return 0
                Dim _rVal As Double = Math.Sqrt(Math.Pow(aX, 2) + Math.Pow(aY, 2) + Math.Pow(aZ, 2))

                If Double.IsInfinity(_rVal) Then
                    _rVal = Double.MaxValue
                End If
                If Double.NaN = _rVal Then _rVal = Double.MaxValue
                Return _rVal
            Catch ex As Exception
                Console.WriteLine("OVERFLOW - TVECTOR.SquareRoot")
                Return Double.MaxValue
            End Try
        End Function
        Public Shared Function Square(aX As Double, aY As Double, aZ As Double) As Double
            Try
                If aX = 0 And aY = 0 And aZ = 0 Then Return 0
                Return Math.Pow(aX, 2) + Math.Pow(aY, 2) + Math.Pow(aZ, 2)
            Catch ex As Exception
                Console.WriteLine("OVERFLOW - TVECTOR.Square")
                Return Double.MaxValue
            End Try
        End Function
        Public Shared Function NormalizeOrdinates(ByRef ioX As Double, ByRef ioY As Double, ByRef ioZ As Double) As Boolean
            Return NormalizeOrdinates(ioX, ioY, ioZ, rVectorIsNull:=False, 0)
        End Function
        Public Shared Function NormalizeOrdinates(ByRef ioX As Double, ByRef ioY As Double, ByRef ioZ As Double, ByRef rVectorIsNull As Boolean) As Boolean
            Return NormalizeOrdinates(ioX, ioY, ioZ, rVectorIsNull:=rVectorIsNull, 0)
        End Function
        Public Shared Function NormalizeOrdinates(ByRef ioX As Double, ByRef ioY As Double, ByRef ioZ As Double, ByRef rVectorIsNull As Boolean, ByRef rLength As Double) As Boolean
            rLength = TVECTOR.SquareRoot(ioX, ioY, ioZ)
            rVectorIsNull = rLength <= 0.000001
            If rVectorIsNull Then
                ioX = 0
                ioY = 0
                ioZ = 0
                rLength = 0
                Return False
            End If
            If rLength = 1 Then
                Return True
            End If
            ioX /= rLength
            If Math.Abs(ioX) <> 1 Then
                ioY /= rLength
                If Math.Abs(ioY) <> 1 Then
                    ioZ /= rLength
                    If Math.Abs(ioZ) = 1 Then
                        ioX = 0
                        ioY = 0
                    End If
                Else
                    ioX = 0
                    ioZ = 0
                End If
            Else
                ioY = 0
                ioZ = 0
            End If
            Return True
        End Function
        Public Shared ReadOnly Property WorldX As TVECTOR
            Get
                Return New TVECTOR(1, 0, 0)
            End Get
        End Property

        Public Shared ReadOnly Property WorldY As TVECTOR
            Get
                Return New TVECTOR(0, 1, 0)
            End Get
        End Property

        Public Shared ReadOnly Property WorldZ As TVECTOR
            Get
                Return New TVECTOR(0, 0, 1)
            End Get
        End Property

        Public Shared Function ToDirection(A As TVECTOR) As TVECTOR
            Dim rVectorIsNull As Boolean
            Dim rLength As Double
            Return ToDirection(A, rVectorIsNull, rLength)
        End Function
        Public Shared Function ToDirection(A As TVECTOR, ByRef rVectorIsNull As Boolean) As TVECTOR
            Dim rLength As Double
            Return ToDirection(A, rVectorIsNull, rLength)
        End Function
        Public Shared Function ToDirection(A As TVECTOR, ByRef rVectorIsNull As Boolean, ByRef rLength As Double) As TVECTOR
            Dim _rVal As New TVECTOR(A)
            rLength = A.Magnitude
            rVectorIsNull = Math.Round(rLength, 8) = 0
            If rVectorIsNull Then Return _rVal

            rLength = A.Magnitude
            rVectorIsNull = rLength = 0
            If Not rVectorIsNull Then
                _rVal.X = A.X / rLength
                _rVal.Y = A.Y / rLength
                _rVal.Z = A.Z / rLength
            End If
            Return _rVal
        End Function
        Public Shared Function DefineByString(aCoordinateString As String, OldCoords As TVECTOR, Optional aDelimiter As Char = ",") As TVECTOR
            Dim rZSet As Boolean = False
            Dim rCoordCount As Integer = 0
            Return DefineByString(aCoordinateString, OldCoords, aDelimiter, rZSet, rCoordCount)
        End Function
        Public Shared Function DefineByString(aCoordinateString As String, OldCoords As TVECTOR, aDelimiter As Char, ByRef rZSet As Boolean, ByRef rCoordCount As Integer) As TVECTOR
            Dim _rVal As New TVECTOR(OldCoords)
            rZSet = False
            rCoordCount = 0

            If String.IsNullOrWhiteSpace(aCoordinateString) Then Return _rVal
            Dim aStr As String = aCoordinateString.Trim()
            Dim sVals() As String
            Dim sString As String = aStr.Replace("(", "").Trim
            sString = sString.Replace(")", "").Trim

            sVals = sString.Split(aDelimiter)

            If sVals.Count >= 1 Then
                aStr = sVals(0).Trim
                _rVal.X = TVALUES.To_DBL(aStr)
                rCoordCount += 1
            End If
            If sVals.Count >= 2 Then
                aStr = sVals(1).Trim
                _rVal.Y = TVALUES.To_DBL(aStr)
                rCoordCount += 1
            End If
            If sVals.Count >= 3 Then
                aStr = sVals(2).Trim
                rZSet = True
                _rVal.Z = TVALUES.To_DBL(aStr)
                rCoordCount += 1
            End If
            Return _rVal
        End Function
        Public Shared Function Reflect(A As TVECTOR, B As TVECTOR) As TVECTOR
            Dim _rVal As TVECTOR
            '^returns reflection of A off of B
            If A.AngleTo(B) < 0 Then
                _rVal = A + A.ComponentAlong(B) * -2
            Else
                _rVal = A
            End If
            Return _rVal
        End Function
        Public Shared Function Orthogonal(A As TVECTOR, B As TVECTOR, ByRef rAlong As TVECTOR) As TVECTOR
            '^returns vector component of A orthogonal to B
            rAlong = A.ComponentAlong(B)
            Return (A - rAlong)
        End Function
        Public Shared Function Random() As TVECTOR
            '^returns  a random vector
            Return New TVECTOR(2 * Rnd() - 1, 2 * Rnd() - 1, 2 * Rnd() - 1)
        End Function
        Public Shared Function TransformToPlane(A As TVECTOR, ByRef FromCS As dxfPlane, ByRef tOCS As dxfPlane) As TVECTOR

            Dim v1 As New TVECTOR(A)
            If FromCS Is Nothing And tOCS Is Nothing Then Return v1
            Dim aCS As dxfPlane
            Dim bCS As dxfPlane
            Dim sp As TVECTOR
            Dim ep As TVECTOR
            Dim ip As TVECTOR
            Dim v2 As TVECTOR
            Dim wX As TVECTOR = TVECTOR.WorldX
            Dim wY As TVECTOR = TVECTOR.WorldY
            Dim wZ As TVECTOR = TVECTOR.WorldZ
            Dim d1 As Double
            Dim d2 As Double
            Dim sInv As Double
            Dim bFlag As Boolean
            Dim dst As Double = 0
            If FromCS Is Nothing Then aCS = New dxfPlane Else aCS = FromCS
            If tOCS Is Nothing Then bCS = New dxfPlane Else bCS = tOCS
            If Not aCS.IsWorld Then
                v2 = New TVECTOR(v1)
                sp = aCS.OriginV
                d1 = 2 * v1.DistanceTo(sp)
                ep = sp + (wX * d1)

                ip = dxfProjections.ToLine(v1, New TLINE(sp, ep), rDistance:=dst, rPointIsOnSegment:=bFlag)
                d2 = sp.DistanceTo(ip)
                If Not bFlag Then sInv = -1 Else sInv = 1
                v2.X = TVALUES.To_DBL(sp.X + sInv * d2)
                ep = sp + (wY * d1)
                ip = dxfProjections.ToLine(v1, New TLINE(sp, ep), rDistance:=dst, rPointIsOnSegment:=bFlag)
                d2 = sp.DistanceTo(ip)
                If Not bFlag Then sInv = -1 Else sInv = 1
                v2.Y = TVALUES.To_DBL(sp.Y + sInv * d2)
                ep = sp + (wZ * d1)
                ip = dxfProjections.ToLine(v1, New TLINE(sp, ep), rDistance:=dst, rPointIsOnSegment:=bFlag)
                d2 = sp.DistanceTo(ip)
                If Not bFlag Then sInv = -1 Else sInv = 1
                v2.Z = TVALUES.To_DBL(sp.Z + sInv * d2)
                v1 = v2
            End If
            If Not bCS.IsWorld Then
                wX = bCS.XDirectionV
                wY = bCS.YDirectionV
                wZ = bCS.ZDirectionV
                v2 = New TVECTOR(v1)
                sp = bCS.OriginV
                d1 = 2 * v1.DistanceTo(sp)
                ep = sp + (wX * d1)
                ip = dxfProjections.ToLine(v1, New TLINE(sp, ep), rDistance:=dst, rPointIsOnSegment:=bFlag)
                d2 = sp.DistanceTo(ip)
                If Not bFlag Then sInv = -1 Else sInv = 1
                v2.X = TVALUES.To_DBL(sp.X + sInv * d2)
                ep = sp + (wY * d1)
                ip = dxfProjections.ToLine(v1, New TLINE(sp, ep), rDistance:=dst, rPointIsOnSegment:=bFlag)
                d2 = sp.DistanceTo(ip)
                If Not bFlag Then sInv = -1 Else sInv = 1
                v2.Y = TVALUES.To_DBL(sp.Y + sInv * d2)
                ep = sp + (wZ * d1)
                ip = dxfProjections.ToLine(v1, New TLINE(sp, ep), rDistance:=dst, rPointIsOnSegment:=bFlag)
                d2 = sp.DistanceTo(ip)
                If Not bFlag Then sInv = -1 Else sInv = 1
                v2.Z = TVALUES.To_DBL(sp.Z + sInv * d2)
                v1 = v2
            End If
            Return v1
        End Function
        Public Shared Function VectorAngle(aVector As TVECTOR, bVector As TVECTOR, ByRef ioNormal As TVECTOR, Optional aPrecis As Integer = -1) As Double
            Dim _rVal As Double
            '#1the from vector
            '#2the to vector
            '#3the normal (cross product of the two passed vectors)
            '^the angle between the two passed vectors in degrees
            '~if the known normal is passed then the counter clockwise angle between the vectors is returned (0 to 360)
            '~otherwise the angle is returned (0 to 180) and the normal is returned
            Dim ang1 As Double
            Dim aN As TVECTOR
            'Dim bN As TVECTOR
            Dim p0 As TVECTOR = aVector
            Dim P1 As TVECTOR = bVector
            If TVECTOR.IsNull(p0) Then p0 = TVECTOR.WorldX
            If TVECTOR.IsNull(P1) Then P1 = New TVECTOR(-1, 0, 0)
            p0.Normalize()
            P1.Normalize()
            If Math.Round((p0.X - P1.X), 2) = 0 And Math.Round((p0.Y - P1.Y), 2) = 0 And Math.Round((p0.Z - P1.Z), 2) = 0 Then
                'same direction
                '        ioNormal = NEW TVECTOR(0, 0, 1)
                _rVal = 0
            ElseIf Math.Round((p0.X + P1.X), 2) = 0 And Math.Round((p0.Y + P1.Y), 2) = 0 And Math.Round((p0.Z + P1.Z), 2) = 0 Then
                'opposite direction
                '        ioNormal = NEW TVECTOR(0, 0, 1)
                _rVal = 180
            Else
                aN = p0.CrossProduct(P1, True)
                ang1 = p0.AngleTo(P1, bReturnDegrees:=True)
                If ioNormal.IsDefined(3) Then
                    'aN *= -1
                    If Not ioNormal.Equals(aN, 2) Then
                        ang1 = 360 - ang1
                    End If
                Else
                    If aN.Z < 0 Then
                        aN *= -1
                        ang1 = 360 - ang1
                    End If
                    ioNormal = aN
                End If
                _rVal = Math.Round(ang1, 6)
            End If
            aPrecis = TVALUES.LimitedValue(aPrecis, -1, 15)
            If aPrecis >= 0 Then
                _rVal = Math.Round(_rVal, aPrecis)
            End If
            Return _rVal
        End Function



        Public Shared Function FromString(aString As String) As TVECTOR
            Dim _rVal As TVECTOR = Zero
            If String.IsNullOrWhiteSpace(aString) Then Return _rVal
            Dim aStr = aString.Trim()
            Dim cVals As TVALUES = TLISTS.ToValues(aStr, ",", False, True, False, True, bRemoveParens:=True)
            If cVals.Count > 0 Then _rVal.X = TVALUES.To_DBL(cVals.Member(0))
            If cVals.Count > 1 Then _rVal.Y = TVALUES.To_DBL(cVals.Member(1))
            If cVals.Count > 2 Then _rVal.Z = TVALUES.To_DBL(cVals.Member(2))
            Return _rVal
        End Function
        Public Shared Function FromObject(aSource As Object, Optional aProjectionPlane As dxfPlane = Nothing, Optional aProjectDirection As dxfDirection = Nothing, Optional aVector As dxfVector = Nothing, Optional aChangeX As Double = 0.0, Optional aChangeY As Double = 0.0, Optional aChangeZ As Double = 0.0) As TVECTOR
            '#1the object with X, Y , Z properties to move to
            '^returns a vector whose coordinates are taken from the passed Object
            '~the passed object must have a numeric X and Y property. Z is optional
            Dim v1 As TVECTOR = Zero

            Dim tname As String = TypeName(aSource).ToUpper()
            Dim aPl As dxfPlane
            Dim aImg As dxfImage
            Dim vars As New TVERTEXVARS(aVector)

            If aSource IsNot Nothing Then
                Try
                    Select Case tname
                        Case "TVECTOR"
                            v1 = DirectCast(aSource, TVECTOR)
                        Case "TVERTEX"
                            Dim vt As TVERTEX = aSource
                            v1 = vt.Vector
                            If aVector Is Nothing Then vars = vt.Vars
                        Case "DXFVECTOR"
                            Dim P1 As dxfVector = aSource
                            v1 = P1.Strukture
                            If aVector Is Nothing Then vars = P1.Vars
                        Case "TPOINT"
                            Dim P1 As TPOINT = aSource
                            v1.SetCoordinates(P1.X, P1.Y, 0, -1)

                        Case "STRING"
                            v1 = TVECTOR.FromString(aSource)
                        Case "DXFDIRECTION"
                            Dim P2 As dxfDirection = aSource
                            v1 = P2.Strukture
                        Case "DXFPLANE"
                            aPl = aSource
                            v1 = aPl.OriginV
                        Case "DXFRECTANGLE"
                            Dim aRect As dxfRectangle = aSource
                            v1 = aRect.CenterV

                        Case "COLDXFVECTORS"
                            Dim Ps As colDXFVectors = aSource
                            Dim vt As TVERTEX = Ps.ItemVertex(1, True)
                            v1 = vt.Vector
                            If aVector Is Nothing Then vars = vt.Vars

                        Case "DXFIMAGE"
                            aImg = DirectCast(aSource, dxfImage)
                            aPl = aImg.UCS
                            v1 = aPl.OriginV
                        Case Else
                            If dxfUtils.CheckProperty(aSource, "X") Then
                                Try
                                    v1.X = TVALUES.ToDouble(aSource.X)
                                Catch ex As Exception
                                    v1.X = 0
                                End Try
                            End If
                            If dxfUtils.CheckProperty(aSource, "Y") Then
                                Try
                                    v1.Y = TVALUES.ToDouble(aSource.Y)
                                Catch ex As Exception
                                    v1.Y = 0
                                End Try
                            End If
                            If dxfUtils.CheckProperty(aSource, "Z") Then
                                Try
                                    v1.Z = TVALUES.ToDouble(aSource.Z)
                                Catch ex As Exception
                                    v1.Z = 0
                                End Try
                            End If
                    End Select
                Catch ex As Exception
                    v1 = TVECTOR.Zero
                End Try
            End If
            If aChangeX <> 0 Or aChangeY <> 0 Or aChangeZ <> 0 Then v1 += New TVECTOR(aChangeX, aChangeY, aChangeZ)
            If aProjectionPlane IsNot Nothing Then
                v1 = dxfProjections.ToPlane(v1, aProjectionPlane, aProjectDirection)
            End If
            If aVector IsNot Nothing Then
                aVector.Strukture = v1
                aVector.Vars = New TVERTEXVARS(vars)
            End If
            Return v1
        End Function

        Public Shared Function IsNull(aVector As TVECTOR, Optional aPrecis As Integer = 0) As Boolean

            If aPrecis <= 0 Then
                Return aVector.X = 0 And aVector.Y = 0 And aVector.Z = 0
            Else
                aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
                Return Math.Round(aVector.X, aPrecis) = 0 And Math.Round(aVector.Y, aPrecis) = 0 And Math.Round(aVector.Z, aPrecis) = 0
            End If

        End Function

        Public Shared Function IsNull(aVector As iVector, Optional aPrecis As Integer = 0) As Boolean
            If aVector Is Nothing Then Return True
            If aPrecis <= 0 Then
                Return aVector.X = 0 And aVector.Y = 0 And aVector.Z = 0
            Else
                aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
                Return Math.Round(aVector.X, aPrecis) = 0 And Math.Round(aVector.Y, aPrecis) = 0 And Math.Round(aVector.Z, aPrecis) = 0
            End If

        End Function

#End Region 'Shared Methods
#Region "Operators"
        Public Shared Operator +(A As TVECTOR, B As TVECTOR) As TVECTOR
            Return New TVECTOR(A.X + B.X, A.Y + B.Y, A.Z + B.Z) With {.Code = A.Code, .Rotation = A.Rotation}
        End Operator
        Public Shared Operator -(A As TVECTOR, B As TVECTOR) As TVECTOR
            Return New TVECTOR(A.X - B.X, A.Y - B.Y, A.Z - B.Z) With {.Code = A.Code, .Rotation = A.Rotation}
        End Operator
        Public Shared Operator *(A As TVECTOR, aScaler As Double) As TVECTOR
            Return New TVECTOR(A.X * aScaler, A.Y * aScaler, A.Z * aScaler) With {.Code = A.Code, .Rotation = A.Rotation}
        End Operator
        Public Shared Operator /(A As TVECTOR, aScaler As Double) As TVECTOR
            Return New TVECTOR(A.X / aScaler, A.Y / aScaler, A.Z / aScaler) With {.Code = A.Code, .Rotation = A.Rotation}
        End Operator
        Public Shared Operator *(A As TVECTOR, B As TVECTOR) As TVECTOR
            Return New TVECTOR(A.X * B.X, A.Y * B.Y, A.Z * B.Z) With {.Code = A.Code, .Rotation = A.Rotation}
        End Operator
        Public Shared Operator =(A As TVECTOR, B As TVECTOR) As Boolean
            Return (A.X = B.X) And (A.Y = B.Y) And (A.Z = B.Z)
        End Operator
        Public Shared Operator <>(A As TVECTOR, B As TVECTOR) As Boolean
            Return (A.X <> B.X) Or (A.Y <> B.Y) Or (A.Z <> B.Z)
        End Operator


        Public Shared Widening Operator CType(aVector As TVECTOR) As dxfVector
            Return New dxfVector(aVector)
        End Operator
        Public Shared Widening Operator CType(aVector As TVECTOR) As TVECTRIX
            Return New TVECTRIX(aVector.X, aVector.Y, aVector.Z, 1)
        End Operator
        Public Shared Widening Operator CType(aVector As TVECTOR) As TPOINT
            Return New TPOINT(aVector.X, aVector.Y, aVector.Code)
        End Operator
        Public Shared Widening Operator CType(aVector As TVECTOR) As dxoPoint
            Return New dxoPoint(aVector.X, aVector.Y, aVector.Code)
        End Operator

        Public Shared Widening Operator CType(aVector As TVECTOR) As TVERTEX
            Return New TVERTEX(aVector, New TVERTEXVARS(-1))
        End Operator
        Public Shared Widening Operator CType(aVector As TVECTOR) As System.Drawing.Point
            Return New System.Drawing.Point(TVALUES.To_INT(aVector.X), TVALUES.To_INT(aVector.Y))
        End Operator
        Public Shared Widening Operator CType(aVector As TVECTOR) As System.Drawing.PointF
            Return New System.Drawing.PointF(TVALUES.To_SNG(aVector.X), TVALUES.To_SNG(aVector.Y))
        End Operator
        Public Shared Widening Operator CType(aVector As TVECTOR) As dxfDirection
            Return New dxfDirection(aVector)
        End Operator


#End Region 'Operators
    End Structure 'TVECTOR
    Friend Structure TVECTORS
        Implements ICloneable
#Region "Members"
        Private _Init As Boolean
        Private _Members() As TVECTOR
#End Region 'Members
#Region "Constructors"
        Public Sub New(Optional aCount As Integer = 0)
            'init --------------------------------------
            _Init = True
            ReDim _Members(-1)
            'init --------------------------------------
            For i As Integer = 1 To aCount
                Add(TVECTOR.Zero)
            Next i
        End Sub
        Public Sub New(aVectors As TVECTORS)
            'init --------------------------------------
            _Init = True
            ReDim _Members(-1)
            'init --------------------------------------
            If aVectors.Count <= 0 Then Return
            _Members = aVectors._Members.Clone()

        End Sub

        Public Sub New(aVectors As IEnumerable(Of iVector))
            'init -----------------------
            _Init = True
            ReDim _Members(-1)
            'init -----------------------
            If aVectors Is Nothing Then Return
            For Each v1 As iVector In aVectors
                If v1 IsNot Nothing Then Add(New TVECTOR(v1))
            Next

        End Sub
        Public Sub New(aVector As TVECTOR)
            'init --------------------------------------
            _Init = True
            ReDim _Members(-1)
            'init --------------------------------------
            Add(aVector)
        End Sub
        Public Sub New(aVector As TVECTOR, bVector As TVECTOR)
            'init --------------------------------------
            _Init = True
            ReDim _Members(-1)
            'init --------------------------------------
            Add(aVector)
            Add(bVector)
        End Sub
        Public Sub New(aVector As TVECTOR, bVector As TVECTOR, cVector As TVECTOR)
            'init --------------------------------------
            _Init = True
            ReDim _Members(-1)
            'init --------------------------------------
            Add(aVector)
            Add(bVector)
            Add(cVector)
        End Sub
        Public Sub New(aVector As TVECTOR, bVector As TVECTOR, cVector As TVECTOR, dVector As TVECTOR)
            'init --------------------------------------
            _Init = True
            ReDim _Members(-1)
            'init --------------------------------------
            Add(aVector)
            Add(bVector)
            Add(cVector)
            Add(dVector)
        End Sub
#End Region 'Constructors
#Region "Properties"
        Public ReadOnly Property Count As Integer
            Get
                If Not _Init Then
                    _Init = True
                    ReDim _Members(-1)
                End If
                Return _Members.Count
            End Get
        End Property

        Public ReadOnly Property Last As TVECTOR
            Get
                If Count > 0 Then
                    Return Item(Count)
                Else
                    Return TVECTOR.Zero
                End If
            End Get
        End Property
        Public Function LengthSummation(Optional bClosed As Boolean = False) As Double
            '#1flag to include the distance from the last to the first
            '^returns the total of the distance between each member
            '~ (1 to 2) + (2 to 3) + (3 to 4) etc.
            If Count <= 1 Then Return 0
            Dim _rVal As Double
            For i As Integer = 1 To Count - 1
                _rVal += Item(i).DistanceTo(Item(i + 1)) ' .Strukture.DistanceTo(v2.Strukture)
            Next i
            If bClosed And Count > 2 Then
                _rVal += Item(Count).DistanceTo(Item(1))
            End If
            Return _rVal

        End Function




#End Region 'Properties
#Region "Methods"

        Public Function Rotation(aIndex As Integer) As Double
            '^returns a member (base 1)
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return 0
            End If
            Return _Members(aIndex - 1).Rotation


        End Function

        Public Sub SetRotation(Value As Double, aIndex As Integer)
            '^returns a member (base 1)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1).Rotation = Value
        End Sub


        Public Function X(aIndex As Integer) As Double
            '^returns a member (base 1)

            If aIndex < 1 Or aIndex > Count Then Return 0
            Return _Members(aIndex - 1).X

        End Function

        Public Function Y(aIndex As Integer) As Double
            '^returns a member (base 1)

            If aIndex < 1 Or aIndex > Count Then Return 0
            Return _Members(aIndex - 1).Y

        End Function
        Public Function Z(aIndex As Integer) As Double
            '^returns a member (base 1)
            If aIndex < 1 Or aIndex > Count Then Return 0
            Return _Members(aIndex - 1).Z


        End Function
        Public Function Coordinates(Optional aPrecis As Integer = 0, Optional bSuppressParens As Boolean = False, Optional bSuppressZ As Boolean = False) As String

            '^a concantonated string of all the vector coordinates in the collection
            '~the delimitor is "¸" (char 184)
            Dim pt As TVECTOR
            Dim _rVal As String = String.Empty
            For i As Integer = 1 To Count
                pt = Item(i)
                If _rVal <> "" Then _rVal += dxfGlobals.Delim
                _rVal += pt.Coordinates(aPrecis, bSuppressParens, bSuppressZ)
            Next i
            Return _rVal

            'Set(value As String)
            '    '^a concantonated string of all the vector coordinates in the collection
            '    '~descriptors differ from coordinates in that they include more information abount the vector
            '    '~the delimitor is "¸" (char 184)
            '    'On Error Resume Next
            '    Clear()
            '    Dim sVals() As String
            '    sVals =value.Split( dxfGlobals.Delim)
            '    Dim i As Integer
            '    Dim P1 As TVECTOR
            '    For i = 0 To sVals.Length -1
            '        P1 = TVECTOR.DefineByString(sVals(i))
            '        Add(P1)
            '    Next i
            'End Set
        End Function

        Public Function CoordinatesP(Optional aPrecis As Integer = 3, Optional bIndexed As Boolean = False) As String

            '^a concantonated string of all the vector coordinates in the collection
            '~the delimitor is "¸" (char 184) and the vector ordinates are round to the passed precision
            '~the coordinates are augmented with the name of the vectors vertex style
            Dim v1 As TVECTOR
            Dim _rVal As String = String.Empty
            For i As Integer = 1 To Count
                v1 = Item(i)
                If Not bIndexed Then
                    If _rVal <> "" Then _rVal += dxfGlobals.Delim
                    _rVal += v1.CoordinatesP(aPrecis)
                Else
                    If _rVal <> "" Then _rVal += vbLf
                    _rVal += i & " - " & v1.CoordinatesP(aPrecis)
                End If
            Next i
            Return _rVal

        End Function
        Public Function Item(aIndex As Integer, Optional bSuppressIndexErr As Boolean = False) As TVECTOR
            '^returns a member (base 1)
            If aIndex < 1 Or aIndex > Count Then
                If bSuppressIndexErr Then Return TVECTOR.Zero
                Throw New IndexOutOfRangeException()
            End If
            Return _Members(aIndex - 1)
        End Function
        Public Sub SetItem(aIndex As Integer, value As TVECTOR)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1) = value
        End Sub
        Public Overrides Function ToString() As String
            Return $"TVECTORS[{ Count}]"
        End Function
        Public Function SubSet(aStartID As Integer, aEndID As Integer, Optional bRemoveSubset As Boolean = False, Optional bReturnClones As Boolean = False) As TVECTORS
            Dim _rVal As New TVECTORS(0)
            If Count <= 0 Then Return _rVal
            Dim si As Integer
            Dim ei As Integer
            dxfUtils.LoopIndices(Count, aStartID, aEndID, si, ei)
            For i As Integer = si To ei
                If bReturnClones Then _rVal.Add(New TVECTOR(Item(i))) Else _rVal.Add(Item(i))
            Next
            If bRemoveSubset Then
                For i As Integer = ei To si Step -1
                    Remove(i)
                Next
            End If
            Return _rVal
        End Function
        Public Function AreaSummation(Optional aPlane As dxfPlane = Nothing, Optional aCollector As colDXFVectors = Nothing) As Double
            If Count() <= 0 Then Return 0
            '#1the plane to use. World By default
            '#2 if not nothing returns the member vectors projected to the subject plane.
            '^returns the 2D area summation of all the vectors in the collection
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim planarvecs As New colDXFVectors
            Dim aPln As New TPLANE(aPlane)
            Dim _rVal As Double = 0
            For i As Integer = 1 To Count
                v1 = Item(i)
                If i < Count Then v2 = Item(i + 1) Else v2 = Item(1)
                v1 = v1.WithRespectTo(aPln)
                v2 = v2.WithRespectTo(aPln)
                _rVal += Math.Abs(v1.X * v2.Y - v2.X * v1.Y)
                planarvecs.AddV(v1)
            Next i
            _rVal = 0.5 * _rVal
            Return _rVal
            If aCollector IsNot Nothing Then aCollector.Append(planarvecs)
        End Function
        Public Function RemoveCoincidentVectors(Optional aPrecis As Integer = 4) As TVECTORS
            Dim _rVal As New TVECTORS(0)
            '^removes and returns the vectors that occur more than once
            Dim cnt As Integer
            Dim P1 As TVECTOR
            Dim P2 As TVECTOR
            Dim d1 As Double
            Dim keep As Boolean
            Dim newCol As New TVECTORS(0)
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            cnt = Count
            If cnt <= 1 Then Return _rVal
            newCol.Add(New TVECTOR(Item(1)))
            For i As Integer = 2 To cnt
                P1 = Item(i)
                keep = True
                For j As Integer = 1 To newCol.Count
                    P2 = newCol.Item(j)
                    d1 = P1.DistanceTo(P2, aPrecis)
                    If d1 <= 0.00000001 Then
                        keep = False
                        Exit For
                    End If
                Next j
                If keep Then
                    newCol.Add(P1)
                Else
                    _rVal.Add(P1)
                End If
            Next i
            _Members = newCol._Members
            Return _rVal
        End Function
        Public Sub SetCode(aIndex As Integer, aCode As dxxVertexStyles)
            If aIndex > 0 And aIndex <= Count Then
                _Members(aIndex - 1).Code = aCode
            End If
        End Sub
        Public Sub Translate(aTranslation As TVECTOR, aPlane As dxfPlane, Optional aStartID As Integer = -1, Optional aEndID As Integer = -1)
            If Count <= 0 Then Return
            Dim bPlanar As Boolean = Not dxfPlane.IsNull(aPlane)
            Dim aPl As TPLANE = IIf(bPlanar, New TPLANE(aPlane), TPLANE.World)
            Dim si As Integer
            Dim ei As Integer
            dxfUtils.LoopIndices(Count, aStartID, aEndID, si, ei)
            For i As Integer = si To ei
                If Not bPlanar Then
                    _Members(i - 1) += aTranslation
                Else
                    If aTranslation.X <> 0 Then _Members(i - 1) += aPl.XDirection * aTranslation.X
                    If aTranslation.Y <> 0 Then _Members(i - 1) += aPl.YDirection * aTranslation.Y
                    If aTranslation.Z <> 0 Then _Members(i - 1) += aPl.ZDirection * aTranslation.Z
                End If
            Next i
        End Sub

        Public Sub Translate(aTranslation As TVECTOR, Optional aStartID As Integer = -1, Optional aEndID As Integer = -1)
            Dim si As Integer
            Dim ei As Integer
            If Not dxfUtils.LoopIndices(Count, aStartID, aEndID, si, ei) Then Return
            For i As Integer = si To ei
                _Members(i - 1) += aTranslation
            Next i
        End Sub
        Public Function Transform(aTransform As TTRANSFORM, Optional aStartID As Integer = -1, Optional aEndID As Integer = -1) As Boolean
            Dim _rVal As Boolean = False
            Dim si As Integer
            Dim ei As Integer
            If Not dxfUtils.LoopIndices(Count, aStartID, aEndID, si, ei) Then Return False
            For i As Integer = si To ei
                If TTRANSFORM.Apply(aTransform, _Members(i - 1)) Then _rVal = True
            Next i
            Return _rVal
        End Function
        Public Sub Update(aIndex As Integer, aMember As TVECTOR, Optional aCode As Byte? = Nothing, Optional bClone As Boolean = True)
            If aIndex > 0 And aIndex <= Count Then
                If bClone Then
                    _Members(aIndex - 1) = New TVECTOR(aMember)
                Else
                    _Members(aIndex - 1) = aMember
                End If
                If aCode.HasValue Then
                    _Members(aIndex - 1).Code = aCode
                End If
            End If
        End Sub
        Public Sub Add(aVector As dxfVector, Optional aCode As Byte? = Nothing, Optional aRotation As Double? = Nothing)
            If aVector IsNot Nothing Then Add(New TVECTOR(aVector), aCode, aRotation)
        End Sub
        Public Sub Add(Optional aX As Double = 0, Optional aY As Double = 0, Optional aZ As Double = 0, Optional aCode As Byte? = Nothing, Optional aRotation As Double? = 0)
            Add(New TVECTOR(aX, aY, aZ), aCode, aRotation)
        End Sub
        Public Sub Add(aVector As TVECTOR, Optional aCode As Byte? = Nothing, Optional aRotation As Double? = Nothing)
            If Count >= Integer.MaxValue Then Return
            System.Array.Resize(_Members, Count + 1)
            _Members(_Members.Count - 1) = New TVECTOR(aVector)
            If aCode.HasValue Then
                _Members(_Members.Count - 1).Code = aCode.Value
            End If
            If aRotation.HasValue Then
                _Members(_Members.Count - 1).Rotation = aRotation.Value
            End If
        End Sub
        Public Sub Add(aVertex As TVERTEX, Optional aCode As Byte? = Nothing, Optional aRotation As Double? = Nothing)
            Add(aVertex.X, aVertex.Y, aVertex.Z, aCode, aRotation)
        End Sub

        Public Sub Append(bVectors As colDXFVectors)
            'On Error Resume Next
            If bVectors Is Nothing Then Return
            If bVectors.Count <= 0 Then Return

            For Each v1 As dxfVector In bVectors
                Add(v1.Strukture)
            Next
        End Sub
        Public Sub Append(bVectors As TVECTORS, Optional aStartID As Integer = 0)
            'On Error Resume Next
            If bVectors.Count <= 0 Then Return

            Dim sid As Integer = aStartID
            If sid < 0 Then sid = 0
            If sid > bVectors.Count Then sid = bVectors.Count
            If sid = 0 Then
                If Count <= 0 Then
                    _Init = True
                    _Members = bVectors._Members.Clone
                Else
                    _Members = _Members.Concat(bVectors._Members).ToArray
                End If
            Else
                For i As Integer = sid To bVectors.Count
                    If Count >= Integer.MaxValue Then Exit For
                    System.Array.Resize(_Members, Count + 1)
                    _Members(_Members.Count - 1) = New TVECTOR(bVectors.Item(i))
                Next i
            End If
        End Sub
        Public Function AppendUnique(bVectors As TVECTORS, Optional aPrecis As Integer = 0) As Integer
            'On Error Resume Next
            Dim _rVal As Integer = 0
            If bVectors.Count <= 0 Then Return 0
            Dim i As Integer
            Dim bKeep As Boolean
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim j As Integer
            Dim d1 As Double
            If aPrecis < 0 Then aPrecis = 0
            If aPrecis > 10 Then aPrecis = 10
            For i = 1 To bVectors.Count
                v1 = bVectors.Item(i)
                bKeep = True
                For j = 1 To Count
                    v2 = Item(j)
                    d1 = Math.Round(v1.DistanceTo(v2), aPrecis)
                    If d1 <= 0.00000001 Then
                        bKeep = False
                        Exit For
                    End If
                Next j
                If bKeep Then
                    _rVal += 1
                    Add(v1)
                End If
            Next i
            Return _rVal
        End Function
        Public Function AreaSummation(aPlane As TPLANE, Optional bSuppressProjection As Boolean = False, Optional aPlaneCollector As colDXFVectors = Nothing) As Double
            Dim _rVal As Double
            '^returns the 2D area summation of all the vectors in the collection

            Dim v1 As TVECTOR
            Dim sumation As Double
            Dim v2 As TVECTOR
            If TPLANE.IsNull(aPlane) Then aPlane = TPLANE.World
            _rVal = 0
            If Count <= 1 Then Return _rVal
            'On Error Resume Next
            For i As Integer = 1 To Count
                v1 = Item(i)
                If i < Count Then v2 = Item(i + 1) Else v2 = Item(1)
                If Not bSuppressProjection Then
                    v1 = v1.ProjectedTo(aPlane)
                    v2 = v2.ProjectedTo(aPlane)
                End If
                v1 = v1.WithRespectTo(aPlane)
                v2 = v2.WithRespectTo(aPlane)
                sumation = (v1.X * v2.Y) - (v2.X * v1.Y)
                _rVal += sumation
                If aPlaneCollector IsNot Nothing Then aPlaneCollector.AddV(v1)
            Next i
            _rVal = 0.5 * Math.Abs(_rVal)
            Return _rVal
        End Function
        Public Sub AddLine(aVector As TVECTOR, bVector As TVECTOR, Optional bTestLast As Boolean = False, Optional aPrecis As Integer = -1)
            If Not bTestLast Or Count <= 0 Then
                AddTwo(aVector, bVector, TVALUES.ToByte(dxxVertexStyles.MOVETO), TVALUES.ToByte(dxxVertexStyles.LINETO))
            Else
                If Item(Count).DistanceTo(aVector, aPrecis) <= 0 Then
                    Add(bVector, TVALUES.ToByte(dxxVertexStyles.LINETO))
                Else
                    AddTwo(aVector, bVector, dxxVertexStyles.MOVETO, dxxVertexStyles.LINETO)
                End If
            End If
        End Sub
        Public Sub AddTwo(aVector As TVECTOR, bVector As TVECTOR, Optional aCode As Byte = 0, Optional bCode As Byte = 0, Optional bDontAddIfLastIsEqual As Boolean = False)
            If bDontAddIfLastIsEqual And Count > 0 Then
                If Item(Count).Equals(aVector, 5) Then
                    Add(bVector, bCode)
                    Return
                End If
            End If
            Add(aVector, aCode)
            Add(bVector, bCode)
        End Sub
        Public Function ConvexHull(Optional aPlane As dxfPlane = Nothing, Optional bOnBorder As Boolean = False) As TVECTORS
            Dim _rVal As New TVECTORS
            '#1the subject vertices
            '#2the subject plane
            '^returns the Convex Hull enclosing the points on the plane
            'On Error Resume Next
            Dim aPln As New TPLANE(aPlane)
            Dim P1 As TVECTOR
            Dim Ps As TVECTOR
            Dim Pl As TVECTOR
            Dim v1 As TVECTOR
            Dim i As Integer
            Dim aVecs As TVECTORS
            Dim bVecs As TVECTORS
            Dim StartIndex As Integer
            Dim LastIndex As Integer
            Dim Selected As Integer
            Dim Cross As Double
            Dim Dot1 As Double
            Dim Dot2 As Double
            bVecs = New TVECTORS

            aVecs = UniqueMembers(5)
            For i = 1 To aVecs.Count
                v1 = aVecs.Item(i)
                v1 = v1.WithRespectTo(aPln)
                v1.Z = 0
                aVecs.Update(i, v1, 0)
                '0 Code means the point has not yet been marked as in the hull
            Next i
            If aVecs.Count <= 3 Then
                bVecs = aVecs
            Else
                'get the left/top most point index
                aVecs.GetVector(dxxPointFilters.GetLeftTop, 0, Nothing, 3, rIndex:=StartIndex)
                LastIndex = StartIndex
                Do
                    Selected = 0
                    Pl = aVecs.Item(LastIndex)
                    For i = 1 To aVecs.Count
                        P1 = aVecs.Item(i)
                        If P1.Code = 0 And i <> LastIndex Then
                            If Selected = 0 Then
                                'no point has been selected yet so select this one
                                Selected = i
                            Else
                                Ps = aVecs.Item(Selected)
                                Cross = (Pl.X - P1.X) * (Ps.Y - P1.Y) - (Pl.Y - P1.Y) * (Ps.X - P1.X)  '= P1.CrossProduct(Ps)
                                '
                                If Cross = 0 Then
                                    'Since we want the points on the border, take the one closer to LastIndex
                                    Dot1 = (P1.X - Pl.X) * (P1.X - Pl.X) + (P1.Y - Pl.Y) * (P1.Y - Pl.Y) ' vecs_DotProduct(Pl, P1, P1) '
                                    Dot2 = (Ps.X - Pl.X) * (Ps.X - Pl.X) + (Ps.Y - Pl.Y) * (Ps.Y - Pl.Y) 'vecs_DotProduct(Pl, Ps, Ps) '
                                    '
                                    If bOnBorder Then
                                        If Dot1 < Dot2 Then
                                            Selected = i
                                        End If
                                        'Since we don't want the points on the border, take the one further from LastIndex
                                    Else
                                        If Dot1 > Dot2 Then
                                            Selected = i
                                        End If
                                    End If
                                ElseIf Cross < 0 Then
                                    'Ps is more counterclockwise
                                    Selected = i
                                End If
                            End If
                        End If
                    Next i
                    'set the last index to the last selected point
                    LastIndex = Selected
                    'mark the selected point as being in the hull
                    aVecs.SetCode(Selected, 1)
                    bVecs.Add(aVecs.Item(Selected))
                Loop While LastIndex <> StartIndex
            End If
            For i = 1 To bVecs.Count
                P1 = bVecs.Item(i)
                _rVal.Add(aPln.Vector(P1.X, P1.Y))
            Next i
            Return _rVal
        End Function
        Public Function Mirrored(aLine As TLINE) As TVECTORS
            Dim _rVal As New TVECTORS(0)
            If aLine.Length <= 0.00001 Then Return _rVal

            For i As Integer = 1 To Count
                Dim v1 As New TVECTOR(_Members(i - 1))
                v1.Mirror(aLine, bSuppressCheck:=True)
                _rVal.Add(v1)
            Next i
            Return _rVal
        End Function
        Public Function Nearest(aSearchV As TVECTOR, ByRef ioDirection As TVECTOR, ByRef rDistance As Double, Optional aTestDir As dxxLineDescripts = dxxLineDescripts.Normal) As TVECTOR
            rDistance = 0.0
            Dim rIndex As Integer = 0
            Return Nearest(aSearchV, ioDirection, aTestDir, rDistance, rIndex)
        End Function
        Public Function Nearest(aSearchV As TVECTOR, ByRef ioDirection As TVECTOR, Optional aTestDir As dxxLineDescripts = dxxLineDescripts.Normal) As TVECTOR
            Dim rDistance As Double = 0.0
            Dim rIndex As Integer = 0
            Return Nearest(aSearchV, ioDirection, aTestDir, rDistance, rIndex)
        End Function
        Public Function Nearest(aSearchV As TVECTOR, ByRef ioDirection As TVECTOR, aTestDir As dxxLineDescripts, ByRef rDistance As Double, ByRef rIndex As Integer) As TVECTOR
            rDistance = 0
            rIndex = 0
            Dim _rVal As TVECTOR = TVECTOR.Zero
            If Count <= 0 Then Return _rVal
            '#2the vector to compare to
            '#3if DEFINED the direction to use for testing
            '#4how to test the passed direction Normal means no test
            '#5returns the index of the returned vector. -1 means the passed vectors were empty or no vectors lie on the test direction.
            '#6flag to remove the found vector from the passed array
            '^returns the vector from the vector array which is the nearest to the passed vector
            Dim v1 As TVECTOR
            Dim d1 As Double
            Dim minval As Double
            Dim i As Integer
            Dim bTestDir As Boolean
            Dim tDir As TVECTOR
            Dim bFlag As Boolean
            Dim aDir As TVECTOR
            Dim bKeep As Boolean
            rIndex = -1
            rDistance = 0
            If Not TVECTOR.IsNull(ioDirection, 3) Then
                bTestDir = (aTestDir = dxxLineDescripts.Infinite) Or (aTestDir = dxxLineDescripts.InfiniteNegative) Or (aTestDir = dxxLineDescripts.InfinitePositive)
            End If
            If bTestDir Then
                tDir = ioDirection.Normalized(bFlag)
                If bFlag Then Return _rVal
            End If
            minval = Double.MaxValue
            For i = 1 To Count
                v1 = _Members(i - 1)
                bKeep = False
                If Not bTestDir Then
                    d1 = v1.DistanceTo(aSearchV)
                    If d1 < minval Then bKeep = True
                Else
                    aDir = aSearchV.DirectionTo(v1, False, rDirectionIsNull:=bFlag, rDistance:=d1)
                    If bFlag Then aDir = tDir
                    bKeep = aDir.Equals(tDir, True, 3, bFlag)
                    If bKeep Then
                        If aTestDir = dxxLineDescripts.InfiniteNegative Then
                            bKeep = bFlag
                        ElseIf aTestDir = dxxLineDescripts.InfinitePositive Then
                            bKeep = Not bFlag
                        End If
                    End If
                End If
                If bKeep Then
                    minval = d1
                    rIndex = i
                End If
            Next i
            If rIndex > -1 Then
                rDistance = minval
                _rVal = _Members(rIndex - 1)
            End If
            If Not bTestDir Then ioDirection = aSearchV.DirectionTo(_rVal)
            Return _rVal
        End Function
        Public Sub Clear()
            _Init = True
            ReDim _Members(-1)

        End Sub
        Public Function Clone() As TVECTORS

            Return New TVECTORS(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TVECTORS(Me)
        End Function
        Public Function BoundingCircle(Optional aPlane As dxfPlane = Nothing) As TARC
            Dim _rVal As New TARC("") With {.Plane = New TPLANE(aPlane)}
            If Count <= 0 Then Return _rVal
            Dim cHull As TVECTORS = ConvexHull(aPlane)


            Dim d1 As Double
            Dim dMax As Double
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim aPln As New TPLANE(aPlane)
            Dim aAr As New TARC With {.Plane = aPln, .Radius = 0.0000001}
            Dim aArs(0 To 3) As TARC
            Dim ids() As Integer = {0, -1, -1, -1, -1}
            Dim Pts(0 To 4) As TVECTOR
            Dim aVecs As TVECTORS
            aAr.Plane.Origin = cHull.Item(1, True)
            'first get the two pints that are farthest apart
            For i As Integer = 0 To cHull.Count - 1
                v1 = cHull.Item(i + 1)
                For j As Integer = 0 To cHull.Count - 1
                    If i <> j Then
                        v2 = cHull.Item(j + 1)
                        d1 = dxfProjections.DistanceTo(v1, v2)
                        If d1 > dMax Then
                            ids(1) = i
                            ids(2) = j
                            dMax = d1
                        End If
                    End If
                Next j
            Next i
            Pts(1) = cHull.Item(ids(1) + 1)
            Pts(2) = cHull.Item(ids(2) + 1)
            aAr.Radius = Math.Round(Pts(1).DistanceTo(Pts(2)) / 2, 8)
            aAr.Plane.Origin = Pts(1).MidPt(Pts(2))
            If ids(1) <> ids(2) And aAr.Radius > 0 Then
                'see if all the points are in the circle
                dMax = aAr.Radius
                For i = 0 To cHull.Count - 1
                    If i <> ids(1) And i <> ids(2) Then
                        v1 = cHull.Item(i + 1)
                        d1 = aAr.Plane.Origin.DistanceTo(v1, 8)
                        If d1 > dMax Then
                            ids(3) = i
                            Pts(3) = v1
                            dMax = d1
                            'keep the one that is the farthest out
                        End If
                    End If
                Next i
                'at least one is not in the circle
                If ids(3) >= 0 Then
                    'create a three point circle with the first two as the start pts and the new one
                    'as an pt on the circle between the original two
                    aAr = dxfPrimatives.ArcThreePointV(Pts(1), Pts(3), Pts(2), True, aAr.Plane, True)
                    dMax = Math.Round(aAr.Radius, 8)
                    'see if any are out of the new circle
                    For i = 0 To cHull.Count - 1
                        If i <> ids(1) And i <> ids(2) And i <> ids(3) Then
                            v1 = cHull.Item(i + 1)
                            d1 = aAr.Plane.Origin.DistanceTo(v1, 8)
                            If d1 > dMax Then
                                ids(4) = i
                                Pts(4) = v1
                                dMax = d1
                                'keep the one that is the farthest out
                            End If
                        End If
                    Next i
                    If ids(4) >= 0 Then
                        'one of the three posible 3 pt circles using the 4 points is nthe solution
                        '(1,2,4) , (1,3,4) , (2,3,4)
                        aVecs = New TVECTORS(3)
                        'try 1,2,4
                        aVecs.SetItem(1, Pts(1))
                        aVecs.SetItem(2, Pts(2))
                        aVecs.SetItem(3, Pts(4))
                        aVecs.Clockwise(aAr.Plane, 0)
                        aArs(1) = dxfPrimatives.ArcThreePointV(aVecs.Item(1), aVecs.Item(2), aVecs.Item(3), True, aAr.Plane, True)
                        dMax = Math.Round(aArs(1).Radius, 8)
                        'aArs(1).Bulge = 0
                        'see if any are out of the new new circle
                        For i = 0 To cHull.Count - 1
                            If i <> ids(1) And i <> ids(2) And i <> ids(4) Then
                                v1 = cHull.Item(i + 1)
                                d1 = aArs(1).Plane.Origin.DistanceTo(v1, 8)
                                If d1 > dMax Then
                                    'aArs(1).Bulge = 1
                                    Exit For
                                End If
                            End If
                        Next i
                        aVecs.SetItem(1, Pts(1))
                        aVecs.SetItem(2, Pts(3))
                        aVecs.SetItem(3, Pts(4))
                        aVecs.Clockwise(aAr.Plane, 0)
                        aArs(2) = dxfPrimatives.ArcThreePointV(aVecs.Item(1), aVecs.Item(2), aVecs.Item(3), True, aAr.Plane, True)
                        dMax = Math.Round(aArs(2).Radius, 8)
                        'aArs(2).Bulge = 0
                        'see if any are out of the new new circle
                        For i = 0 To cHull.Count - 1
                            If i <> ids(1) And i <> ids(3) And i <> ids(4) Then
                                v1 = cHull.Item(i + 1)
                                d1 = aArs(2).Plane.Origin.DistanceTo(v1, 8)
                                If d1 > dMax Then
                                    'aArs(2).Bulge = 1
                                    Exit For
                                End If
                            End If
                        Next i
                        aVecs.SetItem(1, Pts(2))
                        aVecs.SetItem(2, Pts(3))
                        aVecs.SetItem(3, Pts(4))
                        aVecs.Clockwise(aAr.Plane, 0)
                        aArs(3) = dxfPrimatives.ArcThreePointV(aVecs.Item(1), aVecs.Item(2), aVecs.Item(3), True, aAr.Plane, True)
                        dMax = Math.Round(aArs(3).Radius, 8)
                        'aArs(3).Bulge = 0
                        'see if any are out of the new new circle
                        For i = 0 To cHull.Count - 1
                            If i <> ids(2) And i <> ids(3) And i <> ids(4) Then
                                v1 = cHull.Item(i + 1)
                                d1 = aArs(3).Plane.Origin.DistanceTo(v1, 8)
                                If d1 > dMax Then
                                    'aArs(3).Bulge = 1
                                    Exit For
                                End If
                            End If
                        Next i
                        'keep the smallest arc of the three possible that encloses all the points
                        Dim j As Integer = -1
                        For i = 1 To 3
                            If aArs(i).Bulge = 0 Then
                                If j = -1 Then
                                    j = i
                                Else
                                    If aArs(i).Radius < aArs(j).Radius Then j = i
                                End If
                            End If
                        Next i
                        If j > 0 Then
                            aAr = aArs(j)
                        End If
                    End If
                End If
            End If
            aAr.StartAngle = 0
            aAr.EndAngle = 360
            Return aAr
        End Function
        Public Function Bounds(aPlane As TPLANE) As TPLANE

            Dim _rVal As New TPLANE(aPlane)
            Dim aHt As Double
            Dim aWd As Double
            _rVal.Origin = PlanarCenter(aPlane, aWd, aHt)
            _rVal.Height = aHt
            _rVal.Width = aWd
            Return _rVal
        End Function
        Public Function BoundingRectangle(aBasePlane As TPLANE, Optional aPlane As dxfPlane = Nothing, Optional bSuppressProjection As Boolean = False) As TPLANE
            Dim _rVal As New TPLANE(aBasePlane)
            _rVal.SetDimensions(0, 0)
            If Not dxfPlane.IsNull(aPlane) Then
                _rVal.Define(aPlane.OriginV, aPlane.XDirectionV, aPlane.YDirectionV)
            End If
            Dim aHt As Double
            Dim aWd As Double
            Dim cp As TVECTOR = PlanarCenter(_rVal, aWd, aHt, bSuppressProjection)
            _rVal.Define(cp, _rVal.XDirection, _rVal.YDirection, aHt, aWd)

            Return _rVal
        End Function
        Public Sub Project(aDirection As TVECTOR, aDistance As Object, Optional bSuppressNormalize As Boolean = False)
            Dim dst As Double = TVALUES.To_DBL(aDistance)
            If dst = 0 Then Return
            If TVECTOR.IsNull(aDirection) Then Return
            Dim aDir As TVECTOR
            Dim vAdd As TVECTOR
            Dim bFlag As Boolean
            If Not bSuppressNormalize Then aDir = aDirection.Normalized(bFlag) Else aDir = aDirection
            If bFlag Then Return
            If dst <= 0 Then aDir *= -1
            vAdd = aDir * Math.Abs(dst)
            Translate(vAdd)
        End Sub
        Public Function Projected(aDirection As TVECTOR, aDistance As Object, Optional bSuppressNormalize As Boolean = False) As TVECTORS
            Dim _rVal As New TVECTORS(Me)
            Dim dst As Double = TVALUES.To_DBL(aDistance)
            If dst = 0 Then Return _rVal
            If TVECTOR.IsNull(aDirection) Then Return _rVal
            _rVal.Project(aDirection, aDistance, bSuppressNormalize)
            Return _rVal
        End Function
        Public Function ProjectTo(aPlane As TPLANE) As Boolean
            '#1the plane to project to
            '^projects the passed vector to the passed plane
            '~the project direction is assumed to be the z direction of the plane
            If Not aPlane.DirectionsAreDefined Then Return False
            Dim _rVal As Boolean = False
            For i As Integer = 0 To Count - 1
                Dim d1 As Double = 0
                Dim v1 As TVECTOR = dxfProjections.ToPlane(_Members(i), aPlane, d1)
                If d1 <> 0 Then _rVal = True
                _Members(i) = v1
            Next
            Return _rVal
        End Function
        Public Function ProjectedTo(aPlane As TPLANE) As TVECTORS
            If Not aPlane.DirectionsAreDefined Then Return Me
            Dim _rVal As New TVECTORS(0)
            '#1the plane to project to
            '^projects the passed vector to the passed plane
            '~the project direction is assumed to be the z direction of the plane
            Dim i As Integer
            For i = 0 To Count - 1
                _rVal.Add(_Members(i).ProjectedTo(aPlane))
            Next
            Return _rVal
        End Function
        Public Function PlanarCenter(aPlane As TPLANE, Optional bRedfineVectors As Boolean = False) As TVECTOR
            Dim rXSpan As Double = 0.0
            Dim rYSpan As Double = 0.0
            Return PlanarCenter(aPlane, rXSpan, rYSpan, bRedfineVectors)
        End Function
        Public Function PlanarCenter(aPlane As TPLANE, ByRef rXSpan As Double, ByRef rYSpan As Double, Optional bRedfineVectors As Boolean = False) As TVECTOR
            rXSpan = 0
            rYSpan = 0

            If TPLANE.IsNull(aPlane) Then aPlane = TPLANE.World
            '#1the plane to test
            '#2returns the horizontal span of the vectors in the collection with respect to the calulated center and the horizontal direction of the passed plane
            '#3returns the vertical span of the vectors in the collection with respect to the calulated center and the vertical direction of the passed plane
            '^the center of all the members with respect to the horizontal and vertical directions of the passed plane
            If Count = 0 Then Return aPlane.Origin
            Dim i As Integer
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim UL As TVECTOR
            Dim LR As TVECTOR
            Dim aLims As TLIMITS
            For i = 1 To Count
                v1 = Item(i)
                v2 = v1.WithRespectTo(aPlane)
                If bRedfineVectors Then SetItem(i, v2)
                If i = 1 Then
                    aLims = New TLIMITS(v2)
                Else
                    aLims.Update(v2)
                End If
            Next i
            rXSpan = aLims.Width
            rYSpan = aLims.Height
            UL = aPlane.Origin + aPlane.XDirection * aLims.Left
            UL += aPlane.YDirection * aLims.Top
            LR = aPlane.Origin + aPlane.XDirection * aLims.Right
            LR += aPlane.YDirection * aLims.Bottom
            Return UL.Interpolate(LR, 0.5)
        End Function
        Public Function Remove(aIndex As Integer) As TVECTOR
            Dim _rVal As New TVECTOR(0)
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return TVECTOR.Zero
            End If
            If aIndex = Count Then
                _rVal = Last
                System.Array.Resize(_Members, Count - 1)
            Else
                Dim newMems As New TVECTORS(0)
                For i As Integer = 1 To Count
                    If i <> aIndex Then
                        newMems.Add(_Members(i - 1))
                    Else
                        _rVal = _Members(i - 1)
                    End If
                Next i
                _Members = newMems._Members
            End If
            Return _rVal
        End Function
        Public Function Line(aSPIndex As Integer, aEPIndex As Integer) As TLINE
            Dim _rVal As New TLINE("")
            If aSPIndex > 0 And aSPIndex <= Count Then _rVal.SPT = New TVECTOR(Item(aSPIndex))
            If aEPIndex > 0 And aEPIndex <= Count Then _rVal.EPT = New TVECTOR(Item(aEPIndex))
            Return _rVal
        End Function
        Public Sub Print(Optional aTag As String = "", Optional bWithVertexCodes As Boolean = False)
            System.Diagnostics.Debug.WriteLine("")
            System.Diagnostics.Debug.WriteLine(Trim(aTag & " TVECTORS{" & Count & "}"))
            Dim aStr As String
            For i As Integer = 1 To Count
                aStr = i.ToString & " - " & Item(i).ToString
                If bWithVertexCodes Then aStr += $",{ Item(i).CodeString}"
                System.Diagnostics.Debug.WriteLine("   " & aStr)
            Next
        End Sub

        Public Function ToLineSegments(Optional bClosed As Boolean = False) As TSEGMENTS
            Dim _rVal As New TSEGMENTS(0)
            If Count <= 1 Then Return _rVal
            If Count = 2 Then bClosed = False
            Dim v2 As TVECTOR
            For i As Integer = 1 To Count
                Dim v1 As TVECTOR = _Members(i - 1)
                If i < Count Then
                    v2 = _Members(i)
                    _rVal.Add(New TSEGMENT(New TLINE(v1, v2)))

                Else
                    If Not bClosed Then Exit For
                    v2 = _Members(0)
                    _rVal.Add(New TSEGMENT(New TLINE(v1, v2)))
                End If

            Next
            Return _rVal
        End Function


        Public Function CounterClockwise(aPlane As TPLANE, aBaseAngle As Double) As Boolean
            Return Clockwise(aPlane, aBaseAngle, bReverseSort:=True)
        End Function
        Public Function Clockwise(aPlane As TPLANE, aBaseAngle As Double, Optional bReverseSort As Boolean = False) As Boolean
            Dim _rVal As Boolean
            If Count <= 1 Or Not aPlane.DirectionsAreDefined Then Return _rVal
            Dim v1 As TVECTOR
            Dim vp As TVECTOR
            Dim ang1 As Double
            Dim cp As TVECTOR = aPlane.Origin
            Dim d1 As Double
            Dim tpl As Tuple(Of Double, TVECTOR)
            Dim srt As New List(Of Tuple(Of Double, TVECTOR))
            Dim xDir As TVECTOR = aPlane.XDirection
            Dim bDir As TVECTOR
            Dim aN As TVECTOR = aPlane.ZDirection
            If aBaseAngle <> 0 Then xDir.RotateAbout(aN, aBaseAngle, False, True)
            For i As Integer = 1 To Count
                v1 = Item(i)
                vp = v1.WithRespectTo(aPlane)
                bDir = cp.DirectionTo(vp, False, rDistance:=d1)
                If d1 = 0 Then
                    ang1 = 0
                Else
                    ang1 = xDir.AngleTo(bDir, aN)
                    If ang1 = 360 Then ang1 = 0
                End If
                srt.Add(New Tuple(Of Double, TVECTOR)(ang1, v1))
            Next i
            srt.Sort(Function(tupl1 As Tuple(Of Double, TVECTOR), tupl2 As Tuple(Of Double, TVECTOR)) tupl1.Item1.CompareTo(tupl2.Item1))
            If Not bReverseSort Then srt.Reverse()
            For i As Integer = 1 To Count
                tpl = srt.Item(i - 1)
                _Members(i - 1) = tpl.Item2
            Next
            Return _rVal
        End Function
        Public Function WithRespectToPlane(aNewPlane As TPLANE, Optional aXScale As Double = 1, Optional aYScale As Double = 1, Optional aZScale As Double = 1, Optional aRotation As Double? = 0, Optional aPrecis As Integer = 8) As TVECTORS
            Dim _rVal As New TVECTORS(0)
            Dim bPlane As New TPLANE(aNewPlane)
            If aRotation.HasValue Then
                bPlane.Revolve(aRotation.Value, False)
            End If
            For i As Integer = 1 To Count
                _rVal.Add(Item(i).WithRespectTo(bPlane, aXScale, aYScale, aZScale, aPrecis, 0))
            Next i
            Return _rVal
        End Function

        Public Function WithRespectToPlane(aNewPlane As dxfPlane, Optional aXScale As Double = 1, Optional aYScale As Double = 1, Optional aZScale As Double = 1, Optional aRotation As Double? = 0, Optional aPrecis As Integer = 8) As TVECTORS
            Dim _rVal As New TVECTORS(0)
            Dim bPlane As New TPLANE(aNewPlane)
            If aRotation.HasValue <> 0 Then

                bPlane.Revolve(aRotation.Value, False)
            End If
            For i As Integer = 1 To Count
                _rVal.Add(Item(i).WithRespectTo(bPlane, aXScale, aYScale, aZScale, aPrecis, 0))
            Next i
            Return _rVal
        End Function

        Public Function Farthest(aSearchV As TVECTOR, ByRef ioDirection As TVECTOR, Optional aTestDir As dxxLineDescripts = dxxLineDescripts.Normal) As TVECTOR
            Dim rDistance As Double = 0.0
            Dim rIndex As Integer = 0
            Return Farthest(aSearchV, ioDirection, aTestDir, rDistance, rIndex)
        End Function
        Public Function Farthest(aSearchV As TVECTOR, ByRef ioDirection As TVECTOR, aTestDir As dxxLineDescripts, ByRef rDistance As Double, ByRef rIndex As Integer) As TVECTOR
            Dim _rVal As TVECTOR = TVECTOR.Zero
            '#1the vectors to search
            '#2the vector to compare to
            '#3if the direction to use for testing
            '#4how to test the passed direction Normal means no test
            '#5returns the index of the returned vector. -1 means the passed vectors were empty or no vectors lie on the test direction.
            '^returns the vector from the vector array which is the farthest from the passed vector
            Dim v1 As TVECTOR
            Dim d1 As Double
            Dim maxval As Double
            Dim i As Integer
            Dim bTestDir As Boolean
            Dim tDir As TVECTOR
            Dim bFlag As Boolean
            Dim aDir As TVECTOR
            Dim bKeep As Boolean
            rIndex = -1
            rDistance = 0
            If Count <= 0 Then Return _rVal
            bTestDir = (aTestDir = dxxLineDescripts.Infinite) Or (aTestDir = dxxLineDescripts.InfiniteNegative) Or (aTestDir = dxxLineDescripts.InfinitePositive)
            If bTestDir Then
                tDir = ioDirection.Normalized(bFlag)
                If bFlag Then Return _rVal
            End If
            maxval = -1
            For i = 1 To Count
                v1 = _Members(i - 1)
                bKeep = False
                If Not bTestDir Then
                    d1 = v1.DistanceTo(aSearchV)
                    If d1 > maxval Then bKeep = True
                Else
                    aDir = aSearchV.DirectionTo(v1, False, rDirectionIsNull:=bFlag, rDistance:=d1)
                    If bFlag Then aDir = tDir
                    bKeep = aDir.Equals(tDir, True, 3, bFlag)
                    If bKeep Then
                        If aTestDir = dxxLineDescripts.InfiniteNegative Then
                            bKeep = bFlag
                        ElseIf aTestDir = dxxLineDescripts.InfinitePositive Then
                            bKeep = Not bFlag
                        End If
                    End If
                End If
                If bKeep Then
                    maxval = d1
                    rIndex = i
                End If
            Next i
            If rIndex > 0 Then
                rDistance = maxval
                _rVal = _Members(rIndex - 1)
            End If
            If Not bTestDir Then ioDirection = aSearchV.DirectionTo(_rVal)
            Return _rVal
        End Function
        Public Function GetAtCoordinate(Optional aX As Double? = Nothing, Optional aY As Double? = Nothing, Optional aZ As Double? = Nothing, Optional aPrecis As Integer = 3, Optional aCS As dxfPlane = Nothing, Optional bReturnClones As Boolean = False) As TVECTORS
            Dim _rVal As New TVECTORS
            '#1the subject vectors
            '#2the X coordinate to match
            '#3the Y coordinate to match
            '#4the Z coordinate to match
            '^searchs for and returns vectors from the collection whose coordinates match the passed coordinates
            '~if an any of the ordinates (X, Y or Z) are not passed or are not numeric they are not used in the comparison.
            '~say ony an X value is passed, then all the vectors with the same X ordinate are returned regarless of their
            '~respective Y and Z ordinate values.
            If Count <= 0 Then Return _rVal
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            Try
                Dim DoX As Boolean = aX.HasValue
                Dim doY As Boolean = aY.HasValue
                Dim doZ As Boolean = aZ.HasValue
                Dim withrespct As Boolean = aCS IsNot Nothing
                Dim isMatchX As Boolean
                Dim isMatchY As Boolean
                Dim isMatchZ As Boolean
                Dim xx As Double
                Dim yy As Double
                Dim zz As Double
                Dim v1 As TVECTOR
                Dim aDif As Double
                Dim i As Integer
                Dim aPl As New TPLANE(aCS)

                If DoX Then xx = aX.Value
                If doY Then yy = aY.Value
                If doZ Then zz = aZ.Value
                If Not DoX And Not doY And Not doZ Then Return _rVal
                For i = 1 To Count
                    v1 = _Members(i - 1)
                    If withrespct Then v1 = v1.WithRespectTo(aPl)
                    isMatchX = True
                    isMatchY = True
                    isMatchZ = True
                    If DoX Then
                        aDif = Math.Round(v1.X - xx, aPrecis)
                        isMatchX = (aDif = 0)
                    End If
                    If doY Then
                        aDif = Math.Round(v1.Y - yy, aPrecis)
                        isMatchY = (aDif = 0)
                    End If
                    If doZ Then
                        aDif = Math.Round(v1.Z - zz, aPrecis)
                        isMatchZ = (aDif = 0)
                    End If
                    If isMatchX And isMatchY And isMatchZ Then
                        If bReturnClones Then
                            _rVal.Add(New TVECTOR(v1))
                        Else
                            _rVal.Add(v1)
                        End If
                    End If
                Next i
                Return _rVal
            Catch ex As Exception
            End Try
            Return _rVal
        End Function
        Public Function GetByCode(aCode As Long) As TVECTORS
            Dim _rVal As New TVECTORS
            Dim i As Integer
            For i = 1 To Count
                If _Members(i - 1).Code = aCode Then
                    _rVal.Add(New TVECTOR(_Members(i - 1)))
                End If
            Next i
            Return _rVal
        End Function
        Public Function GetLimits(Optional aPlane As dxfPlane = Nothing, Optional aPrecis As Integer = -1) As TLIMITS
            Dim _rVal As New TLIMITS
            If Count <= 0 Then Return _rVal
            _rVal = New TLIMITS(True)
            Dim v1 As TVECTOR
            Dim bPln As Boolean
            Dim aPl As New TPLANE(aPlane, bPln)
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            For i As Integer = 1 To Count
                v1 = _Members(i - 1)
                If bPln Then v1 = v1.WithRespectTo(aPl, aPrecis:=aPrecis)
                _rVal.Update(v1)
            Next i
            Return _rVal
        End Function
        Public Function GetDimensions(Optional aPrecis As Integer = -1, Optional aPlane As dxfPlane = Nothing) As TVECTOR
            Dim rMinX As Double = 0.0
            Dim rMaxX As Double = 0.0
            Dim rMinY As Double = 0.0
            Dim rMaxY As Double = 0.0
            Dim rMinZ As Double = 0.0
            Dim rMaxZ As Double = 0.0
            Return GetDimensions(rMinX, rMaxX, rMinY, rMaxY, rMinZ, rMaxZ, aPrecis, aPlane)
        End Function
        Public Function GetDimensions(ByRef rMinX As Double, ByRef rMaxX As Double, ByRef rMinY As Double, ByRef rMaxY As Double, ByRef rMinZ As Double, ByRef rMaxZ As Double, Optional aPrecis As Integer = -1, Optional aPlane As dxfPlane = Nothing) As TVECTOR
            Dim _rVal As TVECTOR = TVECTOR.Zero
            '^returns then dimensions of the collection of vectors
            Dim aLims As TLIMITS = GetLimits(aPlane, aPrecis)
            rMinX = aLims.Left
            rMinY = aLims.Bottom
            rMinZ = aLims.MinZ
            rMaxX = aLims.Right
            rMaxY = aLims.Top
            rMaxZ = aLims.MaxZ
            _rVal.X = Math.Abs(rMaxX - rMinX)
            _rVal.Y = Math.Abs(rMaxY - rMinY)
            _rVal.Z = Math.Abs(rMaxZ - rMinZ)
            Return _rVal
        End Function
        Public Function GetOrdinate(aSearchParam As dxxOrdinateTypes, Optional aPlane As dxfPlane = Nothing) As Double
            Dim _rVal As Double
            '#1the vectors to search
            '#2parameter controling the value returned
            '^returns the requested ordinate based on the search parameter and the members of the current collection
            Dim v1 As TVECTOR
            Dim vMax As TVECTOR
            Dim vMin As TVECTOR
            Dim bPln As Boolean
            Dim imaxX As Integer
            Dim iminX As Integer
            Dim imaxY As Integer
            Dim iminY As Integer
            Dim imaxZ As Integer
            Dim iminZ As Integer
            Dim aPl As New TPLANE(aPlane, bPln)
            vMax = New TVECTOR(Single.MaxValue, Single.MaxValue, Single.MaxValue)
            vMin = New TVECTOR(Single.MaxValue, Single.MaxValue, Single.MaxValue)
            For i As Integer = 1 To Count
                v1 = _Members(i - 1)
                If bPln Then v1 = v1.WithRespectTo(aPl)
                If v1.X > vMax.X Then
                    imaxX = i + 1
                    vMax.X = v1.X
                End If
                If v1.X < vMin.X Then
                    iminX = i + 1
                    vMin.X = v1.X
                End If
                If v1.Y > vMax.Y Then
                    imaxY = i + 1
                    vMax.Y = v1.Y
                End If
                If v1.Y < vMin.Y Then
                    iminY = i + 1
                    vMin.Y = v1.Y
                End If
                If v1.Z > vMax.Z Then
                    imaxZ = i + 1
                    vMax.Z = v1.Z
                End If
                If v1.Z < vMin.Z Then
                    iminZ = i + 1
                    vMin.Z = v1.Z
                End If
            Next i
            Select Case aSearchParam
                Case dxxOrdinateTypes.MinX
                    If iminX > 0 Then
                        _rVal = vMin.X
                    End If
                Case dxxOrdinateTypes.MinY
                    If iminY > 0 Then
                        _rVal = vMin.Y
                    End If
                Case dxxOrdinateTypes.MinZ
                    If iminZ > 0 Then
                        _rVal = vMin.Z
                    End If
                Case dxxOrdinateTypes.MaxX
                    If imaxX > 0 Then
                        _rVal = vMax.X
                    End If
                Case dxxOrdinateTypes.MaxY
                    If imaxY > 0 Then
                        _rVal = vMax.Y
                    End If
                Case dxxOrdinateTypes.MaxZ
                    If imaxZ > 0 Then
                        _rVal = vMax.Z
                    End If
                Case dxxOrdinateTypes.MidX
                    If imaxX > 0 And iminX > 0 Then
                        _rVal = vMin.X + (vMax.X - vMin.X) / 2
                    End If
                Case dxxOrdinateTypes.MidY
                    If imaxY > 0 And iminY > 0 Then
                        _rVal = vMin.Y + (vMax.Y - vMin.Y) / 2
                    End If
                Case dxxOrdinateTypes.MidZ
                    If imaxZ > 0 And iminZ > 0 Then
                        _rVal = vMin.Z + (vMax.Z - vMin.Z) / 2
                    End If
            End Select
            Return _rVal
        End Function
        Public Function GetVector(aControlFlag As dxxPointFilters, Optional aOrdinate As Double = 0.0, Optional aCS As dxfPlane = Nothing, Optional aPrecis As Integer = 3) As TVECTOR
            Dim rIndex As Integer = 0
            Return GetVector(aControlFlag, aOrdinate, aCS, aPrecis, rIndex)
        End Function
        Public Function GetVector(aControlFlag As dxxPointFilters, aOrdinate As Double, aCS As dxfPlane, aPrecis As Integer, ByRef rIndex As Integer) As TVECTOR
            Dim _rVal As TVECTOR = TVECTOR.Zero
            rIndex = 0
            If Count <= 0 Then Return _rVal
            '#2flag indicating what type of vector to search for
            '#3the ordinate to search for if the search is ordinate specific
            '#4an optional coordinate system to use
            '#5a precision for numerical comparison (1 to 8)
            '#6returns the index of the matching point
            '^returns a vector from the collection whose properties or position in the collection match the passed control flag
            If aPrecis < 1 Then aPrecis = 1
            If aPrecis > 8 Then aPrecis = 8
            Dim v1 As TVECTOR
            Dim aPl As New TPLANE(aCS)
            Dim Y1 As Double
            Dim x1 As Double
            Dim i As Integer
            Dim comp As Double
            Dim d1 As Double
            Dim d2 As Double
            Dim maxval As Double
            Dim minval As Double
            Dim id1 As Long
            Dim ID2 As Long
            Dim aLims As TLIMITS
            Dim withrespct As Boolean = aCS IsNot Nothing

            'search for vectors at nearest to the passed ordinate
            If aControlFlag >= dxxPointFilters.NearestToX And aControlFlag <= dxxPointFilters.FarthestFromZ Then
                rIndex = 1
                id1 = 1
                ID2 = 1
                v1 = _Members(0)
                If withrespct Then v1 = v1.WithRespectTo(aPl)
                If Count = 1 Then Return New TVECTOR(v1)
                Select Case aControlFlag
                    Case dxxPointFilters.NearestToX, dxxPointFilters.FarthestFromX
                        d1 = Math.Abs(v1.X - aOrdinate)
                    Case dxxPointFilters.NearestToY, dxxPointFilters.FarthestFromY
                        d1 = Math.Abs(v1.Y - aOrdinate)
                    Case dxxPointFilters.NearestToZ, dxxPointFilters.FarthestFromZ
                        d1 = Math.Abs(v1.Z - aOrdinate)
                End Select
                minval = d1
                maxval = d1
                For i = 2 To Count
                    v1 = _Members(i - 1)
                    If withrespct Then v1 = v1.WithRespectTo(aPl)
                    Select Case aControlFlag
                        Case dxxPointFilters.NearestToX, dxxPointFilters.FarthestFromX
                            d2 = Math.Abs(v1.X - aOrdinate)
                        Case dxxPointFilters.NearestToY, dxxPointFilters.FarthestFromY
                            d2 = Math.Abs(v1.Y - aOrdinate)
                        Case dxxPointFilters.NearestToZ, dxxPointFilters.FarthestFromZ
                            d2 = Math.Abs(v1.Z - aOrdinate)
                    End Select
                    If d2 >= maxval Then
                        id1 = i
                        maxval = d2
                    End If
                    If d2 <= minval Then
                        ID2 = i
                        minval = d2
                    End If
                Next i
                rIndex = id1
                If aControlFlag = dxxPointFilters.NearestToX Or aControlFlag = dxxPointFilters.NearestToY Or aControlFlag = dxxPointFilters.NearestToZ Then
                    rIndex = ID2
                End If
            End If
            'search for vectors at extremes
            'returns the first one that satisfies
            If aControlFlag > 11 And aControlFlag < 18 Then
                Select Case aControlFlag
                    Case dxxPointFilters.AtMaxX
                        aOrdinate = GetOrdinate(dxxOrdinateTypes.MaxX, aCS)
                        aControlFlag = dxxPointFilters.AtX
                    Case dxxPointFilters.AtMaxY
                        aOrdinate = GetOrdinate(dxxOrdinateTypes.MaxY, aCS)
                        aControlFlag = dxxPointFilters.AtY
                    Case dxxPointFilters.AtMaxZ
                        aOrdinate = GetOrdinate(dxxOrdinateTypes.MaxZ, aCS)
                        aControlFlag = dxxPointFilters.AtZ
                    Case dxxPointFilters.AtMinX
                        aOrdinate = GetOrdinate(dxxOrdinateTypes.MinX, aCS)
                        aControlFlag = dxxPointFilters.AtX
                    Case dxxPointFilters.AtMinY
                        aOrdinate = GetOrdinate(dxxOrdinateTypes.MinY, aCS)
                        aControlFlag = dxxPointFilters.AtY
                    Case dxxPointFilters.AtMinZ
                        aOrdinate = GetOrdinate(dxxOrdinateTypes.MinZ, aCS)
                        aControlFlag = dxxPointFilters.AtZ
                End Select
            End If
            'searching for a vector at a particular ordinate (ie at X = 10)
            'returns the first one that satisfies
            If aControlFlag > 8 And aControlFlag < 12 Then
                For i = 1 To Count
                    v1 = Item(i)
                    If withrespct Then v1 = v1.WithRespectTo(aPl)
                    Select Case aControlFlag
                        Case dxxPointFilters.AtX
                            comp = Math.Abs(v1.X - aOrdinate)
                        Case dxxPointFilters.AtY
                            comp = Math.Abs(v1.Y - aOrdinate)
                        Case dxxPointFilters.AtX
                            comp = Math.Abs(v1.Z - aOrdinate)
                    End Select
                    If Math.Round(Math.Abs(comp), aPrecis) = 0 Then
                        rIndex = i
                        Exit For
                    End If
                Next i
            End If
            'searching for a relative vector (lower left, top right etc. etc.)
            If aControlFlag >= dxxPointFilters.GetTopLeft And aControlFlag <= dxxPointFilters.GetRightBottom Then
                rIndex = 1
                aLims = GetLimits(aCS, aPrecis)
                Select Case aControlFlag
                    Case dxxPointFilters.GetBottomLeft, dxxPointFilters.GetLeftBottom
                        Y1 = aLims.Bottom
                        x1 = aLims.Left
                        comp = Single.MaxValue
                        For i = 1 To Count
                            v1 = Item(i)
                            If withrespct Then v1 = v1.WithRespectTo(aPl)
                            If aControlFlag = dxxPointFilters.GetBottomLeft Then
                                If Math.Round(v1.Y - Y1, aPrecis) = 0 Then
                                    If v1.X < comp Then
                                        rIndex = i
                                        comp = v1.X
                                    End If
                                End If
                            Else
                                If Math.Round(v1.X - x1, aPrecis) = 0 Then
                                    If v1.Y < comp Then
                                        rIndex = i
                                        comp = v1.Y
                                    End If
                                End If
                            End If
                        Next i
                    Case dxxPointFilters.GetBottomRight, dxxPointFilters.GetRightBottom
                        Y1 = aLims.Bottom
                        x1 = aLims.Right
                        comp = Single.MaxValue
                        If aControlFlag = dxxPointFilters.GetRightBottom Then comp = -comp
                        For i = 1 To Count
                            v1 = Item(i)
                            If withrespct Then v1 = v1.WithRespectTo(aPl)
                            If aControlFlag = dxxPointFilters.GetBottomRight Then
                                If Math.Round(v1.Y - Y1, aPrecis) = 0 Then
                                    If v1.X > comp Then
                                        rIndex = i
                                        comp = v1.X
                                    End If
                                End If
                            Else
                                If Math.Round(v1.X - x1, aPrecis) = 0 Then
                                    If v1.Y < comp Then
                                        rIndex = i
                                        comp = v1.Y
                                    End If
                                End If
                            End If
                        Next i
                    Case dxxPointFilters.GetTopLeft, dxxPointFilters.GetLeftTop
                        Y1 = aLims.Top
                        x1 = aLims.Left
                        comp = Single.MaxValue
                        If aControlFlag = dxxPointFilters.GetLeftTop Then comp = -comp
                        For i = 1 To Count
                            v1 = Item(i)
                            If withrespct Then v1 = v1.WithRespectTo(aPl)
                            If aControlFlag = dxxPointFilters.GetTopLeft Then
                                If Math.Round(v1.Y - Y1, aPrecis) = 0 Then
                                    If v1.X < comp Then
                                        rIndex = i
                                        comp = v1.X
                                    End If
                                End If
                            Else
                                If Math.Round(v1.X - x1, aPrecis) = 0 Then
                                    If v1.Y > comp Then
                                        rIndex = i
                                        comp = v1.Y
                                    End If
                                End If
                            End If
                        Next i
                    Case dxxPointFilters.GetTopRight, dxxPointFilters.GetRightTop
                        Y1 = aLims.Top
                        x1 = aLims.Right
                        comp = Single.MaxValue
                        For i = 1 To Count
                            v1 = Item(i)
                            If withrespct Then v1 = v1.WithRespectTo(aPl)
                            If aControlFlag = dxxPointFilters.GetTopRight Then
                                If Math.Round(v1.Y - Y1, aPrecis) = 0 Then
                                    If v1.X > comp Then
                                        rIndex = i
                                        comp = v1.X
                                    End If
                                End If
                            Else
                                If Math.Round(v1.X - x1, aPrecis) = 0 Then
                                    If v1.Y > comp Then
                                        rIndex = i
                                        comp = v1.Y
                                    End If
                                End If
                            End If
                        Next i
                End Select
            End If
            If rIndex < 1 Or rIndex > Count Then rIndex = 0
            If rIndex > 0 Then _rVal = Item(rIndex)
            Return _rVal
        End Function
        Public Function NearestToLine(aLine As TLINE, Optional bReturnFarthest As Boolean = False) As TVECTOR
            Dim rDistance As Double
            Dim rIndex As Integer = 0
            Return NearestToLine(aLine, bReturnFarthest, rDistance, rIndex)
        End Function
        Public Function NearestToLine(aLine As TLINE, bReturnFarthest As Boolean, ByRef rDistance As Double, ByRef rIndex As Integer) As TVECTOR
            Dim _rVal As TVECTOR = TVECTOR.Zero
            '#1the vectors to search
            '#2the line to compare to
            '#3flag to returnt he farthest istead of the nearest
            '#4returns the distance from the returnd vector to the passed line
            '#5returns the index of the returned vector. -1 means the passed vectors were empty
            '^returns the vector from the vector array which is the nearest to the passed vector
            Dim v1 As TVECTOR
            Dim d1 As Double
            Dim compval As Double
            Dim i As Integer
            Dim v2 As TVECTOR
            rIndex = -1
            rDistance = 0
            If aLine.SPT.DistanceTo(aLine.EPT) <= 0.00001 Or Count <= 0 Then Return _rVal
            If Not bReturnFarthest Then
                compval = Single.MaxValue
            Else
                compval = -1
            End If
            For i = 1 To Count
                v1 = _Members(i - 1)
                v2 = dxfProjections.ToLine(v1, aLine, d1)
                If Not bReturnFarthest Then
                    If d1 < compval Then
                        compval = d1
                        rIndex = i
                    End If
                Else
                    If d1 > compval Then
                        compval = d1
                        rIndex = i
                    End If
                End If
            Next i
            If rIndex > -1 Then
                rDistance = compval
                _rVal = _Members(rIndex - 1)
            End If
            Return _rVal
        End Function
        Public Function ShearX(aBaseVector As TVECTOR, aAngle As Double) As TVECTORS 'byRef aPlane As TPLANE, aAngle As Double
            Dim _rVal As New TVECTORS(0)
            If aAngle = 0 Then Return _rVal
            Dim v0 As TVECTOR = aBaseVector
            Dim tang As Double = Math.Tan(Math.Abs(aAngle) * Math.PI / 180)

            For i As Integer = 1 To Count
                Dim v1 As New TVECTOR(_Members(i - 1))

                Dim dY As Double = Math.Abs(v1.Y - v0.Y)
                If dY <> 0 Then
                    Dim dX As Double = tang * dY
                    If v1.Y > v0.Y Then
                        If aAngle < 0 Then dX = -dX
                    Else
                        If aAngle > 0 Then dX = -dX
                    End If
                    v1.X += dX
                End If
                _rVal.Add(v1)
            Next i
            Return _rVal
        End Function
        Public Sub SetOrdinate(aOrdinate As dxxOrdinateDescriptors, aValue As Double)
            For i = 1 To Count
                If aOrdinate = dxxOrdinateDescriptors.Y Then
                    _Members(i - 1).Y = aValue
                ElseIf aOrdinate = dxxOrdinateDescriptors.Z Then
                    _Members(i - 1).Z = aValue
                Else
                    _Members(i - 1).X = aValue
                End If
            Next i
        End Sub
        Public Function UniqueMembers(Optional aPrecis As Integer = 4) As TVECTORS
            Dim _rVal As New TVECTORS(0)
            '^removes and returns the vectors that occur more than once
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim bKeep As Boolean
            Dim d1 As Double
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            For i As Integer = 1 To Count
                v1 = _Members(i - 1)
                bKeep = True
                For j As Integer = 1 To _rVal.Count
                    v2 = _rVal.Item(j)
                    d1 = Math.Round(v1.DistanceTo(v2), aPrecis)
                    If d1 <= 0.00000001 Then
                        bKeep = False
                        Exit For
                    End If
                Next j
                If bKeep Then
                    _rVal.Add(New TVECTOR(v1), v1.Code)
                End If
            Next
            Return _rVal
        End Function
        Public Sub Rotate(aOrigin As TVECTOR, aAxis As TVECTOR, aAngle As Double, Optional bInRadians As Boolean = False)
            Dim rChanged As Boolean = False
            Rotate(aOrigin, aAxis, aAngle, bInRadians, rChanged)
        End Sub
        Public Sub Rotate(aOrigin As TVECTOR, aAxis As TVECTOR, aAngle As Double, bInRadians As Boolean, ByRef rChanged As Boolean)
            rChanged = False
            If Math.Abs(aAngle) <= 0.00000001 Then Return
            Dim aAx As TVECTOR
            Dim aFlg As Boolean
            aAx = aAxis.Normalized(aFlg)
            If aFlg Then Return
            For i As Integer = 1 To Count
                If _Members(i - 1).RotateAbout(aOrigin, aAx, aAngle, bInRadians, True) Then rChanged = True
            Next i
        End Sub
        Public Sub SortNearestToFarthest(aRef As TVECTOR, Optional aFarthestToNeareast As Boolean = False, Optional bRemoveCoinc As Boolean = False, Optional aPrecis As Integer = 4)
            Dim newMems As New TVECTORS(0)
            Dim i As Integer
            Dim v1 As TVECTOR
            Dim d1 As Double
            Dim d2 As Double
            Dim bVectors As New TVECTORS(Me)
            Dim idx As Integer
            If bRemoveCoinc Then
                bVectors = bVectors.UniqueMembers(aPrecis)
            End If
            Do Until bVectors.Count = 0
                If bVectors.Count <= 1 Then
                    newMems.Add(bVectors.Item(1))
                    Exit Do
                Else
                    If aFarthestToNeareast Then d1 = -Double.MaxValue Else d1 = Double.MaxValue
                    idx = -1
                    For i = 1 To bVectors.Count
                        v1 = bVectors.Item(i)
                        d2 = v1.DistanceTo(aRef)
                        If (aFarthestToNeareast And d2 > d1) Or (Not aFarthestToNeareast And d2 < d1) Then
                            idx = i
                            d1 = d2
                        End If
                    Next i
                    v1 = bVectors.Item(idx)
                    newMems.Add(v1)
                    bVectors.Remove(idx)
                End If
            Loop
            _Members = newMems._Members
        End Sub
        Public Sub SortRelativeToLine(aLine As TLINE, aNormal As TVECTOR, Optional aFarthestToNeareast As Boolean = False, Optional bIgnoreDirection As Boolean = True)
            Dim newMems As New TVECTORS(0)
            If Count <= 1 Then Return
            Dim aFlag As Boolean
            Dim i As Integer
            Dim v1 As TVECTOR
            Dim d1 As Double
            Dim v2 As TVECTOR
            Dim zDir As TVECTOR = aNormal.Normalized(aFlag)
            Dim upDir As TVECTOR
            Dim aPln As New TPLANE("")
            If aFlag Then Return
            Dim srt As New List(Of Tuple(Of Double, TVECTOR))
            v1 = aLine.SPT.DirectionTo(aLine.EPT, False, aFlag)
            If aFlag Then Return
            v2 = zDir.CrossProduct(v1, True)
            aPln.Define(aLine.SPT, v1, v2)
            For i = 1 To Count
                v1 = _Members(i - 1).ProjectedTo(aPln)
                v2 = dxfProjections.ToLine(v1, aLine, d1)
                d1 = Math.Round(d1, 6)
                If d1 <> 0 And Not bIgnoreDirection Then
                    upDir = v2.DirectionTo(v1)
                    If Not upDir.Equals(aPln.YDirection, 3) Then d1 = -d1
                End If
                srt.Add(New Tuple(Of Double, TVECTOR)(d1, _Members(i - 1)))
            Next i
            srt.Sort(Function(tupl1 As Tuple(Of Double, TVECTOR), tupl2 As Tuple(Of Double, TVECTOR)) tupl1.Item1.CompareTo(tupl2.Item1))
            If aFarthestToNeareast = dxxSortOrders.FarthestToNearest Then srt.Reverse()
            Dim tpl As Tuple(Of Double, TVECTOR)
            For i = 1 To srt.Count
                tpl = srt.Item(i - 1)
                _Members(i - 1) = tpl.Item2
            Next
        End Sub
        Public Function ToPath(aPlane As TPLANE, bClosed As Boolean, Optional aDisplayVars As TDISPLAYVARS = Nothing, Optional aDomain As dxxDrawingDomains = dxxDrawingDomains.Model) As TPATH
            Dim _rVal As New TPATH(aDomain, aDisplayVars, New dxfPlane(aPlane))
            Dim aLoop As New TVECTORS(Me)
            Dim i As Integer
            If bClosed And Count > 0 Then
                aLoop.Add(New TVECTOR(Item(1)))
            End If
            For i = 1 To aLoop.Count
                If i = 1 Then
                    aLoop.SetCode(i, dxxVertexStyles.MOVETO)
                Else
                    aLoop.SetCode(i, dxxVertexStyles.LINETO)
                End If
            Next i
            _rVal.AddLoop(aLoop)
            Return _rVal
        End Function
        Public Function ToGraphicPath(aPixelSize As Integer, aDeviceSize As System.Drawing.Size) As System.Drawing.Drawing2D.GraphicsPath
            Dim _rVal As New System.Drawing.Drawing2D.GraphicsPath
            Dim v1 As TVECTOR
            Dim ptype As Byte
            Dim p1 As System.Drawing.PointF
            Dim pthPts(0) As PointF
            Dim pthCds(0) As Byte
            Dim pxsz As Single = CSng(aPixelSize)
            If pxsz <= 0 Then pxsz = 1
            Dim cnt As Integer = 0
            Dim pty As System.Drawing.Drawing2D.PathPointType
            Dim gpath As System.Drawing.Drawing2D.GraphicsPath
            Dim maxX As Double
            Dim maxY As Double
            Double.TryParse(aDeviceSize.Width, maxX)

            Double.TryParse(aDeviceSize.Height, maxY)
            'maxY /= 2
            'maxX /= 2

            'cnt = 0
            For vi As Integer = 1 To Count
                v1 = New TVECTOR(_Members(vi - 1).X, _Members(vi - 1).Y) With {.Code = _Members(vi - 1).Code}
                If v1.Y >= 0 Then
                    If v1.Y > maxY Then
                        v1.Y = maxY
                    End If
                Else
                    If Math.Abs(v1.Y) > maxY Then
                        v1.Y = -maxY
                    End If
                End If
                If v1.X >= 0 Then
                    If v1.X > maxX Then
                        v1.X = maxX
                    End If
                Else
                    If Math.Abs(v1.X) > maxX Then
                        v1.X = -maxX
                    End If
                End If

                ptype = v1.Code
                p1 = CType(v1, System.Drawing.PointF)

                If ptype = dxfGlobals.PT_MOVETO Then
                    If cnt > 1 Then
                        Try
                            gpath = New System.Drawing.Drawing2D.GraphicsPath(pthPts, pthCds)
                            _rVal.AddPath(gpath, False)
                        Catch ex As Exception
                            Debug.WriteLine($"TVECTORS.ToGraphicPath ERROR : { ex.Message}")
                        End Try
                    End If
                    cnt = 1
                    ReDim pthPts(0)
                    ReDim pthCds(0)
                    pty = Drawing2D.PathPointType.Start
                    pthPts(cnt - 1) = p1
                    pthCds(cnt - 1) = pty
                ElseIf ptype = dxfGlobals.PT_LINETO Then
                    pty = Drawing2D.PathPointType.Line
                    cnt += 1
                    System.Array.Resize(pthPts, cnt)
                    System.Array.Resize(pthCds, cnt)
                    pthPts(cnt - 1) = p1
                    pthCds(cnt - 1) = pty
                ElseIf ptype = dxfGlobals.PT_BEZIERTO Then
                    pty = Drawing2D.PathPointType.Bezier
                    cnt += 1
                    System.Array.Resize(pthPts, cnt)
                    System.Array.Resize(pthCds, cnt)
                    pthPts(cnt - 1) = p1
                    pthCds(cnt - 1) = pty
                ElseIf ptype = dxfGlobals.PT_PIXELTO Then
                    If cnt > 1 Then
                        Try
                            gpath = New System.Drawing.Drawing2D.GraphicsPath(pthPts, pthCds)
                            _rVal.AddPath(gpath, False)
                        Catch ex As Exception
                            Debug.WriteLine($"TVECTORS.ToGraphicPath ERROR : { ex.Message}")
                        End Try
                    End If
                    ReDim pthPts(4)
                    ReDim pthCds(4)
                    pthPts(0) = New PointF(p1.X - 0.5 * pxsz, p1.Y + 0.5 * pxsz)
                    pthPts(1) = New PointF(p1.X - 0.5 * pxsz, p1.Y - 0.5 * pxsz)
                    pthPts(2) = New PointF(p1.X + 0.5 * pxsz, p1.Y - 0.5 * pxsz)
                    pthPts(3) = New PointF(p1.X + 0.5 * pxsz, p1.Y + 0.5 * pxsz)
                    pthPts(4) = pthPts(0)
                    pthCds(0) = Drawing2D.PathPointType.Start
                    pthCds(1) = Drawing2D.PathPointType.Line
                    pthCds(2) = Drawing2D.PathPointType.Line
                    pthCds(3) = Drawing2D.PathPointType.Line
                    pthCds(4) = Drawing2D.PathPointType.Line
                    Try
                        gpath = New System.Drawing.Drawing2D.GraphicsPath(pthPts, pthCds)
                        _rVal.AddPath(gpath, False)
                    Catch ex As Exception
                        Debug.WriteLine($"TVECTORS.ToGraphicPath ERROR : { ex.Message}")
                    End Try
                    cnt = 0
                ElseIf ptype = dxfGlobals.PT_CLOSEFIGURE Then
                    If cnt > 1 Then
                        Try
                            gpath = New System.Drawing.Drawing2D.GraphicsPath(pthPts, pthCds)
                            _rVal.AddPath(gpath, False)
                        Catch ex As Exception
                            Debug.WriteLine($"TVECTORS.ToGraphicPath ERROR : { ex.Message}")
                        End Try
                    End If
                    cnt = 0
                End If
            Next vi
            If cnt > 1 Then
                Try
                    gpath = New System.Drawing.Drawing2D.GraphicsPath(pthPts, pthCds)

                    _rVal.AddPath(gpath, False)
                Catch ex As Exception
                    Debug.WriteLine($"TVECTORS.ToGraphicPath ERROR : { ex.Message}")
                End Try
            End If
            Return _rVal

        End Function
        Public Sub Reverse()
            If Count <= 1 Then Return
            _Members.Reverse()
        End Sub
        Public Function Reversed() As TVECTORS
            Dim _rVal As New TVECTORS(Me)
            _rVal.Reverse()
            Return _rVal
        End Function
        Public Function ToList() As List(Of TVECTOR)
            If Count <= 0 Then Return New List(Of TVECTOR)
            Return _Members.ToList()
        End Function
#End Region 'Methods
#Region "Shared Methods"

        Public Shared ReadOnly Property Zero As TVECTORS
            Get
                Return New TVECTORS(0)
            End Get
        End Property

        Public Shared Function Array(aStartPt As TVECTOR, aColumnCount As Integer, aRowCount As Integer, aXPitch As Double, aYPitch As Double, Optional aPitchType As dxxPitchTypes = dxxPitchTypes.Rectangular, Optional aRotation As Double? = Nothing, Optional aMaxCount As Long = 0, Optional aPlane As dxfPlane = Nothing, Optional aCollector As colDXFVectors = Nothing) As TVECTORS
            Dim _rVal As New TVECTORS
            '#1the point to start the array from
            '#2the number of columns for the array
            '#3the number of rows for the array
            '#4the distance between columns in the array
            '#5the distance between rows in the array
            '#6the type of pitch to apply
            '#7a rotation to apply
            '#8a limit to the number of points to add
            '#9 the plane to use for the X and Y Directions
            '^Adds a rectangular array to the collection based on the passed information.
            '~the array is built to the right and down from the starting point. Negative pitches reverse the subject direction.
            Dim iR As Integer
            Dim iC As Integer
            Dim aPln As New TPLANE(aPlane)
            Dim org As TVECTOR
            Dim v1 As TVECTOR
            Dim cnt As Long
            Dim f1 As Integer
            aXPitch = Math.Round(aXPitch, 8)
            aYPitch = Math.Round(aYPitch, 8)
            If aXPitch = 0 Then aColumnCount = 1
            If aYPitch = 0 Then aRowCount = 1
            aColumnCount = Math.Abs(aColumnCount)
            aRowCount = Math.Abs(aRowCount)
            aRotation = TVALUES.NormAng(TVALUES.To_DBL(aRotation), False, True, True)
            If aRowCount = 0 And aColumnCount = 0 Then Return _rVal

            If aRotation.HasValue Then aPln.Revolve(aRotation.Value)
            If aPitchType < dxxPitchTypes.Rectangular Or aPitchType > dxxPitchTypes.InvertedTriangular Then aPitchType = dxxPitchTypes.Rectangular
            org = aStartPt
            If aPitchType = dxxPitchTypes.InvertedTriangular Then f1 = -1 Else f1 = 1
            cnt = 0
            For iR = 1 To aRowCount
                v1 = org
                If iR > 1 Then v1 *= aPln.YDirection * (-aYPitch * (iR - 1))
                If aPitchType <> dxxPitchTypes.Rectangular Then
                    If f1 = -1 Then
                        v1 += aPln.XDirection * (0.5 * aXPitch)
                    End If
                End If
                For iC = 1 To aColumnCount
                    If iC > 1 Then v1 += aPln.XDirection * aXPitch
                    cnt += 1
                    _rVal.Add(v1)
                    If aCollector IsNot Nothing Then aCollector.AddV(v1)
                    If aMaxCount > 0 And cnt >= aMaxCount Then Exit For
                Next iC
                f1 = -f1
                If aMaxCount > 0 And cnt >= aMaxCount Then Exit For
            Next iR
            Return _rVal
        End Function
        Public Shared Function Appended(a As TVECTORS, b As TVECTORS, Optional aStartID As Integer = 0) As TVECTORS
            Dim _rVal As New TVECTORS(a)
            'On Error Resume Next
            If b.Count <= 0 Then Return _rVal
            Dim sid As Integer
            If aStartID > 0 Then sid = aStartID
            If sid > b.Count - 1 Then sid = b.Count - 1
            For i As Integer = sid To b.Count - 1
                If _rVal.Count >= Integer.MaxValue Then Exit For
                If i >= 0 And i <= b.Count - 1 Then
                    _rVal.Add(b.Item(i + 1))
                End If
            Next i
            Return _rVal
        End Function


#End Region 'Shared Methods
#Region "Operators"
        Public Shared Widening Operator CType(aVectors As TVECTORS) As colDXFVectors
            Return New colDXFVectors(aVectors)
        End Operator
        Public Shared Widening Operator CType(aVectors As TVECTORS) As TPOINTS
            Dim _rVal As New TPOINTS(0)
            Dim i As Integer
            For i = 1 To aVectors.Count
                _rVal.Add(aVectors.Item(i))
            Next
        End Operator
#End Region 'Operators
    End Structure 'TVECTORS

    Friend Structure TVERTEXVARS
        Implements ICloneable
#Region "Members"
        Private _Bulge As Double
        Public Col As Integer
        Public ColID As Integer
        Public Color As Integer
        Public EndWidth As Double
        Public Flag As String
        Public Index As Integer
        Public Inverted As Boolean
        Public LayerName As String
        Public Linetype As String
        Public LTScale As Double
        Public Mark As Boolean
        Public Radius As Double
        Public Row As Integer
        Public StartWidth As Double
        Public Suppressed As Boolean
        Public Tag As String
        Public Value As Double
        Public GUID As String
#End Region 'Members
#Region "Constructors"
        Public Sub New(aIndex As Integer)
            'init ---------------------------------------
            _Bulge = 0
            Col = 0
            ColID = 0
            Color = dxxColors.ByBlock
            EndWidth = 0
            Flag = String.Empty
            Index = 0
            Inverted = False
            LayerName = String.Empty
            Linetype = String.Empty
            LTScale = 0
            Mark = False
            Radius = 0
            Row = 0
            StartWidth = 0
            Suppressed = False
            Tag = String.Empty
            Value = 0
            GUID = String.Empty
            'init ---------------------------------------
            Index = aIndex
        End Sub


        Public Sub New(aVector As iVector)
            'init ---------------------------------------
            _Bulge = 0
            Col = 0
            ColID = 0
            Color = dxxColors.ByBlock
            EndWidth = 0
            Flag = String.Empty
            Index = 0
            Inverted = False
            LayerName = String.Empty
            Linetype = String.Empty
            LTScale = 0
            Mark = False
            Radius = 0
            Row = 0
            StartWidth = 0
            Suppressed = False
            Tag = String.Empty
            Value = 0
            GUID = String.Empty
            'init ---------------------------------------
            If aVector Is Nothing Then Return
            If TypeOf aVector Is dxfVector Then
                Dim v1 As dxfVector = DirectCast(aVector, dxfVector)
                Dim aVars As TVERTEXVARS = v1.Vars
                _Bulge = aVars._Bulge
                Col = aVars.Col
                ColID = aVars.ColID
                Color = aVars.Color
                EndWidth = aVars.EndWidth
                Flag = aVars.Flag
                Index = aVars.Index
                Inverted = aVars.Inverted
                LayerName = aVars.LayerName
                Linetype = aVars.Linetype
                LTScale = aVars.LTScale
                Mark = aVars.Mark
                Radius = aVars.Radius
                Row = aVars.Row
                Tag = aVars.Tag
                StartWidth = aVars.StartWidth
                Suppressed = aVars.Suppressed
                Value = aVars.Value
                GUID = aVars.GUID
            Else
                Try
                    Dim oV As Object = aVector
                    If dxfUtils.CheckProperty(oV, "Radius") Then

                        Radius = TVALUES.To_DBL(oV.Radius)
                    End If
                    If dxfUtils.CheckProperty(oV, "Tag") Then

                        Tag = TVALUES.To_STR(oV.Tag)
                    End If
                    If dxfUtils.CheckProperty(oV, "Flag") Then

                        Flag = TVALUES.To_STR(oV.Flag)
                    End If
                Catch ex As Exception
                    Console.WriteLine($"VERTEXVARS Creation Exception : {ex.Message}")
                End Try

            End If

        End Sub

        Friend Sub New(aVars As TVERTEXVARS)
            _Bulge = aVars._Bulge
            Col = aVars.Col
            ColID = aVars.ColID
            Color = aVars.Color
            EndWidth = aVars.EndWidth
            Flag = aVars.Flag
            Index = aVars.Index
            Inverted = aVars.Inverted
            LayerName = aVars.LayerName
            Linetype = aVars.Linetype
            LTScale = aVars.LTScale
            Mark = aVars.Mark
            Radius = aVars.Radius
            Row = aVars.Row
            Tag = aVars.Tag
            StartWidth = aVars.StartWidth
            Suppressed = aVars.Suppressed
            Value = aVars.Value
            GUID = aVars.GUID
        End Sub
        Public Sub New(aLinetype As String, Optional aColor As dxxColors = dxxColors.ByBlock, Optional aLTScale As Double = 1)
            'init ---------------------------------------

            _Bulge = 0
            Col = 0
            ColID = 0
            Color = dxxColors.ByBlock
            EndWidth = 0
            Flag = String.Empty
            Index = 0
            Inverted = False
            LayerName = String.Empty
            Linetype = String.Empty
            LTScale = 0
            Mark = False
            Radius = 0
            Row = 0
            StartWidth = 0
            Suppressed = False
            Tag = String.Empty
            Value = 0
            GUID = String.Empty
            'init ---------------------------------------

            If String.IsNullOrWhiteSpace(aLinetype) Then aLinetype = dxfLinetypes.ByBlock
            Linetype = aLinetype.Trim()
            LTScale = Math.Abs(aLTScale)
            Color = aColor
        End Sub
#End Region 'Constructors
#Region "Properties"
        Public Property Bulge As Double
            Get
                'Return _Bulge
                If Inverted Then Return -1 * _Bulge Else Return _Bulge
            End Get
            Set(value As Double)
                _Bulge = Math.Abs(value)
            End Set
        End Property
#End Region 'Properties
#Region "Methods"
        Public Function Clone() As TVERTEXVARS
            Return New TVERTEXVARS(Me)
        End Function


        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TVERTEXVARS(Me)
        End Function
#End Region 'Methods
    End Structure 'TVERTEXVARS

    Friend Structure TVERTEX
        Implements ICloneable
#Region "Members"
        Public Vars As TVERTEXVARS
        Public Vector As TVECTOR
#End Region 'Members
#Region "Constructors"
        Public Sub New(Optional aX As Double = 0, Optional aY As Double = 0, Optional aZ As Double = 0, Optional aVertexRadius As Double = 0, Optional bInverted As Boolean = False, Optional aTag As String = "", Optional aFlag As String = "")
            ' init ---------------------------------
            Vars = New TVERTEXVARS(-1)
            Vector = New TVECTOR(aX, aY, aZ)
            ' init ---------------------------------
            Vars.Radius = Math.Round(aVertexRadius, 8)
            Vars.Inverted = bInverted
            If aTag IsNot Nothing Then Vars.Tag = aTag
        End Sub
        Public Sub New(aVector As TVECTOR, aVars As TVERTEXVARS)
            ' init ---------------------------------
            Vars = New TVERTEXVARS(aVars)
            Vector = New TVECTOR(aVector)
            ' init ---------------------------------
        End Sub
        Public Sub New(aVector As TVECTOR)
            ' init ---------------------------------
            Vars = New TVERTEXVARS(-1)
            Vector = New TVECTOR(aVector)
            ' init ---------------------------------

        End Sub
        Public Sub New(aVertex As TVERTEX)
            ' init ---------------------------------
            Vars = New TVERTEXVARS(aVertex.Vars)
            Vector = New TVECTOR(aVertex.Vector)
            ' init ---------------------------------

        End Sub
        Public Sub New(aVector As iVector)
            ' init ---------------------------------
            Vars = New TVERTEXVARS(aVector)
            Vector = New TVECTOR(aVector)
            ' init ---------------------------------

        End Sub

        Public Sub New(aPlane As dxfPlane, Optional aX As Double = 0, Optional aY As Double = 0, Optional aZ As Double = 0, Optional aTag As String = "", Optional aFlag As String = "", Optional aRotation As Double = 0)
            ' init ---------------------------------
            Vars = New TVERTEXVARS()
            Vector = New TVECTOR(aPlane, aX, aY, aZ, aRotation)
            ' init ---------------------------------


            If Not String.IsNullOrEmpty(aTag) Then Tag = aTag
            If Not String.IsNullOrEmpty(aFlag) Then Flag = aFlag
        End Sub

        Friend Sub New(aPlane As TPLANE, Optional aX As Double = 0, Optional aY As Double = 0, Optional aZ As Double = 0, Optional aTag As String = "", Optional aFlag As String = "", Optional aRotation As Double = 0)
            ' init ---------------------------------
            Vars = New TVERTEXVARS()
            Vector = New TVECTOR(aPlane, aX, aY, aZ, aRotation)
            ' init ---------------------------------


            If Not String.IsNullOrEmpty(aTag) Then Tag = aTag
            If Not String.IsNullOrEmpty(aFlag) Then Flag = aFlag
        End Sub

#End Region 'Constructors
#Region "Properties"
        Public Property Code As Byte
            Get
                Return Vector.Code
            End Get
            Set(value As Byte)
                Vector.Code = value
            End Set
        End Property
        Public Property Bulge As Double
            Get
                Return Vars.Bulge
            End Get
            Set(value As Double)
                Vars.Bulge = value
            End Set
        End Property
        Public Property Col As Integer
            Get
                Return Vars.Col
            End Get
            Set(value As Integer)
                Vars.Col = value
            End Set
        End Property
        Public Property ColID As Integer
            Get
                Return Vars.ColID
            End Get
            Set(value As Integer)
                Vars.ColID = value
            End Set
        End Property
        Public Property Color As Integer
            Get
                Return Vars.Color
            End Get
            Set(value As Integer)
                Vars.Color = value
            End Set
        End Property
        Public Property EndWidth As Double
            Get
                Return Vars.EndWidth
            End Get
            Set(value As Double)
                Vars.EndWidth = value
            End Set
        End Property
        Public Property Flag As String
            Get
                If (String.IsNullOrEmpty(Vars.Flag)) Then Vars.Flag = ""
                Return Vars.Flag
            End Get
            Set(value As String)
                Vars.Flag = value
            End Set
        End Property
        Public Property Index As Integer
            Get
                Return Vars.Index
            End Get
            Set(value As Integer)
                Vars.Index = value
            End Set
        End Property
        Public Property Inverted As Boolean
            Get
                Return Vars.Inverted
            End Get
            Set(value As Boolean)
                Vars.Inverted = value
            End Set
        End Property
        Public ReadOnly Property Magnitude As Double
            '^returns the magnitude of the vector
            '~the magniture is the square root of the sum of the squares of the current coorddinates (X^2 + Y^2 + Z^2)
            Get
                Return Vector.Magnitude
            End Get
        End Property
        Public Property LayerName As String
            Get
                Return Vars.LayerName
            End Get
            Set(value As String)
                Vars.LayerName = value
            End Set
        End Property
        Public Property Linetype As String
            Get
                Return Vars.Linetype
            End Get
            Set(value As String)
                Vars.Linetype = value
            End Set
        End Property
        Public Property LTScale As Double
            Get
                Return Vars.LTScale
            End Get
            Set(value As Double)
                Vars.LTScale = value
            End Set
        End Property
        Public Property Mark As Boolean
            Get
                Return Vars.Mark
            End Get
            Set(value As Boolean)
                Vars.Mark = value
            End Set
        End Property
        Public Property Radius As Double
            Get
                Return Vars.Radius
            End Get
            Set(value As Double)
                Vars.Radius = value
            End Set
        End Property
        Public Property Rotation As Double
            Get
                Return Vector.Rotation
            End Get
            Set(value As Double)
                Vector.Rotation = value
            End Set
        End Property
        Public Property Row As Integer
            Get
                Return Vars.Row
            End Get
            Set(value As Integer)
                Vars.Row = value
            End Set
        End Property
        Public Property StartWidth As Double
            Get
                Return Vars.StartWidth
            End Get
            Set(value As Double)
                Vars.StartWidth = value
            End Set
        End Property
        Public Property Suppressed As Boolean
            Get
                Return Vars.Suppressed
            End Get
            Set(value As Boolean)
                Vars.Suppressed = value
            End Set
        End Property
        Public Property Tag As String
            Get
                Return Vars.Tag
            End Get
            Set(value As String)
                Vars.Tag = value
            End Set
        End Property

        Public Property GUID As String
            Get
                Return Vars.GUID
            End Get
            Set(value As String)
                Vars.GUID = value
            End Set
        End Property

        Public Property Value As Double
            Get
                Return Vars.Value
            End Get
            Set(value As Double)
                Vars.Value = value
            End Set
        End Property
        Public Property X As Double
            Get
                Return Vector.X
            End Get
            Set(value As Double)
                Vector.X = value
            End Set
        End Property
        Public Property Y As Double
            Get
                Return Vector.Y
            End Get
            Set(value As Double)
                Vector.Y = value
            End Set
        End Property
        Public Property Z As Double
            Get
                Return Vector.Z
            End Get
            Set(value As Double)
                Vector.Z = value
            End Set
        End Property
#End Region 'Properties
#Region "Methods"

        Public Function DisplaySettings(Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLinetype As String = "")
            If String.IsNullOrWhiteSpace(aLayer) Then aLayer = LayerName
            If String.IsNullOrWhiteSpace(aLinetype) Then aLinetype = Linetype
            If aColor = dxxColors.Undefined Then aColor = Color

            Return dxfDisplaySettings.Null(aLayer, aColor, aLinetype)
        End Function
        Public Function GetValue(aPropertyType As dxxVectorProperties) As Object
            Dim rInvalid As Boolean = False
            Return GetValue(aPropertyType, rInvalid)
        End Function
        Public Function GetValue(aPropertyType As dxxVectorProperties, ByRef rInvalid As Boolean) As Object
            rInvalid = False
            Select Case aPropertyType
                Case dxxVectorProperties.X
                    Return X
                Case dxxVectorProperties.Y
                    Return Y
                Case dxxVectorProperties.Z
                    Return Z
                Case dxxVectorProperties.Coordinates
                    Return "(" & X.ToString & "," & Y.ToString & "," & Z.ToString & ")"
                Case dxxVectorProperties.Radius
                    Return Radius
                Case dxxVectorProperties.Inverted
                    Return Inverted
                Case dxxVectorProperties.Color
                    Return Color
                Case dxxVectorProperties.StartWidth
                    Return StartWidth
                Case dxxVectorProperties.EndWidth
                    Return EndWidth
                Case dxxVectorProperties.Rotation
                    Return Rotation
                Case dxxVectorProperties.Tag
                    Return Tag
                Case dxxVectorProperties.Flag
                    Return Flag
                Case dxxVectorProperties.Value
                    Return Value
                Case dxxVectorProperties.Mark
                    Return Mark
                Case dxxVectorProperties.LayerName
                    Return LayerName
                Case dxxVectorProperties.Color
                    Return Color
                Case dxxVectorProperties.Linetype
                    Return Linetype
                Case dxxVectorProperties.LTScale
                    Return LTScale
                Case dxxVectorProperties.Suppressed
                    Return Suppressed
                Case dxxVectorProperties.Row
                    Return Row
                Case dxxVectorProperties.Col
                    Return Col
                Case Else
                    rInvalid = True
                    Return String.Empty
            End Select
        End Function
        Public Function SetValue(aPropertyType As dxxVectorProperties, aValue As Object) As Boolean
            Dim rInvalid As Boolean = False
            Return SetValue(aPropertyType, aValue, rInvalid)
        End Function
        Public Function SetValue(aPropertyType As dxxVectorProperties, aValue As Object, ByRef rInvalid As Boolean) As Boolean
            rInvalid = False
            Dim _rVal As Boolean
            Select Case aPropertyType
                Case dxxVectorProperties.X
                    If Not TVALUES.IsNumber(aValue) Then Return False
                    Dim dval As Double = TVALUES.To_DBL(aValue)
                    _rVal = dval <> Vector.X
                    If Not _rVal Then Vector.X = dval
                    Return _rVal
                Case dxxVectorProperties.Y
                    If Not TVALUES.IsNumber(aValue) Then Return False
                    Dim dval As Double = TVALUES.To_DBL(aValue)
                    _rVal = dval <> Vector.Y
                    If Not _rVal Then Vector.Y = dval
                    Return _rVal
                Case dxxVectorProperties.Z
                    If Not TVALUES.IsNumber(aValue) Then Return False
                    Dim dval As Double = TVALUES.To_DBL(aValue)
                    _rVal = dval <> Vector.Z
                    If Not _rVal Then Vector.Z = dval
                    Return _rVal
                Case dxxVectorProperties.Coordinates
                    Dim dval As String
                    If aValue IsNot Nothing Then dval = aValue.ToString Else dval = ""
                    If String.IsNullOrWhiteSpace(dval) Then Return False
                    _rVal = String.Compare(dval, GetValue(dxxVectorProperties.Coordinates), ignoreCase:=True) <> 0
                    If Not _rVal Then Vector = New TVECTOR(dval)
                    Return _rVal
                Case dxxVectorProperties.Radius
                    If Not TVALUES.IsNumber(aValue) Then Return False
                    Dim dval As Double = TVALUES.To_DBL(aValue)
                    _rVal = dval <> Vars.Radius
                    If Not _rVal Then Vars.Radius = dval
                    Return _rVal
                Case dxxVectorProperties.Inverted
                    Dim dval As Boolean = TVALUES.ToBoolean(aValue, Vars.Inverted)
                    _rVal = dval <> Vars.Inverted
                    If Not _rVal Then Vars.Inverted = dval
                    Return _rVal
                Case dxxVectorProperties.Color
                    If Not TVALUES.IsNumber(aValue) Then Return False
                    Dim dval As Integer = TVALUES.To_INT(aValue)
                    _rVal = dval <> Vars.Color
                    If Not _rVal Then Vars.Color = dval
                    Return _rVal
                Case dxxVectorProperties.StartWidth
                    If Not TVALUES.IsNumber(aValue) Then Return False
                    Dim dval As Double = TVALUES.To_DBL(aValue)
                    _rVal = dval <> Vars.StartWidth
                    If Not _rVal Then Vars.StartWidth = dval
                    Return _rVal
                Case dxxVectorProperties.EndWidth
                    If Not TVALUES.IsNumber(aValue) Then Return False
                    Dim dval As Double = TVALUES.To_DBL(aValue)
                    _rVal = dval <> EndWidth
                    If Not _rVal Then Vars.EndWidth = dval
                    Return _rVal
                Case dxxVectorProperties.Rotation
                    If Not TVALUES.IsNumber(aValue) Then Return False
                    Dim dval As Double = TVALUES.To_DBL(aValue)
                    _rVal = dval <> Vector.Rotation
                    If Not _rVal Then Vector.Rotation = dval
                    Return _rVal
                Case dxxVectorProperties.Tag
                    Dim dval As String
                    If aValue IsNot Nothing Then dval = aValue.ToString Else dval = ""
                    _rVal = dval <> Vars.Tag
                    If Not _rVal Then Vars.Tag = dval
                    Return _rVal
                Case dxxVectorProperties.Flag
                    Dim dval As String
                    If aValue IsNot Nothing Then dval = aValue.ToString Else dval = ""
                    _rVal = dval <> Vars.Flag
                    If Not _rVal Then Vars.Flag = dval
                    Return _rVal
                Case dxxVectorProperties.Value
                    _rVal = Vars.Value <> aValue
                    Vars.Value = aValue
                    Return _rVal
                Case dxxVectorProperties.Mark
                    Dim dval As Boolean = TVALUES.ToBoolean(aValue, Vars.Mark)
                    _rVal = dval <> Vars.Mark
                    If Not _rVal Then Vars.Mark = dval
                    Return _rVal
                Case dxxVectorProperties.LayerName
                    Dim dval As String
                    If aValue IsNot Nothing Then dval = aValue.ToString Else dval = ""
                    _rVal = dval <> Vars.LayerName
                    If Not _rVal Then Vars.LayerName = dval
                    Return _rVal
                Case dxxVectorProperties.Linetype
                    Dim dval As String
                    If aValue IsNot Nothing Then dval = aValue.ToString Else dval = ""
                    _rVal = dval <> Vars.Linetype
                    If Not _rVal Then Vars.Linetype = dval
                    Return _rVal
                Case dxxVectorProperties.LTScale
                    If Not TVALUES.IsNumber(aValue) Then Return False
                    Dim dval As Double = TVALUES.To_DBL(aValue)
                    _rVal = dval <> Vars.LTScale
                    If Not _rVal Then Vars.LTScale = dval
                    Return _rVal
                Case dxxVectorProperties.Suppressed
                    Dim dval As Boolean = TVALUES.ToBoolean(aValue, Vars.Suppressed)
                    _rVal = dval <> Vars.Suppressed
                    If Not _rVal Then Vars.Suppressed = dval
                    Return _rVal
                Case dxxVectorProperties.Row
                    If Not TVALUES.IsNumber(aValue) Then Return False
                    Dim dval As Integer = TVALUES.To_INT(aValue)
                    _rVal = dval <> Vars.Row
                    If Not _rVal Then Vars.Row = dval
                    Return _rVal
                Case dxxVectorProperties.Col
                    If Not TVALUES.IsNumber(aValue) Then Return False
                    Dim dval As Integer = TVALUES.To_INT(aValue)
                    _rVal = dval <> Vars.Col
                    If Not _rVal Then Vars.Col = dval
                    Return _rVal
                Case Else
                    rInvalid = True
                    Return False
            End Select
        End Function
        Public Function DistanceTo(A As TVECTOR, Optional aPrecis As Integer = -1) As Double
            Return A.DistanceTo(Vector, aPrecis)
        End Function
        Public Function DistanceTo(A As TVERTEX, Optional aPrecis As Integer = -1) As Double
            Return A.Vector.DistanceTo(Vector, aPrecis)
        End Function
        Public Function WithRespectTo(aPlane As TPLANE, Optional aPrecis As Integer = 8, Optional aScaleFactor As Double = 0.0, Optional bSuppressZ As Boolean = False) As TVERTEX
            Return New TVERTEX(Vector.WithRespectTo(aPlane, aPrecis, aScaleFactor, bSuppressZ), Vars)

        End Function
        Public Function WithRespectTo(aPlane As TPLANE, ByRef rDepth As Double, Optional aPrecis As Integer = 8, Optional bGetDepth As Boolean = False, Optional aScaleFactor As Double = 0.0, Optional bSuppressZ As Boolean = False) As TVERTEX
            Return New TVERTEX(Vector.WithRespectTo(aPlane, rDepth, aPrecis, bGetDepth, aScaleFactor, bSuppressZ), Vars)

        End Function
        Public Function WithRespectTo(aPlane As dxfPlane, Optional aPrecis As Integer = 8, Optional bGetDepth As Boolean = False, Optional aScaleFactor As Double = 0.0, Optional bSuppressZ As Boolean = False) As TVERTEX
            Return New TVERTEX(Vector.WithRespectTo(New TPLANE(aPlane), 0, aPrecis, bGetDepth, aScaleFactor, bSuppressZ), Vars)

        End Function
        Public Function WithRespectTo(aPlane As dxfPlane, ByRef rDepth As Double, Optional aPrecis As Integer = 8, Optional bGetDepth As Boolean = False, Optional aScaleFactor As Double = 0.0, Optional bSuppressZ As Boolean = False) As TVERTEX
            Return New TVERTEX(Vector.WithRespectTo(New TPLANE(aPlane), rDepth, aPrecis, bGetDepth, aScaleFactor, bSuppressZ), Vars)

        End Function

        Public Function SetCoordinates(Optional aX As Double? = Nothing, Optional aY As Double? = Nothing, Optional aZ As Double? = Nothing, Optional aPrecis As Integer = -1) As Boolean

            Dim newX As Double = IIf(aX.HasValue, aX.Value, X)
            Dim newY As Double = IIf(aY.HasValue, aY.Value, Y)
            Dim newZ As Double = IIf(aZ.HasValue, aZ.Value, Z)

            If aPrecis >= 0 Then
                aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
                newX = Math.Round(newX, aPrecis)
                newY = Math.Round(newY, aPrecis)
                newZ = Math.Round(newZ, aPrecis)
            End If

            Dim _rVal As Boolean = newX <> X Or newY <> Y Or newZ <> Z
            X = newX
            Y = newY
            Z = newZ
            Return _rVal
        End Function

        Public Overrides Function ToString() As String
            Dim _rVal As String
            If Z <> 0 Then
                _rVal = $"TVERTEX[{ X:0.000},{Y:0.000},{Vector.CodeString}"
            Else
                _rVal = $"TVERTEX[{ X:0.000},{Y:0.000},{Z:0.000},{Vector.CodeString}"
            End If
            If Vars.Radius = 0 Then _rVal += "]" Else _rVal += $",RAD{Vars.Radius:0.000}]"
            Return _rVal
        End Function
        Public Function Clone() As TVERTEX
            Return New TVERTEX(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TVERTEX(Me)
        End Function
        Public Function ComponentAlong(a As TVERTEX) As TVERTEX
            Dim _rVal As New TVERTEX(Me)
            '^returns the component of this vector along the  passed vector
            Dim numer As Double = (X * a.X + Y * a.Y + Z * a.Z) 'VectorDotProductDBL(A, A)
            Dim denom As Double = (a.X * a.X + a.Y * a.Y + a.Z * a.Z) 'VectorDotProductDBL(A, A)
            If denom <> 0 Then _rVal *= (numer / denom)
            Return _rVal
        End Function
        Public Function ComponentAlong(a As TVECTOR) As TVERTEX
            Dim _rVal As New TVERTEX(Me)
            '^returns the component of this vector along the  passed vector
            Dim numer As Double = X * a.X + Y * a.Y + Z * a.Z 'VectorDotProductDBL(A, A)
            Dim denom As Double = a.X * a.X + a.Y * a.Y + a.Z * a.Z 'VectorDotProductDBL(A, A)
            If denom <> 0 Then _rVal *= numer / denom
            Return _rVal
        End Function
        Public Shared Function FromObject(aSource As Object, Optional aProjectionPlane As dxfPlane = Nothing, Optional aProjectDirection As dxfDirection = Nothing, Optional aVector As dxfVector = Nothing, Optional aChangeX As Double = 0.0, Optional aChangeY As Double = 0.0, Optional aChangeZ As Double = 0.0) As TVERTEX
            '#1the object with X, Y , Z properties to move to
            '^returns a vector whose coordinates are taken from the passed Object
            '~the passed object must have a numeric X and Y property. Z is optional
            Dim v1 As TVERTEX = TVERTEX.Zero
            Dim tname As String = TypeName(aSource).ToUpper()
            If aSource IsNot Nothing Then
                Try
                    Select Case tname
                        Case "TVECTOR"
                            Dim src As TVECTOR = aSource
                            v1.Vector = New TVECTOR(src)
                        Case "TVERTEX"
                            Dim vrt As TVERTEX = aSource
                            v1 = New TVERTEX(vrt)
                        Case "DXFVECTOR"
                            Dim P1 As dxfVector = aSource
                            Dim vrt As TVERTEX = P1.VertexV
                            v1 = New TVERTEX(vrt)
                        Case "STRING"
                            v1.Vector = TVECTOR.FromString(aSource)
                        Case "DXFDIRECTION"
                            Dim P2 As dxfDirection = aSource
                            v1.Vector = P2.Strukture
                        Case "DXFPLANE"
                            Dim aPl As dxfPlane = aSource
                            v1.Vector = aPl.OriginV
                        Case "DXFRECTANGLE"
                            Dim aRect As dxfRectangle
                            aRect = aSource
                            v1.Vector = aRect.CenterV
                        Case "COLDXFVECTORS"
                            Dim Ps As colDXFVectors
                            Ps = aSource
                            v1 = Ps.ItemVertex(1)
                        Case "DXFIMAGE"
                            Dim aImg As dxfImage = aSource
                            Dim aPl As dxfPlane = aImg.UCS
                            v1.Vector = aPl.OriginV
                        Case Else
                            If dxfUtils.CheckProperty(aSource, "X") Then
                                Try
                                    v1.X = TVALUES.ToDouble(aSource.X)
                                Catch ex As Exception
                                    v1.X = 0
                                End Try
                            End If
                            If dxfUtils.CheckProperty(aSource, "Y") Then
                                Try
                                    v1.Y = TVALUES.ToDouble(aSource.Y)
                                Catch ex As Exception
                                    v1.Y = 0
                                End Try
                            End If
                            If dxfUtils.CheckProperty(aSource, "Z") Then
                                Try
                                    v1.Z = TVALUES.ToDouble(aSource.Z)
                                Catch ex As Exception
                                    v1.Z = 0
                                End Try
                            End If
                    End Select
                Catch ex As Exception
                    v1 = TVERTEX.Zero
                End Try
            End If
            If aChangeX <> 0 Or aChangeY <> 0 Or aChangeZ <> 0 Then v1 += New TVECTOR(aChangeX, aChangeY, aChangeZ)
            If aProjectionPlane IsNot Nothing Then
                v1.Vector = dxfProjections.ToPlane(v1.Vector, aProjectionPlane.Strukture, aProjectDirection)
            End If
            Return v1
        End Function
        Public Function ProjectedTo(aLine As TLINE) As TVERTEX
            Dim rOrthoDirection As TVECTOR
            Dim rDistance As Double = 0.0
            Dim rPointIsOnSegment As Boolean = False
            Dim rDirectionPositive As Boolean = False
            Return ProjectedTo(aLine, rOrthoDirection, rDistance, rPointIsOnSegment, rDirectionPositive)
        End Function
        Public Function ProjectedTo(aLine As TLINE, ByRef rOrthoDirection As TVECTOR) As TVERTEX
            Dim rDistance As Double = 0.0
            Dim rPointIsOnSegment As Boolean = False
            Dim rDirectionPositive As Boolean = False
            Return ProjectedTo(aLine, rOrthoDirection, rDistance, rPointIsOnSegment, rDirectionPositive)
        End Function
        Public Function ProjectedTo(aLine As TLINE, ByRef rDistance As Double) As TVERTEX
            Dim rOrthoDirection As TVECTOR
            Dim rPointIsOnSegment As Boolean = False
            Dim rDirectionPositive As Boolean = False
            Return ProjectedTo(aLine, rOrthoDirection, rDistance, rPointIsOnSegment, rDirectionPositive)
        End Function
        Public Function ProjectedTo(aLine As TLINE, ByRef rOrthoDirection As TVECTOR, ByRef rDistance As Double, ByRef rPointIsOnSegment As Boolean, ByRef rDirectionPositive As Boolean) As TVERTEX
            '#1the subject line
            '#2returns then orthogal distance to the segment from the vector
            '#3returns the orthogonal direction to the segment from the vector
            '#4returns true if then returned vector lines on the passed segment
            '#5returns true if the returned vector lies in the same direction from the start point as the end point (positive direction)
            '^returns a clone of this vector projected orthogonally onto the passed  line
            Dim _rVal As New TVERTEX(Me)
            _rVal.ProjectTo(aLine, rOrthoDirection, rDistance, rPointIsOnSegment, rDirectionPositive)
            Return _rVal
        End Function
        Public Function ProjectTo(aLine As TLINE) As Boolean
            Dim rOrthoDirection As TVECTOR
            Dim rDistance As Double = 0.0
            Dim rPointIsOnSegment As Boolean = False
            Dim rDirectionPositive As Boolean = False
            Return ProjectTo(aLine, rOrthoDirection, rDistance, rPointIsOnSegment, rDirectionPositive)
        End Function
        Public Function ProjectTo(aLine As TLINE, ByRef rOrthoDirection As TVECTOR) As Boolean
            Dim rDistance As Double = 0.0
            Dim rPointIsOnSegment As Boolean = False
            Dim rDirectionPositive As Boolean = False
            Return ProjectTo(aLine, rOrthoDirection, rDistance, rPointIsOnSegment, rDirectionPositive)
        End Function
        Public Function ProjectTo(aLine As TLINE, ByRef rDistance As Double) As Boolean
            Dim rOrthoDirection As TVECTOR
            Dim rPointIsOnSegment As Boolean = False
            Dim rDirectionPositive As Boolean = False
            Return ProjectTo(aLine, rOrthoDirection, rDistance, rPointIsOnSegment, rDirectionPositive)
        End Function
        Public Function ProjectTo(aLine As TLINE, ByRef rOrthoDirection As TVECTOR, ByRef rDistance As Double, ByRef rPointIsOnSegment As Boolean, ByRef rDirectionPositive As Boolean) As Boolean
            '#1the subject line
            '#2returns then orthogal distance to the segment from the vector
            '#3returns the orthogonal direction to the segment from the vector
            '#4returns true if then returned vector lines on the passed segment
            '#5returns true if the returned vector lies in the same direction from the start point as the end point (positive direction)
            '^projects this vector orthogonally onto the passed  line
            Dim vnew As TVECTOR = dxfProjections.ToLine(Vector, aLine, rDistance, rPointIsOnSegment, rDirectionPositive)
            Return Vector.SetCoordinates(vnew.X, vnew.Y, vnew.Z)
        End Function
        Public Function ProjectTo(aPlane As TPLANE) As Boolean
            Dim aDirection As TVECTOR = aPlane.ZDirection
            Dim rDistance As Double = 0.0
            Dim rAntiNormal As Boolean = False
            Return ProjectTo(aPlane, aDirection, rDistance, rAntiNormal)
        End Function
        Public Function ProjectTo(aPlane As TPLANE, aDirection As TVECTOR) As Boolean
            Dim rDistance As Double = 0.0
            Dim rAntiNormal As Boolean = False
            Return ProjectTo(aPlane, aDirection, rDistance, rAntiNormal)
        End Function
        Public Function ProjectTo(aPlane As TPLANE, aDirection As TVECTOR, ByRef rDistance As Double) As Boolean
            Dim rAntiNormal As Boolean = False
            Return ProjectTo(aPlane, aDirection, rDistance, rAntiNormal)
        End Function
        Public Function ProjectTo(aPlane As TPLANE, aDirection As TVECTOR, ByRef rDistance As Double, ByRef rAntiNormal As Boolean) As Boolean
            '#1the plane to project to
            '#2the direction To use For the projects
            '#3returns the distance the point was moved
            '#4returns True if the points was moved in the opposite direction of the project direction
            '^projects the passed vector to the passed plane
            '~if the passed direction is a null vector then the direction is assumed as the normal of the plane
            Dim vnew As TVECTOR = dxfProjections.ToPlane(Vector, aPlane, aDirection, rDistance, rAntiNormal)
            Dim _rVal As Boolean = vnew <> Vector
            Vector = vnew
            Return _rVal
        End Function
        Public Function ProjectedTo(aPlane As TPLANE, aDirection As TVECTOR, ByRef rDistance As Double, ByRef rAntiNormal As Boolean) As TVERTEX
            '#1the plane to project to
            '#2the direction To use For the projects
            '#3returns the distance the point was moved
            '#4returns True if the points was moved in the opposite direction of the project direction
            '^returns a clone of the vector projected to the passed vector
            '~if the passed direction is a null vector then the direction is assumed as the normal of the plane
            Dim _rVal As New TVERTEX(Me)
            _rVal.Vector = dxfProjections.ToPlane(Vector, aPlane, aDirection, rDistance, rAntiNormal)
            Return _rVal
        End Function
        Public Function ProjectTo(aPlane As TPLANE, ByRef rDistance As Double, ByRef rAntiNormal As Boolean) As Boolean
            '#1the plane to project to
            '#2returns the distance the point was moved
            '#3returns True if the points was moved in the opposite direction of the planes Z axis
            '^projects the passed vector to the passed plane
            '~the project direction is assumed to be the z direction of the plane
            Return ProjectTo(aPlane, aPlane.ZDirection, rDistance, rAntiNormal)
        End Function
        Public Function ProjectedTo(aPlane As TPLANE) As TVERTEX
            Dim rDistance As Double = 0.0
            Dim rAntiNormal As Boolean = False
            Return ProjectedTo(aPlane, aPlane.ZDirection, rDistance, rAntiNormal)
        End Function
        Public Function ProjectedTo(aPlane As TPLANE, ByRef rDistance As Double, ByRef rAntiNormal As Boolean) As TVERTEX
            '#1the plane to project to
            '#2returns the distance the point was moved
            '#3returns True if the points was moved in the opposite direction of the planes Z axis
            '^returns a clone projected to the passed plane
            '~the project direction is assumed to be the z direction of the plane
            Dim _rVal As New TVERTEX(Me)
            _rVal.ProjectTo(aPlane, aPlane.ZDirection, rDistance, rAntiNormal)
            Return _rVal
        End Function
#End Region 'Methods
#Region "Shared Methods"
        Friend Shared ReadOnly Property Zero As TVERTEX
            Get
                Return New TVERTEX(0, 0, 0) With {.Index = 0}
            End Get
        End Property
        Public Shared Function StyleName(aSt As dxxVertexStyles) As String
            If aSt > dxxVertexStyles.CLOSEPATH Then aSt -= dxxVertexStyles.CLOSEPATH
            Return dxfEnums.Description(aSt)
        End Function
        Public Shared Function PropertyValue(A As TVERTEX, aPropEnum As dxxVectorProperties, Optional aPrecis As Integer = -1) As Object
            Dim rUnknowProp As Boolean = False
            Dim rName As String = String.Empty
            Return PropertyValue(A, aPropEnum, aPrecis, rUnknowProp, rName)
        End Function
        Public Shared Function PropertyValue(A As TVERTEX, aPropEnum As dxxVectorProperties, aPrecis As Integer, ByRef rUnknowProp As Boolean) As Object
            Dim rName As String = String.Empty
            Return PropertyValue(A, aPropEnum, aPrecis, rUnknowProp, rName)
        End Function
        Public Shared Function PropertyValue(A As TVERTEX, aPropEnum As dxxVectorProperties, aPrecis As Integer, ByRef rUnknowProp As Boolean, ByRef rName As String) As Object
            Dim _rVal As Object
            rUnknowProp = False
            rName = dxfEnums.Description(aPropEnum)

            Dim bPrecis As Integer = -1
            Select Case aPropEnum
                Case dxxVectorProperties.X
                    _rVal = A.X
                    bPrecis = aPrecis
                Case dxxVectorProperties.Y
                    _rVal = A.Y
                    bPrecis = aPrecis
                Case dxxVectorProperties.Z
                    _rVal = A.Z
                    bPrecis = aPrecis
                Case dxxVectorProperties.Inverted
                    _rVal = A.Inverted
                Case dxxVectorProperties.EndWidth
                    _rVal = A.EndWidth
                    bPrecis = aPrecis
                Case dxxVectorProperties.StartWidth
                    _rVal = A.StartWidth
                    bPrecis = aPrecis
                Case dxxVectorProperties.Flag
                    _rVal = A.Flag
                Case dxxVectorProperties.Mark
                    _rVal = A.Mark
                Case dxxVectorProperties.Radius
                    _rVal = A.Radius
                    bPrecis = aPrecis
                Case dxxVectorProperties.Tag
                    _rVal = A.Tag
                Case dxxVectorProperties.Value
                    _rVal = A.Value
                Case dxxVectorProperties.Rotation
                    _rVal = A.Vector.Rotation
                    bPrecis = aPrecis
                Case dxxVectorProperties.Color
                    _rVal = A.Color
                Case dxxVectorProperties.LayerName
                    _rVal = A.LayerName
                Case dxxVectorProperties.Linetype
                    _rVal = A.Linetype
                Case dxxVectorProperties.LTScale
                    _rVal = A.LTScale
                    bPrecis = aPrecis
                Case dxxVectorProperties.Suppressed
                    _rVal = A.Suppressed
                Case dxxVectorProperties.Row
                    _rVal = A.Row
                Case dxxVectorProperties.Col
                    _rVal = A.Col
                Case Else
                    rUnknowProp = True
                    _rVal = ""
            End Select
            If _rVal Is Nothing Then _rVal = ""
            If bPrecis >= 0 Then _rVal = Math.Round(_rVal, TVALUES.LimitedValue(bPrecis, 0, 15))
            Return _rVal
        End Function
#End Region 'Share Methods
#Region "Operators"
        Public Shared Operator +(A As TVERTEX, B As TVERTEX) As TVERTEX
            Dim _rVal As New TVERTEX(A)
            _rVal.Vector += B.Vector
            Return _rVal
        End Operator
        Public Shared Operator -(A As TVERTEX, B As TVERTEX) As TVERTEX
            Dim _rVal As New TVERTEX(A)
            _rVal.Vector -= B.Vector
            Return _rVal
        End Operator
        Public Shared Operator *(A As TVERTEX, aScaler As Double) As TVERTEX
            Dim _rVal As New TVERTEX(A)
            _rVal.Vector *= aScaler
            Return _rVal
        End Operator
        Public Shared Operator /(A As TVERTEX, aScaler As Double) As TVERTEX
            Dim _rVal As New TVERTEX(A)
            _rVal.Vector /= aScaler
            Return _rVal
        End Operator
        Public Shared Operator *(A As TVERTEX, B As TVERTEX) As TVERTEX
            Dim _rVal As New TVERTEX(A)
            _rVal.Vector *= B.Vector
            Return _rVal
        End Operator
        Public Shared Operator =(A As TVERTEX, B As TVERTEX) As Boolean
            Return A.Vector = B.Vector
        End Operator
        Public Shared Operator <>(A As TVERTEX, B As TVERTEX) As Boolean
            Return A.Vector <> B.Vector
        End Operator

        Public Shared Widening Operator CType(aVertex As TVERTEX) As dxfVector
            Return New dxfVector(aVertex)
        End Operator
#End Region 'OPerators
    End Structure 'TVERTEX

    Friend Structure TVERTICES
        Implements ICloneable
#Region "Members"
        Private _Init As Boolean
        Private _Members() As TVERTEX
#End Region 'Members
#Region "Constructors"
        Public Sub New(aCount As Integer)
            'init -----------------------
            _Init = True
            ReDim _Members(-1)
            'init -----------------------
            If aCount <= 0 Then Return
            ReDim _Members(0 To aCount - 1)
            For i As Integer = 0 To aCount - 1
                _Members(i) = TVERTEX.Zero
            Next

        End Sub
        Public Sub New(aVector As TVECTOR)
            'init -----------------------
            _Init = True
            ReDim _Members(-1)
            'init -----------------------
            Add(New TVERTEX(aVector))
        End Sub
        Public Sub New(aVertex As TVERTEX)
            'init -----------------------
            _Init = True
            ReDim _Members(-1)
            'init -----------------------
            Add(New TVERTEX(aVertex))
        End Sub
        Public Sub New(aVertex As TVERTEX, bVertex As TVERTEX)
            'init -----------------------
            _Init = True
            ReDim _Members(-1)
            'init -----------------------
            Add(New TVERTEX(aVertex))
            Add(New TVERTEX(bVertex))
        End Sub

        Public Sub New(aVectors As IEnumerable(Of iVector))
            'init -----------------------
            _Init = True
            ReDim _Members(-1)
            'init -----------------------
            If aVectors Is Nothing Then Return
            For Each v1 As iVector In aVectors
                Add(New TVERTEX(v1))
            Next

        End Sub

        Public Sub New(aVertices As TVERTICES)
            'init -----------------------
            _Init = True
            ReDim _Members(-1)
            'init -----------------------
            If aVertices.Count > 0 Then _Members = aVertices._Members.Clone()

        End Sub

        Public Sub New(aVectors As TVECTORS)
            'init -----------------------
            _Init = True
            ReDim _Members(-1)
            'init -----------------------
            If aVectors.Count <= 0 Then Return
            For i As Integer = 1 To aVectors.Count
                Add(New TVERTEX(aVectors.Item(i)))
            Next

        End Sub
#End Region 'Constructors

#Region "Properties"
        Public ReadOnly Property Count As Integer
            Get
                If Not _Init Then
                    _Init = True
                    ReDim _Members(-1)
                End If
                Return _Members.Count
            End Get
        End Property
        Public ReadOnly Property First As TVERTEX
            Get
                Return Item(1)
            End Get
        End Property
        Public ReadOnly Property Last As TVERTEX
            Get
                Return Item(Count)
            End Get
        End Property



#End Region 'Properties

#Region "Methods"

        Public Function Vector(aIndex As Integer) As TVECTOR
            '^returns a member (base 1)
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return TVECTOR.Zero
            End If
            _Members(aIndex - 1).Index = aIndex
            Return _Members(aIndex - 1).Vector

        End Function


        Public Sub SetRotation(Value As Double, aIndex As Integer)
            '^returns a member (base 1)
            If aIndex < 1 Or aIndex > Count() Then Return
            _Members(aIndex - 1).Vector.Rotation = Value
        End Sub

        Public Sub SetVector(Value As TVECTOR, aIndex As Integer)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1).Vector = Value
            _Members(aIndex - 1).Index = aIndex
        End Sub
        Public Function X(aIndex As Integer) As Double
            '^returns a member (base 1)

            If aIndex < 1 Or aIndex > Count Then Return 0
            Return _Members(aIndex - 1).X

        End Function

        Public Function Y(aIndex As Integer) As Double
            '^returns a member (base 1)

            If aIndex < 1 Or aIndex > Count Then Return 0
            Return _Members(aIndex - 1).Y
        End Function
        Public Function Z(aIndex As Integer) As Double
            '^returns a member (base 1)

            If aIndex < 1 Or aIndex > Count Then Return 0
            Return _Members(aIndex - 1).Z
        End Function
        Public Sub SetX(Value As Double, aIndex As Integer)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1).X = Value
        End Sub
        Public Sub SetY(Value As Double, aIndex As Integer)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1).Y = Value
        End Sub
        Public Sub SetZ(Value As Double, aIndex As Integer)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1).Z = Value
        End Sub
        Public Function Suppressed(aIndex As Integer) As Boolean
            '^returns a member (base 1)

            If aIndex < 1 Or aIndex > Count Then Return False
            Return _Members(aIndex - 1).Suppressed

        End Function

        Public Function Inverted(aIndex As Integer) As Boolean
            '^returns a member (base 1)

            If aIndex < 1 Or aIndex > Count Then Return False
            Return _Members(aIndex - 1).Inverted

        End Function



        Public Sub SetInverted(Value As Boolean, aIndex As Integer)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1).Inverted = Value
        End Sub
        Public Sub SetMark(Value As Boolean, aIndex As Integer)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1).Mark = Value
        End Sub
        Public Sub SetSuppressed(Value As Boolean, aIndex As Integer)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1).Suppressed = Value
        End Sub

        Public Function Mark(aIndex As Integer) As Boolean
            '^returns a member (base 1)

            If aIndex < 1 Or aIndex > Count Then Return False
            Return _Members(aIndex - 1).Mark

        End Function
        Public Function Radius(aIndex As Integer) As Double
            '^returns a member (base 1)
            If aIndex < 1 Or aIndex > Count Then Return 0
            Return _Members(aIndex - 1).Radius

        End Function

        Public Function StartWidth(aIndex As Integer) As Double
            '^returns a member (base 1)

            If aIndex < 1 Or aIndex > Count Then Return 0
            Return _Members(aIndex - 1).StartWidth

        End Function
        Public Function EndWidth(aIndex As Integer) As Double
            '^returns a member (base 1)
            If aIndex < 1 Or aIndex > Count Then Return 0
            Return _Members(aIndex - 1).EndWidth
        End Function

        Public Function Flag(aIndex As Integer) As String
            '^returns a member (base 1)
            If aIndex < 1 Or aIndex > Count Then Return String.Empty
            Return _Members(aIndex - 1).Flag

        End Function
        Public Function Tag(aIndex As Integer) As String
            '^returns a member (base 1)

            If aIndex < 1 Or aIndex > Count Then Return String.Empty
            Return _Members(aIndex - 1).Tag

        End Function

        Public Function Value(aIndex As Integer) As Double
            '^returns a member (base 1)

            If aIndex < 1 Or aIndex > Count Then Return 0
            Return _Members(aIndex - 1).Value

        End Function
        Public Sub SetValue(aValue As Double, aIndex As Integer)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1).Value = aValue
        End Sub

        Public Sub SetFlag(aValue As String, aIndex As Integer, Optional bMark As Boolean? = Nothing)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1).Flag = aValue
            If (bMark.HasValue) Then _Members(aIndex - 1).Mark = bMark.Value
        End Sub

        Public Sub SetEndWidth(aValue As Double, aIndex As Integer)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1).EndWidth = aValue
        End Sub
        Public Sub SetRadius(aValue As Double, aIndex As Integer)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1).Radius = aValue
        End Sub
        Public Sub SetStartWidth(aValue As Double, aIndex As Integer)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1).StartWidth = aValue
        End Sub


        Public Function Item(aIndex As Integer) As TVERTEX
            '^returns a member (base 1)
            If aIndex < 1 Or aIndex > Count Then
                Return TVERTEX.Zero
            End If
            _Members(aIndex - 1).Index = aIndex
            Return _Members(aIndex - 1)
        End Function
        Public Sub SetItem(aIndex As Integer, aVertex As TVERTEX)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1) = aVertex
            _Members(aIndex - 1).Index = aIndex
        End Sub
        Public Function Bounds(aPlane As TPLANE) As TPLANE


            Dim aHt As Double
            Dim aWd As Double
            Dim _rVal As New TPLANE(aPlane) With {.Origin = PlanarCenter(aPlane, aWd, aHt)}
            _rVal.Height = aHt
            _rVal.Width = aWd
            Return _rVal
        End Function
        Public Function CounterClockwise(aPlane As TPLANE, aBaseAngle As Double) As Boolean
            Return Clockwise(aPlane, aBaseAngle, bReverseSort:=True)
        End Function
        Public Function Clockwise(aPlane As TPLANE, aBaseAngle As Double, Optional bReverseSort As Boolean = False) As Boolean
            Dim _rVal As Boolean
            If Count <= 1 Or Not aPlane.DirectionsAreDefined Then Return _rVal
            Dim v1 As TVERTEX
            Dim vp As TVECTOR
            Dim ang1 As Double
            Dim cp As TVECTOR = aPlane.Origin
            Dim d1 As Double
            Dim tpl As Tuple(Of Double, TVERTEX)
            Dim srt As New List(Of Tuple(Of Double, TVERTEX))
            Dim xDir As TVECTOR = aPlane.XDirection
            Dim bDir As TVECTOR
            Dim aN As TVECTOR = aPlane.ZDirection
            If aBaseAngle <> 0 Then xDir.RotateAbout(aN, aBaseAngle, False, True)
            For i As Integer = 1 To Count
                v1 = Item(i)
                vp = v1.Vector.WithRespectTo(aPlane)
                bDir = cp.DirectionTo(vp, False, rDistance:=d1)
                If d1 = 0 Then
                    ang1 = 0
                Else
                    ang1 = xDir.AngleTo(bDir, aN)
                    If ang1 = 360 Then ang1 = 0
                End If
                srt.Add(New Tuple(Of Double, TVERTEX)(ang1, v1))
            Next i
            srt.Sort(Function(tupl1 As Tuple(Of Double, TVERTEX), tupl2 As Tuple(Of Double, TVERTEX)) tupl1.Item1.CompareTo(tupl2.Item1))
            If Not bReverseSort Then srt.Reverse()
            For i As Integer = 1 To Count
                tpl = srt.Item(i - 1)
                _Members(i - 1) = tpl.Item2
            Next
            Return _rVal
        End Function
        Public Function PlanarCenter(aPlane As TPLANE, ByRef rXSpan As Double, ByRef rYSpan As Double, Optional bRedfineVectors As Boolean = False) As TVECTOR

            If TPLANE.IsNull(aPlane) Then aPlane = TPLANE.World
            '#1the plane to test
            '#2returns the horizontal span of the vectors in the collection with respect to the calulated center and the horizontal direction of the passed plane
            '#3returns the vertical span of the vectors in the collection with respect to the calulated center and the vertical direction of the passed plane
            '^the center of all the members with respect to the horizontal and vertical directions of the passed plane
            rXSpan = 0
            rYSpan = 0
            If Count = 0 Then Return aPlane.Origin
            Dim v1 As TVERTEX
            Dim v2 As TVERTEX
            Dim UL As TVECTOR
            Dim LR As TVECTOR
            Dim aLims As TLIMITS
            For i As Integer = 1 To Count
                v1 = _Members(i - 1)
                v2 = v1.WithRespectTo(aPlane)
                If bRedfineVectors Then SetItem(i, v2)
                If i = 1 Then
                    aLims = New TLIMITS(v2.Vector)
                Else
                    aLims.Update(v2.Vector)
                End If
            Next
            rXSpan = aLims.Width
            rYSpan = aLims.Height
            UL = aPlane.Origin + aPlane.XDirection * aLims.Left
            UL += aPlane.YDirection * aLims.Top
            LR = aPlane.Origin + aPlane.XDirection * aLims.Right
            LR += aPlane.YDirection * aLims.Bottom
            Return UL.Interpolate(LR, 0.5)
        End Function
        Public Function Add(aVertex As TVERTEX, Optional aCode As Byte? = Nothing) As TVERTEX
            If Count >= Integer.MaxValue Then Return Nothing
            Array.Resize(_Members, Count + 1)
            _Members(_Members.Count - 1) = New TVERTEX(aVertex)
            If aCode.HasValue Then _Members(_Members.Count - 1).Code = aCode.Value
            Return _Members(_Members.Count - 1)
        End Function
        Public Function Add(aVertex As TVECTOR) As TVERTEX
            If Count >= Integer.MaxValue Then Return Nothing
            Array.Resize(_Members, Count + 1)
            _Members(_Members.Count - 1) = New TVERTEX(aVertex)
            Return _Members(_Members.Count - 1)
        End Function
        Public Sub Insert(aVertex As TVERTEX, aInsertAfter As Integer)
            Dim i As Integer
            Dim idx As Integer
            Dim v1 As New TVERTEX(aVertex)
            Dim newMems(0) As TVERTEX
            Dim j As Integer
            System.Array.Resize(newMems, Count + 1)
            If aInsertAfter <= 1 Then
                idx = 1
            Else
                idx = aInsertAfter
                If idx > Count Then idx = Count
            End If
            For i = 1 To Count
                newMems(j) = _Members(i - 1)
                j += 1
                If i = idx Then
                    newMems(j) = aVertex
                    j += 1
                End If
            Next i
            _Members = newMems
        End Sub
        Public Function Add(aVector As TVECTOR, Optional aVertexRadius As Double? = Nothing, Optional aInverted As Boolean = False, Optional aCode As Byte? = Nothing) As TVERTEX
            If Count >= Integer.MaxValue Then Return Nothing
            Dim v1 As New TVERTEX(aVector) With {
                    .Inverted = aInverted
                }
            If aVertexRadius.HasValue Then
                v1.Radius = Math.Round(aVertexRadius.Value, 8)
                If v1.Radius < 0 Then
                    v1.Radius = Math.Abs(v1.Radius)
                    v1.Inverted = True
                End If
            End If
            If aCode.HasValue Then v1.Vector.Code = aCode.Value
            Return Add(v1)
        End Function
        Public Sub Add(Optional aX As Double = 0, Optional aY As Double = 0, Optional aZ As Double = 0, Optional aVertexRadius As Double? = Nothing, Optional aInverted As Boolean? = Nothing, Optional aCode As Byte? = Nothing, Optional aBulge As Double = 0)
            Dim v1 As New TVERTEX(aX, aY, aZ) With {
                    .Bulge = aBulge
                }
            If aVertexRadius.HasValue Then
                v1.Radius = TVALUES.To_DBL(aVertexRadius.Value, aPrecis:=8)
                If v1.Radius < 0 Then
                    v1.Radius = Math.Abs(v1.Radius)
                    v1.Inverted = True
                End If
            End If
            If aInverted.HasValue Then
                v1.Inverted = aInverted.Value
            End If
            If aCode.HasValue Then v1.Vector.Code = aCode.Value
            Add(v1)
        End Sub
        Public Sub Clear()
            _Init = True
            ReDim _Members(-1)

        End Sub
        Public Function GetAtCoordinate(Optional aX As Double? = Nothing, Optional aY As Double? = Nothing, Optional aZ As Double? = Nothing, Optional aPrecis As Integer = 3, Optional bJustOne As Boolean = False) As TVERTICES
            Dim _rVal As New TVERTICES(0)
            '#1the subject vertices
            '#2the X coordinate to match
            '#3the Y coordinate to match
            '#4the Z coordinate to match
            '^searchs for and returns vertices from the collection whose coordinates match the passed coordinates
            '~if an any of the ordinates (X, Y or Z) are not passed or are not numeric they are not used in the comparison.
            '~say ony an X value is passed, then all the vertices with the same X ordinate are returned regarless of their
            '~respective Y and Z ordinate values.
            Try
                If Count <= 0 Then Return _rVal
                aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
                Dim doX As Boolean = aX.HasValue
                Dim doY As Boolean = aY.HasValue
                Dim doZ As Boolean = aZ.HasValue
                Dim isMatchX As Boolean
                Dim isMatchY As Boolean
                Dim isMatchZ As Boolean
                Dim xx As Double
                Dim yy As Double
                Dim zz As Double
                Dim v1 As TVECTOR
                Dim aDif As Double
                Dim i As Integer
                If doX Then xx = aX.Value
                If doY Then yy = aY.Value
                If doZ Then zz = aZ.Value
                If Not doX And Not doY And Not doZ Then Return _rVal
                For i = 1 To Count
                    v1 = Vector(i)
                    isMatchX = True
                    isMatchY = True
                    isMatchZ = True
                    If doX Then
                        aDif = Math.Round(v1.X - xx, aPrecis)
                        isMatchX = (aDif = 0)
                    End If
                    If doY Then
                        aDif = Math.Round(v1.Y - yy, aPrecis)
                        isMatchY = (aDif = 0)
                    End If
                    If doZ Then
                        aDif = Math.Round(v1.Z - zz, aPrecis)
                        isMatchZ = (aDif = 0)
                    End If
                    If isMatchX And isMatchY And isMatchZ Then
                        _rVal.Add(_Members(i - 1))
                        If bJustOne Then Exit For
                    End If
                Next i
                Return _rVal
            Catch ex As Exception
                Return _rVal
            End Try
        End Function
        Public Function GetByTagList(aTags As String, Optional aDelimiter As String = ",", Optional bContainsString As Boolean = False, Optional aFlag As Object = Nothing, Optional bRemove As Boolean = False, Optional bReturnInverse As Boolean = False, Optional aValue As Double? = Nothing) As TVERTICES
            Dim _rVal As New TVERTICES(0)
            '#1the list of tags to search for
            '#2the delimiter that seperates the values in the list
            '#3flag to include any member whose tag string contains the passed search strings instead of a full string match
            '#4a flag value to apply to the search
            '#5a entity type to match
            '#6flag to to remove the matches from the collection
            '#7flag to return the members that don't match the search criteria
            '#8an optional value to include in the search criteria
            '^returns all the entities that match the search criteria

            Dim aStr As String = String.Empty
            Dim bTest As Boolean = aFlag IsNot Nothing

            Dim bTestVal As Boolean = aValue IsNot Nothing
            If bTestVal Then bTestVal = aValue.HasValue
            Dim tVals As TVALUES = TLISTS.ToValues(aTags, aDelimiter)

            Dim vIDs As New Dictionary(Of Integer, Integer)
            If bTest Then aStr = aFlag.ToString()
            For i As Integer = 1 To Count
                Dim aMem As TVERTEX = Item(i)
                For j As Integer = 1 To tVals.Count
                    Dim aTag As String = tVals.Member(j)
                    Dim bKeep As Boolean = False
                    If Not bContainsString Then
                        If String.Compare(aMem.Tag, aTag, True) = 0 Then bKeep = True
                    Else
                        If aMem.Tag.IndexOf(aTag, StringComparison.OrdinalIgnoreCase) + 1 > 0 Then bKeep = True
                    End If
                    If bKeep And bTest Then
                        If Not bContainsString Then
                            If String.Compare(aMem.Flag, aStr, True) <> 0 Then bKeep = False
                        Else
                            If aMem.Flag.IndexOf(aStr, StringComparison.OrdinalIgnoreCase) < 0 Then bKeep = False
                        End If
                    End If
                    If bKeep And bTestVal Then
                        If aMem.Value <> aValue.Value Then bKeep = False
                    End If
                    If Not bReturnInverse Then
                        If bKeep Then
                            _rVal.Add(aMem)
                            vIDs.Add(i, i)
                        End If
                    Else
                        If Not bKeep Then
                            _rVal.Add(aMem)
                            vIDs.Add(i, i)
                        End If
                    End If
                Next j
            Next i
            If bRemove And vIDs.Count > 0 Then
                If vIDs.Count = Count Then
                    Clear()
                    Return _rVal
                End If
                Dim nMems(Count - vIDs.Count) As TVERTEX
                Dim j As Integer = 0
                For i As Integer = 1 To Count
                    If vIDs.ContainsKey(i) Then
                        nMems(j) = Item(j)
                        j += 1
                    End If
                Next
                _Members = nMems
            End If
            Return _rVal
        End Function
        Public Function GetByPropertyValue(aProperty As dxxVectorProperties, aPropertyValue As Object, Optional aPrecis As Integer = 3, Optional bRemove As Boolean = False) As TVERTICES
            Dim _rVal As New TVERTICES(0)
            If aProperty < 1 Or aProperty > dxxVectorProperties.Suppressed Then Return _rVal
            Dim aMem As TVERTEX
            Dim aVal As Object
            Dim bVal As Object
            Dim aFlg As Boolean
            Dim vIDs As New Dictionary(Of Integer, Integer)
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            bVal = aPropertyValue
            Select Case aProperty
                Case dxxVectorProperties.EndWidth, dxxVectorProperties.StartWidth, dxxVectorProperties.Radius, dxxVectorProperties.Value, dxxVectorProperties.Rotation, dxxVectorProperties.LTScale
                    If TVALUES.IsNumber(aPropertyValue) Then
                        If aPrecis >= 0 Then bVal = Math.Round(aPropertyValue, aPrecis)
                    End If
                Case Else
            End Select
            For i As Integer = 1 To Count
                aMem = Item(i)
                aVal = TVERTEX.PropertyValue(aMem, aProperty, aPrecis, aFlg)
                If aFlg Then Exit For
                If aVal = bVal Then
                    _rVal.Add(aMem)
                    vIDs.Add(i, i)
                End If
            Next i
            If bRemove And vIDs.Count > 0 Then
                If vIDs.Count = Count Then
                    Clear()
                    Return _rVal
                End If
                Dim nMems(Count - vIDs.Count) As TVERTEX
                Dim j As Integer = 0
                For i As Integer = 1 To Count
                    If vIDs.ContainsKey(i) Then
                        nMems(j) = Item(j)
                        j += 1
                    End If
                Next
                _Members = nMems
            End If
            Return _rVal
        End Function
        Public Function GetPropertyValues(aProperty As dxxVectorProperties, Optional bUniqueValues As Boolean = False, Optional aPrecis As Integer = 3) As TVALUES
            Dim _rVal As New TVALUES(0)
            If aProperty < 1 Or aProperty > dxxVectorProperties.Suppressed Then Return _rVal
            Dim aMem As TVERTEX
            Dim aVal As Object
            Dim bKeep As Boolean
            Dim aFlg As Boolean
            For i As Integer = 1 To Count
                aMem = Item(i)
                aVal = TVERTEX.PropertyValue(aMem, aProperty, aPrecis, aFlg)
                If aFlg Then Exit For
                If bUniqueValues Then
                    bKeep = True
                    For j As Integer = 1 To _rVal.Count
                        If _rVal.Item(j) = aVal Then
                            bKeep = False
                            Exit For
                        End If
                    Next j
                    If bKeep Then _rVal.Add(aVal)
                Else
                    _rVal.Add(aVal)
                End If
            Next i
            Return _rVal
        End Function
        Public Function RemoveAtCoordinate(Optional aX As Double? = Nothing, Optional aY As Double? = Nothing, Optional aZ As Double? = Nothing, Optional aPrecis As Integer = 3, Optional aPlane As dxfPlane = Nothing) As TVERTICES
            Dim _rVal As New TVERTICES(0)
            '#1the subject vertices
            '#2the X coordinate to match
            '#3the Y coordinate to match
            '#4the Z coordinate to match
            '^searchs for and returns vertices from the collection whose coordinates match the passed coordinates
            '~if an any of the ordinates (X, Y or Z) are not passed or are not numeric they are not used in the comparison.
            '~say ony an X value is passed, then all the vertices with the same X ordinate are returned regarless of their
            '~respective Y and Z ordinate values.
            Try
                If Count <= 0 Then Return _rVal
                aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)

                If Not aX.HasValue And Not aY.HasValue And Not aZ.HasValue Then Return _rVal

                Dim isMatchX As Boolean
                Dim isMatchY As Boolean
                Dim isMatchZ As Boolean
                Dim v1 As TVECTOR

                Dim iKeep As Integer = 0
                Dim aKeepers(0) As TVERTEX
                For i As Integer = 1 To Count
                    v1 = Vector(i)
                    If Not dxfPlane.IsNull(aPlane) Then
                        v1 = v1.WithRespectTo(aPlane)
                    End If
                    isMatchX = True
                    isMatchY = True
                    isMatchZ = True
                    If aX.HasValue Then
                        isMatchX = Math.Round(v1.X - aX.Value, aPrecis) = 0
                    End If
                    If aY.HasValue Then
                        isMatchY = Math.Round(v1.Y - aY.Value, aPrecis) = 0

                    End If
                    If aZ.HasValue Then
                        isMatchZ = Math.Round(v1.Z - aZ.Value, aPrecis) = 0
                    End If
                    If isMatchX And isMatchY And isMatchZ Then
                        _rVal.Add(New TVERTEX(_Members(i - 1)))
                    Else
                        iKeep += 1
                        System.Array.Resize(aKeepers, iKeep)
                        aKeepers(iKeep - 1) = New TVERTEX(_Members(i - 1))
                    End If
                Next i
                _Members = aKeepers
                Return _rVal
            Catch ex As Exception
                Return _rVal
            End Try
        End Function
        Public Function WithRespectToPlane(aPlane As TPLANE, Optional aPrecis As Integer = 8, Optional aScaleFactor As Double = 0, Optional bSuppressZ As Boolean = False) As TVERTICES
            If TPLANE.IsNull(aPlane) Or Count = 0 Then Return Clone()
            '#1the structure of the plane
            '#2the number of decimals to round the returned vertices coordinates to
            '#3a scale factor to apply (0 means no scale applied)
            '^returns the vertices whos coordinates of the passed vector with respect to the center and origin of the passed plane
            Dim _rVal As New TVERTICES(Count)
            Dim vrt As TVERTEX
            For i As Integer = 1 To Count
                vrt = New TVERTEX(Item(i))
                vrt.Vector = vrt.Vector.WithRespectTo(aPlane, aPrecis:=aPrecis, aScaleFactor:=aScaleFactor, bSuppressZ:=bSuppressZ)
                _rVal.SetItem(i, vrt)
            Next i
            Return _rVal
        End Function
        Public Sub SortRelativeToLine(aLine As TLINE, aNormal As TVECTOR, Optional aFarthestToNeareast As Boolean = False, Optional bIgnoreDirection As Boolean = True)
            Dim rIDs As List(Of Integer) = Nothing
            SortRelativeToLine(aLine, aNormal, aFarthestToNeareast, rIDs, bIgnoreDirection)
        End Sub
        Public Sub SortRelativeToLine(aLine As TLINE, aNormal As TVECTOR, aFarthestToNeareast As Boolean, ByRef rIDs As List(Of Integer), Optional bIgnoreDirection As Boolean = True)
            Dim _rVal As New TVERTICES(0)
            rIDs = New List(Of Integer)
            If Count <= 1 Then
                If Count > 0 Then
                    rIDs.Add(1)
                End If
                Return
            End If
            Dim aFlag As Boolean
            Dim i As Integer
            Dim v1 As TVECTOR
            Dim d1 As Double
            Dim v2 As TVECTOR
            Dim upDir As TVECTOR
            Dim zDir = aNormal.Normalized(aFlag)
            If aFlag Then Return
            v1 = aLine.SPT.DirectionTo(aLine.EPT, False, aFlag)
            If aFlag Then Return
            Dim aPln As New TPLANE(aLine.SPT, v1, zDir.CrossProduct(v1))
            Dim tpl As Tuple(Of Double, TVERTEX, Integer)
            Dim srt As New List(Of Tuple(Of Double, TVERTEX, Integer))
            Dim mem As TVERTEX
            For i = 1 To Count
                mem = Item(i)
                v1 = mem.Vector.ProjectedTo(aPln)
                v2 = dxfProjections.ToLine(v1, aLine, d1)
                d1 = Math.Round(d1, 6)
                If d1 <> 0 And Not bIgnoreDirection Then
                    upDir = v2.DirectionTo(v1)
                    If Not upDir.Equals(aPln.YDirection, 3) Then d1 = -d1
                End If
                srt.Add(New Tuple(Of Double, TVERTEX, Integer)(d1, mem, i))
            Next i
            srt.Sort(Function(tupl1 As Tuple(Of Double, TVERTEX, Integer), tupl2 As Tuple(Of Double, TVERTEX, Integer)) tupl1.Item1.CompareTo(tupl2.Item1))
            If aFarthestToNeareast Then srt.Reverse()
            Dim j As Integer = 0
            For Each tpl In srt
                i = tpl.Item3
                _Members(j) = tpl.Item2
                j += 1
                rIDs.Add(tpl.Item3)
            Next
        End Sub
        Public Sub SortByDistanceToLine(aLine As TLINE, Optional aFarthestToNeareast As Boolean = False)
            If Count <= 1 Then Return
            If aLine.SPT.DistanceTo(aLine.EPT, 6) = 0 Then Return

            Dim srt As New List(Of Tuple(Of Double, TVERTEX))

            For i As Integer = 1 To Count
                Dim mem As TVERTEX = Item(i)
                Dim d1 As Double = mem.Vector.DistanceTo(aLine, aPrecis:=6)
                srt.Add(New Tuple(Of Double, TVERTEX)(d1, New TVERTEX(mem)))
            Next i
            srt.Sort(Function(tupl1 As Tuple(Of Double, TVERTEX), tupl2 As Tuple(Of Double, TVERTEX)) tupl1.Item1.CompareTo(tupl2.Item1))
            If aFarthestToNeareast Then srt.Reverse()
            For i = 1 To Count
                SetItem(i, srt.Item(i - 1).Item2)
            Next i
        End Sub
        Public Sub RemoveCoincident(Optional aPrecis As Integer = 4)
            Dim newMems As New TVERTICES(0)
            '^removes and returns the vectors that occur more than once
            Dim i As Integer
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim j As Integer
            Dim bKeep As Boolean
            Dim d1 As Double
            If aPrecis < 0 Then aPrecis = 0
            If aPrecis > 10 Then aPrecis = 10
            For i = 1 To Count
                v1 = Vector(i)
                bKeep = True
                For j = 1 To newMems.Count
                    v2 = newMems.Vector(j)
                    d1 = Math.Round(v1.DistanceTo(v2), aPrecis)
                    If d1 <= 0.00000001 Then
                        bKeep = False
                        Exit For
                    End If
                Next j
                If bKeep Then
                    newMems.Add(Item(i))
                End If
            Next i
            _Members = newMems._Members
        End Sub
        Public Function NearestToLine(aLine As TLINE, Optional bReturnFarthest As Boolean = False) As TVERTEX
            Dim rIndex As Integer = 0
            Return NearestToLine(aLine, bReturnFarthest, rIndex)
        End Function
        Public Function NearestToLine(aLine As TLINE, bReturnFarthest As Boolean, ByRef rIndex As Integer) As TVERTEX
            rIndex = 0

            If Count <= 0 Then Return Nothing
            If Count <= 1 Then Return First
            Dim _rVal As New TVERTEX
            If aLine.SPT.DistanceTo(aLine.EPT, 6) = 0 Then Return Nothing
            Dim v1 As TVERTEX
            Dim d1 As Double
            Dim d2 As Double
            Dim v2 As TVECTOR
            Dim idx As Integer
            If bReturnFarthest Then d1 = -Double.MaxValue Else d1 = Double.MaxValue
            For i As Integer = 1 To Count
                v1 = _Members(i - 1)
                v2 = dxfProjections.ToLine(v1.Vector, aLine, d2)
                If bReturnFarthest Then
                    If d2 >= d1 Then
                        d1 = d2
                        idx = i
                    End If
                Else
                    If d2 <= d1 Then
                        d1 = d2
                        idx = i
                    End If
                End If
            Next i
            rIndex = idx
            Return _Members(rIndex - 1)
        End Function
        Public Sub ProjectTo(aPlane As TPLANE, Optional bReturnWithRespectTo As Boolean = False, Optional aDirection As dxfDirection = Nothing)
            If TPLANE.IsNull(aPlane) Then Return
            If Count <= 0 Then Return
            '#1the vector to project
            '#2the structure of the plane to project to
            '#3 the project direction
            '^projects the passeds vector to the passed plane
            '~if the passed direction is a null vector then the direction is assumed as the normal of the plane

            Dim aDir As TVECTOR
            Dim newMems(0 To Count - 1) As TVERTEX
            If TPLANE.IsNull(aPlane) Then bReturnWithRespectTo = False
            If aDirection IsNot Nothing Then aDir = New TVECTOR(aDirection) Else aDir = aPlane.ZDirection
            For i As Integer = 1 To Count
                newMems(i - 1) = Item(i)
                Dim v1 As TVECTOR = dxfProjections.ToPlane(newMems(i - 1).Vector, aPlane, aDir)
                newMems(i - 1).Vector = v1
                If bReturnWithRespectTo Then
                    newMems(i - 1).Vector = newMems(i - 1).Vector.WithRespectTo(aPlane)
                End If
            Next i
            _Members = newMems
        End Sub
        Public Function ProjectedTo(aPlane As TPLANE, Optional bReturnWithRespectTo As Boolean = False, Optional aDirection As dxfDirection = Nothing) As TVERTICES
            Dim _rVal As New TVERTICES(Me)
            If Count <= 0 Then Return _rVal
            _rVal.ProjectTo(aPlane, bReturnWithRespectTo, aDirection)
            Return _rVal
            '#1the vector to project
            '#2the structure of the plane to project to
            '#3 the project direction
            '^projects the passeds vector to the passed plane
            '~if the passed direction is a null vector then the direction is assumed as the normal of the plane
        End Function
        Public Function GetByValue(aValue As Double) As TVERTICES
            Dim _rVal As New TVERTICES(0)
            For i As Integer = 1 To Count
                If _Members(i - 1).Value = aValue Then
                    _rVal.Add(_Members(i - 1))
                End If
            Next i
            Return _rVal
        End Function
        Public Function GetByTag(Optional aTag As String = Nothing, Optional aFlag As String = Nothing, Optional bIgnoreCase As Boolean = True) As TVERTICES
            Dim _rVal As New TVERTICES(0)
            Dim tags As Boolean = aTag IsNot Nothing
            Dim flags As Boolean = aFlag IsNot Nothing
            If Not tags And Not flags Then Return _rVal
            Dim v1 As TVERTEX
            Dim keep As Boolean
            For i As Integer = 1 To Count
                v1 = _Members(i - 1)
                keep = True
                If tags Then
                    If String.Compare(v1.Tag, aTag, ignoreCase:=bIgnoreCase) <> 0 Then keep = False
                End If
                If flags Then
                    If String.Compare(v1.Flag, aFlag, ignoreCase:=bIgnoreCase) <> 0 Then keep = False
                End If
                If keep Then
                    _rVal.Add(_Members(i - 1))
                End If
            Next i
            Return _rVal
        End Function
        Public Function DXFProperties(aZDirection As TVECTOR, aGlobalWidth As Double, ByRef rVCount As Integer, ByRef rUniWidth As Double) As TPROPERTIES
            Dim _rVal As New TPROPERTIES
            rVCount = 0
            rUniWidth = -1
            If Count <= 0 Then Return _rVal
            Dim aPl As TPLANE = TPLANE.ArbitraryCS(aZDirection)
            Dim i As Integer
            Dim v1 As TVERTEX
            Dim v2 As TVECTOR
            Dim uwd As Double
            Dim buniwd As Boolean
            uwd = -1
            buniwd = True
            rVCount = Count
            If aGlobalWidth < 0 Then aGlobalWidth = 0
            aGlobalWidth = Math.Round(aGlobalWidth, 8)
            For i = 1 To Count
                v1 = _Members(i - 1)
                v1.StartWidth = Math.Round(v1.StartWidth, 8)
                v1.EndWidth = Math.Round(v1.EndWidth, 8)
                If v1.EndWidth < 0 Then v1.EndWidth = 0
                If v1.StartWidth < 0 Then v1.StartWidth = 0
                If uwd < 0 Then uwd = v1.StartWidth
                If v1.StartWidth <> uwd Then buniwd = False
                If v1.EndWidth <> uwd Then buniwd = False
                v2 = v1.Vector.WithRespectTo(aPl, aPrecis:=15)
                _rVal.Add(New TPROPERTY(10, v2.X, $"Vertex { i }.X", dxxPropertyTypes.dxf_Double))
                _rVal.Add(New TPROPERTY(20, v2.Y, $"Vertex { i }.Y", dxxPropertyTypes.dxf_Double))
                If aGlobalWidth <= 0 Then
                    _rVal.Add(New TPROPERTY(40, v1.StartWidth, "Start Width", dxxPropertyTypes.dxf_Double))
                    _rVal.Add(New TPROPERTY(41, v1.EndWidth, "End Width", dxxPropertyTypes.dxf_Double))
                End If
                If v1.Radius <> 0 Then
                    _rVal.Add(New TPROPERTY(42, v1.Bulge, "Bulge", dxxPropertyTypes.dxf_Double))
                End If
            Next i
            If buniwd Then rUniWidth = uwd
            Return _rVal
        End Function
        Public Function Clone() As TVERTICES

            Return New TVERTICES(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TVERTICES(Me)
        End Function
        Public Overrides Function ToString() As String
            Return $"TVERTICES[{Count }]"
        End Function
        Public Sub Reverse()
            If Count <= 1 Then Return
            _Members.Reverse()
        End Sub
        Public Function Reversed() As TVERTICES
            Dim _rVal As New TVERTICES(Me)
            _rVal.Reverse()
            Return _rVal
        End Function
        Public Function LayerName(aIndex As Integer) As String
            Dim _rVal As String = String.Empty
            If aIndex < 1 Or aIndex > Count Then Return _rVal
            _rVal = _Members(aIndex - 1).LayerName
            Return _rVal
        End Function
        Public Function LayerNames(Optional aLayerToInclude As String = "", Optional bUnquieValues As Boolean = True) As List(Of String)
            Dim _rVal As New List(Of String)
            '^all of the layernames names referenced by the entities in the collection
            If Not String.IsNullOrWhiteSpace(aLayerToInclude) Then _rVal.Add(aLayerToInclude.Trim)
            Dim aMem As dxfVector
            Dim lname As String
            Dim keep As Boolean
            For i As Integer = 1 To Count
                aMem = _Members(i - 1)
                lname = aMem.LayerName
                keep = True
                If bUnquieValues Then
                    keep = _rVal.IndexOf(lname) < 0
                End If
                If keep Then _rVal.Add(lname)
            Next i
            Return _rVal
        End Function
        Public Function Color(aIndex As Integer) As dxxColors
            Dim _rVal As dxxColors = dxxColors.ByLayer
            If aIndex < 1 Or aIndex > Count Then Return _rVal
            _rVal = _Members(aIndex - 1).Color
            Return _rVal
        End Function
        Public Function Colors(Optional aColorToInclude As dxxColors = dxxColors.Undefined, Optional bUnquieValues As Boolean = True) As List(Of dxxColors)
            Dim _rVal As New List(Of dxxColors)
            '^all of the colors referenced by the entities in the collection
            If aColorToInclude <> dxxColors.Undefined Then _rVal.Add(aColorToInclude)
            Dim aMem As dxfVector
            Dim lname As dxxColors
            Dim keep As Boolean
            For i As Integer = 1 To Count
                aMem = _Members(i - 1)
                lname = aMem.Color
                keep = True
                If bUnquieValues Then
                    keep = _rVal.IndexOf(lname) < 0
                End If
                If keep Then _rVal.Add(lname)
            Next i
            Return _rVal
        End Function
        Public Function Linetype(aIndex As Integer) As String
            Dim _rVal As String = String.Empty
            If aIndex < 1 Or aIndex > Count Then Return _rVal
            _rVal = _Members(aIndex - 1).Linetype
            Return _rVal
        End Function
        Public Function Linetypes(Optional aLTToInclude As String = "", Optional bUnquieValues As Boolean = True) As List(Of String)
            Dim _rVal As New List(Of String)
            '^all of the linetype names referenced by the entities in the collection
            If Not String.IsNullOrWhiteSpace(aLTToInclude) Then _rVal.Add(aLTToInclude.Trim)
            Dim aMem As TVERTEX
            Dim lname As String
            Dim keep As Boolean
            For i As Integer = 1 To Count
                aMem = _Members(i - 1)
                lname = aMem.Linetype
                keep = True
                If bUnquieValues Then
                    keep = _rVal.IndexOf(lname) < 0
                End If
                If keep Then _rVal.Add(lname)
            Next i
            Return _rVal
        End Function

        Public Function ToList() As List(Of TVERTEX)
            If Count <= 0 Then Return New List(Of TVERTEX)
            Return _Members.ToList()
        End Function

        Public Function UniqueMembers(Optional aPrecis As Integer = 4) As TVERTICES
            Dim _rVal As New TVERTICES(0)
            '^removes and returns the vectors that occur more than once
            Dim v1 As TVERTEX
            Dim v2 As TVERTEX
            Dim bKeep As Boolean
            Dim d1 As Double
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            For i As Integer = 1 To Count
                v1 = _Members(i - 1)
                bKeep = True
                For j As Integer = 1 To _rVal.Count
                    v2 = _rVal.Item(j)
                    d1 = Math.Round(v1.DistanceTo(v2), aPrecis)
                    If d1 <= 0.00000001 Then
                        bKeep = False
                        Exit For
                    End If
                Next j
                If bKeep Then
                    _rVal.Add(New TVERTEX(v1))
                End If
            Next
            Return _rVal
        End Function
#End Region 'Methods
#Region "Shared Methods"
        Public Shared ReadOnly Property Zero As TVERTICES
            Get
                Return New TVERTICES(0)
            End Get
        End Property

#End Region 'Shared Methods
#Region "Operators"
        Public Shared Widening Operator CType(aVertices As TVERTICES) As TVECTORS

            Dim _rVal As New TVECTORS(0)
            For i = 1 To aVertices.Count
                _rVal.Add(New TVECTOR(aVertices.Item(i)))
            Next
            Return _rVal
        End Operator
        Public Shared Widening Operator CType(aVertices As TVERTICES) As colDXFVectors
            Return New colDXFVectors(aVertices)
        End Operator
#End Region 'Operators
    End Structure 'TVERTICES
    Friend Structure TVERTEXTARRAY
        Implements ICloneable
#Region "Members"
        Private _Init As Boolean
        Private _Members() As TVERTICES
#End Region 'Members
#Region "Constructors"
        Public Sub New(Optional aCount As Integer = 0)
            'init -------------------------------------------
            _Init = True
            ReDim _Members(-1)
            'init -------------------------------------------
            If aCount <= 0 Then Return
            For i As Integer = 1 To aCount
                Add(New TVERTICES(0))
            Next
        End Sub

        Public Sub New(aArray As TVERTEXTARRAY)
            'init -------------------------------------------
            _Init = True
            ReDim _Members(-1)
            'init -------------------------------------------

            For i As Integer = 1 To aArray.Count
                Add(New TVERTICES(aArray.Item(i)))
            Next
        End Sub

#End Region 'Constructors
#Region "Properties"

#End Region 'Properties
#Region "Methods"
        Public Function Count(Optional aArrayIndex As Integer = 0) As Integer

            If Not _Init Then
                _Init = True
                ReDim _Members(-1)
            End If
            If aArrayIndex <= 0 Then
                Return _Members.Count
            Else
                If aArrayIndex > Count Then Return 0 Else Return Item(aArrayIndex).Count
            End If

        End Function

        Public Function Item(aArrayIndex As Integer) As TVERTICES
            'Base 1
            If aArrayIndex < 1 Or aArrayIndex > Count() Then Return Nothing
            Return _Members(aArrayIndex - 1)
        End Function
        Public Sub SetItem(aIndex As Integer, value As TVERTICES)
            If aIndex < 1 Or aIndex > Count() Then Return
            _Members(aIndex - 1) = value
        End Sub
        Public Function Vertex(aArrayIndex As Integer, aIndex As Integer) As TVERTEX
            If aArrayIndex < 1 Or aArrayIndex > Count() Then Return Nothing
            Return Item(aArrayIndex).Item(aIndex)
        End Function
        Public Sub SetVertex(aArrayIndex As Integer, aIndex As Integer, value As TVERTEX)
            If aArrayIndex < 1 Or aArrayIndex > Count() Then Return
            _Members(aArrayIndex - 1).SetItem(aIndex, value)
        End Sub


        Public Sub Clear(aArrayIndex As Integer)
            If aArrayIndex <= 0 Then
                _Init = True
                ReDim _Members(-1)
            Else
                If aArrayIndex > Count() Then Return
                _Members(aArrayIndex - 1).Clear()
            End If
        End Sub
        Public Sub Add(aVertices As TVERTICES)
            System.Array.Resize(_Members, Count() + 1)
            _Members(_Members.Count - 1) = aVertices
        End Sub
        Public Sub Add(aArrayIndex As Integer, aVertex As TVERTEX)

            If aArrayIndex < 1 Or aArrayIndex > Count() Then Return
            _Members(aArrayIndex - 1).Add(aVertex)
        End Sub
        Public Function Clone() As TVERTEXTARRAY
            Return New TVERTEXTARRAY(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TVERTEXTARRAY(Me)
        End Function

#End Region 'Methods
    End Structure 'TVERTEXTARRAY


End Namespace
