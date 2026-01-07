
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics.Structures

    Friend Structure TVECTRIX
        Implements ICloneable
#Region "Members"
        Public X As Double
        Public Y As Double
        Public Z As Double
        Public s As Double
#End Region 'Members
#Region "Constructors"
        Public Sub New(aX As Double, aY As Double, aZ As Double, S1 As Double)
            'init -----------------------------
            X = aX
            Y = aY
            Z = aZ
            s = S1
            'init -----------------------------
        End Sub
        Public Sub New(aVectrix As TVECTRIX)
            'init -----------------------------
            X = aVectrix.X
            Y = aVectrix.Y
            Z = aVectrix.Z
            s = aVectrix.s
            'init -----------------------------
        End Sub
#End Region 'Constructors
#Region "Properties"
        Public Function Dot(B As TVECTRIX) As Double

            Return (X * B.X) + (Y * B.Y) + (Z * B.Z) + (s * B.s)

        End Function
        Public Function Dot(M As TMATRIX4) As TVECTRIX
            Dim T As TMATRIX4 = M.Transposed()
            Return New TVECTRIX(Me.Dot(T.A), Dot(T.B), Dot(T.C), Dot(T.D))
        End Function
        Public ReadOnly Property Vector As TVECTOR
            Get
                Return New TVECTOR(X, Y, Z)
            End Get
        End Property
        Public ReadOnly Property Point As System.Drawing.Point
            Get
                Return New System.Drawing.Point(TValues.To_INT(X), TValues.To_INT(Y))
            End Get
        End Property
        Public ReadOnly Property Summation As Double
            Get
                Return X + Y + Z + s
            End Get
        End Property
