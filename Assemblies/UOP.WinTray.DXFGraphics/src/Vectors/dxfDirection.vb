

Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public Class dxfDirection

        Implements iLine
#Region "Members"
        Private _RotationAxis As TVECTOR
        Private _Struc As TVECTOR
        Private _StrucLast As TVECTOR
#End Region 'Members
#Region "Events"
        Public Event DirectionChange()
        Public Event ComponentsChange()
#End Region 'Events
#Region "Constructors"
        Public Sub New(Optional aX As Double = 1, Optional aY As Double = 0, Optional aZ As Double = 0)
            _Struc = New TVECTOR(aX, aY, aZ)
            _Struc.Normalize()
            _StrucLast = New TVECTOR(_Struc)
        End Sub
        Public Sub New(aCoords As String)
            _Struc = TVECTOR.FromString(aCoords)
            _Struc.Normalize()
            _StrucLast = New TVECTOR(_Struc)
        End Sub
        Friend Sub New(aVector As TVECTOR)
            _Struc = New TVECTOR(aVector)
            _Struc.Normalize()
            _StrucLast = New TVECTOR(_Struc)
        End Sub
        Public Sub New(aVector As iVector)
            _Struc = New TVECTOR(aVector, True)
            _StrucLast = New TVECTOR(_Struc)
        End Sub

        Public Sub New(aDirection As dxfDirection)
            _Struc = New TVECTOR(aDirection)
            _StrucLast = New TVECTOR(_Struc)
        End Sub
        Public Sub New(aLine As iLine, bInvert As Boolean)
            _Struc = New TVECTOR(1, 0, 0)
            _StrucLast = New TVECTOR(_Struc)
            If aLine Is Nothing Then Return
            Dim v1 As TVECTOR = New TVECTOR(aLine.EndPt) - New TVECTOR(aLine.StartPt)
            If v1.IsNull(8) Then Return
            If bInvert Then v1 *= -1
            _Struc = v1.Normalized
            _StrucLast = New TVECTOR(_Struc)
        End Sub

