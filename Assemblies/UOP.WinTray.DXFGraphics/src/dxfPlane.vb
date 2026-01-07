
Imports UOP.DXFGraphics.Utilities

Imports UOP.DXFGraphics.Structures
Imports Microsoft.VisualBasic.Devices

Namespace UOP.DXFGraphics
    Public Class dxfPlane

#Region "Members"
        Private _Struc As TPLANE
        Friend OwnerPtr As WeakReference
#End Region 'Members
#Region "Events"
        Public Event PlaneChange(aEvent As dxfPlaneEvent)
#End Region 'Events
#Region "Constructors"
        Public Sub New()
            Init()
        End Sub
        Friend Sub New(aStructure As TPLANE, Optional aImageGUID As String = "")
            Init()
            _Struc = aStructure
            _Struc.ImageGUID = aImageGUID
        End Sub
        Public Sub New(aOrigin As iVector, Optional aRotation As Double = 0)
            Init()
            _Struc.Origin = New TVECTOR(aOrigin)
            If aRotation <> 0 Then
                Revolve(aRotation)
            End If
        End Sub

        Public Sub New(aOrigin As iVector, aZDir As iVector, Optional aRotation As Double = 0)
            Init()
            _Struc.Origin = New TVECTOR(aOrigin)
            If aZDir IsNot Nothing Then
                Dim v1 As New TVECTOR(aZDir, True)
                ExtrusionDirection = v1.Coordinates
            End If
            If aRotation <> 0 Then
                Revolve(aRotation)
            End If
        End Sub

        Public Sub New(aOrigin As iVector, aZDir As dxfDirection, Optional aRotation As Double = 0)
            Init()
            _Struc.Origin = New TVECTOR(aOrigin)
            If aZDir IsNot Nothing Then
                Dim v1 As New TVECTOR(aZDir)
                ExtrusionDirection = v1.Coordinates
            End If
            If aRotation <> 0 Then
                Revolve(aRotation)
            End If
        End Sub

        Public Sub New(aX As Double, aY As Double, Optional aZ As Double = 0, Optional aZDir As iVector = Nothing)
            Init()
            _Struc.Origin = New TVECTOR(aX, aY, aZ)
            If aZDir IsNot Nothing Then
                Dim v1 As New TVECTOR(aZDir)
                v1.Normalize()
                ExtrusionDirection = v1.Coordinates
            End If
        End Sub

        Public Sub New(aOrigin As iVector, aXDir As dxfDirection, aYDir As dxfDirection)
            Init()

            _Struc.Origin = New TVECTOR(aOrigin)
            Dim xdir As TVECTOR = _Struc.XDirection
            Dim ydir As TVECTOR = _Struc.YDirection

            If aXDir Is Nothing And aYDir Is Nothing Then Return
            If aXDir IsNot Nothing Then
                xdir = New TVECTOR(aXDir)
            End If
            If aYDir IsNot Nothing Then
                ydir = New TVECTOR(aYDir)
            End If

            _Struc = New TPLANE(_Struc.Origin, xdir, ydir)
        End Sub

        Public Sub New(aPlane As dxfPlane, Optional aOrigin As iVector = Nothing, Optional aRotation As Double = 0)
            _Struc = New TPLANE(aPlane)
            Dim nullplane As Boolean = dxfPlane.IsNull(aPlane)
            Dim xdir As TVECTOR = _Struc.XDirection
            Dim ydir As TVECTOR = _Struc.YDirection
            Dim org As TVECTOR = _Struc.Origin

            If Not nullplane Then
                xdir = New TVECTOR(aPlane.XDirection)
                ydir = New TVECTOR(aPlane.YDirection)

            End If

            If aOrigin IsNot Nothing Then org = New TVECTOR(aOrigin)
            If aRotation <> 0 Then
                xdir.RotateAbout(TVECTOR.WorldZ, aRotation)
                ydir.RotateAbout(TVECTOR.WorldZ, aRotation)
            End If

            _Struc.Define(org, xdir, ydir)
        End Sub
        Private Sub Init()
            _Struc = TPLANE.World
            OwnerPtr = Nothing
        End Sub
#End Region 'Constructors
#Region "Properties"
        Friend Overridable ReadOnly Property MyOwner As dxfHandleOwner
            Get
                If OwnerPtr Is Nothing Then Return Nothing
                If Not OwnerPtr.IsAlive Then
                    OwnerPtr = Nothing
                    Return Nothing
                End If
                Dim _rVal As dxfHandleOwner = TryCast(OwnerPtr.Target, dxfHandleOwner)
                Return _rVal
            End Get
        End Property
        Public ReadOnly Property Angles As dxfVector
            Get
                Return CType(_Struc.EulerAngles, dxfVector)
            End Get
        End Property
        Public ReadOnly Property Ascent As Double
            Get
                Return _Struc.Height - _Struc.Descent
            End Get
        End Property
        Public ReadOnly Property AspectRatio As Double
            Get
                '^the ratio of the width to the height (w/h)
                Return _Struc.AspectRatio(False)
            End Get
        End Property
        Friend Property Descent As Double
            Get
                Return _Struc.Descent
            End Get
            Set(value As Double)
                _Struc.Descent = Math.Abs(value)
            End Set
        End Property


        Public ReadOnly Property Diagonal As Double
            Get
                '^the length of the diagonal of the rectangle
                If _Struc.Width <> 0 Or _Struc.Height <> 0 Then Return Math.Sqrt(_Struc.Width ^ 2 + _Struc.Height ^ 2) Else Return 0
            End Get
        End Property
        Friend Property DisplayProps As TDISPLAYVARS
            Get
                Return _Struc.DisplayProps
            End Get
            Set(value As TDISPLAYVARS)
                _Struc.DisplayProps = value
            End Set
        End Property
        Public Property ExtrusionDirection As String
            Get
                Return ZDirection.Components
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = "(0,0,1)" Else value = value.Trim()
                Dim aZDir As New dxfDirection(value)
                Dim aPln As TPLANE = TPLANE.ArbitraryCS(aZDir.Strukture, True, False)
                Define(_Struc.Origin, aPln.XDirection, aPln.YDirection)
            End Set
        End Property
        Public Property Height As Double
            Get
                '^a height asigned to the plane
                Return _Struc.Height
            End Get
            Set(value As Double)
                '^a height asigned to the plane
                value = Math.Abs(value)
                If _Struc.Height <> value Then
                    _Struc.Height = value
                    RaiseChangeEvent(dxxCoordinateSystemEventTypes.Dimensions)
                End If
            End Set
        End Property
        Friend ReadOnly Property Image As dxfImage
            Get
                If Not String.IsNullOrWhiteSpace(_Struc.ImageGUID) Then Return dxfEvents.GetImage(_Struc.ImageGUID) Else Return Nothing
            End Get
        End Property
        Friend Property ImageGUID As String
            Get
                Return _Struc.ImageGUID
            End Get
            Set(value As String)
                _Struc.ImageGUID = value
            End Set
        End Property
        Public ReadOnly Property IsDefined As Boolean
            Get
                Return dxfPlane.IsNull(Me)
            End Get
        End Property
        Public Property IsDirty As Boolean
            Get
                Return _Struc.IsDirty
            End Get
            Set(value As Boolean)
                _Struc.IsDirty = value
            End Set
        End Property
        Public ReadOnly Property IsWorld As Boolean
            Get
                '^a coordinate system is the world system if has nor translation, rotation or scales
                Return _Struc.IsWorld
            End Get
        End Property
        Public Property Name As String
            Get
                '^a user assigned name for the plane
                Return _Struc.Name
            End Get
            Set(value As String)
                _Struc.Name = IIf(value Is Nothing, "", value.Trim())
            End Set
        End Property

        Public Property Tag As String
            Get
                '^a user assigned tag for the plane
                Return _Struc.Tag
            End Get
            Set(value As String)
                _Struc.Tag = IIf(value Is Nothing, "", value)
            End Set
        End Property

        Public Property Flag As String
            Get
                '^a user assigned flag for the plane
                Return _Struc.Flag
            End Get
            Set(value As String)
                _Struc.Flag = IIf(value Is Nothing, "", value)
            End Set
        End Property

        Public Property Handle As String
            Get
                '^a user assigned handle for the plane
                Return _Struc.Handle
            End Get
            Friend Set(value As String)
                _Struc.Handle = IIf(value Is Nothing, "", value.Trim())
            End Set
        End Property

        Public Property Value As Object
            Get
                '^a user assigned handle for the plane
                Return _Struc.Value
            End Get
            Friend Set(value As Object)
                _Struc.Value = value
            End Set
        End Property
        Public ReadOnly Property Normal As dxfDirection
            Get
                '^the planes normal direction (Z direction)
                Return New dxfDirection With {.Strukture = _Struc.ZDirection, .RotationAxis = _Struc.XDirection}
            End Get
        End Property
        Public Property Origin As dxfVector
            '^returns a new vector located at the cuurent origin
            '~applying translations etc. to this point does not affect the planes origin.
            '~the only way to move the plane is through it's Move, Project, Translate etc. functions or by using Set .Origin = .
            Get
                Return New dxfVector(_Struc.Origin)
            End Get
            Set(value As dxfVector)

                OriginV = New TVECTOR(value)
            End Set
        End Property
        Friend Property OriginV As TVECTOR
            Get
                '^the origin of the plane
                Return _Struc.Origin
            End Get
            Set(value As TVECTOR)
                '^the origin of the plane
                Define(value, _Struc.XDirection, _Struc.YDirection)
            End Set
        End Property
        Public ReadOnly Property Pitch As Double
            Get
                '^the angle between the y axis and the global y axis
                '~rotation about gobal Y axis
                Return Angles.Y
            End Get
        End Property
        Public ReadOnly Property PixelHeight As Double
            Get
                '^a height in pixels based on the current units
                Return _Struc.Height * ToPixels
            End Get
        End Property
        Public ReadOnly Property PixelWidth As Double
            Get
                '^a width in pixels based on the current units
                Return _Struc.Width * ToPixels
            End Get
        End Property
        Public ReadOnly Property Roll As Double
            Get
                '^the angle between the z axis and the global z axis
                '~rotation about global X axis
                Return Angles.X
            End Get
        End Property
        Friend Property ShearAngle As Double
            Get
                Return _Struc.ShearAngle
            End Get
            Set(value As Double)
                _Struc.ShearAngle = value
            End Set
        End Property

        Friend Property Index As Integer
            Get
                Return _Struc.Index
            End Get
            Set(value As Integer)
                _Struc.Index = value
            End Set
        End Property
        Friend Property Strukture As TPLANE
            Get
                '^the planes structure which carries its variables
                Return _Struc
            End Get
            Set(value As TPLANE)
                If TPLANE.IsNull(value) Then Return
                _Struc = New TPLANE(value)
            End Set
        End Property
        Public ReadOnly Property ToInches As Double
            Get
                Return dxfUtils.FactorToInches(_Struc.Units)
            End Get
        End Property
        Public ReadOnly Property ToPixels As Double
            Get
                Return dxfUtils.FactorFromTo(_Struc.Units, dxxDeviceUnits.Pixels)
            End Get
        End Property
        Public ReadOnly Property UnitName As String
            Get
                '^the name of the units assigned tot the plane
                Return dxfEnums.Description(_Struc.Units)
            End Get
        End Property
        Friend Property Units As dxxDeviceUnits
            Get
                '^the units assigned tot the plane
                Return _Struc.Units
            End Get
            Set(value As dxxDeviceUnits)
                '^the units assigned tot the plane
                If value < 1 Or value > 7 Then Return
                If _Struc.Units <> value Then
                    Dim uscale As Double
                    If _Struc.Units <> dxxDeviceUnits.Pixels Then
                        uscale = ToPixels
                        _Struc.Width *= uscale
                        _Struc.Height *= uscale
                        If _Struc.Descent <> 0 Then
                            _Struc.Descent *= uscale
                        End If
                    End If
                    _Struc.Units = value
                    uscale = dxfUtils.FactorFromTo(dxxDeviceUnits.Pixels, value)
                    _Struc.Width *= uscale
                    _Struc.Height *= uscale
                    If _Struc.Descent <> 0 Then
                        _Struc.Descent *= uscale
                    End If
                    RaiseChangeEvent(dxxCoordinateSystemEventTypes.Units)
                End If
            End Set
        End Property
        Public Property Width As Double
            Get
                '^a width assigned to the plane
                Return _Struc.Width
            End Get
            Set(value As Double)
                '^a width assigned to the plane
                value = Math.Abs(value)
                If _Struc.Width <> value Then
                    _Struc.Width = value
                    RaiseChangeEvent(dxxCoordinateSystemEventTypes.Dimensions)
                End If
            End Set
        End Property
        Public Property X As Double
            Get
                Return _Struc.Origin.X
            End Get
            Set(value As Double)
                SetCoordinates(value)
            End Set
        End Property
        Public ReadOnly Property XDirection As dxfDirection
            Get
                '^the X direction of the plane
                Return New dxfDirection With {.Strukture = _Struc.XDirection, .RotationAxis = _Struc.ZDirection}
            End Get
        End Property
        Friend Property XDirectionV As TVECTOR
            Get
                '^the X direction of the plane
                Return _Struc.XDirection
            End Get
            Set(value As TVECTOR)
                AlignXToV(value)
            End Set
        End Property
        Public Property Y As Double
            Get
                Return _Struc.Origin.Y
            End Get
            Set(value As Double)
                SetCoordinates(NewY:=value)
            End Set
        End Property
        Public ReadOnly Property Yaw As Double
            Get
                '^the angle between the x axis and the global x axis
                '~rotation about global X axis
                Return Angles.Z
            End Get
        End Property
        Public ReadOnly Property YDirection As dxfDirection
            Get
                '^the Y direction of the plane
                Return New dxfDirection With {.Strukture = _Struc.YDirection, .RotationAxis = _Struc.ZDirection}
            End Get
        End Property
        Friend Property YDirectionV As TVECTOR
            Get
                '^the Y direction of the plane
                Return _Struc.YDirection
            End Get
            Set(value As TVECTOR)
                AlignYToV(value)
            End Set
        End Property
        Public Property Z As Double
            Get
                Return _Struc.Origin.Z
            End Get
            Set(value As Double)
                SetCoordinates(NewZ:=value)
            End Set
        End Property
        Public ReadOnly Property ZDirection As dxfDirection
            Get
                '^the Z direction of the plane
                Return New dxfDirection With {.Strukture = _Struc.ZDirection, .RotationAxis = _Struc.XDirection}
            End Get
        End Property
        Friend Property ZDirectionV As TVECTOR
            Get
                '^the planes normal direction (Z direction)
                Return _Struc.ZDirection
            End Get
            Set(value As TVECTOR)
                AlignZToV(value)
            End Set
        End Property
