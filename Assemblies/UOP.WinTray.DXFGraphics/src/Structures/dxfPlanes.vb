

Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics.Structures

#Region "Structures"
    Friend Structure TPLANE
        Implements ICloneable
#Region "Members"
        Public DisplayProps As TDISPLAYVARS
        Public Origin As TVECTOR
        Friend _XDirection As TVECTOR
        Friend _YDirection As TVECTOR
        Public Descent As Double
        Public Height As Double
        Public Width As Double
        Public Index As Integer
        Public IsDirty As Boolean
        Public Name As String
        Public ShearAngle As Double
        Public Tag As String
        Public Flag As String
        Public Handle As String
        Public Units As dxxDeviceUnits
        Public Value As Double
        Public ImageGUID As String
#End Region 'Members
#Region "Constructors"
        Public Sub New(aPlane As dxfPlane, Optional aOrigin As iVector = Nothing)

            'init ----------------------
            DisplayProps = New TDISPLAYVARS("0")
            Origin = New TVECTOR(aOrigin)
            _XDirection = TVECTOR.WorldX
            _YDirection = TVECTOR.WorldY
            Width = 0
            Height = 0
            Descent = 0
            Units = dxxDeviceUnits.Pixels
            Name = "World"
            Tag = ""
            Flag = ""
            Handle = ""
            ShearAngle = 0
            Index = 0
            Value = 0
            ImageGUID = ""
            'init ----------------------

            If dxfPlane.IsNull(aPlane) Then Return

            DisplayProps = New TDISPLAYVARS(aPlane.DisplayProps)

            If aOrigin IsNot Nothing Then Origin = New TVECTOR(aOrigin) Else Origin = New TVECTOR(aPlane.Origin)
            _XDirection = New TVECTOR(aPlane.XDirection)
            _YDirection = New TVECTOR(aPlane.YDirection)
            Width = aPlane.Width
            Height = aPlane.Height
            Descent = aPlane.Descent
            Units = aPlane.Units
            Name = aPlane.Name
            Tag = aPlane.Tag
            Flag = aPlane.Flag
            Handle = aPlane.Handle
            ShearAngle = aPlane.ShearAngle
            Index = aPlane.Index
            Value = aPlane.Value
            ImageGUID = aPlane.ImageGUID


        End Sub

        Friend Sub New(aPlane As dxfPlane, aOrigin As TVECTOR?)

            'init ----------------------
            DisplayProps = New TDISPLAYVARS("0")
            Origin = IIf(aOrigin.HasValue, New TVECTOR(aOrigin.Value), TVECTOR.Zero)
            _XDirection = TVECTOR.WorldX
            _YDirection = TVECTOR.WorldY
            Width = 0
            Height = 0
            Descent = 0
            Units = dxxDeviceUnits.Pixels
            Name = "World"
            Tag = ""
            Flag = ""
            Handle = ""
            ShearAngle = 0
            Index = 0
            Value = 0
            ImageGUID = ""
            'init ----------------------

            If dxfPlane.IsNull(aPlane) Then Return

            DisplayProps = New TDISPLAYVARS(aPlane.DisplayProps)

            If Not aOrigin.HasValue Then Origin = New TVECTOR(aPlane.Origin)
            _XDirection = New TVECTOR(aPlane.XDirection)
            _YDirection = New TVECTOR(aPlane.YDirection)
            Width = aPlane.Width
            Height = aPlane.Height
            Descent = aPlane.Descent
            Units = aPlane.Units
            Name = aPlane.Name
            Tag = aPlane.Tag
            Flag = aPlane.Flag
            Handle = aPlane.Handle
            ShearAngle = aPlane.ShearAngle
            Index = aPlane.Index
            Value = aPlane.Value
            ImageGUID = aPlane.ImageGUID


        End Sub

        Public Sub New(aPlane As TPLANE, aOrigin As iVector)

            'init ----------------------
            Width = aPlane.Width
            Height = aPlane.Height
            Descent = aPlane.Descent
            Units = aPlane.Units
            Name = aPlane.Name
            Flag = aPlane.Flag
            Handle = aPlane.Handle
            Origin = New TVECTOR(aPlane.Origin)
            ShearAngle = aPlane.ShearAngle
            Tag = aPlane.Tag
            Index = aPlane.Index
            Value = aPlane.Value
            _XDirection = New TVECTOR(aPlane.XDirection)
            _YDirection = New TVECTOR(aPlane.YDirection)
            DisplayProps = New TDISPLAYVARS(aPlane.DisplayProps)
            ImageGUID = ""
            'init ----------------------


            If aOrigin Is Nothing Then Origin = New TVECTOR(aPlane)



        End Sub
        Public Sub New(aPlane As TPLANE, Optional aNewName As String = Nothing, Optional bClearDims As Boolean = False, Optional aImageGUID As String = Nothing)

            'init ----------------------
            Width = aPlane.Width
            Height = aPlane.Height
            Descent = aPlane.Descent
            Units = aPlane.Units
            Name = aPlane.Name
            Flag = aPlane.Flag
            Handle = aPlane.Handle
            Origin = New TVECTOR(aPlane.Origin)
            ShearAngle = aPlane.ShearAngle
            Tag = aPlane.Tag
            Index = aPlane.Index
            Value = aPlane.Value
            _XDirection = New TVECTOR(aPlane.XDirection)
            _YDirection = New TVECTOR(aPlane.YDirection)
            DisplayProps = New TDISPLAYVARS(aPlane.DisplayProps)
            ImageGUID = ""
            'init ----------------------
            If bClearDims Then
                Width = 0
                Height = 0
                Descent = 0
            End If
            If Not String.IsNullOrWhiteSpace(aNewName) Then Name = aNewName.Trim()
            If Not String.IsNullOrWhiteSpace(aImageGUID) Then ImageGUID = aImageGUID.Trim()

            If TPLANE.IsNull(Me) Then
                _XDirection = TVECTOR.WorldX
                _YDirection = TVECTOR.WorldY
            End If

        End Sub

        Public Sub New(aPlane As dxfPlane, ByRef rPlaneIsDefine As Boolean)
            'init ----------------------
            DisplayProps = New TDISPLAYVARS("0")
            Origin = TVECTOR.Zero
            _XDirection = TVECTOR.WorldX
            _YDirection = TVECTOR.WorldY
            Width = 0
            Height = 0
            Descent = 0
            Units = dxxDeviceUnits.Pixels
            Name = "World"
            Tag = ""
            Flag = ""
            Handle = ""
            ShearAngle = 0
            Index = 0
            Value = Nothing
            ImageGUID = ""
            'init ----------------------
            rPlaneIsDefine = Not dxfPlane.IsNull(aPlane)
            If rPlaneIsDefine Then
                Name = aPlane.Name
                Units = aPlane.Units
                rPlaneIsDefine = aPlane.IsDefined
            End If
            If rPlaneIsDefine Then
                Define(aPlane.OriginV, aPlane.XDirectionV, aPlane.YDirectionV)
            End If
        End Sub
        Public Sub New(Optional aName As String = "World", Optional aUnits As dxxDeviceUnits = dxxDeviceUnits.Pixels)
            'init ----------------------
            DisplayProps = New TDISPLAYVARS("0")
            Origin = TVECTOR.Zero
            _XDirection = TVECTOR.WorldX
            _YDirection = TVECTOR.WorldY
            Width = 0
            Height = 0
            Descent = 0
            Units = dxxDeviceUnits.Pixels
            Name = "World"
            Tag = ""
            Flag = ""
            Handle = ""
            ShearAngle = 0
            Index = 0
            Value = Nothing
            ImageGUID = ""
            'init ----------------------

            If Not String.IsNullOrWhiteSpace(aName) Then Name = aName.Trim
            Units = aUnits
        End Sub
        Public Sub New(aWidth As Double, aHeight As Double, Optional aName As String = "World", Optional aUnits As dxxDeviceUnits = dxxDeviceUnits.Pixels)

            'init ----------------------
            DisplayProps = New TDISPLAYVARS("0")
            Origin = TVECTOR.Zero
            _XDirection = TVECTOR.WorldX
            _YDirection = TVECTOR.WorldY
            Width = 0
            Height = 0
            Descent = 0
            Units = dxxDeviceUnits.Pixels
            Name = "World"
            Tag = ""
            Flag = ""
            Handle = ""
            ShearAngle = 0
            Index = 0
            Value = Nothing
            ImageGUID = ""
            'init ----------------------
            Width = aWidth
            Height = aHeight
            If Not String.IsNullOrWhiteSpace(aName) Then Name = aName.Trim

            Units = aUnits

        End Sub

        Public Sub New(aRectangle As iRectangle, Optional aOrigin As dxfVector = Nothing)

            'init ----------------------
            DisplayProps = New TDISPLAYVARS("0")
            Origin = New TVECTOR(aOrigin)
            _XDirection = TVECTOR.WorldX
            _YDirection = TVECTOR.WorldY
            Width = 0
            Height = 0
            Descent = 0
            Units = dxxDeviceUnits.Pixels
            Name = "World"
            Tag = String.Empty
            Flag = String.Empty
            Handle = String.Empty
            ShearAngle = 0
            Index = 0
            Value = Nothing
            ImageGUID = String.Empty
            'init ----------------------

            If dxfPlane.IsNull(aRectangle) Then Return

            Width = aRectangle.Width
            Height = aRectangle.Height
            If aOrigin Is Nothing Then Origin = New TVECTOR(aRectangle.Center)

            If TypeOf aRectangle Is dxfRectangle Then
                Dim rec As dxfRectangle = DirectCast(aRectangle, dxfRectangle)
                DisplayProps = New TDISPLAYVARS(rec.DisplayStructure)
                _XDirection = New TVECTOR(rec.XDirection)
                _YDirection = New TVECTOR(rec.YDirection)
                Descent = rec.Descent
                Units = rec.Units
                Name = rec.Name
                Tag = rec.Tag
                Flag = rec.Flag
                Handle = rec.Handle
                ShearAngle = rec.ShearAngle
                Index = rec.Index
                Value = rec.Value
            Else
                Dim plane As dxfPlane = aRectangle.Plane
                If plane IsNot Nothing Then
                    _XDirection = New TVECTOR(plane.XDirection)
                    _YDirection = New TVECTOR(plane.YDirection)

                End If

                Try
                    Dim oV As Object = aRectangle
                    If dxfUtils.CheckProperty(oV, "Tag") Then

                        Tag = oV.Tag.ToString()
                    End If
                Catch ex As Exception
                    Console.WriteLine($"TPLANE Creation Exception : {ex.Message}")
                End Try
            End If







        End Sub

        Public Sub New(aOrigin As TVECTOR, aXDir As TVECTOR, aYDir As TVECTOR, Optional aWidth As Double = 0, Optional aHeight As Double = 0)
            'init ----------------------
            DisplayProps = New TDISPLAYVARS("0")
            Origin = TVECTOR.Zero
            _XDirection = TVECTOR.WorldX
            _YDirection = TVECTOR.WorldY
            Width = 0
            Height = 0
            Descent = 0
            Units = dxxDeviceUnits.Pixels
            Name = "World"
            Tag = ""
            Flag = ""
            Handle = ""
            ShearAngle = 0
            Index = 0
            Value = 0
            ImageGUID = ""
            'init ----------------------
            Define(aOrigin, aXDir, aYDir, aHeight:=aHeight, aWidth:=aWidth)
        End Sub
        Friend Sub New(aName As String, aTextBox As TCHARBOX)
            'init ----------------------
            DisplayProps = New TDISPLAYVARS("0")
            Origin = TVECTOR.Zero
            _XDirection = TVECTOR.WorldX
            _YDirection = TVECTOR.WorldY
            Width = 0
            Height = 0
            Descent = 0
            Units = dxxDeviceUnits.Pixels
            Name = "World"
            Tag = ""
            Flag = ""
            Handle = ""
            ShearAngle = 0
            Index = 0
            Value = 0
            ImageGUID = ""
            'init ----------------------

            If Not String.IsNullOrWhiteSpace(aName) Then Name = aName.Trim()
            Origin = New TVECTOR(aTextBox.BasePt)

            CopyDirections(aTextBox)
        End Sub
        Friend Sub New(aBasePt As TVECTOR, aXDir As TVECTOR, aYDir As TVECTOR, aAscent As Double, aDescent As Double, aWidth As Double, Optional aShearAngle As Double = 0, Optional aName As String = "")
            'init ----------------------
            DisplayProps = New TDISPLAYVARS("0")
            Origin = TVECTOR.Zero
            _XDirection = TVECTOR.WorldX
            _YDirection = TVECTOR.WorldY
            Width = 0
            Height = 0
            Descent = 0
            Units = dxxDeviceUnits.Pixels
            Name = "World"
            Tag = ""
            Flag = ""
            Handle = ""
            ShearAngle = 0
            Index = 0
            Value = 0
            ImageGUID = ""
            'init ----------------------

            If Not String.IsNullOrWhiteSpace(aName) Then Name = aName.Trim()
            Define(aBasePt, aXDir, aYDir, Math.Abs(aAscent) + Math.Abs(aDescent), Math.Abs(aWidth))
            Descent = Math.Abs(aDescent)
            Origin += XDirection * 0.5 * Width
            Origin += YDirection * (-Descent + (0.5 * Height))
        End Sub