#End Region 'Properties
#Region "Methods"
        Public Function IsEqual(B As TVECTRIX, Optional aPrecis As Integer = 4) As Boolean
            '^returns true if the passed vectors are equal within the passed precision
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            If aPrecis = 0 Then
                Return (X = B.X) And (Y = B.Y) And (Z = B.Z) And (s = B.s)
            Else
                Return (Math.Round((X - B.X), aPrecis) = 0 And Math.Round((Y - B.Y), aPrecis) = 0 And Math.Round((Z - B.Z), aPrecis) = 0 And Math.Round((s - B.s), aPrecis) = 0)
            End If
        End Function
        Public Function IsNull(Optional aPrecis As Integer = 4) As Boolean
            '^returns true if the passed vectors are equal within the passed precision
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            If aPrecis = 0 Then
                Return (X = 0) And (Y = 0) And (Z = 0) And (s = 0)
            Else
                Return (Math.Round(X, aPrecis) = 0 And Math.Round(Y, aPrecis) = 0 And Math.Round(Z, aPrecis) = 0 And Math.Round(s, aPrecis) = 0)
            End If
        End Function
        Public Function Clone() As TVECTRIX
            Return New TVECTRIX(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TVECTRIX(Me)
        End Function
        Public Sub Scale(aScaler As Double)
            X *= aScaler
            Y *= aScaler
            Z *= aScaler
            s *= aScaler
        End Sub
        Public Function Scaled(aScaler As Double) As TVECTRIX
            Dim _rVal As New TVECTRIX(X, Y, Z, s)
            _rVal.Scale(aScaler)
            Return _rVal
        End Function
        Public Function Interpolate(B As TVECTRIX, Alpha As Double) As TVECTRIX
            Return New TVECTRIX(X + Alpha * (B.X - X), Y + Alpha * (B.Y - Y), Z + Alpha * (B.Z - Z), s + Alpha * (B.s - s))
        End Function
        Public Sub Invert()
            Try
                If X <> 0 Then X = 1 / X
                If Y <> 0 Then Y = 1 / Y
                If Z <> 0 Then Z = 1 / Z
                If s <> 0 Then s = 1 / s
            Catch ex As Exception
            End Try
        End Sub
        Public Function Inverted() As TVECTRIX
            Dim _rVal As TVECTRIX = New TVECTRIX(Me)
            _rVal.Invert()
            Return _rVal
        End Function
        Public Overrides Function ToString() As String
            Return "(" & Format(X, "0.000") & "," & Format(Y, "0.000") & "," & Format(Z, "0.000") & "," & Format(s, "0.000") & ")"
        End Function
#End Region 'Methods
#Region "Shared Methods"
        Public Shared Function Random() As TVECTRIX
            Randomize()
            Return New TVECTRIX(2 * Rnd() - 1, 2 * Rnd() - 1, 2 * Rnd() - 1, 2 * Rnd() - 1)
        End Function
#End Region 'shared methods
#Region "Operators"
        Public Shared Operator +(A As TVECTRIX, B As TVECTRIX) As TVECTRIX
            Return New TVECTRIX(A.X + B.X, A.Y + B.Y, A.Z + B.Z, A.s + B.s)
        End Operator
        Public Shared Operator -(A As TVECTRIX, B As TVECTRIX) As TVECTRIX
            Return New TVECTRIX(A.X - B.X, A.Y - B.Y, A.Z - B.Z, A.s - B.s)
        End Operator
        Public Shared Operator *(A As TVECTRIX, B As TVECTRIX) As TVECTRIX
            Return New TVECTRIX(A.X * B.X, A.Y * B.Y, A.Z * B.Z, A.s * B.s)
        End Operator
        Public Shared Operator \(A As TVECTRIX, B As TVECTRIX) As Double
            Return A.Dot(B.Inverted())
        End Operator
        Public Shared Widening Operator CType(aVectrix As TVECTRIX) As TVECTOR
            If aVectrix.s <> 0 And aVectrix.s <> 1 Then
                Return New TVECTOR(aVectrix.X / aVectrix.s, aVectrix.Y / aVectrix.s, aVectrix.Z / aVectrix.s)
            Else
                Return New TVECTOR(aVectrix.X, aVectrix.Y, aVectrix.Z)
            End If
        End Operator
#End Region 'OPerators
    End Structure 'TVECTRIX
    Friend Structure TMATRIX4
        Implements ICloneable
#Region "Members"
        Public A As TVECTRIX
        Public B As TVECTRIX
        Public C As TVECTRIX
        Public D As TVECTRIX
        Public Name As String
#End Region 'Members
#Region "Constructors"
        Public Sub New(aName As String)
            'init ----------------------------------------------------------------------
            A = New TVECTRIX(0, 0, 0, 0)
            B = New TVECTRIX(0, 0, 0, 0)
            C = New TVECTRIX(0, 0, 0, 0)
            D = New TVECTRIX(0, 0, 0, 0)
            Name = IIf(String.IsNullOrWhiteSpace(aName), "", aName.Trim())
            'init ----------------------------------------------------------------------
        End Sub

        Public Sub New(aRow1 As TVECTRIX, aRow2 As TVECTRIX, aRow3 As TVECTRIX, aRow4 As TVECTRIX, Optional aName As String = "")
            'init ----------------------------------------------------------------------
            A = aRow1
            B = aRow2
            C = aRow3
            D = aRow4
            Name = IIf(String.IsNullOrWhiteSpace(aName), "", aName.Trim())
            'init ----------------------------------------------------------------------
        End Sub
        Public Sub New(aMatrix As TMATRIX4)
            'init ----------------------------------------------------------------------
            A = New TVECTRIX(aMatrix.A)
            B = New TVECTRIX(aMatrix.B)
            C = New TVECTRIX(aMatrix.C)
            D = New TVECTRIX(aMatrix.D)
            Name = aMatrix.Name
            'init ----------------------------------------------------------------------
        End Sub


#End Region 'Constructors
#Region "Properties"
        Public Property Row1 As TVECTRIX
            Get
                Return A
            End Get
            Set(value As TVECTRIX)
                A = value
            End Set
        End Property
        Public Property Row2 As TVECTRIX
            Get
                Return B
            End Get
            Set(value As TVECTRIX)
                B = value
            End Set
        End Property
        Public Property Row3 As TVECTRIX
            Get
                Return C
            End Get
            Set(value As TVECTRIX)
                C = value
            End Set
        End Property
        Public Property Row4 As TVECTRIX
            Get
                Return D
            End Get
            Set(value As TVECTRIX)
                D = value
            End Set
        End Property
        Public Property Col1 As TVECTRIX
            Get
                Return New TVECTRIX(A.X, B.X, C.X, D.X)
            End Get
            Set(value As TVECTRIX)
                A.X = value.X
                B.X = value.Y
                C.X = value.Z
                D.X = value.s
            End Set
        End Property
        Public Property Col2 As TVECTRIX
            Get
                Return New TVECTRIX(A.Y, B.Y, C.Y, D.Y)
            End Get
            Set(value As TVECTRIX)

                A.Y = value.X
                B.Y = value.Y
                C.Y = value.Z
                D.Y = value.s
            End Set
        End Property
        Public Property Col3 As TVECTRIX
            Get
                Return New TVECTRIX(A.Z, B.Z, C.Z, D.Z)
            End Get
            Set(value As TVECTRIX)

                A.Z = value.X
                B.Z = value.Y
                C.Z = value.Z
                D.Z = value.s
            End Set
        End Property
        Public Property Col4 As TVECTRIX
            Get
                Return New TVECTRIX(A.s, B.s, C.s, D.s)
            End Get
            Set(value As TVECTRIX)

                A.s = value.X
                B.s = value.Y
                C.s = value.Z
                D.s = value.s
            End Set
        End Property
        Public ReadOnly Property Determinant As Double
            Get
                Return dxfMath.Determinant4By4(A.X, A.Y, A.Z, A.s, B.X, B.Y, B.Z, B.s, C.X, C.Y, C.Z, C.s, D.X, D.Y, D.Z, D.s)
            End Get
        End Property
        Public Function Dot(M As TMATRIX4) As TMATRIX4
            Dim _rVal As New TMATRIX4
            Dim T As TMATRIX4 = M.Transposed()
            'T has its M's columns as its rows
            _rVal.A = New TVECTRIX(A.Dot(T.A), A.Dot(T.B), A.Dot(T.C), A.Dot(T.D))
            _rVal.B = New TVECTRIX(B.Dot(T.A), B.Dot(T.B), B.Dot(T.C), B.Dot(T.D))
            _rVal.C = New TVECTRIX(C.Dot(T.A), C.Dot(T.B), C.Dot(T.C), C.Dot(T.D))
            _rVal.D = New TVECTRIX(D.Dot(T.A), D.Dot(T.B), D.Dot(T.C), D.Dot(T.D))
            Return _rVal

        End Function
        Public Function IsNull(Optional aPrecis As Integer = 0) As Boolean


            If Not A.IsNull(aPrecis) Then Return False
            If Not B.IsNull(aPrecis) Then Return False
            If Not C.IsNull(aPrecis) Then Return False
            Return D.IsNull(aPrecis)

        End Function

        Public Function IsIdentity(Optional aPrecis As Integer = 0) As Boolean

            Dim _rVal As Boolean = False
            If A.IsEqual(New TVECTRIX(1, 0, 0, 0), aPrecis) Then
                If B.IsEqual(New TVECTRIX(0, 1, 0, 0), aPrecis) Then
                    If C.IsEqual(New TVECTRIX(0, 0, 1, 0), aPrecis) Then
                        If D.IsEqual(New TVECTRIX(0, 0, 0, 1), aPrecis) Then
                            _rVal = True
                        End If
                    End If
                End If
            End If
            Return _rVal

        End Function
#End Region 'Properties
#Region "Methods"
        Public Sub Columns_Get(ByRef rCol1 As TVECTRIX, ByRef rCol2 As TVECTRIX, ByRef rCol3 As TVECTRIX, ByRef rCol4 As TVECTRIX)
            rCol1 = Col1
            rCol2 = Col2
            rCol3 = Col3
            rCol4 = Col4
        End Sub
        Public Sub Columns_Set(aCol1 As TVECTRIX, aCol2 As TVECTRIX, aCol3 As TVECTRIX, aCol4 As TVECTRIX)
            Col1 = aCol1
            Col2 = aCol2
            Col3 = aCol3
            Col4 = aCol4
        End Sub
        Friend Function IsEqual(ByRef M As TMATRIX4, Optional aPrecis As Integer = 4) As Boolean
            Dim _rVal As Boolean = False
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            If D.IsEqual(M.D, aPrecis) Then 'last row contains translations so do it first
                If C.IsEqual(M.C, aPrecis) Then
                    If B.IsEqual(M.B, aPrecis) Then
                        _rVal = A.IsEqual(M.A, aPrecis)
                    End If
                End If
            End If
            Return _rVal
        End Function
        Public Sub Scale(aScaler As Double)
            A.Scale(aScaler)
            B.Scale(aScaler)
            C.Scale(aScaler)
            D.Scale(aScaler)
        End Sub
        Public Function Scaled(aScaler As Double) As TMATRIX4
            Return New TMATRIX4(A.Scaled(aScaler), B.Scaled(aScaler), C.Scaled(aScaler), D.Scaled(aScaler))
        End Function

        Public Function Clone() As TMATRIX4
            Return New TMATRIX4(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TMATRIX4(Me)
        End Function
        Public Sub Invert()
            Dim _rVal As New TMATRIX4(Me)
            Dim T As New TMATRIX4
            T.A.X = dxfMath.Determinant3by3(B.Y, B.Z, B.s, C.Y, C.Z, C.s, D.Y, D.Z, D.s)
            T.A.Y = -dxfMath.Determinant3by3(B.X, B.Z, B.s, C.X, C.Z, C.s, D.X, D.Z, D.s)
            T.A.Z = dxfMath.Determinant3by3(B.X, B.Y, B.s, C.X, C.Y, C.s, D.X, D.Y, D.s)
            T.A.s = -dxfMath.Determinant3by3(B.X, B.Y, B.Z, C.X, C.Y, C.Z, D.X, D.Y, D.Z)
            T.B.X = -dxfMath.Determinant3by3(A.Y, A.Z, A.s, C.Y, C.Z, C.s, D.Y, D.Z, D.s)
            T.B.Y = dxfMath.Determinant3by3(A.X, A.Z, A.s, C.X, C.Z, C.s, D.X, D.Z, D.s)
            T.B.Z = -dxfMath.Determinant3by3(A.X, A.Y, A.s, C.X, C.Y, C.s, D.X, D.Y, D.s)
            T.B.s = dxfMath.Determinant3by3(A.X, A.Y, A.Z, C.X, C.Y, C.Z, D.X, D.Y, D.Z)
            T.C.X = dxfMath.Determinant3by3(A.Y, A.Z, A.s, B.Y, B.Z, B.s, D.Y, D.Z, D.s)
            T.C.Y = -dxfMath.Determinant3by3(A.X, A.Z, A.s, B.X, B.Z, B.s, D.X, D.Z, D.s)
            T.C.Z = dxfMath.Determinant3by3(A.X, A.Y, A.s, B.X, B.Y, B.s, D.X, D.Y, D.s)
            T.C.s = -dxfMath.Determinant3by3(A.X, A.Y, A.Z, B.X, B.Y, B.Z, D.X, D.Y, D.Z)
            T.D.X = -dxfMath.Determinant3by3(A.Y, A.Z, A.s, B.Y, B.Z, B.s, C.Y, C.Z, C.s)
            T.D.Y = dxfMath.Determinant3by3(A.X, A.Z, A.s, B.X, B.Z, B.s, C.X, C.Z, C.s)
            T.D.Z = -dxfMath.Determinant3by3(A.X, A.Y, A.s, B.X, B.Y, B.s, C.X, C.Y, C.s)
            T.D.s = dxfMath.Determinant3by3(A.X, A.Y, A.Z, B.X, B.Y, B.Z, C.X, C.Y, C.Z)
            _rVal = T.Scaled(1 / Determinant).Transposed
            A = New TVECTRIX(_rVal.A)
            B = New TVECTRIX(_rVal.B)
            C = New TVECTRIX(_rVal.C)
            D = New TVECTRIX(_rVal.D)
        End Sub
        Public Function Inverted() As TMATRIX4
            Dim _rVal As TMATRIX4 = New TMATRIX4(Me)
            _rVal.Invert()
            Return _rVal
        End Function
        Public Sub Transpose()
            Dim _rVal As New TMATRIX4(Col1, Col2, Col3, Col4)
            A = _rVal.A
            B = _rVal.B
            C = _rVal.C
            D = _rVal.D

        End Sub
        Public Function Transposed() As TMATRIX4
            Dim _rVal As TMATRIX4 = New TMATRIX4(Me)
            _rVal.Transpose()
            Return _rVal
        End Function
        Public Function Interpolate(M As TMATRIX4, Alpha As Double) As TMATRIX4
            Return New TMATRIX4(A.Interpolate(M.A, Alpha), B.Interpolate(M.B, Alpha), C.Interpolate(M.C, Alpha), D.Interpolate(M.D, Alpha))
        End Function
        Public Overrides Function ToString() As String
            Return A.X & vbTab & A.Y & vbTab & A.Z & vbTab & A.s & vbCrLf &
B.X & vbTab & B.Y & vbTab & B.Z & vbTab & B.s & vbCrLf &
C.X & vbTab & C.Y & vbTab & C.Z & vbTab & C.s & vbCrLf &
 D.X & vbTab & D.Y & vbTab & D.Z & vbTab & D.s
        End Function
        Public Function Multiply(aVector As TVECTOR) As TVECTOR
            Dim rChanged As Boolean = False
            Return Multiply(aVector, rChanged)
        End Function
        Public Function Multiply(aVector As TVECTOR, ByRef rChanged As Boolean) As TVECTOR
            rChanged = False

            Dim _rVal As New TVECTOR(aVector)
            '^applies the transformation matrix to the passed vector
            Dim vtrx As TVECTRIX = Multiply(CType(aVector, TVECTRIX))
            Dim v1 As TVECTOR = CType(vtrx, TVECTOR)
            _rVal.X = v1.X
            _rVal.Y = v1.Y
            _rVal.Z = v1.Z
            rChanged = aVector.X <> _rVal.X Or aVector.Y <> _rVal.Y Or aVector.Z <> _rVal.Z
            Return _rVal
        End Function
        Public Function Multiply(aVectrix As TVECTRIX) As TVECTRIX
            Dim rChanged As Boolean = False
            Return Multiply(aVectrix, rChanged)
        End Function
        Public Function Multiply(aVectrix As TVECTRIX, ByRef rChanged As Boolean) As TVECTRIX
            rChanged = False
            Dim _rVal As New TVECTRIX(aVectrix)
            '^applies the transformation matrix to the passed vector
            Dim vtrx As New TVECTRIX(aVectrix)
            _rVal.X = vtrx.Dot(A)
            _rVal.Y = vtrx.Dot(B)
            _rVal.Z = vtrx.Dot(C)
            _rVal.s = vtrx.Dot(D)
            rChanged = _rVal.X <> aVectrix.X Or _rVal.Y <> aVectrix.Y Or _rVal.Z <> aVectrix.Z Or _rVal.s <> aVectrix.s
            Return _rVal
        End Function
#End Region 'Methods
#Region "Shared Methods"
        Public Shared Function ShearXY(aXShearAngle As Double, aYShearAngle As Double, Optional bInRadians As Boolean = True) As TMATRIX4

            If Not bInRadians Then
                aXShearAngle *= Math.PI / 180
                aYShearAngle *= Math.PI / 180
            End If
            Dim _rVal As New TMATRIX4(
    New TVECTRIX(1, aYShearAngle, 0, 0),
New TVECTRIX(aXShearAngle, 1, 0, 0),
New TVECTRIX(0, 0, 1, 0),
New TVECTRIX(0, 0, 0, 1)
    ) With {
                .Name = $"ShearXY_{ aXShearAngle }_{ aYShearAngle}"
    }
            Return _rVal
        End Function
        Public Shared Function YawMatrix(aAngle As Double, Optional bInRadians As Boolean = False) As TMATRIX4
            If Not bInRadians Then aAngle *= Math.PI / 180
            Return New TMATRIX4(New TVECTRIX(Math.Cos(aAngle), Math.Sin(aAngle), 0, 0), New TVECTRIX(-Math.Sin(aAngle), Math.Cos(aAngle), 0, 0), New TVECTRIX(0, 0, 1, 0), New TVECTRIX(0, 0, 0, 1), "YawMatrix")
        End Function
        Public Shared Function RollMatrix(aAngle As Double, Optional bInRadians As Boolean = False) As TMATRIX4
            If Not bInRadians Then aAngle *= Math.PI / 180
            Return New TMATRIX4(New TVECTRIX(1, 0, 0, 0), New TVECTRIX(0, Math.Cos(aAngle), Math.Sin(aAngle), 0), New TVECTRIX(0, -Math.Sin(aAngle), Math.Cos(aAngle), 0), New TVECTRIX(0, 0, 0, 1), "RollMatrix")
        End Function
        Public Shared Function PitchMatrix(aAngle As Double, Optional bInRadians As Boolean = False) As TMATRIX4
            If Not bInRadians Then aAngle *= Math.PI / 180
            Return New TMATRIX4(New TVECTRIX(Math.Cos(aAngle), Math.Sin(aAngle), 0, 0), New TVECTRIX(-Math.Sin(aAngle), Math.Cos(aAngle), 0, 0), New TVECTRIX(0, 0, 1, 0), New TVECTRIX(0, 0, 0, 1), "PitchMatrix")
        End Function
        Public Shared Function PlanarTransformMatrix(aPlane As TPLANE) As TMATRIX4
            If TPLANE.IsNull(aPlane) Then Return TMATRIX4.Identity()
            Dim x As TVECTOR = aPlane.XDirection
            Dim y As TVECTOR = aPlane.YDirection
            Dim z As TVECTOR = aPlane.ZDirection
            Dim o As TVECTOR = aPlane.Origin
            Return New TMATRIX4(New TVECTRIX(x.X, x.Y, x.Z, o.X), New TVECTRIX(y.X, y.Y, y.Z, o.Y), New TVECTRIX(z.X, z.Y, z.Z, o.Z), New TVECTRIX(0, 0, 0, 1), aPlane.Name)
        End Function
        Public Shared Function Identity() As TMATRIX4
            Return New TMATRIX4(New TVECTRIX(1, 0, 0, 0), New TVECTRIX(0, 1, 0, 0), New TVECTRIX(0, 0, 1, 0), New TVECTRIX(0, 0, 0, 1), "Identity")
        End Function
        Public Shared Function Null() As TMATRIX4
            Return New TMATRIX4(New TVECTRIX(0, 0, 0, 0), New TVECTRIX(0, 0, 0, 0), New TVECTRIX(0, 0, 0, 0), New TVECTRIX(0, 0, 0, 0), "Null")
        End Function
        Public Shared Function Random() As TMATRIX4
            Return New TMATRIX4(TVECTRIX.Random(), TVECTRIX.Random(), TVECTRIX.Random(), TVECTRIX.Random())
        End Function
#End Region 'Shared Methods"
#Region "Operators"
        Public Shared Operator +(A As TMATRIX4, B As TMATRIX4) As TMATRIX4
            Return New TMATRIX4((A.A + B.A), (A.B + B.B), (A.C + B.C), (A.D + B.D))
        End Operator
        Public Shared Operator -(A As TMATRIX4, B As TMATRIX4) As TMATRIX4
            Return New TMATRIX4((A.A - B.A), (A.B - B.B), (A.C - B.C), (A.D - B.D))
        End Operator
        Public Shared Operator *(A As TMATRIX4, B As TMATRIX4) As TMATRIX4
            Return A.Dot(B)

        End Operator
        Public Shared Operator \(A As TMATRIX4, B As TMATRIX4) As TMATRIX4
            Return A.Dot(B.Inverted())
        End Operator
#End Region 'Operators
    End Structure 'TMATRIX4

End Namespace