#End Region 'Properties
#Region "Methods"

        Function Descriptor(Optional bIncludeAspect As Boolean = False, Optional bIncludeName As Boolean = True, Optional aPrecis As Integer = 3) As String

            '^a string that describes the Plane
            Return _Struc.Descriptor(bIncludeAspect, bIncludeName, aPrecis)

        End Function

        Private Sub RaiseChangeEvent(aType As dxxCoordinateSystemEventTypes)
            Dim aEvent As New dxfPlaneEvent(aType, aPlane:=Me)
            RaiseEvent PlaneChange(aEvent)
            Dim owner As dxfHandleOwner = MyOwner
            If owner IsNot Nothing Then owner.RespondToPlaneChangeEvent(aEvent)
        End Sub
        Public Function AlignTo(aPlane As dxfPlane, Optional bMoveTo As Boolean = False) As Boolean
            Dim _rVal As Boolean
            Dim aPl As New TPLANE("")
            If dxfPlane.IsNull(aPlane) Then aPl = New TPLANE("") Else aPl = New TPLANE(aPlane)
            Dim aOrigin As TVECTOR
            If bMoveTo Then aOrigin = aPl.Origin Else aOrigin = _Struc.Origin
            _rVal = Define(aOrigin, aPl.XDirection, aPl.YDirection)
            Return _rVal
        End Function
        Public Function AlignXTo(aDirectionObj As iVector) As Boolean
            '#1 the direction to align the X axis to
            '^aligns the systems X axis to the passed direction
            If aDirectionObj Is Nothing Then Return False

            Return AlignXToV(New TVECTOR(aDirectionObj))

        End Function
        Friend Function AlignXToV(aDirection As TVECTOR) As Boolean
            '#1 the direction to align the X axis to
            '^aligns the systems X axis to the passed direction
            Dim newStruc As TPLANE = _Struc.AlignedTo(aDirection, dxxAxisDescriptors.X)
            Return Define(newStruc.Origin, newStruc.XDirection, newStruc.YDirection)
        End Function
        Public Function AlignYTo(aDirectionObj As iVector) As Boolean

            '#1 the direction to align the Y axis to
            '^aligns the systems Y axis to the passed direction
            If aDirectionObj Is Nothing Then Return False

            Return AlignYToV(New TVECTOR(aDirectionObj))

        End Function

        Public Function AlignYTo(aDirection As dxfDirection) As Boolean

            '#1 the direction to align the Y axis to
            '^aligns the systems Y axis to the passed direction
            If aDirection Is Nothing Then Return False

            Return AlignYToV(New TVECTOR(aDirection))

        End Function
        Friend Function AlignYToV(aDirection As TVECTOR) As Boolean
            '#1 the direction to align the Y axis to
            '^aligns the systems Y axis to the passed direction
            Dim newStruc As TPLANE = _Struc.AlignedTo(aDirection, dxxAxisDescriptors.Y)
            Return Define(newStruc.Origin, newStruc.XDirection, newStruc.YDirection)
        End Function
        Public Function AlignZTo(aDirectionObj As iVector) As Boolean

            '#1 the direction to align the Z axis to
            '^aligns the systems Z axis to the passed direction
            If aDirectionObj Is Nothing Then Return False

            Return AlignZToV(New TVECTOR(aDirectionObj))
        End Function

        Public Function AlignZTo(aDirection As dxfDirection) As Boolean

            '#1 the direction to align the Z axis to
            '^aligns the systems Z axis to the passed direction
            If aDirection Is Nothing Then Return False

            Return AlignZToV(New TVECTOR(aDirection))
        End Function
        Friend Function AlignZToV(aDirection As TVECTOR) As Boolean
            '#1 the direction to align the Z axis to
            '^aligns the systems Z axis to the passed direction
            Dim newStruc As TPLANE = _Struc.AlignedTo(aDirection, dxxAxisDescriptors.Z)
            Return Define(newStruc.Origin, newStruc.XDirection, newStruc.YDirection)
        End Function
        Public Function AngularDirection(aAngle As Double, Optional bInRadians As Boolean = False) As dxfDirection
            '^returns the direction indicated by the passed angle direction on the indicated plane
            Return New dxfDirection With {.Strukture = AngularDirectionV(aAngle, bInRadians)}
        End Function
        Friend Function AngularDirectionV(aAngle As Double, Optional bInRadians As Boolean = False) As TVECTOR
            Dim _rVal As TVECTOR = TVECTOR.Zero
            '^returns the direction indicated by the passed angle direction on the indicated plane
            _rVal = _Struc.XDirection.Clone
            _rVal.RotateAbout(_Struc.ZDirection, aAngle, bInRadians)
            Return _rVal
        End Function

        Public Function AngleTo(aVector As iVector) As Double
            If aVector Is Nothing Then Return 0

            Dim v1 As dxfVector = dxfVector.FromIVector(aVector).WithRespectToPlane(Me)
            If v1.X = 0 And v1.Y = 0 Then Return 0
            If v1.X = 0 Then
                If v1.Y > 0 Then Return 90 Else Return 270
            End If

            If v1.Y = 0 Then

                If v1.X > 0 Then Return 0 Else Return 180
            End If

            Dim ang As Double = Math.Atan(Math.Abs(v1.Y) / Math.Abs(v1.X)) * 180 / Math.PI
            If v1.X > 0 Then
                If v1.Y > 0 Then Return ang Else Return 360 - ang
            Else
                If v1.Y > 0 Then Return 180 - ang Else Return 180 + ang
            End If
        End Function

        Public Function Axis(aAxisDescr As dxxAxisDescriptors, Optional aLength As Double = 1, Optional aStartPt As iVector = Nothing, Optional aColor As dxxColors = dxxColors.Undefined, Optional aXOffset As Double = 0.0, Optional aYOffset As Double = 0.0, Optional aZOffset As Double = 0.0) As dxeLine
            '#1 the primary plane direction to align the returned line with
            '#2the length for the axis
            '#3an optional origin for the axis
            '^returns a line beginning at the planes origin extending in the planes X direction
            '~if the passed length is negative then the axis extends in the opposite direction
            Return _Struc.Axis(aAxisDescr, aLength, aStartPt, aColor, aXOffset, aYOffset, aZOffset)
        End Function
        Public Function ClearDimensions() As Boolean
            Dim _rVal As Boolean
            If _Struc.Height <> 0 Then _rVal = True
            If _Struc.Width <> 0 Then _rVal = True
            _Struc.Height = 0
            _Struc.Width = 0
            _Struc.Descent = 0
            If _rVal Then RaiseChangeEvent(dxxCoordinateSystemEventTypes.Dimensions)
            Return _rVal
        End Function
        Public Function Clone() As dxfPlane
            '^returns a new object with properties matching those of the cloned object
            Return New dxfPlane(_Struc)
        End Function
        Public Function ContainsLine(aLine As dxeLine, Optional aFudgeFactor As Double = 0.001) As Boolean
            Dim _rVal As Boolean
            '#1the line to test
            '#2a fudge factor for the test
            '^returns True if the both the start and end pt of the passed line lies on the plane
            If aLine IsNot Nothing Then
                _rVal = ContainsLineV(New TLINE(aLine), aFudgeFactor)
            End If
            Return _rVal
        End Function
        Friend Function ContainsLineV(aLine As TLINE, Optional aFudgeFactor As Double = 0.001) As Boolean
            '#1the line to test
            '#2a fudge factor for the test
            '^returns True if the both the start and end pt of the passed line lies on the plane
            Return _Struc.Contains(aLine, aFudgeFactor)
        End Function
        Public Function ContainsPoint(aVectorXY As iVector, Optional aFudgeFactor As Double = 0.001, Optional aRecHeight As Double = 0.0, Optional aRecWidth As Double = 0.0) As Boolean
            Dim rDistance As Double = 0.0
            Return ContainsPoint(aVectorXY, rDistance, aFudgeFactor, aRecHeight, aRecWidth)
        End Function
        Public Function ContainsPoint(aVectorXY As iVector, ByRef rDistance As Double, Optional aFudgeFactor As Double = 0.001, Optional aRecHeight As Double = 0.0, Optional aRecWidth As Double = 0.0) As Boolean
            '#1the vector to test
            '#2returns the distance from the vector to the plane
            '#3a number from 0.1 to  0.000001 which is used to determine if the vector is on the plane
            '#4an optional rectangular height to test
            '#5an optional rectangular width to test
            '^returns true if the perpendicular distance from the vector to the plane is less than the fudge factor
            '~if the rectangular height and width are passed then False is returned if the vector is on the plane but falls outside
            '~of a rectangle centered at the planes origin and aligned with the planes directions of the passed height and width
            If aVectorXY Is Nothing Then Return False
            Return ContainsVector(New TVECTOR(aVectorXY), rDistance, aFudgeFactor, aRecHeight, aRecWidth)
        End Function
        Public Function ContainsVector(aVector As dxfVector, Optional aFudgeFactor As Double = 0.001, Optional aRecHeight As Double = 0.0, Optional aRecWidth As Double = 0.0) As Boolean
            Dim rDistance As Double = 0.0
            Return ContainsVector(aVector, rDistance, aFudgeFactor, aRecHeight, aRecWidth)
        End Function
        Public Function ContainsVector(aVector As dxfVector, ByRef rDistance As Double, Optional aFudgeFactor As Double = 0.001, Optional aRecHeight As Double = 0.0, Optional aRecWidth As Double = 0.0) As Boolean
            Dim _rVal As Boolean
            '#1the vector to test
            '#2returns the distance from the vector to the plane
            '#3a number from 0.1 to  0.000001 which is used to determine if the vector is on the plane
            '#4an optional rectangular height to test
            '#5an optional rectangular width to test
            '^returns true if the perpendicular distance from the vector to the plane is less than the fudge factor
            '~if the rectangular height and width are passed then False is returned if the vector is on the plane but falls outside
            '~of a rectangle centered at the planes origin and aligned with the planes directions of the passed height and width
            If aVector Is Nothing Then Return _rVal
            _rVal = ContainsVector(aVector.Strukture, rDistance, aFudgeFactor, aRecHeight, aRecWidth)
            Return _rVal
        End Function
        Friend Function ContainsVector(aVector As TVECTOR, Optional aFudgeFactor As Double = 0.001, Optional aRecHeight As Double = 0.0, Optional aRecWidth As Double = 0.0) As Boolean
            Dim rDistance As Double = 0.0
            Return ContainsVector(aVector, rDistance, aFudgeFactor, aRecHeight, aRecWidth)
        End Function
        Friend Function ContainsVector(aVector As TVECTOR, ByRef rDistance As Double, Optional aFudgeFactor As Double = 0.001, Optional aRecHeight As Double = 0.0, Optional aRecWidth As Double = 0.0) As Boolean
            '#1the vector to test
            '#2returns the distance from the vector to the plane
            '#3a number from 0.1 to  0.000001 which is used to determine if the vector is on the plane
            '#4an optional rectangular height to test
            '#5an optional rectangular width to test
            '^returns true if the perpendicular distance from the vector to the plane is less than the fudge factor
            '~if the rectangular height and width are passed then False is returned if the vector is on the plane but falls outside
            '~of a rectangle centered at the planes origin and aligned with the planes directions of the passed height and width
            Return Strukture.ContainsVector(aVector, rDistance, aFudgeFactor, aRecHeight, aRecWidth)
        End Function
        Friend Function ConvertVectorToV(aVector As TVECTOR, Optional aFromPlane As dxfPlane = Nothing, Optional aScaleFactor As Double = 0.0) As TVECTOR
            Dim _rVal As TVECTOR = TVECTOR.Zero
            _rVal = aVector
            If aFromPlane IsNot Nothing Then
                aScaleFactor = Math.Round(Math.Abs(aScaleFactor), 6)
                If aScaleFactor = 0 Then aScaleFactor = 1
                _rVal = aVector.TransferedToPlane(aFromPlane.Strukture, _Struc, aScaleFactor, aScaleFactor, aScaleFactor, 0)
            End If
            Return _rVal
        End Function
        Public Sub Cooefficents(ByRef rA As Double, ByRef rB As Double, ByRef rC As Double, ByRef rD As Double)
            '^returns the coeeficients of the planes equation
            '~Ax + By + Cz + D = 0
            Dim n As dxfDirection = Normal
            rA = n.X
            rB = n.Y
            rC = n.Z
            rD = -rA * Origin.X - rB * Origin.Y - rC * Origin.Z
        End Sub
        Public Function CopyTo(aCenter As iVector, Optional aRotation As Double = 0.0) As dxfPlane
            Dim _rVal As dxfPlane
            '^returns a new object with properties matching those of the cloned object
            _rVal = Clone()
            If aCenter IsNot Nothing Then _rVal.OriginV = New TVECTOR(aCenter)
            If aRotation <> 0 Then _rVal.Rotate(aRotation, bInRadians:=False)
            Return _rVal
        End Function
        Public Function CreateArc(aX As Double, aY As Double, aRadius As Double, Optional aStartAngle As Double = 0, Optional aEndAngle As Double = 360, Optional aZ As Double = 0, Optional aRotation As Double? = Nothing, Optional aLayerName As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aTag As String = Nothing, Optional aFlag As String = Nothing) As dxeArc
            Dim _rVal As New dxeArc
            _rVal.SetArcLineStructure(_Struc.CreateArc(aX, aY, aRadius, aStartAngle, aEndAngle, aZ, aRotation))
            _rVal.LCLSet(aLayerName, aColor, aLineType)
            If aTag IsNot Nothing Then _rVal.Tag = aTag
            If aFlag IsNot Nothing Then _rVal.Flag = aFlag
            Return _rVal
        End Function
        Public Function CreateCircle(aX As Double, aY As Double, aRadius As Double, Optional aZ As Double = 0, Optional aRotation As Double? = Nothing, Optional aLayerName As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aTag As String = Nothing, Optional aFlag As String = Nothing) As dxeArc
            Dim _rVal As New dxeArc(_Struc.CreateArc(aX, aY, aRadius, 0, 360, aZ, aRotation))
            _rVal.LCLSet(aLayerName, aColor, aLineType)
            _rVal.TFVSet(aTag, aFlag)
            Return _rVal
        End Function
        Public Function CreateLine(Optional aSPX As Double = 0, Optional aSPY As Double = 0, Optional aEPX As Double = 0, Optional aEPY As Double = 0, Optional aZ As Double = 0, Optional aLayerName As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aTag As String = Nothing, Optional aFlag As String = Nothing) As dxeLine
            Dim _rVal As New dxeLine With {.StartPtV = Strukture.Vector(aSPX, aSPY, aZ), .EndPtV = Strukture.Vector(aEPX, aEPY, aZ)}
            '#1the X coordinate for the start pt of the new line
            '#2the Y coordinate for the start pt of the new line
            '#3the X coordinate for the end pt of the new line
            '#4the Y coordinate for the end pt of the new line
            '#5the elevation value for the line
            '^used to create a new line with respect to the origin of the system
            _rVal.LCLSet(aLayerName, aColor, aLineType)
            If aTag IsNot Nothing Then _rVal.Tag = aTag
            If aFlag IsNot Nothing Then _rVal.Flag = aFlag
            Return _rVal
        End Function
        Public Function CreateParallelLines(aCenter As dxfVector, aGap As Double, aLength As Double, aAngle As Double, ByRef rLine1 As dxeLine, ByRef rLine2 As dxeLine) As List(Of dxeLine)
            Dim _rVal As New List(Of dxeLine)
            Dim v1 As TVECTOR
            Dim mpt As TVECTOR
            Dim aDir As TVECTOR = _Struc.XDirection.Clone
            Dim bDir As TVECTOR
            Dim sp As TVECTOR
            Dim ep As TVECTOR
            If aAngle <> 0 Then aDir.RotateAbout(_Struc.ZDirection, aAngle)
            If aCenter Is Nothing Then v1 = _Struc.Origin Else v1 = aCenter.Strukture
            bDir = aDir.RotatedAbout(_Struc.ZDirection, 90).Normalized
            mpt = v1 + bDir * (0.5 * aGap)
            sp = mpt + aDir * (0.5 * aLength)
            ep = mpt + aDir * (-0.5 * aLength)
            rLine1 = New dxeLine(sp, ep)
            mpt = v1 + bDir * (-0.5 * aGap)
            sp = mpt + aDir * (0.5 * aLength)
            ep = mpt + aDir * (-0.5 * aLength)
            rLine2 = New dxeLine(sp, ep)
            _rVal.Add(rLine1)
            _rVal.Add(rLine2)
            Return _rVal
        End Function
        Public Function CreatePolarLine(aLength As Double, aAngle As Double, Optional bInRadians As Boolean = False, Optional aElevation As Double = 0.0, Optional aMidPt As dxfVector = Nothing, Optional bFromEndPt As Boolean = False) As dxeLine
            Dim _rVal As dxeLine = Nothing
            '#1the length of the line
            '#2the angle for the line
            '#3flag indicating that the angle is degrees or radians
            '#4the Z coordinate for the new line
            '#5the mid point of the line
            '#6flag to begin then line at the midpoint and project the end the desired angle
            '^used to create a new polar line with respect to the passed origin
            '~the returned lines mid point is at the passed origin and is oriented at the passed angle with respect to the passed x direction
            _rVal = New dxeLine
            If aMidPt Is Nothing Then
                _rVal.LineStructure = TLINE.PolarLine(_Struc.XDirection, ZDirectionV, aLength, aAngle, bInRadians, aElevation, _Struc.Origin, bFromEndPt)
            Else
                _rVal.LineStructure = TLINE.PolarLine(_Struc.XDirection, ZDirectionV, aLength, aAngle, bInRadians, aElevation, aMidPt.Strukture, bFromEndPt)
            End If
            Return _rVal
        End Function
        Friend Function CreatePolarLineV(aLength As Double, aAngle As Double, bInRadians As Boolean, aElevation As Double, aMidPt As TVECTOR, Optional bFromEndPt As Boolean = False) As TLINE
            Dim _rVal As New TLINE
            '#1the length of the line
            '#2the angle for the line
            '#3flag indicating that the angle is degrees or radians
            '#4the Z coordinate for the new line
            '#5the mid point of the line
            '#6flag to begin then line at the midpoint and project the end the desired angle
            '^used to create a new polar line with respect to the passed origin
            '~the returned lines mid point is at the passed origin and is oriented at the passed angle with respec to the passed x direction
            _rVal = TLINE.PolarLine(_Struc.XDirection, ZDirectionV, aLength, aAngle, bInRadians, aElevation, aMidPt, bFromEndPt)
            Return _rVal
        End Function
        Public Function CreateVectors(aVectors As IEnumerable(Of iVector), Optional aOrigin As dxfVector = Nothing, Optional aElevation As Double? = Nothing) As colDXFVectors
            '#1the vector or vectors objects (anything with XYZ Properties)
            '^used to create a collection of dxoVectors based on the passed collection or single xyz object(s)
            '~clones are returned if real vectors are passed
            Return _Struc.CreateVectors(aVectors, aOrigin, aElevation)
        End Function
        Friend Function Define(aOrigin As TVECTOR, aXDir As TVECTOR, aYDir As TVECTOR, Optional bSuppressEvnts As Boolean = False, Optional aHeight As Double? = Nothing, Optional aWidth As Double? = Nothing) As Boolean
            Dim rOriginChange As Boolean = False
            Dim rDirectionChange As Boolean = False
            Dim rDimChange As Boolean = False
            Return Define(aOrigin, aXDir, aYDir, rOriginChange, rDirectionChange, rDimChange, bSuppressEvnts, aHeight, aWidth)
        End Function
        Friend Function Define(aOrigin As TVECTOR, aXDir As TVECTOR, aYDir As TVECTOR, ByRef rOriginChange As Boolean, ByRef rOrientationChange As Boolean, ByRef rDimensionChange As Boolean, Optional bSuppressEvnts As Boolean = False, Optional aHeight As Double? = Nothing, Optional aWidth As Double? = Nothing) As Boolean
            Dim _rVal As Boolean

            Dim oldStruc As New TPLANE(_Struc)
            Dim aType As dxxCoordinateSystemEventTypes
            Dim iGUID As String = _Struc.ImageGUID
            _Struc = oldStruc.ReDefined(aOrigin, aXDir, aYDir, rOriginChange, rOrientationChange, rDimensionChange, _rVal, aHeight, aWidth)
            _Struc.ImageGUID = iGUID
            If _rVal Then
                If Not bSuppressEvnts Then
                    If rOriginChange Then aType = dxxCoordinateSystemEventTypes.Origin
                    If rOrientationChange Then aType += dxxCoordinateSystemEventTypes.Orientation
                    If rDimensionChange Then aType += dxxCoordinateSystemEventTypes.Dimensions
                    RaiseChangeEvent(aType)
                End If
                If Not String.IsNullOrWhiteSpace(_Struc.ImageGUID) And rOriginChange Or rOrientationChange Then
                    Dim aImage As dxfImage = dxfEvents.GetImage(_Struc.ImageGUID)
                    If aImage Is Nothing Then
                        _Struc.ImageGUID = ""
                    Else
                        aImage.obj_UCS = _Struc
                        If rOriginChange Or rOrientationChange Then
                            Dim aUCS As New dxoUCS(_Struc, bIsGlobal:=True)
                            Dim aProp As TPROPERTY
                            If rOriginChange Then
                                aProp = New TPROPERTY(2, _Struc.Origin.Coordinates(0), "Origin", dxxPropertyTypes.dxf_String, oldStruc.Origin.Coordinates(0))
                                aImage.RespondToTableMemberEvent(aUCS, False, aProp)
                            End If
                            If rOrientationChange Then
                                aProp = New TPROPERTY(2, _Struc.Directions, "Orientation", dxxPropertyTypes.dxf_String, oldStruc.Directions)
                                aImage.RespondToTableMemberEvent(aUCS, False, aProp)
                            End If
                        End If
                    End If
                End If
            End If
            Return _rVal
        End Function
        Public Function DefineWithLines(aLine As dxeLine, bLine As dxeLine) As Boolean
            Dim _rVal As Boolean
            '#1the line to use to get the x direction from
            '#2the line to use to get the y direction from
            '^defines the primary directions based on the passed directions
            '~if the two directions are parallel or either is null nothing is done.
            '~if the y direction is not perpendicular to the x the orthogal component
            '~of the y with respect to the x is used for the final y direction.
            If aLine Is Nothing Or bLine IsNot Nothing Then Return _rVal
            Dim aPl As New TPLANE("")
            Dim bFlag As Boolean
            aPl = TPLANE.DefineXY(aLine.DirectionV, bLine.DirectionV, bFlag, _Struc.Origin)
            If bFlag Then Return _rVal 'undefinable
            _rVal = Define(aPl.Origin, aPl.XDirection, aPl.YDirection)
            Return _rVal
        End Function
        Public Function DefineXY(aXDirObj As iVector, aYDirObj As iVector, Optional aCenter As iVector = Nothing) As Boolean
            Dim _rVal As Boolean
            '#1the object to use to get the x direction from
            '#2the object to use to get the y direction from
            '^defines the primary directions based on the passed directions
            '~if the two directions are parallel or either is null nothing is done.
            '~if the y direction is not perpendicular to the x the orthogal component
            '~of the y with respect to the x is used for the final y direction.
            If aXDirObj Is Nothing Or aYDirObj Is Nothing Then Return _rVal
            Dim aPl As New TPLANE("")
            Dim bFlag As Boolean
            aPl = TPLANE.DefineXY(New TVECTOR(aXDirObj), New TVECTOR(aYDirObj), bFlag, _Struc.Origin)
            If bFlag Then Return _rVal 'undefinable
            If aCenter IsNot Nothing Then aPl.Origin = New TVECTOR(aCenter)
            _rVal = Define(aPl.Origin, aPl.XDirection, aPl.YDirection)
            Return _rVal
        End Function
        Public Function Dimensions(Optional aUnits As dxxDeviceUnits = dxxDeviceUnits.Undefined) As String
            Dim _rVal As String = String.Empty
            Dim aSF As Double
            If aUnits <= 0 Then
                aUnits = _Struc.Units
                aSF = 1
            Else
                aSF = dxfUtils.FactorFromTo(_Struc.Units, aUnits)
            End If
            If aUnits = dxxDeviceUnits.Pixels Or aUnits = dxxDeviceUnits.Characters Then
                _rVal = TVALUES.To_INT(_Struc.Width * aSF) & "W x " & TVALUES.To_INT(_Struc.Height * aSF) & "H " & dxfEnums.Description(aUnits)
            ElseIf aUnits = dxxDeviceUnits.Points Or aUnits = dxxDeviceUnits.Centimeters Or aUnits = dxxDeviceUnits.Millimeters Then
                _rVal = Format(_Struc.Width * aSF, "0.0") & "W x " & Format(_Struc.Height * aSF, "0.0") & "H " & dxfEnums.Description(aUnits)
            Else
                _rVal = Format(_Struc.Width * aSF, "0.00") & "W x " & Format(_Struc.Height * aSF, "0.00") & "H " & dxfEnums.Description(aUnits)
            End If
            Return _rVal
        End Function
        Public Function Direction(aAngle As Double, Optional bInRadians As Boolean = False) As dxfDirection
            '#1the angle used to orient the returned direction
            '#2flag indicating that the passed angle is in radians
            '^returns a direction vector that lies on the plane and is oriented at the passed
            '^angle with respect to the current x direction
            Return New dxfDirection With {.Strukture = _Struc.Direction(aAngle, bInRadians)}
        End Function
        Friend Function DirectionV(aAngle As Double, Optional bInRadians As Boolean = False) As TVECTOR
            '#1the angle used to orient the returned direction
            '#2flag indicating that the passed angle is in radians
            '^returns a direction vector that lies on the plane and is oriented at the passed
            '^angle with respect to the current x direction
            Return _Struc.Direction(aAngle, bInRadians)
        End Function
        Public Sub GetDimensions(ByRef rWidth As Double, ByRef rHeight As Double, Optional aMultiplier As Double = 1)
            '#1returns the width
            '#2returns the height
            '^returns the dimensions of the rectangle
            _Struc.GetDimensions(rWidth, rHeight, aMultiplier)
        End Sub
        Public Function HorizontalLine(aYDim As Double, Optional aLength As Double = 1, Optional aRotation As Double = 0.0, Optional bCenterOnOrigin As Boolean = False, Optional aDisplaySettings As dxfDisplaySettings = Nothing) As dxeLine
            Return New dxeLine(_Struc.HorizontalLine(aYDim, aLength, aRotation, bCenterOnOrigin)) With {.PlaneV = _Struc, .DisplaySettings = aDisplaySettings}
        End Function
        Public Function IsEqual(aPlane As dxfPlane, Optional aPrecision As Integer = 1, Optional bCompareOrigin As Boolean = False, Optional bCompareRotation As Boolean = False) As Boolean
            If dxfPlane.IsNull(aPlane) Then Return False
            Dim rIsOrthogonal As Boolean = False
            Dim rNormAngle As Double = 0.0
            '#1the plane to test
            '#2the precision for numerical equallity comparison
            '#3flag to compare the origin
            Return IsEqual(aPlane, aPrecision, bCompareOrigin, bCompareRotation, rIsOrthogonal, rNormAngle)
        End Function
        Public Function IsEqual(aPlane As dxfPlane, aPrecision As Integer, bCompareOrigin As Boolean, bCompareRotation As Boolean, ByRef rIsOrthogonal As Boolean, ByRef rNormAngle As Double) As Boolean
            If dxfPlane.IsNull(aPlane) Then Return False
            Dim diffs As String = String.Empty
            '#1the plane to test
            '#2the precision for numerical equallity comparison
            '#3flag to compare the origin
            Return TPLANES.Compare(_Struc, aPlane.Strukture, aPrecision, bCompareOrigin, bCompareRotation, rIsOrthogonal, rNormAngle, diffs)
        End Function
        Public Function IsOrthogonalTo(aPlane As dxfPlane, Optional aNormAngle As Double = 0.0, Optional aPrecision As Integer = 3) As Boolean
            '#1the plane to test this one against
            '#2returns the angle between this plane's normal and the passed one
            '#3the precision to round the angle to for the comparison to 90
            '^returns True if the normal of this plane is orthogonal to the normal of the passed one
            aNormAngle = 0
            If Not dxfPlane.IsNull(aPlane) Then Return _Struc.IsOrthogonalTo(aPlane.Strukture, aNormAngle, aPrecision) Else Return False
        End Function
        Public Function LineIntersection(aLine As dxeLine) As dxfVector
            Dim rIntersectIsOnLine As Boolean = False
            Return LineIntersection(aLine, rIntersectIsOnLine)
        End Function
        Public Function LineIntersection(aLine As dxeLine, ByRef rIntersectIsOnLine As Boolean) As dxfVector
            '#1the line to test
            '#returns true if the intsection point lines on the passed line
            '^returns the intersection of this plane and the passed line
            rIntersectIsOnLine = False
            Dim rCoPlanar As Boolean = False
            If aLine Is Nothing Then Return Nothing
            Return LineIntersectionV(New TLINE(aLine), rIntersectIsOnLine, rCoPlanar)
        End Function
        Friend Function LineIntersectionV(aLine As TLINE) As dxfVector
            Dim rIntersectIsOnLine As Boolean = False
            Dim rCoplanar As Boolean = False
            Return LineIntersectionV(aLine, rIntersectIsOnLine, rCoplanar)
        End Function
        Friend Function LineIntersectionV(aLine As TLINE, ByRef rIntersectIsOnLine As Boolean, ByRef rCoplanar As Boolean) As dxfVector
            Dim _rVal As dxfVector = Nothing
            '#1the line to test
            '#2returns true if the intersection point lines on the passed line
            '#3returns true if the passed line lies on the passed plane
            '^returns the intersection of the plane and the passed line
            Dim v1 As TVECTOR
            v1 = dxfIntersections.LinePlane(aLine, _Struc, rIntersectIsOnLine, rCoplanar)
            If Not rCoplanar Or (rCoplanar And rIntersectIsOnLine) Then
                _rVal = New dxfVector With {.Strukture = v1}
            End If
            Return _rVal
        End Function

        Public Function Mirrored(aMirrorAxis As iLine, Optional bMirrorOrigin As Boolean = False, Optional bMirrorDirections As Boolean = True) As dxfPlane
            Dim _rVal As New dxfPlane(Me)
            '#1the line to mirror across
            '#2flag to mirror the origin
            '#3flag to mirror the directions
            '^mirrors the plane across the passed line
            '~returns a clone of the plane mirrored about the passed line
            If aMirrorAxis Is Nothing Then Return _rVal
            _rVal.Mirror(New TLINE(aMirrorAxis), bMirrorOrigin, bMirrorDirections, False)
            Return _rVal
        End Function

        Public Function Mirror(aMirrorAxis As iLine, Optional bMirrorOrigin As Boolean = False, Optional bMirrorDirections As Boolean = True) As Boolean
            Dim _rVal As Boolean
            '#1the line to mirror across
            '#2flag to mirror the origin
            '#3flag to mirror the directions
            '^mirrors the plane across the passed line
            '~returns True if the plane actually moves from this process
            If aMirrorAxis Is Nothing Then Return _rVal
            _rVal = Mirror(New TVECTOR(aMirrorAxis.StartPt), New TVECTOR(aMirrorAxis.EndPt), bMirrorOrigin, bMirrorDirections, False)
            Return _rVal
        End Function
        Friend Function Mirror(aMirrorAxis As TLINE, Optional bMirrorOrigin As Boolean = False, Optional bMirrorDirections As Boolean = True, Optional bSuppressEvnts As Boolean = True) As Boolean
            Dim _rVal As Boolean
            '#1the start pt of the line to mirror across
            '#2the end pt of the line to mirror across
            '#3flag to mirror the origin
            '#4flag to mirror the directions
            '^mirrors the system across the passed line
            '~returns True if the system actually moves from this process
            Dim newStruc As New TPLANE("")
            newStruc = _Struc.Mirrored(aMirrorAxis.SPT, aMirrorAxis.EPT, bMirrorOrigin, bMirrorDirections, False, _rVal)
            If _rVal Then Define(newStruc.Origin, newStruc.XDirection, newStruc.YDirection, bSuppressEvnts:=bSuppressEvnts)
            Return _rVal
        End Function

        Friend Function Mirror(aSP As TVECTOR, aEP As TVECTOR, Optional bMirrorOrigin As Boolean = False, Optional bMirrorDirections As Boolean = True, Optional bSuppressEvnts As Boolean = True) As Boolean
            Dim _rVal As Boolean
            '#1the start pt of the line to mirror across
            '#2the end pt of the line to mirror across
            '#3flag to mirror the origin
            '#4flag to mirror the directions
            '^mirrors the system across the passed line
            '~returns True if the system actually moves from this process
            Dim newStruc As New TPLANE("")
            newStruc = _Struc.Mirrored(aSP, aEP, bMirrorOrigin, bMirrorDirections, False, _rVal)
            If _rVal Then Define(newStruc.Origin, newStruc.XDirection, newStruc.YDirection, bSuppressEvnts:=bSuppressEvnts)
            Return _rVal
        End Function
        Public Function Move(Optional ChangeX As Double = 0.0, Optional ChangeY As Double = 0.0, Optional ChangeZ As Double = 0.0, Optional aPlane As dxfPlane = Nothing) As Boolean
            Dim _rVal As Boolean
            '#1the X displacement
            '#2the Y displacement
            '#3the Z displacement
            '#4a coordinate system to get the X,Y and Z directions from
            '^used to change the coordinates of the entity by translation
            '~if the coordinate system is nothing then the displacement is added to the current coordinates
            '~otherwise the displacement is applied with respect to the systems X, Y and Z directions
            _rVal = Translate(New TVECTOR(ChangeX, ChangeY, ChangeZ), False, aPlane)
            Return _rVal
        End Function
        Friend Function MoveFromToV(BasePoint As TVECTOR, DestinationPoint As TVECTOR) As Boolean
            '^used to move the object from one reference vector to another
            Return Translate(DestinationPoint - BasePoint)
        End Function
        Public Function MoveTo(aDestinationXY As iVector, Optional aChangeX As Double = 0.0, Optional aChangeY As Double = 0.0, Optional aChangeZ As Double = 0.0) As Boolean
            '#1the object with X, Y , Z properties to move to
            '#2the distance to change the vector's X value after it is moved to the destination
            '#3the distance to change the vector's Y value after it is moved to the destination
            '#4the distance to change the vector's Z value after it is moved to the destination
            '^used to move an existing vector to the coordinates of another Object
            Dim v1 As New TVECTOR(aDestinationXY)
            v1.X += aChangeX
            v1.Y += aChangeY
            v1.Z += aChangeZ

            Return Translate(v1 - _Struc.Origin)
        End Function
        Public Function MoveToVector(aDestinationPoint As dxfVector, Optional aChangeX As Double = 0.0, Optional aChangeY As Double = 0.0, Optional aChangeZ As Double = 0.0) As Boolean
            Dim _rVal As Boolean
            '#1the destination point
            '#2a x displacement to apply after the move
            '#3a y displacement to apply after the move
            '#4a z displacement to apply after the move
            '^moves the entity from its current insertion point to the passed point
            '~returns True if the entity actually moves from this process
            Dim aDest As TVECTOR
            If aDestinationPoint IsNot Nothing Then aDest = aDestinationPoint.Strukture
            _rVal = Translate((aDest + New TVECTOR(aChangeX, aChangeY, aChangeZ)) - _Struc.Origin)
            Return _rVal
        End Function
        Public Function Translate(aTranslationX As Double, aTranslationY As Double, Optional aTranslationZ As Double = 0, Optional aPlane As dxfPlane = Nothing) As Boolean
            Return Translate(New TVECTOR(aTranslationX, aTranslationY, aTranslationZ), False, aPlane)
        End Function
        Public Function Translate(aTranslation As iVector, Optional aPlane As dxfPlane = Nothing) As Boolean
            Return Translate(New TVECTOR(aTranslation), False, aPlane)
        End Function
        Friend Function Translate(aTranslation As TVECTOR, Optional bSuppressEvnts As Boolean = False, Optional aPlane As dxfPlane = Nothing) As Boolean
            If TVECTOR.IsNull(aTranslation) Then Return False
            '#1the displacement to apply
            '#2flag to suppress the change event
            '#3a coordinate system to get the X,Y and Z directions from
            '^used to change the coordinates of the entity by translation
            '~if the coordinate system is nothing then the displacement is added to the current coordinates
            '~otherwise the displacement is applied with respect to the systems X, Y and Z directions
            Dim aOrigin As TVECTOR = _Struc.Origin
            If dxfPlane.IsNull(aPlane) Then aOrigin += aTranslation Else aOrigin = New TPLANE(aPlane).VectorRelative(aOrigin, aTranslation.X, aTranslation.Y, aTranslation.Z)
            Return Define(aOrigin, _Struc.XDirection, _Struc.YDirection, bSuppressEvnts)
        End Function
        Public Function Project(aDirectionObj As iVector, aDistance As Double) As Boolean

            If aDirectionObj Is Nothing Or aDistance = 0 Then Return False
            '#1the direction to project in
            '#2the distance to project
            '^projects the origin in the passed direction the requested distance
            Dim aDirection As New dxfDirection(aDirectionObj)
            If aDirection Is Nothing Then Return False
            Return Define(_Struc.Origin + aDirection.Strukture * aDistance, _Struc.XDirection, _Struc.YDirection)
        End Function

        Public Function Project(aDirection As dxfDirection, aDistance As Double) As Boolean

            If aDirection Is Nothing Or aDistance = 0 Then Return False
            '#1the direction to project in
            '#2the distance to project
            '^projects the origin in the passed direction the requested distance

            Return Define(_Struc.Origin + aDirection.Strukture * aDistance, _Struc.XDirection, _Struc.YDirection)
        End Function
        Public Function Rectangle(Optional aCenter As dxfVector = Nothing) As dxfRectangle
            Dim _rVal As dxfRectangle = Nothing
            '^returns a rectangle defined on the plane
            _rVal = New dxfRectangle(_Struc)
            If aCenter IsNot Nothing Then _rVal.CenterV = aCenter.Strukture
            Return _rVal
        End Function
        Public Function Reset(Optional aOriginObj As iVector = Nothing, Optional bResetUnits As Boolean = False) As Boolean
            If bResetUnits Then _Struc.Units = dxxDeviceUnits.Pixels
            Return Define(New TVECTOR(aOriginObj), TVECTOR.WorldX, TVECTOR.WorldY)
        End Function
        Public Function ResetDirections() As Boolean
            '^resets the planes x, y and z directions back to the global x, y and z directions
            Dim v1 As TVECTOR = TVECTOR.WorldX
            Dim v2 As TVECTOR = TVECTOR.WorldY
            Dim _rVal As Boolean = Not _Struc.XDirection.Equals(v1, 6) Or Not _Struc.YDirection.Equals(v2, 6)
            If Not _rVal Then Return False
            Return Define(_Struc.Origin, v1, v2)

        End Function
        Public Function ResetOrigin() As Boolean
            Return Define(New TVECTOR(0, 0, 0), _Struc.XDirection, _Struc.YDirection)
        End Function
        Public Function Rotate(aAngle As Double, Optional bInRadians As Boolean = False, Optional bLocal As Boolean = True) As Boolean
            '#1the angle to rotate
            '#2flag indicating if the passed angle is in radians
            '^rotates the plane about its z axis

            If Not bLocal Then
                Dim rAxis As dxeLine = New dxeLine(TPLANES.Axis(New TPLANE("World"), dxxAxisDescriptors.Z, 1, aStartPt:=Origin))
                Return RotateAboutLine(rAxis, aAngle, bInRadians, False, True)

            Else
                Return Revolve(aAngle, bInRadians)
            End If

        End Function
        Public Function Rotate(aAngle As Double, ByRef rAxis As dxeLine, Optional bInRadians As Boolean = False, Optional bLocal As Boolean = True) As Boolean
            '#1the angle to rotate
            '#2flag indicating if the passed angle is in radians
            '#3returns the line used for the rotation axis. The line always starts at the planes origin.
            '4flag to rotate the about the planes z axis (local) or the world z axis direction.  (local is default)
            '^rotates the plane about its z axis
            If bLocal Then
                rAxis = ZAxis()
                Return Revolve(aAngle, bInRadians)
            Else
                rAxis = New dxeLine(TPLANES.Axis(New TPLANE("World"), dxxAxisDescriptors.Z, 1, aStartPt:=Origin))
                Return RotateAboutLine(rAxis, aAngle, bInRadians, False, True)
            End If

        End Function

        Public Function Revolve(aAngle As Double, Optional bInRadians As Boolean = False) As Boolean
            If aAngle = 0 Then Return False
            '#1the angle to rotate
            '#2flag indicating if the passed angle is in radians
            '^used to change the orientation and/or the origin of the system by rotating the it about its z axis
            Return Define(OriginV, XDirectionV.RotatedAbout(ZDirectionV, aAngle, bInRadians, True), YDirectionV.RotatedAbout(ZDirectionV, aAngle, bInRadians, True))
        End Function
        Public Function Revolved(aAngle As Double, Optional bInRadians As Boolean = False) As dxfPlane
            Dim _rVal As New TPLANE(Me)
            _rVal.Revolve(aAngle, bInRadians)
            Return New dxfPlane(_rVal)
            '#1the angle to rotate
            '#2flag indicating if the passed angle is in radians
            '^used to change the orientation and/or the origin of the system by rotating the it about its z axis
        End Function

        Public Function RotateAbout(aAxisObj As iLine, aAngle As Double, Optional bInRadians As Boolean = False, Optional bRotateOrigin As Boolean = False, Optional bRotateDirections As Boolean = True, Optional aAxis As dxeLine = Nothing) As Boolean
            If aAxisObj IsNot Nothing Then Return False

            '#1the line object to rotate about
            '#2the angle to rotate
            '#3flag indicating if the passed angle is in radians
            '#4flag to rotate the origin of the plane about the axis
            '#5flag to rotate the X,Y and X directions about the axis
            '#6returns the line used for the rotation axis
            '#7flag to raise no change events
            '^used to change the orientation and/or the origin of the plane by rotating the it about the passed axis
            Return RotateAboutLine(aAxis, aAngle, bInRadians, bRotateOrigin, bRotateDirections)

        End Function
        Public Function RotateAboutLine(aLine As iLine, aAngle As Double, Optional bInRadians As Boolean = False, Optional bRotateOrigin As Boolean = False, Optional bRotateDirections As Boolean = True) As Boolean

            '#1the line object to rotate about
            '#2the angle to rotate
            '#3flag indicating if the passed angle is in radians
            '#4flag to rotate the origin of the plane about the axis
            '#5flag to rotate the X,Y and X directions about the axis
            '#6returns the line used for the rotation axis
            '#7flag to raise no change events
            '^used to change the orientation and/or the origin of the plane by rotating the it about the passed axis
            If aLine Is Nothing Then Return False
            Dim l1 As New TLINE(aLine)
            If l1.Length <= 0 Then Return False
            Return RotateAboutLine(l1, aAngle, bInRadians, bRotateOrigin, bRotateDirections)

        End Function
        Friend Function RotateAboutLine(aLine As TLINE, aAngle As Double, Optional bInRadians As Boolean = False, Optional bRotateOrigin As Boolean = False, Optional bRotateDirections As Boolean = True) As Boolean
            '#1the line object to rotate about
            '#2the angle to rotate
            '#3flag indicating if the passed angle is in radians
            '#4flag to rotate the origin of the plane about the axis
            '#5flag to rotate the X,Y and X directions about the axis
            '#6returns the line used for the rotation axis
            '#7flag to raise no change events
            '^used to change the orientation and/or the origin of the plane by rotating the it about the passed axis

            If aLine.Length <= 0 Then Return False
            Return RotateAboutV(aLine.SPT, aLine.EPT, aAngle, bInRadians, bRotateOrigin, bRotateDirections, False)
        End Function
        Friend Function RotateAboutV(aOrigin As TVECTOR, aDirection As TVECTOR, aAngle As Double, Optional bInRadians As Boolean = False, Optional bRotateOrigin As Boolean = False, Optional bRotateDirections As Boolean = True, Optional bSuppressEvnts As Boolean = False) As Boolean
            '#1the line object to rotate about
            '#2the angle to rotate
            '#3flag indicating if the passed angle is in radians
            '#4flag to rotate the origin of the system about the axis
            '#5flag to rotate the X,Y and Zdirections about the axis
            '#6returns the line used for the rotation axis
            '#7flag to raise no change events
            '^used to change the orientation and/or the origin of the system by rotating the it about the passed axis
            If Not bRotateOrigin And Not bRotateDirections Then Return False
            aAngle = TVALUES.NormAng(aAngle, bInRadians, True)
            If aAngle = 0 Then Return False
            Dim aStruc As TPLANE = _Struc.RotatedAbout(aOrigin, aDirection, aAngle, bInRadians, bRotateOrigin, bRotateDirections)
            Return Define(aStruc.Origin, aStruc.XDirection, aStruc.YDirection, bSuppressEvnts:=bSuppressEvnts)
        End Function
        Friend Function RotateV(aAngle As Double, Optional bInRadians As Boolean = False, Optional bLocal As Boolean = True, Optional bSuppressEvnts As Boolean = True) As Boolean
            Dim rAxis As dxeLine = Nothing
            Return RotateV(aAngle, bInRadians, rAxis, bLocal, bSuppressEvnts)
        End Function
        Friend Function RotateV(aAngle As Double, bInRadians As Boolean, ByRef rAxis As dxeLine, Optional bLocal As Boolean = True, Optional bSuppressEvnts As Boolean = True) As Boolean
            '#1the angle to rotate
            '#2flag indicating if the passed angle is in radians
            '#3returns the line used for the rotation axis
            '^rotates the plane about its z axis
            If bLocal Then rAxis = ZAxis() Else rAxis = New dxeLine(TPLANES.Axis(New TPLANE("WORLD"), dxxAxisDescriptors.Z, aStartPt:=Origin))
            Return RotateAboutV(rAxis.StartPtV, rAxis.DirectionV, aAngle, bInRadians, False, True, bSuppressEvnts)
        End Function
        Public Sub ScaleUp(aScaleX As Double, Optional aScaleY As Double = 0.0, Optional aReference As dxfVector = Nothing)
            If aReference Is Nothing Then ScaleUpV(aScaleX, aScaleY, _Struc.Origin) Else ScaleUpV(aScaleX, aScaleY, aReference.Strukture)
        End Sub
        Friend Function ScaleUpV(aScaleX As Double, aScaleY As Double, aReference As TVECTOR, Optional bSuppressEvnts As Boolean = True, Optional aScaleZ As Double = 0.0) As Boolean
            Dim _rVal As Boolean
            If aScaleX <= 0 Then aScaleX = 1
            If aScaleY <= 0 Then aScaleY = aScaleX
            If aScaleZ <= 0 Then aScaleZ = 1
            If aScaleX = 1 And aScaleY = 1 And aScaleZ = 1 Then Return _rVal
            If (_Struc.Width <> 0 Or _Struc.Width <> 0 Or _Struc.Height <> 0 Or _Struc.Descent <> 0) Then _rVal = True
            _Struc.Width = aScaleX * _Struc.Width
            _Struc.Height = aScaleY * _Struc.Height
            If _Struc.Descent <> 0 Then
                _Struc.Descent = aScaleY * _Struc.Descent
            End If
            Dim aOrigin As TVECTOR = _Struc.Origin.Scaled(aScaleX, aReference, aScaleY, aScaleZ)
            If Define(aOrigin, _Struc.XDirection, _Struc.YDirection, bSuppressEvnts:=bSuppressEvnts) Then _rVal = True
            Return _rVal
        End Function
        Public Function SetCoordinates(Optional NewX As Double? = Nothing, Optional NewY As Double? = Nothing, Optional NewZ As Double? = Nothing) As Boolean
            '#1the value to set the X coordinate to
            '#2the value to set the Y coordinate to
            '#3the value to set the Z coordinate to
            '^moves the planes with the passed coordinates at its center
            '~unpassed or non-numeric values are ignored and the current ordinates are used
            Dim v1 As TVECTOR = _Struc.Origin
            If v1.Update(NewX, NewY, NewZ) Then Return Define(v1, _Struc.XDirection, _Struc.YDirection) Else Return False
        End Function
        Public Function SetDimensions(Optional aWidth As Double? = Nothing, Optional aHeight As Double? = Nothing, Optional aMulitplier As Double = 1) As Boolean
            Dim _rVal As Boolean
            '#1the new width
            '#2the new height
            '^sets the dimensions of the rectangle and returns True if it changes
            _rVal = _Struc.SetDimensions(aWidth, aHeight)
            If _rVal Then RaiseChangeEvent(dxxCoordinateSystemEventTypes.Dimensions)
            Return _rVal
        End Function
        Friend Function SetOrigin(aVector As TVECTOR, Optional bSuppressEvnts As Boolean = True) As Boolean
            Return Define(aVector, _Struc.XDirection, _Struc.YDirection, bSuppressEvnts:=bSuppressEvnts)
        End Function
        Public Function Spin(aAngle As Double, Optional bInRadians As Boolean = False, Optional bLocal As Boolean = True) As Boolean
            Dim rAxis As dxeLine = Nothing
            Return Spin(aAngle, rAxis, bInRadians, bLocal)
        End Function
        Public Function Spin(aAngle As Double, ByRef rAxis As dxeLine, Optional bInRadians As Boolean = False, Optional bLocal As Boolean = True) As Boolean
            '#1the angle to tip
            '#2flag indicating if the passed angle is in radians
            '^rotates the plane about its y axis
            If bLocal Then rAxis = YAxis() Else rAxis = New dxeLine(TPLANES.Axis(New TPLANE("World"), dxxAxisDescriptors.Y, aStartPt:=Origin))
            Return RotateAboutLine(rAxis, aAngle, bInRadians, False, True)
        End Function
        Friend Function SpinV(aAngle As Double, Optional bInRadians As Boolean = False, Optional bLocal As Boolean = True, Optional bSuppressEvnts As Boolean = True) As Boolean
            Dim rAxis As dxeLine = Nothing
            Return SpinV(aAngle, bInRadians, rAxis, bLocal, bSuppressEvnts)
        End Function
        Friend Function SpinV(aAngle As Double, bInRadians As Boolean, ByRef rAxis As dxeLine, Optional bLocal As Boolean = True, Optional bSuppressEvnts As Boolean = True) As Boolean
            '#1the angle to tip
            '#2flag indicating if the passed angle is in radians
            '^rotates the plane about its y axis
            If bLocal Then rAxis = YAxis() Else rAxis = New dxeLine(TPLANES.Axis(New TPLANE("World"), dxxAxisDescriptors.Y, aStartPt:=Origin))
            Return RotateAboutV(rAxis.StartPtV, rAxis.DirectionV, aAngle, bInRadians, False, True, bSuppressEvnts)
        End Function
        Public Function Tip(aAngle As Double, Optional bInRadians As Boolean = False, Optional bLocal As Boolean = True) As Boolean
            Dim rAxis As dxeLine = Nothing
            Return Tip(aAngle, rAxis, bInRadians, bLocal)
        End Function
        Public Function Tip(aAngle As Double, ByRef rAxis As dxeLine, Optional bInRadians As Boolean = False, Optional bLocal As Boolean = True) As Boolean
            '#1the angle to tip
            '#2flag indicating if the passed angle is in radians
            '^rotates the plane about its x axis
            If bLocal Then rAxis = XAxis() Else rAxis = New dxeLine(TPLANES.Axis(New TPLANE("World"), dxxAxisDescriptors.X, aStartPt:=Origin))
            Return RotateAboutLine(rAxis, aAngle, bInRadians, False, True)
        End Function
        Friend Function TipV(aAngle As Double, Optional bInRadians As Boolean = False, Optional bLocal As Boolean = True, Optional bSuppressEvnts As Boolean = True) As Boolean
            Dim rAxis As dxeLine = Nothing
            Return TipV(aAngle, bInRadians, rAxis, bLocal)
        End Function
        Friend Function TipV(aAngle As Double, bInRadians As Boolean, ByRef rAxis As dxeLine, Optional bLocal As Boolean = True, Optional bSuppressEvnts As Boolean = True) As Boolean
            '#1the angle to tip
            '#2flag indicating if the passed angle is in radians
            '^rotates the plane about its x axis
            If bLocal Then rAxis = XAxis() Else rAxis = New dxeLine(TPLANES.Axis(New TPLANE("World"), dxxAxisDescriptors.X, aStartPt:=Origin))
            Return RotateAboutV(rAxis.StartPtV, rAxis.DirectionV, aAngle, bInRadians, False, True, bSuppressEvnts)
        End Function
        Public Function TransferVectors(aVectors As colDXFVectors, Optional aRotation As Double = 0.0, Optional bNoZ As Boolean = False) As colDXFVectors
            Dim _rVal As New colDXFVectors
            '^used to create a new vector with respect to the origin of the system

            If aVectors Is Nothing Then Return _rVal

            For i As Integer = 1 To aVectors.Count
                Dim v1 = aVectors.ItemVertex(i)
                If Not bNoZ Then
                    v1.Vector = _Struc.Vector(v1.Vector.X, v1.Vector.Y, v1.Vector.Z, aVertexType:=TVALUES.ToByte(v1.Vector.Code))
                Else
                    v1.Vector = _Struc.Vector(v1.Vector.X, v1.Vector.Y, 0, aVertexType:=TVALUES.ToByte(v1.Vector.Code))
                End If
                If aRotation <> 0 Then v1.Vector.RotateAbout(_Struc.Origin, _Struc.ZDirection, aRotation, False)
                _rVal.Add(CType(v1, dxfVector))
            Next i
            Return _rVal
        End Function
        Public Function PolarVector(aDistance As Double, aAngle As Double, Optional bInRadians As Boolean = False, Optional aElevation As Double = 0.0, Optional aOrigin As iVector = Nothing) As dxfVector
            Return New dxfVector(_Struc.PolarVector(aDistance, aAngle, bInRadians, aElevation, aOrigin))
            '#1the distance for the point
            '#2the angle for the point
            '#3flag indicating that the angle is degrees or radians
            '#4the Z coordinate for the new point
            '#5an optional plane origin
            '^used to create a new polar point with respect to the passed plane
        End Function

        ''' <summary>
        '''used to create a new vector with respect to the origin of the system
        ''' </summary>
        ''' <param name="aX">the X coordinate for the new vector</param>
        ''' <param name="aY">the Y coordinate for the new vector</param>
        ''' <param name="aZ">the Z coordinate for the new vector</param>
        ''' <param name="aOrigin">an orgin to use other than the planes origin to use as the basis for the returned vector</param>
        ''' <param name="aVertexRadius">a radius to assign to the new vector</param>
        ''' <param name="aVectorRotation">a rotation to assign to the new vector</param>
        ''' <param name="aLayerName">a layername to assign to the new vector</param>
        ''' <param name="aColor">a color to assign to the new vector </param>
        ''' <param name="aLineType">a linetype to assign to the new vector</param>
        ''' <param name="aTag"> a tag to assign to the new vector</param>
        ''' <param name="aFlag">a flag to assign to the new vector </param>
        ''' <returns></returns>
        Public Function Vector(Optional aX As Double = 0, Optional aY As Double = 0, Optional aZ As Double = 0, Optional aOrigin As iVector = Nothing, Optional aVertexRadius As Double? = Nothing, Optional aVectorRotation As Double? = Nothing, Optional aLayerName As String = "", Optional aColor As dxxColors? = Nothing, Optional aLineType As String = Nothing, Optional aTag As String = Nothing, Optional aFlag As String = Nothing) As dxfVector
            Dim _rVal As New dxfVector(Strukture.Vector(aX, aY, aZ, aOrigin:=aOrigin))
            _rVal.LCLSet(aLayerName, aColor, aLineType)
            If aTag IsNot Nothing Then _rVal.Tag = aTag
            If aFlag IsNot Nothing Then _rVal.Flag = aFlag
            If aVertexRadius.HasValue Then _rVal.VertexRadius = aVertexRadius.Value
            If aVectorRotation.HasValue Then _rVal.Rotation = aVectorRotation.Value
            Return _rVal
        End Function


        ''' <summary>
        '''used to create a new vector with respect to the origin of the system
        ''' </summary>
        ''' <param name="aXYZObject">the vector to use to establish the ordinates of the returned vector</param>
        ''' <param name="aOrigin">an orgin to use other than the planes origin to use as the basis for the returned vector</param>
        ''' <param name="aVertexRadius">a radius to assign to the new vector</param>
        ''' <param name="aVectorRotation">a rotation to assign to the new vector</param>
        ''' <param name="aLayerName">a layername to assign to the new vector</param>
        ''' <param name="aColor">a color to assign to the new vector </param>
        ''' <param name="aLineType">a linetype to assign to the new vector</param>
        ''' <param name="aTag"> a tag to assign to the new vector</param>
        ''' <param name="aFlag">a flag to assign to the new vector </param>
        ''' <returns></returns>
        Public Function Vector(aXYZObject As iVector, Optional aOrigin As dxfVector = Nothing, Optional aVertexRadius As Double? = Nothing, Optional aVectorRotation As Double? = Nothing, Optional aLayerName As String = "", Optional aColor As dxxColors? = Nothing, Optional aLineType As String = Nothing, Optional aTag As String = Nothing, Optional aFlag As String = Nothing) As dxfVector

            Dim _rVal As dxfVector = dxfVector.FromIVector(aXYZObject, bReturnSomething:=True, bCloneIt:=True)
            _rVal.Strukture = Strukture.Vector(_rVal.X, _rVal.Y, _rVal.Z, aOrigin:=aOrigin)
            _rVal.LCLSet(aLayerName, aColor, aLineType)
            If aTag IsNot Nothing Then _rVal.Tag = aTag
            If aFlag IsNot Nothing Then _rVal.Flag = aFlag
            If aVertexRadius.HasValue Then _rVal.VertexRadius = aVertexRadius.Value
            If aVectorRotation.HasValue Then _rVal.Rotation = aVectorRotation.Value
            Return _rVal
        End Function


        Friend Function VectorV(Optional aX As Double = 0, Optional aY As Double = 0, Optional aZ As Double = 0, Optional aOrigin As dxfVector = Nothing, Optional aVectorRotation As Double = 0) As TVECTOR
            Return Strukture.Vector(aX, aY, aZ, aOrigin:=aOrigin, aVectorRotation:=aVectorRotation)
            '#1the X coordinate for the new vector
            '#2the Y coordinate for the new vector
            '#3the Z coordinate for the new vector
            '^used to create a new vector with respect to the origin of the system

        End Function

        Friend Function VertexV(Optional aX As Double = 0, Optional aY As Double = 0, Optional aZ As Double = 0, Optional aOrigin As dxfVector = Nothing, Optional aVertexRadius As Double? = Nothing, Optional aVectorRotation As Double = 0.0, Optional aLayerName As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aTag As String = Nothing, Optional aFlag As String = Nothing) As TVERTEX
            Dim _rVal As New TVERTEX With {.Vector = Strukture.Vector(aX, aY, aZ, aOrigin:=aOrigin)}
            '#1the X coordinate for the new vector
            '#2the Y coordinate for the new vector
            '#3the Z coordinate for the new vector
            '^used to create a new vector with respect to the origin of the system
            If aLayerName IsNot Nothing Then _rVal.LayerName = aLayerName.Trim()
            If aLineType IsNot Nothing Then _rVal.Linetype = aLineType.Trim()
            _rVal.Color = aColor
            If aTag IsNot Nothing Then _rVal.Tag = aTag
            If aFlag IsNot Nothing Then _rVal.Flag = aFlag
            If aVertexRadius.HasValue Then _rVal.Radius = aVertexRadius.Value
            _rVal.Rotation = aVectorRotation
            Return _rVal
        End Function

        Public Function Vector(aX As Double, aY As Double, aZ As Double) As dxfVector
            Return Vector(aX, aY, aZ, Nothing, Nothing)
        End Function
        Public Function VectorPolar(aDistance As Double, aAngle As Double, Optional bInRadians As Boolean = False, Optional aOrigin As dxfVector = Nothing, Optional aElevation As Double = 0.0, Optional aVertexRadius As Double? = Nothing, Optional aLayerName As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aTag As String = "", Optional aFlag As String = "") As dxfVector
            Dim _rVal As dxfVector = Nothing
            '#1the distance for the point
            '#2the angle for the point
            '#3flag indicating that the angle is degrees or radians
            '#4the Z coordinate for the new point
            '#7the base point to start from (the origin is used if not passed)
            '^used to create a new point with respect to the origin of the system
            _rVal = New dxfVector(Me.Strukture.PolarVector(aDistance, aAngle, bInRadians, aElevation, aOrigin))
            _rVal.LCLSet(aLayerName, aColor, aLineType)
            If aTag IsNot Nothing Then _rVal.Tag = aTag
            If aFlag IsNot Nothing Then _rVal.Flag = aFlag
            If aVertexRadius.HasValue Then _rVal.VertexRadius = aVertexRadius.Value
            Return _rVal
        End Function
        Public Function VectorRelative(aVectorObj As iVector, Optional aXDisplacement As Double = 0.0, Optional aYDisplacement As Double = 0.0, Optional aZDisplacement As Double = 0.0, Optional aVertexRadius As Double? = Nothing, Optional aLayerName As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aTag As String = "", Optional aFlag As String = "") As dxfVector
            Dim _rVal As New dxfVector(_Struc.VectorRelative(New TVECTOR(aVectorObj), aXDisplacement, aYDisplacement, aZDisplacement))
            '^used to create a new vector with respect to the origin of the system

            _rVal.LCLSet(aLayerName, aColor, aLineType)
            If aTag IsNot Nothing Then _rVal.Tag = aTag
            If aFlag IsNot Nothing Then _rVal.Flag = aFlag
            If aVertexRadius IsNot Nothing Then
                If aVertexRadius.HasValue Then _rVal.VertexRadius = aVertexRadius.Value

            End If
            Return _rVal
        End Function
        Public Function VerticalLine(aXDim As Double, Optional aLength As Double = 1, Optional aRotation As Double = 0.0, Optional bCenterOnOrigin As Boolean = False, Optional aDisplaySettings As dxfDisplaySettings = Nothing) As dxeLine
            Return New dxeLine(_Struc.VerticalLine(aXDim, aLength, aRotation, bCenterOnOrigin)) With {.PlaneV = _Struc, .DisplaySettings = aDisplaySettings}
        End Function
        Public Function XAxis(Optional aLength As Double = 1, Optional aStartPt As iVector = Nothing, Optional aColor As dxxColors = dxxColors.Red, Optional aXOffset As Double = 0.0, Optional aYOffset As Double = 0.0, Optional aZOffset As Double = 0.0) As dxeLine
            '#1the length for the axis
            '#2an optional origin for the axis
            '^returns a line beginning at the planes origin extending in the planes X direction
            '~if the passed length is negative then the axis extends in the opposite direction
            Return _Struc.Axis(dxxAxisDescriptors.X, aLength, aStartPt, aColor, aXOffset, aYOffset, aZOffset)
        End Function
        Public Function XZPlane(Optional aOrigin As dxfVector = Nothing) As dxfPlane
            Return New dxfPlane(_Struc.StandardPlane(dxxStandardPlanes.XZ, aOrigin))
        End Function
        Public Function YAxis(Optional aLength As Double = 1, Optional aStartPt As iVector = Nothing, Optional aXOffset As Double = 0.0, Optional aYOffset As Double = 0.0, Optional aZOffset As Double = 0.0) As dxeLine
            '#1the length for the axis
            '#2an optional origin for the axis
            '^returns a line beginning at the planes origin extending in the planes Y direction
            '~if the passed length is negative then the axis extends in the opposite direction
            Return _Struc.Axis(dxxAxisDescriptors.Y, aLength, aStartPt, aXOffset:=aXOffset, aYOffset:=aYOffset, aZOffset:=aZOffset)
        End Function
        Public Function YZPlane(Optional aOrigin As dxfVector = Nothing) As dxfPlane
            Return New dxfPlane(_Struc.StandardPlane(dxxStandardPlanes.YZ, aOrigin))
        End Function
        Public Function ZAxis(Optional aLength As Double = 1, Optional aStartPt As iVector = Nothing, Optional aXOffset As Double = 0.0, Optional aYOffset As Double = 0.0, Optional aZOffset As Double = 0.0) As dxeLine
            '#1the length for the axis
            '#2an optional origin for the axis
            '^returns a line beginning at the planes origin extending in the planes normal (Z) direction
            '~if the passed length is negative then the axis extends in the opposite direction
            Return _Struc.Axis(dxxAxisDescriptors.Z, aLength, aStartPt, aXOffset:=aXOffset, aYOffset:=aYOffset, aZOffset:=aZOffset)
        End Function

        Public Sub VectorWithRespectTo(aVector As iVector)
            If aVector Is Nothing Then Return
            Dim v1 As New TVECTOR(aVector)
            v1 = v1.WithRespectTo(Me)
            aVector.X = v1.X
            aVector.Y = v1.Y

        End Sub
        Public Overrides Function ToString() As String
            Return _Struc.ToString
        End Function