#End Region 'Constructors
#Region "Properties"
        Public ReadOnly Property Angles As String
            Get
                Return $"({ XAngle},{ YAngle },{ ZAngle })"

            End Get
        End Property

        Public Property Components As String
            Get
                '^returns a text string containing the vector's coordinates
                '~vectors have a basic coordinate string like "(12.12,25.70,18.00)"
                Return _Struc.Coordinates(15)
            End Get
            Set(value As String)
                '^returns a text string containing the vector's coordinates
                '~vectors have a basic coordinate string like "(12.12,25.70,18.00)"
                Strukture = TVECTOR.DefineByString(value, _Struc)
            End Set
        End Property
        Public ReadOnly Property EndPt As dxfVector
            Get
                '^the end vector of a direction is eqault to its current components
                Return New dxfVector(_Struc)
            End Get
        End Property
        Public ReadOnly Property LastDirection As dxfDirection
            Get
                Return New dxfDirection(_StrucLast)
            End Get
        End Property
        Friend ReadOnly Property LastDirectionV As TVECTOR
            Get
                Return _StrucLast
            End Get
        End Property
        Public ReadOnly Property Length As Double
            Get
                Return _Struc.Magnitude
            End Get
        End Property
        Public Function Ordinate(aOrdinate As dxxOrdinateDescriptors) As Double

            Select Case aOrdinate
                Case dxxOrdinateDescriptors.Z
                    Return Z
                Case dxxOrdinateDescriptors.Y
                    Return Y
                Case Else
                    Return X
            End Select

        End Function

        Friend Property RotationAxis As TVECTOR
            Get
                Return _RotationAxis
            End Get
            Set(value As TVECTOR)
                _RotationAxis = value
            End Set
        End Property
        Public ReadOnly Property StartPt As dxfVector
            Get
                '^the start vector of a direction is always 0,0,0
                Return dxfVector.Zero
            End Get
        End Property
        Friend Property Strukture As TVECTOR
            Get
                Return _Struc
            End Get
            Set(value As TVECTOR)
                If TVECTOR.IsNull(value) Then Return
                value.Normalize()
                value.X = Math.Round(value.X, 15)
                value.Y = Math.Round(value.Y, 15)
                value.Z = Math.Round(value.Z, 15)
                If (_Struc.X <> value.X) Or (_Struc.Y <> value.Y) Or (_Struc.Z <> value.Z) Then
                    _StrucLast = _Struc
                    _Struc = value
                    RaiseEvent DirectionChange()
                    RaiseEvent ComponentsChange()
                End If
            End Set
        End Property
        Public Property X As Double 'Implements iVector.X
            Get
                Return _Struc.X
            End Get
            Set(value As Double)
                SetComponents(value, _Struc.Y, _Struc.Z)
            End Set
        End Property
        Public ReadOnly Property XAngle As Double
            Get
                '^the angle between this direction and the world X axis
                Return Math.Round(dxfMath.ArcCosine(_Struc.X, False) * 180 / Math.PI, 4)
            End Get
        End Property
        Public Property Y As Double 'Implements iVector.Y
            Get
                Return _Struc.Y
            End Get
            Set(value As Double)
                SetComponents(_Struc.X, value, _Struc.Z)
            End Set
        End Property
        Public ReadOnly Property YAngle As Double
            Get
                '^the angle between this direction and the world Y axis
                Return Math.Round(dxfMath.ArcCosine(_Struc.Y, False) * 180 / Math.PI, 4)
            End Get
        End Property
        Public Property Z As Double 'Implements iVector.Z
            Get
                Return _Struc.Z
            End Get
            Set(value As Double)
                SetComponents(_Struc.X, _Struc.Y, value)
            End Set
        End Property
        Public ReadOnly Property ZAngle As Double
            Get
                '^the angle between this direction and the world Z axis
                Return Math.Round(dxfMath.ArcCosine(_Struc.Z, False) * 180 / Math.PI, 4)
            End Get
        End Property