#End Region 'Constructors
#Region "Properties"
        Public ReadOnly Property DirectionsAreDefined As Boolean
            Get
                Dim _rVal As Boolean = True
                Dim xFlag As Boolean
                If TVECTOR.IsNull(_XDirection) Then
                    _XDirection = TVECTOR.WorldX
                    _YDirection = TVECTOR.WorldY
                    _rVal = False
                Else
                    _XDirection.Normalize(xFlag)
                    If xFlag Then
                        _XDirection = TVECTOR.WorldX
                        _YDirection = TVECTOR.WorldY
                        _rVal = False
                    End If
                End If
                If TVECTOR.IsNull(_YDirection) Then
                    _YDirection = TVECTOR.WorldZ.CrossProduct(_XDirection)
                    _rVal = False
                Else
                    _YDirection.Normalize(xFlag)
                    If xFlag Then
                        _YDirection = TVECTOR.WorldZ.CrossProduct(_XDirection)
                        _rVal = False
                    End If
                End If
                If TVECTOR.IsNull(_XDirection, 6) Then Return False
                If TVECTOR.IsNull(_YDirection, 6) Then Return False
                Return _rVal
            End Get
        End Property
        Public Function EulerAngles(Optional bReturnInverse As Boolean = False) As TVECTOR

            '^returns the Euler Angles of the plane in degrees)
            Dim aRoll As Double = dxfMath.ArcSine(-_YDirection.Z, True)
            Dim aPitch As Double = dxfMath.ArcTan2(_XDirection.Z, ZDirection.Z, True)
            Dim aYaw As Double = dxfMath.ArcTan2(_YDirection.X, _YDirection.Y, True)
            If Not bReturnInverse Then
                Return New TVECTOR(aRoll, aPitch, aYaw)
            Else
                Return New TVECTOR(-aRoll, -aPitch, -aYaw)
            End If

        End Function
        Public Function Directions(Optional aPrecis As Integer = 0) As String

            Dim _rVal As String = String.Empty
            If aPrecis > 10 Or aPrecis <= 0 Then aPrecis = 10
            _rVal = _XDirection.Coordinates(aPrecis)
            _rVal += dxfGlobals.Delim & _YDirection.Coordinates(aPrecis)
            _rVal += dxfGlobals.Delim & ZDirection.Coordinates(aPrecis)
            Return _rVal
        End Function
        Public ReadOnly Property Pitch As Double
            Get
                '^the angle between the y axis and the global y axis
                '~rotation about gobal Y axis
                Return dxfMath.ArcSine(-_YDirection.Z, True)
            End Get
        End Property
        Public ReadOnly Property Roll As Double
            Get
                '^the angle between the x axis and the global x axis
                '~rotation about gobal x axis
                Return dxfMath.ArcTan2(_XDirection.Z, ZDirection.Z, True)
            End Get
        End Property
        Public ReadOnly Property Yaw As Double
            Get
                '^the angle between the z axis and the global z axis
                '~rotation about gobal Z axis
                Return dxfMath.ArcTan2(_YDirection.X, _YDirection.Y, True)
            End Get
        End Property
        '^returns the aspect ratio of the plane
        '~aspect is Width / Height

        Public ReadOnly Property PlanarBounds As TSEGMENTS
            Get
                Dim _rVal = New TSEGMENTS(0)
                Dim vPts As TVECTORS = Corners(True, False)
                _rVal.Add(New TSEGMENT(vPts.Item(1), vPts.Item(2), False))
                _rVal.Add(New TSEGMENT(vPts.Item(2), vPts.Item(3), False))
                _rVal.Add(New TSEGMENT(vPts.Item(3), vPts.Item(4), False))
                _rVal.Add(New TSEGMENT(vPts.Item(4), vPts.Item(1), False))
                Return _rVal
            End Get
        End Property

        Public ReadOnly Property Right As Double
            Get
                Return Origin.X + 0.5 * Width
            End Get
        End Property

        Public ReadOnly Property Left As Double
            Get
                Return Origin.X - 0.5 * Width
            End Get
        End Property

        Public ReadOnly Property Top As Double
            Get
                Return Origin.Y + 0.5 * Height
            End Get
        End Property

        Public ReadOnly Property Bottom As Double
            Get
                Return Origin.Y - 0.5 * Height
            End Get
        End Property

        Public ReadOnly Property Diagonal As Double
            Get
                Return Math.Round(Math.Sqrt(Width ^ 2 + Height ^ 2), 4)
            End Get
        End Property

        Public ReadOnly Property Base As Double
            Get
                Return Origin.Y - 0.5 * Height + Descent
            End Get
        End Property

        Public ReadOnly Property IsDefined As Boolean
            Get
                Return Not TPLANE.IsNull(Me)
            End Get
        End Property

        Public Property X As Double
            Get
                Return Origin.X
            End Get
            Set(value As Double)
                Origin.X = value
            End Set
        End Property

        Public Property Y As Double
            Get
                Return Origin.Y
            End Get
            Set(value As Double)
                Origin.Y = value
            End Set
        End Property

        Public Property Z As Double
            Get
                Return Origin.Z
            End Get
            Set(value As Double)
                Origin.Z = value
            End Set
        End Property


        Public ReadOnly Property IsWorld As Boolean
            Get
                If Math.Round(_XDirection.X, 6) <> 1 Then Return False
                If Math.Round(_YDirection.Y, 6) <> 1 Then Return False
                Return True
            End Get
        End Property
        Public ReadOnly Property XDirection As TVECTOR
            Get
                Return _XDirection
            End Get
        End Property
        Public ReadOnly Property YDirection As TVECTOR
            Get
                Return _YDirection
            End Get
        End Property
        Public ReadOnly Property ZDirection As TVECTOR
            Get
                Return _XDirection.CrossProduct(_YDirection)
            End Get
        End Property
        Public ReadOnly Property Properties As TPROPERTIES
            Get
                Dim _rVal As New TPROPERTIES("", bNonDXF:=True)
                _rVal.Add(New TPROPERTY(1, Height, "Height", dxxPropertyTypes.dxf_Double, bNonDXF:=True))
                _rVal.Add(New TPROPERTY(2, Width, "Width", dxxPropertyTypes.dxf_Double, bNonDXF:=True))
                _rVal.Add(New TPROPERTY(3, Origin.Coordinates(0), "Origin", dxxPropertyTypes.dxf_String, bNonDXF:=True))
                _rVal.Add(New TPROPERTY(4, XDirection.Coordinates(0), "X", dxxPropertyTypes.dxf_String, bNonDXF:=True))
                _rVal.Add(New TPROPERTY(5, YDirection.Coordinates(0), "Y", dxxPropertyTypes.dxf_String, bNonDXF:=True))
                _rVal.Add(New TPROPERTY(6, ZDirection.Coordinates(0), "Z", dxxPropertyTypes.dxf_String, bNonDXF:=True))
                _rVal.Add(New TPROPERTY(7, Units, "Units", dxxPropertyTypes.dxf_Integer, bNonDXF:=True))

                Return _rVal
            End Get
        End Property