#End Region 'Methods
#Region "Shared MEthods"

        Public Shared Function CreateAxis(aPlane As dxfPlane, aPoint As iVector, Optional aAxisDescriptor As dxxAxisDescriptors = dxxAxisDescriptors.Z, Optional aAxisLength As Double = 10) As dxeLine
            If aAxisLength = 0 Then aAxisLength = 10

            Dim pln As New TPLANE(aPlane)
            Dim line As TLINE = TPLANES.Axis(pln, aAxisDescriptor, aAxisLength)
            Dim dir As TVECTOR = line.Direction

            If dir.IsNull() Then Return Nothing
            Dim spt As New TVECTOR(aPoint)
            Return New dxeLine(New TLINE(spt, dir, aAxisLength))

        End Function

        Public Shared Function CreateAxis(aPlane As dxfPlane, aLine As iLine, Optional aAxisDescriptor As dxxAxisDescriptors = dxxAxisDescriptors.Z, Optional aAxisLength As Double = 10) As dxeLine
            If aAxisLength = 0 Then aAxisLength = 10

            Dim dir As TVECTOR
            Dim line As TLINE
            If aLine Is Nothing Then
                Dim pln As New TPLANE(aPlane)
                line = TPLANES.Axis(pln, aAxisDescriptor, aAxisLength)
                dir = line.Direction
            Else
                line = New TLINE(aLine)

                dir = line.Direction
            End If

            If dir.IsNull() Then Return Nothing

            'Return New dxeLine(New TLINE(line.SPT, line.SPT + dir, aAxisLength))
            Return New dxeLine(New TLINE(line.SPT, dir, aAxisLength))

        End Function

        Public Shared ReadOnly Property World As dxfPlane
            Get
                Return New dxfPlane(dxfVector.Zero) With {.Name = "World"}
            End Get
        End Property

        Friend Shared Function IsNull(aPlane As TPLANE) As Boolean

            If TVECTOR.IsNull(aPlane._XDirection) Then Return True
            Return TVECTOR.IsNull(aPlane._YDirection)

        End Function

        ''' <summary>
        ''' returns true if the plane is null or its directions are undefined
        ''' </summary>
        ''' <param name="aPlane"></param>
        ''' <returns></returns>
        Public Shared Function IsNull(aPlane As dxfPlane) As Boolean

            If aPlane Is Nothing Then Return True
            If dxfVector.IsNull(aPlane.XDirection) Then Return True
            Return dxfVector.IsNull(aPlane.YDirection)

        End Function
        Public Shared Function IsNull(aRectangle As iRectangle) As Boolean

            If aRectangle Is Nothing Then Return True
            Return IsNull(aRectangle.Plane)

        End Function


#End Region 'Shared Methods
    End Class 'dxfPlane
End Namespace