#End Region 'Properties
#Region "Methods"
        Public Function AlignTo(aDirection As iVector) As Boolean
            If aDirection Is Nothing Then Return False
            '#1the direct object to align to (must have single X, Y and Z properties)
            '^used to align the direction to the passed direction


            Return SetStructure(New dxfDirection(aDirection))
        End Function

        Public Function AngleTo(aDirectionObj As iVector, Optional aNormal As dxfDirection = Nothing) As Double
            Dim _rVal As Double
            '^the angle between this direction and the passed direction
            '~in degrees
            _rVal = dxfUtils.AngleBetweenVectorObjects(Me, aDirectionObj, aNormal)
            Return _rVal
        End Function

        Public Function AngleTo(aDirection As dxfDirection) As Double

            '^the angle between this direction and the passed direction
            '~in degrees
            Try
                Dim rNormal As dxfDirection = CrossProduct(aDirection)
                If aDirection Is Nothing Then Return 0

                Dim aN As New TVECTOR(rNormal)
                Dim v1 As New TVECTOR(Me)
                Dim v2 As New TVECTOR(aDirection)
                Return v1.AngleTo(v2, aN)

            Catch ex As Exception
                Throw New Exception($"dxfDirection.{ Reflection.MethodBase.GetCurrentMethod.Name} - { ex.Message}")
                Return 0
            End Try
            Return 0
        End Function

        Public Function AngleTo(aDirection As dxfDirection, ByRef rNormal As dxfDirection) As Double

            '^the angle between this direction and the passed direction
            '~in degrees
            Try
                rNormal = CrossProduct(aDirection)
                If aDirection Is Nothing Then Return 0

                Dim aN As New TVECTOR(rNormal)
                Dim v1 As New TVECTOR(Me)
                Dim v2 As New TVECTOR(aDirection)
                Return v1.AngleTo(v2, aN)

            Catch ex As Exception
                Throw New Exception($"dxfDirection.{ Reflection.MethodBase.GetCurrentMethod.Name} - { ex.Message}")
                Return 0
            End Try
            Return 0
        End Function

        Public Function Clone() As dxfDirection
            Return New dxfDirection(_Struc) With {.RotationAxis = _RotationAxis}
            '^returns a new object with properties matching those of the cloned object
        End Function
        Public Function ComponentAlong(aV As iVector) As dxfVector
            '^returns vectors component  along the passed vector
            Return New dxfVector With {.Strukture = ComponentAlongV(New TVECTOR(aV))}
        End Function
        Friend Function ComponentAlongV(aV As TVECTOR) As TVECTOR
            '^returns vectors component  along the passed vector
            Return _Struc.ComponentAlong(aV)
        End Function
        Public Function CoordinatesR(Optional aPrecis As Integer = 3) As String
            '^returns a text string containing the vector's coordinates rounded to the passed precisions
            Dim fmat As String = $"#,0:{New String("0", TVALUES.LimitedValue(aPrecis, 1, 15))}"
            Return $"({X:fmat},{Y:fmat},{Z:fmat})"
        End Function
        Public Function CrossProduct(aVectorObj As iVector) As dxfDirection
            Return New dxfDirection(_Struc.CrossProduct(New TVECTOR(aVectorObj)))
        End Function
        Public Function CrossProduct(aDirection As dxfDirection) As dxfDirection
            Return New dxfDirection(_Struc.CrossProduct(New TVECTOR(aDirection)))
        End Function
        Public Sub DefineByAngleX(aPlane As dxfPlane, aAngle As Double, Optional bInRadians As Boolean = False)

            Dim plane As New TPLANE(aPlane)
            Dim aDir As New dxfDirection(plane.XDirection)
            aDir.RotateAbout(New dxfVector(plane.ZDirection), aAngle, bInRadians)
            Strukture = aDir.Strukture

        End Sub
        Public Function EquatesTo(aX As Double, aY As Double, aZ As Double, Optional aPrecis As Integer = 4, Optional bCompareInverse As Boolean = True) As Boolean
            Dim rIsInverseEqual As Boolean = False
            Return EquatesTo(aX, aY, aZ, aPrecis, bCompareInverse, rIsInverseEqual)
        End Function
        Public Function EquatesTo(aX As Double, aY As Double, aZ As Double, aPrecis As Integer, bCompareInverse As Boolean, ByRef rIsInverseEqual As Boolean) As Boolean
            Dim _rVal As Boolean
            '#1 the X of the vector to compare to
            '#2 the Y of the vector to compare to
            '#3 the Z of the vector to compare to
            '#4the precision to apply to the comparision
            '#5flag to also compare the vector to the inverse of this one
            '^Test to see if the passed vector is at the same coordinates as the current vector
            Dim aVector As New TVECTOR(aX, aY, aZ)
            aVector.Normalize()
            rIsInverseEqual = False
            _rVal = aVector.Equals(_Struc, bCompareInverse, aPrecis, rIsInverseEqual)
            Return _rVal
        End Function

        Public Function Inverse() As dxfDirection
            '^returns the opposite direction
            Return New dxfDirection(_Struc * -1)
        End Function
        Public Sub Invert()
            '^reverses the direction
            Strukture = _Struc * -1
        End Sub
        Public Function IsEqual(aVectorObj As iVector, Optional aPrecis As Integer = 4, Optional bCompareInverse As Boolean = False) As Boolean
            Dim rIsInverseEqual As Boolean = False
            Return IsEqual(aVectorObj, aPrecis, bCompareInverse, rIsInverseEqual)
        End Function
        Public Function IsEqual(aVectorObj As iVector, aPrecis As Integer, bCompareInverse As Boolean, ByRef rIsInverseEqual As Boolean) As Boolean
            rIsInverseEqual = False

            '#1 the vector to compare to
            '#2the precision to apply to the comparision
            '#3flag to also compare the vector to the inverse of this one
            '^Test to see if the passed vector is at the same coordinates as the current vector
            Dim aVector As New TVECTOR(aVectorObj)
            Dim _rVal As Boolean = aVector.Normalized.Equals(_Struc, aPrecis)
            If bCompareInverse And Not _rVal Then
                rIsInverseEqual = (aVector * -1).Equals(_Struc, aPrecis)
                _rVal = rIsInverseEqual
            End If
            Return _rVal
        End Function

        Public Function IsEqual(aDirection As dxfDirection, Optional aPrecis As Integer = 4, Optional bCompareInverse As Boolean = False) As Boolean
            Dim rIsInverseEqual As Boolean = False
            Return IsEqual(aDirection, aPrecis, bCompareInverse, rIsInverseEqual)
        End Function
        Public Function IsEqual(aDirection As dxfDirection, aPrecis As Integer, bCompareInverse As Boolean, ByRef rIsInverseEqual As Boolean) As Boolean
            rIsInverseEqual = False

            '#1 the vector to compare to
            '#2the precision to apply to the comparision
            '#3flag to also compare the vector to the inverse of this one
            '^Test to see if the passed vector is at the same coordinates as the current vector
            Dim aVector As New TVECTOR(aDirection)
            Dim _rVal As Boolean = aVector.Equals(_Struc, aPrecis)
            If bCompareInverse And Not _rVal Then
                rIsInverseEqual = (aVector * -1).Equals(_Struc, aPrecis)
                _rVal = rIsInverseEqual
            End If
            Return _rVal
        End Function

        Public Function IsUnity(Optional aPrecis As Integer = 6) As Boolean
            '^Test to see if the vector is at 1,1,1
            '~the difference in coordinates must be less that 0.000001 inches to be considered the same.
            Return _Struc.Equals(New TVECTOR(1, 1, 1), aPrecis)
        End Function

        Public Function IsWorld(aPrimary As dxxOrdinateDescriptors, Optional bEitherWay As Boolean = True, Optional aPrecis As Double = 3) As Boolean
            If aPrimary = dxxOrdinateDescriptors.Z Then
                Return _Struc.Equals(TVECTOR.WorldZ, bEitherWay, aPrecis)
            ElseIf aPrimary = dxxOrdinateDescriptors.Y Then
                Return _Struc.Equals(TVECTOR.WorldY, bEitherWay, aPrecis)
            Else
                Return _Struc.Equals(TVECTOR.WorldX, bEitherWay, aPrecis)
            End If
        End Function
        Public Function IsZero(Optional aPrecis As Integer = 6) As Boolean

            '^Test to see if the vector is at 0,0,0
            '~the difference in coordinates must be less that 0.000001 inches to be considered the same.
            Return _Struc.Equals(TVECTOR.Zero, aPrecis)

        End Function
        Public Function Mirror(aLineObj As iLine, Optional bSuppressEvnt As Boolean = False) As Boolean

            '#1the line to mirror across
            '^mirrors the vector across the passed line
            '~returns True if the vector actually moves from this process
            If aLineObj Is Nothing Then Return False
            Return Mirror(New TVECTOR(aLineObj.StartPt), New TVECTOR(aLineObj.EndPt), bSuppressEvnt)
        End Function
        Friend Function Mirror(aSP As TVECTOR, aEP As TVECTOR, Optional bSuppressEvnt As Boolean = False) As Boolean


            '#1the start pt of the line to mirror across
            '#2the endt pt of the line to mirror across
            '#3flag to suppress change events
            '^mirrors the vector across the passed line
            '~returns True if the vector actually moves from this process
            If dxfProjections.DistanceTo(aSP, aEP) < 0.00001 Then Return False
            Dim v1 As New TVECTOR(_Struc)
            Dim _rVal As Boolean = v1.Mirror(New TLINE(aSP, aEP))
            If _rVal Then
                SetStructure(v1)
                If Not bSuppressEvnt Then RaiseEvent DirectionChange()
                RaiseEvent ComponentsChange()
            End If
            Return _rVal
        End Function
        Public Function Mirrored(aLineObj As iLine) As dxfDirection
            Dim _rVal As New dxfDirection(Me)
            '#1the line to mirror across
            '^returns the vector mirrored across the passed line

            _rVal.Mirror(aLineObj)
            Return _rVal
        End Function
        Public Function ProjectToPlane(aPlane As dxfPlane, Optional aDirection As iVector = Nothing) As dxfDirection
            Dim rDistance As Double = 0.0
            Return ProjectToPlane(aPlane, aDirection, rDistance)
        End Function
        Public Function ProjectToPlane(aPlane As dxfPlane, aDirection As iVector, ByRef rDistance As Double) As dxfDirection
            Dim _rVal As dxfDirection
            Dim P1 As New dxfVector(_Struc)
            P1.ProjectToPlane(aPlane, aDirection, rDistance)
            _rVal = New dxfDirection
            If Not P1.IsZero Then _rVal.Strukture = P1.Strukture
            Return _rVal
        End Function

        Public Function Rotate(aAngle As Double, Optional bInRadians As Boolean = False, Optional aPlane As dxfPlane = Nothing, Optional aAxis As dxeLine = Nothing, Optional bSuppressEvents As Boolean = False) As Boolean
            Dim _rVal As Boolean
            '#1the angle to rotate
            '#2flag indicating the passed angle is in radians
            '#3the OCS which is used as the Z axis of rotation if the paased object is a vector
            '#4returns the axis was was used for the rotation
            '^rotates the entity about the z axis or the passed coordinate system the requested angle
            If dxfPlane.IsNull(aPlane) Then
                If (Math.Round((_RotationAxis.X), 3) = 0 And Math.Round((_RotationAxis.Y), 3) = 0 And Math.Round((_RotationAxis.Z), 3) = 0) Then
                    aAxis = dxeLine.ByProjection(dxfVector.Zero, New dxfVector(0, 0, 1), 10)
                Else
                    aAxis = New dxeLine
                    aAxis.EndPt.SetStructure(_RotationAxis)
                End If
            Else
                aAxis = aPlane.ZAxis(10)
            End If
            If bInRadians Then aAngle *= 180 / Math.PI
            aAngle = TVALUES.NormAng(aAngle, False, True)
            If aAngle = 0 Then Return _rVal
            Dim oldV As TVECTOR
            Dim newV As TVECTOR
            oldV = _Struc
            newV = oldV.RotatedAbout(New TVECTOR(0, 0, 0), aAxis.Direction.Strukture, aAngle, False)
            _rVal = Not oldV.Equals(newV, 0)
            If Not _rVal Then Return _rVal
            '==============================================
            If Not bSuppressEvents Then
                SetComponents(newV.X, newV.Y, newV.Z)
            Else
                SetStructure(newV)
            End If
            Return _rVal
        End Function
        Public Function RotateAboutLine(aLine As iLine, aAngle As Double, Optional bInRadians As Boolean = False, Optional aPlane As dxfPlane = Nothing, Optional bSuppressEvents As Boolean = False) As Boolean
            '#1the line or vector to rotate about
            '#2the angle to rotate
            '#3flag indicating the passed angle is in radians
            '#4the OCS which is used as the Z axis of rotation if the paased object is a vector
            '#5returns the axis was was used for the rotation
            '^rotates the entity about the passed axis the requested angle
            '~if the passed object is a vector the entity is rotated about the Z axis of the passed coordinated system
            Dim aAxis As dxeLine = dxfPlane.CreateAxis(aPlane, aLine)
            If aAxis Is Nothing Then Return False
            Return RotateAboutV(aAxis.DirectionV, aAngle, bInRadians, bSuppressEvents)
        End Function
        Public Function RotateAboutLine(aLine As iLine, aAngle As Double, ByRef rAxis As dxeLine, Optional bInRadians As Boolean = False, Optional aPlane As dxfPlane = Nothing, Optional bSuppressEvents As Boolean = False) As Boolean
            '#1the line or vector to rotate about
            '#2the angle to rotate
            '#3flag indicating the passed angle is in radians
            '#4the OCS which is used as the Z axis of rotation if the paased object is a vector
            '#5returns the axis was was used for the rotation
            '^rotates the entity about the passed axis the requested angle
            '~if the passed object is a vector the entity is rotated about the Z axis of the passed coordinated system
            rAxis = dxfPlane.CreateAxis(aPlane, aLine)
            If rAxis Is Nothing Then Return False
            Return RotateAboutV(rAxis.DirectionV, aAngle, bInRadians, bSuppressEvents)
        End Function
        Public Function RotateAbout(aPoint As iVector, aAngle As Double, Optional bInRadians As Boolean = False, Optional aPlane As dxfPlane = Nothing, Optional bSuppressEvents As Boolean = False) As Boolean
            '#1the line or vector to rotate about
            '#2the angle to rotate
            '#3flag indicating the passed angle is in radians
            '#4the OCS which is used as the Z axis of rotation if the paased object is a vector
            '#5returns the axis was was used for the rotation
            '^rotates the entity about the passed axis the requested angle
            '~if the passed object is a vector the entity is rotated about the Z axis of the passed coordinated system
            Dim aAxis As dxeLine = dxfPlane.CreateAxis(aPlane, aPoint)
            If aAxis Is Nothing Then Return False
            Return RotateAboutV(aAxis.DirectionV, aAngle, bInRadians, bSuppressEvents)
        End Function

        Public Function RotateAbout(aPoint As iVector, rAxis As dxeLine, aAngle As Double, Optional bInRadians As Boolean = False, Optional aPlane As dxfPlane = Nothing, Optional bSuppressEvents As Boolean = False) As Boolean
            '#1the line or vector to rotate about
            '#2the angle to rotate
            '#3flag indicating the passed angle is in radians
            '#4the OCS which is used as the Z axis of rotation if the paased object is a vector
            '#5returns the axis was was used for the rotation
            '^rotates the entity about the passed axis the requested angle
            '~if the passed object is a vector the entity is rotated about the Z axis of the passed coordinated system
            rAxis = dxfPlane.CreateAxis(aPlane, aPoint)
            If rAxis Is Nothing Then Return False
            Return RotateAboutV(rAxis.DirectionV, aAngle, bInRadians, bSuppressEvents)
        End Function
        Friend Function RotateAboutV(aDirection As TVECTOR, aAngle As Double, Optional bInRadians As Boolean = False, Optional bSuppressEvents As Boolean = False) As Boolean
            Dim _rVal As Boolean
            '#1the line or vector to rotate about
            '#2the angle to rotate
            '#3flag indicating the passed angle is in radians
            '#4the OCS which is used as the Z axis of rotation if the paased object is a vector
            '#5returns the axis was was used for the rotation
            '^rotates the entity about the passed axis the requested angle
            '~if the passed object is a vector the entity is rotated about the Z axis of the passed coordinated system
            If aDirection.Equals(_Struc, 0) Then Return _rVal
            If bInRadians Then aAngle *= 180 / Math.PI
            aAngle = TVALUES.NormAng(aAngle, False, True)
            If aAngle = 0 Then Return _rVal
            Dim oldV As TVECTOR
            Dim newV As TVECTOR
            oldV = _Struc
            newV = oldV.RotatedAbout(aDirection, aAngle, False).Normalized
            _rVal = Not oldV.Equals(newV, 0)
            If Not _rVal Then Return _rVal
            _StrucLast = _Struc
            '==============================================
            If Not bSuppressEvents Then
                SetComponents(newV.X, newV.Y, newV.Z)
            Else
                SetStructure(newV)
            End If
            Return _rVal
        End Function
        Public Function Rotated(aAngle As Double, Optional bInRadians As Boolean = False, Optional aPlane As dxfPlane = Nothing,
                                Optional aAxis As dxeLine = Nothing) As dxfDirection
            ', Optional bSuppressEvents As Boolean = False
            Dim _rVal As dxfDirection
            '#1the angle to rotate
            '#2flag indicating the passed angle is in radians
            '#3the OCS which is used as the Z axis of rotation if the paased object is a vector
            '#4returns the axis was was used for the rotation
            '^rotates the entity about the z axis or the passed coordinate system the requested angle
            _rVal = Clone()
            _rVal.Rotate(aAngle, bInRadians, aPlane, aAxis, True)
            Return _rVal
        End Function
        Public Function SetComponents(Optional NewX As Double = 0, Optional NewY As Double = 0, Optional NewZ As Double = 0) As Boolean
            Dim _rVal As Boolean
            '#1the value to set the X coordinate to
            '#2the value to set the Y coordinate to
            '#3the value to set the Z coordinate to
            '^used to set the coordinates of an existing vector object
            '~this is the default method of the class
            Dim XChange As Double
            Dim YChange As Double
            Dim ZChange As Double
            XChange = TVALUES.To_DBL(NewX, aPrecis:=15, aDefault:=_Struc.X) - _Struc.X
            YChange = TVALUES.To_DBL(NewY, aPrecis:=15, aDefault:=_Struc.Y) - _Struc.Y
            ZChange = TVALUES.To_DBL(NewZ, aPrecis:=15, aDefault:=_Struc.Z) - _Struc.Z
            If XChange = 0 And YChange = 0 And ZChange = 0 Then Return _rVal
            '==============================================
            Dim newV As TVECTOR
            newV = (_Struc + New TVECTOR(XChange, YChange, ZChange)).Normalized()
            newV.X = Math.Round(newV.X, 15)
            newV.Y = Math.Round(newV.Y, 15)
            newV.Z = Math.Round(newV.Z, 15)
            _rVal = (newV.X <> _Struc.X) Or (newV.Y <> _Struc.Y) Or (newV.Z <> _Struc.Z)
            If _rVal Then
                _StrucLast = _Struc
                _Struc = newV
                RaiseEvent DirectionChange()
                RaiseEvent ComponentsChange()
            End If
            Return _rVal
        End Function
        Friend Function SetStructure(newStruc As TVECTOR) As Boolean
            Dim _rVal As Boolean
            If TVECTOR.IsNull(newStruc) Then Return _rVal
            newStruc.Normalize()
            If Not TVECTOR.IsNull(newStruc, 15) Then
                newStruc.X = Math.Round(newStruc.X, 15)
                newStruc.Y = Math.Round(newStruc.Y, 15)
                newStruc.Z = Math.Round(newStruc.Z, 15)
                _rVal = (newStruc.X <> _Struc.X) Or (newStruc.Y <> _Struc.Y) Or (newStruc.Z <> _Struc.Z)
                If _rVal Then
                    _StrucLast = _Struc
                    _Struc = newStruc
                    _rVal = True
                    RaiseEvent ComponentsChange()
                End If
            End If
            Return _rVal
        End Function
        Friend Function SetStructure(newStruc As iVector) As Boolean

            If TVECTOR.IsNull(newStruc) Then Return False
            Dim v1 As New TVECTOR(newStruc)
            v1.Normalize()
            If TVECTOR.IsNull(v1, 15) Then Return False
            v1.X = Math.Round(v1.X, 15)
            v1.Y = Math.Round(v1.Y, 15)
            v1.Z = Math.Round(v1.Z, 15)
            If (v1.X = _Struc.X) And (v1.Y = _Struc.Y) And (v1.Z = _Struc.Z) Then Return False

            _StrucLast = _Struc
            _Struc = v1

            RaiseEvent ComponentsChange()
            Return True

        End Function
        Public ReadOnly Property ToVector As dxfVector
            Get
                Return New dxfVector(X, Y, Z)
            End Get
        End Property
        Public Overrides Function ToString() As String
            If Z <> 0 Then
                Return $"dxfDirection [{X:0.000###},{ Y:0.000###},{ Z:0.000###}]"
            Else
                Return $"dxfDirection [{ X:0.000###},{ Y:0.000###}]"
            End If
        End Function