#End Region 'Properties
#Region "Methods"
        Public Function AspectRatio(Optional bNoZeros As Boolean = True) As Double
            If Height <> 0 Then
                Return Math.Abs(Width / Height)
            Else
                If bNoZeros Then Return 1 Else Return 0
            End If

        End Function

        Public Function XAxis(Optional aLength As Double = 1) As TLINE
            aLength = Math.Abs(aLength)
            If aLength <= 0 Then aLength = 1
            Dim sp As New TVECTOR(Origin)
            Dim ep As TVECTOR = sp + _XDirection * aLength
            Return New TLINE(sp, ep)

        End Function
        Public Function YAxis(Optional aLength As Double = 1) As TLINE

            aLength = Math.Abs(aLength)
            If aLength <= 0 Then aLength = 1
            Dim sp As New TVECTOR(Origin)
            Dim ep As TVECTOR = sp + _YDirection * aLength
            Return New TLINE(sp, ep)

        End Function

        Public Function ZAxis(Optional aLength As Double = 1) As TLINE

            aLength = Math.Abs(aLength)
            If aLength <= 0 Then aLength = 1
            Dim sp As New TVECTOR(Origin)
            Dim ep As TVECTOR = sp + ZDirection * aLength
            Return New TLINE(sp, ep)

        End Function

        Public Function OrthographicProjectionMatrix(Optional bCenterOnDimensions As Boolean = True, Optional aScaler As iVector = Nothing) As TMATRIX4
            Dim c1 As TVECTRIX = CType(_XDirection, TVECTRIX)
            Dim c2 As TVECTRIX = CType(_YDirection, TVECTRIX)
            Dim c3 As TVECTRIX = CType(_XDirection.RotatedAbout(ZDirection, 45, False), TVECTRIX)
            Dim c4 As New TVECTRIX(0, 0, 0, 1)
            Dim M As TMATRIX4 = TMATRIX4.Identity
            M.Columns_Set(c1, c2, c3, c4)
            Dim P As TMATRIX4
            Dim T As TMATRIX4 = M.Transposed
            Dim S As TMATRIX4 = T * M
            Dim U As TMATRIX4 = S.Inverted
            P = M * U * T
            P.Columns_Get(c1, c2, c3, c4)
            Dim v1 As TVECTOR = TVECTOR.Zero
            'add the translation
            v1 = v1.ProjectedTo(Me)
            If bCenterOnDimensions Then v1 += New TVECTOR(0.5 * Width, 0.5 * Height)
            c1.s = v1.X
            c2.s = v1.Y
            If aScaler IsNot Nothing Then
                c1.X *= aScaler.X
                c2.Y *= aScaler.Y
                c3.Z *= aScaler.Z
            End If
            P.Columns_Set(c1, c2, c3, c4)
            Return P
        End Function
        Public Function Axis(aAxisDescr As dxxAxisDescriptors, Optional aLength As Double = 1, Optional aStartPt As iVector = Nothing, Optional aColor As dxxColors = dxxColors.Undefined, Optional aXOffset As Double = 0.0, Optional aYOffset As Double = 0.0, Optional aZOffset As Double = 0.0) As dxeLine
            '#1 the primary plane direction to align the returned line with
            '#2the length for the axis
            '#3an optional origin for the axis
            '^returns a line beginning at the planes origin extending in the planes X direction
            '~if the passed length is negative then the axis extends in the opposite direction
            Dim aDir As TVECTOR

            If aAxisDescr = dxxAxisDescriptors.Z Then
                aDir = ZDirection
            ElseIf aAxisDescr = dxxAxisDescriptors.Y Then
                aDir = YDirection
            Else
                aDir = XDirection
            End If
            If aLength = 0 Then aLength = 1
            Dim sp As TVECTOR = New TVECTOR(Origin)
            If aStartPt IsNot Nothing Then sp = New TVECTOR(aStartPt)
            If aXOffset <> 0 Then sp += XDirection * aXOffset
            If aYOffset <> 0 Then sp += YDirection * aYOffset
            If aZOffset <> 0 Then sp += ZDirection * aZOffset
            Dim ep As TVECTOR = sp + aDir * aLength
            Dim _rVal As New dxeLine(New TLINE(sp, ep))
            If aColor <> dxxColors.Undefined Then _rVal.Color = aColor
            Return _rVal
        End Function
        Public Function ProjectedToPlane(aPlane As TPLANE) As TPLANE
            Dim _rVal As New TPLANE(Me, New dxfVector(Origin.ProjectedTo(aPlane)))

            If Width <> 0 Or Height <> 0 Then
                Dim crners As TVECTORS = RectanglePts(False).ProjectedTo(aPlane)
                Dim bnds As TPLANE = crners.BoundingRectangle(aPlane)
                _rVal.Width = bnds.Width
                _rVal.Height = bnds.Height
            End If
            Return _rVal
        End Function
        Public Function CreateArc(aX As Double, aY As Double, aRadius As Double, Optional aStartAngle As Double = 0, Optional aEndAngle As Double = 360, Optional aZ As Double = 0, Optional aRotation As Double? = Nothing) As TSEGMENT

            Dim rot As Double = 0
            If aRotation.HasValue Then rot = aRotation.Value
            Dim bPln As New TPLANE(Me, New dxfVector(Vector(aX, aY, aZ, aVectorRotation:=rot)))
            Dim a As New TARC(bPln, bPln.Origin, Math.Abs(aRadius), aStartAngle, aEndAngle)
            Return New TSEGMENT(New TARC(bPln, bPln.Origin, Math.Abs(aRadius), aStartAngle, aEndAngle))


        End Function
        Public Function AlignTo(aDirection As TVECTOR, Optional aAxis As dxxAxisDescriptors = dxxAxisDescriptors.Z) As Boolean
            Dim rNormal As dxfVector = Nothing
            Dim rAlignmentAngle As Double = 0.0
            Return AlignTo(aDirection, aAxis, rNormal, rAlignmentAngle)
        End Function

        Public Function AlignTo(aDirection As iVector, Optional aAxis As dxxAxisDescriptors = dxxAxisDescriptors.Z) As Boolean
            Dim rNormal As dxfVector = Nothing
            Dim rAlignmentAngle As Double = 0.0
            Return AlignTo(New TVECTOR(aDirection), aAxis, rNormal, rAlignmentAngle)
        End Function
        Public Function AlignTo(aDirection As TVECTOR, aAxis As dxxAxisDescriptors, ByRef rNormal As dxfVector, ByRef rAlignmentAngle As Double) As Boolean

            If TVECTOR.IsNull(aDirection, 4) Then Return False
            '#1the plane to align
            '#2the direction to align to
            '#3the axis of the plane to align to the passed direction
            '#4returns the rotation axis used to rotate the plane into alignment
            '#5returns the angle the plane had to be rotated to align it to the passed direction
            '^returns a copy of the passed plane with the its indicated primary axis aligned to the passed direction
            Dim d1 As TVECTOR
            Dim d2 As TVECTOR
            Dim isnull As Boolean
            Dim bAligned As Boolean
            Dim bInverted As Boolean
            Dim aN As TVECTOR
            aN = ZDirection
            rNormal = New dxfVector(aN)
            rAlignmentAngle = 0
            d1 = aDirection.Normalized(isnull)
            If isnull Then Return False
            Select Case aAxis
                Case dxxAxisDescriptors.X
                    d2 = XDirection
                Case dxxAxisDescriptors.Y
                    d2 = YDirection
                Case Else
                    d2 = ZDirection
            End Select
            bAligned = d1.Equals(d2, True, 4, bInverted)
            If bAligned Then
                If bInverted Then
                    aN = XDirection
                    rAlignmentAngle = 180
                Else
                    Return False
                End If
            Else
                aN = d2.CrossProduct(d1)
                rAlignmentAngle = d2.AngleTo(d1, aN)
            End If
            rNormal.Strukture = aN
            Return Rotate(Origin, aN, rAlignmentAngle, False, False, True)
        End Function
        Public Function AlignedTo(aDirection As TVECTOR, Optional aAxis As dxxAxisDescriptors = dxxAxisDescriptors.Z) As TPLANE
            Dim rNormal As dxfVector = Nothing
            Dim rAlignmentAngle As Double = 0.0
            Return AlignedTo(aDirection, aAxis, rNormal, rAlignmentAngle)
        End Function
        Public Function AlignedTo(aDirection As TVECTOR, aAxis As dxxAxisDescriptors, ByRef rNormal As dxfVector, ByRef rAlignmentAngle As Double) As TPLANE
            Dim _rVal As New TPLANE(Me)

            If TVECTOR.IsNull(aDirection, 4) Then Return _rVal
            _rVal.AlignTo(aDirection, aAxis, rNormal, rAlignmentAngle)
            Return _rVal
            '#1the plane to align
            '#2the direction to align to
            '#3the axis of the plane to align to the passed direction
            '#4returns the rotation axis used to rotate the plane into alignment
            '#5returns the angle the plane had to be rotated to align it to the passed direction
            '^returns a copy of the passed plane with the its indicated primary axis aligned to the passed direction
        End Function
        Public Function AngleVector(aAngle As Double, aDistance As Double, bInRadians As Boolean, Optional aElevation As Double? = Nothing) As TVECTOR
            Dim _rVal As New TVECTOR(Origin)
            '#1the coordinate system to use
            '#2the Point to project from
            '#3the direction (angle) to project in
            '#4the distance to project
            '#5flag to indicate if the passed angle is in Radians
            '^returns a vector located the passed distance away at the passed angle from the passed vector
            '~if the passed vector is nothing then the center of the passed coodinate system is used as the base point.
            '~the vector is rotated about the z axis of the passed OCS

            Dim aDir As TVECTOR = _XDirection.Normalized
            If aAngle <> 0 Then aDir.RotateAbout(ZDirection, aAngle, bInRadians)
            If aDistance <> 0 Then _rVal += aDir * aDistance
            If aElevation.HasValue Then
                _rVal += ZDirection * aElevation.Value
            End If

            Return _rVal
        End Function

        Public Function AngleVector(aPoint As iVector, aAngle As Double, aDistance As Double, bInRadians As Boolean, Optional aElevation As Double? = Nothing) As TVERTEX
            Dim _rVal As New TVERTEX(aPoint)

            '#1the coordinate system to use
            '#2the Point to project from
            '#3the direction (angle) to project in
            '#4the distance to project
            '#5flag to indicate if the passed angle is in Radians
            '^returns a vector located the passed distance away at the passed angle from the passed vector
            '~if the passed vector is nothing then the center of the passed coodinate system is used as the base point.
            '~the vector is rotated about the z axis of the passed OCS

            Dim aDir As TVECTOR = _XDirection.Normalized
            If aAngle <> 0 Then aDir.RotateAbout(ZDirection, aAngle, bInRadians)
            If aDistance <> 0 Then _rVal += aDir * aDistance
            If aElevation.HasValue Then _rVal += ZDirection * aElevation.Value
            Return _rVal
        End Function

        Public Function AngleVector(aPoint As TVECTOR, aAngle As Double, aDistance As Double, bInRadians As Boolean, Optional aElevation As Double? = Nothing) As TVECTOR
            Dim _rVal As New TVECTOR(aPoint)

            '#1the coordinate system to use
            '#2the Point to project from
            '#3the direction (angle) to project in
            '#4the distance to project
            '#5flag to indicate if the passed angle is in Radians
            '^returns a vector located the passed distance away at the passed angle from the passed vector
            '~if the passed vector is nothing then the center of the passed coodinate system is used as the base point.
            '~the vector is rotated about the z axis of the passed OCS

            Dim aDir As TVECTOR = _XDirection.Normalized
            If aAngle <> 0 Then aDir.RotateAbout(ZDirection, aAngle, bInRadians)
            If aDistance <> 0 Then _rVal += aDir * aDistance
            If aElevation.HasValue Then _rVal += ZDirection * aElevation.Value
            Return _rVal
        End Function
        Public Function BestFitScale(aFitWidth As Double, aFitHeight As Double, Optional bWholeNumsOnly As Boolean = False, Optional bReturnANSIStandard As Boolean = False, Optional aWidthBuffer As Double? = Nothing, Optional aHeightBuffer As Double? = Nothing, Optional bMetricScales As Boolean = False) As Double
            Dim rScaleString As String = String.Empty
            Return BestFitScale(aFitWidth, aFitHeight, bWholeNumsOnly, bReturnANSIStandard, rScaleString, aWidthBuffer, aHeightBuffer, bMetricScales)
        End Function
        Public Function BestFitScale(aFitWidth As Double, aFitHeight As Double, bWholeNumsOnly As Boolean, bReturnANSIStandard As Boolean, ByRef rScaleString As String, Optional aWidthBuffer As Double? = Nothing, Optional aHeightBuffer As Double? = Nothing, Optional bMetricScales As Boolean = False) As Double
            rScaleString = "  1"
            If Not (Width > 0 And Height > 0) Then Return 1
            Dim fW As Double = Math.Abs(aFitWidth)
            Dim fH As Double = Math.Abs(aFitHeight)
            Dim aspct As Double
            Dim sclup As Double
            Dim aPln As New TPLANE(Me)
            Dim aBuf As Double

            If fW = 0 And fH = 0 Then Return 1
            If aWidthBuffer.HasValue Then
                aBuf = aWidthBuffer.Value
                If aBuf > 0.95 * aPln.Width Then aBuf = 0.95 * aPln.Width
                aPln.Width -= aBuf
            End If
            If aHeightBuffer.HasValue Then
                aBuf = aHeightBuffer.Value
                If aBuf > 0.95 * aPln.Height Then aBuf = 0.95 * aPln.Height
                aPln.Height -= aBuf
            End If
            aspct = aPln.AspectRatio(False)
            If fW = 0 And fH <> 0 Then fW = fH * aspct
            If fW <> 0 And fH = 0 Then fH = fW / aspct
            'determine the scale up to include the requested rectangle dims inside the fit rec
            sclup = 1
            If fW > aPln.Width Then sclup = fW / aPln.Width
            If fH > aPln.Height * sclup Then sclup = fH / aPln.Height
            If bReturnANSIStandard Then
                rScaleString = dxfUtils.NearestAnsiScale(sclup, sclup, bMetricScales)
            Else
                If bWholeNumsOnly Then
                    sclup = dxfMath.RoundTo(sclup, dxxRoundToLimits.One, True)
                    rScaleString = "  " & sclup
                Else
                    rScaleString = "  " & Format(sclup, "0.0#")
                End If
            End If
            Return sclup
        End Function
        Public Function Borders() As TLINES
            Dim _rVal As New TLINES(0)
            Dim wd As Double = Width / 2
            Dim ht As Double = Height / 2
            If wd > 0 Or ht > 0 Then
                If wd > 0 And ht > 0 Then
                    _rVal.Add(Vector(-wd, ht), Vector(-wd, -ht))
                    _rVal.Add(New TLINE(_rVal.Item(1).EPT, Vector(wd, -ht)))
                    _rVal.Add(New TLINE(_rVal.Item(2).EPT, Vector(wd, ht)))
                    _rVal.Add(New TLINE(_rVal.Item(3).EPT, _rVal.Item(1).SPT))
                Else
                    If wd > 0 Then
                        _rVal.Add(New TLINE(Vector(-wd, 0), Vector(wd, 0)))
                    Else
                        _rVal.Add(New TLINE(Vector(0, -ht), Vector(0, ht)))
                    End If
                End If
            Else
                _rVal.Add(New TLINE(Origin, Origin))
            End If
            Return _rVal
        End Function
        Public Function Contains(aPlane As TPLANE, Optional aFudgeFactor As Double = 0.001, Optional bSuppressPlaneTest As Boolean = False, Optional bOnIsIn As Boolean = False) As Boolean
            If TPLANE.IsNull(aPlane) Then Return False
            '#1the plane to test
            '#2a fudge factor to apply
            '#4flag to assume the passed is co-planar with this one
            '#5returns true if the passed vector lies on the corner of the rectangle
            '^returns true if the corners of the passed plane
            Dim pTest As Boolean
            Dim v1 As TVECTOR = aPlane.Point(dxxRectanglePts.TopLeft)
            If Not bSuppressPlaneTest Then v1 = v1.ProjectedTo(Me)
            'v1 = v1.WithRespectTo(Me)
            Dim v2 As TVECTOR = Point(dxxRectanglePts.TopLeft)
            Dim onB As Boolean
            Dim onC As Boolean
            pTest = Not Contains(v1, aFudgeFactor, onB, onC, True, False, True)
            If Not pTest And (onB Or onC) And bOnIsIn Then pTest = True
            If Not pTest Then
                Return False
            Else
                v1 = aPlane.Point(dxxRectanglePts.BottomLeft)
                If Not bSuppressPlaneTest Then v1 = v1.ProjectedTo(Me)
                'v1 = v1.WithRespectTo(Me)
                pTest = Not Contains(v1, aFudgeFactor, onB, onC, True, False, True)
                If Not pTest And (onB Or onC) And bOnIsIn Then pTest = True
                If Not pTest Then
                    Return False
                Else
                    v1 = aPlane.Point(dxxRectanglePts.BottomRight)
                    If Not bSuppressPlaneTest Then v1 = v1.ProjectedTo(Me)
                    'v1 = v1.WithRespectTo(Me)
                    pTest = Not Contains(v1, aFudgeFactor, onB, onC, True, False, True)
                    If Not pTest And (onB Or onC) And bOnIsIn Then pTest = True
                    If Not pTest Then
                        Return False
                    Else
                        v1 = aPlane.Point(dxxRectanglePts.TopRight)
                        If Not bSuppressPlaneTest Then v1 = v1.ProjectedTo(Me)
                        'v1 = v1.WithRespectTo(Me)
                        pTest = Not Contains(v1, aFudgeFactor, onB, onC, True, False, True)
                        If Not pTest And (onB Or onC) And bOnIsIn Then pTest = True
                        If Not pTest Then Return False
                    End If
                End If
            End If
            Return True
        End Function
        Public Function Contains(aVertex As TVERTEX, Optional aFudgeFactor As Double = 0.001, Optional bSuppressPlaneTest As Boolean = False, Optional bSuppressEdgeTest As Boolean = False, Optional bSimpleTest As Boolean = False) As Boolean
            Dim rIsonBound As Boolean = False
            Dim rIsCorner As Boolean = False
            Return Contains(aVertex.Vector, aFudgeFactor, rIsonBound, rIsCorner, bSuppressPlaneTest, bSuppressEdgeTest, bSimpleTest)
        End Function
        Public Function Contains(aVertex As TVERTEX, aFudgeFactor As Double, ByRef rIsonBound As Boolean, ByRef rIsCorner As Boolean, Optional bSuppressPlaneTest As Boolean = False, Optional bSuppressEdgeTest As Boolean = False, Optional bSimpleTest As Boolean = False) As Boolean
            Return Contains(aVertex.Vector, aFudgeFactor, rIsonBound, rIsCorner, bSuppressPlaneTest, bSuppressEdgeTest, bSimpleTest)
        End Function
        Public Function Contains(aVector As TVECTOR, Optional aFudgeFactor As Double = 0.001, Optional bSuppressPlaneTest As Boolean = False, Optional bSuppressEdgeTest As Boolean = False, Optional bSimpleTest As Boolean = False) As Boolean
            Dim rIsonBound As Boolean = False
            Dim rIsCorner As Boolean = False
            Return Contains(aVector, aFudgeFactor, rIsonBound, rIsCorner, bSuppressPlaneTest, bSuppressEdgeTest, bSimpleTest)
        End Function
        Public Function Contains(aVector As TVECTOR, aFudgeFactor As Double, ByRef rIsonBound As Boolean, ByRef rIsCorner As Boolean, Optional bSuppressPlaneTest As Boolean = False, Optional bSuppressEdgeTest As Boolean = False, Optional bSimpleTest As Boolean = False) As Boolean
            rIsonBound = False
            rIsCorner = False

            Dim _rVal As Boolean
            '#1the subject plane
            '#2the vector to test
            '#3a fudge factor to apply
            '#4returns true if the passed vector lies on the bounds of the rectangle
            '#5returns true if the passed vector lies on the corner of the rectangle
            '^returns true if the passed vector lies within the bounds of the rectangle
            If bSimpleTest Then
                Dim v1 As TVECTOR
                Dim tl As TVECTOR
                Dim br As TVECTOR
                tl = Point(dxxRectanglePts.TopLeft).Rounded(3)
                br = Point(dxxRectanglePts.BottomRight).Rounded(3)
                v1 = aVector.Rounded(3)
                If v1.X >= tl.X And v1.X <= br.X Then
                    If v1.Y >= br.Y And v1.Y <= tl.Y Then
                        _rVal = True
                        If v1.X = tl.X Then rIsonBound = True
                        If v1.Y = tl.Y Then rIsonBound = True
                        If v1.X = br.X Then rIsonBound = True
                        If v1.Y = br.Y Then rIsonBound = True
                        If rIsonBound Then
                            If v1.X = tl.X And v1.Y = tl.Y Then rIsCorner = True
                            If v1.X = tl.X And v1.Y = br.Y Then rIsCorner = True
                            If v1.X = br.X And v1.Y = br.Y Then rIsCorner = True
                            If v1.X = br.X And v1.Y = tl.Y Then rIsCorner = True
                        End If
                    End If
                End If
            Else
                Dim d1 As Double
                Dim b1 As Boolean
                Dim B2 As Boolean


                Dim diag As Double = Diagonal
                Dim f1 As Double = TVALUES.LimitedValue(Math.Abs(aFudgeFactor), 0.000001, 0.1)
                'see if its on my plane
                If Not bSuppressPlaneTest Then
                    d1 = aVector.DistanceTo(Me, -1)
                    If Math.Abs(d1) > f1 Then Return _rVal
                End If
                'gross check
                d1 = Origin.DistanceTo(aVector)
                If d1 > diag Then Return _rVal
                Dim cnt As Integer
                Dim boundlines As TLINES = Edges
                'chech the bounds
                If Not bSuppressEdgeTest Then
                    For i As Integer = 1 To boundlines.Count
                        If boundlines.Item(i).ContainsVector(aVector, f1, b1, B2, False) Then rIsonBound = True
                        If rIsonBound Then
                            rIsCorner = b1 Or B2
                            Exit For
                        End If
                    Next i
                End If
                If Not rIsonBound Then
                    cnt = 0
                    Dim ip As TVECTOR
                    Dim exists As Boolean = False
                    Dim planevec As TVECTOR = aVector.ProjectedTo(Me)
                    Dim testLine As New TLINE(planevec, XDirection, 6 * diag)
                    Dim ontest As Boolean = False
                    Dim onedge As Boolean = False
                    Dim bparel As Boolean = False
                    Dim coinc As Boolean = False

                    For i = 1 To boundlines.Count
                        ip = testLine.IntersectionPt(boundlines.Item(i), bparel, coinc, rIsOnFirstLine:=ontest, rIsOnSecondLine:=onedge, rInterceptExists:=exists)
                        If exists And ontest And onedge Then
                            cnt += 1
                        End If
                    Next i
                    If cnt > 0 Then
                        _rVal = cnt Mod 2 = 1
                    End If
                Else
                    _rVal = True
                End If
            End If
            Return _rVal
        End Function
        Public Function Contains(aVectors As TVECTORS, Optional aFudgeFactor As Double = 0.001, Optional rInsideCount As Integer = 0, Optional bBailOnFalse As Boolean = False, Optional bSuppressPlaneTest As Boolean = False, Optional bSuppressEdgeTest As Boolean = False, Optional bSimpleTest As Boolean = False) As Boolean
            rInsideCount = 0
            '#1the vectors to test
            '#2a fudge factor to apply
            '^returns true if the passed vector lies within the bounds of the rectangle
            Dim v1 As TVECTOR
            Dim b1 As Boolean
            Dim b2 As Boolean
            For i As Integer = 1 To aVectors.Count
                v1 = aVectors.Item(i)
                If Contains(v1, aFudgeFactor, b1, b2, bSuppressPlaneTest, bSuppressEdgeTest, bSimpleTest) Then
                    rInsideCount += 1
                Else
                    If bBailOnFalse Then Return False
                End If
            Next i
            Return rInsideCount > 0
        End Function
        Public Function Contains(aLine As TLINE, Optional aFudgeFactor As Double = 0.001) As Boolean

            Dim aFlg As Boolean
            Dim d1 As Double
            Dim aDir As TVECTOR = aLine.Direction(False, aFlg, d1)
            If aFlg Then
                Return ContainsVector(aLine.SPT, aFudgeFactor:=aFudgeFactor)
            Else
                Dim sp As TVECTOR = aLine.SPT
                Dim ep As TVECTOR = aLine.EPT
                If d1 < 1 Then
                    sp += aDir * -1
                    ep += aDir * 1
                End If
                If Not ContainsVector(sp, aFudgeFactor:=aFudgeFactor) Then Return False
                Return ContainsVector(ep, aFudgeFactor:=aFudgeFactor)
            End If
        End Function
        Public Function ContainsVector(aVector As TVECTOR, Optional aFudgeFactor As Double = 0.001, Optional aRecHeight As Double = 0.0, Optional aRecWidth As Double = 0.0) As Boolean
            Dim rDistance As Double = 0.0
            Return ContainsVector(aVector, rDistance, aFudgeFactor, aRecHeight, aRecWidth)
        End Function
        Public Function ContainsVector(aVector As TVECTOR, ByRef rDistance As Double, aFudgeFactor As Double, Optional aRecHeight As Double = 0.0, Optional aRecWidth As Double = 0.0) As Boolean

            Dim _rVal As Boolean
            '#1the plane to dxeDimension
            '#2the vector to dxeDimension
            '#3returns the distance from the vector to the plane
            '#4a number from 0.1 to  0.000001 which is used to determine if the vector is on the plane
            '#5an optional rectangular height to dxeDimension
            '#6an optional rectangular width to dxeDimension
            '^returns true if the perpendicular distance from the vector to the plane is less than the fudge factor
            '~if the rectangular height and width are passed then False is returned if the vector is on the plane but falls outside
            '~of a rectangle centered at the planes origin and aligned with the planes directions of the passed height and width
            aFudgeFactor = TVALUES.LimitedValue(aFudgeFactor, 0.000001, 0.1)
            Dim ip As TVECTOR = dxfProjections.ToPlane(aVector, Me, rDistance:=rDistance)
            Dim v1 As TVECTOR
            rDistance = Math.Round(rDistance, 6)
            _rVal = (rDistance <= aFudgeFactor)
            aRecHeight = Math.Abs(aRecHeight)
            aRecWidth = Math.Abs(aRecWidth)
            If _rVal And aRecHeight > 0 And aRecWidth > 0 Then
                v1 = ip.WithRespectTo(Me)
                If Math.Abs(v1.X) > aRecWidth / 2 Then _rVal = False
                If Math.Abs(v1.Y) > aRecHeight / 2 Then _rVal = False
            End If
            Return _rVal
        End Function
        Public Sub CopyDirections(aPlane As TPLANE)

            If TPLANE.IsNull(aPlane) Then Return
            Define(Origin, aPlane.XDirection, aPlane.YDirection)
        End Sub
        Public Sub CopyDirections(aCharbox As TCHARBOX)
            If Not aCharbox.IsDefined Then Return
            Define(aCharbox.BasePt, aCharbox.XDirection, aCharbox.YDirection)
        End Sub
        Public Function Corners(Optional bSuppressShear As Boolean = True, Optional bClosed As Boolean = False) As TVECTORS
            '^returns corners of the rectangle
            Dim _rVal As TVECTORS
            If bClosed Then _rVal = New TVECTORS(6) Else _rVal = New TVECTORS(4)
            _rVal.SetItem(1, Point(dxxRectanglePts.TopLeft, bSuppressShear, aCode:=dxxVertexStyles.MOVETO))
            _rVal.SetItem(2, Point(dxxRectanglePts.BottomLeft, bSuppressShear, aCode:=dxxVertexStyles.LINETO))
            _rVal.SetItem(3, Point(dxxRectanglePts.BottomRight, bSuppressShear, aCode:=dxxVertexStyles.LINETO))
            _rVal.SetItem(4, Point(dxxRectanglePts.TopRight, bSuppressShear, aCode:=dxxVertexStyles.LINETO))
            If bClosed Then
                _rVal.Update(5, _rVal.Item(1), aCode:=TVALUES.ToByte(dxxVertexStyles.LINETO))
                _rVal.Update(6, _rVal.Item(2), aCode:=TVALUES.ToByte(dxxVertexStyles.LINETO))
            End If
            Return _rVal
        End Function
        Public Function CreateVectors(aVectors As IEnumerable(Of iVector), Optional aOrigin As iVector = Nothing, Optional aElevation As Double? = Nothing, Optional bMaintainZValue As Boolean = False) As colDXFVectors
            Dim _rVal As New colDXFVectors
            '#1the subject plane
            '#2the vector or vectors objects (anything with XYZ Properties)
            '^used to create a collection of dxoVectors based on the passed collection or single xyz object(s)
            '~clones are returned 
            'initialize
            If aVectors Is Nothing Then Return _rVal

            Dim elev As Double = 0

            If aElevation.HasValue Then elev = aElevation.Value
            Dim aPl As New TPLANE(Me)
            If aOrigin IsNot Nothing Then aPl.Origin = New TVECTOR(aOrigin)
            Try
                _rVal = New colDXFVectors()
                For Each v As iVector In aVectors
                    Dim v1 As TVECTOR = aPl.Vector(v.X, v.Y, elev)
                    If Not bMaintainZValue Then
                        _rVal.Add(aPl.Vector(v.X, v.Y, elev))
                    Else
                        _rVal.Add(aPl.Vector(v.X, v.Y, v.Z))
                    End If

                Next v

            Catch ex As Exception
                Throw ex
            End Try
            Return _rVal
        End Function
        Public Function DefineLimits(aLimits As TLIMITS) As TPLANE
            Return New TPLANE(Me, New dxfVector(Vector(aLimits.Left + 0.5 * aLimits.Width, aLimits.Bottom + 0.5 * aLimits.Height))) With {.Height = aLimits.Height, .Width = aLimits.Width, .Descent = Math.Abs(aLimits.base - aLimits.Bottom)}

        End Function
        Public Function Direction(aAngle As Double, Optional bInRadians As Boolean = False) As TVECTOR
            Dim _rVal As New TVECTOR(XDirection)
            '#1the subject plane
            '#2the angle to convert to a direction
            '#3flag indicating that the passed angle is in radians
            '^returns a direction vector for the passed plane oriented by the passed angle with respect to the planes x direction
            If aAngle <> 0 Then _rVal.RotateAbout(ZDirection, aAngle, bInRadians, True)
            Return _rVal
        End Function
        Public Function DirectionDescriptor(Optional bIncludeZ As Boolean = False, Optional aPrecis As Integer = 3) As String
            Dim _rVal As String = String.Empty
            '^a string that describes the Plane
            If aPrecis <= 0 Then aPrecis = 0
            If aPrecis > 10 Then aPrecis = 10
            _rVal = " X" & _XDirection.Coordinates(aPrecis) & " Y" & _YDirection.Coordinates(aPrecis)
            If bIncludeZ Then
                _rVal += " N" & ZDirection.Coordinates(aPrecis)
            End If
            Return _rVal
        End Function
        Public Sub Expand(Optional aXExpansion As Double = 0, Optional aYExpansion As Double = 0, Optional bFractionsPassed As Boolean = False)
            If bFractionsPassed Then
                aXExpansion *= Width
                aYExpansion *= Height
            End If
            Width += aXExpansion
            Height += aYExpansion
            Width = Math.Abs(Width)
            Height = Math.Abs(Height)
        End Sub
        Public Sub GetDimensions(ByRef rWidth As Double, ByRef rHeight As Double, Optional aMultiplier As Double = 1)
            '#1the subject plane
            '#2returns the width
            '#3returns the height
            '^returns the dimensions of the rectangle
            rWidth = Width * Math.Abs(aMultiplier)
            rHeight = Height * Math.Abs(aMultiplier)
        End Sub
        Public Function GripPoint(aAlignm As dxxRectangularAlignments) As TVECTOR
            Dim _rVal As New TVECTOR(0)
            Select Case aAlignm
                Case dxxRectangularAlignments.MiddleCenter
                    _rVal = Origin
                Case dxxRectangularAlignments.MiddleLeft
                    _rVal = Point(dxxRectanglePts.MiddleLeft)
                Case dxxRectangularAlignments.MiddleRight
                    _rVal = Point(dxxRectanglePts.MiddleRight)
                Case dxxRectangularAlignments.TopCenter
                    _rVal = Point(dxxRectanglePts.TopCenter)
                Case dxxRectangularAlignments.TopLeft
                    _rVal = Point(dxxRectanglePts.TopLeft)
                Case dxxRectangularAlignments.TopRight
                    _rVal = Point(dxxRectanglePts.TopRight)
                Case dxxRectangularAlignments.BottomCenter
                    _rVal = Point(dxxRectanglePts.BottomCenter)
                Case dxxRectangularAlignments.BottomLeft
                    _rVal = Point(dxxRectanglePts.BottomLeft)
                Case dxxRectangularAlignments.BottomRight
                    _rVal = Point(dxxRectanglePts.BottomRight)
                Case Else
                    _rVal = Origin
            End Select
            Return _rVal
        End Function
        Public Function HorizontalLine(aYDim As Double, Optional aLength As Double = 1, Optional aRotation As Double = 0.0, Optional bCenterOnOrigin As Boolean = False) As TLINE
            If Not bCenterOnOrigin Then
                Return New TLINE(Vector(0, aYDim, aRotation:=aRotation), Vector(aLength, aYDim, aRotation:=aRotation))
            Else
                Return New TLINE(Vector(-0.5 * aLength, aYDim, aRotation:=aRotation), Vector(0.5 * aLength, aYDim, aRotation:=aRotation))
            End If
        End Function

        Public Function VerticalLine(aXDim As Double, Optional aLength As Double = 1, Optional aRotation As Double = 0.0, Optional bCenterOnOrigin As Boolean = False) As TLINE
            If Not bCenterOnOrigin Then
                Return New TLINE(Vector(aXDim, 0, aRotation:=aRotation), Vector(aXDim, aLength, aRotation:=aRotation))
            Else
                Return New TLINE(Vector(aXDim, -0.5 * aLength, aRotation:=aRotation), Vector(aXDim, 0.5 * aLength, aRotation:=aRotation))
            End If
        End Function

        Public Function Limits(Optional bApplyShear As Boolean = False, Optional aAngle As Double = 0.0) As TLIMITS
            Dim v1 As TVECTOR
            Dim _rVal As New TLIMITS With {
                    .Left = Left,
                    .Right = Right,
                    .Top = Top,
                    .Bottom = Bottom,
                    .base = Base
                    }
            If bApplyShear And ShearAngle <> 0 Then
                If ShearAngle > 0 Then
                    v1 = Point(dxxRectanglePts.BottomLeft, False).WithRespectTo(Me)
                    If v1.X < _rVal.Left Then _rVal.Left = v1.X
                    v1 = Point(dxxRectanglePts.TopRight, False).WithRespectTo(Me)
                    If v1.X > _rVal.Right Then _rVal.Right = v1.X
                Else
                    v1 = Point(dxxRectanglePts.TopLeft, False).WithRespectTo(Me)
                    If v1.X < _rVal.Left Then _rVal.Left = v1.X
                    v1 = Point(dxxRectanglePts.BottomRight, False).WithRespectTo(Me)
                    If v1.X > _rVal.Right Then _rVal.Right = v1.X
                End If
            End If
            Return _rVal
        End Function
        Public Function Line(aLength As Double, Optional aRotation As Double = 0.0, Optional aMidPt As dxfVector = Nothing, Optional aElevation As Double = 0.0, Optional bFromEndPt As Boolean = False) As TLINE
            Dim mpt As TVECTOR
            If aMidPt Is Nothing Then mpt = New TVECTOR(Origin) Else mpt = aMidPt.Strukture
            Return TLINE.PolarLine(XDirection, ZDirection, aLength, aRotation, False, aElevation, mpt, bFromEndPt, True)
        End Function
        Friend Function LineH(aYOrdinate As Double, Optional aLength As Double = 0.0, Optional aInvert As Boolean = False, Optional bByStartPt As Boolean = False) As TLINE
            Return LineH(Vector(0, aYOrdinate), TVALUES.To_DBL(aLength), aInvert, bByStartPt)
        End Function
        Friend Function LineH(aVector As TVECTOR, Optional aLength As Double = 0.0, Optional aInvert As Boolean = False, Optional bByStartPt As Boolean = False) As TLINE
            Dim l1 As Double = aLength
            If l1 = 0 Then l1 = Width
            If l1 = 0 Then aLength = 1
            l1 = Math.Abs(l1)
            Dim v1 As New TVECTOR(aVector)

            Dim f1 As Integer = 1
            If l1 < 0 Then f1 = -1
            If Not bByStartPt Then
                Return New TLINE(v1 + (XDirection * (-f1 * 0.5 * l1)), v1 + (XDirection * (f1 * 0.5 * l1)))
            Else
                Return New TLINE(v1, v1 + (XDirection * (f1 * l1)))
            End If
        End Function
        Public Function LinePolar(aStartPt As TVECTOR, aAngle As Double, aLength As Double, bInRadians As Boolean) As TLINE

            If aLength = 0 Then aLength = 1
            Return New TLINE(New TVECTOR(aStartPt), aStartPt + Direction(aAngle, bInRadians) * aLength)
        End Function
        Public Function LineV(aXOrdinate As Double, Optional aLength As Double = 0, Optional aInvert As Boolean = False, Optional bByStartPt As Boolean = False) As TLINE
            Return LineV(Vector(aXOrdinate, 0, 0), aLength, aInvert, bByStartPt)
        End Function
        Public Function LineV(aVector As TVECTOR, Optional aLength As Double = 0.0, Optional aInvert As Boolean = False, Optional bByStartPt As Boolean = False) As TLINE
            Dim l1 As Double = aLength
            If l1 = 0 Then l1 = Width
            If l1 = 0 Then aLength = 1
            l1 = Math.Abs(l1)
            Dim v1 As New TVECTOR(aVector)
            Dim f1 As Integer = 1
            If l1 < 0 Then f1 = -1
            If Not bByStartPt Then
                Return New TLINE(v1 + (YDirection * (-f1 * 0.5 * l1)), v1 + (YDirection * (f1 * 0.5 * l1)))
            Else
                Return New TLINE(v1, v1 + (YDirection * (f1 * l1)))
            End If
        End Function
        Public Function Mirror(aLine As TLINE, Optional bMirrorOrigin As Boolean = False, Optional bMirrorDirections As Boolean = True, Optional bSuppressCheck As Boolean = False) As Boolean
            Return Mirror(aLine.SPT, aLine.EPT, bMirrorOrigin, bMirrorDirections, bSuppressCheck)
            '#1 line to mirror across
            '#2flag to mirror the origin
            '#3flag to mirror the directions
            '^mirrors the system across the passed line
            '~returns True if the system actually moves from this process
        End Function
        Public Function Mirror(aSP As TVECTOR, aEP As TVECTOR, Optional bMirrorOrigin As Boolean = False, Optional bMirrorDirections As Boolean = True, Optional bSuppressCheck As Boolean = False) As Boolean

            Dim _rVal As Boolean
            '#2the start pt of the line to mirror across
            '#3the end pt of the line to mirror across
            '#4flag to mirror the origin
            '#5flag to mirror the directions
            '^mirrors the system across the passed line
            '~returns True if the system actually moves from this process
            If Not bMirrorOrigin And Not bMirrorOrigin Then Return False
            If Not bSuppressCheck Then
                If dxfProjections.DistanceTo(aSP, aEP) < 0.00001 Then Return False
            End If
            Dim v0 As TVECTOR = TVECTOR.Zero
            If bMirrorOrigin Then
                If Origin.Mirror(New TLINE(aSP, aEP), bSuppressCheck:=True) Then _rVal = True
            End If
            If bMirrorDirections Then
                Dim aDir As TVECTOR
                aDir = aSP.DirectionTo(aEP)
                If _XDirection.Mirror(New TLINE(v0, aDir), bSuppressCheck:=True) Then _rVal = True
                If _YDirection.Mirror(New TLINE(v0, aDir), bSuppressCheck:=True) Then _rVal = True
            End If
            Return _rVal
        End Function
        Public Function Mirrored(aSP As TVECTOR, aEP As TVECTOR, Optional bMirrorOrigin As Boolean = False, Optional bMirrorDirections As Boolean = True, Optional bSuppressCheck As Boolean = False) As TPLANE
            Dim rChanged As Boolean = False
            Return Mirrored(aSP, aEP, bMirrorOrigin, bMirrorDirections, bSuppressCheck, rChanged)
        End Function
        Public Function Mirrored(aSP As TVECTOR, aEP As TVECTOR, bMirrorOrigin As Boolean, bMirrorDirections As Boolean, bSuppressCheck As Boolean, ByRef rChanged As Boolean) As TPLANE
            rChanged = False
            Dim _rVal As New TPLANE(Me)

            '#2the start pt of the line to mirror across
            '#3the end pt of the line to mirror across
            '#4flag to mirror the origin
            '#5flag to mirror the directions
            '^mirrors the system across the passed line
            '~returns True if the system actually moves from this process
            If Not bMirrorOrigin And Not bMirrorOrigin Then Return _rVal
            If Not bSuppressCheck Then
                If dxfProjections.DistanceTo(aSP, aEP) < 0.00001 Then Return _rVal
            End If
            rChanged = _rVal.Mirror(aSP, aEP, bMirrorOrigin, bMirrorDirections, bSuppressCheck)
            Return _rVal
        End Function
        Public Sub Resize(Optional aWidthAdder As Double = 0.0, Optional aHeightAdder As Double = 0.0, Optional bRecenter As Boolean = False)
            If aWidthAdder = 0 And aHeightAdder = 0 Then Return
            If aWidthAdder <> 0 Then
                Width = Math.Abs(Width + aWidthAdder)
                If bRecenter Then Origin += XDirection * (0.5 * aWidthAdder)
            End If
            If aHeightAdder <> 0 Then
                Height = Math.Abs(Height + aHeightAdder)
                If bRecenter Then
                    Origin += YDirection * (0.5 * aHeightAdder)
                End If
            End If
        End Sub
        Public Function Revolve(aAngle As Double, Optional bInRadians As Boolean = False) As Boolean
            If aAngle = 0 Then Return False
            '#1the angle to rotate
            '#2flag indicating if the passed angle is in radians
            '^used to change the orientation and/or the origin of the system by rotating the it about its z axis
            Return Define(Origin, _XDirection.RotatedAbout(ZDirection, aAngle, bInRadians, True), _YDirection.RotatedAbout(ZDirection, aAngle, bInRadians, True))
        End Function
        Public Function Revolved(aAngle As Double, Optional bInRadians As Boolean = False) As TPLANE
            Dim _rVal As New TPLANE(Me)
            _rVal.Revolve(aAngle, bInRadians)
            Return _rVal
            '#1the angle to rotate
            '#2flag indicating if the passed angle is in radians
            '^used to change the orientation and/or the origin of the system by rotating the it about its z axis
        End Function
        Public Function RotateAbout(aOrigin As TVECTOR, aAxis As TVECTOR, aAngle As Double, Optional bInRadians As Boolean = False, Optional bRotateOrigin As Boolean = False, Optional bRotateDirections As Boolean = True, Optional bSuppressNorm As Boolean = False) As Boolean


            If Not bRotateOrigin And Not bRotateDirections Then Return False
            aAngle = TVALUES.NormAng(aAngle, bInRadians, True)
            If aAngle = 0 Then Return False
            '#1the line object to rotate about
            '#2the angle to rotate
            '#3flag indicating if the passed angle is in radians
            '#4flag to rotate the origin of the system about the axis
            '#5flag to rotate the X,Y and Zdirections about the axis
            '#7returns the line used for the rotation axis
            '#8flag to raise no change events
            '^used to change the orientation and/or the origin of the system by rotating the it about the passed axis
            Dim aXs As TVECTOR
            Dim aFlg As Boolean
            Dim xDir As New TVECTOR(XDirection)
            Dim yDir As New TVECTOR(YDirection)
            Dim v0 As New TVECTOR(Origin)
            Dim ang As Double = IIf(Not bInRadians, aAngle * Math.PI / 180, aAngle)
            If Not bSuppressNorm Then
                aXs = aAxis.Normalized(aFlg)
                If aFlg Then Return False
            Else
                aXs = aAxis
            End If
            If bRotateDirections Then
                Dim norm As TVECTOR = aAxis.Normalized()
                xDir.RotateAbout(TVECTOR.Zero, norm, ang, True, True)
                yDir.RotateAbout(TVECTOR.Zero, norm, ang, True, True)
            End If
            If bRotateOrigin Then
                v0.RotateAbout(aOrigin, aXs, ang, True, True)
            End If
            Return Define(v0, xDir, yDir)
        End Function
        Public Function RotatedAbout(aOrigin As TVECTOR, aAxis As TVECTOR, aAngle As Double, Optional bInRadians As Boolean = False, Optional bRotateOrigin As Boolean = False, Optional bRotateDirections As Boolean = True, Optional bSuppressNorm As Boolean = False) As TPLANE
            Dim _rVal As New TPLANE(Me)
            _rVal.RotateAbout(aOrigin, aAxis, aAngle, bInRadians, bRotateOrigin, bRotateDirections, bSuppressNorm)
            Return _rVal
            '#1the line object to rotate about
            '#2the angle to rotate
            '#3flag indicating if the passed angle is in radians
            '#4flag to rotate the origin of the system about the axis
            '#5flag to rotate the X,Y and Zdirections about the axis
            '#7returns the line used for the rotation axis
            '#8flag to raise no change events
            '^used to change the orientation and/or the origin of the system by rotating the it about the passed axis
        End Function
        Public Function RotatedTo(bPlane As TPLANE, ByRef rAxis1 As TVECTOR, ByRef rAngle1 As Double, ByRef rAxis2 As TVECTOR, ByRef rAngle2 As Double) As TPLANE
            rAxis1 = ZDirection
            rAxis2 = ZDirection
            rAngle1 = 0
            rAngle2 = 0
            Dim rPlane As New TPLANE(Me)
            '#1the plane to align
            '#2the the plane to align to
            '#3returns the axis used to align the Z axes of the planes
            '#4returns the angle the Z axes of the planes
            '#5returns the axis used to align the X axes of the planes
            '#4returns the angle the X axes of the planes
            '^returns a copy of the passed plane rotated to align to the alignement plane
            Dim d1 As TVECTOR
            Dim d2 As TVECTOR
            Dim bAligned As Boolean
            Dim bInverted As Boolean
            d1 = rPlane.ZDirection
            d2 = bPlane.ZDirection
            bAligned = d1.Equals(d2, True, 4, bInverted)
            If bAligned Then
                If bInverted Then
                    rAxis1 = rPlane.XDirection
                    rAngle1 = 180
                End If
            Else
                rAxis1 = d2.CrossProduct(d1)
                rAngle1 = d2.AngleTo(d1, rAxis1)
            End If
            If rAngle1 <> 0 Then
                rPlane.RotateAbout(rPlane.Origin, rAxis1, rAngle1, False, False, True)
            End If
            d1 = rPlane.XDirection
            d2 = bPlane.XDirection
            bAligned = d1.Equals(d2, True, 4, bInverted)
            rAxis2 = rPlane.ZDirection
            If bAligned Then
                If bInverted Then
                    rAngle1 = 180
                End If
            Else
                rAngle2 = d2.AngleTo(d1, rAxis2)
            End If
            If rAngle2 <> 0 Then
                rPlane.RotateAbout(rPlane.Origin, rAxis2, rAngle2, False, False, True)
            End If
            Return rPlane
        End Function
        Public Function IsCoplanar(aPlane As TPLANE, Optional aPrecis As Integer = 4) As Boolean
            If TPLANE.IsNull(aPlane) Then Return False
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            If Not ZDirection.Equals(ZDirection, True, aPrecis) Then Return False
            If Math.Round(Origin.DistanceTo(Me, -1), aPrecis) <> 0 Then Return False
            Return Math.Round((Origin + (XDirection * 10)).DistanceTo(Me, -1), aPrecis) = 0
        End Function
        Public Function Rotate(aAngle As Double, Optional bInRadians As Boolean = False, Optional bRotateOrigin As Boolean = False, Optional bRotateDirections As Boolean = True, Optional bSuppressNorm As Boolean = False) As Boolean
            Return Rotate(Origin, ZDirection, aAngle, bInRadians, bRotateOrigin, bRotateDirections, bSuppressNorm)
            '#1the angle to rotate
            '#2flag indicating if the passed angle is in radians
            '#3flag to rotate the origin of the system about the axis
            '#4flag to rotate the X,Y and Zdirections about the axis
            '#8flag to raise no change events
            '^used to change the orientation and/or the origin of the system by rotating the it about the passed axis
        End Function
        Public Function Rotate(aOrigin As TVECTOR, aAxis As TVECTOR, aAngle As Double, Optional bInRadians As Boolean = False, Optional bRotateOrigin As Boolean = False, Optional bRotateDirections As Boolean = True, Optional bSuppressNorm As Boolean = False) As Boolean
            Dim _rVal As Boolean
            '#1the line object to rotate about
            '#2the angle to rotate
            '#3flag indicating if the passed angle is in radians
            '#4flag to rotate the origin of the system about the axis
            '#5flag to rotate the X,Y and Zdirections about the axis
            '#7returns the line used for the rotation axis
            '#8flag to raise no change events
            '^used to change the orientation and/or the origin of the system by rotating the it about the passed axis
            If Not bRotateOrigin And Not bRotateDirections Then Return _rVal
            aAngle = TVALUES.NormAng(aAngle, bInRadians, True)
            If aAngle = 0 Then Return _rVal
            Dim aXs As TVECTOR
            Dim aFlg As Boolean
            If Not bSuppressNorm Then
                aXs = aAxis.Normalized(aFlg)
                If aFlg Then Return _rVal
            Else
                aXs = aAxis
            End If
            If bRotateDirections Then
                If _XDirection.RotateAbout(aXs, aAngle, bInRadians, True) Then _rVal = True
                If _YDirection.RotateAbout(aXs, aAngle, bInRadians, True) Then _rVal = True
            End If
            If bRotateOrigin Then
                If Origin.RotateAbout(aOrigin, aXs, aAngle, bInRadians, True) Then _rVal = True
            End If
            Return _rVal
        End Function
        Public Function AlignmentPoint(aAlignment As dxxRectangularAlignments, Optional bSuppressShear As Boolean = True, Optional aCode As Byte = 0) As TVECTOR
            Dim rHAlign As dxxHorizontalJustifications = dxxHorizontalJustifications.Left
            Dim rVAlign As dxxVerticalJustifications = dxxVerticalJustifications.Top
            Return AlignmentPoint(aAlignment, bSuppressShear, rHAlign, rVAlign, aCode)
        End Function
        Public Function AlignmentPoint(aAlignment As dxxRectangularAlignments, bSuppressShear As Boolean, ByRef rHAlign As dxxHorizontalJustifications, ByRef rVAlign As dxxVerticalJustifications, Optional aCode As Byte = 0) As TVECTOR
            Dim _rVal As TVECTOR
            Dim aShear As Double
            Dim dX As Double
            Dim dY As Double
            If Not bSuppressShear Then aShear = ShearAngle
            dxfUtils.ParseRectangleAlignment(aAlignment, rHAlign, rVAlign)
            If rHAlign = dxxHorizontalJustifications.Left Then
                dX = -0.5 * Width
            ElseIf rHAlign = dxxHorizontalJustifications.Right Then
                dX = 0.5 * Width
            End If
            If rVAlign = dxxVerticalJustifications.Top Then
                dY = 0.5 * Height
            ElseIf rVAlign = dxxVerticalJustifications.Bottom Then
                dY = -0.5 * Height
            End If
            _rVal = Vector(dX, dY, aShearXAngle:=aShear)
            _rVal.Code = aCode
            Return _rVal
        End Function
        Public Function Edge(aEdge As dxxRectangleLines, Optional bSuppressShear As Boolean = True, Optional aLengthAdder As Double = 0.0) As TLINE
            Dim P1 As dxxRectanglePts
            Dim P2 As dxxRectanglePts
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Select Case aEdge
                Case dxxRectangleLines.Baseline
                    P1 = dxxRectanglePts.BaselineLeft
                    P2 = dxxRectanglePts.BaselineRight
                Case dxxRectangleLines.BottomEdge
                    P1 = dxxRectanglePts.BottomLeft
                    P2 = dxxRectanglePts.BottomRight
                Case dxxRectangleLines.Diagonal1
                    P1 = dxxRectanglePts.TopLeft
                    P2 = dxxRectanglePts.BottomRight
                Case dxxRectangleLines.Diagonal2
                    P1 = dxxRectanglePts.BottomLeft
                    P2 = dxxRectanglePts.TopRight
                Case dxxRectangleLines.LeftEdge
                    P1 = dxxRectanglePts.TopLeft
                    P2 = dxxRectanglePts.BottomLeft
                Case dxxRectangleLines.RightEdge
                    P1 = dxxRectanglePts.TopRight
                    P2 = dxxRectanglePts.BottomRight
                Case dxxRectangleLines.TopEdge
                    P1 = dxxRectanglePts.TopLeft
                    P2 = dxxRectanglePts.TopRight
                Case Else
                    P1 = dxxRectanglePts.BaselineLeft
                    P2 = dxxRectanglePts.BaselineRight
            End Select
            v1 = Point(P1, bSuppressShear)
            v2 = Point(P2, bSuppressShear)
            If aLengthAdder <> 0 Then
                Dim aDir As TVECTOR
                aDir = v1.DirectionTo(v2)
                v1 += aDir * (-0.5 * aLengthAdder)
                v2 += aDir * (0.5 * aLengthAdder)
            End If
            Return New TLINE(v1, v2)
        End Function
        Friend ReadOnly Property Edges As TLINES
            Get
                Dim _rVal As New TLINES(0)
                _rVal.Add(Edge(dxxRectangleLines.LeftEdge))
                _rVal.Add(Edge(dxxRectangleLines.TopEdge))
                _rVal.Add(Edge(dxxRectangleLines.RightEdge))
                _rVal.Add(Edge(dxxRectangleLines.BottomEdge))
                Return _rVal
            End Get
        End Property
        Public Function Descriptor(Optional bIncludeAspect As Boolean = False, Optional bIncludeName As Boolean = True, Optional aPrecis As Integer = 3) As String

            '^a string that describes the Plane

            If aPrecis <= 0 Then aPrecis = 0
            If aPrecis > 10 Then aPrecis = 10


            Dim _rVal As String = $"O{ Origin.Coordinates(aPrecis)}"
            _rVal += $" X{_XDirection.Coordinates(aPrecis)} Y { _YDirection.Coordinates(aPrecis) } N { ZDirection.Coordinates(aPrecis)}"
            _rVal += $" {Math.Round(Width, aPrecis):0.000} x {Math.Round(Height, aPrecis):0.000}  {dxfEnums.Description(Units)}"
            If bIncludeAspect And Height <> 0 Then
                _rVal += " {" + $"{Math.Round(Width / Height, aPrecis):0.000}" + "}"
            End If
            If bIncludeName Then

                If Name <> "" Then _rVal += $" [{ Name }]"
            End If
            Return _rVal
        End Function
        Public Function PolarVector(aDistance As Double, aAngle As Double, Optional bInRadians As Boolean = False, Optional aElevation As Double = 0.0, Optional aOrigin As iVector = Nothing) As TVECTOR
            Dim _rVal As New TVECTOR(Origin)
            '#1the distance for the point
            '#2the angle for the point
            '#3flag indicating that the angle is degrees or radians
            '#4the Z coordinate for the new point
            '#5an optional plane origin
            '^used to create a new polar point with respect to the passed plane
            Dim xDir As New TVECTOR(XDirection)
            Dim zDir As TVECTOR = ZDirection
            If aOrigin IsNot Nothing Then _rVal = New TVECTOR(aOrigin)
            Dim trans As New TVECTOR(0, 0, 0)
            If aDistance <> 0 Then
                xDir.RotateAbout(zDir, aAngle, bInRadians)
                trans += xDir * aDistance

            End If
            If aElevation <> 0 Then trans += zDir * aElevation
            If Not TVECTOR.IsNull(trans) Then _rVal += trans
            Return _rVal
        End Function


        Public Function Vector(Optional aVectorX As Double = 0, Optional aVectorY As Double = 0, Optional aVectorZ As Double = 0,
                                   Optional aShearXAngle As Double = 0, Optional bWorldOrigin As Boolean = False, Optional aOrigin As iVector = Nothing,
                                   Optional aVertexType As dxxVertexStyles = dxxVertexStyles.UNDEFINED, Optional aRotation As Double = 0, Optional aVectorRotation As Double = 0.0,
                                   Optional aCode As Byte = 0) As TVECTOR
            '#1the X coordinate for the new vector
            '#2the Y coordinate for the new vector
            '#3the Z coordinate for the new vector
            '#4an X shear angle to apply
            '#5flag to return the vector with respect to 0,0,0
            '#6the origin to use if the world origin flag is false (if not passed or Nothing the system origin is used)
            '^used to create a new vector with respect to the origin of the system
            Dim _rVal As New TVECTOR(Origin)
            If aOrigin IsNot Nothing Then
                _rVal = New TVECTOR(aOrigin)

            End If
            Dim xFctrT As Double = 0
            If Not bWorldOrigin Then
                If aOrigin Is Nothing Then _rVal = New TVECTOR(Origin) Else _rVal = New TVECTOR(aOrigin)
            End If
            _rVal.Code = aCode
            Dim aY As Double = aVectorY

            If aY <> 0 Then _rVal += _YDirection * aY
            If aShearXAngle <> 0 And aY <> 0 Then

                xFctrT = Math.Abs(aY) * Math.Tan(aShearXAngle * Math.PI / 180)
                If aY < 0 Then
                    xFctrT *= -1
                End If


            End If
            Dim dX As Double = aVectorX + xFctrT

            If dX <> 0 Then _rVal += _XDirection * dX
            If aVectorZ <> 0 Then _rVal += ZDirection * aVectorZ
            If aRotation <> 0 Then _rVal.RotateAbout(Origin, ZDirection, aRotation, False)
            _rVal.Code = TVALUES.ToByte(aVertexType)

            _rVal.Rotation = aVectorRotation
            Return _rVal
        End Function
        Public Function Vertex(Optional aVectorX As Double = 0, Optional aVectorY As Double = 0, Optional aVectorZ As Double = 0, Optional aRadius As Double = 0, Optional bInverted As Boolean? = Nothing,
                                   Optional aShearXAngle As Double = 0, Optional bWorldOrigin As Boolean = False, Optional aOrigin As dxfVector = Nothing,
                                   Optional aVertexType As dxxVertexStyles = dxxVertexStyles.UNDEFINED, Optional aRotation As Double = 0,
                                   Optional aVectorRotation As Double = 0.0, Optional aTag As String = "", Optional aFlag As String = "", Optional aValue As Double = 0) As TVERTEX
            Dim _rVal As New TVERTEX With {.Vector = Vector(aVectorX, aVectorY, aVectorZ, aShearXAngle, bWorldOrigin, aOrigin, aVertexType), .Tag = aTag, .Flag = aFlag, .Value = aValue}
            '#2the Y coordinate for the new vector
            '#3the Z coordinate for the new vector
            '#4an X shear angle to apply
            '#5flag to return the vector with respect to 0,0,0
            '#6the origin to use if the world origin flag is false (if not passed or Nothing the system origin is used)
            '^used to create a new vector with respect to the origin of the system

            _rVal.Radius = Math.Round(aRadius, 8)
            If _rVal.Radius < 0 Then
                _rVal.Radius = Math.Abs(_rVal.Radius)
                _rVal.Inverted = True
            End If

            If bInverted.HasValue Then
                _rVal.Inverted = bInverted.Value
            End If
            Return _rVal
        End Function
        Public Function VectorRelative(aBaseVector As TVECTOR, Optional aVectorX As Double? = Nothing, Optional aVectorY As Double? = Nothing, Optional aVectorZ As Double? = Nothing, Optional aVertexType As dxxVertexStyles = dxxVertexStyles.UNDEFINED) As TVECTOR

            '#2the base vector
            '#3the Y coordinate for the new vector
            '#4the Z coordinate for the new vector
            '#5an X shear angle to apply
            '#6flag to return the vector with respect to 0,0,0
            '#7the origin to use if the world origin flag is false (if not passed or Nothing the system origin is used)
            '^used to create a new vector with respect to the origin of the system
            Dim aVector As New TVECTOR(aBaseVector)

            If aVectorX.HasValue Then aVector += XDirection * aVectorX.Value
            If aVectorY.HasValue Then aVector += YDirection * aVectorY.Value
            If aVectorZ.HasValue Then aVector += ZDirection * aVectorZ.Value
            aVector.Code = TVALUES.ToByte(aVertexType)
            Return aVector
        End Function
        Public Function Verify(Optional bRedefine As Boolean = True, Optional aDefault As dxfPlane = Nothing) As Boolean
            Dim _rVal As Boolean = True
            Dim aFlg As Boolean
            _XDirection.Normalize(aFlg)
            If aFlg Then _rVal = False
            _YDirection.Normalize(aFlg)
            If aFlg Then _rVal = False
            If Not _rVal And bRedefine Then
                If aDefault IsNot Nothing Then
                    Define(Origin, aDefault.XDirectionV, aDefault.YDirectionV)
                Else
                    ResetDirections()
                End If
            End If
            Return _rVal
        End Function
        Public Function WorldVector(aVector As TVECTOR) As TVECTOR
            '#1a vector whose coordinates are with respect to the systems directions and origin
            '^converts then passed  vector to a word cs vector
            Return New TVECTOR(Origin) With {.Code = aVector.Code, .Rotation = aVector.Rotation} + XDirection * aVector.X + YDirection * aVector.Y + ZDirection * aVector.Z
        End Function
        Public Function WorldVector(aVector As dxfVector) As dxfVector
            '#1a vector whose coordinates are with respect to the systems directions and origin
            '^converts then passed  vector to a word cs vector
            Return New dxfVector(WorldVector(New TVECTOR(aVector)))
        End Function

        Public Function WorldVectors(aVectors As TVECTORS) As TVECTORS
            '#1a vectors whose coordinates are with respect to the systems directions and origin
            '^converts then passed  vector to a word cs vector
            Dim _rVal As New TVECTORS(aVectors.Count)
            For i As Integer = 1 To aVectors.Count
                _rVal.SetItem(i, WorldVector(aVectors.Item(i)))
            Next i
            Return _rVal
        End Function
        Public Overrides Function ToString() As String
            Dim _rVal As String = $" {Name} [ { _XDirection.ToString.Replace("TVECTOR", "XDIR") } { _YDirection.ToString.Replace("TVECTOR", "YDIR") } { Origin.ToString.Replace("TVECTOR", "ORIGIN") }]"
            If Height <> 0 Or Width <> 0 Then _rVal += $"<{ Width:0.0#} Wx{ Height:0.0#}H>"

            Return _rVal
        End Function
        Public Function Clone(Optional bClearDimensions As Boolean = False, Optional aNewName As String = Nothing) As TPLANE

            Return New TPLANE(Me, aNewName, bClearDimensions)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TPLANE(Me)
        End Function

        Public Function IsOrthogonalTo(bPlane As TPLANE, ByRef rNormAngle As Double, Optional aPrecis As Integer = 3) As Boolean
            Dim rParallel As Boolean = False
            Return IsOrthogonalTo(bPlane, rNormAngle, rParallel, aPrecis)
        End Function
        Public Function IsOrthogonalTo(bPlane As TPLANE, ByRef rNormAngle As Double, aPrecis As Integer, ByRef rParallel As Boolean) As Boolean
            Dim _rVal As Boolean
            '#1the  plane to test
            '#2returns the angle between this plane's normal and the passed one
            '#3the precision to round the angle to for the comparison to 90
            '#4returns True if the planes are orthogonal
            '^returns True if the normal of this plane is orthogonal to the normal of the passed one
            Dim aN As TVECTOR
            Dim inverseequal As Boolean
            aPrecis = TVALUES.LimitedValue(Math.Abs(aPrecis), 1, 6, 3)
            rParallel = False

            rNormAngle = 0
            If ZDirection.Equals(bPlane.ZDirection, True, aPrecis, inverseequal) Then
                If inverseequal Then rNormAngle = 180 Else rNormAngle = 0
                _rVal = False
                rParallel = True
            Else
                aN = ZDirection.CrossProduct(bPlane.ZDirection)
                rNormAngle = ZDirection.AngleTo(bPlane.ZDirection, aN)
                rNormAngle = Math.Round(rNormAngle, aPrecis)
                If (rNormAngle = 90 Or rNormAngle = 270) Then
                    _rVal = True
                End If
            End If
            Return _rVal
        End Function
        Friend Sub ExpandToVectors(aVectors As TVECTORS)
            If aVectors.Count <= 0 Then Return
            Dim aHt As Double
            Dim aWd As Double
            Dim d1 As Integer = 1
            Dim v1 As TVECTOR
            Dim dif As Double
            If Not (Width > 0 Or Height > 0) Then
                Origin = aVectors.PlanarCenter(Me, aWd, aHt)
                Height = aHt
                Width = aWd
            Else
                For i As Integer = 1 To aVectors.Count
                    v1 = aVectors.Item(i).WithRespectTo(Me)
                    dif = Math.Abs(v1.X) - 0.5 * Width
                    If dif > 0 Then
                        If v1.X < 0 Then d1 = -1 Else d1 = 1
                        Origin += _XDirection * (d1 * 0.5 * dif)
                        Width += Math.Abs(dif)
                    End If
                    dif = Math.Abs(v1.Y) - 0.5 * Height
                    If dif > 0 Then
                        If v1.Y < 0 Then d1 = -1 Else d1 = 1
                        Origin += _YDirection * (d1 * 0.5 * dif)
                        Height += Math.Abs(dif)
                    End If
                Next i
            End If
        End Sub
        Public Function MovedTo(aOrigin As TVECTOR) As TPLANE
            Return New TPLANE(Me, New dxfVector(aOrigin))

        End Function
        Public Function Define(aOrigin As TVECTOR, aXDir As TVECTOR, aYDir As TVECTOR, Optional aHeight As Double? = Nothing, Optional aWidth As Double? = Nothing) As Boolean
            Dim rOriginChange As Boolean = False
            Dim rDirectionChange As Boolean = False
            Dim rDimChange As Boolean = False
            Return Define(aOrigin, aXDir, aYDir, rOriginChange, rDirectionChange, rDimChange, aHeight, aWidth)
        End Function
        Public Function Define(aOrigin As TVECTOR, aXDir As TVECTOR, aYDir As TVECTOR, ByRef rOriginChange As Boolean, ByRef rDirectionChange As Boolean, ByRef rDimChange As Boolean, Optional aHeight As Double? = Nothing, Optional aWidth As Double? = Nothing) As Boolean
            rDimChange = False
            rDirectionChange = Not DirectionsAreDefined()
            rOriginChange = Not Origin.Equals(aOrigin, 6)

            Dim xErr As Boolean
            Dim yErr As Boolean
            Dim wd As Double = Width
            Dim ht As Double = Height
            If aWidth.HasValue Then wd = aWidth.Value
            If aHeight.HasValue Then ht = aHeight.Value


            If wd <> Width Then rDimChange = True
            If ht <> Height Then rDimChange = True
            Dim xDir As TVECTOR = TVECTOR.ToDirection(aXDir, xErr)
            Dim yDir As TVECTOR = TVECTOR.ToDirection(aYDir, yErr)
            Dim zDir As TVECTOR = _XDirection.CrossProduct(_YDirection)
            If xErr And yErr Then
                xDir = _XDirection
                yDir = _YDirection
                zDir = ZDirection
            Else

                If xErr And Not yErr Then

                    xDir = yDir.CrossProduct(zDir, True)
                    xErr = False
                ElseIf yErr And Not xErr Then

                    yDir = zDir.CrossProduct(xDir, True)
                    yErr = False
                Else
                    zDir = xDir.CrossProduct(yDir)
                End If
            End If
            If xDir.Equals(yDir, True, 6) Then
                yDir = zDir.CrossProduct(xDir, True)
            End If
            If Math.Abs(xDir.AngleTo(yDir, zDir, 4)) <> 90 Then
                yDir = zDir.CrossProduct(xDir, True)
            End If
            If Not _XDirection.Equals(xDir, 6) Then rDirectionChange = True
            If Not _YDirection.Equals(yDir, 6) Then rDirectionChange = True

            Height = ht
            Width = wd
            _XDirection = xDir
            _YDirection = yDir
            Origin = New TVECTOR(aOrigin)
            Return rOriginChange Or rDirectionChange Or rDimChange
        End Function
        Public Function Point(aAlignment As dxxRectanglePts, Optional bSuppressShear As Boolean = True, Optional bAllowForDescent As Boolean = False, Optional aCode As Byte = 0, Optional aXOffset As Double = 0, Optional aYOffset As Double = 0) As TVECTOR

            Dim aShear As Double
            Dim dX As Double
            Dim dY As Double
            If Not bSuppressShear Then aShear = ShearAngle
            Select Case aAlignment
                Case dxxRectanglePts.BaselineCenter
                    dX = 0
                    dY = -0.5 * Height + Descent
                Case dxxRectanglePts.BaselineLeft
                    dX = -0.5 * Width
                    dY = -0.5 * Height + Descent
                Case dxxRectanglePts.BaselineRight
                    dX = 0.5 * Width
                    dY = -0.5 * Height + Descent
                Case dxxRectanglePts.BottomCenter
                    dX = 0
                    dY = -0.5 * Height
                Case dxxRectanglePts.BottomLeft
                    dX = -0.5 * Width
                    dY = -0.5 * Height
                Case dxxRectanglePts.BottomRight
                    dX = 0.5 * Width
                    dY = -0.5 * Height
                Case dxxRectanglePts.MiddleCenter
                    dX = 0
                    dY = 0
                    If bAllowForDescent Then dY = -0.5 * Height + Descent + 0.5 * (Height - Descent) Else dY = 0
                Case dxxRectanglePts.MiddleLeft
                    dX = -0.5 * Width
                    If bAllowForDescent Then dY = -0.5 * Height + Descent + 0.5 * (Height - Descent) Else dY = 0
                Case dxxRectanglePts.MiddleRight
                    dX = 0.5 * Width
                    If bAllowForDescent Then dY = -0.5 * Height + Descent + 0.5 * (Height - Descent) Else dY = 0
                Case dxxRectanglePts.TopCenter
                    dX = 0
                    dY = 0.5 * Height
                Case dxxRectanglePts.TopLeft
                    dX = -0.5 * Width
                    dY = 0.5 * Height
                Case dxxRectanglePts.TopRight
                    dX = 0.5 * Width
                    dY = 0.5 * Height
                Case dxxRectanglePts.UnderlineLeft
                    dX = -0.5 * Width
                    dY = -0.5 * Height + Descent - 0.2 * (Height - Descent)
                Case dxxRectanglePts.UnderlineRight
                    dX = 0.5 * Width
                    dY = -0.5 * Height + Descent - 0.2 * (Height - Descent)
                Case dxxRectanglePts.OverlineLeft
                    dX = -0.5 * Width
                    dY = 0.5 * Height + 0.2 * Descent
                Case dxxRectanglePts.OverlineRight
                    dX = 0.5 * Width
                    dY = 0.5 * Height + 0.2 * Descent
                Case Else
                    dX = 0
                    dY = 0
            End Select
            Return Vector(dX + aXOffset, dY + aYOffset, aShearXAngle:=aShear, aCode:=aCode)

        End Function
        Public Function ResetDirections() As Boolean
            Dim v1 As TVECTOR = TVECTOR.WorldX
            Dim v2 As TVECTOR = TVECTOR.WorldY
            Dim _rVal As Boolean = Not _XDirection.Equals(v1, 6) Or Not _YDirection.Equals(v2, 6)
            _XDirection = v1
            _YDirection = v2
            Return _rVal

        End Function
        Public Function Rescale(aScaleFactor As Double, aReference As TVECTOR, Optional aCS As dxfPlane = Nothing) As Boolean

            '#1the factor to scale the entity by
            '#2the reference point to rescale the entities position with respect to
            '^rescales the entity in space and dimension by the passed factor
            aScaleFactor = Math.Abs(aScaleFactor)
            If aScaleFactor = 1 Or aScaleFactor = 0 Then Return False
            If Descent <> 0 Then Descent = Math.Abs(aScaleFactor * Descent)

            Dim cp As TVECTOR = Origin.Scaled(aScaleFactor, aReference, aScaleFactor, aScaleFactor, aCS)
            Return Define(cp, _XDirection, _YDirection, aScaleFactor * Height, aScaleFactor * Width)
        End Function
        Public Function Rescaled(aScaleFactor As Double, aReference As TVECTOR, Optional aCS As dxfPlane = Nothing) As TPLANE
            Dim rPlaneChange As Boolean = False
            Return Rescaled(aScaleFactor, aReference, aCS, rPlaneChange)
        End Function
        Public Function Rescaled(aScaleFactor As Double, aReference As TVECTOR, aCS As dxfPlane, ByRef rPlaneChange As Boolean) As TPLANE
            Dim _rVal As New TPLANE(Me)
            '#1the factor to scale the entity by
            '#2the reference point to rescale the entities position with respect to
            '^rescales the entity in space and dimension by the passed factor
            rPlaneChange = _rVal.Rescale(aScaleFactor, aReference, aCS)
            Return _rVal
        End Function
        Public Sub Translate(aTranslation As TVECTOR, Optional aCS As dxfPlane = Nothing)
            '#1the displacement to apply
            '#2a coordinate system to get the X,Y and Z directions from

            If TVECTOR.IsNull(aTranslation) Then Return
            '^used to change the coordinates of the entity by translation
            '~if the coordinate system is nothing then the displacement is added to the current coordinates
            '~otherwise the displacement is applied with respect to the systems X, Y and Z directions
            Dim v0 As TVECTOR
            If dxfPlane.IsNull(aCS) Then v0 = Origin + aTranslation Else v0 = New TPLANE(aCS).VectorRelative(Origin, aTranslation.X, aTranslation.Y, aTranslation.Z)
            Origin = v0
        End Sub

        Public Sub Translate(aTranslation As TVECTOR, aCS As dxfPlane, ByRef rPlaneChange As Boolean)
            '#1the displacement to apply
            '#2a coordinate system to get the X,Y and Z directions from
            rPlaneChange = False
            If TVECTOR.IsNull(aTranslation) Then Return
            '^used to change the coordinates of the entity by translation
            '~if the coordinate system is nothing then the displacement is added to the current coordinates
            '~otherwise the displacement is applied with respect to the systems X, Y and Z directions
            Dim v0 As TVECTOR
            If dxfPlane.IsNull(aCS) Then v0 = Origin + aTranslation Else v0 = New TPLANE(aCS).VectorRelative(Origin, aTranslation.X, aTranslation.Y, aTranslation.Z)
            rPlaneChange = Origin <> v0
            Origin = v0
        End Sub

        Public Function RectanglePts(bIncludeBaseline As Boolean, Optional bSuppressShear As Boolean = True) As TVECTORS
            Dim _rVal As New TVECTORS
            If Descent = 0 Then bIncludeBaseline = False
            _rVal = New TVECTORS
            If Width > 0 Or Height > 0 Then
                If Not bIncludeBaseline Then
                    _rVal.Add(Point(dxxRectanglePts.TopLeft, bSuppressShear), TVALUES.ToByte(dxxVertexStyles.MOVETO))
                    _rVal.Add(Point(dxxRectanglePts.BottomLeft, bSuppressShear), TVALUES.ToByte(dxxVertexStyles.LINETO))
                    _rVal.Add(Point(dxxRectanglePts.BottomRight, bSuppressShear), TVALUES.ToByte(dxxVertexStyles.LINETO))
                    _rVal.Add(Point(dxxRectanglePts.TopRight, bSuppressShear), TVALUES.ToByte(dxxVertexStyles.LINETO))
                    _rVal.Add(Point(dxxRectanglePts.TopLeft, bSuppressShear), TVALUES.ToByte(dxxVertexStyles.LINETO))
                Else
                    _rVal.Add(Point(dxxRectanglePts.BaselineLeft, bSuppressShear), TVALUES.ToByte(dxxVertexStyles.MOVETO))
                    _rVal.Add(Point(dxxRectanglePts.BaselineRight, bSuppressShear), TVALUES.ToByte(dxxVertexStyles.LINETO))
                    _rVal.Add(Point(dxxRectanglePts.TopRight, bSuppressShear), TVALUES.ToByte(dxxVertexStyles.LINETO))
                    _rVal.Add(Point(dxxRectanglePts.TopLeft, bSuppressShear), TVALUES.ToByte(dxxVertexStyles.LINETO))
                    _rVal.Add(Point(dxxRectanglePts.BottomLeft, bSuppressShear), TVALUES.ToByte(dxxVertexStyles.LINETO))
                    _rVal.Add(Point(dxxRectanglePts.BottomRight, bSuppressShear), TVALUES.ToByte(dxxVertexStyles.LINETO))
                    _rVal.Add(Point(dxxRectanglePts.BaselineRight, bSuppressShear), TVALUES.ToByte(dxxVertexStyles.LINETO))
                End If
            End If
            Return _rVal
        End Function
        Public Sub Stretch(aDist As Double, Optional aStretchWidth As Boolean = True, Optional aStretchHeight As Boolean = True, Optional bMaintainBaseline As Boolean = False, Optional bMaintainOrigin As Boolean = True)
            If (Not aStretchWidth And Not aStretchHeight) Or aDist = 0 Then Return
            If Descent = 0 Then bMaintainBaseline = False
            If aStretchWidth Then
                Width = Math.Abs(Width + aDist)
                If Not bMaintainOrigin Then
                    Origin += XDirection * (0.5 * aDist)
                End If
            End If
            If aStretchHeight Then
                If Height > 0 Then
                    If bMaintainBaseline Then
                        Dim f1 As Double
                        Dim d2 As Double
                        f1 = Math.Abs(Height + aDist) / Height
                        d2 = Height - (f1 * Height)
                        Height = f1 * Height
                        Descent *= f1
                        Origin += YDirection * (0.5 * d2)
                    Else
                        Height = Math.Abs(Height + aDist)
                        Descent = 0
                        If Not bMaintainOrigin Then
                            Origin += YDirection * (0.5 * aDist)
                        End If
                    End If
                Else
                    Height = Math.Abs(Height + aDist)
                    Descent = 0
                    If Not bMaintainOrigin Then
                        Origin += YDirection * (0.5 * aDist)
                    End If
                End If
            End If
        End Sub
        Public Function Stretched(aDist As Double, Optional aStretchWidth As Boolean = True, Optional aStretchHeight As Boolean = True, Optional bMaintainBaseline As Boolean = False, Optional bMaintainOrigin As Boolean = False) As TPLANE
            Dim _rVal As New TPLANE(Me)
            If (Not aStretchWidth And Not aStretchHeight) Or aDist = 0 Then Return _rVal
            _rVal.Stretch(aDist, aStretchWidth, aStretchHeight, bMaintainBaseline, bMaintainOrigin)
            Return _rVal
        End Function
        Public Function ToPlane(Optional aScaleFactor As Double = 0.0) As dxfPlane
            Dim bPln As New TPLANE(Me)
            If aScaleFactor <> 0 Then
                bPln.Width = TVALUES.To_DBL(bPln.Width * Math.Abs(aScaleFactor))
                bPln.Height = TVALUES.To_DBL(bPln.Height * Math.Abs(aScaleFactor))
            End If
            Return New dxfPlane(bPln)
        End Function
        Public Function ToRectangle(Optional aScaleFactor As Double = 0.0) As dxfRectangle
            Dim bPln As New TPLANE(Me)
            If aScaleFactor <> 0 Then
                bPln.Width = TVALUES.To_DBL(bPln.Width * Math.Abs(aScaleFactor))
                bPln.Height = TVALUES.To_DBL(bPln.Height * Math.Abs(aScaleFactor))
            End If
            Return New dxfRectangle(bPln)
        End Function
        Public Function ReDefined(aOrigin As TVECTOR, aXDir As TVECTOR, aYDir As TVECTOR, Optional aHeight As Double? = Nothing, Optional aWidth As Double? = Nothing) As TPLANE
            Dim rOriginChange As Boolean = False
            Dim rDirectionChange As Boolean = False
            Dim rDimChange As Boolean = False
            Dim rPlaneChange As Boolean = False
            Return ReDefined(aOrigin, aXDir, aYDir, rOriginChange, rDirectionChange, rDimChange, rPlaneChange, aHeight, aWidth)
        End Function
        Public Function ReDefined(aOrigin As TVECTOR, aXDir As TVECTOR, aYDir As TVECTOR, ByRef rOriginChange As Boolean, ByRef rDirectionChange As Boolean, ByRef rDimChange As Boolean, ByRef rPlaneChange As Boolean, Optional aHeight As Double? = Nothing, Optional aWidth As Double? = Nothing) As TPLANE
            Dim _rVal As New TPLANE(Me)
            rPlaneChange = _rVal.Define(aOrigin, aXDir, aYDir, rOriginChange, rDirectionChange, rDimChange, aHeight, aWidth)
            Return _rVal
        End Function
        Public Function StandardPlane(aType As dxxStandardPlanes, Optional aOrigin As dxfVector = Nothing, Optional aRotation As Double = 0.0) As TPLANE
            Dim _rVal As New TPLANE(Me)
            Dim org As TVECTOR
            org = Origin
            If aOrigin IsNot Nothing Then _rVal.Origin = aOrigin.Strukture
            If aType = dxxStandardPlanes.XZ Then
                _rVal.Name = "XZ"
                _rVal._XDirection = _XDirection
                _rVal._YDirection = ZDirection
            ElseIf aType = dxxStandardPlanes.YZ Then
                _rVal.Name = "YZ"
                _rVal._XDirection = (ZDirection * -1)
                _rVal._YDirection = _YDirection
            Else
                _rVal.Name = "XY"
            End If
            If aRotation <> 0 Then _rVal.Revolve(aRotation, False)
            Return _rVal
        End Function
        Public Function ScaleDimensions(Optional aWidthScaler As Double? = Nothing, Optional aHeightScaler As Double? = Nothing) As Boolean
            Dim _rVal As Boolean
            '#1the subject plane
            '#2the scale factor to apply to the width
            '#3the scale factor to apply to the height
            '^scales the dimensions of the rectangle and returns True if it changes
            If Not aWidthScaler.HasValue And Not aHeightScaler.HasValue Then Return False


            Dim wd As Double = Width
            Dim ht As Double = Height

            If aWidthScaler.HasValue Then
                wd *= Math.Abs(aWidthScaler.Value)
            End If
            If aHeightScaler.HasValue Then
                ht *= Math.Abs(aHeightScaler.Value)
            End If
            If ht <> Height Or wd <> Width Then _rVal = True
            Height = ht
            Width = wd
            Return _rVal
        End Function
        Public Sub ScaleUp(aScaleX As Double?, aScaleY As Double?, aReference As TVECTOR, Optional aScaleZ As Double? = Nothing)
            If Not aScaleX.HasValue Then aScaleX = 1
            If Not aScaleY.HasValue Then aScaleY = aScaleX
            If Not aScaleZ.HasValue Then aScaleZ = 1
            If aScaleX.Value = 1 And aScaleY.Value = 1 And aScaleZ.Value = 1 Then Return
            Width *= aScaleX.Value  '* Width
            Height *= aScaleY.Value '* Height
            If Descent <> 0 Then
                Descent *= aScaleY.Value '* Descent
            End If
            Origin.Scale(aScaleX.Value, aReference, aScaleY.Value, aScaleZ.Value)
        End Sub
        Public Function SetDimensions(Optional aWidth As Double? = Nothing, Optional aHeight As Double? = Nothing, Optional aMultiplier As Double = 1) As Boolean
            '#1the subject plane
            '#2the new width
            '#3the new height
            '#4a scale factor to apply
            '^sets the dimensions of the rectangle and returns True if it changes
            '~returns true if the dimensions change
            Dim wd As Double = Width
            Dim ht As Double = Height
            If aWidth.HasValue Then wd = Math.Abs(aWidth.Value)
            If aHeight.HasValue Then ht = Math.Abs(aHeight.Value)
            If aMultiplier <> 0 Then
                wd *= Math.Abs(aMultiplier)
                ht *= Math.Abs(aMultiplier)
            End If
            If ht <> Height Or wd <> Width Then
                Height = ht
                Width = wd
                Return True
            Else
                Return False
            End If
        End Function
        Public Function IsEqualTo(aPlane As TPLANE, bCompareDirections As Boolean, bCompareDimensions As Boolean, bCompareOrigin As Boolean, Optional aPrecis As Integer = 3) As Boolean
            If TPLANE.IsNull(aPlane) Then Return False
            Dim _rVal As Boolean = True
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            If bCompareDirections Then
                If Not ZDirection.Equals(aPlane.ZDirection, aPrecis) Then
                    _rVal = False
                Else
                    If Not XDirection.Equals(aPlane.XDirection, aPrecis) Then _rVal = False
                End If
            End If
            If bCompareDimensions Then
                If Math.Round(Width, aPrecis) <> Math.Round(aPlane.Width, aPrecis) Then
                    _rVal = False
                Else
                    If Math.Round(Height, aPrecis) <> Math.Round(aPlane.Height, aPrecis) Then _rVal = False
                End If
            End If
            If bCompareOrigin Then
                If Not Origin.Equals(aPlane.Origin, aPrecis) Then _rVal = False
            End If
            Return _rVal
        End Function
#End Region 'Methods
#Region "Shared Methods"

        Friend Shared Function IsNull(aPlane As TPLANE) As Boolean

            If TVECTOR.IsNull(aPlane._XDirection) Then Return True
            Return TVECTOR.IsNull(aPlane._YDirection)

        End Function
        Friend Shared Function IsNull(aCharBox As TCHARBOX) As Boolean

            If TVECTOR.IsNull(aCharBox.XDirection) Then Return True
            Return TVECTOR.IsNull(aCharBox.YDirection)

        End Function

        Public Shared Function IsNull(aPlane As dxfPlane) As Boolean

            If aPlane Is Nothing Then Return True
            If dxfVector.IsNull(aPlane.XDirection) Then Return True
            Return dxfVector.IsNull(aPlane.YDirection)

        End Function
        Public Shared Function IsNull(aCharBox As dxoCharBox) As Boolean
            If aCharBox Is Nothing Then Return True
            If dxfVector.IsNull(aCharBox.XDirection) Then Return True
            Return dxfVector.IsNull(aCharBox.YDirection)

        End Function
        Public Shared ReadOnly Property World
            Get
                Return New TPLANE("World")
            End Get
        End Property
        Public Shared Function AngleVec(aPointXY As iVector, aAngle As Double, aDistance As Double, Optional bInRadians As Boolean = False, Optional aCS As dxfPlane = Nothing) As dxfVector

            '#1the Point to project from
            '#2the direction (angle) to project in
            '#3the distance to project
            '#4flag to indicate if the passed angle is in Radians
            '#5the coordinate system to use
            '^returns a vector located the passed distance away at the passed angle from the passed vector
            '~if the passed vector is nothing then the center of the passed coodinate system is used as the base point.
            '~the vector is rotated about the z axis of the passed OCS
            Dim aPl As New TPLANE(aCS)
            Return New dxfVector(aPl.AngleVector(aPointXY, aAngle, aDistance, bInRadians))

        End Function
        Public Shared Function DefineXY(aXDir As TVECTOR, aYDir As TVECTOR, ByRef rUndefinable As Boolean, aOrigin As TVECTOR) As TPLANE
            Dim _rVal As New TPLANE("")
            '#1the object to use to get the x direction from
            '#2the object to use to get the y direction from
            '#3flag returns true if the passed directions cannot be used to define a plane
            '#4te origin to center the new plane at
            '^defines the primary directions based on the passed directions
            '~if the two directions are parallel or either is null nothing is done.
            '~if the y direction is not perpendicular to the x the orthogal component
            '~of the y with respect to the x is used for the final y direction.
            rUndefinable = TVECTOR.IsNull(aXDir) And TVECTOR.IsNull(aYDir)
            If Not rUndefinable Then Return _rVal
            Dim isnull As Boolean
            Dim xDir As TVECTOR = aXDir.Normalized(isnull)
            If isnull Then Return _rVal 'the x is null
            Dim yDir As TVECTOR = aYDir.Normalized(isnull)
            If isnull Then Return _rVal 'the y is null
            If xDir.Equals(yDir, True, 3) Then Return _rVal 'the lines are parallel
            rUndefinable = False
            Dim v1 As TVECTOR
            yDir = TVECTOR.Orthogonal(yDir, xDir, v1)
            _rVal = _rVal.ReDefined(aOrigin, xDir, yDir)
            Return _rVal
        End Function
        Public Shared Function ArbitraryCS(aExtrusionDirection As dxfVector, Optional bSuppressNormalization As Boolean = False, Optional bInvert As Boolean = False) As TPLANE
            If aExtrusionDirection Is Nothing Then Return TPLANE.World
            Return ArbitraryCS(aExtrusionDirection.Strukture, bSuppressNormalization, bInvert)
        End Function
        Public Shared Function ArbitraryCS(aExtrusionDirection As TVECTOR, Optional bSuppressNormalization As Boolean = False, Optional bInvert As Boolean = False) As TPLANE
            Dim _rVal As New TPLANE("")
            '^the ArbitraryCS is the arbitray coordinate system as defines in the AutoCAD DXF reference
            '~the ArbitraryCS Z axis is aligned to the entities OCS Z axis but the X and Y direction are arbitrarily defined.
            Dim aN As TVECTOR
            Dim aX As TVECTOR
            Dim aY As TVECTOR
            Dim isnull As Boolean
            Dim aLim As Double
            Dim org As TVECTOR
            Dim aZ As Double
            aLim = 0.015625 '(1 / 64)
            If Not bSuppressNormalization Then aN = aExtrusionDirection.Normalized(isnull) Else aN = aExtrusionDirection
            aZ = Math.Round(aN.Z, 6)
            If Math.Abs(aZ) = 1 Then
                aN = New TVECTOR(0, 0, aZ)
                aX = TVECTOR.WorldX
                aY = TVECTOR.WorldY
                If aZ = 1 Then
                    aX = TVECTOR.WorldX
                    aY = TVECTOR.WorldY
                Else
                    aX = New TVECTOR(-1, 0, 0)
                    aY = TVECTOR.WorldY
                End If
            Else
                If isnull Then aN = TVECTOR.WorldZ
                If bInvert Then aN *= -1
                If (Math.Abs(aN.X) < aLim) And (Math.Abs(aN.Y) < aLim) Then
                    aX = TVECTOR.WorldY.CrossProduct(aN, True)
                Else
                    aX = TVECTOR.WorldZ.CrossProduct(aN, True)
                End If
                aY = aN.CrossProduct(aX, True)
            End If
            _rVal.Define(org, aX, aY)
            Return _rVal
        End Function