#End Region 'Methods
#Region "Shared Methods"


        Public Shared ReadOnly Property WorldX As dxfDirection
            Get
                Return New dxfDirection(1, 0, 0)
            End Get
        End Property

        Public Shared ReadOnly Property WorldY As dxfDirection
            Get
                Return New dxfDirection(0, 1, 0)
            End Get
        End Property
        Public Shared ReadOnly Property WorldZ As dxfDirection
            Get
                Return New dxfDirection(0, 0, 1)
            End Get
        End Property

        Public Property Tag As String ' Implements iVector.Tag
            Get
                Return String.Empty
            End Get
            Set(value As String)
                value = String.Empty
            End Set
        End Property

        Public Property Flag As String 'Implements iVector.Flag
            Get
                Return String.Empty
            End Get
            Set(value As String)
                value = String.Empty
            End Set
        End Property

        Private Property iLine_StartPt As iVector Implements iLine.StartPt
            Get
                Return StartPt
            End Get
            Set(value As iVector)
                ''Throw New NotImplementedException()
            End Set
        End Property

        Private Property iLine_EndPt As iVector Implements iLine.EndPt
            Get
                Return EndPt
            End Get
            Set(value As iVector)
                'Throw New NotImplementedException()
            End Set
        End Property



#End Region 'Shared Methods
#Region "Operators"
        Public Shared Operator +(A As dxfDirection, B As dxfDirection) As dxfDirection
            Return New dxfDirection(A.X + B.X, A.Y + B.Y, A.Z + B.Z)
        End Operator
        Public Shared Operator -(A As dxfDirection, B As dxfDirection) As dxfDirection
            Return New dxfDirection(A.X - B.X, A.Y - B.Y, A.Z - B.Z)
        End Operator
        Public Shared Operator *(A As dxfDirection, aScaler As Double) As dxfVector
            Return New dxfVector(A.X * aScaler, A.Y * aScaler, A.Z * aScaler)
        End Operator
        Public Shared Operator /(A As dxfDirection, aScaler As Double) As dxfVector
            Return New dxfVector(A.X / aScaler, A.Y / aScaler, A.Z / aScaler)
        End Operator
        Public Shared Operator *(A As dxfDirection, B As dxfDirection) As dxfDirection
            Return New dxfDirection(A.X * B.X, A.Y * B.Y, A.Z * B.Z)
        End Operator
        Public Shared Operator =(A As dxfDirection, B As dxfDirection) As Boolean
            If A Is Nothing And B IS Nothing Then Return True
            If A Is Nothing Or B IS Nothing Then Return False
            Return (A.X = B.X) And (A.Y = B.Y) And (A.Z = B.Z)
        End Operator
        Public Shared Operator <>(A As dxfDirection, B As dxfDirection) As Boolean
            If A Is Nothing And B IS Nothing Then Return False
            If A Is Nothing Or B IS Nothing Then Return True
            Return (A.X <> B.X) Or (A.Y <> B.Y) Or (A.Z <> B.Z)
        End Operator
#End Region 'Operators
    End Class 'dxfDirection
End Namespace