#End Region 'Shared Methods
    End Structure 'TPLANE
    Friend Structure TPLANES
#Region "Shared Methods"
        Public Shared Function Axis(aPlane As TPLANE, aAxisDescr As dxxAxisDescriptors, Optional aLength As Double = 1, Optional aStartPt As iVector = Nothing, Optional aColor As dxxColors = dxxColors.Undefined, Optional aXOffset As Double = 0.0, Optional aYOffset As Double = 0.0, Optional aZOffset As Double = 0.0) As TLINE
            '#1the direction to align the new line to
            '#2the length for the axis
            '#3an optional origin for the axis
            '^returns a line beginning at the planes origin extending in the planes X direction
            '~if the passed length is negative then the axis extends in the opposite direction
            Dim aDir As TVECTOR
            Dim sp As TVECTOR
            If aAxisDescr = dxxAxisDescriptors.Z Then
                aDir = aPlane.ZDirection
            ElseIf aAxisDescr = dxxAxisDescriptors.Y Then
                aDir = aPlane.YDirection
            Else
                aDir = aPlane.XDirection
            End If
            If aLength = 0 Then aLength = 1
            If aStartPt IsNot Nothing Then sp = New TVECTOR(aStartPt) Else sp = New TVECTOR(aPlane.Origin)
            If aXOffset <> 0 Then sp += aPlane.XDirection * aXOffset
            If aYOffset <> 0 Then sp += aPlane.YDirection * aYOffset
            If aZOffset <> 0 Then sp += aPlane.ZDirection * aZOffset
            Return New TLINE(sp, sp + aDir * aLength)
        End Function
        Public Shared Function Compare(aPlane As TPLANE, bPlane As TPLANE, Optional aPrecision As Integer = 3, Optional bCompareOrigin As Boolean = False, Optional bCompareRotation As Boolean = False) As Boolean
            Dim rIsOrthogonal As Boolean = False
            Dim rNormAngle As Double = 0.0
            Dim rDifString As String = String.Empty
            Return Compare(aPlane, bPlane, aPrecision, bCompareOrigin, bCompareRotation, rIsOrthogonal, rNormAngle, rDifString)
        End Function
        Public Shared Function Compare(aPlane As TPLANE, bPlane As TPLANE, aPrecision As Integer, bCompareOrigin As Boolean, bCompareRotation As Boolean, ByRef rIsOrthogonal As Boolean, ByRef rNormAngle As Double, ByRef rDifString As String) As Boolean
            rDifString = ""
            rIsOrthogonal = False
            rNormAngle = 0D
            If TPLANE.IsNull(aPlane) Then Return False
            If TPLANE.IsNull(bPlane) Then Return False
            Dim _rVal As Boolean
            '#1the first plane to compare
            '#2the second plane to compare
            '#3the precision for numerical equallity comparison
            '#4flag to compare the origin
            '#5flag indicating that the planes can only be equal if their x directions are equal
            '#6returns true if the Z directions are 90 degrees appart (orthogonal)
            '#6returns the angle between the planes normals
            '^returns True if the planes are equal based on the arguments
            Dim fudge As Double
            aPrecision = TVALUES.LimitedValue(aPrecision, 1, 15, 3)
            fudge = 1 / TVALUES.To_DBL($"1{New String("0", aPrecision)}")
            rIsOrthogonal = aPlane.IsOrthogonalTo(bPlane, rNormAngle, aPrecision)
            If aPlane.Origin.LiesOn(bPlane, fudge) Then
                If Not rIsOrthogonal Then
                    _rVal = (rNormAngle = 0 Or rNormAngle = 180)
                    If Not _rVal Then TLISTS.Add(rDifString, "Directions")
                End If
            End If
            If bCompareOrigin Then
                Dim dst As Double = aPlane.Origin.DistanceTo(bPlane.Origin, aPrecision)
                If dst > fudge Then
                    TLISTS.Add(rDifString, "Origin")
                    _rVal = False
                End If
            End If
            If bCompareRotation Then
                If Not aPlane.XDirection.Equals(bPlane.XDirection, aPrecision) Then
                    TLISTS.Add(rDifString, "Rotation")
                    _rVal = False
                End If
            End If
            Return _rVal
        End Function
        Public Shared Function Compare2(aPlane As TPLANE, bPlane As TPLANE, Optional aPrecis As Integer = 4, Optional bCompareOrigin As Boolean = True, Optional bCompareDimensions As Boolean = True, Optional bCompareDirections As Boolean = True) As Boolean
            Dim rDifString As String = String.Empty
            Return Compare2(aPlane, bPlane, aPrecis, rDifString, bCompareOrigin, bCompareDimensions, bCompareDirections)
        End Function
        Public Shared Function Compare2(aPlane As TPLANE, bPlane As TPLANE, aPrecis As Integer, ByRef rDifString As String, Optional bCompareOrigin As Boolean = True, Optional bCompareDimensions As Boolean = True, Optional bCompareDirections As Boolean = True) As Boolean
            rDifString = ""
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            If bCompareDirections Then
                If Not aPlane.ZDirection.Equals(bPlane.ZDirection, aPrecis) Then
                    rDifString = "Directions"
                Else
                    If Not aPlane.XDirection.Equals(bPlane.XDirection, aPrecis) Then rDifString = "Directions"
                End If
            End If
            If bCompareOrigin Then
                If Not aPlane.Origin.Equals(bPlane.Origin, aPrecis) Then
                    TLISTS.Add(rDifString, "Origin")
                End If
            End If
            If bCompareDimensions Then
                If Math.Round(aPlane.Width - bPlane.Width, aPrecis) <> 0 Then
                    TLISTS.Add(rDifString, "Width")
                End If
                If Math.Round(aPlane.Height - bPlane.Height, aPrecis) <> 0 Then
                    TLISTS.Add(rDifString, "Height")
                End If
            End If
            Return (rDifString = "")
        End Function
        Public Shared Function FromNormal(aDirection As TVECTOR, aOrigin As TVECTOR, Optional bInvert As Boolean = False) As TPLANE

            Dim _rVal As New TPLANE("")
            Dim aN As TVECTOR
            Dim aX As TVECTOR = TVECTOR.Zero
            Dim aY As TVECTOR = TVECTOR.Zero
            Dim isnull As Boolean
            Dim aLim As Double
            _rVal.Origin = New TVECTOR(aOrigin)
            Dim aZ As Double
            aLim = 0.015625 '(1 / 64)
            aN = aDirection.Normalized(isnull)
            If isnull Then aN = TVECTOR.WorldZ
            aZ = Math.Round(aN.Z, 6)
            If Math.Abs(aZ) = 1 Then
                aN = New TVECTOR(0, 0, aZ)
                If aZ = 1 Then
                    aX = TVECTOR.WorldX
                    aY = TVECTOR.WorldY
                Else
                    aX = New TVECTOR(-1, 0, 0)
                    aY = TVECTOR.WorldY
                End If
            Else
                If bInvert Then aN *= -1
                If (Math.Abs(aN.X) < aLim) And (Math.Abs(aN.Y) < aLim) Then
                    aX = TVECTOR.WorldY
                    aX = aX.CrossProduct(aN)
                Else
                    aX = TVECTOR.WorldZ
                    aX = aX.CrossProduct(aN)
                End If
                aY = aN.CrossProduct(aX)
            End If
            _rVal.Define(_rVal.Origin, aX, aY)
            Return _rVal
        End Function
#End Region 'Shared Methods
    End Structure 'TPLANES
#End Region 'Structures

End Namespace
